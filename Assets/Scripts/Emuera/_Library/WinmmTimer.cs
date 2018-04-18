//using System;
//using System.Runtime.InteropServices;
//using UnityEngine;

namespace MinorShift._Library
{
	/// <summary>
	/// wrapされたtimer。外からは、このTickCountだけを呼び出す。
	/// </summary>
	internal sealed class WinmmTimer
	{
		static WinmmTimer()
		{
			instance = new WinmmTimer();
		}
		private WinmmTimer()
		{
			//mm_BeginPeriod(1);
		}
		//~WinmmTimer()
		//{
		//	mm_EndPeriod(1);
		//}

		/// <summary>
		/// 起動時にBeginPeriod、終了時にEndPeriodを呼び出すためだけのインスタンス。
		/// staticなデストラクタがあればいらないんだけど
		/// </summary>
		private static volatile WinmmTimer instance;

		public static uint TickCount
        {
            get
            {
                return (uint)(System.DateTime.Now.Ticks / 10000);
            }
        }

        //[DllImport("winmm.dll", EntryPoint = "timeGetTime")]
        //private static extern uint mm_GetTime();
        //[DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        //private static extern uint mm_BeginPeriod(uint uMilliseconds);
        //[DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        //private static extern uint mm_EndPeriod(uint uMilliseconds);
    }
}
