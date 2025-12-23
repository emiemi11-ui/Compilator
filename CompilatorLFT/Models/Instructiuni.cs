using System;
using System.Collections.Generic;
using System.Linq;
using CompilatorLFT.Models.Expressions;

namespace CompilatorLFT.Models.Statements
{
    /// <summary>
    /// Abstract base class for all statements.
    /// </summary>
    /// <remarks>
    /// Reference: Dragon Book, Ch. 5 - Syntax-Directed Translation
    /// </remarks>
    public abstract class Statement : SyntaxNode
    {
    }

    /// <summary>
    /// Variable declaration statement.
    /// Syntax: int a, b=5, c;
    ///         double x=3.14;
    ///         string s="test";
    /// </summary>
    public sealed class DeclarationStatement : Statement
    {
        /// <summary>The type keyword (int/double/string).</summary>
        public Token TypeKeyword { get; }

        /// <summary>
        /// List of declarations: (identifier, optional_init_expression)
        /// If expression is null -> declaration without initialization
        /// </summary>
        public List<(Token identifier, Expression initExpression)> Declarations { get; }

        /// <summary>Final semicolon.</summary>
        public Token Semicolon { get; }

        public override TokenType Type => TokenType.DeclarationStatement;

        public DeclarationStatement(
            Token typeKeyword,
            List<(Token, Expression)> declarations,
            Token semicolon)
        {
            TypeKeyword = typeKeyword ?? throw new ArgumentNullException(nameof(typeKeyword));
            Declarations = declarations ?? throw new ArgumentNullException(nameof(declarations));
            Semicolon = semicolon ?? throw new ArgumentNullException(nameof(semicolon));

            if (!typeKeyword.IsTypeKeyword())
                throw new ArgumentException("Must be type keyword (int/double/string)");

            if (declarations.Count == 0)
                throw new ArgumentException("At least one declaration must exist");
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return TypeKeyword;

            foreach (var (id, expr) in Declarations)
            {
                yield return id;
                if (expr != null)
                    yield return expr;
            }

            yield return Semicolon;
        }
    }

    /// <summary>
    /// Assignment statement.
    /// Syntax: a = expression;
    /// </summary>
    public sealed class AssignmentStatement : Statement
    {
        /// <summary>The variable identifier.</summary>
        public Token Identifier { get; }

        /// <summary>The assignment operator '='.</summary>
        public Token AssignOperator { get; }

        /// <summary>The expression that is evaluated and assigned.</summary>
        public Expression Expression { get; }

        /// <summary>Final semicolon (can be null in for).</summary>
        public Token Semicolon { get; }

        public override TokenType Type => TokenType.AssignmentStatement;

        public AssignmentStatement(
            Token identifier,
            Token assignOperator,
            Expression expression,
            Token semicolon)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            AssignOperator = assignOperator ?? throw new ArgumentNullException(nameof(assignOperator));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Semicolon = semicolon; // Can be null in for

            if (identifier.Type != TokenType.Identifier)
                throw new ArgumentException("Must be identifier");

            if (assignOperator.Type != TokenType.Equal)
                throw new ArgumentException("Must be assignment operator '='");
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
            yield return AssignOperator;
            yield return Expression;
            if (Semicolon != null)
                yield return Semicolon;
        }
    }

    /// <summary>
    /// Statement that consists only of an expression.
    /// Syntax: expression;
    /// </summary>
    public sealed class ExpressionStatement : Statement
    {
        /// <summary>The evaluated expression.</summary>
        public Expression Expression { get; }

        /// <summary>Final semicolon.</summary>
        public Token Semicolon { get; }

        public override TokenType Type => TokenType.ExpressionStatement;

        public ExpressionStatement(Expression expression, Token semicolon)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Semicolon = semicolon; // Can be null in for increment
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Expression;
            if (Semicolon != null)
                yield return Semicolon;
        }
    }

    /// <summary>
    /// FOR statement.
    /// Syntax: for (init; condition; increment) statement
    /// </summary>
    public sealed class ForStatement : Statement
    {
        public Token ForKeyword { get; }
        public Token OpenParen { get; }

        /// <summary>Initialization statement (e.g.: int i=0).</summary>
        public Statement Initialization { get; }

        /// <summary>Condition expression (e.g.: i&lt;10).</summary>
        public Expression Condition { get; }

        public Token Semicolon { get; }

        /// <summary>Increment statement (e.g.: i=i+1).</summary>
        public Statement Increment { get; }

        public Token CloseParen { get; }

        /// <summary>Loop body (can be simple statement or block).</summary>
        public Statement Body { get; }

        public override TokenType Type => TokenType.ForStatement;

        public ForStatement(
            Token forKeyword,
            Token openParen,
            Statement initialization,
            Expression condition,
            Token semicolon,
            Statement increment,
            Token closeParen,
            Statement body)
        {
            ForKeyword = forKeyword ?? throw new ArgumentNullException(nameof(forKeyword));
            OpenParen = openParen ?? throw new ArgumentNullException(nameof(openParen));
            Initialization = initialization;
            Condition = condition;
            Semicolon = semicolon ?? throw new ArgumentNullException(nameof(semicolon));
            Increment = increment;
            CloseParen = closeParen ?? throw new ArgumentNullException(nameof(closeParen));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ForKeyword;
            yield return OpenParen;
            if (Initialization != null) yield return Initialization;
            if (Condition != null) yield return Condition;
            yield return Semicolon;
            if (Increment != null) yield return Increment;
            yield return CloseParen;
            yield return Body;
        }
    }

    /// <summary>
    /// WHILE statement.
    /// Syntax: while (condition) statement
    /// </summary>
    public sealed class WhileStatement : Statement
    {
        public Token WhileKeyword { get; }
        public Token OpenParen { get; }
        public Expression Condition { get; }
        public Token CloseParen { get; }
        public Statement Body { get; }

        public override TokenType Type => TokenType.WhileStatement;

        public WhileStatement(
            Token whileKeyword,
            Token openParen,
            Expression condition,
            Token closeParen,
            Statement body)
        {
            WhileKeyword = whileKeyword ?? throw new ArgumentNullException(nameof(whileKeyword));
            OpenParen = openParen ?? throw new ArgumentNullException(nameof(openParen));
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            CloseParen = closeParen ?? throw new ArgumentNullException(nameof(closeParen));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return WhileKeyword;
            yield return OpenParen;
            yield return Condition;
            yield return CloseParen;
            yield return Body;
        }
    }

    /// <summary>
    /// IF statement (with optional else).
    /// Syntax: if (condition) statement [else statement]
    /// </summary>
    public sealed class IfStatement : Statement
    {
        public Token IfKeyword { get; }
        public Token OpenParen { get; }
        public Expression Condition { get; }
        public Token CloseParen { get; }
        public Statement ThenBody { get; }

        // Optional
        public Token ElseKeyword { get; }
        public Statement ElseBody { get; }

        public override TokenType Type => TokenType.IfStatement;

        public IfStatement(
            Token ifKeyword,
            Token openParen,
            Expression condition,
            Token closeParen,
            Statement thenBody,
            Token elseKeyword = null,
            Statement elseBody = null)
        {
            IfKeyword = ifKeyword ?? throw new ArgumentNullException(nameof(ifKeyword));
            OpenParen = openParen ?? throw new ArgumentNullException(nameof(openParen));
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            CloseParen = closeParen ?? throw new ArgumentNullException(nameof(closeParen));
            ThenBody = thenBody ?? throw new ArgumentNullException(nameof(thenBody));
            ElseKeyword = elseKeyword;
            ElseBody = elseBody;
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return IfKeyword;
            yield return OpenParen;
            yield return Condition;
            yield return CloseParen;
            yield return ThenBody;

            if (ElseKeyword != null)
            {
                yield return ElseKeyword;
                yield return ElseBody;
            }
        }
    }

    /// <summary>
    /// Block of statements between braces.
    /// Syntax: { statement1; statement2; ... }
    /// </summary>
    public sealed class BlockStatement : Statement
    {
        public Token OpenBrace { get; }
        public List<Statement> Statements { get; }
        public Token CloseBrace { get; }

        public override TokenType Type => TokenType.Block;

        public BlockStatement(
            Token openBrace,
            List<Statement> statements,
            Token closeBrace)
        {
            OpenBrace = openBrace ?? throw new ArgumentNullException(nameof(openBrace));
            Statements = statements ?? new List<Statement>();
            CloseBrace = closeBrace ?? throw new ArgumentNullException(nameof(closeBrace));
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenBrace;

            foreach (var stmt in Statements)
                yield return stmt;

            yield return CloseBrace;
        }
    }

    /// <summary>
    /// Print statement (from Grigoraș 6.5).
    /// Syntax: print expression;
    /// </summary>
    public sealed class PrintStatement : Statement
    {
        public Token PrintKeyword { get; }
        public Token OpenParen { get; }
        public Expression Expression { get; }
        public Token CloseParen { get; }
        public Token Semicolon { get; }

        public override TokenType Type => TokenType.PrintStatement;

        public PrintStatement(
            Token printKeyword,
            Token openParen,
            Expression expression,
            Token closeParen,
            Token semicolon)
        {
            PrintKeyword = printKeyword ?? throw new ArgumentNullException(nameof(printKeyword));
            OpenParen = openParen; // Can be null for print expr; syntax
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            CloseParen = closeParen; // Can be null for print expr; syntax
            Semicolon = semicolon ?? throw new ArgumentNullException(nameof(semicolon));
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return PrintKeyword;
            if (OpenParen != null) yield return OpenParen;
            yield return Expression;
            if (CloseParen != null) yield return CloseParen;
            yield return Semicolon;
        }
    }

    /// <summary>
    /// Break statement for exiting loops.
    /// Syntax: break;
    /// </summary>
    public sealed class BreakStatement : Statement
    {
        public Token BreakKeyword { get; }
        public Token Semicolon { get; }

        public override TokenType Type => TokenType.BreakStatement;

        public BreakStatement(Token breakKeyword, Token semicolon)
        {
            BreakKeyword = breakKeyword ?? throw new ArgumentNullException(nameof(breakKeyword));
            Semicolon = semicolon ?? throw new ArgumentNullException(nameof(semicolon));
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return BreakKeyword;
            yield return Semicolon;
        }
    }

    /// <summary>
    /// Continue statement for skipping to next iteration.
    /// Syntax: continue;
    /// </summary>
    public sealed class ContinueStatement : Statement
    {
        public Token ContinueKeyword { get; }
        public Token Semicolon { get; }

        public override TokenType Type => TokenType.ContinueStatement;

        public ContinueStatement(Token continueKeyword, Token semicolon)
        {
            ContinueKeyword = continueKeyword ?? throw new ArgumentNullException(nameof(continueKeyword));
            Semicolon = semicolon ?? throw new ArgumentNullException(nameof(semicolon));
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ContinueKeyword;
            yield return Semicolon;
        }
    }

    /// <summary>
    /// Return statement for returning from functions.
    /// Syntax: return expression; or return;
    /// </summary>
    public sealed class ReturnStatement : Statement
    {
        public Token ReturnKeyword { get; }
        public Expression Expression { get; }  // Can be null for void return
        public Token Semicolon { get; }

        public override TokenType Type => TokenType.ReturnStatement;

        public ReturnStatement(Token returnKeyword, Expression expression, Token semicolon)
        {
            ReturnKeyword = returnKeyword ?? throw new ArgumentNullException(nameof(returnKeyword));
            Expression = expression;  // Can be null
            Semicolon = semicolon ?? throw new ArgumentNullException(nameof(semicolon));
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ReturnKeyword;
            if (Expression != null) yield return Expression;
            yield return Semicolon;
        }
    }

    /// <summary>
    /// Function parameter definition.
    /// </summary>
    public sealed class Parameter
    {
        public Token TypeKeyword { get; }
        public Token Identifier { get; }

        public Parameter(Token typeKeyword, Token identifier)
        {
            TypeKeyword = typeKeyword ?? throw new ArgumentNullException(nameof(typeKeyword));
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        }
    }

    /// <summary>
    /// Function declaration statement (from Flex & Bison Ch. 3).
    /// Syntax: function name(params) { body } or type name(params) { body }
    /// </summary>
    public sealed class FunctionDeclaration : Statement
    {
        public Token ReturnType { get; }  // Can be null if 'function' keyword is used
        public Token FunctionKeyword { get; }  // Can be null if type is used
        public Token Name { get; }
        public Token OpenParen { get; }
        public List<Parameter> Parameters { get; }
        public Token CloseParen { get; }
        public BlockStatement Body { get; }

        public override TokenType Type => TokenType.FunctionDeclaration;

        public FunctionDeclaration(
            Token returnType,
            Token functionKeyword,
            Token name,
            Token openParen,
            List<Parameter> parameters,
            Token closeParen,
            BlockStatement body)
        {
            ReturnType = returnType;
            FunctionKeyword = functionKeyword;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            OpenParen = openParen ?? throw new ArgumentNullException(nameof(openParen));
            Parameters = parameters ?? new List<Parameter>();
            CloseParen = closeParen ?? throw new ArgumentNullException(nameof(closeParen));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            if (ReturnType != null) yield return ReturnType;
            if (FunctionKeyword != null) yield return FunctionKeyword;
            yield return Name;
            yield return OpenParen;
            // Parameters aren't SyntaxNodes, so we skip them
            yield return CloseParen;
            yield return Body;
        }
    }

    /// <summary>
    /// Compound assignment statement (+=, -=, *=, /=, %=).
    /// Syntax: x += expr;
    /// </summary>
    public sealed class CompoundAssignmentStatement : Statement
    {
        public Token Identifier { get; }
        public Token Operator { get; }
        public Expression Expression { get; }
        public Token Semicolon { get; }

        public override TokenType Type => TokenType.AssignmentStatement;

        public CompoundAssignmentStatement(
            Token identifier,
            Token operatorToken,
            Expression expression,
            Token semicolon)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Operator = operatorToken ?? throw new ArgumentNullException(nameof(operatorToken));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Semicolon = semicolon;  // Can be null in for increment
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
            yield return Operator;
            yield return Expression;
            if (Semicolon != null) yield return Semicolon;
        }
    }

    /// <summary>
    /// Struct field definition.
    /// </summary>
    public sealed class StructField
    {
        public Token TypeKeyword { get; }
        public Token Identifier { get; }

        public StructField(Token typeKeyword, Token identifier)
        {
            TypeKeyword = typeKeyword ?? throw new ArgumentNullException(nameof(typeKeyword));
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        }
    }

    /// <summary>
    /// Struct declaration statement.
    /// Syntax: struct Name { int x; double y; }
    /// </summary>
    public sealed class StructDeclaration : Statement
    {
        public Token StructKeyword { get; }
        public Token Name { get; }
        public Token OpenBrace { get; }
        public List<StructField> Fields { get; }
        public Token CloseBrace { get; }

        public override TokenType Type => TokenType.StructDeclaration;

        public StructDeclaration(
            Token structKeyword,
            Token name,
            Token openBrace,
            List<StructField> fields,
            Token closeBrace)
        {
            StructKeyword = structKeyword ?? throw new ArgumentNullException(nameof(structKeyword));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            OpenBrace = openBrace ?? throw new ArgumentNullException(nameof(openBrace));
            Fields = fields ?? new List<StructField>();
            CloseBrace = closeBrace ?? throw new ArgumentNullException(nameof(closeBrace));
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return StructKeyword;
            yield return Name;
            yield return OpenBrace;
            // Fields aren't SyntaxNodes, so we skip them
            yield return CloseBrace;
        }
    }

    /// <summary>
    /// Array assignment statement for setting array elements.
    /// Syntax: arr[i] = value;
    /// </summary>
    public sealed class ArrayAssignmentStatement : Statement
    {
        public Expressions.ArrayAccessExpression ArrayAccess { get; }
        public Token AssignOperator { get; }
        public Expressions.Expression Expression { get; }
        public Token Semicolon { get; }

        public override TokenType Type => TokenType.AssignmentStatement;

        public ArrayAssignmentStatement(
            Expressions.ArrayAccessExpression arrayAccess,
            Token assignOperator,
            Expressions.Expression expression,
            Token semicolon)
        {
            ArrayAccess = arrayAccess ?? throw new ArgumentNullException(nameof(arrayAccess));
            AssignOperator = assignOperator ?? throw new ArgumentNullException(nameof(assignOperator));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Semicolon = semicolon;
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ArrayAccess;
            yield return AssignOperator;
            yield return Expression;
            if (Semicolon != null)
                yield return Semicolon;
        }
    }

    /// <summary>
    /// Root node - the complete program.
    /// Contains the list of all top-level statements.
    /// </summary>
    public sealed class Program : SyntaxNode
    {
        public List<Statement> Statements { get; }
        public List<FunctionDeclaration> Functions { get; }
        public List<StructDeclaration> Structs { get; }

        public override TokenType Type => TokenType.Program;

        public Program(List<Statement> statements, List<FunctionDeclaration> functions = null, List<StructDeclaration> structs = null)
        {
            Statements = statements ?? new List<Statement>();
            Functions = functions ?? new List<FunctionDeclaration>();
            Structs = structs ?? new List<StructDeclaration>();
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            foreach (var s in Structs)
                yield return s;
            foreach (var func in Functions)
                yield return func;
            foreach (var stmt in Statements)
                yield return stmt;
        }
    }
}
