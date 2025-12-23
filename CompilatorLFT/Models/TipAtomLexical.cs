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

        /// <summary>Keyword 'void' for functions without return value</summary>
        KeywordVoid,

        /// <summary>Keyword 'bool' for boolean type</summary>
        KeywordBool,

        /// <summary>Keyword 'struct' for structure type</summary>
        KeywordStruct,

        /// <summary>Keyword 'pointer' for pointer type</summary>
        KeywordPointer,

        /// <summary>Keyword 'array' for array type</summary>
        KeywordArray,

        /// <summary>Keyword 'new' for allocation</summary>
        KeywordNew,

        /// <summary>Keyword 'null' for null value</summary>
        KeywordNull,

        // ==================== KEYWORDS - CONTROL FLOW ====================

        /// <summary>Keyword 'for' for loop</summary>
        KeywordFor,

        /// <summary>Keyword 'while' for loop</summary>
        KeywordWhile,

        /// <summary>Keyword 'if' for conditional statement</summary>
        KeywordIf,

        /// <summary>Keyword 'else' for alternative branch</summary>
        KeywordElse,

        /// <summary>Keyword 'break' for exiting loops</summary>
        KeywordBreak,

        /// <summary>Keyword 'continue' for skipping to next iteration</summary>
        KeywordContinue,

        /// <summary>Keyword 'return' for returning from functions</summary>
        KeywordReturn,

        // ==================== KEYWORDS - FUNCTIONS ====================

        /// <summary>Keyword 'function' for function declarations</summary>
        KeywordFunction,

        /// <summary>Keyword 'print' for output (Grigoraș 6.5)</summary>
        KeywordPrint,

        /// <summary>Keyword 'true' for boolean literal</summary>
        KeywordTrue,

        /// <summary>Keyword 'false' for boolean literal</summary>
        KeywordFalse,

        // ==================== ARITHMETIC OPERATORS ====================

        /// <summary>Addition operator '+' or unary plus</summary>
        Plus,

        /// <summary>Subtraction operator '-' or unary minus</summary>
        Minus,

        /// <summary>Multiplication operator '*'</summary>
        Star,

        /// <summary>Division operator '/'</summary>
        Slash,

        /// <summary>Modulo operator '%'</summary>
        Percent,

        // ==================== INCREMENT/DECREMENT ====================

        /// <summary>Increment operator '++'</summary>
        PlusPlus,

        /// <summary>Decrement operator '--'</summary>
        MinusMinus,

        // ==================== COMPOUND ASSIGNMENT ====================

        /// <summary>Add-assign operator '+='</summary>
        PlusEqual,

        /// <summary>Subtract-assign operator '-='</summary>
        MinusEqual,

        /// <summary>Multiply-assign operator '*='</summary>
        StarEqual,

        /// <summary>Divide-assign operator '/='</summary>
        SlashEqual,

        /// <summary>Modulo-assign operator '%='</summary>
        PercentEqual,

        // ==================== LOGICAL OPERATORS ====================

        /// <summary>Logical AND operator '&&'</summary>
        LogicalAnd,

        /// <summary>Logical OR operator '||'</summary>
        LogicalOr,

        /// <summary>Logical NOT operator '!'</summary>
        LogicalNot,

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

        /// <summary>Open bracket '['</summary>
        OpenBracket,

        /// <summary>Close bracket ']'</summary>
        CloseBracket,

        /// <summary>Colon ':'</summary>
        Colon,

        /// <summary>Dot '.'</summary>
        Dot,

        /// <summary>Ampersand '&amp;' for address-of</summary>
        Ampersand,

        /// <summary>Arrow '->' for pointer member access</summary>
        Arrow,

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

        /// <summary>Node for boolean literal expression</summary>
        BooleanExpression,

        /// <summary>Node for logical expression (&&, ||)</summary>
        LogicalExpression,

        /// <summary>Node for function call expression</summary>
        FunctionCallExpression,

        /// <summary>Node for array access expression (arr[i])</summary>
        ArrayAccessExpression,

        /// <summary>Node for array literal expression [1, 2, 3]</summary>
        ArrayLiteralExpression,

        /// <summary>Node for member access expression (obj.member)</summary>
        MemberAccessExpression,

        /// <summary>Node for address-of expression (&amp;var)</summary>
        AddressOfExpression,

        /// <summary>Node for dereference expression (*ptr)</summary>
        DereferenceExpression,

        /// <summary>Node for increment/decrement expression</summary>
        IncrementExpression,

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

        /// <summary>Node for print statement (Grigoraș 6.5)</summary>
        PrintStatement,

        /// <summary>Node for break statement</summary>
        BreakStatement,

        /// <summary>Node for continue statement</summary>
        ContinueStatement,

        /// <summary>Node for return statement</summary>
        ReturnStatement,

        /// <summary>Node for function declaration</summary>
        FunctionDeclaration,

        /// <summary>Node for struct declaration</summary>
        StructDeclaration,

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

        /// <summary>Boolean type (bool)</summary>
        Bool,

        /// <summary>Void type (void) - for functions without return value</summary>
        Void,

        /// <summary>Array type (array)</summary>
        Array,

        /// <summary>Pointer type (pointer)</summary>
        Pointer,

        /// <summary>Struct type (struct)</summary>
        Struct,

        /// <summary>Undefined type or error</summary>
        Unknown
    }
}
