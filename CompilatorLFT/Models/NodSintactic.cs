using System;
using System.Collections.Generic;
using CompilatorLFT.Models;

namespace CompilatorLFT.Models
{
    /// <summary>
    /// Abstract base class for all nodes in the abstract syntax tree (AST).
    /// </summary>
    /// <remarks>
    /// Reference: Dragon Book, Ch. 5 - "Syntax-Directed Translation"
    ///
    /// The abstract syntax tree (AST) is a hierarchical representation of the program
    /// where:
    /// - Internal nodes represent operators or language constructs
    /// - Leaves represent operands (literals, variables)
    ///
    /// This class uses the Composite Pattern to allow uniform treatment
    /// of simple nodes (leaves) and composite nodes (with children).
    /// </remarks>
    public abstract class SyntaxNode
    {
        #region Abstract Properties

        /// <summary>
        /// The type of the syntax node.
        /// </summary>
        public abstract TokenType Type { get; }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Gets all direct children of this node.
        /// </summary>
        /// <returns>
        /// Enumeration of children. For leaves (e.g.: Token) returns empty collection.
        /// </returns>
        /// <remarks>
        /// This method allows traversal of the syntax tree using
        /// the Visitor or Iterator pattern.
        /// </remarks>
        public abstract IEnumerable<SyntaxNode> GetChildren();

        #endregion

        #region Tree Display Methods

        /// <summary>
        /// Displays the syntax tree in hierarchical format to console.
        /// </summary>
        /// <param name="indent">Current indentation level (for recursion)</param>
        /// <param name="isLast">Indicates if this node is the last child of the parent</param>
        /// <remarks>
        /// Display format:
        /// <code>
        /// └──BinaryExpression
        ///     ├──NumericExpression
        ///     │   └──IntegerNumber 5
        ///     ├──Plus +
        ///     └──NumericExpression
        ///         └──IntegerNumber 3
        /// </code>
        /// </remarks>
        public virtual void DisplayTree(string indent = "", bool isLast = true)
        {
            // Prefix for current line
            string prefix = isLast ? "└──" : "├──";

            // Display node type
            Console.Write(indent);
            Console.Write(prefix);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(Type);
            Console.ResetColor();

            // For lexical tokens with value, display the value too
            if (this is Token token && token.Value != null)
            {
                Console.Write(" ");
                Console.ForegroundColor = ConsoleColor.Yellow;

                // Special formatting for strings
                if (token.Value is string str)
                {
                    Console.Write($"\"{str}\"");
                }
                else
                {
                    Console.Write(token.Value);
                }

                Console.ResetColor();
            }

            Console.WriteLine();

            // Update indentation for children
            string newIndent = indent + (isLast ? "    " : "│   ");

            // Get children
            var children = GetChildren();
            var childList = new List<SyntaxNode>(children);

            // Display each child recursively
            for (int i = 0; i < childList.Count; i++)
            {
                bool isLastChild = (i == childList.Count - 1);
                childList[i].DisplayTree(newIndent, isLastChild);
            }
        }

        /// <summary>
        /// Counts the total number of nodes in the tree rooted at this node.
        /// </summary>
        /// <returns>Number of nodes (including this node)</returns>
        public int CountNodes()
        {
            int count = 1; // This node

            foreach (var child in GetChildren())
            {
                count += child.CountNodes();
            }

            return count;
        }

        /// <summary>
        /// Calculates the height of the tree rooted at this node.
        /// </summary>
        /// <returns>Height (0 for leaves)</returns>
        public int CalculateHeight()
        {
            var children = GetChildren();
            if (!children.GetEnumerator().MoveNext())
                return 0; // Leaf

            int maxHeight = 0;
            foreach (var child in children)
            {
                int childHeight = child.CalculateHeight();
                if (childHeight > maxHeight)
                    maxHeight = childHeight;
            }

            return maxHeight + 1;
        }

        /// <summary>
        /// Creates a text representation (S-expression style) of the tree.
        /// </summary>
        /// <returns>String representing the tree</returns>
        /// <example>
        /// For "3 + 5":
        /// (BinaryExpression (Number 3) + (Number 5))
        /// </example>
        public string ToSExpression()
        {
            var children = GetChildren();
            var childList = new List<SyntaxNode>(children);

            if (childList.Count == 0)
            {
                // Leaf
                if (this is Token token && token.Value != null)
                {
                    if (token.Value is string str)
                        return $"({token.Type} \"{str}\")";
                    else
                        return $"({token.Type} {token.Value})";
                }
                return $"({Type})";
            }

            // Internal node
            string result = $"({Type}";
            foreach (var child in childList)
            {
                result += " " + child.ToSExpression();
            }
            result += ")";

            return result;
        }

        #endregion

        #region Debugging Methods

        /// <summary>
        /// Returns simple text representation for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"{Type} [{CountNodes()} nodes, h={CalculateHeight()}]";
        }

        #endregion
    }
}
