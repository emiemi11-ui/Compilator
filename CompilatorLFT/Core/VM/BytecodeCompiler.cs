using System;
using System.Collections.Generic;
using CompilatorLFT.Models;
using CompilatorLFT.Models.Expressions;
using CompilatorLFT.Models.Statements;

namespace CompilatorLFT.Core.VM
{
    /// <summary>
    /// Compiles the Abstract Syntax Tree (AST) to bytecode.
    /// Produces executable code for the Virtual Machine.
    /// </summary>
    /// <remarks>
    /// Reference: Dragon Book, Ch. 8 - Intermediate Code Generation
    /// Reference: Dragon Book, Ch. 8.4 - Translation of Expressions
    /// Reference: Grigoraș "Proiectarea Compilatoarelor", Cap. 6.4
    ///
    /// Compilation strategy:
    /// - Post-order traversal of AST for expressions (operands before operator)
    /// - Control flow uses labels and jumps
    /// - Function calls use call/return convention
    /// - Stack-based evaluation model
    /// </remarks>
    public class BytecodeCompiler
    {
        #region Private Fields

        private BytecodeProgram _program;
        private int _labelCounter;
        private readonly Dictionary<string, int> _loopStartLabels;
        private readonly Dictionary<string, int> _loopEndLabels;
        private readonly Stack<string> _loopStack;

        #endregion

        #region Properties

        /// <summary>The compiled bytecode program.</summary>
        public BytecodeProgram CompiledProgram => _program;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new bytecode compiler.
        /// </summary>
        public BytecodeCompiler()
        {
            _loopStartLabels = new Dictionary<string, int>();
            _loopEndLabels = new Dictionary<string, int>();
            _loopStack = new Stack<string>();
            Reset();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Compiles a program to bytecode.
        /// </summary>
        /// <param name="program">The AST to compile</param>
        /// <returns>The compiled bytecode program</returns>
        public BytecodeProgram Compile(Program program)
        {
            if (program == null)
                throw new ArgumentNullException(nameof(program));

            Reset();

            // Compile functions first (they may be called before definition)
            foreach (var function in program.Functions)
            {
                CompileFunction(function);
            }

            // Compile top-level statements
            foreach (var statement in program.Statements)
            {
                CompileStatement(statement);
            }

            // Add HALT at the end
            Emit(OpCode.HALT);

            // Resolve all label references
            _program.ResolveLabels();

            return _program;
        }

        /// <summary>
        /// Resets the compiler state.
        /// </summary>
        public void Reset()
        {
            _program = new BytecodeProgram();
            _labelCounter = 0;
            _loopStartLabels.Clear();
            _loopEndLabels.Clear();
            _loopStack.Clear();
        }

        #endregion

        #region Emit Helpers

        /// <summary>
        /// Emits an instruction with no operand.
        /// </summary>
        private int Emit(OpCode opCode, int line = 0)
        {
            return _program.AddInstruction(new Instruction(opCode, line));
        }

        /// <summary>
        /// Emits an instruction with one operand.
        /// </summary>
        private int Emit(OpCode opCode, object operand, int line = 0)
        {
            return _program.AddInstruction(new Instruction(opCode, operand, line));
        }

        /// <summary>
        /// Emits an instruction with two operands.
        /// </summary>
        private int Emit(OpCode opCode, object operand, object operand2, int line = 0)
        {
            return _program.AddInstruction(new Instruction(opCode, operand, operand2, line));
        }

        /// <summary>
        /// Creates a new unique label.
        /// </summary>
        private string NewLabel()
        {
            return $"L{_labelCounter++}";
        }

        /// <summary>
        /// Marks the current address with a label.
        /// </summary>
        private void MarkLabel(string label)
        {
            _program.RegisterLabel(label, _program.Instructions.Count);
        }

        /// <summary>
        /// Gets the current instruction address.
        /// </summary>
        private int CurrentAddress => _program.Instructions.Count;

        #endregion

        #region Function Compilation

        /// <summary>
        /// Compiles a function declaration.
        /// </summary>
        private void CompileFunction(FunctionDeclaration function)
        {
            // Register function entry point
            _program.RegisterFunction(function.Name.Text, CurrentAddress);

            // Compile function body
            CompileBlock(function.Body);

            // Ensure there's a return at the end
            var lastInstr = _program.Instructions.Count > 0
                ? _program.Instructions[^1]
                : null;

            if (lastInstr == null || (lastInstr.OpCode != OpCode.RET && lastInstr.OpCode != OpCode.RET_VAL))
            {
                Emit(OpCode.RET, function.Name.Line);
            }
        }

        #endregion

        #region Statement Compilation

        /// <summary>
        /// Compiles a statement.
        /// </summary>
        private void CompileStatement(Statement statement)
        {
            if (statement == null) return;

            switch (statement)
            {
                case DeclarationStatement decl:
                    CompileDeclaration(decl);
                    break;

                case AssignmentStatement assign:
                    CompileAssignment(assign);
                    break;

                case CompoundAssignmentStatement compAssign:
                    CompileCompoundAssignment(compAssign);
                    break;

                case IfStatement ifStmt:
                    CompileIfStatement(ifStmt);
                    break;

                case WhileStatement whileStmt:
                    CompileWhileStatement(whileStmt);
                    break;

                case ForStatement forStmt:
                    CompileForStatement(forStmt);
                    break;

                case BlockStatement block:
                    CompileBlock(block);
                    break;

                case PrintStatement print:
                    CompilePrintStatement(print);
                    break;

                case ReturnStatement ret:
                    CompileReturnStatement(ret);
                    break;

                case BreakStatement brk:
                    CompileBreakStatement(brk);
                    break;

                case ContinueStatement cont:
                    CompileContinueStatement(cont);
                    break;

                case ExpressionStatement exprStmt:
                    CompileExpression(exprStmt.Expression);
                    Emit(OpCode.POP);  // Discard result
                    break;
            }
        }

        /// <summary>
        /// Compiles a variable declaration.
        /// </summary>
        private void CompileDeclaration(DeclarationStatement decl)
        {
            foreach (var (id, expr) in decl.Declarations)
            {
                if (expr != null)
                {
                    // Compile initializer
                    CompileExpression(expr);
                    Emit(OpCode.STORE, id.Text, id.Line);
                }
            }
        }

        /// <summary>
        /// Compiles an assignment statement.
        /// </summary>
        private void CompileAssignment(AssignmentStatement assign)
        {
            CompileExpression(assign.Expression);
            Emit(OpCode.STORE, assign.Identifier.Text, assign.Identifier.Line);
        }

        /// <summary>
        /// Compiles a compound assignment statement.
        /// </summary>
        private void CompileCompoundAssignment(CompoundAssignmentStatement compAssign)
        {
            // Load current value
            Emit(OpCode.LOAD, compAssign.Identifier.Text, compAssign.Identifier.Line);

            // Compile right-hand side
            CompileExpression(compAssign.Expression);

            // Apply operation
            var opCode = compAssign.Operator.Type switch
            {
                TokenType.PlusEqual => OpCode.ADD,
                TokenType.MinusEqual => OpCode.SUB,
                TokenType.StarEqual => OpCode.MUL,
                TokenType.SlashEqual => OpCode.DIV,
                TokenType.PercentEqual => OpCode.MOD,
                _ => throw new InvalidOperationException($"Unknown compound operator: {compAssign.Operator.Type}")
            };

            Emit(opCode);

            // Store result
            Emit(OpCode.STORE, compAssign.Identifier.Text, compAssign.Identifier.Line);
        }

        /// <summary>
        /// Compiles an if statement.
        /// </summary>
        private void CompileIfStatement(IfStatement ifStmt)
        {
            var elseLabel = NewLabel();
            var endLabel = NewLabel();

            // Compile condition
            CompileExpression(ifStmt.Condition);

            // Jump to else if false
            Emit(OpCode.JMPF, elseLabel, ifStmt.IfKeyword.Line);

            // Compile then branch
            CompileStatement(ifStmt.ThenBody);

            if (ifStmt.ElseBody != null)
            {
                // Jump over else
                Emit(OpCode.JMP, endLabel);
            }

            // Else label
            MarkLabel(elseLabel);

            if (ifStmt.ElseBody != null)
            {
                // Compile else branch
                CompileStatement(ifStmt.ElseBody);
                MarkLabel(endLabel);
            }
        }

        /// <summary>
        /// Compiles a while statement.
        /// </summary>
        private void CompileWhileStatement(WhileStatement whileStmt)
        {
            var startLabel = NewLabel();
            var endLabel = NewLabel();

            // Register loop labels for break/continue
            string loopId = $"while_{_loopStack.Count}";
            _loopStack.Push(loopId);
            _loopStartLabels[loopId] = CurrentAddress;

            // Start label
            MarkLabel(startLabel);

            // Compile condition
            CompileExpression(whileStmt.Condition);

            // Jump to end if false
            Emit(OpCode.JMPF, endLabel, whileStmt.WhileKeyword.Line);

            // Register end label for break
            _loopEndLabels[loopId] = -1;  // Will be resolved

            // Compile body
            CompileStatement(whileStmt.Body);

            // Jump back to start
            Emit(OpCode.JMP, startLabel);

            // End label
            MarkLabel(endLabel);
            _loopEndLabels[loopId] = CurrentAddress;

            _loopStack.Pop();
        }

        /// <summary>
        /// Compiles a for statement.
        /// </summary>
        private void CompileForStatement(ForStatement forStmt)
        {
            var startLabel = NewLabel();
            var endLabel = NewLabel();
            var incrementLabel = NewLabel();

            // Register loop
            string loopId = $"for_{_loopStack.Count}";
            _loopStack.Push(loopId);

            // Compile initialization
            if (forStmt.Initialization != null)
            {
                CompileStatement(forStmt.Initialization);
            }

            // Start label (condition check)
            MarkLabel(startLabel);
            _loopStartLabels[loopId] = CurrentAddress;

            // Compile condition
            if (forStmt.Condition != null)
            {
                CompileExpression(forStmt.Condition);
                Emit(OpCode.JMPF, endLabel, forStmt.ForKeyword.Line);
            }

            // Compile body
            CompileStatement(forStmt.Body);

            // Increment label (for continue)
            MarkLabel(incrementLabel);

            // Compile increment
            if (forStmt.Increment != null)
            {
                CompileStatement(forStmt.Increment);
            }

            // Jump back to condition
            Emit(OpCode.JMP, startLabel);

            // End label
            MarkLabel(endLabel);
            _loopEndLabels[loopId] = CurrentAddress;

            _loopStack.Pop();
        }

        /// <summary>
        /// Compiles a block statement.
        /// </summary>
        private void CompileBlock(BlockStatement block)
        {
            foreach (var stmt in block.Statements)
            {
                CompileStatement(stmt);
            }
        }

        /// <summary>
        /// Compiles a print statement.
        /// </summary>
        private void CompilePrintStatement(PrintStatement print)
        {
            CompileExpression(print.Expression);
            Emit(OpCode.PRINTLN, print.PrintKeyword.Line);
        }

        /// <summary>
        /// Compiles a return statement.
        /// </summary>
        private void CompileReturnStatement(ReturnStatement ret)
        {
            if (ret.Expression != null)
            {
                CompileExpression(ret.Expression);
                Emit(OpCode.RET_VAL, ret.ReturnKeyword.Line);
            }
            else
            {
                Emit(OpCode.RET, ret.ReturnKeyword.Line);
            }
        }

        /// <summary>
        /// Compiles a break statement.
        /// </summary>
        private void CompileBreakStatement(BreakStatement brk)
        {
            if (_loopStack.Count == 0)
            {
                throw new InvalidOperationException("break outside of loop");
            }

            string loopId = _loopStack.Peek();
            // Jump to end label (will be resolved later)
            Emit(OpCode.JMP, $"{loopId}_end", brk.BreakKeyword.Line);
        }

        /// <summary>
        /// Compiles a continue statement.
        /// </summary>
        private void CompileContinueStatement(ContinueStatement cont)
        {
            if (_loopStack.Count == 0)
            {
                throw new InvalidOperationException("continue outside of loop");
            }

            string loopId = _loopStack.Peek();
            int startAddr = _loopStartLabels[loopId];
            Emit(OpCode.JMP, startAddr, cont.ContinueKeyword.Line);
        }

        #endregion

        #region Expression Compilation

        /// <summary>
        /// Compiles an expression using post-order traversal.
        /// </summary>
        private void CompileExpression(Expression expression)
        {
            if (expression == null) return;

            switch (expression)
            {
                case NumericExpression num:
                    CompileNumericExpression(num);
                    break;

                case StringExpression str:
                    CompileStringExpression(str);
                    break;

                case BooleanExpression boolExpr:
                    CompileBooleanExpression(boolExpr);
                    break;

                case IdentifierExpression id:
                    CompileIdentifierExpression(id);
                    break;

                case BinaryExpression binary:
                    CompileBinaryExpression(binary);
                    break;

                case UnaryExpression unary:
                    CompileUnaryExpression(unary);
                    break;

                case LogicalExpression logical:
                    CompileLogicalExpression(logical);
                    break;

                case NotExpression not:
                    CompileNotExpression(not);
                    break;

                case ParenthesizedExpression paren:
                    CompileExpression(paren.Expression);
                    break;

                case FunctionCallExpression call:
                    CompileFunctionCall(call);
                    break;

                case IncrementExpression inc:
                    CompileIncrementExpression(inc);
                    break;

                case ArrayAccessExpression arrayAccess:
                    CompileArrayAccess(arrayAccess);
                    break;
            }
        }

        /// <summary>
        /// Compiles a numeric literal.
        /// </summary>
        private void CompileNumericExpression(NumericExpression num)
        {
            Emit(OpCode.PUSH, num.Number.Value, num.Number.Line);
        }

        /// <summary>
        /// Compiles a string literal.
        /// </summary>
        private void CompileStringExpression(StringExpression str)
        {
            Emit(OpCode.PUSH, str.StringValue.Value, str.StringValue.Line);
        }

        /// <summary>
        /// Compiles a boolean literal.
        /// </summary>
        private void CompileBooleanExpression(BooleanExpression boolExpr)
        {
            Emit(OpCode.PUSH, boolExpr.Value, boolExpr.BoolToken.Line);
        }

        /// <summary>
        /// Compiles an identifier reference.
        /// </summary>
        private void CompileIdentifierExpression(IdentifierExpression id)
        {
            Emit(OpCode.LOAD, id.Identifier.Text, id.Identifier.Line);
        }

        /// <summary>
        /// Compiles a binary expression.
        /// </summary>
        private void CompileBinaryExpression(BinaryExpression binary)
        {
            // Post-order: compile operands first, then operator
            CompileExpression(binary.Left);
            CompileExpression(binary.Right);

            var opCode = binary.Operator.Type switch
            {
                TokenType.Plus => OpCode.ADD,
                TokenType.Minus => OpCode.SUB,
                TokenType.Star => OpCode.MUL,
                TokenType.Slash => OpCode.DIV,
                TokenType.Percent => OpCode.MOD,
                TokenType.LessThan => OpCode.LT,
                TokenType.GreaterThan => OpCode.GT,
                TokenType.LessThanOrEqual => OpCode.LE,
                TokenType.GreaterThanOrEqual => OpCode.GE,
                TokenType.EqualEqual => OpCode.EQ,
                TokenType.NotEqual => OpCode.NE,
                _ => throw new InvalidOperationException($"Unknown binary operator: {binary.Operator.Type}")
            };

            Emit(opCode, binary.Operator.Line);
        }

        /// <summary>
        /// Compiles a unary expression.
        /// </summary>
        private void CompileUnaryExpression(UnaryExpression unary)
        {
            CompileExpression(unary.Operand);

            if (unary.Operator.Type == TokenType.Minus)
            {
                Emit(OpCode.NEG, unary.Operator.Line);
            }
        }

        /// <summary>
        /// Compiles a logical expression with short-circuit evaluation.
        /// </summary>
        private void CompileLogicalExpression(LogicalExpression logical)
        {
            var endLabel = NewLabel();

            // Compile left operand
            CompileExpression(logical.Left);

            if (logical.Operator.Type == TokenType.LogicalAnd)
            {
                // Short-circuit AND: if left is false, skip right
                Emit(OpCode.DUP);
                Emit(OpCode.JMPF, endLabel);
                Emit(OpCode.POP);
            }
            else // LogicalOr
            {
                // Short-circuit OR: if left is true, skip right
                Emit(OpCode.DUP);
                Emit(OpCode.JMPT, endLabel);
                Emit(OpCode.POP);
            }

            // Compile right operand
            CompileExpression(logical.Right);

            MarkLabel(endLabel);
        }

        /// <summary>
        /// Compiles a NOT expression.
        /// </summary>
        private void CompileNotExpression(NotExpression not)
        {
            CompileExpression(not.Operand);
            Emit(OpCode.NOT, not.Operator.Line);
        }

        /// <summary>
        /// Compiles a function call.
        /// </summary>
        private void CompileFunctionCall(FunctionCallExpression call)
        {
            // Push arguments in order
            foreach (var arg in call.Arguments)
            {
                CompileExpression(arg);
            }

            // Check if it's a built-in function
            if (IsBuiltInFunction(call.FunctionName.Text))
            {
                Emit(OpCode.CALL_BUILTIN, call.FunctionName.Text, call.Arguments.Count, call.FunctionName.Line);
            }
            else
            {
                Emit(OpCode.CALL, call.FunctionName.Text, call.Arguments.Count, call.FunctionName.Line);
            }
        }

        /// <summary>
        /// Compiles an increment/decrement expression.
        /// </summary>
        private void CompileIncrementExpression(IncrementExpression inc)
        {
            // Load current value
            Emit(OpCode.LOAD, inc.Identifier.Text, inc.Identifier.Line);

            if (inc.IsPrefix)
            {
                // Prefix: increment first, then use
                Emit(inc.IsIncrement ? OpCode.INC : OpCode.DEC);
                Emit(OpCode.DUP);  // Keep value for expression result
                Emit(OpCode.STORE, inc.Identifier.Text);
            }
            else
            {
                // Postfix: use first, then increment
                Emit(OpCode.DUP);  // Keep original value for expression result
                Emit(inc.IsIncrement ? OpCode.INC : OpCode.DEC);
                Emit(OpCode.STORE, inc.Identifier.Text);
            }
        }

        /// <summary>
        /// Compiles an array access expression.
        /// </summary>
        private void CompileArrayAccess(ArrayAccessExpression arrayAccess)
        {
            // Get array name
            string arrayName = arrayAccess.Array switch
            {
                IdentifierExpression id => id.Identifier.Text,
                _ => throw new InvalidOperationException("Complex array access not supported")
            };

            // Compile index
            CompileExpression(arrayAccess.Index);

            // Load array element
            Emit(OpCode.ALOAD, arrayName, arrayAccess.OpenBracket.Line);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if a function is a built-in function.
        /// </summary>
        private bool IsBuiltInFunction(string name)
        {
            var builtIns = new HashSet<string>
            {
                "print", "sqrt", "abs", "exp", "log", "sin", "cos", "tan",
                "pow", "min", "max", "floor", "ceil", "round", "length",
                "input", "parseInt", "parseDouble", "toString"
            };

            return builtIns.Contains(name);
        }

        #endregion

        #region Display Methods

        /// <summary>
        /// Displays the compiled bytecode.
        /// </summary>
        public void DisplayBytecode()
        {
            _program.Display();
        }

        /// <summary>
        /// Gets compilation statistics.
        /// </summary>
        public string GetStatistics()
        {
            return _program.GetStatistics();
        }

        #endregion
    }
}
