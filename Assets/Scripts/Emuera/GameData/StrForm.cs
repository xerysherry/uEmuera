using System;
using System.Text;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.GameData.Variable;
using MinorShift.Emuera.Sub;
using MinorShift._Library;
using MinorShift.Emuera.GameData.Function;
using System.Collections.Generic;

namespace MinorShift.Emuera.GameData
{
	internal sealed class StrForm
	{
		private StrForm() { }
		string[] strs = null;//terms.Length + 1
		IOperandTerm[] terms = null;

		#region static
		static FormattedStringMethod formatCurlyBrace = null;
		static FormattedStringMethod formatPercent = null;
		static FormattedStringMethod formatYenAt = null;
		static FunctionMethodTerm NameTarget = null;// "***"
		static FunctionMethodTerm CallnameMaster = null;// "+++"
		static FunctionMethodTerm CallnamePlayer = null;// "==="
		static FunctionMethodTerm NameAssi = null;// "///"
		static FunctionMethodTerm CallnameTarget = null;// "$$$"
		public static void Initialize()
		{
			formatCurlyBrace = new FormatCurlyBrace();
			formatPercent = new FormatPercent();
			formatYenAt = new FormatYenAt();
			VariableToken nameID = GlobalStatic.VariableData.GetSystemVariableToken("NAME");
			VariableToken callnameID = GlobalStatic.VariableData.GetSystemVariableToken("CALLNAME");
			IOperandTerm[] zeroArg = new IOperandTerm[] { new SingleTerm(0) };
			VariableTerm target = new VariableTerm(GlobalStatic.VariableData.GetSystemVariableToken("TARGET"), zeroArg);
			VariableTerm master = new VariableTerm(GlobalStatic.VariableData.GetSystemVariableToken("MASTER"), zeroArg);
			VariableTerm player = new VariableTerm(GlobalStatic.VariableData.GetSystemVariableToken("PLAYER"), zeroArg);
			VariableTerm assi = new VariableTerm(GlobalStatic.VariableData.GetSystemVariableToken("ASSI"), zeroArg);

			VariableTerm nametarget = new VariableTerm(nameID, new IOperandTerm[] { target });
			VariableTerm callnamemaster = new VariableTerm(callnameID, new IOperandTerm[] { master });
			VariableTerm callnameplayer = new VariableTerm(callnameID, new IOperandTerm[] { player });
			VariableTerm nameassi = new VariableTerm(nameID, new IOperandTerm[] { assi });
			VariableTerm callnametarget = new VariableTerm(callnameID, new IOperandTerm[] { target });
			NameTarget = new FunctionMethodTerm(formatPercent, new IOperandTerm[] { nametarget, null, null });
            CallnameMaster = new FunctionMethodTerm(formatPercent, new IOperandTerm[] { callnamemaster, null, null });
            CallnamePlayer = new FunctionMethodTerm(formatPercent, new IOperandTerm[] { callnameplayer, null, null });
            NameAssi = new FunctionMethodTerm(formatPercent, new IOperandTerm[] { nameassi, null, null });
            CallnameTarget = new FunctionMethodTerm(formatPercent, new IOperandTerm[] { callnametarget, null, null });
		}

		public static StrForm FromWordToken(StrFormWord wt)
		{
			StrForm ret = new StrForm();
			ret.strs = wt.Strs;
			IOperandTerm[] termArray = new IOperandTerm[wt.SubWords.Length];
			for (int i = 0; i < wt.SubWords.Length; i++)
			{
				SubWord SWT = wt.SubWords[i];
				TripleSymbolSubWord tSymbol = SWT as TripleSymbolSubWord;
				if (tSymbol != null)
				{
					switch (tSymbol.Code)
					{
						case '*':
							termArray[i] = NameTarget;
							continue;
						case '+':
							termArray[i] = CallnameMaster;
							continue;
						case '=':
							termArray[i] = CallnamePlayer;
							continue;
						case '/':
							termArray[i] = NameAssi;
							continue;
						case '$':
							termArray[i] = CallnameTarget;
							continue;
					}
					throw new ExeEE("何かおかしい");
				}
                WordCollection wc;
				IOperandTerm operand;
				YenAtSubWord yenat = SWT as YenAtSubWord;
				if (yenat != null)
				{
					wc = yenat.Words;
                    if (wc != null)
                    {
                        operand = ExpressionParser.ReduceIntegerTerm(wc, TermEndWith.EoL);
                        if (!wc.EOL)
                            throw new CodeEE("三項演算子\\@の第一オペランドが異常です");
                    }
                    else
                        operand = new SingleTerm(0);
					IOperandTerm left = new StrFormTerm(StrForm.FromWordToken(yenat.Left));
					IOperandTerm right;
					if (yenat.Right == null)
						right = new SingleTerm("");
					else
						right = new StrFormTerm(StrForm.FromWordToken(yenat.Right));
					termArray[i] = new FunctionMethodTerm(formatYenAt, new IOperandTerm[] { operand, left, right });
					continue;
				}
                wc = SWT.Words;
                operand = ExpressionParser.ReduceExpressionTerm(wc, TermEndWith.Comma);
                if (operand == null)
                {
                    if (SWT is CurlyBraceSubWord)
                        throw new CodeEE("{}の中に式が存在しません");
                    else
                        throw new CodeEE("%%の中に式が存在しません");
                }
                IOperandTerm second = null;
				SingleTerm third = null;
				wc.ShiftNext();
                if (!wc.EOL)
                {
                    second = ExpressionParser.ReduceIntegerTerm(wc, TermEndWith.Comma);

                    wc.ShiftNext();
                    if (!wc.EOL)
                    {
                        IdentifierWord id = wc.Current as IdentifierWord;
                        if (id == null)
                            throw new CodeEE("','の後にRIGHT又はLEFTがありません");
                        if (string.Equals(id.Code, "LEFT", Config.SCVariable))//標準RIGHT
                            third = new SingleTerm(1);
                        else if (!string.Equals(id.Code, "RIGHT", Config.SCVariable))
                            throw new CodeEE("','の後にRIGHT又はLEFT以外の単語があります");
                        wc.ShiftNext();
                    }
                    if (!wc.EOL)
                        throw new CodeEE("RIGHT又はLEFTの後に余分な文字があります");
                }
				if (SWT is CurlyBraceSubWord)
				{
					if (operand.GetOperandType() != typeof(Int64))
						throw new CodeEE("{}の中の式が数式ではありません");
					termArray[i] = new FunctionMethodTerm(formatCurlyBrace, new IOperandTerm[] { operand, second, third });
					continue;
				}
				if (operand.GetOperandType() != typeof(string))
					throw new CodeEE("%%の中の式が文字列式ではありません");
				termArray[i] = new FunctionMethodTerm(formatPercent, new IOperandTerm[] { operand, second, third });
			}
            ret.terms = termArray;
			return ret;
		}
		#endregion

		public bool IsConst
		{
			get
			{
				return (strs.Length == 1);
			}
		}

		public IOperandTerm GetIOperandTerm()
		{
			if((strs.Length == 2) && (strs[0].Length == 0) && (strs[1].Length == 0))
				return terms[0];
			return null;
		}

		public void Restructure(ExpressionMediator exm)
		{
			if (strs.Length == 1)
				return;
			bool canRestructure = false;
			for (int i = 0; i < terms.Length; i++)
			{
				terms[i] = terms[i].Restructure(exm);
				if (terms[i] is SingleTerm)
				{
					canRestructure = true;
				}
			}
			if (!canRestructure)
				return;
			List<string> strList = new List<string>();
			List<IOperandTerm> termList = new List<IOperandTerm>();
			strList.AddRange(strs);
			termList.AddRange(terms);
			for (int i = 0; i < termList.Count; i++)
			{
				if (termList[i] is SingleTerm)
				{
					string str = termList[i].GetStrValue(exm);
					strList[i] = strList[i] + str + strList[i + 1];
					termList.RemoveAt(i);
					strList.RemoveAt(i + 1);
					i--;
				}
			}
			strs = new string[strList.Count];
			terms = new IOperandTerm[termList.Count];
			strList.CopyTo(strs);
			termList.CopyTo(terms);
			return;
		}

		public string GetString(ExpressionMediator exm)
		{
			if (strs.Length == 1)
				return strs[0];
			StringBuilder builder = new StringBuilder(100);
			for (int i = 0; i < strs.Length - 1; i++)
			{
				builder.Append(strs[i]);
				builder.Append(terms[i].GetStrValue(exm));
			}
			builder.Append(strs[strs.Length - 1]);
			return builder.ToString();
		}

		#region FormattedStringMethod 書式付文字列の内部
		private abstract class FormattedStringMethod : FunctionMethod
		{
			public FormattedStringMethod()
			{
				CanRestructure = true;
				ReturnType = typeof(string);
				argumentTypeArray = null;
			}
			public override string CheckArgumentType(string name, IOperandTerm[] arguments) { throw new ExeEE("型チェックは呼び出し元が行うこと"); }
			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments) { throw new ExeEE("戻り値の型が違う"); }
			public override SingleTerm GetReturnValue(ExpressionMediator exm, IOperandTerm[] arguments) { return new SingleTerm(GetStrValue(exm, arguments)); }
		}

		private sealed class FormatCurlyBrace : FormattedStringMethod
		{
			public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				string ret = arguments[0].GetIntValue(exm).ToString();
				if (arguments[1] == null)
					return ret;
				if (arguments[2] != null)
					ret = ret.PadRight((int)arguments[1].GetIntValue(exm), ' ');//LEFT
				else
					ret = ret.PadLeft((int)arguments[1].GetIntValue(exm), ' ');//RIGHT
				return ret;
			}
		}

		private sealed class FormatPercent : FormattedStringMethod
		{
			public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				string ret = arguments[0].GetStrValue(exm);
				if (arguments[1] == null)
					return ret;
				int totalLength = (int)arguments[1].GetIntValue(exm);
				int currentLength = LangManager.GetStrlenLang(ret);
				totalLength -= currentLength - ret.Length;//全角文字の数だけマイナス。タブ文字？ゼロ幅文字？知るか！
				if (totalLength < ret.Length)
					return ret;//PadLeftは0未満を送ると例外を投げる
				if (arguments[2] != null)
					ret = ret.PadRight(totalLength, ' ');//LEFT
				else
					ret = ret.PadLeft(totalLength, ' ');//RIGHT
				return ret;
			}
		}

		private sealed class FormatYenAt : FormattedStringMethod
		{//Operator のTernaryIntStrStrとやってることは同じ
			public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				return (arguments[0].GetIntValue(exm) != 0) ? arguments[1].GetStrValue(exm) : arguments[2].GetStrValue(exm);
			}
		}

		#endregion
	}
}