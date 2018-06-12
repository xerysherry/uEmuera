using System;
using System.Collections.Generic;
//using System.Drawing;
using MinorShift.Emuera.Sub;
using MinorShift.Emuera.GameData;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.GameData.Variable;
using MinorShift.Emuera.GameView;
using MinorShift.Emuera.GameData.Function;
using MinorShift.Emuera.GameProc.Function;
using uEmuera.Drawing;

namespace MinorShift.Emuera.GameProc
{
	internal sealed partial class Process
	{
		private void runScriptProc()
		{
			while (true)
			{
				//bool sequential = state.Sequential;
				state.ShiftNextLine();
				//WinmmTimerから時間を取得するのはそれ自体結構なコストがかかるので10000行に一回くらいで。
				if (Config.InfiniteLoopAlertTime > 0 && (state.lineCount % 10000 == 0))
					checkInfiniteLoop();
				LogicalLine line = state.CurrentLine;
				InstructionLine func = line as InstructionLine;
				//これがNULLになる様な処理は現状ないはず
				//if (line == null)
				//	throw new ExeEE("Emuera.exeは次に実行する行を見失いました");
				if (line.IsError)
					throw new CodeEE(line.ErrMes);
				else if (func != null)
				{//1753 InstructionLineを先に持ってきてみる。わずかに速くなった気がしないでもない
					if (!Program.DebugMode && func.Function.IsDebug())
					{//非DebugモードでのDebug系命令。何もしない。（SIF文のためにコメント行扱いにはできない）
						continue;
					}
					if (func.Argument == null)
					{
						ArgumentParser.SetArgumentTo(func);
						if (func.IsError)
							throw new CodeEE(func.ErrMes);
					}
					if ((skipPrint) && (func.Function.IsPrint()))
					{
						if ((userDefinedSkip) && (func.Function.IsInput()))
						{
							console.PrintError("表示スキップ中にデフォルト値を持たないINPUTに遭遇しました");
							console.PrintError("INPUTに必要な処理をNOSKIP～ENDNOSKIPで囲むか、SKIPDISP 0～SKIPDISP 1で囲ってください");
							throw new CodeEE("無限ループに入る可能性が高いため実行を終了します");
						}
						continue;
					}
					if (func.Function.Instruction != null)
						func.Function.Instruction.DoInstruction(exm, func, state);
					else if (func.Function.IsFlowContorol())
						doFlowControlFunction(func);
					else
						doNormalFunction(func);
				}
				else if ((line is NullLine) || (line is FunctionLabelLine))
				{//（関数終端） or ファイル終端
					//if (sequential)
					//{//流れ落ちてきた
					if (!state.IsFunctionMethod)
						vEvaluator.RESULT = 0;
					state.Return(0);
					//}
					//1750 飛んできた直後にShiftNextが入るのでここが実行されることは無いはず
					//else//CALLやJUMPで飛んできた
					//return;
				}
				else if (line is GotoLabelLine)
					continue;//＄ラベル。何もすることはない。
				else if (line is InvalidLine)
				{
					if (string.IsNullOrEmpty(line.ErrMes))
						throw new CodeEE("読込に失敗した行が実行されました。エラーの詳細は読込時の警告を参照してください。");
					else
						throw new CodeEE(line.ErrMes);
				}
				//現在そんなものはない
				//else
				//	throw new ExeEE("定義されていない種類の行です");
				if (!console.IsRunning || state.ScriptEnd)
					return;
			}
		}

		public void DoDebugNormalFunction(InstructionLine func, bool munchkin)
		{
			if (func.Function.Instruction != null)
				func.Function.Instruction.DoInstruction(exm, func, state);
			else
				doNormalFunction(func);
			if (munchkin)
				vEvaluator.IamaMunchkin();
		}

		#region normal
		void doNormalFunction(InstructionLine func)
		{
			Int64 iValue = 0;
			string str = null;
			IOperandTerm term = null;
			switch (func.FunctionCode)
			{

				case FunctionCode.PRINTBUTTON://変数の内容
					{
						if (skipPrint)
							break;
                        exm.Console.UseUserStyle = true;
                        exm.Console.UseSetColorStyle = true;
                        SpButtonArgument bArg = (SpButtonArgument)func.Argument;
						str = bArg.PrintStrTerm.GetStrValue(exm);
						//ボタン処理に絡んで表示がおかしくなるため、PRINTBUTTONでの改行コードはオミット
						str = str.Replace("\n", "");
						if (bArg.ButtonWord.GetOperandType() == typeof(long))
							exm.Console.PrintButton(str, bArg.ButtonWord.GetIntValue(exm));
						else
							exm.Console.PrintButton(str, bArg.ButtonWord.GetStrValue(exm));
					}
					break;
				case FunctionCode.PRINTBUTTONC://変数の内容
				case FunctionCode.PRINTBUTTONLC:
					{
						if (skipPrint)
							break;
                        exm.Console.UseUserStyle = true;
                        exm.Console.UseSetColorStyle = true;
                        SpButtonArgument bArg = (SpButtonArgument)func.Argument;
						str = bArg.PrintStrTerm.GetStrValue(exm);
						//ボタン処理に絡んで表示がおかしくなるため、PRINTBUTTONでの改行コードはオミット
						str = str.Replace("\n", "");
						bool isRight = (func.FunctionCode == FunctionCode.PRINTBUTTONC) ? true : false;
						if (bArg.ButtonWord.GetOperandType() == typeof(long))
							exm.Console.PrintButtonC(str, bArg.ButtonWord.GetIntValue(exm), isRight);
						else
							exm.Console.PrintButtonC(str, bArg.ButtonWord.GetStrValue(exm), isRight);
					}
					break;
				case FunctionCode.PRINTPLAIN:
				case FunctionCode.PRINTPLAINFORM:
					{
						if (skipPrint)
							break;
                        exm.Console.UseUserStyle = true;
                        exm.Console.UseSetColorStyle = true;
                        term = ((ExpressionArgument)func.Argument).Term;
						exm.Console.PrintPlain(term.GetStrValue(exm));
					}
					break;
				case FunctionCode.DRAWLINE://画面の左端から右端まで----と線を引く。
					if (skipPrint)
						break;
					exm.Console.PrintBar();
					exm.Console.NewLine();
					break;
				case FunctionCode.CUSTOMDRAWLINE:
				case FunctionCode.DRAWLINEFORM:
					{
						if (skipPrint)
							break;
						term = ((ExpressionArgument)func.Argument).Term;
						str = term.GetStrValue(exm);
						exm.Console.printCustomBar(str);
						//exm.Console.setStBar(str);
						//exm.Console.PrintBar();
						exm.Console.NewLine();
						//exm.Console.setStBar(Config.DrawLineString);
					}
					break;
				case FunctionCode.PRINT_ABL://能力。引数は登録番号
				case FunctionCode.PRINT_TALENT://素質
				case FunctionCode.PRINT_MARK://刻印
				case FunctionCode.PRINT_EXP://経験
					{
						if (skipPrint)
							break;
						ExpressionArgument intExpArg = (ExpressionArgument)func.Argument;
						Int64 target = intExpArg.Term.GetIntValue(exm);
						exm.Console.Print(vEvaluator.GetCharacterDataString(target, func.FunctionCode));
						exm.Console.NewLine();
					}
					break;
				case FunctionCode.PRINT_PALAM://パラメータ
					{
						if (skipPrint)
							break;
						ExpressionArgument intExpArg = (ExpressionArgument)func.Argument;
						Int64 target = intExpArg.Term.GetIntValue(exm);
						int count = 0;
						///100以降は否定の珠とかなので表示しない
						for (int i = 0; i < 100; i++)
						{
							string printStr = vEvaluator.GetCharacterParamString(target, i);
							if (printStr != null)
							{
								exm.Console.PrintC(printStr, true);
								count++;
								if ((Config.PrintCPerLine > 0) && (count % Config.PrintCPerLine == 0))
									exm.Console.PrintFlush(false);
							}
						}
						exm.Console.PrintFlush(false);
						exm.Console.RefreshStrings(false);
					}
					break;
				case FunctionCode.PRINT_ITEM://所持アイテム
					if (skipPrint)
						break;
					exm.Console.Print(vEvaluator.GetHavingItemsString());
					exm.Console.NewLine();
					break;
				case FunctionCode.PRINT_SHOPITEM://ショップで売っているアイテム
					{
						if (skipPrint)
							break;
						int length = Math.Min(vEvaluator.ITEMSALES.Length, vEvaluator.ITEMNAME.Length);
						if (length > vEvaluator.ITEMPRICE.Length)
							length = vEvaluator.ITEMPRICE.Length;
						int count = 0;
						for (int i = 0; i < length; i++)
						{
							if (vEvaluator.ItemSales(i))
							{
								string printStr = vEvaluator.ITEMNAME[i];
								if (printStr == null)
									printStr = "";
								Int64 price = vEvaluator.ITEMPRICE[i];
								// 1.52a改変部分　（単位の差し替えおよび前置、後置に対応）
								if (Config.MoneyFirst)
									exm.Console.PrintC(string.Format("[{2}] {0}({3}{1})", printStr, price, i, Config.MoneyLabel), false);
								else
									exm.Console.PrintC(string.Format("[{2}] {0}({1}{3})", printStr, price, i, Config.MoneyLabel), false);
								count++;
								if ((Config.PrintCPerLine > 0) && (count % Config.PrintCPerLine == 0))
									exm.Console.PrintFlush(false);
							}
						}
						exm.Console.PrintFlush(false);
						exm.Console.RefreshStrings(false);
					}
					break;
				case FunctionCode.UPCHECK://パラメータの変動
					vEvaluator.UpdateInUpcheck(exm.Console, skipPrint);
					break;
				case FunctionCode.CUPCHECK://パラメータの変動(任意キャラ版)
					{
						ExpressionArgument intExpArg = (ExpressionArgument)func.Argument;
						Int64 target = intExpArg.Term.GetIntValue(exm);
						vEvaluator.CUpdateInUpcheck(exm.Console, target, skipPrint);
					}
					break;
				case FunctionCode.DELALLCHARA:
					{
						vEvaluator.DelAllCharacter();
						break;
					}
				case FunctionCode.PICKUPCHARA:
					{
						ExpressionArrayArgument intExpArg = (ExpressionArrayArgument)func.Argument;
						Int64[] NoList = new Int64[intExpArg.TermList.Length];
						Int64 charaNum = vEvaluator.CHARANUM;
						for (int i = 0; i < intExpArg.TermList.Length; i++)
						{
							IOperandTerm term_i = intExpArg.TermList[i];
							NoList[i] = term_i.GetIntValue(exm);
							if (!(term_i is VariableTerm) || ((((VariableTerm)term_i).Identifier.Code != VariableCode.MASTER) && (((VariableTerm)term_i).Identifier.Code != VariableCode.ASSI) && (((VariableTerm)term_i).Identifier.Code != VariableCode.TARGET)))
								if (NoList[i] < 0 || NoList[i] >= charaNum)
									throw new CodeEE("命令PICKUPCHARAの第" + (i + 1).ToString() + "引数にキャラリストの範囲外の値(" + NoList[i].ToString() + ")が与えられました");
						}
						vEvaluator.PickUpChara(NoList);
					}
					break;
				case FunctionCode.ADDDEFCHARA:
					{
						//デバッグコマンドなら通す
						if ((func.ParentLabelLine != null) && (func.ParentLabelLine.LabelName != "SYSTEM_TITLE"))
							throw new CodeEE("@SYSTEM_TITLE以外でこの命令を使うことはできません");
						vEvaluator.AddCharacterFromCsvNo(0);
						if (GlobalStatic.GameBaseData.DefaultCharacter > 0)
							vEvaluator.AddCharacterFromCsvNo(GlobalStatic.GameBaseData.DefaultCharacter);
						break;
					}
				case FunctionCode.PUTFORM://@SAVEINFO関数でのみ使用可能。PRINTFORMと同様の書式でセーブデータに概要をつける。
					{
						term = ((ExpressionArgument)func.Argument).Term;
						str = term.GetStrValue(exm);
						if (vEvaluator.SAVEDATA_TEXT != null)
							vEvaluator.SAVEDATA_TEXT += str;
						else
							vEvaluator.SAVEDATA_TEXT = str;
						break;
					}
				case FunctionCode.QUIT://ゲームを終了
					exm.Console.Quit();
					break;

				case FunctionCode.VARSIZE:
					{
						SpVarsizeArgument versizeArg = (SpVarsizeArgument)func.Argument;
						VariableToken varID = versizeArg.VariableID;
						vEvaluator.VarSize(varID);
					}
					break;
				case FunctionCode.SAVEDATA:
					{
						SpSaveDataArgument spSavedataArg = (SpSaveDataArgument)func.Argument;
						Int64 target = spSavedataArg.Target.GetIntValue(exm);
						if (target < 0)
							throw new CodeEE("SAVEDATAの引数に負の値(" + target.ToString() + ")が指定されました");
						else if (target > int.MaxValue)
							throw new CodeEE("SAVEDATAの引数(" + target.ToString() + ")が大きすぎます");
						string savemes = spSavedataArg.StrExpression.GetStrValue(exm);
						if (savemes.Contains("\n"))
							throw new CodeEE("SAVEDATAのセーブテキストに改行文字が与えられました（セーブデータが破損するため改行文字は使えません）");
						if (!vEvaluator.SaveTo((int)target, savemes))
						{
							console.PrintError("SAVEDATA命令によるセーブ中に予期しないエラーが発生しました");
						}
					}
					break;

				case FunctionCode.POWER:
					{
						SpPowerArgument powerArg = (SpPowerArgument)func.Argument;
						double x = powerArg.X.GetIntValue(exm);
						double y = powerArg.Y.GetIntValue(exm);
						double pow = Math.Pow(x, y);
						if (double.IsNaN(pow))
							throw new CodeEE("累乗結果が非数値です");
						else if (double.IsInfinity(pow))
							throw new CodeEE("累乗結果が無限大です");
						else if ((pow >= Int64.MaxValue) || (pow <= Int64.MinValue))
							throw new CodeEE("累乗結果(" + pow.ToString() + ")が64ビット符号付き整数の範囲外です");
						powerArg.VariableDest.SetValue((long)pow, exm);
						break;
					}
				case FunctionCode.SWAP:
					{
						SpSwapVarArgument arg = (SpSwapVarArgument)func.Argument;
						//1756beta2+v11
						//値を読み出す前に添え字を確定させておかないと、RANDが添え字にある場合正しく処理できない
						FixedVariableTerm vTerm1 = arg.var1.GetFixedVariableTerm(exm);
						FixedVariableTerm vTerm2 = arg.var2.GetFixedVariableTerm(exm);
						if (vTerm1.GetOperandType() != vTerm2.GetOperandType())
							throw new CodeEE("入れ替える変数の型が異なります");
						if (vTerm1.GetOperandType() == typeof(Int64))
						{
							Int64 temp = vTerm1.GetIntValue(exm);
							vTerm1.SetValue(vTerm2.GetIntValue(exm), exm);
							vTerm2.SetValue(temp, exm);
						}
						else if (arg.var1.GetOperandType() == typeof(string))
						{
							string temps = vTerm1.GetStrValue(exm);
							vTerm1.SetValue(vTerm2.GetStrValue(exm), exm);
							vTerm2.SetValue(temps, exm);
						}
						else
						{
							throw new CodeEE("不明な変数型です");
						}
						break;
					}
				case FunctionCode.GETTIME:
					{
						long date = DateTime.Now.Year;
						date = date * 100 + DateTime.Now.Month;
						date = date * 100 + DateTime.Now.Day;
						date = date * 100 + DateTime.Now.Hour;
						date = date * 100 + DateTime.Now.Minute;
						date = date * 100 + DateTime.Now.Second;
						date = date * 1000 + DateTime.Now.Millisecond;
						vEvaluator.RESULT = date;//17桁。2京くらい。
						vEvaluator.RESULTS = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
					}
					break;
				case FunctionCode.SETCOLOR:
					{
						SpColorArgument colorArg = (SpColorArgument)func.Argument;
						Int64 colorR;
						Int64 colorG;
						Int64 colorB;
						if (colorArg.RGB != null)
						{
							Int64 colorRGB = colorArg.RGB.GetIntValue(exm);
							colorR = (colorRGB & 0xFF0000) >> 16;
							colorG = (colorRGB & 0x00FF00) >> 8;
							colorB = (colorRGB & 0x0000FF);
						}
						else
						{
							colorR = colorArg.R.GetIntValue(exm);
							colorG = colorArg.G.GetIntValue(exm);
							colorB = colorArg.B.GetIntValue(exm);
							if ((colorR < 0) || (colorG < 0) || (colorB < 0))
								throw new CodeEE("SETCOLORの引数に0未満の値が指定されました");
							if ((colorR > 255) || (colorG > 255) || (colorB > 255))
								throw new CodeEE("SETCOLORの引数に255を超える値が指定されました");
						}
						Color c = Color.FromArgb((Int32)colorR, (Int32)colorG, (Int32)colorB);
						exm.Console.SetStringStyle(c);
					}
					break;
				case FunctionCode.SETCOLORBYNAME:
					{
						string colorName = func.Argument.ConstStr;
						Color c = Color.FromName(colorName);
						if (c.A == 0)
						{
							if (str.Equals("transparent", StringComparison.OrdinalIgnoreCase))
								throw new CodeEE("無色透明(Transparent)は色として指定できません");
							throw new CodeEE("指定された色名\"" + colorName + "\"は無効な色名です");
						}
						exm.Console.SetStringStyle(c);
					}
					break;
				case FunctionCode.SETBGCOLOR:
					{
						SpColorArgument colorArg = (SpColorArgument)func.Argument;
						Int64 colorR;
						Int64 colorG;
						Int64 colorB;
						if (colorArg.IsConst)
						{
							Int64 colorRGB = colorArg.ConstInt;
							colorR = (colorRGB & 0xFF0000) >> 16;
							colorG = (colorRGB & 0x00FF00) >> 8;
							colorB = (colorRGB & 0x0000FF);
						}
						else if (colorArg.RGB != null)
						{
							Int64 colorRGB = colorArg.RGB.GetIntValue(exm);
							colorR = (colorRGB & 0xFF0000) >> 16;
							colorG = (colorRGB & 0x00FF00) >> 8;
							colorB = (colorRGB & 0x0000FF);
						}
						else
						{
							colorR = colorArg.R.GetIntValue(exm);
							colorG = colorArg.G.GetIntValue(exm);
							colorB = colorArg.B.GetIntValue(exm);
							if ((colorR < 0) || (colorG < 0) || (colorB < 0))
								throw new CodeEE("SETCOLORの引数に0未満の値が指定されました");
							if ((colorR > 255) || (colorG > 255) || (colorB > 255))
								throw new CodeEE("SETCOLORの引数に255を超える値が指定されました");
						}
						Color c = Color.FromArgb((Int32)colorR, (Int32)colorG, (Int32)colorB);
						exm.Console.SetBgColor(c);
					}
					break;
				case FunctionCode.SETBGCOLORBYNAME:
					{
						string colorName = func.Argument.ConstStr;
						Color c = Color.FromName(colorName);
						if (c.A == 0)
						{
							if (str.Equals("transparent", StringComparison.OrdinalIgnoreCase))
								throw new CodeEE("無色透明(Transparent)は色として指定できません");
							throw new CodeEE("指定された色名\"" + colorName + "\"は無効な色名です");
						}
						exm.Console.SetBgColor(c);
					}
					break;
				case FunctionCode.FONTSTYLE:
					{
						FontStyle fs = FontStyle.Regular;
						if (func.Argument.IsConst)
							iValue = func.Argument.ConstInt;
						else
							iValue = ((ExpressionArgument)func.Argument).Term.GetIntValue(exm);
						if ((iValue & 1) != 0)
							fs |= FontStyle.Bold;
						if ((iValue & 2) != 0)
							fs |= FontStyle.Italic;
						if ((iValue & 4) != 0)
							fs |= FontStyle.Strikeout;
						if ((iValue & 8) != 0)
							fs |= FontStyle.Underline;
						exm.Console.SetStringStyle(fs);
					}
					break;
				case FunctionCode.SETFONT:
					if (func.Argument.IsConst)
						str = func.Argument.ConstStr;
					else
						str = ((ExpressionArgument)func.Argument).Term.GetStrValue(exm);
					exm.Console.SetFont(str);
					break;
				case FunctionCode.ALIGNMENT:
					str = func.Argument.ConstStr;
					if (str.Equals("LEFT", Config.SCVariable))
						exm.Console.Alignment = DisplayLineAlignment.LEFT;
					else if (str.Equals("CENTER", Config.SCVariable))
						exm.Console.Alignment = DisplayLineAlignment.CENTER;
					else if (str.Equals("RIGHT", Config.SCVariable))
						exm.Console.Alignment = DisplayLineAlignment.RIGHT;
					else
						throw new CodeEE("ALIGNMENTのキーワード\"" + str + "\"は未定義です");
					break;

				case FunctionCode.REDRAW:
					if (func.Argument.IsConst)
						iValue = func.Argument.ConstInt;
					else
						iValue = ((ExpressionArgument)func.Argument).Term.GetIntValue(exm);
					exm.Console.SetRedraw(iValue);
					break;

				case FunctionCode.RESET_STAIN:
					{
						if (func.Argument.IsConst)
							iValue = func.Argument.ConstInt;
						else
							iValue = ((ExpressionArgument)func.Argument).Term.GetIntValue(exm);
						vEvaluator.SetDefaultStain(iValue);
					}
					break;
				case FunctionCode.SPLIT:
					{
						SpSplitArgument spSplitArg = (SpSplitArgument)func.Argument;
						string target = spSplitArg.TargetStr.GetStrValue(exm);
						string[] split = new string[] { spSplitArg.Split.GetStrValue(exm) };
						string[] retStr = target.Split(split, StringSplitOptions.None);
						spSplitArg.Num.SetValue(retStr.Length, exm);
						if (retStr.Length > spSplitArg.Var.GetLength(0))
						{
							string[] temp = retStr;
							retStr = new string[spSplitArg.Var.GetLength(0)];
							Array.Copy(temp, retStr, retStr.Length);
							//throw new CodeEE("SPLITによる分割後の文字列の数が配列変数の要素数を超えています");
						}
						spSplitArg.Var.SetValue(retStr, new long[] { 0, 0, 0 });
					}
					break;
				case FunctionCode.PRINTCPERLINE:
					{
						SpGetIntArgument spGetintArg = (SpGetIntArgument)func.Argument;
						spGetintArg.VarToken.SetValue((Int64)Config.PrintCPerLine, exm);
					}
					break;
				case FunctionCode.SAVENOS:
					{
						SpGetIntArgument spGetintArg = (SpGetIntArgument)func.Argument;
						spGetintArg.VarToken.SetValue((Int64)Config.SaveDataNos, exm);
					}
					break;
				case FunctionCode.FORCEKANA:
					if (func.Argument.IsConst)
						iValue = func.Argument.ConstInt;
					else
						iValue = ((ExpressionArgument)func.Argument).Term.GetIntValue(exm);
					exm.ForceKana(iValue);
					break;
				case FunctionCode.SKIPDISP:
					{
						iValue = (func.Argument.IsConst) ? func.Argument.ConstInt : ((ExpressionArgument)func.Argument).Term.GetIntValue(exm);
						skipPrint = (iValue != 0);
						userDefinedSkip = (iValue != 0);
						vEvaluator.RESULT = (skipPrint) ? 1L : 0L;
					}
					break;
				case FunctionCode.NOSKIP:
					{
						if (func.JumpTo == null)
							throw new CodeEE("対応するENDNOSKIPのないNOSKIPです");
						saveSkip = skipPrint;
						if (skipPrint)
							skipPrint = false;
					}
					break;
				case FunctionCode.ENDNOSKIP:
					{
						if (func.JumpTo == null)
							throw new CodeEE("対応するNOSKIPのないENDNOSKIPです");
						if (saveSkip)
							skipPrint = true;
					}
					break;
				case FunctionCode.OUTPUTLOG:
					exm.Console.OutputLog(null);
					break;
				case FunctionCode.ARRAYSHIFT: //配列要素をずらす
					{
						SpArrayShiftArgument arrayArg = (SpArrayShiftArgument)func.Argument;
						if (!arrayArg.VarToken.Identifier.IsArray1D)
							throw new CodeEE("ARRAYSHIFTは1次元配列および配列型キャラクタ変数のみに対応しています");
						FixedVariableTerm dest = arrayArg.VarToken.GetFixedVariableTerm(exm);
						int shift = (int)arrayArg.Num1.GetIntValue(exm);
						if (shift == 0)
							break;
						int start = (int)arrayArg.Num3.GetIntValue(exm);
						if (start < 0)
							throw new CodeEE("ARRAYSHIFTの第４引数が負の値(" + start.ToString() + ")です");
						int num;
						if (arrayArg.Num4 != null)
						{
							num = (int)arrayArg.Num4.GetIntValue(exm);
							if (num < 0)
								throw new CodeEE("ARRAYSHIFTの第５引数が負の値(" + num.ToString() + ")です");
							if (num == 0)
								break;
						}
						else
							num = -1;
						if (dest.Identifier.IsInteger)
						{
							Int64 def = arrayArg.Num2.GetIntValue(exm);
							vEvaluator.ShiftArray(dest, shift, def, start, num);
						}
						else
						{
							string defs = arrayArg.Num2.GetStrValue(exm);
							vEvaluator.ShiftArray(dest, shift, defs, start, num);
						}
						break;
					}
				case FunctionCode.ARRAYREMOVE:
					{
						SpArrayControlArgument arrayArg = (SpArrayControlArgument)func.Argument;
						if (!arrayArg.VarToken.Identifier.IsArray1D)
							throw new CodeEE("ARRAYREMOVEは1次元配列および配列型キャラクタ変数のみに対応しています");
						FixedVariableTerm p = arrayArg.VarToken.GetFixedVariableTerm(exm);
						int start = (int)arrayArg.Num1.GetIntValue(exm);
						int num = (int)arrayArg.Num2.GetIntValue(exm);
						if (start < 0)
							throw new CodeEE("ARRAYREMOVEの第２引数が負の値(" + start.ToString() + ")です");
						if (num < 0)
							throw new CodeEE("ARRAYREMOVEの第３引数が負の値(" + start.ToString() + ")です");
						if (num == 0)
							break;
						vEvaluator.RemoveArray(p, start, num);
						break;
					}
				case FunctionCode.ARRAYSORT:
					{
						SpArraySortArgument arrayArg = (SpArraySortArgument)func.Argument;
						if (!arrayArg.VarToken.Identifier.IsArray1D)
							throw new CodeEE("ARRAYRESORTは1次元配列および配列型キャラクタ変数のみに対応しています");
						FixedVariableTerm p = arrayArg.VarToken.GetFixedVariableTerm(exm);
						int start = (int)arrayArg.Num1.GetIntValue(exm);
						if (start < 0)
							throw new CodeEE("ARRAYSORTの第３引数が負の値(" + start.ToString() + ")です");
						int num = 0;
						if (arrayArg.Num2 != null)
						{
							num = (int)arrayArg.Num2.GetIntValue(exm);
							if (num < 0)
								throw new CodeEE("ARRAYSORTの第４引数が負の値(" + start.ToString() + ")です");
							if (num == 0)
								break;
						}
						else
							num = -1;
						vEvaluator.SortArray(p, arrayArg.Order, start, num);
						break;
					}
				case FunctionCode.ARRAYCOPY:
					{
						SpCopyArrayArgument arrayArg = (SpCopyArrayArgument)func.Argument;
						IOperandTerm varName1 = arrayArg.VarName1;
						IOperandTerm varName2 = arrayArg.VarName2;
						VariableToken[] vars = new VariableToken[2] { null, null };
						if (!(varName1 is SingleTerm) || !(varName2 is SingleTerm))
						{
							string[] names = new string[2] { null, null };
							names[0] = varName1.GetStrValue(exm);
							names[1] = varName2.GetStrValue(exm);
							if ((vars[0] = GlobalStatic.IdentifierDictionary.GetVariableToken(names[0], null, true)) == null)
								throw new CodeEE("ARRAYCOPY命令の第１引数(" + names[0] + ")が有効な変数名ではありません");
							if (!vars[0].IsArray1D && !vars[0].IsArray2D && !vars[0].IsArray3D)
								throw new CodeEE("ARRAYCOPY命令の第１引数\"" + names[0] + "\"は配列変数ではありません");
							if (vars[0].IsCharacterData)
								throw new CodeEE("ARRAYCOPY命令の第１引数\"" + names[0] + "\"はキャラクタ変数です（対応していません）");
							if ((vars[1] = GlobalStatic.IdentifierDictionary.GetVariableToken(names[1], null, true)) == null)
								throw new CodeEE("ARRAYCOPY命令の第２引数(" + names[0] + ")が有効な変数名ではありません");
							if (!vars[1].IsArray1D && !vars[1].IsArray2D && !vars[1].IsArray3D)
								throw new CodeEE("ARRAYCOPY命令の第２引数\"" + names[1] + "\"は配列変数ではありません");
							if (vars[1].IsCharacterData)
								throw new CodeEE("ARRAYCOPY命令の第２引数\"" + names[1] + "\"はキャラクタ変数です（対応していません）");
							if (vars[1].IsConst)
								throw new CodeEE("ARRAYCOPY命令の第２引数\"" + names[1] + "\"は値を変更できない変数です");
							if ((vars[0].IsArray1D && !vars[1].IsArray1D) || (vars[0].IsArray2D && !vars[1].IsArray2D) || (vars[0].IsArray3D && !vars[1].IsArray3D))
								throw new CodeEE("ARRAYCOPY命令の２つの配列変数の次元数が一致していません");
							if ((vars[0].IsInteger && vars[1].IsString) || (vars[0].IsString && vars[1].IsInteger))
								throw new CodeEE("ARRAYCOPY命令の２つの配列変数の型が一致していません");
						}
						else
						{
							vars[0] = GlobalStatic.IdentifierDictionary.GetVariableToken(((SingleTerm)varName1).Str, null, true);
							vars[1] = GlobalStatic.IdentifierDictionary.GetVariableToken(((SingleTerm)varName2).Str, null, true);
							if ((vars[0].IsInteger && vars[1].IsString) || (vars[0].IsString && vars[1].IsInteger))
								throw new CodeEE("ARRAYCOPY命令の２つの配列変数の型が一致していません");
						}
						vEvaluator.CopyArray(vars[0], vars[1]);
					}
					break;
				case FunctionCode.ENCODETOUNI:
					{
						//int length = Encoding.UTF32.GetEncoder().GetByteCount(target.ToCharArray(), 0, target.Length, false);
						//byte[] bytes = new byte[length];
						//Encoding.UTF32.GetEncoder().GetBytes(target.ToCharArray(), 0, target.Length, bytes, 0, false);
						//vEvaluator.setEncodingResult(bytes);
						term = ((ExpressionArgument)func.Argument).Term;
						string target = term.GetStrValue(exm);

						int length = vEvaluator.RESULT_ARRAY.Length;
						// result:0には長さが入るのでその分-1
						if (target.Length > length - 1)
							throw new CodeEE(String.Format("ENCODETOUNIの引数が長すぎます（現在{0}文字。最大{1}文字まで）", target.Length, length - 1));

						int[] ary = new int[target.Length];
						for (int i = 0; i < target.Length; i++)
							ary[i] = char.ConvertToUtf32(target, i);
						vEvaluator.SetEncodingResult(ary);
					}
					break;
				case FunctionCode.ASSERT:
					if (((ExpressionArgument)func.Argument).Term.GetIntValue(exm) == 0)
						throw new CodeEE("ASSERT文の引数が0です");
					break;
				case FunctionCode.THROW:
					throw new CodeEE(((ExpressionArgument)func.Argument).Term.GetStrValue(exm));
				case FunctionCode.CLEARTEXTBOX:
					GlobalStatic.MainWindow.clear_richText();
					break;
				case FunctionCode.STRDATA:
					{
						//表示データが空なら何もしないで飛ぶ
						if (func.dataList.Count == 0)
						{
							state.JumpTo(func.JumpTo);
							return;
						}
						int count = func.dataList.Count;
						int choice = (int)exm.VEvaluator.GetNextRand(count);
						List<InstructionLine> iList = func.dataList[choice];
						int i = 0;
						foreach (InstructionLine selectedLine in iList)
						{
							state.CurrentLine = selectedLine;
							if (selectedLine.Argument == null)
								ArgumentParser.SetArgumentTo(selectedLine);
							term = ((ExpressionArgument)selectedLine.Argument).Term;
							str += term.GetStrValue(exm);
							if (++i < (int)iList.Count)
								str += "\n";
						}
						((StrDataArgument)func.Argument).Var.SetValue(str, exm);
						//ジャンプするが、流れが連続であることを保証。
						state.JumpTo(func.JumpTo);
						break;
					}
#if UEMUERA_DEBUG
				default:
					throw new ExeEE("未定義の関数");
#endif
			}
			return;
		}

		bool saveSkip = false;
		bool userDefinedSkip = false;

		#endregion

		#region flow control

		bool doFlowControlFunction(InstructionLine func)
		{
			switch (func.FunctionCode)
			{
				case FunctionCode.LOADDATA:
					{
						ExpressionArgument intExpArg = (ExpressionArgument)func.Argument;
						Int64 target = intExpArg.Term.GetIntValue(exm);
						if (target < 0)
							throw new CodeEE("LOADDATAの引数に負の値(" + target.ToString() + ")が指定されました");
						else if (target > int.MaxValue)
							throw new CodeEE("LOADDATAの引数(" + target.ToString() + ")が大きすぎます");
						//EraDataResult result = vEvaluator.checkData((int)target);
						EraDataResult result = vEvaluator.CheckData((int)target, EraSaveFileType.Normal);
						if (result.State != EraDataState.OK)
							throw new CodeEE("不正なデータをロードしようとしました");

						if (!vEvaluator.LoadFrom((int)target))
							throw new ExeEE("ファイルのロード中に予期しないエラーが発生しました");
						state.ClearFunctionList();
						state.SystemState = SystemStateCode.LoadData_DataLoaded;
						return false;
					}

				case FunctionCode.TRYCALLLIST:
				case FunctionCode.TRYJUMPLIST:
					{
						//if (!sequential)//RETURNで帰ってきた
						//{
						//	state.JumpTo(func.JumpTo);
						//	break;
						//}
						string funcName = "";
						CalledFunction callto = null;
						SpCallArgment cfa = null;
						foreach (InstructionLine iLine in func.callList)
						{

							cfa = (SpCallArgment)iLine.Argument;
							funcName = cfa.FuncnameTerm.GetStrValue(exm);
							if (Config.ICFunction)
								funcName = funcName.ToUpper();
							callto = CalledFunction.CallFunction(this, funcName, func.JumpTo);
							if (callto == null)
								continue;
							callto.IsJump = func.Function.IsJump();
							string errMes;
							UserDefinedFunctionArgument args = callto.ConvertArg(cfa.RowArgs, out errMes);
							if (args == null)
								throw new CodeEE(errMes);
							state.IntoFunction(callto, args, exm);
							return true;
						}
						state.JumpTo(func.JumpTo);
					}
					break;
				case FunctionCode.TRYGOTOLIST:
					{
						string funcName = "";
						LogicalLine jumpto = null;
						foreach (InstructionLine iLine in func.callList)
						{
							if (iLine.Argument == null)
								ArgumentParser.SetArgumentTo(iLine);
							funcName = ((SpCallArgment)iLine.Argument).FuncnameTerm.GetStrValue(exm);
							if (Config.ICVariable)
								funcName = funcName.ToUpper();
							jumpto = state.CurrentCalled.CallLabel(this, funcName);
							if (jumpto != null)
								break;
						}
						if (jumpto == null)
							state.JumpTo(func.JumpTo);
						else
							state.JumpTo(jumpto);
					}
					break;
				case FunctionCode.CALLTRAIN:
					{
						ExpressionArgument intExpArg = (ExpressionArgument)func.Argument;
						Int64 count = intExpArg.Term.GetIntValue(exm);
						SetCommnds(count);
						return false;
					}
				case FunctionCode.STOPCALLTRAIN:
					{
						if (isCTrain)
						{
							ClearCommands();
							skipPrint = false;
						}
						return false;
					}
				case FunctionCode.DOTRAIN:
					{
						switch (state.SystemState)
						{
							//case SystemStateCode.Train_Begin://BEGIN TRAINから。
							case SystemStateCode.Train_CallEventTrain://@EVENTTRAINの呼び出し中。スキップ可能
							case SystemStateCode.Train_CallShowStatus://@SHOW_STATUSの呼び出し中
							//case SystemStateCode.Train_CallComAbleXX://@COM_ABLExxの呼び出し中。
							case SystemStateCode.Train_CallShowUserCom://@SHOW_USERCOMの呼び出し中
							//case SystemStateCode.Train_WaitInput://入力待ち状態。選択が実行可能ならEVENTCOMからCOMxx、そうでなければ@USERCOMにRESULTを渡す
							//case SystemStateCode.Train_CallEventCom://@EVENTCOMの呼び出し中
							//case SystemStateCode.Train_CallComXX://@COMxxの呼び出し中
							//case SystemStateCode.Train_CallSourceCheck://@SOURCE_CHECKの呼び出し中
							case SystemStateCode.Train_CallEventComEnd://@EVENTCOMENDの呼び出し中。スキップ可能。Train_CallEventTrainへ帰る。@USERCOMの呼び出し中もここ
								break;
							default:
								exm.Console.PrintSystemLine(state.SystemState.ToString());
								throw new CodeEE("DOTRAIN命令をこの位置で実行することはできません");
						}
						coms.Clear();
						isCTrain = false;
						this.count = 0;

						Int64 train = ((ExpressionArgument)func.Argument).Term.GetIntValue(exm);
						if (train < 0)
							throw new CodeEE("DOTRAIN命令に0未満の値が渡されました");
						if (train >= TrainName.Length)
							throw new CodeEE("DOTRAIN命令にTRAINNAMEの配列数以上の値が渡されました");
						doTrainSelectCom = train;
						state.SystemState = SystemStateCode.Train_DoTrain;
						return false;
					}
#if UEMUERA_DEBUG
				default:
					throw new ExeEE("未定義の関数です");
#endif
			}
			return true;
		}




		List<ProcessState> prevStateList = new List<ProcessState>();
		public void saveCurrentState(bool single)
		{
			//怖いところだが、現状起こらない現象なので一旦消してみる
			//if (single && (prevStateList.Count > 0))
			//	throw new ExeEE("記憶している状態があるのに再度記憶しようとした");
			if (state != null)
			{
				prevStateList.Add(state);
				state = state.Clone();
			}
		}

		public void loadPrevState()
		{
			//怖いところだが、現状起こらない現象なので一旦消してみる
			//if (prevStateList.Count == 0)
			//	throw new ExeEE("記憶している状態がないのに呼び戻しされた");
			if (state != null)
			{
				state.ClearFunctionList();
				state = prevStateList[prevStateList.Count - 1];
				deletePrevState();
			}
		}

		private void deletePrevState()
		{
			if (prevStateList.Count == 0)
				return;
			prevStateList.RemoveAt(prevStateList.Count - 1);
		}

		private void deleteAllPrevState()
		{
			foreach (ProcessState state in prevStateList)
				state.ClearFunctionList();
			prevStateList.Clear();
		}

		public ProcessState getCurrentState
		{
			get { return state; }
		}
		#endregion
	}
}
