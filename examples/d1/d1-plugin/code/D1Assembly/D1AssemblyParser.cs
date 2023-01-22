//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.9.2
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from /Users/dcronqvist/repos/logix/examples/d1/d1-plugin/code/D1Assembly/D1Assembly.g4 by ANTLR 4.9.2

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace D1Plugin.D1Assembly {
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9.2")]
[System.CLSCompliant(false)]
public partial class D1AssemblyParser : Parser {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, T__1=2, REGISTER=3, LABEL=4, IMHEXADECIMAL=5, HEXADECIMAL=6, IMDECIMAL=7, 
		DECIMAL=8, NEWLINE=9, WS=10;
	public const int
		RULE_program = 0, RULE_line = 1, RULE_instrline = 2, RULE_symbolline = 3, 
		RULE_symbol = 4, RULE_arglist = 5, RULE_argument = 6;
	public static readonly string[] ruleNames = {
		"program", "line", "instrline", "symbolline", "symbol", "arglist", "argument"
	};

	private static readonly string[] _LiteralNames = {
		null, "':'", "','"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, "REGISTER", "LABEL", "IMHEXADECIMAL", "HEXADECIMAL", 
		"IMDECIMAL", "DECIMAL", "NEWLINE", "WS"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "D1Assembly.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string SerializedAtn { get { return new string(_serializedATN); } }

	static D1AssemblyParser() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}

		public D1AssemblyParser(ITokenStream input) : this(input, Console.Out, Console.Error) { }

		public D1AssemblyParser(ITokenStream input, TextWriter output, TextWriter errorOutput)
		: base(input, output, errorOutput)
	{
		Interpreter = new ParserATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	public partial class ProgramContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public LineContext[] line() {
			return GetRuleContexts<LineContext>();
		}
		[System.Diagnostics.DebuggerNonUserCode] public LineContext line(int i) {
			return GetRuleContext<LineContext>(i);
		}
		public ProgramContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_program; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			ID1AssemblyVisitor<TResult> typedVisitor = visitor as ID1AssemblyVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitProgram(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public ProgramContext program() {
		ProgramContext _localctx = new ProgramContext(Context, State);
		EnterRule(_localctx, 0, RULE_program);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 17;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==LABEL) {
				{
				{
				State = 14;
				line();
				}
				}
				State = 19;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class LineContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public InstrlineContext instrline() {
			return GetRuleContext<InstrlineContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public SymbollineContext symbolline() {
			return GetRuleContext<SymbollineContext>(0);
		}
		public LineContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_line; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			ID1AssemblyVisitor<TResult> typedVisitor = visitor as ID1AssemblyVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitLine(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public LineContext line() {
		LineContext _localctx = new LineContext(Context, State);
		EnterRule(_localctx, 2, RULE_line);
		try {
			State = 22;
			ErrorHandler.Sync(this);
			switch ( Interpreter.AdaptivePredict(TokenStream,1,Context) ) {
			case 1:
				EnterOuterAlt(_localctx, 1);
				{
				State = 20;
				instrline();
				}
				break;
			case 2:
				EnterOuterAlt(_localctx, 2);
				{
				State = 21;
				symbolline();
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class InstrlineContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode LABEL() { return GetToken(D1AssemblyParser.LABEL, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public SymbolContext symbol() {
			return GetRuleContext<SymbolContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public ArglistContext arglist() {
			return GetRuleContext<ArglistContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode[] NEWLINE() { return GetTokens(D1AssemblyParser.NEWLINE); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode NEWLINE(int i) {
			return GetToken(D1AssemblyParser.NEWLINE, i);
		}
		public InstrlineContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_instrline; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			ID1AssemblyVisitor<TResult> typedVisitor = visitor as ID1AssemblyVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitInstrline(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public InstrlineContext instrline() {
		InstrlineContext _localctx = new InstrlineContext(Context, State);
		EnterRule(_localctx, 4, RULE_instrline);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 25;
			ErrorHandler.Sync(this);
			switch ( Interpreter.AdaptivePredict(TokenStream,2,Context) ) {
			case 1:
				{
				State = 24;
				symbol();
				}
				break;
			}
			State = 27;
			Match(LABEL);
			State = 29;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << REGISTER) | (1L << LABEL) | (1L << IMHEXADECIMAL) | (1L << HEXADECIMAL) | (1L << IMDECIMAL) | (1L << DECIMAL))) != 0)) {
				{
				State = 28;
				arglist();
				}
			}

			State = 32;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			do {
				{
				{
				State = 31;
				Match(NEWLINE);
				}
				}
				State = 34;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			} while ( _la==NEWLINE );
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class SymbollineContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public SymbolContext symbol() {
			return GetRuleContext<SymbolContext>(0);
		}
		public SymbollineContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_symbolline; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			ID1AssemblyVisitor<TResult> typedVisitor = visitor as ID1AssemblyVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitSymbolline(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public SymbollineContext symbolline() {
		SymbollineContext _localctx = new SymbollineContext(Context, State);
		EnterRule(_localctx, 6, RULE_symbolline);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 36;
			symbol();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class SymbolContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode LABEL() { return GetToken(D1AssemblyParser.LABEL, 0); }
		public SymbolContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_symbol; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			ID1AssemblyVisitor<TResult> typedVisitor = visitor as ID1AssemblyVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitSymbol(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public SymbolContext symbol() {
		SymbolContext _localctx = new SymbolContext(Context, State);
		EnterRule(_localctx, 8, RULE_symbol);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 38;
			Match(LABEL);
			State = 39;
			Match(T__0);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class ArglistContext : ParserRuleContext {
		public ArgumentContext _argument;
		public IList<ArgumentContext> _args = new List<ArgumentContext>();
		[System.Diagnostics.DebuggerNonUserCode] public ArgumentContext[] argument() {
			return GetRuleContexts<ArgumentContext>();
		}
		[System.Diagnostics.DebuggerNonUserCode] public ArgumentContext argument(int i) {
			return GetRuleContext<ArgumentContext>(i);
		}
		public ArglistContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_arglist; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			ID1AssemblyVisitor<TResult> typedVisitor = visitor as ID1AssemblyVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitArglist(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public ArglistContext arglist() {
		ArglistContext _localctx = new ArglistContext(Context, State);
		EnterRule(_localctx, 10, RULE_arglist);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 41;
			_localctx._argument = argument();
			_localctx._args.Add(_localctx._argument);
			State = 46;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==T__1) {
				{
				{
				State = 42;
				Match(T__1);
				State = 43;
				_localctx._argument = argument();
				_localctx._args.Add(_localctx._argument);
				}
				}
				State = 48;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class ArgumentContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode REGISTER() { return GetToken(D1AssemblyParser.REGISTER, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode LABEL() { return GetToken(D1AssemblyParser.LABEL, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode HEXADECIMAL() { return GetToken(D1AssemblyParser.HEXADECIMAL, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode DECIMAL() { return GetToken(D1AssemblyParser.DECIMAL, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode IMDECIMAL() { return GetToken(D1AssemblyParser.IMDECIMAL, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode IMHEXADECIMAL() { return GetToken(D1AssemblyParser.IMHEXADECIMAL, 0); }
		public ArgumentContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_argument; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			ID1AssemblyVisitor<TResult> typedVisitor = visitor as ID1AssemblyVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitArgument(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public ArgumentContext argument() {
		ArgumentContext _localctx = new ArgumentContext(Context, State);
		EnterRule(_localctx, 12, RULE_argument);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 49;
			_la = TokenStream.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << REGISTER) | (1L << LABEL) | (1L << IMHEXADECIMAL) | (1L << HEXADECIMAL) | (1L << IMDECIMAL) | (1L << DECIMAL))) != 0)) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
				ErrorHandler.ReportMatch(this);
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	private static char[] _serializedATN = {
		'\x3', '\x608B', '\xA72A', '\x8133', '\xB9ED', '\x417C', '\x3BE7', '\x7786', 
		'\x5964', '\x3', '\f', '\x36', '\x4', '\x2', '\t', '\x2', '\x4', '\x3', 
		'\t', '\x3', '\x4', '\x4', '\t', '\x4', '\x4', '\x5', '\t', '\x5', '\x4', 
		'\x6', '\t', '\x6', '\x4', '\a', '\t', '\a', '\x4', '\b', '\t', '\b', 
		'\x3', '\x2', '\a', '\x2', '\x12', '\n', '\x2', '\f', '\x2', '\xE', '\x2', 
		'\x15', '\v', '\x2', '\x3', '\x3', '\x3', '\x3', '\x5', '\x3', '\x19', 
		'\n', '\x3', '\x3', '\x4', '\x5', '\x4', '\x1C', '\n', '\x4', '\x3', '\x4', 
		'\x3', '\x4', '\x5', '\x4', ' ', '\n', '\x4', '\x3', '\x4', '\x6', '\x4', 
		'#', '\n', '\x4', '\r', '\x4', '\xE', '\x4', '$', '\x3', '\x5', '\x3', 
		'\x5', '\x3', '\x6', '\x3', '\x6', '\x3', '\x6', '\x3', '\a', '\x3', '\a', 
		'\x3', '\a', '\a', '\a', '/', '\n', '\a', '\f', '\a', '\xE', '\a', '\x32', 
		'\v', '\a', '\x3', '\b', '\x3', '\b', '\x3', '\b', '\x2', '\x2', '\t', 
		'\x2', '\x4', '\x6', '\b', '\n', '\f', '\xE', '\x2', '\x3', '\x3', '\x2', 
		'\x5', '\n', '\x2', '\x34', '\x2', '\x13', '\x3', '\x2', '\x2', '\x2', 
		'\x4', '\x18', '\x3', '\x2', '\x2', '\x2', '\x6', '\x1B', '\x3', '\x2', 
		'\x2', '\x2', '\b', '&', '\x3', '\x2', '\x2', '\x2', '\n', '(', '\x3', 
		'\x2', '\x2', '\x2', '\f', '+', '\x3', '\x2', '\x2', '\x2', '\xE', '\x33', 
		'\x3', '\x2', '\x2', '\x2', '\x10', '\x12', '\x5', '\x4', '\x3', '\x2', 
		'\x11', '\x10', '\x3', '\x2', '\x2', '\x2', '\x12', '\x15', '\x3', '\x2', 
		'\x2', '\x2', '\x13', '\x11', '\x3', '\x2', '\x2', '\x2', '\x13', '\x14', 
		'\x3', '\x2', '\x2', '\x2', '\x14', '\x3', '\x3', '\x2', '\x2', '\x2', 
		'\x15', '\x13', '\x3', '\x2', '\x2', '\x2', '\x16', '\x19', '\x5', '\x6', 
		'\x4', '\x2', '\x17', '\x19', '\x5', '\b', '\x5', '\x2', '\x18', '\x16', 
		'\x3', '\x2', '\x2', '\x2', '\x18', '\x17', '\x3', '\x2', '\x2', '\x2', 
		'\x19', '\x5', '\x3', '\x2', '\x2', '\x2', '\x1A', '\x1C', '\x5', '\n', 
		'\x6', '\x2', '\x1B', '\x1A', '\x3', '\x2', '\x2', '\x2', '\x1B', '\x1C', 
		'\x3', '\x2', '\x2', '\x2', '\x1C', '\x1D', '\x3', '\x2', '\x2', '\x2', 
		'\x1D', '\x1F', '\a', '\x6', '\x2', '\x2', '\x1E', ' ', '\x5', '\f', '\a', 
		'\x2', '\x1F', '\x1E', '\x3', '\x2', '\x2', '\x2', '\x1F', ' ', '\x3', 
		'\x2', '\x2', '\x2', ' ', '\"', '\x3', '\x2', '\x2', '\x2', '!', '#', 
		'\a', '\v', '\x2', '\x2', '\"', '!', '\x3', '\x2', '\x2', '\x2', '#', 
		'$', '\x3', '\x2', '\x2', '\x2', '$', '\"', '\x3', '\x2', '\x2', '\x2', 
		'$', '%', '\x3', '\x2', '\x2', '\x2', '%', '\a', '\x3', '\x2', '\x2', 
		'\x2', '&', '\'', '\x5', '\n', '\x6', '\x2', '\'', '\t', '\x3', '\x2', 
		'\x2', '\x2', '(', ')', '\a', '\x6', '\x2', '\x2', ')', '*', '\a', '\x3', 
		'\x2', '\x2', '*', '\v', '\x3', '\x2', '\x2', '\x2', '+', '\x30', '\x5', 
		'\xE', '\b', '\x2', ',', '-', '\a', '\x4', '\x2', '\x2', '-', '/', '\x5', 
		'\xE', '\b', '\x2', '.', ',', '\x3', '\x2', '\x2', '\x2', '/', '\x32', 
		'\x3', '\x2', '\x2', '\x2', '\x30', '.', '\x3', '\x2', '\x2', '\x2', '\x30', 
		'\x31', '\x3', '\x2', '\x2', '\x2', '\x31', '\r', '\x3', '\x2', '\x2', 
		'\x2', '\x32', '\x30', '\x3', '\x2', '\x2', '\x2', '\x33', '\x34', '\t', 
		'\x2', '\x2', '\x2', '\x34', '\xF', '\x3', '\x2', '\x2', '\x2', '\b', 
		'\x13', '\x18', '\x1B', '\x1F', '$', '\x30',
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
} // namespace D1Plugin.D1Assembly