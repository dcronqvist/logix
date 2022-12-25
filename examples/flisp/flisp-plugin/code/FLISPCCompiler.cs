using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;
using Antlr4.Runtime;

namespace FlispPlugin;

public class FuncDef
{
    public FLISPCParser.FuncdefContext Context { get; set; }
    public string Name { get; set; }
    public string Symbol { get; set; }
    public Dictionary<string, string> Args { get; set; } = new();
    public Dictionary<string, string> Locals { get; set; } = new();

    public FuncDef(string name, string symbol, FLISPCParser.FuncdefContext context)
    {
        Name = name;
        Symbol = symbol;
        Context = context;
    }

    public void AddArg(string name, string symbol)
    {
        Args.Add(name, symbol);
    }

    public void AddLocal(string name, string symbol)
    {
        Locals.Add(name, symbol);
    }
}

public class FLISPCCompiler : FLISPCBaseVisitor<object>
{
    public StringBuilder Output { get; set; } = new();
    public List<FuncDef> FuncDefs { get; set; } = new();

    private void Emit(string s)
    {
        Output.AppendLine(s);
    }

    private int _symbolCounter = 0;
    private string GetFreeSymbol(string add = null)
    {
        return $"{add ?? "sym"}{_symbolCounter++}";
    }

    private List<string> GetParams(FLISPCParser.FuncdefContext context)
    {
        List<string> s = new();
        int i = 0;
        if (context.paramlist() == null)
        {
            return s;
        }

        while (context.paramlist().SYMBOL(i) != null)
        {
            s.Add(context.paramlist().SYMBOL(i).GetText());
            i++;
        }
        return s;
    }

    private List<string> GetDeclaredVars(FLISPCParser.FuncdefContext context)
    {
        List<string> s = new();
        int i = 0;
        foreach (var ld in context.block()._lines)
        {
            if (ld.declaration() != null)
            {
                var dc = ld.declaration();
                s.Add(dc.SYMBOL().GetText());
            }
        }

        return s;
    }

    private FuncDef GetFunc(string name)
    {
        foreach (var func in FuncDefs)
        {
            if (func.Name == name)
            {
                return func;
            }
        }

        throw new Exception($"Function {name} not found!");
    }

    public override object VisitProgram([NotNull] FLISPCParser.ProgramContext context)
    {
        int i = 0;
        while (context.funcdef(i) != null)
        {
            var funcdef = context.funcdef(i);
            var name = funcdef.SYMBOL().GetText();
            var funcSymbol = GetFreeSymbol(name);
            var paramList = GetParams(funcdef);
            var locals = GetDeclaredVars(funcdef);

            var def = new FuncDef(name, funcSymbol, funcdef);
            foreach (var param in paramList)
            {
                def.AddArg(param, GetFreeSymbol(param));
            }
            foreach (var local in locals)
            {
                def.AddLocal(local, GetFreeSymbol(local));
            }

            FuncDefs.Add(def);

            i++;
        }

        if (this.GetFunc("main") is null)
        {
            throw new Exception("No main function found!");
        }

        var main = this.GetFunc("main");

        if (main.Args.Count > 0)
        {
            throw new Exception("Main function cannot have parameters!");
        }

        this.Emit("RMB 16"); // Reserved space for stuff
        this.Emit("ORG $10");
        this.Emit($"LDSP #$10");
        this.Emit($"JSR {main.Symbol}");
        var endSymbol = GetFreeSymbol("end");
        this.Emit($"{endSymbol}: NOP");
        this.Emit($"JMP {endSymbol}");

        foreach (var f in this.FuncDefs)
        {
            this.VisitFuncdef(f.Context);
        }

        return null;
    }

    private FuncDef _currentFunc;
    public override object VisitFuncdef([NotNull] FLISPCParser.FuncdefContext context)
    {
        /*
        funcSymbol: NOP
        param1:     RMB 1
        param2:     RMB 1
        param3:     RMB 1
                    LDA param1
                    PSHA
                    RTS
                    
        */

        var name = context.SYMBOL().GetText();
        var func = this.GetFunc(name);
        _currentFunc = func;

        foreach (var param in func.Args)
        {
            Emit($"{param.Value}: RMB 1");
        }
        foreach (var local in func.Locals)
        {
            Emit($"{local.Value}: RMB 1");
        }
        Emit($"{func.Symbol}: NOP");

        return this.VisitBlock(context.block());
    }

    public override object VisitExpr([Antlr4.Runtime.Misc.NotNull] FLISPCParser.ExprContext context)
    {
        // When evaluating an expression, the result should always be in the A register after

        if (context.SYMBOL() != null)
        {
            var name = context.SYMBOL().GetText();

            if (this._currentFunc.Locals.ContainsKey(name))
            {
                var symbol = this._currentFunc.Locals[name];
                Emit($"LDA {symbol}");
            }
            else if (this._currentFunc.Args.ContainsKey(name))
            {
                var symbol = this._currentFunc.Args[name];
                Emit($"LDA {symbol}");
            }
            else
            {
                throw new Exception($"Symbol {name} not found!");
            }
        }
        else if (context.LITERAL() != null)
        {
            var literal = context.LITERAL().GetText();
            Emit($"LDA #${literal.Substring(2)}");
        }
        else if (context.add != null)
        {
            // Addition
            this.VisitExpr(context.expr(0));
            this.Emit("PSHA");
            this.VisitExpr(context.expr(1));
            this.Emit("STA $00");
            this.Emit("PULA");
            this.Emit("ADDA $00");
            // Should be done now
        }
        else if (context.sub != null)
        {
            // Subtraction
            this.VisitExpr(context.expr(0));
            this.Emit("PSHA");
            this.VisitExpr(context.expr(1));
            this.Emit("STA $00");
            this.Emit("PULA");
            this.Emit("SUBA $00");
        }
        else if (context.mult != null)
        {
            // Multiplication
            // $00 = A
            // $01 = B
            // $02 = Result
            // Iterative addition method
            this.Emit("CLR $00");
            this.Emit("CLR $01");
            this.Emit("CLR $02");

            this.VisitExpr(context.expr(0));
            this.Emit("STA $00");
            this.VisitExpr(context.expr(1));
            this.Emit("STA $01");

            var loopStart = this.GetFreeSymbol("mulstart");

            this.Emit($"{loopStart}: LDA $02");
            this.Emit("ADDA $00");
            this.Emit("STA $02");
            this.Emit("DEC $01");
            this.Emit($"BNE {loopStart}");
            this.Emit("LDA $02");
        }
        else if (context.paren != null)
        {
            // Parentheses
            this.VisitExpr(context.expr(0));
        }
        else if (context.funccall() != null)
        {

            var funcname = context.funccall().SYMBOL().GetText();
            var func = this.GetFunc(funcname);

            var args = func.Args;
            int i = 0;
            foreach (var a in args)
            {
                // Console.WriteLine("Storing argument");
                // this.Emit($"LDA {a.Value}");
                // this.Emit("PSHA");

                this.VisitExpr(context.funccall().arglist()._exprs[i]);
                this.Emit($"STA {a.Value}");
                i++;
            }

            this.Emit($"JSR {func.Symbol}");
        }

        return null;
    }

    public override object VisitAssignment([Antlr4.Runtime.Misc.NotNull] FLISPCParser.AssignmentContext context)
    {
        if (this._currentFunc.Locals.TryGetValue(context.SYMBOL().GetText(), out var symbol))
        {
            this.VisitExpr(context.expr()); // This should leave the value in the A register
            Emit($"STA {symbol}");
        }
        else if (this._currentFunc.Args.TryGetValue(context.SYMBOL().GetText(), out symbol))
        {
            this.VisitExpr(context.expr()); // This should leave the value in the A register
            Emit($"STA {symbol}");
        }
        else
        {
            throw new Exception($"Symbol {context.SYMBOL().GetText()} not found!");
        }

        return null;
    }

    public override object VisitDeclaration([Antlr4.Runtime.Misc.NotNull] FLISPCParser.DeclarationContext context)
    {
        var symbol = this._currentFunc.Locals[context.SYMBOL().GetText()];
        this.VisitExpr(context.expr()); // This should leave the value in the A register
        Emit($"STA {symbol}");
        return null;
    }

    public override object VisitReturn([Antlr4.Runtime.Misc.NotNull] FLISPCParser.ReturnContext context)
    {
        // // RESET ARGUMENTS
        // foreach (var arg in this._currentFunc.Args.Reverse())
        // {
        //     this.Emit("PULA");
        //     this.Emit($"STA {arg.Value}");
        // }

        this.VisitExpr(context.expr());
        this.Emit("RTS");
        return null;
    }

    public override object VisitIfstatement([Antlr4.Runtime.Misc.NotNull] FLISPCParser.IfstatementContext context)
    {
        var end = this.GetFreeSymbol("ifend");
        var elseLabel = this.GetFreeSymbol("else");

        this.VisitExpr(context.expr());
        this.Emit("TSTA");
        this.Emit($"BEQ {elseLabel}");
        this.VisitBlock(context.block(0));
        this.Emit($"JMP {end}");
        this.Emit($"{elseLabel}: NOP");
        this.VisitBlock(context.block(1));
        this.Emit($"{end}: NOP");

        //return base.VisitIfstatement(context);
        return null;
    }

    public override object VisitWhilestatement([Antlr4.Runtime.Misc.NotNull] FLISPCParser.WhilestatementContext context)
    {
        var start = this.GetFreeSymbol("whilestart");
        var end = this.GetFreeSymbol("whileend");

        this.Emit($"{start}: NOP");
        this.VisitExpr(context.expr());
        this.Emit("TSTA");
        this.Emit($"BEQ {end}");
        this.VisitBlock(context.block());
        this.Emit($"JMP {start}");
        this.Emit($"{end}: NOP");

        //return base.VisitWhilestatement(context);
        return null;
    }

    public string Compile(string code)
    {
        var input = new AntlrInputStream(code);
        var lexer = new FLISPCLexer(input);
        var tokens = new CommonTokenStream(lexer);
        var parser = new FLISPCParser(tokens);
        var tree = parser.program();

        this.VisitProgram(tree);

        return Output.ToString();
    }
}