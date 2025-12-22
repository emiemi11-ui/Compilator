using System;
using System.Collections.Generic;
using System.Linq;
using CompilatorLFT.Models;
using CompilatorLFT.Models.Expressions;
using CompilatorLFT.Models.Statements;

// Type alias to avoid conflict with entry point Program class
using ProgramNode = CompilatorLFT.Models.Statements.Program;

namespace CompilatorLFT.Core
{
    /// <summary>
    /// Performs machine-independent code optimizations on the AST.
    /// Implements various optimization techniques from compiler theory.
    /// </summary>
    /// <remarks>
    /// Reference: Dragon Book, Ch. 9 - Machine-Independent Optimizations
    /// Reference: Dragon Book, Ch. 9.2 - Constant Folding
    /// Reference: Dragon Book, Ch. 9.5 - Dead Code Elimination
    /// Reference: Grigoraș "Proiectarea Compilatoarelor", Cap. 6.5
    ///
    /// Optimization techniques implemented:
    /// 1. Constant Folding - Evaluate constant expressions at compile time
    /// 2. Dead Code Elimination - Remove unreachable or unused code
    /// 3. Constant Propagation - Replace variables with known constant values
    /// 4. Algebraic Simplification - Simplify expressions using algebraic identities
    /// 5. Strength Reduction - Replace expensive operations with cheaper ones
    /// </remarks>
    public class CodeOptimizer
    {
        #region Private Fields

        private int _constantFoldingCount;
        private int _deadCodeEliminationCount;
        private int _algebraicSimplificationCount;
        private int _strengthReductionCount;
        private int _totalOptimizations;
        private readonly Dictionary<string, object> _knownConstants;
        private readonly List<string> _optimizationLog;

        #endregion

        #region Properties

        /// <summary>Total number of optimizations applied.</summary>
        public int TotalOptimizations => _totalOptimizations;

        /// <summary>Number of constant folding optimizations.</summary>
        public int ConstantFoldingCount => _constantFoldingCount;

        /// <summary>Number of dead code eliminations.</summary>
        public int DeadCodeEliminationCount => _deadCodeEliminationCount;

        /// <summary>Number of algebraic simplifications.</summary>
        public int AlgebraicSimplificationCount => _algebraicSimplificationCount;

        /// <summary>Number of strength reductions.</summary>
        public int StrengthReductionCount => _strengthReductionCount;

        /// <summary>Detailed log of optimizations applied.</summary>
        public IReadOnlyList<string> OptimizationLog => _optimizationLog;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new code optimizer.
        /// </summary>
        public CodeOptimizer()
        {
            _knownConstants = new Dictionary<string, object>();
            _optimizationLog = new List<string>();
            Reset();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Optimizes the entire program.
        /// </summary>
        /// <param name="program">The program to optimize</param>
        /// <returns>The optimized program</returns>
        public ProgramNode Optimize(ProgramNode program)
        {
            if (program == null)
                throw new ArgumentNullException(nameof(program));

            Reset();

            // Multiple optimization passes for better results
            int passCount = 0;
            int prevOptimizations;

            do
            {
                prevOptimizations = _totalOptimizations;
                passCount++;

                // Optimize all statements
                var optimizedStatements = new List<Statement>();
                foreach (var statement in program.Statements)
                {
                    var optimized = OptimizeStatement(statement);
                    if (optimized != null)
                    {
                        optimizedStatements.Add(optimized);
                    }
                }
                program.Statements.Clear();
                program.Statements.AddRange(optimizedStatements);

                // Optimize all functions
                foreach (var function in program.Functions)
                {
                    OptimizeFunction(function);
                }

            } while (_totalOptimizations > prevOptimizations && passCount < 5);

            _optimizationLog.Add($"Optimization complete after {passCount} passes");

            return program;
        }

        /// <summary>
        /// Resets optimization counters.
        /// </summary>
        public void Reset()
        {
            _constantFoldingCount = 0;
            _deadCodeEliminationCount = 0;
            _algebraicSimplificationCount = 0;
            _strengthReductionCount = 0;
            _totalOptimizations = 0;
            _knownConstants.Clear();
            _optimizationLog.Clear();
        }

        #endregion

        #region Statement Optimization

        /// <summary>
        /// Optimizes a single statement.
        /// </summary>
        /// <returns>The optimized statement, or null if eliminated</returns>
        private Statement OptimizeStatement(Statement statement)
        {
            if (statement == null) return null;

            return statement switch
            {
                DeclarationStatement decl => OptimizeDeclaration(decl),
                AssignmentStatement assign => OptimizeAssignment(assign),
                CompoundAssignmentStatement compAssign => OptimizeCompoundAssignment(compAssign),
                IfStatement ifStmt => OptimizeIfStatement(ifStmt),
                WhileStatement whileStmt => OptimizeWhileStatement(whileStmt),
                ForStatement forStmt => OptimizeForStatement(forStmt),
                BlockStatement block => OptimizeBlock(block),
                PrintStatement print => OptimizePrintStatement(print),
                ReturnStatement ret => OptimizeReturnStatement(ret),
                ExpressionStatement exprStmt => OptimizeExpressionStatement(exprStmt),
                _ => statement  // Keep unchanged
            };
        }

        /// <summary>
        /// Optimizes a function declaration.
        /// </summary>
        private void OptimizeFunction(FunctionDeclaration function)
        {
            if (function?.Body == null) return;

            var optimizedBody = OptimizeBlock(function.Body);
            // Note: BlockStatement is sealed, so we can't replace it directly
            // We optimize the statements in place
        }

        /// <summary>
        /// Optimizes a declaration statement.
        /// </summary>
        private DeclarationStatement OptimizeDeclaration(DeclarationStatement decl)
        {
            var optimizedDeclarations = new List<(Token, Expression)>();

            foreach (var (id, expr) in decl.Declarations)
            {
                Expression optimizedExpr = expr;

                if (expr != null)
                {
                    optimizedExpr = OptimizeExpression(expr);

                    // Track constant values for propagation
                    if (IsConstantExpression(optimizedExpr, out object constValue))
                    {
                        _knownConstants[id.Text] = constValue;
                    }
                }

                optimizedDeclarations.Add((id, optimizedExpr));
            }

            return new DeclarationStatement(decl.TypeKeyword, optimizedDeclarations, decl.Semicolon);
        }

        /// <summary>
        /// Optimizes an assignment statement.
        /// </summary>
        private AssignmentStatement OptimizeAssignment(AssignmentStatement assign)
        {
            var optimizedExpr = OptimizeExpression(assign.Expression);

            // Track constant values for propagation
            if (IsConstantExpression(optimizedExpr, out object constValue))
            {
                _knownConstants[assign.Identifier.Text] = constValue;
            }
            else
            {
                // Variable is no longer constant
                _knownConstants.Remove(assign.Identifier.Text);
            }

            if (optimizedExpr != assign.Expression)
            {
                return new AssignmentStatement(
                    assign.Identifier,
                    assign.AssignOperator,
                    optimizedExpr,
                    assign.Semicolon);
            }

            return assign;
        }

        /// <summary>
        /// Optimizes a compound assignment statement.
        /// </summary>
        private Statement OptimizeCompoundAssignment(CompoundAssignmentStatement compAssign)
        {
            var optimizedExpr = OptimizeExpression(compAssign.Expression);

            // Variable is no longer constant after modification
            _knownConstants.Remove(compAssign.Identifier.Text);

            if (optimizedExpr != compAssign.Expression)
            {
                return new CompoundAssignmentStatement(
                    compAssign.Identifier,
                    compAssign.Operator,
                    optimizedExpr,
                    compAssign.Semicolon);
            }

            return compAssign;
        }

        /// <summary>
        /// Optimizes an if statement with dead code elimination.
        /// </summary>
        private Statement OptimizeIfStatement(IfStatement ifStmt)
        {
            var optimizedCondition = OptimizeExpression(ifStmt.Condition);

            // Check if condition is constant
            if (IsConstantExpression(optimizedCondition, out object condValue))
            {
                bool condBool = ConvertToBool(condValue);

                if (condBool)
                {
                    // Condition always true - keep then body, eliminate else
                    LogOptimization("Dead code elimination", "if-else with always-true condition");
                    _deadCodeEliminationCount++;
                    _totalOptimizations++;

                    return OptimizeStatement(ifStmt.ThenBody);
                }
                else
                {
                    // Condition always false - keep else body (if exists), eliminate if
                    LogOptimization("Dead code elimination", "if-else with always-false condition");
                    _deadCodeEliminationCount++;
                    _totalOptimizations++;

                    if (ifStmt.ElseBody != null)
                    {
                        return OptimizeStatement(ifStmt.ElseBody);
                    }
                    else
                    {
                        return null;  // Eliminate entire if statement
                    }
                }
            }

            // Optimize then and else bodies
            var optimizedThen = OptimizeStatement(ifStmt.ThenBody);
            Statement optimizedElse = ifStmt.ElseBody != null
                ? OptimizeStatement(ifStmt.ElseBody)
                : null;

            // If both branches are empty, eliminate
            if (optimizedThen == null && optimizedElse == null)
            {
                LogOptimization("Dead code elimination", "if with empty branches");
                _deadCodeEliminationCount++;
                _totalOptimizations++;
                return null;
            }

            return new IfStatement(
                ifStmt.IfKeyword,
                ifStmt.OpenParen,
                optimizedCondition,
                ifStmt.CloseParen,
                optimizedThen ?? new BlockStatement(ifStmt.ThenBody is BlockStatement b ? b.OpenBrace : null,
                    new List<Statement>(),
                    ifStmt.ThenBody is BlockStatement b2 ? b2.CloseBrace : null),
                ifStmt.ElseKeyword,
                optimizedElse);
        }

        /// <summary>
        /// Optimizes a while statement.
        /// </summary>
        private Statement OptimizeWhileStatement(WhileStatement whileStmt)
        {
            var optimizedCondition = OptimizeExpression(whileStmt.Condition);

            // Check if condition is constant false
            if (IsConstantExpression(optimizedCondition, out object condValue))
            {
                if (!ConvertToBool(condValue))
                {
                    // Condition always false - eliminate entire loop
                    LogOptimization("Dead code elimination", "while with always-false condition");
                    _deadCodeEliminationCount++;
                    _totalOptimizations++;
                    return null;
                }
            }

            var optimizedBody = OptimizeStatement(whileStmt.Body);

            return new WhileStatement(
                whileStmt.WhileKeyword,
                whileStmt.OpenParen,
                optimizedCondition,
                whileStmt.CloseParen,
                optimizedBody ?? whileStmt.Body);
        }

        /// <summary>
        /// Optimizes a for statement.
        /// </summary>
        private Statement OptimizeForStatement(ForStatement forStmt)
        {
            Statement optimizedInit = forStmt.Initialization != null
                ? OptimizeStatement(forStmt.Initialization)
                : null;

            Expression optimizedCondition = forStmt.Condition != null
                ? OptimizeExpression(forStmt.Condition)
                : null;

            Statement optimizedIncrement = forStmt.Increment != null
                ? OptimizeStatement(forStmt.Increment)
                : null;

            // Check if condition is constant false
            if (optimizedCondition != null &&
                IsConstantExpression(optimizedCondition, out object condValue))
            {
                if (!ConvertToBool(condValue))
                {
                    // Loop never executes - keep only initialization
                    LogOptimization("Dead code elimination", "for with always-false condition");
                    _deadCodeEliminationCount++;
                    _totalOptimizations++;
                    return optimizedInit;
                }
            }

            var optimizedBody = OptimizeStatement(forStmt.Body);

            return new ForStatement(
                forStmt.ForKeyword,
                forStmt.OpenParen,
                optimizedInit,
                optimizedCondition,
                forStmt.Semicolon,
                optimizedIncrement,
                forStmt.CloseParen,
                optimizedBody ?? forStmt.Body);
        }

        /// <summary>
        /// Optimizes a block statement.
        /// </summary>
        private BlockStatement OptimizeBlock(BlockStatement block)
        {
            var optimizedStatements = new List<Statement>();
            bool foundReturn = false;

            foreach (var stmt in block.Statements)
            {
                if (foundReturn)
                {
                    // Dead code after return
                    LogOptimization("Dead code elimination", "code after return");
                    _deadCodeEliminationCount++;
                    _totalOptimizations++;
                    continue;
                }

                var optimized = OptimizeStatement(stmt);
                if (optimized != null)
                {
                    optimizedStatements.Add(optimized);

                    if (stmt is ReturnStatement)
                    {
                        foundReturn = true;
                    }
                }
            }

            return new BlockStatement(block.OpenBrace, optimizedStatements, block.CloseBrace);
        }

        /// <summary>
        /// Optimizes a print statement.
        /// </summary>
        private PrintStatement OptimizePrintStatement(PrintStatement print)
        {
            var optimizedExpr = OptimizeExpression(print.Expression);

            return new PrintStatement(
                print.PrintKeyword,
                print.OpenParen,
                optimizedExpr,
                print.CloseParen,
                print.Semicolon);
        }

        /// <summary>
        /// Optimizes a return statement.
        /// </summary>
        private ReturnStatement OptimizeReturnStatement(ReturnStatement ret)
        {
            if (ret.Expression == null)
                return ret;

            var optimizedExpr = OptimizeExpression(ret.Expression);

            return new ReturnStatement(ret.ReturnKeyword, optimizedExpr, ret.Semicolon);
        }

        /// <summary>
        /// Optimizes an expression statement.
        /// </summary>
        private ExpressionStatement OptimizeExpressionStatement(ExpressionStatement exprStmt)
        {
            var optimizedExpr = OptimizeExpression(exprStmt.Expression);

            return new ExpressionStatement(optimizedExpr, exprStmt.Semicolon);
        }

        #endregion

        #region Expression Optimization

        /// <summary>
        /// Optimizes an expression.
        /// </summary>
        private Expression OptimizeExpression(Expression expression)
        {
            if (expression == null) return null;

            return expression switch
            {
                BinaryExpression binary => OptimizeBinaryExpression(binary),
                UnaryExpression unary => OptimizeUnaryExpression(unary),
                ParenthesizedExpression paren => OptimizeParenthesizedExpression(paren),
                LogicalExpression logical => OptimizeLogicalExpression(logical),
                NotExpression not => OptimizeNotExpression(not),
                IdentifierExpression id => OptimizeIdentifierExpression(id),
                _ => expression  // NumericExpression, StringExpression, etc. - already optimal
            };
        }

        /// <summary>
        /// Optimizes a binary expression with constant folding and algebraic simplification.
        /// </summary>
        private Expression OptimizeBinaryExpression(BinaryExpression binary)
        {
            var left = OptimizeExpression(binary.Left);
            var right = OptimizeExpression(binary.Right);

            // Constant folding: both operands are constant
            if (IsConstantExpression(left, out object leftVal) &&
                IsConstantExpression(right, out object rightVal))
            {
                var result = EvaluateBinaryOperation(leftVal, binary.Operator, rightVal);
                if (result != null)
                {
                    LogOptimization("Constant folding", $"{leftVal} {binary.Operator.Text} {rightVal} = {result}");
                    _constantFoldingCount++;
                    _totalOptimizations++;
                    return CreateConstantExpression(result, binary.Operator.Line, binary.Operator.Column);
                }
            }

            // Algebraic simplification
            var simplified = ApplyAlgebraicSimplification(left, binary.Operator, right);
            if (simplified != null)
            {
                return simplified;
            }

            // Strength reduction
            var reduced = ApplyStrengthReduction(left, binary.Operator, right);
            if (reduced != null)
            {
                return reduced;
            }

            // Return with optimized operands
            if (left != binary.Left || right != binary.Right)
            {
                return new BinaryExpression(left, binary.Operator, right);
            }

            return binary;
        }

        /// <summary>
        /// Optimizes a unary expression.
        /// </summary>
        private Expression OptimizeUnaryExpression(UnaryExpression unary)
        {
            var operand = OptimizeExpression(unary.Operand);

            // Constant folding: -constant
            if (unary.Operator.Type == TokenType.Minus &&
                IsConstantExpression(operand, out object val))
            {
                if (val is int intVal)
                {
                    LogOptimization("Constant folding", $"-{intVal} = {-intVal}");
                    _constantFoldingCount++;
                    _totalOptimizations++;
                    return CreateConstantExpression(-intVal, unary.Operator.Line, unary.Operator.Column);
                }
                if (val is double doubleVal)
                {
                    LogOptimization("Constant folding", $"-{doubleVal} = {-doubleVal}");
                    _constantFoldingCount++;
                    _totalOptimizations++;
                    return CreateConstantExpression(-doubleVal, unary.Operator.Line, unary.Operator.Column);
                }
            }

            // Double negation: --x = x
            if (operand is UnaryExpression innerUnary &&
                unary.Operator.Type == TokenType.Minus &&
                innerUnary.Operator.Type == TokenType.Minus)
            {
                LogOptimization("Algebraic simplification", "--x = x");
                _algebraicSimplificationCount++;
                _totalOptimizations++;
                return innerUnary.Operand;
            }

            if (operand != unary.Operand)
            {
                return new UnaryExpression(unary.Operator, operand);
            }

            return unary;
        }

        /// <summary>
        /// Optimizes a parenthesized expression.
        /// </summary>
        private Expression OptimizeParenthesizedExpression(ParenthesizedExpression paren)
        {
            var inner = OptimizeExpression(paren.Expression);

            // Remove unnecessary parentheses around simple expressions
            if (inner is NumericExpression || inner is IdentifierExpression ||
                inner is StringExpression || inner is BooleanExpression)
            {
                LogOptimization("Simplification", "removed unnecessary parentheses");
                _algebraicSimplificationCount++;
                _totalOptimizations++;
                return inner;
            }

            if (inner != paren.Expression)
            {
                return new ParenthesizedExpression(paren.OpenParen, inner, paren.CloseParen);
            }

            return paren;
        }

        /// <summary>
        /// Optimizes a logical expression.
        /// </summary>
        private Expression OptimizeLogicalExpression(LogicalExpression logical)
        {
            var left = OptimizeExpression(logical.Left);
            var right = OptimizeExpression(logical.Right);

            // Short-circuit with constant left operand
            if (IsConstantExpression(left, out object leftVal))
            {
                bool leftBool = ConvertToBool(leftVal);

                if (logical.Operator.Type == TokenType.LogicalAnd)
                {
                    if (!leftBool)
                    {
                        // false && x = false
                        LogOptimization("Constant folding", "false && x = false");
                        _constantFoldingCount++;
                        _totalOptimizations++;
                        return CreateBooleanExpression(false, logical.Operator.Line, logical.Operator.Column);
                    }
                    else
                    {
                        // true && x = x
                        LogOptimization("Algebraic simplification", "true && x = x");
                        _algebraicSimplificationCount++;
                        _totalOptimizations++;
                        return right;
                    }
                }
                else if (logical.Operator.Type == TokenType.LogicalOr)
                {
                    if (leftBool)
                    {
                        // true || x = true
                        LogOptimization("Constant folding", "true || x = true");
                        _constantFoldingCount++;
                        _totalOptimizations++;
                        return CreateBooleanExpression(true, logical.Operator.Line, logical.Operator.Column);
                    }
                    else
                    {
                        // false || x = x
                        LogOptimization("Algebraic simplification", "false || x = x");
                        _algebraicSimplificationCount++;
                        _totalOptimizations++;
                        return right;
                    }
                }
            }

            if (left != logical.Left || right != logical.Right)
            {
                return new LogicalExpression(left, logical.Operator, right);
            }

            return logical;
        }

        /// <summary>
        /// Optimizes a NOT expression.
        /// </summary>
        private Expression OptimizeNotExpression(NotExpression not)
        {
            var operand = OptimizeExpression(not.Operand);

            // !constant
            if (IsConstantExpression(operand, out object val))
            {
                bool result = !ConvertToBool(val);
                LogOptimization("Constant folding", $"!{val} = {result}");
                _constantFoldingCount++;
                _totalOptimizations++;
                return CreateBooleanExpression(result, not.Operator.Line, not.Operator.Column);
            }

            // Double negation: !!x = x (for boolean)
            if (operand is NotExpression innerNot)
            {
                LogOptimization("Algebraic simplification", "!!x = x");
                _algebraicSimplificationCount++;
                _totalOptimizations++;
                return innerNot.Operand;
            }

            if (operand != not.Operand)
            {
                return new NotExpression(not.Operator, operand);
            }

            return not;
        }

        /// <summary>
        /// Optimizes an identifier expression with constant propagation.
        /// </summary>
        private Expression OptimizeIdentifierExpression(IdentifierExpression id)
        {
            // Constant propagation: replace variable with known constant value
            if (_knownConstants.TryGetValue(id.Identifier.Text, out object constValue))
            {
                LogOptimization("Constant propagation", $"{id.Identifier.Text} = {constValue}");
                _constantFoldingCount++;
                _totalOptimizations++;
                return CreateConstantExpression(constValue, id.Identifier.Line, id.Identifier.Column);
            }

            return id;
        }

        #endregion

        #region Algebraic Simplification

        /// <summary>
        /// Applies algebraic identities to simplify expressions.
        /// </summary>
        private Expression ApplyAlgebraicSimplification(Expression left, Token op, Expression right)
        {
            // Check for x + 0 = x, x - 0 = x, x * 1 = x, x / 1 = x
            if (IsConstantExpression(right, out object rightVal))
            {
                // x + 0 = x, x - 0 = x
                if ((op.Type == TokenType.Plus || op.Type == TokenType.Minus) &&
                    IsZero(rightVal))
                {
                    LogOptimization("Algebraic simplification", $"x {op.Text} 0 = x");
                    _algebraicSimplificationCount++;
                    _totalOptimizations++;
                    return left;
                }

                // x * 1 = x, x / 1 = x
                if ((op.Type == TokenType.Star || op.Type == TokenType.Slash) &&
                    IsOne(rightVal))
                {
                    LogOptimization("Algebraic simplification", $"x {op.Text} 1 = x");
                    _algebraicSimplificationCount++;
                    _totalOptimizations++;
                    return left;
                }

                // x * 0 = 0
                if (op.Type == TokenType.Star && IsZero(rightVal))
                {
                    LogOptimization("Algebraic simplification", "x * 0 = 0");
                    _algebraicSimplificationCount++;
                    _totalOptimizations++;
                    return CreateConstantExpression(0, op.Line, op.Column);
                }
            }

            // 0 + x = x
            if (IsConstantExpression(left, out object leftVal))
            {
                if (op.Type == TokenType.Plus && IsZero(leftVal))
                {
                    LogOptimization("Algebraic simplification", "0 + x = x");
                    _algebraicSimplificationCount++;
                    _totalOptimizations++;
                    return right;
                }

                // 1 * x = x
                if (op.Type == TokenType.Star && IsOne(leftVal))
                {
                    LogOptimization("Algebraic simplification", "1 * x = x");
                    _algebraicSimplificationCount++;
                    _totalOptimizations++;
                    return right;
                }

                // 0 * x = 0
                if (op.Type == TokenType.Star && IsZero(leftVal))
                {
                    LogOptimization("Algebraic simplification", "0 * x = 0");
                    _algebraicSimplificationCount++;
                    _totalOptimizations++;
                    return CreateConstantExpression(0, op.Line, op.Column);
                }
            }

            return null;  // No simplification applied
        }

        /// <summary>
        /// Applies strength reduction to replace expensive operations.
        /// </summary>
        private Expression ApplyStrengthReduction(Expression left, Token op, Expression right)
        {
            // x * 2 = x + x (cheaper on some architectures)
            // x * 2^n = x << n (not applicable here, but conceptually)

            // x / 2 with integer = x >> 1 (not applicable here)

            // For now, we focus on powers of 2 multiplication
            if (op.Type == TokenType.Star && IsConstantExpression(right, out object rightVal))
            {
                if (IsPowerOfTwo(rightVal, out int power) && power <= 3)
                {
                    // x * 2 = x + x
                    if (power == 1)
                    {
                        LogOptimization("Strength reduction", "x * 2 = x + x");
                        _strengthReductionCount++;
                        _totalOptimizations++;

                        var plusToken = new Token(TokenType.Plus, "+", null, op.Line, op.Column, 0);
                        return new BinaryExpression(left, plusToken, left);
                    }
                }
            }

            return null;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if an expression is a constant.
        /// </summary>
        private bool IsConstantExpression(Expression expression, out object value)
        {
            value = null;

            switch (expression)
            {
                case NumericExpression num:
                    value = num.Number.Value;
                    return true;

                case BooleanExpression boolExpr:
                    value = boolExpr.Value;
                    return true;

                case StringExpression str:
                    value = str.StringValue.Value;
                    return true;

                case ParenthesizedExpression paren:
                    return IsConstantExpression(paren.Expression, out value);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Evaluates a binary operation on constants.
        /// </summary>
        private object EvaluateBinaryOperation(object left, Token op, object right)
        {
            // Arithmetic operations
            if (left is int leftInt && right is int rightInt)
            {
                return op.Type switch
                {
                    TokenType.Plus => leftInt + rightInt,
                    TokenType.Minus => leftInt - rightInt,
                    TokenType.Star => leftInt * rightInt,
                    TokenType.Slash when rightInt != 0 => leftInt / rightInt,
                    TokenType.Percent when rightInt != 0 => leftInt % rightInt,
                    TokenType.LessThan => leftInt < rightInt,
                    TokenType.GreaterThan => leftInt > rightInt,
                    TokenType.LessThanOrEqual => leftInt <= rightInt,
                    TokenType.GreaterThanOrEqual => leftInt >= rightInt,
                    TokenType.EqualEqual => leftInt == rightInt,
                    TokenType.NotEqual => leftInt != rightInt,
                    _ => null
                };
            }

            // Double operations
            double leftDouble = Convert.ToDouble(left);
            double rightDouble = Convert.ToDouble(right);

            return op.Type switch
            {
                TokenType.Plus => leftDouble + rightDouble,
                TokenType.Minus => leftDouble - rightDouble,
                TokenType.Star => leftDouble * rightDouble,
                TokenType.Slash when Math.Abs(rightDouble) > double.Epsilon => leftDouble / rightDouble,
                TokenType.LessThan => leftDouble < rightDouble,
                TokenType.GreaterThan => leftDouble > rightDouble,
                TokenType.LessThanOrEqual => leftDouble <= rightDouble,
                TokenType.GreaterThanOrEqual => leftDouble >= rightDouble,
                TokenType.EqualEqual => Math.Abs(leftDouble - rightDouble) < double.Epsilon,
                TokenType.NotEqual => Math.Abs(leftDouble - rightDouble) >= double.Epsilon,
                _ => null
            };
        }

        /// <summary>
        /// Creates a constant expression from a value.
        /// </summary>
        private Expression CreateConstantExpression(object value, int line, int column)
        {
            if (value is bool boolVal)
            {
                return CreateBooleanExpression(boolVal, line, column);
            }

            if (value is int intVal)
            {
                var token = new Token(TokenType.IntegerNumber, intVal.ToString(), intVal, line, column, 0);
                return new NumericExpression(token);
            }

            if (value is double doubleVal)
            {
                // Check if it's actually an integer
                if (Math.Abs(doubleVal - Math.Round(doubleVal)) < double.Epsilon &&
                    doubleVal >= int.MinValue && doubleVal <= int.MaxValue)
                {
                    int intValue = (int)doubleVal;
                    var token = new Token(TokenType.IntegerNumber, intValue.ToString(), intValue, line, column, 0);
                    return new NumericExpression(token);
                }

                var dToken = new Token(TokenType.DecimalNumber, doubleVal.ToString(), doubleVal, line, column, 0);
                return new NumericExpression(dToken);
            }

            if (value is string strVal)
            {
                var token = new Token(TokenType.StringLiteral, $"\"{strVal}\"", strVal, line, column, 0);
                return new StringExpression(token);
            }

            return null;
        }

        /// <summary>
        /// Creates a boolean expression.
        /// </summary>
        private Expression CreateBooleanExpression(bool value, int line, int column)
        {
            var tokenType = value ? TokenType.KeywordTrue : TokenType.KeywordFalse;
            var token = new Token(tokenType, value.ToString().ToLower(), value, line, column, 0);
            return new BooleanExpression(token);
        }

        /// <summary>
        /// Converts a value to boolean.
        /// </summary>
        private bool ConvertToBool(object value)
        {
            return value switch
            {
                bool b => b,
                int i => i != 0,
                double d => Math.Abs(d) > double.Epsilon,
                string s => !string.IsNullOrEmpty(s),
                _ => false
            };
        }

        /// <summary>
        /// Checks if a value is zero.
        /// </summary>
        private bool IsZero(object value)
        {
            return value switch
            {
                int i => i == 0,
                double d => Math.Abs(d) < double.Epsilon,
                _ => false
            };
        }

        /// <summary>
        /// Checks if a value is one.
        /// </summary>
        private bool IsOne(object value)
        {
            return value switch
            {
                int i => i == 1,
                double d => Math.Abs(d - 1.0) < double.Epsilon,
                _ => false
            };
        }

        /// <summary>
        /// Checks if a value is a power of two.
        /// </summary>
        private bool IsPowerOfTwo(object value, out int power)
        {
            power = 0;
            int intVal;

            if (value is int i)
                intVal = i;
            else if (value is double d && Math.Abs(d - Math.Round(d)) < double.Epsilon)
                intVal = (int)d;
            else
                return false;

            if (intVal <= 0) return false;

            while ((intVal & 1) == 0)
            {
                intVal >>= 1;
                power++;
            }

            return intVal == 1;
        }

        /// <summary>
        /// Logs an optimization.
        /// </summary>
        private void LogOptimization(string type, string description)
        {
            _optimizationLog.Add($"[{type}] {description}");
        }

        #endregion

        #region Display Methods

        /// <summary>
        /// Displays optimization statistics.
        /// </summary>
        public void DisplayStatistics()
        {
            Console.WriteLine("\n=== OPTIMIZATION STATISTICS ===");

            if (_totalOptimizations == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No optimizations were applied.");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Total optimizations applied: {_totalOptimizations}");
            Console.ResetColor();

            Console.WriteLine("\nBreakdown by technique:");
            Console.WriteLine($"  Constant Folding:        {_constantFoldingCount}");
            Console.WriteLine($"  Dead Code Elimination:   {_deadCodeEliminationCount}");
            Console.WriteLine($"  Algebraic Simplification: {_algebraicSimplificationCount}");
            Console.WriteLine($"  Strength Reduction:      {_strengthReductionCount}");

            if (_optimizationLog.Count > 0)
            {
                Console.WriteLine($"\nOptimization details (last 10):");
                foreach (var log in _optimizationLog.TakeLast(10))
                {
                    Console.WriteLine($"  - {log}");
                }
            }
        }

        /// <summary>
        /// Gets a summary string.
        /// </summary>
        public string GetSummary()
        {
            return $"Optimizations: {_totalOptimizations} total " +
                   $"(CF:{_constantFoldingCount}, DCE:{_deadCodeEliminationCount}, " +
                   $"AS:{_algebraicSimplificationCount}, SR:{_strengthReductionCount})";
        }

        #endregion
    }
}
