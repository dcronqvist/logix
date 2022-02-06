using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using LogiX.SaveSystem;
using LogiX.Components;
using System.Numerics;
using LogiX;

namespace LogiX.GateAlgebra;

public class VisitorCreateCircuit : IGateAlgebraVisitor<CircuitDescription?>
{
    List<Component> components = new List<Component>();
    Component currentFrom;
    Component currentTo;
    Dictionary<string, Switch> switches = new Dictionary<string, Switch>();

    public VisitorCreateCircuit()
    {

    }

    public CircuitDescription? Visit(IParseTree tree)
    {
        tree.Accept(this);
        return new CircuitDescription(components);
    }

    public CircuitDescription? VisitAssignment([NotNull] GateAlgebraParser.AssignmentContext context)
    {
        string name = context.variable().GetText();
        //Console.WriteLine($"Ass: Creating new output {name}");


        Lamp l = new Lamp(1, Vector2.Zero, name);
        currentTo = l;
        context.primary().Accept(this);

        Console.WriteLine($"Ass: Connecting wire from {currentFrom.Text} to {l.ID}");
        Wire w = new Wire(1, l, 0, currentFrom, 0);
        l.SetInputWire(0, w);
        currentFrom.AddOutputWire(0, w);

        if (!components.Contains(l))
        {
            components.Add(l);
        }
        return null;
    }

    public CircuitDescription? VisitChildren(IRuleNode node)
    {
        throw new NotImplementedException();
    }

    public CircuitDescription? VisitComponent([NotNull] GateAlgebraParser.ComponentContext context)
    {
        foreach (GateAlgebraParser.AssignmentContext ac in context.assignment())
        {
            ac.Accept(this);
        }
        return null;
    }

    public CircuitDescription? VisitErrorNode(IErrorNode node)
    {
        throw new NotImplementedException();
    }

    public CircuitDescription? VisitExpression([NotNull] GateAlgebraParser.ExpressionContext context)
    {
        if (context.variable() != null)
        {
            context.variable().Accept(this);
        }
        else if (context.primary() != null)
        {
            context.primary().Accept(this);
        }
        else
        {
            context.expression().Accept(this);
        }
        return null;
    }

    public CircuitDescription? VisitGate([NotNull] GateAlgebraParser.GateContext context)
    {
        currentTo = new LogicGate(2, false, Util.GetGateLogicFromName(context.Start.Text.ToUpper()), Vector2.Zero);
        //Console.WriteLine($"Gate: Created new {currentTo.Text} gate");
        return null;
    }

    public CircuitDescription? VisitPrimary([NotNull] GateAlgebraParser.PrimaryContext context)
    {
        context.expression(0).Accept(this);

        if (context.gate().Length > 0)
        {
            for (int i = 0; i < context.gate().Length; i++)
            {
                context.gate(i).Accept(this);
                Component previousTo = currentTo;

                Wire w1 = new Wire(1, currentTo, 0, currentFrom, 0);

                if (!components.Contains(currentTo))
                    components.Add(currentTo);
                if (!components.Contains(currentFrom))
                    components.Add(currentFrom);

                currentTo.SetInputWire(0, w1);
                currentFrom.AddOutputWire(0, w1);

                //Console.WriteLine($"Prim1: Connecting wire from {currentFrom.Text} gate to {currentTo.Text}");

                context.expression(i + 1).Accept(this);

                Wire w2 = new Wire(1, previousTo, 1, currentFrom, 0);
                previousTo.SetInputWire(1, w2);
                currentFrom.AddOutputWire(0, w2);

                //Console.WriteLine($"Prim2: Connecting wire from {currentFrom.Text} gate to {previousTo.Text}");

                if (!components.Contains(currentFrom))
                {
                    components.Add(currentFrom);
                }

                currentFrom = previousTo;
            }
        }
        else
        {
            if (!components.Contains(currentFrom))
            {
                components.Add(currentFrom);
            }

            if (!components.Contains(currentTo))
            {
                components.Add(currentTo);
            }

            Wire w3 = new Wire(0, currentTo, 0, currentFrom, 0);
            currentTo.SetInputWire(0, w3);
            currentFrom.AddOutputWire(0, w3);
        }

        return null;
    }

    public CircuitDescription? VisitTerminal(ITerminalNode node)
    {
        throw new NotImplementedException();
    }

    public CircuitDescription? VisitVariable([NotNull] GateAlgebraParser.VariableContext context)
    {
        if (!switches.ContainsKey(context.Start.Text))
        {
            switches.Add(context.Start.Text, new Switch(1, Vector2.Zero, context.Start.Text));
        }
        currentFrom = switches[context.Start.Text];
        //Console.WriteLine($"Var: Current switch is now: {context.Start.Text}");
        return null;
    }
}