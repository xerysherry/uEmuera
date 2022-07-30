using System;
using System.Collections.Generic;
using System.Text;
using MinorShift.Emuera.Sub;
using System.Text.RegularExpressions;
using MinorShift.Emuera.GameData.Variable;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.GameView;
using MinorShift.Emuera.GameData;
using MinorShift.Emuera.GameData.Function;
using MinorShift.Emuera.GameProc.Function;

namespace MinorShift.Emuera.GameProc
{
	internal static class LogicalLineParser
	{
		public static bool ParseSharpLine(FunctionLabelLine label, StringStream st, ScriptPosition position, List<string> OnlyLabel)
		{
			st.ShiftNext();//'#'を飛ばす
			string token = LexicalAnalyzer.ReadSingleIdentifier(st);//#～自体にはマクロ非適用
			if (Config.ICFunction)
				token = token.ToUpper();
            //#行として不正な行でもAnalyzeに行って引っかかることがあるので、先に存在しない#～は弾いてしまう
            if (token == null || (token != "SINGLE" && token != "LATER" && token != "PRI" && token != "ONLY" && token != "FUNCTION" && token != "FUNCTIONS" 
                && token != "LOCALSIZE" && token != "LOCALSSIZE" && token != "DIM" && token != "DIMS"))
            {
                ParserMediator.Warn("解釈できない#行です", position, 1);
                return false;
            }
			try
			{
				WordCollection wc = LexicalAnalyzer.Analyse(st, LexEndWith.EoL, LexAnalyzeFlag.AllowAssignment);
				switch (token)
				{
					case "SINGLE":
						if (label.IsMethod)
						{
							ParserMediator.Warn("式中関数では#SINGLEは機能しません", position, 1);
							break;
						}
						else if (!label.IsEvent)
						{
							ParserMediator.Warn("イベント関数以外では#SINGLEは機能しません", position, 1);
							break;
						}
						else if (label.IsSingle)
						{
							ParserMediator.Warn("#SINGLEが重複して使われています", position, 1);
							break;
						}
						else if (label.IsOnly)
						{
							ParserMediator.Warn("#ONLYが指定されたイベント関数では#SINGLEは機能しません", position, 1);
							break;
						}
						label.IsSingle = true;
						break;
					case "LATER":
						if (label.IsMethod)
						{
							ParserMediator.Warn("式中関数では#LATERは機能しません", position, 1);
							break;
						}
						else if (!label.IsEvent)
						{
							ParserMediator.Warn("イベント関数以外では#LATERは機能しません", position, 1);
							break;
						}
						else if (label.IsLater)
						{
							ParserMediator.Warn("#LATERが重複して使われています", position, 1);
							break;
						}
						else if (label.IsOnly)
						{
							ParserMediator.Warn("#ONLYが指定されたイベント関数では#LATERは機能しません", position, 1);
							break;
						}
						else if (label.IsPri)
							ParserMediator.Warn("#PRIと#LATERが重複して使われています(この関数は2度呼ばれます)", position, 1);
						label.IsLater = true;
						break;
					case "PRI":
						if (label.IsMethod)
						{
							ParserMediator.Warn("式中関数では#PRIは機能しません", position, 1);
							break;
						}
						else if (!label.IsEvent)
						{
							ParserMediator.Warn("イベント関数以外では#PRIは機能しません", position, 1);
							break;
						}
						else if (label.IsPri)
						{
							ParserMediator.Warn("#PRIが重複して使われています", position, 1);
							break;
						}
						else if (label.IsOnly)
						{
							ParserMediator.Warn("#ONLYが指定されたイベント関数では#PRIは機能しません", position, 1);
							break;
						}
						else if (label.IsLater)
							ParserMediator.Warn("#PRIと#LATERが重複して使われています(この関数は2度呼ばれます)", position, 1);
						label.IsPri = true;
						break;
					case "ONLY":
						if (label.IsMethod)
						{
							ParserMediator.Warn("式中関数では#ONLYは機能しません", position, 1);
							break;
						}
						else if (!label.IsEvent)
						{
							ParserMediator.Warn("イベント関数以外では#ONLYは機能しません", position, 1);
							break;
						}
						else if (label.IsOnly)
						{
							ParserMediator.Warn("#ONLYが重複して使われています", position, 1);
							break;
						}
						else if (OnlyLabel.Contains(label.LabelName))
							ParserMediator.Warn("このイベント関数\"@" + label.LabelName + "\"にはすでに#ONLYが宣言されています（この関数は実行されません）", position, 1);
						OnlyLabel.Add(label.LabelName);
						label.IsOnly = true;
						if (label.IsPri)
						{
							ParserMediator.Warn("このイベント関数には#PRIが宣言されていますが無視されます", position, 1);
							label.IsPri = false;
						}
						if (label.IsLater)
						{
							ParserMediator.Warn("このイベント関数には#LATERが宣言されていますが無視されます", position, 1);
							label.IsLater = false;
						}
						if (label.IsSingle)
						{
							ParserMediator.Warn("このイベント関数には#SINGLEが宣言されていますが無視されます", position, 1);
							label.IsSingle = false;
						}
						break;
					case "FUNCTION":
					case "FUNCTIONS":
						if (!string.IsNullOrEmpty(label.LabelName) && char.IsDigit(label.LabelName[0]))
						{
							ParserMediator.Warn("#" + token + "属性は関数名が数字で始まる関数には指定できません", position, 1);
							label.IsError = true;
							label.ErrMes = "関数名が数字で始まっています";
							break;
						}
						if (label.IsMethod)
						{
							if ((label.MethodType == typeof(Int64) && token == "FUNCTION") || (label.MethodType == typeof(string) && token == "FUNCTIONS"))
							{
								ParserMediator.Warn("関数" + label.LabelName + "にはすでに#" + token + "が宣言されています(この行は無視されます)", position, 1);
								return false;
							}
							if (label.MethodType == typeof(Int64) && token == "FUNCTIONS")
								ParserMediator.Warn("関数" + label.LabelName + "にはすでに#FUNCTIONが宣言されています", position, 2);
							else if (label.MethodType == typeof(string) && token == "FUNCTION")
								ParserMediator.Warn("関数" + label.LabelName + "にはすでに#FUNCTIONSが宣言されています", position, 2);
							return false;
						}
						if (label.Depth == 0)
						{
							ParserMediator.Warn("システム関数に#" + token + "が指定されています", position, 2);
							return false;
						}
						label.IsMethod = true;
						label.Depth = 0;
						if (token == "FUNCTIONS")
							label.MethodType = typeof(string);
						else
							label.MethodType = typeof(Int64);
						if (label.IsPri)
						{
							ParserMediator.Warn("式中関数では#PRIは機能しません", position, 1);
							label.IsPri = false;
						}
						if (label.IsLater)
						{
							ParserMediator.Warn("式中関数では#LATERは機能しません", position, 1);
							label.IsLater = false;
						}
						if (label.IsSingle)
						{
							ParserMediator.Warn("式中関数では#SINGLEは機能しません", position, 1);
							label.IsSingle = false;
						}
						if (label.IsOnly)
						{
							ParserMediator.Warn("式中関数では#ONLYは機能しません", position, 1);
							label.IsOnly = false;
						}
						break;
					case "LOCALSIZE":
					case "LOCALSSIZE":
						{
							if (wc.EOL)
							{
								ParserMediator.Warn("#" + token + "の後に有効な数値が指定されていません", position, 2);
								break;
							}
                            //イベント関数では指定しても無視される
                            if (label.IsEvent)
                            {
                                ParserMediator.Warn("イベント関数では#" + token + "による" + token.Substring(0, token.Length - 4)+ "のサイズ指定は無視されます", position, 1);
                                break;
                            }
							IOperandTerm arg = ExpressionParser.ReduceIntegerTerm(wc, TermEndWith.EoL);
                            if ((!(arg.Restructure(null) is SingleTerm sizeTerm)) || (sizeTerm.GetOperandType() != typeof(Int64)))
                            {
                                ParserMediator.Warn("#" + token + "の後に有効な定数式が指定されていません", position, 2);
                                break;
                            }
                            if (sizeTerm.Int <= 0)
							{
								ParserMediator.Warn("#" + token + "に0以下の値(" + sizeTerm.Int.ToString() + ")が与えられました。設定は無視されます", position, 1);
								break;
							}
							if (sizeTerm.Int >= Int32.MaxValue)
							{
								ParserMediator.Warn("#" + token + "に大きすぎる値(" + sizeTerm.Int.ToString() + ")が与えられました。設定は無視されます", position, 1);
								break;
							}
							int size = (int)sizeTerm.Int;
							if (token == "LOCALSIZE")
							{
								if (GlobalStatic.IdentifierDictionary.getLocalIsForbid("LOCAL"))
								{
									ParserMediator.Warn("#" + token + "が指定されていますが変数LOCALは使用禁止されています", position, 2);
									break;
								}
								if (label.LocalLength > 0)
									ParserMediator.Warn("この関数にはすでに#LOCALSIZEが定義されています。（以前の定義は無視されます）", position, 1);
								label.LocalLength = size;
							}
							else
							{
								if (GlobalStatic.IdentifierDictionary.getLocalIsForbid("LOCALS"))
								{
									ParserMediator.Warn("#" + token + "が指定されていますが変数LOCALSは使用禁止されています", position, 2);
									break;
								}
								if (label.LocalsLength > 0)
									ParserMediator.Warn("この関数にはすでに#LOCALSSIZEが定義されています。（以前の定義は無視されます）", position, 1);
								label.LocalsLength = size;
							}
						}
						break;
					case "DIM":
					case "DIMS":
						{
							UserDefinedVariableData data = UserDefinedVariableData.Create(wc, token == "DIMS", true, position);
							if (!label.AddPrivateVariable(data))
							{
								ParserMediator.Warn("変数名" + data.Name + "は既に使用されています", position, 2);
								return false;
							}
							break;
						}
					default:
						ParserMediator.Warn("解釈できない#行です", position, 1);
						break;
				}
				if (!wc.EOL)
					ParserMediator.Warn("#の識別子の後に余分な文字があります", position, 1);
			}
			catch (Exception e)
			{
				ParserMediator.Warn(e.Message, position, 2);
				goto err;
			}
			return true;
		err:
			return false;
		}
		
		public static LogicalLine ParseLine(string str, EmueraConsole console)
		{
			ScriptPosition position = new ScriptPosition();
			StringStream stream = new StringStream(str);
			return ParseLine(stream, position, console);
		}

		public static LogicalLine ParseLabelLine(StringStream stream, ScriptPosition position, EmueraConsole console)
		{
			bool isFunction = (stream.Current == '@');
			//int lineNo = position.LineNo;
			string labelName = "";
			string errMes = "";
			try
			{
				int warnLevel = -1;
                stream.ShiftNext();//@か$を除去
				WordCollection wc = LexicalAnalyzer.Analyse(stream, LexEndWith.EoL, LexAnalyzeFlag.AllowAssignment);
				if (wc.EOL || !(wc.Current is IdentifierWord))
				{
					errMes = "関数名が不正であるか存在しません";
					goto err;
				}
				labelName = ((IdentifierWord)wc.Current).Code;
				wc.ShiftNext();
				if (Config.ICVariable)
					labelName = labelName.ToUpper();
				GlobalStatic.IdentifierDictionary.CheckUserLabelName(ref errMes, ref warnLevel, isFunction, labelName);
				if (warnLevel >= 0)
				{
					if (warnLevel >= 2)
						goto err;
					ParserMediator.Warn(errMes, position, warnLevel);
				}
				if (!isFunction)//$ならこの時点で終了
				{
					if (!wc.EOL)
						ParserMediator.Warn("$で始まるラベルに引数が設定されています", position, 1);
					return new GotoLabelLine(position, labelName);
				}



				//labelName = LexicalAnalyzer.ReadString(stream, StrEndWith.LeftParenthesis_Bracket_Comma_Semicolon);
				//labelName = labelName.Trim();
				//if (Config.ICVariable)
				//    labelName = labelName.ToUpper();
				//GlobalStatic.IdentifierDictionary.CheckUserLabelName(ref errMes, ref warnLevel, isFunction, labelName);
				//if(warnLevel >= 0)
				//{
				//    if (warnLevel >= 2)
				//        goto err;
				//    ParserMediator.Warn(errMes, position, warnLevel);
				//}
				//if (!isFunction)//$ならこの時点で終了
				//{
				//    LexicalAnalyzer.SkipWhiteSpace(stream);
				//    if (!stream.EOS)
				//        ParserMediator.Warn("$で始まるラベルに引数が設定されています", position, 1);
				//    return new GotoLabelLine(position, labelName);
				//}

				////関数名部分に_renameを使えないように変更
				//if (ParserMediator.RenameDic != null && ((stream.ToString().IndexOf("[[") >= 0) && (stream.ToString().IndexOf("]]") >= 0)))
				//{
				//    string line = stream.ToString();
				//    foreach (KeyValuePair<string, string> pair in ParserMediator.RenameDic)
				//        line = line.Replace(pair.Key, pair.Value);
				//    stream = new StringStream(line);
				//}
				//WordCollection wc = null;
				//wc = LexicalAnalyzer.Analyse(stream, LexEndWith.EoL, LexAnalyzeFlag.AllowAssignment);
				if (Program.AnalysisMode)
					console.PrintC("@" + labelName, false);
				FunctionLabelLine funclabelLine = new FunctionLabelLine(position, labelName, wc);
				if (IdentifierDictionary.IsEventLabelName(labelName))
				{
					funclabelLine.IsEvent = true;
					funclabelLine.IsSystem = true;
					funclabelLine.Depth = 0;
				}
				else if (IdentifierDictionary.IsSystemLabelName(labelName))
				{
					funclabelLine.IsSystem = true;
					funclabelLine.Depth = 0;
				}
				return funclabelLine;
			}
			catch (CodeEE e)
			{
				errMes = e.Message;
			}
		err:
			uEmuera.Media.SystemSounds.Hand.Play();
			if (isFunction)
			{
				if(labelName.Length == 0)
					labelName = "<Error>";
				return new InvalidLabelLine(position, labelName, errMes);
			}
			return new InvalidLine(position, errMes);
		}
		
		
		public static LogicalLine ParseLine(StringStream stream, ScriptPosition position, EmueraConsole console)
		{
			//int lineNo = position.LineNo;
			string errMes;
			LexicalAnalyzer.SkipWhiteSpace(stream);//先頭のホワイトスペースを読み飛ばす
			if (stream.EOS)
				return null;
			//コメント行かどうかはここに来る前に判定しておく
			try
			{
				#region 前置インクリメント、デクリメント行
				if (stream.Current == '+' || stream.Current == '-')
				{
					char op = stream.Current;
					WordCollection wc = LexicalAnalyzer.Analyse(stream, LexEndWith.EoL, LexAnalyzeFlag.None);
                    if ((!(wc.Current is OperatorWord opWT)) || ((opWT.Code != OperatorCode.Increment) && (opWT.Code != OperatorCode.Decrement)))
                    {
                        if (op == '+')
                            errMes = "行が\'+\'から始まっていますが、インクリメントではありません";
                        else
                            errMes = "行が\'-\'から始まっていますが、デクリメントではありません";
                        goto err;
                    }
                    wc.ShiftNext();
					//token = EpressionParser.単語一個分取得(wc)
					//token非変数
					//token文字列形
					//token変更不可能
					//if (wc != EOS)
					//
					return new InstructionLine(position, FunctionIdentifier.SETFunction, opWT.Code, wc, null);
				}
				#endregion
				IdentifierWord idWT = LexicalAnalyzer.ReadFirstIdentifierWord(stream);
				if (idWT != null)
				{
					FunctionIdentifier func = GlobalStatic.IdentifierDictionary.GetFunctionIdentifier(idWT.Code);
					//命令文
					if (func != null)//関数文
					{
						if (stream.EOS) //引数の無い関数
							return new InstructionLine(position, func, stream);
						if ((stream.Current != ';') && (stream.Current != ' ') && (stream.Current != '\t') && (!Config.SystemAllowFullSpace || (stream.Current != '　')))
						{
							if (stream.Current == '　')
								errMes = "命令で行が始まっていますが、命令の直後に半角スペース・タブ以外の文字が来ています(この警告はシステムオプション「" + Config.GetConfigName(ConfigCode.SystemAllowFullSpace) + "」により無視できます)";
							else
								errMes = "命令で行が始まっていますが、命令の直後に半角スペース・タブ以外の文字が来ています";
							goto err;
						}
						stream.ShiftNext();
						return new InstructionLine(position, func, stream);
					}
				}
				LexicalAnalyzer.SkipWhiteSpace(stream);
				if (stream.EOS)
				{
					errMes = "解釈できない行です";
					goto err;
				}
				//命令行ではない→代入行のはず
				stream.Seek(0, System.IO.SeekOrigin.Begin);
				OperatorCode assignOP = OperatorCode.NULL;
				WordCollection wc1 = LexicalAnalyzer.Analyse(stream, LexEndWith.Operator, LexAnalyzeFlag.None);
				//if (idWT != null)
				//	wc1.Collection.Insert(0, idWT);
				try
				{
					assignOP = LexicalAnalyzer.ReadAssignmentOperator(stream);
				}
				catch(CodeEE)
				{
					errMes = "解釈できない行です";
					goto err;
				}
				//eramaker互換警告
				//stream.Jump(-1);
				//if ((stream.Current != ' ') && (stream.Current != '\t'))
				//{
				//	errMes = "変数で行が始まっていますが、演算子の直前に半角スペースまたはタブがありません";
				//	goto err;
				//}
				//stream.ShiftNext();


				if (assignOP == OperatorCode.Equal)
				{
					if (console != null)
						ParserMediator.Warn("代入演算子に\"==\"が使われています", position, 0);
					//"=="を代入文に使うのは本当はおかしいが結構使われているので仕様にする
					assignOP = OperatorCode.Assignment;
				}
				return new InstructionLine(position, FunctionIdentifier.SETFunction, assignOP, wc1, stream);
			err:
				return new InvalidLine(position, errMes);
			}
			catch (CodeEE e)
			{
				uEmuera.Media.SystemSounds.Hand.Play();
				return new InvalidLine(position, e.Message);
			}
		}
		
	}
}
