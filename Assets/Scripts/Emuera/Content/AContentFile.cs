using System;
using System.Collections.Generic;
using System.Text;

namespace MinorShift.Emuera.Content
{
	abstract class AContentFile : IDisposable
	{
		//protected bool Loaded = false;
		//public bool Enabled { get; protected set; }
		public abstract bool IsCreated { get; }

		public abstract void Dispose();
	}
}
