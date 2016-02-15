﻿using System.Collections.Generic;

namespace TypeCobol.Compiler.CodeElements
{
    public class Node {
        private IList<Node> children_ = new List<Node>();
        public IList<Node> Children {
            get { return new System.Collections.ObjectModel.ReadOnlyCollection<Node>(children_); }
            private set { throw new System.InvalidOperationException(); }
        }
        public CodeElement CodeElement { get; internal set; }
        public Node Parent { get; internal set; }

        public Node(CodeElement e) { CodeElement = e; }

        internal void Add(Node child) {
            children_.Add(child);
            child.Parent = this;
        }
    }
}