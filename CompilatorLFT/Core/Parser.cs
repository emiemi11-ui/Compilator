using System;
using System.Collections.Generic;
using CompilatorLFT.Models;
using CompilatorLFT.Models.Expresii;
using CompilatorLFT.Models.Instructiuni;
using CompilatorLFT.Utils;

namespace CompilatorLFT.Core
{
    /// <summary>
    /// Analizator sintactic (parser) recursive descent.
    /// </summary>
    /// <remarks>
    /// Referinta: Dragon Book, Cap. 4 - Syntax Analysis
    /// Implementeaza gramatica:
    /// Program := Instructiune*
    /// Instructiune := Declaratie | Atribuire | For | While | If | Bloc | ExpresieStandalone
    /// Expresie := ExpresieRelationala
    /// ExpresieRelationala := Termen (OpRelational Termen)*
    /// Termen := Factor (('+' | '-') Factor)*
    /// Factor := Primar (('*' | '/') Primar)*
    /// Primar := '-' Primar | '(' Expresie ')' | Literal | Identificator
    /// </remarks>
    public class Parser
    {
        #region Campuri private

        private readonly AtomLexical[] _tokeni;
        private int _index;
        private readonly List<EroareCompilare> _erori;
        private readonly TabelSimboluri _tabelSimboluri;

        #endregion

        #region Proprietati

        /// <summary>Tokenul curent.</summary>
        private AtomLexical AtomCurent => _index < _tokeni.Length ?
            _tokeni[_index] : _tokeni[^1];

        /// <summary>Lista de erori de parsare.</summary>
        public IReadOnlyList<EroareCompilare> Erori => _erori;

        /// <summary>Tabelul de simboluri populat.</summary>
        public TabelSimboluri TabelSimboluri => _tabelSimboluri;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializeaza parser-ul cu textul sursa.
        /// </summary>
        /// <param name="text">Textul sursa de parsat</param>
        public Parser(string text)
        {
            var lexer = new Lexer(text);
            var tokeni = lexer.Tokenizeaza();

            _tokeni = tokeni.ToArray();
            _index = 0;
            _erori = new List<EroareCompilare>();
            _tabelSimboluri = new TabelSimboluri();

            // Adauga erorile lexicale
            _erori.AddRange(lexer.Erori);
        }

        #endregion

        #region Metode helper

        /// <summary>
        /// Consuma tokenul curent si avanseaza.
        /// </summary>
        private AtomLexical ConsumaAtom()
        {
            var atom = AtomCurent;
            _index++;
            return atom;
        }

        /// <summary>
        /// Verifica tipul curent si consuma daca se potriveste.
        /// </summary>
        private AtomLexical VerificaTip(TipAtomLexical tipAsteptat)
        {
            if (AtomCurent.Tip == tipAsteptat)
            {
                return ConsumaAtom();
            }

            _erori.Add(EroareCompilare.Sintactica(
                AtomCurent.Linie, AtomCurent.Coloana,
                $"se astepta '{tipAsteptat}' dar s-a gasit '{AtomCurent.Tip}'"));

            return new AtomLexical(
                TipAtomLexical.Invalid, "", null,
                AtomCurent.Linie, AtomCurent.Coloana, 0);
        }

        /// <summary>
        /// Verifica daca atomul curent este de unul din tipurile date.
        /// </summary>
        private bool PrivesteSiUrmator(params TipAtomLexical[] tipuri)
        {
            foreach (var tip in tipuri)
            {
                if (AtomCurent.Tip == tip)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Recupereaza dupa o eroare - avanseaza pana la un punct sigur.
        /// </summary>
        private void RecupereazaDupaEroare()
        {
            while (!PrivesteSiUrmator(
                TipAtomLexical.PunctVirgula,
                TipAtomLexical.AcoladaInchisa,
                TipAtomLexical.Terminator))
            {
                ConsumaAtom();
            }

            if (AtomCurent.Tip == TipAtomLexical.PunctVirgula)
            {
                ConsumaAtom();
            }
        }

        #endregion

        #region Parsing Program

        /// <summary>
        /// Parseaza intregul program.
        /// </summary>
        /// <returns>Arborele sintactic al programului</returns>
        public ProgramComplet ParseazaProgram()
        {
            var instructiuni = new List<Instructiune>();

            while (AtomCurent.Tip != TipAtomLexical.Terminator)
            {
                try
                {
                    var instr = ParseazaInstructiune();
                    if (instr != null)
                    {
                        instructiuni.Add(instr);
                    }
                }
                catch (Exception ex)
                {
                    _erori.Add(EroareCompilare.Sintactica(
                        AtomCurent.Linie, AtomCurent.Coloana,
                        ex.Message));
                    RecupereazaDupaEroare();
                }
            }

            return new ProgramComplet(instructiuni);
        }

        #endregion

        #region Parsing Instructiuni

        /// <summary>
        /// Parseaza o instructiune.
        /// </summary>
        private Instructiune ParseazaInstructiune()
        {
            // Declaratie (int/double/string ...)
            if (PrivesteSiUrmator(
                TipAtomLexical.CuvantCheieInt,
                TipAtomLexical.CuvantCheieDouble,
                TipAtomLexical.CuvantCheieString))
            {
                return ParseazaDeclaratie();
            }

            // For
            if (PrivesteSiUrmator(TipAtomLexical.CuvantCheieFor))
            {
                return ParseazaFor();
            }

            // While
            if (PrivesteSiUrmator(TipAtomLexical.CuvantCheieWhile))
            {
                return ParseazaWhile();
            }

            // If
            if (PrivesteSiUrmator(TipAtomLexical.CuvantCheieIf))
            {
                return ParseazaIf();
            }

            // Bloc
            if (PrivesteSiUrmator(TipAtomLexical.AcoladaDeschisa))
            {
                return ParseazaBloc();
            }

            // Atribuire sau Expresie standalone
            if (PrivesteSiUrmator(TipAtomLexical.Identificator))
            {
                // Salvam pozitia pentru backtracking
                int pozitieInceput = _index;
                var id = ConsumaAtom();

                if (AtomCurent.Tip == TipAtomLexical.Egal)
                {
                    // E atribuire
                    var egal = ConsumaAtom();
                    var expr = ParseazaExpresie();
                    var punctVirgula = VerificaTip(TipAtomLexical.PunctVirgula);

                    // Verificare semantica
                    if (!_tabelSimboluri.Exista(id.Text))
                    {
                        _erori.Add(EroareCompilare.Semantica(
                            id.Linie, id.Coloana,
                            $"variabila '{id.Text}' nu a fost declarata"));
                    }

                    return new InstructiuneAtribuire(id, egal, expr, punctVirgula);
                }
                else
                {
                    // E expresie standalone - backtrack
                    _index = pozitieInceput;
                    var expr = ParseazaExpresie();
                    var punctVirgula = VerificaTip(TipAtomLexical.PunctVirgula);
                    return new InstructiuneExpresie(expr, punctVirgula);
                }
            }

            // Expresie standalone
            var expresie = ParseazaExpresie();
            var pv = VerificaTip(TipAtomLexical.PunctVirgula);
            return new InstructiuneExpresie(expresie, pv);
        }

        /// <summary>
        /// Parseaza o declaratie de variabile.
        /// </summary>
        private InstructiuneDeclaratie ParseazaDeclaratie()
        {
            var tipCuvant = ConsumaAtom();
            var tipDat = TabelSimboluri.ConvertesteLaTipDat(tipCuvant.Tip);

            var declaratii = new List<(AtomLexical, Expresie)>();

            do
            {
                var id = VerificaTip(TipAtomLexical.Identificator);

                // Adauga in tabel simboluri
                _tabelSimboluri.Adauga(id.Text, tipDat, id.Linie, id.Coloana, _erori);

                Expresie expr = null;

                // Initializare?
                if (AtomCurent.Tip == TipAtomLexical.Egal)
                {
                    ConsumaAtom(); // Skip '='
                    expr = ParseazaExpresie();
                }

                declaratii.Add((id, expr));

            } while (AtomCurent.Tip == TipAtomLexical.Virgula && ConsumaAtom() != null);

            var punctVirgula = VerificaTip(TipAtomLexical.PunctVirgula);

            return new InstructiuneDeclaratie(tipCuvant, declaratii, punctVirgula);
        }

        /// <summary>
        /// Parseaza o instructiune FOR.
        /// </summary>
        private InstructiuneFor ParseazaFor()
        {
            var cuvantFor = ConsumaAtom();
            var parantezaDeschisa = VerificaTip(TipAtomLexical.ParantezaDeschisa);

            // Initializare
            Instructiune init = null;
            if (!PrivesteSiUrmator(TipAtomLexical.PunctVirgula))
            {
                init = ParseazaInstructiuneForInit();
            }
            else
            {
                ConsumaAtom(); // Skip ;
            }

            // Conditie
            Expresie conditie = null;
            if (!PrivesteSiUrmator(TipAtomLexical.PunctVirgula))
            {
                conditie = ParseazaExpresie();
            }

            var punctVirgula = VerificaTip(TipAtomLexical.PunctVirgula);

            // Increment
            Instructiune increment = null;
            if (!PrivesteSiUrmator(TipAtomLexical.ParantezaInchisa))
            {
                increment = ParseazaInstructiuneForIncrement();
            }

            var parantezaInchisa = VerificaTip(TipAtomLexical.ParantezaInchisa);

            // Corp
            var corp = ParseazaInstructiune();

            return new InstructiuneFor(
                cuvantFor, parantezaDeschisa,
                init, conditie, punctVirgula, increment,
                parantezaInchisa, corp);
        }

        /// <summary>
        /// Parseaza initializarea din FOR.
        /// </summary>
        private Instructiune ParseazaInstructiuneForInit()
        {
            if (PrivesteSiUrmator(
                TipAtomLexical.CuvantCheieInt,
                TipAtomLexical.CuvantCheieDouble,
                TipAtomLexical.CuvantCheieString))
            {
                return ParseazaDeclaratie();
            }

            // Atribuire
            var id = VerificaTip(TipAtomLexical.Identificator);

            if (!_tabelSimboluri.Exista(id.Text))
            {
                _erori.Add(EroareCompilare.Semantica(
                    id.Linie, id.Coloana,
                    $"variabila '{id.Text}' nu a fost declarata"));
            }

            var egal = VerificaTip(TipAtomLexical.Egal);
            var expr = ParseazaExpresie();
            var punctVirgula = VerificaTip(TipAtomLexical.PunctVirgula);

            return new InstructiuneAtribuire(id, egal, expr, punctVirgula);
        }

        /// <summary>
        /// Parseaza incrementul din FOR (fara ;).
        /// </summary>
        private Instructiune ParseazaInstructiuneForIncrement()
        {
            var id = VerificaTip(TipAtomLexical.Identificator);

            if (!_tabelSimboluri.Exista(id.Text))
            {
                _erori.Add(EroareCompilare.Semantica(
                    id.Linie, id.Coloana,
                    $"variabila '{id.Text}' nu a fost declarata"));
            }

            var egal = VerificaTip(TipAtomLexical.Egal);
            var expr = ParseazaExpresie();

            // Fara punct si virgula aici!
            return new InstructiuneAtribuire(id, egal, expr, null);
        }

        /// <summary>
        /// Parseaza o instructiune WHILE.
        /// </summary>
        private InstructiuneWhile ParseazaWhile()
        {
            var cuvantWhile = ConsumaAtom();
            var parantezaDeschisa = VerificaTip(TipAtomLexical.ParantezaDeschisa);
            var conditie = ParseazaExpresie();
            var parantezaInchisa = VerificaTip(TipAtomLexical.ParantezaInchisa);
            var corp = ParseazaInstructiune();

            return new InstructiuneWhile(
                cuvantWhile, parantezaDeschisa,
                conditie, parantezaInchisa, corp);
        }

        /// <summary>
        /// Parseaza o instructiune IF.
        /// </summary>
        private InstructiuneIf ParseazaIf()
        {
            var cuvantIf = ConsumaAtom();
            var parantezaDeschisa = VerificaTip(TipAtomLexical.ParantezaDeschisa);
            var conditie = ParseazaExpresie();
            var parantezaInchisa = VerificaTip(TipAtomLexical.ParantezaInchisa);
            var corpAdevarat = ParseazaInstructiune();

            AtomLexical cuvantElse = null;
            Instructiune corpFals = null;

            if (AtomCurent.Tip == TipAtomLexical.CuvantCheieElse)
            {
                cuvantElse = ConsumaAtom();
                corpFals = ParseazaInstructiune();
            }

            return new InstructiuneIf(
                cuvantIf, parantezaDeschisa,
                conditie, parantezaInchisa, corpAdevarat,
                cuvantElse, corpFals);
        }

        /// <summary>
        /// Parseaza un bloc de instructiuni.
        /// </summary>
        private Bloc ParseazaBloc()
        {
            var acoladaDeschisa = ConsumaAtom();
            var instructiuni = new List<Instructiune>();

            while (!PrivesteSiUrmator(
                TipAtomLexical.AcoladaInchisa,
                TipAtomLexical.Terminator))
            {
                var instr = ParseazaInstructiune();
                if (instr != null)
                {
                    instructiuni.Add(instr);
                }
            }

            var acoladaInchisa = VerificaTip(TipAtomLexical.AcoladaInchisa);

            return new Bloc(acoladaDeschisa, instructiuni, acoladaInchisa);
        }

        #endregion

        #region Parsing Expresii

        /// <summary>
        /// Parseaza o expresie.
        /// </summary>
        private Expresie ParseazaExpresie()
        {
            return ParseazaExpresieRelationala();
        }

        /// <summary>
        /// Parseaza o expresie relationala.
        /// Precedenta: &lt;, &gt;, &lt;=, &gt;=, ==, !=
        /// </summary>
        private Expresie ParseazaExpresieRelationala()
        {
            var stanga = ParseazaTermen();

            while (PrivesteSiUrmator(
                TipAtomLexical.MaiMic,
                TipAtomLexical.MaiMare,
                TipAtomLexical.MaiMicEgal,
                TipAtomLexical.MaiMareEgal,
                TipAtomLexical.EgalEgal,
                TipAtomLexical.Diferit))
            {
                var op = ConsumaAtom();
                var dreapta = ParseazaTermen();
                stanga = new ExpresieBinara(stanga, op, dreapta);
            }

            return stanga;
        }

        /// <summary>
        /// Parseaza un termen (adunare/scadere).
        /// </summary>
        private Expresie ParseazaTermen()
        {
            var stanga = ParseazaFactor();

            while (PrivesteSiUrmator(
                TipAtomLexical.Plus,
                TipAtomLexical.Minus))
            {
                var op = ConsumaAtom();
                var dreapta = ParseazaFactor();
                stanga = new ExpresieBinara(stanga, op, dreapta);
            }

            return stanga;
        }

        /// <summary>
        /// Parseaza un factor (inmultire/impartire).
        /// </summary>
        private Expresie ParseazaFactor()
        {
            var stanga = ParseazaPrimar();

            while (PrivesteSiUrmator(
                TipAtomLexical.Star,
                TipAtomLexical.Slash))
            {
                var op = ConsumaAtom();
                var dreapta = ParseazaPrimar();
                stanga = new ExpresieBinara(stanga, op, dreapta);
            }

            return stanga;
        }

        /// <summary>
        /// Parseaza o expresie primara.
        /// </summary>
        private Expresie ParseazaPrimar()
        {
            // Minus unar
            if (AtomCurent.Tip == TipAtomLexical.Minus)
            {
                var op = ConsumaAtom();
                var operand = ParseazaPrimar();
                return new ExpresieUnara(op, operand);
            }

            // Plus unar - EROARE conform cerintelor!
            if (AtomCurent.Tip == TipAtomLexical.Plus)
            {
                _erori.Add(EroareCompilare.Lexicala(
                    AtomCurent.Linie, AtomCurent.Coloana,
                    "plus unar nu este permis"));
                ConsumaAtom();
                return ParseazaPrimar();
            }

            // Paranteze
            if (AtomCurent.Tip == TipAtomLexical.ParantezaDeschisa)
            {
                var parantezaDeschisa = ConsumaAtom();
                var expr = ParseazaExpresie();
                var parantezaInchisa = VerificaTip(TipAtomLexical.ParantezaInchisa);
                return new ExpresieCuParanteze(parantezaDeschisa, expr, parantezaInchisa);
            }

            // Numar intreg
            if (AtomCurent.Tip == TipAtomLexical.NumarIntreg)
            {
                var atom = ConsumaAtom();
                return new ExpresieNumerica(atom);
            }

            // Numar zecimal
            if (AtomCurent.Tip == TipAtomLexical.NumarZecimal)
            {
                var atom = ConsumaAtom();
                return new ExpresieNumerica(atom);
            }

            // String literal
            if (AtomCurent.Tip == TipAtomLexical.StringLiteral)
            {
                var atom = ConsumaAtom();
                return new ExpresieString(atom);
            }

            // Identificator (variabila)
            if (AtomCurent.Tip == TipAtomLexical.Identificator)
            {
                var id = ConsumaAtom();

                // Verificare semantica
                if (!_tabelSimboluri.Exista(id.Text))
                {
                    _erori.Add(EroareCompilare.Semantica(
                        id.Linie, id.Coloana,
                        $"variabila '{id.Text}' nu a fost declarata"));
                }

                return new ExpresieIdentificator(id);
            }

            // Eroare
            _erori.Add(EroareCompilare.Sintactica(
                AtomCurent.Linie, AtomCurent.Coloana,
                $"expresie invalida - token neasteptat '{AtomCurent.Tip}'"));

            // Returneaza un placeholder pentru a continua parsarea
            var placeholder = new AtomLexical(
                TipAtomLexical.NumarIntreg, "0", 0,
                AtomCurent.Linie, AtomCurent.Coloana, _index);

            if (AtomCurent.Tip != TipAtomLexical.Terminator)
            {
                ConsumaAtom();
            }

            return new ExpresieNumerica(placeholder);
        }

        #endregion
    }
}
