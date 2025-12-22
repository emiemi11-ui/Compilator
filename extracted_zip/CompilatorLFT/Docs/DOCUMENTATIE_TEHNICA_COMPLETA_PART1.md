# 📚 DOCUMENTAȚIE TEHNICĂ COMPLETĂ - COMPILATOR LFT
## Ghid Complet pentru Implementare Proiect Nota 10

---

## 📋 CUPRINS

1. [Viziune Generală](#1-viziune-generală)
2. [Arhitectură Detaliată](#2-arhitectură-detaliată)
3. [Gramatica Formală](#3-gramatica-formală)
4. [Clasele Implementate](#4-clasele-implementate)
5. [Clasele de Implementat](#5-clasele-de-implementat)
6. [Algoritmi Esențiali](#6-algoritmi-esențiali)
7. [Exemple Complete](#7-exemple-complete)
8. [Testare](#8-testare)
9. [Debugging](#9-debugging)

---

## 1. VIZIUNE GENERALĂ

### 1.1 Ce Face Compilatorul

Compilatorul transformă **cod sursă** într-un **program executabil** prin mai multe faze:

```
INTRARE (Text)                    IEȘIRE
    ↓
┌─────────────────┐
│  Analiza        │  → Lista de Tokeni
│  Lexicală       │     [int, a, =, 5, ;]
│  (Lexer)        │
└────────┬────────┘
         ↓
┌─────────────────┐
│  Analiza        │  → Arbore Sintactic Abstract (AST)
│  Sintactică     │     ExpresieBinara(a, +, b)
│  (Parser)       │
└────────┬────────┘
         ↓
┌─────────────────┐
│  Analiza        │  → Validări + Tabel Simboluri
│  Semantică      │     {a: int=5, b: int=3}
│  (TabelSimburi) │
└────────┬────────┘
         ↓
┌─────────────────┐
│  Evaluare       │  → Rezultate
│  (Evaluator)    │     a+b = 8
└─────────────────┘
```

### 1.2 Componente Principale

| Componentă | Fișier | Responsabilitate | Complexitate |
|------------|--------|------------------|--------------|
| **Lexer** | Core/Lexer.cs | Text → Tokeni | ⭐⭐⭐⭐ |
| **Parser** | Core/Parser.cs | Tokeni → AST | ⭐⭐⭐⭐⭐ |
| **TabelSimboluri** | Core/TabelSimboluri.cs | Validări semantice | ⭐⭐⭐ |
| **Evaluator** | Core/Evaluator.cs | Calculează rezultate | ⭐⭐⭐⭐ |
| **AtomLexical** | Models/AtomLexical.cs | Reprezentare token | ⭐⭐ |
| **Expresii** | Models/Expresii.cs | Noduri AST expresii | ⭐⭐ |
| **Instructiuni** | Models/Instructiuni.cs | Noduri AST instrucțiuni | ⭐⭐ |

---

## 2. ARHITECTURĂ DETALIATĂ

### 2.1 Diagrama Flux de Date

```
┌────────────────────────────────────────────────────────────┐
│                      PROGRAM.CS                            │
│  - Citește fișier sau input consola                        │
│  - Orchestrează toate componentele                         │
└──────────────────────┬─────────────────────────────────────┘
                       │ string text
                       ↓
┌────────────────────────────────────────────────────────────┐
│                      LEXER.CS                              │
│  INPUT:  "int a = 5 + 3;"                                  │
│  OUTPUT: [CuvantCheieInt, Identificator(a), Egal,          │
│          NumarIntreg(5), Plus, NumarIntreg(3),             │
│          PunctVirgula, Terminator]                         │
│                                                            │
│  ALGORITM:                                                 │
│  1. Citește caracter cu caracter                           │
│  2. Identifică pattern-uri (cifre, litere, operatori)      │
│  3. Creează AtomLexical pentru fiecare token              │
│  4. Tracking linie/coloană pentru fiecare token            │
│  5. Raportează erori lexicale cu poziție exactă            │
└──────────────────────┬─────────────────────────────────────┘
                       │ List<AtomLexical>
                       ↓
┌────────────────────────────────────────────────────────────┐
│                      PARSER.CS                             │
│  INPUT:  Lista tokeni                                      │
│  OUTPUT: Arbore Sintactic Abstract (AST)                   │
│                                                            │
│  ALGORITM (Recursive Descent Parser):                      │
│  1. ParseazaProgram() → lista instrucțiuni                 │
│  2. ParseazaInstructiune() → Declaratie/Atribuire/For/...  │
│  3. ParseazaExpresie() → ExpresieBinara/Unara/...         │
│  4. Respectă precedența operatorilor                       │
│  5. Construiește AST bottom-up                            │
│  6. Raportează erori sintactice                            │
│                                                            │
│  În paralel: TabelSimboluri                                │
│  - Înregistrează variabile declarate                       │
│  - Validează variabile la utilizare                        │
└──────────────────────┬─────────────────────────────────────┘
                       │ AST + TabelSimboluri
                       ↓
┌────────────────────────────────────────────────────────────┐
│                   EVALUATOR.CS                             │
│  INPUT:  AST + TabelSimboluri                              │
│  OUTPUT: Rezultate calcule                                 │
│                                                            │
│  ALGORITM (Visitor Pattern):                               │
│  1. Traversează AST recursiv                               │
│  2. Pentru fiecare nod:                                    │
│     - ExpresieNumerica → returnează valoarea               │
│     - ExpresieBinara → evaluează stânga și dreapta,        │
│       apoi aplică operatorul                               │
│     - ExpresieIdentificator → caută în tabel simboluri     │
│  3. Aplică conversii de tip (int → double când necesar)    │
│  4. Validează operații (împărțire la 0, tipuri)           │
│  5. Raportează erori semantice                             │
└────────────────────────────────────────────────────────────┘
```

### 2.2 Dependențe între Module

```
                    ┌──────────────┐
                    │  Program.cs  │
                    └──────┬───────┘
                           │
         ┌─────────────────┼─────────────────┐
         │                 │                 │
         ↓                 ↓                 ↓
    ┌─────────┐      ┌─────────┐      ┌──────────┐
    │ Lexer   │──→   │ Parser  │──→   │Evaluator │
    └────┬────┘      └────┬────┘      └────┬─────┘
         │                │                 │
         ↓                ↓                 ↓
    ┌────────────────────────────────────────┐
    │         TabelSimboluri.cs              │
    └────────────────────────────────────────┘
                         │
                         ↓
    ┌────────────────────────────────────────┐
    │       EroareCompilare.cs               │
    └────────────────────────────────────────┘
```

**Regula de aur**: 
- Lexer NU știe despre Parser
- Parser NU știe despre Evaluator
- Toți folosesc Models (AtomLexical, Expresii, etc.)
- Toți raportează erori prin EroareCompilare

---

## 3. GRAMATICA FORMALĂ

### 3.1 Gramatica Completă (BNF)

```bnf
Program          ::= Instructiune*

Instructiune     ::= Declaratie
                   | Atribuire
                   | ExpresieSimple ';'
                   | InstructiuneFor
                   | InstructiuneWhile
                   | InstructiuneIf
                   | Bloc

Declaratie       ::= TipBaza ListaDeclaratii ';'

TipBaza          ::= 'int' | 'double' | 'string'

ListaDeclaratii  ::= Declarator (',' Declarator)*

Declarator       ::= IDENTIFICATOR ('=' Expresie)?

Atribuire        ::= IDENTIFICATOR '=' Expresie ';'

InstructiuneFor  ::= 'for' '(' (Declaratie | Atribuire | ';')
                              Expresie? ';'
                              (Atribuire | ExpresieSimple)?
                         ')' (Instructiune | Bloc)

InstructiuneWhile ::= 'while' '(' Expresie ')' (Instructiune | Bloc)

InstructiuneIf    ::= 'if' '(' Expresie ')' (Instructiune | Bloc)
                      ('else' (Instructiune | Bloc))?

Bloc             ::= '{' Instructiune* '}'

Expresie         ::= Termen (('+' | '-') Termen)*

Termen           ::= Factor (('*' | '/') Factor)*

Factor           ::= ('-')? ExpresieAtom

ExpresieAtom     ::= NUMAR_INTREG
                   | NUMAR_ZECIMAL
                   | STRING_LITERAL
                   | IDENTIFICATOR
                   | '(' Expresie ')'

IDENTIFICATOR    ::= [a-zA-Z_][a-zA-Z0-9_]*

NUMAR_INTREG     ::= [0-9]+

NUMAR_ZECIMAL    ::= [0-9]+ '.' [0-9]+

STRING_LITERAL   ::= '"' [^"]* '"'
```

### 3.2 Precedența Operatorilor

| Precedență | Operatori | Asociativitate | Exemple |
|------------|-----------|----------------|---------|
| 1 (cea mai mare) | `()` | N/A | `(a + b)` |
| 2 | `-` (unar) | Dreapta | `-a`, `-(x+y)` |
| 3 | `*`, `/` | Stânga | `a * b`, `x / y` |
| 4 (cea mai mică) | `+`, `-` | Stânga | `a + b`, `x - y` |

**Exemplu evaluare cu precedență:**
```
Input:  3 + 4 * 5
Parse:  3 + (4 * 5)    // * are precedență mai mare
Result: 3 + 20 = 23

Input:  (3 + 4) * 5
Parse:  (3 + 4) * 5    // () forțează precedența
Result: 7 * 5 = 35
```

### 3.3 Arborele Sintactic pentru "3 + 4 * 5"

```
        ExpresieBinara(+)
       /                 \
      /                   \
ExpresieNumerica(3)   ExpresieBinara(*)
                      /               \
                     /                 \
              ExpresieNumerica(4)  ExpresieNumerica(5)
```

**Observație**: Arborele reflectă precedența! `*` este mai jos (se evaluează primul).

---

## 4. CLASELE IMPLEMENTATE (Detaliat)

### 4.1 TipAtomLexical.cs - Enumerări

**Scop**: Definește TOATE tipurile de tokeni și noduri din sistem.

**Structură**:
```csharp
enum TipAtomLexical {
    // LITERALI (valorile concrete din cod)
    NumarIntreg,     // 42, -17, 0
    NumarZecimal,    // 3.14, -0.5
    StringLiteral,   // "hello", "test"
    
    // IDENTIFICATORI
    Identificator,   // a, suma, _temp
    
    // CUVINTE CHEIE - TIPURI
    CuvantCheieInt, CuvantCheieDouble, CuvantCheieString,
    
    // CUVINTE CHEIE - CONTROL FLOW
    CuvantCheieFor, CuvantCheieWhile, CuvantCheieIf, CuvantCheieElse,
    
    // OPERATORI ARITMETICI
    Plus, Minus, Star, Slash,
    
    // OPERATORI RELAȚIONALI
    MaiMic, MaiMare, MaiMicEgal, MaiMareEgal, EgalEgal, Diferit,
    
    // DELIMITATORI
    PunctVirgula, Virgula, Egal,
    ParantezaDeschisa, ParantezaInchisa,
    AcoladaDeschisa, AcoladaInchisa,
    
    // SPECIALE
    Spatiu, LinieNoua, Terminator, Invalid,
    
    // NODURI AST - EXPRESII
    ExpresieNumerica, ExpresieBinara, ExpresieUnara,
    ExpresieCuParanteze, ExpresieIdentificator, ExpresieString,
    
    // NODURI AST - INSTRUCTIUNI
    InstructiuneDeclaratie, InstructiuneAtribuire, InstructiuneExpresie,
    InstructiuneFor, InstructiuneWhile, InstructiuneIf, Bloc, Program
}

enum TipEroare { Lexicala, Sintactica, Semantica }
enum TipDat { Int, Double, String, Necunoscut }
```

**Utilizare în Lexer**:
```csharp
// Când Lexer-ul găsește "int", returnează:
new AtomLexical(TipAtomLexical.CuvantCheieInt, "int", null, linie, coloana, pozitie);

// Când găsește "123":
new AtomLexical(TipAtomLexical.NumarIntreg, "123", 123, linie, coloana, pozitie);
```

### 4.2 AtomLexical.cs - Reprezentarea unui Token

**Scop**: Reprezintă un token cu TOATE informațiile necesare.

**Proprietăți critice**:
```csharp
public class AtomLexical : NodSintactic {
    public TipAtomLexical Tip { get; }        // Ce fel de token
    public string Text { get; }                // Textul original
    public object Valoare { get; }             // Valoarea parsată (pentru literali)
    public int Linie { get; }                  // Linia în cod (1-indexed)
    public int Coloana { get; }                // Coloana în cod (1-indexed)
    public int PozitieAbsoluta { get; }        // Poziția în string (0-indexed)
}
```

**Exemplu complet**:
```csharp
// Pentru "int a = 5;" la linia 10, coloana 5

AtomLexical[] tokeni = {
    new AtomLexical(
        tip: TipAtomLexical.CuvantCheieInt,
        text: "int",
        valoare: null,
        linie: 10,
        coloana: 5,
        pozitieAbsoluta: 142
    ),
    new AtomLexical(
        tip: TipAtomLexical.Identificator,
        text: "a",
        valoare: "a",
        linie: 10,
        coloana: 9,
        pozitieAbsoluta: 146
    ),
    new AtomLexical(
        tip: TipAtomLexical.Egal,
        text: "=",
        valoare: null,
        linie: 10,
        coloana: 11,
        pozitieAbsoluta: 148
    ),
    new AtomLexical(
        tip: TipAtomLexical.NumarIntreg,
        text: "5",
        valoare: 5,
        linie: 10,
        coloana: 13,
        pozitieAbsoluta: 150
    ),
    new AtomLexical(
        tip: TipAtomLexical.PunctVirgula,
        text: ";",
        valoare: null,
        linie: 10,
        coloana: 14,
        pozitieAbsoluta: 151
    )
};
```

**Metode helper importante**:
```csharp
bool EsteCuvantCheieTip()        // true pentru int, double, string
bool EsteOperatorAritmetic()      // true pentru +, -, *, /
bool EsteOperatorRelational()     // true pentru <, >, <=, >=, ==, !=
bool EsteLiteral()                // true pentru numere și string-uri
TipDat ObtineTipDat()            // convertește tip atom → tip dat
```

### 4.3 EroareCompilare.cs - Raportare Erori

**Scop**: Raportează erori în formatul OBLIGATORIU.

**Format strict**:
```
la linia X, coloana Y: eroare [lexicală|sintactică|semantică] - [mesaj descriptiv]
```

**Exemplu utilizare**:
```csharp
// Eroare lexicală - caracter invalid
var eroare1 = EroareCompilare.Lexicala(
    linie: 5,
    coloana: 12,
    mesaj: "caracter invalid '@' în identificator",
    textSursa: "int sum@total;"
);
Console.WriteLine(eroare1);
// Output: la linia 5, coloana 12: eroare lexicală - caracter invalid '@' în identificator

// Eroare semantică - variabilă nedeclarată
var eroare2 = EroareCompilare.Semantica(
    linie: 10,
    coloana: 5,
    mesaj: "variabila 'x' nu a fost declarată"
);
Console.WriteLine(eroare2);
// Output: la linia 10, coloana 5: eroare semantică - variabila 'x' nu a fost declarată

// Cu context vizual
Console.WriteLine(eroare1.ToStringCuContext());
// Output:
// la linia 5, coloana 12: eroare lexicală - caracter invalid '@' în identificator
//   Context: int sum@total;
//               ^
```

**Factory methods** pentru ușurință:
```csharp
EroareCompilare.Lexicala(linie, coloana, mesaj, context);
EroareCompilare.Sintactica(linie, coloana, mesaj, context);
EroareCompilare.Semantica(linie, coloana, mesaj, context);
```

### 4.4 NodSintactic.cs - Baza AST

**Scop**: Clasa de bază pentru TOATE nodurile din arborele sintactic.

**Composite Pattern**:
```csharp
public abstract class NodSintactic {
    public abstract TipAtomLexical Tip { get; }
    public abstract IEnumerable<NodSintactic> ObtineCopii();
}
```

**De ce este important**:
1. **Uniformitate**: Toate nodurile au aceeași interfață
2. **Traversare**: Putem parcurge arborele recursiv
3. **Visitor Pattern**: Evaluatorul poate "vizita" fiecare nod
4. **Afișare**: Putem afișa arborele ușor

**Metode esențiale implementate**:
```csharp
void AfiseazaArbore(string indentare, bool estUltim)
    // Afișează arborele cu caractere box-drawing
    // └──ExpresieBinara
    //     ├──ExpresieNumerica 3
    //     ├──Plus +
    //     └──ExpresieNumerica 5

int NumaraNoduri()
    // Returnează numărul total de noduri din arbore

int CalculeazaInaltime()
    // Calculează înălțimea arborelui

string ToSExpression()
    // Convertește la format S-expression: (+ 3 5)
```

### 4.5 Expresii.cs - Noduri pentru Expresii

**Scop**: Definește TOATE tipurile de expresii din limbaj.

**Ierarhie**:
```
NodSintactic (abstract)
    ↓
Expresie (abstract)
    ↓
├── ExpresieNumerica (sealed)      - literali numerici: 42, 3.14
├── ExpresieString (sealed)         - literali string: "hello"
├── ExpresieIdentificator (sealed)  - variabile: a, suma
├── ExpresieBinara (sealed)         - operații binare: a + b
├── ExpresieUnara (sealed)          - operații unare: -a
└── ExpresieCuParanteze (sealed)    - cu paranteze: (a + b)
```

**Detaliu: ExpresieBinara**
```csharp
public sealed class ExpresieBinara : Expresie {
    public Expresie Stanga { get; }      // Operandul stâng
    public AtomLexical Operator { get; } // Operatorul (+, -, *, /, <, >, etc.)
    public Expresie Dreapta { get; }     // Operandul drept
    
    public override IEnumerable<NodSintactic> ObtineCopii() {
        yield return Stanga;
        yield return Operator;
        yield return Dreapta;
    }
}
```

**Exemplu construire AST pentru "3 + 4"**:
```csharp
// Pas 1: Creăm nodurile pentru literali
var trei = new ExpresieNumerica(
    new AtomLexical(TipAtomLexical.NumarIntreg, "3", 3, 1, 1, 0)
);
var patru = new ExpresieNumerica(
    new AtomLexical(TipAtomLexical.NumarIntreg, "4", 4, 1, 5, 4)
);

// Pas 2: Creăm operatorul
var plus = new AtomLexical(TipAtomLexical.Plus, "+", null, 1, 3, 2);

// Pas 3: Combinăm într-o expresie binară
var expresie = new ExpresieBinara(trei, plus, patru);

// Afișare arbore:
expresie.AfiseazaArbore();
// Output:
// └──ExpresieBinara
//     ├──ExpresieNumerica 3
//     ├──Plus +
//     └──ExpresieNumerica 4
```

### 4.6 Variabila.cs - Entry în Tabelul de Simboluri

**Scop**: Reprezintă o variabilă declarată în program.

**Proprietăți critice**:
```csharp
public class Variabila {
    public string Nume { get; }              // "a", "suma", "_temp"
    public TipDat Tip { get; }               // Int, Double, String
    public object Valoare { get; set; }      // Valoarea curentă (sau null)
    public bool EsteInitializata { get; set; } // A fost atribuită o valoare?
    public int LinieDeclaratie { get; }      // Pentru erori
    public int ColoanaDeclaratie { get; }    // Pentru erori
}
```

**Validări importante**:
```csharp
bool ValidareaTipului(object valoare) {
    // Verifică dacă valoarea este compatibilă cu tipul variabilei
    return Tip switch {
        TipDat.Int => valoare is int,
        TipDat.Double => valoare is double || valoare is int, // Conversie implicită!
        TipDat.String => valoare is string,
        _ => false
    };
}

void SeteazaValoare(object valoare) {
    if (!ValidareaTipului(valoare))
        throw new ArgumentException("Tip incompatibil");
    
    Valoare = valoare;
    EsteInitializata = true;
}
```

**Exemplu utilizare**:
```csharp
// Declarație: int a;
var varA = new Variabila("a", TipDat.Int, linie: 1, coloana: 5);
Console.WriteLine(varA.EsteInitializata); // false

// Atribuire: a = 5;
varA.SeteazaValoare(5);
Console.WriteLine(varA.EsteInitializata); // true
Console.WriteLine(varA.Valoare);          // 5

// Eroare: nu pot atribui string la int
varA.SeteazaValoare("test"); // ❌ ArgumentException
```

---

## 5. CLASELE DE IMPLEMENTAT (Specificații Complete)

### 5.1 Instructiuni.cs - Noduri pentru Instrucțiuni

**Fișier**: `Models/Instructiuni.cs`
**Linii estimate**: ~400 linii
**Complexitate**: ⭐⭐⭐ (Medie - multe clase similare)

**Cerință**: Definește 7 tipuri de instrucțiuni.

**Structură completă**:

```csharp
using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Instrucțiune de declarație.
    /// Exemplu: int a, b=5, c;
    /// </summary>
    public sealed class InstructiuneDeclaratie : Instructiune
    {
        public AtomLexical TipCuvantCheie { get; }  // int, double sau string
        
        // Lista de declaratori: fiecare poate avea sau nu inițializare
        // Exemplu pentru "int a, b=5":
        //   [(nume: "a", valoare: null), (nume: "b", valoare: ExpresieNumerica(5))]
        public List<(AtomLexical nume, Expresie valoareInitiala)> Declaratori { get; }
        
        public AtomLexical PunctVirgula { get; }
        
        public override TipAtomLexical Tip => TipAtomLexical.InstructiuneDeclaratie;
        
        public InstructiuneDeclaratie(
            AtomLexical tipCuvantCheie,
            List<(AtomLexical, Expresie)> declaratori,
            AtomLexical punctVirgula)
        {
            TipCuvantCheie = tipCuvantCheie;
            Declaratori = declaratori;
            PunctVirgula = punctVirgula;
        }
        
        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            yield return TipCuvantCheie;
            
            foreach (var (nume, valoare) in Declaratori)
            {
                yield return nume;
                if (valoare != null)
                    yield return valoare;
            }
            
            yield return PunctVirgula;
        }
    }

    /// <summary>
    /// Instrucțiune de atribuire.
    /// Exemplu: a = 5 + 3;
    /// </summary>
    public sealed class InstructiuneAtribuire : Instructiune
    {
        public AtomLexical Identificator { get; }  // Variabila care primește valoarea
        public AtomLexical Egal { get; }            // Token-ul '='
        public Expresie Valoare { get; }            // Expresia din dreapta
        public AtomLexical PunctVirgula { get; }    // Token-ul ';'
        
        public override TipAtomLexical Tip => TipAtomLexical.InstructiuneAtribuire;
        
        public InstructiuneAtribuire(
            AtomLexical identificator,
            AtomLexical egal,
            Expresie valoare,
            AtomLexical punctVirgula)
        {
            Identificator = identificator;
            Egal = egal;
            Valoare = valoare;
            PunctVirgula = punctVirgula;
        }
        
        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            yield return Identificator;
            yield return Egal;
            yield return Valoare;
            yield return PunctVirgula;
        }
    }

    /// <summary>
    /// Instrucțiune expresie (doar evaluează, nu atribuie).
    /// Exemplu: a + b;  sau  5 * 3;
    /// Utilă pentru afișarea arborelui conform cerințelor.
    /// </summary>
    public sealed class InstructiuneExpresie : Instructiune
    {
        public Expresie Expresie { get; }
        public AtomLexical PunctVirgula { get; }
        
        public override TipAtomLexical Tip => TipAtomLexical.InstructiuneExpresie;
        
        public InstructiuneExpresie(Expresie expresie, AtomLexical punctVirgula)
        {
            Expresie = expresie;
            PunctVirgula = punctVirgula;
        }
        
        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            yield return Expresie;
            yield return PunctVirgula;
        }
    }

    /// <summary>
    /// Instrucțiune for.
    /// Exemplu: for (int i=0; i<10; i=i+1) { ... }
    /// </summary>
    public sealed class InstructiuneFor : Instructiune
    {
        public AtomLexical CuvantCheieFor { get; }
        public AtomLexical ParantezaDeschisa { get; }
        
        // Inițializare: poate fi declarație sau atribuire
        public Instructiune Initializare { get; }
        
        // Condiție: expresie booleană (ex: i < 10)
        public Expresie Conditie { get; }
        public AtomLexical PunctVirgula1 { get; }
        
        // Increment: de obicei atribuire (ex: i = i + 1)
        public Instructiune Increment { get; }
        
        public AtomLexical ParantezaInchisa { get; }
        
        // Corp: o singură instrucțiune sau un bloc
        public Instructiune Corp { get; }
        
        public override TipAtomLexical Tip => TipAtomLexical.InstructiuneFor;
        
        public InstructiuneFor(
            AtomLexical cuvantCheieFor,
            AtomLexical parantezaDeschisa,
            Instructiune initializare,
            Expresie conditie,
            AtomLexical punctVirgula1,
            Instructiune increment,
            AtomLexical parantezaInchisa,
            Instructiune corp)
        {
            CuvantCheieFor = cuvantCheieFor;
            ParantezaDeschisa = parantezaDeschisa;
            Initializare = initializare;
            Conditie = conditie;
            PunctVirgula1 = punctVirgula1;
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
            yield return PunctVirgula1;
            if (Increment != null) yield return Increment;
            yield return ParantezaInchisa;
            yield return Corp;
        }
    }

    /// <summary>
    /// Instrucțiune while.
    /// Exemplu: while (i < 10) { ... }
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
    /// Instrucțiune if.
    /// Exemplu: if (a > b) { ... } else { ... }
    /// </summary>
    public sealed class InstructiuneIf : Instructiune
    {
        public AtomLexical CuvantCheieIf { get; }
        public AtomLexical ParantezaDeschisa { get; }
        public Expresie Conditie { get; }
        public AtomLexical ParantezaInchisa { get; }
        public Instructiune CorpAdevarat { get; }
        
        // Opțional: ramura else
        public AtomLexical CuvantCheieElse { get; }  // poate fi null
        public Instructiune CorpFals { get; }         // poate fi null
        
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

    /// <summary>
    /// Bloc de instrucțiuni între acolade.
    /// Exemplu: { int a = 5; a = a + 1; }
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
            
            foreach (var instructiune in Instructiuni)
                yield return instructiune;
            
            yield return AcoladaInchisa;
        }
    }

    /// <summary>
    /// Programul complet (rădăcina AST-ului).
    /// </summary>
    public sealed class ProgramComplet : Instructiune
    {
        public List<Instructiune> Instructiuni { get; }
        
        public override TipAtomLexical Tip => TipAtomLexical.Program;
        
        public ProgramComplet(List<Instructiune> instructiuni)
        {
            Instructiuni = instructiuni ?? new List<Instructiune>();
        }
        
        public override IEnumerable<NodSintactic> ObtineCopii()
        {
            return Instructiuni;
        }
    }
}
```

**NOTĂ IMPORTANTĂ**: Această clasă este PURĂ structură de date. Nu conține logică, doar definește nodurile AST.

---

### 5.2 Lexer.cs - Analiza Lexicală (CRITIC!)

**Fișier**: `Core/Lexer.cs`
**Linii estimate**: ~400-500 linii
**Complexitate**: ⭐⭐⭐⭐⭐ (Foarte înaltă)

**Algoritm de lucru**:

```
1. Primește string text
2. Inițializează poziția = 0, linie = 1, coloană = 1
3. WHILE nu s-a ajuns la sfârșit:
   a. Citește caracterul curent
   b. Determină ce tip de token începe aici:
      - Cifră → parsează număr (int sau double)
      - Literă/_ → parsează identificator sau cuvânt cheie
      - " → parsează string literal
      - +, -, *, / → operator aritmetic
      - <, >, = → operator relațional (atenție la <=, >=, ==, !=)
      - ;, ,, (, ), {, } → delimitator
      - Spațiu/tab → skip (dar incrementează coloana)
      - \n → skip (incrementează linia, resetează coloana)
      - Altceva → EROARE LEXICALĂ
   c. Creează AtomLexical cu poziția corectă
   d. Adaugă în listă
   e. Avansează poziția
4. Adaugă token Terminator la sfârșit
5. Returnează lista de tokeni
```

**Pseudocod complet**:

```csharp
public class Lexer
{
    private readonly string _text;
    private int _pozitie;
    private int _linie;
    private int _coloana;
    private List<EroareCompilare> _erori;
    
    // Regex pentru identificatori: ^[a-zA-Z_][a-zA-Z0-9_]*$
    private static readonly Regex RegexIdentificator = 
        new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");
    
    // Cuvinte cheie
    private static readonly HashSet<string> CuvinteChei = new HashSet<string> {
        "int", "double", "string", "for", "while", "if", "else"
    };
    
    public Lexer(string text)
    {
        _text = text ?? "";
        _pozitie = 0;
        _linie = 1;
        _coloana = 1;
        _erori = new List<EroareCompilare>();
    }
    
    private char CaracterCurent => 
        _pozitie < _text.Length ? _text[_pozitie] : '\0';
    
    private char CaracterUrmator => 
        _pozitie + 1 < _text.Length ? _text[_pozitie + 1] : '\0';
    
    private void Avanseaza()
    {
        if (CaracterCurent == '\n')
        {
            _linie++;
            _coloana = 1;
        }
        else
        {
            _coloana++;
        }
        _pozitie++;
    }
    
    public List<AtomLexical> Tokenizeaza()
    {
        var tokeni = new List<AtomLexical>();
        
        while (CaracterCurent != '\0')
        {
            var token = UrmatorulToken();
            
            // Skip spatii și linii noi
            if (token.Tip != TipAtomLexical.Spatiu && 
                token.Tip != TipAtomLexical.LinieNoua)
            {
                tokeni.Add(token);
            }
        }
        
        // Adaugă terminator
        tokeni.Add(AtomLexical.Eof(_linie, _coloana, _pozitie));
        
        return tokeni;
    }
    
    private AtomLexical UrmatorulToken()
    {
        // SPAȚII ȘI LINII NOI
        if (char.IsWhiteSpace(CaracterCurent))
        {
            return TokenizeazaSpatiu();
        }
        
        // NUMERE
        if (char.IsDigit(CaracterCurent))
        {
            return TokenizeazaNumar();
        }
        
        // IDENTIFICATORI ȘI CUVINTE CHEIE
        if (char.IsLetter(CaracterCurent) || CaracterCurent == '_')
        {
            return TokenizeazaIdentificator();
        }
        
        // STRING LITERALI
        if (CaracterCurent == '"')
        {
            return TokenizeazaString();
        }
        
        // OPERATORI ȘI DELIMITATORI
        return TokenizeazaOperatorSauDelimitator();
    }
    
    private AtomLexical TokenizeazaNumar()
    {
        int start = _pozitie;
        int linieStart = _linie;
        int coloanaStart = _coloana;
        
        // Citește cifre
        while (char.IsDigit(CaracterCurent))
        {
            Avanseaza();
        }
        
        // Verifică pentru punct zecimal
        if (CaracterCurent == '.' && char.IsDigit(CaracterUrmator))
        {
            // Număr zecimal
            Avanseaza(); // Skip '.'
            
            while (char.IsDigit(CaracterCurent))
            {
                Avanseaza();
            }
            
            string text = _text.Substring(start, _pozitie - start);
            
            if (double.TryParse(text, out double valoare))
            {
                return AtomLexical.NumarDouble(
                    text, valoare, linieStart, coloanaStart, start);
            }
            else
            {
                _erori.Add(EroareCompilare.Lexicala(
                    linieStart, coloanaStart,
                    $"număr zecimal invalid '{text}'"));
                
                return new AtomLexical(
                    TipAtomLexical.Invalid, text, null,
                    linieStart, coloanaStart, start);
            }
        }
        else
        {
            // Număr întreg
            string text = _text.Substring(start, _pozitie - start);
            
            if (int.TryParse(text, out int valoare))
            {
                return AtomLexical.NumarInt(
                    text, valoare, linieStart, coloanaStart, start);
            }
            else
            {
                _erori.Add(EroareCompilare.Lexicala(
                    linieStart, coloanaStart,
                    $"număr întreg invalid '{text}' (depășește Int32.MaxValue)"));
                
                return new AtomLexical(
                    TipAtomLexical.Invalid, text, null,
                    linieStart, coloanaStart, start);
            }
        }
    }
    
    private AtomLexical TokenizeazaIdentificator()
    {
        int start = _pozitie;
        int linieStart = _linie;
        int coloanaStart = _coloana;
        
        // Citește litere, cifre și underscore
        while (char.IsLetterOrDigit(CaracterCurent) || CaracterCurent == '_')
        {
            Avanseaza();
        }
        
        string text = _text.Substring(start, _pozitie - start);
        
        // Verifică dacă este cuvânt cheie
        if (CuvinteChei.Contains(text))
        {
            TipAtomLexical tipCuvantCheie = text switch
            {
                "int" => TipAtomLexical.CuvantCheieInt,
                "double" => TipAtomLexical.CuvantCheieDouble,
                "string" => TipAtomLexical.CuvantCheieString,
                "for" => TipAtomLexical.CuvantCheieFor,
                "while" => TipAtomLexical.CuvantCheieWhile,
                "if" => TipAtomLexical.CuvantCheieIf,
                "else" => TipAtomLexical.CuvantCheieElse,
                _ => TipAtomLexical.Invalid
            };
            
            return new AtomLexical(
                tipCuvantCheie, text, text,
                linieStart, coloanaStart, start);
        }
        
        // Verifică validitate identificator
        if (!RegexIdentificator.IsMatch(text))
        {
            _erori.Add(EroareCompilare.Lexicala(
                linieStart, coloanaStart,
                $"identificator invalid '{text}'"));
        }
        
        return AtomLexical.Id(text, linieStart, coloanaStart, start);
    }
    
    private AtomLexical TokenizeazaString()
    {
        int start = _pozitie;
        int linieStart = _linie;
        int coloanaStart = _coloana;
        
        Avanseaza(); // Skip ghilimele deschise
        
        var sb = new StringBuilder();
        
        while (CaracterCurent != '"' && CaracterCurent != '\0')
        {
            if (CaracterCurent == '\n')
            {
                _erori.Add(EroareCompilare.Lexicala(
                    linieStart, coloanaStart,
                    "string literal neînchis (lipsește ghilimele închise)"));
                break;
            }
            
            sb.Append(CaracterCurent);
            Avanseaza();
        }
        
        if (CaracterCurent == '"')
        {
            Avanseaza(); // Skip ghilimele închise
        }
        else
        {
            _erori.Add(EroareCompilare.Lexicala(
                linieStart, coloanaStart,
                "string literal neînchis (lipsește ghilimele închise)"));
        }
        
        string valoare = sb.ToString();
        string textComplet = _text.Substring(start, _pozitie - start);
        
        return AtomLexical.String(valoare, linieStart, coloanaStart, start);
    }
    
    private AtomLexical TokenizeazaOperatorSauDelimitator()
    {
        int start = _pozitie;
        int linieStart = _linie;
        int coloanaStart = _coloana;
        char c = CaracterCurent;
        
        // OPERATORI CU 2 CARACTERE (<=, >=, ==, !=)
        if (c == '<' && CaracterUrmator == '=')
        {
            Avanseaza(); Avanseaza();
            return AtomLexical.Operator(
                TipAtomLexical.MaiMicEgal, "<=",
                linieStart, coloanaStart, start);
        }
        if (c == '>' && CaracterUrmator == '=')
        {
            Avanseaza(); Avanseaza();
            return AtomLexical.Operator(
                TipAtomLexical.MaiMareEgal, ">=",
                linieStart, coloanaStart, start);
        }
        if (c == '=' && CaracterUrmator == '=')
        {
            Avanseaza(); Avanseaza();
            return AtomLexical.Operator(
                TipAtomLexical.EgalEgal, "==",
                linieStart, coloanaStart, start);
        }
        if (c == '!' && CaracterUrmator == '=')
        {
            Avanseaza(); Avanseaza();
            return AtomLexical.Operator(
                TipAtomLexical.Diferit, "!=",
                linieStart, coloanaStart, start);
        }
        
        // OPERATORI ȘI DELIMITATORI CU 1 CARACTER
        TipAtomLexical tip = c switch
        {
            '+' => TipAtomLexical.Plus,
            '-' => TipAtomLexical.Minus,
            '*' => TipAtomLexical.Star,
            '/' => TipAtomLexical.Slash,
            '<' => TipAtomLexical.MaiMic,
            '>' => TipAtomLexical.MaiMare,
            '=' => TipAtomLexical.Egal,
            ';' => TipAtomLexical.PunctVirgula,
            ',' => TipAtomLexical.Virgula,
            '(' => TipAtomLexical.ParantezaDeschisa,
            ')' => TipAtomLexical.ParantezaInchisa,
            '{' => TipAtomLexical.AcoladaDeschisa,
            '}' => TipAtomLexical.AcoladaInchisa,
            _ => TipAtomLexical.Invalid
        };
        
        if (tip == TipAtomLexical.Invalid)
        {
            _erori.Add(EroareCompilare.Lexicala(
                linieStart, coloanaStart,
                $"caracter invalid '{c}'"));
        }
        
        Avanseaza();
        
        return AtomLexical.Operator(
            tip, c.ToString(),
            linieStart, coloanaStart, start);
    }
    
    private AtomLexical TokenizeazaSpatiu()
    {
        int start = _pozitie;
        int linieStart = _linie;
        int coloanaStart = _coloana;
        
        bool esteLinieNoua = CaracterCurent == '\n';
        
        while (char.IsWhiteSpace(CaracterCurent))
        {
            Avanseaza();
        }
        
        string text = _text.Substring(start, _pozitie - start);
        TipAtomLexical tip = esteLinieNoua ? 
            TipAtomLexical.LinieNoua : TipAtomLexical.Spatiu;
        
        return new AtomLexical(
            tip, text, null,
            linieStart, coloanaStart, start);
    }
    
    public List<EroareCompilare> Erori => _erori;
}
```

**Teste pentru Lexer**:
```csharp
// Test 1: Numere
Input: "123 45.67"
Output: [NumarIntreg(123), NumarZecimal(45.67), Terminator]

// Test 2: Identificatori și cuvinte cheie
Input: "int suma _temp"
Output: [CuvantCheieInt, Identificator(suma), Identificator(_temp), Terminator]

// Test 3: String-uri
Input: "\"hello world\""
Output: [StringLiteral("hello world"), Terminator]

// Test 4: Operatori
Input: "a + b * c <= d"
Output: [Identificator(a), Plus, Identificator(b), Star, Identificator(c), 
         MaiMicEgal, Identificator(d), Terminator]

// Test 5: Tracking poziție
Input: "int a;"  (linia 1)
Output:
  - CuvantCheieInt("int") @ linie=1, coloana=1
  - Identificator("a") @ linie=1, coloana=5
  - PunctVirgula(";") @ linie=1, coloana=6
```

---

### 5.3 TabelSimboluri.cs - Gestionare Variabile

**Fișier**: `Core/TabelSimboluri.cs`
**Linii estimate**: ~250 linii
**Complexitate**: ⭐⭐⭐ (Medie)

**Pseudocod complet**:

```csharp
public class TabelSimboluri
{
    private Dictionary<string, Variabila> _variabile;
    
    public TabelSimboluri()
    {
        _variabile = new Dictionary<string, Variabila>();
    }
    
    /// <summary>
    /// Declară o nouă variabilă.
    /// VALIDĂRI:
    /// - Nu există deja o variabilă cu același nume
    /// </summary>
    public void DeclararaVariabila(
        string nume, 
        TipDat tip, 
        int linie, 
        int coloana,
        List<EroareCompilare> erori)
    {
        if (_variabile.ContainsKey(nume))
        {
            var existenta = _variabile[nume];
            erori.Add(EroareCompilare.Semantica(
                linie, coloana,
                $"declarație duplicată pentru variabila '{nume}' " +
                $"(declarată deja la linia {existenta.LinieDeclaratie}, " +
                $"coloana {existenta.ColoanaDeclaratie})"
            ));
            return;
        }
        
        _variabile[nume] = new Variabila(nume, tip, linie, coloana);
    }
    
    /// <summary>
    /// Declară și inițializează o variabilă.
    /// </summary>
    public void DeclaraVarabilaCuInitializare(
        string nume,
        TipDat tip,
        object valoare,
        int linie,
        int coloana,
        List<EroareCompilare> erori)
    {
        DeclararaVariabila(nume, tip, linie, coloana, erori);
        
        if (!_variabile.ContainsKey(nume))
            return; // A fost eroare la declarare
        
        SeteazaValoare(nume, valoare, linie, coloana, erori);
    }
    
    /// <summary>
    /// Setează valoarea unei variabile.
    /// VALIDĂRI:
    /// - Variabila există
    /// - Tipul valorii este compatibil
    /// </summary>
    public void SeteazaValoare(
        string nume,
        object valoare,
        int linie,
        int coloana,
        List<EroareCompilare> erori)
    {
        if (!_variabile.ContainsKey(nume))
        {
            erori.Add(EroareCompilare.Semantica(
                linie, coloana,
                $"variabila '{nume}' nu a fost declarată"
            ));
            return;
        }
        
        var variabila = _variabile[nume];
        
        // Verificare tip
        if (!variabila.ValidareaTipului(valoare))
        {
            string tipValoare = valoare?.GetType().Name ?? "null";
            erori.Add(EroareCompilare.Semantica(
                linie, coloana,
                $"tipul valorii '{tipValoare}' nu corespunde cu " +
                $"tipul variabilei '{variabila.Tip}'"
            ));
            return;
        }
        
        variabila.SeteazaValoare(valoare);
    }
    
    /// <summary>
    /// Obține valoarea unei variabile.
    /// VALIDĂRI:
    /// - Variabila există
    /// - Variabila a fost inițializată
    /// </summary>
    public object ObtineValoare(
        string nume,
        int linie,
        int coloana,
        List<EroareCompilare> erori)
    {
        if (!_variabile.ContainsKey(nume))
        {
            erori.Add(EroareCompilare.Semantica(
                linie, coloana,
                $"variabila '{nume}' nu a fost declarată"
            ));
            return null;
        }
        
        var variabila = _variabile[nume];
        
        if (!variabila.EsteInitializata)
        {
            erori.Add(EroareCompilare.Semantica(
                linie, coloana,
                $"variabila '{nume}' folosită înainte de inițializare " +
                $"(declarată la linia {variabila.LinieDeclaratie})"
            ));
            return null;
        }
        
        return variabila.Valoare;
    }
    
    /// <summary>
    /// Verifică dacă o variabilă există.
    /// </summary>
    public bool Exista(string nume) => _variabile.ContainsKey(nume);
    
    /// <summary>
    /// Obține informații despre o variabilă.
    /// </summary>
    public Variabila ObtineVariabila(string nume)
    {
        return _variabile.ContainsKey(nume) ? _variabile[nume] : null;
    }
    
    /// <summary>
    /// Afișează toate variabilele din tabel.
    /// </summary>
    public void AfiseazaVariabile()
    {
        Console.WriteLine("\n=== TABEL SIMBOLURI ===");
        
        if (_variabile.Count == 0)
        {
            Console.WriteLine("(gol)");
            return;
        }
        
        foreach (var variabila in _variabile.Values.OrderBy(v => v.LinieDeclaratie))
        {
            Console.WriteLine(variabila);
        }
    }
}
```

---

Continui cu Parser.cs și restul componentelor în următorul fișier...

