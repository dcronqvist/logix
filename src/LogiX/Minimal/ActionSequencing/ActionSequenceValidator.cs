using Antlr4.Runtime;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Architecture.Serialization;

namespace LogiX.Minimal.ActionSequencing;

public class ActionSequenceValidator
{
    public string Text { get; private set; }
    public Circuit Circuit { get; private set; }

    public ActionSequenceValidator(Circuit circuit, string text)
    {
        this.Text = text;
        this.Circuit = circuit;
    }

    private class ValidationErrorListener : BaseErrorListener
    {
        public List<string> SyntaxErrors { get; private set; } = new();

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            this.SyntaxErrors.Add($"line {line}:{charPositionInLine} {msg}");
            base.SyntaxError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e);
        }
    }

    public bool TryValidatePins(out string[] errors)
    {
        try
        {
            var inputStream = new AntlrInputStream(this.Text);
            var lexer = new ActionSequencing.ActionSequenceLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new ActionSequencing.ActionSequenceParser(tokenStream);
            var errorListener = new ValidationErrorListener();

            parser.RemoveErrorListeners();
            parser.AddErrorListener(errorListener);

            var tree = parser.program();
            var visitor = new PinChecker();
            visitor.Visit(tree);

            var foundPins = visitor.GetPins();
            var ers = new List<string>();

            var pinsInCircuit = this.Circuit.GetAllComponentsOfType("logix_builtin.script_type.PIN").Select(cd => cd.Data as PinData).Select(pd => (pd.Label, pd.Bits));

            foreach (var foundPin in foundPins)
            {
                if (!pinsInCircuit.Contains(foundPin))
                {
                    ers.Add($"Pin {foundPin.Item1}, with width {foundPin.Item2} not found in circuit.");
                }
            }

            var foundPushButtons = visitor.GetPushButtons();

            var pushButtonsInCircuit = this.Circuit.GetAllComponentsOfType("logix_builtin.script_type.PUSHBUTTON").Select(cd => cd.Data as PushButtonData).Select(pd => (pd.Label));

            foreach (var foundPushButton in foundPushButtons)
            {
                if (!pushButtonsInCircuit.Contains(foundPushButton))
                {
                    ers.Add($"Push button {foundPushButton} not found in circuit.");
                }
            }

            var foundRams = visitor.GetRams();
            var ramsInCircuit = this.Circuit.GetAllComponentsOfType("logix_builtin.script_type.RAM").Select(cd => cd.Data as RamData).Select(pd => pd.Label);

            foreach (var foundRam in foundRams)
            {
                if (!ramsInCircuit.Contains(foundRam))
                {
                    ers.Add($"Ram {foundRam} not found in circuit.");
                }
            }

            var foundKeyboards = visitor.GetKeyboards();
            var keyboardsInCircuit = this.Circuit.GetAllComponentsOfType("logix_builtin.script_type.KEYBOARD").Select(cd => cd.Data as KeyboardData).Select(pd => pd.Label);

            foreach (var foundKeyboard in foundKeyboards)
            {
                if (!keyboardsInCircuit.Contains(foundKeyboard))
                {
                    ers.Add($"Keyboard {foundKeyboard} not found in circuit.");
                }
            }

            var foundTTYs = visitor.GetTTYs();
            var ttysInCircuit = this.Circuit.GetAllComponentsOfType("logix_builtin.script_type.TTY").Select(cd => cd.Data as TTYData).Select(pd => pd.Label);

            foreach (var foundTTY in foundTTYs)
            {
                if (!ttysInCircuit.Contains(foundTTY))
                {
                    ers.Add($"TTY {foundTTY} not found in circuit.");
                }
            }

            var foundDisks = visitor.GetDisks();
            var disksInCircuit = this.Circuit.GetAllComponentsOfType("logix_builtin.script_type.DISK").Select(cd => cd.Data as DiskData).Select(pd => pd.Label);

            foreach (var foundDisk in foundDisks)
            {
                if (!disksInCircuit.Contains(foundDisk))
                {
                    ers.Add($"Disk {foundDisk} not found in circuit.");
                }
            }

            var finalChecker = new FinalChecker();
            finalChecker.Visit(tree);
            var hasFinalEndOrContinue = finalChecker.HasFinalEndOrContinue;

            if (!hasFinalEndOrContinue)
            {
                ers.Add("No final end or continue statement found.");
            }

            ers.AddRange(errorListener.SyntaxErrors);

            errors = ers.ToArray();
            return ers.Count == 0;
        }
        catch (Exception e)
        {
            errors = new string[] { e.Message };
            return false;
        }
    }
}