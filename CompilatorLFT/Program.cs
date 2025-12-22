using System;
using System.Collections.Generic;
using System.Linq;
using CompilatorLFT.Core;
using CompilatorLFT.Models.Statements;
using CompilatorLFT.Utils;

namespace CompilatorLFT
{
    /// <summary>
    /// Main program for the LFT Compiler.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            DisplayHeader();

            // Check command line arguments
            if (args.Length > 0)
            {
                // Run from file specified as argument
                RunFromFile(args[0]);
                return;
            }

            // Interactive menu
            while (true)
            {
                DisplayMenu();
                Console.Write("\nChoice: ");
                string choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        RunFromFileInteractive();
                        break;

                    case "2":
                        RunInteractive();
                        break;

                    case "3":
                        RunAutomatedTests();
                        break;

                    case "4":
                        DisplayExamples();
                        break;

                    case "5":
                    case "q":
                    case "Q":
                        Console.WriteLine("\nGoodbye!");
                        return;

                    default:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Invalid choice! Enter a number between 1-5.");
                        Console.ResetColor();
                        break;
                }

                Console.WriteLine("\nPress ENTER to continue...");
                Console.ReadLine();
                Console.Clear();
            }
        }

        #region UI Display

        static void DisplayHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║            LFT COMPILER - ACADEMIC PROJECT               ║");
            Console.WriteLine("║       Formal Languages and Translators - UAIC Iasi       ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void DisplayMenu()
        {
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║            MAIN MENU                 ║");
            Console.WriteLine("╠══════════════════════════════════════╣");
            Console.WriteLine("║  1. Read from file                   ║");
            Console.WriteLine("║  2. Manual code input                ║");
            Console.WriteLine("║  3. Run automated tests              ║");
            Console.WriteLine("║  4. Display examples                 ║");
            Console.WriteLine("║  5. Exit                             ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
        }

        #endregion

        #region Run Modes

        static void RunFromFileInteractive()
        {
            Console.Write("\nEnter file path: ");
            string path = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(path))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Path cannot be empty!");
                Console.ResetColor();
                return;
            }

            RunFromFile(path);
        }

        static void RunFromFile(string path)
        {
            string content = FileReader.ReadFile(path);

            if (content == null)
                return;

            Console.WriteLine("\n=== SOURCE CODE ===");
            FileReader.DisplayWithLineNumbers(content);

            CompileAndRun(content);
        }

        static void RunInteractive()
        {
            Console.WriteLine("\nEnter source code (empty line to finish):");
            Console.WriteLine("Example: int a = 5; 3 + 4;");
            Console.WriteLine();

            var lines = new List<string>();
            int lineNumber = 1;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{lineNumber,3} | ");
                Console.ResetColor();

                string line = Console.ReadLine();

                if (string.IsNullOrEmpty(line))
                    break;

                lines.Add(line);
                lineNumber++;
            }

            if (lines.Count == 0)
            {
                Console.WriteLine("No code entered!");
                return;
            }

            string content = string.Join("\n", lines);
            CompileAndRun(content);
        }

        #endregion

        #region Compilation and Execution

        static void CompileAndRun(string content)
        {
            // PHASE 1: LEXICAL ANALYSIS
            Console.WriteLine("\n" + new string('=', 50));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("PHASE 1: LEXICAL ANALYSIS");
            Console.ResetColor();
            Console.WriteLine(new string('=', 50));

            var lexer = new Lexer(content);
            var tokens = lexer.Tokenize();

            Console.WriteLine($"Generated tokens: {tokens.Count}");

            if (lexer.Errors.Any())
            {
                DisplayErrors("LEXICAL ERRORS", lexer.Errors);
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Lexical analysis successful!");
            Console.ResetColor();

            // Display tokens (optional)
            Console.WriteLine("\nTokens:");
            foreach (var token in tokens.Take(20))
            {
                Console.WriteLine($"  {token}");
            }
            if (tokens.Count > 20)
            {
                Console.WriteLine($"  ... and {tokens.Count - 20} more tokens");
            }

            // PHASE 2: SYNTACTIC AND SEMANTIC ANALYSIS
            Console.WriteLine("\n" + new string('=', 50));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("PHASE 2: SYNTACTIC & SEMANTIC ANALYSIS");
            Console.ResetColor();
            Console.WriteLine(new string('=', 50));

            var parser = new Parser(content);
            var program = parser.ParseProgram();

            Console.WriteLine($"Parsed statements: {program.Statements.Count}");
            Console.WriteLine($"Declared variables: {parser.SymbolTable.VariableCount}");

            if (parser.Errors.Any())
            {
                DisplayErrors("SYNTACTIC/SEMANTIC ERRORS", parser.Errors);
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Syntactic analysis successful!");
            Console.WriteLine("Semantic analysis successful!");
            Console.ResetColor();

            // DISPLAY SYNTAX TREE
            Console.WriteLine("\n" + new string('=', 50));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("SYNTAX TREE (AST)");
            Console.ResetColor();
            Console.WriteLine(new string('=', 50));

            program.DisplayTree();

            // PHASE 3: EVALUATION AND EXECUTION
            Console.WriteLine("\n" + new string('=', 50));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("PHASE 3: EVALUATION & EXECUTION");
            Console.ResetColor();
            Console.WriteLine(new string('=', 50));

            var evaluator = new Evaluator(parser.SymbolTable);
            evaluator.ExecuteProgram(program);

            if (evaluator.Errors.Any())
            {
                DisplayErrors("RUNTIME ERRORS", evaluator.Errors);
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nExecution successful!");
            Console.ResetColor();

            // FINAL SYMBOL TABLE
            Console.WriteLine("\n" + new string('=', 50));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("SYMBOL TABLE (FINAL STATE)");
            Console.ResetColor();
            Console.WriteLine(new string('=', 50));

            parser.SymbolTable.DisplayVariables();
        }

        static void DisplayErrors(string title, IEnumerable<CompilationError> errors)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n{title}:");
            foreach (var error in errors)
            {
                Console.WriteLine($"  X {error}");
            }
            Console.ResetColor();
        }

        #endregion

        #region Automated Tests

        static void RunAutomatedTests()
        {
            Console.WriteLine("\n" + new string('=', 60));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("RUNNING AUTOMATED TESTS");
            Console.ResetColor();
            Console.WriteLine(new string('=', 60));

            var tests = new List<(string name, string code, bool expectSuccess)>
            {
                // Basic tests
                ("Simple declaration", "int a;", true),
                ("Declaration with initialization", "int a = 5;", true),
                ("Multiple declarations", "int a, b=3, c;", true),
                ("Arithmetic expression", "3 + 5;", true),
                ("Operator precedence", "3 + 4 * 5;", true),
                ("Parentheses", "(3 + 4) * 5;", true),
                ("Unary minus", "int a = -5;", true),
                ("Double literal", "double x = 3.14;", true),
                ("String literal", "string s = \"hello\";", true),
                ("String concatenation", "string a = \"hello\"; string b = \" world\"; string c = a + b;", true),

                // Assignments and operations
                ("Assignment and calculation", "int a = 5; int b = 3; int c = a + b;", true),
                ("Relational operators", "int a = 5; int b = 3; a > b;", true),

                // Control structures
                ("Simple if", "int a = 5; int b = 0; if (a > 3) { b = 10; }", true),
                ("If-else", "int a = 2; int b = 0; if (a > 3) { b = 10; } else { b = 20; }", true),
                ("While", "int i = 0; int sum = 0; while (i < 5) { sum = sum + i; i = i + 1; }", true),
                ("For", "int sum = 0; for (int i = 0; i < 5; i = i + 1) { sum = sum + i; }", true),

                // Error tests (should fail)
                ("Error: undeclared variable", "x = 5;", false),
                ("Error: duplicate declaration", "int a; int a;", false),
                ("Error: unary plus", "int a = +5;", false),
            };

            int passed = 0;
            int total = tests.Count;

            foreach (var (name, code, expectSuccess) in tests)
            {
                bool success = RunTest(name, code, expectSuccess);
                if (success)
                    passed++;
            }

            // Final report
            Console.WriteLine("\n" + new string('=', 60));
            double percent = (double)passed / total * 100;

            if (passed == total)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"ALL TESTS PASSED! ({passed}/{total} - {percent:F0}%)");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Tests passed: {passed}/{total} ({percent:F0}%)");
            }
            Console.ResetColor();
        }

        static bool RunTest(string name, string code, bool expectSuccess)
        {
            try
            {
                var parser = new Parser(code);
                var program = parser.ParseProgram();

                bool hasParseErrors = parser.Errors.Any();

                if (!hasParseErrors)
                {
                    var evaluator = new Evaluator(parser.SymbolTable);
                    evaluator.ExecuteProgram(program);

                    bool hasEvalErrors = evaluator.Errors.Any();

                    if (expectSuccess && !hasEvalErrors)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  PASS: {name}");
                        Console.ResetColor();
                        return true;
                    }
                    else if (!expectSuccess && hasEvalErrors)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  PASS: {name} (error detected correctly)");
                        Console.ResetColor();
                        return true;
                    }
                }
                else
                {
                    if (!expectSuccess)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  PASS: {name} (error detected correctly)");
                        Console.ResetColor();
                        return true;
                    }
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  FAIL: {name}");
                Console.ResetColor();
                return false;
            }
            catch (Exception ex)
            {
                if (!expectSuccess)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  PASS: {name} (error detected: {ex.Message})");
                    Console.ResetColor();
                    return true;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  FAIL: {name} - Exception: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        #endregion

        #region Examples

        static void DisplayExamples()
        {
            Console.WriteLine("\n" + new string('=', 60));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("SOURCE CODE EXAMPLES");
            Console.ResetColor();
            Console.WriteLine(new string('=', 60));

            var examples = new[]
            {
                ("Declarations and expressions", @"
int a = 5;
int b = 3;
int sum = a + b;
sum * 2;
"),
                ("Control structures", @"
int sum = 0;
for (int i = 1; i <= 10; i = i + 1) {
    sum = sum + i;
}
sum;
"),
                ("Conditionals", @"
int x = 7;
int result;
if (x > 5) {
    result = 100;
} else {
    result = 0;
}
result;
"),
                ("Strings", @"
string greeting = ""Hello"";
string name = "" World"";
string message = greeting + name;
message;
")
            };

            foreach (var (title, code) in examples)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n--- {title} ---");
                Console.ResetColor();
                Console.WriteLine(code.Trim());
            }
        }

        #endregion
    }
}
