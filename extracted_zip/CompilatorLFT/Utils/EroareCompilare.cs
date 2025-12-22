using System;
using CompilatorLFT.Models;

namespace CompilatorLFT.Utils
{
    /// <summary>
    /// Reprezintă o eroare de compilare cu informații detaliate despre locație și tip.
    /// </summary>
    /// <remarks>
    /// Format obligatoriu: "la linia X, coloana Y: eroare [tip] - [mesaj]"
    /// Referință: Cerințele proiectului, punctul 6
    /// </remarks>
    public class EroareCompilare
    {
        #region Proprietăți

        /// <summary>
        /// Numărul liniei unde a apărut eroarea (indexare de la 1).
        /// </summary>
        public int Linie { get; }

        /// <summary>
        /// Numărul coloanei unde a apărut eroarea (indexare de la 1).
        /// </summary>
        public int Coloana { get; }

        /// <summary>
        /// Tipul erorii: lexicală, sintactică sau semantică.
        /// </summary>
        public TipEroare Tip { get; }

        /// <summary>
        /// Mesajul descriptiv al erorii.
        /// </summary>
        public string Mesaj { get; }

        /// <summary>
        /// Text sursă care a cauzat eroarea (opțional, pentru context).
        /// </summary>
        public string TextSursa { get; }

        #endregion

        #region Constructori

        /// <summary>
        /// Inițializează o nouă instanță a clasei <see cref="EroareCompilare"/>.
        /// </summary>
        /// <param name="linie">Numărul liniei (indexare de la 1)</param>
        /// <param name="coloana">Numărul coloanei (indexare de la 1)</param>
        /// <param name="tip">Tipul erorii</param>
        /// <param name="mesaj">Mesajul descriptiv</param>
        /// <param name="textSursa">Text sursă opțional pentru context</param>
        /// <exception cref="ArgumentException">
        /// Dacă linia sau coloana sunt mai mici decât 1, sau dacă mesajul este gol
        /// </exception>
        public EroareCompilare(int linie, int coloana, TipEroare tip, string mesaj, string textSursa = "")
        {
            if (linie < 1)
                throw new ArgumentException("Numărul liniei trebuie să fie mai mare sau egal cu 1", nameof(linie));
            
            if (coloana < 1)
                throw new ArgumentException("Numărul coloanei trebuie să fie mai mare sau egal cu 1", nameof(coloana));
            
            if (string.IsNullOrWhiteSpace(mesaj))
                throw new ArgumentException("Mesajul erorii nu poate fi gol", nameof(mesaj));

            Linie = linie;
            Coloana = coloana;
            Tip = tip;
            Mesaj = mesaj;
            TextSursa = textSursa ?? string.Empty;
        }

        #endregion

        #region Metode publice

        /// <summary>
        /// Returnează reprezentarea textuală a erorii în formatul obligatoriu.
        /// </summary>
        /// <returns>
        /// String în formatul: "la linia X, coloana Y: eroare [tip] - [mesaj]"
        /// </returns>
        /// <example>
        /// <code>
        /// var eroare = new EroareCompilare(5, 12, TipEroare.Semantica, "variabila 'x' nu a fost declarată");
        /// Console.WriteLine(eroare.ToString());
        /// // Output: la linia 5, coloana 12: eroare semantică - variabila 'x' nu a fost declarată
        /// </code>
        /// </example>
        public override string ToString()
        {
            string tipStr = ObtineDenumireTip();
            return $"la linia {Linie}, coloana {Coloana}: eroare {tipStr} - {Mesaj}";
        }

        /// <summary>
        /// Returnează reprezentarea textuală cu context adițional (text sursă).
        /// </summary>
        /// <returns>String cu eroarea și textul sursă care a cauzat-o</returns>
        public string ToStringCuContext()
        {
            string reprezentare = ToString();
            
            if (!string.IsNullOrWhiteSpace(TextSursa))
            {
                reprezentare += Environment.NewLine;
                reprezentare += $"  Context: {TextSursa}";
                
                // Adaugă indicator vizual pentru poziția exactă
                if (Coloana <= TextSursa.Length)
                {
                    reprezentare += Environment.NewLine;
                    reprezentare += "  " + new string(' ', Coloana - 1) + "^";
                }
            }
            
            return reprezentare;
        }

        /// <summary>
        /// Compară două erori pentru egalitate bazată pe linie, coloană și mesaj.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is EroareCompilare alta)
            {
                return Linie == alta.Linie && 
                       Coloana == alta.Coloana && 
                       Tip == alta.Tip && 
                       Mesaj == alta.Mesaj;
            }
            return false;
        }

        /// <summary>
        /// Returnează hash code pentru eroare.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Linie, Coloana, Tip, Mesaj);
        }

        #endregion

        #region Metode helper private

        /// <summary>
        /// Convertește tipul erorii în denumire pentru afișare.
        /// </summary>
        /// <returns>Denumirea tipului în limba română</returns>
        private string ObtineDenumireTip()
        {
            return Tip switch
            {
                TipEroare.Lexicala => "lexicală",
                TipEroare.Sintactica => "sintactică",
                TipEroare.Semantica => "semantică",
                _ => "necunoscută"
            };
        }

        #endregion

        #region Factory Methods (pentru ușurință)

        /// <summary>
        /// Creează o eroare lexicală.
        /// </summary>
        public static EroareCompilare Lexicala(int linie, int coloana, string mesaj, string textSursa = "")
        {
            return new EroareCompilare(linie, coloana, TipEroare.Lexicala, mesaj, textSursa);
        }

        /// <summary>
        /// Creează o eroare sintactică.
        /// </summary>
        public static EroareCompilare Sintactica(int linie, int coloana, string mesaj, string textSursa = "")
        {
            return new EroareCompilare(linie, coloana, TipEroare.Sintactica, mesaj, textSursa);
        }

        /// <summary>
        /// Creează o eroare semantică.
        /// </summary>
        public static EroareCompilare Semantica(int linie, int coloana, string mesaj, string textSursa = "")
        {
            return new EroareCompilare(linie, coloana, TipEroare.Semantica, mesaj, textSursa);
        }

        #endregion
    }
}
