# CompilatorLFT

A compiler implementation for the Formal Languages and Translators course at UAIC Iasi.

## Features

- **Lexer** - Tokenizes source code into lexical atoms
- **Parser** - Builds an Abstract Syntax Tree (AST)
- **Evaluator** - Interprets and executes AST
- **Virtual Machine** - Bytecode compilation and execution
- **REPL** - Interactive Read-Eval-Print Loop
- **Static Analyzer** - Code analysis and warnings
- **Code Optimizer** - Optimization passes
- **Three-Address Code Generator** - Intermediate representation

## Language Features

- Data types: `int`, `float`, `string`, `bool`
- Arrays with indexing
- Control flow: `if/else`, `while`, `for`
- Functions with parameters
- Arithmetic and logical operators
- String comparison (`==`, `!=`)
- Built-in functions: `print()`, `read()`, `array()`, `len()`

## Requirements

- .NET 8.0 SDK

## Build

```bash
cd CompilatorLFT
dotnet build
```

## Run

```bash
# Interactive menu
dotnet run

# Run a source file
dotnet run -- file.lft
```

## Project Structure

```
CompilatorLFT/
├── Core/
│   ├── Lexer.cs              # Lexical analysis
│   ├── Parser.cs             # Syntax analysis
│   ├── Evaluator.cs          # AST interpretation
│   ├── StaticAnalyzer.cs     # Static analysis
│   ├── CodeOptimizer.cs      # Optimization
│   ├── REPL.cs               # Interactive mode
│   └── VM/                   # Virtual machine
│       ├── Bytecode.cs
│       ├── BytecodeCompiler.cs
│       └── VirtualMachine.cs
├── Models/
│   ├── AtomLexical.cs        # Token definitions
│   ├── NodSintactic.cs       # AST node types
│   ├── Expresii.cs           # Expression nodes
│   └── Instructiuni.cs       # Statement nodes
├── Utils/
│   └── EroareCompilare.cs    # Error handling
├── Tests/
│   └── TestSuite.cs          # Automated tests
└── Program.cs                # Entry point
```

## Example

```
int x = 10;
int y = 20;
int sum = x + y;
print(sum);
```

## License

Academic project - UAIC Iasi
