using MinorShift._Library;
using MinorShift.Emuera.Content;
using System;
using System.Collections.Generic;
//using System.Drawing;
//using System.Drawing.Imaging;
using System.Text;
using uEmuera.Drawing;
using uEmuera.Forms;

namespace MinorShift.Emuera.GameView
{
	class ConsoleImagePart : AConsoleDisplayPart
	{
		public ConsoleImagePart(string resName, string resNameb, int raw_height, int raw_width, int raw_ypos)
		{
			top = 0;
			bottom = Config.FontSize;
			Str = "";
			ResourceName = resName ?? "";
			ButtonResourceName = resNameb;

            cImage = Content.AppContents.GetContent<CroppedImage>(ResourceName);
            if(cImage != null && !cImage.Enabled)
                cImage = null;
#if !UNITY_EDITOR
            if(cImage == null)
            {
#endif
                StringBuilder sb = new StringBuilder();
                sb.Append("<img src='");
                sb.Append(ResourceName);
                if(ButtonResourceName != null)
                {
                    sb.Append("' srcb='");
                    sb.Append(ButtonResourceName);
                }
                if(raw_height != 0)
                {
                    sb.Append("' height='");
                    sb.Append(raw_height.ToString());
                }
                if(raw_width != 0)
                {
                    sb.Append("' width='");
                    sb.Append(raw_height.ToString());
                }
                if(raw_ypos != 0)
                {
                    sb.Append("' ypos='");
                    sb.Append(raw_height.ToString());
                }
                sb.Append("'>");
                AltText = sb.ToString();
#if !UNITY_EDITOR
                Str = AltText;
                return;
            }
#else
            if(cImage == null)
            {
                Str = AltText;
                return;
            }
#endif  

            int height = 0;
			if (cImage.NoResize)
			{
				height = cImage.Rectangle.Height;
				Width = cImage.Rectangle.Width;
			}
			else
			{
				
				if (raw_height == 0)
					height = Config.FontSize;
				else
					height = Config.FontSize * raw_height / 100;
				if (raw_width == 0)
				{
					Width = cImage.Rectangle.Width * height / cImage.Rectangle.Height;
					XsubPixel = ((float)cImage.Rectangle.Width * height) / cImage.Rectangle.Height - Width;
				}
				else
				{
					Width = Config.FontSize * raw_width / 100;
					XsubPixel = ((float)Config.FontSize * raw_width / 100f) - Width;
				}
			}
			top = raw_ypos * Config.FontSize / 100;
			destRect = new Rectangle(0, top, Width, height);
			if (destRect.Width < 0)
			{
				destRect.X = -destRect.Width;
				Width = -destRect.Width;
			}
			if (destRect.Height < 0)
			{
				destRect.Y = destRect.Y - destRect.Height;
				height = -destRect.Height;
			}
			bottom = top + height;
			//if(top > 0)
			//	top = 0;
			//if(bottom < Config.FontSize)
			//	bottom = Config.FontSize;
			if (ButtonResourceName != null)
			{
                if(ButtonResourceName == ResourceName)
                    cImageB = cImage;
                else
                {
                    cImageB = Content.AppContents.GetContent<CroppedImage>(ButtonResourceName);
                    if(cImageB != null && !cImageB.Enabled)
                        cImageB = null;
                }
			}
		}

        public CroppedImage cropped_image { get { return cImage; } }
        public Rectangle dest_rect { get { return destRect; } }

		private readonly CroppedImage cImage;
		private readonly CroppedImage cImageB;
		private readonly int top;
		private readonly int bottom;
		private readonly Rectangle destRect;
#pragma warning disable CS0649 // フィールド 'ConsoleImagePart.ia' は割り当てられません。常に既定値 null を使用します。
		private readonly ImageAttributes ia;
#pragma warning restore CS0649 // フィールド 'ConsoleImagePart.ia' は割り当てられません。常に既定値 null を使用します。
		public readonly string ResourceName;
		public readonly string ButtonResourceName;
		public override int Top { get { return top; } }
		public override int Bottom { get { return bottom; } }
		
		public override bool CanDivide { get { return false; } }
		public override void SetWidth(StringMeasure sm, float subPixel)
		{
			if (this.Error)
			{
				Width = 0;
				return;
			}
			if (cImage != null)
				return;
			Width = sm.GetDisplayLength(Str, Config.Font);
			XsubPixel = subPixel;
		}

		public override string ToString()
		{
			if (AltText == null)
				return "";
			return AltText;
		}

		public override void DrawTo(Graphics graph, int pointY, bool isSelecting, bool isBackLog, TextDrawingMode mode)
		{
			//if (this.Error)
			//	return;
			//CroppedImage img = cImage;
			//if (isSelecting && cImageB != null)
			//	img = cImageB;
			//Rectangle rect = destRect;
			////PointX微調整
			//rect.X = destRect.X + PointX + Config.DrawingParam_ShapePositionShift;
			//rect.Y = destRect.Y + pointY;

			//if (img != null)
			//{
			//	if(ia == null)
			//		graph.DrawImage(img.BaseImage.Bitmap, rect, img.Rectangle, GraphicsUnit.Pixel);
			//	else
			//		graph.DrawImage(img.BaseImage.Bitmap, rect, img.Rectangle.X,img.Rectangle.Y,img.Rectangle.Width,img.Rectangle.Height , GraphicsUnit.Pixel,ia);
			//}
			//else
			//{
			//	if (mode == TextDrawingMode.GRAPHICS)
			//		graph.DrawString(Str, Config.Font, new SolidBrush(Config.ForeColor), new Point(PointX, pointY));
			//	else
			//		TextRenderer.DrawText(graph, Str, Config.Font, new Point(PointX, pointY), Config.ForeColor, TextFormatFlags.NoPrefix);
			//}
		}

		public override void GDIDrawTo(int pointY, bool isSelecting, bool isBackLog)
		{
			//if (this.Error)
			//	return;
			//CroppedImage img = cImage;
			//if (isSelecting && cImageB != null)
			//	img = cImageB;
			//if (img != null)
			//	GDI.DrawImage(PointX + destRect.X, pointY+ destRect.Y, Width, destRect.Height, img.BaseImage.GDIhDC, img.Rectangle);
			//else
			//	GDI.TabbedTextOutFull(Config.Font, Config.ForeColor, Str, PointX, pointY);
		}
	}
}
