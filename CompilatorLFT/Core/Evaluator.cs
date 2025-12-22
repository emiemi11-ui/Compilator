using System;
using System.Collections.Generic;
using CompilatorLFT.Models;
using CompilatorLFT.Models.Expresii;
using CompilatorLFT.Models.Instructiuni;
using CompilatorLFT.Utils;

namespace CompilatorLFT.Core
{
    /// <summary>
    /// Evaluator pentru expresii si executor pentru instructiuni.
    /// </summary>
    /// <remarks>
    /// Referinta: Dragon Book, Cap. 5 - Syntax-Directed Translation
    /// Implementeaza Visitor Pattern pentru traversarea AST.
    /// </remarks>
    public class Evaluator
    {
        #region Campuri private

        private readonly TabelSimboluri _tabelSimboluri;
        private readonly List<EroareCompilare> _erori;
        private readonly List<string> _output;

        // Constanta pentru comparatie floating point
        private const double EPSILON = 1e-10;

        // Limita pentru bucle (pentru a evita bucle infinite)
        private const int LIMITA_ITERATII = 100000;

        #endregion

        #region Proprietati

        /// <summary>Lista de erori de evaluare.</summary>
        public IReadOnlyList<EroareCompilare> Erori => _erori;

        /// <summary>Output-ul generat de evaluare.</summary>
        public IReadOnlyList<string> Output => _output;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializeaza evaluatorul cu tabelul de simboluri.
        /// </summary>
        /// <param name="tabelSimboluri">Tabelul de simboluri populat de parser</param>
        public Evaluator(TabelSimboluri tabelSimboluri)
        {
            _tabelSimboluri = tabelSimboluri ?? throw new ArgumentNullException(nameof(tabelSimboluri));
            _erori = new List<EroareCompilare>();
            _output = new List<string>();
        }

        #endregion

        #region Executie Program

        /// <summary>
        /// Executa intregul program.
        /// </summary>
        /// <param name="program">Programul de executat</param>
        public void ExecutaProgram(ProgramComplet program)
        {
            foreach (var instructiune in program.Instructiuni)
            {
                ExecutaInstructiune(instructiune);
            }
        }

        /// <summary>
        /// Executa o instructiune.
        /// </summary>
        private void ExecutaInstructiune(Instructiune instructiune)
        {
            switch (instructiune)
            {
                case InstructiuneDeclaratie decl:
                    ExecutaDeclaratie(decl);
                    break;

                case InstructiuneAtribuire atrib:
                    ExecutaAtribuire(atrib);
                    break;

                case InstructiuneExpresie expr:
                    ExecutaExpresieStandalone(expr);
                    break;

                case InstructiuneFor instrFor:
                    ExecutaFor(instrFor);
                    break;

                case InstructiuneWhile instrWhile:
                    ExecutaWhile(instrWhile);
                    break;

                case InstructiuneIf instrIf:
                    ExecutaIf(instrIf);
                    break;

                case Bloc bloc:
                    ExecutaBloc(bloc);
                    break;
            }
        }

        /// <summary>
        /// Executa o declaratie de variabile.
        /// </summary>
        private void ExecutaDeclaratie(InstructiuneDeclaratie decl)
        {
            var tipDat = TabelSimboluri.ConvertesteLaTipDat(decl.TipCuvantCheie.Tip);

            foreach (var (id, exprInit) in decl.Declaratii)
            {
                if (exprInit != null)
                {
                    var valoare = EvalueazaExpresie(exprInit);

                    if (valoare != null)
                    {
                        var valoareConvertita = ConvertesteLaTip(valoare, tipDat, id.Linie, id.Coloana);

                        if (valoareConvertita != null)
                        {
                            _tabelSimboluri.SeteazaValoare(
                                id.Text, valoareConvertita,
                                id.Linie, id.Coloana, _erori);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Executa o atribuire.
        /// </summary>
        private void ExecutaAtribuire(InstructiuneAtribuire atrib)
        {
            var valoare = EvalueazaExpresie(atrib.Expresie);

            if (valoare != null)
            {
                var variabila = _tabelSimboluri.Obtine(atrib.Identificator.Text);

                if (variabila != null)
                {
                    var valoareConvertita = ConvertesteLaTip(
                        valoare, variabila.Tip,
                        atrib.Identificator.Linie, atrib.Identificator.Coloana);

                    if (valoareConvertita != null)
                    {
                        _tabelSimboluri.SeteazaValoare(
                            atrib.Identificator.Text, valoareConvertita,
                            atrib.Identificator.Linie, atrib.Identificator.Coloana,
                            _erori);
                    }
                }
            }
        }

        /// <summary>
        /// Executa o expresie standalone si afiseaza rezultatul.
        /// </summary>
        private void ExecutaExpresieStandalone(InstructiuneExpresie expr)
        {
            var rezultat = EvalueazaExpresie(expr.Expresie);

            if (rezultat != null)
            {
                string output = $"Rezultat: {FormateazaValoare(rezultat)}";
                _output.Add(output);
                Console.WriteLine(output);
            }
        }

        /// <summary>
        /// Executa o instructiune FOR.
        /// </summary>
        private void ExecutaFor(InstructiuneFor instrFor)
        {
            // Initializare
            if (instrFor.Initializare != null)
            {
                ExecutaInstructiune(instrFor.Initializare);
            }

            int iteratii = 0;

            // Bucla
            while (true)
            {
                if (++iteratii > LIMITA_ITERATII)
                {
                    _erori.Add(EroareCompilare.Semantica(
                        instrFor.CuvantCheieFor.Linie,
                        instrFor.CuvantCheieFor.Coloana,
                        $"bucla for a depasit limita de {LIMITA_ITERATII} iteratii"));
                    break;
                }

                // Evalueaza conditia
                if (instrFor.Conditie != null)
                {
                    var conditie = EvalueazaExpresie(instrFor.Conditie);

                    if (!EsteAdevarat(conditie))
                        break;
                }

                // Executa corpul
                ExecutaInstructiune(instrFor.Corp);

                // Increment
                if (instrFor.Increment != null)
                {
                    ExecutaInstructiune(instrFor.Increment);
                }
            }
        }

        /// <summary>
        /// Executa o instructiune WHILE.
        /// </summary>
        private void ExecutaWhile(InstructiuneWhile instrWhile)
        {
            int iteratii = 0;

            while (true)
            {
                if (++iteratii > LIMITA_ITERATII)
                {
                    _erori.Add(EroareCompilare.Semantica(
                        instrWhile.CuvantCheieWhile.Linie,
                        instrWhile.CuvantCheieWhile.Coloana,
                        $"bucla while a depasit limita de {LIMITA_ITERATII} iteratii"));
                    break;
                }

                var conditie = EvalueazaExpresie(instrWhile.Conditie);

                if (!EsteAdevarat(conditie))
                    break;

                ExecutaInstructiune(instrWhile.Corp);
            }
        }

        /// <summary>
        /// Executa o instructiune IF.
        /// </summary>
        private void ExecutaIf(InstructiuneIf instrIf)
        {
            var conditie = EvalueazaExpresie(instrIf.Conditie);

            if (EsteAdevarat(conditie))
            {
                ExecutaInstructiune(instrIf.CorpAdevarat);
            }
            else if (instrIf.CorpFals != null)
            {
                ExecutaInstructiune(instrIf.CorpFals);
            }
        }

        /// <summary>
        /// Executa un bloc de instructiuni.
        /// </summary>
        private void ExecutaBloc(Bloc bloc)
        {
            foreach (var instr in bloc.Instructiuni)
            {
                ExecutaInstructiune(instr);
            }
        }

        #endregion

        #region Evaluare Expresii

        /// <summary>
        /// Evalueaza o expresie si returneaza rezultatul.
        /// </summary>
        public object EvalueazaExpresie(Expresie expresie)
        {
            return expresie switch
            {
                ExpresieNumerica num => EvalueazaExpresieNumerica(num),
                ExpresieString str => EvalueazaExpresieString(str),
                ExpresieIdentificator id => EvalueazaExpresieIdentificator(id),
                ExpresieUnara unara => EvalueazaExpresieUnara(unara),
                ExpresieBinara binara => EvalueazaExpresieBinara(binara),
                ExpresieCuParanteze par => EvalueazaExpresie(par.Expresie),
                _ => null
            };
        }

        private object EvalueazaExpresieNumerica(ExpresieNumerica num)
        {
            return num.Numar.Valoare;
        }

        private object EvalueazaExpresieString(ExpresieString str)
        {
            return str.ValoareString.Valoare;
        }

        private object EvalueazaExpresieIdentificator(ExpresieIdentificator id)
        {
            return _tabelSimboluri.ObtineValoare(
                id.Identificator.Text,
                id.Identificator.Linie,
                id.Identificator.Coloana,
                _erori);
        }

        private object EvalueazaExpresieUnara(ExpresieUnara unara)
        {
            var operand = EvalueazaExpresie(unara.Operand);

            if (operand == null)
                return null;

            // Doar minus unar
            if (unara.Operator.Tip == TipAtomLexical.Minus)
            {
                if (operand is int i)
                    return -i;

                if (operand is double d)
                    return -d;

                _erori.Add(EroareCompilare.Semantica(
                    unara.Operator.Linie, unara.Operator.Coloana,
                    $"operatorul minus unar nu se poate aplica pe tip {operand.GetType().Name}"));
            }

            return null;
        }

        private object EvalueazaExpresieBinara(ExpresieBinara binara)
        {
            var stanga = EvalueazaExpresie(binara.Stanga);
            var dreapta = EvalueazaExpresie(binara.Dreapta);

            if (stanga == null || dreapta == null)
                return null;

            var op = binara.Operator;

            // Operatii aritmetice
            if (op.EsteOperatorAritmetic())
            {
                return EvalueazaOperatieAritmetica(stanga, op, dreapta);
            }

            // Operatii relationale
            if (op.EsteOperatorRelational())
            {
                return EvalueazaOperatieRelationala(stanga, op, dreapta);
            }

            _erori.Add(EroareCompilare.Semantica(
                op.Linie, op.Coloana,
                $"operator necunoscut '{op.Text}'"));

            return null;
        }

        #endregion

        #region Operatii Aritmetice

        private object EvalueazaOperatieAritmetica(object stanga, AtomLexical op, object dreapta)
        {
            // STRING + STRING (concatenare)
            if (stanga is string str1 && dreapta is string str2)
            {
                if (op.Tip == TipAtomLexical.Plus)
                {
                    return str1 + str2;
                }

                _erori.Add(EroareCompilare.Semantica(
                    op.Linie, op.Coloana,
                    $"operatia '{op.Text}' nu este suportata pentru string-uri (doar + pentru concatenare)"));
                return null;
            }

            // STRING cu NUMAR - EROARE
            if (stanga is string || dreapta is string)
            {
                _erori.Add(EroareCompilare.Semantica(
                    op.Linie, op.Coloana,
                    "incompatibilitate tipuri: nu se poate combina string cu numar"));
                return null;
            }

            // INT op INT -> INT
            if (stanga is int i1 && dreapta is int i2)
            {
                return op.Tip switch
                {
                    TipAtomLexical.Plus => CheckedAdd(i1, i2, op),
                    TipAtomLexical.Minus => CheckedSubtract(i1, i2, op),
                    TipAtomLexical.Star => CheckedMultiply(i1, i2, op),
                    TipAtomLexical.Slash => DivideInt(i1, i2, op),
                    _ => null
                };
            }

            // DOUBLE op DOUBLE -> DOUBLE
            if (stanga is double d1 && dreapta is double d2)
            {
                return op.Tip switch
                {
                    TipAtomLexical.Plus => d1 + d2,
                    TipAtomLexical.Minus => d1 - d2,
                    TipAtomLexical.Star => d1 * d2,
                    TipAtomLexical.Slash => DivideDouble(d1, d2, op),
                    _ => null
                };
            }

            // INT op DOUBLE -> DOUBLE (conversie implicita)
            if (stanga is int ii && dreapta is double dd)
            {
                return EvalueazaOperatieAritmetica((double)ii, op, dd);
            }

            if (stanga is double di && dreapta is int iii)
            {
                return EvalueazaOperatieAritmetica(di, op, (double)iii);
            }

            _erori.Add(EroareCompilare.Semantica(
                op.Linie, op.Coloana,
                $"incompatibilitate tipuri: {stanga?.GetType().Name ?? "null"} {op.Text} {dreapta?.GetType().Name ?? "null"}"));

            return null;
        }

        private object CheckedAdd(int a, int b, AtomLexical op)
        {
            try
            {
                return checked(a + b);
            }
            catch (OverflowException)
            {
                _erori.Add(EroareCompilare.Semantica(op.Linie, op.Coloana, "overflow la adunare intregi"));
                return null;
            }
        }

        private object CheckedSubtract(int a, int b, AtomLexical op)
        {
            try
            {
                return checked(a - b);
            }
            catch (OverflowException)
            {
                _erori.Add(EroareCompilare.Semantica(op.Linie, op.Coloana, "overflow la scadere intregi"));
                return null;
            }
        }

        private object CheckedMultiply(int a, int b, AtomLexical op)
        {
            try
            {
                return checked(a * b);
            }
            catch (OverflowException)
            {
                _erori.Add(EroareCompilare.Semantica(op.Linie, op.Coloana, "overflow la inmultire intregi"));
                return null;
            }
        }

        private object DivideInt(int a, int b, AtomLexical op)
        {
            if (b == 0)
            {
                _erori.Add(EroareCompilare.Semantica(op.Linie, op.Coloana, "impartire la zero"));
                return null;
            }
            return a / b;
        }

        private object DivideDouble(double a, double b, AtomLexical op)
        {
            if (Math.Abs(b) < EPSILON)
            {
                _erori.Add(EroareCompilare.Semantica(op.Linie, op.Coloana, "impartire la zero"));
                return null;
            }
            return a / b;
        }

        #endregion

        #region Operatii Relationale

        private object EvalueazaOperatieRelationala(object stanga, AtomLexical op, object dreapta)
        {
            // Comparatii doar intre numere
            if (!EsteNumar(stanga) || !EsteNumar(dreapta))
            {
                _erori.Add(EroareCompilare.Semantica(
                    op.Linie, op.Coloana,
                    "operatori relationali se aplica doar pe numere"));
                return null;
            }

            double val1 = ConverteLaDouble(stanga);
            double val2 = ConverteLaDouble(dreapta);

            return op.Tip switch
            {
                TipAtomLexical.MaiMic => val1 < val2,
                TipAtomLexical.MaiMare => val1 > val2,
                TipAtomLexical.MaiMicEgal => val1 <= val2,
                TipAtomLexical.MaiMareEgal => val1 >= val2,
                TipAtomLexical.EgalEgal => Math.Abs(val1 - val2) < EPSILON,
                TipAtomLexical.Diferit => Math.Abs(val1 - val2) >= EPSILON,
                _ => null
            };
        }

        #endregion

        #region Metode Helper

        private bool EsteNumar(object valoare)
        {
            return valoare is int || valoare is double;
        }

        private double ConverteLaDouble(object valoare)
        {
            return valoare switch
            {
                int i => i,
                double d => d,
                _ => 0
            };
        }

        private bool EsteAdevarat(object valoare)
        {
            if (valoare is bool b)
                return b;

            // Pentru numere: 0 = false, altceva = true
            if (valoare is int i)
                return i != 0;

            if (valoare is double d)
                return Math.Abs(d) >= EPSILON;

            return false;
        }

        private object ConvertesteLaTip(object valoare, TipDat tip, int linie, int coloana)
        {
            if (valoare == null)
                return null;

            return tip switch
            {
                TipDat.Int when valoare is int => valoare,
                TipDat.Int when valoare is double d => (int)d,
                TipDat.Double when valoare is double => valoare,
                TipDat.Double when valoare is int i => (double)i,
                TipDat.String when valoare is string => valoare,
                _ => ReturneazaEroareConversie(valoare, tip, linie, coloana)
            };
        }

        private object ReturneazaEroareConversie(object valoare, TipDat tip, int linie, int coloana)
        {
            _erori.Add(EroareCompilare.Semantica(
                linie, coloana,
                $"nu se poate converti {valoare?.GetType().Name ?? "null"} la {tip}"));
            return null;
        }

        private string FormateazaValoare(object valoare)
        {
            if (valoare is string str)
                return $"\"{str}\"";

            if (valoare is bool b)
                return b ? "true" : "false";

            return valoare?.ToString() ?? "null";
        }

        #endregion
    }
}
