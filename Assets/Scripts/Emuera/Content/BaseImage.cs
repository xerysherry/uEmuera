using MinorShift._Library;
using System;
using System.Collections.Generic;
using System.Text;
using uEmuera.Drawing;

namespace MinorShift.Emuera.Content
{
	internal sealed class BaseImage : AContentFile
	{
		public BaseImage(string name, string path)
			: base(name, path)
		{}
        //public Bitmap Bitmap;
        //Graphics g;
        //public IntPtr GDIhDC{get;private set;}
        //IntPtr hBitmap;
        //IntPtr hDefaultImg;

        public Bitmap Bitmap { get { return texture; } }
        Bitmap texture;
        public Bitmap GDIhDC { get { return texture; } }

        public void Load(bool useGDI)
		{
			if (Loaded)
				return;

            Enabled = System.IO.File.Exists(Filepath);
            if(Enabled)
            {
                texture = new Bitmap();
                texture.name = Filepath;
            }
            //Enabled = true;
            //try
            //{
            //	Bitmap = new Bitmap(Filepath);
            //	if (useGDI)
            //	{
            //		hBitmap = Bitmap.GetHbitmap();
            //		g = Graphics.FromImage(Bitmap);
            //		GDIhDC = g.GetHdc();
            //		hDefaultImg = GDI.SelectObject(GDIhDC, hBitmap);
            //	}
            //	Loaded = true;
            //	Enabled = true;
            //}
            //catch
            //{
            //	return;
            //}
            //return;

            //todo
		}

		public override void Dispose()
		{
            //if (Bitmap == null)
            //	return;
            //if (g != null)
            //{
            //	GDI.SelectObject(GDIhDC, hDefaultImg);
            //	GDI.DeleteObject(hBitmap);
            //	g.ReleaseHdc(GDIhDC);
            //	g.Dispose();
            //	g = null;
            //}
            //Bitmap.Dispose();
            //Bitmap = null;

            if(texture == null)
                return;
            texture = null;
		}
	}
}
