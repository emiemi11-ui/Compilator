using System;
using System.IO;

namespace CompilatorLFT.Utils
{
    /// <summary>
    /// Utility for reading source files.
    /// </summary>
    public static class FileReader
    {
        /// <summary>
        /// Reads the content of a text file.
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>File content or null if error</returns>
        public static string ReadFile(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: File path cannot be empty!");
                    Console.ResetColor();
                    return null;
                }

                if (!File.Exists(filePath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: File '{filePath}' does not exist!");
                    Console.ResetColor();
                    return null;
                }

                return File.ReadAllText(filePath);
            }
            catch (IOException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error reading file: {ex.Message}");
                Console.ResetColor();
                return null;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: You do not have permission for file: {ex.Message}");
                Console.ResetColor();
                return null;
            }
        }

        /// <summary>
        /// Checks if a file exists.
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>True if file exists</returns>
        public static bool FileExists(string filePath)
        {
            return !string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath);
        }

        /// <summary>
        /// Displays file content with line numbers.
        /// </summary>
        /// <param name="content">Content to display</param>
        public static void DisplayWithLineNumbers(string content)
        {
            if (string.IsNullOrEmpty(content))
                return;

            var lines = content.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{i + 1,3} | ");
                Console.ResetColor();
                Console.WriteLine(lines[i].TrimEnd('\r'));
            }
        }
    }
}
