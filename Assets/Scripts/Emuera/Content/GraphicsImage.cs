using MinorShift._Library;
using System;
using System.Collections.Generic;
//using System.Drawing;
//using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using uEmuera.Drawing;
//using System.Threading.Tasks;

namespace MinorShift.Emuera.Content
{
	internal sealed class GraphicsImage : AbstractImage
	{
		//public Bitmap Bitmap;
		//public IntPtr GDIhDC { get; protected set; }
		//protected Graphics g;
		//protected IntPtr hBitmap;
		//protected IntPtr hDefaultImg;

		public GraphicsImage(int id)
		{
			ID = id;
			//g = null;
			//Bitmap = null;
			//created = false;
			//locked = false;
		}
		public readonly int ID;
        //Size size;
        //Brush brush = null;
        //Pen pen = null;
        //Font font = null;
        //Bitmap b;
        //Graphics g;


        ////bool created;
        ////bool locked;
        //public void LockGraphics()
        //{
        //	//if (locked)
        //	//	return;
        //	//g = Graphics.FromImage(b);
        //	//locked = true;
        //}
        //public void UnlockGraphics()
        //{
        //	//if (!locked)
        //	//	return;
        //	//g.Dispose();
        //	//g = null;
        //	//locked = false;
        //}

        #region Bitmap書き込み・作成

        /// <summary>
        /// GCREATE(int ID, int width, int height)
        /// Graphicsの基礎となるBitmapを作成する。エラーチェックは呼び出し元でのみ行う
        /// </summary>
        public void GCreate(int x, int y, bool useGDI)
        {
            this.GDispose();
            is_created = true;
            width = x;
            height = y;
        }

        internal void GCreateFromF(Bitmap bmp, bool useGDI)
        {
            //if(useGDI)
            //    throw new NotImplementedException();
            //this.GDispose();
            //Bitmap = new Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //size = new Size(bmp.Width, bmp.Height);
            //g = Graphics.FromImage(Bitmap);
            //g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);

            this.GDispose();
            is_created = true;
            width = bmp.Width;
            height = bmp.Height;

        }

        /// <summary>
        /// GCLEAR(int ID, int cARGB)
        /// エラーチェックは呼び出し元でのみ行う
        /// </summary>
        public void GClear(uEmuera.Drawing.Color c)
        {
        //	if (g == null)
        //		throw new NullReferenceException();
        //	g.Clear(c);
        }

        /// <summary>
        /// GDRAWTEXTGDRAWTEXT int ID, str text, int x, int y
        /// エラーチェックは呼び出し元でのみ行う
        /// </summary>
        //public void GDrawString(string text, int x, int y)
        //{
        //	if (g == null)
        //		throw new NullReferenceException();
        //	Font usingFont = font;
        //	if (usingFont == null)
        //		usingFont = Config.Font;
        //	if (brush != null)
        //	{
        //		g.DrawString(text, usingFont, brush, x, y);
        //	}
        //	else
        //	{
        //		using (SolidBrush b = new SolidBrush(Config.ForeColor))
        //			g.DrawString(text, usingFont, b, x, y);
        //	}
        //}
        /// <summary>
        /// GDRAWTEXTGDRAWTEXT int ID, str text, int x, int y, int width, int height
        /// エラーチェックは呼び出し元でのみ行う
        /// </summary>
        //public void GDrawString(string text, int x, int y, int width, int height)
        //{
        //	if (g == null)
        //		throw new NullReferenceException();
        //	Font usingFont = font;
        //	if (usingFont == null)
        //		usingFont = Config.Font;
        //	if (brush != null)
        //	{
        //		g.DrawString(text, usingFont, brush, new RectangleF(x,y,width,height));
        //	}
        //	else
        //	{
        //		using (SolidBrush b = new SolidBrush(Config.ForeColor))
        //			g.DrawString(text, usingFont, b, new RectangleF(x, y, width, height));
        //	}
        //}

        /// <summary>
        /// GDRAWRECTANGLE(int ID, int x, int y, int width, int height)
        /// エラーチェックは呼び出し元でのみ行う
        /// </summary>
        //public void GDrawRectangle(Rectangle rect)
        //{
        //	if (g == null)
        //		throw new NullReferenceException();
        //	if (pen != null)
        //	{
        //		g.DrawRectangle(pen, rect);
        //	}
        //	else
        //	{
        //		using (Pen p = new Pen(Config.ForeColor))
        //			g.DrawRectangle(p, rect);
        //	}
        //}

        /// <summary>
        /// GFILLRECTANGLE(int ID, int x, int y, int width, int height)
        /// エラーチェックは呼び出し元でのみ行う
        /// </summary>
        public void GFillRectangle(Rectangle rect)
        {
        //	if (g == null)
        //		throw new NullReferenceException();
        //	if (brush != null)
        //	{
        //		g.FillRectangle(brush, rect);
        //	}
        //	else
        //	{
        //		using (SolidBrush b = new SolidBrush(Config.BackColor))
        //			g.FillRectangle(b, rect);
        //	}
        }

		/// <summary>
		/// GDRAWCIMG(int ID, str imgName, int destX, int destY, int destWidth, int destHeight)
		/// エラーチェックは呼び出し元でのみ行う
		/// </summary>
		public void GDrawCImg(ASprite img, Rectangle destRect)
		{
			//if (g == null)
			//	throw new NullReferenceException();
			//img.GraphicsDraw(g, destRect);
		}

		/// <summary>
		/// GDRAWCIMG(int ID, str imgName, int destX, int destY, int destWidth, int destHeight, float[][] cm)
		/// エラーチェックは呼び出し元でのみ行う
		/// </summary>
		public void GDrawCImg(ASprite img, Rectangle destRect, float[][] cm)
		{
			//if (g == null)
			//	throw new NullReferenceException();
			//ImageAttributes imageAttributes = new ImageAttributes();
			//ColorMatrix colorMatrix = new ColorMatrix(cm);
			//imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			//img.GraphicsDraw(g, destRect, imageAttributes);
		}

        /// <summary>
        /// GDRAWG(int ID, int srcID, int destX, int destY, int destWidth, int destHeight, int srcX, int srcY, int srcWidth, int srcHeight)
        /// エラーチェックは呼び出し元でのみ行う
        /// </summary>
        public void GDrawG(GraphicsImage srcGra, Rectangle destRect, Rectangle srcRect)
        {
        //	if (g == null)
        //		throw new NullReferenceException();
        //	Bitmap src = srcGra.GetBitmap();
        //	g.DrawImage(src, destRect, srcRect, GraphicsUnit.Pixel);
        }


        /// <summary>
        /// GDRAWG(int ID, int srcID, int destX, int destY, int destWidth, int destHeight, int srcX, int srcY, int srcWidth, int srcHeight, float[][] cm)
        /// エラーチェックは呼び出し元でのみ行う
        /// </summary>
        public void GDrawG(GraphicsImage srcGra, Rectangle destRect, Rectangle srcRect, float[][] cm)
        {
        //	if (g == null)
        //		throw new NullReferenceException();
        //	Bitmap src = srcGra.GetBitmap();
        //	ImageAttributes imageAttributes = new ImageAttributes();
        //	ColorMatrix colorMatrix = new ColorMatrix(cm);
        //	imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default,ColorAdjustType.Bitmap);
        //	//g.DrawImage(img.Bitmap, destRect, srcRect, GraphicsUnit.Pixel, imageAttributes);なんでこのパターンないのさ
        //	g.DrawImage(src, destRect, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, GraphicsUnit.Pixel, imageAttributes);

        }


        /// <summary>
        /// GDRAWGWITHMASK(int ID, int srcID, int maskID, int destX, int destY)
        /// エラーチェックは呼び出し元でのみ行う
        /// </summary>
        public void GDrawGWithMask(GraphicsImage srcGra, GraphicsImage maskGra, Point destPoint)
        {
        //	if (g == null)
        //		throw new NullReferenceException();
        //	Bitmap destImg = this.GetBitmap();
        //	byte[] srcBytes = BytesFromBitmap(srcGra.GetBitmap());
        //	byte[] srcMaskBytes = BytesFromBitmap(maskGra.GetBitmap());
        //	Rectangle destRect = new Rectangle(destPoint.X, destPoint.Y, srcGra.Width, srcGra.Height);

        //	System.Drawing.Imaging.BitmapData bmpData =
        //		destImg.LockBits(new Rectangle(0,0, destImg.Width,destImg.Height),
        //		System.Drawing.Imaging.ImageLockMode.ReadWrite,
        //		PixelFormat.Format32bppArgb);
        //	try
        //	{
        //		IntPtr ptr = bmpData.Scan0;
        //		byte[] pixels = new byte[bmpData.Stride * destImg.Height];
        //		System.Runtime.InteropServices.Marshal.Copy(ptr, pixels, 0, pixels.Length);


        //		for (int y = 0; y < srcGra.Height; y++)
        //		{

        //			int destIndex = ((destPoint.Y + y) * destImg.Width + destPoint.X) * 4;
        //			int srcIndex = ((0 + y) * srcGra.Width + 0) * 4;
        //			for (int x = 0; x < srcGra.Width; x++)
        //			{
        //				if (srcMaskBytes[srcIndex] == 255)//完全不透明
        //				{
        //					pixels[destIndex++] = srcBytes[srcIndex++];
        //					pixels[destIndex++] = srcBytes[srcIndex++];
        //					pixels[destIndex++] = srcBytes[srcIndex++];
        //					pixels[destIndex++] = srcBytes[srcIndex++];
        //				}
        //				else if (srcMaskBytes[srcIndex] == 0)//完全透明
        //				{
        //					destIndex += 4;
        //					srcIndex += 4;
        //				}
        //				else//半透明 alpha/255ではなく（alpha+1）/256で計算しているがたぶん誤差
        //				{
        //					int mask = srcMaskBytes[srcIndex]; mask++;
        //					pixels[destIndex] = (byte)((srcBytes[srcIndex] * mask + pixels[destIndex] * (256 - mask)) >> 8); srcIndex++; destIndex++;
        //					pixels[destIndex] = (byte)((srcBytes[srcIndex] * mask + pixels[destIndex] * (256 - mask)) >> 8); srcIndex++; destIndex++;
        //					pixels[destIndex] = (byte)((srcBytes[srcIndex] * mask + pixels[destIndex] * (256 - mask)) >> 8); srcIndex++; destIndex++;
        //					pixels[destIndex] = (byte)((srcBytes[srcIndex] * mask + pixels[destIndex] * (256 - mask)) >> 8); srcIndex++; destIndex++;
        //				}
        //			}
        //		}

        //		// Bitmapへコピー
        //		System.Runtime.InteropServices.Marshal.Copy(pixels, 0, ptr, pixels.Length);
        //	}
        //	finally
        //	{
        //		destImg.UnlockBits(bmpData);
        //	}
        }

        public void GSetFont(uEmuera.Drawing.Font r)
        {
        //	if (font != null)
        //		font.Dispose();
        //	font = r;
        }
        public void GSetBrush(Brush r)
        {
        //	if (brush != null)
        //		brush.Dispose();
        //	brush = r;
        }
        public void GSetPen(Pen r)
        {
        //	if (pen != null)
        //		pen.Dispose();
        //	pen = r;
        }
        //private static byte[] BytesFromBitmap(Bitmap bmp)
        //{
        //	BitmapData bmpData = bmp.LockBits(
        //	  new Rectangle(0, 0, bmp.Width, bmp.Height),
        //	  ImageLockMode.ReadOnly,  // 書き込むときはReadAndWriteで
        //	  PixelFormat.Format32bppArgb
        //	);
        //	if (bmpData.Stride < 0)
        //		throw new Exception();//変な形式のが送られてくることはありえないはずだが一応
        //	byte[] pixels = new byte[bmpData.Stride * bmp.Height];
        //	try
        //	{ 
        //		IntPtr ptr = bmpData.Scan0;
        //		System.Runtime.InteropServices.Marshal.Copy(ptr, pixels, 0, pixels.Length);
        //	}
        //	finally
        //	{
        //		bmp.UnlockBits(bmpData);

        //	}
        //	return pixels;
        //}

        /// <summary>
        /// GTOARRAY int ID, var array
        /// エラーチェックは呼び出し元でのみ行う
        /// <returns></returns>
        //public bool GBitmapToInt64Array(Int64[,] array, int xstart, int ystart)
        //{
        //	if (g == null || Bitmap == null)
        //		throw new NullReferenceException();
        //	int w = Bitmap.Width;
        //	int h = Bitmap.Height;
        //	if (xstart + w > array.GetLength(0) || ystart + h > array.GetLength(1))
        //		return false;
        //	Rectangle rect = new Rectangle(0, 0, w, h);
        //	System.Drawing.Imaging.BitmapData bmpData =
        //		Bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
        //		PixelFormat.Format32bppArgb);
        //	IntPtr ptr = bmpData.Scan0;
        //	byte[] rgbValues = new byte[w * h * 4];
        //	Marshal.Copy(ptr, rgbValues, 0, rgbValues.Length);
        //	Bitmap.UnlockBits(bmpData);
        //	int i = 0;
        //	for (int y = 0; y < h; y++)
        //	{
        //		for (int x = 0; x < w; x++)
        //		{
        //			array[x + xstart, y + ystart] =
        //			rgbValues[i++] + //B
        //			(((Int64)rgbValues[i++]) << 8) + //G
        //			(((Int64)rgbValues[i++]) << 16) + //R
        //			(((Int64)rgbValues[i++]) << 24);  //A
        //		}
        //	}
        //	return true;
        //}


        /// <summary>
        /// GFROMARRAY int ID, var array
        /// エラーチェックは呼び出し元でのみ行う
        /// <returns></returns>
        //public bool GByteArrayToBitmap(Int64[,] array, int xstart, int ystart)
        //{
        //	if (g == null || Bitmap == null)
        //		throw new NullReferenceException();
        //	int w = Bitmap.Width;
        //	int h = Bitmap.Height;
        //	if (xstart + w > array.GetLength(0) || ystart + h > array.GetLength(1))
        //		return false;

        //	byte[] rgbValues = new byte[w * h * 4];
        //	int i = 0;
        //	for (int y = 0; y < h; y++)
        //	{
        //		for (int x = 0; x < w; x++)
        //		{
        //			Int64 c = array[x + xstart, y + ystart];
        //			rgbValues[i++] = (byte)(c & 0xFF);//B
        //			rgbValues[i++] = (byte)((c >> 8) & 0xFF);//G
        //			rgbValues[i++] = (byte)((c >> 16) & 0xFF);//R
        //			rgbValues[i++] = (byte)((c >> 24) & 0xFF);//A
        //		}
        //	}
        //	Rectangle rect = new Rectangle(0, 0, w, h);
        //	System.Drawing.Imaging.BitmapData bmpData =
        //		Bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly,
        //		PixelFormat.Format32bppArgb);
        //	IntPtr ptr = bmpData.Scan0;
        //	Marshal.Copy(rgbValues, 0, ptr, rgbValues.Length);
        //	Bitmap.UnlockBits(bmpData);
        //	return true;
        //}
        #endregion
        #region Bitmap読み込み・削除
        /// <summary>
        /// 未作成ならエラー
        /// </summary>
        //public Bitmap GetBitmap()
        //{
        //	if (Bitmap == null)
        //		throw new NullReferenceException();
        //	//UnlockGraphics();
        //	return Bitmap;
        //}
        /// <summary>
        /// GSETCOLOR(int ID, int cARGB, int x, int y)
        /// エラーチェックは呼び出し元でのみ行う
        /// </summary>
        public void GSetColor(uEmuera.Drawing.Color c, int x, int y)
        {
        	if (Bitmap == null)
        		throw new NullReferenceException();
            //	//UnlockGraphics();
            //	Bitmap.SetPixel(x, y, c);
        }

        /// <summary>
        /// GGETCOLOR(int ID, int x, int y)
        /// エラーチェックは呼び出し元でのみ行う。特に画像範囲内であるかどうかチェックすること
        /// </summary>
        public uEmuera.Drawing.Color GGetColor(int x, int y)
        {
        	if (Bitmap == null)
        		throw new NullReferenceException();
        	//UnlockGraphics();
        	return Bitmap.GetPixel(x, y);
        }


        /// <summary>
        /// GDISPOSE(int ID)
        /// </summary>
        public void GDispose()
        {
            //	size = new Size(0, 0);
            //	if (Bitmap == null)
            //		return;
            //	if (gdi)
            //	{
            //		GDI.SelectObject(GDIhDC, hDefaultImg);
            //		GDI.DeleteObject(hBitmap);
            //		g.ReleaseHdc(GDIhDC);
            //	}
            //	if (g != null)
            //		g.Dispose();
            //	if (Bitmap != null)
            //		Bitmap.Dispose();
            //	if (brush != null)
            //		brush.Dispose();
            //	if (pen != null)
            //		pen.Dispose();
            //	if (font != null)
            //		font.Dispose();
            //	g = null;
            //	Bitmap = null;
            //	brush = null;
            //	pen = null;
            //	font = null;


            is_created = false;
            width = 0;
            height = 0;
        }

        public override void Dispose()
        {
            //	this.GDispose();
            //if(render_texture != null)
            //{
            //    UnityEngine.Object.Destroy(render_texture);
            //}
            this.GDispose();
        }

        ~GraphicsImage()
        {
            Dispose();
        }
        #endregion

//#region 状態判定（Bitmap読み書きを伴わない）
        //public override bool IsCreated { get { return g != null; } }
        public override bool IsCreated { get { return is_created; } }
        bool is_created = false;

        /// <summary>
        /// int GWIDTH(int ID)
        /// </summary>
        public int Width { get { return width; } }
        int width = 0;
		/// <summary>
		/// int GHEIGHT(int ID)
		/// </summary>
		public int Height { get { return height; } }
        int height = 0;
        //#endregion
	}
}
