using System;
using CompilatorLFT.Models;

namespace CompilatorLFT.Models
{
    /// <summary>
    /// Represents a variable in the symbol table.
    /// </summary>
    /// <remarks>
    /// Reference: Dragon Book, Ch. 2.7 - "Symbol Tables"
    ///
    /// The symbol table stores information about each declared identifier:
    /// - Name (identifier)
    /// - Type (int, double, string)
    /// - Current value
    /// - Whether it has been initialized
    /// - Declaration location (for errors)
    /// </remarks>
    public class Variable
    {
        #region Properties

        /// <summary>
        /// The variable name (identifier).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The variable type (int, double, string).
        /// </summary>
        public DataType Type { get; }

        /// <summary>
        /// The current value of the variable.
        /// </summary>
        /// <remarks>
        /// Can be:
        /// - int for DataType.Int
        /// - double for DataType.Double
        /// - string for DataType.String
        /// - null if uninitialized
        /// </remarks>
        public object Value { get; set; }

        /// <summary>
        /// Indicates whether the variable has been initialized with a value.
        /// </summary>
        public bool IsInitialized { get; set; }

        /// <summary>
        /// The line where the variable was declared (for errors).
        /// </summary>
        public int DeclarationLine { get; }

        /// <summary>
        /// The column where the variable was declared (for errors).
        /// </summary>
        public int DeclarationColumn { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new uninitialized variable.
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="type">Variable type</param>
        /// <param name="line">Declaration line</param>
        /// <param name="column">Declaration column</param>
        /// <exception cref="ArgumentException">
        /// If name is empty or type is unknown
        /// </exception>
        public Variable(string name, DataType type, int line, int column)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Variable name cannot be empty", nameof(name));

            if (type == DataType.Unknown)
                throw new ArgumentException("Variable type must be valid", nameof(type));

            Name = name;
            Type = type;
            Value = null;
            IsInitialized = false;
            DeclarationLine = line;
            DeclarationColumn = column;
        }

        /// <summary>
        /// Initializes a new variable with an initial value.
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="type">Variable type</param>
        /// <param name="value">Initial value</param>
        /// <param name="line">Declaration line</param>
        /// <param name="column">Declaration column</param>
        public Variable(string name, DataType type, object value, int line, int column)
            : this(name, type, line, column)
        {
            SetValue(value);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the variable value with type validation.
        /// </summary>
        /// <param name="value">The new value</param>
        /// <exception cref="ArgumentException">
        /// If value does not match the variable type
        /// </exception>
        public void SetValue(object value)
        {
            // Type validation
            if (!ValidateType(value))
            {
                throw new ArgumentException(
                    $"Value of type '{value?.GetType().Name ?? "null"}' " +
                    $"does not match variable type '{Type}'",
                    nameof(value));
            }

            Value = value;
            IsInitialized = true;
        }

        /// <summary>
        /// Checks if a value is compatible with the variable type.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if compatible, False otherwise</returns>
        public bool ValidateType(object value)
        {
            if (value == null)
                return false;

            return Type switch
            {
                DataType.Int => value is int,
                DataType.Double => value is double || value is int, // Int can be promoted to double
                DataType.String => value is string,
                _ => false
            };
        }

        /// <summary>
        /// Gets the variable value cast to the specified type.
        /// </summary>
        /// <typeparam name="T">The desired type</typeparam>
        /// <returns>The cast value</returns>
        /// <exception cref="InvalidOperationException">
        /// If the variable is not initialized
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// If the value cannot be cast to the desired type
        /// </exception>
        public T GetValue<T>()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException(
                    $"Variable '{Name}' was not initialized");
            }

            if (Value is T castValue)
                return castValue;

            throw new InvalidCastException(
                $"Variable '{Name}' value cannot be cast to {typeof(T).Name}");
        }

        /// <summary>
        /// Resets the variable to uninitialized state.
        /// </summary>
        public void Reset()
        {
            Value = null;
            IsInitialized = false;
        }

        /// <summary>
        /// Returns text representation for debugging.
        /// </summary>
        public override string ToString()
        {
            string state = IsInitialized ? $" = {FormatValue()}" : " (uninitialized)";
            return $"{Type} {Name}{state}";
        }

        /// <summary>
        /// Format value for display.
        /// </summary>
        private string FormatValue()
        {
            if (Value == null)
                return "null";

            if (Value is string str)
                return $"\"{str}\"";

            return Value.ToString();
        }

        /// <summary>
        /// Compares two variables for equality (based on name).
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is Variable other)
            {
                return Name == other.Name;
            }
            return false;
        }

        /// <summary>
        /// Returns hash code based on name.
        /// </summary>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        #endregion
    }
}
