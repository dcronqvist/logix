using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Architecture.Serialization;

namespace LogiX.Minimal.ActionSequencing;

public class ActionSequenceRunner : ActionSequenceBaseVisitor<object>
{
    public string Text { get; private set; }
    public Circuit Circuit { get; private set; }

    public Simulation Simulation { get; private set; }
    public Dictionary<string, Pin> Pins { get; private set; }
    public Dictionary<string, PushButton> PushButtons { get; private set; }

    private bool Terminate { get; set; } = false;

    public ActionSequenceRunner(Circuit circuit, string text)
    {
        this.Text = text;
        this.Circuit = circuit;
    }

    public void Run()
    {
        this.Simulation = Simulation.FromCircuit(this.Circuit);
        this.Pins = this.Simulation.GetComponentsOfType<Pin>().Where(p => ((PinData)p.GetDescriptionData()).Label != "").ToDictionary(p => ((PinData)p.GetDescriptionData()).Label, p => p);
        this.PushButtons = this.Simulation.GetComponentsOfType<PushButton>().Where(p => ((PushButtonData)p.GetDescriptionData()).Label != "").ToDictionary(p => ((PushButtonData)p.GetDescriptionData()).Label, p => p);

        var inputStream = new AntlrInputStream(this.Text);
        var lexer = new ActionSequencing.ActionSequenceLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new ActionSequencing.ActionSequenceParser(tokenStream);

        var tree = parser.program();

        this.Simulation.Tick();
        this.Visit(tree);

        if (!this.Terminate)
        {
            while (true)
            {
                this.Simulation.Tick();
            }
        }
    }

    private void PrintCurrentPins()
    {
        foreach (var pin in this.Pins)
        {
            Console.WriteLine($"{pin.Key}: 0b{pin.Value.CurrentValues.Select(v => v == LogicValue.UNDEFINED ? "X" : (v == LogicValue.HIGH ? "1" : "0")).Aggregate((a, b) => a + b)}");
        }
    }

    private (uint, int) GetValueStringAsUInt(string valueString)
    {
        if (valueString.StartsWith("0b"))
        {
            return (Convert.ToUInt32(valueString.Substring(2), 2), valueString.Length - 2);
        }
        else if (valueString.StartsWith("0x"))
        {
            return (Convert.ToUInt32(valueString.Substring(2), 16), (valueString.Length - 2) * 4);
        }
        else
        {
            return (Convert.ToUInt32(valueString), valueString.Length);
        }
    }

    private LogicValue[] EvaluateExp(ActionSequenceParser.ExpContext exp)
    {
        if (exp.pinexp() != null)
        {
            return this.EvaluateExp(exp.pinexp());
        }
        else if (exp.ramexp() != null)
        {
            return this.EvaluateExp(exp.ramexp());
        }
        else if (exp.literalexp() != null)
        {
            return this.EvaluateExp(exp.literalexp());
        }

        return null;
    }

    private LogicValue[] EvaluateExp(ActionSequenceParser.LiteralexpContext literal)
    {
        var text = literal.GetText();
        var (value, width) = GetValueStringAsUInt(text);
        return value.GetAsLogicValues(width);
    }

    private LogicValue[] EvaluateExp(ActionSequenceParser.RamexpContext ram)
    {
        var r = this.Simulation.GetComponentsOfType<RAM>().Where(r => ((RamData)r.GetDescriptionData()).Label == ram.PIN_ID().GetText()).First();

        if (ram.HEX_LITERAL() != null)
        {
            var (address, width) = GetValueStringAsUInt(ram.HEX_LITERAL().GetText());
            return ((RamData)r.GetDescriptionData()).Memory[address].GetAsLogicValues(8);
        }
        else
        {
            var (address, width) = GetValueStringAsUInt(ram.BINARY_LITERAL().GetText());
            return ((RamData)r.GetDescriptionData()).Memory[address].GetAsLogicValues(8);
        }
    }

    private LogicValue[] EvaluateExp(ActionSequenceParser.PinexpContext pin)
    {
        var pinID = pin.PIN_ID().GetText();
        var p = this.Pins[pinID];
        return p.CurrentValues;
    }

    public override object VisitAssignment([NotNull] ActionSequenceParser.AssignmentContext context)
    {
        if (context.pinexp() != null)
        {
            var pin = context.pinexp().PIN_ID().GetText();
            var value = EvaluateExp(context.exp());

            this.Pins[pin].CurrentValues = value;
            return this.VisitChildren(context);
        }
        else
        {
            var ram = context.ramexp().PIN_ID().GetText();
            var r = this.Simulation.GetComponentsOfType<RAM>().Where(r => ((RamData)r.GetDescriptionData()).Label == ram).First();

            if (context.ramexp().HEX_LITERAL() != null)
            {
                // address using hex literal
                var (address, width) = GetValueStringAsUInt(context.ramexp().HEX_LITERAL().GetText());
                var value = EvaluateExp(context.exp());

                ((RamData)r.GetDescriptionData()).Memory[address] = value.Reverse().GetAsByte();
            }
            else
            {
                // address using binary literal
                var (address, width) = GetValueStringAsUInt(context.ramexp().BINARY_LITERAL().GetText());
                var value = EvaluateExp(context.exp());

                ((RamData)r.GetDescriptionData()).Memory[address] = value.Reverse().GetAsByte();
            }

            return this.VisitChildren(context);
        }
    }

    public override object VisitEnd([NotNull] ActionSequenceParser.EndContext context)
    {
        this.Terminate = true;
        return base.VisitEnd(context);
    }

    public override object VisitContinue([NotNull] ActionSequenceParser.ContinueContext context)
    {
        this.Terminate = false;
        return base.VisitContinue(context);
    }

    public override object VisitWait([NotNull] ActionSequenceParser.WaitContext context)
    {
        if (context.DECIMAL_LITERAL() != null)
        {
            // We are going to wait for a specified number of ticks, and then continue.
            var ticks = Convert.ToInt32(context.DECIMAL_LITERAL().GetText());
            for (int i = 0; i < ticks; i++)
            {
                this.Simulation.Tick();
            }
        }
        else
        {
            // We are going to wait until a specified boolexp is true, and then continue.
            var boolexp = context.boolexp();

            while (!EvaluateBoolExp(boolexp))
            {
                this.Simulation.Tick();
            }
        }

        return base.VisitWait(context);
    }

    public override object VisitPush([NotNull] ActionSequenceParser.PushContext context)
    {
        var pin = context.PIN_ID().GetText();
        var pushButton = this.PushButtons[pin];
        pushButton._value = LogicValue.HIGH;

        if (context.DECIMAL_LITERAL() != null)
        {
            var literalText = context.DECIMAL_LITERAL().GetText();
            var (ticks, width) = GetValueStringAsUInt(literalText);
            for (int i = 0; i < ticks; i++)
            {
                this.Simulation.Tick();
            }
        }
        else
        {
            var boolexp = context.boolexp();
            while (!EvaluateBoolExp(boolexp))
            {
                this.Simulation.Tick();
            }
        }
        pushButton._value = LogicValue.LOW;
        return base.VisitPush(context);
    }

    private bool EvaluateBoolExp(ActionSequenceParser.BoolexpContext context)
    {
        if (context.exp(0) != null)
        {
            // We can evaluate
            var left = EvaluateExp(context.exp(0));
            var right = EvaluateExp(context.exp(1));

            if (context.GetText().Contains("=="))
            {
                // Equals
                return left.SequenceEqual(right);
            }
            else
            {
                // Not equals
                return !left.SequenceEqual(right);
            }
        }
        else
        {
            var left = EvaluateBoolExp(context.boolexp(0));
            var right = EvaluateBoolExp(context.boolexp(1));

            if (context.GetText().Contains("&&"))
            {
                // And
                return left && right;
            }
            else
            {
                // Or
                return left || right;
            }
        }
    }

    public override object VisitPrint([NotNull] ActionSequenceParser.PrintContext context)
    {
        var text = context.GetText().Substring(7, context.GetText().Length - 8);

        var sb = new StringBuilder();

        var i = 0;
        while (i < text.Length)
        {
            if (text[i] == '$')
            {
                i++;
                var mode = text[i + 1];
                i += 2;
                var pin = text.Substring(i + 1, text.IndexOf('}', i) - i - 1);

                var values = this.Pins[pin].CurrentValues;

                var formatted = mode switch
                {
                    'x' => "0x" + values.Reverse().GetAsHexString(),
                    'b' => "0b" + values.Select(v => v == LogicValue.UNDEFINED ? "X" : (v == LogicValue.HIGH ? "1" : "0")).Aggregate((a, b) => a + b),
                    'd' => values.Reverse().GetAsUInt().ToString(),
                    _ => throw new Exception("Invalid format mode"),
                };

                sb.Append(formatted);
                i = text.IndexOf('}', i) + 1;
            }
            else
            {
                sb.Append(text[i]);
                i++;
            }
        }

        Console.WriteLine(sb.ToString());
        return base.VisitPrint(context);
    }
}