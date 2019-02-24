using System;
using System.Collections.Generic;
using System.Text;
using MinorShift.Emuera.Sub;
using MinorShift.Emuera.GameData.Variable;
using MinorShift.Emuera.GameData.Function;
using MinorShift.Emuera.GameProc;
//using Microsoft.VisualBasic;
using MinorShift.Emuera.GameProc.Function;
using MinorShift.Emuera.GameView;
//using System.Windows.Forms;
using uEmuera.VisualBasic;

namespace MinorShift.Emuera.GameData.Expression
{
	//1756 元ExpressionEvaluator。GetValueの仕事はなくなったので改名。
	//IOperandTerm間での通信や共通の処理に使う。
	//変数が絡む仕事はVariableEvaluatorへ。
	internal sealed class ExpressionMediator
	{
		public ExpressionMediator(Process proc, VariableEvaluator vev, EmueraConsole console)
		{
			VEvaluator = vev;
			Process = proc;
			Console = console;
		}
		public readonly VariableEvaluator VEvaluator;
		public readonly Process Process;
		public readonly EmueraConsole Console;
		
		
		
		private bool forceHiragana;
		private bool forceKatakana;
		private bool halftoFull;
		
		public void ForceKana(Int64 flag)
		{
			if (flag < 0 || flag > 3)
				throw new CodeEE("命令FORCEKANAの引数が指定可能な範囲(0～3)を超えています");
			forceKatakana = (flag == 1) ? true : false;
			forceHiragana = (flag > 1) ? true : false;
			halftoFull = (flag == 3) ? true : false;
		}
		
		public bool ForceKana()
		{
			return (forceHiragana | forceKatakana | halftoFull);
		}

		public void OutputToConsole(string str, FunctionIdentifier func)
		{
			if (func.IsPrintSingle())
				Console.PrintSingleLine(str, false);
			else
			{
				Console.Print(str);
				if (func.IsNewLine() || func.IsWaitInput())
				{
					Console.NewLine();
					if (func.IsWaitInput())
						Console.ReadAnyKey();
				}
			}
			Console.UseSetColorStyle = true;
		}

		public string ConvertStringType(string str)
		{
			if (!(forceHiragana | forceKatakana | halftoFull))
				return str;
			if (forceKatakana)
                return Strings.StrConv(str, VbStrConv.Katakana, 0x0411);
			else if (forceHiragana)
			{
				if (halftoFull)
                    return Strings.StrConv(str, VbStrConv.Hiragana | VbStrConv.Wide, 0x0411);
				else
                    return Strings.StrConv(str, VbStrConv.Hiragana, 0x0411);
			}
			return str;
		}

		public string CheckEscape(string str)
		{
			StringStream st = new StringStream(str);
			StringBuilder buffer = new StringBuilder();

			while (!st.EOS)
			{
				//エスケープ文字の使用
				if (st.Current == '\\')
				{
					st.ShiftNext();
					switch (st.Current)
					{
						case '\\':
							buffer.Append('\\');
							buffer.Append('\\');
							break;
						case '{':
						case '}':
						case '%':
						case '@':
							buffer.Append('\\');
							buffer.Append(st.Current);
							break;
						default:
							buffer.Append("\\\\");
							buffer.Append(st.Current);
							break;
					}
					st.ShiftNext();
					continue;
				}
				buffer.Append(st.Current);
				st.ShiftNext();
			}
			return buffer.ToString();
		}

		public string CreateBar(Int64 var, Int64 max, Int64 length)
		{
			if (max <= 0)
				throw new CodeEE("BARの最大値が正の値ではありません");
			if (length <= 0)
				throw new CodeEE("BARの長さが正の値ではありません");
			if (length >= 100)//暴走を防ぐため。
				throw new CodeEE("BARが長すぎます");
			StringBuilder builder = new StringBuilder();
			builder.Append('[');
			int count;
			unchecked
			{
				count = (int)(var * length / max);
			}
			if (count < 0)
				count = 0;
			if (count > length)
				count = (int)length;
			builder.Append(Config.BarChar1, count);
			builder.Append(Config.BarChar2, (int)length - count);
			builder.Append(']');
			return builder.ToString();
		}
	}
}


