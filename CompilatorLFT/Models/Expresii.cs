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
}
