# 📘 DOCUMENTAȚIE MASTER - COMPILATOR LFT (PARTEA 2)
## Implementare TabelSimboluri, Evaluator, Program Principal

---

## 5. IMPLEMENTARE TABEL SIMBOLURI

### 5.1 Specificație Funcțională

```
SCOP:
- Stochează informații despre fiecare variabilă declarată
- Validări semantice: duplicat, nedeclarată, neinițializată
- Suport pentru citire/scriere valori cu verificare tipuri

OPERAȚII PRINCIPALE:
- Adauga(nume, tip, linie, coloana) → Declarație nouă
- Exista(nume) → bool
- Obtine(nume) → Variabila
- SeteazaValoare(nume, valoare, linie, coloana) → Cu validări
- ObțineValoare(nume, linie, coloana) → Cu verificare inițializare
```

### 5.2 Pseudocod Complet

```
CLASA TabelSimboluri:
  
  CAMPURI PRIVATE:
    _variabile: Dictionary<string, Variabila>
  
  CONSTRUCTOR():
    _variabile = new Dictionary<string, Variabila>()
  
  METODĂ PUBLICĂ Adauga(nume, tip, linie, coloana, erori) -> bool:
    // Verificare duplicat
    IF _variabile.ContainsKey(nume):
      erori.Add(EroareCompilare.Semantica(linie, coloana,
                $"declarație duplicată pentru variabila '{nume}'"))
      RETURN false
    
    var = new Variabila(nume, tip, linie, coloana)
    _variabile[nume] = var
    RETURN true
  
  METODĂ PUBLICĂ Exista(nume) -> bool:
    RETURN _variabile.ContainsKey(nume)
  
  METODĂ PUBLICĂ Obtine(nume) -> Variabila:
    IF _variabile.ContainsKey(nume):
      RETURN _variabile[nume]
    RETURN null
  
  METODĂ PUBLICĂ SeteazaValoare(nume, valoare, linie, coloana, erori) -> bool:
    // Verificare existență
    IF NOT _variabile.ContainsKey(nume):
      erori.Add(EroareCompilare.Semantica(linie, coloana,
                $"variabila '{nume}' nu a fost declarată"))
      RETURN false
    
    var = _variabile[nume]
    
    // Verificare compatibilitate tipuri
    IF NOT var.ValidareaTipului(valoare):
      tipValoare = valoare?.GetType().Name ?? "null"
      erori.Add(EroareCompilare.Semantica(linie, coloana,
                $"incompatibilitate tipuri: nu se poate atribui {tipValoare} " +
                $"la variabila de tip {var.Tip}"))
      RETURN false
    
    var.SeteazaValoare(valoare)
    RETURN true
  
  METODĂ PUBLICĂ ObțineValoare(nume, linie, coloana, erori) -> object:
    // Verificare existență
    IF NOT _variabile.ContainsKey(nume):
      erori.Add(EroareCompilare.Semantica(linie, coloana,
                $"variabila '{nume}' nu a fost declarată"))
      RETURN null
    
    var = _variabile[nume]
    
    // Verificare inițializare
    IF NOT var.EsteInitializata:
      erori.Add(EroareCompilare.Semantica(linie, coloana,
                $"variabila '{nume}' folosită înainte de inițializare"))
      RETURN null
    
    RETURN var.Valoare
  
  METODĂ PUBLICĂ AfiseazaVariabile():
    Console.WriteLine("\n=== TABEL SIMBOLURI ===")
    
    FOREACH (nume, var) IN _variabile:
      Console.WriteLine($"{var.Tip} {var.Nume} = {FormatareValoare(var.Valoare)}")
  
  METODĂ PRIVATĂ FormatareValoare(valoare) -> string:
    IF valoare == null:
      RETURN "(neinițializată)"
    
    IF valoare is string str:
      RETURN $"\"{str}\""
    
    RETURN valoare.ToString()
```

---

## 6. IMPLEMENTARE EVALUATOR

### 6.1 Specificație Funcțională

```
SCOP:
- Evaluează expresii și returnează rezultatul
- Execută instrucțiuni (atribuiri, for, while, if)
- Implementează conversiile de tipuri (int ↔ double)
- Detectează erori runtime (div by zero, tipuri incompatibile)

PATTERN: Visitor Pattern pentru traversare AST

REGULI CONVERSIE:
int + int → int
double + double → double
int + double → double (int promovat)
string + string → string (doar +, altfel eroare)
string + (int|double) → EROARE
```

### 6.2 Pseudocod Complet

```
CLASA Evaluator:
  
  CAMPURI PRIVATE:
    _tabelSimboluri: TabelSimboluri
    _erori: List<EroareCompilare>
  
  CONSTRUCTOR(tabelSimboluri):
    _tabelSimboluri = tabelSimboluri
    _erori = new List<EroareCompilare>()
  
  PROPRIETĂȚI:
    Erori: IEnumerable<EroareCompilare> (readonly)
  
  // ==================== EVALUARE EXPRESII ====================
  
  METODĂ PUBLICĂ EvalueazaExpresie(expr: Expresie) -> object:
    // LITERAL NUMERIC
    IF expr is ExpresieNumerica num:
      RETURN num.Numar.Valoare  // int sau double
    
    // LITERAL STRING
    IF expr is ExpresieString str:
      RETURN str.ValoareString.Valoare  // string
    
    // IDENTIFICATOR (variabilă)
    IF expr is ExpresieIdentificator id:
      valoare = _tabelSimboluri.ObțineValoare(
                  id.Identificator.Text,
                  id.Identificator.Linie,
                  id.Identificator.Coloana,
                  _erori)
      RETURN valoare
    
    // EXPRESIE UNARĂ (minus unar)
    IF expr is ExpresieUnara unara:
      operand = EvalueazaExpresie(unara.Operand)
      
      IF operand == null:
        RETURN null
      
      IF operand is int i:
        RETURN -i
      
      IF operand is double d:
        RETURN -d
      
      // Eroare: minus unar pe string
      _erori.Add(EroareCompilare.Semantica(
                   unara.Operator.Linie, unara.Operator.Coloana,
                   $"operatorul '-' unar nu se poate aplica pe tip {operand.GetType().Name}"))
      RETURN null
    
    // EXPRESIE BINARĂ
    IF expr is ExpresieBinara binara:
      RETURN EvalueazaExpresieBinara(binara)
    
    // EXPRESIE CU PARANTEZE
    IF expr is ExpresieCuParanteze paranteze:
      RETURN EvalueazaExpresie(paranteze.Expresie)
    
    // Altceva - eroare
    _erori.Add(EroareCompilare.Semantica(1, 1,
                 "expresie de tip necunoscut"))
    RETURN null
  
  // ==================== EVALUARE EXPRESIE BINARĂ ====================
  
  METODĂ PRIVATĂ EvalueazaExpresieBinara(binara: ExpresieBinara) -> object:
    stanga = EvalueazaExpresie(binara.Stanga)
    dreapta = EvalueazaExpresie(binara.Dreapta)
    
    IF stanga == null OR dreapta == null:
      RETURN null
    
    op = binara.Operator
    
    // ==================== OPERAȚII ARITMETICE ====================
    
    IF op.Tip IN {Plus, Minus, Star, Slash}:
      RETURN EvalueazaOperatieAritmetica(stanga, op, dreapta)
    
    // ==================== OPERAȚII RELAȚIONALE ====================
    
    IF op.Tip IN {MaiMic, MaiMare, MaiMicEgal, MaiMareEgal, EgalEgal, Diferit}:
      RETURN EvalueazaOperatieRelationala(stanga, op, dreapta)
    
    // Operator necunoscut
    _erori.Add(EroareCompilare.Semantica(op.Linie, op.Coloana,
                 $"operator necunoscut '{op.Text}'"))
    RETURN null
  
  // ==================== OPERAȚII ARITMETICE ====================
  
  METODĂ PRIVATĂ EvalueazaOperatieAritmetica(stanga, op, dreapta) -> object:
    // STRING + STRING (doar concatenare)
    IF stanga is string str1 AND dreapta is string str2:
      IF op.Tip == Plus:
        RETURN str1 + str2
      ELSE:
        _erori.Add(EroareCompilare.Semantica(op.Linie, op.Coloana,
                     $"operația '{op.Text}' nu este suportată pentru string-uri " +
                     "(doar + pentru concatenare)"))
        RETURN null
    
    // STRING + (INT|DOUBLE) → EROARE
    IF (stanga is string OR dreapta is string):
      _erori.Add(EroareCompilare.Semantica(op.Linie, op.Coloana,
                   "incompatibilitate tipuri: nu se poate combina string cu număr"))
      RETURN null
    
    // INT + INT → INT
    IF stanga is int i1 AND dreapta is int i2:
      SWITCH op.Tip:
        CASE Plus:
          // Verificare overflow
          TRY:
            RETURN checked(i1 + i2)
          CATCH OverflowException:
            _erori.Add(EroareCompilare.Semantica(op.Linie, op.Coloana,
                         "overflow la adunare întregi"))
            RETURN null
        
        CASE Minus:
          TRY:
            RETURN checked(i1 - i2)
          CATCH OverflowException:
            _erori.Add(EroareCompilare.Semantica(op.Linie, op.Coloana,
                         "overflow la scădere întregi"))
            RETURN null
        
        CASE Star:
          TRY:
            RETURN checked(i1 * i2)
          CATCH OverflowException:
            _erori.Add(EroareCompilare.Semantica(op.Linie, op.Coloana,
                         "overflow la înmulțire întregi"))
            RETURN null
        
        CASE Slash:
          IF i2 == 0:
            _erori.Add(EroareCompilare.Semantica(op.Linie, op.Coloana,
                         "împărțire la zero"))
            RETURN null
          RETURN i1 / i2
    
    // DOUBLE + DOUBLE → DOUBLE
    IF stanga is double d1 AND dreapta is double d2:
      SWITCH op.Tip:
        CASE Plus:  RETURN d1 + d2
        CASE Minus: RETURN d1 - d2
        CASE Star:  RETURN d1 * d2
        CASE Slash:
          IF Math.Abs(d2) < 1e-10:  // Aproape zero
            _erori.Add(EroareCompilare.Semantica(op.Linie, op.Coloana,
                         "împărțire la zero"))
            RETURN null
          RETURN d1 / d2
    
    // INT + DOUBLE → DOUBLE (conversie implicită)
    IF stanga is int i AND dreapta is double d:
      RETURN EvalueazaOperatieAritmetica((double)i, op, d)
    
    // DOUBLE + INT → DOUBLE
    IF stanga is double d AND dreapta is int i:
      RETURN EvalueazaOperatieAritmetica(d, op, (double)i)
    
    // Tipuri incompatibile
    _erori.Add(EroareCompilare.Semantica(op.Linie, op.Coloana,
                 $"incompatibilitate tipuri: {stanga.GetType().Name} {op.Text} {dreapta.GetType().Name}"))
    RETURN null
  
  // ==================== OPERAȚII RELAȚIONALE ====================
  
  METODĂ PRIVATĂ EvalueazaOperatieRelationala(stanga, op, dreapta) -> bool?:
    // Comparații doar între numere
    IF NOT (EstNumar(stanga) AND EstNumar(dreapta)):
      _erori.Add(EroareCompilare.Semantica(op.Linie, op.Coloana,
                   "operatori relaționali se aplică doar pe numere"))
      RETURN null
    
    // Convertire la double pentru comparație uniformă
    val1 = ConverteLaDouble(stanga)
    val2 = ConverteLaDouble(dreapta)
    
    SWITCH op.Tip:
      CASE MaiMic:       RETURN val1 < val2
      CASE MaiMare:      RETURN val1 > val2
      CASE MaiMicEgal:   RETURN val1 <= val2
      CASE MaiMareEgal:  RETURN val1 >= val2
      CASE EgalEgal:     RETURN Math.Abs(val1 - val2) < 1e-10
      CASE Diferit:      RETURN Math.Abs(val1 - val2) >= 1e-10
    
    RETURN null
  
  METODĂ PRIVATĂ EstNumar(valoare) -> bool:
    RETURN valoare is int OR valoare is double
  
  METODĂ PRIVATĂ ConverteLaDouble(valoare) -> double:
    IF valoare is int i:
      RETURN (double)i
    IF valoare is double d:
      RETURN d
    THROW new Exception("Valoare nu este număr")
  
  // ==================== EXECUȚIE INSTRUCȚIUNI ====================
  
  METODĂ PUBLICĂ ExecuțăProgram(program: Program):
    FOREACH instr IN program.Instructiuni:
      ExecuțăInstructiune(instr)
  
  METODĂ PRIVATĂ ExecuțăInstructiune(instr: Instructiune):
    // DECLARAȚIE
    IF instr is InstructiuneDeclaratie decl:
      // Declarația a fost deja procesată în Parser
      // Aici doar evaluăm inițializările
      
      tipDat = ConvertesteLaTipDat(decl.TipCuvantCheie.Tip)
      
      FOREACH (id, exprInit) IN decl.Declaratii:
        IF exprInit != null:
          valoare = EvalueazaExpresie(exprInit)
          
          IF valoare != null:
            // Conversie dacă e necesar
            valoareConvertita = ConverteLaTip(valoare, tipDat, id.Linie, id.Coloana)
            
            IF valoareConvertita != null:
              _tabelSimboluri.SeteazaValoare(id.Text, valoareConvertita,
                                             id.Linie, id.Coloana, _erori)
      RETURN
    
    // ATRIBUIRE
    IF instr is InstructiuneAtribuire atrib:
      valoare = EvalueazaExpresie(atrib.Expresie)
      
      IF valoare != null:
        // Obține tipul variabilei
        var = _tabelSimboluri.Obtine(atrib.Identificator.Text)
        
        IF var != null:
          // Conversie dacă e necesar
          valoareConvertita = ConverteLaTip(valoare, var.Tip,
                                            atrib.Identificator.Linie,
                                            atrib.Identificator.Coloana)
          
          IF valoareConvertita != null:
            _tabelSimboluri.SeteazaValoare(atrib.Identificator.Text,
                                           valoareConvertita,
                                           atrib.Identificator.Linie,
                                           atrib.Identificator.Coloana,
                                           _erori)
      RETURN
    
    // EXPRESIE STANDALONE
    IF instr is InstructiuneExpresie instrExpr:
      // Evaluează și afișează rezultatul
      rezultat = EvalueazaExpresie(instrExpr.Expresie)
      
      IF rezultat != null:
        Console.WriteLine($"Rezultat: {FormatareRezultat(rezultat)}")
      RETURN
    
    // FOR
    IF instr is InstructiuneFor instrFor:
      // Execută inițializare
      ExecuțăInstructiune(instrFor.Initializare)
      
      // Bucla
      WHILE true:
        // Evaluează condiție
        conditie = EvalueazaExpresie(instrFor.Conditie)
        
        IF conditie is not bool OR NOT (bool)conditie:
          BREAK
        
        // Execută corp
        ExecuțăInstructiune(instrFor.Corp)
        
        // Execută increment
        ExecuțăInstructiune(instrFor.Increment)
      RETURN
    
    // WHILE
    IF instr is InstructiuneWhile instrWhile:
      WHILE true:
        conditie = EvalueazaExpresie(instrWhile.Conditie)
        
        IF conditie is not bool OR NOT (bool)conditie:
          BREAK
        
        ExecuțăInstructiune(instrWhile.Corp)
      RETURN
    
    // IF
    IF instr is InstructiuneIf instrIf:
      conditie = EvalueazaExpresie(instrIf.Conditie)
      
      IF conditie is bool AND (bool)conditie:
        ExecuțăInstructiune(instrIf.CorpAdevarat)
      ELSE IF instrIf.CorpFals != null:
        ExecuțăInstructiune(instrIf.CorpFals)
      RETURN
    
    // BLOC
    IF instr is Bloc bloc:
      FOREACH instrBlocIntern IN bloc.Instructiuni:
        ExecuțăInstructiune(instrBlocIntern)
      RETURN
  
  // ==================== CONVERSII TIPURI ====================
  
  METODĂ PRIVATĂ ConverteLaTip(valoare, tipDat, linie, coloana) -> object:
    // INT
    IF tipDat == TipDat.Int:
      IF valoare is int:
        RETURN valoare
      
      IF valoare is double d:
        // Truncare double → int
        RETURN (int)d
      
      _erori.Add(EroareCompilare.Semantica(linie, coloana,
                   $"nu se poate converti {valoare.GetType().Name} la int"))
      RETURN null
    
    // DOUBLE
    IF tipDat == TipDat.Double:
      IF valoare is double:
        RETURN valoare
      
      IF valoare is int i:
        // Conversie int → double
        RETURN (double)i
      
      _erori.Add(EroareCompilare.Semantica(linie, coloana,
                   $"nu se poate converti {valoare.GetType().Name} la double"))
      RETURN null
    
    // STRING
    IF tipDat == TipDat.String:
      IF valoare is string:
        RETURN valoare
      
      _erori.Add(EroareCompilare.Semantica(linie, coloana,
                   $"nu se poate converti {valoare.GetType().Name} la string"))
      RETURN null
    
    RETURN null
  
  METODĂ PRIVATĂ FormatareRezultat(rezultat) -> string:
    IF rezultat is string str:
      RETURN $"\"{str}\""
    
    RETURN rezultat.ToString()
```

---

## 7. PROGRAM PRINCIPAL

### 7.1 Pseudocod Program.cs

```
CLASA Program:
  
  METODĂ STATICĂ Main(args: string[]):
    Console.WriteLine("╔════════════════════════════════════════╗")
    Console.WriteLine("║   COMPILATOR LFT - PROIECT NOTA 10    ║")
    Console.WriteLine("╚════════════════════════════════════════╝")
    Console.WriteLine()
    
    // Meniu interactiv
    WHILE true:
      Console.WriteLine("Selectați modul de rulare:")
      Console.WriteLine("1. Citire din fișier")
      Console.WriteLine("2. Introducere manuală cod")
      Console.WriteLine("3. Rulare teste automate")
      Console.WriteLine("4. Ieșire")
      Console.Write("\nAlegere: ")
      
      alegere = Console.ReadLine()
      
      SWITCH alegere:
        CASE "1":
          RuleazaDinFisier()
        
        CASE "2":
          RuleazaInteractiv()
        
        CASE "3":
          RuleazaTesteAutomate()
        
        CASE "4":
          RETURN
        
        DEFAULT:
          Console.WriteLine("Alegere invalidă!")
  
  // ==================== RULARE DIN FIȘIER ====================
  
  METODĂ STATICĂ RuleazaDinFisier():
    Console.Write("Introduceți calea fișierului: ")
    cale = Console.ReadLine()
    
    IF NOT File.Exists(cale):
      Console.WriteLine($"Fișierul '{cale}' nu există!")
      RETURN
    
    continut = File.ReadAllText(cale)
    
    Console.WriteLine("\n=== COD SURSĂ ===")
    AfiseazaCodCuNumereLinii(continut)
    
    CompileazaSiRuleaza(continut)
  
  // ==================== RULARE INTERACTIVĂ ====================
  
  METODĂ STATICĂ RuleazaInteractiv():
    Console.WriteLine("Introduceți cod (CTRL+Z pe linie nouă pentru final):")
    
    linii = new List<string>()
    numarLinie = 1
    
    WHILE true:
      Console.Write($"{numarLinie:D3} | ")
      linie = Console.ReadLine()
      
      IF linie == null:  // CTRL+Z
        BREAK
      
      linii.Add(linie)
      numarLinie++
    
    continut = string.Join("\n", linii)
    
    CompileazaSiRuleaza(continut)
  
  // ==================== COMPILARE ȘI RULARE ====================
  
  METODĂ STATICĂ CompileazaSiRuleaza(continut: string):
    Console.WriteLine("\n" + new string('=', 50))
    Console.WriteLine("FAZA 1: ANALIZA LEXICALĂ")
    Console.WriteLine(new string('=', 50))
    
    // LEXER
    lexer = new Lexer(continut)
    tokeni = new List<AtomLexical>()
    
    WHILE true:
      atom = lexer.UrmatorulAtom()
      tokeni.Add(atom)
      
      IF atom.Tip == Terminator:
        BREAK
    
    Console.WriteLine($"Tokeni generați: {tokeni.Count}")
    
    IF lexer.Erori.Any():
      Console.ForegroundColor = ConsoleColor.Red
      Console.WriteLine("\nERORI LEXICALE:")
      FOREACH eroare IN lexer.Erori:
        Console.WriteLine(eroare.ToStringCuContext())
      Console.ResetColor()
      RETURN
    
    Console.ForegroundColor = ConsoleColor.Green
    Console.WriteLine("✓ Analiza lexicală reușită!")
    Console.ResetColor()
    
    // PARSER
    Console.WriteLine("\n" + new string('=', 50))
    Console.WriteLine("FAZA 2: ANALIZA SINTACTICĂ & SEMANTICĂ")
    Console.WriteLine(new string('=', 50))
    
    parser = new Parser(continut)
    program = parser.ParseazaProgram()
    
    Console.WriteLine($"Instrucțiuni parsate: {program.Instructiuni.Count}")
    Console.WriteLine($"Variabile declarate: {parser.TabelSimboluri.NumărVariabile}")
    
    IF parser.Erori.Any():
      Console.ForegroundColor = ConsoleColor.Red
      Console.WriteLine("\nERORI SINTACTICE/SEMANTICE:")
      FOREACH eroare IN parser.Erori:
        Console.WriteLine(eroare.ToStringCuContext())
      Console.ResetColor()
      RETURN
    
    Console.ForegroundColor = ConsoleColor.Green
    Console.WriteLine("✓ Analiza sintactică reușită!")
    Console.WriteLine("✓ Analiza semantică reușită!")
    Console.ResetColor()
    
    // AFIȘARE ARBORE SINTACTIC
    Console.WriteLine("\n" + new string('=', 50))
    Console.WriteLine("ARBORE SINTACTIC")
    Console.WriteLine(new string('=', 50))
    
    program.AfiseazaArbore()
    
    // EVALUARE
    Console.WriteLine("\n" + new string('=', 50))
    Console.WriteLine("FAZA 3: EVALUARE & EXECUȚIE")
    Console.WriteLine(new string('=', 50))
    
    evaluator = new Evaluator(parser.TabelSimboluri)
    evaluator.ExecuțăProgram(program)
    
    IF evaluator.Erori.Any():
      Console.ForegroundColor = ConsoleColor.Red
      Console.WriteLine("\nERORI RUNTIME:")
      FOREACH eroare IN evaluator.Erori:
        Console.WriteLine(eroare)
      Console.ResetColor()
      RETURN
    
    Console.ForegroundColor = ConsoleColor.Green
    Console.WriteLine("\n✓ Execuție reușită!")
    Console.ResetColor()
    
    // TABEL SIMBOLURI FINAL
    Console.WriteLine("\n" + new string('=', 50))
    Console.WriteLine("TABEL SIMBOLURI (STARE FINALĂ)")
    Console.WriteLine(new string('=', 50))
    
    parser.TabelSimboluri.AfiseazaVariabile()
  
  // ==================== HELPER METHODS ====================
  
  METODĂ STATICĂ AfiseazaCodCuNumereLinii(continut: string):
    linii = continut.Split('\n')
    
    FOR i = 0 TO linii.Length - 1:
      Console.WriteLine($"{i+1:D3} | {linii[i]}")
```

---

## 8. SUITE TESTE

### 8.1 Organizare Teste

```
CLASA TestSuite:
  
  METODĂ STATICĂ RuleazaToateTestele():
    Console.WriteLine("╔════════════════════════════════════════╗")
    Console.WriteLine("║        RULARE TESTE AUTOMATE          ║")
    Console.WriteLine("╚════════════════════════════════════════╝\n")
    
    totaleTeste = 0
    testeReușite = 0
    
    // Teste Lexer
    Console.WriteLine("=== TESTE LEXER ===")
    testeReușite += RuleazaTesteLexer(ref totaleTeste)
    
    // Teste Parser
    Console.WriteLine("\n=== TESTE PARSER ===")
    testeReușite += RuleazaTesteParser(ref totaleTeste)
    
    // Teste Evaluator
    Console.WriteLine("\n=== TESTE EVALUATOR ===")
    testeReușite += RuleazaTesteEvaluator(ref totaleTeste)
    
    // Teste Integrare
    Console.WriteLine("\n=== TESTE INTEGRARE ===")
    testeReușite += RuleazaTesteIntegrare(ref totaleTeste)
    
    // Raport final
    Console.WriteLine("\n" + new string('=', 50))
    Console.WriteLine($"RAPORT FINAL: {testeReușite}/{totaleTeste} teste reușite")
    
    procentaj = (double)testeReușite / totaleTeste * 100
    Console.WriteLine($"Rata de succes: {procentaj:F2}%")
    
    IF testeReușite == totaleTeste:
      Console.ForegroundColor = ConsoleColor.Green
      Console.WriteLine("✓ TOATE TESTELE AU TRECUT!")
      Console.ResetColor()
    ELSE:
      Console.ForegroundColor = ConsoleColor.Red
      Console.WriteLine($"✗ {totaleTeste - testeReușite} teste au eșuat")
      Console.ResetColor()
```

### 8.2 Exemple Teste Concrete

```
TEST 1: Declarație simplă
Input:  "int a;"
Assert: 
  - 0 erori
  - 1 variabilă în tabel: a (int, neinițializată)

TEST 2: Declarație cu inițializare
Input:  "int a = 5;"
Assert:
  - 0 erori
  - 1 variabilă: a = 5

TEST 3: Declarații multiple
Input:  "int a, b=3, c;"
Assert:
  - 0 erori
  - 3 variabile: a (neinit), b=3, c (neinit)

TEST 4: Eroare declarație duplicată
Input:  "int a;\nint a;"
Assert:
  - 1 eroare semantică la linia 2
  - Mesaj: "declarație duplicată pentru variabila 'a'"

TEST 5: Expresie aritmetică simplă
Input:  "3 + 5;"
Assert:
  - 0 erori
  - Rezultat: 8

TEST 6: Precedență operatori
Input:  "3 + 4 * 5;"
Assert:
  - 0 erori
  - Rezultat: 23 (nu 35!)

TEST 7: Conversie int → double
Input:  "int a=5;\ndouble b=2.5;\ndouble c=a+b;"
Assert:
  - 0 erori
  - c = 7.5

TEST 8: Truncare double → int
Input:  "int a=5;\ndouble b=2.5;\nint c=a*b;"
Assert:
  - 0 erori
  - c = 12 (truncare din 12.5)

TEST 9: Concatenare string
Input:  "string s1=\"hello\";\nstring s2=\" world\";\nstring s3=s1+s2;"
Assert:
  - 0 erori
  - s3 = "hello world"

TEST 10: Eroare string + int
Input:  "string s=\"test\";\nint n=5;\nstring r=s+n;"
Assert:
  - 1 eroare semantică la linia 3
  - Mesaj: "incompatibilitate tipuri"

TEST 11: Împărțire la zero
Input:  "int a=5;\nint b=0;\nint c=a/b;"
Assert:
  - 1 eroare semantică la linia 3
  - Mesaj: "împărțire la zero"

TEST 12: Variabilă nedeclarată
Input:  "x = 5;"
Assert:
  - 1 eroare semantică
  - Mesaj: "variabila 'x' nu a fost declarată"

TEST 13: Variabilă neinițializată
Input:  "int a;\nint b=a;"
Assert:
  - 1 eroare semantică la linia 2
  - Mesaj: "variabila 'a' folosită înainte de inițializare"

TEST 14: FOR simplu
Input:  "int sum=0;\nfor (int i=0; i<5; i=i+1) { sum=sum+i; }"
Assert:
  - 0 erori
  - sum = 10 (0+1+2+3+4)

TEST 15: WHILE
Input:  "int i=0;\nint sum=0;\nwhile (i<5) { sum=sum+i; i=i+1; }"
Assert:
  - 0 erori
  - sum = 10, i = 5

TEST 16: IF-ELSE
Input:  "int a=5;\nint b;\nif (a>3) { b=10; } else { b=20; }"
Assert:
  - 0 erori
  - b = 10

TEST 17: Minus unar
Input:  "int a=-5+3;"
Assert:
  - 0 erori
  - a = -2

TEST 18: Eroare plus unar
Input:  "int a=+5;"
Assert:
  - 1 eroare lexicală
  - Mesaj: "plus unar nu este permis"

TEST 19: Paranteze
Input:  "int a=(3+4)*5;"
Assert:
  - 0 erori
  - a = 35

TEST 20: FOR imbricat
Input:  
  "int sum=0;\n" +
  "for (int i=0; i<3; i=i+1) {\n" +
  "  for (int j=0; j<2; j=j+1) {\n" +
  "    sum=sum+1;\n" +
  "  }\n" +
  "}"
Assert:
  - 0 erori
  - sum = 6 (3*2 iterații)
```

---

CONTINUARE ÎN URMĂTORUL FIȘIER...
