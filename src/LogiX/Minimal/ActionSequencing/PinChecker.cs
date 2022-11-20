using Antlr4.Runtime.Misc;
using LogiX.Architecture.Serialization;

namespace LogiX.Minimal.ActionSequencing;

public class PinChecker : ActionSequenceBaseVisitor<object>
{
    public List<(string, int)> Pins { get; private set; } = new();
    public List<string> PushButtons { get; private set; } = new();
    public List<string> Rams { get; private set; } = new();
    public List<string> TTYs { get; private set; } = new();
    public List<string> Keyboards { get; private set; } = new();
    public List<string> Disks { get; private set; } = new();

    private int ConvertValueStringToInt(string value)
    {
        if (value.StartsWith("0b"))
        {
            return value.Length - 2;
        }
        else if (value.StartsWith("0x"))
        {
            return (value.Length - 2) * 4;
        }
        else
        {
            return value.Split(':').Last().Length;
        }
    }

    private int GetBitWidthOfLiteralExp(ActionSequenceParser.LiteralexpContext exp)
    {
        var value = exp.GetText();
        return this.ConvertValueStringToInt(value);
    }

    private int GetBitWidthOfRamExp(ActionSequenceParser.RamexpContext exp)
    {
        return 8; // Always a byte
    }

    private int GetBitWidthOfExp(ActionSequenceParser.ExpContext exp)
    {
        if (exp.pinexp() != null)
        {
            return -1;
        }
        else if (exp.ramexp() != null)
        {
            return GetBitWidthOfRamExp(exp.ramexp());
        }
        else if (exp.literalexp() != null)
        {
            return GetBitWidthOfLiteralExp(exp.literalexp());
        }

        return -1;
    }

    public override object VisitAssignment([NotNull] ActionSequenceParser.AssignmentContext context)
    {
        if (context.pinexp() != null)
        {
            var pin = context.pinexp().PIN_ID().GetText();
            var bitWidth = GetBitWidthOfExp(context.exp());

            if (bitWidth != -1)
            {
                this.Pins.Add((pin, bitWidth));
            }
        }

        return this.VisitChildren(context);
    }

    public override object VisitBoolexp([NotNull] ActionSequenceParser.BoolexpContext context)
    {
        var widthLeft = GetBitWidthOfExp(context.exp(0));
        var widthRight = GetBitWidthOfExp(context.exp(1));

        if (widthLeft != widthRight && widthLeft != -1 && widthRight != -1)
        {
            throw new Exception($"line {context.SourceInterval} Widths of left and right side of expression do not match.");
        }

        return this.VisitChildren(context);
    }

    public override object VisitPush([NotNull] ActionSequenceParser.PushContext context)
    {
        var pin = context.PIN_ID().GetText();
        this.PushButtons.Add(pin);

        return base.VisitPush(context);
    }

    public override object VisitRamexp([NotNull] ActionSequenceParser.RamexpContext context)
    {
        var ram = context.PIN_ID().GetText();
        this.Rams.Add(ram);
        return base.VisitRamexp(context);
    }

    public override object VisitConnectKeyboard([NotNull] ActionSequenceParser.ConnectKeyboardContext context)
    {
        var keyboard = context.PIN_ID().GetText();
        this.Keyboards.Add(keyboard);

        return base.VisitConnectKeyboard(context);
    }

    public override object VisitConnectTTY([NotNull] ActionSequenceParser.ConnectTTYContext context)
    {
        var tty = context.PIN_ID().GetText();
        this.TTYs.Add(tty);

        return base.VisitConnectTTY(context);
    }

    public override object VisitMountDisk([NotNull] ActionSequenceParser.MountDiskContext context)
    {
        var disk = context.PIN_ID().GetText();
        this.Disks.Add(disk);
        return base.VisitMountDisk(context);
    }

    public List<(string, int)> GetPins()
    {
        return this.Pins.Distinct().ToList();
    }

    public List<string> GetPushButtons()
    {
        return this.PushButtons.Distinct().ToList();
    }

    public List<string> GetRams()
    {
        return this.Rams.Distinct().ToList();
    }

    public List<string> GetTTYs()
    {
        return this.TTYs.Distinct().ToList();
    }

    public List<string> GetKeyboards()
    {
        return this.Keyboards.Distinct().ToList();
    }

    public List<string> GetDisks()
    {
        return this.Disks.Distinct().ToList();
    }
}