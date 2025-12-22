# 📘 DOCUMENTAȚIE MASTER - COMPILATOR LFT
## Ghid Complet pentru Implementare cu Claude Code

---

## 📋 TABLE OF CONTENTS

1. [Arhitectura Completă](#1-arhitectura-completa)
2. [Specificații Detaliate Fiecare Componentă](#2-specificatii-detaliate)
3. [Implementare Lexer](#3-implementare-lexer)
4. [Implementare Parser](#4-implementare-parser)
5. [Implementare Tabel Simboluri](#5-implementare-tabel-simboluri)
6. [Implementare Evaluator](#6-implementare-evaluator)
7. [Program Principal](#7-program-principal)
8. [Suite Teste](#8-suite-teste)
9. [Exemple Execuție](#9-exemple-executie)
10. [Debugging & Troubleshooting](#10-debugging)

---

## 1. ARHITECTURA COMPLETĂ

### 1.1 Viziune de Ansamblu

```
┌─────────────────────────────────────────────────────────────┐
│                      COMPILATOR LFT                          │
│                                                              │
│  ┌──────────┐    ┌──────────┐    ┌──────────┐             │
│  │  LEXER   │───▶│  PARSER  │───▶│EVALUATOR │             │
│  │(Scanning)│    │(Parsing) │    │(Exec)    │             │
│  └────┬─────┘    └────┬─────┘    └────┬─────┘             │
│       │               │               │                     │
│       │               │               │                     │
│   ┌───▼────┐      ┌───▼────┐     ┌───▼────┐               │
│   │ Tokens │      │  AST   │     │Results │               │
│   └────────┘      └────────┘     └────────┘               │
│                       │                                     │
│                   ┌───▼─────────┐                          │
│                   │TabelSimboluri│                          │
│                   └─────────────┘                          │
└─────────────────────────────────────────────────────────────┘

FLUX DE DATE:
1. Text sursă → LEXER → Stream tokeni
2. Stream tokeni → PARSER → AST + Tabel Simboluri
3. AST + Tabel → EVALUATOR → Rezultate
4. Erori colectate în fiecare fază
```

### 1.2 Structura Directoare

```
CompilatorLFT/
│
├── Models/                          # Modele de date (COMPLETAT 100%)
│   ├── TipAtomLexical.cs           ✅ 80+ tipuri, 3 enumerări
│   ├── AtomLexical.cs              ✅ Token cu tracking linie/coloană
│   ├── NodSintactic.cs             ✅ Bază AST, Composite Pattern
│   ├── Expresii.cs                 ✅ 6 clase: Numerica, String, Id, Binara, Unara, Paranteze
│   ├── Instructiuni.cs             ⏳ 7 clase: Declaratie, Atribuire, Expresie, For, While, If, Bloc
│   └── Variabila.cs                ✅ Entry tabel simboluri cu validări
│
├── Core/                            # Logica principală (DE IMPLEMENTAT)
│   ├── Lexer.cs                    ⏳ ~400 linii - Analiza lexicală
│   ├── Parser.cs                   ⏳ ~600 linii - Analiza sintactică  
│   ├── TabelSimboluri.cs           ⏳ ~250 linii - Gestionare variabile
│   └── Evaluator.cs                ⏳ ~400 linii - Evaluare & execuție
│
├── Utils/                           # Utilități
│   ├── EroareCompilare.cs          ✅ Gestionare erori cu format
│   └── CititorFisier.cs            ⏳ ~50 linii - I/O operations
│
├── Tests/                           # Suite teste
│   └── TestSuite.cs                ⏳ 20+ teste unitare
│
├── Program.cs                       ⏳ ~200 linii - Entry point
├── CompilatorLFT.csproj            ⏳ Fișier proiect .NET
└── README.md                        ✅ Documentație utilizator

LEGENDĂ:
✅ = Implementat 100%
⏳ = De implementat
📊 Progres total: 35% (1400/4000 linii)
```

### 1.3 Dependențe între Module

```
Lexer
  ↓
  └─→ AtomLexical (folosește)
  └─→ EroareCompilare (generează)

Parser
  ↓
  ├─→ Lexer (consumă tokeni)
  ├─→ Expresii (construiește)
  ├─→ Instructiuni (construiește)
  ├─→ TabelSimboluri (populează)
  └─→ EroareCompilare (generează)

TabelSimboluri
  ↓
  ├─→ Variabila (stochează)
  └─→ EroareCompilare (generează)

Evaluator
  ↓
  ├─→ Expresii (traversează)
  ├─→ Instructiuni (execută)
  ├─→ TabelSimboluri (citește/scrie)
  └─→ EroareCompilare (generează)
```

---

## 2. SPECIFICAȚII DETALIATE

### 2.1 Instructiuni.cs - SPECIFICAȚIE COMPLETĂ

```csharp
// FIȘIER: Models/Instructiuni.cs
// SCOP: Definește toate tipurile de instrucțiuni din limbaj
// REFERINȚĂ: Dragon Book Cap. 5 - Syntax-Directed Translation

using System;
using System.Collections.Generic;
using System.Linq;
using CompilatorLFT.Models;
using CompilatorLFT.Models.Expresii;

namespace CompilatorLFT.Models.Instructiuni
{
    /// <summary>
    /// Clasa abstractă de bază pentru toate instrucțiunile.
    /// </summary>
    public abstract class Instructiune : NodSintactic
    {
    }

    // ==================== DECLARAȚII ====================

    /// <summary>
    /// Instrucțiune de declarație de variabile.
    /// Sintaxă: int a, b=5, c;
    ///          double x=3.14;
    ///          string s="test";
    /// </summary>
    /// <example>
    /// int a, b=5, c;
    ///   ↓
    /// InstructiuneDeclaratie {
    ///   TipCuvantCheie: "int"
    ///   Declaratii: [
    ///     ("a", null),      // fără inițializare
    ///     ("b", Expr(5)),   // cu inițializare
    ///     ("c", null)
    ///   ]
    /// }
    /// </example>
    public sealed class InstructiuneDeclaratie : Instructiune
    {
        /// <summary>Cuvântul cheie pentru tip (int/double/string).</summary>
        public AtomLexical TipCuvantCheie { get; }

        /// <summary>
        /// Lista declarații: (identificator, expresie_inițializare_opțională)
        /// Dacă expresie este null → declarație fără inițializare
        /// </summary>
        public List<(AtomLexical identificator, Expresie expresieInit)> Declaratii { get; }

        /// <summary>Punct și virgulă final.</summary>
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
                throw new ArgumentException("Trebuie să fie cuvânt cheie pentru tip (int/double/string)");

            if (declaratii.Count == 0)
                throw new ArgumentException("Trebuie să existe cel puțin o declarație");
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

    // ==================== ATRIBUIRI ====================

    /// <summary>
    /// Instrucțiune de atribuire.
    /// Sintaxă: a = expresie;
    /// </summary>
    /// <example>
    /// a = 5 + 3;
    ///   ↓
    /// InstructiuneAtribuire {
    ///   Identificator: "a"
    ///   Expresie: ExpresieBinara(5, +, 3)
    /// }
    /// </example>
    public sealed class InstructiuneAtribuire : Instructiune
    {
        /// <summary>Identificatorul variabilei.</summary>
        public AtomLexical Identificator { get; }

        /// <summary>Operatorul de atribuire '='.</summary>
        public AtomLexical OperatorEgal { get; }

        /// <summary>Expresia care se evaluează și se atribuie.</summary>
        public Expresie Expresie { get; }

        /// <summary>Punct și virgulă final.</summary>
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
            PunctVirgula = punctVirgula ?? throw new ArgumentNullException(nameof(punctVirgula));

            if (identificator.Tip != TipAtomLexical.Identificator)
                throw new ArgumentException("Trebuie să fie identificator");

            if (operatorEgal.Tip != TipAtomLexical.Egal)
                throw new ArgumentException("Trebuie să fie operator '='");
        }

        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            yield return Identificator;
            yield return OperatorEgal;
            yield return Expresie;
            yield return PunctVirgula;
        }
    }

    // ==================== EXPRESII STANDALONE ====================

    /// <summary>
    /// Instrucțiune care constă doar dintr-o expresie.
    /// Sintaxă: expresie;
    /// </summary>
    /// <example>
    /// 3 + 5;  // Evaluează dar nu atribuie
    /// a * b;
    /// </example>
    public sealed class InstructiuneExpresie : Instructiune
    {
        /// <summary>Expresia evaluată.</summary>
        public Expresie Expresie { get; }

        /// <summary>Punct și virgulă final.</summary>
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

    // ==================== STRUCTURI CONTROL ====================

    /// <summary>
    /// Instrucțiune FOR.
    /// Sintaxă: for (init; conditie; increment) instructiune
    ///         for (init; conditie; increment) { bloc }
    /// </summary>
    /// <example>
    /// for (int i=0; i&lt;10; i=i+1) {
    ///     suma = suma + i;
    /// }
    /// </example>
    public sealed class InstructiuneFor : Instructiune
    {
        public AtomLexical CuvantCheieFor { get; }
        public AtomLexical ParantezaDeschisa { get; }

        /// <summary>Instrucțiune inițializare (ex: int i=0).</summary>
        public Instructiune Initializare { get; }

        /// <summary>Expresie condiție (ex: i&lt;10).</summary>
        public Expresie Conditie { get; }

        public AtomLexical PunctVirgula { get; }

        /// <summary>Instrucțiune increment (ex: i=i+1).</summary>
        public Instructiune Increment { get; }

        public AtomLexical ParantezaInchisa { get; }

        /// <summary>Corp buclei (poate fi instrucțiune simplă sau bloc).</summary>
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
            CuvantCheieFor = cuvantCheieFor;
            ParantezaDeschisa = parantezaDeschisa;
            Initializare = initializare;
            Conditie = conditie;
            PunctVirgula = punctVirgula;
            Increment = increment;
            ParantezaInchisa = parantezaInchisa;
            Corp = corp;
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
    /// Instrucțiune WHILE.
    /// Sintaxă: while (conditie) instructiune
    ///         while (conditie) { bloc }
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
            CuvantCheieWhile = cuvantCheieWhile;
            ParantezaDeschisa = parantezaDeschisa;
            Conditie = conditie;
            ParantezaInchisa = parantezaInchisa;
            Corp = corp;
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
    /// Instrucțiune IF (cu else opțional).
    /// Sintaxă: if (conditie) instructiune
    ///         if (conditie) instructiune else instructiune
    /// </summary>
    public sealed class InstructiuneIf : Instructiune
    {
        public AtomLexical CuvantCheieIf { get; }
        public AtomLexical ParantezaDeschisa { get; }
        public Expresie Conditie { get; }
        public AtomLexical ParantezaInchisa { get; }
        public Instructiune CorpAdevarat { get; }

        // Opțional
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
            CuvantCheieIf = cuvantCheieIf;
            ParantezaDeschisa = parantezaDeschisa;
            Conditie = conditie;
            ParantezaInchisa = parantezaInchisa;
            CorpAdevarat = corpAdevarat;
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

    // ==================== BLOC ====================

    /// <summary>
    /// Bloc de instrucțiuni între acolade.
    /// Sintaxă: { instructiune1; instructiune2; ... }
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
            AcoladaDeschisa = acoladaDeschisa;
            Instructiuni = instructiuni ?? new List<Instructiune>();
            AcoladaInchisa = acoladaInchisa;
        }

        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            yield return AcoladaDeschisa;

            foreach (var instr in Instructiuni)
                yield return instr;

            yield return AcoladaInchisa;
        }
    }

    // ==================== PROGRAM ====================

    /// <summary>
    /// Nod rădăcină - programul complet.
    /// Conține lista tuturor instrucțiunilor de la nivel superior.
    /// </summary>
    public sealed class Program : NodSintactic
    {
        public List<Instructiune> Instructiuni { get; }

        public override TipAtomLexical Tip => TipAtomLexical.Program;

        public Program(List<Instructiune> instructiuni)
        {
            Instructiuni = instructiuni ?? new List<Instructiune>();
        }

        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            return Instructiuni.Cast<NodSintactic>();
        }
    }
}
```

---

## 3. IMPLEMENTARE LEXER

### 3.1 Specificație Funcțională Completă

```
SCOP LEXER:
- Transformă text sursă în stream de tokeni (atomi lexicali)
- Tracking precis linie/coloană pentru fiecare token
- Detectare și raportare erori lexicale
- Ignorare spații albe și comentarii (dacă există)

INPUT:  String text sursă
OUTPUT: Lista<AtomLexical> + Lista<EroareCompilare>

EXEMPLE:
Input:  "int a = 5;"
Output: [
  AtomLexical(CuvantCheieInt, "int", null, 1, 1, 0),
  AtomLexical(Identificator, "a", "a", 1, 5, 4),
  AtomLexical(Egal, "=", null, 1, 7, 6),
  AtomLexical(NumarIntreg, "5", 5, 1, 9, 8),
  AtomLexical(PunctVirgula, ";", null, 1, 10, 9)
]
```

### 3.2 Pseudocod Detaliat

```
CLASA Lexer:
  
  CAMPURI PRIVATE:
    _text: string              // Text sursă complet
    _pozitie: int             // Poziție curentă în text (0-indexed)
    _linie: int               // Linia curentă (1-indexed)
    _coloana: int             // Coloana curentă (1-indexed)
    _erori: List<EroareCompilare>
  
  PROPRIETĂȚI:
    CaracterCurent: char      // _text[_pozitie] sau '\0' dacă EOF
    Erori: IEnumerable<EroareCompilare> (readonly)
  
  CONSTRUCTOR(text: string):
    _text = text
    _pozitie = 0
    _linie = 1
    _coloana = 1
    _erori = new List()
  
  METODĂ PUBLICĂ UrmatorulAtom() -> AtomLexical:
    // Skip spații și newlines
    WHILE CaracterCurent ∈ {' ', '\t', '\r', '\n'}:
      IF CaracterCurent == '\n':
        _linie++
        _coloana = 1
      ELSE:
        _coloana++
      _pozitie++
    
    // EOF
    IF CaracterCurent == '\0':
      RETURN AtomLexical.Eof(_linie, _coloana, _pozitie)
    
    // Salvează poziția de start
    liniaStart = _linie
    coloanaStart = _coloana
    pozitieStart = _pozitie
    
    // NUMERE: [0-9]+(\.[0-9]+)?
    IF IsDigit(CaracterCurent):
      RETURN ScanNumar(liniaStart, coloanaStart, pozitieStart)
    
    // STRING-URI: "..."
    IF CaracterCurent == '"':
      RETURN ScanString(liniaStart, coloanaStart, pozitieStart)
    
    // IDENTIFICATORI și CUVINTE CHEIE: [a-zA-Z_][a-zA-Z0-9_]*
    IF IsLetter(CaracterCurent) OR CaracterCurent == '_':
      RETURN ScanIdentificator(liniaStart, coloanaStart, pozitieStart)
    
    // OPERATORI ȘI DELIMITATORI
    SWITCH CaracterCurent:
      CASE '+':
        Avanseaza()
        RETURN AtomLexical.Operator(Plus, "+", liniaStart, coloanaStart, pozitieStart)
      
      CASE '-':
        Avanseaza()
        RETURN AtomLexical.Operator(Minus, "-", liniaStart, coloanaStart, pozitieStart)
      
      CASE '*':
        Avanseaza()
        RETURN AtomLexical.Operator(Star, "*", liniaStart, coloanaStart, pozitieStart)
      
      CASE '/':
        Avanseaza()
        RETURN AtomLexical.Operator(Slash, "/", liniaStart, coloanaStart, pozitieStart)
      
      CASE '<':
        Avanseaza()
        IF CaracterCurent == '=':
          Avanseaza()
          RETURN AtomLexical.Operator(MaiMicEgal, "<=", ...)
        RETURN AtomLexical.Operator(MaiMic, "<", ...)
      
      CASE '>':
        Avanseaza()
        IF CaracterCurent == '=':
          Avanseaza()
          RETURN AtomLexical.Operator(MaiMareEgal, ">=", ...)
        RETURN AtomLexical.Operator(MaiMare, ">", ...)
      
      CASE '=':
        Avanseaza()
        IF CaracterCurent == '=':
          Avanseaza()
          RETURN AtomLexical.Operator(EgalEgal, "==", ...)
        RETURN AtomLexical.Operator(Egal, "=", ...)
      
      CASE '!':
        Avanseaza()
        IF CaracterCurent == '=':
          Avanseaza()
          RETURN AtomLexical.Operator(Diferit, "!=", ...)
        ELSE:
          AdaugaEroare(liniaStart, coloanaStart, Lexicala, 
                       "operator '!' invalid, se aștepta '!='")
          RETURN AtomLexical.Operator(Invalid, "!", ...)
      
      CASE '(':
        Avanseaza()
        RETURN AtomLexical.Operator(ParantezaDeschisa, "(", ...)
      
      CASE ')':
        Avanseaza()
        RETURN AtomLexical.Operator(ParantezaInchisa, ")", ...)
      
      CASE '{':
        Avanseaza()
        RETURN AtomLexical.Operator(AcoladaDeschisa, "{", ...)
      
      CASE '}':
        Avanseaza()
        RETURN AtomLexical.Operator(AcoladaInchisa, "}", ...)
      
      CASE ';':
        Avanseaza()
        RETURN AtomLexical.Operator(PunctVirgula, ";", ...)
      
      CASE ',':
        Avanseaza()
        RETURN AtomLexical.Operator(Virgula, ",", ...)
      
      DEFAULT:
        // Caracter invalid
        caracterInvalid = CaracterCurent
        Avanseaza()
        AdaugaEroare(liniaStart, coloanaStart, Lexicala,
                     $"caracter invalid '{caracterInvalid}'")
        RETURN AtomLexical.Operator(Invalid, caracterInvalid.ToString(), ...)
  
  METODĂ PRIVATĂ ScanNumar(linia, coloana, pozitie) -> AtomLexical:
    start = _pozitie
    
    // Scanează partea întreagă
    WHILE IsDigit(CaracterCurent):
      Avanseaza()
    
    // Verifică dacă e double (cu punct)
    IF CaracterCurent == '.':
      Avanseaza()
      
      // Trebuie să urmeze cifre după punct
      IF NOT IsDigit(CaracterCurent):
        text = _text.Substring(start, _pozitie - start)
        AdaugaEroare(linia, coloana, Lexicala,
                     $"număr zecimal invalid '{text}' - lipsesc cifre după punct")
        RETURN AtomLexical.Operator(Invalid, text, linie, coloana, pozitie)
      
      WHILE IsDigit(CaracterCurent):
        Avanseaza()
      
      // Double
      text = _text.Substring(start, _pozitie - start)
      IF TryParse double(text) -> valoare:
        RETURN AtomLexical.NumarDouble(text, valoare, linie, coloana, pozitie)
      ELSE:
        AdaugaEroare(linie, coloana, Lexicala,
                     $"număr zecimal '{text}' depășește limitele Double")
        RETURN AtomLexical.Operator(Invalid, text, linie, coloana, pozitie)
    
    ELSE:
      // Int
      text = _text.Substring(start, _pozitie - start)
      IF TryParse int(text) -> valoare:
        RETURN AtomLexical.NumarInt(text, valoare, linie, coloana, pozitie)
      ELSE:
        AdaugaEroare(linie, coloana, Lexicala,
                     $"număr întreg '{text}' depășește limitele Int32")
        RETURN AtomLexical.Operator(Invalid, text, linie, coloana, pozitie)
  
  METODĂ PRIVATĂ ScanString(linia, coloana, pozitie) -> AtomLexical:
    Avanseaza()  // Skip ghilimea deschisă
    start = _pozitie
    
    WHILE CaracterCurent != '"' AND CaracterCurent != '\0':
      IF CaracterCurent == '\n':
        // String nu poate conține newline
        AdaugaEroare(linie, coloana, Lexicala,
                     "string neînchis - lipsește ghilimele de închidere")
        text = _text.Substring(start, _pozitie - start)
        RETURN AtomLexical.Operator(Invalid, text, linie, coloana, pozitie)
      Avanseaza()
    
    IF CaracterCurent == '\0':
      // Am ajuns la EOF fără să închidem string-ul
      AdaugaEroare(linie, coloana, Lexicala,
                   "string neînchis - lipsește ghilimele de închidere")
      text = _text.Substring(start, _pozitie - start)
      RETURN AtomLexical.Operator(Invalid, text, linie, coloana, pozitie)
    
    // CaracterCurent == '"'
    text = _text.Substring(start, _pozitie - start)
    Avanseaza()  // Skip ghilimea închisă
    
    RETURN AtomLexical.String(text, linie, coloana, pozitie)
  
  METODĂ PRIVATĂ ScanIdentificator(linia, coloana, pozitie) -> AtomLexical:
    start = _pozitie
    
    WHILE IsLetterOrDigit(CaracterCurent) OR CaracterCurent == '_':
      Avanseaza()
    
    text = _text.Substring(start, _pozitie - start)
    
    // Verifică dacă e cuvânt cheie
    tipCuvantCheie = RecunoasteCuvantCheie(text)
    
    IF tipCuvantCheie != null:
      RETURN AtomLexical.Operator(tipCuvantCheie, text, linie, coloana, pozitie)
    ELSE:
      // Identificator
      // Validare regex: ^[a-zA-Z_][a-zA-Z0-9_]*$
      IF NOT Regex.IsMatch(text, "^[a-zA-Z_][a-zA-Z0-9_]*$"):
        AdaugaEroare(linie, coloana, Lexicala,
                     $"identificator invalid '{text}'")
        RETURN AtomLexical.Operator(Invalid, text, linie, coloana, pozitie)
      
      RETURN AtomLexical.Id(text, linie, coloana, pozitie)
  
  METODĂ PRIVATĂ RecunoasteCuvantCheie(text: string) -> TipAtomLexical?:
    SWITCH text:
      CASE "int":    RETURN CuvantCheieInt
      CASE "double": RETURN CuvantCheieDouble
      CASE "string": RETURN CuvantCheieString
      CASE "for":    RETURN CuvantCheieFor
      CASE "while":  RETURN CuvantCheieWhile
      CASE "if":     RETURN CuvantCheieIf
      CASE "else":   RETURN CuvantCheieElse
      DEFAULT:       RETURN null
  
  METODĂ PRIVATĂ Avanseaza():
    IF CaracterCurent == '\n':
      _linie++
      _coloana = 1
    ELSE:
      _coloana++
    _pozitie++
  
  METODĂ PRIVATĂ AdaugaEroare(linie, coloana, tip, mesaj):
    _erori.Add(new EroareCompilare(linie, coloana, tip, mesaj))
```

### 3.3 Exemple Test pentru Lexer

```
TEST 1: Declarație simplă
INPUT:  "int a;"
OUTPUT: [
  CuvantCheieInt("int", 1:1),
  Identificator("a", 1:5),
  PunctVirgula(";", 1:6),
  Terminator
]

TEST 2: Expresie cu numere
INPUT:  "3.14 + 5"
OUTPUT: [
  NumarZecimal("3.14", 3.14, 1:1),
  Plus("+", 1:6),
  NumarIntreg("5", 5, 1:8),
  Terminator
]

TEST 3: String literal
INPUT:  "string s = \"hello\";"
OUTPUT: [
  CuvantCheieString("string", 1:1),
  Identificator("s", 1:8),
  Egal("=", 1:10),
  StringLiteral("hello", 1:12),
  PunctVirgula(";", 1:19),
  Terminator
]

TEST 4: Operatori relaționali
INPUT:  "a <= b"
OUTPUT: [
  Identificator("a", 1:1),
  MaiMicEgal("<=", 1:3),
  Identificator("b", 1:6),
  Terminator
]

TEST 5: Eroare - caracter invalid
INPUT:  "int a @ 5;"
OUTPUT: [
  CuvantCheieInt("int", 1:1),
  Identificator("a", 1:5),
  Invalid("@", 1:7),
  NumarIntreg("5", 5, 1:9),
  PunctVirgula(";", 1:10),
  Terminator
]
ERORI: [
  "la linia 1, coloana 7: eroare lexicală - caracter invalid '@'"
]

TEST 6: Eroare - string neînchis
INPUT:  "string s = \"hello"
OUTPUT: [
  CuvantCheieString("string", 1:1),
  Identificator("s", 1:8),
  Egal("=", 1:10),
  Invalid("hello", 1:12),
  Terminator
]
ERORI: [
  "la linia 1, coloana 12: eroare lexicală - string neînchis - lipsește ghilimele de închidere"
]

TEST 7: Tracking linii multiple
INPUT:  
  "int a;\n" +
  "a = 5;"
OUTPUT: [
  CuvantCheieInt("int", 1:1),
  Identificator("a", 1:5),
  PunctVirgula(";", 1:6),
  Identificator("a", 2:1),
  Egal("=", 2:3),
  NumarIntreg("5", 5, 2:5),
  PunctVirgula(";", 2:6),
  Terminator
]
```

---

## 4. IMPLEMENTARE PARSER

### 4.1 Gramatica Formală

```
Program := Instructiune*

Instructiune := Declaratie
              | Atribuire
              | ExpresieStandalone
              | For
              | While
              | If
              | Bloc

Declaratie := TipCuvantCheie ListaDeclaratii ';'

TipCuvantCheie := 'int' | 'double' | 'string'

ListaDeclaratii := Declaratie (',' Declaratie)*

Declaratie := Identificator ('=' Expresie)?

Atribuire := Identificator '=' Expresie ';'

ExpresieStandalone := Expresie ';'

For := 'for' '(' Instructiune Expresie ';' Instructiune ')' Instructiune

While := 'while' '(' Expresie ')' Instructiune

If := 'if' '(' Expresie ')' Instructiune ('else' Instructiune)?

Bloc := '{' Instructiune* '}'

// EXPRESII cu precedență operatori

Expresie := ExpresieRelationala

ExpresieRelationala := Termen (('<' | '>' | '<=' | '>=' | '==' | '!=') Termen)*

Termen := Factor (('+' | '-') Factor)*

Factor := Primar (('*' | '/') Primar)*

Primar := '-' Primar                // Minus unar
        | '(' Expresie ')'           // Paranteze
        | NumarIntreg
        | NumarZecimal
        | StringLiteral
        | Identificator

NOTĂ: Plus unar (+) NU este suportat conform cerințelor!
```

### 4.2 Precedență Operatori (de la mare la mic)

```
1. Paranteze: ( )
2. Minus unar: -
3. Înmulțire/împărțire: * /
4. Adunare/scădere: + -
5. Relaționali: < > <= >= == !=

Asociativitate: Toți operatorii binari sunt asociativi la stânga
Exemplu: 5 - 3 - 1 = (5 - 3) - 1 = 1
```

### 4.3 Pseudocod Detaliat Parser

```
CLASA Parser:
  
  CAMPURI PRIVATE:
    _tokeni: AtomLexical[]      // Array tokeni de la Lexer
    _index: int                 // Index curent în array
    _erori: List<EroareCompilare>
    _tabelSimboluri: TabelSimboluri
  
  PROPRIETĂȚI:
    AtomCurent: AtomLexical     // _tokeni[_index]
    Erori: IEnumerable<EroareCompilare> (readonly)
    TabelSimboluri: TabelSimboluri (readonly)
  
  CONSTRUCTOR(text: string):
    lexer = new Lexer(text)
    tokeni = new List<AtomLexical>()
    
    LOOP:
      atom = lexer.UrmatorulAtom()
      IF atom.Tip != Spatiu AND atom.Tip != LinieNoua:
        tokeni.Add(atom)
      IF atom.Tip == Terminator:
        BREAK
    
    _tokeni = tokeni.ToArray()
    _index = 0
    _erori = new List<EroareCompilare>()
    _erori.AddRange(lexer.Erori)
    _tabelSimboluri = new TabelSimboluri()
  
  // ==================== METODE HELPER ====================
  
  METODĂ PRIVATĂ ConsumaAtom() -> AtomLexical:
    atom = AtomCurent
    _index++
    RETURN atom
  
  METODĂ PRIVATĂ VerificaTip(tipAsteptat: TipAtomLexical) -> AtomLexical:
    IF AtomCurent.Tip == tipAsteptat:
      RETURN ConsumaAtom()
    ELSE:
      AdaugaEroare(AtomCurent.Linie, AtomCurent.Coloana, Sintactica,
                   $"se aștepta '{tipAsteptat}' dar s-a găsit '{AtomCurent.Tip}'")
      // Returnează atom invalid pentru a continua parsing-ul
      RETURN new AtomLexical(Invalid, "", null, AtomCurent.Linie, AtomCurent.Coloana, ...)
  
  METODĂ PRIVATĂ PrivesteSiUrmator(tipuri: params TipAtomLexical[]) -> bool:
    RETURN tipuri.Contains(AtomCurent.Tip)
  
  // ==================== PARSING PROGRAM ====================
  
  METODĂ PUBLICĂ ParseazaProgram() -> Program:
    instructiuni = new List<Instructiune>()
    
    WHILE AtomCurent.Tip != Terminator:
      TRY:
        instr = ParseazaInstructiune()
        IF instr != null:
          instructiuni.Add(instr)
      CATCH Exception e:
        // Eroare de parsing - încearcă să te recuperezi
        AdaugaEroare(AtomCurent.Linie, AtomCurent.Coloana, Sintactica, e.Message)
        RecupereazaDupaEroare()
    
    RETURN new Program(instructiuni)
  
  METODĂ PRIVATĂ RecupereazaDupaEroare():
    // Avansează până la următorul ';' sau '}' sau Terminator
    WHILE AtomCurent.Tip NOT IN {PunctVirgula, AcoladaInchisa, Terminator}:
      ConsumaAtom()
    
    IF AtomCurent.Tip == PunctVirgula:
      ConsumaAtom()  // Skip punct și virgulă
  
  // ==================== PARSING INSTRUCTIUNI ====================
  
  METODĂ PRIVATĂ ParseazaInstructiune() -> Instructiune:
    // Declarație (int/double/string ...)
    IF PrivesteSiUrmator(CuvantCheieInt, CuvantCheieDouble, CuvantCheieString):
      RETURN ParseazaDeclaratie()
    
    // For
    IF PrivesteSiUrmator(CuvantCheieFor):
      RETURN ParseazaFor()
    
    // While
    IF PrivesteSiUrmator(CuvantCheieWhile):
      RETURN ParseazaWhile()
    
    // If
    IF PrivesteSiUrmator(CuvantCheieIf):
      RETURN ParseazaIf()
    
    // Bloc
    IF PrivesteSiUrmator(AcoladaDeschisa):
      RETURN ParseazaBloc()
    
    // Atribuire sau ExpresieStandalone
    // Trebuie să privim mai departe: a = ... sau a + ...
    IF PrivesteSiUrmator(Identificator):
      // Salvează poziția pentru backtracking
      pozitieInceput = _index
      id = ConsumaAtom()
      
      IF AtomCurent.Tip == Egal:
        // E atribuire: a = expresie;
        egal = ConsumaAtom()
        expr = ParseazaExpresie()
        punctVirgula = VerificaTip(PunctVirgula)
        
        RETURN new InstructiuneAtribuire(id, egal, expr, punctVirgula)
      ELSE:
        // E expresie standalone: a + b;
        // Refacem poziția
        _index = pozitieInceput
        expr = ParseazaExpresie()
        punctVirgula = VerificaTip(PunctVirgula)
        
        RETURN new InstructiuneExpresie(expr, punctVirgula)
    
    // Expresie standalone (începe cu număr, string, etc.)
    expr = ParseazaExpresie()
    punctVirgula = VerificaTip(PunctVirgula)
    
    RETURN new InstructiuneExpresie(expr, punctVirgula)
  
  // ==================== PARSING DECLARAȚII ====================
  
  METODĂ PRIVATĂ ParseazaDeclaratie() -> InstructiuneDeclaratie:
    tipCuvant = ConsumaAtom()  // int/double/string
    tipDat = ConvertesteLaTipDat(tipCuvant.Tip)
    
    declaratii = new List<(AtomLexical, Expresie)>()
    
    LOOP:
      id = VerificaTip(Identificator)
      
      // Verifică în tabel simboluri - declarație duplicată?
      IF _tabelSimboluri.Exista(id.Text):
        AdaugaEroare(id.Linie, id.Coloana, Semantica,
                     $"declarație duplicată pentru variabila '{id.Text}'")
      ELSE:
        // Adaugă în tabel simboluri (neinițializată deocamdată)
        _tabelSimboluri.Adauga(id.Text, tipDat, id.Linie, id.Coloana)
      
      expr = null
      
      // Inițializare?
      IF AtomCurent.Tip == Egal:
        ConsumaAtom()  // Skip '='
        expr = ParseazaExpresie()
        
        // Setează valoarea în tabel simboluri
        IF expr != null:
          // Aici ar trebui să evaluăm expresia, dar o facem mai târziu
          // Marcăm doar că variabila va fi inițializată
      
      declaratii.Add((id, expr))
      
      // Mai sunt declarații?
      IF AtomCurent.Tip == Virgula:
        ConsumaAtom()
        CONTINUE
      ELSE:
        BREAK
    
    punctVirgula = VerificaTip(PunctVirgula)
    
    RETURN new InstructiuneDeclaratie(tipCuvant, declaratii, punctVirgula)
  
  // ==================== PARSING STRUCTURI CONTROL ====================
  
  METODĂ PRIVATĂ ParseazaFor() -> InstructiuneFor:
    cuvantCheieFor = ConsumaAtom()
    parantezaDeschisa = VerificaTip(ParantezaDeschisa)
    
    init = ParseazaInstructiune()
    conditie = ParseazaExpresie()
    punctVirgula = VerificaTip(PunctVirgula)
    increment = ParseazaInstructiune() // Fără ; la sfârșit aici!
    
    parantezaInchisa = VerificaTip(ParantezaInchisa)
    corp = ParseazaInstructiune()
    
    RETURN new InstructiuneFor(cuvantCheieFor, parantezaDeschisa,
                               init, conditie, punctVirgula, increment,
                               parantezaInchisa, corp)
  
  METODĂ PRIVATĂ ParseazaWhile() -> InstructiuneWhile:
    cuvantCheieWhile = ConsumaAtom()
    parantezaDeschisa = VerificaTip(ParantezaDeschisa)
    conditie = ParseazaExpresie()
    parantezaInchisa = VerificaTip(ParantezaInchisa)
    corp = ParseazaInstructiune()
    
    RETURN new InstructiuneWhile(cuvantCheieWhile, parantezaDeschisa,
                                 conditie, parantezaInchisa, corp)
  
  METODĂ PRIVATĂ ParseazaIf() -> InstructiuneIf:
    cuvantCheieIf = ConsumaAtom()
    parantezaDeschisa = VerificaTip(ParantezaDeschisa)
    conditie = ParseazaExpresie()
    parantezaInchisa = VerificaTip(ParantezaInchisa)
    corpAdevarat = ParseazaInstructiune()
    
    cuvantCheieElse = null
    corpFals = null
    
    IF AtomCurent.Tip == CuvantCheieElse:
      cuvantCheieElse = ConsumaAtom()
      corpFals = ParseazaInstructiune()
    
    RETURN new InstructiuneIf(cuvantCheieIf, parantezaDeschisa,
                              conditie, parantezaInchisa, corpAdevarat,
                              cuvantCheieElse, corpFals)
  
  METODĂ PRIVATĂ ParseazaBloc() -> Bloc:
    acoladaDeschisa = ConsumaAtom()
    instructiuni = new List<Instructiune>()
    
    WHILE AtomCurent.Tip != AcoladaInchisa AND AtomCurent.Tip != Terminator:
      instr = ParseazaInstructiune()
      instructiuni.Add(instr)
    
    acoladaInchisa = VerificaTip(AcoladaInchisa)
    
    RETURN new Bloc(acoladaDeschisa, instructiuni, acoladaInchisa)
  
  // ==================== PARSING EXPRESII ====================
  
  METODĂ PRIVATĂ ParseazaExpresie() -> Expresie:
    RETURN ParseazaExpresieRelationala()
  
  METODĂ PRIVATĂ ParseazaExpresieRelationala() -> Expresie:
    stanga = ParseazaTermen()
    
    WHILE PrivesteSiUrmator(MaiMic, MaiMare, MaiMicEgal, MaiMareEgal, EgalEgal, Diferit):
      op = ConsumaAtom()
      dreapta = ParseazaTermen()
      stanga = new ExpresieBinara(stanga, op, dreapta)
    
    RETURN stanga
  
  METODĂ PRIVATĂ ParseazaTermen() -> Expresie:
    stanga = ParseazaFactor()
    
    WHILE PrivesteSiUrmator(Plus, Minus):
      op = ConsumaAtom()
      dreapta = ParseazaFactor()
      stanga = new ExpresieBinara(stanga, op, dreapta)
    
    RETURN stanga
  
  METODĂ PRIVATĂ ParseazaFactor() -> Expresie:
    stanga = ParseazaPrimar()
    
    WHILE PrivesteSiUrmator(Star, Slash):
      op = ConsumaAtom()
      dreapta = ParseazaPrimar()
      stanga = new ExpresieBinara(stanga, op, dreapta)
    
    RETURN stanga
  
  METODĂ PRIVATĂ ParseazaPrimar() -> Expresie:
    // Minus unar
    IF AtomCurent.Tip == Minus:
      op = ConsumaAtom()
      operand = ParseazaPrimar()
      RETURN new ExpresieUnara(op, operand)
    
    // Plus unar - EROARE conform cerințelor!
    IF AtomCurent.Tip == Plus:
      AdaugaEroare(AtomCurent.Linie, AtomCurent.Coloana, Lexicala,
                   "plus unar nu este permis")
      ConsumaAtom()  // Skip
      RETURN ParseazaPrimar()
    
    // Paranteze
    IF AtomCurent.Tip == ParantezaDeschisa:
      parantezaDeschisa = ConsumaAtom()
      expr = ParseazaExpresie()
      parantezaInchisa = VerificaTip(ParantezaInchisa)
      RETURN new ExpresieCuParanteze(parantezaDeschisa, expr, parantezaInchisa)
    
    // Literal număr întreg
    IF AtomCurent.Tip == NumarIntreg:
      atom = ConsumaAtom()
      RETURN new ExpresieNumerica(atom)
    
    // Literal număr zecimal
    IF AtomCurent.Tip == NumarZecimal:
      atom = ConsumaAtom()
      RETURN new ExpresieNumerica(atom)
    
    // Literal string
    IF AtomCurent.Tip == StringLiteral:
      atom = ConsumaAtom()
      RETURN new ExpresieString(atom)
    
    // Identificator (variabilă)
    IF AtomCurent.Tip == Identificator:
      id = ConsumaAtom()
      
      // Verificare semantică: există variabila?
      IF NOT _tabelSimboluri.Exista(id.Text):
        AdaugaEroare(id.Linie, id.Coloana, Semantica,
                     $"variabila '{id.Text}' nu a fost declarată")
      
      RETURN new ExpresieIdentificator(id)
    
    // Eroare
    AdaugaEroare(AtomCurent.Linie, AtomCurent.Coloana, Sintactica,
                 $"expresie invalidă - token neașteptat '{AtomCurent.Tip}'")
    
    // Încearcă să continue
    ConsumaAtom()
    RETURN ParseazaPrimar()
```

---

CONTINUARE ÎN URMĂTORUL FIȘIER...
