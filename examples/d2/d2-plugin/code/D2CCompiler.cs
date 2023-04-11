using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace D2Plugin;

public class Preprocessor : D2CBaseVisitor<object>
{
    public string Source { get; set; }

    public Preprocessor(string source)
    {
        Source = source;
    }

    public override object VisitProgram([NotNull] D2CParser.ProgramContext context)
    {
        var x = base.VisitProgram(context);
        var lines = this.Source.Split('\n');
        this.Source = lines.Where(l => !l.StartsWith("#")).Aggregate((a, b) => a + "\n" + b);
        return x;
    }

    public override object VisitPreprodirs([NotNull] D2CParser.PreprodirsContext context)
    {
        if (context.DEFINE() is not null)
        {
            this.PreproDefine(context);
        }

        return base.VisitPreprodirs(context);
    }

    public void PreproDefine(D2CParser.PreprodirsContext context)
    {
        var name = context.ID().GetText();
        var value = context.rvalue().GetText();
        this.Source = this.Source.Replace(name, value);
    }
}

public class D2CType
{
    public bool IsPointer { get; set; }
    public string Type { get; set; }
    public int Bytes => this.IsPointer ? 2 : this.Type switch
    {
        "short" => 2,
        "char" => 1,
        _ => throw new Exception("Unknown type: " + this.Type)
    };

    public static readonly D2CType Void = new D2CType(false, "void");
    public static readonly D2CType Short = new D2CType(false, "short");
    public static readonly D2CType Char = new D2CType(false, "char");
    public static readonly D2CType VoidP = new D2CType(true, "void");
    public static readonly D2CType ShortP = new D2CType(true, "short");
    public static readonly D2CType CharP = new D2CType(true, "char");

    public D2CType(bool isPointer, string type)
    {
        this.IsPointer = isPointer;
        this.Type = type;
    }

    public bool Is(bool pointer, string type)
    {
        return this.IsPointer == pointer && this.Type == type;
    }

    public bool Matches(D2CType other)
    {
        return this.IsPointer == other.IsPointer && this.Type == other.Type;
    }

    public override string ToString()
    {
        return this.Type + (this.IsPointer ? "*" : "");
    }
}

public class Variable
{
    public string Identifier { get; set; }
    public D2CType Type { get; set; }

    public Variable(string identifier, D2CType type)
    {
        this.Identifier = identifier;
        this.Type = type;
    }
}

public abstract class FuncDef
{
    public string Identifier { get; set; }
    public D2CType ReturnType { get; set; }
    public List<Variable> Parameters { get; set; }

    public FuncDef(string identifier, D2CType returnType, List<Variable> parameters)
    {
        this.Identifier = identifier;
        this.ReturnType = returnType;
        this.Parameters = parameters;
    }

    public abstract void GetASM(D2AssemblerCompiler compiler);
}

public class ASTFuncDef : FuncDef
{
    public D2CParser.FuncdefContext Context { get; set; }

    public ASTFuncDef(string identifier, D2CType returnType, List<Variable> parameters, D2CParser.FuncdefContext context) : base(identifier, returnType, parameters)
    {
        this.Context = context;
    }

    public override void GetASM(D2AssemblerCompiler compiler)
    {
        var id = Context.ID().GetText();
        compiler.CurrentFuncDef = compiler.CurrentContext.GetFuncDef(id);

        var funcLabel = $"{id}_{compiler.CurrentContext.GetNextLabel()}";
        compiler.FuncLabels.Add(compiler.CurrentFuncDef, funcLabel);

        var l = new LocalsCollector();
        l.VisitFuncdef(Context);
        var locals = l.Locals;

        compiler.CurrentContext = compiler.CurrentContext.CreateChildContext();

        foreach (var arg in compiler.CurrentFuncDef.Parameters.Concat(locals))
        {
            compiler.CurrentContext.AddVariable(arg);
            var label = $"{id}_{arg.Identifier}_{compiler.CurrentContext.GetNextLabel()}";
            compiler.VariableLabels.Add(arg, label);
            compiler.EmitLine($"{label}:");
            compiler.EmitLine($"  .word $0000");
        }

        compiler.EmitLine($"{funcLabel}:");

        // Emit frame prologue to push frame
        compiler.EmitLine($"  pha");
        compiler.EmitLine($"  lda {compiler.valueTemp}");
        compiler.EmitLine($"  pha");
        compiler.EmitLine($"  lda {compiler.valueTemp}+1");
        compiler.EmitLine($"  pha");
        compiler.EmitLine($"  lda {compiler.binopTemp}");
        compiler.EmitLine($"  pha");
        compiler.EmitLine($"  lda {compiler.binopTemp}+1");
        compiler.EmitLine($"  pha");

        compiler.EmitLine($"  phx");
        compiler.EmitLine($"  phy");
        compiler.EmitLine($"  php");

        compiler.VisitBlock(Context.block());

        compiler.EmitLine($"  plp");
        compiler.EmitLine($"  ply");
        compiler.EmitLine($"  plx");

        compiler.EmitLine($"  pla");
        compiler.EmitLine($"  sta {compiler.binopTemp}+1");
        compiler.EmitLine($"  pla");
        compiler.EmitLine($"  sta {compiler.binopTemp}");
        compiler.EmitLine($"  pla");
        compiler.EmitLine($"  sta {compiler.valueTemp}+1");
        compiler.EmitLine($"  pla");
        compiler.EmitLine($"  sta {compiler.valueTemp}");
        compiler.EmitLine($"  pla");

        if (compiler.CurrentFuncDef.ReturnType.Matches(D2CType.Void))
        {
            if (compiler.CurrentFuncDef.Identifier == "irq")
            {
                compiler.EmitLine($"  rti");
            }
            else
            {
                compiler.EmitLine($"  rts"); // Return from void function, other functions will return from their own return statements
            }
        }

        compiler.CurrentContext = compiler.CurrentContext.Pop();
    }
}

public class BuiltinFuncDef : FuncDef
{
    public string ASM { get; set; }

    public BuiltinFuncDef(string identifier, D2CType returnType, List<Variable> parameters, string asm) : base(identifier, returnType, parameters)
    {
        this.ASM = asm;
    }

    public override void GetASM(D2AssemblerCompiler compiler)
    {
        var lines = this.ASM.Split('\n');
        foreach (var line in lines)
        {
            compiler.EmitLine(line);
        }
    }
}

public class FuncCollector : D2CBaseVisitor<List<FuncDef>>
{
    private D2CType GetD2CType(D2CParser.TypeContext context)
    {
        if (context.pointertype() is not null)
        {
            return new D2CType(true, context.pointertype().puretype().ID().GetText());
        }
        else
        {
            return new D2CType(false, context.puretype().ID().GetText());
        }
    }

    private List<Variable> GetParameterList(D2CParser.ParamlistContext context)
    {
        var parameters = new List<Variable>();
        foreach (var param in context._decls)
        {
            var type = GetD2CType(param.type());
            var id = param.ID().GetText();
            parameters.Add(new Variable(id, type));
        }
        return parameters;
    }

    public override List<FuncDef> VisitProgram([NotNull] D2CParser.ProgramContext context)
    {
        var funcDefs = new List<FuncDef>();
        foreach (var funcDef in context.funcdef())
        {
            funcDefs.AddRange(this.VisitFuncdef(funcDef));
        }
        return funcDefs;
    }

    public override List<FuncDef> VisitFuncdef([NotNull] D2CParser.FuncdefContext context)
    {
        var returnType = GetD2CType(context.type());
        var id = context.ID().GetText();
        var paramList = GetParameterList(context.paramlist());
        var funcDef = new ASTFuncDef(id, returnType, paramList, context);

        return new List<FuncDef>() { funcDef };
    }
}

public class Context
{
    public List<Variable> Variables { get; set; }
    public Dictionary<string, FuncDef> FuncDefs { get; set; }
    public Context Parent { get; set; }

    public Context(Context parent)
    {
        this.Variables = new List<Variable>();
        this.FuncDefs = new Dictionary<string, FuncDef>();
        this.Parent = parent;
    }

    public int LabelCounter { get; set; } = 0;
    public string GetNextLabel()
    {
        return $"L{this.LabelCounter++}";
    }

    public void AddVariable(Variable variable)
    {
        this.Variables.Add(variable);
    }

    public Variable GetVariable(string identifier)
    {
        var variable = this.Variables.FirstOrDefault(x => x.Identifier == identifier);
        if (variable is not null)
        {
            return variable;
        }
        else if (this.Parent is not null)
        {
            return this.Parent.GetVariable(identifier);
        }
        else
        {
            throw new Exception($"Variable {identifier} not found");
        }
    }

    public void AddFuncDef(FuncDef funcDef)
    {
        this.FuncDefs.Add(funcDef.Identifier, funcDef);
    }

    public FuncDef GetFuncDef(string identifier)
    {
        var funcDef = this.FuncDefs.Values.FirstOrDefault(x => x.Identifier == identifier);
        if (funcDef is not null)
        {
            return funcDef;
        }
        else if (this.Parent is not null)
        {
            return this.Parent.GetFuncDef(identifier);
        }
        else
        {
            throw new Exception($"Function {identifier} not found");
        }
    }

    public Context CreateChildContext()
    {
        return new Context(this);
    }

    public Context Pop()
    {
        return this.Parent;
    }
}

public class TypeChecker : D2CBaseVisitor<object>
{
    public Context CurrentContext { get; set; }
    public FuncDef CurrentFuncDef { get; set; }
    bool FoundReturn { get; set; }

    public Dictionary<string, Func<D2CType, D2CType, D2CType>> BinaryOperators { get; set; }

    public TypeChecker(Context initialContext)
    {
        this.CurrentContext = initialContext;
        this.FoundReturn = false;
        this.BinaryOperators = new() {
            { "+", (a, b) => a},
            { "-", (a, b) => a},
            { "*", (a, b) => a},
            { "/", (a, b) => a},
            { "%", (a, b) => a},
            { "==", (a, b) => D2CType.Char},
            { "!=", (a, b) => D2CType.Char},
            { "<", (a, b) => D2CType.Char},
            { ">", (a, b) => D2CType.Char},
            { "<=", (a, b) => D2CType.Char},
            { ">=", (a, b) => D2CType.Char},
            { "&&", (a, b) => D2CType.Char},
            { "||", (a, b) => D2CType.Char},
            { "|", (a, b) => a},
            { "&", (a, b) => a},
            { "^", (a, b) => a},
        };
    }

    private D2CType GetD2CType(D2CParser.TypeContext context)
    {
        if (context.pointertype() is not null)
        {
            return new D2CType(true, context.pointertype().puretype().ID().GetText());
        }
        else
        {
            return new D2CType(false, context.puretype().ID().GetText());
        }
    }

    public D2CType InferType(Context context, D2CParser.RvalueContext rvalue)
    {
        if (rvalue.ID() is not null)
        {
            return context.GetVariable(rvalue.ID().GetText()).Type;
        }
        else if (rvalue.DECIMALLIT() is not null)
        {
            return D2CType.Short;
        }
        else if (rvalue.HEXADECILIT() is not null)
        {
            return D2CType.Short;
        }
        else if (rvalue.BINARYLIT() is not null)
        {
            return D2CType.Short;
        }
        else if (rvalue.STRINGLIT() is not null)
        {
            return D2CType.CharP;
        }
        else if (rvalue.paren is not null)
        {
            return InferType(context, rvalue.rvalue(0));
        }
        else if (rvalue.funccall() is not null)
        {
            return context.GetFuncDef(rvalue.funccall().ID().GetText()).ReturnType;
        }
        else if (rvalue.postfixop() is not null)
        {
            return InferType(context, rvalue.rvalue(0));
        }
        else if (rvalue.prefixop() is not null)
        {
            var op = rvalue.prefixop().GetText();

            if (op == "*")
            {
                return D2CType.Char;
            }

            return InferType(context, rvalue.rvalue(0));
        }
        else if (rvalue.binop() is not null)
        {
            var left = InferType(context, rvalue.rvalue(0));
            var right = InferType(context, rvalue.rvalue(1));

            return this.BinaryOperators[rvalue.binop().GetText()].Invoke(left, right);
        }
        else if (rvalue.casted is not null)
        {
            return GetD2CType(rvalue.type());
        }

        throw new Exception($"Cannot infer type of {rvalue.GetText()}");
    }

    public D2CType InferType(Context context, D2CParser.LvalueContext lvalue)
    {
        if (lvalue.STAR() is not null)
        {
            return D2CType.Char;
        }
        else
        {
            return context.GetVariable(lvalue.ID().GetText()).Type;
        }
    }

    public void CheckType(Context context, D2CParser.RvalueContext rvalue, D2CType expectedType)
    {
        var inferred = InferType(context, rvalue);
        Console.WriteLine($"Inferred type {inferred} for {rvalue.GetText()}");

        if (inferred.Matches(expectedType))
        {
            return;
        }
        else
        {
            throw new Exception($"Expected type {expectedType} but got {inferred}");
        }
    }

    public override object VisitProgram([NotNull] D2CParser.ProgramContext context)
    {
        var funcDefs = context.funcdef();

        foreach (var funcDef in funcDefs)
        {
            this.FoundReturn = false;
            this.VisitFuncdef(funcDef);

            if (!this.CurrentFuncDef.ReturnType.Matches(D2CType.Void))
            {
                if (!this.FoundReturn)
                {
                    throw new Exception($"Function {this.CurrentFuncDef.Identifier} does not return a value");
                }
            }
        }

        return null;
    }

    public override object VisitFuncdef([NotNull] D2CParser.FuncdefContext context)
    {
        var id = context.ID().GetText();
        this.CurrentFuncDef = this.CurrentContext.GetFuncDef(id);

        foreach (var arg in this.CurrentFuncDef.Parameters)
        {
            this.CurrentContext.AddVariable(arg);
        }

        return base.VisitFuncdef(context);
    }

    public override object VisitBlock([NotNull] D2CParser.BlockContext context)
    {
        this.CurrentContext = this.CurrentContext.CreateChildContext();
        var x = base.VisitBlock(context);
        this.CurrentContext = this.CurrentContext.Pop();
        return x;
    }

    public override object VisitVardecl([NotNull] D2CParser.VardeclContext context)
    {
        var type = GetD2CType(context.type());
        var id = context.ID().GetText();

        this.CurrentContext.AddVariable(new Variable(id, type));

        if (context.EQ() is not null)
        {
            var rvalue = context.rvalue();
            CheckType(this.CurrentContext, rvalue, type);
        }

        return base.VisitVardecl(context);
    }

    public override object VisitAssignment([NotNull] D2CParser.AssignmentContext context)
    {
        var ltype = InferType(this.CurrentContext, context.lvalue());
        CheckType(this.CurrentContext, context.rvalue(), ltype);

        return base.VisitAssignment(context);
    }

    public override object VisitRetstm([NotNull] D2CParser.RetstmContext context)
    {
        var retType = this.CurrentFuncDef.ReturnType;

        if (context.rvalue() is not null)
        {
            CheckType(this.CurrentContext, context.rvalue(), retType);
        }
        else
        {
            if (!retType.Matches(D2CType.Void))
            {
                throw new Exception($"Expected return type {retType} but got void");
            }
        }

        this.FoundReturn = true;
        return base.VisitRetstm(context);
    }

    public override object VisitIfstatement([NotNull] D2CParser.IfstatementContext context)
    {
        var rvalue = context.rvalue();
        CheckType(this.CurrentContext, rvalue, D2CType.Char); // No bools, only numeric types

        return base.VisitIfstatement(context);
    }

    public override object VisitIfstatementelse([NotNull] D2CParser.IfstatementelseContext context)
    {
        if (context.rvalue() is not null)
        {
            var rvalue = context.rvalue();
            CheckType(this.CurrentContext, rvalue, D2CType.Char); // No bools, only numeric types
        }

        return base.VisitIfstatementelse(context);
    }

    public override object VisitWhileloop([NotNull] D2CParser.WhileloopContext context)
    {
        var rvalue = context.rvalue();
        CheckType(this.CurrentContext, rvalue, D2CType.Char); // No bools, only numeric types

        return base.VisitWhileloop(context);
    }

    public override object VisitFunccall([NotNull] D2CParser.FunccallContext context)
    {
        var func = this.CurrentContext.GetFuncDef(context.ID().GetText());
        var args = context.arglist()._args;

        for (int i = 0; i < func.Parameters.Count; i++)
        {
            var param = func.Parameters[i];
            var arg = args[i];

            CheckType(this.CurrentContext, arg, param.Type);
        }

        return base.VisitFunccall(context);
    }

    public override object VisitRvalue([NotNull] D2CParser.RvalueContext context)
    {
        this.InferType(this.CurrentContext, context);
        return base.VisitRvalue(context);
    }
}

public class LocalsCollector : D2CBaseVisitor<List<Variable>>
{
    public List<Variable> Locals { get; } = new List<Variable>();

    private D2CType GetD2CType(D2CParser.TypeContext context)
    {
        if (context.pointertype() is not null)
        {
            return new D2CType(true, context.pointertype().puretype().ID().GetText());
        }
        else
        {
            return new D2CType(false, context.puretype().ID().GetText());
        }
    }

    public override List<Variable> VisitVardecl([NotNull] D2CParser.VardeclContext context)
    {
        var type = GetD2CType(context.type());
        var id = context.ID().GetText();

        this.Locals.Add(new Variable(id, type));

        return base.VisitVardecl(context);
    }
}

public class D2AssemblerCompiler : D2CBaseVisitor<object>
{
    public StringBuilder Output { get; } = new StringBuilder();
    public Context CurrentContext { get; set; }
    public FuncDef CurrentFuncDef { get; set; }
    public Dictionary<Variable, string> VariableLabels { get; } = new();
    public Dictionary<FuncDef, string> FuncLabels { get; } = new();

    public Dictionary<string, Func<D2CType, D2CType, D2CType>> BinaryOperators { get; set; }


    public D2AssemblerCompiler(Context initialContext)
    {
        this.CurrentContext = initialContext;
        this.BinaryOperators = new() {
            { "+", (a, b) => a},
            { "-", (a, b) => a},
            { "*", (a, b) => a},
            { "/", (a, b) => a},
            { "%", (a, b) => a},
            { "==", (a, b) => D2CType.Char},
            { "!=", (a, b) => D2CType.Char},
            { "<", (a, b) => D2CType.Char},
            { ">", (a, b) => D2CType.Char},
            { "<=", (a, b) => D2CType.Char},
            { ">=", (a, b) => D2CType.Char},
            { "&&", (a, b) => D2CType.Char},
            { "||", (a, b) => D2CType.Char},
            { "|", (a, b) => a},
            { "&", (a, b) => a},
            { "^", (a, b) => a},
        };
    }

    private D2CType GetD2CType(D2CParser.TypeContext context)
    {
        if (context.pointertype() is not null)
        {
            return new D2CType(true, context.pointertype().puretype().ID().GetText());
        }
        else
        {
            return new D2CType(false, context.puretype().ID().GetText());
        }
    }

    public D2CType InferType(Context context, D2CParser.RvalueContext rvalue)
    {
        if (rvalue.ID() is not null)
        {
            return context.GetVariable(rvalue.ID().GetText()).Type;
        }
        else if (rvalue.DECIMALLIT() is not null)
        {
            return D2CType.Short;
        }
        else if (rvalue.HEXADECILIT() is not null)
        {
            return D2CType.Short;
        }
        else if (rvalue.BINARYLIT() is not null)
        {
            return D2CType.Short;
        }
        else if (rvalue.STRINGLIT() is not null)
        {
            return D2CType.CharP;
        }
        else if (rvalue.paren is not null)
        {
            return InferType(context, rvalue.rvalue(0));
        }
        else if (rvalue.funccall() is not null)
        {
            return context.GetFuncDef(rvalue.funccall().ID().GetText()).ReturnType;
        }
        else if (rvalue.postfixop() is not null)
        {
            return InferType(context, rvalue.rvalue(0));
        }
        else if (rvalue.prefixop() is not null)
        {
            return InferType(context, rvalue.rvalue(0));
        }
        else if (rvalue.binop() is not null)
        {
            var left = InferType(context, rvalue.rvalue(0));
            var right = InferType(context, rvalue.rvalue(1));

            return this.BinaryOperators[rvalue.binop().GetText()].Invoke(left, right);
        }
        else if (rvalue.casted is not null)
        {
            return GetD2CType(rvalue.type());
        }

        throw new Exception($"Cannot infer type of {rvalue.GetText()}");
    }


    public void EmitLine(string s)
    {
        this.Output.AppendLine(s);
        Console.WriteLine(s);
    }

    private string GetLabelForLValue(D2CParser.LvalueContext context)
    {
        if (context.STAR() is null)
        {
            var id = context.ID().GetText();
            var var = this.CurrentContext.GetVariable(id);
            return this.VariableLabels[var];
        }
        else
        {
            if (context.ptrlabel is not null)
            {
                var id = context.ID().GetText();
                var var = this.CurrentContext.GetVariable(id);
                var label = this.VariableLabels[var];
                return $"{label}";
            }
            else if (context.ptrhex is not null)
            {
                var num = Convert.ToInt32(context.HEXADECILIT().GetText(), 16);
                var hex = num.ToString("X4");
                return $"${hex}";
            }
            else if (context.ptrdec is not null)
            {
                var num = int.Parse(context.DECIMALLIT().GetText());
                var hex = num.ToString("X4");
                return $"${hex}";
            }

            throw new Exception($"Cannot get label for {context.GetText()}");
        }
    }

    public string binopTemp;
    public string valueTemp;
    public override object VisitProgram([NotNull] D2CParser.ProgramContext context)
    {
        this.EmitLine($".org $8000");
        this.binopTemp = $"binop_temp_{this.CurrentContext.GetNextLabel()}";
        this.valueTemp = $"value_temp_{this.CurrentContext.GetNextLabel()}";
        this.EmitLine($"{this.binopTemp}:");
        this.EmitLine($"  .word $0000");
        this.EmitLine($"{this.valueTemp}:");
        this.EmitLine($"  .word $0000");

        foreach (var funcDef in context.funcdef())
        {
            this.VisitFuncdef(funcDef);
        }

        var mainFunc = this.CurrentContext.GetFuncDef("main");
        var mainLabel = this.FuncLabels[mainFunc];
        var irqFunc = this.CurrentContext.GetFuncDef("irq");
        var irqLabel = this.FuncLabels[irqFunc];

        var resetLabel = $"reset_{this.CurrentContext.GetNextLabel()}";

        this.EmitLine($"{resetLabel}:");
        this.EmitLine($"  ldx #$C0");
        this.EmitLine($"  ldy #$00");
        this.EmitLine($"  lsp");
        this.EmitLine($"  sei"); // enable interrupts
        this.EmitLine($"  jsr {mainLabel}");
        this.EmitLine($"{resetLabel}_loop:");
        this.EmitLine($"  jmp {resetLabel}_loop");

        this.EmitLine($".org $fffc");
        this.EmitLine($".word {irqLabel}");
        this.EmitLine($".word {resetLabel}");

        return null;
    }

    public override object VisitFuncdef([NotNull] D2CParser.FuncdefContext context)
    {
        var id = context.ID().GetText();
        var funcDef = this.CurrentContext.GetFuncDef(id);
        funcDef.GetASM(this);
        return null;
    }

    public override object VisitVardecl([NotNull] D2CParser.VardeclContext context)
    {
        if (context.rvalue() is not null)
        {
            // Emit rvalue to accumulator
            // Store accumulator to variable
            var id = context.ID().GetText();
            var variable = this.CurrentContext.GetVariable(id);
            var type = variable.Type;
            var label = this.VariableLabels[variable];

            this.VisitRvalue(context.rvalue());

            for (int i = 0; i < type.Bytes; i++)
            {
                this.EmitLine($"  lda {this.valueTemp}+{i}");
                this.EmitLine($"  sta {label}+{i}");
            }
        }

        return null;
    }

    public override object VisitAssignment([NotNull] D2CParser.AssignmentContext context)
    {
        var labelForLValue = GetLabelForLValue(context.lvalue());
        var type = this.InferType(this.CurrentContext, context.rvalue());

        this.VisitRvalue(context.rvalue());

        for (int i = 0; i < type.Bytes; i++)
        {
            this.EmitLine($"  lda {this.valueTemp}+{i}");
            this.EmitLine($"  sta {labelForLValue}+{i}");
        }

        return null;
    }

    public override object VisitRetstm([NotNull] D2CParser.RetstmContext context)
    {
        if (context.rvalue() is not null)
        {
            this.VisitRvalue(context.rvalue());
        }

        this.EmitLine($"  rts");

        return null;
    }

    public override object VisitRvalue([NotNull] D2CParser.RvalueContext context)
    {
        if (context.ID() is not null)
        {
            // Just a variable
            var id = context.ID().GetText();
            var variable = this.CurrentContext.GetVariable(id);
            var label = this.VariableLabels[variable];

            for (int i = 0; i < variable.Type.Bytes; i++)
            {
                this.EmitLine($"  lda {label}+{i}");
                this.EmitLine($"  sta {this.valueTemp}+{i}");
            }
        }
        else if (context.casted is not null)
        {
            var type = this.InferType(this.CurrentContext, context.rvalue(0));
            var castTo = this.GetD2CType(context.casted);

            this.VisitRvalue(context.rvalue(0)); // Put value in valueTemp
        }
        else if (context.paren is not null)
        {
            return this.VisitRvalue(context.rvalue(0));
        }
        else if (context.prefixop() is not null)
        {
            var op = context.prefixop().GetText();

            if (op == "*")
            {
                // Assume value to be a pointer and get value at address
                this.VisitRvalue(context.rvalue(0)); // Put value in valueTemp
                this.EmitLine($"  lda ({this.valueTemp})");
                this.EmitLine($"  sta {this.valueTemp}");
            }
        }
        else if (context.postfixop() is not null)
        {

        }
        else if (context.binop() is not null)
        {
            var op = context.binop().GetText();
            var leftType = this.InferType(this.CurrentContext, context.rvalue(0));
            var rightType = this.InferType(this.CurrentContext, context.rvalue(1));

            // Will always assume left type
            var type = leftType;

            var opToInstr = (string op, int bytes) =>
            {
                if (op == "+")
                {
                    if (bytes == 1)
                    {
                        // Left value now stored in valueTemp
                        this.VisitRvalue(context.rvalue(0));
                        // Move left value to binopTemp
                        this.EmitLine($"  lda {this.valueTemp}");
                        this.EmitLine($"  sta {this.binopTemp}");

                        // Right value now stored in valueTemp
                        this.VisitRvalue(context.rvalue(1));
                        // Move right value to accumulator
                        this.EmitLine($"  lda {this.valueTemp}");

                        // Add left value to accumulator
                        this.EmitLine($"  adc {this.binopTemp}");

                        // Store result in valueTemp
                        this.EmitLine($"  sta {this.valueTemp}");
                    }
                    else if (bytes == 2)
                    {
                        // Left value now stored in valueTemp
                        this.VisitRvalue(context.rvalue(0));
                        // Move left value to binopTemp
                        this.EmitLine($"  lda {this.valueTemp}");
                        this.EmitLine($"  sta {this.binopTemp}");
                        this.EmitLine($"  lda {this.valueTemp}+1");
                        this.EmitLine($"  sta {this.binopTemp}+1");

                        // Right value now stored in valueTemp
                        this.VisitRvalue(context.rvalue(1));

                        // Move low byte of right value to accumulator
                        this.EmitLine($"  lda {this.valueTemp}");
                        // Add low byte of left value to accumulator
                        this.EmitLine($"  adc {this.binopTemp}");
                        // Store low byte result in valueTemp
                        this.EmitLine($"  sta {this.valueTemp}");

                        // Move high byte of right value to accumulator
                        this.EmitLine($"  lda {this.valueTemp}+1");
                        // Add high byte of left value to accumulator
                        this.EmitLine($"  adc {this.binopTemp}+1");
                        // Store high byte result in valueTemp
                        this.EmitLine($"  sta {this.valueTemp}+1");
                    }

                    return;
                }
                else if (op == "-")
                {
                    if (bytes == 1)
                    {
                        // Left value now stored in valueTemp
                        this.VisitRvalue(context.rvalue(0));
                        // Move left value to binopTemp
                        this.EmitLine($"  lda {this.valueTemp}");
                        this.EmitLine($"  sta {this.binopTemp}");

                        // Right value now stored in valueTemp
                        this.VisitRvalue(context.rvalue(1));
                        // Move right value to accumulator
                        this.EmitLine($"  lda {this.valueTemp}");

                        // Subtract left value from accumulator
                        this.EmitLine($"  sbc {this.binopTemp}");

                        // Store result in valueTemp
                        this.EmitLine($"  sta {this.valueTemp}");
                    }
                    else if (bytes == 2)
                    {
                        // Right value now stored in valueTemp
                        this.VisitRvalue(context.rvalue(1));
                        // Move right value to binopTemp
                        this.EmitLine($"  lda {this.valueTemp}");
                        this.EmitLine($"  sta {this.binopTemp}");
                        this.EmitLine($"  lda {this.valueTemp}+1");
                        this.EmitLine($"  sta {this.binopTemp}+1");

                        // Left value now stored in valueTemp
                        this.VisitRvalue(context.rvalue(0));

                        // Move high byte of left value to accumulator
                        this.EmitLine($"  lda {this.valueTemp}+1");
                        // Subtract high byte of right value from accumulator
                        this.EmitLine($"  sbc {this.binopTemp}+1");
                        this.EmitLine($"  php"); // Preserve carry flag
                        // Store high byte result in valueTemp
                        this.EmitLine($"  sta {this.valueTemp}+1");

                        // Move low byte of left value to accumulator
                        this.EmitLine($"  lda {this.valueTemp}");
                        // Subtract low byte of right value from accumulator
                        this.EmitLine($"  plp"); // Restore carry flag
                        this.EmitLine($"  sbc {this.binopTemp}");
                        // Store low byte result in valueTemp
                        this.EmitLine($"  sta {this.valueTemp}");
                    }

                    return;
                }
                else if (op == "==")
                {
                    if (bytes == 1)
                    {
                        // Right value now stored in valueTemp
                        this.VisitRvalue(context.rvalue(1));
                        // Move right value to binopTemp
                        this.EmitLine($"  lda {this.valueTemp}");
                        this.EmitLine($"  sta {this.binopTemp}");

                        // Left value now stored in valueTemp
                        this.VisitRvalue(context.rvalue(0));
                        // Move left value to accumulator
                        this.EmitLine($"  lda {this.valueTemp}");

                        // Compare left value to right value
                        this.EmitLine($"  cmp {this.binopTemp}");
                        // If zero flag is set, set valueTemp to 1, otherwise set valueTemp to 0

                        var labelZeroSet = this.CurrentContext.GetNextLabel();
                        var labelExit = this.CurrentContext.GetNextLabel();

                        this.EmitLine($"  jeq {labelZeroSet}");
                        this.EmitLine($"  lda #0");
                        this.EmitLine($"  jmp {labelExit}");

                        this.EmitLine($"{labelZeroSet}:");
                        this.EmitLine($"  lda #1");

                        this.EmitLine($"{labelExit}:");
                        this.EmitLine($"  sta {this.valueTemp}");
                    }
                    else if (bytes == 2)
                    {
                        // Right value now stored in valueTemp
                        this.VisitRvalue(context.rvalue(1));
                        // Move right value to binopTemp
                        this.EmitLine($"  lda {this.valueTemp}");
                        this.EmitLine($"  sta {this.binopTemp}");
                        this.EmitLine($"  lda {this.valueTemp}+1");
                        this.EmitLine($"  sta {this.binopTemp}+1");

                        // Left value now stored in valueTemp
                        this.VisitRvalue(context.rvalue(0));

                        // Move high byte of left value to accumulator
                        this.EmitLine($"  lda {this.valueTemp}+1");
                        // Compare high byte of left value to high byte of right value
                        this.EmitLine($"  cmp {this.binopTemp}+1");
                        // If zero flag is set, set valueTemp to 1, otherwise set valueTemp to 0

                        var labelZeroSet = this.CurrentContext.GetNextLabel();
                        var labelExit = this.CurrentContext.GetNextLabel();

                        this.EmitLine($"  jeq HIGH_{labelZeroSet}");
                        this.EmitLine($"  lda #0");
                        this.EmitLine($"  jmp HIGH_{labelExit}");

                        this.EmitLine($"HIGH_{labelZeroSet}:");
                        this.EmitLine($"  lda #1");

                        this.EmitLine($"HIGH_{labelExit}:");
                        this.EmitLine($"  sta {this.valueTemp}+1");

                        // Move low byte of left value to accumulator
                        this.EmitLine($"  lda {this.valueTemp}");
                        // Compare low byte of left value to low byte of right value
                        this.EmitLine($"  cmp {this.binopTemp}");
                        // If zero flag is set, set valueTemp to 1, otherwise set valueTemp to 0

                        this.EmitLine($"  jeq LOW_{labelZeroSet}");
                        this.EmitLine($"  lda #0");
                        this.EmitLine($"  jmp LOW_{labelExit}");

                        this.EmitLine($"LOW_{labelZeroSet}:");
                        this.EmitLine($"  lda #1");

                        this.EmitLine($"LOW_{labelExit}:");
                        this.EmitLine($"  sta {this.valueTemp}");
                    }

                    return;
                }

                throw new NotImplementedException($"Unknown operator {op}");
            };

            opToInstr(op, type.Bytes);
        }
        else if (context.funccall() is not null)
        {
            var id = context.funccall().ID().GetText();
            var func = this.CurrentContext.GetFuncDef(id);
            var funcLabel = this.FuncLabels[func];

            // Push the parameters onto the stack
            for (int i = 0; i < context.funccall().arglist()._args.Count; i++)
            {
                var arg = context.funccall().arglist()._args[i];
                this.VisitRvalue(arg);

                var param = func.Parameters[i];
                var paramLabel = this.VariableLabels[param];

                for (int j = 0; j < param.Type.Bytes; j++)
                {
                    this.EmitLine($"  lda {this.valueTemp}+{j}");
                    this.EmitLine($"  sta {paramLabel}+{j}");
                }
            }

            this.EmitLine($"  jsr {funcLabel}");
        }
        else if (context.DECIMALLIT() is not null)
        {
            var val = context.DECIMALLIT().GetText();
            var valInt = int.Parse(val);
            var valHex = valInt.ToString("X4");

            this.EmitLine($"  lda #${valHex.Substring(2, 2)}");
            this.EmitLine($"  sta {this.valueTemp}");
            this.EmitLine($"  lda #${valHex.Substring(0, 2)}");
            this.EmitLine($"  sta {this.valueTemp}+1");
        }
        else if (context.HEXADECILIT() is not null)
        {
            var val = context.HEXADECILIT().GetText();
            var valHex = val.Substring(2, 4);

            this.EmitLine($"  lda #${valHex.Substring(2, 2)}");
            this.EmitLine($"  sta {this.valueTemp}");
            this.EmitLine($"  lda #${valHex.Substring(0, 2)}");
            this.EmitLine($"  sta {this.valueTemp}+1");
        }
        else if (context.BINARYLIT() is not null)
        {

        }
        else if (context.STRINGLIT() is not null)
        {

        }

        return null;
    }

    public override object VisitIfstatement([NotNull] D2CParser.IfstatementContext context)
    {
        var labelEnd = this.CurrentContext.GetNextLabel();
        var labelElse = this.CurrentContext.GetNextLabel();
        this.VisitRvalue(context.rvalue());

        // Assume valueTemp to contain a single byte value that is either 0 or non-zero
        this.EmitLine($"  lda {this.valueTemp}");
        this.EmitLine($"  cmp #$00");
        this.EmitLine($"  jeq {labelElse}");
        this.VisitBlock(context.block());
        this.EmitLine($"  jmp {labelEnd}");
        this.EmitLine($"{labelElse}:");

        if (context.ifstatementelse() is not null)
        {
            this.VisitIfstatementelse(context.ifstatementelse());
        }

        this.EmitLine($"{labelEnd}:");

        return null;
    }

    public override object VisitIfstatementelse([NotNull] D2CParser.IfstatementelseContext context)
    {
        if (context.rvalue() is null)
        {
            // Normal else block
            this.VisitBlock(context.block());
        }
        else
        {
            // Else if block
            var labelEnd = this.CurrentContext.GetNextLabel();
            var labelElse = this.CurrentContext.GetNextLabel();
            this.VisitRvalue(context.rvalue());

            // Assume valueTemp to contain a single byte value that is either 0 or non-zero
            this.EmitLine($"  lda {this.valueTemp}");
            this.EmitLine($"  cmp #$00");
            this.EmitLine($"  jeq {labelElse}");
            this.VisitBlock(context.block());
            this.EmitLine($"  jmp {labelEnd}");
            this.EmitLine($"{labelElse}:");

            if (context.ifstatementelse() is not null)
            {
                this.VisitIfstatementelse(context.ifstatementelse());
            }

            this.EmitLine($"{labelEnd}:");
        }

        return null;
    }

    public override object VisitWhileloop([NotNull] D2CParser.WhileloopContext context)
    {
        var labelStart = this.CurrentContext.GetNextLabel();
        var labelEnd = this.CurrentContext.GetNextLabel();

        this.EmitLine($"{labelStart}:");
        this.VisitRvalue(context.rvalue());

        // Assume valueTemp to contain a single byte value that is either 0 or non-zero
        this.EmitLine($"  lda {this.valueTemp}");
        this.EmitLine($"  cmp #$00");
        this.EmitLine($"  jeq {labelEnd}");
        this.VisitBlock(context.block());
        this.EmitLine($"  jmp {labelStart}");
        this.EmitLine($"{labelEnd}:");

        return null;
    }
}

public class D2CCompiler
{
    public string Compile(string source)
    {
        var inputStream = new AntlrInputStream(source);
        var lexer = new D2CLexer(inputStream);
        var commonTokenStream = new CommonTokenStream(lexer);
        var parser = new D2CParser(commonTokenStream);

        var preprocessor = new Preprocessor(source);
        preprocessor.VisitProgram(parser.program());
        var preprocessed = preprocessor.Source;
        Console.WriteLine(preprocessed);

        inputStream = new AntlrInputStream(preprocessed);
        lexer = new D2CLexer(inputStream);
        commonTokenStream = new CommonTokenStream(lexer);
        parser = new D2CParser(commonTokenStream);

        var funcCollector = new FuncCollector();
        var funcDefs = funcCollector.VisitProgram(parser.program());

        if (!funcDefs.Any(x => x.Identifier == "main" && x.ReturnType.Is(false, "void") && x.Parameters.Count == 0))
        {
            throw new Exception("main function not found");
        }

        if (!funcDefs.Any(x => x.Identifier == "irq" && x.ReturnType.Is(false, "void") && x.Parameters.Count == 0))
        {
            throw new Exception("irq function not found");
        }

        Console.WriteLine(funcDefs.Select(x => x.Identifier).Aggregate((x, y) => x + ", " + y));

        inputStream = new AntlrInputStream(preprocessed);
        lexer = new D2CLexer(inputStream);
        commonTokenStream = new CommonTokenStream(lexer);
        parser = new D2CParser(commonTokenStream);

        var typeCheckerContext = new Context(null);
        funcDefs.ForEach(x => typeCheckerContext.AddFuncDef(x));
        var typeChecker = new TypeChecker(typeCheckerContext);
        typeChecker.VisitProgram(parser.program());
        Console.WriteLine("Type checking done");

        var d2AssemblerCompiler = new D2AssemblerCompiler(typeCheckerContext);

        parser.Reset();
        d2AssemblerCompiler.VisitProgram(parser.program());

        return d2AssemblerCompiler.Output.ToString();
    }
}