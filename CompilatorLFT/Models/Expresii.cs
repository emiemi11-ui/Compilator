using System;
using System.Collections.Generic;
using CompilatorLFT.Models;

namespace CompilatorLFT.Models.Expressions
{
    /// <summary>
    /// Abstract base class for all expressions.
    /// </summary>
    /// <remarks>
    /// An expression is a construct that evaluates to a value.
    /// Examples: 5, a+b, "hello", (x*y)+z
    /// </remarks>
    public abstract class Expression : SyntaxNode
    {
    }

    #region Simple Expressions

    /// <summary>
    /// Expression for a numeric literal (integer or decimal).
    /// </summary>
    /// <example>
    /// 42, 3.14, -17
    /// </example>
    public sealed class NumericExpression : Expression
    {
        /// <summary>The lexical token containing the numeric value.</summary>
        public Token Number { get; }

        public override TokenType Type => TokenType.NumericExpression;

        public NumericExpression(Token number)
        {
            Number = number ?? throw new ArgumentNullException(nameof(number));

            if (number.Type != TokenType.IntegerNumber &&
                number.Type != TokenType.DecimalNumber)
            {
                throw new ArgumentException(
                    "Token must be IntegerNumber or DecimalNumber",
                    nameof(number));
            }
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Number;
        }
    }

    /// <summary>
    /// Expression for a string literal.
    /// </summary>
    /// <example>
    /// "hello", "test 123", ""
    /// </example>
    public sealed class StringExpression : Expression
    {
        /// <summary>The lexical token containing the string value.</summary>
        public Token StringValue { get; }

        public override TokenType Type => TokenType.StringExpression;

        public StringExpression(Token stringValue)
        {
            StringValue = stringValue ?? throw new ArgumentNullException(nameof(stringValue));

            if (stringValue.Type != TokenType.StringLiteral)
            {
                throw new ArgumentException(
                    "Token must be StringLiteral",
                    nameof(stringValue));
            }
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return StringValue;
        }
    }

    /// <summary>
    /// Expression for an identifier (variable).
    /// </summary>
    /// <example>
    /// a, sum, _temp, var123
    /// </example>
    public sealed class IdentifierExpression : Expression
    {
        /// <summary>The lexical token containing the variable name.</summary>
        public Token Identifier { get; }

        public override TokenType Type => TokenType.IdentifierExpression;

        public IdentifierExpression(Token identifier)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));

            if (identifier.Type != TokenType.Identifier)
            {
                throw new ArgumentException(
                    "Token must be Identifier",
                    nameof(identifier));
            }
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
        }
    }

    #endregion

    #region Compound Expressions

    /// <summary>
    /// Binary expression (with two operands and an operator).
    /// </summary>
    /// <remarks>
    /// Supported operators:
    /// - Arithmetic: +, -, *, /
    /// - Relational: &lt;, &gt;, &lt;=, &gt;=, ==, !=
    ///
    /// Examples: a+b, 3*5, x&lt;=y
    /// </remarks>
    public sealed class BinaryExpression : Expression
    {
        /// <summary>The expression on the left of the operator.</summary>
        public Expression Left { get; }

        /// <summary>The binary operator (+, -, *, /, &lt;, &gt;, etc.).</summary>
        public Token Operator { get; }

        /// <summary>The expression on the right of the operator.</summary>
        public Expression Right { get; }

        public override TokenType Type => TokenType.BinaryExpression;

        public BinaryExpression(Expression left, Token operatorToken, Expression right)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Operator = operatorToken ?? throw new ArgumentNullException(nameof(operatorToken));
            Right = right ?? throw new ArgumentNullException(nameof(right));

            // Validation: operator must be arithmetic or relational
            if (!operatorToken.IsArithmeticOperator() && !operatorToken.IsRelationalOperator())
            {
                throw new ArgumentException(
                    "Operator must be arithmetic or relational",
                    nameof(operatorToken));
            }
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return Operator;
            yield return Right;
        }
    }

    /// <summary>
    /// Unary expression (one operator and one operand).
    /// </summary>
    /// <remarks>
    /// Supported operators:
    /// - Unary minus: -a, -(x+y)
    ///
    /// NOTE: Unary plus (+a) is NOT supported as per requirements.
    /// </remarks>
    public sealed class UnaryExpression : Expression
    {
        /// <summary>The unary operator (only -).</summary>
        public Token Operator { get; }

        /// <summary>The expression to which the operator is applied.</summary>
        public Expression Operand { get; }

        public override TokenType Type => TokenType.UnaryExpression;

        public UnaryExpression(Token operatorToken, Expression operand)
        {
            Operator = operatorToken ?? throw new ArgumentNullException(nameof(operatorToken));
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));

            // Validation: only unary minus is allowed
            if (operatorToken.Type != TokenType.Minus)
            {
                throw new ArgumentException(
                    "Only minus operator (-) is supported as unary operator",
                    nameof(operatorToken));
            }
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Operator;
            yield return Operand;
        }
    }

    /// <summary>
    /// Parenthesized expression for forcing precedence.
    /// </summary>
    /// <example>
    /// (a + b), (3 * (x + y))
    /// </example>
    public sealed class ParenthesizedExpression : Expression
    {
        /// <summary>Open parenthesis '('.</summary>
        public Token OpenParen { get; }

        /// <summary>The expression inside the parentheses.</summary>
        public Expression Expression { get; }

        /// <summary>Close parenthesis ')'.</summary>
        public Token CloseParen { get; }

        public override TokenType Type => TokenType.ParenthesizedExpression;

        public ParenthesizedExpression(
            Token openParen,
            Expression expression,
            Token closeParen)
        {
            OpenParen = openParen ??
                throw new ArgumentNullException(nameof(openParen));
            Expression = expression ??
                throw new ArgumentNullException(nameof(expression));
            CloseParen = closeParen ??
                throw new ArgumentNullException(nameof(closeParen));

            if (openParen.Type != TokenType.OpenParen)
            {
                throw new ArgumentException(
                    "First token must be OpenParen",
                    nameof(openParen));
            }

            if (closeParen.Type != TokenType.CloseParen)
            {
                throw new ArgumentException(
                    "Third token must be CloseParen",
                    nameof(closeParen));
            }
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenParen;
            yield return Expression;
            yield return CloseParen;
        }
    }

    #endregion

    #region Boolean and Logical Expressions

    /// <summary>
    /// Expression for a boolean literal (true/false).
    /// </summary>
    /// <example>
    /// true, false
    /// </example>
    public sealed class BooleanExpression : Expression
    {
        /// <summary>The lexical token containing the boolean value.</summary>
        public Token BoolToken { get; }

        /// <summary>The boolean value.</summary>
        public bool Value { get; }

        public override TokenType Type => TokenType.BooleanExpression;

        public BooleanExpression(Token boolToken)
        {
            BoolToken = boolToken ?? throw new ArgumentNullException(nameof(boolToken));

            if (boolToken.Type != TokenType.KeywordTrue && boolToken.Type != TokenType.KeywordFalse)
            {
                throw new ArgumentException(
                    "Token must be KeywordTrue or KeywordFalse",
                    nameof(boolToken));
            }

            Value = boolToken.Type == TokenType.KeywordTrue;
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return BoolToken;
        }
    }

    /// <summary>
    /// Logical expression (&&, ||).
    /// </summary>
    /// <remarks>
    /// Implements short-circuit evaluation.
    /// Examples: a && b, x || y, (a > 5) && (b < 10)
    /// </remarks>
    public sealed class LogicalExpression : Expression
    {
        /// <summary>The expression on the left of the operator.</summary>
        public Expression Left { get; }

        /// <summary>The logical operator (&& or ||).</summary>
        public Token Operator { get; }

        /// <summary>The expression on the right of the operator.</summary>
        public Expression Right { get; }

        public override TokenType Type => TokenType.LogicalExpression;

        public LogicalExpression(Expression left, Token operatorToken, Expression right)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Operator = operatorToken ?? throw new ArgumentNullException(nameof(operatorToken));
            Right = right ?? throw new ArgumentNullException(nameof(right));

            if (operatorToken.Type != TokenType.LogicalAnd &&
                operatorToken.Type != TokenType.LogicalOr)
            {
                throw new ArgumentException(
                    "Operator must be && or ||",
                    nameof(operatorToken));
            }
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return Operator;
            yield return Right;
        }
    }

    /// <summary>
    /// Logical NOT expression (!expr).
    /// </summary>
    public sealed class NotExpression : Expression
    {
        /// <summary>The NOT operator (!).</summary>
        public Token Operator { get; }

        /// <summary>The expression to negate.</summary>
        public Expression Operand { get; }

        public override TokenType Type => TokenType.UnaryExpression;

        public NotExpression(Token operatorToken, Expression operand)
        {
            Operator = operatorToken ?? throw new ArgumentNullException(nameof(operatorToken));
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));

            if (operatorToken.Type != TokenType.LogicalNot)
            {
                throw new ArgumentException(
                    "Operator must be !",
                    nameof(operatorToken));
            }
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Operator;
            yield return Operand;
        }
    }

    #endregion

    #region Function Call Expression

    /// <summary>
    /// Function call expression.
    /// </summary>
    /// <example>
    /// add(5, 3), print("hello"), sqrt(16)
    /// </example>
    public sealed class FunctionCallExpression : Expression
    {
        /// <summary>The function name.</summary>
        public Token FunctionName { get; }

        /// <summary>Open parenthesis '('.</summary>
        public Token OpenParen { get; }

        /// <summary>The list of argument expressions.</summary>
        public List<Expression> Arguments { get; }

        /// <summary>Close parenthesis ')'.</summary>
        public Token CloseParen { get; }

        public override TokenType Type => TokenType.FunctionCallExpression;

        public FunctionCallExpression(
            Token functionName,
            Token openParen,
            List<Expression> arguments,
            Token closeParen)
        {
            FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
            OpenParen = openParen ?? throw new ArgumentNullException(nameof(openParen));
            Arguments = arguments ?? new List<Expression>();
            CloseParen = closeParen ?? throw new ArgumentNullException(nameof(closeParen));
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return FunctionName;
            yield return OpenParen;
            foreach (var arg in Arguments)
                yield return arg;
            yield return CloseParen;
        }
    }

    #endregion

    #region Increment/Decrement Expressions

    /// <summary>
    /// Increment or decrement expression (++, --).
    /// </summary>
    /// <example>
    /// i++, ++i, j--, --j
    /// </example>
    public sealed class IncrementExpression : Expression
    {
        /// <summary>The variable being incremented/decremented.</summary>
        public Token Identifier { get; }

        /// <summary>The operator (++ or --).</summary>
        public Token Operator { get; }

        /// <summary>True if prefix (++i), false if postfix (i++).</summary>
        public bool IsPrefix { get; }

        /// <summary>True if increment (++), false if decrement (--).</summary>
        public bool IsIncrement { get; }

        public override TokenType Type => TokenType.IncrementExpression;

        public IncrementExpression(Token identifier, Token operatorToken, bool isPrefix)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Operator = operatorToken ?? throw new ArgumentNullException(nameof(operatorToken));
            IsPrefix = isPrefix;
            IsIncrement = operatorToken.Type == TokenType.PlusPlus;

            if (identifier.Type != TokenType.Identifier)
            {
                throw new ArgumentException("Must be an identifier", nameof(identifier));
            }

            if (operatorToken.Type != TokenType.PlusPlus &&
                operatorToken.Type != TokenType.MinusMinus)
            {
                throw new ArgumentException("Operator must be ++ or --", nameof(operatorToken));
            }
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            if (IsPrefix)
            {
                yield return Operator;
                yield return Identifier;
            }
            else
            {
                yield return Identifier;
                yield return Operator;
            }
        }
    }

    #endregion

    #region Array Access Expression

    /// <summary>
    /// Array access expression.
    /// </summary>
    /// <example>
    /// arr[0], matrix[i][j]
    /// </example>
    public sealed class ArrayAccessExpression : Expression
    {
        /// <summary>The array expression being indexed.</summary>
        public Expression Array { get; }

        /// <summary>Open bracket '['.</summary>
        public Token OpenBracket { get; }

        /// <summary>The index expression.</summary>
        public Expression Index { get; }

        /// <summary>Close bracket ']'.</summary>
        public Token CloseBracket { get; }

        public override TokenType Type => TokenType.ArrayAccessExpression;

        public ArrayAccessExpression(
            Expression array,
            Token openBracket,
            Expression index,
            Token closeBracket)
        {
            Array = array ?? throw new ArgumentNullException(nameof(array));
            OpenBracket = openBracket ?? throw new ArgumentNullException(nameof(openBracket));
            Index = index ?? throw new ArgumentNullException(nameof(index));
            CloseBracket = closeBracket ?? throw new ArgumentNullException(nameof(closeBracket));
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Array;
            yield return OpenBracket;
            yield return Index;
            yield return CloseBracket;
        }
    }

    #endregion

    #region Array Literal Expression

    /// <summary>
    /// Array literal expression for creating arrays inline.
    /// </summary>
    /// <example>
    /// [1, 2, 3], ["a", "b", "c"]
    /// </example>
    public sealed class ArrayLiteralExpression : Expression
    {
        /// <summary>Open bracket '['.</summary>
        public Token OpenBracket { get; }

        /// <summary>The list of element expressions.</summary>
        public List<Expression> Elements { get; }

        /// <summary>Close bracket ']'.</summary>
        public Token CloseBracket { get; }

        public override TokenType Type => TokenType.ArrayLiteralExpression;

        public ArrayLiteralExpression(
            Token openBracket,
            List<Expression> elements,
            Token closeBracket)
        {
            OpenBracket = openBracket ?? throw new ArgumentNullException(nameof(openBracket));
            Elements = elements ?? new List<Expression>();
            CloseBracket = closeBracket ?? throw new ArgumentNullException(nameof(closeBracket));
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenBracket;
            foreach (var elem in Elements)
                yield return elem;
            yield return CloseBracket;
        }
    }

    #endregion

    #region Struct Member Access Expression

    /// <summary>
    /// Struct member access expression.
    /// </summary>
    /// <example>
    /// person.name, point.x
    /// </example>
    public sealed class MemberAccessExpression : Expression
    {
        /// <summary>The object expression being accessed.</summary>
        public Expression Object { get; }

        /// <summary>The dot operator '.'.</summary>
        public Token Dot { get; }

        /// <summary>The member name.</summary>
        public Token Member { get; }

        public override TokenType Type => TokenType.MemberAccessExpression;

        public MemberAccessExpression(
            Expression obj,
            Token dot,
            Token member)
        {
            Object = obj ?? throw new ArgumentNullException(nameof(obj));
            Dot = dot ?? throw new ArgumentNullException(nameof(dot));
            Member = member ?? throw new ArgumentNullException(nameof(member));
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Object;
            yield return Dot;
            yield return Member;
        }
    }

    #endregion

    #region Pointer Operations

    /// <summary>
    /// Address-of expression (&amp;variable).
    /// </summary>
    /// <example>
    /// &amp;x, &amp;arr[0]
    /// </example>
    public sealed class AddressOfExpression : Expression
    {
        /// <summary>The ampersand operator '&amp;'.</summary>
        public Token Operator { get; }

        /// <summary>The expression to get address of.</summary>
        public Expression Operand { get; }

        public override TokenType Type => TokenType.AddressOfExpression;

        public AddressOfExpression(Token op, Expression operand)
        {
            Operator = op ?? throw new ArgumentNullException(nameof(op));
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Operator;
            yield return Operand;
        }
    }

    /// <summary>
    /// Dereference expression (*pointer).
    /// </summary>
    /// <example>
    /// *ptr, **pptr
    /// </example>
    public sealed class DereferenceExpression : Expression
    {
        /// <summary>The star operator '*'.</summary>
        public Token Operator { get; }

        /// <summary>The pointer expression to dereference.</summary>
        public Expression Operand { get; }

        public override TokenType Type => TokenType.DereferenceExpression;

        public DereferenceExpression(Token op, Expression operand)
        {
            Operator = op ?? throw new ArgumentNullException(nameof(op));
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Operator;
            yield return Operand;
        }
    }

    #endregion
}
