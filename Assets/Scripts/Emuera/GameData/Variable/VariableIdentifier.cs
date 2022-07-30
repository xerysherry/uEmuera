using System;
using System.Collections.Generic;
using System.Text;
using MinorShift.Emuera.Sub;

namespace MinorShift.Emuera.GameData.Variable
{
	//1756 全ての機能をVariableTokenとManagerに委譲、消滅
	//……しようと思ったがConstantDataから参照されているので捨て切れなかった。
	/// <summary>
	/// VariableCodeのラッパー
	/// </summary>
	internal sealed class VariableIdentifier
	{
		private VariableIdentifier(VariableCode code)
		{ this.code = code; }
		private VariableIdentifier(VariableCode code, string scope)
		{ this.code = code; this.scope = scope; }
		readonly VariableCode code;
		readonly string scope;
		public VariableCode Code
		{ get { return code; } }
		public string Scope
		{ get { return scope; } }
		public int CodeInt
		{ get { return (int)(code & VariableCode.__LOWERCASE__); } }
		public VariableCode CodeFlag
		{ get { return code & VariableCode.__UPPERCASE__; } }
		//public int Dimension
		//{
		//    get
		//    {
		//        int dim = 0;
		//        if ((code & VariableCode.__ARRAY_1D__) == VariableCode.__ARRAY_1D__)
		//            dim++;
		//        if ((code & VariableCode.__CHARACTER_DATA__) == VariableCode.__CHARACTER_DATA__)
		//            dim++;
		//        if ((code & VariableCode.__ARRAY_2D__) == VariableCode.__ARRAY_2D__)
		//            dim += 2;
		//        return dim;
		//    }
		//}

		public bool IsNull
		{
			get
			{
				return code == VariableCode.__NULL__;
			}
		}
		public bool IsCharacterData
		{
			get
			{
				return ((code & VariableCode.__CHARACTER_DATA__) == VariableCode.__CHARACTER_DATA__);
			}
		}
		public bool IsInteger
		{
			get
			{
				return ((code & VariableCode.__INTEGER__) == VariableCode.__INTEGER__);
			}
		}
		public bool IsString
		{
			get
			{
				return ((code & VariableCode.__STRING__) == VariableCode.__STRING__);
			}
		}
		public bool IsArray1D
		{
			get
			{
				return ((code & VariableCode.__ARRAY_1D__) == VariableCode.__ARRAY_1D__);
			}
		}
		public bool IsArray2D
		{
			get
			{
				return ((code & VariableCode.__ARRAY_2D__) == VariableCode.__ARRAY_2D__);
			}
		}
		public bool IsArray3D
		{
			get
			{
				return ((code & VariableCode.__ARRAY_3D__) == VariableCode.__ARRAY_3D__);
			}
		}
		public bool Readonly
		{
			get
			{
				return ((code & VariableCode.__UNCHANGEABLE__) == VariableCode.__UNCHANGEABLE__);
			}
		}
		public bool IsCalc
		{
			get
			{
				return ((code & VariableCode.__CALC__) == VariableCode.__CALC__);
			}
		}
		public bool IsLocal
		{
			get
			{
				return ((code & VariableCode.__LOCAL__) == VariableCode.__LOCAL__);
			}
		}
		//public bool IsConstant
		//{
		//    get
		//    {
		//        return ((code & VariableCode.__CONSTANT__) == VariableCode.__CONSTANT__);
		//    }
		//}
        public bool CanForbid
        {
            get
            {
                return ((code & VariableCode.__CAN_FORBID__) == VariableCode.__CAN_FORBID__);
            }
        }
		readonly static Dictionary<string, VariableCode> nameDic = new Dictionary<string, VariableCode>();
		readonly static Dictionary<string, VariableCode> localvarNameDic = new Dictionary<string, VariableCode>();
		readonly static Dictionary<VariableCode, List<VariableCode>> extSaveListDic = new Dictionary<VariableCode, List<VariableCode>>();

		public static Dictionary<string, VariableCode> GetVarNameDic()
		{
			return nameDic;
		}


		static VariableIdentifier()
		{
			Array array = Enum.GetValues(typeof(VariableCode));

			nameDic.Add(VariableCode.__FILE__.ToString(), VariableCode.__FILE__);
			nameDic.Add(VariableCode.__LINE__.ToString(), VariableCode.__LINE__);
			nameDic.Add(VariableCode.__FUNCTION__.ToString(), VariableCode.__FUNCTION__);
			foreach (object name in array)
			{
				VariableCode code = (VariableCode)name;
				string key = code.ToString();
				if ((key == null) || (key.StartsWith("__") && key.EndsWith("__")))
					continue;
				if (Config.ICVariable)
					key = key.ToUpper();
				if (nameDic.ContainsKey(key))
					continue;
#if UEMUERA_DEBUG
				if ((code & VariableCode.__ARRAY_2D__) == VariableCode.__ARRAY_2D__)
				{
					if ((code & VariableCode.__ARRAY_1D__) == VariableCode.__ARRAY_1D__)
						throw new ExeEE("ARRAY2DとARRAY1Dは排他");
				}
				if (((code & VariableCode.__INTEGER__) != VariableCode.__INTEGER__)
					&& ((code & VariableCode.__STRING__) != VariableCode.__STRING__))
						throw new ExeEE("INTEGERとSTRINGのどちらかは必須");
				if (((code & VariableCode.__INTEGER__) == VariableCode.__INTEGER__)
					&& ((code & VariableCode.__STRING__) == VariableCode.__STRING__))
						throw new ExeEE("INTEGERとSTRINGは排他");
				if((code & VariableCode.__EXTENDED__) != VariableCode.__EXTENDED__)
				{
					if ((code & VariableCode.__SAVE_EXTENDED__) == VariableCode.__SAVE_EXTENDED__)
							throw new ExeEE("SAVE_EXTENDEDにはEXTENDEDフラグ必須");
					if ((code & VariableCode.__LOCAL__) == VariableCode.__LOCAL__)
							throw new ExeEE("LOCALにはEXTENDEDフラグ必須");
					if ((code & VariableCode.__GLOBAL__) == VariableCode.__GLOBAL__)
							throw new ExeEE("GLOBALにはEXTENDEDフラグ必須");
					if ((code & VariableCode.__ARRAY_2D__) == VariableCode.__ARRAY_2D__)
							throw new ExeEE("ARRAY2DにはEXTENDEDフラグ必須");
				}
				if (((code & VariableCode.__SAVE_EXTENDED__) == VariableCode.__SAVE_EXTENDED__)
					&& ((code & VariableCode.__UNCHANGEABLE__) == VariableCode.__UNCHANGEABLE__))
						throw new ExeEE("CALCとSAVE_EXTENDEDは排他");
				if (((code & VariableCode.__SAVE_EXTENDED__) == VariableCode.__SAVE_EXTENDED__)
					&& ((code & VariableCode.__CALC__) == VariableCode.__CALC__))
						throw new ExeEE("UNCHANGEABLEとSAVE_EXTENDEDは排他");
				if (((code & VariableCode.__SAVE_EXTENDED__) == VariableCode.__SAVE_EXTENDED__)
					&& ((code & VariableCode.__ARRAY_2D__) == VariableCode.__ARRAY_2D__)
					&& ((code & VariableCode.__STRING__) == VariableCode.__STRING__))
						throw new ExeEE("STRINGかつARRAY2DのSAVE_EXTENDEDは未実装");
#endif
				nameDic.Add(key, code);
				////セーブが必要な変数リストの作成

				////__SAVE_EXTENDED__フラグ持ち
				//if ((code & VariableCode.__SAVE_EXTENDED__) == VariableCode.__SAVE_EXTENDED__)
				//{
				//    if ((code & VariableCode.__CHARACTER_DATA__) == VariableCode.__CHARACTER_DATA__)
				//        charaSaveDataList.Add(code);
				//    else
				//        saveDataList.Add(code);
				//}
				//else if ( ((code & VariableCode.__EXTENDED__) != VariableCode.__EXTENDED__)
				//    && ((code & VariableCode.__CALC__) != VariableCode.__CALC__)
				//    && ((code & VariableCode.__UNCHANGEABLE__) != VariableCode.__UNCHANGEABLE__)
				//    && ((code & VariableCode.__LOCAL__) != VariableCode.__LOCAL__)
				//    && (!key.StartsWith("NOTUSE_")) )
				//{//eramaker由来の変数でセーブするもの

				//    VariableCode flag = code & (VariableCode.__ARRAY_1D__ | VariableCode.__ARRAY_2D__ | VariableCode.__ARRAY_3D__ | VariableCode.__STRING__ | VariableCode.__INTEGER__ | VariableCode.__CHARACTER_DATA__);
				//    int codeInt = (int)VariableCode.__LOWERCASE__ & (int)code;
				//    switch (flag)
				//    {
				//        case VariableCode.__CHARACTER_DATA__ | VariableCode.__INTEGER__:
				//            if (codeInt < (int)VariableCode.__COUNT_SAVE_CHARACTER_INTEGER__)
				//                charaSaveDataList.Add(code);
				//            break;
				//        case VariableCode.__CHARACTER_DATA__ | VariableCode.__STRING__:
				//            if (codeInt < (int)VariableCode.__COUNT_SAVE_CHARACTER_STRING__)
				//                charaSaveDataList.Add(code);
				//            break;
				//        case VariableCode.__CHARACTER_DATA__ | VariableCode.__INTEGER__ | VariableCode.__ARRAY_1D__:
				//            if (codeInt < (int)VariableCode.__COUNT_SAVE_CHARACTER_INTEGER_ARRAY__)
				//                charaSaveDataList.Add(code);
				//            break;
				//        case VariableCode.__CHARACTER_DATA__ | VariableCode.__STRING__ | VariableCode.__ARRAY_1D__:
				//            if (codeInt < (int)VariableCode.__COUNT_SAVE_CHARACTER_STRING_ARRAY__)
				//                charaSaveDataList.Add(code);
				//            break;
				//        case VariableCode.__INTEGER__:
				//            if (codeInt < (int)VariableCode.__COUNT_SAVE_INTEGER__)
				//                saveDataList.Add(code);
				//            break;
				//        case VariableCode.__STRING__:
				//            if (codeInt < (int)VariableCode.__COUNT_SAVE_STRING__)
				//                saveDataList.Add(code);
				//            break;
				//        case VariableCode.__INTEGER__ | VariableCode.__ARRAY_1D__:
				//            if (codeInt < (int)VariableCode.__COUNT_SAVE_INTEGER_ARRAY__)
				//                saveDataList.Add(code);
				//            break;
				//        case VariableCode.__STRING__ | VariableCode.__ARRAY_1D__:
				//            if (codeInt < (int)VariableCode.__COUNT_SAVE_STRING_ARRAY__)
				//                saveDataList.Add(code);
				//            break;
				//    }
				//}

				
				if ((code & VariableCode.__LOCAL__) == VariableCode.__LOCAL__)
					localvarNameDic.Add(key, code);
				if ((code & VariableCode.__SAVE_EXTENDED__) == VariableCode.__SAVE_EXTENDED__)
				{
					VariableCode flag = code &
						(VariableCode.__ARRAY_1D__ | VariableCode.__ARRAY_2D__ | VariableCode.__ARRAY_3D__ | VariableCode.__CHARACTER_DATA__ | VariableCode.__STRING__ | VariableCode.__INTEGER__);

                    if(!extSaveListDic.TryGetValue(flag, out var var_code_list))
                    {
                        var_code_list = new List<VariableCode>();
                        extSaveListDic.Add(flag, var_code_list);
                    }
                    var_code_list.Add(code);
				}
			}
		}

		public static List<VariableCode> GetExtSaveList(VariableCode flag)
		{
			VariableCode gFlag = flag &
				(VariableCode.__ARRAY_1D__ | VariableCode.__ARRAY_2D__ | VariableCode.__ARRAY_3D__ | VariableCode.__CHARACTER_DATA__ | VariableCode.__STRING__ | VariableCode.__INTEGER__);
            if (!extSaveListDic.TryGetValue(gFlag, out var var_code_list))
				return new List<VariableCode>();
			return var_code_list;
		}

		public static VariableIdentifier GetVariableId(VariableCode code)
		{
			return new VariableIdentifier(code);
		}

		public static VariableIdentifier GetVariableId(string key)
		{
			return GetVariableId(key, null);
		}
		public static VariableIdentifier GetVariableId(string key, string subStr)
		{
			VariableCode ret = VariableCode.__NULL__;
			if (string.IsNullOrEmpty(key))
				return null;
			if (Config.ICVariable)
				key = key.ToUpper();
			if (subStr != null)
			{
				if (Config.ICFunction)
					subStr = subStr.ToUpper();
				if (localvarNameDic.TryGetValue(key, out ret))
					return new VariableIdentifier(ret, subStr);
				if (nameDic.ContainsKey(key))
					throw new CodeEE("ローカル変数でない変数" + key + "に対して@が使われました");
				throw new CodeEE("@の使い方が不正です");
			}
			if (nameDic.TryGetValue(key, out ret))
				return new VariableIdentifier(ret);
			else
				return null;
		}
		public override string ToString()
		{
			return code.ToString();
		}
	}
}