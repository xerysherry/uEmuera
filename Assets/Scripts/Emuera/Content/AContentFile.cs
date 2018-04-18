using System;
using System.Collections.Generic;
using System.Text;

namespace MinorShift.Emuera.Content
{
	abstract class AContentFile : IDisposable
	{
		public AContentFile(string name, string path)
		{
			Name = name;
			Filepath = path;
		}
		public readonly string Name;
		public readonly string Filepath;
		protected bool Loaded = false;
		public bool Enabled { get; protected set; }

		public abstract void Dispose();
	}
}
