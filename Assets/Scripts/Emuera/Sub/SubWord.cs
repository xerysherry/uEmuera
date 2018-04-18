namespace MinorShift.Emuera.Sub
{
	/// <summary>
	/// FormattedStringWTの中身用のトークン
	/// </summary>
	internal abstract class SubWord
	{
		protected SubWord(WordCollection w) { words = w; }
		readonly WordCollection words;
		public WordCollection Words { get { return words; } }
		public bool IsMacro = false;
		public virtual void SetIsMacro()
		{
			IsMacro = true;
			if(Words != null)
				Words.SetIsMacro();
			
		}
	}

	internal sealed class TripleSymbolSubWord : SubWord
	{
		public TripleSymbolSubWord(char c) : base(null) { code = c; }
		readonly char code;
		public char Code { get { return code; } }
	}
	internal sealed class CurlyBraceSubWord : SubWord
	{
		public CurlyBraceSubWord(WordCollection w) : base(w) { }
	}

	internal sealed class PercentSubWord : SubWord
	{
		public PercentSubWord(WordCollection w) : base(w) { }
	}

	internal sealed class YenAtSubWord : SubWord
	{
		public YenAtSubWord(WordCollection w, StrFormWord fsLeft, StrFormWord fsRight)
			: base(w)
		{
			left = fsLeft;
			right = fsRight;
		}
		readonly StrFormWord left;
		readonly StrFormWord right;
		public StrFormWord Left { get { return left; } }
		public StrFormWord Right { get { return right; } }

		public override void SetIsMacro()
		{
			IsMacro = true;
			Words.SetIsMacro();
			Left.SetIsMacro();
			Right.SetIsMacro();
		}
	}
}
