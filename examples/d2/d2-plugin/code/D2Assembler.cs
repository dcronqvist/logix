using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace D2Plugin;

public enum InstructionType
{
    Implicit,
    Immediate,
    Absolute,
    AbsoluteX,
    AbsoluteY,
    IndirectX,
    IndirectY,
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

public class D2Assembler : D2AssemblyBaseVisitor<object>
{
    public List<Instruction> Instructions { get; set; } = new();
    public Dictionary<string, List<byte>> Symbols { get; set; } = new();
    public Dictionary<string, List<int>> SymbolReferences { get; set; } = new();
    public List<byte> Bytes { get; set; } = new();
    public int CurrentAddress { get; set; } = 0;

    public D2Assembler()
    {
        this.AddInstruction("nop", 0x00, InstructionType.Implicit);
        this.AddInstruction("lda", 0x01, InstructionType.Immediate);
        this.AddInstruction("lda", 0x02, InstructionType.Absolute);
        this.AddInstruction("lda", 0x03, InstructionType.AbsoluteX);
        this.AddInstruction("lda", 0x04, InstructionType.AbsoluteY);
        this.AddInstruction("sta", 0x05, InstructionType.Absolute);
        this.AddInstruction("sta", 0x06, InstructionType.AbsoluteX);
        this.AddInstruction("sta", 0x07, InstructionType.AbsoluteY);

        this.AddInstruction("ldx", 0x08, InstructionType.Immediate);
        this.AddInstruction("ldx", 0x09, InstructionType.Absolute);
        this.AddInstruction("ldx", 0x0A, InstructionType.AbsoluteX);
        this.AddInstruction("ldx", 0x0B, InstructionType.AbsoluteY);
        this.AddInstruction("stx", 0x0C, InstructionType.Absolute);
        this.AddInstruction("stx", 0x0D, InstructionType.AbsoluteX);
        this.AddInstruction("stx", 0x0E, InstructionType.AbsoluteY);

        this.AddInstruction("ldy", 0x0F, InstructionType.Immediate);
        this.AddInstruction("ldy", 0x10, InstructionType.Absolute);
        this.AddInstruction("ldy", 0x11, InstructionType.AbsoluteX);
        this.AddInstruction("ldy", 0x12, InstructionType.AbsoluteY);
        this.AddInstruction("sty", 0x13, InstructionType.Absolute);
        this.AddInstruction("sty", 0x14, InstructionType.AbsoluteX);
        this.AddInstruction("sty", 0x15, InstructionType.AbsoluteY);

        this.AddInstruction("tax", 0x16, InstructionType.Implicit);
        this.AddInstruction("tay", 0x17, InstructionType.Implicit);
        this.AddInstruction("txa", 0x18, InstructionType.Implicit);
        this.AddInstruction("tya", 0x19, InstructionType.Implicit);

        this.AddInstruction("ina", 0x1A, InstructionType.Implicit);
        this.AddInstruction("inx", 0x1B, InstructionType.Implicit);
        this.AddInstruction("iny", 0x1C, InstructionType.Implicit);
        this.AddInstruction("inc", 0x1D, InstructionType.Absolute);
        this.AddInstruction("inc", 0x1E, InstructionType.AbsoluteX);
        this.AddInstruction("inc", 0x1F, InstructionType.AbsoluteY);

        this.AddInstruction("dea", 0x20, InstructionType.Implicit);
        this.AddInstruction("dex", 0x21, InstructionType.Implicit);
        this.AddInstruction("dey", 0x22, InstructionType.Implicit);
        this.AddInstruction("dec", 0x23, InstructionType.Absolute);
        this.AddInstruction("dec", 0x24, InstructionType.AbsoluteX);
        this.AddInstruction("dec", 0x25, InstructionType.AbsoluteY);

        this.AddInstruction("adc", 0x26, InstructionType.Immediate);
        this.AddInstruction("adc", 0x27, InstructionType.Absolute);
        this.AddInstruction("adc", 0x28, InstructionType.AbsoluteX);
        this.AddInstruction("adc", 0x29, InstructionType.AbsoluteY);

        this.AddInstruction("sbc", 0x2A, InstructionType.Immediate);
        this.AddInstruction("sbc", 0x2B, InstructionType.Absolute);
        this.AddInstruction("sbc", 0x2C, InstructionType.AbsoluteX);
        this.AddInstruction("sbc", 0x2D, InstructionType.AbsoluteY);

        this.AddInstruction("and", 0x2E, InstructionType.Immediate);
        this.AddInstruction("and", 0x2F, InstructionType.Absolute);
        this.AddInstruction("and", 0x30, InstructionType.AbsoluteX);
        this.AddInstruction("and", 0x31, InstructionType.AbsoluteY);

        this.AddInstruction("ora", 0x32, InstructionType.Immediate);
        this.AddInstruction("ora", 0x33, InstructionType.Absolute);
        this.AddInstruction("ora", 0x34, InstructionType.AbsoluteX);
        this.AddInstruction("ora", 0x35, InstructionType.AbsoluteY);

        this.AddInstruction("eor", 0x36, InstructionType.Immediate);
        this.AddInstruction("eor", 0x37, InstructionType.Absolute);
        this.AddInstruction("eor", 0x38, InstructionType.AbsoluteX);
        this.AddInstruction("eor", 0x39, InstructionType.AbsoluteY);

        this.AddInstruction("rol", 0x3A, InstructionType.Implicit);
        this.AddInstruction("ror", 0x3B, InstructionType.Implicit);

        this.AddInstruction("pha", 0x3C, InstructionType.Implicit);
        this.AddInstruction("pla", 0x3D, InstructionType.Implicit);
        this.AddInstruction("phx", 0x3E, InstructionType.Implicit);
        this.AddInstruction("plx", 0x3F, InstructionType.Implicit);
        this.AddInstruction("phy", 0x40, InstructionType.Implicit);
        this.AddInstruction("ply", 0x41, InstructionType.Implicit);
        this.AddInstruction("php", 0x42, InstructionType.Implicit);
        this.AddInstruction("plp", 0x43, InstructionType.Implicit);

        this.AddInstruction("jmp", 0x44, InstructionType.Absolute);
        this.AddInstruction("jsr", 0x45, InstructionType.Absolute);
        this.AddInstruction("rts", 0x46, InstructionType.Implicit);

        this.AddInstruction("jeq", 0x47, InstructionType.Absolute);
        this.AddInstruction("jne", 0x48, InstructionType.Absolute);
        this.AddInstruction("jcs", 0x49, InstructionType.Absolute);
        this.AddInstruction("jcc", 0x4A, InstructionType.Absolute);
        this.AddInstruction("jns", 0x4B, InstructionType.Absolute);
        this.AddInstruction("jnc", 0x4C, InstructionType.Absolute);
        this.AddInstruction("jvs", 0x4D, InstructionType.Absolute);
        this.AddInstruction("jvc", 0x4E, InstructionType.Absolute);

        this.AddInstruction("cmp", 0x4F, InstructionType.Immediate);
        this.AddInstruction("cmp", 0x50, InstructionType.Absolute);
        this.AddInstruction("cmp", 0x51, InstructionType.AbsoluteX);
        this.AddInstruction("cmp", 0x52, InstructionType.AbsoluteY);

        this.AddInstruction("bit", 0x53, InstructionType.Immediate);
        this.AddInstruction("bit", 0x54, InstructionType.Absolute);
        this.AddInstruction("bit", 0x55, InstructionType.AbsoluteX);
        this.AddInstruction("bit", 0x56, InstructionType.AbsoluteY);

        this.AddInstruction("clz", 0x57, InstructionType.Implicit);
        this.AddInstruction("sez", 0x58, InstructionType.Implicit);
        this.AddInstruction("clc", 0x59, InstructionType.Implicit);
        this.AddInstruction("sec", 0x5A, InstructionType.Implicit);
        this.AddInstruction("cln", 0x5B, InstructionType.Implicit);
        this.AddInstruction("sen", 0x5C, InstructionType.Implicit);
        this.AddInstruction("clv", 0x5D, InstructionType.Implicit);
        this.AddInstruction("sev", 0x5E, InstructionType.Implicit);
        this.AddInstruction("cli", 0x5F, InstructionType.Implicit);
        this.AddInstruction("sei", 0x60, InstructionType.Implicit);

        this.AddInstruction("rti", 0x61, InstructionType.Implicit);
        this.AddInstruction("lsp", 0x62, InstructionType.Implicit); // will take x as high byte and y as low byte and put into stack pointer
        this.AddInstruction("brk", 0x63, InstructionType.Implicit);
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

    private void Emit(IEnumerable<byte> bytes)
    {
        foreach (var b in bytes)
        {
            Emit(b);
        }
    }

    private void AddSymbolWithValue(string symbol, List<byte> bytes)
    {
        Symbols.Add(symbol, bytes);

        if (SymbolReferences.ContainsKey(symbol))
        {
            foreach (var reference in SymbolReferences[symbol])
            {
                for (int i = 0; i < bytes.Count; i++)
                {
                    Bytes[reference + i] = bytes[i];
                }
            }
        }
    }

    private void AddSymbolAtAddress(string symbol)
    {
        var list = new List<byte>();
        list.Add((byte)(CurrentAddress & 0xFF));
        list.Add((byte)((CurrentAddress >> 8) & 0xFF));

        this.AddSymbolWithValue(symbol, list);
    }

    private void AddSymbolReference(string symbol)
    {
        if (!SymbolReferences.ContainsKey(symbol))
        {
            SymbolReferences.Add(symbol, new List<int>());
        }

        SymbolReferences[symbol].Add(CurrentAddress);
    }

    private bool TryGetInstructionTypeFromArgument(D2AssemblyParser.ArgumentContext arg, out InstructionType type)
    {
        if (arg is null)
        {
            type = InstructionType.Implicit;
            return true;
        }

        if (arg.immediate() is not null)
        {
            type = InstructionType.Immediate;
            return true;
        }
        else if (arg.number() is not null)
        {
            if (arg.x is not null)
            {
                type = InstructionType.AbsoluteX;
                return true;
            }
            else if (arg.y is not null)
            {
                type = InstructionType.AbsoluteY;
                return true;
            }
            else
            {
                type = InstructionType.Absolute;
                return true;
            }
        }

        type = InstructionType.Immediate;
        return false;
    }

    private bool TryGetNumberBits(D2AssemblyParser.NumberContext num, out int bits)
    {
        if (num.bin is not null)
        {
            bits = num.BINARY().GetText().Substring(1).Length;
            return true;
        }
        else if (num.hex is not null)
        {
            bits = num.HEXADECIMAL().GetText().Substring(1).Length * 4;
            return true;
        }
        else if (num.dec is not null)
        {
            var dec = num.DECIMAL().GetText();

            var log = Math.Log2(int.Parse(dec));

            bits = (int)Math.Ceiling(log);
            return true;
        }

        bits = 0;
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

    private void ExecuteORG(D2AssemblyParser.ArgumentContext arg, InstructionType type)
    {
        if (type != InstructionType.Absolute)
        {
            throw new Exception($"org expects an address as argument");
        }

        var num = arg.number();
        if (num.bin is null || num.hex is null || num.dec is null)
        {
            throw new Exception($"org expects a literal address as argument");
        }

        var (bytes, number) = this.EvaluateNumber(num);

        if (number < CurrentAddress)
        {
            throw new Exception($"org expects an address greater than the current address");
        }

        while (number > CurrentAddress)
        {
            this.Emit(0);
        }
    }

    private (List<byte>, ushort) EvaluateNumber(D2AssemblyParser.NumberContext num)
    {
        if (num.bin is not null)
        {
            // Binary number

            if (this.TryGetNumberBits(num, out var bits))
            {
                if (bits > 8)
                {
                    // Must return two bytes
                    var x = Convert.ToUInt16(num.BINARY().GetText().Substring(1), 2);
                    var list = new List<byte>();
                    list.Add((byte)(x & 0xFF));
                    list.Add((byte)(x >> 8));
                    return (list, x);
                }
                else
                {
                    // Must return one byte
                    var x = Convert.ToByte(num.BINARY().GetText().Substring(1), 2);
                    var list = new List<byte>();
                    list.Add(x);
                    return (list, x);
                }
            }
        }
        else if (num.hex is not null)
        {
            // Hexadecimal number

            if (this.TryGetNumberBits(num, out var bits))
            {
                if (bits > 8)
                {
                    // Must return two bytes
                    var x = Convert.ToUInt16(num.HEXADECIMAL().GetText().Substring(1), 16);
                    var list = new List<byte>();
                    list.Add((byte)(x & 0xFF));
                    list.Add((byte)(x >> 8));
                    return (list, x);
                }
                else
                {
                    // Must return one byte
                    var x = Convert.ToByte(num.HEXADECIMAL().GetText().Substring(1), 16);
                    var list = new List<byte>();
                    list.Add(x);
                    return (list, x);
                }
            }
        }
        else if (num.dec is not null)
        {
            // Decimal number

            if (this.TryGetNumberBits(num, out var bits))
            {
                if (bits > 8)
                {
                    // Must return two bytes
                    var x = Convert.ToUInt16(num.DECIMAL().GetText(), 10);
                    var list = new List<byte>();
                    list.Add((byte)(x & 0xFF));
                    list.Add((byte)(x >> 8));
                    return (list, x);
                }
                else
                {
                    // Must return one byte
                    var x = Convert.ToByte(num.DECIMAL().GetText(), 10);
                    var list = new List<byte>();
                    list.Add(x);
                    return (list, x);
                }
            }
        }
        else if (num.lab is not null)
        {
            // Is label
            var label = num.LABEL().GetText();
            if (Symbols.ContainsKey(label))
            {
                var s = Symbols[label];
                return (s, (ushort)(s[0] + (s[1] << 8)));
            }
            else
            {
                AddSymbolReference(label);
                return (new List<byte>() { 0, 0 }, 0);
            }
        }
        else if (num.lowbyte is not null)
        {
            var (l, n) = this.EvaluateNumber(num.number(0));
            return (new List<byte>() { (byte)(n & 0xFF) }, (byte)(n & 0xFF));
        }
        else if (num.highbyte is not null)
        {
            var (l, n) = this.EvaluateNumber(num.number(0));
            return (new List<byte>() { (byte)(n >> 8) }, (byte)(n >> 8));
        }
        else if (num.plus is not null)
        {
            var (lleft, nleft) = this.EvaluateNumber(num.number(0));
            var (lright, nright) = this.EvaluateNumber(num.number(1));

            var result = nleft + nright;
            Console.WriteLine($"plus: {nleft} + {nright} = {result}");

            if (lleft.Count == 1 && lright.Count == 1)
            {
                Console.WriteLine($"plus: {nleft} + {nright} = {result} (byte)");
                return (new List<byte>() { (byte)result }, (byte)result);
            }
            else
            {
                return (new List<byte>() { (byte)(result & 0xFF), (byte)(result >> 8) }, (ushort)result);
            }
        }
        else if (num.minus is not null)
        {
            var (lleft, nleft) = this.EvaluateNumber(num.number(0));
            var (lright, nright) = this.EvaluateNumber(num.number(1));

            var result = nleft - nright;

            if (lleft.Count == 1 && lright.Count == 1)
            {
                return (new List<byte>() { (byte)result }, (byte)result);
            }
            else
            {
                return (new List<byte>() { (byte)(result & 0xFF), (byte)(result >> 8) }, (ushort)result);
            }
        }
        else if (num.mult is not null)
        {
            var (lleft, nleft) = this.EvaluateNumber(num.number(0));
            var (lright, nright) = this.EvaluateNumber(num.number(1));

            var result = nleft * nright;

            if (lleft.Count == 1 && lright.Count == 1)
            {
                return (new List<byte>() { (byte)result }, (byte)result);
            }
            else
            {
                return (new List<byte>() { (byte)(result & 0xFF), (byte)(result >> 8) }, (ushort)result);
            }
        }
        else if (num.and is not null)
        {
            var (lleft, nleft) = this.EvaluateNumber(num.number(0));
            var (lright, nright) = this.EvaluateNumber(num.number(1));

            var result = nleft & nright;

            if (lleft.Count == 1 && lright.Count == 1)
            {
                return (new List<byte>() { (byte)result }, (byte)result);
            }
            else
            {
                return (new List<byte>() { (byte)(result & 0xFF), (byte)(result >> 8) }, (ushort)result);
            }
        }
        else if (num.or is not null)
        {
            var (lleft, nleft) = this.EvaluateNumber(num.number(0));
            var (lright, nright) = this.EvaluateNumber(num.number(1));

            var result = nleft | nright;

            if (lleft.Count == 1 && lright.Count == 1)
            {
                return (new List<byte>() { (byte)result }, (byte)result);
            }
            else
            {
                return (new List<byte>() { (byte)(result & 0xFF), (byte)(result >> 8) }, (ushort)result);
            }
        }

        throw new Exception($"Invalid number");
    }

    private List<byte> EvaluateArgument(D2AssemblyParser.ArgumentContext arg)
    {
        if (arg.immediate() is not null)
        {
            var (bytes, number) = this.EvaluateNumber(arg.immediate().number());

            if (bytes.Count != 1)
            {
                throw new Exception($"Immediate value must be 8 bits");
            }

            return bytes;
        }
        else if (arg.number() is not null)
        {
            var (bytes, number) = this.EvaluateNumber(arg.number());

            if (bytes.Count == 1)
            {
                // Add a padding of 0
                bytes.Add(0);
            }

            return bytes;
        }

        throw new Exception($"Invalid argument");
    }

    private void EmitInstruction(Instruction instruction, D2AssemblyParser.ArgumentContext arg)
    {
        this.Emit(instruction.Opcode);

        Action x = instruction.Type switch
        {
            InstructionType.Implicit => () => { }
            ,
            InstructionType.Immediate => () => this.Emit(this.EvaluateArgument(arg)),
            InstructionType.Absolute => () =>
            {
                this.Emit(this.EvaluateArgument(arg));
            }
            ,
            InstructionType.AbsoluteX => () => this.Emit(this.EvaluateArgument(arg)),
            InstructionType.AbsoluteY => () => this.Emit(this.EvaluateArgument(arg))
            ,
            _ => () => { }
        };

        x.Invoke();
    }

    public override object VisitConstantline([NotNull] D2AssemblyParser.ConstantlineContext context)
    {
        var label = context.LABEL();

        ushort value = 0;
        int bits = 0;

        if (context.BINARY() is not null)
        {
            value = Convert.ToUInt16(context.BINARY().GetText().Substring(1), 2);
            bits = context.BINARY().GetText().Substring(1).Length;
        }
        else if (context.HEXADECIMAL() is not null)
        {
            value = Convert.ToUInt16(context.HEXADECIMAL().GetText().Substring(1), 16);
            bits = context.HEXADECIMAL().GetText().Substring(1).Length * 4;
        }
        else if (context.DECIMAL() is not null)
        {
            value = Convert.ToUInt16(context.DECIMAL().GetText(), 10);
            bits = (int)Math.Ceiling(Math.Log2(ushort.Parse(context.DECIMAL().GetText())));
        }

        if (bits <= 8)
        {
            this.AddSymbolWithValue(label.GetText(), new List<byte>() { (byte)value });
        }
        else
        {
            this.AddSymbolWithValue(label.GetText(), new List<byte>() { (byte)(value & 0xFF), (byte)(value >> 8) });
        }

        return base.VisitConstantline(context);
    }

    public override object VisitDirectiveline([NotNull] D2AssemblyParser.DirectivelineContext context)
    {
        var dir = context.directive();

        if (dir.orgdir() is not null)
        {
            var (bytes, number) = this.EvaluateNumber(dir.orgdir().number());
            while (this.CurrentAddress < number)
            {
                this.Emit(0);
            }
        }
        else if (dir.worddir() is not null)
        {
            var (bytes, number) = this.EvaluateNumber(dir.worddir().number());
            if (bytes.Count != 2)
            {
                throw new Exception($"Word directive must be 16 bits");
            }

            this.Emit(bytes);
        }
        else if (dir.asciizdir() is not null)
        {
            var s = dir.asciizdir().STRINGLITERAL().GetText().Substring(1, dir.asciizdir().STRINGLITERAL().GetText().Length - 2);
            foreach (var c in s)
            {
                this.Emit((byte)c);
            }
            this.Emit(0);
        }

        return base.VisitDirectiveline(context);
    }

    public override object VisitSymbolline([NotNull] D2AssemblyParser.SymbollineContext context)
    {
        var symbol = context.symbol().LABEL().GetText();
        this.AddSymbolAtAddress(symbol);
        return base.VisitSymbolline(context);
    }

    public override object VisitInstrline([NotNull] D2AssemblyParser.InstrlineContext context)
    {
        var instruction = context.INSTRUCTION().GetText();
        var arg = context.argument();

        if (!this.TryGetInstructionTypeFromArgument(arg, out var type))
        {
            throw new Exception($"Invalid set of arguments");
        }

        if (context.symbol() != null)
        {
            var symbol = context.symbol().LABEL().GetText();
            this.AddSymbolAtAddress(symbol);
        }

        if (!this.TryGetInstruction(instruction, type, out var instr))
        {
            throw new Exception($"Unknown instruction: {instruction} with type {type}");
        }

        this.EmitInstruction(instr, arg);

        return base.VisitInstrline(context);
    }

    public byte[] Assemble(string s)
    {
        var input = new AntlrInputStream(s);
        var lexer = new D2AssemblyLexer(input);
        var tokens = new CommonTokenStream(lexer);
        var parser = new D2AssemblyParser(tokens);
        var tree = parser.prog();

        this.Visit(tree);

        if (this.Bytes.Count > (Math.Pow(2, 16)))
        {
            throw new Exception($"Program is too large: {this.Bytes.Count} / 65536 available bytes");
        }

        return this.Bytes.ToArray();
    }
}