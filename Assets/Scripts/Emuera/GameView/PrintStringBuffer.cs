using System;
using System.Collections.Generic;
using System.Text;
//using System.Drawing;
//using System.Windows.Forms;
using MinorShift.Emuera.Sub;
using uEmuera.Drawing;

namespace MinorShift.Emuera.GameView
{
	/*
	 * ConsoleStyledString = string + StringStyle
	 * ConsoleButtonString = (ConsoleStyledString) * n + ButtonValue
	 * ConsoleDisplayLine = (ConsoleButtonString) * n
	 * PrintStringBufferはERBのPRINT命令からConsoleDisplayLineを作る
	*/

	/// <summary>
	/// PRINT命令を貯める＆最終的に解決するクラス
	/// </summary>
	internal sealed class PrintStringBuffer
	{
		public PrintStringBuffer(EmueraConsole parent)
		{
			this.parent = parent;
		}
		readonly EmueraConsole parent;
		StringBuilder builder = new StringBuilder();
		List<AConsoleDisplayPart> m_stringList = new List<AConsoleDisplayPart>();
		StringStyle lastStringStyle = new StringStyle();
		List<ConsoleButtonString> m_buttonList = new List<ConsoleButtonString>();

		public int BufferStrLength
		{
			get
			{
				int length = 0;

                var count = m_stringList.Count;
                AConsoleDisplayPart css = null;
                for(var i=0; i<count; ++i)
				{
                    css = m_stringList[i];
					if (css is ConsoleStyledString)
						length += css.Str.Length;
					else
						length += 1;
				}
				return length;
			}
		}

		public void Append(AConsoleDisplayPart part)
		{
			if (builder.Length != 0)
			{
				m_stringList.Add(new ConsoleStyledString(builder.ToString(), lastStringStyle));
				builder.Remove(0, builder.Length);
			}
			m_stringList.Add(part);
		}

		public void Append(string str, StringStyle style)
		{
			Append(str, style, false);
		}

		public void Append(string str, StringStyle style, bool force_button)
		{
			if (BufferStrLength > 2000)
				return;
			if (force_button)
				fromCssToButton();
			if ((builder.Length == 0) || (lastStringStyle == style))
			{
				if (builder.Length > 2000)
					return;
				if (builder.Length + str.Length > 2000)
					str = str.Substring(0, 2000 - builder.Length) + "※※※バッファーの文字数が2000字(全角1000字)を超えています。これ以降は表示できません※※※";
				builder.Append(str);
				lastStringStyle = style;
			}
			else
			{
				m_stringList.Add(new ConsoleStyledString(builder.ToString(), lastStringStyle));
				builder.Remove(0, builder.Length);
				builder.Append(str);
				lastStringStyle = style;
			}
			if (force_button)
				fromCssToButton();
		}

		public void AppendButton(string str, StringStyle style, string input)
		{
			fromCssToButton();
			m_stringList.Add(new ConsoleStyledString(str, style));
			if (m_stringList.Count == 0)
				return;
			m_buttonList.Add(createButton(m_stringList, input));
			m_stringList.Clear();
		}



		public void AppendButton(string str, StringStyle style, long input)
		{
			fromCssToButton();
			m_stringList.Add(new ConsoleStyledString(str, style));
			if (m_stringList.Count == 0)
				return;
			m_buttonList.Add(createButton(m_stringList, input));
			m_stringList.Clear();
		}

		public void AppendPlainText(string str, StringStyle style)
		{
			fromCssToButton();
			m_stringList.Add(new ConsoleStyledString(str, style));
			if (m_stringList.Count == 0)
				return;
			m_buttonList.Add(createPlainButton(m_stringList));
			m_stringList.Clear();
		}

		public bool IsEmpty
		{
			get
			{
				return ((m_buttonList.Count == 0) && (builder.Length == 0) && (m_stringList.Count == 0));
			}
		}

		public override string ToString()
		{
			StringBuilder buf = new StringBuilder();
			foreach (ConsoleButtonString button in m_buttonList)
				buf.Append(button.ToString());
			foreach (AConsoleDisplayPart css in m_stringList)
				buf.Append(css.Str);
			buf.Append(builder);
			return buf.ToString();
		}

		public ConsoleDisplayLine AppendAndFlushErrButton(string str, StringStyle style, string input, ScriptPosition pos, StringMeasure sm)
		{
			fromCssToButton();
			m_stringList.Add(new ConsoleStyledString(str, style));
			if (m_stringList.Count == 0)
				return null;
			m_buttonList.Add(createButton(m_stringList, input, pos));
			m_stringList.Clear();
			return FlushSingleLine(sm, false);
		}

		public ConsoleDisplayLine FlushSingleLine(StringMeasure stringMeasure, bool temporary)
		{
			fromCssToButton();
			setWidthToButtonList(m_buttonList, stringMeasure, true);
			ConsoleButtonString[] dispLineButtonArray = new ConsoleButtonString[m_buttonList.Count];
			m_buttonList.CopyTo(dispLineButtonArray);
			ConsoleDisplayLine line = new ConsoleDisplayLine(dispLineButtonArray, true, temporary);
			this.clearBuffer();
			return line;
		}

		public ConsoleDisplayLine[] Flush(StringMeasure stringMeasure, bool temporary)
		{
			fromCssToButton();
			ConsoleDisplayLine[] ret = PrintStringBuffer.ButtonsToDisplayLines(m_buttonList, stringMeasure, false, temporary);
			this.clearBuffer();
			return ret;
		}

		private static ConsoleDisplayLine m_buttonsToDisplayLine(List<ConsoleButtonString> lineButtonList, bool firstLine, bool temporary)
		{
			ConsoleButtonString[] dispLineButtonArray = new ConsoleButtonString[lineButtonList.Count];
			lineButtonList.CopyTo(dispLineButtonArray);
			lineButtonList.Clear();
			return new ConsoleDisplayLine(dispLineButtonArray, firstLine, temporary);
		}

		public static ConsoleDisplayLine[] ButtonsToDisplayLines(List<ConsoleButtonString> buttonList, StringMeasure stringMeasure, bool nobr, bool temporary)
		{
			if (buttonList.Count == 0)
				return new ConsoleDisplayLine[0];
			setWidthToButtonList(buttonList, stringMeasure, nobr);
			List<ConsoleDisplayLine> lineList = new List<ConsoleDisplayLine>();
			List<ConsoleButtonString> lineButtonList = new List<ConsoleButtonString>();
			int windowWidth = Config.DrawableWidth;
			bool firstLine = true;
			for (int i = 0; i < buttonList.Count; i++)
			{
				if (buttonList[i] == null)
				{//強制改行フラグ
					lineList.Add(m_buttonsToDisplayLine(lineButtonList, firstLine, temporary));
					firstLine = false;
					buttonList.RemoveAt(i);
					i--;
					continue;
				}
				if (nobr || ((buttonList[i].PointX + buttonList[i].Width <= windowWidth)))
				{//改行不要モードであるか表示可能領域に収まるならそのままでよい
					lineButtonList.Add(buttonList[i]);
					continue;
				}
				//新しい表示行を作る

				//ボタンを分割するか？
				//「ボタンの途中で行を折りかえさない」がfalseなら分割する
				//このボタンが単体で表示可能領域を上回るなら分割必須
				//クリック可能なボタンでないなら分割する。ただし「ver1739以前の非ボタン折り返しを再現する」ならクリックの可否を区別しない
				if ((!Config.ButtonWrap) || (lineButtonList.Count == 0) || (!buttonList[i].IsButton && !Config.CompatiLinefeedAs1739))
				{//ボタン分割する
					int divIndex = getDivideIndex(buttonList[i], stringMeasure);
					if (divIndex > 0)
					{
						ConsoleButtonString newButton = buttonList[i].DivideAt(divIndex, stringMeasure);
						//newButton.CalcPointX(buttonList[i].PointX + buttonList[i].Width);
						buttonList.Insert(i + 1, newButton);
						lineButtonList.Add(buttonList[i]);
						i++;
					}
					else if (divIndex == 0 && (lineButtonList.Count > 0))
					{//まるごと次の行に送る
					}
					else//分割できない要素のみで構成されたボタンは分割できない
					{
						lineButtonList.Add(buttonList[i]);
						continue;
					}
				}
				lineList.Add(m_buttonsToDisplayLine(lineButtonList, firstLine, temporary));
				firstLine = false;
				//位置調整
//				shiftX = buttonList[i].PointX;
				int pointX = 0;
				for (int j = i; j < buttonList.Count; j++)
				{
					if (buttonList[j] == null)//強制改行を挟んだ後は調整無用
						break;
					buttonList[j].CalcPointX(pointX);
					pointX += buttonList[j].Width;
				}
				i--;//buttonList[i]は新しい行に含めないので次の行のために再検討する必要がある(直後のi++と相殺)
			}
			if (lineButtonList.Count > 0)
			{
				lineList.Add(m_buttonsToDisplayLine(lineButtonList, firstLine, temporary));
			}
			ConsoleDisplayLine[] ret = new ConsoleDisplayLine[lineList.Count];
			lineList.CopyTo(ret);
			return ret;
		}

		/// <summary>
		/// 1810beta003新規 マークアップ用 Append とFlushを同時にやる
		/// </summary>
		/// <param name="str"></param>
		/// <param name="stringMeasure"></param>
		/// <returns></returns>
		public ConsoleDisplayLine[] PrintHtml(string str, StringMeasure stringMeasure)
		{
			throw new NotImplementedException();
		}

		#region Flush用privateメソッド

		private void clearBuffer()
		{
			builder.Remove(0, builder.Length);
			m_stringList.Clear();
			m_buttonList.Clear();
		}

		/// <summary>
		/// cssListをbuttonに変換し、buttonListに追加。
		/// この時点ではWidthなどは考えない。
		/// </summary>
		private void fromCssToButton()
		{
			if (builder.Length != 0)
			{
				m_stringList.Add(new ConsoleStyledString(builder.ToString(), lastStringStyle));
				builder.Remove(0, builder.Length);
			}
			if (m_stringList.Count == 0)
				return;
			m_buttonList.AddRange(createButtons(m_stringList));
			m_stringList.Clear();
		}

		/// <summary>
		/// 物理行を１つのボタンへ。
		/// </summary>
		/// <returns></returns>
		private ConsoleButtonString createButton(List<AConsoleDisplayPart> cssList, string input)
		{
			AConsoleDisplayPart[] cssArray = new AConsoleDisplayPart[cssList.Count];
			cssList.CopyTo(cssArray);
			cssList.Clear();
			return new ConsoleButtonString(parent, cssArray, input);
		}
		private ConsoleButtonString createButton(List<AConsoleDisplayPart> cssList, string input, ScriptPosition pos)
		{
			AConsoleDisplayPart[] cssArray = new AConsoleDisplayPart[cssList.Count];
			cssList.CopyTo(cssArray);
			cssList.Clear();
			return new ConsoleButtonString(parent, cssArray, input, pos);
		}
		private ConsoleButtonString createButton(List<AConsoleDisplayPart> cssList, long input)
		{
			AConsoleDisplayPart[] cssArray = new AConsoleDisplayPart[cssList.Count];
			cssList.CopyTo(cssArray);
			cssList.Clear();
			return new ConsoleButtonString(parent, cssArray, input);
		}
		private ConsoleButtonString createPlainButton(List<AConsoleDisplayPart> cssList)
		{
			AConsoleDisplayPart[] cssArray = new AConsoleDisplayPart[cssList.Count];
			cssList.CopyTo(cssArray);
			cssList.Clear();
			return new ConsoleButtonString(parent, cssArray);
		}

		/// <summary>
		/// 物理行をボタン単位に分割。引数のcssListの内容は変更される場合がある。
		/// </summary>
		/// <returns></returns>
		private ConsoleButtonString[] createButtons(List<AConsoleDisplayPart> cssList)
		{
			StringBuilder buf = new StringBuilder();
			for (int i = 0; i < cssList.Count; i++)
			{
				buf.Append(cssList[i].Str);
			}
			List<ButtonPrimitive> bpList = ButtonStringCreator.SplitButton(buf.ToString());
			ConsoleButtonString[] ret = new ConsoleButtonString[bpList.Count];
			AConsoleDisplayPart[] cssArray = null;
			if (ret.Length == 1)
			{
				cssArray = new AConsoleDisplayPart[cssList.Count];
				cssList.CopyTo(cssArray);
				if (bpList[0].CanSelect)
					ret[0] = new ConsoleButtonString(parent, cssArray, bpList[0].Input);
				else
					ret[0] = new ConsoleButtonString(parent, cssArray);
				return ret;
			}
			int cssStartCharIndex = 0;
			int buttonEndCharIndex = 0;
			int cssIndex = 0;
			List<AConsoleDisplayPart> buttonCssList = new List<AConsoleDisplayPart>();
			for (int i = 0; i < ret.Length; i++)
			{
				ButtonPrimitive bp = bpList[i];
				buttonEndCharIndex += bp.Str.Length;
				while (true)
				{
					if (cssIndex >= cssList.Count)
						break;
					AConsoleDisplayPart css = cssList[cssIndex];
					if (cssStartCharIndex + css.Str.Length >= buttonEndCharIndex)
					{//ボタンの終端を発見
						int used = buttonEndCharIndex - cssStartCharIndex;
						if (used > 0 && css.CanDivide)
						{//cssの区切りの途中でボタンの区切りがある。
							
							ConsoleStyledString newCss = ((ConsoleStyledString)css).DivideAt(used);
							if (newCss != null)
							{
								cssList.Insert(cssIndex + 1, newCss);
								newCss.PointX = css.PointX + css.Width;
							}
						}
						buttonCssList.Add(css);
						cssStartCharIndex += css.Str.Length;
						cssIndex++;
						break;
					}
					//ボタンの終端はまだ先。
					buttonCssList.Add(css);
					cssStartCharIndex += css.Str.Length;
					cssIndex++;
				}
				cssArray = new AConsoleDisplayPart[buttonCssList.Count];
				buttonCssList.CopyTo(cssArray);
				if (bp.CanSelect)
					ret[i] = new ConsoleButtonString(parent, cssArray, bp.Input);
				else
					ret[i] = new ConsoleButtonString(parent, cssArray);
				buttonCssList.Clear();
			}
			return ret;

		}


		//stringListにPointX、Widthを追加
		private static void setWidthToButtonList(List<ConsoleButtonString> buttonList, StringMeasure stringMeasure, bool nobr)
		{
			int pointX = 0;
			//int count = buttonList.Count;
			//1.824 修正。サブピクセルの初期値を0から0.5fにすることで端数処理吸収
			float subPixel = 0.5f;
			for (int i = 0; i < buttonList.Count; i++)
			{
				ConsoleButtonString button = buttonList[i];
				if (button == null)
				{//改行フラグ
					pointX = 0;
					continue;
				}
				button.CalcWidth(stringMeasure, subPixel);
				button.CalcPointX(pointX);
				pointX = button.PointX + button.Width;
				//これは何がしたいんだろう…
				if (button.PointXisLocked)
					subPixel = 0;
				//pointX += button.Width;
				subPixel = button.XsubPixel;
			}
			return;
			
			//1815 バグバグなのでコメントアウト Width測定の省略はいずれやりたい
			////1815 alignLeft, nobrを前提にした新方式
			////PointXの直接指定を可能にし、Width測定を一部省略
			//ConsoleStyledString lastCss = null;
			//for (int i = 0; i < buttonList.Count; i++)
			//{
			//    ConsoleButtonString button = buttonList[i];
			//    if (button == null)
			//    {//改行フラグ
			//        pointX = 0;
			//        lastCss = null;
			//        continue;
			//    }
			//    for (int j = 0; j < button.StrArray.Length; j++)
			//    {
			//        ConsoleStyledString css = button.StrArray[j];
			//        if (css.PointXisLocked)//位置固定フラグ
			//        {//位置固定なら前のcssのWidth測定を省略
			//            pointX = css.PointX;
			//            if (lastCss != null)
			//            {
			//                lastCss.Width = css.PointX - lastCss.PointX;
			//                if (lastCss.Width < 0)
			//                    lastCss.Width = 0;
			//            }
			//        }
			//        else
			//        {
			//            if (lastCss != null)
			//            {
			//                lastCss.SetWidth(stringMeasure);
			//                pointX += lastCss.Width;
			//            }
			//            css.PointX = pointX;
			//        }
			//    }
			//}
			////ConsoleButtonStringの位置・幅を決定（クリック可能域の決定のために必要）
			//for (int i = 0; i < buttonList.Count; i++)
			//{
			//    ConsoleButtonString button = buttonList[i];
			//    if (button == null || button.StrArray.Length == 0)
			//        continue;
			//    button.PointX = button.StrArray[0].PointX;
			//    lastCss = button.StrArray[button.StrArray.Length - 1];
			//    if (lastCss.Width >= 0)
			//        button.Width = lastCss.PointX - button.PointX + lastCss.Width;
			//    else if (i >= buttonList.Count - 1 || buttonList[i+1] == null || buttonList[i+1].StrArray.Length == 0)//行末
			//        button.Width = Config.WindowX;//右端のボタンについては右側全部をボタン領域にしてしまう
			//    else
			//        button.Width = buttonList[i+1].StrArray[0].PointX - button.PointX;
			//    if (button.Width < 0)
			//        button.Width = 0;//pos指定次第ではクリック不可能なボタンができてしまう。まあ仕方ない
			//}
		}

		private static int getDivideIndex(ConsoleButtonString button, StringMeasure sm)
		{
			AConsoleDisplayPart divCss = null;
			int pointX = button.PointX;
			int strLength = 0;
			int index = 0;

            int count = button.StrArray.Length;
            AConsoleDisplayPart css = null;
            for(var i=0; i<count; ++i)
			{
                css = button.StrArray[i];
				if (pointX + css.Width > Config.DrawableWidth)
				{
					if (index == 0 && !css.CanDivide)
						continue;
					divCss = css;
					break;
				}
				index++;
				strLength += css.Str.Length;
				pointX += css.Width;
			}
			if (divCss != null)
			{
				int cssDivIndex = getDivideIndex(divCss, sm);
				if (cssDivIndex > 0)
					strLength += cssDivIndex;
			}
			return strLength;
		}

		private static int getDivideIndex(AConsoleDisplayPart part, StringMeasure sm)
		{
			if (!part.CanDivide)
				return -1;
			ConsoleStyledString css = part as ConsoleStyledString;
			if (part == null)
				return -1;
			int widthLimit = Config.DrawableWidth - css.PointX;
			string str = css.Str;
			Font font = css.Font;
            int highLength = str.Length;//widthLimitを超える最低の文字index(文字数-1)。
			int lowLength = 0;//超えない最大の文字index。
			//int i = (int)(widthLimit / fontDisplaySize);//およその文字数を推定
			//if (i > str.Length - 1)//配列の外を参照しないように。
			//	i = str.Length - 1;
			int i = lowLength;//およその文字数を推定←やめた

			int point;
			string test = null;
			while ((highLength - lowLength) > 1)//差が一文字以下になるまで繰り返す。
			{
				test = str.Substring(0, i);
				point = sm.GetDisplayLength(test, font);
				if (point <= widthLimit)//サイズ内ならlowLengthを更新。文字数を増やす。
				{
					lowLength = i;
					i++;
				}
				else//サイズ外ならhighLengthを更新。文字数を減らす。
				{
					highLength = i;
					i--;
				}
			}
			return lowLength;
		}
		#endregion

	}
}
