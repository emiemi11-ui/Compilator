using System;
using System.Collections.Generic;
using System.Linq;

namespace CompilatorLFT.Core.VM
{
    /// <summary>
    /// Stack-based virtual machine for bytecode execution.
    /// Executes compiled bytecode programs.
    /// </summary>
    /// <remarks>
    /// Reference: Dragon Book, Ch. 8.6 - Stack Machines
    /// Reference: Dragon Book, Ch. 8 - Code Generation
    /// Reference: Grigoraș "Proiectarea Compilatoarelor", Cap. 6.4
    ///
    /// Architecture:
    /// - Stack-based execution model
    /// - Separate stack for operands
    /// - Call stack for function invocations
    /// - Global memory for variables
    ///
    /// Similar to: JVM, Python VM, CLR
    /// </remarks>
    public class VirtualMachine
    {
        #region Private Fields

        private readonly Stack<object> _operandStack;
        private readonly Stack<CallFrame> _callStack;
        private readonly Dictionary<string, object> _globalMemory;
        private readonly Dictionary<string, List<object>> _arrays;
        private int _instructionPointer;
        private BytecodeProgram _program;
        private bool _running;
        private int _instructionsExecuted;
        private const int MaxInstructions = 1000000;  // Safety limit

        #endregion

        #region Properties

        /// <summary>Current instruction pointer.</summary>
        public int InstructionPointer => _instructionPointer;

        /// <summary>Whether VM is running.</summary>
        public bool IsRunning => _running;

        /// <summary>Number of instructions executed.</summary>
        public int InstructionsExecuted => _instructionsExecuted;

        /// <summary>Current stack size.</summary>
        public int StackSize => _operandStack.Count;

        /// <summary>Current call depth.</summary>
        public int CallDepth => _callStack.Count;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new virtual machine.
        /// </summary>
        public VirtualMachine()
        {
            _operandStack = new Stack<object>();
            _callStack = new Stack<CallFrame>();
            _globalMemory = new Dictionary<string, object>();
            _arrays = new Dictionary<string, List<object>>();
            Reset();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes a bytecode program.
        /// </summary>
        /// <param name="program">The program to execute</param>
        public void Execute(BytecodeProgram program)
        {
            if (program == null)
                throw new ArgumentNullException(nameof(program));

            Reset();
            _program = program;
            _running = true;

            while (_running && _instructionPointer < _program.Instructions.Count)
            {
                if (_instructionsExecuted++ > MaxInstructions)
                {
                    throw new VirtualMachineException(
                        $"Exceeded maximum instruction limit ({MaxInstructions}). Possible infinite loop.");
                }

                var instruction = _program.Instructions[_instructionPointer];

                try
                {
                    ExecuteInstruction(instruction);
                }
                catch (Exception ex) when (!(ex is VirtualMachineException))
                {
                    throw new VirtualMachineException(
                        $"Error at instruction {_instructionPointer}: {instruction}\n{ex.Message}",
                        ex);
                }

                _instructionPointer++;
            }
        }

        /// <summary>
        /// Resets the VM state.
        /// </summary>
        public void Reset()
        {
            _operandStack.Clear();
            _callStack.Clear();
            _globalMemory.Clear();
            _arrays.Clear();
            _instructionPointer = 0;
            _running = false;
            _instructionsExecuted = 0;
        }

        #endregion

        #region Instruction Execution

        /// <summary>
        /// Executes a single instruction.
        /// </summary>
        private void ExecuteInstruction(Instruction instruction)
        {
            switch (instruction.OpCode)
            {
                // Stack operations
                case OpCode.PUSH:
                    _operandStack.Push(instruction.Operand);
                    break;

                case OpCode.POP:
                    _operandStack.Pop();
                    break;

                case OpCode.DUP:
                    _operandStack.Push(_operandStack.Peek());
                    break;

                case OpCode.SWAP:
                    {
                        var a = _operandStack.Pop();
                        var b = _operandStack.Pop();
                        _operandStack.Push(a);
                        _operandStack.Push(b);
                    }
                    break;

                // Memory operations
                case OpCode.LOAD:
                    ExecuteLoad((string)instruction.Operand);
                    break;

                case OpCode.STORE:
                    ExecuteStore((string)instruction.Operand);
                    break;

                case OpCode.ALOAD:
                    ExecuteArrayLoad((string)instruction.Operand);
                    break;

                case OpCode.ASTORE:
                    ExecuteArrayStore((string)instruction.Operand);
                    break;

                // Arithmetic operations
                case OpCode.ADD:
                    ExecuteBinaryOp((a, b) => Add(a, b));
                    break;

                case OpCode.SUB:
                    ExecuteBinaryOp((a, b) => Subtract(a, b));
                    break;

                case OpCode.MUL:
                    ExecuteBinaryOp((a, b) => Multiply(a, b));
                    break;

                case OpCode.DIV:
                    ExecuteBinaryOp((a, b) => Divide(a, b));
                    break;

                case OpCode.MOD:
                    ExecuteBinaryOp((a, b) => Modulo(a, b));
                    break;

                case OpCode.NEG:
                    ExecuteUnaryOp(Negate);
                    break;

                case OpCode.INC:
                    ExecuteUnaryOp(v => Add(v, 1));
                    break;

                case OpCode.DEC:
                    ExecuteUnaryOp(v => Subtract(v, 1));
                    break;

                // Comparison operations
                case OpCode.EQ:
                    ExecuteBinaryOp((a, b) => Equals(a, b));
                    break;

                case OpCode.NE:
                    ExecuteBinaryOp((a, b) => !Equals(a, b));
                    break;

                case OpCode.LT:
                    ExecuteBinaryOp((a, b) => LessThan(a, b));
                    break;

                case OpCode.LE:
                    ExecuteBinaryOp((a, b) => LessThanOrEqual(a, b));
                    break;

                case OpCode.GT:
                    ExecuteBinaryOp((a, b) => GreaterThan(a, b));
                    break;

                case OpCode.GE:
                    ExecuteBinaryOp((a, b) => GreaterThanOrEqual(a, b));
                    break;

                // Logical operations
                case OpCode.AND:
                    ExecuteBinaryOp((a, b) => ToBoolean(a) && ToBoolean(b));
                    break;

                case OpCode.OR:
                    ExecuteBinaryOp((a, b) => ToBoolean(a) || ToBoolean(b));
                    break;

                case OpCode.NOT:
                    ExecuteUnaryOp(v => !ToBoolean(v));
                    break;

                // Control flow
                case OpCode.JMP:
                    _instructionPointer = (int)instruction.Operand - 1;  // -1 because we increment after
                    break;

                case OpCode.JMPF:
                    if (!ToBoolean(_operandStack.Pop()))
                    {
                        _instructionPointer = (int)instruction.Operand - 1;
                    }
                    break;

                case OpCode.JMPT:
                    if (ToBoolean(_operandStack.Pop()))
                    {
                        _instructionPointer = (int)instruction.Operand - 1;
                    }
                    break;

                case OpCode.LABEL:
                    // Labels are no-ops at runtime
                    break;

                // Function operations
                case OpCode.CALL:
                    ExecuteCall((string)instruction.Operand, (int)instruction.Operand2);
                    break;

                case OpCode.CALL_BUILTIN:
                    ExecuteBuiltInCall((string)instruction.Operand, (int)instruction.Operand2);
                    break;

                case OpCode.RET:
                    ExecuteReturn(false);
                    break;

                case OpCode.RET_VAL:
                    ExecuteReturn(true);
                    break;

                // I/O operations
                case OpCode.PRINT:
                    Console.Write(_operandStack.Pop());
                    break;

                case OpCode.PRINTLN:
                    Console.WriteLine(_operandStack.Pop());
                    break;

                case OpCode.INPUT:
                    _operandStack.Push(Console.ReadLine());
                    break;

                // Type conversions
                case OpCode.TO_INT:
                    ExecuteUnaryOp(v => Convert.ToInt32(ToDouble(v)));
                    break;

                case OpCode.TO_DOUBLE:
                    ExecuteUnaryOp(ToDouble);
                    break;

                case OpCode.TO_STRING:
                    ExecuteUnaryOp(v => v?.ToString() ?? "");
                    break;

                case OpCode.TO_BOOL:
                    ExecuteUnaryOp(ToBoolean);
                    break;

                // Special
                case OpCode.NOP:
                    break;

                case OpCode.HALT:
                    _running = false;
                    break;

                default:
                    throw new VirtualMachineException($"Unknown opcode: {instruction.OpCode}");
            }
        }

        #endregion

        #region Memory Operations

        /// <summary>
        /// Loads a variable onto the stack.
        /// </summary>
        private void ExecuteLoad(string name)
        {
            // Check local variables in current frame first
            if (_callStack.Count > 0)
            {
                var frame = _callStack.Peek();
                if (frame.Locals.TryGetValue(name, out object localValue))
                {
                    _operandStack.Push(localValue);
                    return;
                }
            }

            // Then check global memory
            if (_globalMemory.TryGetValue(name, out object value))
            {
                _operandStack.Push(value);
            }
            else
            {
                throw new VirtualMachineException($"Variable '{name}' not found");
            }
        }

        /// <summary>
        /// Stores the top of stack to a variable.
        /// </summary>
        private void ExecuteStore(string name)
        {
            var value = _operandStack.Pop();

            // Store in local frame if in a function
            if (_callStack.Count > 0)
            {
                var frame = _callStack.Peek();
                frame.Locals[name] = value;
            }
            else
            {
                _globalMemory[name] = value;
            }
        }

        /// <summary>
        /// Loads an array element.
        /// </summary>
        private void ExecuteArrayLoad(string arrayName)
        {
            var index = (int)ToDouble(_operandStack.Pop());

            if (!_arrays.TryGetValue(arrayName, out var array))
            {
                throw new VirtualMachineException($"Array '{arrayName}' not found");
            }

            if (index < 0 || index >= array.Count)
            {
                throw new VirtualMachineException($"Array index {index} out of bounds [0, {array.Count - 1}]");
            }

            _operandStack.Push(array[index]);
        }

        /// <summary>
        /// Stores to an array element.
        /// </summary>
        private void ExecuteArrayStore(string arrayName)
        {
            var index = (int)ToDouble(_operandStack.Pop());
            var value = _operandStack.Pop();

            if (!_arrays.TryGetValue(arrayName, out var array))
            {
                _arrays[arrayName] = new List<object>();
                array = _arrays[arrayName];
            }

            // Extend array if necessary
            while (array.Count <= index)
            {
                array.Add(null);
            }

            array[index] = value;
        }

        #endregion

        #region Function Operations

        /// <summary>
        /// Executes a function call.
        /// </summary>
        private void ExecuteCall(string functionName, int argCount)
        {
            if (!_program.FunctionTable.TryGetValue(functionName, out int address))
            {
                throw new VirtualMachineException($"Function '{functionName}' not found");
            }

            // Create new call frame
            var frame = new CallFrame
            {
                FunctionName = functionName,
                ReturnAddress = _instructionPointer,
                Locals = new Dictionary<string, object>()
            };

            // Pop arguments into locals
            var args = new object[argCount];
            for (int i = argCount - 1; i >= 0; i--)
            {
                args[i] = _operandStack.Pop();
            }

            // Store as arg0, arg1, etc.
            for (int i = 0; i < argCount; i++)
            {
                frame.Locals[$"arg{i}"] = args[i];
            }

            _callStack.Push(frame);

            // Jump to function
            _instructionPointer = address - 1;  // -1 because we increment after
        }

        /// <summary>
        /// Executes a built-in function call.
        /// </summary>
        private void ExecuteBuiltInCall(string functionName, int argCount)
        {
            var args = new object[argCount];
            for (int i = argCount - 1; i >= 0; i--)
            {
                args[i] = _operandStack.Pop();
            }

            object result = ExecuteBuiltIn(functionName, args);

            if (result != null)
            {
                _operandStack.Push(result);
            }
        }

        /// <summary>
        /// Executes a return from function.
        /// </summary>
        private void ExecuteReturn(bool hasReturnValue)
        {
            object returnValue = null;
            if (hasReturnValue)
            {
                returnValue = _operandStack.Pop();
            }

            if (_callStack.Count == 0)
            {
                _running = false;
                return;
            }

            var frame = _callStack.Pop();
            _instructionPointer = frame.ReturnAddress;

            if (hasReturnValue)
            {
                _operandStack.Push(returnValue);
            }
        }

        /// <summary>
        /// Executes a built-in function.
        /// </summary>
        private object ExecuteBuiltIn(string name, object[] args)
        {
            return name switch
            {
                "print" => ExecutePrint(args),
                "sqrt" => Math.Sqrt(ToDouble(args[0])),
                "abs" => Math.Abs(ToDouble(args[0])),
                "exp" => Math.Exp(ToDouble(args[0])),
                "log" => Math.Log(ToDouble(args[0])),
                "sin" => Math.Sin(ToDouble(args[0])),
                "cos" => Math.Cos(ToDouble(args[0])),
                "tan" => Math.Tan(ToDouble(args[0])),
                "pow" => Math.Pow(ToDouble(args[0]), ToDouble(args[1])),
                "min" => Math.Min(ToDouble(args[0]), ToDouble(args[1])),
                "max" => Math.Max(ToDouble(args[0]), ToDouble(args[1])),
                "floor" => Math.Floor(ToDouble(args[0])),
                "ceil" => Math.Ceiling(ToDouble(args[0])),
                "round" => Math.Round(ToDouble(args[0])),
                "length" => GetLength(args[0]),
                "input" => Console.ReadLine(),
                "parseInt" => int.TryParse(args[0]?.ToString(), out int i) ? i : 0,
                "parseDouble" => double.TryParse(args[0]?.ToString(), out double d) ? d : 0.0,
                "toString" => args[0]?.ToString() ?? "",
                _ => throw new VirtualMachineException($"Unknown built-in function: {name}")
            };
        }

        private object ExecutePrint(object[] args)
        {
            Console.WriteLine(string.Join(" ", args.Select(a => a?.ToString() ?? "null")));
            return null;
        }

        private int GetLength(object obj)
        {
            return obj switch
            {
                string s => s.Length,
                List<object> list => list.Count,
                _ => 0
            };
        }

        #endregion

        #region Arithmetic Operations

        private void ExecuteBinaryOp(Func<object, object, object> operation)
        {
            var right = _operandStack.Pop();
            var left = _operandStack.Pop();
            _operandStack.Push(operation(left, right));
        }

        private void ExecuteUnaryOp(Func<object, object> operation)
        {
            var operand = _operandStack.Pop();
            _operandStack.Push(operation(operand));
        }

        private object Add(object left, object right)
        {
            // String concatenation
            if (left is string || right is string)
            {
                return (left?.ToString() ?? "") + (right?.ToString() ?? "");
            }

            // Integer arithmetic
            if (left is int l && right is int r)
            {
                return l + r;
            }

            return ToDouble(left) + ToDouble(right);
        }

        private object Subtract(object left, object right)
        {
            if (left is int l && right is int r)
            {
                return l - r;
            }
            return ToDouble(left) - ToDouble(right);
        }

        private object Multiply(object left, object right)
        {
            if (left is int l && right is int r)
            {
                return l * r;
            }
            return ToDouble(left) * ToDouble(right);
        }

        private object Divide(object left, object right)
        {
            double rightVal = ToDouble(right);
            if (Math.Abs(rightVal) < double.Epsilon)
            {
                throw new VirtualMachineException("Division by zero");
            }

            if (left is int l && right is int r && r != 0)
            {
                return l / r;
            }

            return ToDouble(left) / rightVal;
        }

        private object Modulo(object left, object right)
        {
            if (left is int l && right is int r)
            {
                if (r == 0)
                {
                    throw new VirtualMachineException("Modulo by zero");
                }
                return l % r;
            }
            return ToDouble(left) % ToDouble(right);
        }

        private object Negate(object value)
        {
            if (value is int i)
            {
                return -i;
            }
            return -ToDouble(value);
        }

        #endregion

        #region Comparison Operations

        private bool LessThan(object left, object right)
        {
            if (left is int l && right is int r)
            {
                return l < r;
            }
            return ToDouble(left) < ToDouble(right);
        }

        private bool LessThanOrEqual(object left, object right)
        {
            if (left is int l && right is int r)
            {
                return l <= r;
            }
            return ToDouble(left) <= ToDouble(right);
        }

        private bool GreaterThan(object left, object right)
        {
            if (left is int l && right is int r)
            {
                return l > r;
            }
            return ToDouble(left) > ToDouble(right);
        }

        private bool GreaterThanOrEqual(object left, object right)
        {
            if (left is int l && right is int r)
            {
                return l >= r;
            }
            return ToDouble(left) >= ToDouble(right);
        }

        private new bool Equals(object left, object right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;

            if (left is int l1 && right is int r1)
            {
                return l1 == r1;
            }

            if (left is string || right is string)
            {
                return left.ToString() == right.ToString();
            }

            return Math.Abs(ToDouble(left) - ToDouble(right)) < double.Epsilon;
        }

        #endregion

        #region Type Conversion

        private double ToDouble(object value)
        {
            return value switch
            {
                null => 0.0,
                int i => i,
                double d => d,
                bool b => b ? 1.0 : 0.0,
                string s => double.TryParse(s, out double d) ? d : 0.0,
                _ => Convert.ToDouble(value)
            };
        }

        private bool ToBoolean(object value)
        {
            return value switch
            {
                null => false,
                bool b => b,
                int i => i != 0,
                double d => Math.Abs(d) > double.Epsilon,
                string s => !string.IsNullOrEmpty(s),
                _ => true
            };
        }

        #endregion

        #region Display Methods

        /// <summary>
        /// Displays VM state.
        /// </summary>
        public void DisplayState()
        {
            Console.WriteLine("\n=== VM STATE ===");
            Console.WriteLine($"Instruction Pointer: {_instructionPointer}");
            Console.WriteLine($"Instructions Executed: {_instructionsExecuted}");
            Console.WriteLine($"Stack Size: {_operandStack.Count}");
            Console.WriteLine($"Call Depth: {_callStack.Count}");
            Console.WriteLine($"Global Variables: {_globalMemory.Count}");
            Console.WriteLine($"Arrays: {_arrays.Count}");

            if (_globalMemory.Count > 0)
            {
                Console.WriteLine("\nGlobal Memory:");
                foreach (var (name, value) in _globalMemory)
                {
                    Console.WriteLine($"  {name} = {value}");
                }
            }

            if (_operandStack.Count > 0)
            {
                Console.WriteLine("\nOperand Stack (top to bottom):");
                foreach (var item in _operandStack.Take(10))
                {
                    Console.WriteLine($"  {item}");
                }
            }
        }

        /// <summary>
        /// Gets execution statistics.
        /// </summary>
        public string GetStatistics()
        {
            return $"VM: {_instructionsExecuted} instructions executed, " +
                   $"{_globalMemory.Count} variables, " +
                   $"max stack: {StackSize}";
        }

        #endregion
    }

    /// <summary>
    /// Represents a function call frame on the call stack.
    /// </summary>
    public class CallFrame
    {
        public string FunctionName { get; set; }
        public int ReturnAddress { get; set; }
        public Dictionary<string, object> Locals { get; set; }
    }

    /// <summary>
    /// Exception thrown by the virtual machine.
    /// </summary>
    public class VirtualMachineException : Exception
    {
        public VirtualMachineException(string message) : base(message) { }
        public VirtualMachineException(string message, Exception inner) : base(message, inner) { }
    }
}
