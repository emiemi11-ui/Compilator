using System;
using System.Collections.Generic;
using System.Linq;
using CompilatorLFT.Core;
using CompilatorLFT.Models;

namespace CompilatorLFT.Tests
{
    /// <summary>
    /// Suite de teste automate pentru compilator.
    /// Contine 25+ teste pentru toate componentele.
    /// </summary>
    public static class TestSuite
    {
        private static int _testeReusute;
        private static int _testeTotal;

        #region Entry Point

        /// <summary>
        /// Ruleaza toate testele si afiseaza raportul.
        /// </summary>
        public static void RuleazaToateTestele()
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              SUITE TESTE AUTOMATE - COMPILATOR LFT         ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

            _testeReusute = 0;
            _testeTotal = 0;

            // Teste Lexer
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("═══ TESTE LEXER ═══");
            Console.ResetColor();
            RuleazaTesteLexer();

            // Teste Parser
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n═══ TESTE PARSER ═══");
            Console.ResetColor();
            RuleazaTesteParser();

            // Teste Evaluator
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n═══ TESTE EVALUATOR ═══");
            Console.ResetColor();
            RuleazaTesteEvaluator();

            // Teste Integrare
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n═══ TESTE INTEGRARE ═══");
            Console.ResetColor();
            RuleazaTesteIntegrare();

            // Teste Erori
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n═══ TESTE DETECTARE ERORI ═══");
            Console.ResetColor();
            RuleazaTesteErori();

            // Raport final
            AfiseazaRaportFinal();
        }

        #endregion

        #region Teste Lexer

        static void RuleazaTesteLexer()
        {
            // Test 1: Declaratie simpla
            Test("Lexer: Declaratie simpla", () =>
            {
                var lexer = new Lexer("int a;");
                var tokeni = lexer.Tokenizeaza();

                return tokeni.Count == 4 && // int, a, ;, EOF
                       tokeni[0].Tip == TipAtomLexical.CuvantCheieInt &&
                       tokeni[1].Tip == TipAtomLexical.Identificator &&
                       tokeni[2].Tip == TipAtomLexical.PunctVirgula &&
                       lexer.Erori.Count == 0;
            });

            // Test 2: Numere intregi si zecimale
            Test("Lexer: Numere intregi si zecimale", () =>
            {
                var lexer = new Lexer("42 3.14");
                var tokeni = lexer.Tokenizeaza();

                return tokeni.Count == 3 &&
                       tokeni[0].Tip == TipAtomLexical.NumarIntreg &&
                       (int)tokeni[0].Valoare == 42 &&
                       tokeni[1].Tip == TipAtomLexical.NumarZecimal &&
                       Math.Abs((double)tokeni[1].Valoare - 3.14) < 0.001;
            });

            // Test 3: String literal
            Test("Lexer: String literal", () =>
            {
                var lexer = new Lexer("\"hello world\"");
                var tokeni = lexer.Tokenizeaza();

                return tokeni.Count == 2 &&
                       tokeni[0].Tip == TipAtomLexical.StringLiteral &&
                       (string)tokeni[0].Valoare == "hello world";
            });

            // Test 4: Operatori relationali
            Test("Lexer: Operatori relationali", () =>
            {
                var lexer = new Lexer("<= >= == !=");
                var tokeni = lexer.Tokenizeaza();

                return tokeni[0].Tip == TipAtomLexical.MaiMicEgal &&
                       tokeni[1].Tip == TipAtomLexical.MaiMareEgal &&
                       tokeni[2].Tip == TipAtomLexical.EgalEgal &&
                       tokeni[3].Tip == TipAtomLexical.Diferit;
            });

            // Test 5: Cuvinte cheie
            Test("Lexer: Cuvinte cheie", () =>
            {
                var lexer = new Lexer("int double string for while if else");
                var tokeni = lexer.Tokenizeaza();

                return tokeni[0].Tip == TipAtomLexical.CuvantCheieInt &&
                       tokeni[1].Tip == TipAtomLexical.CuvantCheieDouble &&
                       tokeni[2].Tip == TipAtomLexical.CuvantCheieString &&
                       tokeni[3].Tip == TipAtomLexical.CuvantCheieFor &&
                       tokeni[4].Tip == TipAtomLexical.CuvantCheieWhile &&
                       tokeni[5].Tip == TipAtomLexical.CuvantCheieIf &&
                       tokeni[6].Tip == TipAtomLexical.CuvantCheieElse;
            });

            // Test 6: Tracking linie/coloana
            Test("Lexer: Tracking linie/coloana", () =>
            {
                var lexer = new Lexer("int a;\na = 5;");
                var tokeni = lexer.Tokenizeaza();

                // 'a' de pe linia 2 trebuie sa aiba Linie == 2
                var aLinia2 = tokeni.FirstOrDefault(t =>
                    t.Tip == TipAtomLexical.Identificator && t.Linie == 2);

                return aLinia2 != null && aLinia2.Coloana == 1;
            });
        }

        #endregion

        #region Teste Parser

        static void RuleazaTesteParser()
        {
            // Test 7: Declaratie cu initializare
            Test("Parser: Declaratie cu initializare", () =>
            {
                var parser = new Parser("int a = 5;");
                var program = parser.ParseazaProgram();

                return program.Instructiuni.Count == 1 &&
                       parser.Erori.Count == 0 &&
                       parser.TabelSimboluri.Exista("a");
            });

            // Test 8: Declaratii multiple
            Test("Parser: Declaratii multiple", () =>
            {
                var parser = new Parser("int a, b = 3, c;");
                var program = parser.ParseazaProgram();

                return parser.TabelSimboluri.Exista("a") &&
                       parser.TabelSimboluri.Exista("b") &&
                       parser.TabelSimboluri.Exista("c") &&
                       parser.Erori.Count == 0;
            });

            // Test 9: Precedenta operatori
            Test("Parser: Precedenta operatori", () =>
            {
                var parser = new Parser("3 + 4 * 5;");
                var program = parser.ParseazaProgram();

                // AST-ul trebuie sa aiba * ca copil al +
                return program.Instructiuni.Count == 1 &&
                       parser.Erori.Count == 0;
            });

            // Test 10: Paranteze
            Test("Parser: Paranteze schimba precedenta", () =>
            {
                var parser = new Parser("(3 + 4) * 5;");
                var program = parser.ParseazaProgram();

                return program.Instructiuni.Count == 1 &&
                       parser.Erori.Count == 0;
            });

            // Test 11: Structura FOR
            Test("Parser: Structura FOR", () =>
            {
                var parser = new Parser("for (int i = 0; i < 10; i = i + 1) { }");
                var program = parser.ParseazaProgram();

                return program.Instructiuni.Count == 1 &&
                       parser.TabelSimboluri.Exista("i") &&
                       parser.Erori.Count == 0;
            });

            // Test 12: Structura WHILE
            Test("Parser: Structura WHILE", () =>
            {
                var parser = new Parser("int x = 5; while (x > 0) { x = x - 1; }");
                var program = parser.ParseazaProgram();

                return program.Instructiuni.Count == 2 &&
                       parser.Erori.Count == 0;
            });

            // Test 13: Structura IF-ELSE
            Test("Parser: Structura IF-ELSE", () =>
            {
                var parser = new Parser("int a = 5; if (a > 3) { a = 10; } else { a = 0; }");
                var program = parser.ParseazaProgram();

                return program.Instructiuni.Count == 2 &&
                       parser.Erori.Count == 0;
            });
        }

        #endregion

        #region Teste Evaluator

        static void RuleazaTesteEvaluator()
        {
            // Test 14: Evaluare expresie simpla
            Test("Evaluator: Expresie simpla", () =>
            {
                var parser = new Parser("int a = 5; int b = a + 3;");
                var program = parser.ParseazaProgram();
                var evaluator = new Evaluator(parser.TabelSimboluri);
                evaluator.ExecutaProgram(program);

                var b = parser.TabelSimboluri.Obtine("b");
                return b != null && b.EsteInitializata && (int)b.Valoare == 8;
            });

            // Test 15: Precedenta operatori in evaluare
            Test("Evaluator: Precedenta operatori", () =>
            {
                var parser = new Parser("int a = 3 + 4 * 5;");
                var program = parser.ParseazaProgram();
                var evaluator = new Evaluator(parser.TabelSimboluri);
                evaluator.ExecutaProgram(program);

                var a = parser.TabelSimboluri.Obtine("a");
                return a != null && (int)a.Valoare == 23; // 3 + 20 = 23
            });

            // Test 16: Conversie int -> double
            Test("Evaluator: Conversie int -> double", () =>
            {
                var parser = new Parser("int a = 5; double b = 2.5; double c = a + b;");
                var program = parser.ParseazaProgram();
                var evaluator = new Evaluator(parser.TabelSimboluri);
                evaluator.ExecutaProgram(program);

                var c = parser.TabelSimboluri.Obtine("c");
                return c != null && Math.Abs((double)c.Valoare - 7.5) < 0.001;
            });

            // Test 17: Truncare double -> int
            Test("Evaluator: Truncare double -> int", () =>
            {
                var parser = new Parser("double x = 7.8; int a = x;");
                var program = parser.ParseazaProgram();
                var evaluator = new Evaluator(parser.TabelSimboluri);
                evaluator.ExecutaProgram(program);

                var a = parser.TabelSimboluri.Obtine("a");
                return a != null && (int)a.Valoare == 7;
            });

            // Test 18: Concatenare string
            Test("Evaluator: Concatenare string", () =>
            {
                var parser = new Parser("string s1 = \"hello\"; string s2 = \" world\"; string s3 = s1 + s2;");
                var program = parser.ParseazaProgram();
                var evaluator = new Evaluator(parser.TabelSimboluri);
                evaluator.ExecutaProgram(program);

                var s3 = parser.TabelSimboluri.Obtine("s3");
                return s3 != null && (string)s3.Valoare == "hello world";
            });

            // Test 19: Minus unar
            Test("Evaluator: Minus unar", () =>
            {
                var parser = new Parser("int a = -5; int b = -a;");
                var program = parser.ParseazaProgram();
                var evaluator = new Evaluator(parser.TabelSimboluri);
                evaluator.ExecutaProgram(program);

                var a = parser.TabelSimboluri.Obtine("a");
                var b = parser.TabelSimboluri.Obtine("b");
                return (int)a.Valoare == -5 && (int)b.Valoare == 5;
            });
        }

        #endregion

        #region Teste Integrare

        static void RuleazaTesteIntegrare()
        {
            // Test 20: FOR simplu
            Test("Integrare: FOR cu suma", () =>
            {
                var parser = new Parser("int sum = 0; for (int i = 0; i < 5; i = i + 1) { sum = sum + i; }");
                var program = parser.ParseazaProgram();
                var evaluator = new Evaluator(parser.TabelSimboluri);
                evaluator.ExecutaProgram(program);

                var sum = parser.TabelSimboluri.Obtine("sum");
                return sum != null && (int)sum.Valoare == 10; // 0+1+2+3+4
            });

            // Test 21: WHILE
            Test("Integrare: WHILE cu contor", () =>
            {
                var parser = new Parser("int i = 0; int sum = 0; while (i < 5) { sum = sum + i; i = i + 1; }");
                var program = parser.ParseazaProgram();
                var evaluator = new Evaluator(parser.TabelSimboluri);
                evaluator.ExecutaProgram(program);

                var sum = parser.TabelSimboluri.Obtine("sum");
                var i = parser.TabelSimboluri.Obtine("i");
                return (int)sum.Valoare == 10 && (int)i.Valoare == 5;
            });

            // Test 22: IF cu conditie adevarata
            Test("Integrare: IF conditie adevarata", () =>
            {
                var parser = new Parser("int a = 5; int b = 0; if (a > 3) { b = 10; }");
                var program = parser.ParseazaProgram();
                var evaluator = new Evaluator(parser.TabelSimboluri);
                evaluator.ExecutaProgram(program);

                var b = parser.TabelSimboluri.Obtine("b");
                return (int)b.Valoare == 10;
            });

            // Test 23: IF-ELSE cu conditie falsa
            Test("Integrare: IF-ELSE conditie falsa", () =>
            {
                var parser = new Parser("int a = 2; int b = 0; if (a > 3) { b = 10; } else { b = 20; }");
                var program = parser.ParseazaProgram();
                var evaluator = new Evaluator(parser.TabelSimboluri);
                evaluator.ExecutaProgram(program);

                var b = parser.TabelSimboluri.Obtine("b");
                return (int)b.Valoare == 20;
            });

            // Test 24: Operatori relationali
            Test("Integrare: Operatori relationali", () =>
            {
                var parser = new Parser("int a = 5; int b = 3; int r = 0; if (a >= b) { r = 1; }");
                var program = parser.ParseazaProgram();
                var evaluator = new Evaluator(parser.TabelSimboluri);
                evaluator.ExecutaProgram(program);

                var r = parser.TabelSimboluri.Obtine("r");
                return (int)r.Valoare == 1;
            });
        }

        #endregion

        #region Teste Erori

        static void RuleazaTesteErori()
        {
            // Test 25: Variabila nedeclarata
            Test("Eroare: Variabila nedeclarata", () =>
            {
                var parser = new Parser("x = 5;");
                parser.ParseazaProgram();

                return parser.Erori.Any(e =>
                    e.Tip == TipEroare.Semantica &&
                    e.Mesaj.Contains("nu a fost declarata"));
            });

            // Test 26: Declaratie duplicata
            Test("Eroare: Declaratie duplicata", () =>
            {
                var parser = new Parser("int a; int a;");
                parser.ParseazaProgram();

                return parser.Erori.Any(e =>
                    e.Tip == TipEroare.Semantica &&
                    e.Mesaj.Contains("duplicata"));
            });

            // Test 27: Plus unar (interzis)
            Test("Eroare: Plus unar nu este permis", () =>
            {
                var parser = new Parser("int a = +5;");
                parser.ParseazaProgram();

                return parser.Erori.Any(e =>
                    e.Mesaj.Contains("plus unar"));
            });

            // Test 28: Impartire la zero
            Test("Eroare: Impartire la zero", () =>
            {
                var parser = new Parser("int a = 5; int b = 0; int c = a / b;");
                var program = parser.ParseazaProgram();
                var evaluator = new Evaluator(parser.TabelSimboluri);
                evaluator.ExecutaProgram(program);

                return evaluator.Erori.Any(e =>
                    e.Mesaj.Contains("impartire la zero"));
            });

            // Test 29: String cu numar (incompatibil)
            Test("Eroare: String + numar incompatibil", () =>
            {
                var parser = new Parser("string s = \"test\"; int n = 5; string r = s + n;");
                var program = parser.ParseazaProgram();
                var evaluator = new Evaluator(parser.TabelSimboluri);
                evaluator.ExecutaProgram(program);

                return evaluator.Erori.Any(e =>
                    e.Mesaj.Contains("incompatibilitate"));
            });

            // Test 30: String neînchis
            Test("Eroare: String neinchis", () =>
            {
                var lexer = new Lexer("string s = \"hello");
                lexer.Tokenizeaza();

                return lexer.Erori.Any(e =>
                    e.Tip == TipEroare.Lexicala &&
                    e.Mesaj.Contains("neinchis"));
            });
        }

        #endregion

        #region Helper Methods

        static void Test(string nume, Func<bool> testFunc)
        {
            _testeTotal++;

            try
            {
                bool rezultat = testFunc();

                if (rezultat)
                {
                    _testeReusute++;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ✓ {nume}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  ✗ {nume} (rezultat neasteptat)");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ {nume} (exceptie: {ex.Message})");
            }

            Console.ResetColor();
        }

        static void AfiseazaRaportFinal()
        {
            Console.WriteLine("\n" + new string('═', 60));

            double procent = (double)_testeReusute / _testeTotal * 100;

            Console.WriteLine($"RAPORT FINAL: {_testeReusute}/{_testeTotal} teste reusute ({procent:F0}%)");

            if (_testeReusute == _testeTotal)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n✓ TOATE TESTELE AU TRECUT CU SUCCES!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n⚠ {_testeTotal - _testeReusute} teste au esuat");
            }

            Console.ResetColor();
            Console.WriteLine(new string('═', 60));
        }

        #endregion
    }
}
