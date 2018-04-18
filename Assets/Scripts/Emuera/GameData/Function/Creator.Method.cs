using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.Sub;
using MinorShift.Emuera.GameProc;
using MinorShift._Library;
using MinorShift.Emuera.GameData.Variable;
//using System.Drawing;
//using Microsoft.VisualBasic;
//using System.Windows.Forms;
using MinorShift.Emuera.GameView;
using uEmuera.Drawing;
using uEmuera.VisualBasic;

namespace MinorShift.Emuera.GameData.Function
{

    internal static partial class FunctionMethodCreator
    {
        #region CSVデータ関係
        private sealed class GetcharaMethod : FunctionMethod
        {
            public GetcharaMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                CanRestructure = false;
            }
            
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                //通常２つ、１つ省略可能で１～２の引数が必要。
                if (arguments.Length < 1)
                    return name + "関数には少なくとも1つの引数が必要です";
                if (arguments.Length > 2)
                    return name + "関数の引数が多すぎます";

                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                if (arguments[0].GetOperandType() != typeof(Int64))
                    return name + "関数の1番目の引数の型が正しくありません";
                //2は省略可能
                if ((arguments.Length == 2) && (arguments[1] != null) && (arguments[1].GetOperandType() != typeof(Int64)))
                    return name + "関数の2番目の引数の型が正しくありません";
                return null;
            }
			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 integer = arguments[0].GetIntValue(exm);
                Int64 chara = -1L;

				if (!Config.CompatiSPChara)
				{
					//if ((arguments.Length > 1) && (arguments[1] != null) && (arguments[1].GetIntValue(exm) != 0))
					return exm.VEvaluator.GetChara(integer);
				}
				//以下互換性用の旧処理
                bool CheckSp = false;
                if ((arguments.Length > 1) && (arguments[1] != null) && (arguments[1].GetIntValue(exm) != 0))
                    CheckSp = true;
                if (CheckSp)
                {
                    chara = exm.VEvaluator.GetChara_UseSp(integer, false);
                    if (chara != -1)
                        return chara;
                    else
                        return exm.VEvaluator.GetChara_UseSp(integer, true);
                }
                else
                    return exm.VEvaluator.GetChara_UseSp(integer, false);
            }
        }

        private sealed class GetspcharaMethod : FunctionMethod
        {
            public GetspcharaMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(Int64) };
                CanRestructure = false;
            }
			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
				if(!Config.CompatiSPChara)
					throw new CodeEE("SPキャラ関係の機能は標準では使用できません(互換性オプション「SPキャラを使用する」をONにしてください)");
                Int64 integer = arguments[0].GetIntValue(exm);
                return exm.VEvaluator.GetChara_UseSp(integer, true);
            }
        }

        private sealed class CsvStrDataMethod : FunctionMethod
        {
            CharacterStrData charaStr;
            public CsvStrDataMethod()
            {
                ReturnType = typeof(string);
				argumentTypeArray = null;
                charaStr = CharacterStrData.NAME;
                CanRestructure = true;
            }
            public CsvStrDataMethod(CharacterStrData cStr)
            {
                ReturnType = typeof(string);
				argumentTypeArray = null;
				charaStr = cStr;
				CanRestructure = true;
            }
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                if (arguments.Length < 1)
                    return name + "関数には少なくとも1つの引数が必要です";
                if (arguments.Length > 2)
                    return name + "関数の引数が多すぎます";
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                if (!arguments[0].IsInteger)
                    return name + "関数の1番目の引数が数値ではありません";
                if (arguments.Length == 1)
                    return null;
                if ((arguments[1] != null) && (arguments[1].GetOperandType() != typeof(Int64)))
                    return name + "関数の2番目の変数が数値ではありません";
                return null;
            }
            public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                long x = arguments[0].GetIntValue(exm);
				long y = (arguments.Length > 1 && arguments[1] != null) ? arguments[1].GetIntValue(exm) : 0;
				if (!Config.CompatiSPChara && y != 0)
					throw new CodeEE("SPキャラ関係の機能は標準では使用できません(互換性オプション「SPキャラを使用する」をONにしてください)");
                return exm.VEvaluator.GetCharacterStrfromCSVData(x, charaStr, (y != 0), 0);
            }
        }

        private sealed class CsvcstrMethod : FunctionMethod
        {
            public CsvcstrMethod()
            {
                ReturnType = typeof(string);
                argumentTypeArray = null;
                CanRestructure = true;
            }
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                if (arguments.Length < 2)
                    return name + "関数には少なくとも2つの引数が必要です";
                if (arguments.Length > 3)
                    return name + "関数の引数が多すぎます";
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                if (!arguments[0].IsInteger)
                    return name + "関数の1番目の引数が数値ではありません";
                if (arguments[1] == null)
                    return name + "関数の2番目の引数は省略できません";
                if (arguments[1].GetOperandType() != typeof(Int64))
                    return name + "関数の2番目の変数が数値ではありません";
                if (arguments.Length == 2)
                    return null;
                if ((arguments[2] != null) && (arguments[2].GetOperandType() != typeof(Int64)))
                    return name + "関数の3番目の変数が数値ではありません";
                return null;
            }
            public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                long x = arguments[0].GetIntValue(exm);
                long y = arguments[1].GetIntValue(exm);
                long z = (arguments.Length == 3 && arguments[2] != null) ? arguments[2].GetIntValue(exm) : 0;
				if(!Config.CompatiSPChara && z != 0)
					throw new CodeEE("SPキャラ関係の機能は標準では使用できません(互換性オプション「SPキャラを使用する」をONにしてください)");
                return exm.VEvaluator.GetCharacterStrfromCSVData(x, CharacterStrData.CSTR, (z != 0), y);
            }
        }

        private sealed class CsvDataMethod : FunctionMethod
        {
            CharacterIntData charaInt;
            public CsvDataMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                charaInt = CharacterIntData.BASE;
                CanRestructure = true;
            }
            public CsvDataMethod(CharacterIntData cInt)
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
				charaInt = cInt;
				CanRestructure = true;
            }
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                if (arguments.Length < 2)
                    return name + "関数には少なくとも2つの引数が必要です";
                if (arguments.Length > 3)
                    return name + "関数の引数が多すぎます";
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                if (!arguments[0].IsInteger)
                    return name + "関数の1番目の引数が数値ではありません";
                if (arguments[1] == null)
                    return name + "関数の2番目の引数は省略できません";
                if (arguments[1].GetOperandType() != typeof(Int64))
                    return name + "関数の2番目の変数が数値ではありません";
                if (arguments.Length == 2)
                    return null;
                if ((arguments[2] != null) && (arguments[2].GetOperandType() != typeof(Int64)))
                    return name + "関数の3番目の変数が数値ではありません";
                return null;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                long x = arguments[0].GetIntValue(exm);
                long y = arguments[1].GetIntValue(exm);
                long z = (arguments.Length == 3 && arguments[2] != null) ? arguments[2].GetIntValue(exm) : 0;
				if(!Config.CompatiSPChara && z != 0)
					throw new CodeEE("SPキャラ関係の機能は標準では使用できません(互換性オプション「SPキャラを使用する」をONにしてください)");
                return exm.VEvaluator.GetCharacterIntfromCSVData(x, charaInt, (z != 0), y);
            }
        }

        private sealed class FindcharaMethod : FunctionMethod
        {
            public FindcharaMethod(bool last)
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                CanRestructure = false;
                isLast = last;
            }
            bool isLast;
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                //通常3つ、1つ省略可能で2～3の引数が必要。
                if (arguments.Length < 2)
                    return name + "関数には少なくとも2つの引数が必要です";
                if (arguments.Length > 4)
                    return name + "関数の引数が多すぎます";

                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                if (!(arguments[0] is VariableTerm))
                    return name + "関数の1番目の引数の型が正しくありません";
                if (!(((VariableTerm)arguments[0]).Identifier.IsCharacterData))
                    return name + "関数の1番目の引数の変数がキャラクタ変数ではありません";
                if (arguments[1] == null)
                    return name + "関数の2番目の引数は省略できません";
                if (arguments[1].GetOperandType() != arguments[0].GetOperandType())
                    return name + "関数の2番目の引数の型が正しくありません";
                //3番目は省略可能
                if ((arguments.Length >= 3) && (arguments[2] != null) && (arguments[2].GetOperandType() != typeof(Int64)))
                    return name + "関数の3番目の引数の型が正しくありません";
                //4番目は省略可能
                if ((arguments.Length >= 4) && (arguments[3] != null) && (arguments[3].GetOperandType() != typeof(Int64)))
                    return name + "関数の4番目の引数の型が正しくありません";
                return null;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                VariableTerm vTerm = (VariableTerm)arguments[0];
                VariableToken varID = vTerm.Identifier;

                Int64 elem = 0;
                if (vTerm.Identifier.IsArray1D)
                    elem = vTerm.GetElementInt(1, exm);
                else if (vTerm.Identifier.IsArray2D)
                {
                    elem = vTerm.GetElementInt(1, exm) << 32;
                    elem += vTerm.GetElementInt(2, exm);
                }
                Int64 startindex = 0;
                Int64 lastindex = exm.VEvaluator.CHARANUM;
                if (arguments.Length >= 3 && arguments[2] != null)
                    startindex = arguments[2].GetIntValue(exm);
                if (arguments.Length >= 4 && arguments[3] != null)
                    lastindex = arguments[3].GetIntValue(exm);
                Int64 ret = -1;
                if (startindex < 0 || startindex >= exm.VEvaluator.CHARANUM)
                    throw new CodeEE((isLast ? "" : "") + "関数の第3引数(" + startindex.ToString() + ")はキャラクタ位置の範囲外です");
                if (lastindex < 0 || lastindex > exm.VEvaluator.CHARANUM)
                    throw new CodeEE((isLast ? "" : "") + "関数の第4引数(" + lastindex.ToString() + ")はキャラクタ位置の範囲外です");
                if (varID.IsString)
                {
                    string word = arguments[1].GetStrValue(exm);
                    ret = exm.VEvaluator.FindChara(varID, elem, word, startindex, lastindex, isLast);
                }
                else
                {
                    Int64 word = arguments[1].GetIntValue(exm);
                    ret = exm.VEvaluator.FindChara(varID, elem, word, startindex, lastindex, isLast);
                }
                return (ret);
            }
        }

        private sealed class ExistCsvMethod : FunctionMethod
        {
            public ExistCsvMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                CanRestructure = true;
            }
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                if (arguments.Length < 1)
                    return name + "関数には少なくとも1つの引数が必要です";
                if (arguments.Length > 2)
                    return name + "関数の引数が多すぎます";
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                if (!arguments[0].IsInteger)
                    return name + "関数の1番目の引数が数値ではありません";
                if (arguments.Length == 1)
                    return null;
                if ((arguments[1] != null) && (arguments[1].GetOperandType() != typeof(Int64)))
                    return name + "関数の2番目の変数が数値ではありません";
                return null;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 no = arguments[0].GetIntValue(exm);
                bool isSp =(arguments.Length == 2 && arguments[1] != null) ? (arguments[1].GetIntValue(exm) != 0) : false;
				if(!Config.CompatiSPChara && isSp)
					throw new CodeEE("SPキャラ関係の機能は標準では使用できません(互換性オプション「SPキャラを使用する」をONにしてください)");

                return (exm.VEvaluator.ExistCsv(no, isSp));
            }
        }
        #endregion

        #region 汎用処理系
        private sealed class VarsizeMethod : FunctionMethod
        {
            public VarsizeMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                CanRestructure = true;
				//1808beta009 参照型変数の追加によりちょっと面倒になった
				HasUniqueRestructure = true;
            }
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                if (arguments.Length < 1)
                    return name + "関数には少なくとも1つの引数が必要です";
                if (arguments.Length > 2)
                    return name + "関数の引数が多すぎます";
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                if (!arguments[0].IsString)
                    return name + "関数の1番目の引数が文字列ではありません";
                if (arguments[0] is SingleTerm)
                {
                    string varName = ((SingleTerm)arguments[0]).Str;
                    if (GlobalStatic.IdentifierDictionary.GetVariableToken(varName, null, true) == null)
                        return name + "関数の1番目の引数が変数名ではありません";
                }
                if (arguments.Length == 1)
                    return null;
                if ((arguments[1] != null) && (arguments[1].GetOperandType() != typeof(Int64)))
                    return name + "関数の2番目の変数が数値ではありません";
                if (arguments.Length == 2)
                    return null;
                return null;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                VariableToken var = GlobalStatic.IdentifierDictionary.GetVariableToken(arguments[0].GetStrValue(exm), null, true);
                if (var == null)
                    throw new CodeEE("VARSIZEの1番目の引数(\"" + arguments[0].GetStrValue(exm) + "\")が変数名ではありません");
                int dim = 0;
                if (arguments.Length == 2 && arguments[1] != null)
                    dim = (int)arguments[1].GetIntValue(exm);
                return (var.GetLength(dim));
            }
			public override bool UniqueRestructure(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				arguments[0].Restructure(exm);
				if (arguments.Length > 1)
					arguments[1].Restructure(exm);
				if (arguments[0] is SingleTerm && (arguments.Length == 1 || arguments[1] is SingleTerm))
				{
					VariableToken var = GlobalStatic.IdentifierDictionary.GetVariableToken(arguments[0].GetStrValue(exm), null, true);
					if (var == null || var.IsReference)//可変長の場合は定数化できない
						return false;
					return true;
				}
				return false;
			}
        }

        private sealed class CheckfontMethod : FunctionMethod
        {
			public CheckfontMethod()
			{
				ReturnType = typeof(Int64);
				argumentTypeArray = new Type[] { typeof(string) };
				CanRestructure = true;//起動中に変わることもそうそうないはず……
			}
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                //string str = arguments[0].GetStrValue(exm);
                //System.Drawing.Text.InstalledFontCollection ifc = new System.Drawing.Text.InstalledFontCollection();
                //Int64 isInstalled = 0;
                //foreach (System.Drawing.FontFamily ff in ifc.Families)
                //{
                //    if (ff.Name == str)
                //    {
                //        isInstalled = 1;
                //        break;
                //    }
                //}
                //return (isInstalled);
                //TODO
                return 1;
            }

        }

        private sealed class CheckdataMethod : FunctionMethod
        {
			public CheckdataMethod(string name, EraSaveFileType type)
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(Int64) };
                CanRestructure = false;
				this.name = name;
				this.type = type;
            }
			string name = null;
			EraSaveFileType type;
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 target = arguments[0].GetIntValue(exm);
                if (target < 0)
                    throw new CodeEE(name + "の引数に負の値(" + target.ToString() + ")が指定されました");
                else if (target > int.MaxValue)
                    throw new CodeEE(name + "の引数(" + target.ToString() + ")が大きすぎます");
				EraDataResult result = null;
				result = exm.VEvaluator.CheckData((int)target, type);
                exm.VEvaluator.RESULTS = result.DataMes;
                return ((long)result.State);
            }
        }

		/// <summary>
		/// ファイル名をstringで指定する版・CHKVARDATAとCHKCHARADATAはこっちに分類
		/// </summary>
		private sealed class CheckdataStrMethod : FunctionMethod
		{
			public CheckdataStrMethod(string name, EraSaveFileType type)
			{
				ReturnType = typeof(Int64);
				argumentTypeArray = new Type[] { typeof(string) };
				CanRestructure = false;
				this.name = name;
				this.type = type;
			}
			string name = null;
			EraSaveFileType type;
			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				string datFilename = arguments[0].GetStrValue(exm);
				EraDataResult result = null;
				result = exm.VEvaluator.CheckData(datFilename, type);
				exm.VEvaluator.RESULTS = result.DataMes;
				return ((long)result.State);
			}
		}

		/// <summary>
		/// ファイル探索関数
		/// </summary>
		private sealed class FindFilesMethod : FunctionMethod
		{
			public FindFilesMethod(string name, EraSaveFileType type)
			{
				ReturnType = typeof(Int64);
				argumentTypeArray = null;
				CanRestructure = false;
				this.name = name;
				this.type = type;
			}
			string name = null;
			EraSaveFileType type;

			public override string CheckArgumentType(string name, IOperandTerm[] arguments)
			{
				if (arguments.Length > 1)
					return name + "関数の引数が多すぎます";
				if (arguments.Length == 0 || arguments[0] == null)
					return null;
				if (!arguments[0].IsString)
					return name + "関数の1番目の引数が文字列ではありません";
				return null;
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				string pattern = "*";
				if (arguments.Length > 0 && arguments[0] != null)
					pattern = arguments[0].GetStrValue(exm);
				List<string> filepathes = null;
				filepathes = exm.VEvaluator.GetDatFiles(type == EraSaveFileType.CharVar, pattern);
				string[] results = exm.VEvaluator.VariableData.DataStringArray[(int)(VariableCode.RESULTS & VariableCode.__LOWERCASE__)];
				if (filepathes.Count <= results.Length)
					filepathes.CopyTo(results);
				else
					filepathes.CopyTo(0, results, 0, results.Length);
				return filepathes.Count;
			}
		}
		

        private sealed class IsSkipMethod : FunctionMethod
        {
            public IsSkipMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { };
                CanRestructure = false;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                return exm.Process.SkipPrint ? 1L : 0L;
            }
        }

		private sealed class MesSkipMethod : FunctionMethod
		{
			public MesSkipMethod(bool warn)
			{
				ReturnType = typeof(Int64);
				argumentTypeArray = null;
				CanRestructure = false;
				this.warn = warn;
			}
			bool warn;
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                if (arguments.Length > 0)
					return name + "関数の引数が多すぎます";
				if (warn)
					ParserMediator.Warn("関数MOUSESKIP()は推奨されません。代わりに関数MESSKIP()を使用してください", GlobalStatic.Process.GetScaningLine(), 1, false, false, null);
                return null;
            }
			public override long GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				return GlobalStatic.Console.MesSkip ? 1L : 0L;
			}
		}


        private sealed class GetColorMethod : FunctionMethod
        {
            public GetColorMethod(bool isDef)
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { };
                CanRestructure = isDef;
                defaultColor = isDef;
            }
            bool defaultColor;
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Color color = (defaultColor) ? Config.ForeColor : GlobalStatic.Console.StringStyle.Color;
                return (color.ToArgb() & 0xFFFFFF);
            }
        }

        private sealed class GetFocusColorMethod : FunctionMethod
        {
            public GetFocusColorMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { };
                CanRestructure = true;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                return (Config.FocusColor.ToArgb() & 0xFFFFFF);
            }
        }

        private sealed class GetBGColorMethod : FunctionMethod
        {
            public GetBGColorMethod(bool isDef)
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { };
                CanRestructure = isDef;
                defaultColor = isDef;
            }
            bool defaultColor;
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Color color = (defaultColor) ? Config.BackColor : GlobalStatic.Console.bgColor;
                return (color.ToArgb() & 0xFFFFFF);
            }
        }

        private sealed class GetStyleMethod : FunctionMethod
        {
            public GetStyleMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { };
                CanRestructure = false;
            }

            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                FontStyle fontstyle = GlobalStatic.Console.StringStyle.FontStyle;
                long ret = 0;
                if ((fontstyle & FontStyle.Bold) == FontStyle.Bold)
                    ret |= 1;
                if ((fontstyle & FontStyle.Italic) == FontStyle.Italic)
                    ret |= 2;
                if ((fontstyle & FontStyle.Strikeout) == FontStyle.Strikeout)
                    ret |= 4;
                if ((fontstyle & FontStyle.Underline) == FontStyle.Underline)
                    ret |= 8;
                return (ret);
            }
        }

        private sealed class GetFontMethod : FunctionMethod
        {
            public GetFontMethod()
            {
                ReturnType = typeof(string);
                argumentTypeArray = new Type[] { };
                CanRestructure = false;
            }
            public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                return (GlobalStatic.Console.StringStyle.Fontname);
            }
        }

        private sealed class BarStringMethod : FunctionMethod
        {
            public BarStringMethod()
            {
                ReturnType = typeof(string);
                argumentTypeArray = new Type[] { typeof(long), typeof(long), typeof(long) };
                CanRestructure = true;
            }
            public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                long var = arguments[0].GetIntValue(exm);
                long max = arguments[1].GetIntValue(exm);
                long length = arguments[2].GetIntValue(exm);
                return exm.CreateBar(var, max, length);
            }
        }

        private sealed class CurrentAlignMethod : FunctionMethod
        {
            public CurrentAlignMethod()
            {
                ReturnType = typeof(string);
                argumentTypeArray = new Type[] { };
                CanRestructure = false;
            }
            public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                if (exm.Console.Alignment == GameView.DisplayLineAlignment.LEFT)
                    return "LEFT";
                else if (exm.Console.Alignment == GameView.DisplayLineAlignment.CENTER)
                    return "CENTER";
                else
                    return "RIGHT";
            }
        }

        private sealed class CurrentRedrawMethod : FunctionMethod
        {
            public CurrentRedrawMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { };
                CanRestructure = false;
            }
            public override long GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                return (exm.Console.Redraw == GameView.ConsoleRedraw.None) ? 0L : 1L;
            }
        }

		private sealed class ColorFromNameMethod : FunctionMethod
		{
			public ColorFromNameMethod()
			{
				ReturnType = typeof(Int64);
				argumentTypeArray = new Type[] { typeof(string) };
				CanRestructure = true;
			}
			public override long GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				string colorName = arguments[0].GetStrValue(exm);
				Color color = Color.FromName(colorName);
				int i = 0;
				if (color.A > 0)
					i = (color.R << 16) + (color.G << 8) + color.B;
				else
				{
					if (colorName.Equals("transparent", StringComparison.OrdinalIgnoreCase))
						throw new CodeEE("無色透明(Transparent)は色として指定できません");
					//throw new CodeEE("指定された色名\"" + colorName + "\"は無効な色名です");
					i = -1;
				}
				return i;
			}
		}

		private sealed class ColorFromRGBMethod : FunctionMethod
		{
			public ColorFromRGBMethod()
			{
				ReturnType = typeof(Int64);
				argumentTypeArray = new Type[] { typeof(long), typeof(long), typeof(long) };
				CanRestructure = true;
			}
			public override long GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				long r = arguments[0].GetIntValue(exm);
				if(r < 0 || r > 255)
					throw new CodeEE("第１引数が0から255の範囲外です");
				long g = arguments[1].GetIntValue(exm);
				if(g< 0 || g > 255)
					throw new CodeEE("第２引数が0から255の範囲外です");
				long b = arguments[2].GetIntValue(exm);
				if(b < 0 || b > 255)
					throw new CodeEE("第３引数が0から255の範囲外です");
				return (r << 16) + (g << 8) + b;
			}
		}
		/// <summary>
		/// 1810 作ったけど保留
		/// </summary>
		private sealed class GetRefMethod : FunctionMethod
		{
			public GetRefMethod()
			{
				ReturnType = typeof(string);
				argumentTypeArray = null;
				CanRestructure = false;
			}
			public override string CheckArgumentType(string name, IOperandTerm[] arguments)
			{
				if (arguments.Length < 1)
					return name + "関数には少なくとも1つの引数が必要です";
				if (arguments.Length > 1)
					return name + "関数の引数が多すぎます";
				if (arguments[0] == null)
					return name + "関数の1番目の引数は省略できません";
				if (!(arguments[0] is UserDefinedRefMethodNoArgTerm))
					return name + "関数の1番目の引数が関数参照ではありません";
				return null;
			}
			public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				return ((UserDefinedRefMethodNoArgTerm)arguments[0]).GetRefName();
			}
		}
        #endregion

        #region 定数取得
        private sealed class MoneyStrMethod : FunctionMethod
        {
            public MoneyStrMethod()
            {
                ReturnType = typeof(string);
                argumentTypeArray = null;
                CanRestructure = true;
            }
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                //通常2つ、1つ省略可能で1～2の引数が必要。
                if (arguments.Length < 1)
                    return name + "関数には少なくとも1つの引数が必要です";
                if (arguments.Length > 2)
                    return name + "関数の引数が多すぎます";
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                if (arguments[0].GetOperandType() != typeof(Int64))
                    return name + "関数の1番目の引数の型が正しくありません";
                if ((arguments.Length >= 2) && (arguments[1] != null) && (arguments[1].GetOperandType() != typeof(string)))
                    return name + "関数の2番目の引数の型が正しくありません";
                return null;
            }
            public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                long money = arguments[0].GetIntValue(exm);
                if ((arguments.Length < 2) || (arguments[1] == null))
                    return (Config.MoneyFirst) ? Config.MoneyLabel + money.ToString() : money.ToString() + Config.MoneyLabel;
                string format = arguments[1].GetStrValue(exm);
                string ret;
                try
                {
                    ret = money.ToString(format);
                }
                catch (FormatException)
                {
                    throw new CodeEE("MONEYSTR関数の第2引数の書式指定が間違っています");
                }
                return (Config.MoneyFirst) ? Config.MoneyLabel + ret : ret + Config.MoneyLabel;
            }
        }

        private sealed class GetPrintCPerLineMethod : FunctionMethod
        {
            public GetPrintCPerLineMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { };
                CanRestructure = true;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                return (Config.PrintCPerLine);
            }
        }

        private sealed class PrintCLengthMethod : FunctionMethod
        {
            public PrintCLengthMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { };
                CanRestructure = true;
            }
            public override long GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                return (Config.PrintCLength);
            }
        }

        private sealed class GetSaveNosMethod : FunctionMethod
        {
            public GetSaveNosMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { };
                CanRestructure = true;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                return (Config.SaveDataNos);
            }
        }

        private sealed class GettimeMethod : FunctionMethod
        {
            public GettimeMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { };
                CanRestructure = false;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                long date = DateTime.Now.Year;
                date = date * 100 + DateTime.Now.Month;
                date = date * 100 + DateTime.Now.Day;
                date = date * 100 + DateTime.Now.Hour;
                date = date * 100 + DateTime.Now.Minute;
                date = date * 100 + DateTime.Now.Second;
                date = date * 1000 + DateTime.Now.Millisecond;
                return (date);//17桁。2京くらい。
            }
        }

        private sealed class GettimesMethod : FunctionMethod
        {
            public GettimesMethod()
            {
                ReturnType = typeof(string);
                argumentTypeArray = new Type[] { };
                CanRestructure = false;
            }
            public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                return (DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            }
        }

        private sealed class GetmsMethod : FunctionMethod
        {
            public GetmsMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { };
                CanRestructure = false;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                //西暦0001年1月1日からの経過時間をミリ秒で。
                //Ticksは100ナノ秒単位であるが実際にはそんな精度はないので無駄。
                return (DateTime.Now.Ticks / 10000);
            }
        }

        private sealed class GetSecondMethod : FunctionMethod
        {
            public GetSecondMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { };
                CanRestructure = false;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                //西暦0001年1月1日からの経過時間を秒で。
                //Ticksは100ナノ秒単位であるが実際にはそんな精度はないので無駄。
                return (DateTime.Now.Ticks / 10000000);
            }
        }
        #endregion

        #region 数学関数
        private sealed class RandMethod : FunctionMethod
        {
            public RandMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                CanRestructure = false;
            }

            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                //通常2つ、1つ省略可能で1～2の引数が必要。
                if (arguments.Length < 1)
                    return name + "関数には少なくとも1つの引数が必要です";
                if (arguments.Length > 2)
                    return name + "関数の引数が多すぎます";
                if (arguments.Length == 1)
                {
                    if (arguments[0] == null)
                        return name + "関数には少なくとも1つの引数が必要です";
                    if ((arguments[0].GetOperandType() != typeof(Int64)))
                        return name + "関数の1番目の引数の型が正しくありません";
                    return null;
                }
                //1番目は省略可能
                if ((arguments[0] != null) && (arguments[0].GetOperandType() != typeof(Int64)))
                    return name + "関数の1番目の引数の型が正しくありません";
                if ((arguments[1] != null) && (arguments[1].GetOperandType() != typeof(Int64)))
                    return name + "関数の2番目の引数の型が正しくありません";
                return null;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 max = 0;
                Int64 min = 0;
                if (arguments.Length == 1)
                    max = arguments[0].GetIntValue(exm);
                else
                {
                    if (arguments[0] != null)
                        min = arguments[0].GetIntValue(exm);
                    max = arguments[1].GetIntValue(exm);
                }
                if (max <= min)
                {
                    if (min == 0)
                        throw new CodeEE("RANDの最大値に0以下の値(" + max.ToString() + ")が指定されました");
                    else
                        throw new CodeEE("RANDの最大値に最小値以下の値(" + max.ToString() + ")が指定されました");
                }
                return (exm.VEvaluator.GetNextRand(max - min) + min);
            }
        }

        private sealed class MaxMethod : FunctionMethod
        {
            bool isMax;
            public MaxMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                isMax = true;
                CanRestructure = true;
            }
            public MaxMethod(bool max)
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                isMax = max;
                CanRestructure = true;
            }
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                if (arguments.Length < 1)
                    return name + "関数には少なくとも1つの引数が必要です";
                for (int i = 0; i < arguments.Length; i++)
                {
                    if (arguments[i] == null)
                        return name + "関数の" + (i + 1).ToString() + "番目の引数は省略できません";
                    if (arguments[i].GetOperandType() != typeof(Int64))
                        return name + "関数の" + (i + 1).ToString() + "番目の引数の型が正しくありません";
                }
                return null;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 ret = arguments[0].GetIntValue(exm);

                for (int i = 1; i < arguments.Length; i++)
                {
                    Int64 newRet = arguments[i].GetIntValue(exm);
                    if (isMax)
                    {
                        if (ret < newRet)
                            ret = newRet;
                    }
                    else
                    {
                        if (ret > newRet)
                            ret = newRet;
                    }
                }
                return (ret);
            }
        }

        private sealed class AbsMethod : FunctionMethod
        {
            public AbsMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(Int64) };
                CanRestructure = true;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 ret = arguments[0].GetIntValue(exm);
                return (Math.Abs(ret));
            }
        }

        private sealed class PowerMethod : FunctionMethod
        {
            public PowerMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(Int64), typeof(Int64) };
                CanRestructure = true;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 x = arguments[0].GetIntValue(exm);
                Int64 y = arguments[1].GetIntValue(exm);
                double pow = Math.Pow(x, y);
                if (double.IsNaN(pow))
                    throw new CodeEE("累乗結果が非数値です");
                else if (double.IsInfinity(pow))
                    throw new CodeEE("累乗結果が無限大です");
                else if ((pow >= Int64.MaxValue) || (pow <= Int64.MinValue))
                    throw new CodeEE("累乗結果(" + pow.ToString() + ")が64ビット符号付き整数の範囲外です");
                return ((long)pow);
            }
        }

        private sealed class SqrtMethod : FunctionMethod
        {
            public SqrtMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(Int64) };
                CanRestructure = true;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 ret = arguments[0].GetIntValue(exm);
                if (ret < 0)
                    throw new CodeEE("SQRT関数の引数に負の値が指定されました");
                return ((Int64)Math.Sqrt(ret));
            }
        }

        private sealed class CbrtMethod : FunctionMethod
        {
            public CbrtMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(Int64) };
                CanRestructure = true;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 ret = arguments[0].GetIntValue(exm);
                if (ret < 0)
                    throw new CodeEE("CBRT関数の引数に負の値が指定されました");
                return ((Int64)Math.Pow((double)ret, 1.0 / 3.0));
            }
        }

        private sealed class LogMethod : FunctionMethod
        {
            double Base;
            public LogMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(Int64) };
                Base = Math.E;
                CanRestructure = true;
            }
            public LogMethod(double b)
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(Int64) };
                Base = b;
                CanRestructure = true;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 ret = arguments[0].GetIntValue(exm);
                if (ret <= 0)
                    throw new CodeEE("対数関数の引数に0以下の値が指定されました");
                if (Base <= 0.0d)
                    throw new CodeEE("対数関数の底に0以下の値が指定されました");
                double dret = (double)ret;
                if (Base == Math.E)
                    dret = Math.Log(dret);
                else
                    dret = Math.Log10(dret);
                if (double.IsNaN(dret))
                    throw new CodeEE("計算値が非数値です");
                else if (double.IsInfinity(dret))
                    throw new CodeEE("計算値が無限大です");
                else if ((dret >= Int64.MaxValue) || (dret <= Int64.MinValue))
                    throw new CodeEE("計算結果(" + dret.ToString() + ")が64ビット符号付き整数の範囲外です");
                return ((Int64)dret);
            }
        }

        private sealed class ExpMethod : FunctionMethod
        {
            public ExpMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(Int64) };
                CanRestructure = true;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 ret = arguments[0].GetIntValue(exm);
                double dret = Math.Exp((double)ret);
                if (double.IsNaN(dret))
                    throw new CodeEE("計算値が非数値です");
                else if (double.IsInfinity(dret))
                    throw new CodeEE("計算値が無限大です");
                else if ((dret >= Int64.MaxValue) || (dret <= Int64.MinValue))
                    throw new CodeEE("計算結果(" + dret.ToString() + ")が64ビット符号付き整数の範囲外です");

                return ((Int64)dret);
            }
        }

        private sealed class SignMethod : FunctionMethod
        {

            public SignMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(Int64) };
                CanRestructure = true;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 ret = arguments[0].GetIntValue(exm);
                return (Math.Sign(ret));
            }
        }

        private sealed class GetLimitMethod : FunctionMethod
        {
            public GetLimitMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(Int64), typeof(Int64), typeof(Int64) };
                CanRestructure = true;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 value = arguments[0].GetIntValue(exm);
                Int64 min = arguments[1].GetIntValue(exm);
                Int64 max = arguments[2].GetIntValue(exm);
                Int64 ret = 0;
                if (value < min)
                    ret = min;
                else if (value > max)
                    ret = max;
                else
                    ret = value;
                return (ret);
            }
        }
        #endregion

        #region 変数操作系
        private sealed class SumArrayMethod : FunctionMethod
        {
            bool isCharaRange;
            public SumArrayMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                isCharaRange = false;
                CanRestructure = false;
            }
            public SumArrayMethod(bool isChara)
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                isCharaRange = isChara;
                CanRestructure = false;
            }
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                if (arguments.Length < 1)
                    return name + "関数には少なくとも1つの引数が必要です";
                if (arguments.Length > 3)
                    return name + "関数の引数が多すぎます";
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                if (!(arguments[0] is VariableTerm))
                    return name + "関数の1番目の引数が変数ではありません";
                VariableTerm varToken = (VariableTerm)arguments[0];
                if (varToken.IsString)
                    return name + "関数の1番目の引数が数値変数ではありません";
                if (isCharaRange && !varToken.Identifier.IsCharacterData)
                    return name + "関数の1番目の引数がキャラクタ変数ではありません";
                if (!isCharaRange && !varToken.Identifier.IsArray1D && !varToken.Identifier.IsArray2D && !varToken.Identifier.IsArray3D)
                    return name + "関数の1番目の引数が配列変数ではありません";
                if (arguments.Length == 1)
                    return null;
                if ((arguments[1] != null) && (arguments[1].GetOperandType() != typeof(Int64)))
                    return name + "関数の2番目の変数が数値ではありません";
                if (arguments.Length == 2)
                    return null;
                if ((arguments[2] != null) && (arguments[2].GetOperandType() != typeof(Int64)))
                    return name + "関数の3番目の変数が数値ではありません";
                return null;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                VariableTerm varTerm = (VariableTerm)arguments[0];
                Int64 index1 = (arguments.Length >= 2 && arguments[1] != null) ? arguments[1].GetIntValue(exm) : 0;
                Int64 index2 = (arguments.Length == 3 && arguments[2] != null) ? arguments[2].GetIntValue(exm) : (isCharaRange ? exm.VEvaluator.CHARANUM : varTerm.GetLastLength());

                FixedVariableTerm p = varTerm.GetFixedVariableTerm(exm);
                if (!isCharaRange)
                {
                    p.IsArrayRangeValid(index1, index2, "SUMARRAY", 2L, 3L);
                    return (exm.VEvaluator.GetArraySum(p, index1, index2));
                }
                else
                {
                    Int64 charaNum = exm.VEvaluator.CHARANUM;
                    if (index1 >= charaNum || index1 < 0 || index2 > charaNum || index2 < 0)
                        throw new CodeEE("SUMCARRAY関数の範囲指定がキャラクタ配列の範囲を超えています(" + index1.ToString() + "～" + index2.ToString() + ")");
                    return (exm.VEvaluator.GetArraySumChara(p, index1, index2));
                }
            }
        }

        private sealed class MatchMethod : FunctionMethod
        {
            bool isCharaRange;
            public MatchMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                isCharaRange = false;
                CanRestructure = false;
                HasUniqueRestructure = true;
            }
            public MatchMethod(bool isChara)
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                isCharaRange = isChara;
                CanRestructure = false;
                HasUniqueRestructure = true;
            }
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                if (arguments.Length < 2)
                    return name + "関数には少なくとも2つの引数が必要です";
                if (arguments.Length > 4)
                    return name + "関数の引数が多すぎます";
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                if (!(arguments[0] is VariableTerm))
                    return name + "関数の1番目の引数が変数ではありません";
                VariableTerm varToken = (VariableTerm)arguments[0];
                if (isCharaRange && !varToken.Identifier.IsCharacterData)
                    return name + "関数の1番目の引数がキャラクタ変数ではありません";
                if (!isCharaRange && (varToken.Identifier.IsArray2D || varToken.Identifier.IsArray3D))
                    return name + "関数は二重配列・三重配列には対応していません";
                if (!isCharaRange && !varToken.Identifier.IsArray1D)
                    return name + "関数の1番目の引数が配列変数ではありません";
                if (arguments[1] == null)
                    return name + "関数の2番目の引数は省略できません";
                if (arguments[1].GetOperandType() != arguments[0].GetOperandType())
                    return name + "関数の1番目の引数と2番目の引数の型が異なります";
                if ((arguments.Length >= 3) && (arguments[2] != null) && (arguments[2].GetOperandType() != typeof(Int64)))
                    return name + "関数の3番目の引数の型が正しくありません";
                if ((arguments.Length >= 4) && (arguments[3] != null) && (arguments[3].GetOperandType() != typeof(Int64)))
                    return name + "関数の4番目の引数の型が正しくありません";
                return null;
            }

            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                VariableTerm varTerm = arguments[0] as VariableTerm;
                Int64 start = (arguments.Length > 2 && arguments[2] != null) ? arguments[2].GetIntValue(exm) : 0;
                Int64 end = (arguments.Length > 3 && arguments[3] != null) ? arguments[3].GetIntValue(exm) : (isCharaRange ? exm.VEvaluator.CHARANUM : varTerm.GetLength());

                FixedVariableTerm p = varTerm.GetFixedVariableTerm(exm);
                if (!isCharaRange)
                {
                    p.IsArrayRangeValid(start, end, "MATCH", 3L, 4L);
                    if (arguments[0].GetOperandType() == typeof(Int64))
                    {
                        Int64 targetValue = arguments[1].GetIntValue(exm);
                        return (exm.VEvaluator.GetMatch(p, targetValue, start, end));
                    }
                    else
                    {
                        string targetStr = arguments[1].GetStrValue(exm);
                        return (exm.VEvaluator.GetMatch(p, targetStr, start, end));
                    }
                }
                else
                {
                    Int64 charaNum = exm.VEvaluator.CHARANUM;
                    if (start >= charaNum || start < 0 || end > charaNum || end < 0)
                        throw new CodeEE("CMATCH関数の範囲指定がキャラクタ配列の範囲を超えています(" + start.ToString() + "～" + end.ToString() + ")");
                    if (arguments[0].GetOperandType() == typeof(Int64))
                    {
                        Int64 targetValue = arguments[1].GetIntValue(exm);
                        return (exm.VEvaluator.GetMatchChara(p, targetValue, start, end));
                    }
                    else
                    {
                        string targetStr = arguments[1].GetStrValue(exm);
                        return (exm.VEvaluator.GetMatchChara(p, targetStr, start, end));
                    }
                }
            }

            public override bool UniqueRestructure(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                arguments[0].Restructure(exm);
                for (int i = 1; i < arguments.Length; i++)
                {
                    if (arguments[i] == null)
                        continue;
                    arguments[i] = arguments[i].Restructure(exm);
                }
                return false;
            }
        }

        private sealed class GroupMatchMethod : FunctionMethod
        {
            public GroupMatchMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                CanRestructure = false;
            }
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                if (arguments.Length < 2)
                    return name + "関数には少なくとも2つの引数が必要です";
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                Type baseType = arguments[0].GetOperandType();
                for (int i = 1; i < arguments.Length; i++)
                {
                    if (arguments[i] == null)
                        return name + "関数の" + (i + 1).ToString() + "番目の引数は省略できません";
                    if (arguments[i].GetOperandType() != baseType)
                        return name + "関数の" + (i + 1).ToString() + "番目の引数の型が正しくありません";
                }
                return null;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 ret = 0;
                if (arguments[0].GetOperandType() == typeof(Int64))
                {
                    Int64 baseValue = arguments[0].GetIntValue(exm);
                    for (int i = 1; i < arguments.Length; i++)
                    {
                        if (baseValue == arguments[i].GetIntValue(exm))
                            ret += 1;
                    }
                }
                else
                {
                    string baseString = arguments[0].GetStrValue(exm);
                    for (int i = 1; i < arguments.Length; i++)
                    {
                        if (baseString == arguments[i].GetStrValue(exm))
                            ret += 1;
                    }
                }
                return (ret);
            }
        }

        private sealed class NosamesMethod : FunctionMethod
        {
            public NosamesMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                CanRestructure = false;
            }
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                if (arguments.Length < 2)
                    return name + "関数には少なくとも2つの引数が必要です";
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                Type baseType = arguments[0].GetOperandType();
                for (int i = 1; i < arguments.Length; i++)
                {
                    if (arguments[i] == null)
                        return name + "関数の" + (i + 1).ToString() + "番目の引数は省略できません";
                    if (arguments[i].GetOperandType() != baseType)
                        return name + "関数の" + (i + 1).ToString() + "番目の引数の型が正しくありません";
                }
                return null;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                if (arguments[0].GetOperandType() == typeof(Int64))
                {
                    Int64 baseValue = arguments[0].GetIntValue(exm);
                    for (int i = 1; i < arguments.Length; i++)
                    {
                        if (baseValue == arguments[i].GetIntValue(exm))
                            return 0L;
                    }
                }
                else
                {
                    string baseValue = arguments[0].GetStrValue(exm);
                    for (int i = 1; i < arguments.Length; i++)
                    {
                        if (baseValue == arguments[i].GetStrValue(exm))
                            return 0L;
                    }
                }
                return 1L;
            }
        }

        private sealed class AllsamesMethod : FunctionMethod
        {
            public AllsamesMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                CanRestructure = false;
            }
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                if (arguments.Length < 2)
                    return name + "関数には少なくとも2つの引数が必要です";
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                Type baseType = arguments[0].GetOperandType();
                for (int i = 1; i < arguments.Length; i++)
                {
                    if (arguments[i] == null)
                        return name + "関数の" + (i + 1).ToString() + "番目の引数は省略できません";
                    if (arguments[i].GetOperandType() != baseType)
                        return name + "関数の" + (i + 1).ToString() + "番目の引数の型が正しくありません";
                }
                return null;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                if (arguments[0].GetOperandType() == typeof(Int64))
                {
                    Int64 baseValue = arguments[0].GetIntValue(exm);
                    for (int i = 1; i < arguments.Length; i++)
                    {
                        if (baseValue != arguments[i].GetIntValue(exm))
                            return 0L;
                    }
                }
                else
                {
                    string baseValue = arguments[0].GetStrValue(exm);
                    for (int i = 1; i < arguments.Length; i++)
                    {
                        if (baseValue != arguments[i].GetStrValue(exm))
                            return 0L;
                    }
                }
                return 1L;
            }
        }

        private sealed class MaxArrayMethod : FunctionMethod
        {
            bool isCharaRange;
            bool isMax;
            string funcName;
            public MaxArrayMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                isCharaRange = false;
                isMax = true;
                funcName = "MAXARRAY";
                CanRestructure = false;
            }
            public MaxArrayMethod(bool isChara)
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                isCharaRange = isChara;
                isMax = true;
                if (isCharaRange)
                    funcName = "MAXCARRAY";
                else
                    funcName = "MAXARRAY";
                CanRestructure = false;
            }
            public MaxArrayMethod(bool isChara, bool isMaxFunc)
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                isCharaRange = isChara;
                isMax = isMaxFunc;
                funcName = (isMax ? "MAX" : "MIN") + (isCharaRange ? "C" : "") + "ARRAY";
                CanRestructure = false;
            }
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                if (arguments.Length < 1)
                    return name + "関数には少なくとも1つの引数が必要です";
                if (arguments.Length > 3)
                    return name + "関数の引数が多すぎます";
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                if (!(arguments[0] is VariableTerm))
                    return name + "関数の1番目の引数が変数ではありません";
                VariableTerm varToken = (VariableTerm)arguments[0];
                if (isCharaRange && !varToken.Identifier.IsCharacterData)
                    return name + "関数の1番目の引数がキャラクタ変数ではありません";
                if (!varToken.IsInteger)
                    return name + "関数の1番目の引数が数値変数ではありません";
                if (!isCharaRange && (varToken.Identifier.IsArray2D || varToken.Identifier.IsArray3D))
                    return name + "関数は二重配列・三重配列には対応していません";
                if (!varToken.Identifier.IsArray1D)
                    return name + "関数の1番目の引数が配列変数ではありません";
                if ((arguments.Length >= 2) && (arguments[1] != null) && (arguments[1].GetOperandType() != typeof(Int64)))
                    return name + "関数の2番目の引数の型が正しくありません";
                if ((arguments.Length >= 3) && (arguments[2] != null) && (arguments[2].GetOperandType() != typeof(Int64)))
                    return name + "関数の3番目の引数の型が正しくありません";
                return null;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                VariableTerm vTerm = (VariableTerm)arguments[0];
                Int64 start = (arguments.Length > 1 && arguments[1] != null) ? arguments[1].GetIntValue(exm) : 0;
                Int64 end = (arguments.Length > 2 && arguments[2] != null) ? end = arguments[2].GetIntValue(exm) : (isCharaRange ? exm.VEvaluator.CHARANUM : vTerm.GetLength());
                FixedVariableTerm p = vTerm.GetFixedVariableTerm(exm);
                if (!isCharaRange)
                {
                    p.IsArrayRangeValid(start, end, funcName, 2L, 3L);
                    return (exm.VEvaluator.GetMaxArray(p, start, end, isMax));
                }
                else
                {
                    Int64 charaNum = exm.VEvaluator.CHARANUM;
                    if (start >= charaNum || start < 0 || end > charaNum || end < 0)
                        throw new CodeEE(funcName + "関数の範囲指定がキャラクタ配列の範囲を超えています(" + start.ToString() + "～" + end.ToString() + ")");
                    return (exm.VEvaluator.GetMaxArrayChara(p, start, end, isMax));
                }
            }
        }

        private sealed class GetbitMethod : FunctionMethod
        {
            public GetbitMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(Int64), typeof(Int64) };
                CanRestructure = true;
            }
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                string ret = base.CheckArgumentType(name, arguments);
                if (ret != null)
                    return ret;
                if (arguments[1] is SingleTerm)
                {
                    Int64 m = ((SingleTerm)arguments[1]).Int;
                    if (m < 0 || m > 63)
                        return "GETBIT関数の第２引数(" + m.ToString() + ")が範囲(０～６３)を超えています";
                }
                return null;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 n = arguments[0].GetIntValue(exm);
                Int64 m = arguments[1].GetIntValue(exm);
                if ((m < 0) || (m > 63))
                    throw new CodeEE("GETBIT関数の第２引数(" + m.ToString() + ")が範囲(０～６３)を超えています");
                int mi = (int)m;
                return ((n >> mi) & 1);
            }
        }

        private sealed class GetnumMethod : FunctionMethod
        {
            public GetnumMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                CanRestructure = true;
                HasUniqueRestructure = true;
            }
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                if (arguments.Length != 2)
                    return name + "関数には2つの引数が必要です";
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                if (!(arguments[0] is VariableTerm))
                    return name + "関数の1番目の引数の型が正しくありません";
                if (arguments[1] == null)
                    return name + "関数の2番目の引数は省略できません";
                if (arguments[1].GetOperandType() != typeof(string))
                    return name + "関数の2番目の引数の型が正しくありません";
                return null;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                VariableTerm vToken = (VariableTerm)arguments[0];
                VariableCode varCode = vToken.Identifier.Code;
                string key = arguments[1].GetStrValue(exm);
                int ret = 0;
                if (exm.VEvaluator.Constant.TryKeywordToInteger(out ret, varCode, key, -1))
                    return ret;
                else
                    return -1;
            }
            public override bool UniqueRestructure(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                arguments[1] = arguments[1].Restructure(exm);
                return arguments[1] is SingleTerm;
            }
        }

		private sealed class GetnumBMethod : FunctionMethod
		{
			public GetnumBMethod()
			{
				ReturnType = typeof(Int64);
				argumentTypeArray = new Type[] { typeof(string), typeof(string) };
				CanRestructure = true;
			}
			public override string CheckArgumentType(string name, IOperandTerm[] arguments)
			{
				string errStr = base.CheckArgumentType(name, arguments);
				if (errStr != null)
					return errStr;
				if (arguments[0] == null)
					return name + "関数の1番目の引数は省略できません";
				if (arguments[0] is SingleTerm)
				{
					string varName = ((SingleTerm)arguments[0]).Str;
					if (GlobalStatic.IdentifierDictionary.GetVariableToken(varName, null, true) == null)
						return name + "関数の1番目の引数が変数名ではありません";
				}
				return null;
			}
			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				VariableToken var = GlobalStatic.IdentifierDictionary.GetVariableToken(arguments[0].GetStrValue(exm), null, true);
				if (var == null)
					throw new CodeEE("GETNUMBの1番目の引数(\"" + arguments[0].GetStrValue(exm) + "\")が変数名ではありません");
				string key = arguments[1].GetStrValue(exm);
				int ret = 0;
                if (exm.VEvaluator.Constant.TryKeywordToInteger(out ret, var.Code, key, -1))
                    return ret;
                else
                    return -1;
			}
		}

        private sealed class GetPalamLVMethod : FunctionMethod
        {
            public GetPalamLVMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(Int64), typeof(Int64) };
                CanRestructure = false;
            }
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                string errStr = base.CheckArgumentType(name, arguments);
                if (errStr != null)
                    return errStr;
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                return null;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 value = arguments[0].GetIntValue(exm);
                Int64 maxLv = arguments[1].GetIntValue(exm);

                return (exm.VEvaluator.getPalamLv(value, maxLv));
            }
        }

        private sealed class GetExpLVMethod : FunctionMethod
        {
            public GetExpLVMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(Int64), typeof(Int64) };
                CanRestructure = false;
            }
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                string errStr = base.CheckArgumentType(name, arguments);
                if (errStr != null)
                    return errStr;
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                return null;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 value = arguments[0].GetIntValue(exm);
                Int64 maxLv = arguments[1].GetIntValue(exm);

                return (exm.VEvaluator.getExpLv(value, maxLv));
            }
        }

        private sealed class FindElementMethod : FunctionMethod
        {
            public FindElementMethod(bool last)
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                CanRestructure = true; //すべて定数項ならできるはず
                HasUniqueRestructure = true;
                isLast = last;
                funcName = isLast ? "FINDLASTELEMENT" : "FINDELEMENT";
            }
            bool isLast;
            string funcName;
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                if (arguments.Length < 2)
                    return name + "関数には少なくとも2つの引数が必要です";
                if (arguments.Length > 5)
                    return name + "関数の引数が多すぎます";
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                VariableTerm varToken = arguments[0] as VariableTerm;
                if (varToken == null)
                    return name + "関数の1番目の引数が変数ではありません";
                if (varToken.Identifier.IsArray2D || varToken.Identifier.IsArray3D)
                    return name + "関数は二重配列・三重配列には対応していません";
                if (!varToken.Identifier.IsArray1D)
                    return name + "関数の1番目の引数が配列変数ではありません";
                Type baseType = arguments[0].GetOperandType();
                if (arguments[1] == null)
                    return name + "関数の2番目の引数は省略できません";
                if (arguments[1].GetOperandType() != baseType)
                    return name + "関数の2番目の引数の型が正しくありません";
                if ((arguments.Length >= 3) && (arguments[2] != null) && (arguments[2].GetOperandType() != typeof(Int64)))
                    return name + "関数の3番目の引数の型が正しくありません";
                if ((arguments.Length >= 4) && (arguments[3] != null) && (arguments[3].GetOperandType() != typeof(Int64)))
                    return name + "関数の4番目の引数の型が正しくありません";
                if ((arguments.Length >= 5) && (arguments[4] != null) && (arguments[4].GetOperandType() != typeof(Int64)))
                    return name + "関数の5番目の引数の型が正しくありません";
                return null;
            }

            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                bool isExact = false;
                VariableTerm varTerm = (VariableTerm)arguments[0];

                Int64 start = (arguments.Length > 2 && arguments[2] != null) ? arguments[2].GetIntValue(exm) : 0;
                Int64 end = (arguments.Length > 3 && arguments[3] != null) ? arguments[3].GetIntValue(exm) : varTerm.GetLength();
                if (arguments.Length > 4 && arguments[4] != null)
                    isExact = (arguments[4].GetIntValue(exm) != 0);

                FixedVariableTerm p = varTerm.GetFixedVariableTerm(exm);
                p.IsArrayRangeValid(start, end, funcName, 3L, 4L);

                if (arguments[0].GetOperandType() == typeof(Int64))
                {
                    Int64 targetValue = arguments[1].GetIntValue(exm);
                    return exm.VEvaluator.FindElement(p, targetValue, start, end, isExact, isLast);
                }
                else
                {
                    Regex targetString = null;
                    try
                    {
                        targetString = new Regex(arguments[1].GetStrValue(exm));
                    }
                    catch (ArgumentException)
                    {
                        throw new CodeEE("第2引数が正規表現として不正です");
                    }
                    return exm.VEvaluator.FindElement(p, targetString, start, end, isExact, isLast);
                }
            }
            
            
            public override bool UniqueRestructure(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                bool isConst = true;

                arguments[0].Restructure(exm);
                VariableTerm varToken = arguments[0] as VariableTerm;
                isConst = varToken.Identifier.IsConst;

                for (int i = 1; i < arguments.Length; i++)
                {
                    if (arguments[i] == null)
                        continue;
                    arguments[i] = arguments[i].Restructure(exm);
                    if (isConst && !(arguments[i] is SingleTerm))
                        isConst = false;
                }
                return isConst;
            }
        }

        private sealed class InRangeMethod : FunctionMethod
        {
            public InRangeMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(Int64), typeof(Int64), typeof(Int64) };
                CanRestructure = true;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 value = arguments[0].GetIntValue(exm);
                Int64 min = arguments[1].GetIntValue(exm);
                Int64 max = arguments[2].GetIntValue(exm);
                return ((value >= min) && (value <= max)) ? 1L : 0L;
            }
        }

        private sealed class InRangeArrayMethod : FunctionMethod
        {
            public InRangeArrayMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                CanRestructure = false;
            }
            public InRangeArrayMethod(bool isChara)
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                isCharaRange = isChara;
                CanRestructure = false;
            }
            private bool isCharaRange = false;
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                if (arguments.Length < 2)
                    return name + "関数には少なくとも2つの引数が必要です";
                if (arguments.Length > 6)
                    return name + "関数の引数が多すぎます";
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                if (!(arguments[0] is VariableTerm))
                    return name + "関数の1番目の引数が変数ではありません";
                VariableTerm varToken = (VariableTerm)arguments[0];
                if (isCharaRange && !varToken.Identifier.IsCharacterData)
                    return name + "関数の1番目の引数がキャラクタ変数ではありません";
                if (!isCharaRange && (varToken.Identifier.IsArray2D || varToken.Identifier.IsArray3D))
                    return name + "関数は二重配列・三重配列には対応していません";
                if (!isCharaRange && !varToken.Identifier.IsArray1D)
                    return name + "関数の1番目の引数が配列変数ではありません";
                if (!varToken.IsInteger)
                    return name + "関数の1番目の引数が数値型変数ではありません";
                if (arguments[1] == null)
                    return name + "関数の2番目の引数は省略できません";
                if (arguments[1].GetOperandType() != typeof(Int64))
                    return name + "関数の2番目の引数が数値型ではありません";
                if (arguments[2] == null)
                    return name + "関数の3番目の引数は省略できません";
                if (arguments[2].GetOperandType() != typeof(Int64))
                    return name + "関数の3番目の引数が数値型ではありません";
                if ((arguments.Length >= 4) && (arguments[3] != null) && (arguments[3].GetOperandType() != typeof(Int64)))
                    return name + "関数の4番目の引数の型が正しくありません";
                if ((arguments.Length >= 5) && (arguments[4] != null) && (arguments[4].GetOperandType() != typeof(Int64)))
                    return name + "関数の5番目の引数の型が正しくありません";
                return null;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 min = arguments[1].GetIntValue(exm);
                Int64 max = arguments[2].GetIntValue(exm);

                VariableTerm varTerm = arguments[0] as VariableTerm;
                Int64 start = (arguments.Length > 3 && arguments[3] != null) ? arguments[3].GetIntValue(exm) : 0;
                Int64 end = (arguments.Length > 4 && arguments[4] != null) ? arguments[4].GetIntValue(exm) : (isCharaRange ? exm.VEvaluator.CHARANUM : varTerm.GetLength());

                FixedVariableTerm p = varTerm.GetFixedVariableTerm(exm);

                if (!isCharaRange)
                {
                    p.IsArrayRangeValid(start, end, "INRANGEARRAY", 4L, 5L);
                    return (exm.VEvaluator.GetInRangeArray(p, min, max, start, end));
                }
                else
                {
                    Int64 charaNum = exm.VEvaluator.CHARANUM;
                    if (start >= charaNum || start < 0 || end > charaNum || end < 0)
                        throw new CodeEE("INRANGECARRAY関数の範囲指定がキャラクタ配列の範囲を超えています(" + start.ToString() + "～" + end.ToString() + ")");
                    return (exm.VEvaluator.GetInRangeArrayChara(p, min, max, start, end));
                }
            }
        }
        #endregion

        #region 文字列操作系
        private sealed class StrlenMethod : FunctionMethod
        {
            public StrlenMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(string) };
                CanRestructure = true;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                string str = arguments[0].GetStrValue(exm);
                return (LangManager.GetStrlenLang(str));
            }
        }

        private sealed class StrlenuMethod : FunctionMethod
        {
            public StrlenuMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(string) };
                CanRestructure = true;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                string str = arguments[0].GetStrValue(exm);
                return (str.Length);
            }
        }

        private sealed class SubstringMethod : FunctionMethod
        {
            public SubstringMethod()
            {
                ReturnType = typeof(string);
                argumentTypeArray = null;
                CanRestructure = true;
            }

            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                //通常３つ、２つ省略可能で１～３の引数が必要。
                if (arguments.Length < 1)
                    return name + "関数には少なくとも1つの引数が必要です";
                if (arguments.Length > 3)
                    return name + "関数の引数が多すぎます";

                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                if (arguments[0].GetOperandType() != typeof(string))
                    return name + "関数の1番目の引数の型が正しくありません";
                //2、３は省略可能
                if ((arguments.Length >= 2) && (arguments[1] != null) && (arguments[1].GetOperandType() != typeof(Int64)))
                    return name + "関数の2番目の引数の型が正しくありません";
                if ((arguments.Length >= 3) && (arguments[2] != null) && (arguments[2].GetOperandType() != typeof(Int64)))
                    return name + "関数の3番目の引数の型が正しくありません";
                return null;
            }
            public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                string str = arguments[0].GetStrValue(exm);
                int start = 0;
                int length = -1;
                if ((arguments.Length >= 2) && (arguments[1] != null))
                    start = (int)arguments[1].GetIntValue(exm);
                if ((arguments.Length >= 3) && (arguments[2] != null))
                    length = (int)arguments[2].GetIntValue(exm);

                return (LangManager.GetSubStringLang(str, start, length));
            }
        }

        private sealed class SubstringuMethod : FunctionMethod
        {
            public SubstringuMethod()
            {
                ReturnType = typeof(string);
                argumentTypeArray = null;
                CanRestructure = true;
            }

            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                //通常３つ、２つ省略可能で１～３の引数が必要。
                if (arguments.Length < 1)
                    return name + "関数には少なくとも1つの引数が必要です";
                if (arguments.Length > 3)
                    return name + "関数の引数が多すぎます";

                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                if (arguments[0].GetOperandType() != typeof(string))
                    return name + "関数の1番目の引数の型が正しくありません";
                //2、３は省略可能
                if ((arguments.Length >= 2) && (arguments[1] != null) && (arguments[1].GetOperandType() != typeof(Int64)))
                    return name + "関数の2番目の引数の型が正しくありません";
                if ((arguments.Length >= 3) && (arguments[2] != null) && (arguments[2].GetOperandType() != typeof(Int64)))
                    return name + "関数の3番目の引数の型が正しくありません";
                return null;
            }
            public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                string str = arguments[0].GetStrValue(exm);
                int start = 0;
                int length = -1;
                if ((arguments.Length >= 2) && (arguments[1] != null))
                    start = (int)arguments[1].GetIntValue(exm);
                if ((arguments.Length >= 3) && (arguments[2] != null))
                    length = (int)arguments[2].GetIntValue(exm);
                if ((start >= str.Length) || (length == 0))
                    return ("");
                if ((length < 0) || (length > str.Length))
                    length = str.Length;
                if (start <= 0)
                {
                    if (length == str.Length)
                        return (str);
                    else
                        start = 0;
                }
                if ((start + length) > str.Length)
                    length = str.Length - start;

                return (str.Substring(start, length));
            }
        }

        private sealed class StrfindMethod : FunctionMethod
        {
            public StrfindMethod(bool unicode)
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = null;
                CanRestructure = true;
				this.unicode = unicode;
            }
			bool unicode = false;
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                //通常３つ、１つ省略可能で２～３の引数が必要。
                if (arguments.Length < 2)
                    return name + "関数には少なくとも2つの引数が必要です";
                if (arguments.Length > 3)
                    return name + "関数の引数が多すぎます";
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                if (arguments[0].GetOperandType() != typeof(string))
                    return name + "関数の1番目の引数の型が正しくありません";
                if (arguments[1] == null)
                    return name + "関数の2番目の引数は省略できません";
                if (arguments[1].GetOperandType() != typeof(string))
                    return name + "関数の2番目の引数の型が正しくありません";
                //3つ目は省略可能
                if ((arguments.Length >= 3) && (arguments[2] != null) && (arguments[2].GetOperandType() != typeof(Int64)))
                    return name + "関数の3番目の引数の型が正しくありません";
                return null;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {

                string target = arguments[0].GetStrValue(exm);
                string word = arguments[1].GetStrValue(exm);
                int JISstart = 0, UFTstart = 0;
				if ((arguments.Length >= 3) && (arguments[2] != null))
				{
					if (unicode)
					{
						UFTstart = (int)arguments[2].GetIntValue(exm);
					}
					else
					{
						JISstart = (int)arguments[2].GetIntValue(exm);
						UFTstart = LangManager.GetUFTIndex(target, JISstart);
					}
				}
                if (UFTstart < 0 || UFTstart >= target.Length)
                    return (-1);
                int index = target.IndexOf(word, UFTstart);
				if (index > 0 && !unicode)
                {
                    string subStr = target.Substring(0, index);
                    index = LangManager.GetStrlenLang(subStr);
                }
                return (index);
            }
        }

        private sealed class StrCountMethod : FunctionMethod
        {
            public StrCountMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(string), typeof(string) };
                CanRestructure = true;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Regex reg = null;
                try
                {
                    reg = new Regex(arguments[1].GetStrValue(exm));
                }
                catch (ArgumentException e)
                {
                    throw new CodeEE("第2引数が正規表現として不正です：" + e.Message);
                }
                return (reg.Matches(arguments[0].GetStrValue(exm)).Count);
            }
        }

        private sealed class ToStrMethod : FunctionMethod
        {
            public ToStrMethod()
            {
                ReturnType = typeof(string);
                argumentTypeArray = null;
                CanRestructure = true;
            }

            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                //通常2つ、1つ省略可能で1～2の引数が必要。
                if (arguments.Length < 1)
                    return name + "関数には少なくとも1つの引数が必要です";
                if (arguments.Length > 2)
                    return name + "関数の引数が多すぎます";
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                if (arguments[0].GetOperandType() != typeof(Int64))
                    return name + "関数の1番目の引数の型が正しくありません";
                if ((arguments.Length >= 2) && (arguments[1] != null) && (arguments[1].GetOperandType() != typeof(string)))
                    return name + "関数の2番目の引数の型が正しくありません";
                return null;
            }
            public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 i = arguments[0].GetIntValue(exm);
                if ((arguments.Length < 2) || (arguments[1] == null))
                    return (i.ToString());
                string format = arguments[1].GetStrValue(exm);
                string ret;
                try
                {
                    ret = i.ToString(format);
                }
                catch (FormatException)
                {
                    throw new CodeEE("TOSTR関数の書式指定が間違っています");
                }
                return (ret);
            }
        }

        private sealed class ToIntMethod : FunctionMethod
        {
            public ToIntMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(string) };
                CanRestructure = true;
            }

            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                string str = arguments[0].GetStrValue(exm);
                if (str == null || str == "")
                    return (0);
                //全角文字が入ってるなら無条件で0を返す
                if (str.Length < LangManager.GetStrlenLang(str))
                    return (0);
                StringStream st = new StringStream(str);
                if (!char.IsDigit(st.Current) && st.Current != '+' && st.Current != '-')
                    return (0);
                else if ((st.Current == '+' || st.Current == '-') && !char.IsDigit(st.Next))
                    return (0);
                Int64 ret = LexicalAnalyzer.ReadInt64(st, true);
                if (!st.EOS)
                {
                    if (st.Current == '.')
                    {
                        st.ShiftNext();
                        while (!st.EOS)
                        {
                            if (!char.IsDigit(st.Current))
                                return (0);
                            st.ShiftNext();
                        }
                    }
                    else
                        return (0);
                }
                return ret;
            }
        }

        //難読化用属性。enum.ToString()やenum.Parse()を行うなら(Exclude=true)にすること。
        [global::System.Reflection.Obfuscation(Exclude = false)]
        //TOUPPER等の処理を汎用化するためのenum
        enum StrFormType
        {
            Upper = 0,
            Lower = 1,
            Half = 2,
            Full = 3,
        };

        private sealed class StrChangeStyleMethod : FunctionMethod
        {
            StrFormType strType;
            public StrChangeStyleMethod()
            {
                ReturnType = typeof(string);
                argumentTypeArray = new Type[] { typeof(string) };
                strType = StrFormType.Upper;
                CanRestructure = true;
            }
            public StrChangeStyleMethod(StrFormType type)
            {
                ReturnType = typeof(string);
                argumentTypeArray = new Type[] { typeof(string) };
                strType = type;
                CanRestructure = true;
            }
            public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                string str = arguments[0].GetStrValue(exm);
                if (str == null || str == "")
                    return ("");
                switch (strType)
                {
                    case StrFormType.Upper:
                        return (str.ToUpper());
                    case StrFormType.Lower:
                        return (str.ToLower());
                    case StrFormType.Half:
                        return (Strings.StrConv(str, VbStrConv.Narrow, Config.Language));
                    case StrFormType.Full:
                        return (Strings.StrConv(str, VbStrConv.Wide, Config.Language));
                }
                return ("");
            }
        }

        private sealed class LineIsEmptyMethod : FunctionMethod
        {
            public LineIsEmptyMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { };
                CanRestructure = false;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                return GlobalStatic.Console.EmptyLine ? 1L : 0L;
            }
        }

        private sealed class ReplaceMethod : FunctionMethod
        {
            public ReplaceMethod()
            {
                ReturnType = typeof(string);
                argumentTypeArray = new Type[] { typeof(string), typeof(string), typeof(string) };
                CanRestructure = true;
            }
            public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                string baseString = arguments[0].GetStrValue(exm);
                Regex reg;
                try
                {
                    reg = new Regex(arguments[1].GetStrValue(exm));
                }
                catch (ArgumentException e)
                {
                    throw new CodeEE("第２引数が正規表現として不正です：" + e.Message);
                }
                return (reg.Replace(baseString, arguments[2].GetStrValue(exm)));
            }
        }

        private sealed class UnicodeMethod : FunctionMethod
        {
            public UnicodeMethod()
            {
                ReturnType = typeof(string);
                argumentTypeArray = new Type[] { typeof(Int64) };
                CanRestructure = true;
            }
            public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 i = arguments[0].GetIntValue(exm);
                if ((i < 0) || (i > 0xFFFF))
                    throw new CodeEE("UNICODE関数に範囲外の値(" + i.ToString() + ")が渡されました");
                string s = new string(new char[] { (char)i });// char.ConvertFromUtf32(i);

                return (s);
            }
        }

        private sealed class UnicodeByteMethod : FunctionMethod
        {
            public UnicodeByteMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(string) };
                CanRestructure = true;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                string target = arguments[0].GetStrValue(exm);
                int length = Encoding.UTF32.GetEncoder().GetByteCount(target.ToCharArray(), 0, target.Length, false);
                byte[] bytes = new byte[length];
                Encoding.UTF32.GetEncoder().GetBytes(target.ToCharArray(), 0, target.Length, bytes, 0, false);
                Int64 i = (Int64)BitConverter.ToInt32(bytes, 0);

                return (i);
            }
        }

        private sealed class ConvertIntMethod : FunctionMethod
        {
            public ConvertIntMethod()
            {
                ReturnType = typeof(string);
                argumentTypeArray = new Type[] { typeof(Int64), typeof(Int64) };
                CanRestructure = true;
            }
            public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                Int64 toBase = arguments[1].GetIntValue(exm);
                if ((toBase != 2) && (toBase != 8) && (toBase != 10) && (toBase != 16))
                    throw new CodeEE("CONVERT関数の第２引数は2, 8, 10, 16のいずれかでなければなりません");
                return Convert.ToString(arguments[0].GetIntValue(exm), (int)toBase);
            }
        }

        private sealed class IsNumericMethod : FunctionMethod
        {
            public IsNumericMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { typeof(string) };
                CanRestructure = true;
            }
            public override long GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                string baseStr = arguments[0].GetStrValue(exm);

                //全角文字があるなら数値ではない
                if (baseStr.Length < LangManager.GetStrlenLang(baseStr))
                    return (0);
                StringStream st = new StringStream(baseStr);
                if (!char.IsDigit(st.Current) && st.Current != '+' && st.Current != '-')
                    return (0);
                else if ((st.Current == '+' || st.Current == '-') && !char.IsDigit(st.Next))
                    return (0);
                Int64 ret = LexicalAnalyzer.ReadInt64(st, true);
                if (!st.EOS)
                {
                    if (st.Current == '.')
                    {
                        st.ShiftNext();
                        while (!st.EOS)
                        {
                            if (!char.IsDigit(st.Current))
                                return (0);
                            st.ShiftNext();
                        }
                    }
                    else
                        return (0);
                }
                return 1;
            }
        }

        private sealed class EscapeMethod : FunctionMethod
        {
            public EscapeMethod()
            {
                ReturnType = typeof(string);
                argumentTypeArray = new Type[] { typeof(string) };
                CanRestructure = true;
            }
            public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                return Regex.Escape(arguments[0].GetStrValue(exm));
            }
        }

        private sealed class EncodeToUniMethod : FunctionMethod
        {
            public EncodeToUniMethod()
            {
                ReturnType = typeof(Int64);
                argumentTypeArray = new Type[] { null };
                CanRestructure = true;
            }
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                //通常2つ、1つ省略可能で1～2の引数が必要。
                if (arguments.Length < 1)
                    return name + "関数には少なくとも1つの引数が必要です";
                if (arguments.Length > 2)
                    return name + "関数の引数が多すぎます";
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                if (arguments[0].GetOperandType() != typeof(string))
                    return name + "関数の1番目の引数の型が正しくありません";
                if ((arguments.Length >= 2) && (arguments[1] != null) && (arguments[1].GetOperandType() != typeof(Int64)))
                    return name + "関数の2番目の引数の型が正しくありません";
                return null;
            }
            public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                string baseStr = arguments[0].GetStrValue(exm);
                if (baseStr.Length == 0)
                    return -1;
                Int64 position = (arguments.Length > 1 && arguments[1] != null) ? arguments[1].GetIntValue(exm) : 0;
                if (position < 0)
                    throw new CodeEE("ENCOIDETOUNI関数の第２引数(" + position.ToString() + ")が負の値です");
                if (position >= baseStr.Length)
                    throw new CodeEE("ENCOIDETOUNI関数の第２引数(" + position.ToString() + ")が第１引数の文字列(" + baseStr + ")の文字数を超えています");
                return char.ConvertToUtf32(baseStr, (int)position);
            }
        }

        public sealed class CharAtMethod : FunctionMethod
        {
            public CharAtMethod()
            {
                ReturnType = typeof(string);
                argumentTypeArray = new Type[] { typeof(string), typeof(Int64) };
                CanRestructure = true;
            }
            public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                string str = arguments[0].GetStrValue(exm);
                Int64 pos = arguments[1].GetIntValue(exm);
                if (pos < 0 || pos >= str.Length)
                    return "";
                return str[(int)pos].ToString();
            }
        }

        public sealed class GetLineStrMethod : FunctionMethod
        {
            public GetLineStrMethod()
            {
                ReturnType = typeof(string);
                argumentTypeArray = new Type[] { typeof(string) };
                CanRestructure = true;
            }
            public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
                string str = arguments[0].GetStrValue(exm);
				if (string.IsNullOrEmpty(str))
					throw new CodeEE("GETLINESTR関数の引数が空文字列です");
                return exm.Console.getStBar(str);
            }
        }

		public sealed class StrFormMethod : FunctionMethod
		{
			public StrFormMethod()
			{
				ReturnType = typeof(string);
				argumentTypeArray = new Type[] { typeof(string) };
                HasUniqueRestructure = true;
				CanRestructure = true;
			}
			public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				string str = arguments[0].GetStrValue(exm);
				string destStr = str;
				try
				{
					StrFormWord wt = LexicalAnalyzer.AnalyseFormattedString(new StringStream(str), FormStrEndWith.EoL, false);
					StrForm strForm = StrForm.FromWordToken(wt);
					destStr = strForm.GetString(exm);
				}
				catch(CodeEE e)
				{
					throw new CodeEE("STRFORM関数:文字列\"" + str + "\"の展開エラー:" + e.Message);
				}
				catch
				{
					throw new CodeEE("STRFORM関数:文字列\"" + str+ "\"の展開処理中にエラーが発生しました");
				}
				return destStr;
			}
            public override bool UniqueRestructure(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                arguments[0].Restructure(exm);
                //引数が文字列式等ならお手上げなので諦める
                if (!(arguments[0] is SingleTerm) && !(arguments[0] is VariableTerm))
                    return false;
                //引数が確定値でない文字列変数なら無条件で不可（結果が可変なため）
                if ((arguments[0] is VariableTerm) && !(((VariableTerm)arguments[0]).Identifier.IsConst))
                    return false;
                string str = arguments[0].GetStrValue(exm);
                try
                {
                    StrFormWord wt = LexicalAnalyzer.AnalyseFormattedString(new StringStream(str), FormStrEndWith.EoL, false);
                    StrForm strForm = StrForm.FromWordToken(wt);
                    if (!strForm.IsConst)
                        return false;
                }
                catch
                {
                    //パースできないのはエラーがあるかここではわからないからとりあえず考えない
                    return false;
                }
                return true;
            }
        }

        public sealed class JoinMethod : FunctionMethod
        {
            public JoinMethod()
            {
                ReturnType = typeof(string);
                argumentTypeArray = null;
                HasUniqueRestructure = false;
                CanRestructure = false;
            }
            public override string CheckArgumentType(string name, IOperandTerm[] arguments)
            {
                if (arguments.Length < 1)
                    return name + "関数には少なくとも1つの引数が必要です";
                if (arguments.Length > 4)
                    return name + "関数の引数が多すぎます";
                if (arguments[0] == null)
                    return name + "関数の1番目の引数は省略できません";
                if (!(arguments[0] is VariableTerm))
                    return name + "関数の1番目の引数が変数ではありません";
                VariableTerm varToken = (VariableTerm)arguments[0];
                if (!varToken.Identifier.IsArray1D && !varToken.Identifier.IsArray2D && !varToken.Identifier.IsArray3D)
                    return name + "関数の1番目の引数が配列変数ではありません";
                if (arguments.Length == 1)
                    return null;
                if ((arguments[1] != null) && (arguments[1].GetOperandType() != typeof(string)))
                    return name + "関数の2番目の変数が文字列ではありません";
                if (arguments.Length == 2)
                    return null;
                if ((arguments[2] != null) && (arguments[2].GetOperandType() != typeof(Int64)))
                    return name + "関数の3番目の変数が数値ではありません";
                if (arguments.Length == 3)
                    return null;
                if ((arguments[3] != null) && (arguments[3].GetOperandType() != typeof(Int64)))
                    return name + "関数の4番目の変数が数値ではありません";
                return null;
            }
            public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
            {
                VariableTerm varTerm = (VariableTerm)arguments[0];
                string delimiter = (arguments.Length >= 2 && arguments[1] != null) ? arguments[1].GetStrValue(exm) : ",";
                Int64 index1 = (arguments.Length >= 3 && arguments[2] != null) ? arguments[2].GetIntValue(exm) : 0;
                Int64 index2 = (arguments.Length == 4 && arguments[3] != null) ? arguments[3].GetIntValue(exm) : varTerm.GetLastLength();

                FixedVariableTerm p = varTerm.GetFixedVariableTerm(exm);

                p.IsArrayRangeValid(index1, index2, "STRJOIN", 2L, 3L);
                return (exm.VEvaluator.GetJoinedStr(p, delimiter, index1, index2));
            }
        }
		
		public sealed class GetConfigMethod : FunctionMethod
		{
			public GetConfigMethod(bool typeisInt)
			{
				if(typeisInt)
				{
					funcname = "GETCONFIG";
					ReturnType = typeof(Int64);
				}
				else
				{
					funcname = "GETCONFIGS";
					ReturnType = typeof(string);
				}
				argumentTypeArray = new Type[] { typeof(string) };
				CanRestructure = true;
			}
			private readonly string funcname;
			private SingleTerm getSingleTerm(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				string str = arguments[0].GetStrValue(exm);
				if(str == null || str.Length == 0)
					throw new CodeEE(funcname + "関数に空文字列が渡されました");
				string errMes = null;
				SingleTerm term = ConfigData.Instance.GetConfigValueInERB(str, ref errMes);
				if(errMes != null)
					throw new CodeEE(funcname + "関数:" + errMes);
				return term;
			}
			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				if(ReturnType != typeof(Int64))
					throw new ExeEE(funcname + "関数:不正な呼び出し");
				SingleTerm term = getSingleTerm(exm, arguments);
				if(term.GetOperandType() != typeof(Int64))
					throw new CodeEE(funcname + "関数:型が違います（GETCONFIGS関数を使用してください）");
				return term.Int;
			}
			public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				if(ReturnType != typeof(string))
					throw new ExeEE(funcname + "関数:不正な呼び出し");
				SingleTerm term = getSingleTerm(exm, arguments);
				if (term.GetOperandType() != typeof(string))
					throw new CodeEE(funcname + "関数:型が違います（GETCONFIG関数を使用してください）");
				return term.Str;
			}
		}
        #endregion

		#region html系

		private sealed class HtmlGetPrintedStrMethod : FunctionMethod
		{
			public HtmlGetPrintedStrMethod()
			{
				ReturnType = typeof(string);
				argumentTypeArray = null;
				CanRestructure = false;
			}

			public override string CheckArgumentType(string name, IOperandTerm[] arguments)
			{
				//通常１つ。省略可能。
				if (arguments.Length > 1)
					return name + "関数の引数が多すぎます";
				if (arguments.Length == 0|| arguments[0] == null)
					return null;
				if (arguments[0].GetOperandType() != typeof(Int64))
					return name + "関数の1番目の引数の型が正しくありません";
				return null;
			}
			public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				Int64 lineNo = 0;
				if (arguments.Length > 0)
					lineNo = arguments[0].GetIntValue(exm);
				if (lineNo < 0)
					throw new CodeEE("引数を0未満にできません");
				ConsoleDisplayLine[] dispLines = exm.Console.GetDisplayLines(lineNo);
				if (dispLines == null)
					return "";
				return HtmlManager.DisplayLine2Html(dispLines, true);
			}
		}

		private sealed class HtmlPopPrintingStrMethod : FunctionMethod
		{
			public HtmlPopPrintingStrMethod()
			{
				ReturnType = typeof(string);
				argumentTypeArray = new Type[] { };
				CanRestructure = false;
			}

			public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				ConsoleDisplayLine[] dispLines = exm.Console.PopDisplayingLines();
				if (dispLines == null)
					return "";
				return HtmlManager.DisplayLine2Html(dispLines, false);
			}
		}

		private sealed class HtmlToPlainTextMethod : FunctionMethod
		{
			public HtmlToPlainTextMethod()
			{
				ReturnType = typeof(string);
                argumentTypeArray = new Type[] { typeof(string) };
				CanRestructure = false;
			}
			public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				return HtmlManager.Html2PlainText(arguments[0].GetStrValue(exm));
			}
		}
		private sealed class HtmlEscapeMethod : FunctionMethod
		{
			public HtmlEscapeMethod()
			{
				ReturnType = typeof(string);
                argumentTypeArray = new Type[] { typeof(string) };
				CanRestructure = false;
			}
			public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				return HtmlManager.Escape(arguments[0].GetStrValue(exm));
			}
		}
		#endregion
	}
}
