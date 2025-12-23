using System;
using System.Collections.Generic;
using System.Linq;
using CompilatorLFT.Core;
using CompilatorLFT.Models;
using CompilatorLFT.Utils;

namespace CompilatorLFT.Tests
{
    /// <summary>
    /// Automated test suite for the compiler.
    /// Contains 25+ tests for all components.
    /// </summary>
    public static class TestSuite
    {
        private static int _passedTests;
        private static int _totalTests;

        #region Entry Point

        /// <summary>
        /// Runs all tests and displays the report.
        /// </summary>
        public static void RunAllTests()
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║             AUTOMATED TEST SUITE - LFT COMPILER            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

            _passedTests = 0;
            _totalTests = 0;

            // Lexer Tests
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== LEXER TESTS ===");
            Console.ResetColor();
            RunLexerTests();

            // Parser Tests
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== PARSER TESTS ===");
            Console.ResetColor();
            RunParserTests();

            // Evaluator Tests
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== EVALUATOR TESTS ===");
            Console.ResetColor();
            RunEvaluatorTests();

            // Integration Tests
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== INTEGRATION TESTS ===");
            Console.ResetColor();
            RunIntegrationTests();

            // Error Detection Tests
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== ERROR DETECTION TESTS ===");
            Console.ResetColor();
            RunErrorDetectionTests();

            // Final Report
            DisplayFinalReport();
        }

        #endregion

        #region Lexer Tests

        static void RunLexerTests()
        {
            // Test 1: Simple declaration
            Test("Lexer: Simple declaration", () =>
            {
                var lexer = new Lexer("int a;");
                var tokens = lexer.Tokenize();

                return tokens.Count == 4 && // int, a, ;, EOF
                       tokens[0].Type == TokenType.KeywordInt &&
                       tokens[1].Type == TokenType.Identifier &&
                       tokens[2].Type == TokenType.Semicolon &&
                       lexer.Errors.Count == 0;
            });

            // Test 2: Integers and decimals
            Test("Lexer: Integers and decimals", () =>
            {
                var lexer = new Lexer("42 3.14");
                var tokens = lexer.Tokenize();

                return tokens.Count == 3 &&
                       tokens[0].Type == TokenType.IntegerNumber &&
                       (int)tokens[0].Value == 42 &&
                       tokens[1].Type == TokenType.DecimalNumber &&
                       Math.Abs((double)tokens[1].Value - 3.14) < 0.001;
            });

            // Test 3: String literal
            Test("Lexer: String literal", () =>
            {
                var lexer = new Lexer("\"hello world\"");
                var tokens = lexer.Tokenize();

                return tokens.Count == 2 &&
                       tokens[0].Type == TokenType.StringLiteral &&
                       (string)tokens[0].Value == "hello world";
            });

            // Test 4: Relational operators
            Test("Lexer: Relational operators", () =>
            {
                var lexer = new Lexer("<= >= == !=");
                var tokens = lexer.Tokenize();

                return tokens[0].Type == TokenType.LessThanOrEqual &&
                       tokens[1].Type == TokenType.GreaterThanOrEqual &&
                       tokens[2].Type == TokenType.EqualEqual &&
                       tokens[3].Type == TokenType.NotEqual;
            });

            // Test 5: Keywords
            Test("Lexer: Keywords", () =>
            {
                var lexer = new Lexer("int double string for while if else");
                var tokens = lexer.Tokenize();

                return tokens[0].Type == TokenType.KeywordInt &&
                       tokens[1].Type == TokenType.KeywordDouble &&
                       tokens[2].Type == TokenType.KeywordString &&
                       tokens[3].Type == TokenType.KeywordFor &&
                       tokens[4].Type == TokenType.KeywordWhile &&
                       tokens[5].Type == TokenType.KeywordIf &&
                       tokens[6].Type == TokenType.KeywordElse;
            });

            // Test 6: Line/column tracking
            Test("Lexer: Line/column tracking", () =>
            {
                var lexer = new Lexer("int a;\na = 5;");
                var tokens = lexer.Tokenize();

                // 'a' on line 2 should have Line == 2
                var aLine2 = tokens.FirstOrDefault(t =>
                    t.Type == TokenType.Identifier && t.Line == 2);

                return aLine2 != null && aLine2.Column == 1;
            });
        }

        #endregion

        #region Parser Tests

        static void RunParserTests()
        {
            // Test 7: Declaration with initialization
            Test("Parser: Declaration with initialization", () =>
            {
                var parser = new Parser("int a = 5;");
                var program = parser.ParseProgram();

                return program.Statements.Count == 1 &&
                       parser.Errors.Count == 0 &&
                       parser.SymbolTable.Exists("a");
            });

            // Test 8: Multiple declarations
            Test("Parser: Multiple declarations", () =>
            {
                var parser = new Parser("int a, b = 3, c;");
                var program = parser.ParseProgram();

                return parser.SymbolTable.Exists("a") &&
                       parser.SymbolTable.Exists("b") &&
                       parser.SymbolTable.Exists("c") &&
                       parser.Errors.Count == 0;
            });

            // Test 9: Operator precedence
            Test("Parser: Operator precedence", () =>
            {
                var parser = new Parser("3 + 4 * 5;");
                var program = parser.ParseProgram();

                // AST should have * as child of +
                return program.Statements.Count == 1 &&
                       parser.Errors.Count == 0;
            });

            // Test 10: Parentheses
            Test("Parser: Parentheses change precedence", () =>
            {
                var parser = new Parser("(3 + 4) * 5;");
                var program = parser.ParseProgram();

                return program.Statements.Count == 1 &&
                       parser.Errors.Count == 0;
            });

            // Test 11: FOR structure
            Test("Parser: FOR structure", () =>
            {
                var parser = new Parser("for (int i = 0; i < 10; i = i + 1) { }");
                var program = parser.ParseProgram();

                return program.Statements.Count == 1 &&
                       parser.SymbolTable.Exists("i") &&
                       parser.Errors.Count == 0;
            });

            // Test 12: WHILE structure
            Test("Parser: WHILE structure", () =>
            {
                var parser = new Parser("int x = 5; while (x > 0) { x = x - 1; }");
                var program = parser.ParseProgram();

                return program.Statements.Count == 2 &&
                       parser.Errors.Count == 0;
            });

            // Test 13: IF-ELSE structure
            Test("Parser: IF-ELSE structure", () =>
            {
                var parser = new Parser("int a = 5; if (a > 3) { a = 10; } else { a = 0; }");
                var program = parser.ParseProgram();

                return program.Statements.Count == 2 &&
                       parser.Errors.Count == 0;
            });
        }

        #endregion

        #region Evaluator Tests

        static void RunEvaluatorTests()
        {
            // Test 14: Simple expression evaluation
            Test("Evaluator: Simple expression", () =>
            {
                var parser = new Parser("int a = 5; int b = a + 3;");
                var program = parser.ParseProgram();
                var evaluator = new Evaluator();
                evaluator.ExecuteProgram(program);

                var errors = new List<CompilationError>();
                var b = evaluator.ScopeManager.LookupVariable("b", 0, 0, errors);
                return b != null && b.IsInitialized && (int)b.Value == 8;
            });

            // Test 15: Operator precedence in evaluation
            Test("Evaluator: Operator precedence", () =>
            {
                var parser = new Parser("int a = 3 + 4 * 5;");
                var program = parser.ParseProgram();
                var evaluator = new Evaluator();
                evaluator.ExecuteProgram(program);

                var errors = new List<CompilationError>();
                var a = evaluator.ScopeManager.LookupVariable("a", 0, 0, errors);
                return a != null && (int)a.Value == 23; // 3 + 20 = 23
            });

            // Test 16: Int -> double conversion
            Test("Evaluator: Int -> double conversion", () =>
            {
                var parser = new Parser("int a = 5; double b = 2.5; double c = a + b;");
                var program = parser.ParseProgram();
                var evaluator = new Evaluator();
                evaluator.ExecuteProgram(program);

                var errors = new List<CompilationError>();
                var c = evaluator.ScopeManager.LookupVariable("c", 0, 0, errors);
                return c != null && Math.Abs((double)c.Value - 7.5) < 0.001;
            });

            // Test 17: Double -> int truncation
            Test("Evaluator: Double -> int truncation", () =>
            {
                var parser = new Parser("double x = 7.8; int a = x;");
                var program = parser.ParseProgram();
                var evaluator = new Evaluator();
                evaluator.ExecuteProgram(program);

                var errors = new List<CompilationError>();
                var a = evaluator.ScopeManager.LookupVariable("a", 0, 0, errors);
                return a != null && (int)a.Value == 7;
            });

            // Test 18: String concatenation
            Test("Evaluator: String concatenation", () =>
            {
                var parser = new Parser("string s1 = \"hello\"; string s2 = \" world\"; string s3 = s1 + s2;");
                var program = parser.ParseProgram();
                var evaluator = new Evaluator();
                evaluator.ExecuteProgram(program);

                var errors = new List<CompilationError>();
                var s3 = evaluator.ScopeManager.LookupVariable("s3", 0, 0, errors);
                return s3 != null && (string)s3.Value == "hello world";
            });

            // Test 19: Unary minus
            Test("Evaluator: Unary minus", () =>
            {
                var parser = new Parser("int a = -5; int b = -a;");
                var program = parser.ParseProgram();
                var evaluator = new Evaluator();
                evaluator.ExecuteProgram(program);

                var errors = new List<CompilationError>();
                var a = evaluator.ScopeManager.LookupVariable("a", 0, 0, errors);
                var b = evaluator.ScopeManager.LookupVariable("b", 0, 0, errors);
                return (int)a.Value == -5 && (int)b.Value == 5;
            });
        }

        #endregion

        #region Integration Tests

        static void RunIntegrationTests()
        {
            // Test 20: Simple FOR
            Test("Integration: FOR with sum", () =>
            {
                var parser = new Parser("int sum = 0; for (int i = 0; i < 5; i = i + 1) { sum = sum + i; }");
                var program = parser.ParseProgram();
                var evaluator = new Evaluator();
                evaluator.ExecuteProgram(program);

                var errors = new List<CompilationError>();
                var sum = evaluator.ScopeManager.LookupVariable("sum", 0, 0, errors);
                return sum != null && (int)sum.Value == 10; // 0+1+2+3+4
            });

            // Test 21: WHILE
            Test("Integration: WHILE with counter", () =>
            {
                var parser = new Parser("int i = 0; int sum = 0; while (i < 5) { sum = sum + i; i = i + 1; }");
                var program = parser.ParseProgram();
                var evaluator = new Evaluator();
                evaluator.ExecuteProgram(program);

                var errors = new List<CompilationError>();
                var sum = evaluator.ScopeManager.LookupVariable("sum", 0, 0, errors);
                var i = evaluator.ScopeManager.LookupVariable("i", 0, 0, errors);
                return (int)sum.Value == 10 && (int)i.Value == 5;
            });

            // Test 22: IF with true condition
            Test("Integration: IF true condition", () =>
            {
                var parser = new Parser("int a = 5; int b = 0; if (a > 3) { b = 10; }");
                var program = parser.ParseProgram();
                var evaluator = new Evaluator();
                evaluator.ExecuteProgram(program);

                var errors = new List<CompilationError>();
                var b = evaluator.ScopeManager.LookupVariable("b", 0, 0, errors);
                return (int)b.Value == 10;
            });

            // Test 23: IF-ELSE with false condition
            Test("Integration: IF-ELSE false condition", () =>
            {
                var parser = new Parser("int a = 2; int b = 0; if (a > 3) { b = 10; } else { b = 20; }");
                var program = parser.ParseProgram();
                var evaluator = new Evaluator();
                evaluator.ExecuteProgram(program);

                var errors = new List<CompilationError>();
                var b = evaluator.ScopeManager.LookupVariable("b", 0, 0, errors);
                return (int)b.Value == 20;
            });

            // Test 24: Relational operators
            Test("Integration: Relational operators", () =>
            {
                var parser = new Parser("int a = 5; int b = 3; int r = 0; if (a >= b) { r = 1; }");
                var program = parser.ParseProgram();
                var evaluator = new Evaluator();
                evaluator.ExecuteProgram(program);

                var errors = new List<CompilationError>();
                var r = evaluator.ScopeManager.LookupVariable("r", 0, 0, errors);
                return (int)r.Value == 1;
            });

            // Test 25: Recursive function (factorial)
            Test("Integration: Recursive factorial", () =>
            {
                var code = @"
                    function factorial(int n) {
                        if (n <= 1) {
                            return 1;
                        }
                        return n * factorial(n - 1);
                    }
                    int result = factorial(5);
                ";
                var parser = new Parser(code);
                var program = parser.ParseProgram();

                // Should parse without errors (recursion is allowed)
                if (parser.Errors.Any(e => e.Message.Contains("factorial") && e.Message.Contains("not defined")))
                    return false;

                var evaluator = new Evaluator();
                evaluator.ExecuteProgram(program);

                var errors = new List<CompilationError>();
                var result = evaluator.ScopeManager.LookupVariable("result", 0, 0, errors);
                return result != null && result.IsInitialized && (int)result.Value == 120; // 5! = 120
            });

            // Test 26: Double recursive function (fibonacci)
            Test("Integration: Recursive fibonacci", () =>
            {
                var code = @"
                    function fib(int n) {
                        if (n <= 1) {
                            return n;
                        }
                        return fib(n - 1) + fib(n - 2);
                    }
                    int result = fib(6);
                ";
                var parser = new Parser(code);
                var program = parser.ParseProgram();

                // Should parse without errors (double recursion allowed)
                if (parser.Errors.Any(e => e.Message.Contains("fib") && e.Message.Contains("not defined")))
                    return false;

                var evaluator = new Evaluator();
                evaluator.ExecuteProgram(program);

                var errors = new List<CompilationError>();
                var result = evaluator.ScopeManager.LookupVariable("result", 0, 0, errors);
                return result != null && result.IsInitialized && (int)result.Value == 8; // fib(6) = 8
            });

            // Test 27: Countdown recursion (sum)
            Test("Integration: Recursive countdown sum", () =>
            {
                var code = @"
                    function countdown(int n) {
                        if (n <= 0) {
                            return 0;
                        }
                        return n + countdown(n - 1);
                    }
                    int result = countdown(10);
                ";
                var parser = new Parser(code);
                var program = parser.ParseProgram();

                if (parser.Errors.Count > 0)
                    return false;

                var evaluator = new Evaluator();
                evaluator.ExecuteProgram(program);

                var errors = new List<CompilationError>();
                var result = evaluator.ScopeManager.LookupVariable("result", 0, 0, errors);
                return result != null && result.IsInitialized && (int)result.Value == 55; // 1+2+...+10 = 55
            });
        }

        #endregion

        #region Error Detection Tests

        static void RunErrorDetectionTests()
        {
            // Test 28: Undeclared variable
            Test("Error: Undeclared variable", () =>
            {
                var parser = new Parser("x = 5;");
                parser.ParseProgram();

                return parser.Errors.Any(e =>
                    e.Type == ErrorType.Semantic &&
                    e.Message.Contains("was not declared"));
            });

            // Test 29: Duplicate declaration
            Test("Error: Duplicate declaration", () =>
            {
                var parser = new Parser("int a; int a;");
                var program = parser.ParseProgram();
                var evaluator = new Evaluator();
                evaluator.ExecuteProgram(program);

                // Duplicate declaration is now detected during execution
                return evaluator.Errors.Any(e =>
                    e.Type == ErrorType.Semantic &&
                    e.Message.Contains("already declared"));
            });

            // Test 30: Unary plus (forbidden)
            Test("Error: Unary plus not allowed", () =>
            {
                var parser = new Parser("int a = +5;");
                parser.ParseProgram();

                return parser.Errors.Any(e =>
                    e.Message.Contains("unary plus"));
            });

            // Test 31: Division by zero
            Test("Error: Division by zero", () =>
            {
                var parser = new Parser("int a = 5; int b = 0; int c = a / b;");
                var program = parser.ParseProgram();
                var evaluator = new Evaluator();
                evaluator.ExecuteProgram(program);

                return evaluator.Errors.Any(e =>
                    e.Message.Contains("division by zero"));
            });

            // Test 32: String with number (incompatible)
            Test("Error: String + number incompatible", () =>
            {
                var parser = new Parser("string s = \"test\"; int n = 5; string r = s + n;");
                var program = parser.ParseProgram();
                var evaluator = new Evaluator();
                evaluator.ExecuteProgram(program);

                return evaluator.Errors.Any(e =>
                    e.Message.Contains("mismatch"));
            });

            // Test 33: Unclosed string
            Test("Error: Unclosed string", () =>
            {
                var lexer = new Lexer("string s = \"hello");
                lexer.Tokenize();

                return lexer.Errors.Any(e =>
                    e.Type == ErrorType.Lexical &&
                    e.Message.Contains("unclosed"));
            });

            // Test 34: Undefined function should still error
            Test("Error: Undefined function", () =>
            {
                var parser = new Parser("int x = undefinedFunc();");
                parser.ParseProgram();

                return parser.Errors.Any(e =>
                    e.Type == ErrorType.Semantic &&
                    e.Message.Contains("undefinedFunc") &&
                    e.Message.Contains("not defined"));
            });
        }

        #endregion

        #region Helper Methods

        static void Test(string name, Func<bool> testFunc)
        {
            _totalTests++;

            try
            {
                bool result = testFunc();

                if (result)
                {
                    _passedTests++;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  PASS: {name}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  FAIL: {name} (unexpected result)");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  FAIL: {name} (exception: {ex.Message})");
            }

            Console.ResetColor();
        }

        static void DisplayFinalReport()
        {
            Console.WriteLine("\n" + new string('=', 60));

            double percent = (double)_passedTests / _totalTests * 100;

            Console.WriteLine($"FINAL REPORT: {_passedTests}/{_totalTests} tests passed ({percent:F0}%)");

            if (_passedTests == _totalTests)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nALL TESTS PASSED SUCCESSFULLY!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n{_totalTests - _passedTests} tests failed");
            }

            Console.ResetColor();
            Console.WriteLine(new string('=', 60));
        }

        #endregion
    }
}
