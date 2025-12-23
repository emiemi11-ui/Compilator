using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CompilatorLFT.Models;
using CompilatorLFT.Utils;

namespace CompilatorLFT.Core
{
    /// <summary>
    /// Lexical analyzer that transforms source text into a stream of tokens.
    /// </summary>
    /// <remarks>
    /// Reference: Dragon Book, Ch. 3 - Lexical Analysis
    /// </remarks>
    public class Lexer
    {
        #region Private Fields

        private readonly string _text;
        private int _position;
        private int _line;
        private int _column;
        private readonly List<CompilationError> _errors;

        // Regex for identifier validation
        private static readonly Regex IdentifierRegex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");

        // Keywords
        private static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
        {
            // Type keywords
            { "int", TokenType.KeywordInt },
            { "double", TokenType.KeywordDouble },
            { "string", TokenType.KeywordString },
            { "void", TokenType.KeywordVoid },
            { "bool", TokenType.KeywordBool },
            // New type keywords
            { "struct", TokenType.KeywordStruct },
            { "pointer", TokenType.KeywordPointer },
            { "new", TokenType.KeywordNew },
            { "null", TokenType.KeywordNull },
            // Control flow keywords
            { "for", TokenType.KeywordFor },
            { "while", TokenType.KeywordWhile },
            { "if", TokenType.KeywordIf },
            { "else", TokenType.KeywordElse },
            { "break", TokenType.KeywordBreak },
            { "continue", TokenType.KeywordContinue },
            { "return", TokenType.KeywordReturn },
            // Function and output keywords
            { "function", TokenType.KeywordFunction },
            { "print", TokenType.KeywordPrint },
            // Boolean literals
            { "true", TokenType.KeywordTrue },
            { "false", TokenType.KeywordFalse }
        };

        #endregion

        #region Properties

        /// <summary>Current character in text.</summary>
        private char CurrentChar => _position < _text.Length ? _text[_position] : '\0';

        /// <summary>Next character in text.</summary>
        private char NextChar => _position + 1 < _text.Length ? _text[_position + 1] : '\0';

        /// <summary>Character two positions ahead in text.</summary>
        private char PeekNextChar => _position + 2 < _text.Length ? _text[_position + 2] : '\0';

        /// <summary>List of lexical errors.</summary>
        public IReadOnlyList<CompilationError> Errors => _errors;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new lexer for the given text.
        /// </summary>
        /// <param name="text">Source text to analyze</param>
        public Lexer(string text)
        {
            _text = text ?? string.Empty;
            _position = 0;
            _line = 1;
            _column = 1;
            _errors = new List<CompilationError>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Tokenizes the entire text and returns the list of tokens.
        /// </summary>
        /// <returns>List of lexical tokens</returns>
        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (CurrentChar != '\0')
            {
                var token = GetNextToken();

                // Skip whitespace and newlines
                if (token.Type != TokenType.Whitespace &&
                    token.Type != TokenType.NewLine)
                {
                    tokens.Add(token);
                }
            }

            // Add terminator
            tokens.Add(Token.Eof(_line, _column, _position));

            return tokens;
        }

        #endregion

        #region Private Methods - Tokenization

        /// <summary>
        /// Gets the next token from the stream.
        /// </summary>
        private Token GetNextToken()
        {
            // WHITESPACE AND NEWLINES
            if (char.IsWhiteSpace(CurrentChar))
            {
                return TokenizeWhitespace();
            }

            // SINGLE-LINE COMMENTS (// ...)
            if (CurrentChar == '/' && NextChar == '/')
            {
                return SkipSingleLineComment();
            }

            // MULTI-LINE COMMENTS (/* ... */)
            if (CurrentChar == '/' && NextChar == '*')
            {
                return SkipMultiLineComment();
            }

            // TRIPLE-DASH COMMENTS (--- ...)
            if (CurrentChar == '-' && NextChar == '-' && PeekNextChar == '-')
            {
                return SkipTripleDashComment();
            }

            // NUMBERS
            if (char.IsDigit(CurrentChar))
            {
                return TokenizeNumber();
            }

            // IDENTIFIERS AND KEYWORDS
            if (char.IsLetter(CurrentChar) || CurrentChar == '_')
            {
                return TokenizeIdentifier();
            }

            // STRING LITERALS
            if (CurrentChar == '"')
            {
                return TokenizeString();
            }

            // OPERATORS AND DELIMITERS
            return TokenizeOperatorOrDelimiter();
        }

        /// <summary>
        /// Skips a single-line comment (// ...).
        /// </summary>
        private Token SkipSingleLineComment()
        {
            int start = _position;
            int lineStart = _line;
            int columnStart = _column;

            // Skip //
            Advance();
            Advance();

            // Skip until end of line or end of file
            while (CurrentChar != '\n' && CurrentChar != '\0')
            {
                Advance();
            }

            // Return whitespace token (will be skipped)
            return new Token(
                TokenType.Whitespace, "//comment", null,
                lineStart, columnStart, start);
        }

        /// <summary>
        /// Skips a multi-line comment (/* ... */).
        /// </summary>
        private Token SkipMultiLineComment()
        {
            int start = _position;
            int lineStart = _line;
            int columnStart = _column;

            // Skip /*
            Advance();
            Advance();

            // Skip until */ or end of file
            while (!(CurrentChar == '*' && NextChar == '/') && CurrentChar != '\0')
            {
                Advance();
            }

            if (CurrentChar == '\0')
            {
                _errors.Add(CompilationError.Lexical(
                    lineStart, columnStart,
                    "unclosed multi-line comment"));
            }
            else
            {
                // Skip */
                Advance();
                Advance();
            }

            // Return whitespace token (will be skipped)
            return new Token(
                TokenType.Whitespace, "/*comment*/", null,
                lineStart, columnStart, start);
        }

        /// <summary>
        /// Skips a triple-dash comment (--- ...).
        /// Can be either line comment (--- to end of line) or inline (--- text ---).
        /// </summary>
        private Token SkipTripleDashComment()
        {
            int start = _position;
            int lineStart = _line;
            int columnStart = _column;

            // Skip ---
            Advance();
            Advance();
            Advance();

            // Skip until end of line, end of file, or closing ---
            while (CurrentChar != '\n' && CurrentChar != '\0')
            {
                // Check for closing ---
                if (CurrentChar == '-' && NextChar == '-' && PeekNextChar == '-')
                {
                    Advance(); // skip first -
                    Advance(); // skip second -
                    Advance(); // skip third -
                    break;
                }
                Advance();
            }

            // Return whitespace token (will be skipped)
            return new Token(
                TokenType.Whitespace, "---comment---", null,
                lineStart, columnStart, start);
        }

        /// <summary>
        /// Tokenizes a number (integer or decimal).
        /// </summary>
        private Token TokenizeNumber()
        {
            int start = _position;
            int lineStart = _line;
            int columnStart = _column;

            // Read digits
            while (char.IsDigit(CurrentChar))
            {
                Advance();
            }

            // Check for decimal point
            if (CurrentChar == '.' && char.IsDigit(NextChar))
            {
                // Decimal number
                Advance(); // Skip '.'

                while (char.IsDigit(CurrentChar))
                {
                    Advance();
                }

                string text = _text.Substring(start, _position - start);

                if (double.TryParse(text, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double value))
                {
                    return Token.DoubleNumber(text, value, lineStart, columnStart, start);
                }
                else
                {
                    _errors.Add(CompilationError.Lexical(
                        lineStart, columnStart,
                        $"invalid decimal number '{text}'"));

                    return new Token(
                        TokenType.Invalid, text, null,
                        lineStart, columnStart, start);
                }
            }
            else
            {
                // Integer number
                string text = _text.Substring(start, _position - start);

                if (int.TryParse(text, out int value))
                {
                    return Token.IntNumber(text, value, lineStart, columnStart, start);
                }
                else
                {
                    _errors.Add(CompilationError.Lexical(
                        lineStart, columnStart,
                        $"invalid integer '{text}' (exceeds Int32.MaxValue)"));

                    return new Token(
                        TokenType.Invalid, text, null,
                        lineStart, columnStart, start);
                }
            }
        }

        /// <summary>
        /// Tokenizes an identifier or keyword.
        /// </summary>
        private Token TokenizeIdentifier()
        {
            int start = _position;
            int lineStart = _line;
            int columnStart = _column;

            // Read letters, digits and underscore
            while (char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_')
            {
                Advance();
            }

            string text = _text.Substring(start, _position - start);

            // Check if it's a keyword
            if (Keywords.TryGetValue(text, out TokenType keywordType))
            {
                return new Token(
                    keywordType, text, text,
                    lineStart, columnStart, start);
            }

            // Check identifier validity
            if (!IdentifierRegex.IsMatch(text))
            {
                _errors.Add(CompilationError.Lexical(
                    lineStart, columnStart,
                    $"invalid identifier '{text}'"));

                return new Token(
                    TokenType.Invalid, text, null,
                    lineStart, columnStart, start);
            }

            return Token.Id(text, lineStart, columnStart, start);
        }

        /// <summary>
        /// Tokenizes a string literal.
        /// </summary>
        private Token TokenizeString()
        {
            int start = _position;
            int lineStart = _line;
            int columnStart = _column;

            Advance(); // Skip opening quote

            var sb = new StringBuilder();

            while (CurrentChar != '"' && CurrentChar != '\0')
            {
                if (CurrentChar == '\n')
                {
                    _errors.Add(CompilationError.Lexical(
                        lineStart, columnStart,
                        "unclosed string literal (missing closing quote)"));

                    string partialText = _text.Substring(start, _position - start);
                    return new Token(
                        TokenType.Invalid, partialText, null,
                        lineStart, columnStart, start);
                }

                sb.Append(CurrentChar);
                Advance();
            }

            if (CurrentChar == '"')
            {
                Advance(); // Skip closing quote
            }
            else
            {
                _errors.Add(CompilationError.Lexical(
                    lineStart, columnStart,
                    "unclosed string literal (missing closing quote)"));

                string partialText = _text.Substring(start, _position - start);
                return new Token(
                    TokenType.Invalid, partialText, null,
                    lineStart, columnStart, start);
            }

            string value = sb.ToString();

            return Token.String(value, lineStart, columnStart, start);
        }

        /// <summary>
        /// Tokenizes an operator or delimiter.
        /// </summary>
        private Token TokenizeOperatorOrDelimiter()
        {
            int start = _position;
            int lineStart = _line;
            int columnStart = _column;
            char c = CurrentChar;

            // TWO-CHARACTER OPERATORS

            // Comparison operators
            if (c == '<' && NextChar == '=')
            {
                Advance(); Advance();
                return Token.Operator(
                    TokenType.LessThanOrEqual, "<=",
                    lineStart, columnStart, start);
            }
            if (c == '>' && NextChar == '=')
            {
                Advance(); Advance();
                return Token.Operator(
                    TokenType.GreaterThanOrEqual, ">=",
                    lineStart, columnStart, start);
            }
            if (c == '=' && NextChar == '=')
            {
                Advance(); Advance();
                return Token.Operator(
                    TokenType.EqualEqual, "==",
                    lineStart, columnStart, start);
            }
            if (c == '!' && NextChar == '=')
            {
                Advance(); Advance();
                return Token.Operator(
                    TokenType.NotEqual, "!=",
                    lineStart, columnStart, start);
            }

            // Logical operators
            if (c == '&' && NextChar == '&')
            {
                Advance(); Advance();
                return Token.Operator(
                    TokenType.LogicalAnd, "&&",
                    lineStart, columnStart, start);
            }

            // Arrow operator (->)
            if (c == '-' && NextChar == '>')
            {
                Advance(); Advance();
                return Token.Operator(
                    TokenType.Arrow, "->",
                    lineStart, columnStart, start);
            }
            if (c == '|' && NextChar == '|')
            {
                Advance(); Advance();
                return Token.Operator(
                    TokenType.LogicalOr, "||",
                    lineStart, columnStart, start);
            }

            // Increment/Decrement operators
            if (c == '+' && NextChar == '+')
            {
                Advance(); Advance();
                return Token.Operator(
                    TokenType.PlusPlus, "++",
                    lineStart, columnStart, start);
            }
            if (c == '-' && NextChar == '-')
            {
                Advance(); Advance();
                return Token.Operator(
                    TokenType.MinusMinus, "--",
                    lineStart, columnStart, start);
            }

            // Compound assignment operators
            if (c == '+' && NextChar == '=')
            {
                Advance(); Advance();
                return Token.Operator(
                    TokenType.PlusEqual, "+=",
                    lineStart, columnStart, start);
            }
            if (c == '-' && NextChar == '=')
            {
                Advance(); Advance();
                return Token.Operator(
                    TokenType.MinusEqual, "-=",
                    lineStart, columnStart, start);
            }
            if (c == '*' && NextChar == '=')
            {
                Advance(); Advance();
                return Token.Operator(
                    TokenType.StarEqual, "*=",
                    lineStart, columnStart, start);
            }
            if (c == '/' && NextChar == '=')
            {
                Advance(); Advance();
                return Token.Operator(
                    TokenType.SlashEqual, "/=",
                    lineStart, columnStart, start);
            }
            if (c == '%' && NextChar == '=')
            {
                Advance(); Advance();
                return Token.Operator(
                    TokenType.PercentEqual, "%=",
                    lineStart, columnStart, start);
            }

            // SINGLE-CHARACTER OPERATORS AND DELIMITERS
            TokenType type = c switch
            {
                '+' => TokenType.Plus,
                '-' => TokenType.Minus,
                '*' => TokenType.Star,
                '/' => TokenType.Slash,
                '%' => TokenType.Percent,
                '<' => TokenType.LessThan,
                '>' => TokenType.GreaterThan,
                '=' => TokenType.Equal,
                '!' => TokenType.LogicalNot,
                '&' => TokenType.Ampersand,
                '.' => TokenType.Dot,
                ';' => TokenType.Semicolon,
                ',' => TokenType.Comma,
                ':' => TokenType.Colon,
                '(' => TokenType.OpenParen,
                ')' => TokenType.CloseParen,
                '{' => TokenType.OpenBrace,
                '}' => TokenType.CloseBrace,
                '[' => TokenType.OpenBracket,
                ']' => TokenType.CloseBracket,
                _ => TokenType.Invalid
            };

            if (type == TokenType.Invalid)
            {
                _errors.Add(CompilationError.Lexical(
                    lineStart, columnStart,
                    $"invalid character '{c}'"));
            }

            Advance();

            return Token.Operator(
                type, c.ToString(),
                lineStart, columnStart, start);
        }

        /// <summary>
        /// Tokenizes whitespace and newlines.
        /// </summary>
        private Token TokenizeWhitespace()
        {
            int start = _position;
            int lineStart = _line;
            int columnStart = _column;

            bool isNewLine = CurrentChar == '\n';

            while (char.IsWhiteSpace(CurrentChar))
            {
                if (CurrentChar == '\n')
                    isNewLine = true;
                Advance();
            }

            string text = _text.Substring(start, _position - start);
            TokenType type = isNewLine ?
                TokenType.NewLine : TokenType.Whitespace;

            return new Token(
                type, text, null,
                lineStart, columnStart, start);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Advances to the next character, updating line and column.
        /// </summary>
        private void Advance()
        {
            if (CurrentChar == '\n')
            {
                _line++;
                _column = 1;
            }
            else
            {
                _column++;
            }
            _position++;
        }

        #endregion
    }
}
