﻿      * 7 CodeElements errors
      * "1"@(4:8>4:16): [27:1] Syntax error : Illegal default section in library.
      * "1"@(24:8>24:14): [27:1] Syntax error : Illegal FILE SECTION in function "FunDeclare.StrangelyReturnsItsInput" declaration
      * "1"@(44:12>44:26): [27:1] Syntax error : a is not a parameter.
      * "1"@(45:12>45:26): [27:1] Syntax error : b is not a parameter.
      * "1"@(46:12>46:26): [27:1] Syntax error : c is not a parameter.
      * "1"@(54:12>54:27): [27:1] Syntax error : Ambiguous reference to symbol result
      * "1"@(88:8>88:16): [27:1] Syntax error : Illegal non-function item in library
      * 1 ProgramClass errors
      * "1"@(1:12>1:25): [27:1] Syntax error : extraneous input '01totoPICX.' expecting {ProcedureDivisionHeader, WorkingStorageSectionHeader, LocalStorageSectionHeader, LinkageSectionHeader, FileDescriptionEntry, FunctionDeclarationEnd}
       IDENTIFICATION DIVISION.
       PROGRAM-ID. FunDeclare.
       
       PROCEDURE DIVISION.
            .
       
      *DECLARE function DoesNothing PUBLIC.                                   
       PROGRAM-ID. DoesNothing.                                               
         PROCEDURE DIVISION                                                   
         .                                                                    
           DISPLAY 'I DO NOTHING'
           .
       END PROGRAM DoesNothing.                                               

      *DECLARE function ReturnsZero PUBLIC.                                   
       PROGRAM-ID. ReturnsZero.                                               
         DATA DIVISION.
         LINKAGE SECTION.                                                     
           01 result PIC 9(04)                                                
         PROCEDURE DIVISION                                                   
             RETURNING result                                                 
         .                                                                    
           MOVE 0 TO result.
           .
       END PROGRAM ReturnsZero.                                               

      * ERROR Illegal FILE SECTION
      *DECLARE function StrangelyReturnsItsInput PRIVATE.                     
       PROGRAM-ID. StrangelyReturnsItsInput.                                  
         DATA DIVISION.
         FILE SECTION.
           01 toto PIC X.
         LINKAGE SECTION.
           01 x PIC 9(04)                                                     
           01 result PIC 9(04)                                                
         PROCEDURE DIVISION                                                   
             USING BY REFERENCE x                                             
             RETURNING result                                                 
         .                                                                    
           IF x = 0
             MOVE 0 TO result
           ELSE
             MOVE x TO result
           END-IF.
       END PROGRAM StrangelyReturnsItsInput.                                  

      * ERROR because x, y and result shouldn't be in LINKAGE
      *DECLARE function SumThreeWithClutterInLinkage PRIVATE.                 
       PROGRAM-ID. SumThreeWithClutterInLinkage.                              
         DATA DIVISION.
         LINKAGE SECTION.
           01 x PIC 9(04).
           01 y PIC 9(04).
           01 a PIC 9(04).
           01 b PIC 9(04).
           01 c PIC 9(04).
           01 result PIC 9(04).
           01 z PIC 9(04)                                                     
         PROCEDURE DIVISION                                                   
             USING BY REFERENCE x                                             
                   BY REFERENCE y                                             
                   BY REFERENCE z                                             
             RETURNING result                                                 
         .                                                                    
           MOVE 0 TO result.
           ADD x to result.
           ADD y to result.
           ADD z to result.
       END PROGRAM SumThreeWithClutterInLinkage.                              
       
      *DECLARE function SwapParameters PRIVATE.                               
       PROGRAM-ID. SwapParameters.                                            
         DATA DIVISION.
         WORKING-STORAGE SECTION.
           01 tmp PIC 9(04).
         LINKAGE SECTION.                                                     
           01 x PIC 9(04)                                                     
           01 y PIC 9(04)                                                     
         PROCEDURE DIVISION                                                   
             USING BY REFERENCE x                                             
                   BY REFERENCE y                                             
         .                                                                    
           MOVE x TO tmp
           MOVE y TO x
           MOVE tmp TO y
           .
       END PROGRAM SwapParameters.                                            

      * ERROR because x and y should be INOUT
      * ERROR because y INPUT vs OUTPUT types differ
      *DECLARE function SwapParametersWrong PRIVATE.                          
       PROGRAM-ID. SwapParametersWrong.                                       
         LINKAGE SECTION.                                                     
           01 x PIC 9(04)                                                     
           01 y PIC 9(04)                                                     
           01 a PIC 9(04)                                                     
           01 b PIC 9(04)                                                     
         PROCEDURE DIVISION                                                   
             USING BY REFERENCE x                                             
                   BY REFERENCE y                                             
                   BY REFERENCE a                                             
                   BY REFERENCE b                                             
         .                                                                    
           CONTINUE.
       END PROGRAM SwapParametersWrong.                                       

       ILLEGAL-NON-FUNCTION-PARAGRAPH.
           CONTINUE.
       
       END PROGRAM FunDeclare.
       
