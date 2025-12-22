using System;

namespace CompilatorLFT.Models
{
    /// <summary>
    /// Enumerare pentru toate tipurile de atomi lexicali recunoscuți de compilator.
    /// Respectă ierarhia: Literali → Identificatori → Operatori → Delimitatori → Noduri AST
    /// </summary>
    /// <remarks>
    /// Referință: Dragon Book, Cap. 3 - Lexical Analysis
    /// </remarks>
    public enum TipAtomLexical
    {
        // ==================== LITERALI ====================
        
        /// <summary>Număr întreg (ex: 42, -17, 0)</summary>
        NumarIntreg,
        
        /// <summary>Număr zecimal cu punct (ex: 3.14, -0.5, 2.0)</summary>
        NumarZecimal,
        
        /// <summary>Literal string între ghilimele (ex: "hello", "test 123")</summary>
        StringLiteral,

        // ==================== IDENTIFICATORI ====================
        
        /// <summary>Nume de variabilă sau funcție (ex: suma, _temp, var123)</summary>
        Identificator,

        // ==================== CUVINTE CHEIE - TIPURI ====================
        
        /// <summary>Cuvânt cheie 'int' pentru tip întreg</summary>
        CuvantCheieInt,
        
        /// <summary>Cuvânt cheie 'double' pentru tip zecimal</summary>
        CuvantCheieDouble,
        
        /// <summary>Cuvânt cheie 'string' pentru tip șir caractere</summary>
        CuvantCheieString,

        // ==================== CUVINTE CHEIE - CONTROL FLOW ====================
        
        /// <summary>Cuvânt cheie 'for' pentru bucla for</summary>
        CuvantCheieFor,
        
        /// <summary>Cuvânt cheie 'while' pentru bucla while</summary>
        CuvantCheieWhile,
        
        /// <summary>Cuvânt cheie 'if' pentru instrucțiune condițională</summary>
        CuvantCheieIf,
        
        /// <summary>Cuvânt cheie 'else' pentru ramura alternativă</summary>
        CuvantCheieElse,

        // ==================== OPERATORI ARITMETICI ====================
        
        /// <summary>Operator adunare '+' sau plus unar</summary>
        Plus,
        
        /// <summary>Operator scădere '-' sau minus unar</summary>
        Minus,
        
        /// <summary>Operator înmulțire '*'</summary>
        Star,
        
        /// <summary>Operator împărțire '/'</summary>
        Slash,

        // ==================== OPERATORI RELAȚIONALI ====================
        
        /// <summary>Operator mai mic '&lt;'</summary>
        MaiMic,
        
        /// <summary>Operator mai mare '&gt;'</summary>
        MaiMare,
        
        /// <summary>Operator mai mic sau egal '&lt;='</summary>
        MaiMicEgal,
        
        /// <summary>Operator mai mare sau egal '&gt;='</summary>
        MaiMareEgal,
        
        /// <summary>Operator egalitate '=='</summary>
        EgalEgal,
        
        /// <summary>Operator diferit '!='</summary>
        Diferit,

        // ==================== DELIMITATORI ====================
        
        /// <summary>Delimitator punct și virgulă ';'</summary>
        PunctVirgula,
        
        /// <summary>Delimitator virgulă ','</summary>
        Virgula,
        
        /// <summary>Operator atribuire '='</summary>
        Egal,
        
        /// <summary>Paranteză deschisă '('</summary>
        ParantezaDeschisa,
        
        /// <summary>Paranteză închisă ')'</summary>
        ParantezaInchisa,
        
        /// <summary>Acoladă deschisă '{'</summary>
        AcoladaDeschisa,
        
        /// <summary>Acoladă închisă '}'</summary>
        AcoladaInchisa,

        // ==================== SPECIALE ====================
        
        /// <summary>Spațiu alb (ignorat în parsing)</summary>
        Spatiu,
        
        /// <summary>Linie nouă '\n' (pentru tracking linii)</summary>
        LinieNoua,
        
        /// <summary>Terminator de fișier/șir</summary>
        Terminator,
        
        /// <summary>Token invalid (eroare lexicală)</summary>
        Invalid,

        // ==================== NODURI ARBORE SINTACTIC - EXPRESII ====================
        
        /// <summary>Nod pentru expresie numerică literală</summary>
        ExpresieNumerica,
        
        /// <summary>Nod pentru expresie binară (ex: a + b)</summary>
        ExpresieBinara,
        
        /// <summary>Nod pentru expresie unară (ex: -a)</summary>
        ExpresieUnara,
        
        /// <summary>Nod pentru expresie cu paranteze (ex: (a + b))</summary>
        ExpresieCuParanteze,
        
        /// <summary>Nod pentru expresie identificator (variabilă)</summary>
        ExpresieIdentificator,
        
        /// <summary>Nod pentru expresie string literal</summary>
        ExpresieString,

        // ==================== NODURI ARBORE SINTACTIC - INSTRUCTIUNI ====================
        
        /// <summary>Nod pentru instrucțiune de declarație (ex: int a, b=5;)</summary>
        InstructiuneDeclaratie,
        
        /// <summary>Nod pentru instrucțiune de atribuire (ex: a = 10;)</summary>
        InstructiuneAtribuire,
        
        /// <summary>Nod pentru instrucțiune expresie (ex: a + b;)</summary>
        InstructiuneExpresie,
        
        /// <summary>Nod pentru instrucțiune for</summary>
        InstructiuneFor,
        
        /// <summary>Nod pentru instrucțiune while</summary>
        InstructiuneWhile,
        
        /// <summary>Nod pentru instrucțiune if</summary>
        InstructiuneIf,
        
        /// <summary>Nod pentru bloc de instrucțiuni între acolade</summary>
        Bloc,
        
        /// <summary>Nod rădăcină pentru programul complet</summary>
        Program
    }

    /// <summary>
    /// Enumerare pentru tipurile de erori de compilare.
    /// </summary>
    /// <remarks>
    /// Conform cerințelor: erori lexicale, sintactice și semantice
    /// </remarks>
    public enum TipEroare
    {
        /// <summary>Eroare la nivel de analiză lexicală (caracter invalid, număr malformat, etc.)</summary>
        Lexicala,
        
        /// <summary>Eroare la nivel de analiză sintactică (paranteze nebalansate, lipsă ';', etc.)</summary>
        Sintactica,
        
        /// <summary>Eroare la nivel de analiză semantică (variabilă nedeclarată, tipuri incompatibile, etc.)</summary>
        Semantica
    }

    /// <summary>
    /// Enumerare pentru tipurile de date suportate de limbaj.
    /// </summary>
    public enum TipDat
    {
        /// <summary>Tip întreg (int)</summary>
        Int,
        
        /// <summary>Tip zecimal (double)</summary>
        Double,
        
        /// <summary>Tip șir de caractere (string)</summary>
        String,
        
        /// <summary>Tip nedefinit sau eroare</summary>
        Necunoscut
    }
}
