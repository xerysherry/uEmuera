using System;
using System.Collections.Generic;
using MinorShift._Library;
using uEmuera.Drawing;

namespace uEmuera.Forms
{
    public enum FormWindowState
    {
        Normal = 0,
        Minimized = 1,
        Maximized = 2
    }

    public class Timer : IDisposable
    {
        public static void Update()
        {
            var curr_tick = WinmmTimer.TickCount;
            var iter = timers.GetEnumerator();
            while(iter.MoveNext())
            {
                var timer = iter.Current;
                if(curr_tick - timer.last_tick < timer.Interval)
                    continue;
                timer.last_tick = curr_tick;

                if(!timer.Enabled)
                    continue;
                timer.Tick(timer, EventArgs.Empty);
            }
        }
        static HashSet<Timer> timers = new HashSet<Timer>();

        public Timer()
        {
            timers.Add(this);
        }

        public bool Enabled { get; set; }
        public int Interval { get; set; }
        public object Tag { get; set; }

        public event EventHandler Tick;
        public uint last_tick = 0;

        public void Start()
        {}
        public void Stop()
        {}
        public void Dispose()
        {
            timers.Remove(this);
        }
    }

    public enum TextFormatFlags
    {
        Default = 0,
        Left = 0,
        Top = 0,
        GlyphOverhangPadding = 0,
        HorizontalCenter = 1,
        Right = 2,
        VerticalCenter = 4,
        Bottom = 8,
        WordBreak = 16,
        SingleLine = 32,
        ExpandTabs = 64,
        NoClipping = 256,
        ExternalLeading = 512,
        NoPrefix = 2048,
        Internal = 4096,
        TextBoxControl = 8192,
        PathEllipsis = 16384,
        EndEllipsis = 32768,
        ModifyString = 65536,
        RightToLeft = 131072,
        WordEllipsis = 262144,
        NoFullWidthCharacterBreak = 524288,
        HidePrefix = 1048576,
        PrefixOnly = 2097152,
        PreserveGraphicsClipping = 16777216,
        PreserveGraphicsTranslateTransform = 33554432,
        NoPadding = 268435456,
        LeftAndRightPadding = 536870912
    }

    public static class TextRenderer
    {
        public static void DrawText(uEmuera.Drawing.Graphics graph, string Str, uEmuera.Drawing.Font font,
                            uEmuera.Drawing.Point pt, uEmuera.Drawing.Color color, TextFormatFlags flags)
        { }
    }

    public enum DialogResult
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Abort = 3,
        Retry = 4,
        Ignore = 5,
        Yes = 6,
        No = 7
    }

    public enum MessageBoxButtons
    {
        OK = 0,
        OKCancel = 1,
        AbortRetryIgnore = 2,
        YesNoCancel = 3,
        YesNo = 4,
        RetryCancel = 5
    }

    public static class MessageBox
    {
        public static DialogResult Show(string text)
        {
            return Show(text, "提示");
        }
        public static DialogResult Show(string text, string caption)
        {
            return Show(text, caption, MessageBoxButtons.OK);
        }
        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
        {
            //todo
            uEmuera.Logger.Info(text);
            return DialogResult.None;
        }
    }

    public class ScrollBar
    {
        public int Value { get; set; }
        public int Maximum { get; set; }
        public int Minimum { get; set; }
        public bool Enabled { get; set; }
    }

    public static class Control
    {
        public static Point MousePosition { get; set; }
    }

    public sealed class PictureBox
    {
        public Point PointToClient(object mousePosition)
        {
            //throw new NotImplementedException();
            return Point.Empty;
        }
        public Rectangle ClientRectangle;
        public int Width { get { return ClientRectangle.Width; } }
        public int Height { get { return ClientRectangle.Height; } }
    }

    public sealed class ToolTip
    {
        internal void RemoveAll()
        {
            throw new NotImplementedException();
        }

        internal void SetToolTip(PictureBox mainPicBox, string title)
        {
            throw new NotImplementedException();
        }

        public uEmuera.Drawing.Color ForeColor;
        public uEmuera.Drawing.Color BackColor;
        public int InitialDelay = 0;
    }

    public sealed class TextBox
    {
        public string Text { get; set; }
        public uEmuera.Drawing.Color ForeColor;
        public uEmuera.Drawing.Color BackColor;
    }
}