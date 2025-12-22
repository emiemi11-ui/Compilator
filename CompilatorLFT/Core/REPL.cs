using System;
using System.Collections.Generic;
using System.Linq;
using CompilatorLFT.Models;
using CompilatorLFT.Models.Statements;
using CompilatorLFT.Utils;

namespace CompilatorLFT.Core
{
    /// <summary>
    /// Read-Eval-Print Loop for interactive code execution.
    /// Provides an interactive command-line interface for the compiler.
    /// </summary>
    /// <remarks>
    /// Reference: Flex & Bison, Ch. 3 - Interactive Calculator
    /// Reference: Dragon Book, Ch. 1 - Introduction to Compilers
    ///
    /// Features:
    /// - Line-by-line code execution
    /// - Persistent symbol table between commands
    /// - Command history
    /// - Special commands for inspection and control
    /// - Multi-line input support
    ///
    /// Similar to: Python REPL, Node.js REPL, Scala REPL
    /// </remarks>
    public class REPL
    {
        #region Private Fields

        private SymbolTable _symbolTable;
        private Dictionary<string, FunctionDeclaration> _functions;
        private readonly List<string> _history;
        private readonly List<CompilationError> _errors;
        private bool _running;
        private int _lineNumber;
        private bool _showAst;
        private bool _showTac;
        private bool _verbose;

        #endregion

        #region Properties

        /// <summary>Whether the REPL is running.</summary>
        public bool IsRunning => _running;

        /// <summary>Command history.</summary>
        public IReadOnlyList<string> History => _history;

        /// <summary>Current line number.</summary>
        public int LineNumber => _lineNumber;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new REPL instance.
        /// </summary>
        public REPL()
        {
            _history = new List<string>();
            _errors = new List<CompilationError>();
            Reset();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the REPL main loop.
        /// </summary>
        public void Start()
        {
            _running = true;
            DisplayWelcome();

            while (_running)
            {
                try
                {
                    Console.Write(GetPrompt());
                    string input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                        continue;

                    ProcessInput(input.Trim());
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Executes a single line of code.
        /// </summary>
        /// <param name="code">The code to execute</param>
        /// <returns>True if execution succeeded</returns>
        public bool Execute(string code)
        {
            _errors.Clear();

            try
            {
                // Lexical analysis
                var lexer = new Lexer(code);
                var tokens = lexer.Tokenize();

                if (lexer.Errors.Any())
                {
                    DisplayErrors("Lexical", lexer.Errors);
                    return false;
                }

                // Syntactic analysis (with existing symbol table)
                var parser = new Parser(code);
                var program = parser.ParseProgram();

                // Merge symbols
                foreach (var variable in parser.SymbolTable.Variables)
                {
                    if (!_symbolTable.Exists(variable.Name))
                    {
                        _symbolTable.Add(variable.Name, variable.Type,
                            variable.DeclarationLine, variable.DeclarationColumn, _errors);
                    }
                }

                // Merge functions
                foreach (var (name, func) in parser.Functions)
                {
                    _functions[name] = func;
                }

                if (parser.Errors.Any())
                {
                    DisplayErrors("Syntactic/Semantic", parser.Errors);
                    return false;
                }

                // Show AST if enabled
                if (_showAst)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\n=== AST ===");
                    Console.ResetColor();
                    program.DisplayTree();
                }

                // Show TAC if enabled
                if (_showTac)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\n=== TAC ===");
                    Console.ResetColor();
                    var tacGenerator = new ThreeAddressCodeGenerator(_symbolTable, _functions);
                    tacGenerator.GenerateCode(program);
                    tacGenerator.DisplayGeneratedCode();
                }

                // Evaluate
                var evaluator = new Evaluator(_symbolTable, _functions);
                evaluator.ExecuteProgram(program);

                if (evaluator.Errors.Any())
                {
                    DisplayErrors("Runtime", evaluator.Errors);
                    return false;
                }

                _lineNumber++;
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Execution error: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        /// <summary>
        /// Resets the REPL state.
        /// </summary>
        public void Reset()
        {
            _symbolTable = new SymbolTable();
            _functions = new Dictionary<string, FunctionDeclaration>();
            _errors.Clear();
            _lineNumber = 1;
            _showAst = false;
            _showTac = false;
            _verbose = false;
        }

        #endregion

        #region Input Processing

        /// <summary>
        /// Processes user input.
        /// </summary>
        private void ProcessInput(string input)
        {
            // Add to history
            _history.Add(input);

            // Check for special commands
            if (input.StartsWith(":"))
            {
                ProcessCommand(input);
                return;
            }

            // Check for multi-line input
            if (input.EndsWith("{") && !input.Contains("}"))
            {
                input = ReadMultiLine(input);
            }

            // Execute code
            Execute(input);
        }

        /// <summary>
        /// Reads multi-line input.
        /// </summary>
        private string ReadMultiLine(string firstLine)
        {
            var lines = new List<string> { firstLine };
            int braceCount = 1;

            while (braceCount > 0)
            {
                Console.Write("... ");
                string line = Console.ReadLine();

                if (line == null)
                    break;

                lines.Add(line);
                braceCount += line.Count(c => c == '{');
                braceCount -= line.Count(c => c == '}');
            }

            return string.Join("\n", lines);
        }

        /// <summary>
        /// Processes a special command.
        /// </summary>
        private void ProcessCommand(string input)
        {
            var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            string command = parts[0].ToLower();
            string args = parts.Length > 1 ? parts[1] : "";

            switch (command)
            {
                case ":help":
                case ":h":
                case ":?":
                    DisplayHelp();
                    break;

                case ":vars":
                case ":v":
                    DisplayVariables();
                    break;

                case ":funcs":
                case ":f":
                    DisplayFunctions();
                    break;

                case ":history":
                case ":hist":
                    DisplayHistory();
                    break;

                case ":clear":
                case ":c":
                    Console.Clear();
                    DisplayWelcome();
                    break;

                case ":reset":
                case ":r":
                    Reset();
                    Console.WriteLine("REPL state reset.");
                    break;

                case ":ast":
                    _showAst = !_showAst;
                    Console.WriteLine($"AST display: {(_showAst ? "ON" : "OFF")}");
                    break;

                case ":tac":
                    _showTac = !_showTac;
                    Console.WriteLine($"TAC display: {(_showTac ? "ON" : "OFF")}");
                    break;

                case ":verbose":
                    _verbose = !_verbose;
                    Console.WriteLine($"Verbose mode: {(_verbose ? "ON" : "OFF")}");
                    break;

                case ":type":
                    ShowVariableType(args);
                    break;

                case ":load":
                    LoadFile(args);
                    break;

                case ":examples":
                case ":ex":
                    DisplayExamples();
                    break;

                case ":exit":
                case ":quit":
                case ":q":
                    _running = false;
                    Console.WriteLine("Goodbye!");
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Unknown command: {command}");
                    Console.WriteLine("Type ':help' for available commands.");
                    Console.ResetColor();
                    break;
            }
        }

        #endregion

        #region Display Methods

        /// <summary>
        /// Displays welcome message.
        /// </summary>
        private void DisplayWelcome()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════╗
║                                                               ║
║     ██╗     ███████╗████████╗    ██████╗ ███████╗██████╗ ██╗  ║
║     ██║     ██╔════╝╚══██╔══╝    ██╔══██╗██╔════╝██╔══██╗██║  ║
║     ██║     █████╗     ██║       ██████╔╝█████╗  ██████╔╝██║  ║
║     ██║     ██╔══╝     ██║       ██╔══██╗██╔══╝  ██╔═══╝ ██║  ║
║     ███████╗██║        ██║       ██║  ██║███████╗██║     ███████╗
║     ╚══════╝╚═╝        ╚═╝       ╚═╝  ╚═╝╚══════╝╚═╝     ╚══════╝
║                                                               ║
║     LFT Compiler - Interactive REPL                           ║
║     Type ':help' for available commands                       ║
║                                                               ║
╚═══════════════════════════════════════════════════════════════╝
");
            Console.ResetColor();
        }

        /// <summary>
        /// Displays help information.
        /// </summary>
        private void DisplayHelp()
        {
            Console.WriteLine(@"
=== REPL COMMANDS ===

Code Execution:
  Simply type code and press Enter to execute.
  Multi-line input: start with '{' and end with '}'

Inspection Commands:
  :vars, :v        - Show all declared variables
  :funcs, :f       - Show all declared functions
  :type <var>      - Show type of a variable
  :history, :hist  - Show command history

Display Toggles:
  :ast             - Toggle AST display
  :tac             - Toggle Three-Address Code display
  :verbose         - Toggle verbose output

Session Commands:
  :clear, :c       - Clear screen
  :reset, :r       - Reset REPL state (clear variables)
  :load <file>     - Load and execute a file
  :examples, :ex   - Show example code

Exit:
  :exit, :quit, :q - Exit the REPL

Examples:
  int x = 10;
  double y = 3.14;
  x + y * 2
  function add(int a, int b) { return a + b; }
  print(add(5, 3));
");
        }

        /// <summary>
        /// Displays all variables.
        /// </summary>
        private void DisplayVariables()
        {
            Console.WriteLine("\n=== VARIABLES ===");

            if (_symbolTable.VariableCount == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No variables declared.");
                Console.ResetColor();
                return;
            }

            foreach (var variable in _symbolTable.Variables)
            {
                string value = variable.IsInitialized
                    ? FormatValue(variable.Value)
                    : "<uninitialized>";

                Console.Write($"  {variable.Name}: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{variable.Type}");
                Console.ResetColor();
                Console.WriteLine($" = {value}");
            }
        }

        /// <summary>
        /// Displays all functions.
        /// </summary>
        private void DisplayFunctions()
        {
            Console.WriteLine("\n=== FUNCTIONS ===");

            if (_functions.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No functions declared.");
                Console.ResetColor();
                return;
            }

            foreach (var (name, func) in _functions)
            {
                string returnType = func.ReturnType != null
                    ? func.ReturnType.Text
                    : "dynamic";

                var parameters = string.Join(", ",
                    func.Parameters.Select(p => $"{p.TypeKeyword.Text} {p.Identifier.Text}"));

                Console.Write($"  {name}(");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(parameters);
                Console.ResetColor();
                Console.Write($"): ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(returnType);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Displays command history.
        /// </summary>
        private void DisplayHistory()
        {
            Console.WriteLine("\n=== COMMAND HISTORY ===");

            if (_history.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No commands in history.");
                Console.ResetColor();
                return;
            }

            for (int i = 0; i < _history.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"[{i + 1}] ");
                Console.ResetColor();
                Console.WriteLine(_history[i].Length > 60
                    ? _history[i].Substring(0, 57) + "..."
                    : _history[i]);
            }
        }

        /// <summary>
        /// Shows the type of a variable.
        /// </summary>
        private void ShowVariableType(string varName)
        {
            varName = varName.Trim();

            if (string.IsNullOrEmpty(varName))
            {
                Console.WriteLine("Usage: :type <variable_name>");
                return;
            }

            var variable = _symbolTable.Get(varName);

            if (variable == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Variable '{varName}' not found.");
                Console.ResetColor();
                return;
            }

            Console.Write($"{varName}: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(variable.Type);
            Console.ResetColor();

            if (variable.IsInitialized)
            {
                Console.Write($" = {FormatValue(variable.Value)}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Loads and executes a file.
        /// </summary>
        private void LoadFile(string filename)
        {
            filename = filename.Trim();

            if (string.IsNullOrEmpty(filename))
            {
                Console.WriteLine("Usage: :load <filename>");
                return;
            }

            try
            {
                string content = System.IO.File.ReadAllText(filename);
                Console.WriteLine($"Loading '{filename}'...");

                if (Execute(content))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Successfully loaded '{filename}'");
                    Console.ResetColor();
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"File not found: {filename}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error loading file: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Displays example code.
        /// </summary>
        private void DisplayExamples()
        {
            Console.WriteLine(@"
=== EXAMPLE CODE ===

1. Variable Declaration:
   int x = 10;
   double pi = 3.14159;
   string name = ""LFT"";
   bool active = true;

2. Expressions:
   x * 2 + 5
   sqrt(16) + abs(-5)
   ""Hello "" + ""World""

3. Control Flow:
   if (x > 5) { print(""big""); } else { print(""small""); }
   for (int i = 0; i < 5; i++) { print(i); }
   while (x > 0) { print(x); x--; }

4. Functions:
   function factorial(int n) {
       if (n <= 1) return 1;
       return n * factorial(n - 1);
   }
   print(factorial(5));

5. Built-in Functions:
   print(sqrt(16));      // 4
   print(pow(2, 8));     // 256
   print(length(""test"")); // 4
");
        }

        /// <summary>
        /// Displays errors.
        /// </summary>
        private void DisplayErrors(string phase, IEnumerable<CompilationError> errors)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n{phase} errors:");

            foreach (var error in errors)
            {
                Console.WriteLine($"  × {error}");
            }

            Console.ResetColor();
        }

        /// <summary>
        /// Gets the command prompt.
        /// </summary>
        private string GetPrompt()
        {
            return $"[{_lineNumber}]>>> ";
        }

        /// <summary>
        /// Formats a value for display.
        /// </summary>
        private string FormatValue(object value)
        {
            return value switch
            {
                null => "null",
                string s => $"\"{s}\"",
                bool b => b.ToString().ToLower(),
                double d when Math.Abs(d - Math.Round(d)) < double.Epsilon => ((int)d).ToString(),
                _ => value.ToString()
            };
        }

        #endregion
    }
}
