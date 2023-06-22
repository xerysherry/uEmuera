using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MinorShift.Emuera.Sub;
//using System.Drawing;
using MinorShift.Emuera.GameData.Expression;
using uEmuera.Drawing;

namespace MinorShift.Emuera.GameView
{
	//TODO:1810～
	/* Emuera用Htmlもどきが実装すべき要素
	 * (できるだけhtmlとConsoleDisplayLineとの1:1対応を目指す。<b>と<strong>とか同じ結果になるタグを重複して実装しない)
	 * <p align=""></p> ALIGNMENT命令相当・行頭から行末のみ・行末省略可
	 * <nobr></nobr> PRINTSINGLE相当・行頭から行末のみ・行末省略可
	 * <b><i><u><s> フォント各種・オーバーラップ問題は保留
	 * <button value=""></button> ボタン化・htmlでは明示しない限りボタン化しない
	 * <font face="" color="" bcolor=""></font> フォント指定 色指定 ボタン選択中色指定
	 * 追加<!-- --> コメント
	 * <nonbutton title='～～'> 
	 * <img src='～～' srcb='～～'> 
	 * <shape type='rect' param='0,0,0,0'> 
	 * エスケープ
	 * &amp; &gt; &lt; &quot; &apos; &<>"' ERBにおける利便性を考えると属性値の指定には"よりも'を使いたい。HTML4.1にはないがaposを入れておく
	 * &#nn; &#xnn; Unicode参照 #xFFFF以上は却下
	 */
	/* このクラスがサポートすべきもの
	 * html から ConsoleDisplayLine[] //主に表示用
	 * ConsoleDisplayLine[] から html //現在の表示をstr化して保存？
	 * html から ConsoleDisplayLine[] を経て html //表示を行わずに改行が入る位置のチェックができるかも
	 * html から PlainText(非エスケープ)//
	 * Text から エスケープ済Text
	 */
	/// <summary>
	/// EmueraConsoleのなんちゃってHtml解決用クラス
	/// </summary>
	internal static class HtmlManager
	{
		static HtmlManager()
		{
			repDic.Add('&', "&amp;");
			repDic.Add('>', "&gt;");
			repDic.Add('<', "&lt;");
			repDic.Add('\"', "&quot;");
			repDic.Add('\'', "&apos;");
		}
		static readonly char[] rep = new char[] { '&', '>', '<', '\"', '\'' };
		static readonly Dictionary<char, string> repDic = new Dictionary<char, string>();
		private sealed class HtmlAnalzeStateFontTag
		{
			public int Color = -1;
			public int BColor = -1;
			public string FontName = null;
			//public int PointX = 0;
			//public bool PointXisLocked = false;
		}

		private sealed class HtmlAnalzeStateButtonTag
		{
			public bool IsButton = true;
			public bool IsButtonTag = true;
			public Int64 ButtonValueInt = 0;
			public string ButtonValueStr = null;
			public string ButtonTitle = null;
			public bool ButtonIsInteger = false;
			public int PointX = 0;
			public bool PointXisLocked = false;
		}

		private sealed class HtmlAnalzeState
		{
			public bool LineHead = true;//行頭フラグ。一度もテキストが出てきてない状態
			public FontStyle FontStyle = FontStyle.Regular;
			public List<HtmlAnalzeStateFontTag> FonttagList = new List<HtmlAnalzeStateFontTag>();
			public bool FlagNobr = false;//falseの時に</nobr>するとエラー
			public bool FlagP = false;//falseの時に</p>するとエラー
			public bool FlagNobrClosed = false;//trueの時に</nobr>するとエラー
			public bool FlagPClosed = false;//trueの時に</p>するとエラー
			public DisplayLineAlignment Alignment = DisplayLineAlignment.LEFT;

			/// <summary>
			/// 今まで追加された文字列についてのボタンタグ情報
			/// </summary>
			public HtmlAnalzeStateButtonTag LastButtonTag = null;
			/// <summary>
			/// 最新のボタンタグ情報
			/// </summary>
			public HtmlAnalzeStateButtonTag CurrentButtonTag = null;

			public bool FlagBr = false;//<br>による強制改行の予約
			public bool FlagButton = false;//<button></button>によるボタン化の予約

			public StringStyle GetSS()
			{
				Color c = Config.ForeColor;
				Color b = Config.FocusColor;
				string fontname = null;
				bool colorChanged = false;
				if (FonttagList.Count > 0)
				{
					HtmlAnalzeStateFontTag font = FonttagList[FonttagList.Count - 1];
					fontname = font.FontName;
					if (font.Color >= 0)
					{
						colorChanged = true;
						c = Color.FromArgb(font.Color >> 16, (font.Color >> 8) & 0xFF, font.Color & 0xFF);
					}
					if (font.BColor >= 0)
					{
						b = Color.FromArgb(font.BColor >> 16, (font.BColor >> 8) & 0xFF, font.BColor & 0xFF);
					}
				}
				return new StringStyle(c, colorChanged, b, FontStyle, fontname);
			}
		}

		/// <summary>
		/// 表示行からhtmlへの変換
		/// </summary>
		/// <param name="lines"></param>
		/// <returns></returns>
		public static string DisplayLine2Html(ConsoleDisplayLine[] lines, bool needPandN)
		{
			if (lines == null || lines.Length == 0)
				return "";
			StringBuilder b = new StringBuilder();
			if (needPandN)
			{
				switch (lines[0].Align)
				{
					case DisplayLineAlignment.LEFT:
						b.Append("<p align='left'>");
						break;
					case DisplayLineAlignment.CENTER:
						b.Append("<p align='center'>");
						break;
					case DisplayLineAlignment.RIGHT:
						b.Append("<p align='right'>");
						break;
				}
				b.Append("<nobr>");
			}
			for (int dispCounter = 0; dispCounter < lines.Length; dispCounter++)
			{
				if (dispCounter != 0)
					b.Append("<br>");
				ConsoleButtonString[] buttons = lines[dispCounter].Buttons;
				for (int buttonCounter = 0; buttonCounter < buttons.Length; buttonCounter++)
				{
					string titleValue = null;
					if (!string.IsNullOrEmpty(buttons[buttonCounter].Title))
						titleValue = Escape(buttons[buttonCounter].Title);
					bool hasTag = buttons[buttonCounter].IsButton || titleValue != null
						|| buttons[buttonCounter].PointXisLocked;
					if (hasTag)
					{
						if (buttons[buttonCounter].IsButton)
						{
							string attrValue = Escape(buttons[buttonCounter].Inputs);
							b.Append("<button value='");
							b.Append(attrValue);
							b.Append("'");
						}
						else
						{
							b.Append("<nonbutton");
						}
						if (titleValue != null)
						{
							b.Append(" title='");
							b.Append(titleValue);
							b.Append("'");
						}
						if (buttons[buttonCounter].PointXisLocked)
						{
							b.Append(" pos='");
							b.Append(buttons[buttonCounter].RelativePointX.ToString());
							b.Append("'");
						}
						b.Append(">");
					}
					AConsoleDisplayPart[] parts = buttons[buttonCounter].StrArray;
					for (int cssCounter = 0; cssCounter < parts.Length; cssCounter++)
					{
						if (parts[cssCounter] is ConsoleStyledString)
						{
							ConsoleStyledString css = parts[cssCounter] as ConsoleStyledString;
							b.Append(getStringStyleStartingTag(css.StringStyle));
							b.Append(Escape(css.Str));
							b.Append(getClosingStyleStartingTag(css.StringStyle));
						}
						else if (parts[cssCounter] is ConsoleImagePart)
						{
							b.Append(parts[cssCounter].AltText);
							//ConsoleImagePart img = (ConsoleImagePart)parts[cssCounter];
							//b.Append("<img src='");
							//b.Append(Escape(img.ResourceName));
							//if(img.ButtonResourceName != null)
							//{
							//	b.Append("' srcb='");
							//	b.Append(Escape(img.ButtonResourceName));
							//}
							//b.Append("'>");
						}
						else if (parts[cssCounter] is ConsoleShapePart)
						{
							b.Append(parts[cssCounter].AltText);
						}

					}
					if (hasTag)
					{
						if (buttons[buttonCounter].IsButton)
							b.Append("</button>");
						else
							b.Append("</nonbutton>");
					}

				}
			}
			if(needPandN)
			{
				b.Append("</nobr>");
				b.Append("</p>");
			}
			return b.ToString();
		}

		public static string[] HtmlTagSplit(string str)
		{
			List<string> strList = new List<string>();
			StringStream st = new StringStream(str);
			int found = -1;
			while (!st.EOS)
			{
				found = st.Find('<');
				if (found < 0)
				{
					strList.Add(st.Substring());
					break;
				}
				else if (found > 0)
				{
					strList.Add(st.Substring(st.CurrentPosition, found));
					st.CurrentPosition += found;
				}
				found = st.Find('>');
				if(found < 0)
					return null;
				found++;
				strList.Add(st.Substring(st.CurrentPosition, found));
				st.CurrentPosition += found;
			}
			string[] ret = new string[strList.Count];
			strList.CopyTo(ret);
			return ret;
		}
		
		/// <summary>
		/// htmlから表示行の作成
		/// </summary>
		/// <param name="str">htmlテキスト</param>
		/// <param name="sm"></param>
		/// <param name="console">実際の表示に使わないならnullにする</param>
		/// <returns></returns>
		public static ConsoleDisplayLine[] Html2DisplayLine(string str, StringMeasure sm, EmueraConsole console)
		{
			List<AConsoleDisplayPart> cssList = new List<AConsoleDisplayPart>();
			List<ConsoleButtonString> buttonList = new List<ConsoleButtonString>();
			StringStream st = new StringStream(str);
			int found;
			bool hasComment = str.IndexOf("<!--") >= 0;
			bool hasReturn = str.IndexOf('\n') >= 0;
			HtmlAnalzeState state = new HtmlAnalzeState();
			while (!st.EOS)
			{
				found = st.Find('<');
				if (hasReturn)
				{
					int rFound = st.Find('\n');
					if (rFound >= 0 && (found > rFound || found < 0))
						found = rFound;
				}
				if (found < 0)
				{
					string txt = Unescape(st.Substring());
					cssList.Add(new ConsoleStyledString(txt, state.GetSS()));
					if (state.FlagPClosed)
						throw new CodeEE("</p>の後にテキストがあります");
					if (state.FlagNobrClosed)
						throw new CodeEE("</nobr>の後にテキストがあります");
					break;
				}
				else if (found > 0)
				{
					string txt = Unescape(st.Substring(st.CurrentPosition, found));
					cssList.Add(new ConsoleStyledString(txt, state.GetSS()));
					state.LineHead = false;
					st.CurrentPosition += found;
				}
				//コメントタグのみ特別扱い
				if (hasComment && st.CurrentEqualTo("<!--"))
				{
					st.CurrentPosition += 4;
					found = st.Find("-->");
					if (found < 0)
						throw new CodeEE("コメンdト終了タグ\"-->\"がみつかりません");
					st.CurrentPosition += found + 3;
					continue;
				}
				if (hasReturn && st.Current == '\n')//テキスト中の\nは<br>として扱う
				{
					state.FlagBr = true;
					st.ShiftNext();
				}
				else//タグ解析
				{
					st.ShiftNext();
					AConsoleDisplayPart part = tagAnalyze(state, st);
					if (st.Current != '>')
						throw new CodeEE("タグ終端'>'が見つかりません");
					if (part != null)
						cssList.Add(part);
					st.ShiftNext();
				}

				if (state.FlagBr)
				{
					state.LastButtonTag = state.CurrentButtonTag;
					if (cssList.Count > 0)
						buttonList.Add(cssToButton(cssList, state, console));
					buttonList.Add(null);
				}
				if (state.FlagButton && cssList.Count > 0)
				{
					buttonList.Add(cssToButton(cssList, state, console));
				}
				state.FlagBr = false;
				state.FlagButton = false;
				state.LastButtonTag = state.CurrentButtonTag;
			}
			//</nobr></p>は省略許可
			if (state.CurrentButtonTag != null || state.FontStyle != FontStyle.Regular || state.FonttagList.Count > 0)
				throw new CodeEE("閉じられていないタグがあります");
			if (cssList.Count > 0)
				buttonList.Add(cssToButton(cssList, state, console));

			foreach(ConsoleButtonString button in buttonList)
			{
				if (button != null && button.PointXisLocked)
				{
					if (!state.FlagNobr)
						throw new CodeEE("<nobr>が設定されていない行ではpos属性は使用できません");
					if (state.Alignment != DisplayLineAlignment.LEFT)
						throw new CodeEE("alignがleftでない行ではpos属性は使用できません");
					break;
				}
			}
			ConsoleDisplayLine[] ret = PrintStringBuffer.ButtonsToDisplayLines(buttonList, sm, state.FlagNobr, false);

			foreach (ConsoleDisplayLine dl in ret)
			{
				dl.SetAlignment(state.Alignment);
			}
			return ret;
		}

		public static string Html2PlainText(string str)
		{
			string ret = Regex.Replace(str, "\\<[^<]*\\>", "");
			return Unescape(ret);
		}

		public static string Escape(string str)
		{
			//Net4.5では便利なクラスがあるらしい
			//return System.Web.HttpUtility.HtmlEncode(str);

			int index = 0;
			int found = 0;
			StringBuilder b = new StringBuilder();
			while (index < str.Length)
			{
				found = str.IndexOfAny(rep, index);
				if (found < 0)//見つからなければ以降を追加して終了
				{
					b.Append(str.Substring(index));
					break;
				}
				if (found > index)//間に非エスケープ文字があるなら追加しておく
					b.Append(str.Substring(index, found - index));
				string repnew = repDic[str[found]];
				b.Append(repnew);
				index = found + 1;
			}
			return b.ToString();
		}

		public static string Unescape(string str)
		{
			int index = 0;
			int found = str.IndexOf('&', index);
			if (found < 0)
				return str;
			StringBuilder b = new StringBuilder();
			// &～; をひたすら置換するだけ
			while (index < str.Length)
			{
				found = str.IndexOf('&', index);
				if (found < 0)//見つからなければ以降を追加して終了
				{
					b.Append(str.Substring(index));
					break;
				}
				if (found > index)//間に非エスケープ文字があるなら追加しておく
					b.Append(str.Substring(index, found - index));
				index = found;
				found = str.IndexOf(';', index);
				if (found <= index + 1)
				{
					if (found < 0)
						throw new CodeEE("'&'に対応する';'がみつかりません");
					throw new CodeEE("'&'と';'が連続しています");
				}
				string escWordRow = str.Substring(index + 1, found - index - 1);
				index = found + 1;
				string escWord = escWordRow.ToLower();
				int unicode = 0;
				switch (escWord)
				{
					case "nbsp": b.Append(" "); break;
					case "amp": b.Append("&"); break;
					case "gt": b.Append(">"); break;
					case "lt": b.Append("<"); break;
					case "quot": b.Append("\""); break;
					case "apos": b.Append("\'"); break;
					default:
						{
							int iBbase = 10;
							if (escWord[0] != '#')
								throw new CodeEE("\"&" + escWordRow + ";\"は適切な文字参照ではありません");
							if (escWord.Length > 1 && escWord[1] == 'x')
							{
								iBbase = 16;
								escWord = escWord.Substring(2);
							}
							else
								escWord = escWord.Substring(1);
							try
							{
								unicode = Convert.ToInt32(escWord, iBbase);
							}
							catch
							{

								throw new CodeEE("\"&" + escWordRow + ";\"は適切な文字参照ではありません");
							}

							if (unicode < 0 || unicode > 0xFFFF)
								throw new CodeEE("\"&" + escWordRow + ";\"はUnicodeの範囲外です(サロゲートペアは使えません)");
							b.Append((char)unicode);
							break;
						}
				}
			}
			return b.ToString();
		}

		/// <summary>
		/// ここまでのcssをボタン化。発生原因はbrタグ、行末、ボタンタグ
		/// </summary>
		/// <param name="cssList"></param>
		/// <param name="isbutton"></param>
		/// <param name="state"></param>
		/// <param name="console"></param>
		/// <returns></returns>
		private static ConsoleButtonString cssToButton(List<AConsoleDisplayPart> cssList, HtmlAnalzeState state, EmueraConsole console)
		{
			AConsoleDisplayPart[] css = new AConsoleDisplayPart[cssList.Count];
			cssList.CopyTo(css);
			cssList.Clear();
			ConsoleButtonString ret = null;
			if (state.LastButtonTag != null && state.LastButtonTag.IsButton)
			{
				if (state.LastButtonTag.ButtonIsInteger)
					ret = new ConsoleButtonString(console, css, state.LastButtonTag.ButtonValueInt, state.LastButtonTag.ButtonValueStr);
				else
					ret = new ConsoleButtonString(console, css, state.LastButtonTag.ButtonValueStr);
			}
			else
			{
				ret = new ConsoleButtonString(console, css);
				ret.Title = null;
			}
			if (state.LastButtonTag != null)
			{
				ret.Title = state.LastButtonTag.ButtonTitle;
				if(state.LastButtonTag.PointXisLocked)
				{
					ret.LockPointX(state.LastButtonTag.PointX);
				}
			}
			return ret;
		}

		public static string GetColorToString(Color color)
		{
			StringBuilder b = new StringBuilder();
			b.Append("#");
			int colorValue = color.R * 0x10000 + color.G * 0x100 + color.B;
			b.Append(colorValue.ToString("X6"));
			return b.ToString();
		}
		private static string getStringStyleStartingTag(StringStyle style)
		{
			bool fontChanged = !((style.Fontname == null || style.Fontname == Config.FontName)&& !style.ColorChanged && (style.ButtonColor == Config.FocusColor));
			if (!fontChanged && style.FontStyle == FontStyle.Regular)
				return "";
			StringBuilder b = new StringBuilder();
			if (fontChanged)
			{
				b.Append("<font");
				if (style.Fontname != null && style.Fontname != Config.FontName)
				{
					b.Append(" face='");
					b.Append(HtmlManager.Escape(style.Fontname));
					b.Append("'");
				}
				if (style.ColorChanged)
				{
					b.Append(" color='#");
					int colorValue = style.Color.R * 0x10000 + style.Color.G * 0x100 + style.Color.B;
					b.Append(colorValue.ToString("X6"));
					b.Append("'");
				}
				if (style.ButtonColor != Config.FocusColor)
				{
					b.Append(" bcolor='#");
					int colorValue = style.ButtonColor.R * 0x10000 + style.ButtonColor.G * 0x100 + style.ButtonColor.B;
					b.Append(colorValue.ToString("X6"));
					b.Append("'");
				}
				b.Append(">");
			}
			if (style.FontStyle != FontStyle.Regular)
			{
				if ((style.FontStyle & FontStyle.Strikeout) != FontStyle.Regular)
					b.Append("<s>");
				if ((style.FontStyle & FontStyle.Underline) != FontStyle.Regular)
					b.Append("<u>");
				if ((style.FontStyle & FontStyle.Italic) != FontStyle.Regular)
					b.Append("<i>");
				if ((style.FontStyle & FontStyle.Bold) != FontStyle.Regular)
					b.Append("<b>");
			}

			return b.ToString();
		}

		private static string getClosingStyleStartingTag(StringStyle style)
		{
			bool fontChanged = !((style.Fontname == null || style.Fontname == Config.FontName) && !style.ColorChanged && (style.ButtonColor == Config.FocusColor));
			if (!fontChanged && style.FontStyle == FontStyle.Regular)
				return "";
			StringBuilder b = new StringBuilder();
			if (style.FontStyle != FontStyle.Regular)
			{
				if ((style.FontStyle & FontStyle.Bold) != FontStyle.Regular)
					b.Append("</b>");
				if ((style.FontStyle & FontStyle.Italic) != FontStyle.Regular)
					b.Append("</i>");
				if ((style.FontStyle & FontStyle.Underline) != FontStyle.Regular)
					b.Append("</u>");
				if ((style.FontStyle & FontStyle.Strikeout) != FontStyle.Regular)
					b.Append("</s>");
			}
			if (fontChanged)
				b.Append("</font>");
			return b.ToString();
		}

		private static AConsoleDisplayPart tagAnalyze(HtmlAnalzeState state, StringStream st)
		{
			bool endTag = (st.Current == '/');
			string tag;
			if (endTag)
			{
				st.ShiftNext();
				int found = st.Find('>');
				if (found < 0)
				{
					st.CurrentPosition = st.RowString.Length;
					return null;//戻り先でエラーを出す
				}
				tag = st.Substring(st.CurrentPosition, found).Trim();
				st.CurrentPosition += found;
				FontStyle endStyle = FontStyle.Strikeout;
				switch (tag.ToLower())
				{
					case "b": endStyle = FontStyle.Bold; goto case "s";
					case "i": endStyle = FontStyle.Italic; goto case "s";
					case "u": endStyle = FontStyle.Underline; goto case "s";
					case "s":
						if ((state.FontStyle & endStyle) == FontStyle.Regular)
							throw new CodeEE("</" + tag + ">の前に<" + tag + ">がありません");
						state.FontStyle ^= endStyle;
						return null;
					case "p":
						if ((!state.FlagP) || (state.FlagPClosed))
							throw new CodeEE("</p>の前に<p>がありません");
						state.FlagPClosed = true;
						return null;
					case "nobr":
						if ((!state.FlagNobr) || (state.FlagNobrClosed))
							throw new CodeEE("</nobr>の前に<nobr>がありません");
						state.FlagNobrClosed = true;
						return null;
					case "font":
						if (state.FonttagList.Count == 0)
							throw new CodeEE("</font>の前に<font>がありません");
						state.FonttagList.RemoveAt(state.FonttagList.Count - 1);
						return null;
					case "button":
						if (state.CurrentButtonTag == null || !state.CurrentButtonTag.IsButtonTag)
							throw new CodeEE("</button>の前に<button>がありません");
						state.CurrentButtonTag = null;
						state.FlagButton = true;
						return null;
					case "nonbutton":
						if (state.CurrentButtonTag == null || state.CurrentButtonTag.IsButtonTag)
							throw new CodeEE("</nonbutton>の前に<nonbutton>がありません");
						state.CurrentButtonTag = null;
						state.FlagButton = true;
						return null;
					default:
						throw new CodeEE("終了タグ</"+tag+">は解釈できません");
				}
				//goto error;
			}
			//以降は開始タグ

			bool tempUseMacro = LexicalAnalyzer.UseMacro;
			WordCollection wc = null;
			try
			{
				LexicalAnalyzer.UseMacro = false;//一時的にマクロ展開をやめる
				tag = LexicalAnalyzer.ReadSingleIdentifier(st);
				LexicalAnalyzer.SkipWhiteSpace(st);
				if (st.Current != '>')
					wc = LexicalAnalyzer.Analyse(st, LexEndWith.GreaterThan, LexAnalyzeFlag.AllowAssignment | LexAnalyzeFlag.AllowSingleQuotationStr);
			}
			finally
			{
				LexicalAnalyzer.UseMacro = tempUseMacro;
			}
			if (string.IsNullOrEmpty(tag))
				goto error;
			IdentifierWord word;
			FontStyle newStyle = FontStyle.Strikeout;
            switch (tag.ToLower())
			{
				case "b": newStyle = FontStyle.Bold; goto case "s";
				case "i": newStyle = FontStyle.Italic; goto case "s";
				case "u": newStyle = FontStyle.Underline; goto case "s";
				case "s":
					if (wc != null)
						throw new CodeEE("<" + tag + ">タグにに属性が設定されています");
					if ((state.FontStyle & newStyle) != FontStyle.Regular)
						throw new CodeEE("<" + tag + ">が二重に使われています");
					state.FontStyle |= newStyle;
						return null;
				case "br":
					if (wc != null)
						throw new CodeEE("<" + tag + ">タグにに属性が設定されています");
					state.FlagBr = true;
						return null;
				case "nobr":
					if (wc != null)
						throw new CodeEE("<" + tag + ">タグに属性が設定されています");
					if (!state.LineHead)
						throw new CodeEE("<nobr>が行頭以外で使われています");
					if (state.FlagNobr)
						throw new CodeEE("<nobr>が2度以上使われています");
					state.FlagNobr = true;
						return null;
				case "p":
					{
						if (wc == null)
							throw new CodeEE("<" + tag + ">タグに属性が設定されていません");
						if (!state.LineHead)
							throw new CodeEE("<p>が行頭以外で使われています");
						if (state.FlagNobr)
							throw new CodeEE("<p>が2度以上使われています");
						word = wc.Current as IdentifierWord;
						wc.ShiftNext();
						OperatorWord op = wc.Current as OperatorWord;
						wc.ShiftNext();
						LiteralStringWord attr = wc.Current as LiteralStringWord;
						wc.ShiftNext();
						if (!wc.EOL || word == null || op == null || op.Code != OperatorCode.Assignment || attr == null)
							goto error;
						if (!word.Code.Equals("align", StringComparison.OrdinalIgnoreCase))
							throw new CodeEE("<p>タグの属性名" + word.Code + "は解釈できません");
						string attrValue = Unescape(attr.Str);
						switch (attrValue.ToLower())
						{
							case "left":
								state.Alignment = DisplayLineAlignment.LEFT;
								break;
							case "center":
								state.Alignment = DisplayLineAlignment.CENTER;
								break;
							case "right":
								state.Alignment = DisplayLineAlignment.RIGHT;
								break;
							default:
								throw new CodeEE("属性値" + attr.Str + "は解釈できません");
						}
						state.FlagP = true;
						return null;
					}
				case "img":
					{
						if (wc == null)
							throw new CodeEE("<" + tag + ">タグに属性が設定されていません");
						string attrValue = null;
						string src = null;
						string srcb = null;
						MixedNum height = new MixedNum(); ;
						MixedNum width = new MixedNum(); ;
						MixedNum ypos = new MixedNum(); ;
						while (wc != null && !wc.EOL)
						{
							word = wc.Current as IdentifierWord;
							wc.ShiftNext();
							OperatorWord op = wc.Current as OperatorWord;
							wc.ShiftNext();
							LiteralStringWord attr = wc.Current as LiteralStringWord;
							wc.ShiftNext();
							if (word == null || op == null || op.Code != OperatorCode.Assignment || attr == null)
								goto error;
							attrValue = Unescape(attr.Str);
							if (word.Code.Equals("src", StringComparison.OrdinalIgnoreCase))
							{
								if (src != null)
									throw new CodeEE("<" + tag + ">タグに" + word.Code + "属性が2度以上指定されています");
								src = attrValue;
							}
							else if (word.Code.Equals("srcb", StringComparison.OrdinalIgnoreCase))
							{
								if (srcb != null)
									throw new CodeEE("<" + tag + ">タグに" + word.Code + "属性が2度以上指定されています");
								srcb = attrValue;
							}
							else if (word.Code.Equals("height", StringComparison.OrdinalIgnoreCase))
							{
								if (height.num != 0)
									throw new CodeEE("<" + tag + ">タグに" + word.Code + "属性が2度以上指定されています");
								if (attrValue.EndsWith("px", StringComparison.OrdinalIgnoreCase))
								{
									height.isPx = true;
									attrValue = attrValue.Substring(0, attrValue.Length - 2);
								}
								if (!int.TryParse(attrValue, out height.num))
									throw new CodeEE("<" + tag + ">タグのheight属性の属性値が数値として解釈できません");
							}
							else if (word.Code.Equals("width", StringComparison.OrdinalIgnoreCase))
							{
								if (width.num != 0)
									throw new CodeEE("<" + tag + ">タグに" + word.Code + "属性が2度以上指定されています");
								if (attrValue.EndsWith("px", StringComparison.OrdinalIgnoreCase))
								{
									width.isPx = true;
									attrValue = attrValue.Substring(0, attrValue.Length - 2);
								}
								if (!int.TryParse(attrValue, out width.num))
									throw new CodeEE("<" + tag + ">タグのwidth属性の属性値が数値として解釈できません");
							}
							else if (word.Code.Equals("ypos", StringComparison.OrdinalIgnoreCase))
							{
								if (ypos.num != 0)
									throw new CodeEE("<" + tag + ">タグに" + word.Code + "属性が2度以上指定されています");
								if (attrValue.EndsWith("px", StringComparison.OrdinalIgnoreCase))
								{
									ypos.isPx = true;
									attrValue = attrValue.Substring(0, attrValue.Length - 2);
								}
								if (!int.TryParse(attrValue, out ypos.num))
									throw new CodeEE("<" + tag + ">タグのypos属性の属性値が数値として解釈できません");
							}
							else
								throw new CodeEE("<" + tag + ">タグの属性名" + word.Code + "は解釈できません");
						}
						if (src == null)
							throw new CodeEE("<" + tag + ">タグにsrc属性が設定されていません");
						return new ConsoleImagePart(src, srcb, height, width, ypos);
					}

				case "shape":
					{
						if (wc == null)
							throw new CodeEE("<" + tag + ">タグに属性が設定されていません");
						int[] param = null;
						string type = null;
						int color = -1;
						int bcolor = -1;
						while (!wc.EOL)
						{
							word = wc.Current as IdentifierWord;
							wc.ShiftNext();
							OperatorWord op = wc.Current as OperatorWord;
							wc.ShiftNext();
							LiteralStringWord attr = wc.Current as LiteralStringWord;
							wc.ShiftNext();
							if (word == null || op == null || op.Code != OperatorCode.Assignment || attr == null)
								goto error;
							string attrValue = Unescape(attr.Str);
							switch (word.Code.ToLower())
							{
								case "color":
									if (color >= 0)
										throw new CodeEE("<" + tag + ">タグに" + word.Code + "属性が2度以上指定されています");
									color = stringToColorInt32(attrValue);
									break;
								case "bcolor":
									if (bcolor >= 0)
										throw new CodeEE("<" + tag + ">タグに" + word.Code + "属性が2度以上指定されています");
									bcolor = stringToColorInt32(attrValue);
									break;
								case "type":
									if (type != null)
										throw new CodeEE("<" + tag + ">タグに" + word.Code + "属性が2度以上指定されています");
									type = attrValue;
									break;
								case "param":
									if (param != null)
										throw new CodeEE("<" + tag + ">タグに" + word.Code + "属性が2度以上指定されています");
									{
										string[] tokens = attrValue.Split(',');
										param = new int[tokens.Length];
										for (int i = 0; i < tokens.Length; i++)
										{
											if (!int.TryParse(tokens[i], out param[i]))
												throw new CodeEE("<" + tag + ">タグの" + word.Code + "属性の属性値が数値として解釈できません");
										}
										break;
									}
								default:
									throw new CodeEE("<" + tag + ">タグの属性名" + word.Code + "は解釈できません");
							}
						}
						if (param == null)
							throw new CodeEE("<" + tag + ">タグにparam属性が設定されていません");
						if (type == null)
							throw new CodeEE("<" + tag + ">タグにtype属性が設定されていません");
						Color c = Config.ForeColor;
						Color b = Config.FocusColor;
						if (color >= 0)
						{
							c = Color.FromArgb(color >> 16, (color >> 8) & 0xFF, color & 0xFF);
						}
						if (bcolor >= 0)
						{
							b = Color.FromArgb(bcolor >> 16, (bcolor >> 8) & 0xFF, bcolor & 0xFF);
						}
						return ConsoleShapePart.CreateShape(type, param, c, b, color >= 0);
					}
				case "button":
				case "nonbutton":
					{
						if (state.CurrentButtonTag != null)
							throw new CodeEE("<button>又は<nonbutton>が入れ子にされています");
						HtmlAnalzeStateButtonTag buttonTag = new HtmlAnalzeStateButtonTag();
						bool isButton = tag.ToLower() == "button";
						string attrValue = null;
						string value = null;
						//if (wc == null)
						//	throw new CodeEE("<" + tag + ">タグに属性が設定されていません");
						while (wc != null && !wc.EOL)
						{
							word = wc.Current as IdentifierWord;
							wc.ShiftNext();
							OperatorWord op = wc.Current as OperatorWord;
							wc.ShiftNext();
							LiteralStringWord attr = wc.Current as LiteralStringWord;
							wc.ShiftNext();
							if (word == null || op == null || op.Code != OperatorCode.Assignment || attr == null)
								goto error;
							attrValue = Unescape(attr.Str);
							if (word.Code.Equals("value", StringComparison.OrdinalIgnoreCase))
							{
								if (!isButton)
									throw new CodeEE("<" + tag + ">タグにvalue属性が設定されています");
								if (value != null)
                                    throw new CodeEE("<" + tag + ">タグに" + word.Code + "属性が2度以上指定されています");
								value = attrValue;
							}
							else if (word.Code.Equals("title", StringComparison.OrdinalIgnoreCase))
							{
								if (buttonTag.ButtonTitle != null)
										throw new CodeEE("<" + tag + ">タグに" + word.Code + "属性が2度以上指定されています");
								buttonTag.ButtonTitle = attrValue;
							}
							else if (word.Code.Equals("pos", StringComparison.OrdinalIgnoreCase))
							{
                                //throw new NotImplCodeEE();
                                if (buttonTag.PointXisLocked)
                                    throw new CodeEE("<" + tag + ">タグに" + word.Code + "属性が2度以上指定されています");
                                if (!int.TryParse(attrValue, out int pos))
									throw new CodeEE("<" + tag + ">タグのpos属性の属性値が数値として解釈できません");
								buttonTag.PointX = pos;
								buttonTag.PointXisLocked = true;
							}
							else
								throw new CodeEE("<" + tag + ">タグの属性名" + word.Code + "は解釈できません");
						}
						if (isButton)
						{
                            //if (value == null)
                            //	throw new CodeEE("<" + tag + ">タグにvalue属性が設定されていません");
                            buttonTag.ButtonIsInteger = (Int64.TryParse(value, out long intValue));
                            buttonTag.ButtonValueInt = intValue;
							buttonTag.ButtonValueStr = value;
						}
						buttonTag.IsButton = value != null;
						buttonTag.IsButtonTag = isButton;
						state.CurrentButtonTag = buttonTag;
						state.FlagButton = true;
						return null;
					}
				case "font":
					{
						if (wc == null)
							throw new CodeEE("<" + tag + ">タグに属性が設定されていません");
						HtmlAnalzeStateFontTag font = new HtmlAnalzeStateFontTag();
						while (!wc.EOL)
						{
							word = wc.Current as IdentifierWord;
							wc.ShiftNext();
							OperatorWord op = wc.Current as OperatorWord;
							wc.ShiftNext();
							LiteralStringWord attr = wc.Current as LiteralStringWord;
							wc.ShiftNext();
							if (word == null || op == null || op.Code != OperatorCode.Assignment || attr == null)
								goto error;
							string attrValue = Unescape(attr.Str);
							switch (word.Code.ToLower())
							{
								case "color":
									if (font.Color >= 0)
										throw new CodeEE("<" + tag + ">タグに" + word.Code + "属性が2度以上指定されています");
									font.Color = stringToColorInt32(attrValue);
									break;
								case "bcolor":
									if (font.BColor >= 0)
										throw new CodeEE("<" + tag + ">タグに" + word.Code + "属性が2度以上指定されています");
									font.BColor = stringToColorInt32(attrValue);
									break;
								case "face":
									if (font.FontName != null)
										throw new CodeEE("<" + tag + ">タグに" + word.Code + "属性が2度以上指定されています");
									font.FontName = attrValue;
									break;
								//case "pos":
								//	{
								//		//throw new NotImplCodeEE();
								//		if (font.PointXisLocked)
								//			throw new CodeEE("<" + tag + ">タグに" + word.Code + "属性が2度以上指定されています");
								//		int pos = 0;
								//		if (!int.TryParse(attrValue, out pos))
								//			throw new CodeEE("<font>タグのpos属性の属性値が数値として解釈できません");
								//		font.PointX = pos;
								//		font.PointXisLocked = true;
								//		break;
								//	}
								default:
								throw new CodeEE("<" + tag + ">タグの属性名" + word.Code + "は解釈できません");
							}
						}
						//他のfontタグの内側であるなら未設定項目については外側のfontタグの設定を受け継ぐ(posは除く)
						if (state.FonttagList.Count > 0)
						{
							HtmlAnalzeStateFontTag oldFont = state.FonttagList[state.FonttagList.Count - 1];
							if (font.Color < 0)
								font.Color = oldFont.Color;
							if (font.BColor < 0)
								font.BColor = oldFont.BColor;
							if (font.FontName == null)
								font.FontName = oldFont.FontName;
						}
						state.FonttagList.Add(font);
						return null;
					}
				default:
					goto error;
			}


		error:
			throw new CodeEE("html文字列\"" + st.RowString + "\"のタグ解析中にエラーが発生しました");
		}

		private static int stringToColorInt32(string str)
		{
			if(str.Length == 0)
				throw new CodeEE("色を表す単語又は#RRGGBB値が必要です");
			int i = 0;
			if (str[0] == '#')
			{
				string colorvalue = str.Substring(1);
				try
				{
					i = Convert.ToInt32(colorvalue, 16);
					if (i < 0 || i > 0xFFFFFF)
						throw new CodeEE(colorvalue + "は適切な色指定の範囲外です");
				}
				catch
				{
					throw new CodeEE(colorvalue + "は数値として解釈できません");
				}
			}
			else
			{
				Color color = Color.FromName(str);
				if (color.A == 0)//色名として解釈失敗 エラー確定
				{
					if(str.Equals("transparent", StringComparison.OrdinalIgnoreCase))
						throw new CodeEE("無色透明(Transparent)は色として指定できません");
					try
					{
						i = Convert.ToInt32(str, 16);
					}
					catch//16進数でもない
					{
						throw new CodeEE("指定された色名\"" + str + "\"は無効な色名です");
					}
					//#RRGGBBを意図したのかもしれない
					throw new CodeEE("指定された色名\"" + str + "\"は無効な色名です(16進数で色を指定する場合には数値の前に#が必要です)");
				}
				i = color.R * 0x10000 + color.G * 0x100 + color.B;
			}
			return i;
		}

	}
}
