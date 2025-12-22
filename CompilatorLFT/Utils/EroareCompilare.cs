using System;
using CompilatorLFT.Models;

namespace CompilatorLFT.Utils
{
    /// <summary>
    /// Represents a compilation error with detailed location and type information.
    /// </summary>
    /// <remarks>
    /// Required format: "at line X, column Y: [type] error - [message]"
    /// Reference: Project requirements, point 6
    /// </remarks>
    public class CompilationError
    {
        #region Properties

        /// <summary>
        /// The line number where the error occurred (1-indexed).
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// The column number where the error occurred (1-indexed).
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// The type of error: lexical, syntactic or semantic.
        /// </summary>
        public ErrorType Type { get; }

        /// <summary>
        /// The descriptive error message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Source text that caused the error (optional, for context).
        /// </summary>
        public string SourceText { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilationError"/> class.
        /// </summary>
        /// <param name="line">Line number (1-indexed)</param>
        /// <param name="column">Column number (1-indexed)</param>
        /// <param name="type">Error type</param>
        /// <param name="message">Descriptive message</param>
        /// <param name="sourceText">Optional source text for context</param>
        /// <exception cref="ArgumentException">
        /// If line or column are less than 1, or if message is empty
        /// </exception>
        public CompilationError(int line, int column, ErrorType type, string message, string sourceText = "")
        {
            if (line < 1)
                throw new ArgumentException("Line number must be greater than or equal to 1", nameof(line));

            if (column < 1)
                throw new ArgumentException("Column number must be greater than or equal to 1", nameof(column));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Error message cannot be empty", nameof(message));

            Line = line;
            Column = column;
            Type = type;
            Message = message;
            SourceText = sourceText ?? string.Empty;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the textual representation of the error in the required format.
        /// </summary>
        /// <returns>
        /// String in format: "at line X, column Y: [type] error - [message]"
        /// </returns>
        /// <example>
        /// <code>
        /// var error = new CompilationError(5, 12, ErrorType.Semantic, "variable 'x' was not declared");
        /// Console.WriteLine(error.ToString());
        /// // Output: at line 5, column 12: semantic error - variable 'x' was not declared
        /// </code>
        /// </example>
        public override string ToString()
        {
            string typeStr = GetTypeName();
            return $"at line {Line}, column {Column}: {typeStr} error - {Message}";
        }

        /// <summary>
        /// Returns the textual representation with additional context (source text).
        /// </summary>
        /// <returns>String with error and source text that caused it</returns>
        public string ToStringWithContext()
        {
            string representation = ToString();

            if (!string.IsNullOrWhiteSpace(SourceText))
            {
                representation += Environment.NewLine;
                representation += $"  Context: {SourceText}";

                // Add visual indicator for exact position
                if (Column <= SourceText.Length)
                {
                    representation += Environment.NewLine;
                    representation += "  " + new string(' ', Column - 1) + "^";
                }
            }

            return representation;
        }

        /// <summary>
        /// Compares two errors for equality based on line, column and message.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is CompilationError other)
            {
                return Line == other.Line &&
                       Column == other.Column &&
                       Type == other.Type &&
                       Message == other.Message;
            }
            return false;
        }

        /// <summary>
        /// Returns hash code for the error.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Line, Column, Type, Message);
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Converts the error type to a display name.
        /// </summary>
        /// <returns>Type name in English</returns>
        private string GetTypeName()
        {
            return Type switch
            {
                ErrorType.Lexical => "lexical",
                ErrorType.Syntactic => "syntactic",
                ErrorType.Semantic => "semantic",
                _ => "unknown"
            };
        }

        #endregion

        #region Factory Methods (for convenience)

        /// <summary>
        /// Creates a lexical error.
        /// </summary>
        public static CompilationError Lexical(int line, int column, string message, string sourceText = "")
        {
            return new CompilationError(line, column, ErrorType.Lexical, message, sourceText);
        }

        /// <summary>
        /// Creates a syntactic error.
        /// </summary>
        public static CompilationError Syntactic(int line, int column, string message, string sourceText = "")
        {
            return new CompilationError(line, column, ErrorType.Syntactic, message, sourceText);
        }

        /// <summary>
        /// Creates a semantic error.
        /// </summary>
        public static CompilationError Semantic(int line, int column, string message, string sourceText = "")
        {
            return new CompilationError(line, column, ErrorType.Semantic, message, sourceText);
        }

        #endregion
    }
}
