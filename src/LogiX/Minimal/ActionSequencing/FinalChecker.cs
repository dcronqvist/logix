using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using LogiX.Architecture.Serialization;

namespace LogiX.Minimal.ActionSequencing;

public class FinalChecker : ActionSequenceBaseVisitor<object>
{
    public bool HasFinalEndOrContinue { get; private set; } = false;

    public override object VisitEnd([NotNull] ActionSequenceParser.EndContext context)
    {
        var x = this.VisitChildren(context);
        this.HasFinalEndOrContinue = true;
        return x;
    }

    public override object VisitContinue([NotNull] ActionSequenceParser.ContinueContext context)
    {
        var x = this.VisitChildren(context);
        this.HasFinalEndOrContinue = true;
        return x;
    }

    public override object VisitChildren(IRuleNode node)
    {
        HasFinalEndOrContinue = false;
        return base.VisitChildren(node);
    }
}