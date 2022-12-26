using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace D1Plugin.D1Assembly;

public enum InstructionType
{
    Inherent,
    Immediate,
    Address,
    SPOffset,
    BaseOffset,
    ARegister,
    BRegister,
    PCRegister
}

public class Instruction
{
    public string Identifier { get; set; }
    public byte Opcode { get; set; }
    public InstructionType Type { get; set; }

    public Instruction(string identifier, byte opcode, InstructionType type)
    {
        Identifier = identifier;
        Opcode = opcode;
        Type = type;
    }
}

public class D1Assembler : D1AssemblyBaseVisitor<object>
{
    public List<Instruction> Instructions { get; set; } = new();
    public Dictionary<string, byte> Symbols { get; set; } = new();
    public Dictionary<string, List<int>> SymbolReferences { get; set; } = new();
    public List<byte> Bytes { get; set; } = new();
    public int CurrentAddress { get; set; } = 0;

    public D1Assembler()
    {
        this.AddInstruction("NOP", 0x00, InstructionType.Inherent);
        this.AddInstruction("LDA", 0x01, InstructionType.Immediate);
        this.AddInstruction("LDA", 0x02, InstructionType.Address);
        this.AddInstruction("LDA", 0x03, InstructionType.SPOffset);
        this.AddInstruction("LDA", 0x04, InstructionType.BaseOffset);

        this.AddInstruction("STA", 0x05, InstructionType.Address);
        this.AddInstruction("STA", 0x06, InstructionType.SPOffset);
        this.AddInstruction("STA", 0x07, InstructionType.BaseOffset);

        this.AddInstruction("LDB", 0x08, InstructionType.Immediate);
        this.AddInstruction("LDB", 0x09, InstructionType.Address);
        this.AddInstruction("LDB", 0x0A, InstructionType.SPOffset);
        this.AddInstruction("LDB", 0x0B, InstructionType.BaseOffset);

        this.AddInstruction("STB", 0x0C, InstructionType.Address);
        this.AddInstruction("STB", 0x0D, InstructionType.SPOffset);
        this.AddInstruction("STB", 0x0E, InstructionType.BaseOffset);

        this.AddInstruction("LDSP", 0x0F, InstructionType.Immediate);

        this.AddInstruction("ADD", 0x10, InstructionType.ARegister);
        this.AddInstruction("ADD", 0x11, InstructionType.BRegister);
        this.AddInstruction("ADD", 0x12, InstructionType.Address);

        this.AddInstruction("SUB", 0x13, InstructionType.ARegister);
        this.AddInstruction("SUB", 0x14, InstructionType.BRegister);
        this.AddInstruction("SUB", 0x15, InstructionType.Address);

        this.AddInstruction("PUSH", 0x16, InstructionType.ARegister);
        this.AddInstruction("PUSH", 0x17, InstructionType.BRegister);
        this.AddInstruction("PUSH", 0x18, InstructionType.PCRegister);

        this.AddInstruction("POP", 0x19, InstructionType.ARegister);
        this.AddInstruction("POP", 0x1A, InstructionType.BRegister);
        this.AddInstruction("POP", 0x1B, InstructionType.PCRegister);

        this.AddInstruction("JMP", 0x1C, InstructionType.Address);
        this.AddInstruction("JEZ", 0x1D, InstructionType.Address);
        this.AddInstruction("JNZ", 0x1E, InstructionType.Address);
        this.AddInstruction("JCC", 0x1F, InstructionType.Address);
        this.AddInstruction("JCS", 0x20, InstructionType.Address);

        this.AddInstruction("STST", 0x21, InstructionType.Inherent);
        this.AddInstruction("ATST", 0x22, InstructionType.Inherent);
    }

    private void AddInstruction(string identifier, byte opcode, InstructionType type)
    {
        Instructions.Add(new Instruction(identifier, opcode, type));
    }

    private void Emit(byte b)
    {
        Bytes.Add(b);
        CurrentAddress++;
    }

    private void AddSymbolWithValue(string symbol, byte value)
    {
        Symbols.Add(symbol, value);

        if (SymbolReferences.ContainsKey(symbol))
        {
            foreach (var reference in SymbolReferences[symbol])
            {
                Bytes[reference] = value;
            }
        }
    }

    private void AddSymbolAtAddress(string symbol)
    {
        Symbols.Add(symbol, (byte)CurrentAddress);
    }

    private void AddSymbolReference(string symbol)
    {
        if (!SymbolReferences.ContainsKey(symbol))
        {
            SymbolReferences.Add(symbol, new List<int>());
        }

        SymbolReferences[symbol].Add(CurrentAddress);
    }

    private bool TryGetInstructionTypeFromArguments(IList<D1AssemblyParser.ArgumentContext> args, out InstructionType type)
    {
        if (args.Count == 0)
        {
            type = InstructionType.Inherent;
            return true;
        }

        if (args.Count == 1)
        {
            var arg = args[0];
            if (arg.REGISTER() != null)
            {
                var t = arg.REGISTER().GetText().Substring(1).ToUpper();

                if (t == "A")
                {
                    type = InstructionType.ARegister;
                    return true;
                }
                else if (t == "B")
                {
                    type = InstructionType.BRegister;
                    return true;
                }
                else if (t == "PC")
                {
                    type = InstructionType.PCRegister;
                    return true;
                }

                type = InstructionType.Inherent;
                return false;
            }

            if (arg.LABEL() != null)
            {
                type = InstructionType.Address;
                return true;
            }

            if (arg.HEXADECIMAL() != null)
            {
                type = InstructionType.Address;
                return true;
            }

            if (arg.DECIMAL() != null)
            {
                type = InstructionType.Address;
                return true;
            }

            if (arg.IMHEXADECIMAL() != null)
            {
                type = InstructionType.Immediate;
                return true;
            }

            if (arg.IMDECIMAL() != null)
            {
                type = InstructionType.Immediate;
                return true;
            }
        }

        if (args.Count == 2)
        {
            var arg1 = args[0];
            var arg2 = args[1];

            if (arg1.REGISTER() != null && (arg2.IMDECIMAL != null || arg2.IMHEXADECIMAL != null || arg2.LABEL() != null))
            {
                if (arg1.REGISTER().GetText() != "SP")
                {
                    type = InstructionType.Inherent;
                    return false; // Can only ever use SP as a base register
                }

                type = InstructionType.SPOffset;
                return true;
            }

            if ((arg1.LABEL() != null || arg1.HEXADECIMAL() != null || arg1.DECIMAL() != null) && (arg2.IMDECIMAL != null || arg2.IMHEXADECIMAL != null))
            {
                type = InstructionType.BaseOffset;
                return true;
            }
        }

        type = InstructionType.Inherent;
        return false;
    }

    private bool TryGetInstruction(string identifier, InstructionType type, out Instruction instruction)
    {
        foreach (var instr in Instructions)
        {
            if (instr.Identifier == identifier && instr.Type == type)
            {
                instruction = instr;
                return true;
            }
        }

        instruction = null;
        return false;
    }

    private int orgAddress = 0;
    private void ExecuteORG(D1AssemblyParser.ArgumentContext arg, InstructionType type)
    {
        if (this.Bytes.Count != 0)
        {
            throw new Exception($"ORG must be the first instruction if used");
        }

        if (type != InstructionType.Address)
        {
            throw new Exception($"ORG expects an address as argument");
        }

        var address = arg.HEXADECIMAL() != null ? Convert.ToInt32(arg.HEXADECIMAL().GetText().Substring(1), 16) : Convert.ToInt32(arg.DECIMAL().GetText());
        this.orgAddress = address;

        Enumerable.Repeat(0, address).ToList().ForEach(_ => this.Emit(0));
    }

    private void ExecuteEQU(string symbol, D1AssemblyParser.ArgumentContext arg, InstructionType type)
    {
        if (type != InstructionType.Immediate)
        {
            throw new Exception($"EQU expects an immediate value as argument");
        }

        if (symbol is null)
        {
            throw new Exception($"EQU expects a symbol to be defined");
        }

        var address = arg.IMHEXADECIMAL() != null ? Convert.ToInt32(arg.IMHEXADECIMAL().GetText().Substring(2), 16) : Convert.ToInt32(arg.IMDECIMAL().GetText().Substring(1));
        this.AddSymbolWithValue(symbol, (byte)address);
    }

    private byte EvaluateArgument(D1AssemblyParser.ArgumentContext arg)
    {
        if (arg.HEXADECIMAL() != null)
        {
            return Convert.ToByte(arg.HEXADECIMAL().GetText().Substring(1), 16);
        }

        if (arg.DECIMAL() != null)
        {
            return Convert.ToByte(arg.DECIMAL().GetText());
        }

        if (arg.IMHEXADECIMAL() != null)
        {
            return Convert.ToByte(arg.IMHEXADECIMAL().GetText().Substring(2), 16);
        }

        if (arg.IMDECIMAL() != null)
        {
            return Convert.ToByte(arg.IMDECIMAL().GetText().Substring(1));
        }

        if (arg.LABEL() != null)
        {
            var label = arg.LABEL().GetText();
            if (Symbols.ContainsKey(label))
            {
                return Symbols[label];
            }
            else
            {
                AddSymbolReference(label);
                return 0;
            }
        }

        throw new Exception($"Invalid argument");
    }

    private void EmitInstruction(Instruction instruction, IList<D1AssemblyParser.ArgumentContext> args)
    {
        this.Emit(instruction.Opcode);

        Action x = instruction.Type switch
        {
            InstructionType.Inherent => () => { }
            ,
            InstructionType.Immediate => () => this.Emit(this.EvaluateArgument(args[0])),
            InstructionType.Address => () => this.Emit(this.EvaluateArgument(args[0])),
            InstructionType.SPOffset => () => this.Emit(this.EvaluateArgument(args[1])),
            InstructionType.BaseOffset => () => { this.Emit(this.EvaluateArgument(args[0])); this.Emit(this.EvaluateArgument(args[1])); }
            ,
            _ => () => { }
        };

        x.Invoke();
    }

    public override object VisitSymbolline([NotNull] D1AssemblyParser.SymbollineContext context)
    {
        var symbol = context.symbol().LABEL().GetText();
        this.AddSymbolAtAddress(symbol);
        return base.VisitSymbolline(context);
    }

    public override object VisitInstrline([NotNull] D1AssemblyParser.InstrlineContext context)
    {

        var instruction = context.LABEL().GetText();
        var args = context.arglist()._args;

        if (!this.TryGetInstructionTypeFromArguments(args, out var type))
        {
            throw new Exception($"Invalid set of arguments");
        }

        if (context.symbol() != null && instruction.ToLower() != "equ")
        {
            var symbol = context.symbol().LABEL().GetText();
            this.AddSymbolAtAddress(symbol);
        }

        var s = context.symbol() == null ? null : context.symbol().LABEL().GetText();

        Action whatdo = instruction.ToLower() switch
        {
            "org" => () => this.ExecuteORG(args[0], type),
            "equ" => () => this.ExecuteEQU(s, args[0], type),
            _ => () =>
            {
                if (!this.TryGetInstruction(instruction, type, out var instr))
                {
                    throw new Exception($"Unknown instruction: {instruction} with type {type}");
                }

                this.EmitInstruction(instr, args);
            }
        };

        whatdo.Invoke();

        return base.VisitInstrline(context);
    }

    public byte[] Assemble(string s)
    {
        var input = new AntlrInputStream(s);
        var lexer = new D1AssemblyLexer(input);
        var tokens = new CommonTokenStream(lexer);
        var parser = new D1AssemblyParser(tokens);
        var tree = parser.program();

        this.Visit(tree);

        if (this.Bytes.Count > 256)
        {
            throw new Exception($"Program is too large: {this.Bytes.Count} / 256 available bytes");
        }

        // Pad with 0s to 256 bytes
        this.Bytes.AddRange(Enumerable.Repeat((byte)0, 256 - this.Bytes.Count));

        this.Bytes[255] = (byte)this.orgAddress;

        return this.Bytes.ToArray();
    }
}