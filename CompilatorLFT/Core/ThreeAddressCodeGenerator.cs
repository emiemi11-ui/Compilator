using System;
using System.Collections.Generic;
using System.Text;
using CompilatorLFT.Models;
using CompilatorLFT.Models.Expressions;
using CompilatorLFT.Models.Statements;

namespace CompilatorLFT.Core
{
    /// <summary>
    /// Three-Address Code (TAC) Generator.
    /// </summary>
    /// <remarks>
    /// Reference: Dragon Book, Ch. 6 - Intermediate-Code Generation
    /// Reference: Grigoraș "Proiectarea Compilatoarelor", Cap. 6.4.4
    ///
    /// Generates intermediate code in the form of three-address instructions:
    /// - A = B op C (binary operation)
    /// - A = op B (unary operation)
    /// - A = B (copy)
    /// - goto L (unconditional jump)
    /// - if A goto L (conditional jump)
    /// - if A relop B goto L (conditional jump with comparison)
    /// - param x (function parameter)
    /// - call f, n (function call with n parameters)
    /// - return x (return from function)
    ///
    /// Example from Grigoraș:
    /// Source: delta = b*b - 4*a*c
    /// TAC:
    ///   T1 = b * b
    ///   T2 = 4 * a
    ///   T3 = T2 * c
    ///   T4 = T1 - T3
    ///   delta = T4
    /// </remarks>
    public class ThreeAddressCodeGenerator
    {
        #region Instruction Types

        /// <summary>
        /// Type of TAC instruction.
        /// </summary>
        public enum TACOperationType
        {
            /// <summary>Assignment: A = B</summary>
            Copy,

            /// <summary>Binary operation: A = B op C</summary>
            BinaryOp,

            /// <summary>Unary operation: A = op B</summary>
            UnaryOp,

            /// <summary>Unconditional jump: goto L</summary>
            Goto,

            /// <summary>Conditional jump: if A goto L</summary>
            IfGoto,

            /// <summary>Conditional jump: ifFalse A goto L</summary>
            IfFalseGoto,

            /// <summary>Label: L:</summary>
            Label,

            /// <summary>Function parameter: param x</summary>
            Param,

            /// <summary>Function call: call f, n</summary>
            Call,

            /// <summary>Return from function: return x</summary>
            Return,

            /// <summary>Print statement: print x</summary>
            Print,

            /// <summary>No operation (placeholder)</summary>
            Nop
        }

        /// <summary>
        /// Represents a single three-address instruction.
        /// </summary>
        public class TACInstruction
        {
            /// <summary>Instruction number (address).</summary>
            public int Address { get; set; }

            /// <summary>Type of operation.</summary>
            public TACOperationType Operation { get; set; }

            /// <summary>Result (destination) operand.</summary>
            public string Result { get; set; }

            /// <summary>First source operand.</summary>
            public string Arg1 { get; set; }

            /// <summary>Second source operand (for binary ops).</summary>
            public string Arg2 { get; set; }

            /// <summary>Operator (for binary/unary ops).</summary>
            public string Operator { get; set; }

            /// <summary>Label name (for jumps and labels).</summary>
            public string Label { get; set; }

            /// <summary>Number of parameters (for call).</summary>
            public int ParamCount { get; set; }

            public override string ToString()
            {
                return Operation switch
                {
                    TACOperationType.Copy => $"{Address}: {Result} = {Arg1}",
                    TACOperationType.BinaryOp => $"{Address}: {Result} = {Arg1} {Operator} {Arg2}",
                    TACOperationType.UnaryOp => $"{Address}: {Result} = {Operator} {Arg1}",
                    TACOperationType.Goto => $"{Address}: goto {Label}",
                    TACOperationType.IfGoto => $"{Address}: if {Arg1} goto {Label}",
                    TACOperationType.IfFalseGoto => $"{Address}: ifFalse {Arg1} goto {Label}",
                    TACOperationType.Label => $"{Address}: {Label}:",
                    TACOperationType.Param => $"{Address}: param {Arg1}",
                    TACOperationType.Call => $"{Address}: {Result} = call {Arg1}, {ParamCount}",
                    TACOperationType.Return => string.IsNullOrEmpty(Arg1) ?
                        $"{Address}: return" : $"{Address}: return {Arg1}",
                    TACOperationType.Print => $"{Address}: print {Arg1}",
                    TACOperationType.Nop => $"{Address}: nop",
                    _ => $"{Address}: unknown"
                };
            }
        }

        #endregion

        #region Private Fields

        private readonly List<TACInstruction> _instructions;
        private int _tempCounter;
        private int _labelCounter;
        private int _addressCounter;

        #endregion

        #region Properties

        /// <summary>Generated TAC instructions.</summary>
        public IReadOnlyList<TACInstruction> Instructions => _instructions;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new TAC generator.
        /// </summary>
        public ThreeAddressCodeGenerator()
        {
            _instructions = new List<TACInstruction>();
            _tempCounter = 0;
            _labelCounter = 0;
            _addressCounter = 100; // Start at 100 like in Grigoraș examples
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates a new temporary variable name.
        /// </summary>
        public string NewTemp()
        {
            return $"T{_tempCounter++}";
        }

        /// <summary>
        /// Generates a new label name.
        /// </summary>
        public string NewLabel()
        {
            return $"L{_labelCounter++}";
        }

        /// <summary>
        /// Generates TAC for a complete program.
        /// </summary>
        /// <param name="program">The parsed program</param>
        public void Generate(Program program)
        {
            _instructions.Clear();
            _tempCounter = 0;
            _labelCounter = 0;
            _addressCounter = 100;

            // Generate code for all statements
            foreach (var statement in program.Statements)
            {
                GenerateStatement(statement);
            }
        }

        /// <summary>
        /// Returns the TAC as a formatted string.
        /// </summary>
        public string GetFormattedOutput()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== THREE-ADDRESS CODE (TAC) ===");
            sb.AppendLine("Reference: Grigoraș, Cap. 6.4.4");
            sb.AppendLine();

            foreach (var instr in _instructions)
            {
                sb.AppendLine(instr.ToString());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Displays the TAC to console.
        /// </summary>
        public void DisplayTAC()
        {
            Console.WriteLine("\n" + new string('=', 50));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("THREE-ADDRESS CODE (TAC) - Grigoraș 6.4.4");
            Console.ResetColor();
            Console.WriteLine(new string('=', 50));

            foreach (var instr in _instructions)
            {
                Console.WriteLine(instr.ToString());
            }
        }

        #endregion

        #region Private Methods - Statement Generation

        private void GenerateStatement(Statement statement)
        {
            switch (statement)
            {
                case DeclarationStatement decl:
                    GenerateDeclaration(decl);
                    break;

                case AssignmentStatement assign:
                    GenerateAssignment(assign);
                    break;

                case CompoundAssignmentStatement compound:
                    GenerateCompoundAssignment(compound);
                    break;

                case PrintStatement print:
                    GeneratePrint(print);
                    break;

                case IfStatement ifStmt:
                    GenerateIf(ifStmt);
                    break;

                case WhileStatement whileStmt:
                    GenerateWhile(whileStmt);
                    break;

                case ForStatement forStmt:
                    GenerateFor(forStmt);
                    break;

                case BlockStatement block:
                    foreach (var stmt in block.Statements)
                    {
                        GenerateStatement(stmt);
                    }
                    break;

                case ExpressionStatement expr:
                    GenerateExpression(expr.Expression);
                    break;

                case ReturnStatement ret:
                    GenerateReturn(ret);
                    break;

                case BreakStatement:
                    // Break handled by loop context
                    break;

                case ContinueStatement:
                    // Continue handled by loop context
                    break;
            }
        }

        private void GenerateDeclaration(DeclarationStatement decl)
        {
            foreach (var (id, initExpr) in decl.Declarations)
            {
                if (initExpr != null)
                {
                    string tempResult = GenerateExpression(initExpr);
                    Emit(new TACInstruction
                    {
                        Address = _addressCounter++,
                        Operation = TACOperationType.Copy,
                        Result = id.Text,
                        Arg1 = tempResult
                    });
                }
            }
        }

        private void GenerateAssignment(AssignmentStatement assign)
        {
            string tempResult = GenerateExpression(assign.Expression);
            Emit(new TACInstruction
            {
                Address = _addressCounter++,
                Operation = TACOperationType.Copy,
                Result = assign.Identifier.Text,
                Arg1 = tempResult
            });
        }

        private void GenerateCompoundAssignment(CompoundAssignmentStatement stmt)
        {
            string exprResult = GenerateExpression(stmt.Expression);
            string temp = NewTemp();
            string op = stmt.Operator.Type switch
            {
                TokenType.PlusEqual => "+",
                TokenType.MinusEqual => "-",
                TokenType.StarEqual => "*",
                TokenType.SlashEqual => "/",
                TokenType.PercentEqual => "%",
                _ => "?"
            };

            Emit(new TACInstruction
            {
                Address = _addressCounter++,
                Operation = TACOperationType.BinaryOp,
                Result = temp,
                Arg1 = stmt.Identifier.Text,
                Operator = op,
                Arg2 = exprResult
            });

            Emit(new TACInstruction
            {
                Address = _addressCounter++,
                Operation = TACOperationType.Copy,
                Result = stmt.Identifier.Text,
                Arg1 = temp
            });
        }

        private void GeneratePrint(PrintStatement print)
        {
            string exprResult = GenerateExpression(print.Expression);
            Emit(new TACInstruction
            {
                Address = _addressCounter++,
                Operation = TACOperationType.Print,
                Arg1 = exprResult
            });
        }

        /// <summary>
        /// Generates TAC for if statement.
        /// Example from Grigoraș:
        /// if (a &lt; b) then b = b – a else a = a - b
        ///
        /// 103: if a &lt; b goto 105
        /// 104: goto 108
        /// 105: T1 = b – a
        /// 106: b = T1
        /// 107: goto 110
        /// 108: T2 = a – b
        /// 109: a = T2
        /// 110: ...
        /// </summary>
        private void GenerateIf(IfStatement ifStmt)
        {
            string condResult = GenerateExpression(ifStmt.Condition);

            string labelElse = NewLabel();
            string labelEnd = NewLabel();

            // ifFalse condition goto labelElse
            Emit(new TACInstruction
            {
                Address = _addressCounter++,
                Operation = TACOperationType.IfFalseGoto,
                Arg1 = condResult,
                Label = labelElse
            });

            // Then branch
            GenerateStatement(ifStmt.ThenBody);

            if (ifStmt.ElseBody != null)
            {
                // goto labelEnd (skip else)
                Emit(new TACInstruction
                {
                    Address = _addressCounter++,
                    Operation = TACOperationType.Goto,
                    Label = labelEnd
                });
            }

            // labelElse:
            Emit(new TACInstruction
            {
                Address = _addressCounter++,
                Operation = TACOperationType.Label,
                Label = labelElse
            });

            // Else branch
            if (ifStmt.ElseBody != null)
            {
                GenerateStatement(ifStmt.ElseBody);

                // labelEnd:
                Emit(new TACInstruction
                {
                    Address = _addressCounter++,
                    Operation = TACOperationType.Label,
                    Label = labelEnd
                });
            }
        }

        /// <summary>
        /// Generates TAC for while statement.
        /// Example from Grigoraș:
        /// while (a != b) { if (a &lt; b) ... }
        ///
        /// 101: if a != b goto 103
        /// 102: goto 111
        /// 103: ...
        /// 110: goto 101
        /// 111: ...
        /// </summary>
        private void GenerateWhile(WhileStatement whileStmt)
        {
            string labelStart = NewLabel();
            string labelEnd = NewLabel();

            // labelStart:
            Emit(new TACInstruction
            {
                Address = _addressCounter++,
                Operation = TACOperationType.Label,
                Label = labelStart
            });

            string condResult = GenerateExpression(whileStmt.Condition);

            // ifFalse condition goto labelEnd
            Emit(new TACInstruction
            {
                Address = _addressCounter++,
                Operation = TACOperationType.IfFalseGoto,
                Arg1 = condResult,
                Label = labelEnd
            });

            // Body
            GenerateStatement(whileStmt.Body);

            // goto labelStart
            Emit(new TACInstruction
            {
                Address = _addressCounter++,
                Operation = TACOperationType.Goto,
                Label = labelStart
            });

            // labelEnd:
            Emit(new TACInstruction
            {
                Address = _addressCounter++,
                Operation = TACOperationType.Label,
                Label = labelEnd
            });
        }

        private void GenerateFor(ForStatement forStmt)
        {
            // Initialization
            if (forStmt.Initialization != null)
            {
                GenerateStatement(forStmt.Initialization);
            }

            string labelStart = NewLabel();
            string labelEnd = NewLabel();

            // labelStart:
            Emit(new TACInstruction
            {
                Address = _addressCounter++,
                Operation = TACOperationType.Label,
                Label = labelStart
            });

            // Condition
            if (forStmt.Condition != null)
            {
                string condResult = GenerateExpression(forStmt.Condition);

                // ifFalse condition goto labelEnd
                Emit(new TACInstruction
                {
                    Address = _addressCounter++,
                    Operation = TACOperationType.IfFalseGoto,
                    Arg1 = condResult,
                    Label = labelEnd
                });
            }

            // Body
            GenerateStatement(forStmt.Body);

            // Increment
            if (forStmt.Increment != null)
            {
                GenerateStatement(forStmt.Increment);
            }

            // goto labelStart
            Emit(new TACInstruction
            {
                Address = _addressCounter++,
                Operation = TACOperationType.Goto,
                Label = labelStart
            });

            // labelEnd:
            Emit(new TACInstruction
            {
                Address = _addressCounter++,
                Operation = TACOperationType.Label,
                Label = labelEnd
            });
        }

        private void GenerateReturn(ReturnStatement ret)
        {
            string result = null;
            if (ret.Expression != null)
            {
                result = GenerateExpression(ret.Expression);
            }

            Emit(new TACInstruction
            {
                Address = _addressCounter++,
                Operation = TACOperationType.Return,
                Arg1 = result
            });
        }

        #endregion

        #region Private Methods - Expression Generation

        /// <summary>
        /// Generates TAC for an expression and returns the result location.
        /// Example from Grigoraș:
        /// delta = b*b - 4*a*c
        ///
        /// T1 = b * b
        /// T2 = 4 * a
        /// T3 = T2 * c
        /// T4 = T1 - T3
        /// delta = T4
        /// </summary>
        private string GenerateExpression(Expression expression)
        {
            return expression switch
            {
                NumericExpression num => num.Number.Value.ToString(),
                StringExpression str => $"\"{str.StringValue.Value}\"",
                BooleanExpression boolExpr => boolExpr.Value.ToString().ToLower(),
                IdentifierExpression id => id.Identifier.Text,
                UnaryExpression unary => GenerateUnaryExpression(unary),
                NotExpression not => GenerateNotExpression(not),
                BinaryExpression binary => GenerateBinaryExpression(binary),
                LogicalExpression logical => GenerateLogicalExpression(logical),
                ParenthesizedExpression paren => GenerateExpression(paren.Expression),
                IncrementExpression inc => GenerateIncrementExpression(inc),
                FunctionCallExpression call => GenerateFunctionCall(call),
                _ => "?"
            };
        }

        private string GenerateUnaryExpression(UnaryExpression unary)
        {
            string operand = GenerateExpression(unary.Operand);
            string temp = NewTemp();

            Emit(new TACInstruction
            {
                Address = _addressCounter++,
                Operation = TACOperationType.UnaryOp,
                Result = temp,
                Operator = "-",
                Arg1 = operand
            });

            return temp;
        }

        private string GenerateNotExpression(NotExpression not)
        {
            string operand = GenerateExpression(not.Operand);
            string temp = NewTemp();

            Emit(new TACInstruction
            {
                Address = _addressCounter++,
                Operation = TACOperationType.UnaryOp,
                Result = temp,
                Operator = "!",
                Arg1 = operand
            });

            return temp;
        }

        private string GenerateBinaryExpression(BinaryExpression binary)
        {
            string left = GenerateExpression(binary.Left);
            string right = GenerateExpression(binary.Right);
            string temp = NewTemp();

            string op = binary.Operator.Type switch
            {
                TokenType.Plus => "+",
                TokenType.Minus => "-",
                TokenType.Star => "*",
                TokenType.Slash => "/",
                TokenType.Percent => "%",
                TokenType.LessThan => "<",
                TokenType.GreaterThan => ">",
                TokenType.LessThanOrEqual => "<=",
                TokenType.GreaterThanOrEqual => ">=",
                TokenType.EqualEqual => "==",
                TokenType.NotEqual => "!=",
                _ => "?"
            };

            Emit(new TACInstruction
            {
                Address = _addressCounter++,
                Operation = TACOperationType.BinaryOp,
                Result = temp,
                Arg1 = left,
                Operator = op,
                Arg2 = right
            });

            return temp;
        }

        private string GenerateLogicalExpression(LogicalExpression logical)
        {
            // For short-circuit evaluation, we'd need jumps
            // Simplified version: evaluate both sides
            string left = GenerateExpression(logical.Left);
            string right = GenerateExpression(logical.Right);
            string temp = NewTemp();

            string op = logical.Operator.Type switch
            {
                TokenType.LogicalAnd => "&&",
                TokenType.LogicalOr => "||",
                _ => "?"
            };

            Emit(new TACInstruction
            {
                Address = _addressCounter++,
                Operation = TACOperationType.BinaryOp,
                Result = temp,
                Arg1 = left,
                Operator = op,
                Arg2 = right
            });

            return temp;
        }

        private string GenerateIncrementExpression(IncrementExpression inc)
        {
            string varName = inc.Identifier.Text;
            string temp = NewTemp();

            if (inc.IsPrefix)
            {
                // ++i: increment first, return new value
                Emit(new TACInstruction
                {
                    Address = _addressCounter++,
                    Operation = TACOperationType.BinaryOp,
                    Result = temp,
                    Arg1 = varName,
                    Operator = inc.IsIncrement ? "+" : "-",
                    Arg2 = "1"
                });

                Emit(new TACInstruction
                {
                    Address = _addressCounter++,
                    Operation = TACOperationType.Copy,
                    Result = varName,
                    Arg1 = temp
                });

                return temp;
            }
            else
            {
                // i++: save old value, increment, return old value
                string oldValue = NewTemp();

                Emit(new TACInstruction
                {
                    Address = _addressCounter++,
                    Operation = TACOperationType.Copy,
                    Result = oldValue,
                    Arg1 = varName
                });

                Emit(new TACInstruction
                {
                    Address = _addressCounter++,
                    Operation = TACOperationType.BinaryOp,
                    Result = temp,
                    Arg1 = varName,
                    Operator = inc.IsIncrement ? "+" : "-",
                    Arg2 = "1"
                });

                Emit(new TACInstruction
                {
                    Address = _addressCounter++,
                    Operation = TACOperationType.Copy,
                    Result = varName,
                    Arg1 = temp
                });

                return oldValue;
            }
        }

        private string GenerateFunctionCall(FunctionCallExpression call)
        {
            // Generate code for each argument
            foreach (var arg in call.Arguments)
            {
                string argResult = GenerateExpression(arg);
                Emit(new TACInstruction
                {
                    Address = _addressCounter++,
                    Operation = TACOperationType.Param,
                    Arg1 = argResult
                });
            }

            string temp = NewTemp();

            Emit(new TACInstruction
            {
                Address = _addressCounter++,
                Operation = TACOperationType.Call,
                Result = temp,
                Arg1 = call.FunctionName.Text,
                ParamCount = call.Arguments.Count
            });

            return temp;
        }

        #endregion

        #region Helper Methods

        private void Emit(TACInstruction instruction)
        {
            _instructions.Add(instruction);
        }

        #endregion
    }
}
