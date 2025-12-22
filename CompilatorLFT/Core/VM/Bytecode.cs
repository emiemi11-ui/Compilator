using System;
using System.Collections.Generic;
using System.Text;

namespace CompilatorLFT.Core.VM
{
    /// <summary>
    /// Bytecode instruction set for the virtual machine.
    /// Defines all supported operations for the stack-based VM.
    /// </summary>
    /// <remarks>
    /// Reference: Dragon Book, Ch. 8 - Intermediate Code Generation
    /// Reference: Dragon Book, Ch. 8.6 - Stack Machines
    /// Reference: Grigoraș "Proiectarea Compilatoarelor", Cap. 6.4
    ///
    /// Inspired by: JVM, Python VM (CPython), CLR
    ///
    /// Instruction categories:
    /// - Stack: Operations on the operand stack
    /// - Memory: Load/store for variables and arrays
    /// - Arithmetic: Mathematical operations
    /// - Comparison: Relational operations
    /// - Logical: Boolean operations
    /// - Control: Jump and branching
    /// - Functions: Call and return
    /// - I/O: Input and output
    /// </remarks>
    public enum OpCode : byte
    {
        // ==================== STACK OPERATIONS ====================

        /// <summary>Push constant value onto stack. Operand: value</summary>
        PUSH = 0x01,

        /// <summary>Pop and discard top of stack</summary>
        POP = 0x02,

        /// <summary>Duplicate top of stack</summary>
        DUP = 0x03,

        /// <summary>Swap top two elements</summary>
        SWAP = 0x04,

        // ==================== MEMORY OPERATIONS ====================

        /// <summary>Load variable onto stack. Operand: variable name</summary>
        LOAD = 0x10,

        /// <summary>Store top of stack to variable. Operand: variable name</summary>
        STORE = 0x11,

        /// <summary>Load array element. Operand: array name (index on stack)</summary>
        ALOAD = 0x12,

        /// <summary>Store to array element. Operand: array name (value, index on stack)</summary>
        ASTORE = 0x13,

        /// <summary>Push local variable. Operand: local index</summary>
        LOAD_LOCAL = 0x14,

        /// <summary>Store to local variable. Operand: local index</summary>
        STORE_LOCAL = 0x15,

        // ==================== ARITHMETIC OPERATIONS ====================

        /// <summary>Add: a + b</summary>
        ADD = 0x20,

        /// <summary>Subtract: a - b</summary>
        SUB = 0x21,

        /// <summary>Multiply: a * b</summary>
        MUL = 0x22,

        /// <summary>Divide: a / b</summary>
        DIV = 0x23,

        /// <summary>Modulo: a % b</summary>
        MOD = 0x24,

        /// <summary>Negate: -a</summary>
        NEG = 0x25,

        /// <summary>Increment: a + 1</summary>
        INC = 0x26,

        /// <summary>Decrement: a - 1</summary>
        DEC = 0x27,

        // ==================== COMPARISON OPERATIONS ====================

        /// <summary>Equal: a == b</summary>
        EQ = 0x30,

        /// <summary>Not equal: a != b</summary>
        NE = 0x31,

        /// <summary>Less than: a < b</summary>
        LT = 0x32,

        /// <summary>Less than or equal: a <= b</summary>
        LE = 0x33,

        /// <summary>Greater than: a > b</summary>
        GT = 0x34,

        /// <summary>Greater than or equal: a >= b</summary>
        GE = 0x35,

        // ==================== LOGICAL OPERATIONS ====================

        /// <summary>Logical AND: a && b</summary>
        AND = 0x40,

        /// <summary>Logical OR: a || b</summary>
        OR = 0x41,

        /// <summary>Logical NOT: !a</summary>
        NOT = 0x42,

        // ==================== CONTROL FLOW ====================

        /// <summary>Unconditional jump. Operand: address</summary>
        JMP = 0x50,

        /// <summary>Jump if false (0). Operand: address</summary>
        JMPF = 0x51,

        /// <summary>Jump if true (non-0). Operand: address</summary>
        JMPT = 0x52,

        /// <summary>Define a label (no-op at runtime). Operand: label name</summary>
        LABEL = 0x53,

        // ==================== FUNCTION OPERATIONS ====================

        /// <summary>Call function. Operand: function name, arg count</summary>
        CALL = 0x60,

        /// <summary>Call built-in function. Operand: function name, arg count</summary>
        CALL_BUILTIN = 0x61,

        /// <summary>Return from function</summary>
        RET = 0x62,

        /// <summary>Return with value from function</summary>
        RET_VAL = 0x63,

        /// <summary>Push function argument</summary>
        PUSH_ARG = 0x64,

        // ==================== I/O OPERATIONS ====================

        /// <summary>Print top of stack to console</summary>
        PRINT = 0x70,

        /// <summary>Print with newline</summary>
        PRINTLN = 0x71,

        /// <summary>Read input from console. Operand: type</summary>
        INPUT = 0x72,

        // ==================== TYPE OPERATIONS ====================

        /// <summary>Convert to integer</summary>
        TO_INT = 0x80,

        /// <summary>Convert to double</summary>
        TO_DOUBLE = 0x81,

        /// <summary>Convert to string</summary>
        TO_STRING = 0x82,

        /// <summary>Convert to boolean</summary>
        TO_BOOL = 0x83,

        // ==================== SPECIAL ====================

        /// <summary>No operation</summary>
        NOP = 0xF0,

        /// <summary>Halt execution</summary>
        HALT = 0xFF
    }

    /// <summary>
    /// Represents a single bytecode instruction.
    /// </summary>
    public class Instruction
    {
        #region Properties

        /// <summary>The operation code.</summary>
        public OpCode OpCode { get; set; }

        /// <summary>Primary operand (optional).</summary>
        public object Operand { get; set; }

        /// <summary>Secondary operand (for multi-operand instructions).</summary>
        public object Operand2 { get; set; }

        /// <summary>Source line number (for debugging).</summary>
        public int SourceLine { get; set; }

        /// <summary>Address/index in the instruction list.</summary>
        public int Address { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an instruction with no operand.
        /// </summary>
        public Instruction(OpCode opCode, int sourceLine = 0)
        {
            OpCode = opCode;
            SourceLine = sourceLine;
        }

        /// <summary>
        /// Creates an instruction with one operand.
        /// </summary>
        public Instruction(OpCode opCode, object operand, int sourceLine = 0)
        {
            OpCode = opCode;
            Operand = operand;
            SourceLine = sourceLine;
        }

        /// <summary>
        /// Creates an instruction with two operands.
        /// </summary>
        public Instruction(OpCode opCode, object operand, object operand2, int sourceLine = 0)
        {
            OpCode = opCode;
            Operand = operand;
            Operand2 = operand2;
            SourceLine = sourceLine;
        }

        #endregion

        #region Display Methods

        /// <summary>
        /// Returns a string representation of the instruction.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{Address:D4}: {OpCode,-12}");

            if (Operand != null)
            {
                sb.Append($" {FormatOperand(Operand)}");
            }

            if (Operand2 != null)
            {
                sb.Append($", {FormatOperand(Operand2)}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats an operand for display.
        /// </summary>
        private string FormatOperand(object operand)
        {
            return operand switch
            {
                string s => $"\"{s}\"",
                double d => d.ToString("F2"),
                bool b => b.ToString().ToLower(),
                _ => operand.ToString()
            };
        }

        #endregion
    }

    /// <summary>
    /// Represents a compiled bytecode program.
    /// </summary>
    public class BytecodeProgram
    {
        #region Properties

        /// <summary>List of instructions.</summary>
        public List<Instruction> Instructions { get; }

        /// <summary>Constant pool for string literals and large numbers.</summary>
        public List<object> ConstantPool { get; }

        /// <summary>Function table mapping names to entry points.</summary>
        public Dictionary<string, int> FunctionTable { get; }

        /// <summary>Label table mapping label names to addresses.</summary>
        public Dictionary<string, int> LabelTable { get; }

        /// <summary>Source file name.</summary>
        public string SourceFile { get; set; }

        /// <summary>Compilation timestamp.</summary>
        public DateTime CompiledAt { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new bytecode program.
        /// </summary>
        public BytecodeProgram()
        {
            Instructions = new List<Instruction>();
            ConstantPool = new List<object>();
            FunctionTable = new Dictionary<string, int>();
            LabelTable = new Dictionary<string, int>();
            CompiledAt = DateTime.Now;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds an instruction and returns its address.
        /// </summary>
        public int AddInstruction(Instruction instruction)
        {
            instruction.Address = Instructions.Count;
            Instructions.Add(instruction);
            return instruction.Address;
        }

        /// <summary>
        /// Adds a constant to the pool and returns its index.
        /// </summary>
        public int AddConstant(object value)
        {
            int index = ConstantPool.IndexOf(value);
            if (index >= 0) return index;

            ConstantPool.Add(value);
            return ConstantPool.Count - 1;
        }

        /// <summary>
        /// Registers a function entry point.
        /// </summary>
        public void RegisterFunction(string name, int address)
        {
            FunctionTable[name] = address;
        }

        /// <summary>
        /// Registers a label.
        /// </summary>
        public void RegisterLabel(string name, int address)
        {
            LabelTable[name] = address;
        }

        /// <summary>
        /// Resolves label references to actual addresses.
        /// </summary>
        public void ResolveLabels()
        {
            foreach (var instruction in Instructions)
            {
                if (instruction.Operand is string labelName &&
                    LabelTable.TryGetValue(labelName, out int address))
                {
                    instruction.Operand = address;
                }
            }
        }

        #endregion

        #region Display Methods

        /// <summary>
        /// Displays the bytecode program.
        /// </summary>
        public void Display()
        {
            Console.WriteLine("\n╔════════════════════════════════════════╗");
            Console.WriteLine("║           BYTECODE PROGRAM             ║");
            Console.WriteLine("╠════════════════════════════════════════╣");

            Console.WriteLine($"║ Instructions: {Instructions.Count,-24} ║");
            Console.WriteLine($"║ Constants:    {ConstantPool.Count,-24} ║");
            Console.WriteLine($"║ Functions:    {FunctionTable.Count,-24} ║");
            Console.WriteLine($"║ Labels:       {LabelTable.Count,-24} ║");
            Console.WriteLine("╚════════════════════════════════════════╝");

            // Function table
            if (FunctionTable.Count > 0)
            {
                Console.WriteLine("\n=== FUNCTION TABLE ===");
                foreach (var (name, addr) in FunctionTable)
                {
                    Console.WriteLine($"  {name}: {addr:D4}");
                }
            }

            // Instructions
            Console.WriteLine("\n=== INSTRUCTIONS ===");
            foreach (var instruction in Instructions)
            {
                Console.WriteLine($"  {instruction}");
            }

            // Constant pool
            if (ConstantPool.Count > 0)
            {
                Console.WriteLine("\n=== CONSTANT POOL ===");
                for (int i = 0; i < ConstantPool.Count; i++)
                {
                    Console.WriteLine($"  [{i}]: {ConstantPool[i]}");
                }
            }
        }

        /// <summary>
        /// Gets statistics about the program.
        /// </summary>
        public string GetStatistics()
        {
            return $"Bytecode: {Instructions.Count} instructions, " +
                   $"{ConstantPool.Count} constants, " +
                   $"{FunctionTable.Count} functions";
        }

        #endregion
    }
}
