using System;
using System.Collections.Generic;
using CompilatorLFT.Models;

namespace CompilatorLFT.Models.Expresii
{
    /// <summary>
    /// Clasa abstractă de bază pentru toate expresiile.
    /// </summary>
    /// <remarks>
    /// O expresie este un construct care evaluează la o valoare.
    /// Exemple: 5, a+b, "hello", (x*y)+z
    /// </remarks>
    public abstract class Expresie : NodSintactic
    {
    }

    #region Expresii Simple

    /// <summary>
    /// Expresie pentru un literal numeric (întreg sau zecimal).
    /// </summary>
    /// <example>
    /// 42, 3.14, -17
    /// </example>
    public sealed class ExpresieNumerica : Expresie
    {
        /// <summary>Atomul lexical care conține valoarea numerică.</summary>
        public AtomLexical Numar { get; }

        public override TipAtomLexical Tip => TipAtomLexical.ExpresieNumerica;

        public ExpresieNumerica(AtomLexical numar)
        {
            Numar = numar ?? throw new ArgumentNullException(nameof(numar));
            
            if (numar.Tip != TipAtomLexical.NumarIntreg && 
                numar.Tip != TipAtomLexical.NumarZecimal)
            {
                throw new ArgumentException(
                    "Atomul trebuie să fie NumarIntreg sau NumarZecimal", 
                    nameof(numar));
            }
        }

        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            yield return Numar;
        }
    }

    /// <summary>
    /// Expresie pentru un literal string.
    /// </summary>
    /// <example>
    /// "hello", "test 123", ""
    /// </example>
    public sealed class ExpresieString : Expresie
    {
        /// <summary>Atomul lexical care conține valoarea string.</summary>
        public AtomLexical ValoareString { get; }

        public override TipAtomLexical Tip => TipAtomLexical.ExpresieString;

        public ExpresieString(AtomLexical valoareString)
        {
            ValoareString = valoareString ?? throw new ArgumentNullException(nameof(valoareString));
            
            if (valoareString.Tip != TipAtomLexical.StringLiteral)
            {
                throw new ArgumentException(
                    "Atomul trebuie să fie StringLiteral", 
                    nameof(valoareString));
            }
        }

        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            yield return ValoareString;
        }
    }

    /// <summary>
    /// Expresie pentru un identificator (variabilă).
    /// </summary>
    /// <example>
    /// a, suma, _temp, var123
    /// </example>
    public sealed class ExpresieIdentificator : Expresie
    {
        /// <summary>Atomul lexical care conține numele variabilei.</summary>
        public AtomLexical Identificator { get; }

        public override TipAtomLexical Tip => TipAtomLexical.ExpresieIdentificator;

        public ExpresieIdentificator(AtomLexical identificator)
        {
            Identificator = identificator ?? throw new ArgumentNullException(nameof(identificator));
            
            if (identificator.Tip != TipAtomLexical.Identificator)
            {
                throw new ArgumentException(
                    "Atomul trebuie să fie Identificator", 
                    nameof(identificator));
            }
        }

        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            yield return Identificator;
        }
    }

    #endregion

    #region Expresii Compuse

    /// <summary>
    /// Expresie binară (cu doi operanzi și un operator).
    /// </summary>
    /// <remarks>
    /// Operatori suportați:
    /// - Aritmetici: +, -, *, /
    /// - Relaționali: &lt;, &gt;, &lt;=, &gt;=, ==, !=
    /// 
    /// Exemple: a+b, 3*5, x&lt;=y
    /// </remarks>
    public sealed class ExpresieBinara : Expresie
    {
        /// <summary>Expresia din stânga operatorului.</summary>
        public Expresie Stanga { get; }

        /// <summary>Operatorul binar (+, -, *, /, &lt;, &gt;, etc.).</summary>
        public AtomLexical Operator { get; }

        /// <summary>Expresia din dreapta operatorului.</summary>
        public Expresie Dreapta { get; }

        public override TipAtomLexical Tip => TipAtomLexical.ExpresieBinara;

        public ExpresieBinara(Expresie stanga, AtomLexical operatorAtom, Expresie dreapta)
        {
            Stanga = stanga ?? throw new ArgumentNullException(nameof(stanga));
            Operator = operatorAtom ?? throw new ArgumentNullException(nameof(operatorAtom));
            Dreapta = dreapta ?? throw new ArgumentNullException(nameof(dreapta));

            // Validare: operator trebuie să fie aritmetic sau relațional
            if (!operatorAtom.EsteOperatorAritmetic() && !operatorAtom.EsteOperatorRelational())
            {
                throw new ArgumentException(
                    "Operatorul trebuie să fie aritmetic sau relațional",
                    nameof(operatorAtom));
            }
        }

        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            yield return Stanga;
            yield return Operator;
            yield return Dreapta;
        }
    }

    /// <summary>
    /// Expresie unară (un operator și un operand).
    /// </summary>
    /// <remarks>
    /// Operatori suportați:
    /// - Minus unar: -a, -(x+y)
    /// 
    /// NOTĂ: Plus unar (+a) NU este suportat conform cerințelor.
    /// </remarks>
    public sealed class ExpresieUnara : Expresie
    {
        /// <summary>Operatorul unar (doar -).</summary>
        public AtomLexical Operator { get; }

        /// <summary>Expresia la care se aplică operatorul.</summary>
        public Expresie Operand { get; }

        public override TipAtomLexical Tip => TipAtomLexical.ExpresieUnara;

        public ExpresieUnara(AtomLexical operatorAtom, Expresie operand)
        {
            Operator = operatorAtom ?? throw new ArgumentNullException(nameof(operatorAtom));
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));

            // Validare: doar minus unar este permis
            if (operatorAtom.Tip != TipAtomLexical.Minus)
            {
                throw new ArgumentException(
                    "Doar operatorul minus (-) este suportat ca operator unar",
                    nameof(operatorAtom));
            }
        }

        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            yield return Operator;
            yield return Operand;
        }
    }

    /// <summary>
    /// Expresie cu paranteze pentru forțarea precedenței.
    /// </summary>
    /// <example>
    /// (a + b), (3 * (x + y))
    /// </example>
    public sealed class ExpresieCuParanteze : Expresie
    {
        /// <summary>Paranteza deschisă '('.</summary>
        public AtomLexical ParantezaDeschisa { get; }

        /// <summary>Expresia din interiorul parantezelor.</summary>
        public Expresie Expresie { get; }

        /// <summary>Paranteza închisă ')'.</summary>
        public AtomLexical ParantezaInchisa { get; }

        public override TipAtomLexical Tip => TipAtomLexical.ExpresieCuParanteze;

        public ExpresieCuParanteze(
            AtomLexical parantezaDeschisa,
            Expresie expresie,
            AtomLexical parantezaInchisa)
        {
            ParantezaDeschisa = parantezaDeschisa ?? 
                throw new ArgumentNullException(nameof(parantezaDeschisa));
            Expresie = expresie ?? 
                throw new ArgumentNullException(nameof(expresie));
            ParantezaInchisa = parantezaInchisa ?? 
                throw new ArgumentNullException(nameof(parantezaInchisa));

            if (parantezaDeschisa.Tip != TipAtomLexical.ParantezaDeschisa)
            {
                throw new ArgumentException(
                    "Primul atom trebuie să fie ParantezaDeschisa",
                    nameof(parantezaDeschisa));
            }

            if (parantezaInchisa.Tip != TipAtomLexical.ParantezaInchisa)
            {
                throw new ArgumentException(
                    "Al treilea atom trebuie să fie ParantezaInchisa",
                    nameof(parantezaInchisa));
            }
        }

        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            yield return ParantezaDeschisa;
            yield return Expresie;
            yield return ParantezaInchisa;
        }
    }

    #endregion
}
