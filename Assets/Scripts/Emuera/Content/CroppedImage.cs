using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Text;
using uEmuera.Drawing;

namespace MinorShift.Emuera.Content
{
	internal sealed class CroppedImage : AContentItem
	{
		public CroppedImage(string name, BaseImage image, Rectangle rect, bool noresize) : base(name)
		{
			BaseImage = image;
			Rectangle = rect;
			if (image != null)
				this.Enabled = image.Enabled;
			if (rect.Width <= 0 || rect.Height <= 0)
				this.Enabled = false;
			this.NoResize = noresize;
		}
		public readonly BaseImage BaseImage;
		public readonly Rectangle Rectangle;
		public readonly bool NoResize;
	}
}
