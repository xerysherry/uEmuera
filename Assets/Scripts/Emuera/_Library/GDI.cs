using System;
using System.Collections.Generic;
using System.Text;
//using System.Runtime.InteropServices;
//using System.Drawing;
using MinorShift._Library;
//using System.Windows.Forms;
using uEmuera.Drawing;

namespace MinorShift._Library
{
	//http://www.pinvoke.net/default.aspx/gdi32.BitBlt からコピペ
	/// <summary>
	///     Specifies a raster-operation code. These codes define how the color data for the
	///     source rectangle is to be combined with the color data for the destination
	///     rectangle to achieve the final color.
	/// </summary>
	/// 
	[global::System.Reflection.Obfuscation(Exclude=false)]
	internal enum TernaryRasterOperations : uint
	{
		/// <summary>dest = source</summary>
		SRCCOPY = 0x00CC0020,
		/// <summary>dest = source OR dest</summary>
		SRCPAINT = 0x00EE0086,
		/// <summary>dest = source AND dest</summary>
		SRCAND = 0x008800C6,
		/// <summary>dest = source XOR dest</summary>
		SRCINVERT = 0x00660046,
		/// <summary>dest = source AND (NOT dest)</summary>
		SRCERASE = 0x00440328,
		/// <summary>dest = (NOT source)</summary>
		NOTSRCCOPY = 0x00330008,
		/// <summary>dest = (NOT src) AND (NOT dest)</summary>
		NOTSRCERASE = 0x001100A6,
		/// <summary>dest = (source AND pattern)</summary>
		MERGECOPY = 0x00C000CA,
		/// <summary>dest = (NOT source) OR dest</summary>
		MERGEPAINT = 0x00BB0226,
		/// <summary>dest = pattern</summary>
		PATCOPY = 0x00F00021,
		/// <summary>dest = DPSnoo</summary>
		PATPAINT = 0x00FB0A09,
		/// <summary>dest = pattern XOR dest</summary>
		PATINVERT = 0x005A0049,
		/// <summary>dest = (NOT dest)</summary>
		DSTINVERT = 0x00550009,
		/// <summary>dest = BLACK</summary>
		BLACKNESS = 0x00000042,
		/// <summary>dest = WHITE</summary>
		WHITENESS = 0x00FF0062
	}

	public enum StockObjects
	{
		WHITE_BRUSH = 0,
		LTGRAY_BRUSH = 1,
		GRAY_BRUSH = 2,
		DKGRAY_BRUSH = 3,
		BLACK_BRUSH = 4,
		NULL_BRUSH = 5,
		HOLLOW_BRUSH = NULL_BRUSH,
		WHITE_PEN = 6,
		BLACK_PEN = 7,
		NULL_PEN = 8,
		OEM_FIXED_FONT = 10,
		ANSI_FIXED_FONT = 11,
		ANSI_VAR_FONT = 12,
		SYSTEM_FONT = 13,
		DEVICE_DEFAULT_FONT = 14,
		DEFAULT_PALETTE = 15,
		SYSTEM_FIXED_FONT = 16,
		DEFAULT_GUI_FONT = 17,
		DC_BRUSH = 18,
		DC_PEN = 19,
	}
	public enum StretchMode
	{
		STRETCH_ANDSCANS = 1,
		STRETCH_ORSCANS = 2,
		STRETCH_DELETESCANS = 3,
		STRETCH_HALFTONE = 4,
	}

	internal static class GDI
	{
  //      [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
		//static extern bool TextOut(IntPtr hdc, int nXStart, int nYStart, string lpString, int cbString);
		//[DllImport("gdi32.dll")]
		//static extern uint SetTextColor(IntPtr hdc, int crColor);
		//[DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
		//public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
		//[DllImport("gdi32.dll")]
		//public static extern bool DeleteObject(IntPtr hObject);
		//[DllImport("gdi32.dll")]
		//static extern uint SetBkColor(IntPtr hdc, int crColor);
		//[DllImport("gdi32.dll")]
		//static extern bool Rectangle(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);
		//[DllImport("gdi32.dll")]
		//static extern IntPtr CreateSolidBrush(int crColor);
		//[DllImport("gdi32.dll")]
		//static extern IntPtr CreatePen(int fnPenStyle, int nWidth, int crColor);
  //      [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
		//static extern bool GetTextExtentPoint32(IntPtr hdc, string lpString, int cbString, out Size lpSize);
		//[DllImport("gdi32.dll")]
		//static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth,
		//   int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);
		//[DllImport("gdi32.dll")]
		//static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest,
		//	int nWidthDest, int nHeightDest,
		//	IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
		//	TernaryRasterOperations dwRop);
		//[DllImport("gdi32.dll")]
		//static extern bool SetStretchBltMode(IntPtr hdc, StretchMode iStretchMode);

  //      [DllImport("user32", EntryPoint = "TabbedTextOut", CharSet = CharSet.Auto)]
  //      static extern int TabbedTextOutW(IntPtr hdc, int x, int y, string lpString, int nCount, int nTabPositions, ref int lpnTabStopPositions, int nTabOrigin);
  //      [DllImport("user32", EntryPoint = "GetTabbedTextExtent", CharSet = CharSet.Auto)]
  //      static extern int GetTabbedTextExtentW(IntPtr hdc, string lpString, int nCount, int nTabPositions, ref int lpnTabStopPositions);
		//[DllImport("gdi32.dll")]
		//static extern IntPtr GetStockObject(StockObjects fnObject);
		//[DllImport("gdi32.dll")]
		//static extern bool Polygon(IntPtr hdc, Point[] lpPoints, int nCount);
		//[DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
		//static extern bool Ellipse(IntPtr hdc, int nLeftRect, int nTopRect,
		//   int nRightRect, int nBottomRect);
		//[DllImport("gdi32.dll")]
		//static extern bool Pie(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect,
		//   int nBottomRect, int nXRadial1, int nYRadial1, int nXRadial2, int nYRadial2);

		static IntPtr hDC;
		static Font lastFont = null;
		static IntPtr defaulthFont;
		static IntPtr defaulthBrush;
		static IntPtr defaulthPen;
		static Color lastTextColor;
		static Color lastBrushColor;
		static Color lastPenColor;
		static Size fontMetrics;
		static bool usingStockBrush = false;
		static int devnull;
		//static bool isNt = (System.Environment.OSVersion.Platform == PlatformID.Win32NT) ? true : false;
		static GDI()
		{
			//if (System.Environment.OSVersion.Platform == PlatformID.Win32NT)
			//{
			//	TabbedTextOutFull = TabbedTextOutFullNT;
			//	TabbedTextOut = TabbedTextOutNT;
			//	MeasureText = MeasureTextNT;
			//}
			//else
			//{
			//	TabbedTextOutFull = TabbedTextOutFull98;
			//	TabbedTextOut = TabbedTextOut98;
			//	MeasureText = MeasureText98;
			//}

		}
		public static void GDIStart(Graphics g, Color backGroundColor)
		{
			//GDI.hDC = g.GetHdc();
			//IntPtr hBrush = CreateSolidBrush(ColorTranslator.ToWin32(backGroundColor));
			//defaulthBrush = SelectObject(hDC, hBrush);
			//IntPtr hPen = CreatePen(0, 0, ColorTranslator.ToWin32(backGroundColor));
			//defaulthPen = SelectObject(hDC, hPen);
			//SetTextColor(hDC, ColorTranslator.ToWin32(backGroundColor));
			//SetBkColor(hDC, ColorTranslator.ToWin32(backGroundColor));
			//lastFont = null;
			//lastBrushColor = backGroundColor;
			//lastPenColor = backGroundColor;
			//lastTextColor = backGroundColor;
			//usingStockBrush = false;
			//SetStretchBltMode(GDI.hDC, StretchMode.STRETCH_DELETESCANS);
		}
		public static void GDIEnd(Graphics g)
		{
			//if (lastFont != null)
			//	DeleteObject(SelectObject(hDC, defaulthFont));
			//if (usingStockBrush)
			//	SelectObject(hDC, defaulthBrush);
			//else
			//	DeleteObject(SelectObject(hDC, defaulthBrush));

			//DeleteObject(SelectObject(hDC, defaulthPen));
			//g.ReleaseHdc(hDC);
			//lastFont = null;
		}

		public static void SetFont(Font font)
		{
			//if (lastFont == font)
			//	return;
			//IntPtr hFont = font.ToHfont();
			//IntPtr hOldFont = SelectObject(hDC, hFont);
			//if (lastFont == null)
			//	defaulthFont = hOldFont;
			//else
			//	DeleteObject(hOldFont);
			//lastFont = font;
			//GetTextExtentPoint32(hDC, "あ", "あ".Length, out fontMetrics);
		}
		public static uint SetTextColor(Color color)
		{
            //if (color == lastTextColor)
            //	return 0;
            //lastTextColor = color;
            //return SetTextColor(hDC, ColorTranslator.ToWin32(color));
            return 1;
		}

		public static void SetBrushColor(Color color)
		{
			//if (lastBrushColor == color)
			//	return;
			//IntPtr hBrush = IntPtr.Zero;
			//if (color.A == 0)
			//	hBrush = GetStockObject(StockObjects.NULL_BRUSH);
			//else
			//	hBrush = CreateSolidBrush(ColorTranslator.ToWin32(color));

			//if (usingStockBrush)
			//	SelectObject(hDC, hBrush);
			//else
			//	DeleteObject(SelectObject(hDC, hBrush));
			//usingStockBrush = color.A == 0;
			//lastBrushColor = color;
		}
		public static void SetPenColor(Color color)
		{
			//if (lastPenColor == color)
			//	return;
			//IntPtr hPen = CreatePen(0, 0, ColorTranslator.ToWin32(color));
			//DeleteObject(SelectObject(hDC, hPen));
			//lastPenColor = color;
		}


		public delegate void DelegateTextOut(string str, int x, int y);
		public static DelegateTextOut TabbedTextOut;
		public delegate void DelegateTextOutFull(Font font, Color color, string str, int x, int y);
		public static DelegateTextOutFull TabbedTextOutFull;
		public delegate Size DelegateMeasureText(string str, Font font);
		public static DelegateMeasureText MeasureText;
		public static void TabbedTextOutFull98(Font font, Color color, string str, int x, int y)
		{
			//if (lastFont != font)
			//	SetFont(font);
			//if (lastTextColor != color)
			//	SetTextColor(color);
			//TabbedTextOutW(hDC, x, y, str, LangManager.GetStrlenLang(str), 0, ref devnull, 0);
		}
		public static void TabbedTextOutFullNT(Font font, Color color, string str, int x, int y)
		{
			//if (lastFont != font)
			//	SetFont(font);
			//if (lastTextColor != color)
			//	SetTextColor(color);
			//TabbedTextOutW(hDC, x, y, str, str.Length, 0, ref devnull, 0);
		}

		static void TabbedTextOut98(string str, int x, int y)
		{
			////TextOut(hDC, p.X, p.Y, str, str.Length);
			//TabbedTextOutW(hDC, x, y, str, LangManager.GetStrlenLang(str), 0, ref devnull, 0);
		}
		static void TabbedTextOutNT(string str, int x, int y)
		{
			////TextOut(hDC, p.X, p.Y, str, str.Length);
			//TabbedTextOutW(hDC, x, y, str, str.Length, 0, ref devnull, 0);
		}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    
		public static void FillRect(Rectangle rect)
		{
			//Rectangle(hDC, rect.X, rect.Y, rect.Right, rect.Bottom);
		}

		public static void FillRect(Rectangle rect, Color penColor, Color brushColor)
		{
			SetPenColor(penColor);
			SetBrushColor(brushColor);
			//Rectangle(hDC, rect.X, rect.Y, rect.Right, rect.Bottom);
		}


		//public static void FillGap(int lineHeight, int length, Point pt)
		//{
		//	if (lineHeight <= fontMetrics.Height)
		//		return;
		//	Rectangle rect = new Rectangle(pt.X, pt.Y + fontMetrics.Height, length, lineHeight - fontMetrics.Height);
		//	FillRect(rect);
		//}

		/// <summary>
		/// 必要に応じてStretch。アルファブレンドなし。
		/// </summary>
		public static void DrawImage(int destX, int destY,int width, int height, IntPtr srchDC, Rectangle srcRect)
		{
			//if (srcRect.Height == height && srcRect.Width == width)
			//	BitBlt(hDC, destX, destY, width, height, srchDC, srcRect.X, srcRect.Y, TernaryRasterOperations.SRCCOPY);
			//else
			//	StretchBlt(hDC, destX, destY, width, height, srchDC, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, TernaryRasterOperations.SRCCOPY);
		}

        public static void DrawImage(int destX, int destY, int width, int hegith,
                    Bitmap texture, Rectangle srcrect)
        { }

        #region MesureText用

        static IntPtr hDCMesureText;
		static Font mtLastFont = null;
		static IntPtr mtDefaulthFont;

		static Size MeasureText98(string str, Font font)
		{
            //if (mtLastFont != font)
            //{
            //	IntPtr hFont = font.ToHfont();
            //	IntPtr hOldFont = SelectObject(hDCMesureText, hFont);
            //	if (mtLastFont == null)
            //		mtDefaulthFont = hOldFont;
            //	else
            //		DeleteObject(hOldFont);
            //	mtLastFont = font;
            //	GetTextExtentPoint32(hDCMesureText, "あ", "あ".Length, out fontMetrics);
            //}
            //int ret = GetTabbedTextExtentW(hDCMesureText, str, LangManager.GetStrlenLang(str), 0, ref devnull);
            //Size size = new Size(ret & 0xffff, (ret >> 16) & 0xffff);
            //return size;
            return new Size(16, 16);
		}
		static Size MeasureTextNT(string str, Font font)
		{
            //if (mtLastFont != font)
            //{
            //	IntPtr hFont = font.ToHfont();
            //	IntPtr hOldFont = SelectObject(hDCMesureText, hFont);
            //	if (mtLastFont == null)
            //		mtDefaulthFont = hOldFont;
            //	else
            //		DeleteObject(hOldFont);
            //	mtLastFont = font;
            //	GetTextExtentPoint32(hDCMesureText, "あ", "あ".Length, out fontMetrics);
            //}
            //int ret = GetTabbedTextExtentW(hDCMesureText, str, str.Length, 0, ref devnull);
            //Size size = new Size(ret & 0xffff, (ret >> 16) & 0xffff);
            //return size;
            return new Size(16, 16);
        }

		public static void GdiMesureTextStart(Graphics g)
		{
			//hDCMesureText = g.GetHdc();
			//mtLastFont = null;
		}
		public static void GdiMesureTextEnd(Graphics g)
		{
			//if (mtLastFont != null)
			//	DeleteObject(SelectObject(hDCMesureText, mtDefaulthFont));
			//g.ReleaseHdc(hDCMesureText);
			//mtLastFont = null;
		}
		#endregion
	}
}
