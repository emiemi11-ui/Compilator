using System;
using System.Collections.Generic;
using CompilatorLFT.Models;
using CompilatorLFT.Utils;

namespace CompilatorLFT.Core
{
    /// <summary>
    /// Manages nested scopes for variables and functions.
    /// Implements hierarchical symbol table with scope chaining.
    /// </summary>
    /// <remarks>
    /// Reference: Dragon Book, Ch. 2.7 - Symbol Tables
    /// Reference: Grigoraș "Proiectarea Compilatoarelor", Cap. 6.4
    ///
    /// Scope rules:
    /// - Global scope contains top-level declarations
    /// - Function scope contains parameters and local variables
    /// - Block scope contains variables declared within { }
    /// - Inner scopes can shadow outer scope variables
    /// - Variable lookup proceeds from innermost to outermost scope
    /// </remarks>
    public class ScopeManager
    {
        #region Private Fields

        private readonly Stack<Scope> _scopeStack;
        private int _scopeCounter;
        private readonly List<Scope> _allScopes;  // For debugging

        #endregion

        #region Properties

        /// <summary>Gets the current (innermost) scope.</summary>
        public Scope CurrentScope => _scopeStack.Count > 0 ? _scopeStack.Peek() : null;

        /// <summary>Gets the global (outermost) scope.</summary>
        public Scope GlobalScope { get; private set; }

        /// <summary>Gets the current scope nesting depth.</summary>
        public int CurrentDepth => _scopeStack.Count;

        /// <summary>Gets all scopes for analysis.</summary>
        public IReadOnlyList<Scope> AllScopes => _allScopes;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new ScopeManager with a global scope.
        /// </summary>
        public ScopeManager()
        {
            _scopeStack = new Stack<Scope>();
            _scopeCounter = 0;
            _allScopes = new List<Scope>();

            // Create and push global scope
            GlobalScope = new Scope(_scopeCounter++, "global", null, 0);
            _scopeStack.Push(GlobalScope);
            _allScopes.Add(GlobalScope);
        }

        #endregion

        #region Scope Management

        /// <summary>
        /// Pushes a new scope onto the stack.
        /// </summary>
        /// <param name="name">Descriptive name for the scope (e.g., "function_factorial", "block_42")</param>
        /// <returns>The newly created scope</returns>
        public Scope PushScope(string name)
        {
            var parent = CurrentScope;
            var scope = new Scope(_scopeCounter++, name, parent, _scopeStack.Count);
            _scopeStack.Push(scope);
            _allScopes.Add(scope);
            return scope;
        }

        /// <summary>
        /// Pushes a new function scope with parameter support.
        /// </summary>
        /// <param name="functionName">Name of the function</param>
        /// <returns>The newly created function scope</returns>
        public Scope PushFunctionScope(string functionName)
        {
            return PushScope($"function_{functionName}");
        }

        /// <summary>
        /// Pushes a new block scope (for if/while/for bodies).
        /// </summary>
        /// <param name="line">Line number where block starts</param>
        /// <returns>The newly created block scope</returns>
        public Scope PushBlockScope(int line)
        {
            return PushScope($"block_L{line}");
        }

        /// <summary>
        /// Pops the current scope from the stack.
        /// </summary>
        /// <returns>The popped scope</returns>
        /// <exception cref="InvalidOperationException">If trying to pop the global scope</exception>
        public Scope PopScope()
        {
            if (_scopeStack.Count <= 1)
            {
                throw new InvalidOperationException("Cannot pop the global scope");
            }

            var popped = _scopeStack.Pop();
            popped.MarkClosed();
            return popped;
        }

        /// <summary>
        /// Resets the scope manager to initial state (global scope only).
        /// </summary>
        public void Reset()
        {
            _scopeStack.Clear();
            _allScopes.Clear();
            _scopeCounter = 0;
            GlobalScope = new Scope(_scopeCounter++, "global", null, 0);
            _scopeStack.Push(GlobalScope);
            _allScopes.Add(GlobalScope);
        }

        #endregion

        #region Variable Management

        /// <summary>
        /// Declares a variable in the current scope.
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="type">Variable type</param>
        /// <param name="line">Declaration line</param>
        /// <param name="column">Declaration column</param>
        /// <param name="errors">Error list for reporting</param>
        /// <returns>True if declaration succeeded</returns>
        public bool DeclareVariable(
            string name,
            DataType type,
            int line,
            int column,
            List<CompilationError> errors)
        {
            var currentScope = CurrentScope;

            // Check if already declared in current scope
            if (currentScope.HasVariable(name))
            {
                errors.Add(CompilationError.Semantic(
                    line, column,
                    $"variable '{name}' is already declared in current scope"));
                return false;
            }

            // Check for shadowing (warning, not error)
            var shadowedVar = LookupVariableInChain(name, currentScope.Parent);
            if (shadowedVar != null)
            {
                // Shadowing is allowed but worth noting for static analysis
                currentScope.AddShadowedVariable(name);
            }

            currentScope.AddVariable(name, type, line, column);
            return true;
        }

        /// <summary>
        /// Looks up a variable in the scope chain (innermost to outermost).
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="line">Reference line (for error reporting)</param>
        /// <param name="column">Reference column</param>
        /// <param name="errors">Error list</param>
        /// <returns>The variable or null if not found</returns>
        public Variable LookupVariable(
            string name,
            int line,
            int column,
            List<CompilationError> errors)
        {
            var variable = LookupVariableInChain(name, CurrentScope);

            if (variable == null)
            {
                errors.Add(CompilationError.Semantic(
                    line, column,
                    $"variable '{name}' was not declared"));
                return null;
            }

            return variable;
        }

        /// <summary>
        /// Checks if a variable exists in any accessible scope.
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <returns>True if variable exists</returns>
        public bool VariableExists(string name)
        {
            return LookupVariableInChain(name, CurrentScope) != null;
        }

        /// <summary>
        /// Gets a variable's value with scope resolution.
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="line">Line for error reporting</param>
        /// <param name="column">Column for error reporting</param>
        /// <param name="errors">Error list</param>
        /// <returns>Variable value or null</returns>
        public object GetVariableValue(
            string name,
            int line,
            int column,
            List<CompilationError> errors)
        {
            var variable = LookupVariable(name, line, column, errors);

            if (variable == null)
                return null;

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
        /// Sets a variable's value with type checking.
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="value">Value to set</param>
        /// <param name="line">Line for error reporting</param>
        /// <param name="column">Column for error reporting</param>
        /// <param name="errors">Error list</param>
        /// <returns>True if successful</returns>
        public bool SetVariableValue(
            string name,
            object value,
            int line,
            int column,
            List<CompilationError> errors)
        {
            var variable = LookupVariable(name, line, column, errors);

            if (variable == null)
                return false;

            // Type checking
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

        #endregion

        #region Helper Methods

        /// <summary>
        /// Looks up a variable starting from a given scope.
        /// </summary>
        private Variable LookupVariableInChain(string name, Scope startScope)
        {
            var scope = startScope;

            while (scope != null)
            {
                if (scope.TryGetVariable(name, out var variable))
                    return variable;
                scope = scope.Parent;
            }

            return null;
        }

        /// <summary>
        /// Checks if a value is compatible with a type.
        /// </summary>
        private bool CheckTypeCompatibility(DataType type, object value)
        {
            if (value == null)
                return false;

            return type switch
            {
                DataType.Int => value is int || value is double || value is bool,
                DataType.Double => value is int || value is double || value is bool,
                DataType.String => value is string,
                DataType.Bool => value is bool || value is int || value is double,
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
                DataType.Int when value is bool b => b ? 1 : 0,
                DataType.Double when value is double d => d,
                DataType.Double when value is int i => (double)i,
                DataType.Double when value is bool b => b ? 1.0 : 0.0,
                DataType.String when value is string s => s,
                DataType.Bool when value is bool b => b,
                DataType.Bool when value is int i => i != 0,
                DataType.Bool when value is double d => Math.Abs(d) > 1e-10,
                _ => value
            };
        }

        #endregion

        #region Display Methods

        /// <summary>
        /// Displays the scope hierarchy for debugging.
        /// </summary>
        public void DisplayScopes()
        {
            Console.WriteLine("\n=== SCOPE HIERARCHY ===");

            foreach (var scope in _allScopes)
            {
                string indent = new string(' ', scope.Depth * 2);
                string status = scope.IsClosed ? " (closed)" : " (active)";

                Console.WriteLine($"{indent}[{scope.Id}] {scope.Name}{status}");
                Console.WriteLine($"{indent}  Variables: {scope.VariableCount}");

                foreach (var variable in scope.Variables)
                {
                    string initStatus = variable.IsInitialized
                        ? $"= {variable.Value}"
                        : "(uninitialized)";
                    Console.WriteLine($"{indent}    - {variable.Name}: {variable.Type} {initStatus}");
                }

                if (scope.ShadowedVariables.Count > 0)
                {
                    Console.WriteLine($"{indent}  Shadows: {string.Join(", ", scope.ShadowedVariables)}");
                }
            }
        }

        /// <summary>
        /// Gets scope statistics.
        /// </summary>
        public ScopeStatistics GetStatistics()
        {
            int totalVariables = 0;
            int maxDepth = 0;
            int shadowedCount = 0;

            foreach (var scope in _allScopes)
            {
                totalVariables += scope.VariableCount;
                maxDepth = Math.Max(maxDepth, scope.Depth);
                shadowedCount += scope.ShadowedVariables.Count;
            }

            return new ScopeStatistics
            {
                TotalScopes = _allScopes.Count,
                TotalVariables = totalVariables,
                MaxDepth = maxDepth,
                ShadowedVariables = shadowedCount
            };
        }

        #endregion
    }

    /// <summary>
    /// Represents a single scope level in the symbol table.
    /// </summary>
    public class Scope
    {
        #region Private Fields

        private readonly Dictionary<string, Variable> _variables;
        private readonly HashSet<string> _shadowedVariables;

        #endregion

        #region Properties

        /// <summary>Unique scope identifier.</summary>
        public int Id { get; }

        /// <summary>Descriptive scope name.</summary>
        public string Name { get; }

        /// <summary>Parent scope (null for global).</summary>
        public Scope Parent { get; }

        /// <summary>Nesting depth (0 for global).</summary>
        public int Depth { get; }

        /// <summary>Whether scope is closed (no longer active).</summary>
        public bool IsClosed { get; private set; }

        /// <summary>Number of variables in this scope.</summary>
        public int VariableCount => _variables.Count;

        /// <summary>All variables in this scope.</summary>
        public IEnumerable<Variable> Variables => _variables.Values;

        /// <summary>Variables that shadow outer scope variables.</summary>
        public IReadOnlyCollection<string> ShadowedVariables => _shadowedVariables;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new scope.
        /// </summary>
        /// <param name="id">Unique identifier</param>
        /// <param name="name">Descriptive name</param>
        /// <param name="parent">Parent scope (null for global)</param>
        /// <param name="depth">Nesting depth</param>
        public Scope(int id, string name, Scope parent, int depth)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Parent = parent;
            Depth = depth;
            IsClosed = false;
            _variables = new Dictionary<string, Variable>();
            _shadowedVariables = new HashSet<string>();
        }

        #endregion

        #region Variable Management

        /// <summary>
        /// Adds a variable to this scope.
        /// </summary>
        public void AddVariable(string name, DataType type, int line, int column)
        {
            _variables[name] = new Variable(name, type, line, column);
        }

        /// <summary>
        /// Checks if a variable exists in this scope only.
        /// </summary>
        public bool HasVariable(string name)
        {
            return _variables.ContainsKey(name);
        }

        /// <summary>
        /// Tries to get a variable from this scope only.
        /// </summary>
        public bool TryGetVariable(string name, out Variable variable)
        {
            return _variables.TryGetValue(name, out variable);
        }

        /// <summary>
        /// Records that a variable shadows an outer scope variable.
        /// </summary>
        public void AddShadowedVariable(string name)
        {
            _shadowedVariables.Add(name);
        }

        /// <summary>
        /// Marks the scope as closed (exited).
        /// </summary>
        public void MarkClosed()
        {
            IsClosed = true;
        }

        #endregion

        /// <summary>
        /// Returns a string representation of the scope.
        /// </summary>
        public override string ToString()
        {
            return $"Scope[{Id}]: {Name} (depth={Depth}, vars={VariableCount})";
        }
    }

    /// <summary>
    /// Statistics about scope usage.
    /// </summary>
    public class ScopeStatistics
    {
        public int TotalScopes { get; set; }
        public int TotalVariables { get; set; }
        public int MaxDepth { get; set; }
        public int ShadowedVariables { get; set; }

        public override string ToString()
        {
            return $"Scopes: {TotalScopes}, Variables: {TotalVariables}, " +
                   $"MaxDepth: {MaxDepth}, Shadowed: {ShadowedVariables}";
        }
    }
}
