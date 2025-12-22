using System;

namespace CompilatorLFT.Models
{
    /// <summary>
    /// Enumeration for all lexical token types recognized by the compiler.
    /// Follows the hierarchy: Literals → Identifiers → Operators → Delimiters → AST Nodes
    /// </summary>
    /// <remarks>
    /// Reference: Dragon Book, Ch. 3 - Lexical Analysis
    /// </remarks>
    public enum TokenType
    {
        // ==================== LITERALS ====================

        /// <summary>Integer number (e.g.: 42, -17, 0)</summary>
        IntegerNumber,

        /// <summary>Decimal number with point (e.g.: 3.14, -0.5, 2.0)</summary>
        DecimalNumber,

        /// <summary>String literal between quotes (e.g.: "hello", "test 123")</summary>
        StringLiteral,

        // ==================== IDENTIFIERS ====================

        /// <summary>Variable or function name (e.g.: sum, _temp, var123)</summary>
        Identifier,

        // ==================== KEYWORDS - TYPES ====================

        /// <summary>Keyword 'int' for integer type</summary>
        KeywordInt,

        /// <summary>Keyword 'double' for decimal type</summary>
        KeywordDouble,

        /// <summary>Keyword 'string' for string type</summary>
        KeywordString,

        // ==================== KEYWORDS - CONTROL FLOW ====================

        /// <summary>Keyword 'for' for loop</summary>
        KeywordFor,

        /// <summary>Keyword 'while' for loop</summary>
        KeywordWhile,

        /// <summary>Keyword 'if' for conditional statement</summary>
        KeywordIf,

        /// <summary>Keyword 'else' for alternative branch</summary>
        KeywordElse,

        // ==================== ARITHMETIC OPERATORS ====================

        /// <summary>Addition operator '+' or unary plus</summary>
        Plus,

        /// <summary>Subtraction operator '-' or unary minus</summary>
        Minus,

        /// <summary>Multiplication operator '*'</summary>
        Star,

        /// <summary>Division operator '/'</summary>
        Slash,

        // ==================== RELATIONAL OPERATORS ====================

        /// <summary>Less than operator '&lt;'</summary>
        LessThan,

        /// <summary>Greater than operator '&gt;'</summary>
        GreaterThan,

        /// <summary>Less than or equal operator '&lt;='</summary>
        LessThanOrEqual,

        /// <summary>Greater than or equal operator '&gt;='</summary>
        GreaterThanOrEqual,

        /// <summary>Equality operator '=='</summary>
        EqualEqual,

        /// <summary>Not equal operator '!='</summary>
        NotEqual,

        // ==================== DELIMITERS ====================

        /// <summary>Semicolon delimiter ';'</summary>
        Semicolon,

        /// <summary>Comma delimiter ','</summary>
        Comma,

        /// <summary>Assignment operator '='</summary>
        Equal,

        /// <summary>Open parenthesis '('</summary>
        OpenParen,

        /// <summary>Close parenthesis ')'</summary>
        CloseParen,

        /// <summary>Open brace '{'</summary>
        OpenBrace,

        /// <summary>Close brace '}'</summary>
        CloseBrace,

        // ==================== SPECIAL ====================

        /// <summary>Whitespace (ignored in parsing)</summary>
        Whitespace,

        /// <summary>New line '\n' (for line tracking)</summary>
        NewLine,

        /// <summary>End of file/string terminator</summary>
        EndOfFile,

        /// <summary>Invalid token (lexical error)</summary>
        Invalid,

        // ==================== AST NODES - EXPRESSIONS ====================

        /// <summary>Node for numeric literal expression</summary>
        NumericExpression,

        /// <summary>Node for binary expression (e.g.: a + b)</summary>
        BinaryExpression,

        /// <summary>Node for unary expression (e.g.: -a)</summary>
        UnaryExpression,

        /// <summary>Node for parenthesized expression (e.g.: (a + b))</summary>
        ParenthesizedExpression,

        /// <summary>Node for identifier expression (variable)</summary>
        IdentifierExpression,

        /// <summary>Node for string literal expression</summary>
        StringExpression,

        // ==================== AST NODES - STATEMENTS ====================

        /// <summary>Node for declaration statement (e.g.: int a, b=5;)</summary>
        DeclarationStatement,

        /// <summary>Node for assignment statement (e.g.: a = 10;)</summary>
        AssignmentStatement,

        /// <summary>Node for expression statement (e.g.: a + b;)</summary>
        ExpressionStatement,

        /// <summary>Node for for statement</summary>
        ForStatement,

        /// <summary>Node for while statement</summary>
        WhileStatement,

        /// <summary>Node for if statement</summary>
        IfStatement,

        /// <summary>Node for block of statements between braces</summary>
        Block,

        /// <summary>Root node for complete program</summary>
        Program
    }

    /// <summary>
    /// Enumeration for compilation error types.
    /// </summary>
    /// <remarks>
    /// According to requirements: lexical, syntactic and semantic errors
    /// </remarks>
    public enum ErrorType
    {
        /// <summary>Error at lexical analysis level (invalid character, malformed number, etc.)</summary>
        Lexical,

        /// <summary>Error at syntactic analysis level (unbalanced parentheses, missing ';', etc.)</summary>
        Syntactic,

        /// <summary>Error at semantic analysis level (undeclared variable, incompatible types, etc.)</summary>
        Semantic
    }

    /// <summary>
    /// Enumeration for data types supported by the language.
    /// </summary>
    public enum DataType
    {
        /// <summary>Integer type (int)</summary>
        Int,

        /// <summary>Decimal type (double)</summary>
        Double,

        /// <summary>String type (string)</summary>
        String,

        /// <summary>Undefined type or error</summary>
        Unknown
    }
}
