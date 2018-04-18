using System;
using System.Collections.Generic;
using System.Text;
//using System.Drawing;
//using System.Windows.Forms;
using MinorShift._Library;
using MinorShift.Emuera;
using uEmuera.Drawing;
using uEmuera.Forms;

namespace MinorShift.Emuera.GameView
{
	/// <summary>
	/// 装飾付文字列。stringとStringStyleからなる。
	/// </summary>
	internal sealed class ConsoleStyledString : AConsoleColoredPart
	{
		private ConsoleStyledString() { }
		public ConsoleStyledString(string str, StringStyle style)
		{
            //if ((StaticConfig.TextDrawingMode != TextDrawingMode.GRAPHICS) && (str.IndexOf('\t') >= 0))
            //    str = str.Replace("\t", "");
			this.Str = str;
			this.StringStyle = style;
			Font = Config.GetFont(style.Fontname, style.FontStyle);
			if (Font == null)
			{
				Error = true;
				return;
			}
			Color = style.Color;
			ButtonColor = style.ButtonColor;
            colorChanged = style.ColorChanged;
			if (!colorChanged && Color != Config.ForeColor)
				colorChanged = true;
			PointX = -1;
			Width = -1;
		}
		public Font Font{ get; private set;}
		public StringStyle StringStyle{ get; private set;}
		public override bool CanDivide
		{
			get { return true; }
		}
		//単一のボタンフラグ
		//public bool IsButton { get; set; }
		//indexの文字数の前方文字列とindex以降の後方文字列に分割
		public ConsoleStyledString DivideAt(int index, StringMeasure sm)
		{
			//if ((index <= 0)||(index > Str.Length)||this.Error)
			//	return null;
			ConsoleStyledString ret = DivideAt(index);
			if (ret == null)
				return null;
			this.SetWidth(sm, XsubPixel);
			ret.SetWidth(sm, XsubPixel);
			return ret;
		}
		public ConsoleStyledString DivideAt(int index)
		{
			if ((index <= 0) || (index > Str.Length) || this.Error)
				return null;
			string str = Str.Substring(index, Str.Length - index);
			this.Str = Str.Substring(0, index);
			ConsoleStyledString ret = new ConsoleStyledString();
			ret.Font = this.Font;
			ret.Str = str;
			ret.Color = this.Color;
			ret.ButtonColor = this.ButtonColor;
			ret.colorChanged = this.colorChanged;
			ret.StringStyle = this.StringStyle;
			ret.XsubPixel = this.XsubPixel;
			return ret;
		}

		public override void SetWidth(StringMeasure sm, float subPixel)
		{
			if (this.Error)
			{
				Width = 0;
				return;
			}
			Width = sm.GetDisplayLength(Str, Font);
			XsubPixel = subPixel;
		}

		public override void DrawTo(Graphics graph, int pointY, bool isSelecting, bool isBackLog, TextDrawingMode mode)
		{
			if (this.Error)
				return;
			Color color = this.Color;
			if(isSelecting)
				color = this.ButtonColor;
			else if (isBackLog && !colorChanged)
                color = Config.LogColor;
				
			if (mode == TextDrawingMode.GRAPHICS)
				graph.DrawString(Str, Font, new SolidBrush(color), new Point(PointX, pointY));
			else
				TextRenderer.DrawText(graph, Str, Font, new Point(PointX, pointY), color, TextFormatFlags.NoPrefix);

		}

		public override void GDIDrawTo(int pointY, bool isSelecting, bool isBackLog)
		{
			if (this.Error)
				return;
			Color color = this.Color;
			if(isSelecting)
                color = this.ButtonColor;
			else if (isBackLog && !colorChanged)
                color = Config.LogColor;
			GDI.TabbedTextOutFull(Font,color,Str, PointX, pointY);
			//GDI.SetFont(Font);
			//GDI.SetTextColor(color);
			//GDI.TabbedTextOut(Str, PointX, pointY);
		}

	}
}
