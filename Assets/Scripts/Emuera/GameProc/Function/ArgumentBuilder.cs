using System;
using System.Collections.Generic;
//using System.Drawing;
//using Microsoft.VisualBasic;
using MinorShift.Emuera.Sub;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.GameData.Variable;
using MinorShift.Emuera.GameData;
using MinorShift.Emuera.GameData.Function;
using uEmuera.Drawing;
using uEmuera.VisualBasic;

namespace MinorShift.Emuera.GameProc.Function
{
	internal abstract class ArgumentBuilder
	{
		protected void assignwarn(string mes, InstructionLine line, int level, bool isBackComp)
		{
			bool isError = level >= 2;
			if (isError)
			{
				line.IsError = true;
				line.ErrMes = mes;
			}
			ParserMediator.Warn(mes, line, level, isError, isBackComp);
		}
		protected void warn(string mes, InstructionLine line, int level, bool isBackComp)
		{
			mes = line.Function.Name + "命令:" + mes;
			bool isError = level >= 2;
			if (isError)
			{
				line.IsError = true;
				line.ErrMes = mes;
			}
			ParserMediator.Warn(mes, line, level, isError, isBackComp);
		}
		/// <summary>
		/// 引数の型と数。typeof(void)で任意の型（あるいは個別にチェックするべき引数）。nullでその引数は省略可能
		/// </summary>
		protected Type[] argumentTypeArray;//
		/// <summary>
		/// 最低限必要な引数の数。設定しないと全て省略不可。
		/// </summary>
		protected int minArg = -1;
		/// <summary>
		/// 引数の数に制限なし。
		/// </summary>
		protected bool argAny = false;
		protected bool checkArgumentType(InstructionLine line, ExpressionMediator exm, IOperandTerm[] arguments)
		{
			if (arguments == null)
			{
				warn("引数がありません", line, 2, false);
				return false;
			}
			if ( arguments.Length < minArg || 
				((arguments.Length < argumentTypeArray.Length) && (minArg < 0)) )
			{
				warn("引数が足りません", line, 2, false);
				return false;
			}
			int length = arguments.Length;
			if ((arguments.Length > argumentTypeArray.Length)&&(!argAny))
			{
				warn("引数が多すぎます", line, 1, false);
				length = argumentTypeArray.Length;
			}
			for (int i = 0; i < length; i++)
			{
				Type allowType;
				if ((!argAny) && (argumentTypeArray[i] == null))
					continue;
				else if (argAny && i >= argumentTypeArray.Length)
					allowType = argumentTypeArray[argumentTypeArray.Length - 1];
				else
					allowType = argumentTypeArray[i];
				if (arguments[i] == null)
				{
					if (allowType == null)
						continue;
					warn("第" + (i + 1).ToString() + "引数を認識できません", line, 2, false);
					return false;
				}
				if ((allowType != typeof(void)) && (allowType != arguments[i].GetOperandType()))
				{
					warn("第" + (i + 1).ToString() + "引数の型が正しくありません", line, 2, false);
					return false;
				}
			}
			length = arguments.Length;
			for (int i = 0; i < length; i++)
			{
				if (arguments[i] == null)
					continue;
				arguments[i] = arguments[i].Restructure(exm);
			}
			return true;
		}

		protected VariableTerm getChangeableVariable(IOperandTerm[] terms, int i, InstructionLine line)
		{
            if (!(terms[i - 1] is VariableTerm varTerm))
            {
                warn("第" + i + "引数に変数以外を指定することはできません", line, 2, false);
                return null;
            }
            else if (varTerm.Identifier.IsConst)
            {
                warn("第" + i + "引数に変更できない変数を指定することはできません", line, 2, false);
                return null;
            }
            return varTerm;
		}

		protected WordCollection popWords(InstructionLine line)
		{
			StringStream st = line.PopArgumentPrimitive();
			return LexicalAnalyzer.Analyse(st, LexEndWith.EoL, LexAnalyzeFlag.None);
		}

		protected IOperandTerm[] popTerms(InstructionLine line)
		{
			StringStream st = line.PopArgumentPrimitive();
			WordCollection wc = LexicalAnalyzer.Analyse(st, LexEndWith.EoL, LexAnalyzeFlag.None);
			return ExpressionParser.ReduceArguments(wc, ArgsEndWith.EoL, false);
		}
		public abstract Argument CreateArgument(InstructionLine line, ExpressionMediator exm);
	}


	internal static partial class ArgumentParser
	{
		readonly static Dictionary<FunctionArgType, ArgumentBuilder> argb = new Dictionary<FunctionArgType, ArgumentBuilder>();
		
		public static Dictionary<FunctionArgType, ArgumentBuilder> GetArgumentBuilderDictionary()
		{
			return argb;
		}
		public static ArgumentBuilder GetArgumentBuilder(FunctionArgType key)
		{
			return argb[key];
		}
		readonly static Dictionary<string, ArgumentBuilder> nargb = new Dictionary<string, ArgumentBuilder>();

		/// <summary>
		/// 一般的な引数作成器の呼び出し。数式と文字列式のいずれかのみを引数とし、特殊なチェックが必要ないもの
		/// </summary>
		/// <param name="argstr">大文字のIとSで"IIS"で(int, int, string )のように引数の数と順序を指定する。</param>
		/// <param name="minArg">引数の最低数。これ以降は省略可能</param>
		/// <returns></returns>
		public static ArgumentBuilder GetNormalArgumentBuilder(string argstr, int minArg)
		{
			if (minArg < 0)
				minArg = argstr.Length;
			string key = argstr + minArg.ToString();
            ArgumentBuilder argbuilder = null;
			if (nargb.TryGetValue(key, out argbuilder))
				return argbuilder;
			Type[] types = new Type[argstr.Length];
			for (int i = 0; i < argstr.Length; i++)
			{
				if (argstr[i] == 'I')
					types[i] = typeof(Int64);
				else if (argstr[i] == 'S')
					types[i] = typeof(string);
				else
					throw new ExeEE("異常な指定");
			}
            argbuilder = new Expressions_ArgumentBuilder(types, minArg);
			nargb.Add(key, argbuilder);
			return argbuilder;
		}
		static ArgumentParser()
		{
			argb[FunctionArgType.METHOD] = new METHOD_ArgumentBuilder();
			argb[FunctionArgType.VOID] = new VOID_ArgumentBuilder();
			argb[FunctionArgType.INT_EXPRESSION] = new INT_EXPRESSION_ArgumentBuilder(false);
			argb[FunctionArgType.INT_EXPRESSION_NULLABLE] = new INT_EXPRESSION_ArgumentBuilder(true);
			argb[FunctionArgType.STR_EXPRESSION] = new STR_EXPRESSION_ArgumentBuilder(false);
            argb[FunctionArgType.STR_EXPRESSION_NULLABLE] = new STR_EXPRESSION_ArgumentBuilder(true);
            argb[FunctionArgType.STR] = new STR_ArgumentBuilder(false);
			argb[FunctionArgType.STR_NULLABLE] = new STR_ArgumentBuilder(true);
			argb[FunctionArgType.FORM_STR] = new FORM_STR_ArgumentBuilder(false);
			argb[FunctionArgType.FORM_STR_NULLABLE] = new FORM_STR_ArgumentBuilder(true);
			argb[FunctionArgType.SP_PRINTV] = new SP_PRINTV_ArgumentBuilder();
			argb[FunctionArgType.SP_TIMES] = new SP_TIMES_ArgumentBuilder();
			argb[FunctionArgType.SP_BAR] = new SP_BAR_ArgumentBuilder();
			argb[FunctionArgType.SP_SET] = new SP_SET_ArgumentBuilder();
			argb[FunctionArgType.SP_SETS] = new SP_SET_ArgumentBuilder();
			argb[FunctionArgType.SP_SWAP] = new SP_SWAP_ArgumentBuilder(false);
			argb[FunctionArgType.SP_VAR] = new SP_VAR_ArgumentBuilder();
			argb[FunctionArgType.SP_SAVEDATA] = new SP_SAVEDATA_ArgumentBuilder();
            argb[FunctionArgType.SP_TINPUT] = new SP_TINPUT_ArgumentBuilder();
            argb[FunctionArgType.SP_TINPUTS] = new SP_TINPUTS_ArgumentBuilder();
			argb[FunctionArgType.SP_SORTCHARA] = new SP_SORTCHARA_ArgumentBuilder();
			argb[FunctionArgType.SP_CALL] = new SP_CALL_ArgumentBuilder(false, false);
			argb[FunctionArgType.SP_CALLF] = new SP_CALL_ArgumentBuilder(true, false);
			argb[FunctionArgType.SP_CALLFORM] = new SP_CALL_ArgumentBuilder(false, true);
			argb[FunctionArgType.SP_CALLFORMF] = new SP_CALL_ArgumentBuilder(true, true);
			argb[FunctionArgType.SP_FOR_NEXT] = new SP_FOR_NEXT_ArgumentBuilder();
			argb[FunctionArgType.SP_POWER] = new SP_POWER_ArgumentBuilder();
			argb[FunctionArgType.SP_SWAPVAR] = new SP_SWAPVAR_ArgumentBuilder();
			argb[FunctionArgType.EXPRESSION] = new EXPRESSION_ArgumentBuilder(false);
			argb[FunctionArgType.EXPRESSION_NULLABLE] = new EXPRESSION_ArgumentBuilder(true);
			argb[FunctionArgType.CASE] = new CASE_ArgumentBuilder();
			argb[FunctionArgType.VAR_INT] = new VAR_INT_ArgumentBuilder();
            argb[FunctionArgType.VAR_STR] = new VAR_STR_ArgumentBuilder();
			argb[FunctionArgType.BIT_ARG] = new BIT_ARG_ArgumentBuilder();
			argb[FunctionArgType.SP_VAR_SET] = new SP_VAR_SET_ArgumentBuilder();
			argb[FunctionArgType.SP_BUTTON] = new SP_BUTTON_ArgumentBuilder();
			argb[FunctionArgType.SP_COLOR] = new SP_COLOR_ArgumentBuilder();
			argb[FunctionArgType.SP_SPLIT] = new SP_SPLIT_ArgumentBuilder();
			argb[FunctionArgType.SP_GETINT] = new SP_GETINT_ArgumentBuilder();
			argb[FunctionArgType.SP_CVAR_SET] = new SP_CVAR_SET_ArgumentBuilder();
			argb[FunctionArgType.SP_CONTROL_ARRAY] = new SP_CONTROL_ARRAY_ArgumentBuilder();
			argb[FunctionArgType.SP_SHIFT_ARRAY] = new SP_SHIFT_ARRAY_ArgumentBuilder();
            argb[FunctionArgType.SP_SORTARRAY] = new SP_SORT_ARRAY_ArgumentBuilder();
			argb[FunctionArgType.INT_ANY] = new INT_ANY_ArgumentBuilder();
			argb[FunctionArgType.FORM_STR_ANY] = new FORM_STR_ANY_ArgumentBuilder();
            argb[FunctionArgType.SP_COPYCHARA] = new SP_SWAP_ArgumentBuilder(true);
            argb[FunctionArgType.SP_INPUT] = new SP_INPUT_ArgumentBuilder();
			argb[FunctionArgType.SP_INPUTS] = new SP_INPUTS_ArgumentBuilder();
            argb[FunctionArgType.SP_COPY_ARRAY] = new SP_COPY_ARRAY_Arguments();
			argb[FunctionArgType.SP_SAVEVAR] = new SP_SAVEVAR_ArgumentBuilder();
			argb[FunctionArgType.SP_SAVECHARA] = new SP_SAVECHARA_ArgumentBuilder();
			argb[FunctionArgType.SP_REF] = new SP_REF_ArgumentBuilder(false);
			argb[FunctionArgType.SP_REFBYNAME] = new SP_REF_ArgumentBuilder(true);
			argb[FunctionArgType.SP_HTMLSPLIT] = new SP_HTMLSPLIT_ArgumentBuilder();
			
        }
		
		private sealed class SP_PRINTV_ArgumentBuilder : ArgumentBuilder
		{
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				StringStream st = line.PopArgumentPrimitive();
				WordCollection wc = LexicalAnalyzer.Analyse(st, LexEndWith.EoL, LexAnalyzeFlag.AnalyzePrintV);
				IOperandTerm[] args = ExpressionParser.ReduceArguments(wc, ArgsEndWith.EoL, false);
				for(int i = 0; i< args.Length;i++)
				{
					if(args[i] == null)
						{warn("引数を省略することはできません", line, 2, false); return null;}
					else
						args[i] = args[i].Restructure(exm);
				}
				return new SpPrintVArgument(args);
			}
		}

        private sealed class SP_TIMES_ArgumentBuilder : ArgumentBuilder
		{
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				StringStream st = line.PopArgumentPrimitive();
				WordCollection wc = LexicalAnalyzer.Analyse(st, LexEndWith.Comma, LexAnalyzeFlag.None);
				st.ShiftNext();
				if (st.EOS)
					{warn("引数が足りません", line, 2, false); return null;}
				double d ;
				try
				{
					LexicalAnalyzer.SkipWhiteSpace(st);
					d = LexicalAnalyzer.ReadDouble(st);
					LexicalAnalyzer.SkipWhiteSpace(st);
					if (!st.EOS)
						warn("引数が多すぎます", line, 1, false);
				}
				catch
				{
					warn("第２引数が実数値ではありません（常に0と解釈されます）", line, 1, false);
					d = 0.0;
				}
				IOperandTerm term = ExpressionParser.ReduceExpressionTerm(wc, TermEndWith.EoL);
				if (term == null)
				{ warn("書式が間違っています", line, 2, false); return null; }
				VariableTerm varTerm = term.Restructure(exm) as VariableTerm;
				if (varTerm == null)
				{ warn("第１引数に変数以外を指定することはできません", line, 2, false); return null; }
				else if (varTerm.IsString)
				{ warn("第１引数を文字列変数にすることはできません", line, 2, false); return null; }
				else if (varTerm.Identifier.IsConst)
				{ warn("第１引数に変更できない変数を指定することはできません", line, 2, false); return null; }
				return new SpTimesArgument(varTerm, d);
			}
		}
		
        private sealed class FORM_STR_ANY_ArgumentBuilder : ArgumentBuilder
		{
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				Argument ret = null;
				StringStream st = line.PopArgumentPrimitive();
				List<IOperandTerm> termList = new List<IOperandTerm>();
				LexicalAnalyzer.SkipHalfSpace(st);
				if (st.EOS)
				{
					if (line.FunctionCode == FunctionCode.RETURNFORM)
					{
						termList.Add(new SingleTerm("0"));
						ret = new ExpressionArrayArgument(termList);
						ret.IsConst = true;
						ret.ConstInt = 0;
						return ret;
					}
					warn("引数が設定されていません", line, 2, false);
					return null;
				}
				while (true)
				{
					StrFormWord sfwt = LexicalAnalyzer.AnalyseFormattedString(st, FormStrEndWith.Comma, false);
					IOperandTerm term = ExpressionParser.ToStrFormTerm(sfwt);
					term = term.Restructure(exm);
					termList.Add(term);
					st.ShiftNext();
					if (st.EOS)
						break;
					LexicalAnalyzer.SkipHalfSpace(st);
					if (st.EOS)
					{
					    warn("\',\'の後ろに引数がありません。", line, 1, false);
					    break;
					}
				}
				return new ExpressionArrayArgument(termList);
			}
		}
	
		private sealed class VOID_ArgumentBuilder : ArgumentBuilder
		{
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				StringStream st = line.PopArgumentPrimitive();
				LexicalAnalyzer.SkipWhiteSpace(st);
				if (!st.EOS)
					warn("引数は不要です", line, 1, false);
				return new VoidArgument();
			}
		}

		private sealed class STR_ArgumentBuilder : ArgumentBuilder
		{
			public STR_ArgumentBuilder(bool nullable)
			{
				this.nullable = nullable;
			}

            readonly bool nullable;
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				StringStream st = line.PopArgumentPrimitive();
					string rowStr;
				if (st.EOS)
				{
					if (!nullable)
					{
						warn("引数が設定されていません", line, 2, false);
						return null;
					}
					rowStr = "";
					//1756 処理変更のために完全に見分けが付かなくなってしまった
					//if (line.FunctionCode == FunctionCode.PRINTL)
					//	warn("PRINTLの後ろに空白がありません(eramaker：\'PRINTL\'を表示)", line, 0, true);
				}
				else
					rowStr = st.Substring();
                if (line.FunctionCode == FunctionCode.SETCOLORBYNAME || line.FunctionCode == FunctionCode.SETBGCOLORBYNAME)
				{
                    Color c = Color.FromName(rowStr);
					if (c.A == 0)
					{
						if (rowStr.Equals("transparent", StringComparison.OrdinalIgnoreCase))
							throw new CodeEE("無色透明(Transparent)は色として指定できません");
						throw new CodeEE("指定された色名\"" + rowStr + "\"は無効な色名です");
					}

                }
                Argument ret = new ExpressionArgument(new SingleTerm(rowStr))
                {
                    ConstStr = rowStr,
                    IsConst = true
                };
                return ret;
			}
		}

		private sealed class FORM_STR_ArgumentBuilder : ArgumentBuilder
		{
			public FORM_STR_ArgumentBuilder(bool nullable)
			{
				this.nullable = nullable;
			}

            readonly bool nullable;

			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				StringStream st = line.PopArgumentPrimitive();
				Argument ret;
				if (st.EOS)
				{
					if(!nullable)
					{
						warn("引数が設定されていません", line, 2, false);
						return null;
					}
                    //if (line.FunctionCode == FunctionCode.PRINTFORML)
                    //	warn("PRINTFORMLの後ろに空白がありません(eramaker：\'PRINTFORML\'を表示)", line, 0, true);
                    ret = new ExpressionArgument(new SingleTerm(""))
                    {
                        ConstStr = "",
                        IsConst = true
                    };
                    return ret;
				}
				StrFormWord sfwt = LexicalAnalyzer.AnalyseFormattedString(st, FormStrEndWith.EoL, false);
				IOperandTerm term = ExpressionParser.ToStrFormTerm(sfwt);
				term = term.Restructure(exm);
				ret = new ExpressionArgument(term);
				if(term is SingleTerm)
				{
					ret.ConstStr = term.GetStrValue(exm);
					ret.IsConst = true;
				}
				return ret;
			}
		}

		private sealed class SP_VAR_ArgumentBuilder : ArgumentBuilder
		{
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				StringStream st = line.PopArgumentPrimitive();
                IdentifierWord iw = LexicalAnalyzer.ReadSingleIdentifierWord(st);
                if (iw == null)
                { warn("第１引数を読み取ることができません", line, 2, false); return null; }
				string idStr = iw.Code;
				VariableToken id = GlobalStatic.IdentifierDictionary.GetVariableToken(idStr, null, true);
				if (id == null)
				{ warn("第１引数に変数以外を指定することはできません", line, 2, false); return null; }
				else if ((!id.IsArray1D && !id.IsArray2D && !id.IsArray3D) || (id.Code == VariableCode.RAND))
				{ warn("第１引数に配列でない変数を指定することはできません", line, 2, false); return null; }
				LexicalAnalyzer.SkipWhiteSpace(st);
				if (!st.EOS)
				{
					warn("引数の後に余分な文字があります", line, 1, false);
				}
				return new SpVarsizeArgument(id);
			}
		}

		private sealed class SP_SORTCHARA_ArgumentBuilder : ArgumentBuilder
		{
			
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				VariableTerm varTerm = new VariableTerm(GlobalStatic.VariableData.GetSystemVariableToken("NO"), new IOperandTerm[] { new SingleTerm(0) });
				SortOrder order = SortOrder.ASCENDING;
				WordCollection wc = popWords(line);
				IdentifierWord id = wc.Current as IdentifierWord;
				if (wc.EOL)
				{
					return new SpSortcharaArgument(varTerm, order);
				}
				if ((id != null) && (id.Code.Equals("FORWARD", Config.SCVariable)
					|| (id.Code.Equals("BACK", Config.SCVariable))))
				{
					if (id.Code.Equals("BACK", Config.SCVariable))
						order = SortOrder.DESENDING;
					wc.ShiftNext();
					if (!wc.EOL)
						warn("引数が多すぎます", line, 1, false);
				}
				else
				{
					IOperandTerm term = ExpressionParser.ReduceExpressionTerm(wc, TermEndWith.Comma);
					if (term == null)
					{ warn("書式が間違っています", line, 2, false); return null; }
					varTerm = term.Restructure(exm) as VariableTerm;
					if (varTerm == null)
					{ warn("第１引数に変数以外を指定することはできません", line, 2, false); return null; }
					else if (!varTerm.Identifier.IsCharacterData)
					{ warn("第１引数はキャラクタ変数でなければなりません", line, 2, false); return null; }
					wc.ShiftNext();
					if (!wc.EOL)
					{
						id = wc.Current as IdentifierWord;
						if ((id != null) && (id.Code.Equals("FORWARD", Config.SCVariable)
							|| (id.Code.Equals("BACK", Config.SCVariable))))
						{
							if (id.Code.Equals("BACK", Config.SCVariable))
								order = SortOrder.DESENDING;
							wc.ShiftNext();
							if (!wc.EOL)
								warn("引数が多すぎます", line, 1, false);
						}
						else
						{ warn("書式が間違っています", line, 2, false); return null; }
					}
				}
				return new SpSortcharaArgument(varTerm, order);
			}
		}

        private sealed class SP_SORT_ARRAY_ArgumentBuilder : ArgumentBuilder
        {
            public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
            {
                SortOrder order = SortOrder.ASCENDING;
                WordCollection wc = popWords(line);
                IOperandTerm term3 = new SingleTerm(0);
                IOperandTerm term4 = null;

                if (wc.EOL)
                {
                    warn("書式が間違っています", line, 2, false); return null;
                }

                VariableTerm varTerm;
                IOperandTerm term = ExpressionParser.ReduceExpressionTerm(wc, TermEndWith.Comma);
                if (term == null)
                { warn("書式が間違っています", line, 2, false); return null; }
                varTerm = term.Restructure(exm) as VariableTerm;
                if (varTerm == null)
                { warn("第１引数に変数以外を指定することはできません", line, 2, false); return null; }
                else if (varTerm.Identifier.IsConst)
				{ warn("第１引数が変更できない変数です", line, 2, false); return null; }
                if (!varTerm.Identifier.IsArray1D)
                { warn("第１引数に１次元配列もしくは配列型キャラクタ変数以外を指定することはできません", line, 2, false); return null; }

                wc.ShiftNext();
                IdentifierWord id = wc.Current as IdentifierWord;

                if ((id != null) && (id.Code.Equals("FORWARD", Config.SCVariable) || (id.Code.Equals("BACK", Config.SCVariable))))
                {
                    if (id.Code.Equals("BACK", Config.SCVariable))
                        order = SortOrder.DESENDING;
                    wc.ShiftNext();
                }
                else if (id != null)
                { warn("第２引数にソート方法指定子（FORWARD or BACK）以外が指定されています", line, 2, false); return null; }

                if (id != null)
                {
                    wc.ShiftNext();
                    if (!wc.EOL)
                    {
                        term3 = ExpressionParser.ReduceExpressionTerm(wc, TermEndWith.Comma);
                        if (term3 == null)
                        { warn("第３引数が解釈出来ません", line, 2, false); return null; }
                        if (!term3.IsInteger)
                        { warn("第３引数が数値ではありません", line, 2, false); return null; }
                        wc.ShiftNext();
                        if (!wc.EOL)
                        {
                            term4 = ExpressionParser.ReduceExpressionTerm(wc, TermEndWith.Comma);
                            if (term4 == null)
                            { warn("第４引数が解釈出来ません", line, 2, false); return null; }
                            if (!term4.IsInteger)
                            { warn("第４引数が数値ではありません", line, 2, false); return null; }
                            wc.ShiftNext();
                            if (!wc.EOL)
                                warn("引数が多すぎます", line, 1, false);
                        }
                    }
                }
                return new SpArraySortArgument(varTerm, order, term3, term4);
            }
        }

		private sealed class SP_CALL_ArgumentBuilder : ArgumentBuilder
		{
			public SP_CALL_ArgumentBuilder(bool callf, bool form)
			{
				this.form = form;
				this.callf = callf;
			}

            readonly bool form;
            readonly bool callf;
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				StringStream st = line.PopArgumentPrimitive();
				IOperandTerm funcname;
				if (form)
				{
					StrFormWord sfw = LexicalAnalyzer.AnalyseFormattedString(st, FormStrEndWith.LeftParenthesis_Bracket_Comma_Semicolon, true);
					funcname = ExpressionParser.ToStrFormTerm(sfw);
					funcname = funcname.Restructure(exm);
				}
				else
				{
					string str = LexicalAnalyzer.ReadString(st, StrEndWith.LeftParenthesis_Bracket_Comma_Semicolon);
					str = str.Trim(new char[] { ' ', '\t' });
					funcname = new SingleTerm(str);
				}
				char cur = st.Current;
				WordCollection wc = LexicalAnalyzer.Analyse(st, LexEndWith.EoL, LexAnalyzeFlag.None);
				wc.ShiftNext();

				IOperandTerm[] subNames = null;
				IOperandTerm[] args = null;
                if (cur == '[')
                {
                    subNames = ExpressionParser.ReduceArguments(wc, ArgsEndWith.RightBracket, false);
                    if (!wc.EOL)
                    {
                        if (wc.Current.Type != '(')
                        wc.ShiftNext();
                        args = ExpressionParser.ReduceArguments(wc, ArgsEndWith.RightParenthesis, false);
                    }
                }
				if ((cur == '(') || (cur == ','))
				{
					if (cur == '(')
						args = ExpressionParser.ReduceArguments(wc, ArgsEndWith.RightParenthesis, false);
					else
						args = ExpressionParser.ReduceArguments(wc, ArgsEndWith.EoL, false);
					if (!wc.EOL)
					{ warn("書式が間違っています", line, 2, false); return null; }
				}
				if (subNames == null)
					subNames = new IOperandTerm[0];
				if (args == null)
					args = new IOperandTerm[0];
				for(int i = 0; i < subNames.Length; i++)
					if (subNames != null)
						subNames[i] = subNames[i].Restructure(exm);
				for(int i = 0; i < args.Length; i++)
					if (args[i] != null)
						args[i] = args[i].Restructure(exm);
				Argument ret;
				if(callf)
					ret = new SpCallFArgment(funcname, subNames, args);
				else
					ret = new SpCallArgment(funcname, subNames, args);
                if (funcname is SingleTerm)
                {
                    ret.IsConst = true;
                    ret.ConstStr = funcname.GetStrValue(null);
                    if (ret.ConstStr == "")
                    {
                        warn("関数名が指定されていません", line, 2, false);
                        return null;
                    }
                }
				return ret;
			}
		}

		private sealed class CASE_ArgumentBuilder : ArgumentBuilder
		{
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				WordCollection wc = popWords(line);
				CaseExpression[] args = ExpressionParser.ReduceCaseExpressions(wc);
				if ((!wc.EOL) || (args.Length == 0))
				{ warn("書式が間違っています", line, 2, false); return null; }
				for(int i = 0; i < args.Length; i++)
					args[i].Reduce(exm);
				return new CaseArgument(args);
			}
		}
		
		private sealed class SP_SET_ArgumentBuilder : ArgumentBuilder
		{
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm) 
			{
				WordCollection destWc = line.PopAssignmentDestStr();
				IOperandTerm[] destTerms = ExpressionParser.ReduceArguments(destWc, ArgsEndWith.EoL, false);
				SpSetArgument ret;
				if ((destTerms.Length == 0) || (destTerms[0] == null))
				{ assignwarn("代入文の左辺の読み取りに失敗しました", line, 2, false); return null; }
				if (destTerms.Length != 1)
					{assignwarn("代入文の左辺に余分な','があります", line, 2, false); return null;}
                if (!(destTerms[0] is VariableTerm varTerm))
                {//
                    assignwarn("代入文の左辺に変数以外を指定することはできません", line, 2, false);
                    return null;
                }
                else if (varTerm.Identifier.IsConst)
                {
                    assignwarn("代入文の左辺に変更できない変数を指定することはできません", line, 2, false);
                    return null;
                }
                varTerm.Restructure(exm);
				StringStream st = line.PopArgumentPrimitive();
                if (st == null)
                    st = new StringStream("");
                OperatorCode op = line.AssignOperator;
				IOperandTerm src;
				if(varTerm.IsInteger)
				{
					if (op == OperatorCode.AssignmentStr)
						{ assignwarn("整数型の代入に演算子"+OperatorManager.ToOperatorString(op) + "は使用できません", line, 2, false); return null; }
					if((op == OperatorCode.Increment)||(op == OperatorCode.Decrement))
					{
						LexicalAnalyzer.SkipWhiteSpace(st);
						if (!st.EOS)
						{
							if (op == OperatorCode.Increment)
								{assignwarn("インクリメント行でインクリメント以外の処理が定義されています", line, 2, false);return null;}
							else
								{assignwarn("デクリメント行でデクリメント以外の処理が定義されています", line, 2, false);return null;}
						}
						ret = new SpSetArgument(varTerm, null)
						{
							IsConst = true,
							ConstInt = op == OperatorCode.Increment ? 1 : -1,
							AddConst = true
                        };
						return ret;
					}
					WordCollection srcWc = LexicalAnalyzer.Analyse(st, LexEndWith.EoL, LexAnalyzeFlag.None);
					IOperandTerm[] srcTerms = ExpressionParser.ReduceArguments(srcWc, ArgsEndWith.EoL, false);
					
					if ((srcTerms.Length == 0) || (srcTerms[0] == null))
						{assignwarn("代入文の右辺の読み取りに失敗しました", line, 2, false); return null;}
					if (srcTerms.Length != 1)
					{
						if(op != OperatorCode.Assignment)
						{assignwarn("複合代入演算では右辺に複数の値を含めることはできません", line, 2, false); return null;}
						bool allConst = true;
						Int64[] constValues = new Int64[srcTerms.Length];
						for (int i = 0; i < srcTerms.Length; i++)
						{
							if (srcTerms[i] == null)
							{ assignwarn("代入式の右辺の値は省略できません", line, 2, false); return null; }
							if (!srcTerms[i].IsInteger)
							{ assignwarn("数値型変数に文字列は代入できません", line, 2, false); return null; }
							srcTerms[i] = srcTerms[i].Restructure(exm);
							if (allConst && (srcTerms[i] is SingleTerm))
								constValues[i] = srcTerms[i].GetIntValue(null);
							else
								allConst = false;
						}
                        SpSetArrayArgument arrayarg = new SpSetArrayArgument(varTerm, srcTerms, constValues)
                        {
                            IsConst = allConst
                        };
                        return arrayarg;
					}
					if(!srcTerms[0].IsInteger)
						{assignwarn("数値型変数に文字列は代入できません", line, 2, false); return null;}
					src = srcTerms[0].Restructure(exm);
					if(op == OperatorCode.Assignment)
					{
						ret = new SpSetArgument(varTerm, src);
						if(src is SingleTerm)
						{
							ret.IsConst = true;
							ret.AddConst = false;
							ret.ConstInt = src.GetIntValue(null);
						}
						return ret;
					}
					if((op == OperatorCode.Plus)||(op == OperatorCode.Minus))
					{
						if(src is SingleTerm)
						{
                            ret = new SpSetArgument(varTerm, null)
                            {
                                IsConst = true,
								ConstInt = op == OperatorCode.Plus ? src.GetIntValue(null) : -src.GetIntValue(null),
								AddConst = true
                            };
							return ret;
						}
					}
					src = OperatorMethodManager.ReduceBinaryTerm(op,varTerm, src);
					return new SpSetArgument(varTerm, src);
					
				}
				else
				{
					if (op == OperatorCode.Assignment)
					{
						if (Config.SystemIgnoreStringSet)
						{ assignwarn("文字列代入は禁止されています（'=を用いるかコンフィグオプションを変えてください)", line, 2, false); return null; }
						LexicalAnalyzer.SkipHalfSpace(st);//文字列の代入なら半角スペースだけを読み飛ばす
						//eramakerは代入文では妙なTrim()をする。半端にしか再現できないがとりあえずtrim = true
						StrFormWord sfwt = LexicalAnalyzer.AnalyseFormattedString(st, FormStrEndWith.EoL, true);
						IOperandTerm term = ExpressionParser.ToStrFormTerm(sfwt);
						src = term.Restructure(exm);
						ret = new SpSetArgument(varTerm, src);
						if (src is SingleTerm)
						{
							ret.IsConst = true;
							ret.AddConst = false;
							ret.ConstStr = src.GetStrValue(null);
						}
						return ret;
					}
					else if ((op == OperatorCode.Mult)||(op == OperatorCode.Plus)||(op == OperatorCode.AssignmentStr))
					{
						WordCollection srcWc = LexicalAnalyzer.Analyse(st, LexEndWith.EoL, LexAnalyzeFlag.None);
						IOperandTerm[] srcTerms = ExpressionParser.ReduceArguments(srcWc, ArgsEndWith.EoL, false);
						
						if ((srcTerms.Length == 0) || (srcTerms[0] == null))
							{assignwarn("代入文の右辺の読み取りに失敗しました", line, 2, false); return null;}
						if (op == OperatorCode.AssignmentStr)
						{
							if (srcTerms.Length == 1)
							{
								if (srcTerms[0].IsInteger)
								{ assignwarn("文字列変数に数値型は代入できません", line, 2, false); return null; }
								src = srcTerms[0].Restructure(exm);
								ret = new SpSetArgument(varTerm, src);
								if (src is SingleTerm)
								{
									ret.IsConst = true;
									ret.AddConst = false;
									ret.ConstStr = src.GetStrValue(null);
								}
								return ret;
							}
							bool allConst = true;
							string[] constValues = new string[srcTerms.Length];
							for (int i = 0; i < srcTerms.Length; i++)
							{
								if (srcTerms[i] == null)
								{ assignwarn("代入式の右辺の値は省略できません", line, 2, false); return null; }
								if (srcTerms[i].IsInteger)
								{ assignwarn("文字列変数に数値型は代入できません", line, 2, false); return null; }
								srcTerms[i] = srcTerms[i].Restructure(exm);
								if (allConst && (srcTerms[i] is SingleTerm))
									constValues[i] = srcTerms[i].GetStrValue(null);
								else
									allConst = false;
							}
                            SpSetArrayArgument arrayarg = new SpSetArrayArgument(varTerm, srcTerms, constValues)
                            {
                                IsConst = allConst
                            };
                            return arrayarg;
						}
						if (srcTerms.Length != 1)
						{ assignwarn("代入文の右辺に余分な','があります", line, 2, false); return null; }
							
						src = srcTerms[0].Restructure(exm);
						src = OperatorMethodManager.ReduceBinaryTerm(op, varTerm, src);
						return new SpSetArgument(varTerm, src);
					}
					assignwarn("代入式に使用できない演算子が使われました", line, 2, false);
					return null;
				}
			}
		}
				
		private sealed class METHOD_ArgumentBuilder : ArgumentBuilder
		{
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] args = popTerms(line);
				string errmes = line.Function.Method.CheckArgumentType(line.Function.Name, args);
				if (errmes != null)
					throw new CodeEE(errmes);
				IOperandTerm mTerm = new FunctionMethodTerm(line.Function.Method, args);
				return new MethodArgument(mTerm.Restructure(exm));
			}
		}

        private sealed class SP_INPUTS_ArgumentBuilder : ArgumentBuilder
        {
            public SP_INPUTS_ArgumentBuilder()
            {
                argumentTypeArray = new Type[] { typeof(string) };
                //if (nullable)妥協
                minArg = 0;
            }
            public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
            {
                StringStream st = line.PopArgumentPrimitive();
                Argument ret;
                if (st.EOS)
                {
                    ret = new ExpressionArgument(null);
                    return ret;
                }
                StrFormWord sfwt = LexicalAnalyzer.AnalyseFormattedString(st, FormStrEndWith.EoL, false);
                if (!st.EOS)
                {
                    warn("引数が多すぎます", line, 1, false);
                }
                IOperandTerm term = ExpressionParser.ToStrFormTerm(sfwt);
                term = term.Restructure(exm);
                ret = new ExpressionArgument(term);
                if (term is SingleTerm)
                {
                    ret.ConstStr = term.GetStrValue(exm);
                    if (line.FunctionCode == FunctionCode.ONEINPUTS)
                    {
                        if (string.IsNullOrEmpty(ret.ConstStr))
                        {
                            warn("引数が空文字列なため、引数は無視されます", line, 1, false);
                            return new ExpressionArgument(null);
                        }
                        else if (ret.ConstStr.Length > 1)
                        {
                            warn("ONEINPUTSの引数に２文字以上の文字列が渡されています（２文字目以降は無視されます）", line, 1, false);
                            ret.ConstStr = ret.ConstStr.Remove(1);
                        }
                    }
                    ret.IsConst = true;
                }
                return ret;
            }
        }
        
		#region 正規型 popTerms()とcheckArgumentType()を両方行うもの。考えることは最低限でよい。

		private sealed class INT_EXPRESSION_ArgumentBuilder : ArgumentBuilder
		{
			public INT_EXPRESSION_ArgumentBuilder(bool nullable)
			{
				argumentTypeArray = new Type[] { typeof(Int64) };
				//if (nullable)妥協
				minArg = 0;
				this.nullable = nullable;
			}

            readonly bool nullable;

			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;
				IOperandTerm term;
				if (terms.Length == 0)
				{
					term = new SingleTerm(0);
					if (!nullable)
					{
						if (line.Function.IsExtended())
							warn("省略できない引数が省略されています。Emueraは0を補います", line, 1, false);
						else
							warn("省略できない引数が省略されています。Emueraは0を補いますがeramakerの動作は不定です", line, 1, false);
					}
				}
				else
				{
					term = terms[0];
				}
				
				if (line.FunctionCode == FunctionCode.REPEAT)
				{
					if ((term is SingleTerm) && (term.GetIntValue(null) <= 0L))
					{
						warn("0回以下のREPEATです。(eramakerではエラーになります)", line, 0, true);
					}
					VariableToken count = GlobalStatic.VariableData.GetSystemVariableToken("COUNT");
					VariableTerm repCount = new VariableTerm(count, new IOperandTerm[] { new SingleTerm(0) });
					repCount.Restructure(exm);
					return new SpForNextArgment(repCount, new SingleTerm(0), term, new SingleTerm(1));
				}
				ExpressionArgument ret = new ExpressionArgument(term);
				if (term is SingleTerm)
				{
					Int64 i = term.GetIntValue(null);
					ret.ConstInt = i;
					ret.IsConst = true;
					if (line.FunctionCode == FunctionCode.CLEARLINE)
					{
						if (i <= 0L)
							warn("引数に0以下の値が渡されています(この行は何もしません)", line, 1, false);
					}
					else if (line.FunctionCode == FunctionCode.FONTSTYLE)
					{
						if (i < 0L)
							warn("引数に負の値が渡されています(結果は不定です)", line, 1, false);
					}
				}
				return ret;
			}
		}

		private sealed class INT_ANY_ArgumentBuilder : ArgumentBuilder
		{
			public INT_ANY_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(Int64) };
				minArg = 0;
				argAny = true;
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;

				List<IOperandTerm> termList = new List<IOperandTerm>();
				termList.AddRange(terms);
				ExpressionArrayArgument ret = new ExpressionArrayArgument(termList);
				if (terms.Length == 0)
				{
					if (line.FunctionCode == FunctionCode.RETURN)
					{
						termList.Add(new SingleTerm(0));
						ret.IsConst = true;
						ret.ConstInt = 0;
						return ret;
					}
					warn("引数が設定されていません", line, 2, false);
					return null;
				}
                else if (terms.Length == 1)
                {
                    if (terms[0] is SingleTerm s)
                    {
                        ret.IsConst = true;
                        ret.ConstInt = s.Int;
                        return ret;
                    }
                    else if (line.FunctionCode == FunctionCode.RETURN)
                    {
                        //定数式は定数化してしまうので現行システムでは見つけられない
                        if (terms[0] is VariableTerm)
                            warn("RETURNの引数に変数が渡されています(eramaker：常に0を返します)", line, 0, true);
                        else
                            warn("RETURNの引数に数式が渡されています(eramaker：Emueraとは異なる値を返します)", line, 0, true);
                    }
                }
                else
                {
                    warn(line.Function.Name + "の引数に複数の値が与えられています(eramaker：非対応です)", line, 0, true);
                }
				return ret;
			}
		}
		
		private sealed class STR_EXPRESSION_ArgumentBuilder : ArgumentBuilder
		{
			public STR_EXPRESSION_ArgumentBuilder(bool nullable)
			{
				argumentTypeArray = new Type[] { typeof(string) };
				if (nullable)
					minArg = 0;
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;
				ExpressionArgument ret;
				if (terms.Length == 0)
				{
                    ret = new ExpressionArgument(new SingleTerm(""))
                    {
                        ConstStr = "",
                        IsConst = true
                    };
                    return ret;
				}
				return new ExpressionArgument(terms[0]);
			}
		}

		private sealed class EXPRESSION_ArgumentBuilder : ArgumentBuilder
		{
			public EXPRESSION_ArgumentBuilder(bool nullable)
			{
				argumentTypeArray = new Type[] { typeof(void) };
				if (nullable)
					minArg = 0;
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;
				if (terms.Length == 0)
				{
                    ExpressionArgument ret = new ExpressionArgument(null)
                    {
                        ConstStr = "",
                        ConstInt = 0,
                        IsConst = true
                    };
                    return ret;
				}
				return new ExpressionArgument(terms[0]);
			}
		}

		private sealed class SP_BAR_ArgumentBuilder : ArgumentBuilder
		{
			public SP_BAR_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(Int64), typeof(Int64), typeof(Int64) };
				//minArg = 3;
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;
				return new SpBarArgument(terms[0], terms[1], terms[2]);
			}
		}

		private sealed class SP_SWAP_ArgumentBuilder : ArgumentBuilder
		{
            //emuera1803beta2+v1 第2引数省略型に対応
			public SP_SWAP_ArgumentBuilder(bool nullable)
			{
				argumentTypeArray = new Type[] { typeof(Int64), typeof(Int64) };
                if (nullable)
                    minArg = 1;
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;
                //上の判定で省略不可時はここに来ないので即さばける
                if (terms.Length == 1)
                    terms = new IOperandTerm[] { terms[0], null };
				return new SpSwapCharaArgument(terms[0], terms[1]);
			}
		}

        private sealed class SP_SAVEDATA_ArgumentBuilder : ArgumentBuilder
		{
			public SP_SAVEDATA_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(Int64), typeof(string) };
			}

			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;
				return new SpSaveDataArgument(terms[0], terms[1]);
			}
		}

        private sealed class SP_TINPUT_ArgumentBuilder : ArgumentBuilder
        {
            public SP_TINPUT_ArgumentBuilder()
            {
                argumentTypeArray = new Type[] { typeof(Int64), typeof(Int64), typeof(Int64), typeof(string) };
                minArg = 2;
            }
            public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
            {
                IOperandTerm[] terms = popTerms(line);
                IOperandTerm term3 = null, term4 = null;
                if (!checkArgumentType(line, exm, terms))
                    return null;
                if (terms.Length > 2)
                    term3 = terms[2];
                if (terms.Length > 3)
                    term4 = terms[3];

                return new SpTInputsArgument(terms[0], terms[1], term3, term4);
            }
        }
        
        private sealed class SP_TINPUTS_ArgumentBuilder : ArgumentBuilder
		{
			public SP_TINPUTS_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(Int64), typeof(string), typeof(Int64), typeof(string) };
				minArg = 2;
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
                IOperandTerm term3 = null, term4 = null;
                if (!checkArgumentType(line, exm, terms))
					return null;
                if (terms.Length > 2)
                    term3 = terms[2];
                if (terms.Length > 3)
                    term4 = terms[3];
                return new SpTInputsArgument(terms[0], terms[1], term3, term4);
			}
		}

		private sealed class SP_FOR_NEXT_ArgumentBuilder : ArgumentBuilder
		{
			public SP_FOR_NEXT_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(Int64), null, typeof(Int64), typeof(Int64) };
				minArg = 3;
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;
				VariableTerm varTerm = getChangeableVariable(terms, 1, line);
				if (varTerm == null)
					return null;
				if (varTerm.Identifier.IsCharacterData)
				{ warn("第1引数にキャラクタ変数を指定することはできません", line, 2, false); return null; }

				IOperandTerm start = terms[1];
				IOperandTerm end = terms[2];
				IOperandTerm step;
				if (start == null)
					start = new SingleTerm(0);
				if ((terms.Length > 3) && (terms[3] != null))
					step = terms[3];
				else
					step = new SingleTerm(1);
				if (!start.IsInteger)
				{ warn("第2引数の型が違います", line, 2, false); return null; }
				return new SpForNextArgment(varTerm, start, end, step);
			}
		}

		private sealed class SP_POWER_ArgumentBuilder : ArgumentBuilder
		{
			public SP_POWER_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(Int64), typeof(Int64), typeof(Int64) };
				//minArg = 2;
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;
				VariableTerm varTerm = getChangeableVariable(terms, 1, line);
				if (varTerm == null)
					return null;

				return new SpPowerArgument(varTerm, terms[1], terms[2]);
			}
		}

        private sealed class SP_SWAPVAR_ArgumentBuilder : ArgumentBuilder
		{
			public SP_SWAPVAR_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(void), typeof(void) };
				//minArg = 2;
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;
				VariableTerm x = getChangeableVariable(terms, 1, line);
				if (x == null)
					return null;
				VariableTerm y = getChangeableVariable(terms, 2, line);
				if (y == null)
					return null;
				if (x.GetOperandType() != y.GetOperandType())
				{
					warn("引数の型が異なります", line, 2, false);
					return null;
				}
				return new SpSwapVarArgument(x, y);
			}
		}
		
		private sealed class VAR_INT_ArgumentBuilder : ArgumentBuilder
		{
			public VAR_INT_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(Int64) };
				minArg = 0;
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (terms.Length == 0)
					return new PrintDataArgument(null);
				if (!checkArgumentType(line, exm, terms))
					return null;
				VariableTerm varTerm = getChangeableVariable(terms, 1, line);
				if (varTerm == null)
					return null;
				return new PrintDataArgument(varTerm);
			}
		}

        private sealed class VAR_STR_ArgumentBuilder : ArgumentBuilder
        {
            public VAR_STR_ArgumentBuilder()
            {
                argumentTypeArray = new Type[] { typeof(string) };
                minArg = 0;
            }
            public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
            {
                IOperandTerm[] terms = popTerms(line);
                if (terms.Length == 0)
                {
                    VariableToken varToken = GlobalStatic.VariableData.GetSystemVariableToken("RESULTS");
                    VariableTerm varTerm = new VariableTerm(varToken, new IOperandTerm[] { new SingleTerm(0) });
                    return new StrDataArgument(varTerm);
                }
                if (!checkArgumentType(line, exm, terms))
                    return null;
                VariableTerm x = getChangeableVariable(terms, 1, line);
                if (x == null)
                    return null;
                return new StrDataArgument(x);
            }
        }

		private sealed class BIT_ARG_ArgumentBuilder : ArgumentBuilder
		{
			public BIT_ARG_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(Int64), typeof(Int64) };
				minArg = 2;
                argAny = true;
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;
                VariableTerm varTerm = getChangeableVariable(terms, 1, line);
				if (varTerm == null)
					return null;
                List<IOperandTerm> termList = new List<IOperandTerm>();
                termList.AddRange(terms);
                //最初の項はいらない
                termList.RemoveAt(0);
				BitArgument ret = new BitArgument(varTerm, termList.ToArray());
                for (int i = 0; i < termList.Count; i++)
                {
                    if (termList[i] is SingleTerm term)
                    {
                        Int64 bit = term.Int;
                        if ((bit < 0) || (bit > 63))
                        {
                            warn("第" + Strings.StrConv((i + 2).ToString(), VbStrConv.Wide, Config.Language) + "引数(" + bit.ToString() + ")が範囲(０～６３)を超えています", line, 2, false);
                            return null;
                        }
                    }
                }
				return ret;
			}
		}

		private sealed class SP_VAR_SET_ArgumentBuilder : ArgumentBuilder
		{
			public SP_VAR_SET_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(void), typeof(void), typeof(Int64), typeof(Int64) };
				minArg = 1;
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;
				VariableTerm varTerm = getChangeableVariable(terms, 1, line);
				if (varTerm == null)
					return null;
                if (varTerm.Identifier.IsConst)
                {
					warn("値を変更できない変数" + varTerm.Identifier.Name + "が指定されました", line, 2, false);
                    return null;
                }

				IOperandTerm term, term3 = null, term4 = null;
				if (terms.Length > 1)
					term = terms[1];
				else
				{
					if (varTerm.IsString)
						term = new SingleTerm("");
					else
						term = new SingleTerm(0);
				}
				if (varTerm is VariableNoArgTerm)
				{
					if (terms.Length > 2)
					{
						warn("対象となる変数" + varTerm.Identifier.Name + "の要素を省略する場合には第3引数以降を設定できません", line, 2, false);
						return null;
					}
					return new SpVarSetArgument(new FixedVariableTerm(varTerm.Identifier), term, null, null);
				}
				if (terms.Length > 2)
					term3 = terms[2];
				if (terms.Length > 3)
					term4 = terms[3];
				if (terms.Length >= 3 && !varTerm.Identifier.IsArray1D)
					warn("第３引数以降は1次元配列以外では無視されます", line, 1, false);
				if (term.GetOperandType() != varTerm.GetOperandType())
				{
					warn("２つの引数の型が一致していません", line, 2, false);
					return null;
				}
				return new SpVarSetArgument(varTerm, term, term3, term4);
			}
		}

		private sealed class SP_CVAR_SET_ArgumentBuilder : ArgumentBuilder
		{
			public SP_CVAR_SET_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(void), typeof(void), typeof(void), typeof(Int64), typeof(Int64) };
				minArg = 1;
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;

				VariableTerm varTerm = getChangeableVariable(terms, 1, line);
				if (varTerm == null)
					return null;
				if (!varTerm.Identifier.IsCharacterData)
				{ warn("第１引数にキャラクタ変数以外の変数を指定することはできません", line, 2, false); return null; }
				//1803beta004 暫定CDFLAGを弾く
				if (varTerm.Identifier.IsArray2D)
				{ warn("第１引数に二次元配列の変数を指定することはできません", line, 2, false); return null; }
				IOperandTerm index, term, term4 = null, term5 = null;
				if (terms.Length > 1)
					index = terms[1];
				else
					index = new SingleTerm(0);
				if (terms.Length > 2)
					term = terms[2];
				else
				{
					if (varTerm.IsString)
						term = new SingleTerm("");
					else
						term = new SingleTerm(0);
				}
				if (terms.Length > 3)
					term4 = terms[3];
				if (terms.Length > 4)
					term5 = terms[4];
				if (index is SingleTerm term1 && index.GetOperandType() == typeof(string) && varTerm.Identifier.IsArray1D)
				{
					if (!GlobalStatic.ConstantData.isDefined(varTerm.Identifier.Code, term1.Str))
					{ warn("文字列" + index.GetStrValue(null) + "は変数" + varTerm.Identifier.Name + "の要素ではありません", line, 2, false); return null; }
				}
				if (terms.Length > 3 && !varTerm.Identifier.IsArray1D)
					warn("第４引数以降は1次元配列以外では無視されます", line, 1, false);
				if (term.GetOperandType() != varTerm.GetOperandType())
				{
					warn("２つの引数の型が一致していません", line, 2, false);
					return null;
				}
				return new SpCVarSetArgument(varTerm, index, term, term4, term5);
			}
		}

		private sealed class SP_BUTTON_ArgumentBuilder : ArgumentBuilder
		{
			public SP_BUTTON_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(string), typeof(void) };
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;
				return new SpButtonArgument(terms[0], terms[1]);
			}
		}

		private sealed class SP_COLOR_ArgumentBuilder : ArgumentBuilder
		{
			public SP_COLOR_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(Int64), typeof(Int64), typeof(Int64) };
				minArg = 1;
			}

			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;
                if (terms.Length == 2)
                { warn("SETCOLORの引数の数が不正です(SETCOLORの引数は1個もしくは3個です)", line, 2, false); return null; }
                SpColorArgument arg;
                if (terms.Length == 1)
                {
                    arg = new SpColorArgument(terms[0]);
                    if (terms[0] is SingleTerm)
                    {
                        arg.ConstInt = terms[0].GetIntValue(exm);
                        arg.IsConst = true;
                    }
                }
                else
                {
                    arg = new SpColorArgument(terms[0], terms[1], terms[2]);
                    if ((terms[0] is SingleTerm) && (terms[1] is SingleTerm) && (terms[2] is SingleTerm))
                    {
                        arg.ConstInt = (terms[0].GetIntValue(exm) << 16) + (terms[1].GetIntValue(exm) << 8) + (terms[2].GetIntValue(exm));
                        arg.IsConst = true;
                    }
                }
                return arg;
			}
		}

		private sealed class SP_SPLIT_ArgumentBuilder : ArgumentBuilder
		{
			public SP_SPLIT_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(string), typeof(string), typeof(string), typeof(Int64) };
				minArg = 3;
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;
				VariableTerm x = getChangeableVariable(terms, 3, line);
				if (x == null)
					return null;
				if (!x.Identifier.IsArray1D && !x.Identifier.IsArray2D && !x.Identifier.IsArray3D)
				{ warn("第３引数は配列変数でなければなりません", line, 2, false); return null; }
                VariableTerm term = (terms.Length >= 4) ? getChangeableVariable(terms, 4, line) : new VariableTerm(GlobalStatic.VariableData.GetSystemVariableToken("RESULT"), new IOperandTerm[]{new SingleTerm(0)});
				return new SpSplitArgument(terms[0], terms[1], x.Identifier, term);
			}
		}
		
		private sealed class SP_HTMLSPLIT_ArgumentBuilder : ArgumentBuilder
		{
			public SP_HTMLSPLIT_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(string), typeof(string), typeof(Int64) };
				minArg = 1;
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;
				VariableToken destVar;
				VariableTerm destVarTerm = null;
				VariableTerm term = null;
				if (terms.Length >= 2)
					destVarTerm = getChangeableVariable(terms, 2, line);
				if (destVarTerm != null)
					destVar = destVarTerm.Identifier;
				else
					destVar = GlobalStatic.VariableData.GetSystemVariableToken("RESULTS");
				if (!destVar.IsArray1D || destVar.IsCharacterData)
				{ warn("第２引数は非キャラ型の1次元配列変数でなければなりません", line, 2, false); return null; }
				if (terms.Length >= 3)
					term = getChangeableVariable(terms, 3, line);
				if (term == null)
				{
                    VariableToken varToken = GlobalStatic.VariableData.GetSystemVariableToken("RESULT");
                    term = new VariableTerm(varToken, new IOperandTerm[] { new SingleTerm(0) });
				}
				return new SpHtmlSplitArgument(terms[0], destVar, term);
			}
		}
		
		private sealed class SP_GETINT_ArgumentBuilder : ArgumentBuilder
		{
			public SP_GETINT_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(Int64) };
				minArg = 0;
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (terms.Length == 0)
				{
					VariableToken varToken = GlobalStatic.VariableData.GetSystemVariableToken("RESULT");
					return new SpGetIntArgument(new VariableTerm(varToken, new IOperandTerm[]{new SingleTerm(0)}));
				}
				if (!checkArgumentType(line, exm, terms))
					return null;
				VariableTerm x = getChangeableVariable(terms, 1, line);
				if (x == null)
					return null;
				return new SpGetIntArgument(x);
			}
		}

		private sealed class SP_CONTROL_ARRAY_ArgumentBuilder : ArgumentBuilder
		{
			public SP_CONTROL_ARRAY_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(void), typeof(Int64), typeof(Int64) };
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;
				VariableTerm x = getChangeableVariable(terms, 1, line);
				if (x == null)
					return null;
				return new SpArrayControlArgument(x, terms[1], terms[2]);
			}
		}

		private sealed class SP_SHIFT_ARRAY_ArgumentBuilder : ArgumentBuilder
		{
			public SP_SHIFT_ARRAY_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(void), typeof(Int64), typeof(void), typeof(Int64), typeof(Int64) };
				minArg = 3;
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;

				VariableTerm x = getChangeableVariable(terms, 1, line);
				if (x == null)
					return null;
				if (!x.Identifier.IsArray1D)
				{ warn("第１引数に１次元配列もしくは配列型キャラクタ変数以外を指定することはできません", line, 2, false); return null; }

				if (line.FunctionCode == FunctionCode.ARRAYSHIFT)
				{
					if (terms[0].GetOperandType() != terms[2].GetOperandType())
					{ warn("第１引数と第３引数の型が違います", line, 2, false); return null; }
				}
				IOperandTerm term4 = terms.Length >= 4 ? terms[3] : new SingleTerm(0);
				IOperandTerm term5 = terms.Length >= 5 ? terms[4] : null;
				return new SpArrayShiftArgument(x, terms[1], terms[2], term4, term5);
			}
		}

		private sealed class SP_SAVEVAR_ArgumentBuilder : ArgumentBuilder
		{
			public SP_SAVEVAR_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(string), typeof(string), typeof(void)};
				argAny = true;
				minArg = 3;
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;
				List<VariableToken> varTokens = new List<VariableToken>();
				for (int i = 2; i < terms.Length; i++)
				{
					if (terms[i] == null)
					{ warn("第" + (i + 1) + "引数を省略できません", line, 2, false); return null; }
					VariableTerm vTerm = getChangeableVariable(terms, i + 1, line);
					if (vTerm == null)
						return null;
					VariableToken vToken = vTerm.Identifier;
					if (vToken.IsCharacterData)
					{ warn("キャラクタ変数"+ vToken.Name+"はセーブできません(キャラクタ変数のSAVEにはSAVECHARAを使用します)", line, 2, false); return null; }
					if (vToken.IsPrivate)
					{ warn("プライベート変数" + vToken.Name + "はセーブできません", line, 2, false); return null; }
					if (vToken.IsLocal)
					{ warn("ローカル変数" + vToken.Name + "はセーブできません", line, 2, false); return null; }
					if (vToken.IsConst)
					{ warn("値を変更できない変数はセーブできません", line, 2, false); return null; }
					if (vToken.IsCalc)
					{ warn("疑似変数はセーブできません", line, 2, false); return null; }
					if (vToken.IsReference)
					{ warn("参照型変数はセーブできません", line, 2, false); return null; }
					varTokens.Add(vToken);
				}
				for (int i = 0; i < varTokens.Count; i++)
				{
					for (int j = i + 1; j < varTokens.Count; j++)
						if (varTokens[i] == varTokens[j])
						{
							warn("変数" + varTokens[i].Name + "を二度以上保存しようとしています", line, 1, false);
							return null;
						}
				}
				VariableToken[] arg3 = new VariableToken[varTokens.Count];
				varTokens.CopyTo(arg3);
				return new SpSaveVarArgument(terms[0], terms[1], arg3);
			}
		}

		private sealed class SP_SAVECHARA_ArgumentBuilder : ArgumentBuilder
		{
			public SP_SAVECHARA_ArgumentBuilder()
			{
				argumentTypeArray = new Type[] { typeof(string), typeof(string), typeof(Int64) };
				minArg = 3;
				argAny = true;
			}
			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;

				List<IOperandTerm> termList = new List<IOperandTerm>();
				termList.AddRange(terms);
				ExpressionArrayArgument ret = new ExpressionArrayArgument(termList);

				for (int i = 2; i < termList.Count; i++)
				{
					if (!(termList[i] is SingleTerm))
						continue;
					Int64 iValue = termList[i].GetIntValue(null);
					if (iValue < 0)
					{ warn("キャラ登録番号は正の値でなければなりません", line, 2, false); return null; }
					if (iValue > Int32.MaxValue)
					{ warn("キャラ登録番号が32bit符号付整数の上限を超えています", line, 2, false); return null; }
					for (int j = i + 1; j < termList.Count; j++)
					{
						if (!(termList[j] is SingleTerm))
							continue;
						if (iValue == termList[j].GetIntValue(null))
						{
							warn("キャラ登録番号" + iValue.ToString() + "を二度以上保存しようとしています", line, 1, false);
							return null;
						}
					}
				}
				return ret;
			}
		}

		private sealed class SP_REF_ArgumentBuilder : ArgumentBuilder
		{
			public SP_REF_ArgumentBuilder(bool byname)
			{
				argumentTypeArray = new Type[] { typeof(void), typeof(void) };
				minArg = 2;
				this.byname = byname;
			}

            readonly bool byname;

			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				WordCollection wc = popWords(line);
                wc.ShiftNext();
                if (!(wc.Current is IdentifierWord id) || wc.Current.Type != ',')
				{ warn("書式が間違っています", line, 2, false); return null; }
				wc.ShiftNext();
				IOperandTerm name = null;
                string srcCode = null;
				if (byname)
				{
					name = ExpressionParser.ReduceExpressionTerm(wc, TermEndWith.EoL);
					if (name == null || name.IsInteger || !wc.EOL)
					{ warn("書式が間違っています", line, 2, false); return null; }
					name = name.Restructure(exm);
					if (name is SingleTerm)
						srcCode = name.GetStrValue(exm);
				}
				else
				{
                    wc.ShiftNext();
                    if (!(wc.Current is IdentifierWord id2) || !wc.EOL)
					{ warn("書式が間違っています", line, 2, false); return null; }
					srcCode = id2.Code;
				}
				UserDefinedRefMethod refm = GlobalStatic.IdentifierDictionary.GetRefMethod(id.Code);
				ReferenceToken refVar = null;
				if (refm == null)
				{
					VariableToken token = GlobalStatic.IdentifierDictionary.GetVariableToken(id.Code, null, true);
					if (token == null || !token.IsReference)
					{ warn("第一引数は関数参照か参照型変数でなければなりません", line, 2, false); return null; }
					refVar = (ReferenceToken)token;
				}

				if (refm != null)
				{
					if (srcCode == null)
						return new RefArgument(refm, name);
					UserDefinedRefMethod srcRef = GlobalStatic.IdentifierDictionary.GetRefMethod(srcCode);
					if (srcRef != null)
					{
						return new RefArgument(refm, srcRef);
					}
					FunctionLabelLine label = GlobalStatic.LabelDictionary.GetNonEventLabel(srcCode);
					if (label == null)
					{ warn("式中関数" + srcCode + "が見つかりません", line, 2, false); return null; }
					if (!label.IsMethod)
					{ warn("#FUNCTION(S)属性を持たない関数" + srcCode + "は参照できません", line, 2, false); return null; }
					CalledFunction called = CalledFunction.CreateCalledFunctionMethod(label, label.LabelName);
					return new RefArgument(refm, called);
				}
				else
				{
					if (srcCode == null)
						return new RefArgument(refVar, name);
					VariableToken srcVar = GlobalStatic.IdentifierDictionary.GetVariableToken(srcCode, null, true);
					if (srcVar == null)
					{ warn("変数" + srcCode + "が見つかりません", line, 2, false); return null; }
					return new RefArgument(refVar, srcVar);
				}
			}
		}

        private sealed class SP_INPUT_ArgumentBuilder : ArgumentBuilder
        {
            public SP_INPUT_ArgumentBuilder()
            {
                argumentTypeArray = new Type[] { typeof(Int64) };
                //if (nullable)妥協
                minArg = 0;
            }
            public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
            {
                IOperandTerm[] terms = popTerms(line);
                if (!checkArgumentType(line, exm, terms))
                    return null;
                IOperandTerm term = null;
                ExpressionArgument ret;
                if (terms.Length == 0)
                {
                    ret = new ExpressionArgument(term);
                    return ret;
                }
                else
                {
                    term = terms[0];
                    ret = new ExpressionArgument(term);
                }

                if (term is SingleTerm)
                {
                    Int64 i = term.GetIntValue(null);
                    if (line.FunctionCode == FunctionCode.ONEINPUT)
                    {
                        if (i < 0)
                        {
                            warn("ONEINPUTの引数にONEINPUTが受け取れない負の数数が指定されています（引数を無効とします）", line, 1, false);
                            ret = new ExpressionArgument(null);
                            return ret;
                        }
                        else if (i > 9)
                        {
                            warn("ONEINPUTの引数にONEINPUTが受け取れない2桁以上の数数が指定されています（最初の桁を引数と見なします）", line, 1, false);
                            i = Int64.Parse(i.ToString().Remove(1));
                        }
                    }
                    ret.ConstInt = i;
                    ret.IsConst = true;
                }
                return ret;
            }
        }

        private sealed class SP_COPY_ARRAY_Arguments : ArgumentBuilder
        {
            public SP_COPY_ARRAY_Arguments()
            {
                argumentTypeArray = new Type[] { typeof(string), typeof(string) };
                minArg = 2;
            }

            public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
            {
                IOperandTerm[] terms = popTerms(line);
                if (!checkArgumentType(line, exm, terms))
                    return null;
                VariableToken[] vars = new VariableToken[2] { null, null };
                if (terms[0] is SingleTerm term)
                {
                    if ((vars[0] = GlobalStatic.IdentifierDictionary.GetVariableToken(term.Str, null, true)) == null)
                    {
                        warn("ARRAYCOPY命令の第１引数\"" + term.Str + "\"は変数名として存在しません", line, 2, false);
                        return null;
                    }
                    if (!vars[0].IsArray1D && !vars[0].IsArray2D && !vars[0].IsArray3D)
                    {
                        warn("ARRAYCOPY命令の第１引数\"" + term.Str + "\"は配列変数ではありません", line, 2, false);
                        return null;
                    }
                    if (vars[0].IsCharacterData)
                    {
                        warn("ARRAYCOPY命令の第１引数\"" + term.Str + "\"はキャラクタ変数です（対応していません）", line, 2, false);
                        return null;
                    }
                }
                if (terms[1] is SingleTerm term1)
                {
                    if ((vars[1] = GlobalStatic.IdentifierDictionary.GetVariableToken(term1.Str, null, true)) == null)
                    {
                        warn("ARRAYCOPY命令の第２引数\"" + term1.Str + "\"は変数名として存在しません", line, 2, false);
                        return null;
                    }
                    if (!vars[1].IsArray1D && !vars[1].IsArray2D && !vars[1].IsArray3D)
                    {
                        warn("ARRAYCOPY命令の第２引数\"" + term1.Str + "\"は配列変数ではありません", line, 2, false);
                    }
                    if (vars[1].IsCharacterData)
                    {
                        warn("ARRAYCOPY命令の第２引数\"" + term1.Str + "\"はキャラクタ変数です（対応していません）", line, 2, false);
                        return null;
                    }
                    if (vars[1].IsConst)
                    {
                        warn("ARRAYCOPY命令の第２引数\"" + term1.Str + "\"は値を変更できない変数です", line, 2, false);
                        return null;
                    }
                }
                if ((vars[0] != null) && (vars[1] != null))
                {
                    if ((vars[0].IsArray1D && !vars[1].IsArray1D) || (vars[0].IsArray2D && !vars[1].IsArray2D) || (vars[0].IsArray3D && !vars[1].IsArray3D))
                    {
                        warn("ARRAYCOPY命令の2つの引数の次元が異なります", line, 2, false);
                        return null;
                    }
                    if ((vars[0].IsInteger && vars[1].IsString) || (vars[0].IsString && vars[1].IsInteger))
                    {
                        warn("ARRAYCOPY命令の２つの配列変数の型が一致していません", line, 2, false);
                        return null;
                    }
                }
                return new SpCopyArrayArgument(terms[0], terms[1]);
            }
        }
        #endregion		

		/// <summary>
		/// 一般型。数式と文字列式の組み合わせのみを引数とし、特殊なチェックが必要ないもの
		/// </summary>
		private sealed class Expressions_ArgumentBuilder : ArgumentBuilder
		{
			public Expressions_ArgumentBuilder(Type[] types, int minArgs = -1)
			{
				argumentTypeArray = types;
				this.minArg = minArgs;
			}

			public override Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
			{
				IOperandTerm[] terms = popTerms(line);
				if (!checkArgumentType(line, exm, terms))
					return null;
				return new ExpressionsArgument(argumentTypeArray, terms);
			}
		}
	}
}