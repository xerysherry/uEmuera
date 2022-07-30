using System;
using System.Collections.Generic;
using System.Text;
//using System.Drawing;
using MinorShift.Emuera.Sub;
using uEmuera.Drawing;

namespace MinorShift.Emuera.GameView
{
	/// <summary>
	/// ボタン。1つ以上の装飾付文字列（ConsoleStyledString）からなる。
	/// </summary>
	internal sealed class ConsoleButtonString
	{
		public ConsoleButtonString(EmueraConsole console, AConsoleDisplayPart[] strs)
		{
			this.parent = console;
			this.strArray = strs;
			IsButton = false;
			PointX = -1;
			Width = -1;
            ErrPos = null;
		}
		public ConsoleButtonString(EmueraConsole console, AConsoleDisplayPart[] strs, Int64 input)
			:this(console, strs)
		{
			this.Input = input;
			Inputs = input.ToString();
			IsButton = true;
			IsInteger = true;
			if (console != null)
			{
				Generation = parent.NewButtonGeneration;
				console.UpdateGeneration();
			}
            ErrPos = null;
        }
		public ConsoleButtonString(EmueraConsole console, AConsoleDisplayPart[] strs, string inputs)
			: this(console, strs)
		{
			this.Inputs = inputs;
			IsButton = true;
			IsInteger = false;
			if (console != null)
			{
				Generation = parent.NewButtonGeneration;
				console.UpdateGeneration();
			}
            ErrPos = null;
        }

		public ConsoleButtonString(EmueraConsole console, AConsoleDisplayPart[] strs, Int64 input, string inputs)
			: this(console, strs)
		{
			this.Input = input;
			this.Inputs = inputs;
			IsButton = true;
			IsInteger = true;
			if (console != null)
			{
				Generation = parent.NewButtonGeneration;
				console.UpdateGeneration();
			}
            ErrPos = null;
        }
		public ConsoleButtonString(EmueraConsole console, AConsoleDisplayPart[] strs, string inputs, ScriptPosition pos)
            : this(console, strs)
        {
            this.Inputs = inputs;
            IsButton = true;
            IsInteger = false;
			if (console != null)
			{
				Generation = parent.NewButtonGeneration;
				console.UpdateGeneration();
			}
            ErrPos = pos;
        }

		AConsoleDisplayPart[] strArray;
		public AConsoleDisplayPart[] StrArray { get { return strArray; } }
		EmueraConsole parent;

		public ConsoleDisplayLine ParentLine { get; set; }
		public bool IsButton { get; private set; }
		public bool IsInteger { get; private set; }
		public Int64 Input { get; private set; }
		public string Inputs { get; private set; }
		public int PointX { get; set; }
		public bool PointXisLocked { get; set; }
		public int Width { get; set; }
		public float XsubPixel { get; set; }
		public Int64 Generation { get; private set; }
		public ScriptPosition ErrPos { get; set; }
		public string Title { get; set; }


		public int RelativePointX { get; private set; }
		public void LockPointX(int rel_px)
		{
			PointX = rel_px * Config.FontSize / 100;
			XsubPixel = (rel_px * Config.FontSize / 100.0f) - PointX;
			PointXisLocked = true;
			RelativePointX = rel_px;
		}

		//indexの文字数の前方文字列とindex以降の後方文字列に分割
		public ConsoleButtonString DivideAt(int divIndex, StringMeasure sm)
		{
			if (divIndex <= 0)
				return null;
			List<AConsoleDisplayPart> cssListA = new List<AConsoleDisplayPart>();
			List<AConsoleDisplayPart> cssListB = new List<AConsoleDisplayPart>();
			int index = 0;
			int cssIndex;
			bool b = false;
			for (cssIndex = 0; cssIndex < strArray.Length; cssIndex++)
			{
				if (b)
				{
					cssListB.Add(strArray[cssIndex]);
					continue;
				}
				int length = strArray[cssIndex].Str.Length;
				if (divIndex < index + length)
				{
					ConsoleStyledString oldcss = strArray[cssIndex] as ConsoleStyledString;
					if (oldcss == null || !oldcss.CanDivide)
						throw new ExeEE("文字列分割異常");
					ConsoleStyledString newCss = oldcss.DivideAt(divIndex - index, sm);
					cssListA.Add(oldcss);
					if (newCss != null)
						cssListB.Add(newCss);
					b = true;
					continue;
				}
				else if (divIndex == index + length)
				{
					cssListA.Add(strArray[cssIndex]);
					b = true;
					continue;
				}
				index += length;
				cssListA.Add(strArray[cssIndex]);
			}
			if ((cssIndex >= strArray.Length) && (cssListB.Count == 0))
				return null;
			AConsoleDisplayPart[] cssArrayA = new AConsoleDisplayPart[cssListA.Count];
			AConsoleDisplayPart[] cssArrayB = new AConsoleDisplayPart[cssListB.Count];
			cssListA.CopyTo(cssArrayA);
			cssListB.CopyTo(cssArrayB);
			this.strArray = cssArrayA;
			ConsoleButtonString ret = new ConsoleButtonString(null, cssArrayB);
			this.CalcWidth(sm, XsubPixel);
			ret.CalcWidth(sm,0);
			this.CalcPointX(this.PointX);
			ret.CalcPointX(this.PointX + this.Width);
			ret.parent = this.parent;
			ret.ParentLine = this.ParentLine;
			ret.IsButton = this.IsButton;
			ret.IsInteger = this.IsInteger;
			ret.Input = this.Input;
			ret.Inputs = this.Inputs;
			ret.Generation = this.Generation;
			ret.ErrPos = this.ErrPos;
			ret.Title = this.Title;
			return ret;
		}

		public void CalcWidth(StringMeasure sm, float subpixel)
		{
			Width = -1;
			if ((strArray != null) && (strArray.Length > 0))
			{
				Width = 0;
				for(var i=0; i<strArray.Length; ++i)
				{
                    AConsoleDisplayPart css = strArray[i];
                    if(css.Width <= 0)
						css.SetWidth(sm,subpixel);
					Width += css.Width;
					subpixel = css.XsubPixel;
				}
				if (Width <= 0)
					Width = -1;
			}
			XsubPixel = subpixel;
		}

		/// <summary>
		/// 先にCalcWidthすること。
		/// </summary>
		/// <param name="sm"></param>
		public void CalcPointX(int pointx)
		{
			int px = pointx;
			if (!PointXisLocked)
				PointX = px;
			else
				px = PointX;
			for (int i = 0; i < strArray.Length; i++)
			{
				strArray[i].PointX = px;
				px += strArray[i].Width;
			}
			if (strArray.Length > 0)
			{
				PointX = strArray[0].PointX;
				Width = strArray[strArray.Length - 1].PointX + strArray[strArray.Length - 1].Width - this.PointX;
				//if (Width < 0)
				//	Width = -1;
			}
		}

		internal void ShiftPositionX(int shiftX)
		{
			PointX += shiftX;
            for(var i = 0; i < strArray.Length; ++i)
                strArray[i].PointX += shiftX;
		}

		public void DrawTo(Graphics graph, int pointY, bool isBackLog, TextDrawingMode mode)
		{
			bool isSelecting = (IsButton) && (parent.ButtonIsSelected(this));
            for(var i = 0; i < strArray.Length; ++i)
                strArray[i].DrawTo(graph, pointY, isSelecting, isBackLog, mode);
		}

		public void GDIDrawTo(int pointY, bool isBackLog)
		{
			bool isSelecting = (IsButton) && (parent.ButtonIsSelected(this));
            for(var i = 0; i < strArray.Length; ++i)
                strArray[i].GDIDrawTo(pointY, isSelecting, isBackLog);
		}
		
		public override string ToString()
		{
			if (strArray == null)
				return "";
			string str = "";
            for(var i = 0; i < strArray.Length; ++i)
                str += strArray[i].ToString();
			return str;
		}

	}
}
