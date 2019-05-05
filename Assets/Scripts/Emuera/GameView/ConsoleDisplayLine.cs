using System;
using System.Collections.Generic;
using System.Text;
//using System.Drawing;
using MinorShift._Library;
//using System.Windows.Forms;
using uEmuera.Drawing;

namespace MinorShift.Emuera.GameView
{
	//難読化用属性。enum.ToString()やenum.Parse()を行うなら(Exclude=true)にすること。
	[global::System.Reflection.Obfuscation(Exclude=false)]
	internal enum DisplayLineLastState
	{
		None = 0,
		Normal = 1,
		Selected = 2,
		BackLog = 3,
	}
	
	//難読化用属性。enum.ToString()やenum.Parse()を行うなら(Exclude=true)にすること。
	[global::System.Reflection.Obfuscation(Exclude=false)]
	internal enum DisplayLineAlignment
	{
		LEFT = 0,
		CENTER = 1,
		RIGHT = 2,
	}
	/// <summary>
	/// 表示行。1つ以上のボタン（ConsoleButtonString）からなる
	/// </summary>
	internal sealed class ConsoleDisplayLine
	{
		//public ConsoleDisplayLine(EmueraConsole parentWindow, ConsoleButtonString[] buttons, bool isLogical, bool temporary)
		public ConsoleDisplayLine(ConsoleButtonString[] buttons, bool isLogical, bool temporary)
		{
			//parent = parentWindow;
			if (buttons == null)
			{
				buttons = new ConsoleButtonString[0];
				return;
			}
			this.buttons = buttons;
			for(var i=0; i<buttons.Length; ++i)
                buttons[i].ParentLine = this;
			IsLogicalLine = isLogical;
			IsTemporary = temporary;
		}
		public int LineNo = -1;
		
		///論理行の最初となる場合だけtrue。表示の都合で改行された2行目以降はfalse
		readonly public bool IsLogicalLine = true;
		readonly public bool IsTemporary = false;
		//EmueraConsole parent;
		ConsoleButtonString[] buttons;
		DisplayLineAlignment align;
		public ConsoleButtonString[] Buttons{get{return buttons;}}
		public DisplayLineAlignment Align{get{return align;}}
		bool aligned = false;
		public void SetAlignment(DisplayLineAlignment align)
		{
			if (aligned)
				return;
			this.aligned = true;
			this.align = align;
			if (buttons.Length == 0)
				return;
			//DisplayLineの幅
			int width = 0;
            for(var i = 0; i < buttons.Length; ++i)
                width += buttons[i].Width;
			//現在位置
			int pointX = buttons[0].PointX;

			//目標位置
			int movetoX = 0;
			if (align == DisplayLineAlignment.LEFT)
			{
				//位置固定に対応
				if (IsLogicalLine)
					return;
				movetoX = 0;
			}
			else if (align == DisplayLineAlignment.CENTER)
				movetoX = Config.WindowX / 2 - width / 2;
			else if (align == DisplayLineAlignment.RIGHT)
				movetoX = Config.WindowX - width;

			//移動距離
			int shiftX = movetoX - pointX;
			if(shiftX != 0)
				this.ShiftPositionX(shiftX);
		}

		public void ShiftPositionX(int shiftX)
		{
            for(var i = 0; i < buttons.Length; ++i)
                buttons[i].ShiftPositionX(shiftX);
		}

		public void ChangeStr(ConsoleButtonString[] newButtons)
        {
            buttons = null;
            for(var i = 0; i < newButtons.Length; ++i)
                newButtons[i].ParentLine = this;
			buttons = newButtons;
        }

		public void Clear(Brush brush, Graphics graph, int pointY)
		{
            //Rectangle rect = new Rectangle(0, pointY, Config.WindowX, Config.LineHeight);
            ////graph.FillRectangle(brush, rect);
            ////TODO clear
            //graph.Clear();
		}

		//public ConsoleButtonString GetPointingButton(int pointX)
		//{
		//	////1815 優先順位を逆順にする
		//	////後から描画されるボタンが優先されるように
		//	for (int i = 0; i < buttons.Length; i++)
		//	{
		//		ConsoleButtonString button = buttons[buttons.Length - i - 1];
		//		if ((button.PointX <= pointX) && (button.PointX + button.Width >= pointX))
		//			return button;
		//	}
		//	//foreach (ConsoleButtonString button in buttons)
		//	//{
		//	//    if ((button.PointX <= pointX) && (button.PointX + button.Width >= pointX))
		//	//        return button;
		//	//}
		//	return null;
		//}

		public void DrawTo(Graphics graph, int pointY, bool isBackLog, bool force, TextDrawingMode mode)
		{
            //foreach (ConsoleButtonString button in buttons)
            //    button.DrawTo(graph, pointY, isBackLog, mode);
		}
		
		public void GDIDrawTo(int pointY, bool isBackLog)
		{
			//foreach (ConsoleButtonString button in buttons)
			//	button.GDIDrawTo(pointY, isBackLog);
			//1819 毎回全消去するので穴埋め処理は不要になった
			//int pointX = 0;
			//foreach (ConsoleButtonString button in buttons)
			//{
			//	if (button.Width == 0)
			//		continue;
			//	if (pointX < button.PointX)
			//	{
			//		Rectangle rect = new Rectangle(pointX, pointY, button.PointX - pointX, Config.LineHeight);
			//		GDI.FillRectBGColor(rect);
			//	}
			//	button.GDIDrawTo(pointY, isBackLog);
			//	//フォントの実高さ＜行間の場合隙間ができてしまうので埋める処理
			//	GDI.FillGap(Config.LineHeight, button.Width + (button.PointX - pointX), new Point(pointX, pointY));
			//	pointX = button.PointX + button.Width;
			//}
			//if (pointX < Config.WindowX)
			//{
			//	Rectangle rect = new Rectangle(pointX, pointY, Config.WindowX - pointX, Config.LineHeight);
			//	GDI.FillRectBGColor(rect);
			//}
		}
		
		public override string ToString()
		{
			if (buttons == null)
				return "";
			StringBuilder builder = new StringBuilder();
			for(var i=0; i<buttons.Length; ++i)
				builder.Append(buttons[i].ToString());
			return builder.ToString();
		}
	}
}
