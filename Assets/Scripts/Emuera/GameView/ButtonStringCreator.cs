using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MinorShift.Emuera.Sub;

namespace MinorShift.Emuera.GameView
{
	internal sealed class ButtonPrimitive
	{
		public string Str = "";
		public Int64 Input;
		public bool CanSelect = false;
		public override string ToString()
		{
			return Str;
		}
	}

	internal static class ButtonStringCreator
	{
		public static List<string> Split(string printBuffer)
		{
			List<ButtonPrimitive> list = syn(printBuffer);
			List<string> ret = new List<string>();
			foreach(ButtonPrimitive p in list)
				ret.Add(p.Str);
			return ret;
		}
		public static List<ButtonPrimitive> SplitButton(string printBuffer)
		{
			return syn(printBuffer);
		}

		private static List<ButtonPrimitive> syn(string printBuffer)
		{
			string printString = printBuffer.ToString();
			List<ButtonPrimitive> ret = new List<ButtonPrimitive>();
			if (printString.Length == 0)
				goto nonButton;
			List<string> strs;
			if ((!printString.Contains("[")) || (!printString.Contains("]")))
				goto nonButton;
			strs = lex(new StringStream(printString));
			if (strs == null)
				goto nonButton;
			bool beforeButton = false;//最初のボタン（"[1]"とか）より前にテキストがある
			bool afterButton = false;//最後のボタン（"[1]"とか）より後にテキストがある
			int buttonCount = 0;
			Int64 inpL = 0;
			for (int i = 0; i < strs.Count; i++)
			{
				if (strs[i].Length == 0)
					continue;
				char c = strs[i][0];
				if (LexicalAnalyzer.IsWhiteSpace(c))
				{//ただの空白
				}
				//数値以外はボタン化しない方向にした。
				//else if ((c == '[') && (!isSymbols(strArray[i])))
				else if (isButtonCore(strs[i], ref inpL))
				{//[]で囲まれた文字列。選択肢の核となるかどうかはこの段階では判定しない。
					buttonCount++;
					afterButton = false;
				}
				else
				{//選択肢の説明になるかもしれない文字列
                    afterButton = true;
					if (buttonCount == 0)
						beforeButton = true;
				}
			}
			if (buttonCount <= 1)
			{
                ButtonPrimitive button = new ButtonPrimitive
                {
                    Str = printBuffer.ToString(),
                    CanSelect = (buttonCount >= 1),
                    Input = inpL
                };
                ret.Add(button);
				return ret;
			}
			buttonCount = 0;
			bool alignmentRight = !beforeButton && afterButton;//説明はボタンの右固定
			bool alignmentLeft = beforeButton && !afterButton;//説明はボタンの左固定
			bool alignmentEtc = !alignmentRight && !alignmentLeft;//臨機応変に
			bool canSelect = false;
			Int64 input = 0;

			int state = 0;
			StringBuilder buffer = new StringBuilder();
            void reduce()
            {
                if (buffer.Length == 0)
                    return;
                ButtonPrimitive button = new ButtonPrimitive
                {
                    Str = buffer.ToString(),
                    CanSelect = canSelect,
                    Input = input
                };
                ret.Add(button);
                buffer.Remove(0, buffer.Length);
                canSelect = false;
                input = 0;
            }
            for (int i = 0; i < strs.Count; i++)
			{
				if (strs[i].Length == 0)
					continue;
				char c = strs[i][0];
				if (LexicalAnalyzer.IsWhiteSpace(c))
				{//ただの空白
					if (((state & 3) == 3) && (alignmentEtc) && (strs[i].Length >= 2))
					{//核と説明を含んだものが完成していればボタン生成。
						//一文字以下のスペースはキニシナイ。キャラ購入画面対策
                        reduce();
						buffer.Append(strs[i]);
						state = 0;
					}
					else
					{
						buffer.Append(strs[i]);
					}
					continue;
				}
				if(isButtonCore(strs[i], ref inpL))
				{
					buttonCount++;
					if (((state & 1) == 1) || alignmentRight)
					{//bufferが既に核を含んでいる、又は強制的に右配置
						reduce();
						buffer.Append(strs[i]);
						input = inpL;
						canSelect = true;
						state = 1;
					}//((state & 2) == 2) || 
					else if (alignmentLeft)
					{//bufferが説明を含んでいる、又は強制的に左配置
						buffer.Append(strs[i]);
						input = inpL;
						canSelect = true;
						reduce();
						state = 0;
					}
					else
					{//bufferが空または空白文字列
						buffer.Append(strs[i]);
						input = inpL;
						canSelect = true;
						state = 1;
					}
					continue;
				}
				//else
				//{//選択肢の説明になるかもしれない文字列
					
					buffer.Append(strs[i]);
					state |= 2;
				//}
				
			};
			reduce();
			return ret;
		nonButton:
			ret = new List<ButtonPrimitive>();
            ButtonPrimitive singleButton = new ButtonPrimitive
            {
                Str = printString
            };
            ret.Add(singleButton);
			return ret;
		}
		readonly static Regex numReg = new Regex(@"\[\s*([0][xXbB])?[+-]?[0-9]+([eEpP][0-9]+)?\s*\]");

		/// <summary>
		/// []付き文字列が数値的であるかどうかを調べる
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		private static bool isNumericWord(string str)
		{
			return numReg.IsMatch(str);
		}

		/// <summary>
		/// ボタンの核になるかどうか。とりあえずは整数のみ。
		/// try-catchを利用するので少し重い。
		/// </summary>
		/// <param name="str"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		private static bool isButtonCore(string str, ref long input)
		{
			if((str == null)||(str.Length < 3)||(str[0] != '[')||(str[str.Length-1] != ']'))
				return false;
			if (!isNumericWord(str))
				return false;
			string buttonStr = str.Substring(1, str.Length - 2);
			StringStream stInt = new StringStream(buttonStr);
			LexicalAnalyzer.SkipAllSpace(stInt);
			try
			{
				input = LexicalAnalyzer.ReadInt64(stInt, false);
			}
			catch
			{
				return false; 
			}
			return true;
		}

		/// <summary>
		/// 字句分割
		/// "[1] あ [2] いうえ "を"[1]"," ", "あ"," ","[2]"," ","いうえ"," "に分割
		/// </summary>
		/// <param name="st"></param>
		/// <returns></returns>
		private static List<string> lex(StringStream st)
		{
			List<string> strs = new List<string>();
			int state = 0;
			int startIndex = 0;
            void reduce()
            {
                if (st.CurrentPosition == startIndex)
                    return;
                int length = st.CurrentPosition - startIndex;
                strs.Add(st.Substring(startIndex, length));
                startIndex = st.CurrentPosition;
            }
            while (!st.EOS)
			{
				if (st.Current == '[')
				{
					if (state == 1)//"["内部
						goto unanalyzable;
					reduce();
					state = 1;
					st.ShiftNext();
				}
				else if (st.Current == ']')
				{
					if (state != 1)//"["外部
						goto unanalyzable;
					st.ShiftNext();
					reduce();
					state = 0;
				}
				else if ((state == 0) && (LexicalAnalyzer.IsWhiteSpace(st.Current)))
				{
					reduce();
					LexicalAnalyzer.SkipAllSpace(st);
					reduce();
				}
				else
				{
					st.ShiftNext();
				}
			}
			reduce();
			return strs;
		unanalyzable:
			return null;
		}

	}
}
