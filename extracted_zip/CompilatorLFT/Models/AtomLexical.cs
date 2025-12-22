using System;
using System.Collections.Generic;
using System.Linq;
using CompilatorLFT.Models;

namespace CompilatorLFT.Models
{
    /// <summary>
    /// Reprezintă un atom lexical (token) cu informații complete despre poziție și valoare.
    /// </summary>
    /// <remarks>
    /// Referință: Dragon Book, Cap. 3 - "Tokens, Patterns, and Lexemes"
    /// Un token este o pereche (token-name, attribute-value) unde:
    /// - token-name = tipul atomului lexical
    /// - attribute-value = valoarea asociată (număr, string, identificator)
    /// </remarks>
    public class AtomLexical : NodSintactic
    {
        #region Proprietăți

        /// <summary>
        /// Tipul atomului lexical.
        /// </summary>
        public override TipAtomLexical Tip { get; }

        /// <summary>
        /// Textul original din sursă (lexemul).
        /// </summary>
        /// <example>
        /// Pentru "int a = 123;":
        /// - "int" → lexem pentru CuvantCheieInt
        /// - "a" → lexem pentru Identificator
        /// - "123" → lexem pentru NumarIntreg
        /// </example>
        public string Text { get; }

        /// <summary>
        /// Valoarea parsată a atomului (null pentru operatori/delimitatori).
        /// </summary>
        /// <remarks>
        /// Tipuri posibile:
        /// - int pentru NumarIntreg
        /// - double pentru NumarZecimal
        /// - string pentru StringLiteral și Identificator
        /// - null pentru operatori și delimitatori
        /// </remarks>
        public object Valoare { get; }

        /// <summary>
        /// Numărul liniei unde apare atomul (indexare de la 1).
        /// </summary>
        public int Linie { get; }

        /// <summary>
        /// Numărul coloanei unde începe atomul (indexare de la 1).
        /// </summary>
        public int Coloana { get; }

        /// <summary>
        /// Poziția absolută în textul sursă (indexare de la 0).
        /// </summary>
        public int PozitieAbsoluta { get; }

        /// <summary>
        /// Lungimea atomului în caractere.
        /// </summary>
        public int Lungime => Text?.Length ?? 0;

        #endregion

        #region Constructori

        /// <summary>
        /// Inițializează un nou atom lexical cu informații complete de poziție.
        /// </summary>
        /// <param name="tip">Tipul atomului lexical</param>
        /// <param name="text">Textul original (lexemul)</param>
        /// <param name="valoare">Valoarea parsată (poate fi null)</param>
        /// <param name="linie">Numărul liniei (de la 1)</param>
        /// <param name="coloana">Numărul coloanei (de la 1)</param>
        /// <param name="pozitieAbsoluta">Poziția absolută în text (de la 0)</param>
        /// <exception cref="ArgumentException">
        /// Dacă linia sau coloana sunt invalide
        /// </exception>
        public AtomLexical(
            TipAtomLexical tip,
            string text,
            object valoare,
            int linie,
            int coloana,
            int pozitieAbsoluta)
        {
            if (linie < 1)
                throw new ArgumentException("Linia trebuie să fie >= 1", nameof(linie));
            
            if (coloana < 1)
                throw new ArgumentException("Coloana trebuie să fie >= 1", nameof(coloana));
            
            if (pozitieAbsoluta < 0)
                throw new ArgumentException("Poziția absolută trebuie să fie >= 0", nameof(pozitieAbsoluta));

            Tip = tip;
            Text = text ?? string.Empty;
            Valoare = valoare;
            Linie = linie;
            Coloana = coloana;
            PozitieAbsoluta = pozitieAbsoluta;
        }

        #endregion

        #region Implementare NodSintactic

        /// <summary>
        /// Atomul lexical este frunză în arborele sintactic (nu are copii).
        /// </summary>
        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            return Enumerable.Empty<NodSintactic>();
        }

        #endregion

        #region Metode publice

        /// <summary>
        /// Returnează reprezentarea textuală a atomului pentru debugging.
        /// </summary>
        /// <returns>String cu tip, text și valoare</returns>
        public override string ToString()
        {
            string valoareStr = Valoare != null ? $" = {Valoare}" : "";
            return $"{Tip} '{Text}'{valoareStr} @ ({Linie}:{Coloana})";
        }

        /// <summary>
        /// Verifică dacă atomul este un cuvânt cheie pentru tip de date.
        /// </summary>
        /// <returns>True dacă este int, double sau string</returns>
        public bool EsteCuvantCheieTip()
        {
            return Tip == TipAtomLexical.CuvantCheieInt ||
                   Tip == TipAtomLexical.CuvantCheieDouble ||
                   Tip == TipAtomLexical.CuvantCheieString;
        }

        /// <summary>
        /// Verifică dacă atomul este un operator aritmetic.
        /// </summary>
        /// <returns>True dacă este +, -, *, /</returns>
        public bool EsteOperatorAritmetic()
        {
            return Tip == TipAtomLexical.Plus ||
                   Tip == TipAtomLexical.Minus ||
                   Tip == TipAtomLexical.Star ||
                   Tip == TipAtomLexical.Slash;
        }

        /// <summary>
        /// Verifică dacă atomul este un operator relațional.
        /// </summary>
        /// <returns>True dacă este &lt;, &gt;, &lt;=, &gt;=, ==, !=</returns>
        public bool EsteOperatorRelational()
        {
            return Tip == TipAtomLexical.MaiMic ||
                   Tip == TipAtomLexical.MaiMare ||
                   Tip == TipAtomLexical.MaiMicEgal ||
                   Tip == TipAtomLexical.MaiMareEgal ||
                   Tip == TipAtomLexical.EgalEgal ||
                   Tip == TipAtomLexical.Diferit;
        }

        /// <summary>
        /// Verifică dacă atomul este un literal (număr sau string).
        /// </summary>
        /// <returns>True dacă este NumarIntreg, NumarZecimal sau StringLiteral</returns>
        public bool EsteLiteral()
        {
            return Tip == TipAtomLexical.NumarIntreg ||
                   Tip == TipAtomLexical.NumarZecimal ||
                   Tip == TipAtomLexical.StringLiteral;
        }

        /// <summary>
        /// Obține tipul de date asociat acestui atom lexical.
        /// </summary>
        /// <returns>Tipul de date sau Necunoscut</returns>
        public TipDat ObtineTipDat()
        {
            return Tip switch
            {
                TipAtomLexical.NumarIntreg => TipDat.Int,
                TipAtomLexical.NumarZecimal => TipDat.Double,
                TipAtomLexical.StringLiteral => TipDat.String,
                TipAtomLexical.CuvantCheieInt => TipDat.Int,
                TipAtomLexical.CuvantCheieDouble => TipDat.Double,
                TipAtomLexical.CuvantCheieString => TipDat.String,
                _ => TipDat.Necunoscut
            };
        }

        /// <summary>
        /// Compară doi atomi lexicali pentru egalitate.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is AtomLexical alt)
            {
                return Tip == alt.Tip &&
                       Text == alt.Text &&
                       Equals(Valoare, alt.Valoare) &&
                       Linie == alt.Linie &&
                       Coloana == alt.Coloana;
            }
            return false;
        }

        /// <summary>
        /// Returnează hash code pentru atom.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Tip, Text, Valoare, Linie, Coloana);
        }

        #endregion

        #region Factory Methods (pentru ușurință de creare)

        /// <summary>
        /// Creează un atom lexical pentru un număr întreg.
        /// </summary>
        public static AtomLexical NumarInt(string text, int valoare, int linie, int coloana, int pozitie)
        {
            return new AtomLexical(TipAtomLexical.NumarIntreg, text, valoare, linie, coloana, pozitie);
        }

        /// <summary>
        /// Creează un atom lexical pentru un număr zecimal.
        /// </summary>
        public static AtomLexical NumarDouble(string text, double valoare, int linie, int coloana, int pozitie)
        {
            return new AtomLexical(TipAtomLexical.NumarZecimal, text, valoare, linie, coloana, pozitie);
        }

        /// <summary>
        /// Creează un atom lexical pentru un string literal.
        /// </summary>
        public static AtomLexical String(string text, int linie, int coloana, int pozitie)
        {
            return new AtomLexical(TipAtomLexical.StringLiteral, text, text, linie, coloana, pozitie);
        }

        /// <summary>
        /// Creează un atom lexical pentru un identificator.
        /// </summary>
        public static AtomLexical Id(string nume, int linie, int coloana, int pozitie)
        {
            return new AtomLexical(TipAtomLexical.Identificator, nume, nume, linie, coloana, pozitie);
        }

        /// <summary>
        /// Creează un atom lexical pentru un operator sau delimitator (fără valoare).
        /// </summary>
        public static AtomLexical Operator(TipAtomLexical tip, string text, int linie, int coloana, int pozitie)
        {
            return new AtomLexical(tip, text, null, linie, coloana, pozitie);
        }

        /// <summary>
        /// Creează un atom lexical terminator.
        /// </summary>
        public static AtomLexical Eof(int linie, int coloana, int pozitie)
        {
            return new AtomLexical(TipAtomLexical.Terminator, "\0", null, linie, coloana, pozitie);
        }

        #endregion
    }
}
