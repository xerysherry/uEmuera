using System;
using System.Collections.Generic;
using System.Text;

namespace MinorShift.Emuera.Content
{
	abstract class AContentItem
	{
		protected AContentItem(string name) { Name = name; }
		public readonly string Name;
		public bool Enabled = false;
	}
}
