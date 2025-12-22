using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CompilatorLFT.Models;
using CompilatorLFT.Utils;

namespace CompilatorLFT.Core
{
    /// <summary>
    /// Analizator lexical care transforma textul sursa in stream de tokeni.
    /// </summary>
    /// <remarks>
    /// Referinta: Dragon Book, Cap. 3 - Lexical Analysis
    /// </remarks>
    public class Lexer
    {
        #region Campuri private

        private readonly string _text;
        private int _pozitie;
        private int _linie;
        private int _coloana;
        private readonly List<EroareCompilare> _erori;

        // Regex pentru validare identificatori
        private static readonly Regex RegexIdentificator = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");

        // Cuvinte cheie
        private static readonly Dictionary<string, TipAtomLexical> CuvinteChei = new Dictionary<string, TipAtomLexical>
        {
            { "int", TipAtomLexical.CuvantCheieInt },
            { "double", TipAtomLexical.CuvantCheieDouble },
            { "string", TipAtomLexical.CuvantCheieString },
            { "for", TipAtomLexical.CuvantCheieFor },
            { "while", TipAtomLexical.CuvantCheieWhile },
            { "if", TipAtomLexical.CuvantCheieIf },
            { "else", TipAtomLexical.CuvantCheieElse }
        };

        #endregion

        #region Proprietati

        /// <summary>Caracterul curent din text.</summary>
        private char CaracterCurent => _pozitie < _text.Length ? _text[_pozitie] : '\0';

        /// <summary>Caracterul urmator din text.</summary>
        private char CaracterUrmator => _pozitie + 1 < _text.Length ? _text[_pozitie + 1] : '\0';

        /// <summary>Lista de erori lexicale.</summary>
        public IReadOnlyList<EroareCompilare> Erori => _erori;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializeaza un nou lexer pentru textul dat.
        /// </summary>
        /// <param name="text">Textul sursa de analizat</param>
        public Lexer(string text)
        {
            _text = text ?? string.Empty;
            _pozitie = 0;
            _linie = 1;
            _coloana = 1;
            _erori = new List<EroareCompilare>();
        }

        #endregion

        #region Metode publice

        /// <summary>
        /// Tokenizeaza intregul text si returneaza lista de tokeni.
        /// </summary>
        /// <returns>Lista de atomi lexicali</returns>
        public List<AtomLexical> Tokenizeaza()
        {
            var tokeni = new List<AtomLexical>();

            while (CaracterCurent != '\0')
            {
                var token = UrmatorulToken();

                // Skip spatii si linii noi
                if (token.Tip != TipAtomLexical.Spatiu &&
                    token.Tip != TipAtomLexical.LinieNoua)
                {
                    tokeni.Add(token);
                }
            }

            // Adauga terminator
            tokeni.Add(AtomLexical.Eof(_linie, _coloana, _pozitie));

            return tokeni;
        }

        #endregion

        #region Metode private - Tokenizare

        /// <summary>
        /// Obtine urmatorul token din stream.
        /// </summary>
        private AtomLexical UrmatorulToken()
        {
            // SPATII SI LINII NOI
            if (char.IsWhiteSpace(CaracterCurent))
            {
                return TokenizeazaSpatiu();
            }

            // NUMERE
            if (char.IsDigit(CaracterCurent))
            {
                return TokenizeazaNumar();
            }

            // IDENTIFICATORI SI CUVINTE CHEIE
            if (char.IsLetter(CaracterCurent) || CaracterCurent == '_')
            {
                return TokenizeazaIdentificator();
            }

            // STRING LITERALI
            if (CaracterCurent == '"')
            {
                return TokenizeazaString();
            }

            // OPERATORI SI DELIMITATORI
            return TokenizeazaOperatorSauDelimitator();
        }

        /// <summary>
        /// Tokenizeaza un numar (intreg sau zecimal).
        /// </summary>
        private AtomLexical TokenizeazaNumar()
        {
            int start = _pozitie;
            int linieStart = _linie;
            int coloanaStart = _coloana;

            // Citeste cifre
            while (char.IsDigit(CaracterCurent))
            {
                Avanseaza();
            }

            // Verifica pentru punct zecimal
            if (CaracterCurent == '.' && char.IsDigit(CaracterUrmator))
            {
                // Numar zecimal
                Avanseaza(); // Skip '.'

                while (char.IsDigit(CaracterCurent))
                {
                    Avanseaza();
                }

                string text = _text.Substring(start, _pozitie - start);

                if (double.TryParse(text, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double valoare))
                {
                    return AtomLexical.NumarDouble(text, valoare, linieStart, coloanaStart, start);
                }
                else
                {
                    _erori.Add(EroareCompilare.Lexicala(
                        linieStart, coloanaStart,
                        $"numar zecimal invalid '{text}'"));

                    return new AtomLexical(
                        TipAtomLexical.Invalid, text, null,
                        linieStart, coloanaStart, start);
                }
            }
            else
            {
                // Numar intreg
                string text = _text.Substring(start, _pozitie - start);

                if (int.TryParse(text, out int valoare))
                {
                    return AtomLexical.NumarInt(text, valoare, linieStart, coloanaStart, start);
                }
                else
                {
                    _erori.Add(EroareCompilare.Lexicala(
                        linieStart, coloanaStart,
                        $"numar intreg invalid '{text}' (depaseste Int32.MaxValue)"));

                    return new AtomLexical(
                        TipAtomLexical.Invalid, text, null,
                        linieStart, coloanaStart, start);
                }
            }
        }

        /// <summary>
        /// Tokenizeaza un identificator sau cuvant cheie.
        /// </summary>
        private AtomLexical TokenizeazaIdentificator()
        {
            int start = _pozitie;
            int linieStart = _linie;
            int coloanaStart = _coloana;

            // Citeste litere, cifre si underscore
            while (char.IsLetterOrDigit(CaracterCurent) || CaracterCurent == '_')
            {
                Avanseaza();
            }

            string text = _text.Substring(start, _pozitie - start);

            // Verifica daca este cuvant cheie
            if (CuvinteChei.TryGetValue(text, out TipAtomLexical tipCuvantCheie))
            {
                return new AtomLexical(
                    tipCuvantCheie, text, text,
                    linieStart, coloanaStart, start);
            }

            // Verifica validitate identificator
            if (!RegexIdentificator.IsMatch(text))
            {
                _erori.Add(EroareCompilare.Lexicala(
                    linieStart, coloanaStart,
                    $"identificator invalid '{text}'"));

                return new AtomLexical(
                    TipAtomLexical.Invalid, text, null,
                    linieStart, coloanaStart, start);
            }

            return AtomLexical.Id(text, linieStart, coloanaStart, start);
        }

        /// <summary>
        /// Tokenizeaza un string literal.
        /// </summary>
        private AtomLexical TokenizeazaString()
        {
            int start = _pozitie;
            int linieStart = _linie;
            int coloanaStart = _coloana;

            Avanseaza(); // Skip ghilimele deschise

            var sb = new StringBuilder();

            while (CaracterCurent != '"' && CaracterCurent != '\0')
            {
                if (CaracterCurent == '\n')
                {
                    _erori.Add(EroareCompilare.Lexicala(
                        linieStart, coloanaStart,
                        "string literal neinchis (lipseste ghilimele inchise)"));

                    string textPartial = _text.Substring(start, _pozitie - start);
                    return new AtomLexical(
                        TipAtomLexical.Invalid, textPartial, null,
                        linieStart, coloanaStart, start);
                }

                sb.Append(CaracterCurent);
                Avanseaza();
            }

            if (CaracterCurent == '"')
            {
                Avanseaza(); // Skip ghilimele inchise
            }
            else
            {
                _erori.Add(EroareCompilare.Lexicala(
                    linieStart, coloanaStart,
                    "string literal neinchis (lipseste ghilimele inchise)"));

                string textPartial = _text.Substring(start, _pozitie - start);
                return new AtomLexical(
                    TipAtomLexical.Invalid, textPartial, null,
                    linieStart, coloanaStart, start);
            }

            string valoare = sb.ToString();

            return AtomLexical.String(valoare, linieStart, coloanaStart, start);
        }

        /// <summary>
        /// Tokenizeaza un operator sau delimitator.
        /// </summary>
        private AtomLexical TokenizeazaOperatorSauDelimitator()
        {
            int start = _pozitie;
            int linieStart = _linie;
            int coloanaStart = _coloana;
            char c = CaracterCurent;

            // OPERATORI CU 2 CARACTERE (<=, >=, ==, !=)
            if (c == '<' && CaracterUrmator == '=')
            {
                Avanseaza(); Avanseaza();
                return AtomLexical.Operator(
                    TipAtomLexical.MaiMicEgal, "<=",
                    linieStart, coloanaStart, start);
            }
            if (c == '>' && CaracterUrmator == '=')
            {
                Avanseaza(); Avanseaza();
                return AtomLexical.Operator(
                    TipAtomLexical.MaiMareEgal, ">=",
                    linieStart, coloanaStart, start);
            }
            if (c == '=' && CaracterUrmator == '=')
            {
                Avanseaza(); Avanseaza();
                return AtomLexical.Operator(
                    TipAtomLexical.EgalEgal, "==",
                    linieStart, coloanaStart, start);
            }
            if (c == '!' && CaracterUrmator == '=')
            {
                Avanseaza(); Avanseaza();
                return AtomLexical.Operator(
                    TipAtomLexical.Diferit, "!=",
                    linieStart, coloanaStart, start);
            }

            // OPERATORI SI DELIMITATORI CU 1 CARACTER
            TipAtomLexical tip = c switch
            {
                '+' => TipAtomLexical.Plus,
                '-' => TipAtomLexical.Minus,
                '*' => TipAtomLexical.Star,
                '/' => TipAtomLexical.Slash,
                '<' => TipAtomLexical.MaiMic,
                '>' => TipAtomLexical.MaiMare,
                '=' => TipAtomLexical.Egal,
                ';' => TipAtomLexical.PunctVirgula,
                ',' => TipAtomLexical.Virgula,
                '(' => TipAtomLexical.ParantezaDeschisa,
                ')' => TipAtomLexical.ParantezaInchisa,
                '{' => TipAtomLexical.AcoladaDeschisa,
                '}' => TipAtomLexical.AcoladaInchisa,
                _ => TipAtomLexical.Invalid
            };

            if (tip == TipAtomLexical.Invalid)
            {
                _erori.Add(EroareCompilare.Lexicala(
                    linieStart, coloanaStart,
                    $"caracter invalid '{c}'"));
            }

            Avanseaza();

            return AtomLexical.Operator(
                tip, c.ToString(),
                linieStart, coloanaStart, start);
        }

        /// <summary>
        /// Tokenizeaza spatii si linii noi.
        /// </summary>
        private AtomLexical TokenizeazaSpatiu()
        {
            int start = _pozitie;
            int linieStart = _linie;
            int coloanaStart = _coloana;

            bool esteLinieNoua = CaracterCurent == '\n';

            while (char.IsWhiteSpace(CaracterCurent))
            {
                if (CaracterCurent == '\n')
                    esteLinieNoua = true;
                Avanseaza();
            }

            string text = _text.Substring(start, _pozitie - start);
            TipAtomLexical tip = esteLinieNoua ?
                TipAtomLexical.LinieNoua : TipAtomLexical.Spatiu;

            return new AtomLexical(
                tip, text, null,
                linieStart, coloanaStart, start);
        }

        #endregion

        #region Metode helper

        /// <summary>
        /// Avanseaza la urmatorul caracter, actualizand linia si coloana.
        /// </summary>
        private void Avanseaza()
        {
            if (CaracterCurent == '\n')
            {
                _linie++;
                _coloana = 1;
            }
            else
            {
                _coloana++;
            }
            _pozitie++;
        }

        #endregion
    }
}
