using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using LogiX.SaveSystem;
using LogiX.Components;
using System.Numerics;
using LogiX;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;

namespace LogiX.GateAlgebra;

public class VisitorCreateCircuit : IGateAlgebraVisitor<CircuitDescription?>
{
    List<Component> components = new List<Component>();
    LogicGate currentGate;
    Component currentComponent;
    Dictionary<string, Switch> switches = new Dictionary<string, Switch>();

    bool debug = false;

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
        if (debug)
            Console.WriteLine($"Ass: Creating new output {name}");


        context.primary().Accept(this);

        Lamp l = new Lamp(1, Vector2.Zero, name);
        Console.WriteLine($"Ass: Connecting wire from {currentComponent.Text} to {l.ID}");
        Wire w = new Wire(1, l, 0, currentComponent, 0);
        l.SetInputWire(0, w);
        currentGate.AddOutputWire(0, w);

        components.Add(l);
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
        currentGate = new LogicGate(2, false, Util.GetGateLogicFromName(context.Start.Text.ToUpper()), Vector2.Zero);
        if (debug)
            Console.WriteLine($"Gate: Created new {currentGate.Text} gate");
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
                Component prevCom = currentGate;


                Wire w1 = new Wire(1, currentGate, 0, currentComponent, 0);

                if (!components.Contains(currentGate))
                    components.Add(currentGate);

                currentGate.SetInputWire(0, w1);
                currentComponent.AddOutputWire(0, w1);
                if (debug)
                    Console.WriteLine($"Prim1: Connecting wire from {currentComponent.Text} gate to {currentGate.Text}");

                if (!components.Contains(currentComponent))
                {
                    components.Add(currentComponent);
                }

                context.expression(i + 1).Accept(this);

                Wire w2 = new Wire(1, prevCom, 1, currentComponent, 0);
                prevCom.SetInputWire(1, w2);
                currentComponent.AddOutputWire(0, w2);
                if (debug)
                    Console.WriteLine($"Prim2: Connecting wire from {currentComponent.Text} gate to {prevCom.Text}");

                if (!components.Contains(currentComponent))
                {
                    components.Add(currentComponent);
                }

                currentComponent = prevCom;
            }
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
        currentComponent = switches[context.Start.Text];
        if (debug)
            Console.WriteLine($"Var: Current switch is now: {context.Start.Text}");
        return null;
    }
}