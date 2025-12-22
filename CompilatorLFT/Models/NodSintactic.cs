using System;
using System.Collections.Generic;
using CompilatorLFT.Models;

namespace CompilatorLFT.Models
{
    /// <summary>
    /// Clasa abstractă de bază pentru toate nodurile din arborele sintactic abstract (AST).
    /// </summary>
    /// <remarks>
    /// Referință: Dragon Book, Cap. 5 - "Syntax-Directed Translation"
    /// 
    /// Arborele sintactic abstract (AST) este o reprezentare ierarhică a programului
    /// în care:
    /// - Nodurile interne reprezintă operatori sau constructe de limbaj
    /// - Frunzele reprezintă operanzi (literali, variabile)
    /// 
    /// Această clasă folosește Composite Pattern pentru a permite tratarea uniformă
    /// a nodurilor simple (frunze) și nodurilor compuse (cu copii).
    /// </remarks>
    public abstract class NodSintactic
    {
        #region Proprietăți abstracte

        /// <summary>
        /// Tipul nodului sintactic.
        /// </summary>
        public abstract TipAtomLexical Tip { get; }

        #endregion

        #region Metode abstracte

        /// <summary>
        /// Obține toți copiii directi ai acestui nod.
        /// </summary>
        /// <returns>
        /// Enumerare de copii. Pentru frunze (ex: AtomLexical) returnează colecție goală.
        /// </returns>
        /// <remarks>
        /// Această metodă permite traversarea arborelui sintactic folosind
        /// pattern-ul Visitor sau Iterator.
        /// </remarks>
        public abstract IEnumerable<NodSintactic> ObtineCopii();

        #endregion

        #region Metode pentru afișare arbore

        /// <summary>
        /// Afișează arborele sintactic în format ierarhic la consolă.
        /// </summary>
        /// <param name="indentare">Nivel de indentare curent (pentru recursie)</param>
        /// <param name="estUltim">Indică dacă acest nod este ultimul copil al părintelui</param>
        /// <remarks>
        /// Format afișare:
        /// <code>
        /// └──ExpresieBinara
        ///     ├──ExpresieNumerica
        ///     │   └──NumarIntreg 5
        ///     ├──Plus +
        ///     └──ExpresieNumerica
        ///         └──NumarIntreg 3
        /// </code>
        /// </remarks>
        public virtual void AfiseazaArbore(string indentare = "", bool estUltim = true)
        {
            // Prefix pentru linia curentă
            string prefix = estUltim ? "└──" : "├──";
            
            // Afișează tipul nodului
            Console.Write(indentare);
            Console.Write(prefix);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(Tip);
            Console.ResetColor();

            // Pentru atomi lexicali cu valoare, afișează și valoarea
            if (this is AtomLexical atom && atom.Valoare != null)
            {
                Console.Write(" ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                
                // Formatare specială pentru string-uri
                if (atom.Valoare is string str)
                {
                    Console.Write($"\"{str}\"");
                }
                else
                {
                    Console.Write(atom.Valoare);
                }
                
                Console.ResetColor();
            }

            Console.WriteLine();

            // Actualizează indentarea pentru copii
            string indentareNoua = indentare + (estUltim ? "    " : "│   ");

            // Obține copiii
            var copii = ObtineCopii();
            var listaCopii = new List<NodSintactic>(copii);
            
            // Afișează recursiv fiecare copil
            for (int i = 0; i < listaCopii.Count; i++)
            {
                bool estUltimulCopil = (i == listaCopii.Count - 1);
                listaCopii[i].AfiseazaArbore(indentareNoua, estUltimulCopil);
            }
        }

        /// <summary>
        /// Numără numărul total de noduri din arborele cu rădăcina în acest nod.
        /// </summary>
        /// <returns>Numărul de noduri (inclusiv nodul curent)</returns>
        public int NumaraNoduri()
        {
            int count = 1; // Acest nod
            
            foreach (var copil in ObtineCopii())
            {
                count += copil.NumaraNoduri();
            }
            
            return count;
        }

        /// <summary>
        /// Calculează înălțimea arborelui cu rădăcina în acest nod.
        /// </summary>
        /// <returns>Înălțimea (0 pentru frunze)</returns>
        public int CalculeazaInaltime()
        {
            var copii = ObtineCopii();
            if (!copii.GetEnumerator().MoveNext())
                return 0; // Frunză
            
            int inaltimeMaxima = 0;
            foreach (var copil in copii)
            {
                int inaltimeCopil = copil.CalculeazaInaltime();
                if (inaltimeCopil > inaltimeMaxima)
                    inaltimeMaxima = inaltimeCopil;
            }
            
            return inaltimeMaxima + 1;
        }

        /// <summary>
        /// Creează o reprezentare text (S-expression style) a arborelui.
        /// </summary>
        /// <returns>String reprezentând arborele</returns>
        /// <example>
        /// Pentru "3 + 5":
        /// (ExpresieBinara (Numar 3) + (Numar 5))
        /// </example>
        public string ToSExpression()
        {
            var copii = ObtineCopii();
            var listaCopii = new List<NodSintactic>(copii);
            
            if (listaCopii.Count == 0)
            {
                // Frunză
                if (this is AtomLexical atom && atom.Valoare != null)
                {
                    if (atom.Valoare is string str)
                        return $"({atom.Tip} \"{str}\")";
                    else
                        return $"({atom.Tip} {atom.Valoare})";
                }
                return $"({Tip})";
            }
            
            // Nod intern
            string rezultat = $"({Tip}";
            foreach (var copil in listaCopii)
            {
                rezultat += " " + copil.ToSExpression();
            }
            rezultat += ")";
            
            return rezultat;
        }

        #endregion

        #region Metode pentru debugging

        /// <summary>
        /// Returnează reprezentare text simplă pentru debugging.
        /// </summary>
        public override string ToString()
        {
            return $"{Tip} [{NumaraNoduri()} noduri, h={CalculeazaInaltime()}]";
        }

        #endregion
    }
}
