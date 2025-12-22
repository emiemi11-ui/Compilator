using System;
using CompilatorLFT.Models;

namespace CompilatorLFT.Models
{
    /// <summary>
    /// Reprezintă o variabilă în tabelul de simboluri.
    /// </summary>
    /// <remarks>
    /// Referință: Dragon Book, Cap. 2.7 - "Symbol Tables"
    /// 
    /// Tabelul de simboluri stochează informații despre fiecare identificator declarat:
    /// - Nume (identificator)
    /// - Tip (int, double, string)
    /// - Valoare curentă
    /// - Dacă a fost inițializată
    /// - Locația declarației (pentru erori)
    /// </remarks>
    public class Variabila
    {
        #region Proprietăți

        /// <summary>
        /// Numele variabilei (identificatorul).
        /// </summary>
        public string Nume { get; }

        /// <summary>
        /// Tipul variabilei (int, double, string).
        /// </summary>
        public TipDat Tip { get; }

        /// <summary>
        /// Valoarea curentă a variabilei.
        /// </summary>
        /// <remarks>
        /// Poate fi:
        /// - int pentru TipDat.Int
        /// - double pentru TipDat.Double
        /// - string pentru TipDat.String
        /// - null dacă neinițializată
        /// </remarks>
        public object Valoare { get; set; }

        /// <summary>
        /// Indică dacă variabila a fost inițializată cu o valoare.
        /// </summary>
        public bool EsteInitializata { get; set; }

        /// <summary>
        /// Linia unde a fost declarată variabila (pentru erori).
        /// </summary>
        public int LinieDeclaratie { get; }

        /// <summary>
        /// Coloana unde a fost declarată variabila (pentru erori).
        /// </summary>
        public int ColoanaDeclaratie { get; }

        #endregion

        #region Constructori

        /// <summary>
        /// Inițializează o nouă variabilă nedeclarată.
        /// </summary>
        /// <param name="nume">Numele variabilei</param>
        /// <param name="tip">Tipul variabilei</param>
        /// <param name="linie">Linia declarației</param>
        /// <param name="coloana">Coloana declarației</param>
        /// <exception cref="ArgumentException">
        /// Dacă numele este gol sau tipul este necunoscut
        /// </exception>
        public Variabila(string nume, TipDat tip, int linie, int coloana)
        {
            if (string.IsNullOrWhiteSpace(nume))
                throw new ArgumentException("Numele variabilei nu poate fi gol", nameof(nume));

            if (tip == TipDat.Necunoscut)
                throw new ArgumentException("Tipul variabilei trebuie să fie valid", nameof(tip));

            Nume = nume;
            Tip = tip;
            Valoare = null;
            EsteInitializata = false;
            LinieDeclaratie = linie;
            ColoanaDeclaratie = coloana;
        }

        /// <summary>
        /// Inițializează o nouă variabilă cu valoare inițială.
        /// </summary>
        /// <param name="nume">Numele variabilei</param>
        /// <param name="tip">Tipul variabilei</param>
        /// <param name="valoare">Valoarea inițială</param>
        /// <param name="linie">Linia declarației</param>
        /// <param name="coloana">Coloana declarației</param>
        public Variabila(string nume, TipDat tip, object valoare, int linie, int coloana)
            : this(nume, tip, linie, coloana)
        {
            SeteazaValoare(valoare);
        }

        #endregion

        #region Metode publice

        /// <summary>
        /// Setează valoarea variabilei cu validare de tip.
        /// </summary>
        /// <param name="valoare">Noua valoare</param>
        /// <exception cref="ArgumentException">
        /// Dacă valoarea nu corespunde tipului variabilei
        /// </exception>
        public void SeteazaValoare(object valoare)
        {
            // Validare tip
            if (!ValidareaTipului(valoare))
            {
                throw new ArgumentException(
                    $"Valoarea de tip '{valoare?.GetType().Name ?? "null"}' " +
                    $"nu corespunde tipului variabilei '{Tip}'",
                    nameof(valoare));
            }

            Valoare = valoare;
            EsteInitializata = true;
        }

        /// <summary>
        /// Verifică dacă o valoare este compatibilă cu tipul variabilei.
        /// </summary>
        /// <param name="valoare">Valoarea de verificat</param>
        /// <returns>True dacă compatibil, False altfel</returns>
        public bool ValidareaTipului(object valoare)
        {
            if (valoare == null)
                return false;

            return Tip switch
            {
                TipDat.Int => valoare is int,
                TipDat.Double => valoare is double || valoare is int, // Int poate fi promovat la double
                TipDat.String => valoare is string,
                _ => false
            };
        }

        /// <summary>
        /// Obține valoarea variabilei cu cast la tipul specificat.
        /// </summary>
        /// <typeparam name="T">Tipul dorit</typeparam>
        /// <returns>Valoarea castată</returns>
        /// <exception cref="InvalidOperationException">
        /// Dacă variabila nu este inițializată
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// Dacă valoarea nu poate fi castată la tipul dorit
        /// </exception>
        public T ObtineValoare<T>()
        {
            if (!EsteInitializata)
            {
                throw new InvalidOperationException(
                    $"Variabila '{Nume}' nu a fost inițializată");
            }

            if (Valoare is T valoareCastata)
                return valoareCastata;

            throw new InvalidCastException(
                $"Valoarea variabilei '{Nume}' nu poate fi castată la {typeof(T).Name}");
        }

        /// <summary>
        /// Resetează variabila la starea neinițializată.
        /// </summary>
        public void Reseteaza()
        {
            Valoare = null;
            EsteInitializata = false;
        }

        /// <summary>
        /// Returnează reprezentare text pentru debugging.
        /// </summary>
        public override string ToString()
        {
            string stare = EsteInitializata ? $" = {FormatareValoare()}" : " (neinițializată)";
            return $"{Tip} {Nume}{stare}";
        }

        /// <summary>
        /// Formatare valoare pentru afișare.
        /// </summary>
        private string FormatareValoare()
        {
            if (Valoare == null)
                return "null";

            if (Valoare is string str)
                return $"\"{str}\"";

            return Valoare.ToString();
        }

        /// <summary>
        /// Compară două variabile pentru egalitate (bazat pe nume).
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is Variabila alta)
            {
                return Nume == alta.Nume;
            }
            return false;
        }

        /// <summary>
        /// Returnează hash code bazat pe nume.
        /// </summary>
        public override int GetHashCode()
        {
            return Nume.GetHashCode();
        }

        #endregion
    }
}
