using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MinorShift.Emuera.Sub
{

	//難読化用属性。enum.ToString()やenum.Parse()を行うなら(Exclude=true)にすること。
	[global::System.Reflection.Obfuscation(Exclude = false)]
	internal enum EraDataState
	{
		OK = 0,//ロード可能
		FILENOTFOUND = 1,//ファイルが存在せず
		GAME_ERROR = 2,//ゲームが違う
		VIRSION_ERROR = 3,//バージョンが違う
		ETC_ERROR = 4,//その他のエラー

	}

	internal sealed class EraDataResult
	{
		public EraDataState State = EraDataState.OK;
		public string DataMes = "";
	}

	/// <summary>
	/// セーブデータ読み取り
	/// </summary>
	internal sealed class EraDataReader : IDisposable
	{
		//public EraDataReader(string filepath)
		//{
		//    file = new FileStream(filepath, FileMode.Open, FileAccess.Read);
		//    reader = new StreamReader(file, Config.Encode);
		//}
		public EraDataReader(FileStream file)
		{
			this.file = file;
			file.Seek(0, SeekOrigin.Begin);
			reader = new StreamReader(file, Config.Encode);
		}
		FileStream file;
		StreamReader reader;
		public const string FINISHER = "__FINISHED";
		public const string EMU_1700_START = "__EMUERA_STRAT__";
		public const string EMU_1708_START = "__EMUERA_1708_STRAT__";
		public const string EMU_1729_START = "__EMUERA_1729_STRAT__";
		public const string EMU_1803_START = "__EMUERA_1803_STRAT__";
		public const string EMU_1808_START = "__EMUERA_1808_STRAT__";
		public const string EMU_SEPARATOR = "__EMU_SEPARATOR__";
		#region eramaker
		public string ReadString()
		{
			if (reader == null)
				throw new FileEE("無効なストリームです");
			string str = reader.ReadLine();
			if (str == null)
				throw new FileEE("読み取るべき文字列がありません");
			return str;
		}

		public Int64 ReadInt64()
		{
			if (reader == null)
				throw new FileEE("無効なストリームです");
            string str = reader.ReadLine();
            if (str == null)
				throw new FileEE("読み取るべき数値がありません");
			if (!Int64.TryParse(str, out long ret))
				throw new FileEE("数値として認識できません");
			return ret;
		}


		public void ReadInt64Array(Int64[] array)
		{
			if (reader == null)
				throw new FileEE("無効なストリームです");
			if (array == null)
				throw new FileEE("無効な配列が渡されました");
			int i = -1;
			string str;
            while (true)
            {
                i++;
                str = reader.ReadLine();
                if (str == null)
                    throw new FileEE("予期しないセーブデータの終端です");
                if (str.Equals(FINISHER, StringComparison.Ordinal))
                    break;
                if (i >= array.Length)//配列を超えて保存されていても動じないで読み飛ばす。
                    continue;
                if (!Int64.TryParse(str, out long integer))
                    throw new FileEE("数値として認識できません");
                array[i] = integer;
            }
            for (; i < array.Length; i++)//保存されている値が無いなら0に初期化
				array[i] = 0;
		}

		public void ReadStringArray(string[] array)
		{
			if (reader == null)
				throw new FileEE("無効なストリームです");
			if (array == null)
				throw new FileEE("無効な配列が渡されました");
			int i = -1;
			string str;
			while (true)
			{
				i++;
				str = reader.ReadLine();
				if (str == null)
					throw new FileEE("予期しないセーブデータの終端です");
				if (str.Equals(FINISHER, StringComparison.Ordinal))
					break;
				if (i >= array.Length)//配列を超えて保存されていても動じないで読み飛ばす。
					continue;
				array[i] = str;
			}
			for (; i < array.Length; i++)//保存されている値が無いなら""に初期化
				array[i] = "";
		}
		#endregion
		#region Emuera
		int emu_version = -1;
		public int DataVersion { get { return emu_version; } }
		public bool SeekEmuStart()
		{

			if (reader == null)
				throw new FileEE("無効なストリームです");
			if (reader.EndOfStream)
				return false;
			while (true)
			{
				string str = reader.ReadLine();
				if (str == null)
					return false;
				if (str.Equals(EMU_1700_START, StringComparison.Ordinal))
				{
					emu_version = 1700;
					return true;
				}
				if (str.Equals(EMU_1708_START, StringComparison.Ordinal))
				{
					emu_version = 1708;
					return true;
				}
				if (str.Equals(EMU_1729_START, StringComparison.Ordinal))
				{
					emu_version = 1729;
					return true;
				}
				if (str.Equals(EMU_1803_START, StringComparison.Ordinal))
				{
					emu_version = 1803;
					return true;
				}
				if (str.Equals(EMU_1808_START, StringComparison.Ordinal))
				{
					emu_version = 1808;
					return true;
				}
			}
		}

		public Dictionary<string, string> ReadStringExtended()
		{
			if (reader == null)
				throw new FileEE("無効なストリームです");
			Dictionary<string, string> strList = new Dictionary<string, string>();
			string str;
			while (true)
			{
				str = reader.ReadLine();
				if (str == null)
					throw new FileEE("予期しないセーブデータの終端です");
				if (str.Equals(FINISHER, StringComparison.Ordinal))
					throw new FileEE("セーブデータの形式が不正です");
				if (str.Equals(EMU_SEPARATOR, StringComparison.Ordinal))
					break;
				int index = str.IndexOf(':');
				if (index < 0)
					throw new FileEE("セーブデータの形式が不正です");
				string key = str.Substring(0, index);
				string value = str.Substring(index + 1, str.Length - index - 1);
				if (!strList.ContainsKey(key))
					strList.Add(key, value);
			}
			return strList;
		}
		public Dictionary<string, Int64> ReadInt64Extended()
		{
			if (reader == null)
				throw new FileEE("無効なストリームです");
			Dictionary<string, Int64> intList = new Dictionary<string, Int64>();
			string str;
			while (true)
			{
				str = reader.ReadLine();
				if (str == null)
					throw new FileEE("予期しないセーブデータの終端です");
				if (str.Equals(FINISHER, StringComparison.Ordinal))
					throw new FileEE("セーブデータの形式が不正です");
				if (str.Equals(EMU_SEPARATOR, StringComparison.Ordinal))
					break;
				int index = str.IndexOf(':');
				if (index < 0)
					throw new FileEE("セーブデータの形式が不正です");
				string key = str.Substring(0, index);
				string valueStr = str.Substring(index + 1, str.Length - index - 1);
                if (!Int64.TryParse(valueStr, out long value))
                    throw new FileEE("数値として認識できません");
                if (!intList.ContainsKey(key))
					intList.Add(key, value);
			}
			return intList;
		}

		public Dictionary<string, List<Int64>> ReadInt64ArrayExtended()
		{
			if (reader == null)
				throw new FileEE("無効なストリームです");
			Dictionary<string, List<Int64>> ret = new Dictionary<string, List<Int64>>();
			string str;
			while (true)
			{
				str = reader.ReadLine();
				if (str == null)
					throw new FileEE("予期しないセーブデータの終端です");
				if (str.Equals(FINISHER, StringComparison.Ordinal))
					throw new FileEE("セーブデータの形式が不正です");
				if (str.Equals(EMU_SEPARATOR, StringComparison.Ordinal))
					break;
				string key = str;
				List<Int64> valueList = new List<Int64>();
				while (true)
				{
					str = reader.ReadLine();
					if (str == null)
						throw new FileEE("予期しないセーブデータの終端です");
					if (str.Equals(EMU_SEPARATOR, StringComparison.Ordinal))
						throw new FileEE("セーブデータの形式が不正です");
					if (str.Equals(FINISHER, StringComparison.Ordinal))
						break;
                    if (!Int64.TryParse(str, out long value))
                        throw new FileEE("数値として認識できません");
                    valueList.Add(value);
				}
				if (!ret.ContainsKey(key))
					ret.Add(key, valueList);
			}
			return ret;
		}

		public Dictionary<string, List<string>> ReadStringArrayExtended()
		{
			if (reader == null)
				throw new FileEE("無効なストリームです");
			Dictionary<string, List<string>> ret = new Dictionary<string, List<string>>();
			string str;
			while (true)
			{
				str = reader.ReadLine();
				if (str == null)
					throw new FileEE("予期しないセーブデータの終端です");
				if (str.Equals(FINISHER, StringComparison.Ordinal))
					throw new FileEE("セーブデータの形式が不正です");
				if (str.Equals(EMU_SEPARATOR, StringComparison.Ordinal))
					break;
				string key = str;
				List<string> valueList = new List<string>();
				while (true)
				{
					str = reader.ReadLine();
					if (str == null)
						throw new FileEE("予期しないセーブデータの終端です");
					if (str.Equals(EMU_SEPARATOR, StringComparison.Ordinal))
						throw new FileEE("セーブデータの形式が不正です");
					if (str.Equals(FINISHER, StringComparison.Ordinal))
						break;
					valueList.Add(str);
				}
				if (!ret.ContainsKey(key))
					ret.Add(key, valueList);
			}
			return ret;
		}

		public Dictionary<string, List<Int64[]>> ReadInt64Array2DExtended()
		{
			if (reader == null)
				throw new FileEE("無効なストリームです");
			Dictionary<string, List<Int64[]>> ret = new Dictionary<string, List<Int64[]>>();
			if (emu_version < 1708)
				return ret;
			string str;
			while (true)
			{
				str = reader.ReadLine();
				if (str == null)
					throw new FileEE("予期しないセーブデータの終端です");
				if (str.Equals(FINISHER, StringComparison.Ordinal))
					throw new FileEE("セーブデータの形式が不正です");
				if (str.Equals(EMU_SEPARATOR, StringComparison.Ordinal))
					break;
				string key = str;
				List<Int64[]> valueList = new List<Int64[]>();
				while (true)
				{
					str = reader.ReadLine();
					if (str == null)
						throw new FileEE("予期しないセーブデータの終端です");
					if (str.Equals(EMU_SEPARATOR, StringComparison.Ordinal))
						throw new FileEE("セーブデータの形式が不正です");
					if (str.Equals(FINISHER, StringComparison.Ordinal))
						break;
					if (str.Length == 0)
					{
						valueList.Add(new Int64[0]);
						continue;
					}
					string[] tokens = str.Split(',');
					Int64[] intTokens = new Int64[tokens.Length];

					for (int x = 0; x < tokens.Length; x++)
						if (!Int64.TryParse(tokens[x], out intTokens[x]))
							throw new FileEE(tokens[x] + "は数値として認識できません");
					valueList.Add(intTokens);
				}
				if (!ret.ContainsKey(key))
					ret.Add(key, valueList);
			}
			return ret;
		}

		public Dictionary<string, List<string[]>> ReadStringArray2DExtended()
		{
			if (reader == null)
				throw new FileEE("無効なストリームです");
			Dictionary<string, List<string[]>> ret = new Dictionary<string, List<string[]>>();
			if (emu_version < 1708)
				return ret;
			string str;
			while (true)
			{
				str = reader.ReadLine();
				if (str == null)
					throw new FileEE("予期しないセーブデータの終端です");
				if (str.Equals(FINISHER, StringComparison.Ordinal))
					throw new FileEE("セーブデータの形式が不正です");
				if (str.Equals(EMU_SEPARATOR, StringComparison.Ordinal))
					break;
				throw new FileEE("StringArray2Dのロードには対応していません");
			}
			return ret;
		}

		public Dictionary<string, List<List<Int64[]>>> ReadInt64Array3DExtended()
		{
			if (reader == null)
				throw new FileEE("無効なストリームです");
			Dictionary<string, List<List<Int64[]>>> ret = new Dictionary<string, List<List<Int64[]>>>();
			if (emu_version < 1729)
				return ret;
			string str;
			while (true)
			{
				str = reader.ReadLine();
				if (str == null)
					throw new FileEE("予期しないセーブデータの終端です");
				if (str.Equals(FINISHER, StringComparison.Ordinal))
					throw new FileEE("セーブデータの形式が不正です");
				if (str.Equals(EMU_SEPARATOR, StringComparison.Ordinal))
					break;
				string key = str;
				List<List<Int64[]>> valueList = new List<List<Int64[]>>();
				while (true)
				{
					str = reader.ReadLine();
					if (str == null)
						throw new FileEE("予期しないセーブデータの終端です");
					if (str.Equals(EMU_SEPARATOR, StringComparison.Ordinal))
						throw new FileEE("セーブデータの形式が不正です");
					if (str.Equals(FINISHER, StringComparison.Ordinal))
						break;
					if (str.Contains("{"))
					{
						List<Int64[]> tokenList = new List<long[]>();
						while (true)
						{
							str = reader.ReadLine();
							if (str == "}")
								break;
							if (str.Length == 0)
							{
								tokenList.Add(new Int64[0]);
								continue;
							}
							string[] tokens = str.Split(',');
							Int64[] intTokens = new Int64[tokens.Length];

							for (int x = 0; x < tokens.Length; x++)
								if (!Int64.TryParse(tokens[x], out intTokens[x]))
									throw new FileEE(tokens[x] + "は数値として認識できません");
							tokenList.Add(intTokens);
						}
						valueList.Add(tokenList);
					}
				}
				if (!ret.ContainsKey(key))
					ret.Add(key, valueList);
			}
			return ret;
		}

		public Dictionary<string, List<List<string[]>>> ReadStringArray3DExtended()
		{
			if (reader == null)
				throw new FileEE("無効なストリームです");
			Dictionary<string, List<List<string[]>>> ret = new Dictionary<string, List<List<string[]>>>();
			if (emu_version < 1729)
				return ret;
			string str;
			while (true)
			{
				str = reader.ReadLine();
				if (str == null)
					throw new FileEE("予期しないセーブデータの終端です");
				if (str.Equals(FINISHER, StringComparison.Ordinal))
					throw new FileEE("セーブデータの形式が不正です");
				if (str.Equals(EMU_SEPARATOR, StringComparison.Ordinal))
					break;
				throw new FileEE("StringArray2Dのロードには対応していません");
			}
			return ret;
		}

		#endregion
		#region IDisposable メンバ

		public void Dispose()
		{
			if (reader != null)
				reader.Close();
			else if (file != null)
				file.Close();
			file = null;
			reader = null;
		}

		#endregion
		public void Close()
		{
			this.Dispose();
		}

	}

	/// <summary>
	/// セーブデータ書き込み
	/// </summary>
	internal sealed class EraDataWriter : IDisposable
	{
		//public EraDataWriter(string filepath)
		//{
		//    FileStream file = new FileStream(filepath, FileMode.Create, FileAccess.Write);
		//    writer = new StreamWriter(file, Config.SaveEncode);
		//    //writer = new StreamWriter(filepath, false, Config.SaveEncode);
		//}
		public EraDataWriter(FileStream file)
		{
			this.file = file;
			writer = new StreamWriter(file, Config.SaveEncode);
		}
		
		public const string FINISHER = EraDataReader.FINISHER;
		public const string EMU_START = EraDataReader.EMU_1808_START;
		public const string EMU_SEPARATOR = EraDataReader.EMU_SEPARATOR;
		FileStream file;
		StreamWriter writer;
		#region eramaker
		public void Write(Int64 integer)
		{
			if (writer == null)
				throw new FileEE("無効なストリームです");
			writer.WriteLine(integer.ToString());
		}


		public void Write(string str)
		{
			if (writer == null)
				throw new FileEE("無効なストリームです");
			if (str == null)
				writer.WriteLine("");
			else
				writer.WriteLine(str);
		}

		public void Write(Int64[] array)
		{
			if (writer == null)
				throw new FileEE("無効なストリームです");
			if (array == null)
				throw new FileEE("無効な配列が渡されました");
			int count = -1;
			for (int i = 0; i < array.Length; i++)
				if (array[i] != 0)
					count = i;
			count++;
			for (int i = 0; i < count; i++)
				writer.WriteLine(array[i].ToString());
			writer.WriteLine(FINISHER);
		}
		public void Write(string[] array)
		{
			if (writer == null)
				throw new FileEE("無効なストリームです");
			if (array == null)
				throw new FileEE("無効な配列が渡されました");
			int count = -1;
			for (int i = 0; i < array.Length; i++)
				if (!string.IsNullOrEmpty(array[i]))
					count = i;
			count++;
			for (int i = 0; i < count; i++)
			{
				if (array[i] == null)
					writer.WriteLine("");
				else
					writer.WriteLine(array[i]);
			}
			writer.WriteLine(FINISHER);
		}
		#endregion
		#region Emuera

		public void EmuStart()
		{
			if (writer == null)
				throw new FileEE("無効なストリームです");
			writer.WriteLine(EMU_START);
		}
		public void EmuSeparete()
		{
			if (writer == null)
				throw new FileEE("無効なストリームです");
			writer.WriteLine(EMU_SEPARATOR);
		}

		public void WriteExtended(string key, Int64 value)
		{
			if (writer == null)
				throw new FileEE("無効なストリームです");
			if (value == 0)
				return;
			writer.WriteLine(string.Format("{0}:{1}", key, value));
		}

		public void WriteExtended(string key, string value)
		{
			if (writer == null)
				throw new FileEE("無効なストリームです");
			if (string.IsNullOrEmpty(value))
				return;
			writer.WriteLine(string.Format("{0}:{1}", key, value));
		}


		public void WriteExtended(string key, Int64[] array)
		{
			if (writer == null)
				throw new FileEE("無効なストリームです");
			if (array == null)
				throw new FileEE("無効な配列が渡されました");
			int count = -1;
			for (int i = 0; i < array.Length; i++)
				if (array[i] != 0)
					count = i;
			count++;
			if (count == 0)
				return;
			writer.WriteLine(key);
			for (int i = 0; i < count; i++)
				writer.WriteLine(array[i].ToString());
			writer.WriteLine(FINISHER);
		}
		public void WriteExtended(string key, string[] array)
		{
			if (writer == null)
				throw new FileEE("無効なストリームです");
			if (array == null)
				throw new FileEE("無効な配列が渡されました");
			int count = -1;
			for (int i = 0; i < array.Length; i++)
				if (!string.IsNullOrEmpty(array[i]))
					count = i;
			count++;
			if (count == 0)
				return;
			writer.WriteLine(key);
			for (int i = 0; i < count; i++)
			{
				if (array[i] == null)
					writer.WriteLine("");
				else
					writer.WriteLine(array[i]);
			}
			writer.WriteLine(FINISHER);
		}

		public void WriteExtended(string key, Int64[,] array2D)
		{
			if (writer == null)
				throw new FileEE("無効なストリームです");
			if (array2D == null)
				throw new FileEE("無効な配列が渡されました");
			int countX = 0;
			int length0 = array2D.GetLength(0);
			int length1 = array2D.GetLength(1);
			int[] countY = new int[length0];
			for (int x = 0; x < length0; x++)
			{
				for (int y = 0; y < length1; y++)
				{
					if (array2D[x, y] != 0)
					{
						countX = x + 1;
						countY[x] = y + 1;
					}
				}
			}
			if (countX == 0)
				return;
			writer.WriteLine(key);
			for (int x = 0; x < countX; x++)
			{
				if (countY[x] == 0)
				{
					writer.WriteLine("");
					continue;
				}
				StringBuilder builder = new StringBuilder("");
				for (int y = 0; y < countY[x]; y++)
				{
					builder.Append(array2D[x, y].ToString());
					if (y != countY[x] - 1)
						builder.Append(",");
				}
				writer.WriteLine(builder.ToString());
			}
			writer.WriteLine(FINISHER);
		}

		public void WriteExtended(string key, string[,] array2D)
		{
			throw new NotImplementedException("まだ実装してないよ");
		}

		public void WriteExtended(string key, Int64[, ,] array3D)
		{
			if (writer == null)
				throw new FileEE("無効なストリームです");
			if (array3D == null)
				throw new FileEE("無効な配列が渡されました");
			int countX = 0;
			int length0 = array3D.GetLength(0);
			int length1 = array3D.GetLength(1);
			int length2 = array3D.GetLength(2);
			int[] countY = new int[length0];
			int[,] countZ = new int[length0, length1];
			for (int x = 0; x < length0; x++)
			{
				for (int y = 0; y < length1; y++)
				{
					for (int z = 0; z < length2; z++)
					{
						if (array3D[x, y, z] != 0)
						{
							countX = x + 1;
							countY[x] = y + 1;
							countZ[x, y] = z + 1;
						}
					}
				}
			}
			if (countX == 0)
				return;
			writer.WriteLine(key);
			for (int x = 0; x < countX; x++)
			{
				writer.WriteLine(x.ToString() + "{");
				if (countY[x] == 0)
				{
					writer.WriteLine("}");
					continue;
				}
				for (int y = 0; y < countY[x]; y++)
				{
					StringBuilder builder = new StringBuilder("");
					if (countZ[x, y] == 0)
					{
						writer.WriteLine("");
						continue;
					}
					for (int z = 0; z < countZ[x, y]; z++)
					{
						builder.Append(array3D[x, y, z].ToString());
						if (z != countZ[x, y] - 1)
							builder.Append(",");
					}
					writer.WriteLine(builder.ToString());
				}
				writer.WriteLine("}");
			}
			writer.WriteLine(FINISHER);
		}

		public void WriteExtended(string key, string[, ,] array2D)
		{
			throw new NotImplementedException("まだ実装してないよ");
		}
		#endregion

		#region IDisposable メンバ

		public void Dispose()
		{
			if (writer != null)
				writer.Close();
			else if (file != null)
				file.Close();
			writer = null;
			file = null;
		}

		#endregion
		public void Close()
		{
			this.Dispose();
		}
	}
}
