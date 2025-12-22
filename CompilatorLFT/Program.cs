using System;
using System.Collections.Generic;
using System.Linq;
using CompilatorLFT.Core;
using CompilatorLFT.Models.Instructiuni;
using CompilatorLFT.Utils;

namespace CompilatorLFT
{
    /// <summary>
    /// Program principal pentru Compilatorul LFT.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            AfiseazaHeader();

            // Verificare argumente linie comanda
            if (args.Length > 0)
            {
                // Rulare din fisier specificat ca argument
                RuleazaDinFisier(args[0]);
                return;
            }

            // Meniu interactiv
            while (true)
            {
                AfiseazaMeniu();
                Console.Write("\nAlegere: ");
                string alegere = Console.ReadLine()?.Trim();

                switch (alegere)
                {
                    case "1":
                        RuleazaDinFisierInteractiv();
                        break;

                    case "2":
                        RuleazaInteractiv();
                        break;

                    case "3":
                        RuleazaTesteAutomate();
                        break;

                    case "4":
                        AfiseazaExemple();
                        break;

                    case "5":
                    case "q":
                    case "Q":
                        Console.WriteLine("\nLa revedere!");
                        return;

                    default:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Alegere invalida! Introduceti un numar intre 1-5.");
                        Console.ResetColor();
                        break;
                }

                Console.WriteLine("\nApasati ENTER pentru a continua...");
                Console.ReadLine();
                Console.Clear();
            }
        }

        #region Afisare UI

        static void AfiseazaHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          COMPILATOR LFT - PROIECT ACADEMIC               ║");
            Console.WriteLine("║      Limbaje Formale si Translatoare - UAIC Iasi         ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void AfiseazaMeniu()
        {
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║           MENIU PRINCIPAL            ║");
            Console.WriteLine("╠══════════════════════════════════════╣");
            Console.WriteLine("║  1. Citire din fisier                ║");
            Console.WriteLine("║  2. Introducere manuala cod          ║");
            Console.WriteLine("║  3. Rulare teste automate            ║");
            Console.WriteLine("║  4. Afisare exemple                  ║");
            Console.WriteLine("║  5. Iesire                           ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
        }

        #endregion

        #region Moduri de Rulare

        static void RuleazaDinFisierInteractiv()
        {
            Console.Write("\nIntroduceti calea fisierului: ");
            string cale = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(cale))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Calea nu poate fi goala!");
                Console.ResetColor();
                return;
            }

            RuleazaDinFisier(cale);
        }

        static void RuleazaDinFisier(string cale)
        {
            string continut = CititorFisier.CitesteFisier(cale);

            if (continut == null)
                return;

            Console.WriteLine("\n=== COD SURSA ===");
            CititorFisier.AfiseazaCuNumereLinii(continut);

            CompileazaSiRuleaza(continut);
        }

        static void RuleazaInteractiv()
        {
            Console.WriteLine("\nIntroduceti codul sursa (linie goala pentru a termina):");
            Console.WriteLine("Exemplu: int a = 5; 3 + 4;");
            Console.WriteLine();

            var linii = new List<string>();
            int numarLinie = 1;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{numarLinie,3} | ");
                Console.ResetColor();

                string linie = Console.ReadLine();

                if (string.IsNullOrEmpty(linie))
                    break;

                linii.Add(linie);
                numarLinie++;
            }

            if (linii.Count == 0)
            {
                Console.WriteLine("Nu s-a introdus niciun cod!");
                return;
            }

            string continut = string.Join("\n", linii);
            CompileazaSiRuleaza(continut);
        }

        #endregion

        #region Compilare si Executie

        static void CompileazaSiRuleaza(string continut)
        {
            // FAZA 1: ANALIZA LEXICALA
            Console.WriteLine("\n" + new string('=', 50));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("FAZA 1: ANALIZA LEXICALA");
            Console.ResetColor();
            Console.WriteLine(new string('=', 50));

            var lexer = new Lexer(continut);
            var tokeni = lexer.Tokenizeaza();

            Console.WriteLine($"Tokeni generati: {tokeni.Count}");

            if (lexer.Erori.Any())
            {
                AfiseazaErori("ERORI LEXICALE", lexer.Erori);
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Analiza lexicala reusita!");
            Console.ResetColor();

            // Afisare tokeni (optional)
            Console.WriteLine("\nTokeni:");
            foreach (var token in tokeni.Take(20))
            {
                Console.WriteLine($"  {token}");
            }
            if (tokeni.Count > 20)
            {
                Console.WriteLine($"  ... si inca {tokeni.Count - 20} tokeni");
            }

            // FAZA 2: ANALIZA SINTACTICA SI SEMANTICA
            Console.WriteLine("\n" + new string('=', 50));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("FAZA 2: ANALIZA SINTACTICA & SEMANTICA");
            Console.ResetColor();
            Console.WriteLine(new string('=', 50));

            var parser = new Parser(continut);
            var program = parser.ParseazaProgram();

            Console.WriteLine($"Instructiuni parsate: {program.Instructiuni.Count}");
            Console.WriteLine($"Variabile declarate: {parser.TabelSimboluri.NumarVariabile}");

            if (parser.Erori.Any())
            {
                AfiseazaErori("ERORI SINTACTICE/SEMANTICE", parser.Erori);
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Analiza sintactica reusita!");
            Console.WriteLine("✓ Analiza semantica reusita!");
            Console.ResetColor();

            // AFISARE ARBORE SINTACTIC
            Console.WriteLine("\n" + new string('=', 50));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("ARBORE SINTACTIC (AST)");
            Console.ResetColor();
            Console.WriteLine(new string('=', 50));

            program.AfiseazaArbore();

            // FAZA 3: EVALUARE SI EXECUTIE
            Console.WriteLine("\n" + new string('=', 50));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("FAZA 3: EVALUARE & EXECUTIE");
            Console.ResetColor();
            Console.WriteLine(new string('=', 50));

            var evaluator = new Evaluator(parser.TabelSimboluri);
            evaluator.ExecutaProgram(program);

            if (evaluator.Erori.Any())
            {
                AfiseazaErori("ERORI RUNTIME", evaluator.Erori);
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n✓ Executie reusita!");
            Console.ResetColor();

            // TABEL SIMBOLURI FINAL
            Console.WriteLine("\n" + new string('=', 50));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("TABEL SIMBOLURI (STARE FINALA)");
            Console.ResetColor();
            Console.WriteLine(new string('=', 50));

            parser.TabelSimboluri.AfiseazaVariabile();
        }

        static void AfiseazaErori(string titlu, IEnumerable<EroareCompilare> erori)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n{titlu}:");
            foreach (var eroare in erori)
            {
                Console.WriteLine($"  ✗ {eroare}");
            }
            Console.ResetColor();
        }

        #endregion

        #region Teste Automate

        static void RuleazaTesteAutomate()
        {
            Console.WriteLine("\n" + new string('=', 60));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("RULARE TESTE AUTOMATE");
            Console.ResetColor();
            Console.WriteLine(new string('=', 60));

            var teste = new List<(string nume, string cod, bool asteptaSaReuseasca)>
            {
                // Teste de baza
                ("Declaratie simpla", "int a;", true),
                ("Declaratie cu initializare", "int a = 5;", true),
                ("Declaratii multiple", "int a, b=3, c;", true),
                ("Expresie aritmetica", "3 + 5;", true),
                ("Precedenta operatori", "3 + 4 * 5;", true),
                ("Paranteze", "(3 + 4) * 5;", true),
                ("Minus unar", "int a = -5;", true),
                ("Double literal", "double x = 3.14;", true),
                ("String literal", "string s = \"hello\";", true),
                ("Concatenare string", "string a = \"hello\"; string b = \" world\"; string c = a + b;", true),

                // Atribuiri si operatii
                ("Atribuire si calcul", "int a = 5; int b = 3; int c = a + b;", true),
                ("Operatori relationali", "int a = 5; int b = 3; a > b;", true),

                // Structuri de control
                ("If simplu", "int a = 5; int b = 0; if (a > 3) { b = 10; }", true),
                ("If-else", "int a = 2; int b = 0; if (a > 3) { b = 10; } else { b = 20; }", true),
                ("While", "int i = 0; int sum = 0; while (i < 5) { sum = sum + i; i = i + 1; }", true),
                ("For", "int sum = 0; for (int i = 0; i < 5; i = i + 1) { sum = sum + i; }", true),

                // Teste de erori (trebuie sa esueze)
                ("Eroare: variabila nedeclarata", "x = 5;", false),
                ("Eroare: declaratie duplicata", "int a; int a;", false),
                ("Eroare: plus unar", "int a = +5;", false),
            };

            int reusute = 0;
            int total = teste.Count;

            foreach (var (nume, cod, asteptaSaReuseasca) in teste)
            {
                bool reusit = RuleazaTest(nume, cod, asteptaSaReuseasca);
                if (reusit)
                    reusute++;
            }

            // Raport final
            Console.WriteLine("\n" + new string('=', 60));
            double procent = (double)reusute / total * 100;

            if (reusute == total)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ TOATE TESTELE AU TRECUT! ({reusute}/{total} - {procent:F0}%)");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Teste reusute: {reusute}/{total} ({procent:F0}%)");
            }
            Console.ResetColor();
        }

        static bool RuleazaTest(string nume, string cod, bool asteptaSaReuseasca)
        {
            try
            {
                var parser = new Parser(cod);
                var program = parser.ParseazaProgram();

                bool areEroriParsare = parser.Erori.Any();

                if (!areEroriParsare)
                {
                    var evaluator = new Evaluator(parser.TabelSimboluri);
                    evaluator.ExecutaProgram(program);

                    bool areEroriEvaluare = evaluator.Erori.Any();

                    if (asteptaSaReuseasca && !areEroriEvaluare)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"✓ {nume}");
                        Console.ResetColor();
                        return true;
                    }
                    else if (!asteptaSaReuseasca && areEroriEvaluare)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"✓ {nume} (eroare detectata corect)");
                        Console.ResetColor();
                        return true;
                    }
                }
                else
                {
                    if (!asteptaSaReuseasca)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"✓ {nume} (eroare detectata corect)");
                        Console.ResetColor();
                        return true;
                    }
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ {nume}");
                Console.ResetColor();
                return false;
            }
            catch (Exception ex)
            {
                if (!asteptaSaReuseasca)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ {nume} (eroare detectata: {ex.Message})");
                    Console.ResetColor();
                    return true;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ {nume} - Exceptie: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        #endregion

        #region Exemple

        static void AfiseazaExemple()
        {
            Console.WriteLine("\n" + new string('=', 60));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("EXEMPLE DE COD SURSA");
            Console.ResetColor();
            Console.WriteLine(new string('=', 60));

            var exemple = new[]
            {
                ("Declaratii si expresii", @"
int a = 5;
int b = 3;
int suma = a + b;
suma * 2;
"),
                ("Structuri de control", @"
int sum = 0;
for (int i = 1; i <= 10; i = i + 1) {
    sum = sum + i;
}
sum;
"),
                ("Conditionale", @"
int x = 7;
int rezultat;
if (x > 5) {
    rezultat = 100;
} else {
    rezultat = 0;
}
rezultat;
"),
                ("String-uri", @"
string salut = ""Hello"";
string nume = "" World"";
string mesaj = salut + nume;
mesaj;
")
            };

            foreach (var (titlu, cod) in exemple)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n--- {titlu} ---");
                Console.ResetColor();
                Console.WriteLine(cod.Trim());
            }
        }

        #endregion
    }
}
