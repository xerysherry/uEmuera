using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MinorShift.Emuera.Sub
{
	internal sealed class EraStreamReader : IDisposable
	{
		public EraStreamReader(bool useRename)
		{
			this.useRename = useRename;
		}

		string filepath;
		string filename;
        readonly bool useRename = false;
		int curNo = 0;
		int nextNo = 0;
		StreamReader reader;
		FileStream stream;

		public bool Open(string path)
		{
			return Open(path, Path.GetFileName(path));
		}

		public bool Open(string path, string name)
		{
			//そんなお行儀の悪いことはしていない
			//if (disposed)
			//    throw new ExeEE("破棄したオブジェクトを再利用しようとした");
			//if ((reader != null) || (stream != null) || (filepath != null))
			//    throw new ExeEE("使用中のオブジェクトを別用途に再利用しようとした");
			filepath = path;
			filename = name;
			nextNo = 0;
			curNo = 0;
			try
			{
				stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				reader = new StreamReader(stream, Config.Encode);
			}
			catch
			{
				this.Dispose();
				return false;
			}
			return true;
		}

		public string ReadLine()
		{
			nextNo++;
			curNo = nextNo;
			return reader.ReadLine();
		}

		/// <summary>
		/// 次の有効な行を読む。LexicalAnalyzer経由でConfigを参照するのでConfig完成までつかわないこと。
		/// </summary>
		public StringStream ReadEnabledLine(bool disabled = false)
		{
			string line = null;
			StringStream st = new StringStream();
			curNo = nextNo;
			while (true)
			{
				line = reader.ReadLine();
				curNo++;
				nextNo++;
				if (line == null)
					return null;
				if (line.Length == 0)
					continue;

				if (useRename && (line.IndexOf("[[") >= 0) && (line.IndexOf("]]") >= 0))
				{
					foreach (KeyValuePair<string, string> pair in ParserMediator.RenameDic)
						line = line.Replace(pair.Key, pair.Value);
				}
                st.Set(line);
				LexicalAnalyzer.SkipWhiteSpace(st);
				if (st.EOS)
					continue;
				//[SKIPSTART]～[SKIPEND]中にここが誤爆するので無効化
				if (!disabled)
				{
					if (st.Current == '}')
						throw new CodeEE("予期しない行連結終端記号'}'が見つかりました", new ScriptPosition(filename, curNo));
					if (st.Current == '{')
					{
						if (line.Trim() != "{")
							throw new CodeEE("行連結始端記号'{'の行に'{'以外の文字を含めることはできません", new ScriptPosition(filename, curNo));
						break;
					}
				}
				return st;
			}
			//curNoはこの後加算しない(始端記号の行を行番号とする)
			StringBuilder b = new StringBuilder();
			while (true)
			{
				line = reader.ReadLine();
				nextNo++;
				if (line == null)
				{
					throw new CodeEE("行連結始端記号'{'が使われましたが終端記号'}'が見つかりません", new ScriptPosition(filename, curNo));
				}

				if (useRename && (line.IndexOf("[[") >= 0) && (line.IndexOf("]]") >= 0))
				{
					foreach (KeyValuePair<string, string> pair in ParserMediator.RenameDic)
						line = line.Replace(pair.Key, pair.Value);
				}
				string test = line.TrimStart();
				if (test.Length > 0)
				{
					if (test[0] == '}')
					{
						if (test.Trim() != "}")
							throw new CodeEE("行連結終端記号'}'の行に'}'以外の文字を含めることはできません", new ScriptPosition(filename, nextNo));
						break;
					}
                    //行連結文字なら1字でないとおかしい、というか、こうしないとFORMの数値変数処理が誤爆する。
                    //{
                    //A}
                    //みたいなどうしようもないコードは知ったこっちゃない
					if (test[0] == '{' && test.Length == 1)
						throw new CodeEE("予期しない行連結始端記号'{'が見つかりました", new ScriptPosition(filename, nextNo));
				}
				b.Append(line);
				b.Append(" ");
			}
			st.Set(b.ToString());
			LexicalAnalyzer.SkipWhiteSpace(st);
			return st;
		}

		/// <summary>
		/// 直前に読んだ行の行番号
		/// </summary>
		public int LineNo
		{ get { return curNo; } }
		public string Filename
		{
			get
			{
				return filename;
			}
		}
		//public string Filepath
		//{
		//    get
		//    {
		//        return filepath;
		//    }
		//}

		public void Close() { this.Dispose(); }
		bool disposed = false;
		#region IDisposable メンバ

		public void Dispose()
		{
			if (disposed)
				return;
			if (reader != null)
				reader.Close();
			else if (stream != null)
				stream.Close();
			filepath = null;
			filename = null;
			reader = null;
			stream = null;
			disposed = true;
		}

		#endregion
	}
}
