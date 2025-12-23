using System;
using System.Collections.Generic;
using System.Text;
using CompilatorLFT.Models;
using CompilatorLFT.Models.Expressions;
using CompilatorLFT.Models.Statements;
using ProgramNode = CompilatorLFT.Models.Statements.Program;

namespace CompilatorLFT.Core
{
    /// <summary>
    /// Postfix (Reverse Polish Notation) Generator.
    /// </summary>
    /// <remarks>
    /// Reference: Grigoraș "Proiectarea Compilatoarelor", Cap. 6.4.1-6.4.2
    ///
    /// Generates postfix notation (RPN) from expressions.
    /// Example from Grigoraș:
    /// Input: if a + b > c then x = a – c else x = a – b
    /// Postfix: a b + c > x a c - = x b c - = ?
    ///
    /// Advantages of postfix notation:
    /// - No parentheses needed
    /// - Easy to evaluate using a stack
    /// - Generated easily by bottom-up parser
    ///
    /// Disadvantages:
    /// - Evaluates all subexpressions (no short-circuit)
    /// </remarks>
    public class PostfixGenerator
    {
        #region Private Fields

        private readonly List<string> _tokens;
        private readonly Stack<double> _evaluationStack;

        #endregion

        #region Properties

        /// <summary>Generated postfix tokens.</summary>
        public IReadOnlyList<string> Tokens => _tokens;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new postfix generator.
        /// </summary>
        public PostfixGenerator()
        {
            _tokens = new List<string>();
            _evaluationStack = new Stack<double>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates postfix notation for an expression.
        /// </summary>
        /// <param name="expression">The expression to convert</param>
        /// <returns>List of tokens in postfix order</returns>
        public List<string> GeneratePostfix(Expression expression)
        {
            _tokens.Clear();
            GenerateExpression(expression);
            return new List<string>(_tokens);
        }

        /// <summary>
        /// Generates postfix notation for a complete program.
        /// </summary>
        /// <param name="program">The program to convert</param>
        public void Generate(ProgramNode program)
        {
            _tokens.Clear();

            foreach (var statement in program.Statements)
            {
                GenerateStatement(statement);
            }
        }

        /// <summary>
        /// Evaluates a postfix expression using a stack.
        /// </summary>
        /// <param name="postfix">List of postfix tokens</param>
        /// <param name="variables">Variable values (name -> value)</param>
        /// <returns>The result of evaluation</returns>
        public double EvaluatePostfix(List<string> postfix, Dictionary<string, double> variables = null)
        {
            variables ??= new Dictionary<string, double>();
            _evaluationStack.Clear();

            foreach (var token in postfix)
            {
                // Check if it's a number
                if (double.TryParse(token, out double number))
                {
                    _evaluationStack.Push(number);
                }
                // Check if it's a variable
                else if (variables.ContainsKey(token))
                {
                    _evaluationStack.Push(variables[token]);
                }
                // Check if it's an operator
                else if (IsOperator(token))
                {
                    if (token == "!" || token == "neg")
                    {
                        // Unary operator
                        if (_evaluationStack.Count < 1)
                            throw new InvalidOperationException("Not enough operands for unary operator");

                        double operand = _evaluationStack.Pop();
                        double result = token switch
                        {
                            "!" => operand == 0 ? 1 : 0,
                            "neg" => -operand,
                            _ => operand
                        };
                        _evaluationStack.Push(result);
                    }
                    else
                    {
                        // Binary operator
                        if (_evaluationStack.Count < 2)
                            throw new InvalidOperationException("Not enough operands for binary operator");

                        double right = _evaluationStack.Pop();
                        double left = _evaluationStack.Pop();
                        double result = token switch
                        {
                            "+" => left + right,
                            "-" => left - right,
                            "*" => left * right,
                            "/" => right != 0 ? left / right : 0,
                            "%" => right != 0 ? left % right : 0,
                            "<" => left < right ? 1 : 0,
                            ">" => left > right ? 1 : 0,
                            "<=" => left <= right ? 1 : 0,
                            ">=" => left >= right ? 1 : 0,
                            "==" => Math.Abs(left - right) < 1e-10 ? 1 : 0,
                            "!=" => Math.Abs(left - right) >= 1e-10 ? 1 : 0,
                            "&&" => (left != 0 && right != 0) ? 1 : 0,
                            "||" => (left != 0 || right != 0) ? 1 : 0,
                            _ => 0
                        };
                        _evaluationStack.Push(result);
                    }
                }
                else
                {
                    // Unknown token - treat as variable with value 0
                    _evaluationStack.Push(0);
                }
            }

            return _evaluationStack.Count > 0 ? _evaluationStack.Pop() : 0;
        }

        /// <summary>
        /// Returns the postfix notation as a formatted string.
        /// </summary>
        public string GetFormattedOutput()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== POSTFIX NOTATION (RPN) ===");
            sb.AppendLine("Reference: Grigoraș, Cap. 6.4.1-6.4.2");
            sb.AppendLine();
            sb.AppendLine(string.Join(" ", _tokens));
            return sb.ToString();
        }

        /// <summary>
        /// Displays the postfix notation to console.
        /// </summary>
        public void DisplayPostfix()
        {
            Console.WriteLine("\n" + new string('=', 50));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("POSTFIX NOTATION (RPN) - Grigoraș 6.4.1");
            Console.ResetColor();
            Console.WriteLine(new string('=', 50));

            Console.WriteLine(string.Join(" ", _tokens));
        }

        #endregion

        #region Private Methods - Statement Generation

        private void GenerateStatement(Statement statement)
        {
            switch (statement)
            {
                case DeclarationStatement decl:
                    foreach (var (id, initExpr) in decl.Declarations)
                    {
                        if (initExpr != null)
                        {
                            GenerateExpression(initExpr);
                            _tokens.Add(id.Text);
                            _tokens.Add("=");
                        }
                    }
                    break;

                case AssignmentStatement assign:
                    GenerateExpression(assign.Expression);
                    _tokens.Add(assign.Identifier.Text);
                    _tokens.Add("=");
                    break;

                case CompoundAssignmentStatement compound:
                    _tokens.Add(compound.Identifier.Text);
                    GenerateExpression(compound.Expression);
                    string compOp = compound.Operator.Type switch
                    {
                        TokenType.PlusEqual => "+",
                        TokenType.MinusEqual => "-",
                        TokenType.StarEqual => "*",
                        TokenType.SlashEqual => "/",
                        TokenType.PercentEqual => "%",
                        _ => "?"
                    };
                    _tokens.Add(compOp);
                    _tokens.Add(compound.Identifier.Text);
                    _tokens.Add("=");
                    break;

                case PrintStatement print:
                    GenerateExpression(print.Expression);
                    _tokens.Add("PRINT");
                    break;

                case IfStatement ifStmt:
                    GenerateExpression(ifStmt.Condition);
                    // Simplified: mark branches
                    _tokens.Add("[IF]");
                    GenerateStatement(ifStmt.ThenBody);
                    if (ifStmt.ElseBody != null)
                    {
                        _tokens.Add("[ELSE]");
                        GenerateStatement(ifStmt.ElseBody);
                    }
                    _tokens.Add("[ENDIF]");
                    break;

                case WhileStatement whileStmt:
                    _tokens.Add("[WHILE_START]");
                    GenerateExpression(whileStmt.Condition);
                    _tokens.Add("[WHILE_COND]");
                    GenerateStatement(whileStmt.Body);
                    _tokens.Add("[WHILE_END]");
                    break;

                case ForStatement forStmt:
                    _tokens.Add("[FOR_INIT]");
                    if (forStmt.Initialization != null)
                        GenerateStatement(forStmt.Initialization);
                    _tokens.Add("[FOR_COND]");
                    if (forStmt.Condition != null)
                        GenerateExpression(forStmt.Condition);
                    _tokens.Add("[FOR_BODY]");
                    GenerateStatement(forStmt.Body);
                    _tokens.Add("[FOR_INC]");
                    if (forStmt.Increment != null)
                        GenerateStatement(forStmt.Increment);
                    _tokens.Add("[FOR_END]");
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
                    if (ret.Expression != null)
                        GenerateExpression(ret.Expression);
                    _tokens.Add("RETURN");
                    break;
            }
        }

        #endregion

        #region Private Methods - Expression Generation

        /// <summary>
        /// Generates postfix for an expression.
        /// Uses post-order traversal: left, right, operator
        /// </summary>
        private void GenerateExpression(Expression expression)
        {
            switch (expression)
            {
                case NumericExpression num:
                    _tokens.Add(num.Number.Value.ToString());
                    break;

                case StringExpression str:
                    _tokens.Add($"\"{str.StringValue.Value}\"");
                    break;

                case BooleanExpression boolExpr:
                    _tokens.Add(boolExpr.Value ? "true" : "false");
                    break;

                case IdentifierExpression id:
                    _tokens.Add(id.Identifier.Text);
                    break;

                case UnaryExpression unary:
                    GenerateExpression(unary.Operand);
                    _tokens.Add("neg"); // Unary minus
                    break;

                case NotExpression not:
                    GenerateExpression(not.Operand);
                    _tokens.Add("!");
                    break;

                case BinaryExpression binary:
                    // Post-order: left, right, operator
                    GenerateExpression(binary.Left);
                    GenerateExpression(binary.Right);
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
                    _tokens.Add(op);
                    break;

                case LogicalExpression logical:
                    GenerateExpression(logical.Left);
                    GenerateExpression(logical.Right);
                    string logOp = logical.Operator.Type switch
                    {
                        TokenType.LogicalAnd => "&&",
                        TokenType.LogicalOr => "||",
                        _ => "?"
                    };
                    _tokens.Add(logOp);
                    break;

                case ParenthesizedExpression paren:
                    GenerateExpression(paren.Expression);
                    break;

                case IncrementExpression inc:
                    if (inc.IsPrefix)
                    {
                        // ++i: i 1 + i =
                        _tokens.Add(inc.Identifier.Text);
                        _tokens.Add("1");
                        _tokens.Add(inc.IsIncrement ? "+" : "-");
                        _tokens.Add(inc.Identifier.Text);
                        _tokens.Add("=");
                    }
                    else
                    {
                        // i++: i_old i 1 + i = (returns old value)
                        _tokens.Add(inc.Identifier.Text);
                        _tokens.Add("1");
                        _tokens.Add(inc.IsIncrement ? "+" : "-");
                        _tokens.Add(inc.Identifier.Text);
                        _tokens.Add("=");
                    }
                    break;

                case FunctionCallExpression call:
                    // Push arguments
                    foreach (var arg in call.Arguments)
                    {
                        GenerateExpression(arg);
                    }
                    // Push function name with argument count
                    _tokens.Add($"CALL:{call.FunctionName.Text}/{call.Arguments.Count}");
                    break;
            }
        }

        #endregion

        #region Helper Methods

        private bool IsOperator(string token)
        {
            return token == "+" || token == "-" || token == "*" || token == "/" ||
                   token == "%" || token == "<" || token == ">" || token == "<=" ||
                   token == ">=" || token == "==" || token == "!=" || token == "&&" ||
                   token == "||" || token == "!" || token == "neg";
        }

        #endregion
    }
}
