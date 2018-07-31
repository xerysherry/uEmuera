using System.IO;

namespace MinorShift._Library
{
	public static class Sys
	{
		static Sys()
		{}
        public static void SetWorkFolder(string folder)
        {
            _WorkFolder = folder;
        }
        public static string WorkFolder { get { return _WorkFolder; } }
        private static string _WorkFolder;

        public static void SetSourceFolder(string folder)
        {
            ExeDir = uEmuera.Utils.NormalizePath(_WorkFolder + "/" + folder + "/");
        }
        
		/// <summary>
		/// 実行ファイルのパス
		/// </summary>
		//public static readonly string ExePath;

		/// <summary>
		/// 実行ファイルのディレクトリ。最後に\を付けたstring
		/// </summary>
		public static string ExeDir { get; private set; }

		/// <summary>
		/// 実行ファイルの名前。ディレクトリなし
		/// </summary>
		//public static readonly string ExeName;

		/// <summary>
		/// 2重起動防止。既に同名exeが実行されているならばtrueを返す
		/// </summary>
		/// <returns></returns>
		public static bool PrevInstance()
		{
            //string thisProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            //if (System.Diagnostics.Process.GetProcessesByName(thisProcessName).Length > 1)
            //{
            //	return true;
            //}
            return false;
		}
	}
}

