using System;
using System.Collections.Generic;
using CompilatorLFT.Models;
using CompilatorLFT.Models.Expressions;
using CompilatorLFT.Models.Statements;
using CompilatorLFT.Utils;

namespace CompilatorLFT.Core
{
    /// <summary>
    /// Recursive descent syntax analyzer (parser).
    /// </summary>
    /// <remarks>
    /// Reference: Dragon Book, Ch. 4 - Syntax Analysis
    /// Implements grammar:
    /// Program := Statement*
    /// Statement := Declaration | Assignment | For | While | If | Block | ExpressionStandalone
    /// Expression := RelationalExpression
    /// RelationalExpression := Term (RelOp Term)*
    /// Term := Factor (('+' | '-') Factor)*
    /// Factor := Primary (('*' | '/') Primary)*
    /// Primary := '-' Primary | '(' Expression ')' | Literal | Identifier
    /// </remarks>
    public class Parser
    {
        #region Private Fields

        private readonly Token[] _tokens;
        private int _index;
        private readonly List<CompilationError> _errors;
        private readonly SymbolTable _symbolTable;

        #endregion

        #region Properties

        /// <summary>Current token.</summary>
        private Token CurrentToken => _index < _tokens.Length ?
            _tokens[_index] : _tokens[^1];

        /// <summary>List of parsing errors.</summary>
        public IReadOnlyList<CompilationError> Errors => _errors;

        /// <summary>Populated symbol table.</summary>
        public SymbolTable SymbolTable => _symbolTable;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the parser with source text.
        /// </summary>
        /// <param name="text">Source text to parse</param>
        public Parser(string text)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.Tokenize();

            _tokens = tokens.ToArray();
            _index = 0;
            _errors = new List<CompilationError>();
            _symbolTable = new SymbolTable();

            // Add lexical errors
            _errors.AddRange(lexer.Errors);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Consumes the current token and advances.
        /// </summary>
        private Token ConsumeToken()
        {
            var token = CurrentToken;
            _index++;
            return token;
        }

        /// <summary>
        /// Checks the current type and consumes if it matches.
        /// </summary>
        private Token ExpectType(TokenType expectedType)
        {
            if (CurrentToken.Type == expectedType)
            {
                return ConsumeToken();
            }

            _errors.Add(CompilationError.Syntactic(
                CurrentToken.Line, CurrentToken.Column,
                $"expected '{expectedType}' but found '{CurrentToken.Type}'"));

            return new Token(
                TokenType.Invalid, "", null,
                CurrentToken.Line, CurrentToken.Column, 0);
        }

        /// <summary>
        /// Checks if the current token is one of the given types.
        /// </summary>
        private bool Peek(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (CurrentToken.Type == type)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Recovers after an error - advances to a safe point.
        /// </summary>
        private void RecoverFromError()
        {
            while (!Peek(
                TokenType.Semicolon,
                TokenType.CloseBrace,
                TokenType.EndOfFile))
            {
                ConsumeToken();
            }

            if (CurrentToken.Type == TokenType.Semicolon)
            {
                ConsumeToken();
            }
        }

        #endregion

        #region Program Parsing

        /// <summary>
        /// Parses the entire program.
        /// </summary>
        /// <returns>The syntax tree of the program</returns>
        public Program ParseProgram()
        {
            var statements = new List<Statement>();

            while (CurrentToken.Type != TokenType.EndOfFile)
            {
                try
                {
                    var stmt = ParseStatement();
                    if (stmt != null)
                    {
                        statements.Add(stmt);
                    }
                }
                catch (Exception ex)
                {
                    _errors.Add(CompilationError.Syntactic(
                        CurrentToken.Line, CurrentToken.Column,
                        ex.Message));
                    RecoverFromError();
                }
            }

            return new Program(statements);
        }

        #endregion

        #region Statement Parsing

        /// <summary>
        /// Parses a statement.
        /// </summary>
        private Statement ParseStatement()
        {
            // Declaration (int/double/string ...)
            if (Peek(
                TokenType.KeywordInt,
                TokenType.KeywordDouble,
                TokenType.KeywordString))
            {
                return ParseDeclaration();
            }

            // For
            if (Peek(TokenType.KeywordFor))
            {
                return ParseFor();
            }

            // While
            if (Peek(TokenType.KeywordWhile))
            {
                return ParseWhile();
            }

            // If
            if (Peek(TokenType.KeywordIf))
            {
                return ParseIf();
            }

            // Block
            if (Peek(TokenType.OpenBrace))
            {
                return ParseBlock();
            }

            // Assignment or standalone expression
            if (Peek(TokenType.Identifier))
            {
                // Save position for backtracking
                int startPosition = _index;
                var id = ConsumeToken();

                if (CurrentToken.Type == TokenType.Equal)
                {
                    // It's an assignment
                    var equal = ConsumeToken();
                    var expr = ParseExpression();
                    var semicolon = ExpectType(TokenType.Semicolon);

                    // Semantic check
                    if (!_symbolTable.Exists(id.Text))
                    {
                        _errors.Add(CompilationError.Semantic(
                            id.Line, id.Column,
                            $"variable '{id.Text}' was not declared"));
                    }

                    return new AssignmentStatement(id, equal, expr, semicolon);
                }
                else
                {
                    // It's a standalone expression - backtrack
                    _index = startPosition;
                    var expr = ParseExpression();
                    var semicolon = ExpectType(TokenType.Semicolon);
                    return new ExpressionStatement(expr, semicolon);
                }
            }

            // Standalone expression
            var expression = ParseExpression();
            var pv = ExpectType(TokenType.Semicolon);
            return new ExpressionStatement(expression, pv);
        }

        /// <summary>
        /// Parses a variable declaration.
        /// </summary>
        private DeclarationStatement ParseDeclaration()
        {
            var typeKeyword = ConsumeToken();
            var dataType = SymbolTable.ConvertToDataType(typeKeyword.Type);

            var declarations = new List<(Token, Expression)>();

            do
            {
                var id = ExpectType(TokenType.Identifier);

                // Add to symbol table
                _symbolTable.Add(id.Text, dataType, id.Line, id.Column, _errors);

                Expression expr = null;

                // Initialization?
                if (CurrentToken.Type == TokenType.Equal)
                {
                    ConsumeToken(); // Skip '='
                    expr = ParseExpression();
                }

                declarations.Add((id, expr));

            } while (CurrentToken.Type == TokenType.Comma && ConsumeToken() != null);

            var semicolon = ExpectType(TokenType.Semicolon);

            return new DeclarationStatement(typeKeyword, declarations, semicolon);
        }

        /// <summary>
        /// Parses a FOR statement.
        /// </summary>
        private ForStatement ParseFor()
        {
            var forKeyword = ConsumeToken();
            var openParen = ExpectType(TokenType.OpenParen);

            // Initialization
            Statement init = null;
            if (!Peek(TokenType.Semicolon))
            {
                init = ParseForInit();
            }
            else
            {
                ConsumeToken(); // Skip ;
            }

            // Condition
            Expression condition = null;
            if (!Peek(TokenType.Semicolon))
            {
                condition = ParseExpression();
            }

            var semicolon = ExpectType(TokenType.Semicolon);

            // Increment
            Statement increment = null;
            if (!Peek(TokenType.CloseParen))
            {
                increment = ParseForIncrement();
            }

            var closeParen = ExpectType(TokenType.CloseParen);

            // Body
            var body = ParseStatement();

            return new ForStatement(
                forKeyword, openParen,
                init, condition, semicolon, increment,
                closeParen, body);
        }

        /// <summary>
        /// Parses the initialization part of FOR.
        /// </summary>
        private Statement ParseForInit()
        {
            if (Peek(
                TokenType.KeywordInt,
                TokenType.KeywordDouble,
                TokenType.KeywordString))
            {
                return ParseDeclaration();
            }

            // Assignment
            var id = ExpectType(TokenType.Identifier);

            if (!_symbolTable.Exists(id.Text))
            {
                _errors.Add(CompilationError.Semantic(
                    id.Line, id.Column,
                    $"variable '{id.Text}' was not declared"));
            }

            var equal = ExpectType(TokenType.Equal);
            var expr = ParseExpression();
            var semicolon = ExpectType(TokenType.Semicolon);

            return new AssignmentStatement(id, equal, expr, semicolon);
        }

        /// <summary>
        /// Parses the increment part of FOR (without ;).
        /// </summary>
        private Statement ParseForIncrement()
        {
            var id = ExpectType(TokenType.Identifier);

            if (!_symbolTable.Exists(id.Text))
            {
                _errors.Add(CompilationError.Semantic(
                    id.Line, id.Column,
                    $"variable '{id.Text}' was not declared"));
            }

            var equal = ExpectType(TokenType.Equal);
            var expr = ParseExpression();

            // No semicolon here!
            return new AssignmentStatement(id, equal, expr, null);
        }

        /// <summary>
        /// Parses a WHILE statement.
        /// </summary>
        private WhileStatement ParseWhile()
        {
            var whileKeyword = ConsumeToken();
            var openParen = ExpectType(TokenType.OpenParen);
            var condition = ParseExpression();
            var closeParen = ExpectType(TokenType.CloseParen);
            var body = ParseStatement();

            return new WhileStatement(
                whileKeyword, openParen,
                condition, closeParen, body);
        }

        /// <summary>
        /// Parses an IF statement.
        /// </summary>
        private IfStatement ParseIf()
        {
            var ifKeyword = ConsumeToken();
            var openParen = ExpectType(TokenType.OpenParen);
            var condition = ParseExpression();
            var closeParen = ExpectType(TokenType.CloseParen);
            var thenBody = ParseStatement();

            Token elseKeyword = null;
            Statement elseBody = null;

            if (CurrentToken.Type == TokenType.KeywordElse)
            {
                elseKeyword = ConsumeToken();
                elseBody = ParseStatement();
            }

            return new IfStatement(
                ifKeyword, openParen,
                condition, closeParen, thenBody,
                elseKeyword, elseBody);
        }

        /// <summary>
        /// Parses a block of statements.
        /// </summary>
        private BlockStatement ParseBlock()
        {
            var openBrace = ConsumeToken();
            var statements = new List<Statement>();

            while (!Peek(
                TokenType.CloseBrace,
                TokenType.EndOfFile))
            {
                var stmt = ParseStatement();
                if (stmt != null)
                {
                    statements.Add(stmt);
                }
            }

            var closeBrace = ExpectType(TokenType.CloseBrace);

            return new BlockStatement(openBrace, statements, closeBrace);
        }

        #endregion

        #region Expression Parsing

        /// <summary>
        /// Parses an expression.
        /// </summary>
        private Expression ParseExpression()
        {
            return ParseRelationalExpression();
        }

        /// <summary>
        /// Parses a relational expression.
        /// Precedence: &lt;, &gt;, &lt;=, &gt;=, ==, !=
        /// </summary>
        private Expression ParseRelationalExpression()
        {
            var left = ParseTerm();

            while (Peek(
                TokenType.LessThan,
                TokenType.GreaterThan,
                TokenType.LessThanOrEqual,
                TokenType.GreaterThanOrEqual,
                TokenType.EqualEqual,
                TokenType.NotEqual))
            {
                var op = ConsumeToken();
                var right = ParseTerm();
                left = new BinaryExpression(left, op, right);
            }

            return left;
        }

        /// <summary>
        /// Parses a term (addition/subtraction).
        /// </summary>
        private Expression ParseTerm()
        {
            var left = ParseFactor();

            while (Peek(
                TokenType.Plus,
                TokenType.Minus))
            {
                var op = ConsumeToken();
                var right = ParseFactor();
                left = new BinaryExpression(left, op, right);
            }

            return left;
        }

        /// <summary>
        /// Parses a factor (multiplication/division).
        /// </summary>
        private Expression ParseFactor()
        {
            var left = ParsePrimary();

            while (Peek(
                TokenType.Star,
                TokenType.Slash))
            {
                var op = ConsumeToken();
                var right = ParsePrimary();
                left = new BinaryExpression(left, op, right);
            }

            return left;
        }

        /// <summary>
        /// Parses a primary expression.
        /// </summary>
        private Expression ParsePrimary()
        {
            // Unary minus
            if (CurrentToken.Type == TokenType.Minus)
            {
                var op = ConsumeToken();
                var operand = ParsePrimary();
                return new UnaryExpression(op, operand);
            }

            // Unary plus - ERROR according to requirements!
            if (CurrentToken.Type == TokenType.Plus)
            {
                _errors.Add(CompilationError.Lexical(
                    CurrentToken.Line, CurrentToken.Column,
                    "unary plus is not allowed"));
                ConsumeToken();
                return ParsePrimary();
            }

            // Parentheses
            if (CurrentToken.Type == TokenType.OpenParen)
            {
                var openParen = ConsumeToken();
                var expr = ParseExpression();
                var closeParen = ExpectType(TokenType.CloseParen);
                return new ParenthesizedExpression(openParen, expr, closeParen);
            }

            // Integer number
            if (CurrentToken.Type == TokenType.IntegerNumber)
            {
                var token = ConsumeToken();
                return new NumericExpression(token);
            }

            // Decimal number
            if (CurrentToken.Type == TokenType.DecimalNumber)
            {
                var token = ConsumeToken();
                return new NumericExpression(token);
            }

            // String literal
            if (CurrentToken.Type == TokenType.StringLiteral)
            {
                var token = ConsumeToken();
                return new StringExpression(token);
            }

            // Identifier (variable)
            if (CurrentToken.Type == TokenType.Identifier)
            {
                var id = ConsumeToken();

                // Semantic check
                if (!_symbolTable.Exists(id.Text))
                {
                    _errors.Add(CompilationError.Semantic(
                        id.Line, id.Column,
                        $"variable '{id.Text}' was not declared"));
                }

                return new IdentifierExpression(id);
            }

            // Error
            _errors.Add(CompilationError.Syntactic(
                CurrentToken.Line, CurrentToken.Column,
                $"invalid expression - unexpected token '{CurrentToken.Type}'"));

            // Return a placeholder to continue parsing
            var placeholder = new Token(
                TokenType.IntegerNumber, "0", 0,
                CurrentToken.Line, CurrentToken.Column, _index);

            if (CurrentToken.Type != TokenType.EndOfFile)
            {
                ConsumeToken();
            }

            return new NumericExpression(placeholder);
        }

        #endregion
    }
}
