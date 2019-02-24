using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Text;
using uEmuera.Drawing;

namespace MinorShift.Emuera.Content
{
	abstract class AContentItem
	{
		protected AContentItem(string name) { Name = name; }
		public readonly string Name;
		//public bool Enabled { get; protected set; }
		public abstract bool IsCreated { get; }
	}
	
	internal abstract class ASprite : AContentItem, IDisposable
	{
		public ASprite(string name, Rectangle rect)
			: base(name)
		{
			Rectangle = rect;
		}
		public abstract Color SpriteGetColor(int x, int y);
		public abstract Bitmap Bitmap{get;}
		/// <summary>
		/// Width, Heightは負の値をとり得る
		/// </summary>
		public readonly Rectangle Rectangle;
		public Point Position;
		public abstract void Dispose();

		public void Move(Point point){ Position.Offset(point); }
	}


	internal sealed class SpriteB : ASprite
	{
		public SpriteB(string name, Bitmap bmp, Rectangle rect)
			: base(name, rect)
		{
			this.bmp = bmp;
            bmp.name = name;
		}
		public readonly Bitmap bmp;

		public override Bitmap Bitmap
		{
			get { return bmp; }
		}

		public override bool IsCreated
		{
			get { return (bmp != null); }
		}

		public override void Dispose()
		{
			if (bmp != null)
				bmp.Dispose();
		}

		public override Color SpriteGetColor(int x, int y)
		{
			if (bmp == null)
				return Color.Transparent;
			int bmpX = x + Rectangle.X;
			int bmpY = y + Rectangle.Y;
			if (bmpX < 0 || bmpX >= bmp.Width || bmpY < 0 || bmpY >= bmp.Height)
				return Color.Transparent;

			return bmp.GetPixel(bmpX, bmpY);
		}
	}

	internal sealed class SpriteG : ASprite
	{
		public SpriteG(string name, GraphicsImage gra, Rectangle rect)
			: base(name, rect)
		{
			g = gra;
		}
		public GraphicsImage g;

		public override Bitmap Bitmap
		{
			get
			{
				if (g != null && g.IsCreated)
					return g.Bitmap;
				return null;
			}
		}

		public override bool IsCreated
		{
			get { return (g != null && g.IsCreated); }
		}

		public override void Dispose()
		{
			g = null;
		}

		public override Color SpriteGetColor(int x, int y)
		{
			Bitmap bmp = this.Bitmap;
			if (bmp == null)
				return Color.Transparent;
			int bmpX = x + Rectangle.X;
			int bmpY = y + Rectangle.Y;
			if (bmpX < 0 || bmpX >= bmp.Width || bmpY < 0 || bmpY >= bmp.Height)
				return Color.Transparent;

			return bmp.GetPixel(bmpX, bmpY);
		}
	}

	internal sealed class SpriteF : ASprite
	{
		public SpriteF(string name, ConstImage image, Rectangle rect, Point pos)
			: base(name, rect)
		{
			BaseImage = image;
            this.Position = pos;
		}
		public readonly ConstImage BaseImage;

		public override Bitmap Bitmap
		{
			get
			{
				if (BaseImage != null && BaseImage.IsCreated)
					return BaseImage.Bitmap;
				return null;
			}
		}

		public override bool IsCreated
		{
			get { return (BaseImage != null && BaseImage.IsCreated); }
		}

		/// <summary>
		/// TODO:1823 全てのCroppedImageFが解放されたときにBaseImageを開放する処理を作成する。
		/// </summary>
		public override void Dispose()
		{
			//throw new NotImplementedException();
		}


		public override Color SpriteGetColor(int x, int y)
		{
			Bitmap bmp = this.Bitmap;
			if (bmp == null)
				return Color.Transparent;
			int bmpX = x + Rectangle.X;
			int bmpY = y + Rectangle.Y;
			if (bmpX < 0 || bmpX >= bmp.Width || bmpY < 0 || bmpY >= bmp.Height)
				return Color.Transparent;

			return bmp.GetPixel(bmpX, bmpY);
		}
	}
}
