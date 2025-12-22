using System;
using System.IO;

namespace CompilatorLFT.Utils
{
    /// <summary>
    /// Utilitar pentru citirea fisierelor sursa.
    /// </summary>
    public static class CititorFisier
    {
        /// <summary>
        /// Citeste continutul unui fisier text.
        /// </summary>
        /// <param name="caleFisier">Calea catre fisier</param>
        /// <returns>Continutul fisierului sau null daca eroare</returns>
        public static string CitesteFisier(string caleFisier)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(caleFisier))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Eroare: Calea fisierului nu poate fi goala!");
                    Console.ResetColor();
                    return null;
                }

                if (!File.Exists(caleFisier))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Eroare: Fisierul '{caleFisier}' nu exista!");
                    Console.ResetColor();
                    return null;
                }

                return File.ReadAllText(caleFisier);
            }
            catch (IOException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Eroare la citirea fisierului: {ex.Message}");
                Console.ResetColor();
                return null;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Eroare: Nu aveti permisiuni pentru fisier: {ex.Message}");
                Console.ResetColor();
                return null;
            }
        }

        /// <summary>
        /// Verifica daca un fisier exista.
        /// </summary>
        /// <param name="caleFisier">Calea catre fisier</param>
        /// <returns>True daca fisierul exista</returns>
        public static bool ExistaFisier(string caleFisier)
        {
            return !string.IsNullOrWhiteSpace(caleFisier) && File.Exists(caleFisier);
        }

        /// <summary>
        /// Afiseaza continutul fisierului cu numere de linii.
        /// </summary>
        /// <param name="continut">Continutul de afisat</param>
        public static void AfiseazaCuNumereLinii(string continut)
        {
            if (string.IsNullOrEmpty(continut))
                return;

            var linii = continut.Split('\n');

            for (int i = 0; i < linii.Length; i++)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{i + 1,3} | ");
                Console.ResetColor();
                Console.WriteLine(linii[i].TrimEnd('\r'));
            }
        }
    }
}
