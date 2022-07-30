using System;
using MinorShift.Emuera.GameData.Expression;

namespace MinorShift.Emuera.Sub
{
	internal abstract class Word
	{
		public abstract char Type { get; }
		public bool IsMacro = false;
		public virtual void SetIsMacro()
		{
			IsMacro = true;
		}
	}

	internal sealed class NullWord : Word
	{
		public NullWord() { }
		public override char Type { get { return '\0'; } }
		public override string ToString()
		{
			return "/null/";
		}
	}

	internal sealed class IdentifierWord : Word
	{
		public IdentifierWord(string s) { code = s; }
		readonly string code;
		public string Code { get { return code; } }
		public override char Type { get { return 'A'; } }
		public override string ToString()
		{
			return code;
		}
	}

	internal sealed class LiteralIntegerWord : Word
	{
		public LiteralIntegerWord(Int64 i) { code = i; }
		readonly Int64 code;
		public Int64 Int { get { return code; } }
		public override char Type { get { return '0'; } }
		public override string ToString()
		{
			return code.ToString();
		}
	}

	internal sealed class LiteralStringWord : Word
	{
		public LiteralStringWord(string s) { code = s; }
		readonly string code;
		public string Str { get { return code; } }
		public override char Type { get { return '\"'; } }
		public override string ToString()
		{
			return "\"" + code + "\"";
		}
	}


	internal sealed class OperatorWord : Word
	{
		public OperatorWord(OperatorCode op) { code = op; }
		readonly OperatorCode code;
		public OperatorCode Code { get { return code; } }
		public override char Type { get { return '='; } }
		public override string ToString()
		{
			return code.ToString();
		}
	}

	internal sealed class SymbolWord : Word
	{
		public SymbolWord(char c) { code = c; }
		readonly char code;
		public override char Type { get { return code; } }
		public override string ToString()
		{
			return code.ToString();
		}
	}

	internal sealed class StrFormWord : Word
	{

		public StrFormWord(string[] s, SubWord[] SWT) { strs = s; subwords = SWT; }
		readonly string[] strs;
		readonly SubWord[] subwords;
		public string[] Strs { get { return strs; } }
		public SubWord[] SubWords { get { return subwords; } }
		public override char Type { get { return 'F'; } }//@はSymbolがつかっちゃった
		
		public override void SetIsMacro()
		{
			IsMacro = true;
			foreach(SubWord subword in SubWords)
			{
				subword.SetIsMacro();
			}
		}
	}


	internal sealed class TermWord : Word
	{
		public TermWord(IOperandTerm term) { this.term = term; }
		readonly IOperandTerm term;
		public IOperandTerm Term { get { return term; } }
		public override char Type { get { return 'T'; } }
	}
	
	internal sealed class MacroWord : Word
	{
		public MacroWord(int num) { this.num = num; }
		readonly int num;
		public int Number { get { return num; } }
		public override char Type { get { return 'M'; } }
		public override string ToString()
		{
			return "Arg" + num.ToString();
		}
	}
	
	
	
	
	
}
