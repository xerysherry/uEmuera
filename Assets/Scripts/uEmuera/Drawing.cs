using System;

namespace uEmuera.Drawing
{
    public class Bitmap : IDisposable
    {
        public Bitmap(string path)
        {
            this.path = path;
            this.filename = GenericUtils.GetFilename(path);
        }

        public readonly string path;
        public readonly string filename;
        public string name;
        public Size size;

        public void Dispose()
        { }
        public int Width
        {
            get { return size.Width; }
        }
        public int Height
        {
            get { return size.Height; }
        }
        public Size Size { get { return size; } }
        public Color GetPixel(int x, int y)
        {
            var ti = SpriteManager.GetTextureInfo(name, path);
            var uc = ti.texture.GetPixel(x, y);
            return new Color(uc.r, uc.g, uc.b, uc.a);
        }
        public void SetPixel(Color c, int x, int y)
        {
            var ti = SpriteManager.GetTextureInfo(name, path);
            ti.texture.SetPixel(x, y, new UnityEngine.Color(c.r, c.g, c.b, c.a));
        }
        public void Save(string path)
        {
            var ti = SpriteManager.GetTextureInfo(name, path);
            var data = UnityEngine.ImageConversion.EncodeToPNG(ti.texture);
            System.IO.File.WriteAllBytes(path, data);
        }
    }

    public class BitmapTexture : Bitmap
    {
        public BitmapTexture(string path)
            :base(path)
        {
            var name = string.Concat(":FILE:", filename);
            var tiot = SpriteManager.GetTextureInfoOtherThread(name, path,
                ret =>
                {
                    textureinfo = ret;
                    if(textureinfo == null)
                        return;
                    size.Width = ret.texture.width;
                    size.Height = ret.texture.height;
                });
            while(tiot.mutex == null)
                System.Threading.Thread.Sleep(10);
            tiot.mutex.WaitOne();

            if(textureinfo == null)
                return;
            tiot.mutex.ReleaseMutex();
            tiot.mutex.Close();
        }
        public UnityEngine.Texture2D texture
        {
            get { return textureinfo.texture; }
        }
        SpriteManager.TextureInfo textureinfo = null;
    }

    public class BitmapRenderTexture : Bitmap
    {
        public BitmapRenderTexture(int x, int y)
            :base(null)
        {
            //var rtot = SpriteManager.GetRenderTextureOtherThread(x, y,
            //    ret =>
            //    {
            //        rt = ret;
            //        size.Width = ret.width;
            //        size.Height = ret.height;
            //    });

            size.Width = x;
            size.Height = y;
        }
        //UnityEngine.RenderTexture rt = null;
    }

    public enum GraphicsUnit
    {
        World = 0,
        Display = 1,
        Pixel = 2,
        Point = 3,
        Inch = 4,
        Document = 5,
        Millimeter = 6
    }

    public sealed class Graphics
    {
        public static Graphics instance
        {
            get
            {
                if(instance_ == null)
                    instance_ = new Graphics();
                return instance_;
            }
        }
        static Graphics instance_ = null;

        private Graphics() { }

        public void Clear() { }
        public void DrawImage(Bitmap texture, Rectangle destrect,
                            Rectangle srcrect, GraphicsUnit unit)
        {
            uEmuera.Logger.Info("Graphics.DrawImage " + texture.name);
        }
        public void DrawImage(Bitmap texture, Rectangle destrect,
                            int x, int y, int w, int h, GraphicsUnit unit, ImageAttributes ia)
        {
            uEmuera.Logger.Info("Graphics.DrawImage " + texture.name);
        }
        public void DrawString(string s, Font font, Brush brush, Point point)
        {
            uEmuera.Logger.Info("Graphics.DrawString " + s);
        }
        public void FillRectangel(SolidBrush brush, Rectangle rect)
        { }
        public void Clear(Color color)
        {
            uEmuera.Logger.Info("Graphics.Clear " + color.ToArgb());
        }
    }

    public class Brush
    { }

    public sealed class SolidBrush : Brush
    {
        public SolidBrush(Color color)
        {
            Color = color;
        }

        public Color Color { get; set; }
    }

    public sealed class Pen
    {
        public Pen()
        { }
        public Pen(Color c, Int64 width)
        { }
    }

    public enum FontStyle
    {
        Regular = 0,
        Bold = 1,
        Italic = 2,
        Underline = 4,
        Strikeout = 8
    }

    public class FontFamily
    {
        public FontFamily(string name)
        {
            this.name = name;
        }
        public string Name { get { return name; } }
        string name;
    }

    public sealed class Font : IDisposable
    {
        static bool GetMonospaced(string name)
        {
            return !monospaced_disable_set.Contains(name);
        }
        static readonly System.Collections.Generic.HashSet<string> monospaced_disable_set = 
            new System.Collections.Generic.HashSet<string>
        {
            "ＭＳ Ｐゴシック",
            "MS PGothic",
        };

        public Font(string familyName, float emSize, FontStyle style, 
            GraphicsUnit unit)
        {
            fontFamily = new FontFamily(familyName);
            monospaced = GetMonospaced(familyName);
            size = emSize;
            fontStyle = style;
            graphicsUnit = unit;
        }
        public Font(string familyName, float emSize, FontStyle style, 
            GraphicsUnit unit, byte gdiCharSet)
        {
            fontFamily = new FontFamily(familyName);
            monospaced = GetMonospaced(familyName);
            size = emSize;
            fontStyle = style;
            graphicsUnit = unit;
        }
        public Font(string familyName, float emSize, FontStyle style,
            GraphicsUnit unit, byte gdiCharSet, bool gdiVericalFont)
        {
            fontFamily = new FontFamily(familyName);
            monospaced = GetMonospaced(familyName);
            size = emSize;
            fontStyle = style;
            graphicsUnit = unit;
        }

        public void Dispose()
        { }

        public FontFamily FontFamily { get { return fontFamily; } }
        FontFamily fontFamily;

        public bool Monospaced { get { return monospaced; } }
        bool monospaced = true;

        public float Size { get { return size; } }
        float size;

        public FontStyle Style { get { return fontStyle; } }
        FontStyle fontStyle;

        public bool Bold { get { return (fontStyle & FontStyle.Bold) > 0; } }
        public bool Italic { get { return (fontStyle & FontStyle.Italic) > 0; } }
        public bool Underline { get { return (fontStyle & FontStyle.Underline) > 0; } }
        public bool Strikeout { get { return (fontStyle & FontStyle.Underline) > 0; } }

        public GraphicsUnit Unit { get { return graphicsUnit; } }
        GraphicsUnit graphicsUnit;
    }

    public struct Color
    {
        public static Color FromArgb(int argb)
        {
            return FromArgb(
                    (argb >> 24),
                    ((argb >> 16) & 0xFF),
                    ((argb >> 8) & 0xFF),
                    (argb & 0xFF));
        }
        //public static Color FromArgb(int alpha, Color baseColor);
        public static Color FromArgb(int red, int green, int blue)
        {
            return FromArgb(255, red, green, blue);
        }
        public static Color FromArgb(int alpha, int red, int green, int blue)
        {
            return new Color
            {
                a = alpha / 255.0f,
                r = red / 255.0f,
                g = green / 255.0f,
                b = blue / 255.0f,
            };
        }

        public Color(int R, int G, int B)
        {
            r = R / 255.0f;
            g = G / 255.0f;
            b = B / 255.0f;
            a = 1.0f;
        }
        public Color(int R, int G, int B, int A)
        {
            r = R / 255.0f;
            g = G / 255.0f;
            b = B / 255.0f;
            a = A / 255.0f;
        }
        public Color(float R, float G, float B, float A)
        {
            r = R;
            g = G;
            b = B;
            a = A;
        }

        //public Color(uColor c) { a = c.a; r = c.r; g = c.g; b = c.b; }
        public int R { get { return (int)(r * 255); } }
        public int G { get { return (int)(g * 255); } }
        public int B { get { return (int)(b * 255); } }
        public int A { get { return (int)(a * 255); } }
        public int ToArgb()
        {
            return (A << 24) + (R << 16) + (G << 8) + B;
        }
        public int ToRGBA()
        {
            return (R << 24) + (G << 16) + (B << 8) + A;
        }

        public float a;
        public float r;
        public float g;
        public float b;

        public static readonly Color Black = new Color(0, 0, 0);
        public static readonly Color White = new Color(255, 255, 255);
        public static readonly Color Blue = new Color(0, 0, 255);
        public static readonly Color Red = new Color(255, 0, 0);
        public static readonly Color Green = new Color(0, 255, 0);
        public static readonly Color Grey = new Color(128, 128, 128);
        public static readonly Color Gray = Grey;
        public static readonly Color Transparent = new Color(0, 0, 0, 0);

        //public static Color Clear { get { return new Color(uColor.clear); } }
        //public static Color Cyan { get { return new Color(uColor.cyan); } }
        //public static Color Magenta { get { return new Color(uColor.magenta); } }
        //public static Color Yellow { get { return new Color(uColor.yellow); } }
        public static Color FromName(string name)
        {
            switch(name)
            {
            case "Black":
                return Black;
            case "Blue":
                return Blue;
        //    case "Clear":
        //        return Clear;
        //    case "Cyan":
        //        return Cyan;
            case "Gray":
                return Gray;
            case "Green":
                return Green;
            case "Grey":
                return Grey;
        //    case "Magenta":
        //        return Magenta;
            case "Red":
                return Red;
            case "White":
                return White;
        //    case "Yellow":
        //        return Yellow;
            }
            uEmuera.Logger.Info("Not Match Color '" + name + "'");
            return Black;
        }

        //public uColor ucolor { get { return new uColor(r, g, b, a); } }

        public static bool operator ==(Color left, Color right)
        {
            return  left.A == right.A &&
                    left.R == right.R &&
                    left.G == right.G &&
                    left.B == right.B;
        }
        public static bool operator !=(Color left, Color right)
        {
            return  left.A != right.A ||
                    left.R != right.R ||
                    left.G != right.G ||
                    left.B != right.B;
        }
        public override bool Equals(object obj)
        {
            if(!(obj is Color))
                return false;
            return ((Color)obj) == this;
        }
        public override int GetHashCode()
        {
            return 0;
        }
    }

    public struct Point
    {

        public static readonly Point Empty = new Point(0, 0);

        public Point(Size size)
        {
            X = size.Width;
            Y = size.Height;
        }
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
        public int X { get; set; }
        public int Y { get; set; }
        public void Offset(Point pt)
        {
            X += pt.X;
            Y += pt.Y;
        }
        public bool IsEmpty
        {
            get { return X == 0 && Y == 0; }
        }
    }

    public struct Size
    {
        public static readonly Size zero;

        public Size(Point pt)
        {
            Width = pt.X;
            Height = pt.Y;
        }
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsEmpty
        {
            get { return Width == 0 && Height == 0; }
        }
    }

    public struct Rectangle
    {
        public static Rectangle Intersect(Rectangle left, Rectangle right)
        {
            int l = Math.Max(left.Left, right.Left);
            int r = Math.Min(left.Right, right.Right);
            int t = Math.Max(left.Top, right.Top);
            int b = Math.Min(left.Bottom, right.Bottom);
            if(l < r && t < b)
                return new Rectangle(l, t, r - l, b - t);
            else
                return new Rectangle(0, 0, 0, 0);
        }

        public Rectangle(Point location, Size size)
        {
            X = location.X;
            Y = location.Y;
            Width = size.Width;
            Height = size.Height;
        }
        //public Rectangle(Point location, Vector2 size)
        //{
        //    X = location.X;
        //    Y = location.Y;
        //    Width = (int)size.x;
        //    Height = (int)size.y;
        //}
        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public int Top { get { return Y; } }
        public int Bottom { get { return Y + Height; } }
        public int Left { get { return X; } }
        public int Right { get { return X + Width; } }

        public Size Size { get { return new Size(Width, Height); } }
        public bool IsEmpty { get { return Width == 0 && Height == 0; } }

        public bool Contains(Point point)
        {
            return Left <= point.X && point.X < Right &&
                Top <= point.Y && point.Y < Bottom;
        }
        public bool IntersectsWith(Rectangle rect)
        {
            return !(rect.Bottom <= Top ||
                    rect.Top > Bottom ||
                    rect.Right <= Left ||
                    rect.Left > Right);
        }
    }

    public struct RectangleF
    {
        public RectangleF(Point location, Size size)
        {
            X = location.X;
            Y = location.Y;
            Width = size.Width;
            Height = size.Height;
        }
        //public RectangleF(Point location, Vector2 size)
        //{
        //    X = location.X;
        //    Y = location.Y;
        //    Width = (int)size.x;
        //    Height = (int)size.y;
        //}
        public RectangleF(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public float Top { get { return Y; } }
        public float Bottom { get { return Y + Height; } }
        public float Left { get { return X; } }
        public float Right { get { return X + Width; } }
    }

    public class ImageAttributes
    { }

    public enum StringFormatFlags
    {
        DirectionRightToLeft = 1,
        DirectionVertical = 2,
        FitBlackBox = 4,
        DisplayFormatControl = 32,
        NoFontFallback = 1024,
        MeasureTrailingSpaces = 2048,
        NoWrap = 4096,
        LineLimit = 8192,
        NoClip = 16384
    }

    public class StringFormat
    {

    }

    //public class Bitmap
    //{ }

    public struct CharacterRange
    {
        public CharacterRange(int first, int length)
        {
            First = first;
            Length = length;
        }

        public int First { get; set; }
        public int Length { get; set; }

        //public override bool Equals(object obj);
        //public override int GetHashCode();

        //public static bool operator ==(CharacterRange cr1, CharacterRange cr2);
        //public static bool operator !=(CharacterRange cr1, CharacterRange cr2);
    }
}