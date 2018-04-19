using MinorShift.Emuera.Sub;
using System;
using System.Collections.Generic;
//using System.Drawing;
using System.IO;
using System.Text;
using uEmuera.Drawing;

namespace MinorShift.Emuera.Content
{
	static class AppContents
	{

		static public T GetContent<T>(string name)where T :AContentItem
		{
			if (name == null)
				return null;
			name = name.ToUpper();
			if (!itemDic.ContainsKey(name))
				return null;
			return itemDic[name] as T;
		}

		static public void LoadContents()
		{
			if (!Directory.Exists(Program.ContentDir))
				return;
			try
			{
				List<string> bmpfilelist = new List<string>();
				bmpfilelist.AddRange(Directory.GetFiles(Program.ContentDir, "*.png", SearchOption.TopDirectoryOnly));
				bmpfilelist.AddRange(Directory.GetFiles(Program.ContentDir, "*.bmp", SearchOption.TopDirectoryOnly));
				bmpfilelist.AddRange(Directory.GetFiles(Program.ContentDir, "*.jpg", SearchOption.TopDirectoryOnly));
				bmpfilelist.AddRange(Directory.GetFiles(Program.ContentDir, "*.gif", SearchOption.TopDirectoryOnly));
#if(UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                bmpfilelist.AddRange(Directory.GetFiles(Program.ContentDir, "*.PNG", SearchOption.TopDirectoryOnly));
				bmpfilelist.AddRange(Directory.GetFiles(Program.ContentDir, "*.BMP", SearchOption.TopDirectoryOnly));
				bmpfilelist.AddRange(Directory.GetFiles(Program.ContentDir, "*.JPG", SearchOption.TopDirectoryOnly));
				bmpfilelist.AddRange(Directory.GetFiles(Program.ContentDir, "*.GIF", SearchOption.TopDirectoryOnly));
#endif
                foreach(var filename in bmpfilelist)
				{//リスト化のみ。Loadはまだ
					string name = Path.GetFileName(filename).ToUpper();
					resourceDic.Add(name, new BaseImage(name, filename));
				}
				List<string> csvFiles = new List<string>(Directory.GetFiles(Program.ContentDir, "*.csv", SearchOption.TopDirectoryOnly));
#if(UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                csvFiles.AddRange(Directory.GetFiles(Program.ContentDir, "*.CSV", SearchOption.TopDirectoryOnly));
#endif
                foreach(var filename in csvFiles)
				{
					string[] lines = File.ReadAllLines(filename, Config.Encode);
					foreach (var line in lines)
					{
						if (line.Length == 0)
							continue;
						string str = line.Trim();
						if (str.Length == 0 || str.StartsWith(";"))
							continue;
						string[] tokens = str.Split(',');
						AContentItem item = CreateFromCsv(tokens);
						if (item != null && !itemDic.ContainsKey(item.Name))
							itemDic.Add(item.Name, item);
					}
				}
			}
			catch
			{
				throw new CodeEE("リソースファイルのロード中にエラーが発生しました");
			}
		}

		static public void UnloadContents()
		{
			foreach (var img in resourceDic.Values)
				img.Dispose();
			resourceDic.Clear();
			itemDic.Clear();
		}

		static private AContentItem CreateFromCsv(string[] tokens)
		{
			if(tokens.Length < 2)
				return null;
			string name = tokens[0].Trim().ToUpper();
			string parentName = tokens[1].ToUpper();
			if (name.Length == 0 || parentName.Length == 0)
				return null;
			if (!resourceDic.ContainsKey(parentName))
				return null;
			AContentFile parent = resourceDic[parentName];
			if(parent is BaseImage)
			{
				BaseImage parentImage = parent as BaseImage;
				parentImage.Load(Config.TextDrawingMode == TextDrawingMode.WINAPI);
				if (!parentImage.Enabled)
						return null;
				Rectangle rect = new Rectangle(new Point(0, 0), parentImage.Bitmap.size);
				bool noresize = false;
				if(tokens.Length >= 6)
				{
					int[] rectValue = new int[4];
					bool sccs = true;
					for (int i = 0; i < 4; i++)
						sccs &= int.TryParse(tokens[i + 2], out rectValue[i]);
					if (sccs)
						rect = new Rectangle(rectValue[0], rectValue[1], rectValue[2], rectValue[3]);
					if(tokens.Length >= 7)
					{
						string[] keywordTokens = tokens[6].Split('|');
						foreach(string keyword in keywordTokens)
						{
							switch(keyword.Trim().ToUpper())
							{
								case "NORESIZE":
									throw new NotImplCodeEE();
									noresize = true;
									break;
							}
						}
					}
				}
				CroppedImage image = new CroppedImage(name, parentImage, rect, noresize);
				return image;
			}
			return null;
		}


		static Dictionary<string, AContentFile> resourceDic = new Dictionary<string, AContentFile>();
		static Dictionary<string, AContentItem> itemDic = new Dictionary<string, AContentItem>();

	}
}
