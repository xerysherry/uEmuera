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

		public ConsoleImagePart(string resName, string resNameb, MixedNum raw_height, MixedNum raw_width, MixedNum raw_ypos)
		{
			top = 0;
			bottom = Config.FontSize;
			Str = "";
			ResourceName = resName ?? "";
			ButtonResourceName = resNameb;

            cImage = AppContents.GetSprite(ResourceName);
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
			if(raw_height.num != 0)
			{
				sb.Append("' height='");
				sb.Append(raw_height.num.ToString());
				if (raw_height.isPx)
					sb.Append("px");
			}
			if(raw_width.num != 0)
			{
				sb.Append("' width='");
				sb.Append(raw_width.num.ToString());
				if (raw_width.isPx)
					sb.Append("px");
			}
			if(raw_ypos.num != 0)
			{
				sb.Append("' ypos='");
				sb.Append(raw_ypos.num.ToString());
				if (raw_ypos.isPx)
					sb.Append("px");
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
			if (raw_height.num == 0)//HTMLで高さが指定されていない又は0が指定された場合、フォントサイズをそのまま高さ(px単位)として使用する。
				height = Config.FontSize;
			else//HTMLで高さが指定された場合、フォントサイズの100分率と解釈する。
				height = raw_height.isPx ? raw_height.num : (Config.FontSize * raw_height.num / 100);
			//幅が指定されていない又は0が指定された場合、元画像の縦横比を維持するように幅(px単位)を設定する。1未満は端数としてXsubpixelに記録。
			//負の値が指定される可能性があるが、最終的なWidthは正の値になるようにあとで調整する。
			if (raw_width.num == 0)
			{
				Width = cImage.DestBaseSize.Width * height / cImage.DestBaseSize.Height;
				XsubPixel = ((float)cImage.DestBaseSize.Width * height) / cImage.DestBaseSize.Height - Width;
			}
			else
			{
				Width = raw_width.isPx ? raw_width.num : (Config.FontSize * raw_width.num / 100);
				if (raw_width.isPx)
					XsubPixel = 0;
				else
					XsubPixel = ((float)Config.FontSize * raw_width.num / 100f) - Width;
			}

			top = raw_ypos.isPx ? raw_ypos.num : (raw_ypos.num * Config.FontSize / 100);
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
                    cImageB = AppContents.GetSprite(ButtonResourceName);
                    if(cImageB != null)
                        cImageB = null;
                }
			}
		}

        public ASprite Image { get { return cImage; } }
        public ASprite ImageBackground { get { return cImageB; } }
        public Rectangle rect { get { return cImage.Rectangle; } }
        public Rectangle dest_rect { get { return destRect; } }

		private readonly ASprite cImage;
		private readonly ASprite cImageB;
		private readonly int top;
		private readonly int bottom;
		private readonly Rectangle destRect;
//#pragma warning disable CS0649 // フィールド 'ConsoleImagePart.ia' は割り当てられません。常に既定値 null を使用します。
//		private readonly ImageAttributes ia;
//#pragma warning restore CS0649 // フィールド 'ConsoleImagePart.ia' は割り当てられません。常に既定値 null を使用します。
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
			//ASprite img = cImage;
			//if (isSelecting && cImageB != null)
			//	img = cImageB;
            //
			//if (img != null && img.IsCreated)
			//{
			//	Rectangle rect = destRect;
			//	//PointX微調整
			//	rect.X = destRect.X + PointX + Config.DrawingParam_ShapePositionShift;
			//	rect.Y = destRect.Y + pointY;
			//	img.GraphicsDraw(graph, rect);
			//}
			//else
			//{
			//	if (mode == TextDrawingMode.GRAPHICS)
			//		graph.DrawString(AltText, Config.Font, new SolidBrush(Config.ForeColor), new Point(PointX, pointY));
			//	else
			//		System.Windows.Forms.TextRenderer.DrawText(graph, AltText, Config.Font, new Point(PointX, pointY), Config.ForeColor, System.Windows.Forms.TextFormatFlags.NoPrefix);
			//}
		}

		public override void GDIDrawTo(int pointY, bool isSelecting, bool isBackLog)
		{
			//if (this.Error)
			//	return;
			//SpriteF img = cImage as SpriteF;//Graphicsから作成したImageはGDI対象外
			//if (isSelecting && cImageB != null)
			//	img = cImageB as SpriteF;
			//if (img != null && img.IsCreated)
			//{
			//	int x = PointX + destRect.X;
			//	int y = pointY + destRect.Y;
			//	if (!img.DestBasePosition.IsEmpty)
			//	{
			//		x = x + img.DestBasePosition.X * destRect.Width / img.SrcRectangle.Width;
			//		y = y + img.DestBasePosition.Y * destRect.Height / img.SrcRectangle.Height;
			//	}
			//	GDI.DrawImage(x, y, Width, destRect.Height, img.BaseImage.GDIhDC, img.SrcRectangle);
			//}
			//else
			//	GDI.TabbedTextOutFull(Config.Font, Config.ForeColor, AltText, PointX, pointY);
		}
	}
}
