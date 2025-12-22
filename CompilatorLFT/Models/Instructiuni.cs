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
            Semicolon = semicolon ?? throw new ArgumentNullException(nameof(semicolon));
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Expression;
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
    /// Root node - the complete program.
    /// Contains the list of all top-level statements.
    /// </summary>
    public sealed class Program : SyntaxNode
    {
        public List<Statement> Statements { get; }

        public override TokenType Type => TokenType.Program;

        public Program(List<Statement> statements)
        {
            Statements = statements ?? new List<Statement>();
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Statements.Cast<SyntaxNode>();
        }
    }
}
