using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MinorShift.Emuera.Sub
{
	#region reader/writer共通データ
	public enum EraSaveFileType : byte
	{
		Normal = 0x00,
		Global = 0x01,
		Var = 0x02,
		CharVar = 0x03,
	}

	public enum EraSaveDataType : byte
	{
		Int = 0x00,
		IntArray = 0x01,
		IntArray2D = 0x02,
		IntArray3D = 0x03,
		Str = 0x10,
		StrArray = 0x11,
		StrArray2D = 0x12,
		StrArray3D = 0x13,
		//SOC = 0xFD,//キャラデータ始まり
		Separator = 0xFD,//データ区切り
		EOC = 0xFE,//キャラデータ終わり
		EOF = 0xFF,//ファイル終端
	}

	static class Ebdb//EraBinaryData中のマジックナンバーなバイト
	{
		public const byte Byte = 0xCF;
		public const byte Int16 = 0xD0;//直後の2バイトがInt16
		public const byte Int32 = 0xD1;//直後の4バイトがInt32
		public const byte Int64 = 0xD2;//直後の8バイトがInt64
		public const byte String = 0xD8;//直後がString

		public const byte EoA1 = 0xE0;//データ区切り（一次元
		public const byte EoA2 = 0xE1;//データ区切り（二次元
		public const byte Zero = 0xF0;//直後にゼロが連続する数
		public const byte ZeroA1 = 0xF1;//直後に空配列が連続する数（一次元
		public const byte ZeroA2 = 0xF2;//直後に空配列が連続する数（二次元
		public const byte EoD = 0xFF;//変数データ終わり
	}

	static class EraBDConst
	{
		//Headerはpngのパクリ
		public const UInt64 Header = 0x0A1A0A0D41524589UL;
		public const UInt32 Version1808 = 1808;
		public const UInt32 DataCount = 0;
	}
	#endregion

	/// <summary>
	/// 1808追加 新しいデータ保存形式
	/// 将来形式を変更したときのためにabstractにしておく
	/// </summary>
	internal abstract class EraBinaryDataReader : IDisposable
	{
		private EraBinaryDataReader() {}
		
		protected EraBinaryDataReader(BinaryReader stream, int ver, UInt32[] buf)
		{
			reader = stream;
			version = ver;
			data = buf;
		}
		protected BinaryReader reader = null;
		protected readonly int version = 0;
		protected readonly UInt32[] data = null;

		public abstract int ReaderVersion { get; }
		/// <summary>
		/// FileStreamからReaderを作成
		/// 不正なファイルの場合はnullを返す・例外は投げない
		/// </summary>
		/// <param name="fs"></param>
		/// <returns></returns>
		public static EraBinaryDataReader CreateReader(FileStream fs)
		{
			try
			{
				if ((fs == null) || (fs.Length < 16))
					return null;
				BinaryReader reader = new BinaryReader(fs, Encoding.Unicode);

				if (reader.ReadUInt64() != EraBDConst.Header)
					return null;
				int version = (int)reader.ReadUInt32();
				int datacount = (int)reader.ReadUInt32();
				UInt32[] data = new UInt32[datacount];
				for (int i = 0; i < datacount; i++)
					data[i] = reader.ReadUInt32();
				if (version == EraBDConst.Version1808)
					return new EraBinaryDataReader1808(reader, version, data);
				else
					return null;
			}
			catch
			{
				return null;
			}
		}

		public abstract EraSaveFileType ReadFileType();

		/// <summary>
		/// システム用の特殊処理・圧縮なし
		/// </summary>
		/// <returns></returns>
		public abstract Int64 ReadInt64();

		public abstract string ReadString();
		public abstract Int64 ReadInt();
		public abstract void ReadIntArray(Int64[] refArray, bool needInit);
		public abstract void ReadIntArray2D(Int64[,] refArray, bool needInit);
		public abstract void ReadIntArray3D(Int64[, ,] refArray, bool needInit);
		public abstract void ReadStrArray(string[] refArray, bool needInit);
		public abstract void ReadStrArray2D(string[,] refArray, bool needInit);
		public abstract void ReadStrArray3D(string[, ,] refArray, bool needInit);
		public abstract KeyValuePair<string, EraSaveDataType> ReadVariableCode();
		#region IDisposable メンバ

		public void Dispose()
		{
			if (reader != null)
				reader.Close();
			reader = null;
		}

		#endregion
		public void Close()
		{
			Dispose();
		}

		private sealed class EraBinaryDataReader1808 : EraBinaryDataReader
		{
			public EraBinaryDataReader1808(BinaryReader stream, int ver, UInt32[] buf)
				: base(stream, ver, buf)
			{
			}

			//public bool EOF
			//{
			//    get
			//    {
			//        return (reader.BaseStream.Length == reader.BaseStream.Position);
			//    }
			//}

			public override int ReaderVersion { get { return 1808; } }
			public override EraSaveFileType ReadFileType()
			{
				byte type = reader.ReadByte();
				if (type >= 0 && type <= 3)
					return (EraSaveFileType)type;
				throw new FileEE("ファイルデータ型異常");
			}

			private Int64 m_ReadInt()
			{
				byte b = reader.ReadByte();
				if (b <= Ebdb.Byte)
					return b;
				if (b == Ebdb.Int16)
					return reader.ReadInt16();
				if (b == Ebdb.Int32)
					return reader.ReadInt32();
				if (b == Ebdb.Int64)
					return reader.ReadInt64();
				throw new FileEE("バイナリデータの異常");
			}

			public override Int64 ReadInt64()
			{
				return reader.ReadInt64();
			}

			public override KeyValuePair<string, EraSaveDataType> ReadVariableCode()
			{
				EraSaveDataType type = (EraSaveDataType)reader.ReadByte();
				if (type == EraSaveDataType.EOC || type == EraSaveDataType.EOF || type == EraSaveDataType.Separator)
					return new KeyValuePair<string, EraSaveDataType>(null, type);
				string key = reader.ReadString();
				return new KeyValuePair<string, EraSaveDataType>(key, type);
			}

			//配列じゃないやつは特殊処理
			public override Int64 ReadInt()
			{
				return m_ReadInt();
			}


			public override string ReadString()
			{
				return reader.ReadString();
			}

			public override void ReadIntArray(Int64[] refArray, bool needInit)
			{
				Int64[] oriArray = null;
				byte b;
				int x = 0;
				int saveLength0 = reader.ReadInt32();
				if (refArray == null)//読み捨て。レアケースのはず
					refArray = new Int64[saveLength0];

				int length0 = refArray.Length;

				//保存されたデータの方が大きいとき。レアケースのはず
				if (length0 < saveLength0)
				{
                    oriArray = refArray;
                    //1818修正 サイズ違いの時にあふれないように/配列は最大まで確保、作業するのは重複部分だけ
                    refArray = new Int64[Math.Max(length0, saveLength0)];

                    length0 = Math.Min(length0, saveLength0);
				}
				while (true)
				{
					b = reader.ReadByte();
					if (b == Ebdb.EoD)
						break;
					if (b == Ebdb.Zero)
					{
						int cnt = (int)m_ReadInt();
						if (needInit)
							for (int i = 0; i < cnt; i++)
								refArray[x + i] = 0;
						x += cnt;
						continue;
					}
					if (b <= Ebdb.Byte)
						refArray[x] = b;
					else if (b == Ebdb.Int16)
						refArray[x] = reader.ReadInt16();
					else if (b == Ebdb.Int32)
						refArray[x] = reader.ReadInt32();
					else if (b == Ebdb.Int64)
						refArray[x] = reader.ReadInt64();
					else
						throw new FileEE("バイナリデータの異常");
					x++;
				}
				if (needInit)
					for (; x < length0; x++)
						refArray[x] = 0;
				if (oriArray != null)
				{
					for (x = 0; x < length0; x++)
						oriArray[x] = refArray[x];
				}
				return;
			}
			public override void ReadIntArray2D(Int64[,] refArray, bool needInit)
			{
				Int64[,] oriArray = null;
				byte b;
				int x = 0;
				int y = 0;
				int saveLength0 = reader.ReadInt32();
				int saveLength1 = reader.ReadInt32();
				if (refArray == null)
					refArray = new Int64[saveLength0, saveLength1];
				int length0 = refArray.GetLength(0);
				int length1 = refArray.GetLength(1);

				if (length0 < saveLength0 || length1 < saveLength1)
				{
                    oriArray = refArray;
                    //1818修正 サイズ違いの時にあふれないように/配列は最大まで確保、作業するのは重複部分だけ
                    refArray = new Int64[Math.Max(length0, saveLength0), Math.Max(length1, saveLength1)];

                    length0 = Math.Min(length0, saveLength0);
                    length1 = Math.Min(length1, saveLength1);
				}

				while (true)
				{
					b = reader.ReadByte();
					if (b == Ebdb.EoD)
						break;
					if (b == Ebdb.ZeroA1)
					{
						int cnt = (int)m_ReadInt();
						if (needInit)
							for (int i = 0; i < cnt; i++)
								for (y = 0; y < length1; y++)
									refArray[x + i, y] = 0;
						x += cnt;
						y = 0;
						continue;
					}
					if (b == Ebdb.EoA1)
					{
						if (needInit)
							for (; y < length1; y++)
								refArray[x, y] = 0;
						x++;
						y = 0;
						continue;
					}

					if (b == Ebdb.Zero)
					{
						int cnt = (int)m_ReadInt();
						if (needInit)
							for (int i = 0; i < cnt; i++)
								refArray[x, y + i] = 0;
						y += cnt;
						continue;
					}
					if (b <= Ebdb.Byte)
						refArray[x, y] = b;
					else if (b == Ebdb.Int16)
						refArray[x, y] = reader.ReadInt16();
					else if (b == Ebdb.Int32)
						refArray[x, y] = reader.ReadInt32();
					else if (b == Ebdb.Int64)
						refArray[x, y] = reader.ReadInt64();
					else
						throw new FileEE("バイナリデータの異常");
					y++;
				}
				if (needInit)
				{
					for (; x < length0; x++)
					{
						for (; y < length1; y++)
							refArray[x, y] = 0;
						y = 0;
					}
				}
				if (oriArray != null)
				{
					for (x = 0; x < length0; x++)
						for (y = 0; y < length1; y++)
							oriArray[x, y] = refArray[x, y];
				}
				return;
			}
			/// <summary>
			/// 
			/// </summary>
			/// <param name="refArray">データを書き出す先。読み捨てるならnull</param>
			/// <param name="needInit">データがない部分を0で埋める必要があるか</param>
			public override void ReadIntArray3D(Int64[, ,] refArray, bool needInit)
			{
				Int64[, ,] oriArray = null;
				byte b;
				int x = 0;
				int y = 0;
				int z = 0;
				int saveLength0 = reader.ReadInt32();
				int saveLength1 = reader.ReadInt32();
				int saveLength2 = reader.ReadInt32();
				if (refArray == null)
					refArray = new Int64[saveLength0, saveLength1, saveLength2];
				int length0 = refArray.GetLength(0);
				int length1 = refArray.GetLength(1);
				int length2 = refArray.GetLength(2);

				if (length0 < saveLength0 || length1 < saveLength1 || length2 < saveLength2)
				{
					oriArray = refArray;
                    //1818修正 サイズ違いの時にあふれないように/配列は最大まで確保、作業するのは重複部分だけ
                    refArray = new Int64[Math.Max(length0, saveLength0), Math.Max(length1, saveLength1), Math.Max(length2, saveLength2)];

                    length0 = Math.Min(length0, saveLength0);
                    length1 = Math.Min(length1, saveLength1);
                    length2 = Math.Min(length2, saveLength2);
				}

				while (true)
				{
					b = reader.ReadByte();
					if (b == Ebdb.EoD)
						break;
					if (b == Ebdb.ZeroA2)//cnt分だけ空の行列が連続
					{
						int cnt = (int)m_ReadInt();
						if (needInit)
							for (int i = 0; i < cnt; i++)
								for (y = 0; y < length1; y++)
									for (z = 0; z < length2; z++)
										refArray[x + i, y, z] = 0;
						x += cnt;
						y = 0;
						z = 0;
						continue;
					}
					if (b == Ebdb.EoA2)//行列終わりor残りが全て0
					{
						if (needInit)
						{
							for (; y < length1; y++)
							{
								for (; z < length2; z++)
									refArray[x, y, z] = 0;
								z = 0;
							}
						}
						x++;
						y = 0;
						z = 0;
						continue;
					}

					if (b == Ebdb.ZeroA1)//cnt分だけ空の列が連続
					{
						int cnt = (int)m_ReadInt();
						if (needInit)
							for (int i = 0; i < cnt; i++)
								for (z = 0; z < length2; z++)
									refArray[x, y + i, z] = 0;
						y += cnt;
						z = 0;
						continue;
					}
					if (b == Ebdb.EoA1)//列終わりor残り全て0
					{
						if (needInit)
							for (; z < length2; z++)
								refArray[x, y, z] = 0;
						y++;
						z = 0;
						continue;
					}

					if (b == Ebdb.Zero)//cnt分だけ0が連続
					{
						int cnt = (int)m_ReadInt();
						if (needInit)
							for (int i = 0; i < cnt; i++)
								refArray[x, y, z + i] = 0;
						z += cnt;
						continue;
					}
					if (b <= Ebdb.Byte)
						refArray[x, y, z] = b;
					else if (b == Ebdb.Int16)
						refArray[x, y, z] = reader.ReadInt16();
					else if (b == Ebdb.Int32)
						refArray[x, y, z] = reader.ReadInt32();
					else if (b == Ebdb.Int64)
						refArray[x, y, z] = reader.ReadInt64();
					else
						throw new FileEE("バイナリデータの異常");
					z++;
				}
				if (needInit)
				{

					for (; x < length0; x++)
					{
						for (; y < length1; y++)
						{
							for (; z < length2; z++)
								refArray[x, y, z] = 0;
							z = 0;
						}
						y = 0;
					}
				}
				if (oriArray != null)
				{
					for (x = 0; x < length0; x++)
						for (y = 0; y < length1; y++)
							for (z = 0; z < length2; z++)
								oriArray[x, y, z] = refArray[x, y, z];
				}
				return;
			}
			public override void ReadStrArray(string[] refArray, bool needInit)
			{
				string[] oriArray = null;
				byte b;
				int x = 0;
				int saveLength0 = reader.ReadInt32();
				if (refArray == null)//読み捨て。レアケースのはず
					refArray = new string[saveLength0];

				int length0 = refArray.Length;

				//保存されたデータの方が大きいとき。レアケースのはず
				if (length0 < saveLength0)
				{
                    oriArray = refArray;
                    //1818修正 サイズ違いの時にあふれないように/配列は最大まで確保、作業するのは重複部分だけ
                    refArray = new string[Math.Max(length0, saveLength0)];

                    length0 = Math.Min(length0, saveLength0);
				}
				while (true)
				{
					b = reader.ReadByte();
					if (b == Ebdb.EoD)
						break;
					if (b == Ebdb.Zero)
					{
						int cnt = (int)m_ReadInt();
						if (needInit)
							for (int i = 0; i < cnt; i++)
								refArray[x + i] = null;
						x += cnt;
						continue;
					}
					if (b == Ebdb.String)
						refArray[x] = ReadString();
					else
						throw new FileEE("バイナリデータの異常");
					x++;
				}
				if (needInit)
					for (; x < length0; x++)
						refArray[x] = null;
				if (oriArray != null)
				{
					for (x = 0; x < length0; x++)
						oriArray[x] = refArray[x];
				}
				return;
			}
			public override void ReadStrArray2D(string[,] refArray, bool needInit)
			{
				string[,] oriArray = null;
				byte b;
				int x = 0;
				int y = 0;
				int saveLength0 = reader.ReadInt32();
				int saveLength1 = reader.ReadInt32();
				if (refArray == null)
					refArray = new string[saveLength0, saveLength1];
				int length0 = refArray.GetLength(0);
				int length1 = refArray.GetLength(1);

				if (length0 < saveLength0 || length1 < saveLength1)
				{
                    oriArray = refArray;
                    //1818修正 サイズ違いの時にあふれないように/配列は最大まで確保、作業するのは重複部分だけ
                    refArray = new string[Math.Max(length0, saveLength0), Math.Max(length1, saveLength1)];

                    length0 = Math.Min(length0, saveLength0);
                    length1 = Math.Min(length1, saveLength1);
				}

				while (true)
				{
					b = reader.ReadByte();
					if (b == Ebdb.EoD)
						break;
					if (b == Ebdb.ZeroA1)
					{
						int cnt = (int)m_ReadInt();
						if (needInit)
							for (int i = 0; i < cnt; i++)
								for (y = 0; y < length1; y++)
									refArray[x + i, y] = null;
						x += cnt;
						y = 0;
						continue;
					}
					if (b == Ebdb.EoA1)
					{
						if (needInit)
							for (; y < length1; y++)
								refArray[x, y] = null;
						x++;
						y = 0;
						continue;
					}

					if (b == Ebdb.Zero)
					{
						int cnt = (int)m_ReadInt();
						if (needInit)
							for (int i = 0; i < cnt; i++)
								refArray[x, y + i] = null;
						y += cnt;
						continue;
					}
					if (b == Ebdb.String)
						refArray[x, y] = ReadString();
					else
						throw new FileEE("バイナリデータの異常");
					y++;
				}
				if (needInit)
				{
					for (; x < length0; x++)
					{
						for (; y < length1; y++)
							refArray[x, y] = null;
						y = 0;
					}
				}
				if (oriArray != null)
				{
					for (x = 0; x < length0; x++)
						for (y = 0; y < length1; y++)
							oriArray[x, y] = refArray[x, y];
				}
				return;
			}
			public override void ReadStrArray3D(string[, ,] refArray, bool needInit)
			{
				string[, ,] oriArray = null;
				byte b;
				int x = 0;
				int y = 0;
				int z = 0;
				int saveLength0 = reader.ReadInt32();
				int saveLength1 = reader.ReadInt32();
				int saveLength2 = reader.ReadInt32();
				if (refArray == null)
					refArray = new string[saveLength0, saveLength1, saveLength2];
				int length0 = refArray.GetLength(0);
				int length1 = refArray.GetLength(1);
				int length2 = refArray.GetLength(2);

				if (length0 < saveLength0 || length1 < saveLength1 || length2 < saveLength2)
				{
                    oriArray = refArray;
                    //1818修正 サイズ違いの時にあふれないように/配列は最大まで確保、作業するのは重複部分だけ
                    refArray = new string[Math.Max(length0, saveLength0), Math.Max(length1, saveLength1), Math.Max(length2, saveLength2)];

                    length0 = Math.Min(length0, saveLength0);
                    length1 = Math.Min(length1, saveLength1);
                    length2 = Math.Min(length2, saveLength2);
				}

				while (true)
				{
					b = reader.ReadByte();
					if (b == Ebdb.EoD)
						break;
					if (b == Ebdb.ZeroA2)//cnt分だけ空の行列が連続
					{
						int cnt = (int)m_ReadInt();
						if (needInit)
							for (int i = 0; i < cnt; i++)
								for (y = 0; y < length1; y++)
									for (z = 0; z < length2; z++)
										refArray[x + i, y, z] = null;
						x += cnt;
						y = 0;
						z = 0;
						continue;
					}
					if (b == Ebdb.EoA2)//行列終わりor残りが全て0
					{
						if (needInit)
						{
							for (; y < length1; y++)
							{
								for (; z < length2; z++)
									refArray[x, y, z] = null;
								z = 0;
							}
						}
						x++;
						y = 0;
						z = 0;
						continue;
					}

					if (b == Ebdb.ZeroA1)//cnt分だけ空の列が連続
					{
						int cnt = (int)m_ReadInt();
						if (needInit)
							for (int i = 0; i < cnt; i++)
								for (z = 0; z < length2; z++)
									refArray[x, y + i, z] = null;
						y += cnt;
						z = 0;
						continue;
					}
					if (b == Ebdb.EoA1)//列終わりor残り全て0
					{
						if (needInit)
							for (; z < length2; z++)
								refArray[x, y, z] = null;
						y++;
						z = 0;
						continue;
					}

					if (b == Ebdb.Zero)//cnt分だけ0が連続
					{
						int cnt = (int)m_ReadInt();
						if (needInit)
							for (int i = 0; i < cnt; i++)
								refArray[x, y, z + i] = null;
						z += cnt;
						continue;
					}
					if (b == Ebdb.String)
						refArray[x, y, z] = ReadString();
					else
						throw new FileEE("バイナリデータの異常");
					z++;
				}
				if (needInit)
				{
					for (; x < length0; x++)
					{
						for (; y < length1; y++)
						{
							for (; z < length2; z++)
								refArray[x, y, z] = null;
							z = 0;
						}
						y = 0;
					}
				}
				if (oriArray != null)
				{
					for (x = 0; x < length0; x++)
						for (y = 0; y < length1; y++)
							for (z = 0; z < length2; z++)
								oriArray[x, y, z] = refArray[x, y, z];
				}
				return;
			}
		}
	}
}
