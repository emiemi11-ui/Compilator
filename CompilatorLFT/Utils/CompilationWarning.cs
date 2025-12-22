using System;

namespace CompilatorLFT.Utils
{
    /// <summary>
    /// Represents a compilation warning (non-fatal issue).
    /// Warnings indicate potential problems but don't prevent compilation.
    /// </summary>
    /// <remarks>
    /// Reference: Dragon Book, Ch. 9 - Code Optimization (Data Flow Analysis)
    ///
    /// Warning categories:
    /// - Unused code: Variables, functions, parameters that are declared but never used
    /// - Unreachable code: Code that can never be executed
    /// - Constant conditions: Conditions that always evaluate to true or false
    /// - Potential issues: Division by zero, null references, etc.
    /// </remarks>
    public class CompilationWarning
    {
        #region Properties

        /// <summary>Line number where warning occurs.</summary>
        public int Line { get; }

        /// <summary>Column number where warning occurs.</summary>
        public int Column { get; }

        /// <summary>Type/category of warning.</summary>
        public WarningType Type { get; }

        /// <summary>Descriptive message.</summary>
        public string Message { get; }

        /// <summary>Warning severity level.</summary>
        public WarningSeverity Severity { get; }

        /// <summary>Warning code for suppression.</summary>
        public string Code { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new compilation warning.
        /// </summary>
        public CompilationWarning(
            int line,
            int column,
            WarningType type,
            string message,
            WarningSeverity severity = WarningSeverity.Warning)
        {
            Line = line;
            Column = column;
            Type = type;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Severity = severity;
            Code = $"W{(int)type:D3}";
        }

        #endregion

        #region Factory Methods

        /// <summary>Creates warning for unused variable.</summary>
        public static CompilationWarning UnusedVariable(int line, int column, string name)
        {
            return new CompilationWarning(
                line, column,
                WarningType.UnusedVariable,
                $"variable '{name}' is declared but never used");
        }

        /// <summary>Creates warning for unused function.</summary>
        public static CompilationWarning UnusedFunction(int line, int column, string name)
        {
            return new CompilationWarning(
                line, column,
                WarningType.UnusedFunction,
                $"function '{name}' is declared but never called");
        }

        /// <summary>Creates warning for unused parameter.</summary>
        public static CompilationWarning UnusedParameter(int line, int column, string name, string functionName)
        {
            return new CompilationWarning(
                line, column,
                WarningType.UnusedParameter,
                $"parameter '{name}' in function '{functionName}' is never used");
        }

        /// <summary>Creates warning for unreachable code.</summary>
        public static CompilationWarning UnreachableCode(int line, int column)
        {
            return new CompilationWarning(
                line, column,
                WarningType.UnreachableCode,
                "unreachable code detected");
        }

        /// <summary>Creates warning for code after return statement.</summary>
        public static CompilationWarning CodeAfterReturn(int line, int column)
        {
            return new CompilationWarning(
                line, column,
                WarningType.UnreachableCode,
                "code after 'return' statement will never execute");
        }

        /// <summary>Creates warning for constant condition.</summary>
        public static CompilationWarning ConstantCondition(int line, int column, bool value)
        {
            return new CompilationWarning(
                line, column,
                WarningType.ConstantCondition,
                $"condition is always {value.ToString().ToLower()}");
        }

        /// <summary>Creates warning for always-true condition.</summary>
        public static CompilationWarning AlwaysTrueCondition(int line, int column, string context)
        {
            return new CompilationWarning(
                line, column,
                WarningType.ConstantCondition,
                $"{context} condition is always true");
        }

        /// <summary>Creates warning for always-false condition.</summary>
        public static CompilationWarning AlwaysFalseCondition(int line, int column, string context)
        {
            return new CompilationWarning(
                line, column,
                WarningType.ConstantCondition,
                $"{context} condition is always false - body will never execute");
        }

        /// <summary>Creates warning for potential null reference.</summary>
        public static CompilationWarning PossibleNullReference(int line, int column, string name)
        {
            return new CompilationWarning(
                line, column,
                WarningType.PossibleNullReference,
                $"variable '{name}' may be null when accessed");
        }

        /// <summary>Creates warning for division by constant zero.</summary>
        public static CompilationWarning DivisionByZero(int line, int column)
        {
            return new CompilationWarning(
                line, column,
                WarningType.DivisionByConstantZero,
                "division by constant zero",
                WarningSeverity.Error);
        }

        /// <summary>Creates warning for potential infinite loop.</summary>
        public static CompilationWarning InfiniteLoop(int line, int column)
        {
            return new CompilationWarning(
                line, column,
                WarningType.InfiniteLoop,
                "while condition is always true - potential infinite loop",
                WarningSeverity.Warning);
        }

        /// <summary>Creates warning for variable shadowing.</summary>
        public static CompilationWarning VariableShadowing(int line, int column, string name)
        {
            return new CompilationWarning(
                line, column,
                WarningType.VariableShadowing,
                $"variable '{name}' shadows a variable from an outer scope",
                WarningSeverity.Info);
        }

        /// <summary>Creates warning for uninitialized variable usage.</summary>
        public static CompilationWarning UninitializedVariable(int line, int column, string name)
        {
            return new CompilationWarning(
                line, column,
                WarningType.UninitializedVariable,
                $"variable '{name}' may be used before initialization");
        }

        /// <summary>Creates warning for empty block.</summary>
        public static CompilationWarning EmptyBlock(int line, int column, string context)
        {
            return new CompilationWarning(
                line, column,
                WarningType.EmptyBlock,
                $"empty {context} block",
                WarningSeverity.Info);
        }

        /// <summary>Creates warning for missing return statement.</summary>
        public static CompilationWarning MissingReturn(int line, int column, string functionName)
        {
            return new CompilationWarning(
                line, column,
                WarningType.MissingReturn,
                $"function '{functionName}' may not return a value on all paths");
        }

        /// <summary>Creates warning for comparison with itself.</summary>
        public static CompilationWarning SelfComparison(int line, int column, string varName)
        {
            return new CompilationWarning(
                line, column,
                WarningType.SelfComparison,
                $"comparing '{varName}' to itself is always true/false");
        }

        /// <summary>Creates warning for dead store (value never read).</summary>
        public static CompilationWarning DeadStore(int line, int column, string varName)
        {
            return new CompilationWarning(
                line, column,
                WarningType.DeadStore,
                $"value assigned to '{varName}' is never read");
        }

        /// <summary>Creates warning for redundant condition.</summary>
        public static CompilationWarning RedundantCondition(int line, int column, string description)
        {
            return new CompilationWarning(
                line, column,
                WarningType.RedundantCondition,
                $"redundant condition: {description}",
                WarningSeverity.Info);
        }

        #endregion

        #region Display Methods

        /// <summary>
        /// Returns a formatted string representation of the warning.
        /// </summary>
        public override string ToString()
        {
            string severityStr = Severity switch
            {
                WarningSeverity.Info => "info",
                WarningSeverity.Warning => "warning",
                WarningSeverity.Error => "error",
                _ => "warning"
            };

            return $"at line {Line}, column {Column}: [{Code}] {severityStr} - {Message}";
        }

        /// <summary>
        /// Returns a shorter format for display.
        /// </summary>
        public string ToShortString()
        {
            return $"L{Line}:C{Column}: {Message}";
        }

        #endregion

        #region Equality

        public override bool Equals(object obj)
        {
            if (obj is CompilationWarning other)
            {
                return Line == other.Line &&
                       Column == other.Column &&
                       Type == other.Type &&
                       Message == other.Message;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Line, Column, Type, Message);
        }

        #endregion
    }

    /// <summary>
    /// Types of compilation warnings.
    /// </summary>
    public enum WarningType
    {
        // Unused code (1xx)
        /// <summary>Variable declared but never used.</summary>
        UnusedVariable = 101,

        /// <summary>Function declared but never called.</summary>
        UnusedFunction = 102,

        /// <summary>Function parameter never used.</summary>
        UnusedParameter = 103,

        // Unreachable code (2xx)
        /// <summary>Code that can never be executed.</summary>
        UnreachableCode = 201,

        // Constant/predictable results (3xx)
        /// <summary>Condition is always true or false.</summary>
        ConstantCondition = 301,

        /// <summary>Division by a constant zero.</summary>
        DivisionByConstantZero = 302,

        /// <summary>Potential infinite loop.</summary>
        InfiniteLoop = 303,

        /// <summary>Comparing a variable to itself.</summary>
        SelfComparison = 304,

        /// <summary>Redundant condition check.</summary>
        RedundantCondition = 305,

        // Safety warnings (4xx)
        /// <summary>Possible null reference.</summary>
        PossibleNullReference = 401,

        /// <summary>Variable used before initialization.</summary>
        UninitializedVariable = 402,

        /// <summary>Variable shadows outer scope variable.</summary>
        VariableShadowing = 403,

        // Code quality (5xx)
        /// <summary>Empty block statement.</summary>
        EmptyBlock = 501,

        /// <summary>Missing return statement in function.</summary>
        MissingReturn = 502,

        /// <summary>Value assigned but never read.</summary>
        DeadStore = 503
    }

    /// <summary>
    /// Severity levels for warnings.
    /// </summary>
    public enum WarningSeverity
    {
        /// <summary>Informational message.</summary>
        Info,

        /// <summary>Warning - potential issue.</summary>
        Warning,

        /// <summary>Error-level warning - likely a bug.</summary>
        Error
    }
}
