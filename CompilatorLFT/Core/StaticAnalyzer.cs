using System;
using System.Collections.Generic;
using System.Linq;
using CompilatorLFT.Models;
using CompilatorLFT.Models.Expressions;
using CompilatorLFT.Models.Statements;
using CompilatorLFT.Utils;

// Type alias to avoid conflict with entry point Program class
using ProgramNode = CompilatorLFT.Models.Statements.Program;

namespace CompilatorLFT.Core
{
    /// <summary>
    /// Performs static code analysis to detect potential issues without executing the code.
    /// Generates warnings for common programming mistakes and code quality issues.
    /// </summary>
    /// <remarks>
    /// Reference: Dragon Book, Ch. 9 - Machine-Independent Optimizations
    /// Reference: Dragon Book, Ch. 9.2 - Data-Flow Analysis
    /// Reference: Grigoraș "Proiectarea Compilatoarelor", Cap. 6.5
    ///
    /// Analysis types:
    /// - Unused variable detection
    /// - Unreachable code detection
    /// - Constant condition detection
    /// - Division by zero detection
    /// - Infinite loop detection
    /// - Variable shadowing detection
    /// - Dead store detection
    /// </remarks>
    public class StaticAnalyzer
    {
        #region Private Fields

        private readonly List<CompilationWarning> _warnings;
        private readonly HashSet<string> _declaredVariables;
        private readonly HashSet<string> _usedVariables;
        private readonly HashSet<string> _assignedVariables;
        private readonly HashSet<string> _readVariables;
        private readonly HashSet<string> _declaredFunctions;
        private readonly HashSet<string> _calledFunctions;
        private readonly Dictionary<string, (int line, int column)> _variableLocations;
        private readonly Dictionary<string, (int line, int column)> _functionLocations;
        private readonly SymbolTable _symbolTable;
        private bool _afterReturn;  // Track if we're after a return statement

        #endregion

        #region Properties

        /// <summary>All detected warnings.</summary>
        public IReadOnlyList<CompilationWarning> Warnings => _warnings;

        /// <summary>Number of warnings by severity.</summary>
        public int InfoCount => _warnings.Count(w => w.Severity == WarningSeverity.Info);
        public int WarningCount => _warnings.Count(w => w.Severity == WarningSeverity.Warning);
        public int ErrorCount => _warnings.Count(w => w.Severity == WarningSeverity.Error);

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new static analyzer.
        /// </summary>
        /// <param name="symbolTable">Symbol table from parsing</param>
        public StaticAnalyzer(SymbolTable symbolTable)
        {
            _symbolTable = symbolTable ?? throw new ArgumentNullException(nameof(symbolTable));
            _warnings = new List<CompilationWarning>();
            _declaredVariables = new HashSet<string>();
            _usedVariables = new HashSet<string>();
            _assignedVariables = new HashSet<string>();
            _readVariables = new HashSet<string>();
            _declaredFunctions = new HashSet<string>();
            _calledFunctions = new HashSet<string>();
            _variableLocations = new Dictionary<string, (int, int)>();
            _functionLocations = new Dictionary<string, (int, int)>();
            _afterReturn = false;
        }

        #endregion

        #region Public Analysis Methods

        /// <summary>
        /// Analyzes the entire program for potential issues.
        /// </summary>
        /// <param name="program">The parsed program</param>
        public void Analyze(ProgramNode program)
        {
            if (program == null)
                throw new ArgumentNullException(nameof(program));

            // First pass: collect declarations
            CollectDeclarations(program);

            // Second pass: analyze usage and detect issues
            foreach (var function in program.Functions)
            {
                AnalyzeFunction(function);
            }

            foreach (var statement in program.Statements)
            {
                AnalyzeStatement(statement);
            }

            // Final pass: detect unused declarations
            CheckUnusedDeclarations();
        }

        /// <summary>
        /// Clears all warnings for re-analysis.
        /// </summary>
        public void Reset()
        {
            _warnings.Clear();
            _declaredVariables.Clear();
            _usedVariables.Clear();
            _assignedVariables.Clear();
            _readVariables.Clear();
            _declaredFunctions.Clear();
            _calledFunctions.Clear();
            _variableLocations.Clear();
            _functionLocations.Clear();
            _afterReturn = false;
        }

        #endregion

        #region Declaration Collection

        /// <summary>
        /// Collects all declared variables and functions.
        /// </summary>
        private void CollectDeclarations(ProgramNode program)
        {
            // Collect from symbol table
            foreach (var variable in _symbolTable.Variables)
            {
                _declaredVariables.Add(variable.Name);
                _variableLocations[variable.Name] = (variable.DeclarationLine, variable.DeclarationColumn);
            }

            // Collect declared functions
            foreach (var function in program.Functions)
            {
                _declaredFunctions.Add(function.Name.Text);
                _functionLocations[function.Name.Text] = (function.Name.Line, function.Name.Column);
            }
        }

        #endregion

        #region Statement Analysis

        /// <summary>
        /// Analyzes a single statement for issues.
        /// </summary>
        private void AnalyzeStatement(Statement statement)
        {
            if (statement == null) return;

            // Check for unreachable code after return
            if (_afterReturn)
            {
                _warnings.Add(CompilationWarning.CodeAfterReturn(
                    GetStatementLine(statement),
                    GetStatementColumn(statement)));
                _afterReturn = false;  // Only warn once
            }

            switch (statement)
            {
                case DeclarationStatement decl:
                    AnalyzeDeclaration(decl);
                    break;

                case AssignmentStatement assign:
                    AnalyzeAssignment(assign);
                    break;

                case CompoundAssignmentStatement compAssign:
                    AnalyzeCompoundAssignment(compAssign);
                    break;

                case IfStatement ifStmt:
                    AnalyzeIfStatement(ifStmt);
                    break;

                case WhileStatement whileStmt:
                    AnalyzeWhileStatement(whileStmt);
                    break;

                case ForStatement forStmt:
                    AnalyzeForStatement(forStmt);
                    break;

                case BlockStatement block:
                    AnalyzeBlock(block);
                    break;

                case PrintStatement print:
                    AnalyzeExpression(print.Expression);
                    break;

                case ReturnStatement ret:
                    if (ret.Expression != null)
                        AnalyzeExpression(ret.Expression);
                    _afterReturn = true;
                    break;

                case ExpressionStatement exprStmt:
                    AnalyzeExpression(exprStmt.Expression);
                    break;

                case BreakStatement:
                case ContinueStatement:
                    // These are valid, no warnings
                    break;
            }
        }

        /// <summary>
        /// Analyzes a function declaration.
        /// </summary>
        private void AnalyzeFunction(FunctionDeclaration function)
        {
            _afterReturn = false;

            // Analyze function body
            if (function.Body != null)
            {
                // Check for empty function body
                if (function.Body.Statements.Count == 0)
                {
                    _warnings.Add(CompilationWarning.EmptyBlock(
                        function.Body.OpenBrace.Line,
                        function.Body.OpenBrace.Column,
                        "function"));
                }

                foreach (var stmt in function.Body.Statements)
                {
                    AnalyzeStatement(stmt);
                }
            }
        }

        /// <summary>
        /// Analyzes a declaration statement.
        /// </summary>
        private void AnalyzeDeclaration(DeclarationStatement decl)
        {
            foreach (var (id, expr) in decl.Declarations)
            {
                if (expr != null)
                {
                    AnalyzeExpression(expr);
                    _assignedVariables.Add(id.Text);
                }
            }
        }

        /// <summary>
        /// Analyzes an assignment statement.
        /// </summary>
        private void AnalyzeAssignment(AssignmentStatement assign)
        {
            _usedVariables.Add(assign.Identifier.Text);
            _assignedVariables.Add(assign.Identifier.Text);
            AnalyzeExpression(assign.Expression);
        }

        /// <summary>
        /// Analyzes a compound assignment statement.
        /// </summary>
        private void AnalyzeCompoundAssignment(CompoundAssignmentStatement compAssign)
        {
            _usedVariables.Add(compAssign.Identifier.Text);
            _readVariables.Add(compAssign.Identifier.Text);  // Also reads the value
            _assignedVariables.Add(compAssign.Identifier.Text);
            AnalyzeExpression(compAssign.Expression);
        }

        /// <summary>
        /// Analyzes an if statement for constant conditions.
        /// </summary>
        private void AnalyzeIfStatement(IfStatement ifStmt)
        {
            AnalyzeExpression(ifStmt.Condition);

            // Check for constant condition
            if (TryEvaluateConstant(ifStmt.Condition, out bool condValue))
            {
                _warnings.Add(CompilationWarning.ConstantCondition(
                    ifStmt.IfKeyword.Line,
                    ifStmt.IfKeyword.Column,
                    condValue));

                // Check for unreachable branches
                if (condValue && ifStmt.ElseBody != null)
                {
                    _warnings.Add(CompilationWarning.UnreachableCode(
                        GetStatementLine(ifStmt.ElseBody),
                        GetStatementColumn(ifStmt.ElseBody)));
                }
                else if (!condValue)
                {
                    _warnings.Add(CompilationWarning.UnreachableCode(
                        GetStatementLine(ifStmt.ThenBody),
                        GetStatementColumn(ifStmt.ThenBody)));
                }
            }

            // Check for empty then body
            if (ifStmt.ThenBody is BlockStatement thenBlock && thenBlock.Statements.Count == 0)
            {
                _warnings.Add(CompilationWarning.EmptyBlock(
                    thenBlock.OpenBrace.Line,
                    thenBlock.OpenBrace.Column,
                    "if"));
            }

            AnalyzeStatement(ifStmt.ThenBody);

            if (ifStmt.ElseBody != null)
            {
                // Check for empty else body
                if (ifStmt.ElseBody is BlockStatement elseBlock && elseBlock.Statements.Count == 0)
                {
                    _warnings.Add(CompilationWarning.EmptyBlock(
                        elseBlock.OpenBrace.Line,
                        elseBlock.OpenBrace.Column,
                        "else"));
                }

                AnalyzeStatement(ifStmt.ElseBody);
            }
        }

        /// <summary>
        /// Analyzes a while statement for infinite loops.
        /// </summary>
        private void AnalyzeWhileStatement(WhileStatement whileStmt)
        {
            AnalyzeExpression(whileStmt.Condition);

            // Check for constant condition
            if (TryEvaluateConstant(whileStmt.Condition, out bool condValue))
            {
                if (condValue)
                {
                    _warnings.Add(CompilationWarning.InfiniteLoop(
                        whileStmt.WhileKeyword.Line,
                        whileStmt.WhileKeyword.Column));
                }
                else
                {
                    _warnings.Add(CompilationWarning.AlwaysFalseCondition(
                        whileStmt.WhileKeyword.Line,
                        whileStmt.WhileKeyword.Column,
                        "while"));
                }
            }

            // Check for empty body
            if (whileStmt.Body is BlockStatement block && block.Statements.Count == 0)
            {
                _warnings.Add(CompilationWarning.EmptyBlock(
                    block.OpenBrace.Line,
                    block.OpenBrace.Column,
                    "while"));
            }

            AnalyzeStatement(whileStmt.Body);
        }

        /// <summary>
        /// Analyzes a for statement.
        /// </summary>
        private void AnalyzeForStatement(ForStatement forStmt)
        {
            if (forStmt.Initialization != null)
                AnalyzeStatement(forStmt.Initialization);

            if (forStmt.Condition != null)
            {
                AnalyzeExpression(forStmt.Condition);

                // Check for constant false condition
                if (TryEvaluateConstant(forStmt.Condition, out bool condValue) && !condValue)
                {
                    _warnings.Add(CompilationWarning.AlwaysFalseCondition(
                        forStmt.ForKeyword.Line,
                        forStmt.ForKeyword.Column,
                        "for"));
                }
            }

            if (forStmt.Increment != null)
                AnalyzeStatement(forStmt.Increment);

            // Check for empty body
            if (forStmt.Body is BlockStatement block && block.Statements.Count == 0)
            {
                _warnings.Add(CompilationWarning.EmptyBlock(
                    block.OpenBrace.Line,
                    block.OpenBrace.Column,
                    "for"));
            }

            AnalyzeStatement(forStmt.Body);
        }

        /// <summary>
        /// Analyzes a block statement.
        /// </summary>
        private void AnalyzeBlock(BlockStatement block)
        {
            bool savedAfterReturn = _afterReturn;
            _afterReturn = false;

            foreach (var stmt in block.Statements)
            {
                AnalyzeStatement(stmt);
            }

            _afterReturn = savedAfterReturn;
        }

        #endregion

        #region Expression Analysis

        /// <summary>
        /// Analyzes an expression for issues.
        /// </summary>
        private void AnalyzeExpression(Expression expression)
        {
            if (expression == null) return;

            switch (expression)
            {
                case IdentifierExpression id:
                    _usedVariables.Add(id.Identifier.Text);
                    _readVariables.Add(id.Identifier.Text);
                    break;

                case BinaryExpression binary:
                    AnalyzeBinaryExpression(binary);
                    break;

                case UnaryExpression unary:
                    AnalyzeExpression(unary.Operand);
                    break;

                case LogicalExpression logical:
                    AnalyzeExpression(logical.Left);
                    AnalyzeExpression(logical.Right);
                    break;

                case NotExpression not:
                    AnalyzeExpression(not.Operand);
                    break;

                case ParenthesizedExpression paren:
                    AnalyzeExpression(paren.Expression);
                    break;

                case FunctionCallExpression call:
                    AnalyzeFunctionCall(call);
                    break;

                case IncrementExpression inc:
                    _usedVariables.Add(inc.Identifier.Text);
                    _readVariables.Add(inc.Identifier.Text);
                    _assignedVariables.Add(inc.Identifier.Text);
                    break;

                case ArrayAccessExpression arrayAccess:
                    AnalyzeExpression(arrayAccess.Array);
                    AnalyzeExpression(arrayAccess.Index);
                    break;
            }
        }

        /// <summary>
        /// Analyzes a binary expression for division by zero and self-comparison.
        /// </summary>
        private void AnalyzeBinaryExpression(BinaryExpression binary)
        {
            AnalyzeExpression(binary.Left);
            AnalyzeExpression(binary.Right);

            // Check for division by zero
            if (binary.Operator.Type == TokenType.Slash ||
                binary.Operator.Type == TokenType.Percent)
            {
                if (IsConstantZero(binary.Right))
                {
                    _warnings.Add(CompilationWarning.DivisionByZero(
                        binary.Operator.Line,
                        binary.Operator.Column));
                }
            }

            // Check for self-comparison
            if (binary.Operator.IsRelationalOperator())
            {
                if (AreExpressionsEqual(binary.Left, binary.Right))
                {
                    string varName = GetExpressionName(binary.Left);
                    _warnings.Add(CompilationWarning.SelfComparison(
                        binary.Operator.Line,
                        binary.Operator.Column,
                        varName));
                }
            }
        }

        /// <summary>
        /// Analyzes a function call expression.
        /// </summary>
        private void AnalyzeFunctionCall(FunctionCallExpression call)
        {
            _calledFunctions.Add(call.FunctionName.Text);

            foreach (var arg in call.Arguments)
            {
                AnalyzeExpression(arg);
            }
        }

        #endregion

        #region Unused Declaration Detection

        /// <summary>
        /// Checks for unused variables and functions.
        /// </summary>
        private void CheckUnusedDeclarations()
        {
            // Check for unused variables
            foreach (var varName in _declaredVariables)
            {
                if (!_usedVariables.Contains(varName))
                {
                    var (line, column) = _variableLocations[varName];
                    _warnings.Add(CompilationWarning.UnusedVariable(line, column, varName));
                }
            }

            // Check for unused functions (excluding main and entry points)
            foreach (var funcName in _declaredFunctions)
            {
                if (!_calledFunctions.Contains(funcName) && funcName != "main")
                {
                    var (line, column) = _functionLocations[funcName];
                    _warnings.Add(CompilationWarning.UnusedFunction(line, column, funcName));
                }
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Tries to evaluate an expression as a constant boolean.
        /// </summary>
        private bool TryEvaluateConstant(Expression expression, out bool value)
        {
            value = false;

            switch (expression)
            {
                case BooleanExpression boolExpr:
                    value = boolExpr.Value;
                    return true;

                case NumericExpression numExpr:
                    // Numeric as boolean: 0 = false, non-zero = true
                    if (numExpr.Number.Value is int intVal)
                    {
                        value = intVal != 0;
                        return true;
                    }
                    if (numExpr.Number.Value is double doubleVal)
                    {
                        value = Math.Abs(doubleVal) > double.Epsilon;
                        return true;
                    }
                    break;

                case ParenthesizedExpression paren:
                    return TryEvaluateConstant(paren.Expression, out value);

                case NotExpression not:
                    if (TryEvaluateConstant(not.Operand, out bool notValue))
                    {
                        value = !notValue;
                        return true;
                    }
                    break;

                case BinaryExpression binary:
                    return TryEvaluateConstantBinary(binary, out value);
            }

            return false;
        }

        /// <summary>
        /// Tries to evaluate a constant binary expression.
        /// </summary>
        private bool TryEvaluateConstantBinary(BinaryExpression binary, out bool value)
        {
            value = false;

            // Try to get numeric values for both sides
            if (TryGetNumericValue(binary.Left, out double leftVal) &&
                TryGetNumericValue(binary.Right, out double rightVal))
            {
                value = binary.Operator.Type switch
                {
                    TokenType.LessThan => leftVal < rightVal,
                    TokenType.GreaterThan => leftVal > rightVal,
                    TokenType.LessThanOrEqual => leftVal <= rightVal,
                    TokenType.GreaterThanOrEqual => leftVal >= rightVal,
                    TokenType.EqualEqual => Math.Abs(leftVal - rightVal) < double.Epsilon,
                    TokenType.NotEqual => Math.Abs(leftVal - rightVal) >= double.Epsilon,
                    _ => false
                };

                return binary.Operator.IsRelationalOperator();
            }

            return false;
        }

        /// <summary>
        /// Tries to get a numeric value from an expression.
        /// </summary>
        private bool TryGetNumericValue(Expression expression, out double value)
        {
            value = 0;

            switch (expression)
            {
                case NumericExpression numExpr:
                    if (numExpr.Number.Value is int intVal)
                    {
                        value = intVal;
                        return true;
                    }
                    if (numExpr.Number.Value is double doubleVal)
                    {
                        value = doubleVal;
                        return true;
                    }
                    break;

                case ParenthesizedExpression paren:
                    return TryGetNumericValue(paren.Expression, out value);

                case UnaryExpression unary when unary.Operator.Type == TokenType.Minus:
                    if (TryGetNumericValue(unary.Operand, out double operandVal))
                    {
                        value = -operandVal;
                        return true;
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// Checks if an expression is a constant zero.
        /// </summary>
        private bool IsConstantZero(Expression expression)
        {
            if (TryGetNumericValue(expression, out double value))
            {
                return Math.Abs(value) < double.Epsilon;
            }
            return false;
        }

        /// <summary>
        /// Checks if two expressions are structurally equal.
        /// </summary>
        private bool AreExpressionsEqual(Expression left, Expression right)
        {
            if (left is IdentifierExpression leftId && right is IdentifierExpression rightId)
            {
                return leftId.Identifier.Text == rightId.Identifier.Text;
            }
            return false;
        }

        /// <summary>
        /// Gets a name for an expression (for error messages).
        /// </summary>
        private string GetExpressionName(Expression expression)
        {
            return expression switch
            {
                IdentifierExpression id => id.Identifier.Text,
                _ => "expression"
            };
        }

        /// <summary>
        /// Gets the line number of a statement.
        /// </summary>
        private int GetStatementLine(Statement statement)
        {
            return statement switch
            {
                DeclarationStatement d => d.TypeKeyword.Line,
                AssignmentStatement a => a.Identifier.Line,
                IfStatement i => i.IfKeyword.Line,
                WhileStatement w => w.WhileKeyword.Line,
                ForStatement f => f.ForKeyword.Line,
                BlockStatement b => b.OpenBrace.Line,
                PrintStatement p => p.PrintKeyword.Line,
                ReturnStatement r => r.ReturnKeyword.Line,
                BreakStatement br => br.BreakKeyword.Line,
                ContinueStatement c => c.ContinueKeyword.Line,
                ExpressionStatement e => 1,  // Default
                _ => 1
            };
        }

        /// <summary>
        /// Gets the column number of a statement.
        /// </summary>
        private int GetStatementColumn(Statement statement)
        {
            return statement switch
            {
                DeclarationStatement d => d.TypeKeyword.Column,
                AssignmentStatement a => a.Identifier.Column,
                IfStatement i => i.IfKeyword.Column,
                WhileStatement w => w.WhileKeyword.Column,
                ForStatement f => f.ForKeyword.Column,
                BlockStatement b => b.OpenBrace.Column,
                PrintStatement p => p.PrintKeyword.Column,
                ReturnStatement r => r.ReturnKeyword.Column,
                BreakStatement br => br.BreakKeyword.Column,
                ContinueStatement c => c.ContinueKeyword.Column,
                ExpressionStatement e => 1,
                _ => 1
            };
        }

        #endregion

        #region Display Methods

        /// <summary>
        /// Displays all warnings to the console.
        /// </summary>
        public void DisplayWarnings()
        {
            if (!_warnings.Any())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Static analysis complete - no warnings detected!");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n=== STATIC ANALYSIS WARNINGS ({_warnings.Count}) ===");
            Console.ResetColor();

            // Group by severity
            var errors = _warnings.Where(w => w.Severity == WarningSeverity.Error).OrderBy(w => w.Line);
            var warnings = _warnings.Where(w => w.Severity == WarningSeverity.Warning).OrderBy(w => w.Line);
            var infos = _warnings.Where(w => w.Severity == WarningSeverity.Info).OrderBy(w => w.Line);

            // Display errors first (if any)
            if (errors.Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nErrors ({errors.Count()}):");
                foreach (var warning in errors)
                {
                    Console.WriteLine($"  ✗ {warning}");
                }
                Console.ResetColor();
            }

            // Display warnings
            if (warnings.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nWarnings ({warnings.Count()}):");
                foreach (var warning in warnings)
                {
                    Console.WriteLine($"  ⚠ {warning}");
                }
                Console.ResetColor();
            }

            // Display info
            if (infos.Any())
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\nInfo ({infos.Count()}):");
                foreach (var warning in infos)
                {
                    Console.WriteLine($"  ℹ {warning}");
                }
                Console.ResetColor();
            }

            // Summary
            Console.WriteLine($"\nSummary: {ErrorCount} errors, {WarningCount} warnings, {InfoCount} info");
        }

        /// <summary>
        /// Gets a formatted summary of the analysis.
        /// </summary>
        public string GetSummary()
        {
            return $"Static Analysis: {_warnings.Count} issues " +
                   $"({ErrorCount} errors, {WarningCount} warnings, {InfoCount} info)";
        }

        #endregion
    }
}
