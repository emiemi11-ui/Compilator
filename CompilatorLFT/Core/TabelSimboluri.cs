using System;
using System.Collections.Generic;
using CompilatorLFT.Models;
using CompilatorLFT.Utils;

namespace CompilatorLFT.Core
{
    /// <summary>
    /// Gestioneaza tabelul de simboluri pentru variabile.
    /// </summary>
    /// <remarks>
    /// Referinta: Dragon Book, Cap. 2.7 - Symbol Tables
    /// </remarks>
    public class TabelSimboluri
    {
        #region Campuri private

        private readonly Dictionary<string, Variabila> _variabile;

        #endregion

        #region Proprietati

        /// <summary>Numarul de variabile din tabel.</summary>
        public int NumarVariabile => _variabile.Count;

        /// <summary>Toate variabilele din tabel.</summary>
        public IEnumerable<Variabila> Variabile => _variabile.Values;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializeaza un nou tabel de simboluri gol.
        /// </summary>
        public TabelSimboluri()
        {
            _variabile = new Dictionary<string, Variabila>();
        }

        #endregion

        #region Metode publice

        /// <summary>
        /// Adauga o noua variabila in tabel.
        /// </summary>
        /// <param name="nume">Numele variabilei</param>
        /// <param name="tip">Tipul variabilei</param>
        /// <param name="linie">Linia declaratiei</param>
        /// <param name="coloana">Coloana declaratiei</param>
        /// <param name="erori">Lista de erori pentru raportare</param>
        /// <returns>True daca adaugarea a reusit, False daca exista deja</returns>
        public bool Adauga(string nume, TipDat tip, int linie, int coloana, List<EroareCompilare> erori)
        {
            if (_variabile.ContainsKey(nume))
            {
                erori.Add(EroareCompilare.Semantica(
                    linie, coloana,
                    $"declaratie duplicata pentru variabila '{nume}'"));
                return false;
            }

            var variabila = new Variabila(nume, tip, linie, coloana);
            _variabile[nume] = variabila;
            return true;
        }

        /// <summary>
        /// Verifica daca o variabila exista in tabel.
        /// </summary>
        /// <param name="nume">Numele variabilei</param>
        /// <returns>True daca exista, False altfel</returns>
        public bool Exista(string nume)
        {
            return _variabile.ContainsKey(nume);
        }

        /// <summary>
        /// Obtine o variabila din tabel.
        /// </summary>
        /// <param name="nume">Numele variabilei</param>
        /// <returns>Variabila sau null daca nu exista</returns>
        public Variabila Obtine(string nume)
        {
            return _variabile.TryGetValue(nume, out var variabila) ? variabila : null;
        }

        /// <summary>
        /// Seteaza valoarea unei variabile cu validare de tip.
        /// </summary>
        /// <param name="nume">Numele variabilei</param>
        /// <param name="valoare">Valoarea de setat</param>
        /// <param name="linie">Linia pentru raportare erori</param>
        /// <param name="coloana">Coloana pentru raportare erori</param>
        /// <param name="erori">Lista de erori</param>
        /// <returns>True daca setarea a reusit</returns>
        public bool SeteazaValoare(string nume, object valoare, int linie, int coloana, List<EroareCompilare> erori)
        {
            if (!_variabile.ContainsKey(nume))
            {
                erori.Add(EroareCompilare.Semantica(
                    linie, coloana,
                    $"variabila '{nume}' nu a fost declarata"));
                return false;
            }

            var variabila = _variabile[nume];

            // Verificare compatibilitate tip
            if (!VerificaTipCompatibil(variabila.Tip, valoare))
            {
                string tipValoare = valoare?.GetType().Name ?? "null";
                erori.Add(EroareCompilare.Semantica(
                    linie, coloana,
                    $"incompatibilitate tipuri: nu se poate atribui {tipValoare} la variabila de tip {variabila.Tip}"));
                return false;
            }

            // Conversie daca e necesar
            object valoareConvertita = ConvertesteLaTip(valoare, variabila.Tip);

            variabila.SeteazaValoare(valoareConvertita);
            return true;
        }

        /// <summary>
        /// Obtine valoarea unei variabile cu verificare initializare.
        /// </summary>
        /// <param name="nume">Numele variabilei</param>
        /// <param name="linie">Linia pentru raportare erori</param>
        /// <param name="coloana">Coloana pentru raportare erori</param>
        /// <param name="erori">Lista de erori</param>
        /// <returns>Valoarea sau null daca eroare</returns>
        public object ObtineValoare(string nume, int linie, int coloana, List<EroareCompilare> erori)
        {
            if (!_variabile.ContainsKey(nume))
            {
                erori.Add(EroareCompilare.Semantica(
                    linie, coloana,
                    $"variabila '{nume}' nu a fost declarata"));
                return null;
            }

            var variabila = _variabile[nume];

            if (!variabila.EsteInitializata)
            {
                erori.Add(EroareCompilare.Semantica(
                    linie, coloana,
                    $"variabila '{nume}' folosita inainte de initializare"));
                return null;
            }

            return variabila.Valoare;
        }

        /// <summary>
        /// Afiseaza toate variabilele la consola.
        /// </summary>
        public void AfiseazaVariabile()
        {
            Console.WriteLine("\n=== TABEL SIMBOLURI ===");

            if (_variabile.Count == 0)
            {
                Console.WriteLine("(gol)");
                return;
            }

            foreach (var variabila in _variabile.Values)
            {
                Console.WriteLine(variabila.ToString());
            }
        }

        /// <summary>
        /// Reseteaza tabelul de simboluri.
        /// </summary>
        public void Reseteaza()
        {
            _variabile.Clear();
        }

        #endregion

        #region Metode helper

        /// <summary>
        /// Verifica daca o valoare este compatibila cu un tip.
        /// </summary>
        private bool VerificaTipCompatibil(TipDat tip, object valoare)
        {
            if (valoare == null)
                return false;

            return tip switch
            {
                TipDat.Int => valoare is int || valoare is double,
                TipDat.Double => valoare is int || valoare is double,
                TipDat.String => valoare is string,
                _ => false
            };
        }

        /// <summary>
        /// Converteste o valoare la tipul specificat.
        /// </summary>
        private object ConvertesteLaTip(object valoare, TipDat tip)
        {
            if (valoare == null)
                return null;

            return tip switch
            {
                TipDat.Int when valoare is int i => i,
                TipDat.Int when valoare is double d => (int)d,
                TipDat.Double when valoare is double d => d,
                TipDat.Double when valoare is int i => (double)i,
                TipDat.String when valoare is string s => s,
                _ => valoare
            };
        }

        /// <summary>
        /// Converteste un tip atom lexical la tip de date.
        /// </summary>
        public static TipDat ConvertesteLaTipDat(TipAtomLexical tip)
        {
            return tip switch
            {
                TipAtomLexical.CuvantCheieInt => TipDat.Int,
                TipAtomLexical.CuvantCheieDouble => TipDat.Double,
                TipAtomLexical.CuvantCheieString => TipDat.String,
                _ => TipDat.Necunoscut
            };
        }

        #endregion
    }
}
