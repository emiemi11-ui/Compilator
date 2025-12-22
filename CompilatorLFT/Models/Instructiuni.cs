using System;
using System.Collections.Generic;
using System.Linq;
using CompilatorLFT.Models.Expresii;

namespace CompilatorLFT.Models.Instructiuni
{
    /// <summary>
    /// Clasa abstracta de baza pentru toate instructiunile.
    /// </summary>
    /// <remarks>
    /// Referinta: Dragon Book, Cap. 5 - Syntax-Directed Translation
    /// </remarks>
    public abstract class Instructiune : NodSintactic
    {
    }

    /// <summary>
    /// Instructiune de declaratie de variabile.
    /// Sintaxa: int a, b=5, c;
    ///          double x=3.14;
    ///          string s="test";
    /// </summary>
    public sealed class InstructiuneDeclaratie : Instructiune
    {
        /// <summary>Cuvantul cheie pentru tip (int/double/string).</summary>
        public AtomLexical TipCuvantCheie { get; }

        /// <summary>
        /// Lista declaratii: (identificator, expresie_initializare_optionala)
        /// Daca expresie este null -> declaratie fara initializare
        /// </summary>
        public List<(AtomLexical identificator, Expresie expresieInit)> Declaratii { get; }

        /// <summary>Punct si virgula final.</summary>
        public AtomLexical PunctVirgula { get; }

        public override TipAtomLexical Tip => TipAtomLexical.InstructiuneDeclaratie;

        public InstructiuneDeclaratie(
            AtomLexical tipCuvantCheie,
            List<(AtomLexical, Expresie)> declaratii,
            AtomLexical punctVirgula)
        {
            TipCuvantCheie = tipCuvantCheie ?? throw new ArgumentNullException(nameof(tipCuvantCheie));
            Declaratii = declaratii ?? throw new ArgumentNullException(nameof(declaratii));
            PunctVirgula = punctVirgula ?? throw new ArgumentNullException(nameof(punctVirgula));

            if (!tipCuvantCheie.EsteCuvantCheieTip())
                throw new ArgumentException("Trebuie sa fie cuvant cheie pentru tip (int/double/string)");

            if (declaratii.Count == 0)
                throw new ArgumentException("Trebuie sa existe cel putin o declaratie");
        }

        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            yield return TipCuvantCheie;

            foreach (var (id, expr) in Declaratii)
            {
                yield return id;
                if (expr != null)
                    yield return expr;
            }

            yield return PunctVirgula;
        }
    }

    /// <summary>
    /// Instructiune de atribuire.
    /// Sintaxa: a = expresie;
    /// </summary>
    public sealed class InstructiuneAtribuire : Instructiune
    {
        /// <summary>Identificatorul variabilei.</summary>
        public AtomLexical Identificator { get; }

        /// <summary>Operatorul de atribuire '='.</summary>
        public AtomLexical OperatorEgal { get; }

        /// <summary>Expresia care se evalueaza si se atribuie.</summary>
        public Expresie Expresie { get; }

        /// <summary>Punct si virgula final (poate fi null in for).</summary>
        public AtomLexical PunctVirgula { get; }

        public override TipAtomLexical Tip => TipAtomLexical.InstructiuneAtribuire;

        public InstructiuneAtribuire(
            AtomLexical identificator,
            AtomLexical operatorEgal,
            Expresie expresie,
            AtomLexical punctVirgula)
        {
            Identificator = identificator ?? throw new ArgumentNullException(nameof(identificator));
            OperatorEgal = operatorEgal ?? throw new ArgumentNullException(nameof(operatorEgal));
            Expresie = expresie ?? throw new ArgumentNullException(nameof(expresie));
            PunctVirgula = punctVirgula; // Poate fi null in for

            if (identificator.Tip != TipAtomLexical.Identificator)
                throw new ArgumentException("Trebuie sa fie identificator");

            if (operatorEgal.Tip != TipAtomLexical.Egal)
                throw new ArgumentException("Trebuie sa fie operator '='");
        }

        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            yield return Identificator;
            yield return OperatorEgal;
            yield return Expresie;
            if (PunctVirgula != null)
                yield return PunctVirgula;
        }
    }

    /// <summary>
    /// Instructiune care consta doar dintr-o expresie.
    /// Sintaxa: expresie;
    /// </summary>
    public sealed class InstructiuneExpresie : Instructiune
    {
        /// <summary>Expresia evaluata.</summary>
        public Expresie Expresie { get; }

        /// <summary>Punct si virgula final.</summary>
        public AtomLexical PunctVirgula { get; }

        public override TipAtomLexical Tip => TipAtomLexical.InstructiuneExpresie;

        public InstructiuneExpresie(Expresie expresie, AtomLexical punctVirgula)
        {
            Expresie = expresie ?? throw new ArgumentNullException(nameof(expresie));
            PunctVirgula = punctVirgula ?? throw new ArgumentNullException(nameof(punctVirgula));
        }

        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            yield return Expresie;
            yield return PunctVirgula;
        }
    }

    /// <summary>
    /// Instructiune FOR.
    /// Sintaxa: for (init; conditie; increment) instructiune
    /// </summary>
    public sealed class InstructiuneFor : Instructiune
    {
        public AtomLexical CuvantCheieFor { get; }
        public AtomLexical ParantezaDeschisa { get; }

        /// <summary>Instructiune initializare (ex: int i=0).</summary>
        public Instructiune Initializare { get; }

        /// <summary>Expresie conditie (ex: i&lt;10).</summary>
        public Expresie Conditie { get; }

        public AtomLexical PunctVirgula { get; }

        /// <summary>Instructiune increment (ex: i=i+1).</summary>
        public Instructiune Increment { get; }

        public AtomLexical ParantezaInchisa { get; }

        /// <summary>Corp buclei (poate fi instructiune simpla sau bloc).</summary>
        public Instructiune Corp { get; }

        public override TipAtomLexical Tip => TipAtomLexical.InstructiuneFor;

        public InstructiuneFor(
            AtomLexical cuvantCheieFor,
            AtomLexical parantezaDeschisa,
            Instructiune initializare,
            Expresie conditie,
            AtomLexical punctVirgula,
            Instructiune increment,
            AtomLexical parantezaInchisa,
            Instructiune corp)
        {
            CuvantCheieFor = cuvantCheieFor ?? throw new ArgumentNullException(nameof(cuvantCheieFor));
            ParantezaDeschisa = parantezaDeschisa ?? throw new ArgumentNullException(nameof(parantezaDeschisa));
            Initializare = initializare;
            Conditie = conditie;
            PunctVirgula = punctVirgula ?? throw new ArgumentNullException(nameof(punctVirgula));
            Increment = increment;
            ParantezaInchisa = parantezaInchisa ?? throw new ArgumentNullException(nameof(parantezaInchisa));
            Corp = corp ?? throw new ArgumentNullException(nameof(corp));
        }

        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            yield return CuvantCheieFor;
            yield return ParantezaDeschisa;
            if (Initializare != null) yield return Initializare;
            if (Conditie != null) yield return Conditie;
            yield return PunctVirgula;
            if (Increment != null) yield return Increment;
            yield return ParantezaInchisa;
            yield return Corp;
        }
    }

    /// <summary>
    /// Instructiune WHILE.
    /// Sintaxa: while (conditie) instructiune
    /// </summary>
    public sealed class InstructiuneWhile : Instructiune
    {
        public AtomLexical CuvantCheieWhile { get; }
        public AtomLexical ParantezaDeschisa { get; }
        public Expresie Conditie { get; }
        public AtomLexical ParantezaInchisa { get; }
        public Instructiune Corp { get; }

        public override TipAtomLexical Tip => TipAtomLexical.InstructiuneWhile;

        public InstructiuneWhile(
            AtomLexical cuvantCheieWhile,
            AtomLexical parantezaDeschisa,
            Expresie conditie,
            AtomLexical parantezaInchisa,
            Instructiune corp)
        {
            CuvantCheieWhile = cuvantCheieWhile ?? throw new ArgumentNullException(nameof(cuvantCheieWhile));
            ParantezaDeschisa = parantezaDeschisa ?? throw new ArgumentNullException(nameof(parantezaDeschisa));
            Conditie = conditie ?? throw new ArgumentNullException(nameof(conditie));
            ParantezaInchisa = parantezaInchisa ?? throw new ArgumentNullException(nameof(parantezaInchisa));
            Corp = corp ?? throw new ArgumentNullException(nameof(corp));
        }

        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            yield return CuvantCheieWhile;
            yield return ParantezaDeschisa;
            yield return Conditie;
            yield return ParantezaInchisa;
            yield return Corp;
        }
    }

    /// <summary>
    /// Instructiune IF (cu else optional).
    /// Sintaxa: if (conditie) instructiune [else instructiune]
    /// </summary>
    public sealed class InstructiuneIf : Instructiune
    {
        public AtomLexical CuvantCheieIf { get; }
        public AtomLexical ParantezaDeschisa { get; }
        public Expresie Conditie { get; }
        public AtomLexical ParantezaInchisa { get; }
        public Instructiune CorpAdevarat { get; }

        // Optional
        public AtomLexical CuvantCheieElse { get; }
        public Instructiune CorpFals { get; }

        public override TipAtomLexical Tip => TipAtomLexical.InstructiuneIf;

        public InstructiuneIf(
            AtomLexical cuvantCheieIf,
            AtomLexical parantezaDeschisa,
            Expresie conditie,
            AtomLexical parantezaInchisa,
            Instructiune corpAdevarat,
            AtomLexical cuvantCheieElse = null,
            Instructiune corpFals = null)
        {
            CuvantCheieIf = cuvantCheieIf ?? throw new ArgumentNullException(nameof(cuvantCheieIf));
            ParantezaDeschisa = parantezaDeschisa ?? throw new ArgumentNullException(nameof(parantezaDeschisa));
            Conditie = conditie ?? throw new ArgumentNullException(nameof(conditie));
            ParantezaInchisa = parantezaInchisa ?? throw new ArgumentNullException(nameof(parantezaInchisa));
            CorpAdevarat = corpAdevarat ?? throw new ArgumentNullException(nameof(corpAdevarat));
            CuvantCheieElse = cuvantCheieElse;
            CorpFals = corpFals;
        }

        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            yield return CuvantCheieIf;
            yield return ParantezaDeschisa;
            yield return Conditie;
            yield return ParantezaInchisa;
            yield return CorpAdevarat;

            if (CuvantCheieElse != null)
            {
                yield return CuvantCheieElse;
                yield return CorpFals;
            }
        }
    }

    /// <summary>
    /// Bloc de instructiuni intre acolade.
    /// Sintaxa: { instructiune1; instructiune2; ... }
    /// </summary>
    public sealed class Bloc : Instructiune
    {
        public AtomLexical AcoladaDeschisa { get; }
        public List<Instructiune> Instructiuni { get; }
        public AtomLexical AcoladaInchisa { get; }

        public override TipAtomLexical Tip => TipAtomLexical.Bloc;

        public Bloc(
            AtomLexical acoladaDeschisa,
            List<Instructiune> instructiuni,
            AtomLexical acoladaInchisa)
        {
            AcoladaDeschisa = acoladaDeschisa ?? throw new ArgumentNullException(nameof(acoladaDeschisa));
            Instructiuni = instructiuni ?? new List<Instructiune>();
            AcoladaInchisa = acoladaInchisa ?? throw new ArgumentNullException(nameof(acoladaInchisa));
        }

        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            yield return AcoladaDeschisa;

            foreach (var instr in Instructiuni)
                yield return instr;

            yield return AcoladaInchisa;
        }
    }

    /// <summary>
    /// Nod radacina - programul complet.
    /// Contine lista tuturor instructiunilor de la nivel superior.
    /// </summary>
    public sealed class ProgramComplet : NodSintactic
    {
        public List<Instructiune> Instructiuni { get; }

        public override TipAtomLexical Tip => TipAtomLexical.Program;

        public ProgramComplet(List<Instructiune> instructiuni)
        {
            Instructiuni = instructiuni ?? new List<Instructiune>();
        }

        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            return Instructiuni.Cast<NodSintactic>();
        }
    }
}
