using System;
using System.Collections.Generic;
using MinorShift.Emuera.Sub;
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
		public ASprite(string name, Size size)
			: base(name)
		{
			if (size.Width < 0)
				size.Width = -size.Width;
			if (size.Height < 0)
				size.Height = -size.Height;
			DestBaseSize = size;
		}
		public abstract Color SpriteGetColor(int x, int y);
        public abstract Bitmap Bitmap { get; }
        /// <summary>
        /// 出力される標準のサイズ。正の値のみ。
        /// </summary>
        public readonly Size DestBaseSize;

		/// <summary>
		/// 出力時の位置調整。拡大縮小して出力する場合には同じ比率で調整する。
		/// </summary>
		public Point DestBasePosition;

        public Rectangle Rectangle
        {
            get { return new Rectangle(DestBasePosition, DestBaseSize); }
        }


        public abstract void GraphicsDraw(Graphics g, Point offset);
		public abstract void GraphicsDraw(Graphics g, Rectangle destRect);
		public abstract void GraphicsDraw(Graphics g, Rectangle destRect, ImageAttributes attr);
		public abstract void Dispose();
		public void Move(Point point){ DestBasePosition.Offset(point); }
	}


	internal abstract class ASpriteSingle : ASprite
	{
		public ASpriteSingle(string name, AbstractImage img, Rectangle rect)
			: base(name, rect.Size)
		{
			SrcRectangle = rect;
			BaseImage = img;
		}
		public AbstractImage BaseImage;

		/// <summary>
		/// ソース画像上の位置を指定する四角形。Width, Heightは負の値をとり得る
		/// </summary>
		public readonly Rectangle SrcRectangle;
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
		public override Color SpriteGetColor(int x, int y)
		{
			Bitmap bmp = this.Bitmap;
			if (bmp == null)
				return Color.Transparent;
			int bmpX = x + SrcRectangle.X;
			int bmpY = y + SrcRectangle.Y;
			if (bmpX < 0 || bmpX >= bmp.Width || bmpY < 0 || bmpY >= bmp.Height)
				return Color.Transparent;

			return bmp.GetPixel(bmpX, bmpY);
		}
		public override void Dispose()
		{
			BaseImage = null;
		}


		public override void GraphicsDraw(Graphics g, Point offset)
		{
			offset.Offset(DestBasePosition);
			g.DrawImage(Bitmap, new Rectangle(offset, DestBaseSize), SrcRectangle, GraphicsUnit.Pixel);
		}
		public override void GraphicsDraw(Graphics g, Rectangle destRect)
		{
			if (!DestBasePosition.IsEmpty)
			{
				destRect.X = destRect.X + DestBasePosition.X * destRect.Width / SrcRectangle.Width;
				destRect.Y = destRect.Y + DestBasePosition.Y * destRect.Height / SrcRectangle.Height;
			}
			g.DrawImage(Bitmap, destRect, SrcRectangle, GraphicsUnit.Pixel);
		}

		public override void GraphicsDraw(Graphics g, Rectangle destRect, ImageAttributes attr)
		{
			if (!DestBasePosition.IsEmpty)
			{
				destRect.X = destRect.X + DestBasePosition.X * destRect.Width / SrcRectangle.Width;
				destRect.Y = destRect.Y + DestBasePosition.Y * destRect.Height / SrcRectangle.Height;
			}
			//g.DrawImage(Bitmap, destRect, SrcRectangle, GraphicsUnit.Pixel, attr);←このパターンがない
			g.DrawImage(Bitmap, destRect, SrcRectangle.X, SrcRectangle.Y, SrcRectangle.Width, SrcRectangle.Height, GraphicsUnit.Pixel, attr);
		}

	}

	/// <summary>
	/// ERB中で作るGを元にしたSprite。GDI非対応
	/// </summary>
	internal sealed class SpriteG : ASpriteSingle
	{
		public SpriteG(string name, GraphicsImage gra, Rectangle rect)
			: base(name, gra, rect)
		{
		}

	}

	/// <summary>
	/// ConstImage(csvから作るファイル占有型ベースイメージ)をもとにしたSprite
	/// </summary>
	internal sealed class SpriteF : ASpriteSingle
	{
		public SpriteF(string name, ConstImage image, Rectangle rect, Point pos)
			: base(name, image, rect)
		{
			this.DestBasePosition = pos;
		}
	}

	/// <summary>
	/// AnimeするSprite。中身はほぼSprite
	/// </summary>
	internal sealed class SpriteAnime : ASprite
	{
		public SpriteAnime(string name, Size size)
			: base(name, size)
		{
			FrameList = new List<AnimeFrame>();
			totaltime = 0;
		}
		private sealed class AnimeFrame :IDisposable
		{
			public int index;
			public AbstractImage BaseImage;
			public Rectangle SrcRectangle;
			public Point Offset;
			public int DelayTimeMs;
			public void Normalize(Size parentSize)
			{
				Rectangle rect = Rectangle.Intersect(new Rectangle(Offset, SrcRectangle.Size), new Rectangle(new Point(), parentSize));
				if (rect.IsEmpty)
				{
					BaseImage = null;
					return;
				}
				Offset.X = rect.X;
				Offset.Y = rect.Y;
				SrcRectangle.Width = rect.Width;
				SrcRectangle.Height = rect.Height;
			}
			public void Dispose()
			{
				BaseImage = null;
			}
		}
		List<AnimeFrame> FrameList;
		public Int64 totaltime;

		internal bool AddFrame(AbstractImage parentImage, Rectangle rect, Point pos, int delay)
		{
			AnimeFrame frame = new AnimeFrame();
			frame.index = FrameList.Count;
			frame.BaseImage = parentImage;
			frame.SrcRectangle = rect;
			frame.Offset = pos;
			if(delay <= 0)
				delay = 1;
			frame.DelayTimeMs = delay;
			frame.Normalize(DestBaseSize);
			totaltime += delay;
			FrameList.Add(frame);
			return true;
		}
		
		/// <summary>
		/// アニメの経過時間を削除して最初からやり直す
		/// </summary>
		internal void ResetTime()
		{
			StartTime = -1;
			lastFrameTime = 0;
			lastFrame = -1;
		}

		/// <summary>
		/// 開始時間調整用の値。ミリ秒でUInt32の範囲まで想定。
		/// </summary>
		Int64 StartTime = -1;
		uint lastFrameTime = 0;
		int lastFrame = -1;
		private AnimeFrame GetCurrentFrame()
		{
			if (totaltime <= 0)
				return null;
#if DEBUG
			if (FrameList.Count == 0)
				throw new ExeEE("totaltime > 0なのにFrameListが空");
			if (lastFrame >= FrameList.Count)
				throw new ExeEE("SpriteAnime:最終フレームが範囲外");
#endif
			//一度もフレーム取得したことがない場合は現在時間を記録して最初のフレームを返す。
			if (StartTime < 0)
			{
				StartTime = MinorShift._Library.WinmmTimer.CurrentFrameTime;
				lastFrame = 0;
				return FrameList[0];
			}
			//時間経過なしに複数回呼ばれた場合はさっき返したフレームをもう一度返す。
			if (MinorShift._Library.WinmmTimer.CurrentFrameTime == lastFrameTime && lastFrame >= 0)
				return FrameList[lastFrame];
			//StartTimeからの経過時間をtotaltimeで剰余計算
			Int64 time = (MinorShift._Library.WinmmTimer.CurrentFrameTime - StartTime) % totaltime;
			//winmmtimerは一周して0になることがあり得るのでその場合の対策。C#の剰余の結果の符号は左辺値の符号に等しい。
			if (time < 0)
				time += totaltime;
			foreach(AnimeFrame frame in FrameList)
			{
				time -=frame.DelayTimeMs;
				if (time <= 0)
				{
					lastFrame = frame.index;
					return frame;
				}
			}
			//ここまでこないはず
			throw new ExeEE("SpriteAnime:時間外参照");
		}

		public override bool IsCreated
		{
			get { return true; }
		}

        public override Bitmap Bitmap
        {
            get
            {
                return GetCurrentFrame().BaseImage.Bitmap;
            }
        }

        public override void Dispose()
		{
			foreach (var frame in FrameList)
				frame.Dispose();
			FrameList.Clear();
			totaltime = 0;
			lastFrameTime = 0;
			StartTime = -1;
			lastFrame = -1;
		}


		public override Color SpriteGetColor(int x, int y)
		{
			throw new NotSupportedException();
			//Bitmap bmp = this.Bitmap;
			//if (bmp == null)
			//	return Color.Transparent;
			//int bmpX = x + SrcRectangle.X;
			//int bmpY = y + SrcRectangle.Y;
			//if (bmpX < 0 || bmpX >= bmp.Width || bmpY < 0 || bmpY >= bmp.Height)
			//	return Color.Transparent;

			//return bmp.GetPixel(bmpX, bmpY);
		}


		public override void GraphicsDraw(Graphics g, Point offset)
		{
			AnimeFrame frame = GetCurrentFrame();
			if (frame == null || frame.BaseImage == null || !frame.BaseImage.IsCreated)
				return;
			offset.Offset(DestBasePosition);
			offset.Offset(frame.Offset);
			Rectangle destRect = new Rectangle(offset, frame.SrcRectangle.Size);
			g.DrawImage(frame.BaseImage.Bitmap, destRect, frame.SrcRectangle, GraphicsUnit.Pixel);
			return;
		}

		public override void GraphicsDraw(Graphics g, Rectangle destRect)
		{
			AnimeFrame frame = GetCurrentFrame();
			if (frame == null || frame.BaseImage == null || !frame.BaseImage.IsCreated)
				return;
			destRect.X = destRect.X + (DestBasePosition.X + frame.Offset.X) * destRect.Width / DestBaseSize.Width;
			destRect.Y = destRect.Y + (DestBasePosition.Y + frame.Offset.Y) * destRect.Height / DestBaseSize.Height;
			destRect.Width = frame.SrcRectangle.Width * destRect.Width / DestBaseSize.Width;
			destRect.Height = frame.SrcRectangle.Height * destRect.Height / DestBaseSize.Height;
			g.DrawImage(frame.BaseImage.Bitmap, destRect, frame.SrcRectangle, GraphicsUnit.Pixel);
		}

		public override void GraphicsDraw(Graphics g, Rectangle destRect, ImageAttributes attr)
		{
			AnimeFrame frame = GetCurrentFrame();
			if (frame == null || frame.BaseImage == null || !frame.BaseImage.IsCreated)
				return;
			destRect.X = destRect.X + (DestBasePosition.X + frame.Offset.X) * destRect.Width / DestBaseSize.Width;
			destRect.Y = destRect.Y + (DestBasePosition.Y + frame.Offset.Y) * destRect.Height / DestBaseSize.Height;
			destRect.Width = frame.SrcRectangle.Width * destRect.Width / DestBaseSize.Width;
			destRect.Height = frame.SrcRectangle.Height * destRect.Height / DestBaseSize.Height;
			//g.DrawImage(frame.BaseImage.Bitmap, destRect, SrcRectangle, GraphicsUnit.Pixel, attr);←このパターンがない
			g.DrawImage(frame.BaseImage.Bitmap, destRect, frame.SrcRectangle.X, frame.SrcRectangle.Y, frame.SrcRectangle.Width, frame.SrcRectangle.Height, GraphicsUnit.Pixel, attr);
		}

	}
}
