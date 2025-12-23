using System;
using System.Collections.Generic;
using System.Linq;
using CompilatorLFT.Core;
using CompilatorLFT.Models;
using CompilatorLFT.Models.Statements;
using CompilatorLFT.Utils;
using ProgramNode = CompilatorLFT.Models.Statements.Program;

namespace CompilatorLFT
{
    /// <summary>
    /// Main program with beautiful interactive UI
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Check for file argument
            if (args.Length > 0)
            {
                RunFromFile(args[0]);
                return;
            }

            // Interactive mode
            while (true)
            {
                DisplayHeader();
                DisplayMainMenu();

                Console.Write("\n➤ Alege opțiunea: ");
                string choice = Console.ReadLine()?.Trim();

                Console.Clear();

                switch (choice)
                {
                    case "1":
                        RunInteractiveConsole();
                        break;

                    case "2":
                        RunFromFileInteractive();
                        break;

                    case "3":
                        RunREPL();
                        break;

                    case "4":
                        RunWithOptimization();
                        break;

                    case "5":
                        RunWithStaticAnalysis();
                        break;

                    case "6":
                        RunAutomatedTests();
                        break;

                    case "7":
                        ShowExamples();
                        break;

                    case "8":
                        ShowAbout();
                        break;

                    case "9":
                    case "0":
                    case "exit":
                    case "q":
                        ExitProgram();
                        return;

                    default:
                        ShowError("Opțiune invalidă! Alege un număr între 1-9.");
                        WaitForKey();
                        Console.Clear();
                        break;
                }
            }
        }

        #region UI Display Methods

        static void DisplayHeader()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                               ║");
            Console.WriteLine("║          🚀 COMPILATOR LFT - INTERACTIVE EDITION 🚀          ║");
            Console.WriteLine("║                                                               ║");
            Console.WriteLine("║         Limbaje Formale și Translatoare - UAIC Iași          ║");
            Console.WriteLine("║                                                               ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void DisplayMainMenu()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        MENIU PRINCIPAL                        ║");
            Console.WriteLine("╠═══════════════════════════════════════════════════════════════╣");
            Console.ResetColor();

            PrintMenuOption("1", "💻 Mod Interactiv (Linie cu linie)", ConsoleColor.Green);
            PrintMenuOption("2", "📁 Citire din Fișier", ConsoleColor.Green);
            PrintMenuOption("3", "⚡ REPL Mode (Comenzi rapide)", ConsoleColor.Yellow);
            PrintMenuOption("4", "🔧 Execuție cu Optimizări", ConsoleColor.Magenta);
            PrintMenuOption("5", "🔍 Analiză Statică Avansată", ConsoleColor.Magenta);
            PrintMenuOption("6", "🧪 Teste Automatizate", ConsoleColor.Blue);
            PrintMenuOption("7", "📖 Exemple de Cod", ConsoleColor.Cyan);
            PrintMenuOption("8", "ℹ️  Despre Compilator", ConsoleColor.Gray);
            PrintMenuOption("9", "🚪 Ieșire", ConsoleColor.Red);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        static void PrintMenuOption(string number, string text, ConsoleColor color)
        {
            Console.Write("║  ");
            Console.ForegroundColor = color;
            Console.Write($"[{number}]");
            Console.ResetColor();
            Console.Write(" ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text);

            // Padding for alignment
            int padding = 56 - text.Length;
            if (padding < 0) padding = 0;
            Console.Write(new string(' ', padding));
            Console.WriteLine("║");
            Console.ResetColor();
        }

        static void ShowSectionHeader(string title, string icon = "⚙️")
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"╔═══════════════════════════════════════════════════════════════╗");
            string content = $"  {icon}  {title}";
            int pad = 64 - content.Length;
            if (pad < 0) pad = 0;
            Console.WriteLine($"║{content}{new string(' ', pad)}║");
            Console.WriteLine($"╚═══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void ShowSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ {message}");
            Console.ResetColor();
        }

        static void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ {message}");
            Console.ResetColor();
        }

        static void ShowWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠ {message}");
            Console.ResetColor();
        }

        static void ShowInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"ℹ {message}");
            Console.ResetColor();
        }

        static void WaitForKey()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("Apasă orice tastă pentru a continua...");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        #endregion

        #region Mode 1: Interactive Console

        static void RunInteractiveConsole()
        {
            ShowSectionHeader("MOD INTERACTIV - Introducere Linie cu Linie", "💻");

            ShowInfo("Introdu cod LFT linie cu linie. Scrie o linie goală pentru a executa.");
            Console.WriteLine();
            ShowInfo("Exemplu:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  1 | int a = 5;");
            Console.WriteLine("  2 | int b = 10;");
            Console.WriteLine("  3 | print(a + b);");
            Console.WriteLine("  4 | <enter gol>");
            Console.ResetColor();
            Console.WriteLine();

            var lines = new List<string>();
            int lineNumber = 1;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{lineNumber,3} | ");
                Console.ResetColor();

                string line = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                {
                    if (lines.Count == 0)
                    {
                        ShowWarning("Nu ai introdus niciun cod!");
                        WaitForKey();
                        return;
                    }
                    break;
                }

                lines.Add(line);
                lineNumber++;
            }

            string code = string.Join("\n", lines);
            Console.WriteLine();
            CompileAndExecute(code);
            WaitForKey();
        }

        #endregion

        #region Mode 2: File Input

        static void RunFromFileInteractive()
        {
            ShowSectionHeader("CITIRE DIN FIȘIER", "📁");

            Console.Write("➤ Introdu calea către fișier: ");
            string path = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(path))
            {
                ShowError("Calea nu poate fi goală!");
                WaitForKey();
                return;
            }

            RunFromFile(path);
            WaitForKey();
        }

        static void RunFromFile(string path)
        {
            string content = FileReader.ReadFile(path);

            if (content == null)
            {
                ShowError($"Nu s-a putut citi fișierul: {path}");
                return;
            }

            ShowSuccess($"Fișier încărcat: {path}");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("COD SURSĂ:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.ResetColor();

            FileReader.DisplayWithLineNumbers(content);
            Console.WriteLine();

            CompileAndExecute(content);
        }

        #endregion

        #region Mode 3: REPL Mode

        static void RunREPL()
        {
            ShowSectionHeader("REPL MODE - Read-Eval-Print Loop", "⚡");

            var repl = new REPL();

            ShowInfo("Mod REPL activat! Comenzi disponibile:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  :help     - Afișează ajutor");
            Console.WriteLine("  :vars     - Afișează variabile");
            Console.WriteLine("  :clear    - Șterge ecranul");
            Console.WriteLine("  :exit     - Ieșire REPL");
            Console.ResetColor();
            Console.WriteLine();

            repl.Start();

            WaitForKey();
        }

        #endregion

        #region Mode 4: Optimization Mode

        static void RunWithOptimization()
        {
            ShowSectionHeader("EXECUȚIE CU OPTIMIZĂRI", "🔧");

            Console.Write("➤ Alege sursa:\n");
            Console.WriteLine("  [1] Introdu cod manual");
            Console.WriteLine("  [2] Citește din fișier");
            Console.Write("\n➤ Opțiune: ");

            string choice = Console.ReadLine()?.Trim();
            string code = null;

            if (choice == "1")
            {
                Console.WriteLine("\n➤ Introdu codul (linie goală pentru final):\n");
                var lines = new List<string>();
                int lineNumber = 1;

                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"{lineNumber,3} | ");
                    Console.ResetColor();

                    string line = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) break;

                    lines.Add(line);
                    lineNumber++;
                }

                code = string.Join("\n", lines);
            }
            else if (choice == "2")
            {
                Console.Write("\n➤ Cale fișier: ");
                string path = Console.ReadLine()?.Trim();
                code = FileReader.ReadFile(path);
            }

            if (string.IsNullOrEmpty(code))
            {
                ShowError("Cod invalid sau lipsă!");
                WaitForKey();
                return;
            }

            Console.WriteLine();
            ExecuteWithOptimization(code);
            WaitForKey();
        }

        static void ExecuteWithOptimization(string code)
        {
            try
            {
                // Phase 1: Parsing
                ShowInfo("Faza 1: Analiză Lexicală și Sintactică...");
                var lexer = new Lexer(code);
                var tokens = lexer.Tokenize();

                if (lexer.Errors.Any())
                {
                    ShowError("Erori lexicale detectate:");
                    DisplayErrors(lexer.Errors);
                    return;
                }

                var parser = new Parser(code);
                var program = parser.ParseProgram();

                if (parser.Errors.Any())
                {
                    ShowError("Erori sintactice/semantice detectate:");
                    DisplayErrors(parser.Errors);
                    return;
                }

                ShowSuccess("Parsing complet!");
                Console.WriteLine();

                // Phase 2: Display Original AST
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.WriteLine("ARBORE SINTACTIC ORIGINAL:");
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.ResetColor();
                DisplayPrettyTree(program);
                Console.WriteLine();

                // Phase 3: Optimization
                ShowInfo("Faza 2: Aplicare Optimizări...");
                var optimizer = new CodeOptimizer();
                var optimizedProgram = optimizer.Optimize(program);

                Console.WriteLine();
                ShowSuccess("Optimizări aplicate!");
                optimizer.DisplayStatistics();
                Console.WriteLine();

                // Phase 4: Display Optimized AST
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.WriteLine("ARBORE SINTACTIC OPTIMIZAT:");
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.ResetColor();
                DisplayPrettyTree(optimizedProgram);
                Console.WriteLine();

                // Phase 5: Execution
                ShowInfo("Faza 3: Execuție Cod Optimizat...");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.WriteLine("OUTPUT:");
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.ResetColor();

                var evaluator = new Evaluator();
                evaluator.ExecuteProgram(optimizedProgram);

                Console.WriteLine();
                ShowSuccess("Execuție finalizată!");
            }
            catch (Exception ex)
            {
                ShowError($"Eroare: {ex.Message}");
            }
        }

        #endregion

        #region Mode 5: Static Analysis

        static void RunWithStaticAnalysis()
        {
            ShowSectionHeader("ANALIZĂ STATICĂ AVANSATĂ", "🔍");

            Console.Write("➤ Alege sursa:\n");
            Console.WriteLine("  [1] Introdu cod manual");
            Console.WriteLine("  [2] Citește din fișier");
            Console.Write("\n➤ Opțiune: ");

            string choice = Console.ReadLine()?.Trim();
            string code = null;

            if (choice == "1")
            {
                Console.WriteLine("\n➤ Introdu codul (linie goală pentru final):\n");
                var lines = new List<string>();
                int lineNumber = 1;

                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"{lineNumber,3} | ");
                    Console.ResetColor();

                    string line = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) break;

                    lines.Add(line);
                    lineNumber++;
                }

                code = string.Join("\n", lines);
            }
            else if (choice == "2")
            {
                Console.Write("\n➤ Cale fișier: ");
                string path = Console.ReadLine()?.Trim();
                code = FileReader.ReadFile(path);
            }

            if (string.IsNullOrEmpty(code))
            {
                ShowError("Cod invalid sau lipsă!");
                WaitForKey();
                return;
            }

            Console.WriteLine();
            PerformStaticAnalysis(code);
            WaitForKey();
        }

        static void PerformStaticAnalysis(string code)
        {
            try
            {
                // Phase 1: Parsing
                ShowInfo("Faza 1: Parsing...");
                var parser = new Parser(code);
                var program = parser.ParseProgram();

                if (parser.Errors.Any())
                {
                    ShowError("Erori de parsing:");
                    DisplayErrors(parser.Errors);
                    return;
                }

                ShowSuccess("Parsing complet!");
                Console.WriteLine();

                // Phase 2: Static Analysis
                ShowInfo("Faza 2: Analiză Statică...");
                Console.WriteLine();

                var analyzer = new StaticAnalyzer(parser.SymbolTable);
                analyzer.Analyze(program);

                // Display results
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.WriteLine("REZULTATE ANALIZĂ STATICĂ:");
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.ResetColor();
                Console.WriteLine();

                analyzer.DisplayWarnings();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.WriteLine("SUMAR:");
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.ResetColor();
                Console.WriteLine(analyzer.GetSummary());

                if (analyzer.Warnings.Count == 0)
                {
                    ShowSuccess("Niciun warning detectat! Cod clean! ✨");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Eroare: {ex.Message}");
            }
        }

        #endregion

        #region Mode 6: Automated Tests

        static void RunAutomatedTests()
        {
            ShowSectionHeader("TESTE AUTOMATIZATE", "🧪");

            ShowInfo("Rulare suite de teste...");
            Console.WriteLine();

            try
            {
                Tests.TestSuite.RunAllTests();

                Console.WriteLine();
                ShowSuccess("Toate testele au fost executate!");
            }
            catch (Exception ex)
            {
                ShowError($"Eroare la rularea testelor: {ex.Message}");
            }

            WaitForKey();
        }

        #endregion

        #region Mode 7: Examples

        static void ShowExamples()
        {
            ShowSectionHeader("EXEMPLE DE COD", "📖");

            Console.WriteLine("Alege un exemplu:\n");
            Console.WriteLine("  [1] Factorial Recursiv");
            Console.WriteLine("  [2] Fibonacci");
            Console.WriteLine("  [3] Bubble Sort");
            Console.WriteLine("  [4] Număr Prim");
            Console.WriteLine("  [5] Calculator Simple");
            Console.WriteLine("  [0] Înapoi");

            Console.Write("\n➤ Opțiune: ");
            string choice = Console.ReadLine()?.Trim();

            Console.WriteLine();

            string example = choice switch
            {
                "1" => @"// Factorial Recursiv
function factorial(int n) {
    if (n <= 1) {
        return 1;
    }
    return n * factorial(n - 1);
}

int result = factorial(5);
print(""Factorial 5 = "");
print(result);",

                "2" => @"// Fibonacci
function fibonacci(int n) {
    if (n <= 1) {
        return n;
    }
    return fibonacci(n - 1) + fibonacci(n - 2);
}

print(""Fibonacci 10 = "");
print(fibonacci(10));",

                "3" => @"// Bubble Sort Simulation
int a = 5;
int b = 2;
int c = 8;

print(""Înainte de sortare:"");
print(a);
print(b);
print(c);

// Sortare
if (a > b) {
    int temp = a;
    a = b;
    b = temp;
}

if (b > c) {
    int temp = b;
    b = c;
    c = temp;
}

if (a > b) {
    int temp = a;
    a = b;
    b = temp;
}

print(""După sortare:"");
print(a);
print(b);
print(c);",

                "4" => @"// Verificare Număr Prim
int number = 17;
int isPrime = 1;

if (number <= 1) {
    isPrime = 0;
} else {
    for (int i = 2; i < number; i++) {
        if (number % i == 0) {
            isPrime = 0;
            break;
        }
    }
}

if (isPrime == 1) {
    print(""Număr prim!"");
} else {
    print(""Nu este prim."");
}",

                "5" => @"// Calculator Simple
int a = 10;
int b = 5;

int suma = a + b;
int diferenta = a - b;
int produs = a * b;
int cat = a / b;

print(""Suma: "");
print(suma);
print(""Diferența: "");
print(diferenta);
print(""Produsul: "");
print(produs);
print(""Câtul: "");
print(cat);",

                "0" => null,
                _ => null
            };

            if (example == null)
            {
                return;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("COD EXEMPLU:");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(example);
            Console.ResetColor();
            Console.WriteLine();

            Console.Write("➤ Vrei să execuți acest exemplu? (y/n): ");
            string execute = Console.ReadLine()?.Trim().ToLower();

            if (execute == "y" || execute == "yes" || execute == "da")
            {
                Console.WriteLine();
                CompileAndExecute(example);
            }

            WaitForKey();
        }

        #endregion

        #region Mode 8: About

        static void ShowAbout()
        {
            ShowSectionHeader("DESPRE COMPILATOR", "ℹ️");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("📚 COMPILATOR LFT - Academic Project");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("FUNCȚIONALITĂȚI IMPLEMENTATE:");
            Console.ResetColor();
            Console.WriteLine();

            ShowSuccess("✓ Analiză Lexicală completă");
            ShowSuccess("✓ Analiză Sintactică (Parser recursive descent)");
            ShowSuccess("✓ Analiză Semantică (type checking, scope)");
            ShowSuccess("✓ Evaluator cu suport pentru:");
            Console.WriteLine("    • Variabile (int, double, string, bool)");
            Console.WriteLine("    • Operatori (+, -, *, /, %, ++, --, +=, -=, etc.)");
            Console.WriteLine("    • Control flow (if/else, for, while, break, continue)");
            Console.WriteLine("    • Funcții user-defined cu recursivitate");
            Console.WriteLine("    • Arrays și indexare");
            Console.WriteLine("    • 15+ funcții matematice built-in");
            ShowSuccess("✓ Code Optimizer (7+ tehnici)");
            ShowSuccess("✓ Static Analyzer (10+ detectări)");
            ShowSuccess("✓ REPL interactiv");
            ShowSuccess("✓ Suite teste automatizate");
            ShowSuccess("✓ Scope management complet");
            ShowSuccess("✓ Symbol table avansat");

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  Proiect realizat pentru: Limbaje Formale și Translatoare");
            Console.WriteLine("  Universitatea: UAIC Iași");
            Console.WriteLine("  Referințe: Dragon Book, Grigoraș \"Proiectarea Compilatoarelor\"");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.ResetColor();

            WaitForKey();
        }

        #endregion

        #region Compilation and Execution

        static void CompileAndExecute(string code)
        {
            try
            {
                // Phase 1: Lexical Analysis
                ShowInfo("Faza 1: Analiză Lexicală...");
                var lexer = new Lexer(code);
                var tokens = lexer.Tokenize();

                if (lexer.Errors.Any())
                {
                    ShowError("Erori lexicale:");
                    DisplayErrors(lexer.Errors);
                    return;
                }

                ShowSuccess($"Tokenizare completă! ({tokens.Count} tokeni)");
                Console.WriteLine();

                // Phase 2: Syntactic & Semantic Analysis
                ShowInfo("Faza 2: Analiză Sintactică & Semantică...");
                var parser = new Parser(code);
                var program = parser.ParseProgram();

                if (parser.Errors.Any())
                {
                    ShowError("Erori de parsing:");
                    DisplayErrors(parser.Errors);
                    return;
                }

                ShowSuccess($"Parsing complet! ({program.Statements.Count} instrucțiuni, {program.Functions.Count} funcții)");
                Console.WriteLine();

                // Phase 3: Display AST
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.WriteLine("ARBORE SINTACTIC:");
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.ResetColor();
                DisplayPrettyTree(program);
                Console.WriteLine();

                // Phase 4: Evaluation
                ShowInfo("Faza 3: Evaluare și Execuție...");
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.WriteLine("OUTPUT:");
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.ResetColor();

                var evaluator = new Evaluator();
                evaluator.ExecuteProgram(program);

                Console.WriteLine();
                ShowSuccess("Execuție finalizată!");
            }
            catch (Exception ex)
            {
                ShowError($"Eroare: {ex.Message}");
            }
        }

        static void DisplayErrors(IEnumerable<CompilationError> errors)
        {
            foreach (var error in errors)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"  [{error.Line},{error.Column}] ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{error.Type}: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(error.Message);
                Console.ResetColor();
            }
        }

        #endregion

        #region Pretty Tree Display (Like Colleague's Project)

        static void DisplayPrettyTree(ProgramNode program)
        {
            // Display functions
            if (program.Functions.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("├── Funcții:");
                Console.ResetColor();

                var lastFunction = program.Functions.Last();
                foreach (var function in program.Functions)
                {
                    bool isLast = function == lastFunction;
                    string prefix = isLast ? "    └──" : "    ├──";
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"{prefix} {function.Name.Text}");
                    Console.ResetColor();
                }
            }

            // Display statements
            if (program.Statements.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("└── Instrucțiuni:");
                Console.ResetColor();

                var lastStmt = program.Statements.Last();
                foreach (var stmt in program.Statements)
                {
                    bool isLast = stmt == lastStmt;
                    DisplayNode(stmt, "    ", isLast);
                }
            }
        }

        static void DisplayNode(SyntaxNode node, string indent, bool isLast)
        {
            if (node == null) return;

            string prefix = isLast ? "└──" : "├──";
            string childIndent = indent + (isLast ? "    " : "│   ");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(indent + prefix);
            Console.ForegroundColor = ConsoleColor.White;

            // Display node type and value
            Console.Write($" {node.Type}");

            if (node is Token token && token.Value != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($" = {token.Value}");
            }

            Console.WriteLine();
            Console.ResetColor();

            // Display children
            var children = node.GetChildren().ToList();
            if (children.Any())
            {
                var lastChild = children.Last();
                foreach (var child in children)
                {
                    DisplayNode(child, childIndent, child == lastChild);
                }
            }
        }

        #endregion

        #region Exit

        static void ExitProgram()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                   ║");
            Console.WriteLine("  ║         Mulțumim pentru utilizare! 👋            ║");
            Console.WriteLine("  ║                                                   ║");
            Console.WriteLine("  ║          🚀 COMPILATOR LFT - v1.0 🚀             ║");
            Console.WriteLine("  ║                                                   ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.ResetColor();
        }

        #endregion
    }
}
