using System;
using System.Collections.Generic;
using System.Linq;
using CompilatorLFT.Core;
using CompilatorLFT.Models;
using CompilatorLFT.Models.Statements;
using CompilatorLFT.Utils;

namespace CompilatorLFT
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            DisplayHeader();

            if (args.Length > 0)
            {
                RunFromFile(args[0]);
                return;
            }

            while (true)
            {
                DisplayMenu();
                Console.Write("\n  Alegere: ");
                string choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        RunInteractive();
                        break;
                    case "2":
                        RunFromFileInteractive();
                        break;
                    case "3":
                        RunAutomatedTests();
                        break;
                    case "4":
                        DisplayExamples();
                        break;
                    case "5":
                        DisplayAbout();
                        break;
                    case "6":
                    case "q":
                    case "Q":
                        DisplayExit();
                        return;
                    default:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\n  Optiune invalida! Alege 1-6.");
                        Console.ResetColor();
                        break;
                }

                Console.WriteLine("\n  Apasa ENTER pentru a continua...");
                Console.ReadLine();
                Console.Clear();
            }
        }

        #region UI Display

        static void DisplayHeader()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
   ╔═══════════════════════════════════════════════════════════════╗
   ║                                                               ║
   ║     ██████╗ ██████╗ ███╗   ███╗██████╗ ██╗██╗      █████╗     ║
   ║    ██╔════╝██╔═══██╗████╗ ████║██╔══██╗██║██║     ██╔══██╗    ║
   ║    ██║     ██║   ██║██╔████╔██║██████╔╝██║██║     ███████║    ║
   ║    ██║     ██║   ██║██║╚██╔╝██║██╔═══╝ ██║██║     ██╔══██║    ║
   ║    ╚██████╗╚██████╔╝██║ ╚═╝ ██║██║     ██║███████╗██║  ██║    ║
   ║     ╚═════╝ ╚═════╝ ╚═╝     ╚═╝╚═╝     ╚═╝╚══════╝╚═╝  ╚═╝    ║
   ║                                                               ║
   ║              COMPILATOR LFT - Proiect Academic                ║
   ║         Limbaje Formale si Translatoare - UAIC Iasi           ║
   ║                                                               ║
   ╚═══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void DisplayMenu()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(@"
   ╔═══════════════════════════════════════════════════════════════╗
   ║                        MENIU PRINCIPAL                        ║
   ╠═══════════════════════════════════════════════════════════════╣");
            Console.ResetColor();

            Console.Write("   ║  ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("[1]");
            Console.ResetColor();
            Console.WriteLine(" Mod Interactiv (Scrie cod linie cu linie)            ║");

            Console.Write("   ║  ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("[2]");
            Console.ResetColor();
            Console.WriteLine(" Citire din Fisier                                    ║");

            Console.Write("   ║  ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("[3]");
            Console.ResetColor();
            Console.WriteLine(" Teste Automatizate                                   ║");

            Console.Write("   ║  ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("[4]");
            Console.ResetColor();
            Console.WriteLine(" Exemple de Cod                                       ║");

            Console.Write("   ║  ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("[5]");
            Console.ResetColor();
            Console.WriteLine(" Despre Compilator                                    ║");

            Console.Write("   ║  ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("[6]");
            Console.ResetColor();
            Console.WriteLine(" Iesire                                               ║");

            Console.WriteLine("   ╚═══════════════════════════════════════════════════════════════╝");
        }

        static void DisplayExit()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
   ╔═══════════════════════════════════════════════════════════════╗
   ║                                                               ║
   ║                    La revedere!                               ║
   ║                                                               ║
   ║              Multumim ca ai folosit                           ║
   ║              Compilatorul LFT!                                ║
   ║                                                               ║
   ╚═══════════════════════════════════════════════════════════════╝
");
            Console.ResetColor();
        }

        static void DisplayAbout()
        {
            Console.WriteLine("\n" + new string('═', 65));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("                    DESPRE COMPILATOR");
            Console.ResetColor();
            Console.WriteLine(new string('═', 65));

            Console.WriteLine(@"
   Compilatorul LFT - Limbaje Formale si Translatoare
   ────────────────────────────────────────────────────

   FUNCTIONALITATI:
   ├── Analiza Lexicala (Tokenizare)
   ├── Analiza Sintactica (Parser)
   ├── Analiza Semantica
   ├── Arbore Sintactic (AST)
   ├── Evaluator de Expresii
   └── Tabel de Simboluri

   TIPURI DE DATE SUPORTATE:
   ├── int      - numere intregi
   ├── double   - numere reale
   ├── string   - siruri de caractere
   └── bool     - valori booleene

   STRUCTURI DE CONTROL:
   ├── if / else
   ├── while
   ├── for
   ├── break / continue
   └── functii definite de utilizator

   OPERATORI:
   ├── Aritmetici:  + - * / %
   ├── Relationali: < > <= >= == !=
   ├── Logici:      && || !
   ├── Atribuire:   = += -= *= /= %=
   └── Increment:   ++ --

   FUNCTII BUILT-IN:
   ├── print()  - afisare
   ├── sqrt()   - radical
   └── abs()    - modul
");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("   Proiect realizat pentru cursul LFT - UAIC Iasi");
            Console.ResetColor();
        }

        #endregion

        #region Run Modes

        static void RunInteractive()
        {
            Console.WriteLine("\n" + new string('═', 65));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("                    MOD INTERACTIV");
            Console.ResetColor();
            Console.WriteLine(new string('═', 65));

            Console.WriteLine("\n  Scrie codul sursa (linie goala pentru a termina):");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Exemplu: int a = 5; print(a + 10);");
            Console.ResetColor();
            Console.WriteLine();

            var lines = new List<string>();
            int lineNumber = 1;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"  {lineNumber,3} │ ");
                Console.ResetColor();

                string line = Console.ReadLine();

                if (string.IsNullOrEmpty(line))
                    break;

                lines.Add(line);
                lineNumber++;
            }

            if (lines.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n  Nu ai introdus niciun cod!");
                Console.ResetColor();
                return;
            }

            string content = string.Join("\n", lines);
            CompileAndRun(content);
        }

        static void RunFromFileInteractive()
        {
            Console.WriteLine("\n" + new string('═', 65));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("                    CITIRE DIN FISIER");
            Console.ResetColor();
            Console.WriteLine(new string('═', 65));

            Console.Write("\n  Calea catre fisier: ");
            string path = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(path))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  Calea nu poate fi goala!");
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

            Console.WriteLine("\n  ┌─── COD SURSA ───");
            var lines = content.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"  │ {i + 1,3} │ ");
                Console.ResetColor();
                Console.WriteLine(lines[i].TrimEnd('\r'));
            }
            Console.WriteLine("  └───────────────────");

            CompileAndRun(content);
        }

        #endregion

        #region Compilation and Execution

        static void CompileAndRun(string content)
        {
            // FAZA 1: ANALIZA LEXICALA
            Console.WriteLine("\n  ┌─── FAZA 1: ANALIZA LEXICALA ───");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  │");
            Console.ResetColor();

            var lexer = new Lexer(content);
            var tokens = lexer.Tokenize();

            Console.Write("  │ Tokeni generati: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(tokens.Count);
            Console.ResetColor();

            if (lexer.Errors.Any())
            {
                DisplayErrors("ERORI LEXICALE", lexer.Errors);
                Console.WriteLine("  └───────────────────");
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  │ ✓ Analiza lexicala reusita!");
            Console.ResetColor();
            Console.WriteLine("  └───────────────────");

            // Afisare tokeni (primii 15)
            Console.WriteLine("\n  ┌─── TOKENI ───");
            foreach (var token in tokens.Take(15))
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("  │ ");
                Console.ResetColor();
                Console.WriteLine(token);
            }
            if (tokens.Count > 15)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"  │ ... si inca {tokens.Count - 15} tokeni");
                Console.ResetColor();
            }
            Console.WriteLine("  └───────────────────");

            // FAZA 2: ANALIZA SINTACTICA SI SEMANTICA
            Console.WriteLine("\n  ┌─── FAZA 2: ANALIZA SINTACTICA & SEMANTICA ───");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  │");
            Console.ResetColor();

            var parser = new Parser(content);
            var program = parser.ParseProgram();

            Console.Write("  │ Instructiuni parsate: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(program.Statements.Count);
            Console.ResetColor();

            Console.Write("  │ Functii declarate: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(program.Functions.Count);
            Console.ResetColor();

            Console.Write("  │ Variabile declarate: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(parser.SymbolTable.VariableCount);
            Console.ResetColor();

            if (parser.Errors.Any())
            {
                DisplayErrors("ERORI SINTACTICE/SEMANTICE", parser.Errors);
                Console.WriteLine("  └───────────────────");
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  │ ✓ Analiza sintactica reusita!");
            Console.WriteLine("  │ ✓ Analiza semantica reusita!");
            Console.ResetColor();
            Console.WriteLine("  └───────────────────");

            // ARBORE SINTACTIC
            Console.WriteLine("\n  ┌─── ARBORE SINTACTIC (AST) ───");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  │");
            Console.ResetColor();
            DisplayTreeWithPrefix(program);
            Console.WriteLine("  └───────────────────");

            // FAZA 3: EVALUARE SI EXECUTIE
            Console.WriteLine("\n  ┌─── FAZA 3: EVALUARE & EXECUTIE ───");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  │");
            Console.ResetColor();

            var functions = new Dictionary<string, FunctionDeclaration>(parser.Functions);
            var evaluator = new Evaluator(parser.SymbolTable, functions);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  │ OUTPUT:");
            Console.ResetColor();

            evaluator.ExecuteProgram(program);

            if (evaluator.Errors.Any())
            {
                DisplayErrors("ERORI LA RUNTIME", evaluator.Errors);
                Console.WriteLine("  └───────────────────");
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  │");
            Console.WriteLine("  │ ✓ Executie reusita!");
            Console.ResetColor();
            Console.WriteLine("  └───────────────────");

            // TABEL DE SIMBOLURI FINAL
            Console.WriteLine("\n  ┌─── TABEL DE SIMBOLURI ───");
            DisplaySymbolTable(parser.SymbolTable);
            Console.WriteLine("  └───────────────────");
        }

        static void DisplayTreeWithPrefix(CompilatorLFT.Models.Statements.ProgramNode program)
        {
            Console.Write("  │ ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Program");
            Console.ResetColor();

            int total = program.Functions.Count + program.Statements.Count;
            int current = 0;

            foreach (var func in program.Functions)
            {
                current++;
                string prefix = current == total ? "└── " : "├── ";
                Console.Write($"  │   {prefix}");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Functie: {func.Key}");
                Console.ResetColor();
            }

            foreach (var stmt in program.Statements)
            {
                current++;
                string prefix = current == total ? "└── " : "├── ";
                Console.Write($"  │   {prefix}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(stmt.GetType().Name.Replace("Statement", ""));
                Console.ResetColor();
            }
        }

        static void DisplaySymbolTable(TabelSimboluri table)
        {
            var vars = table.GetAllVariables();
            if (!vars.Any())
            {
                Console.WriteLine("  │ (gol)");
                return;
            }

            Console.WriteLine("  │ ┌────────────────┬──────────┬────────────────┐");
            Console.WriteLine("  │ │     Nume       │   Tip    │    Valoare     │");
            Console.WriteLine("  │ ├────────────────┼──────────┼────────────────┤");

            foreach (var v in vars)
            {
                string name = v.Key.Length > 14 ? v.Key.Substring(0, 11) + "..." : v.Key;
                string type = v.Value.Type ?? "?";
                string value = v.Value.Value?.ToString() ?? "null";
                if (value.Length > 14) value = value.Substring(0, 11) + "...";

                Console.WriteLine($"  │ │ {name,-14} │ {type,-8} │ {value,-14} │");
            }

            Console.WriteLine("  │ └────────────────┴──────────┴────────────────┘");
        }

        static void DisplayErrors(string title, IEnumerable<CompilationError> errors)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  │ X {title}:");
            foreach (var error in errors)
            {
                Console.WriteLine($"  │   • {error}");
            }
            Console.ResetColor();
        }

        #endregion

        #region Automated Tests

        static void RunAutomatedTests()
        {
            Console.WriteLine("\n" + new string('═', 65));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("                    TESTE AUTOMATIZATE");
            Console.ResetColor();
            Console.WriteLine(new string('═', 65));

            var tests = new List<(string name, string code, bool expectSuccess)>
            {
                // Teste de baza
                ("Declaratie simpla", "int a;", true),
                ("Declaratie cu initializare", "int a = 5;", true),
                ("Declaratii multiple", "int a, b=3, c;", true),
                ("Expresie aritmetica", "3 + 5;", true),
                ("Precedenta operatorilor", "3 + 4 * 5;", true),
                ("Paranteze", "(3 + 4) * 5;", true),
                ("Minus unar", "int a = -5;", true),
                ("Literal double", "double x = 3.14;", true),
                ("Literal string", "string s = \"hello\";", true),
                ("Concatenare string", "string a = \"hello\"; string b = \" world\"; string c = a + b;", true),

                // Atribuiri si operatii
                ("Atribuire si calcul", "int a = 5; int b = 3; int c = a + b;", true),
                ("Operatori relationali", "int a = 5; int b = 3; a > b;", true),

                // Structuri de control
                ("If simplu", "int a = 5; int b = 0; if (a > 3) { b = 10; }", true),
                ("If-else", "int a = 2; int b = 0; if (a > 3) { b = 10; } else { b = 20; }", true),
                ("While", "int i = 0; int sum = 0; while (i < 5) { sum = sum + i; i = i + 1; }", true),
                ("For", "int sum = 0; for (int i = 0; i < 5; i = i + 1) { sum = sum + i; }", true),

                // Print
                ("Print cu paranteze", "print(5 + 3);", true),
                ("Print fara paranteze", "print 42;", true),
                ("Print string", "print(\"Hello World\");", true),

                // Operatori logici
                ("AND logic", "int a = 5; bool b = (a > 3) && (a < 10);", true),
                ("OR logic", "int a = 5; bool b = (a > 10) || (a > 3);", true),
                ("NOT logic", "bool a = true; bool b = !a;", true),

                // Tipul bool
                ("Declaratie bool", "bool a = true;", true),
                ("Expresie bool", "bool a = 5 > 3;", true),

                // Comentarii
                ("Comentariu single-line", "int a = 5; // comentariu\nint b = 3;", true),
                ("Comentariu multi-line", "int a = 5; /* multi\nline */ int b = 3;", true),

                // Increment/Decrement
                ("Postfix increment", "int a = 5; a++;", true),
                ("Prefix increment", "int a = 5; ++a;", true),
                ("Postfix decrement", "int a = 5; a--;", true),
                ("For cu i++", "int sum = 0; for (int i = 0; i < 5; i++) { sum = sum + i; }", true),

                // Atribuire compusa
                ("Plus egal", "int a = 5; a += 3;", true),
                ("Minus egal", "int a = 5; a -= 3;", true),
                ("Ori egal", "int a = 5; a *= 3;", true),
                ("Impartit egal", "int a = 10; a /= 2;", true),
                ("Modulo egal", "int a = 10; a %= 3;", true),

                // Functii
                ("Functie si apel", "function add(int a, int b) { return a + b; } int result = add(5, 3);", true),
                ("Functie cu tip return", "int add(int a, int b) { return a + b; } int result = add(5, 3);", true),

                // Functii built-in
                ("Functia sqrt", "double x = sqrt(16);", true),
                ("Functia abs", "int x = abs(-5);", true),

                // Break si continue
                ("Break in bucla", "int i = 0; while (i < 10) { if (i == 5) { break; } i++; }", true),
                ("Continue in bucla", "int sum = 0; for (int i = 0; i < 10; i++) { if (i % 2 == 0) { continue; } sum += i; }", true),

                // Teste de erori
                ("Eroare: variabila nedeclarata", "x = 5;", false),
                ("Eroare: declaratie duplicata", "int a; int a;", false),
                ("Eroare: plus unar", "int a = +5;", false),
            };

            int passed = 0;
            int total = tests.Count;

            Console.WriteLine();
            foreach (var (name, code, expectSuccess) in tests)
            {
                bool success = RunTest(name, code, expectSuccess);
                if (success) passed++;
            }

            // Raport final
            Console.WriteLine("\n" + new string('─', 65));
            double percent = (double)passed / total * 100;

            if (passed == total)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n  ✓ TOATE TESTELE AU TRECUT! ({passed}/{total} - {percent:F0}%)");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n  Teste trecute: {passed}/{total} ({percent:F0}%)");
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
                    var funcs = new Dictionary<string, FunctionDeclaration>(parser.Functions);
                    var evaluator = new Evaluator(parser.SymbolTable, funcs);
                    evaluator.ExecuteProgram(program);

                    bool hasEvalErrors = evaluator.Errors.Any();

                    if (expectSuccess && !hasEvalErrors)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  ✓ PASS: {name}");
                        Console.ResetColor();
                        return true;
                    }
                    else if (!expectSuccess && hasEvalErrors)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  ✓ PASS: {name} (eroare detectata corect)");
                        Console.ResetColor();
                        return true;
                    }
                }
                else
                {
                    if (!expectSuccess)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  ✓ PASS: {name} (eroare detectata corect)");
                        Console.ResetColor();
                        return true;
                    }
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  X FAIL: {name}");
                Console.ResetColor();
                return false;
            }
            catch (Exception ex)
            {
                if (!expectSuccess)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ✓ PASS: {name} (exceptie: {ex.Message})");
                    Console.ResetColor();
                    return true;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  X FAIL: {name} - Exceptie: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        #endregion

        #region Examples

        static void DisplayExamples()
        {
            Console.WriteLine("\n" + new string('═', 65));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("                    EXEMPLE DE COD");
            Console.ResetColor();
            Console.WriteLine(new string('═', 65));

            var examples = new (string title, string code)[]
            {
                ("1. Factorial (recursiv)", @"
function factorial(int n) {
    if (n <= 1) {
        return 1;
    }
    return n * factorial(n - 1);
}

int result = factorial(5);
print(""Factorial de 5 = "");
print(result);  // 120"),

                ("2. Fibonacci", @"
function fib(int n) {
    if (n <= 1) {
        return n;
    }
    return fib(n - 1) + fib(n - 2);
}

print(""Fibonacci(10) = "");
print(fib(10));  // 55"),

                ("3. Bubble Sort", @"
int n = 5;
int a0 = 64; int a1 = 34; int a2 = 25;
int a3 = 12; int a4 = 22;

for (int i = 0; i < n - 1; i++) {
    for (int j = 0; j < n - i - 1; j++) {
        // Swap if needed
        int temp;
        if (j == 0 && a0 > a1) { temp = a0; a0 = a1; a1 = temp; }
        if (j == 1 && a1 > a2) { temp = a1; a1 = a2; a2 = temp; }
        if (j == 2 && a2 > a3) { temp = a2; a2 = a3; a3 = temp; }
        if (j == 3 && a3 > a4) { temp = a3; a3 = a4; a4 = temp; }
    }
}

print(a0); print(a1); print(a2); print(a3); print(a4);"),

                ("4. Numar Prim", @"
function isPrime(int n) {
    if (n < 2) { return 0; }
    for (int i = 2; i * i <= n; i++) {
        if (n % i == 0) {
            return 0;
        }
    }
    return 1;
}

int num = 17;
if (isPrime(num)) {
    print(num);
    print("" este prim"");
} else {
    print(num);
    print("" nu este prim"");
}"),

                ("5. Calculator Simplu", @"
function add(int a, int b) { return a + b; }
function sub(int a, int b) { return a - b; }
function mul(int a, int b) { return a * b; }
function div(int a, int b) { return a / b; }

int x = 10;
int y = 3;

print(""10 + 3 = ""); print(add(x, y));
print(""10 - 3 = ""); print(sub(x, y));
print(""10 * 3 = ""); print(mul(x, y));
print(""10 / 3 = ""); print(div(x, y));")
            };

            Console.WriteLine("\n  Selecteaza un exemplu pentru a-l rula:");
            Console.WriteLine("  ─────────────────────────────────────────");

            for (int i = 0; i < examples.Length; i++)
            {
                Console.Write("  ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"[{i + 1}]");
                Console.ResetColor();
                Console.WriteLine($" {examples[i].title}");
            }

            Console.Write("\n  Alegere (1-5 sau 0 pentru a vedea toate): ");
            string choice = Console.ReadLine()?.Trim();

            if (choice == "0")
            {
                foreach (var (title, code) in examples)
                {
                    Console.WriteLine("\n  ┌─── " + title + " ───");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(code);
                    Console.ResetColor();
                    Console.WriteLine("  └───────────────────────────────");
                }
            }
            else if (int.TryParse(choice, out int idx) && idx >= 1 && idx <= examples.Length)
            {
                var (title, code) = examples[idx - 1];
                Console.WriteLine($"\n  ┌─── {title} ───");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(code);
                Console.ResetColor();
                Console.WriteLine("  └───────────────────────────────");

                Console.Write("\n  Rulezi acest exemplu? (d/n): ");
                if (Console.ReadLine()?.Trim().ToLower() == "d")
                {
                    CompileAndRun(code.Trim());
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  Alegere invalida!");
                Console.ResetColor();
            }
        }

        #endregion
    }
}
