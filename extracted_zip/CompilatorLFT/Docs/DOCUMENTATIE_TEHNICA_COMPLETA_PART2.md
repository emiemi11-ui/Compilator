# 📚 DOCUMENTAȚIE TEHNICĂ COMPLETĂ - PARTEA 2
## Parser, Evaluator și Componente Finale

---

## 5.4 Parser.cs - Analiza Sintactică (CRITIC! CEA MAI COMPLEXĂ!)

**Fișier**: `Core/Parser.cs`
**Linii estimate**: ~600-700 linii
**Complexitate**: ⭐⭐⭐⭐⭐ (MAXIMĂ)

### 5.4.1 Algoritm General

Parser-ul folosește **Recursive Descent Parsing** cu **precedență operatori**:

```
INPUT:  Lista de tokeni de la Lexer
OUTPUT: Arbore Sintactic Abstract (AST)

METODA:
1. Începe de la rădăcină: ParseazaProgram()
2. Pentru fiecare instrucțiune:
   - Detectează tipul (declarație, atribuire, for, etc.)
   - Apelează metoda corespunzătoare
3. Pentru expresii, respectă precedența:
   - Nivel 4: + și - (precedență joasă)
   - Nivel 3: * și / (precedență medie)
   - Nivel 2: minus unar (precedență înaltă)
   - Nivel 1: paranteze și atomi (precedență maximă)
4. Construiește AST bottom-up
5. În paralel: validări semantice prin TabelSimboluri
```

### 5.4.2 Structura Clasei

```csharp
public class Parser
{
    private List<AtomLexical> _tokeni;
    private int _index;
    private TabelSimboluri _tabelSimboluri;
    private List<EroareCompilare> _erori;
    
    public Parser(List<AtomLexical> tokeni)
    {
        _tokeni = tokeni ?? throw new ArgumentNullException(nameof(tokeni));
        _index = 0;
        _tabelSimboluri = new TabelSimboluri();
        _erori = new List<EroareCompilare>();
    }
    
    // Proprietăți helper
    private AtomLexical AtomCurent => Varf(0);
    private AtomLexical AtomUrmator => Varf(1);
    
    private AtomLexical Varf(int offset)
    {
        int pozitie = _index + offset;
        return pozitie < _tokeni.Count ? _tokeni[pozitie] : _tokeni[_tokeni.Count - 1];
    }
    
    private AtomLexical ConsumaAtom()
    {
        var atom = AtomCurent;
        _index++;
        return atom;
    }
    
    private AtomLexical Verifica(TipAtomLexical tipAsteptat)
    {
        if (AtomCurent.Tip != tipAsteptat)
        {
            _erori.Add(EroareCompilare.Sintactica(
                AtomCurent.Linie, AtomCurent.Coloana,
                $"se aștepta '{tipAsteptat}' dar s-a găsit '{AtomCurent.Tip}'"
            ));
            
            // Returnează un token "dummy" pentru a continua parsing-ul
            return new AtomLexical(
                tipAsteptat, "", null,
                AtomCurent.Linie, AtomCurent.Coloana, AtomCurent.PozitieAbsoluta
            );
        }
        
        return ConsumaAtom();
    }
    
    public TabelSimboluri TabelSimboluri => _tabelSimboluri;
    public List<EroareCompilare> Erori => _erori;
}
```

### 5.4.3 Metode de Parsing - Instructiuni

```csharp
/// <summary>
/// Parsează programul complet.
/// Program ::= Instructiune*
/// </summary>
public ProgramComplet ParseazaProgram()
{
    var instructiuni = new List<Instructiune>();
    
    while (AtomCurent.Tip != TipAtomLexical.Terminator)
    {
        try
        {
            var instructiune = ParseazaInstructiune();
            instructiuni.Add(instructiune);
        }
        catch (Exception ex)
        {
            // Recuperare la următorul punct și virgulă
            while (AtomCurent.Tip != TipAtomLexical.PunctVirgula &&
                   AtomCurent.Tip != TipAtomLexical.Terminator)
            {
                ConsumaAtom();
            }
            
            if (AtomCurent.Tip == TipAtomLexical.PunctVirgula)
                ConsumaAtom();
        }
    }
    
    return new ProgramComplet(instructiuni);
}

/// <summary>
/// Parsează o instructiune (detectează tipul și delegă).
/// </summary>
private Instructiune ParseazaInstructiune()
{
    // Declarație: int/double/string
    if (AtomCurent.Tip == TipAtomLexical.CuvantCheieInt ||
        AtomCurent.Tip == TipAtomLexical.CuvantCheieDouble ||
        AtomCurent.Tip == TipAtomLexical.CuvantCheieString)
    {
        return ParseazaDeclaratie();
    }
    
    // For
    if (AtomCurent.Tip == TipAtomLexical.CuvantCheieFor)
    {
        return ParseazaFor();
    }
    
    // While
    if (AtomCurent.Tip == TipAtomLexical.CuvantCheieWhile)
    {
        return ParseazaWhile();
    }
    
    // If
    if (AtomCurent.Tip == TipAtomLexical.CuvantCheieIf)
    {
        return ParseazaIf();
    }
    
    // Bloc
    if (AtomCurent.Tip == TipAtomLexical.AcoladaDeschisa)
    {
        return ParseazaBloc();
    }
    
    // Atribuire sau expresie
    // Distingem: dacă avem "identificator =" → atribuire
    //            altfel → expresie
    if (AtomCurent.Tip == TipAtomLexical.Identificator &&
        AtomUrmator.Tip == TipAtomLexical.Egal)
    {
        return ParseazaAtribuire();
    }
    
    // Expresie simplă (pentru cerința 4: afișare arbore)
    return ParseazaInstructiuneExpresie();
}

/// <summary>
/// Parsează declarație.
/// Declaratie ::= TipBaza ListaDeclaratii ';'
/// ListaDeclaratii ::= Declarator (',' Declarator)*
/// Declarator ::= IDENTIFICATOR ('=' Expresie)?
/// </summary>
private InstructiuneDeclaratie ParseazaDeclaratie()
{
    var tipCuvantCheie = ConsumaAtom();
    TipDat tipDat = tipCuvantCheie.Tip switch
    {
        TipAtomLexical.CuvantCheieInt => TipDat.Int,
        TipAtomLexical.CuvantCheieDouble => TipDat.Double,
        TipAtomLexical.CuvantCheieString => TipDat.String,
        _ => TipDat.Necunoscut
    };
    
    var declaratori = new List<(AtomLexical nume, Expresie valoare)>();
    
    // Primul declarator
    var nume = Verifica(TipAtomLexical.Identificator);
    Expresie valoareInitiala = null;
    
    // Declarație în tabel
    _tabelSimboluri.DeclararaVariabila(
        nume.Text, tipDat, nume.Linie, nume.Coloana, _erori);
    
    // Verifică inițializare
    if (AtomCurent.Tip == TipAtomLexical.Egal)
    {
        ConsumaAtom(); // Consumă '='
        valoareInitiala = ParseazaExpresie();
        
        // Evaluează și setează valoarea (va fi făcut de evaluator)
    }
    
    declaratori.Add((nume, valoareInitiala));
    
    // Declaratori suplimentari
    while (AtomCurent.Tip == TipAtomLexical.Virgula)
    {
        ConsumaAtom(); // Consumă ','
        
        nume = Verifica(TipAtomLexical.Identificator);
        valoareInitiala = null;
        
        _tabelSimboluri.DeclararaVariabila(
            nume.Text, tipDat, nume.Linie, nume.Coloana, _erori);
        
        if (AtomCurent.Tip == TipAtomLexical.Egal)
        {
            ConsumaAtom();
            valoareInitiala = ParseazaExpresie();
        }
        
        declaratori.Add((nume, valoareInitiala));
    }
    
    var punctVirgula = Verifica(TipAtomLexical.PunctVirgula);
    
    return new InstructiuneDeclaratie(tipCuvantCheie, declaratori, punctVirgula);
}

/// <summary>
/// Parsează atribuire.
/// Atribuire ::= IDENTIFICATOR '=' Expresie ';'
/// </summary>
private InstructiuneAtribuire ParseazaAtribuire()
{
    var identificator = Verifica(TipAtomLexical.Identificator);
    
    // Verificare semantică: variabila trebuie să existe
    if (!_tabelSimboluri.Exista(identificator.Text))
    {
        _erori.Add(EroareCompilare.Semantica(
            identificator.Linie, identificator.Coloana,
            $"variabila '{identificator.Text}' nu a fost declarată"
        ));
    }
    
    var egal = Verifica(TipAtomLexical.Egal);
    var valoare = ParseazaExpresie();
    var punctVirgula = Verifica(TipAtomLexical.PunctVirgula);
    
    return new InstructiuneAtribuire(identificator, egal, valoare, punctVirgula);
}

/// <summary>
/// Parsează instrucțiune expresie.
/// InstructiuneExpresie ::= Expresie ';'
/// </summary>
private InstructiuneExpresie ParseazaInstructiuneExpresie()
{
    var expresie = ParseazaExpresie();
    var punctVirgula = Verifica(TipAtomLexical.PunctVirgula);
    
    return new InstructiuneExpresie(expresie, punctVirgula);
}

/// <summary>
/// Parsează for.
/// For ::= 'for' '(' (Declaratie | Atribuire | ';') Expresie? ';' (Atribuire | Expresie)? ')' Instructiune
/// </summary>
private InstructiuneFor ParseazaFor()
{
    var cuvantCheieFor = Verifica(TipAtomLexical.CuvantCheieFor);
    var parantezaDeschisa = Verifica(TipAtomLexical.ParantezaDeschisa);
    
    // Inițializare: poate fi declarație, atribuire sau nimic
    Instructiune initializare = null;
    
    if (AtomCurent.Tip == TipAtomLexical.CuvantCheieInt ||
        AtomCurent.Tip == TipAtomLexical.CuvantCheieDouble ||
        AtomCurent.Tip == TipAtomLexical.CuvantCheieString)
    {
        initializare = ParseazaDeclaratie();
    }
    else if (AtomCurent.Tip == TipAtomLexical.Identificator &&
             AtomUrmator.Tip == TipAtomLexical.Egal)
    {
        initializare = ParseazaAtribuire();
    }
    else if (AtomCurent.Tip == TipAtomLexical.PunctVirgula)
    {
        ConsumaAtom(); // Skip ';'
    }
    
    // Condiție: expresie opțională
    Expresie conditie = null;
    if (AtomCurent.Tip != TipAtomLexical.PunctVirgula)
    {
        conditie = ParseazaExpresie();
    }
    var punctVirgula1 = Verifica(TipAtomLexical.PunctVirgula);
    
    // Increment: atribuire sau expresie opțională
    Instructiune increment = null;
    if (AtomCurent.Tip != TipAtomLexical.ParantezaInchisa)
    {
        if (AtomCurent.Tip == TipAtomLexical.Identificator &&
            AtomUrmator.Tip == TipAtomLexical.Egal)
        {
            // Atribuire fără punct și virgulă în for
            var id = ConsumaAtom();
            var eq = ConsumaAtom();
            var val = ParseazaExpresie();
            increment = new InstructiuneAtribuire(id, eq, val, null);
        }
        else
        {
            var expr = ParseazaExpresie();
            increment = new InstructiuneExpresie(expr, null);
        }
    }
    
    var parantezaInchisa = Verifica(TipAtomLexical.ParantezaInchisa);
    
    // Corp
    var corp = ParseazaInstructiune();
    
    return new InstructiuneFor(
        cuvantCheieFor, parantezaDeschisa,
        initializare, conditie, punctVirgula1,
        increment, parantezaInchisa, corp
    );
}

/// <summary>
/// Parsează while.
/// While ::= 'while' '(' Expresie ')' Instructiune
/// </summary>
private InstructiuneWhile ParseazaWhile()
{
    var cuvantCheieWhile = Verifica(TipAtomLexical.CuvantCheieWhile);
    var parantezaDeschisa = Verifica(TipAtomLexical.ParantezaDeschisa);
    var conditie = ParseazaExpresie();
    var parantezaInchisa = Verifica(TipAtomLexical.ParantezaInchisa);
    var corp = ParseazaInstructiune();
    
    return new InstructiuneWhile(
        cuvantCheieWhile, parantezaDeschisa,
        conditie, parantezaInchisa, corp
    );
}

/// <summary>
/// Parsează if.
/// If ::= 'if' '(' Expresie ')' Instructiune ('else' Instructiune)?
/// </summary>
private InstructiuneIf ParseazaIf()
{
    var cuvantCheieIf = Verifica(TipAtomLexical.CuvantCheieIf);
    var parantezaDeschisa = Verifica(TipAtomLexical.ParantezaDeschisa);
    var conditie = ParseazaExpresie();
    var parantezaInchisa = Verifica(TipAtomLexical.ParantezaInchisa);
    var corpAdevarat = ParseazaInstructiune();
    
    // Else opțional
    AtomLexical cuvantCheieElse = null;
    Instructiune corpFals = null;
    
    if (AtomCurent.Tip == TipAtomLexical.CuvantCheieElse)
    {
        cuvantCheieElse = ConsumaAtom();
        corpFals = ParseazaInstructiune();
    }
    
    return new InstructiuneIf(
        cuvantCheieIf, parantezaDeschisa,
        conditie, parantezaInchisa,
        corpAdevarat, cuvantCheieElse, corpFals
    );
}

/// <summary>
/// Parsează bloc.
/// Bloc ::= '{' Instructiune* '}'
/// </summary>
private Bloc ParseazaBloc()
{
    var acoladaDeschisa = Verifica(TipAtomLexical.AcoladaDeschisa);
    var instructiuni = new List<Instructiune>();
    
    while (AtomCurent.Tip != TipAtomLexical.AcoladaInchisa &&
           AtomCurent.Tip != TipAtomLexical.Terminator)
    {
        instructiuni.Add(ParseazaInstructiune());
    }
    
    var acoladaInchisa = Verifica(TipAtomLexical.AcoladaInchisa);
    
    return new Bloc(acoladaDeschisa, instructiuni, acoladaInchisa);
}
```

### 5.4.4 Metode de Parsing - Expresii (CU PRECEDENȚĂ!)

```csharp
/// <summary>
/// Parsează expresie (nivel cel mai de jos în precedență).
/// Expresie ::= Termen (('+' | '-') Termen)*
/// </summary>
private Expresie ParseazaExpresie()
{
    return ParseazaTermen();
}

/// <summary>
/// Parsează termen (nivel 2 în precedență: + și -).
/// Termen ::= Factor (('+' | '-') Factor)*
/// </summary>
private Expresie ParseazaTermen()
{
    var stanga = ParseazaFactor();
    
    while (AtomCurent.Tip == TipAtomLexical.Plus ||
           AtomCurent.Tip == TipAtomLexical.Minus)
    {
        var operator = ConsumaAtom();
        var dreapta = ParseazaFactor();
        stanga = new ExpresieBinara(stanga, operator, dreapta);
    }
    
    return stanga;
}

/// <summary>
/// Parsează factor (nivel 3 în precedență: * și /).
/// Factor ::= Unara (('*' | '/') Unara)*
/// </summary>
private Expresie ParseazaFactor()
{
    var stanga = ParseazaUnara();
    
    while (AtomCurent.Tip == TipAtomLexical.Star ||
           AtomCurent.Tip == TipAtomLexical.Slash)
    {
        var operator = ConsumaAtom();
        var dreapta = ParseazaUnara();
        stanga = new ExpresieBinara(stanga, operator, dreapta);
    }
    
    return stanga;
}

/// <summary>
/// Parsează expresie unară (nivel 4 în precedență: minus unar).
/// Unara ::= ('-')? Atom
/// 
/// IMPORTANT: Plus unar (+) NU este permis conform cerințelor!
/// </summary>
private Expresie ParseazaUnara()
{
    // Minus unar
    if (AtomCurent.Tip == TipAtomLexical.Minus)
    {
        var operator = ConsumaAtom();
        var operand = ParseazaUnara(); // Recursiv pentru cazuri ca --5
        return new ExpresieUnara(operator, operand);
    }
    
    // Plus unar → EROARE
    if (AtomCurent.Tip == TipAtomLexical.Plus)
    {
        _erori.Add(EroareCompilare.Lexicala(
            AtomCurent.Linie, AtomCurent.Coloana,
            "operatorul plus unar (+) nu este permis"
        ));
        ConsumaAtom(); // Skip
        return ParseazaUnara();
    }
    
    return ParseazaAtom();
}

/// <summary>
/// Parsează atom (nivel cel mai înalt în precedență).
/// Atom ::= NUMAR_INTREG | NUMAR_ZECIMAL | STRING_LITERAL 
///        | IDENTIFICATOR | '(' Expresie ')'
/// </summary>
private Expresie ParseazaAtom()
{
    // Număr întreg
    if (AtomCurent.Tip == TipAtomLexical.NumarIntreg)
    {
        var numar = ConsumaAtom();
        return new ExpresieNumerica(numar);
    }
    
    // Număr zecimal
    if (AtomCurent.Tip == TipAtomLexical.NumarZecimal)
    {
        var numar = ConsumaAtom();
        return new ExpresieNumerica(numar);
    }
    
    // String literal
    if (AtomCurent.Tip == TipAtomLexical.StringLiteral)
    {
        var str = ConsumaAtom();
        return new ExpresieString(str);
    }
    
    // Identificator (variabilă)
    if (AtomCurent.Tip == TipAtomLexical.Identificator)
    {
        var id = ConsumaAtom();
        
        // Verificare semantică: variabila trebuie să existe
        if (!_tabelSimboluri.Exista(id.Text))
        {
            _erori.Add(EroareCompilare.Semantica(
                id.Linie, id.Coloana,
                $"variabila '{id.Text}' nu a fost declarată"
            ));
        }
        
        return new ExpresieIdentificator(id);
    }
    
    // Expresie cu paranteze
    if (AtomCurent.Tip == TipAtomLexical.ParantezaDeschisa)
    {
        var parantezaDeschisa = ConsumaAtom();
        var expresie = ParseazaExpresie();
        var parantezaInchisa = Verifica(TipAtomLexical.ParantezaInchisa);
        
        return new ExpresieCuParanteze(parantezaDeschisa, expresie, parantezaInchisa);
    }
    
    // Altceva → eroare
    _erori.Add(EroareCompilare.Sintactica(
        AtomCurent.Linie, AtomCurent.Coloana,
        $"se aștepta expresie, dar s-a găsit '{AtomCurent.Tip}'"
    ));
    
    // Returnează expresie "dummy" pentru a continua
    return new ExpresieNumerica(
        new AtomLexical(TipAtomLexical.NumarIntreg, "0", 0,
            AtomCurent.Linie, AtomCurent.Coloana, AtomCurent.PozitieAbsoluta)
    );
}
```

### 5.4.5 Exemplu Complet de Parsing

```csharp
// INPUT
string cod = "int a = 3 + 4 * 5;";

// Pas 1: Lexer
var lexer = new Lexer(cod);
var tokeni = lexer.Tokenizeaza();
// Tokeni: [CuvantCheieInt, Identificator(a), Egal, NumarIntreg(3), 
//          Plus, NumarIntreg(4), Star, NumarIntreg(5), PunctVirgula, Terminator]

// Pas 2: Parser
var parser = new Parser(tokeni);
var program = parser.ParseazaProgram();

// Pas 3: Arborele sintactic creat
// ProgramComplet
//   └─InstructiuneDeclaratie
//       ├─CuvantCheieInt "int"
//       ├─Identificator "a"
//       ├─Egal "="
//       ├─ExpresieBinara(+)
//       │   ├─ExpresieNumerica(3)
//       │   ├─Plus "+"
//       │   └─ExpresieBinara(*)          ← * evaluată înainte de +
//       │       ├─ExpresieNumerica(4)
//       │       ├─Star "*"
//       │       └─ExpresieNumerica(5)
//       └─PunctVirgula ";"

// OBSERVAȚIE: Precedența este CORECTĂ!
// 3 + 4 * 5 = 3 + (4 * 5) = 3 + 20 = 23
```

---

## 5.5 Evaluator.cs - Evaluarea Expresiilor

**Fișier**: `Core/Evaluator.cs`
**Linii estimate**: ~400 linii
**Complexitate**: ⭐⭐⭐⭐ (Înaltă)

### 5.5.1 Structura Clasei

```csharp
public class Evaluator
{
    private TabelSimboluri _tabelSimboluri;
    private List<EroareCompilare> _erori;
    
    public Evaluator(TabelSimboluri tabelSimboluri)
    {
        _tabelSimboluri = tabelSimboluri ?? throw new ArgumentNullException();
        _erori = new List<EroareCompilare>();
    }
    
    /// <summary>
    /// Evaluează programul complet.
    /// </summary>
    public void EvalueazaProgram(ProgramComplet program)
    {
        foreach (var instructiune in program.Instructiuni)
        {
            EvalueazaInstructiune(instructiune);
        }
    }
    
    public List<EroareCompilare> Erori => _erori;
}
```

### 5.5.2 Evaluare Instrucțiuni

```csharp
/// <summary>
/// Evaluează o instrucțiune (visitor pattern).
/// </summary>
private void EvalueazaInstructiune(Instructiune instructiune)
{
    switch (instructiune)
    {
        case InstructiuneDeclaratie declaratie:
            EvalueazaDeclaratie(declaratie);
            break;
            
        case InstructiuneAtribuire atribuire:
            EvalueazaAtribuire(atribuire);
            break;
            
        case InstructiuneExpresie expresie:
            EvalueazaInstructiuneExpresie(expresie);
            break;
            
        case InstructiuneFor forLoop:
            EvalueazaFor(forLoop);
            break;
            
        case InstructiuneWhile whileLoop:
            EvalueazaWhile(whileLoop);
            break;
            
        case InstructiuneIf ifStmt:
            EvalueazaIf(ifStmt);
            break;
            
        case Bloc bloc:
            EvalueazaBloc(bloc);
            break;
            
        default:
            throw new NotImplementedException(
                $"Evaluare pentru {instructiune.GetType().Name} nu este implementată");
    }
}

private void EvalueazaDeclaratie(InstructiuneDeclaratie declaratie)
{
    foreach (var (nume, valoareInitiala) in declaratie.Declaratori)
    {
        if (valoareInitiala != null)
        {
            // Evaluează expresia inițială
            var valoare = EvalueazaExpresie(valoareInitiala);
            
            if (valoare != null)
            {
                // Setează în tabelul de simboluri
                _tabelSimboluri.SeteazaValoare(
                    nume.Text, valoare,
                    nume.Linie, nume.Coloana, _erori
                );
            }
        }
    }
}

private void EvalueazaAtribuire(InstructiuneAtribuire atribuire)
{
    var valoare = EvalueazaExpresie(atribuire.Valoare);
    
    if (valoare != null)
    {
        _tabelSimboluri.SeteazaValoare(
            atribuire.Identificator.Text, valoare,
            atribuire.Identificator.Linie, atribuire.Identificator.Coloana,
            _erori
        );
    }
}

private void EvalueazaInstructiuneExpresie(InstructiuneExpresie instructiune)
{
    // Doar evaluează expresia (pentru cerința 4: afișare arbore)
    var rezultat = EvalueazaExpresie(instructiune.Expresie);
    
    // Afișează rezultatul
    if (rezultat != null)
    {
        Console.WriteLine($"Rezultat: {FormatareValoare(rezultat)}");
    }
}

private void EvalueazaFor(InstructiuneFor forLoop)
{
    // Inițializare
    if (forLoop.Initializare != null)
        EvalueazaInstructiune(forLoop.Initializare);
    
    // Buclă
    while (true)
    {
        // Verifică condiția
        if (forLoop.Conditie != null)
        {
            var conditie = EvalueazaExpresie(forLoop.Conditie);
            if (!EsteAdevarat(conditie))
                break;
        }
        
        // Execută corp
        EvalueazaInstructiune(forLoop.Corp);
        
        // Increment
        if (forLoop.Increment != null)
            EvalueazaInstructiune(forLoop.Increment);
    }
}

private void EvalueazaWhile(InstructiuneWhile whileLoop)
{
    while (true)
    {
        // Verifică condiția
        var conditie = EvalueazaExpresie(whileLoop.Conditie);
        if (!EsteAdevarat(conditie))
            break;
        
        // Execută corp
        EvalueazaInstructiune(whileLoop.Corp);
    }
}

private void EvalueazaIf(InstructiuneIf ifStmt)
{
    var conditie = EvalueazaExpresie(ifStmt.Conditie);
    
    if (EsteAdevarat(conditie))
    {
        EvalueazaInstructiune(ifStmt.CorpAdevarat);
    }
    else if (ifStmt.CorpFals != null)
    {
        EvalueazaInstructiune(ifStmt.CorpFals);
    }
}

private void EvalueazaBloc(Bloc bloc)
{
    foreach (var instructiune in bloc.Instructiuni)
    {
        EvalueazaInstructiune(instructiune);
    }
}
```

### 5.5.3 Evaluare Expresii (CU CONVERSII TIPURI!)

```csharp
/// <summary>
/// Evaluează o expresie și returnează valoarea.
/// Returnează: int, double sau string (sau null dacă eroare)
/// </summary>
private object EvalueazaExpresie(Expresie expresie)
{
    switch (expresie)
    {
        case ExpresieNumerica numerica:
            return numerica.Numar.Valoare;
            
        case ExpresieString str:
            return str.ValoareString.Valoare;
            
        case ExpresieIdentificator id:
            return _tabelSimboluri.ObtineValoare(
                id.Identificator.Text,
                id.Identificator.Linie,
                id.Identificator.Coloana,
                _erori
            );
            
        case ExpresieBinara binara:
            return EvalueazaExpresieBinara(binara);
            
        case ExpresieUnara unara:
            return EvalueazaExpresieUnara(unara);
            
        case ExpresieCuParanteze cuParanteze:
            return EvalueazaExpresie(cuParanteze.Expresie);
            
        default:
            throw new NotImplementedException(
                $"Evaluare pentru {expresie.GetType().Name} nu este implementată");
    }
}

private object EvalueazaExpresieBinara(ExpresieBinara binara)
{
    // Evaluează operanzii
    var valoareStanga = EvalueazaExpresie(binara.Stanga);
    var valoareDreapta = EvalueazaExpresie(binara.Dreapta);
    
    if (valoareStanga == null || valoareDreapta == null)
        return null; // Eroare în evaluare
    
    var op = binara.Operator.Tip;
    
    // CAZUL 1: STRING + STRING → concatenare
    if (valoareStanga is string s1 && valoareDreapta is string s2)
    {
        if (op == TipAtomLexical.Plus)
            return s1 + s2;
        
        _erori.Add(EroareCompilare.Semantica(
            binara.Operator.Linie, binara.Operator.Coloana,
            $"operația '{binara.Operator.Text}' nu este suportată pentru string-uri"
        ));
        return null;
    }
    
    // CAZUL 2: STRING + (INT|DOUBLE) → EROARE
    if ((valoareStanga is string || valoareDreapta is string) &&
        !(valoareStanga is string && valoareDreapta is string))
    {
        _erori.Add(EroareCompilare.Semantica(
            binara.Operator.Linie, binara.Operator.Coloana,
            "nu se poate combina string cu int/double"
        ));
        return null;
    }
    
    // CAZUL 3: Operații aritmetice pe numere
    // Conversii: int + int → int
    //            double + double → double
    //            int + double → double (conversie implicită)
    
    bool esteDouble = valoareStanga is double || valoareDreapta is double;
    
    double v1 = Convert.ToDouble(valoareStanga);
    double v2 = Convert.ToDouble(valoareDreapta);
    
    double rezultat = op switch
    {
        TipAtomLexical.Plus => v1 + v2,
        TipAtomLexical.Minus => v1 - v2,
        TipAtomLexical.Star => v1 * v2,
        TipAtomLexical.Slash => ImpărțireSigură(v1, v2, binara.Operator),
        
        // Operatori relaționali
        TipAtomLexical.MaiMic => v1 < v2 ? 1.0 : 0.0,
        TipAtomLexical.MaiMare => v1 > v2 ? 1.0 : 0.0,
        TipAtomLexical.MaiMicEgal => v1 <= v2 ? 1.0 : 0.0,
        TipAtomLexical.MaiMareEgal => v1 >= v2 ? 1.0 : 0.0,
        TipAtomLexical.EgalEgal => Math.Abs(v1 - v2) < 1e-10 ? 1.0 : 0.0,
        TipAtomLexical.Diferit => Math.Abs(v1 - v2) >= 1e-10 ? 1.0 : 0.0,
        
        _ => throw new NotImplementedException($"Operator {op} nu este implementat")
    };
    
    // Returnează int sau double în funcție de tipul operanzilor
    if (esteDouble)
        return rezultat;
    else
        return (int)rezultat; // Truncare pentru int
}

private double ImpărțireSigură(double v1, double v2, AtomLexical operator)
{
    if (Math.Abs(v2) < 1e-10) // Împărțire la zero
    {
        _erori.Add(EroareCompilare.Semantica(
            operator.Linie, operator.Coloana,
            "împărțire la zero"
        ));
        return 0.0;
    }
    
    return v1 / v2;
}

private object EvalueazaExpresieUnara(ExpresieUnara unara)
{
    var valoare = EvalueazaExpresie(unara.Operand);
    
    if (valoare == null)
        return null;
    
    if (unara.Operator.Tip == TipAtomLexical.Minus)
    {
        if (valoare is int i)
            return -i;
        if (valoare is double d)
            return -d;
        
        _erori.Add(EroareCompilare.Semantica(
            unara.Operator.Linie, unara.Operator.Coloana,
            "operatorul minus unar nu se poate aplica pentru acest tip"
        ));
    }
    
    return null;
}

/// <summary>
/// Verifică dacă o valoare este adevărată (pentru if/while).
/// Convenție: 0 = fals, orice altceva = adevărat
/// </summary>
private bool EsteAdevarat(object valoare)
{
    if (valoare is int i)
        return i != 0;
    if (valoare is double d)
        return Math.Abs(d) > 1e-10;
    if (valoare is string s)
        return !string.IsNullOrEmpty(s);
    
    return false;
}

private string FormatareValoare(object valoare)
{
    if (valoare is string s)
        return $"\"{s}\"";
    return valoare?.ToString() ?? "null";
}
```

---

## 5.6 Program.cs - Punctul de Intrare

**Fișier**: `Program.cs`
**Linii estimate**: ~200 linii

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CompilatorLFT.Core;
using CompilatorLFT.Models;
using CompilatorLFT.Models.Instructiuni;
using CompilatorLFT.Utils;

namespace CompilatorLFT
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine("   COMPILATOR LFT - Proiect Academic");
            Console.WriteLine("═══════════════════════════════════════════\n");
            
            string cod;
            
            // Opțiune 1: Fișier ca argument
            if (args.Length > 0)
            {
                string caleFisier = args[0];
                cod = CitesteDinFisier(caleFisier);
            }
            // Opțiune 2: Meniu interactiv
            else
            {
                Console.WriteLine("Selectați modul de introducere:");
                Console.WriteLine("1. Citire din fișier");
                Console.WriteLine("2. Introducere manuală");
                Console.Write("\nAlegere: ");
                
                string alegere = Console.ReadLine();
                
                if (alegere == "1")
                {
                    Console.Write("Calea către fișier: ");
                    string cale = Console.ReadLine();
                    cod = CitesteDinFisier(cale);
                }
                else
                {
                    Console.WriteLine("\nIntroduceți codul (linie goală pentru sfârșit):");
                    cod = CitesteDinConsola();
                }
            }
            
            if (string.IsNullOrWhiteSpace(cod))
            {
                Console.WriteLine("Eroare: Nu s-a introdus cod!");
                return;
            }
            
            // ETAPA 1: ANALIZA LEXICALĂ
            Console.WriteLine("\n─── ANALIZA LEXICALĂ ───");
            var lexer = new Lexer(cod);
            var tokeni = lexer.Tokenizeaza();
            
            if (lexer.Erori.Any())
            {
                AfiseazaErori(lexer.Erori);
                return;
            }
            
            Console.WriteLine($"✓ {tokeni.Count} tokeni recunoscuți");
            
            // ETAPA 2: ANALIZA SINTACTICĂ
            Console.WriteLine("\n─── ANALIZA SINTACTICĂ ───");
            var parser = new Parser(tokeni);
            var program = parser.ParseazaProgram();
            
            if (parser.Erori.Any())
            {
                AfiseazaErori(parser.Erori);
                return;
            }
            
            Console.WriteLine("✓ Arbore sintactic construit");
            
            // ETAPA 3: AFIȘARE ARBORE (pentru instrucțiuni cu expresii)
            Console.WriteLine("\n─── ARBORE SINTACTIC ───");
            AfiseazaArboriPentruExpresii(program);
            
            // ETAPA 4: EVALUARE
            Console.WriteLine("\n─── EVALUARE ───");
            var evaluator = new Evaluator(parser.TabelSimboluri);
            evaluator.EvalueazaProgram(program);
            
            if (evaluator.Erori.Any())
            {
                AfiseazaErori(evaluator.Erori);
            }
            
            // ETAPA 5: AFIȘARE TABEL SIMBOLURI
            Console.WriteLine("\n─── TABEL SIMBOLURI ───");
            parser.TabelSimboluri.AfiseazaVariabile();
            
            Console.WriteLine("\n═══════════════════════════════════════════");
            Console.WriteLine("   Compilare finalizată cu succes!");
            Console.WriteLine("═══════════════════════════════════════════");
        }
        
        static string CitesteDinFisier(string cale)
        {
            try
            {
                if (!File.Exists(cale))
                {
                    Console.WriteLine($"Eroare: Fișierul '{cale}' nu există!");
                    Environment.Exit(1);
                }
                
                return File.ReadAllText(cale);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la citirea fișierului: {ex.Message}");
                Environment.Exit(1);
                return null;
            }
        }
        
        static string CitesteDinConsola()
        {
            var linii = new List<string>();
            
            while (true)
            {
                string linie = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(linie))
                    break;
                
                linii.Add(linie);
            }
            
            return string.Join(Environment.NewLine, linii);
        }
        
        static void AfiseazaErori(List<EroareCompilare> erori)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n╔═══ ERORI ═══╗");
            
            foreach (var eroare in erori)
            {
                Console.WriteLine(eroare.ToStringCuContext());
            }
            
            Console.WriteLine($"\nTotal: {erori.Count} erori");
            Console.ResetColor();
        }
        
        static void AfiseazaArboriPentruExpresii(ProgramComplet program)
        {
            foreach (var instructiune in program.Instructiuni)
            {
                // Conform cerințelor: afișăm arbore DOAR pentru instrucțiuni cu expresii
                if (instructiune is InstructiuneExpresie instrExpr)
                {
                    Console.WriteLine("\nExpresie evaluată:");
                    instrExpr.Expresie.AfiseazaArbore();
                    Console.WriteLine();
                }
            }
        }
    }
}
```

### 5.7 CititorFisier.cs - Utilitati

**Fișier**: `Utils/CititorFisier.cs`

```csharp
using System;
using System.IO;

namespace CompilatorLFT.Utils
{
    /// <summary>
    /// Utilități pentru citirea fișierelor.
    /// </summary>
    public static class CititorFisier
    {
        /// <summary>
        /// Citește conținutul unui fișier.
        /// </summary>
        public static string Citeste(string cale)
        {
            if (string.IsNullOrWhiteSpace(cale))
                throw new ArgumentException("Calea nu poate fi goală", nameof(cale));
            
            if (!File.Exists(cale))
                throw new FileNotFoundException($"Fișierul nu există: {cale}");
            
            return File.ReadAllText(cale);
        }
        
        /// <summary>
        /// Verifică dacă un fișier există.
        /// </summary>
        public static bool Exista(string cale)
        {
            return !string.IsNullOrWhiteSpace(cale) && File.Exists(cale);
        }
    }
}
```

---

## 6. FIȘIER PROIECT .NET

**Fișier**: `CompilatorLFT.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net6.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <RootNamespace>CompilatorLFT</RootNamespace>
    <AssemblyName>CompilatorLFT</AssemblyName>
    <Version>1.0.0</Version>
    <Authors>Student LFT</Authors>
    <Company>Universitatea Alexandru Ioan Cuza Iași</Company>
    <Product>Compilator LFT</Product>
    <Description>Compilator academic pentru subset de limbaj C#</Description>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="System.Text.RegularExpressions" Version="4.3.1" />
  </ItemGroup>

</Project>
```

---

## 7. EXEMPLE COMPLETE DE UTILIZARE

### Exemplu 1: Declarații și Atribuiri Simple

```csharp
// INPUT
int a, b, c=8;
a=6;
b=4;
c/b;
a+b+c;

// OUTPUT
─── ANALIZA LEXICALĂ ───
✓ 29 tokeni recunoscuți

─── ANALIZA SINTACTICĂ ───
✓ Arbore sintactic construit

─── ARBORE SINTACTIC ───

Expresie evaluată:
└──ExpresieBinara
    ├──ExpresieIdentificator c
    ├──Slash /
    └──ExpresieIdentificator b
Rezultat: 2

Expresie evaluată:
└──ExpresieBinara
    ├──ExpresieBinara
    │   ├──ExpresieIdentificator a
    │   ├──Plus +
    │   └──ExpresieIdentificator b
    ├──Plus +
    └──ExpresieIdentificator c
Rezultat: 18

─── TABEL SIMBOLURI ───
int a = 6
int b = 4
int c = 8
```

### Exemplu 2: Conversii Tipuri

```csharp
// INPUT
int a = 5;
double b = 2.5;
double c = a + b;
int d = a * b;

// OUTPUT
─── EVALUARE ───
Conversie implicită: int → double
c = 7.5

Truncare: double → int
d = 12  // (5 * 2.5 = 12.5 → 12)

─── TABEL SIMBOLURI ───
int a = 5
double b = 2.5
double c = 7.5
int d = 12
```

### Exemplu 3: Erori Semantice

```csharp
// INPUT
int a = 5;
string s = "hello";
int result = a + s;

// OUTPUT
╔═══ ERORI ═══╗
la linia 3, coloana 18: eroare semantică - nu se poate combina string cu int/double
  Context: int result = a + s;
                         ^
Total: 1 erori
```

---

## 8. TESTARE

Creați `Tests/TestSuite.cs` cu 20+ teste:

```csharp
// Test 1: Declarații simple
[Test] void TestDeclaratiiSimple()
// Test 2: Precedență operatori
[Test] void TestPrecedenta()
// Test 3: Conversii tipuri
[Test] void TestConversii()
// Test 4: Împărțire la zero
[Test] void TestImpartireLaZero()
// ... și așa mai departe
```

---

## FIN DOCUMENTAȚIE PARTEA 2

Această documentație conține TOTUL pentru finalizarea proiectului!
