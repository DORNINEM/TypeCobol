﻿using System;
using Antlr4.Runtime;
using System.Collections.Generic;
using TypeCobol.Compiler.AntlrUtils;
using TypeCobol.Compiler.CodeElements;
using TypeCobol.Compiler.CodeElements.Expressions;
using TypeCobol.Compiler.CodeModel;
using TypeCobol.Compiler.Parser;
using TypeCobol.Compiler.Parser.Generated;
using TypeCobol.Compiler.CodeElements.Functions;

namespace TypeCobol.Compiler.Diagnostics {


	class ReadOnlyPropertiesChecker: NodeListener {

		private static string[] READONLY_DATATYPES = { "DATE", };

		public IList<Type> GetCodeElements() {
			return new List<Type> { typeof(TypeCobol.Compiler.CodeModel.SymbolWriter), };
		}
		public void OnNode(Node node, ParserRuleContext c, Program program) {
			var element = node.CodeElement as TypeCobol.Compiler.CodeModel.SymbolWriter;
			var table = program.SymbolTable;
			foreach (var pair in element.Symbols) {
				if (pair.Item2 == null) continue; // no receiving item
				var lr = table.Get(pair.Item2);
				if (lr.Count != 1) continue; // ambiguity or not referenced; not my job
				var receiving = lr[0];
				checkReadOnly(node.CodeElement, receiving);
			}
		}

		private static void checkReadOnly(CodeElement e, DataDescriptionEntry receiving) {
			if (receiving.TopLevel == null) return;
		    foreach (var type in READONLY_DATATYPES) {
				if (type.Equals(receiving.TopLevel.DataType.Name.ToUpper())) {
					DiagnosticUtils.AddError(e, type + " properties are read-only");
				}
			}
		}
	}



	class FunctionChecker: NodeListener {
		public IList<Type> GetCodeElements() {
			return new List<Type> { typeof(TypeCobol.Compiler.CodeModel.IdentifierUser), };
		}

		public void OnNode(Node node, ParserRuleContext context, Program program) {
			var element = node.CodeElement as TypeCobol.Compiler.CodeModel.IdentifierUser;
			foreach (var identifier in element.Identifiers) {
				CheckIdentifier(node.CodeElement, program.SymbolTable, identifier);
			}
		}

		private static void CheckIdentifier(CodeElement e, SymbolTable table, Identifier identifier) {
			var fun = identifier as FunctionReference;
			if (fun == null) return;// we only check functions
			var defs = table.GetFunction(fun.Name);
			if (defs.Count != 1) return;// ambiguity is not our job
			var def = defs[0];
			if (fun.Parameters.Count > def.Profile.InputParameters.Count) {
				var message = String.Format("Function {0} only takes {1} parameters", def.Name, def.Profile.InputParameters.Count);
				DiagnosticUtils.AddError(e, message);
			}
			for (int c = 0; c < def.Profile.InputParameters.Count; c++) {
				var expected = def.Profile.InputParameters[c];
				if (c < fun.Parameters.Count) {
					var actual = fun.Parameters[c].Value;
					if (actual is Identifier) {
						var found = table.Get(((Identifier)actual).Name);
						if (found.Count != 1) continue;// ambiguity is not our job
						var type = found[0].DataType;
						// type check. please note:
						// 1- if only one of [actual|expected] types is null, overriden DataType.!= operator will detect of it
						// 2- if both are null, wee WANT it to break: in TypeCobol EVERYTHING should be typed,
						//    and things we cannot know their type as typed as DataType.Unknown (which is a non-null valid type).
						if (type == null || type != expected.DataType) {
							var message = String.Format("Function {0} expected parameter {1} of type {2} (actual: {3})", def.Name, c+1, expected.DataType, type);
							DiagnosticUtils.AddError(e, message);
						}
						var length = found[0].MemoryArea.Length;
						if (length > expected.MemoryArea.Length) {
							var message = String.Format("Function {0} expected parameter {1} of max length {2} (actual: {3})", def.Name, c+1, expected.MemoryArea.Length, length);
							DiagnosticUtils.AddError(e, message);
						}
					}
				} else {
					var message = String.Format("Function {0} is missing parameter {1} of type {2}", def.Name, c+1, expected.DataType);
					DiagnosticUtils.AddError(e, message);
				}
			}
		}
	}



	class FunctionDeclarationChecker: NodeListener {

		public IList<Type> GetCodeElements() {
			return new List<Type> { typeof(FunctionDeclarationHeader), };
		}
		public void OnNode(Node node, ParserRuleContext context, Program program) {
			var header = node.CodeElement as FunctionDeclarationHeader;
			FunctionDeclarationProfile profile = null;
			var profiles = node.GetChildren(typeof(FunctionDeclarationProfile));
			if (profiles.Count < 1) // no PROCEDURE DIVISION internal to function
				DiagnosticUtils.AddError(header, "Function \""+header.Name+"\" has no parameters and does nothing.");
			else if (profiles.Count > 1)
				foreach(var p in profiles)
					DiagnosticUtils.AddError(p.CodeElement, "Function \""+header.Name+"\" can have only one parameters profile.");
			else profile = (FunctionDeclarationProfile)profiles[0].CodeElement;

			var filesection = node.Get("file");
			if (filesection != null) // TCRFUN_DECLARATION_NO_FILE_SECTION
				DiagnosticUtils.AddError(filesection.CodeElement, "Illegal FILE SECTION in function \""+header.Name+"\" declaration", context);

			CheckEveryLinkageItemIsAParameter(node.Get("linkage"), profile.Profile);

			var functions = node.SymbolTable.GetFunction(header.Name, profile.Profile);
			if (functions.Count > 1)
				DiagnosticUtils.AddError(profile, "A function with the same name and profile already exists.", context);
			foreach(var function in functions)
				if (!function.IsProcedure && !function.IsFunction)
					DiagnosticUtils.AddError(profile, "\""+header.Name+"\" is neither procedure nor function.", context);
		}

		private void CheckEveryLinkageItemIsAParameter(Node node, ParametersProfile profile) {
			if (node == null) return; // no LINKAGE SECTION
			var linkage = new List<DataDescriptionEntry>();
			var entries = node.GetChildren(typeof(DataDescriptionEntry));
			foreach(var n in entries) linkage.Add((DataDescriptionEntry)n.CodeElement);
			foreach(var description in linkage) {
				var used = Validate(profile.ReturningParameter, (DataName)description.Name);
				if (used == null) used = GetParameter(profile.InputParameters,  (DataName)description.Name);
				if (used == null) used = GetParameter(profile.OutputParameters, (DataName)description.Name);
				if (used == null) used = GetParameter(profile.InoutParameters,  (DataName)description.Name);
				if (used == null) {
					var data = GetParameter(node, description.Name!=null?description.Name.Name:null);
					DiagnosticUtils.AddError(data, description.Name+" is not a parameter.");
				}
			}
		}
		private ParameterDescription GetParameter(IList<ParameterDescription> parameters, DataName name) {
			if (name == null) return null;
			foreach(var p in parameters)
				if (Validate(p, name) != null) return p;
			return null;
		}
		private ParameterDescription Validate(ParameterDescription parameter, DataName name) {
			if (parameter != null && parameter.Name.Equals(name)) return parameter;
			return null;
		}
		/// <param name="node">LINKAGE SECTION, presumably</param>
		/// <param name="name">Parameter we want</param>
		/// <returns>Parameter as declared in DATA DIVISION</returns>
		private DataDescriptionEntry GetParameter(Node node, string name) {
			var data = node.CodeElement as DataDescriptionEntry;
			if (data != null && data.QualifiedName.Matches(name)) return data;
			foreach(var child in node.Children) {
				var found = GetParameter(child, name);
				if (found != null) return found;
			}
			return null;
		}
	}



	/// <summary>Checks the TypeCobol custom functions rule: TCRFUN_NO_SECTION_OR_PARAGRAPH_IN_LIBRARY.</summary>
	class LibraryChecker: NodeListener {
		public IList<Type> GetCodeElements() {
			return new List<Type> { typeof(ProcedureDivisionHeader), };
		}
		public void OnNode(Node node, ParserRuleContext context, Program program) {
			var pdiv = node.CodeElement as ProcedureDivisionHeader;
			bool isPublicLibrary = false;
			var elementsInError = new List<CodeElement>();
			var errorMessages = new List<string>();
			foreach(var child in node.Children) {
				var ce = child.CodeElement;
				if (child.CodeElement == null) {
					elementsInError.Add(node.CodeElement);
					errorMessages.Add("Illegal default section in library.");
				} else {
					var function = child.CodeElement as FunctionDeclarationHeader;
					if (function != null) {
						isPublicLibrary = isPublicLibrary || function.Visibility == AccessModifier.Public;
					} else {
						elementsInError.Add(child.CodeElement);
						errorMessages.Add("Illegal non-function item in library");
					}
				}
			}
			if (isPublicLibrary) {
				for(int c = 0; c < errorMessages.Count; c++)
					DiagnosticUtils.AddError(elementsInError[c], errorMessages[c], context);
			}
		}
	}


}