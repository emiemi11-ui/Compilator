using System;
using System.Collections.Generic;
using CompilatorLFT.Models;
using CompilatorLFT.Utils;

namespace CompilatorLFT.Core
{
    /// <summary>
    /// Manages the symbol table for variables.
    /// </summary>
    /// <remarks>
    /// Reference: Dragon Book, Ch. 2.7 - Symbol Tables
    /// </remarks>
    public class SymbolTable
    {
        #region Private Fields

        private readonly Dictionary<string, Variable> _variables;

        #endregion

        #region Properties

        /// <summary>Number of variables in the table.</summary>
        public int VariableCount => _variables.Count;

        /// <summary>All variables in the table.</summary>
        public IEnumerable<Variable> Variables => _variables.Values;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new empty symbol table.
        /// </summary>
        public SymbolTable()
        {
            _variables = new Dictionary<string, Variable>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new variable to the table.
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="type">Variable type</param>
        /// <param name="line">Declaration line</param>
        /// <param name="column">Declaration column</param>
        /// <param name="errors">Error list for reporting</param>
        /// <returns>True if addition succeeded, False if already exists</returns>
        public bool Add(string name, DataType type, int line, int column, List<CompilationError> errors)
        {
            if (_variables.ContainsKey(name))
            {
                errors.Add(CompilationError.Semantic(
                    line, column,
                    $"duplicate declaration for variable '{name}'"));
                return false;
            }

            var variable = new Variable(name, type, line, column);
            _variables[name] = variable;
            return true;
        }

        /// <summary>
        /// Checks if a variable exists in the table.
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <returns>True if exists, False otherwise</returns>
        public bool Exists(string name)
        {
            return _variables.ContainsKey(name);
        }

        /// <summary>
        /// Gets a variable from the table.
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <returns>Variable or null if not found</returns>
        public Variable Get(string name)
        {
            return _variables.TryGetValue(name, out var variable) ? variable : null;
        }

        /// <summary>
        /// Sets the value of a variable with type validation.
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="value">Value to set</param>
        /// <param name="line">Line for error reporting</param>
        /// <param name="column">Column for error reporting</param>
        /// <param name="errors">Error list</param>
        /// <returns>True if setting succeeded</returns>
        public bool SetValue(string name, object value, int line, int column, List<CompilationError> errors)
        {
            if (!_variables.ContainsKey(name))
            {
                errors.Add(CompilationError.Semantic(
                    line, column,
                    $"variable '{name}' was not declared"));
                return false;
            }

            var variable = _variables[name];

            // Check type compatibility
            if (!CheckTypeCompatibility(variable.Type, value))
            {
                string valueType = value?.GetType().Name ?? "null";
                errors.Add(CompilationError.Semantic(
                    line, column,
                    $"type mismatch: cannot assign {valueType} to variable of type {variable.Type}"));
                return false;
            }

            // Convert if necessary
            object convertedValue = ConvertToType(value, variable.Type);

            variable.SetValue(convertedValue);
            return true;
        }

        /// <summary>
        /// Gets the value of a variable with initialization check.
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="line">Line for error reporting</param>
        /// <param name="column">Column for error reporting</param>
        /// <param name="errors">Error list</param>
        /// <returns>Value or null if error</returns>
        public object GetValue(string name, int line, int column, List<CompilationError> errors)
        {
            if (!_variables.ContainsKey(name))
            {
                errors.Add(CompilationError.Semantic(
                    line, column,
                    $"variable '{name}' was not declared"));
                return null;
            }

            var variable = _variables[name];

            if (!variable.IsInitialized)
            {
                errors.Add(CompilationError.Semantic(
                    line, column,
                    $"variable '{name}' used before initialization"));
                return null;
            }

            return variable.Value;
        }

        /// <summary>
        /// Displays all variables to console.
        /// </summary>
        public void DisplayVariables()
        {
            Console.WriteLine("\n=== SYMBOL TABLE ===");

            if (_variables.Count == 0)
            {
                Console.WriteLine("(empty)");
                return;
            }

            foreach (var variable in _variables.Values)
            {
                Console.WriteLine(variable.ToString());
            }
        }

        /// <summary>
        /// Resets the symbol table.
        /// </summary>
        public void Reset()
        {
            _variables.Clear();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if a value is compatible with a type.
        /// </summary>
        private bool CheckTypeCompatibility(DataType type, object value)
        {
            if (value == null)
                return false;

            return type switch
            {
                DataType.Int => value is int || value is double,
                DataType.Double => value is int || value is double,
                DataType.String => value is string,
                _ => false
            };
        }

        /// <summary>
        /// Converts a value to the specified type.
        /// </summary>
        private object ConvertToType(object value, DataType type)
        {
            if (value == null)
                return null;

            return type switch
            {
                DataType.Int when value is int i => i,
                DataType.Int when value is double d => (int)d,
                DataType.Double when value is double d => d,
                DataType.Double when value is int i => (double)i,
                DataType.String when value is string s => s,
                _ => value
            };
        }

        /// <summary>
        /// Converts a token type to data type.
        /// </summary>
        public static DataType ConvertToDataType(TokenType type)
        {
            return type switch
            {
                TokenType.KeywordInt => DataType.Int,
                TokenType.KeywordDouble => DataType.Double,
                TokenType.KeywordString => DataType.String,
                _ => DataType.Unknown
            };
        }

        #endregion
    }
}
