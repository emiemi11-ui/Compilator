using System;
using System.Collections.Generic;
using System.Linq;
using CompilatorLFT.Models;

namespace CompilatorLFT.Models
{
    /// <summary>
    /// Represents a lexical token with complete position and value information.
    /// </summary>
    /// <remarks>
    /// Reference: Dragon Book, Ch. 3 - "Tokens, Patterns, and Lexemes"
    /// A token is a pair (token-name, attribute-value) where:
    /// - token-name = the type of lexical token
    /// - attribute-value = the associated value (number, string, identifier)
    /// </remarks>
    public class Token : SyntaxNode
    {
        #region Properties

        /// <summary>
        /// The type of lexical token.
        /// </summary>
        public override TokenType Type { get; }

        /// <summary>
        /// The original text from source (the lexeme).
        /// </summary>
        /// <example>
        /// For "int a = 123;":
        /// - "int" → lexeme for KeywordInt
        /// - "a" → lexeme for Identifier
        /// - "123" → lexeme for IntegerNumber
        /// </example>
        public string Text { get; }

        /// <summary>
        /// The parsed value of the token (null for operators/delimiters).
        /// </summary>
        /// <remarks>
        /// Possible types:
        /// - int for IntegerNumber
        /// - double for DecimalNumber
        /// - string for StringLiteral and Identifier
        /// - null for operators and delimiters
        /// </remarks>
        public object Value { get; }

        /// <summary>
        /// The line number where the token appears (1-indexed).
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// The column number where the token starts (1-indexed).
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// The absolute position in source text (0-indexed).
        /// </summary>
        public int AbsolutePosition { get; }

        /// <summary>
        /// The length of the token in characters.
        /// </summary>
        public int Length => Text?.Length ?? 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new lexical token with complete position information.
        /// </summary>
        /// <param name="type">The type of lexical token</param>
        /// <param name="text">The original text (lexeme)</param>
        /// <param name="value">The parsed value (can be null)</param>
        /// <param name="line">The line number (from 1)</param>
        /// <param name="column">The column number (from 1)</param>
        /// <param name="absolutePosition">The absolute position in text (from 0)</param>
        /// <exception cref="ArgumentException">
        /// If line or column are invalid
        /// </exception>
        public Token(
            TokenType type,
            string text,
            object value,
            int line,
            int column,
            int absolutePosition)
        {
            if (line < 1)
                throw new ArgumentException("Line must be >= 1", nameof(line));

            if (column < 1)
                throw new ArgumentException("Column must be >= 1", nameof(column));

            if (absolutePosition < 0)
                throw new ArgumentException("Absolute position must be >= 0", nameof(absolutePosition));

            Type = type;
            Text = text ?? string.Empty;
            Value = value;
            Line = line;
            Column = column;
            AbsolutePosition = absolutePosition;
        }

        #endregion

        #region SyntaxNode Implementation

        /// <summary>
        /// The lexical token is a leaf in the syntax tree (has no children).
        /// </summary>
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Enumerable.Empty<SyntaxNode>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the textual representation of the token for debugging.
        /// </summary>
        /// <returns>String with type, text and value</returns>
        public override string ToString()
        {
            string valueStr = Value != null ? $" = {Value}" : "";
            return $"{Type} '{Text}'{valueStr} @ ({Line}:{Column})";
        }

        /// <summary>
        /// Checks if the token is a type keyword.
        /// </summary>
        /// <returns>True if int, double, string, bool or void</returns>
        public bool IsTypeKeyword()
        {
            return Type == TokenType.KeywordInt ||
                   Type == TokenType.KeywordDouble ||
                   Type == TokenType.KeywordString ||
                   Type == TokenType.KeywordBool ||
                   Type == TokenType.KeywordVoid;
        }

        /// <summary>
        /// Checks if the token is an arithmetic operator.
        /// </summary>
        /// <returns>True if +, -, *, /, %</returns>
        public bool IsArithmeticOperator()
        {
            return Type == TokenType.Plus ||
                   Type == TokenType.Minus ||
                   Type == TokenType.Star ||
                   Type == TokenType.Slash ||
                   Type == TokenType.Percent;
        }

        /// <summary>
        /// Checks if the token is a logical operator.
        /// </summary>
        /// <returns>True if &&, ||, !</returns>
        public bool IsLogicalOperator()
        {
            return Type == TokenType.LogicalAnd ||
                   Type == TokenType.LogicalOr ||
                   Type == TokenType.LogicalNot;
        }

        /// <summary>
        /// Checks if the token is a compound assignment operator.
        /// </summary>
        /// <returns>True if +=, -=, *=, /=, %=</returns>
        public bool IsCompoundAssignment()
        {
            return Type == TokenType.PlusEqual ||
                   Type == TokenType.MinusEqual ||
                   Type == TokenType.StarEqual ||
                   Type == TokenType.SlashEqual ||
                   Type == TokenType.PercentEqual;
        }

        /// <summary>
        /// Checks if the token is an increment/decrement operator.
        /// </summary>
        /// <returns>True if ++ or --</returns>
        public bool IsIncrementDecrement()
        {
            return Type == TokenType.PlusPlus ||
                   Type == TokenType.MinusMinus;
        }

        /// <summary>
        /// Checks if the token is a relational operator.
        /// </summary>
        /// <returns>True if &lt;, &gt;, &lt;=, &gt;=, ==, !=</returns>
        public bool IsRelationalOperator()
        {
            return Type == TokenType.LessThan ||
                   Type == TokenType.GreaterThan ||
                   Type == TokenType.LessThanOrEqual ||
                   Type == TokenType.GreaterThanOrEqual ||
                   Type == TokenType.EqualEqual ||
                   Type == TokenType.NotEqual;
        }

        /// <summary>
        /// Checks if the token is a literal (number or string).
        /// </summary>
        /// <returns>True if IntegerNumber, DecimalNumber or StringLiteral</returns>
        public bool IsLiteral()
        {
            return Type == TokenType.IntegerNumber ||
                   Type == TokenType.DecimalNumber ||
                   Type == TokenType.StringLiteral;
        }

        /// <summary>
        /// Gets the data type associated with this lexical token.
        /// </summary>
        /// <returns>The data type or Unknown</returns>
        public DataType GetDataType()
        {
            return Type switch
            {
                TokenType.IntegerNumber => DataType.Int,
                TokenType.DecimalNumber => DataType.Double,
                TokenType.StringLiteral => DataType.String,
                TokenType.KeywordInt => DataType.Int,
                TokenType.KeywordDouble => DataType.Double,
                TokenType.KeywordString => DataType.String,
                _ => DataType.Unknown
            };
        }

        /// <summary>
        /// Compares two lexical tokens for equality.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is Token other)
            {
                return Type == other.Type &&
                       Text == other.Text &&
                       Equals(Value, other.Value) &&
                       Line == other.Line &&
                       Column == other.Column;
            }
            return false;
        }

        /// <summary>
        /// Returns hash code for the token.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Text, Value, Line, Column);
        }

        #endregion

        #region Factory Methods (for ease of creation)

        /// <summary>
        /// Creates a lexical token for an integer number.
        /// </summary>
        public static Token IntNumber(string text, int value, int line, int column, int position)
        {
            return new Token(TokenType.IntegerNumber, text, value, line, column, position);
        }

        /// <summary>
        /// Creates a lexical token for a decimal number.
        /// </summary>
        public static Token DoubleNumber(string text, double value, int line, int column, int position)
        {
            return new Token(TokenType.DecimalNumber, text, value, line, column, position);
        }

        /// <summary>
        /// Creates a lexical token for a string literal.
        /// </summary>
        public static Token String(string text, int line, int column, int position)
        {
            return new Token(TokenType.StringLiteral, text, text, line, column, position);
        }

        /// <summary>
        /// Creates a lexical token for an identifier.
        /// </summary>
        public static Token Id(string name, int line, int column, int position)
        {
            return new Token(TokenType.Identifier, name, name, line, column, position);
        }

        /// <summary>
        /// Creates a lexical token for an operator or delimiter (no value).
        /// </summary>
        public static Token Operator(TokenType type, string text, int line, int column, int position)
        {
            return new Token(type, text, null, line, column, position);
        }

        /// <summary>
        /// Creates an end-of-file token.
        /// </summary>
        public static Token Eof(int line, int column, int position)
        {
            return new Token(TokenType.EndOfFile, "\0", null, line, column, position);
        }

        #endregion
    }
}
