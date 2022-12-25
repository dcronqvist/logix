using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace FlispPlugin;

public enum InstructionType
{
    INHERENT,
    IMMEDIATE,
    ABSOLUTE,
    RELATIVE,
}

public class Instruction
{
    public string Name { get; set; }
    public byte OpCode { get; set; }
    public InstructionType Type { get; set; }

    public Instruction(string name, byte opCode, InstructionType type)
    {
        this.Name = name;
        this.OpCode = opCode;
        this.Type = type;
    }
}

public class Assembler : FLISPAssemblyBaseVisitor<byte[]>
{
    public List<Instruction> Instructions { get; set; } = new();
    public byte[] Data { get; set; } = new byte[2048]; // Only this much memory is available.
    public byte CurrentAddress { get; set; } = 0;

    public Dictionary<string, byte> Symbols { get; set; } = new();

    public Assembler()
    {
        this.AddInstruction("NOP", 0x00, InstructionType.INHERENT);
        this.AddInstruction("ANDCC", 0x01, InstructionType.IMMEDIATE);
        this.AddInstruction("ORCC", 0x02, InstructionType.IMMEDIATE);
        // 0x03 is undefined
        // 0x04 is undefined
        this.AddInstruction("CLRA", 0x05, InstructionType.INHERENT);
        this.AddInstruction("NEGA", 0x06, InstructionType.INHERENT);
        this.AddInstruction("INCA", 0x07, InstructionType.INHERENT);
        this.AddInstruction("DECA", 0x08, InstructionType.INHERENT);
        this.AddInstruction("TSTA", 0x09, InstructionType.INHERENT);
        this.AddInstruction("COMA", 0x0A, InstructionType.INHERENT);
        this.AddInstruction("LSLA", 0x0B, InstructionType.INHERENT);
        this.AddInstruction("LSRA", 0x0C, InstructionType.INHERENT);
        this.AddInstruction("ROLA", 0x0D, InstructionType.INHERENT);
        this.AddInstruction("RORA", 0x0E, InstructionType.INHERENT);
        this.AddInstruction("ASRA", 0x0F, InstructionType.INHERENT);
        this.AddInstruction("PSHA", 0x10, InstructionType.INHERENT);
        this.AddInstruction("PSHX", 0x11, InstructionType.INHERENT);
        this.AddInstruction("PSHY", 0x12, InstructionType.INHERENT);
        this.AddInstruction("PSHC", 0x13, InstructionType.INHERENT);
        this.AddInstruction("PULA", 0x14, InstructionType.INHERENT);
        this.AddInstruction("PULX", 0x15, InstructionType.INHERENT);
        this.AddInstruction("PULY", 0x16, InstructionType.INHERENT);
        this.AddInstruction("PULC", 0x17, InstructionType.INHERENT);

        this.AddInstruction("BSR", 0x20, InstructionType.RELATIVE);
        this.AddInstruction("BRA", 0x21, InstructionType.RELATIVE);
        this.AddInstruction("BMI", 0x22, InstructionType.RELATIVE);
        this.AddInstruction("BPL", 0x23, InstructionType.RELATIVE);
        this.AddInstruction("BEQ", 0x24, InstructionType.RELATIVE);
        this.AddInstruction("BNE", 0x25, InstructionType.RELATIVE);
        this.AddInstruction("BVS", 0x26, InstructionType.RELATIVE);
        this.AddInstruction("BVC", 0x27, InstructionType.RELATIVE);
        this.AddInstruction("BCS", 0x28, InstructionType.RELATIVE);
        this.AddInstruction("BCC", 0x29, InstructionType.RELATIVE);
        this.AddInstruction("BHI", 0x2A, InstructionType.RELATIVE);
        this.AddInstruction("BLS", 0x2B, InstructionType.RELATIVE);
        this.AddInstruction("BGT", 0x2C, InstructionType.RELATIVE);
        this.AddInstruction("BGE", 0x2D, InstructionType.RELATIVE);
        this.AddInstruction("BLE", 0x2E, InstructionType.RELATIVE);
        this.AddInstruction("BLT", 0x2F, InstructionType.RELATIVE);

        this.AddInstruction("STX", 0x30, InstructionType.ABSOLUTE);
        this.AddInstruction("STY", 0x31, InstructionType.ABSOLUTE);
        this.AddInstruction("STSP", 0x32, InstructionType.ABSOLUTE);
        this.AddInstruction("JMP", 0x33, InstructionType.ABSOLUTE);
        this.AddInstruction("JSR", 0x34, InstructionType.ABSOLUTE);
        this.AddInstruction("CLR", 0x35, InstructionType.ABSOLUTE);
        this.AddInstruction("NEG", 0x36, InstructionType.ABSOLUTE);
        this.AddInstruction("INC", 0x37, InstructionType.ABSOLUTE);
        this.AddInstruction("DEC", 0x38, InstructionType.ABSOLUTE);
        this.AddInstruction("TST", 0x39, InstructionType.ABSOLUTE);
        this.AddInstruction("COM", 0x3A, InstructionType.ABSOLUTE);
        this.AddInstruction("LSL", 0x3B, InstructionType.ABSOLUTE);
        this.AddInstruction("LSR", 0x3C, InstructionType.ABSOLUTE);
        this.AddInstruction("ROL", 0x3D, InstructionType.ABSOLUTE);
        this.AddInstruction("ROR", 0x3E, InstructionType.ABSOLUTE);
        this.AddInstruction("ASR", 0x3F, InstructionType.ABSOLUTE);

        this.AddInstruction("RTS", 0x43, InstructionType.INHERENT);

        this.AddInstruction("LDX", 0x90, InstructionType.IMMEDIATE);
        this.AddInstruction("LDY", 0x91, InstructionType.IMMEDIATE);
        this.AddInstruction("LDSP", 0x92, InstructionType.IMMEDIATE);
        this.AddInstruction("SUBA", 0x94, InstructionType.IMMEDIATE);
        this.AddInstruction("ADDA", 0x96, InstructionType.IMMEDIATE);
        this.AddInstruction("CMPA", 0x97, InstructionType.IMMEDIATE);

        this.AddInstruction("LDX", 0xA0, InstructionType.ABSOLUTE);
        this.AddInstruction("LDY", 0xA1, InstructionType.ABSOLUTE);
        this.AddInstruction("LDSP", 0xA2, InstructionType.ABSOLUTE);
        this.AddInstruction("SUBA", 0xA4, InstructionType.ABSOLUTE);
        this.AddInstruction("ADDA", 0xA6, InstructionType.ABSOLUTE);
        this.AddInstruction("CMPA", 0xA7, InstructionType.ABSOLUTE);

        this.AddInstruction("STA", 0xE1, InstructionType.ABSOLUTE);

        this.AddInstruction("LDA", 0xF0, InstructionType.IMMEDIATE);
        this.AddInstruction("LDA", 0xF1, InstructionType.ABSOLUTE);
    }

    private bool TryGetInstruction(string opcode, InstructionType type, out Instruction instruction)
    {
        foreach (var inst in this.Instructions)
        {
            if (inst.Name == opcode && inst.Type == type)
            {
                instruction = inst;
                return true;
            }
        }

        instruction = null;
        return false;
    }

    private void EmitByte(byte b)
    {
        this.Data[this.CurrentAddress] = b;
        this.CurrentAddress++;
    }

    private void AddSymbolAtCurrentAddress(string name)
    {
        this.AddSymbolWithValue(name, this.CurrentAddress);
    }

    private void AddSymbolWithValue(string name, byte value)
    {
        this.Symbols.Add(name, value);

        if (this._usedSymbols.ContainsKey(name))
        {
            foreach (var addr in this._usedSymbols[name])
            {
                this.Data[addr] = this.Symbols[name];
            }

            this._usedSymbols.Remove(name);
        }

        if (this._usedSymbolsRelative.ContainsKey(name))
        {
            foreach (var addr in this._usedSymbolsRelative[name])
            {
                int relative = (byte)(this.Symbols[name] - addr);
                if (relative < 0)
                {
                    relative = 256 + relative;
                }

                this.Data[addr] = (byte)relative;
            }

            this._usedSymbolsRelative.Remove(name);
        }
    }

    private void AddInstruction(string name, byte opCode, InstructionType type)
    {
        this.Instructions.Add(new Instruction(name, opCode, type));
    }

    private Dictionary<string, List<byte>> _usedSymbols = new();
    private Dictionary<string, List<byte>> _usedSymbolsRelative = new();
    private byte EvaluateNumber(byte address, FLISPAssemblyParser.NumberContext num)
    {
        if (num.DECI() != null)
        {
            return byte.Parse(num.DECI().GetText());
        }
        else if (num.HEXADECI() != null)
        {
            return byte.Parse(num.HEXADECI().GetText().Substring(1), System.Globalization.NumberStyles.HexNumber);
        }
        else if (num.SYMB() != null)
        {
            if (this.Symbols.ContainsKey(num.SYMB().GetText()) == false)
            {
                // Some way to wait until this is defined. And when it is
                // fill in all the spots that need this value.

                if (!this._usedSymbols.ContainsKey(num.SYMB().GetText()))
                {
                    this._usedSymbols.Add(num.SYMB().GetText(), new List<byte>());
                }

                this._usedSymbols[num.SYMB().GetText()].Add(address);

                return 0;
            }

            return this.Symbols[num.SYMB().GetText()];
        }

        return 0;
    }

    private byte EvaluateNumberAsRelative(byte address, FLISPAssemblyParser.NumberContext num)
    {
        if (num.DECI() != null)
        {
            return byte.Parse(num.DECI().GetText());
        }
        else if (num.HEXADECI() != null)
        {
            return byte.Parse(num.HEXADECI().GetText().Substring(2), System.Globalization.NumberStyles.HexNumber);
        }
        else if (num.SYMB() != null)
        {
            if (this.Symbols.ContainsKey(num.SYMB().GetText()) == false)
            {
                // Some way to wait until this is defined. And when it is
                // fill in all the spots that need this value.

                if (!this._usedSymbolsRelative.ContainsKey(num.SYMB().GetText()))
                {
                    this._usedSymbolsRelative.Add(num.SYMB().GetText(), new List<byte>());
                }

                this._usedSymbolsRelative[num.SYMB().GetText()].Add(address);

                return 0;
            }

            var symbol = this.Symbols[num.SYMB().GetText()];
            var relative = symbol - this.CurrentAddress;
            if (relative < 0)
            {
                relative = 256 + relative;
            }
            return (byte)relative;
        }

        return 0;
    }

    private byte[] EvaluateNumberList(FLISPAssemblyParser.NumberlistContext numList)
    {
        var bytes = new List<byte>();
        this.EvaluateNumberListHelper(numList, ref bytes);
        return bytes.ToArray();
    }

    private void EvaluateNumberListHelper(FLISPAssemblyParser.NumberlistContext numList, ref List<byte> bytes)
    {
        if (numList.number() != null)
        {
            bytes.Add(this.EvaluateNumber(this.CurrentAddress, numList.number()));
        }
        if (numList.numberlist() != null)
        {
            this.EvaluateNumberListHelper(numList.numberlist(), ref bytes);
        }
    }

    public override byte[] VisitOrgdir([NotNull] FLISPAssemblyParser.OrgdirContext context)
    {
        var num = this.EvaluateNumber(this.CurrentAddress, context.number());
        this.CurrentAddress = num;
        this.Data[255] = num;
        return base.VisitOrgdir(context);
    }

    public override byte[] VisitRmbdir([NotNull] FLISPAssemblyParser.RmbdirContext context)
    {
        var amount = this.EvaluateNumber(this.CurrentAddress, context.number());
        this.CurrentAddress += amount;
        return base.VisitRmbdir(context);
    }

    public override byte[] VisitFcbdir([NotNull] FLISPAssemblyParser.FcbdirContext context)
    {
        var num = this.EvaluateNumberList(context.numberlist());
        foreach (var b in num)
        {
            this.EmitByte(b);
        }
        return base.VisitFcbdir(context);
    }

    public override byte[] VisitFcsdir([NotNull] FLISPAssemblyParser.FcsdirContext context)
    {
        var s = context.STRINGLITERAL().GetText();
        var bytes = Encoding.ASCII.GetBytes(s);
        foreach (var b in bytes)
        {
            this.EmitByte(b);
        }
        return base.VisitFcsdir(context);
    }

    public override byte[] VisitEqudir([NotNull] FLISPAssemblyParser.EqudirContext context)
    {
        var symb = context.SYMB().GetText();
        var num = this.EvaluateNumber(this.CurrentAddress, context.number());
        this.AddSymbolWithValue(symb, num);
        return base.VisitEqudir(context);
    }

    public override byte[] VisitLine([NotNull] FLISPAssemblyParser.LineContext context)
    {
        if (context.SYMB() != null)
        {
            var symb = context.SYMB().GetText();
            this.AddSymbolAtCurrentAddress(symb);
        }

        return base.VisitLine(context);
    }

    public override byte[] VisitInstr([NotNull] FLISPAssemblyParser.InstrContext context)
    {
        var opcode = context.ISTRING().GetText();

        if (context.number() != null)
        {
            // ABSOLUTE & RELATIVE
            if (this.TryGetInstruction(opcode, InstructionType.RELATIVE, out var instruction1))
            {
                this.EmitByte(instruction1.OpCode);
                var num = this.EvaluateNumberAsRelative(this.CurrentAddress, context.number());
                this.EmitByte(num);
            }
            else if (this.TryGetInstruction(opcode, InstructionType.ABSOLUTE, out var instruction))
            {
                this.EmitByte(instruction.OpCode);
                var num = this.EvaluateNumber(this.CurrentAddress, context.number());
                this.EmitByte(num);
            }
        }
        else if (context.immediate() != null)
        {
            // IMMEDIATE
            if (this.TryGetInstruction(opcode, InstructionType.IMMEDIATE, out var instruction))
            {
                this.EmitByte(instruction.OpCode);
                var num = this.EvaluateNumber(this.CurrentAddress, context.immediate().number());
                this.EmitByte(num);
            }
        }
        else if (context.number() == null && context.immediate() == null)
        {
            // INHERENT
            if (this.TryGetInstruction(opcode, InstructionType.INHERENT, out var instruction))
            {
                this.EmitByte(instruction.OpCode);
            }
        }

        return base.VisitInstr(context);
    }

    public byte[] Assemble(string input)
    {
        var inputStream = new AntlrInputStream(input);
        var lexer = new FLISPAssemblyLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new FLISPAssemblyParser(tokenStream);
        var tree = parser.program();
        var visitor = new Assembler();
        visitor.Visit(tree);

        if (visitor.CurrentAddress > 255)
        {
            throw new Exception($"Program took {visitor.CurrentAddress} bytes, but only 256 are allocatable.");
        }

        byte[] arr = new byte[256];

        for (int i = 0; i < 256; i++)
        {
            arr[i] = visitor.Data[i];
        }

        return arr;
    }
}