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
    Indirect,
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

    public Dictionary<string, ushort> SymbolAddresses { get; set; } = new();

    public D2Assembler()
    {
        this.AddInstruction("nop", InstructionType.Implicit);
        this.AddInstruction("lda", InstructionType.Immediate);
        this.AddInstruction("lda", InstructionType.Absolute);
        this.AddInstruction("lda", InstructionType.AbsoluteX);
        this.AddInstruction("lda", InstructionType.AbsoluteY);
        this.AddInstruction("lda", InstructionType.Indirect);
        this.AddInstruction("lda", InstructionType.IndirectX);
        this.AddInstruction("lda", InstructionType.IndirectY);
        this.AddInstruction("sta", InstructionType.Absolute);
        this.AddInstruction("sta", InstructionType.AbsoluteX);
        this.AddInstruction("sta", InstructionType.AbsoluteY);
        this.AddInstruction("sta", InstructionType.Indirect);
        this.AddInstruction("sta", InstructionType.IndirectX);
        this.AddInstruction("sta", InstructionType.IndirectY);

        this.AddInstruction("ldx", InstructionType.Immediate);
        this.AddInstruction("ldx", InstructionType.Absolute);
        this.AddInstruction("ldx", InstructionType.AbsoluteX);
        this.AddInstruction("ldx", InstructionType.AbsoluteY);
        this.AddInstruction("ldx", InstructionType.Indirect);
        this.AddInstruction("ldx", InstructionType.IndirectX);
        this.AddInstruction("ldx", InstructionType.IndirectY);
        this.AddInstruction("stx", InstructionType.Absolute);
        this.AddInstruction("stx", InstructionType.AbsoluteX);
        this.AddInstruction("stx", InstructionType.AbsoluteY);
        this.AddInstruction("stx", InstructionType.Indirect);
        this.AddInstruction("stx", InstructionType.IndirectX);
        this.AddInstruction("stx", InstructionType.IndirectY);

        this.AddInstruction("ldy", InstructionType.Immediate);
        this.AddInstruction("ldy", InstructionType.Absolute);
        this.AddInstruction("ldy", InstructionType.AbsoluteX);
        this.AddInstruction("ldy", InstructionType.AbsoluteY);
        this.AddInstruction("ldy", InstructionType.Indirect);
        this.AddInstruction("ldy", InstructionType.IndirectX);
        this.AddInstruction("ldy", InstructionType.IndirectY);
        this.AddInstruction("sty", InstructionType.Absolute);
        this.AddInstruction("sty", InstructionType.AbsoluteX);
        this.AddInstruction("sty", InstructionType.AbsoluteY);
        this.AddInstruction("sty", InstructionType.Indirect);
        this.AddInstruction("sty", InstructionType.IndirectX);
        this.AddInstruction("sty", InstructionType.IndirectY);

        this.AddInstruction("tax", InstructionType.Implicit);
        this.AddInstruction("tay", InstructionType.Implicit);
        this.AddInstruction("txa", InstructionType.Implicit);
        this.AddInstruction("tya", InstructionType.Implicit);

        this.AddInstruction("ina", InstructionType.Implicit);
        this.AddInstruction("inx", InstructionType.Implicit);
        this.AddInstruction("iny", InstructionType.Implicit);
        this.AddInstruction("inc", InstructionType.Absolute);
        this.AddInstruction("inc", InstructionType.AbsoluteX);
        this.AddInstruction("inc", InstructionType.AbsoluteY);
        this.AddInstruction("inc", InstructionType.Indirect);
        this.AddInstruction("inc", InstructionType.IndirectX);
        this.AddInstruction("inc", InstructionType.IndirectY);

        this.AddInstruction("dea", InstructionType.Implicit);
        this.AddInstruction("dex", InstructionType.Implicit);
        this.AddInstruction("dey", InstructionType.Implicit);
        this.AddInstruction("dec", InstructionType.Absolute);
        this.AddInstruction("dec", InstructionType.AbsoluteX);
        this.AddInstruction("dec", InstructionType.AbsoluteY);
        this.AddInstruction("dec", InstructionType.Indirect);
        this.AddInstruction("dec", InstructionType.IndirectX);
        this.AddInstruction("dec", InstructionType.IndirectY);

        this.AddInstruction("adc", InstructionType.Immediate);
        this.AddInstruction("adc", InstructionType.Absolute);
        this.AddInstruction("adc", InstructionType.AbsoluteX);
        this.AddInstruction("adc", InstructionType.AbsoluteY);
        this.AddInstruction("adc", InstructionType.Indirect);
        this.AddInstruction("adc", InstructionType.IndirectX);
        this.AddInstruction("adc", InstructionType.IndirectY);

        this.AddInstruction("sbc", InstructionType.Immediate);
        this.AddInstruction("sbc", InstructionType.Absolute);
        this.AddInstruction("sbc", InstructionType.AbsoluteX);
        this.AddInstruction("sbc", InstructionType.AbsoluteY);
        this.AddInstruction("sbc", InstructionType.Indirect);
        this.AddInstruction("sbc", InstructionType.IndirectX);
        this.AddInstruction("sbc", InstructionType.IndirectY);

        this.AddInstruction("and", InstructionType.Immediate);
        this.AddInstruction("and", InstructionType.Absolute);
        this.AddInstruction("and", InstructionType.AbsoluteX);
        this.AddInstruction("and", InstructionType.AbsoluteY);
        this.AddInstruction("and", InstructionType.Indirect);
        this.AddInstruction("and", InstructionType.IndirectX);
        this.AddInstruction("and", InstructionType.IndirectY);

        this.AddInstruction("ora", InstructionType.Immediate);
        this.AddInstruction("ora", InstructionType.Absolute);
        this.AddInstruction("ora", InstructionType.AbsoluteX);
        this.AddInstruction("ora", InstructionType.AbsoluteY);
        this.AddInstruction("ora", InstructionType.Indirect);
        this.AddInstruction("ora", InstructionType.IndirectX);
        this.AddInstruction("ora", InstructionType.IndirectY);

        this.AddInstruction("eor", InstructionType.Immediate);
        this.AddInstruction("eor", InstructionType.Absolute);
        this.AddInstruction("eor", InstructionType.AbsoluteX);
        this.AddInstruction("eor", InstructionType.AbsoluteY);
        this.AddInstruction("eor", InstructionType.Indirect);
        this.AddInstruction("eor", InstructionType.IndirectX);
        this.AddInstruction("eor", InstructionType.IndirectY);

        this.AddInstruction("rol", InstructionType.Implicit);
        this.AddInstruction("ror", InstructionType.Implicit);

        this.AddInstruction("pha", InstructionType.Implicit);
        this.AddInstruction("pla", InstructionType.Implicit);
        this.AddInstruction("phx", InstructionType.Implicit);
        this.AddInstruction("plx", InstructionType.Implicit);
        this.AddInstruction("phy", InstructionType.Implicit);
        this.AddInstruction("ply", InstructionType.Implicit);
        this.AddInstruction("php", InstructionType.Implicit);
        this.AddInstruction("plp", InstructionType.Implicit);

        this.AddInstruction("jmp", InstructionType.Absolute);
        this.AddInstruction("jsr", InstructionType.Absolute);
        this.AddInstruction("rts", InstructionType.Implicit);

        this.AddInstruction("jeq", InstructionType.Absolute);
        this.AddInstruction("jne", InstructionType.Absolute);
        this.AddInstruction("jcs", InstructionType.Absolute);
        this.AddInstruction("jcc", InstructionType.Absolute);
        this.AddInstruction("jns", InstructionType.Absolute);
        this.AddInstruction("jnc", InstructionType.Absolute);
        this.AddInstruction("jvs", InstructionType.Absolute);
        this.AddInstruction("jvc", InstructionType.Absolute);

        this.AddInstruction("cmp", InstructionType.Immediate);
        this.AddInstruction("cmp", InstructionType.Absolute);
        this.AddInstruction("cmp", InstructionType.AbsoluteX);
        this.AddInstruction("cmp", InstructionType.AbsoluteY);
        this.AddInstruction("cmp", InstructionType.Indirect);
        this.AddInstruction("cmp", InstructionType.IndirectX);
        this.AddInstruction("cmp", InstructionType.IndirectY);

        this.AddInstruction("bit", InstructionType.Immediate);
        this.AddInstruction("bit", InstructionType.Absolute);
        this.AddInstruction("bit", InstructionType.AbsoluteX);
        this.AddInstruction("bit", InstructionType.AbsoluteY);
        this.AddInstruction("bit", InstructionType.Indirect);
        this.AddInstruction("bit", InstructionType.IndirectX);
        this.AddInstruction("bit", InstructionType.IndirectY);

        this.AddInstruction("clz", InstructionType.Implicit);
        this.AddInstruction("sez", InstructionType.Implicit);
        this.AddInstruction("clc", InstructionType.Implicit);
        this.AddInstruction("sec", InstructionType.Implicit);
        this.AddInstruction("cln", InstructionType.Implicit);
        this.AddInstruction("sen", InstructionType.Implicit);
        this.AddInstruction("clv", InstructionType.Implicit);
        this.AddInstruction("sev", InstructionType.Implicit);
        this.AddInstruction("cli", InstructionType.Implicit);
        this.AddInstruction("sei", InstructionType.Implicit);

        this.AddInstruction("rti", InstructionType.Implicit);
        this.AddInstruction("lsp", InstructionType.Implicit); // will take x as high byte and y as low byte and put into stack pointer
        this.AddInstruction("brk", InstructionType.Implicit);
    }

    private byte _opcode = 0;
    private void AddInstruction(string identifier, InstructionType type)
    {
        Instructions.Add(new Instruction(identifier, _opcode++, type));
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
            else if (arg.indirect is not null)
            {
                type = InstructionType.Indirect;
                return true;
            }
            else if (arg.indirectx is not null)
            {
                type = InstructionType.IndirectX;
                return true;
            }
            else if (arg.indirecty is not null)
            {
                type = InstructionType.IndirectY;
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
            _ => () => this.Emit(this.EvaluateArgument(arg))
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