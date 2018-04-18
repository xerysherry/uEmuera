using System;
using System.Collections.Generic;
using System.Text;
//using System.Drawing;
using uEmuera.Drawing;

namespace MinorShift.Emuera.GameView
{
	/// <summary>
	/// 装飾付文字列(ConsoleStyledString)用のスタイル構造体
	/// </summary>
	internal struct StringStyle
	{
		public StringStyle(Color color, FontStyle fontStyle, string fontname)
		{
			this.Color = color;
			this.ButtonColor = Config.FocusColor;
			this.ColorChanged = false;//こっちのパターンでは色変更を後で検知
			this.FontStyle = fontStyle;
			if (string.IsNullOrEmpty(fontname))
				Fontname = Config.FontName;
			else
				this.Fontname = fontname;
		}

		/// <summary>
		/// HTML用。ColorChangedを固定する。
		/// </summary>
		public StringStyle(Color color, bool colorChanged, Color buttonColor, FontStyle fontStyle, string fontname)
		{
			this.Color = color;
			this.ButtonColor = buttonColor;
			this.ColorChanged = colorChanged;
			this.FontStyle = fontStyle;
			if (string.IsNullOrEmpty(fontname))
				Fontname = Config.FontName;
			else
				this.Fontname = fontname;
		}

		public Color Color;
		public Color ButtonColor;
		public bool ColorChanged;
		public FontStyle FontStyle;
		public string Fontname;
		public override bool Equals(object obj)
		{
			if ((obj == null) || (!(obj is StringStyle)))
				return false;
			StringStyle ss = (StringStyle)obj;
			return ((this.Color == ss.Color) && (this.ButtonColor == ss.ButtonColor) && (this.ColorChanged == ss.ColorChanged) && (this.FontStyle == ss.FontStyle) && (this.Fontname.Equals(ss.Fontname,  Config.SCIgnoreCase)));
		}
		public override int GetHashCode()
		{
			return Color.GetHashCode() ^ ButtonColor.GetHashCode() ^ ColorChanged.GetHashCode() ^ FontStyle.GetHashCode() ^ Fontname.GetHashCode();
		}
		public static bool operator ==(StringStyle x, StringStyle y)
		{
			return ((x.Color == y.Color) && (x.ButtonColor == y.ButtonColor) && (x.ColorChanged == y.ColorChanged) && (x.FontStyle == y.FontStyle) && (x.Fontname.Equals(y.Fontname, Config.SCIgnoreCase)));
		}
		public static bool operator !=(StringStyle x, StringStyle y)
		{
			return !(x == y);
		}
	}
}
