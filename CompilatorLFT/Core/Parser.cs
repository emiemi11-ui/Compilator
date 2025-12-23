using System;
using System.Collections.Generic;
using CompilatorLFT.Models;
using CompilatorLFT.Models.Expressions;
using CompilatorLFT.Models.Statements;
using CompilatorLFT.Utils;
using ProgramNode = CompilatorLFT.Models.Statements.Program;

namespace CompilatorLFT.Core
{
    /// <summary>
    /// Recursive descent syntax analyzer (parser).
    /// </summary>
    /// <remarks>
    /// Reference: Dragon Book, Ch. 4 - Syntax Analysis
    /// Reference: Grigoraș "Proiectarea Compilatoarelor", Cap. 6
    /// Reference: Levine "Flex & Bison", Ch. 3
    ///
    /// Extended grammar with functions, print, break/continue, logical operators:
    ///
    /// Program := (FunctionDecl | Statement)*
    /// FunctionDecl := 'function' ID '(' ParamList? ')' Block
    ///               | Type ID '(' ParamList? ')' Block
    /// ParamList := Param (',' Param)*
    /// Param := Type ID
    ///
    /// Statement := Declaration | Assignment | Print | Break | Continue | Return
    ///            | For | While | If | Block | ExpressionStatement
    /// Print := 'print' '(' Expression ')' ';' | 'print' Expression ';'
    /// Break := 'break' ';'
    /// Continue := 'continue' ';'
    /// Return := 'return' Expression? ';'
    ///
    /// Expression := LogicalOrExpression
    /// LogicalOrExpression := LogicalAndExpression ('||' LogicalAndExpression)*
    /// LogicalAndExpression := RelationalExpression ('&&' RelationalExpression)*
    /// RelationalExpression := Term (RelOp Term)*
    /// Term := Factor (('+' | '-') Factor)*
    /// Factor := Unary (('*' | '/' | '%') Unary)*
    /// Unary := ('!' | '-') Unary | Primary
    /// Primary := Literal | '(' Expression ')' | FunctionCall | Identifier | IncrementExpr
    /// FunctionCall := ID '(' ArgList? ')'
    /// ArgList := Expression (',' Expression)*
    /// IncrementExpr := ('++' | '--') ID | ID ('++' | '--')
    /// </remarks>
    public class Parser
    {
        #region Private Fields

        private readonly Token[] _tokens;
        private int _index;
        private readonly List<CompilationError> _errors;
        private readonly SymbolTable _symbolTable;
        private readonly Dictionary<string, FunctionDeclaration> _functions;
        private readonly HashSet<string> _builtInFunctions;

        // Scope management for proper variable scoping
        private readonly Stack<HashSet<string>> _scopeStack;

        #endregion

        #region Properties

        /// <summary>Current token.</summary>
        private Token CurrentToken => _index < _tokens.Length ?
            _tokens[_index] : _tokens[^1];

        /// <summary>Next token (lookahead).</summary>
        private Token NextToken => _index + 1 < _tokens.Length ?
            _tokens[_index + 1] : _tokens[^1];

        /// <summary>List of parsing errors.</summary>
        public IReadOnlyList<CompilationError> Errors => _errors;

        /// <summary>Populated symbol table.</summary>
        public SymbolTable SymbolTable => _symbolTable;

        /// <summary>Declared functions.</summary>
        public IReadOnlyDictionary<string, FunctionDeclaration> Functions => _functions;

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
            _functions = new Dictionary<string, FunctionDeclaration>();
            _scopeStack = new Stack<HashSet<string>>();

            // Built-in functions (Flex & Bison style)
            _builtInFunctions = new HashSet<string>
            {
                "print", "sqrt", "abs", "exp", "log", "sin", "cos", "tan",
                "pow", "min", "max", "floor", "ceil", "round", "length",
                "input", "parseInt", "parseDouble", "toString"
            };

            // Add lexical errors
            _errors.AddRange(lexer.Errors);
        }

        #endregion

        #region Scope Management

        /// <summary>
        /// Pushes a new scope onto the scope stack.
        /// Variables declared in this scope will be removed when PopScope is called.
        /// </summary>
        private void PushScope()
        {
            _scopeStack.Push(new HashSet<string>());
        }

        /// <summary>
        /// Pops the current scope and removes all variables declared in it.
        /// </summary>
        private void PopScope()
        {
            if (_scopeStack.Count > 0)
            {
                var scopeVariables = _scopeStack.Pop();
                _symbolTable.RemoveAll(scopeVariables);
            }
        }

        /// <summary>
        /// Adds a variable to the symbol table and tracks it in the current scope.
        /// </summary>
        private void AddVariableToScope(string name, DataType type, int line, int column)
        {
            if (_symbolTable.Add(name, type, line, column, _errors))
            {
                // Track variable in current scope for cleanup
                if (_scopeStack.Count > 0)
                {
                    _scopeStack.Peek().Add(name);
                }
            }
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
        /// Checks if the next token is one of the given types.
        /// </summary>
        private bool PeekNext(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (NextToken.Type == type)
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

        /// <summary>
        /// Checks if a function is a built-in function.
        /// </summary>
        private bool IsBuiltInFunction(string name)
        {
            return _builtInFunctions.Contains(name);
        }

        #endregion

        #region Program Parsing

        /// <summary>
        /// Parses the entire program.
        /// </summary>
        /// <returns>The syntax tree of the program</returns>
        public ProgramNode ParseProgram()
        {
            var statements = new List<Statement>();
            var functions = new List<FunctionDeclaration>();

            while (CurrentToken.Type != TokenType.EndOfFile)
            {
                try
                {
                    // Check for function declaration
                    if (IsFunctionDeclaration())
                    {
                        var func = ParseFunctionDeclaration();
                        if (func != null)
                        {
                            functions.Add(func);
                            _functions[func.Name.Text] = func;
                        }
                    }
                    else
                    {
                        var stmt = ParseStatement();
                        if (stmt != null)
                        {
                            statements.Add(stmt);
                        }
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

            return new ProgramNode(statements, functions);
        }

        /// <summary>
        /// Checks if we're at a function declaration.
        /// </summary>
        private bool IsFunctionDeclaration()
        {
            // function keyword followed by identifier
            if (CurrentToken.Type == TokenType.KeywordFunction)
                return true;

            // Type followed by identifier and '('
            if (CurrentToken.IsTypeKeyword() &&
                NextToken.Type == TokenType.Identifier)
            {
                // Look ahead for '('
                if (_index + 2 < _tokens.Length &&
                    _tokens[_index + 2].Type == TokenType.OpenParen)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Parses a function declaration.
        /// </summary>
        private FunctionDeclaration ParseFunctionDeclaration()
        {
            Token returnType = null;
            Token functionKeyword = null;

            // Check for 'function' keyword or type
            if (CurrentToken.Type == TokenType.KeywordFunction)
            {
                functionKeyword = ConsumeToken();
            }
            else if (CurrentToken.IsTypeKeyword())
            {
                returnType = ConsumeToken();
            }
            else
            {
                _errors.Add(CompilationError.Syntactic(
                    CurrentToken.Line, CurrentToken.Column,
                    "expected 'function' keyword or return type"));
                return null;
            }

            // Function name
            var name = ExpectType(TokenType.Identifier);

            // Check for duplicate function
            if (_functions.ContainsKey(name.Text))
            {
                _errors.Add(CompilationError.Semantic(
                    name.Line, name.Column,
                    $"function '{name.Text}' is already defined"));
            }

            // Parameters
            var openParen = ExpectType(TokenType.OpenParen);
            var parameters = new List<Parameter>();

            if (!Peek(TokenType.CloseParen))
            {
                do
                {
                    if (CurrentToken.Type == TokenType.Comma)
                        ConsumeToken();

                    var paramType = ConsumeToken();
                    if (!paramType.IsTypeKeyword())
                    {
                        _errors.Add(CompilationError.Syntactic(
                            paramType.Line, paramType.Column,
                            $"expected parameter type but found '{paramType.Type}'"));
                    }

                    // Check for array type syntax: int[]
                    bool isArrayParam = false;
                    if (CurrentToken.Type == TokenType.OpenBracket &&
                        NextToken.Type == TokenType.CloseBracket)
                    {
                        ConsumeToken(); // Skip '['
                        ConsumeToken(); // Skip ']'
                        isArrayParam = true;
                    }

                    var paramName = ExpectType(TokenType.Identifier);
                    parameters.Add(new Parameter(paramType, paramName, isArrayParam));

                } while (CurrentToken.Type == TokenType.Comma);
            }

            var closeParen = ExpectType(TokenType.CloseParen);

            // Push function scope for parameters and local variables
            PushScope();

            // Add parameters to symbol table for body parsing
            foreach (var param in parameters)
            {
                var dataType = param.IsArrayType ? DataType.Array : SymbolTable.ConvertToDataType(param.TypeKeyword.Type);
                AddVariableToScope(param.Identifier.Text, dataType, param.Identifier.Line, param.Identifier.Column);
            }

            // Body
            var body = ParseBlock();

            // Pop function scope - removes function parameters and local variables
            PopScope();

            return new FunctionDeclaration(
                returnType, functionKeyword, name,
                openParen, parameters, closeParen, body);
        }

        #endregion

        #region Statement Parsing

        /// <summary>
        /// Parses a statement.
        /// </summary>
        private Statement ParseStatement()
        {
            // Print statement (Grigoraș 6.5)
            if (Peek(TokenType.KeywordPrint))
            {
                return ParsePrint();
            }

            // Break statement
            if (Peek(TokenType.KeywordBreak))
            {
                return ParseBreak();
            }

            // Continue statement
            if (Peek(TokenType.KeywordContinue))
            {
                return ParseContinue();
            }

            // Return statement
            if (Peek(TokenType.KeywordReturn))
            {
                return ParseReturn();
            }

            // Declaration (int/double/string/bool ...)
            if (Peek(
                TokenType.KeywordInt,
                TokenType.KeywordDouble,
                TokenType.KeywordString,
                TokenType.KeywordBool))
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

            // Prefix increment/decrement: ++i or --i
            if (Peek(TokenType.PlusPlus, TokenType.MinusMinus))
            {
                var expr = ParseIncrementExpression();
                var semicolon = ExpectType(TokenType.Semicolon);
                return new ExpressionStatement(expr, semicolon);
            }

            // Assignment, compound assignment, postfix increment, or standalone expression
            if (Peek(TokenType.Identifier))
            {
                // Save position for backtracking
                int startPosition = _index;
                var id = ConsumeToken();

                // Simple assignment: id = expr;
                if (CurrentToken.Type == TokenType.Equal)
                {
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

                // Compound assignment: id += expr;
                if (CurrentToken.IsCompoundAssignment())
                {
                    var op = ConsumeToken();
                    var expr = ParseExpression();
                    var semicolon = ExpectType(TokenType.Semicolon);

                    if (!_symbolTable.Exists(id.Text))
                    {
                        _errors.Add(CompilationError.Semantic(
                            id.Line, id.Column,
                            $"variable '{id.Text}' was not declared"));
                    }

                    return new CompoundAssignmentStatement(id, op, expr, semicolon);
                }

                // Postfix increment/decrement: id++;
                if (Peek(TokenType.PlusPlus, TokenType.MinusMinus))
                {
                    var op = ConsumeToken();
                    var semicolon = ExpectType(TokenType.Semicolon);

                    if (!_symbolTable.Exists(id.Text))
                    {
                        _errors.Add(CompilationError.Semantic(
                            id.Line, id.Column,
                            $"variable '{id.Text}' was not declared"));
                    }

                    var incExpr = new IncrementExpression(id, op, false);
                    return new ExpressionStatement(incExpr, semicolon);
                }

                // Otherwise, it's a standalone expression - backtrack
                _index = startPosition;
            }

            // Standalone expression
            var expression = ParseExpression();
            var pv = ExpectType(TokenType.Semicolon);
            return new ExpressionStatement(expression, pv);
        }

        /// <summary>
        /// Parses a print statement (Grigoraș 6.5).
        /// </summary>
        private PrintStatement ParsePrint()
        {
            var printKeyword = ConsumeToken();
            Token openParen = null;
            Token closeParen = null;

            // Optional parentheses: print(expr); or print expr;
            if (CurrentToken.Type == TokenType.OpenParen)
            {
                openParen = ConsumeToken();
                var expr = ParseExpression();
                closeParen = ExpectType(TokenType.CloseParen);
                var semicolon = ExpectType(TokenType.Semicolon);
                return new PrintStatement(printKeyword, openParen, expr, closeParen, semicolon);
            }
            else
            {
                var expr = ParseExpression();
                var semicolon = ExpectType(TokenType.Semicolon);
                return new PrintStatement(printKeyword, null, expr, null, semicolon);
            }
        }

        /// <summary>
        /// Parses a break statement.
        /// </summary>
        private BreakStatement ParseBreak()
        {
            var breakKeyword = ConsumeToken();
            var semicolon = ExpectType(TokenType.Semicolon);
            return new BreakStatement(breakKeyword, semicolon);
        }

        /// <summary>
        /// Parses a continue statement.
        /// </summary>
        private ContinueStatement ParseContinue()
        {
            var continueKeyword = ConsumeToken();
            var semicolon = ExpectType(TokenType.Semicolon);
            return new ContinueStatement(continueKeyword, semicolon);
        }

        /// <summary>
        /// Parses a return statement.
        /// </summary>
        private ReturnStatement ParseReturn()
        {
            var returnKeyword = ConsumeToken();
            Expression expr = null;

            // Optional return value
            if (!Peek(TokenType.Semicolon))
            {
                expr = ParseExpression();
            }

            var semicolon = ExpectType(TokenType.Semicolon);
            return new ReturnStatement(returnKeyword, expr, semicolon);
        }

        /// <summary>
        /// Parses a variable declaration.
        /// Supports array type syntax: int[] arr = [1, 2, 3];
        /// </summary>
        private DeclarationStatement ParseDeclaration()
        {
            var typeKeyword = ConsumeToken();
            var dataType = SymbolTable.ConvertToDataType(typeKeyword.Type);

            // Check for array type syntax: int[]
            bool isArrayType = false;
            if (CurrentToken.Type == TokenType.OpenBracket &&
                NextToken.Type == TokenType.CloseBracket)
            {
                ConsumeToken(); // Skip '['
                ConsumeToken(); // Skip ']'
                isArrayType = true;
                dataType = DataType.Array;
            }

            var declarations = new List<(Token, Expression)>();

            do
            {
                var id = ExpectType(TokenType.Identifier);

                // Add to symbol table (with scope tracking if in a scope)
                AddVariableToScope(id.Text, dataType, id.Line, id.Column);

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
        /// For loops have their own scope - variables declared in the initialization
        /// are only visible within the for loop and are removed when the loop ends.
        /// </summary>
        private ForStatement ParseFor()
        {
            var forKeyword = ConsumeToken();
            var openParen = ExpectType(TokenType.OpenParen);

            // Push a new scope for the for loop
            PushScope();

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

            // Pop the for loop scope - removes loop variables from symbol table
            PopScope();

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
                TokenType.KeywordString,
                TokenType.KeywordBool))
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
            // Check for prefix increment/decrement
            if (Peek(TokenType.PlusPlus, TokenType.MinusMinus))
            {
                var op = ConsumeToken();
                var id = ExpectType(TokenType.Identifier);

                if (!_symbolTable.Exists(id.Text))
                {
                    _errors.Add(CompilationError.Semantic(
                        id.Line, id.Column,
                        $"variable '{id.Text}' was not declared"));
                }

                var incExpr = new IncrementExpression(id, op, true);
                return new ExpressionStatement(incExpr, null);
            }

            var identifier = ExpectType(TokenType.Identifier);

            if (!_symbolTable.Exists(identifier.Text))
            {
                _errors.Add(CompilationError.Semantic(
                    identifier.Line, identifier.Column,
                    $"variable '{identifier.Text}' was not declared"));
            }

            // Postfix increment/decrement
            if (Peek(TokenType.PlusPlus, TokenType.MinusMinus))
            {
                var op = ConsumeToken();
                var incExpr = new IncrementExpression(identifier, op, false);
                return new ExpressionStatement(incExpr, null);
            }

            // Compound assignment
            if (CurrentToken.IsCompoundAssignment())
            {
                var op = ConsumeToken();
                var expr = ParseExpression();
                return new CompoundAssignmentStatement(identifier, op, expr, null);
            }

            // Simple assignment
            var equal = ExpectType(TokenType.Equal);
            var expression = ParseExpression();

            // No semicolon here!
            return new AssignmentStatement(identifier, equal, expression, null);
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
        public Expression ParseExpression()
        {
            return ParseLogicalOrExpression();
        }

        /// <summary>
        /// Parses a logical OR expression (||).
        /// </summary>
        private Expression ParseLogicalOrExpression()
        {
            var left = ParseLogicalAndExpression();

            while (Peek(TokenType.LogicalOr))
            {
                var op = ConsumeToken();
                var right = ParseLogicalAndExpression();
                left = new LogicalExpression(left, op, right);
            }

            return left;
        }

        /// <summary>
        /// Parses a logical AND expression (&&).
        /// </summary>
        private Expression ParseLogicalAndExpression()
        {
            var left = ParseRelationalExpression();

            while (Peek(TokenType.LogicalAnd))
            {
                var op = ConsumeToken();
                var right = ParseRelationalExpression();
                left = new LogicalExpression(left, op, right);
            }

            return left;
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
        /// Parses a factor (multiplication/division/modulo).
        /// </summary>
        private Expression ParseFactor()
        {
            var left = ParseUnary();

            while (Peek(
                TokenType.Star,
                TokenType.Slash,
                TokenType.Percent))
            {
                var op = ConsumeToken();
                var right = ParseUnary();
                left = new BinaryExpression(left, op, right);
            }

            return left;
        }

        /// <summary>
        /// Parses a unary expression (!, -, ++, --).
        /// </summary>
        private Expression ParseUnary()
        {
            // Logical NOT
            if (CurrentToken.Type == TokenType.LogicalNot)
            {
                var op = ConsumeToken();
                var operand = ParseUnary();
                return new NotExpression(op, operand);
            }

            // Unary minus
            if (CurrentToken.Type == TokenType.Minus)
            {
                var op = ConsumeToken();
                var operand = ParseUnary();
                return new UnaryExpression(op, operand);
            }

            // Unary plus - ERROR according to requirements!
            if (CurrentToken.Type == TokenType.Plus)
            {
                _errors.Add(CompilationError.Lexical(
                    CurrentToken.Line, CurrentToken.Column,
                    "unary plus is not allowed"));
                ConsumeToken();
                return ParseUnary();
            }

            // Prefix increment/decrement
            if (Peek(TokenType.PlusPlus, TokenType.MinusMinus))
            {
                return ParseIncrementExpression();
            }

            return ParsePrimary();
        }

        /// <summary>
        /// Parses an increment/decrement expression.
        /// </summary>
        private Expression ParseIncrementExpression()
        {
            // Prefix: ++i or --i
            if (Peek(TokenType.PlusPlus, TokenType.MinusMinus))
            {
                var op = ConsumeToken();
                var id = ExpectType(TokenType.Identifier);

                if (!_symbolTable.Exists(id.Text) && !IsBuiltInFunction(id.Text))
                {
                    _errors.Add(CompilationError.Semantic(
                        id.Line, id.Column,
                        $"variable '{id.Text}' was not declared"));
                }

                return new IncrementExpression(id, op, true);
            }

            return ParsePrimary();
        }

        /// <summary>
        /// Parses a primary expression.
        /// </summary>
        private Expression ParsePrimary()
        {
            // Parentheses
            if (CurrentToken.Type == TokenType.OpenParen)
            {
                var openParen = ConsumeToken();
                var expr = ParseExpression();
                var closeParen = ExpectType(TokenType.CloseParen);
                return new ParenthesizedExpression(openParen, expr, closeParen);
            }

            // Array literal [1, 2, 3]
            if (CurrentToken.Type == TokenType.OpenBracket)
            {
                return ParseArrayLiteral();
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

            // Boolean literals
            if (Peek(TokenType.KeywordTrue, TokenType.KeywordFalse))
            {
                var token = ConsumeToken();
                return new BooleanExpression(token);
            }

            // Identifier (variable or function call)
            if (CurrentToken.Type == TokenType.Identifier)
            {
                var id = ConsumeToken();

                // Function call: id(args)
                if (CurrentToken.Type == TokenType.OpenParen)
                {
                    return ParseFunctionCall(id);
                }

                // Postfix increment/decrement: id++ or id--
                if (Peek(TokenType.PlusPlus, TokenType.MinusMinus))
                {
                    var op = ConsumeToken();

                    if (!_symbolTable.Exists(id.Text))
                    {
                        _errors.Add(CompilationError.Semantic(
                            id.Line, id.Column,
                            $"variable '{id.Text}' was not declared"));
                    }

                    return new IncrementExpression(id, op, false);
                }

                // Array access: id[index]
                if (CurrentToken.Type == TokenType.OpenBracket)
                {
                    return ParseArrayAccess(new IdentifierExpression(id));
                }

                // Simple variable reference
                if (!_symbolTable.Exists(id.Text) && !IsBuiltInFunction(id.Text) && !_functions.ContainsKey(id.Text))
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

        /// <summary>
        /// Parses a function call expression.
        /// </summary>
        private FunctionCallExpression ParseFunctionCall(Token functionName)
        {
            var openParen = ConsumeToken();
            var arguments = new List<Expression>();

            // Parse arguments
            if (!Peek(TokenType.CloseParen))
            {
                arguments.Add(ParseExpression());

                while (CurrentToken.Type == TokenType.Comma)
                {
                    ConsumeToken(); // Skip ','
                    arguments.Add(ParseExpression());
                }
            }

            var closeParen = ExpectType(TokenType.CloseParen);

            // Check if function exists (unless built-in)
            if (!IsBuiltInFunction(functionName.Text) && !_functions.ContainsKey(functionName.Text))
            {
                _errors.Add(CompilationError.Semantic(
                    functionName.Line, functionName.Column,
                    $"function '{functionName.Text}' is not defined"));
            }

            return new FunctionCallExpression(functionName, openParen, arguments, closeParen);
        }

        /// <summary>
        /// Parses an array access expression.
        /// </summary>
        private Expression ParseArrayAccess(Expression array)
        {
            while (CurrentToken.Type == TokenType.OpenBracket)
            {
                var openBracket = ConsumeToken();
                var index = ParseExpression();
                var closeBracket = ExpectType(TokenType.CloseBracket);

                array = new ArrayAccessExpression(array, openBracket, index, closeBracket);
            }

            return array;
        }

        /// <summary>
        /// Parses an array literal expression [1, 2, 3].
        /// </summary>
        private Expression ParseArrayLiteral()
        {
            var openBracket = ConsumeToken();  // Skip '['
            var elements = new List<Expression>();

            // Parse elements
            if (!Peek(TokenType.CloseBracket))
            {
                elements.Add(ParseExpression());

                while (CurrentToken.Type == TokenType.Comma)
                {
                    ConsumeToken();  // Skip ','
                    elements.Add(ParseExpression());
                }
            }

            var closeBracket = ExpectType(TokenType.CloseBracket);

            return new ArrayLiteralExpression(openBracket, elements, closeBracket);
        }

        /// <summary>
        /// Parses member access expression (obj.member or obj->member).
        /// </summary>
        private Expression ParseMemberAccess(Expression obj)
        {
            while (Peek(TokenType.Dot, TokenType.Arrow))
            {
                var dot = ConsumeToken();
                var member = ExpectType(TokenType.Identifier);
                obj = new MemberAccessExpression(obj, dot, member);
            }

            return obj;
        }

        #endregion
    }
}
