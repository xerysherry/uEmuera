using System;
using System.Runtime.InteropServices;

namespace MinorShift._Library
{
	internal sealed class WinInput
	{
		//[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static short GetKeyState(int nVirtKey)
        {
            return 0;
        }
	}

    public enum MouseButtons
    {
        None = 0,
        Left = 1048576,
        Right = 2097152,
        Middle = 4194304,
        XButton1 = 8388608,
        XButton2 = 16777216
    }
}