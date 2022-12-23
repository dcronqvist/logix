using System.CommandLine;
using System.Diagnostics;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Architecture.Plugins;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Minimal.ActionSequencing;

public class ActionSequenceRunner : ActionSequenceBaseVisitor<object>
{
    public string Text { get; private set; }
    public Circuit Circuit { get; private set; }
    public string PathToActionSequenceFile { get; private set; }

    public Simulation Simulation { get; private set; }
    public Dictionary<string, Pin> Pins { get; private set; }
    public Dictionary<string, PushButton> PushButtons { get; private set; }

    private Keyboard CurrentKeyboard { get; set; }
    private TTY CurrentTTY { get; set; }
    private LEDMatrix CurrentLEDMatrix { get; set; }
    private int LedMatrixScale { get; set; }

    private bool Terminate { get; set; } = false;

    public ActionSequenceRunner(Circuit circuit, string text, string pathToActionSequenceFile)
    {
        this.Text = text;
        this.Circuit = circuit;
        this.PathToActionSequenceFile = pathToActionSequenceFile;
    }

    private Dictionary<string, RootCommand> _extensions = new();
    private void InitExtensions()
    {
        var extensions = ScriptManager.GetScriptTypes().Where(x => x.Type.IsAssignableTo(typeof(IActionSequenceExtension))).Select(x => (x.Identifier, (IActionSequenceExtension)Activator.CreateInstance(x.Type))).ToList();

        foreach (var (id, ins) in extensions)
        {
            var command = ins.GetCommand(this.Simulation);
            this._extensions.Add(id, command);
        }
    }

    private TextWriter _output;
    private List<PushButton> _depush = new();
    public void Run(TextWriter output)
    {
        this._output = output;
        var stopwatch = new Stopwatch();

        this.Simulation = Simulation.FromCircuit(this.Circuit);
        this.Pins = this.Simulation.GetNodesOfType<Pin>().Where(p => ((PinData)p.GetNodeData()).Label != "").ToDictionary(p => ((PinData)p.GetNodeData()).Label, p => p);
        this.PushButtons = this.Simulation.GetNodesOfType<PushButton>().Where(p => ((PushButtonData)p.GetNodeData()).Label != "").ToDictionary(p => ((PushButtonData)p.GetNodeData()).Label, p => p);

        this.InitExtensions();

        var inputStream = new AntlrInputStream(this.Text);
        var lexer = new ActionSequencing.ActionSequenceLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new ActionSequencing.ActionSequenceParser(tokenStream);

        var tree = parser.program();

        _ = Task.Run(() =>
        {
            while (true)
            {
                var key = Console.ReadKey(true);
                if (this.CurrentKeyboard is not null)
                {
                    if (key.KeyChar == 0x0D)
                    {
                        // Instead of carriage return, send line feed
                        this.CurrentKeyboard.RegisterChar((char)0x0A);
                    }
                    else
                    {
                        this.CurrentKeyboard.RegisterChar(key.KeyChar);
                    }
                }

                var pushButtons = this.Simulation.GetNodesOfType<PushButton>();

                foreach (var pb in pushButtons)
                {
                    var pbData = pb.GetNodeData() as PushButtonData;
                    if (pbData.Hotkey == key.Key.GetAsKey())
                    {
                        pb._hotkeyDown = true;
                        pb.TriggerEvaluationNextTick();
                        _depush.Add(pb);
                    }
                }
            }
        });

        stopwatch.Start();
        this.Visit(tree);

        if (!this.Terminate)
        {
            if (this.CurrentLEDMatrix is not null)
            {
                var data = this.CurrentLEDMatrix.GetNodeData() as LEDMatrixData;
                var scale = this.LedMatrixScale;
                var width = data.Columns * scale;
                var height = data.Rows * scale;
                var ledmatrixWindow = new LEDMatrixWindow(this.LedMatrixScale, this.Simulation, this.CurrentLEDMatrix, this.CurrentKeyboard);

                ledmatrixWindow.Run($"LogiX - {data.Label}", new string[] { }, width, height);
            }
            else
            {
                while (true)
                {
                    this.Simulation.Step();

                    while (_depush.Count > 0)
                    {
                        var pb = _depush[0];
                        _depush.RemoveAt(0);
                        pb._hotkeyDown = false;
                        pb.TriggerEvaluationNextTick();
                    }
                }
            }
        }

        stopwatch.Stop();
        _output.WriteLine($"------------- Execution finished -------------");
        _output.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds} ms");
        _output.WriteLine($"Total ticks: {this.Simulation.TicksSinceStart}");
        _output.WriteLine($"Avg. ticks / second: {((int)(this.Simulation.TicksSinceStart / stopwatch.Elapsed.TotalSeconds)).GetAsHertzString()}");
    }

    private void Step()
    {

    }

    private void PrintCurrentPins()
    {
        foreach (var pin in this.Pins)
        {
            this._output.WriteLine($"{pin.Key}: 0b{(pin.Value.GetNodeData() as PinData).Values.Select(v => v == LogicValue.Z ? "Z" : (v == LogicValue.HIGH ? "1" : "0")).Aggregate((a, b) => a + b)}");
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
        var r = this.Simulation.GetNodesOfType<RAM>().Where(r => ((RamData)r.GetNodeData()).Label == ram.PIN_ID().GetText()).First();

        if (ram.HEX_LITERAL() != null)
        {
            var (address, width) = GetValueStringAsUInt(ram.HEX_LITERAL().GetText());
            return ((RamData)r.GetNodeData()).Memory.GetBytes(address, 1).GetAsLogicValues(8);
        }
        else
        {
            var (address, width) = GetValueStringAsUInt(ram.BINARY_LITERAL().GetText());
            return ((RamData)r.GetNodeData()).Memory.GetBytes(address, 1).GetAsLogicValues(8);
        }
    }

    private LogicValue[] EvaluateExp(ActionSequenceParser.PinexpContext pin)
    {
        var pinID = pin.PIN_ID().GetText();
        var p = this.Pins[pinID];
        return p.GetValues();
    }

    private void ExecuteExtension(string extString)
    {
        var extName = extString.Split(' ').First();
        var ext = this._extensions[extName];

        ext.Invoke(extString.Substring(extName.Length + 1));
    }

    public override object VisitAssignment([NotNull] ActionSequenceParser.AssignmentContext context)
    {
        if (context.pinexp() != null)
        {
            var pin = context.pinexp().PIN_ID().GetText();
            var value = EvaluateExp(context.exp());

            this.Pins[pin].SetValues(value);
            return this.VisitChildren(context);
        }
        else
        {
            var ram = context.ramexp().PIN_ID().GetText();
            var r = this.Simulation.GetNodesOfType<RAM>().Where(r => ((RamData)r.GetNodeData()).Label == ram).First();

            if (context.ramexp().HEX_LITERAL() != null)
            {
                // address using hex literal
                var (address, width) = GetValueStringAsUInt(context.ramexp().HEX_LITERAL().GetText());
                var value = EvaluateExp(context.exp());

                ((RamData)r.GetNodeData()).Memory.SetBytes(address, Utilities.Arrayify(value.Reverse().GetAsByte()));
                r.TriggerEvaluationNextTick();
            }
            else
            {
                // address using binary literal
                var (address, width) = GetValueStringAsUInt(context.ramexp().BINARY_LITERAL().GetText());
                var value = EvaluateExp(context.exp());

                ((RamData)r.GetNodeData()).Memory.SetBytes(address, Utilities.Arrayify(value.Reverse().GetAsByte()));
                r.TriggerEvaluationNextTick();
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
                this.Simulation.Step();
            }
        }
        else
        {
            // We are going to wait until a specified boolexp is true, and then continue.
            var boolexp = context.boolexp();

            while (!EvaluateBoolExp(boolexp))
            {
                this.Simulation.Step();
            }
        }

        return base.VisitWait(context);
    }

    public override object VisitPush([NotNull] ActionSequenceParser.PushContext context)
    {
        var pin = context.PIN_ID().GetText();
        var pushButton = this.PushButtons[pin];
        pushButton._hotkeyDown = true;
        pushButton.TriggerEvaluationNextTick();

        if (context.DECIMAL_LITERAL() != null)
        {
            var literalText = context.DECIMAL_LITERAL().GetText();
            var (ticks, width) = GetValueStringAsUInt(literalText);
            for (int i = 0; i < ticks; i++)
            {
                this.Simulation.Step();
            }
        }
        else
        {
            var boolexp = context.boolexp();
            while (!EvaluateBoolExp(boolexp))
            {
                this.Simulation.Step();
            }
        }
        pushButton._hotkeyDown = false;
        pushButton.TriggerEvaluationNextTick();
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

                var values = this.Pins[pin].GetValues();

                var formatted = mode switch
                {
                    'x' => "0x" + values.Reverse().GetAsHexString(),
                    'b' => "0b" + values.Select(v => v == LogicValue.Z ? "Z" : (v == LogicValue.HIGH ? "1" : "0")).Aggregate((a, b) => a + b),
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

        this._output.WriteLine(sb.ToString());
        return base.VisitPrint(context);
    }

    public override object VisitConnectKeyboard([NotNull] ActionSequenceParser.ConnectKeyboardContext context)
    {
        var pin = context.PIN_ID().GetText();
        var keyboard = this.Simulation.GetNodesOfType<Keyboard>().Where(k => ((KeyboardData)k.GetNodeData()).Label == pin).First();

        this.CurrentKeyboard = keyboard;
        this._output.WriteLine("Connected to keyboard " + pin);

        return base.VisitConnectKeyboard(context);
    }

    public override object VisitConnectTTY([NotNull] ActionSequenceParser.ConnectTTYContext context)
    {
        var pin = context.PIN_ID().GetText();
        var tty = this.Simulation.GetNodesOfType<TTY>().Where(k => ((TTYData)k.GetNodeData()).Label == pin).First();

        this.CurrentTTY = tty;

        this.CurrentTTY.OnCharReceived += (sender, c) =>
        {
            if (c == '\f')
            {
                Console.Clear();
            }
            else if (c == '\b')
            {
                Console.SetCursorPosition(Math.Max(0, Console.CursorLeft - 1), Console.CursorTop);
                Console.Write(' ');
                Console.SetCursorPosition(Math.Max(0, Console.CursorLeft - 1), Console.CursorTop);
            }
            else
            {
                Console.Write(c);
            }
        };

        this._output.WriteLine("Connected to TTY " + pin);
        return base.VisitConnectTTY(context);
    }

    public override object VisitMountDisk([NotNull] ActionSequenceParser.MountDiskContext context)
    {
        // var pin = context.PIN_ID().GetText();
        // var disk = this.Simulation.GetComponentsOfType<Disk>().Where(k => ((DiskData)k.GetDescriptionData()).Label == pin).First();
        // var filePath = context.STRING_LITERAL().GetText().Substring(1, context.STRING_LITERAL().GetText().Length - 2);

        // var path = Path.Combine(this.PathToActionSequenceFile, filePath);

        // if (!disk.TryMountFile(path))
        // {
        //     throw new Exception("Could not mount disk");
        // }
        throw new NotImplementedException("Disk not implemented yet");
        //return base.VisitMountDisk(context);
    }

    public override object VisitConnectLEDMatrix([NotNull] ActionSequenceParser.ConnectLEDMatrixContext context)
    {
        var pin = context.PIN_ID().GetText();
        var ledMatrix = this.Simulation.GetNodesOfType<LEDMatrix>().Where(k => ((LEDMatrixData)k.GetNodeData()).Label == pin).First();

        this.CurrentLEDMatrix = ledMatrix;
        this.LedMatrixScale = int.Parse(context.DECIMAL_LITERAL().GetText());

        this._output.WriteLine("Connected to LED matrix " + pin);
        return base.VisitConnectLEDMatrix(context);
    }

    public override object VisitExt([NotNull] ActionSequenceParser.ExtContext context)
    {
        var extString = context.STRING_LITERAL().GetText();
        extString = extString.Substring(1, extString.Length - 2);

        this.ExecuteExtension(extString);

        return base.VisitExt(context);
    }
}