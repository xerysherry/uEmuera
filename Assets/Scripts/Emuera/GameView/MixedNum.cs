using System;

namespace MinorShift.Emuera.GameView
{
	internal sealed class MixedNum
	{
		public int num = 0;

		public bool isPx = false;

		public static implicit operator MixedNum(int i)
        {
			MixedNum m = new MixedNum();
			m.num = i;
			m.isPx = false;
			return m;
		}
	}
}
