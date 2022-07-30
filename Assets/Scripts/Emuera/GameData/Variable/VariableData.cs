using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
//using System.Windows.Forms;
using MinorShift.Emuera.Sub;
using MinorShift.Emuera.GameProc;

namespace MinorShift.Emuera.GameData.Variable
{
	/// <summary>
	/// 変数全部
	/// </summary>
	internal sealed partial class VariableData : IDisposable
	{

		readonly Int64[] dataInteger;
		readonly string[] dataString;
		readonly Int64[][] dataIntegerArray;
		readonly string[][] dataStringArray;
		readonly Int64[][,] dataIntegerArray2D;
		readonly string[][,] dataStringArray2D;
		readonly Int64[][, ,] dataIntegerArray3D;
		readonly string[][, ,] dataStringArray3D;
		//readonly VariableLocal<Int64, Int64Calculator> localVars;
		//readonly VariableLocal<string, StringCalculator> localString;
		//readonly VariableLocal<Int64, Int64Calculator> argVars;
		//readonly VariableLocal<string, StringCalculator> argString;
		readonly List<CharacterData> characterList;
		public Int64[] DataInteger { get { return dataInteger; } }
		public string[] DataString { get { return dataString; } }
		public Int64[][] DataIntegerArray { get { return dataIntegerArray; } }
		public string[][] DataStringArray { get { return dataStringArray; } }
		public Int64[][,] DataIntegerArray2D { get { return dataIntegerArray2D; } }
		public string[][,] DataStringArray2D { get { return dataStringArray2D; } }
		public Int64[][, ,] DataIntegerArray3D { get { return dataIntegerArray3D; } }
		public string[][, ,] DataStringArray3D { get { return dataStringArray3D; } }
		//public VariableLocal<Int64, Int64Calculator> LocalVars { get { return localVars; } }
		//public VariableLocal<string, StringCalculator> LocalString { get { return localString; } }
		//public VariableLocal<Int64, Int64Calculator> ArgVars { get { return argVars; } }
		//public VariableLocal<string, StringCalculator> ArgString { get { return argString; } }
		public List<CharacterData> CharacterList { get { return characterList; } }
		readonly GameBase gamebase;
		readonly ConstantData constant;
		internal GameBase GameBase { get { return gamebase; } }
		internal ConstantData Constant { get { return constant; } }

		public Int64 LastLoadVersion = -1;
		public Int64 LastLoadNo = -1;
		public string LastLoadText = "";

		Dictionary<string, VariableToken> varTokenDic = new Dictionary<string, VariableToken>();
		Dictionary<string, VariableLocal> localvarTokenDic = new Dictionary<string, VariableLocal>();

		/// <summary>
		/// ユーザー変数のうちStaticかつ非Globalなもの。ERHでのDIM(非GLOBAL) と関数でのDIM (STATIC)の両方。ロードやリセットで初期化が必要。キャラクタ変数は除く。
		/// </summary>
		List<UserDefinedVariableToken> userDefinedStaticVarList = new List<UserDefinedVariableToken>();
		/// <summary>
		/// ユーザー広域変数のうちグローバル属性持ち。
		/// </summary>
		List<UserDefinedVariableToken> userDefinedGlobalVarList = new List<UserDefinedVariableToken>();
		/// <summary>
		/// ユーザー広域変数のうちセーブされるもの。グローバル、キャラクタ変数は除く。
		/// </summary>
		List<UserDefinedVariableToken>[] userDefinedSaveVarList = new List<UserDefinedVariableToken>[6];
		/// <summary>
		/// ユーザー広域変数のうち、グローバルかつセーブされるもの。
		/// </summary>
		List<UserDefinedVariableToken>[] userDefinedGlobalSaveVarList = new List<UserDefinedVariableToken>[6];
		/// <summary>
		/// ユーザー広域変数のうち、キャラクタ変数であるもの。初期化やセーブされるかどうかはCharacterDataの方で判断。
		/// </summary>
		public List<UserDefinedCharaVariableToken> UserDefinedCharaVarList = new List<UserDefinedCharaVariableToken>();

		public VariableData(GameBase gamebase, ConstantData constant)
		{
			this.gamebase = gamebase;
			this.constant = constant;
			characterList = new List<CharacterData>();
			//localVars = new VariableLocal<Int64, Int64Calculator>(constant.VariableIntArrayLength[(int)(VariableCode.__LOWERCASE__ & VariableCode.LOCAL)]);
			//localString = new VariableLocal<string, StringCalculator>(constant.VariableStrArrayLength[(int)(VariableCode.__LOWERCASE__ & VariableCode.LOCALS)]);
			//argVars = new VariableLocal<Int64, Int64Calculator>(constant.VariableIntArrayLength[(int)(VariableCode.__LOWERCASE__ & VariableCode.ARG)]);
			//argString = new VariableLocal<string, StringCalculator>(constant.VariableStrArrayLength[(int)(VariableCode.__LOWERCASE__ & VariableCode.ARGS)]);
			dataInteger = new Int64[(int)VariableCode.__COUNT_INTEGER__];

			dataIntegerArray = new Int64[(int)VariableCode.__COUNT_INTEGER_ARRAY__][];
			for (int i = 0; i < dataIntegerArray.Length; i++)
				dataIntegerArray[i] = new Int64[constant.VariableIntArrayLength[i]];

			dataString = new string[(int)VariableCode.__COUNT_STRING__];

			dataStringArray = new string[(int)VariableCode.__COUNT_STRING_ARRAY__][];

			for (int i = 0; i < dataStringArray.Length; i++)
				dataStringArray[i] = new string[constant.VariableStrArrayLength[i]];


			dataIntegerArray2D = new Int64[(int)VariableCode.__COUNT_INTEGER_ARRAY_2D__][,];
			for (int i = 0; i < dataIntegerArray2D.Length; i++)
			{
				Int64 length64 = constant.VariableIntArray2DLength[i];
				int length = (int)(length64 >> 32);
				int length2 = (int)(length64 & 0x7FFFFFFF);
				dataIntegerArray2D[i] = new Int64[length, length2];
			}
			dataStringArray2D = new string[(int)VariableCode.__COUNT_STRING_ARRAY_2D__][,];
			for (int i = 0; i < dataStringArray2D.Length; i++)
			{
				Int64 length64 = constant.VariableStrArray2DLength[i];
				int length = (int)(length64 >> 32);
				int length2 = (int)(length64 & 0x7FFFFFFF);
				dataStringArray2D[i] = new string[length, length2];
			}
			dataIntegerArray3D = new Int64[(int)VariableCode.__COUNT_INTEGER_ARRAY_3D__][, ,];
			for (int i = 0; i < dataIntegerArray3D.Length; i++)
			{
				Int64 length64 = constant.VariableIntArray3DLength[i];
				int length = (int)(length64 >> 40);
				int length2 = (int)((length64 >> 20) & 0xFFFFF);
				int length3 = (int)(length64 & 0xFFFFF);
				dataIntegerArray3D[i] = new Int64[length, length2, length3];
			}
			dataStringArray3D = new string[(int)VariableCode.__COUNT_STRING_ARRAY_3D__][, ,];
			for (int i = 0; i < dataStringArray3D.Length; i++)
			{
				Int64 length64 = constant.VariableStrArray3DLength[i];
				int length = (int)(length64 >> 40);
				int length2 = (int)((length64 >> 20) & 0xFFFFF);
				int length3 = (int)(length64 & 0xFFFFF);
				dataStringArray3D[i] = new string[length, length2, length3];
			}
			for (int i = 0; i < 6; i++)
			{
				userDefinedSaveVarList[i] = new List<UserDefinedVariableToken>();
				userDefinedGlobalSaveVarList[i] = new List<UserDefinedVariableToken>();
			}



			SetDefaultValue(constant);

			varTokenDic.Add("DAY", new Int1DVariableToken(VariableCode.DAY, this));
			varTokenDic.Add("MONEY", new Int1DVariableToken(VariableCode.MONEY, this));
			varTokenDic.Add("ITEM", new Int1DVariableToken(VariableCode.ITEM, this));
			varTokenDic.Add("FLAG", new Int1DVariableToken(VariableCode.FLAG, this));
			varTokenDic.Add("TFLAG", new Int1DVariableToken(VariableCode.TFLAG, this));
			varTokenDic.Add("UP", new Int1DVariableToken(VariableCode.UP, this));
			varTokenDic.Add("PALAMLV", new Int1DVariableToken(VariableCode.PALAMLV, this));
			varTokenDic.Add("EXPLV", new Int1DVariableToken(VariableCode.EXPLV, this));
			varTokenDic.Add("EJAC", new Int1DVariableToken(VariableCode.EJAC, this));
			varTokenDic.Add("DOWN", new Int1DVariableToken(VariableCode.DOWN, this));
			varTokenDic.Add("RESULT", new Int1DVariableToken(VariableCode.RESULT, this));
			varTokenDic.Add("COUNT", new Int1DVariableToken(VariableCode.COUNT, this));
			varTokenDic.Add("TARGET", new Int1DVariableToken(VariableCode.TARGET, this));
			varTokenDic.Add("ASSI", new Int1DVariableToken(VariableCode.ASSI, this));
			varTokenDic.Add("MASTER", new Int1DVariableToken(VariableCode.MASTER, this));
			varTokenDic.Add("NOITEM", new Int1DVariableToken(VariableCode.NOITEM, this));
			varTokenDic.Add("LOSEBASE", new Int1DVariableToken(VariableCode.LOSEBASE, this));
			varTokenDic.Add("SELECTCOM", new Int1DVariableToken(VariableCode.SELECTCOM, this));
			varTokenDic.Add("ASSIPLAY", new Int1DVariableToken(VariableCode.ASSIPLAY, this));
			varTokenDic.Add("PREVCOM", new Int1DVariableToken(VariableCode.PREVCOM, this));
			varTokenDic.Add("TIME", new Int1DVariableToken(VariableCode.TIME, this));
			varTokenDic.Add("ITEMSALES", new Int1DVariableToken(VariableCode.ITEMSALES, this));
			varTokenDic.Add("PLAYER", new Int1DVariableToken(VariableCode.PLAYER, this));
			varTokenDic.Add("NEXTCOM", new Int1DVariableToken(VariableCode.NEXTCOM, this));
			varTokenDic.Add("PBAND", new Int1DVariableToken(VariableCode.PBAND, this));
			varTokenDic.Add("BOUGHT", new Int1DVariableToken(VariableCode.BOUGHT, this));
			varTokenDic.Add("A", new Int1DVariableToken(VariableCode.A, this));
			varTokenDic.Add("B", new Int1DVariableToken(VariableCode.B, this));
			varTokenDic.Add("C", new Int1DVariableToken(VariableCode.C, this));
			varTokenDic.Add("D", new Int1DVariableToken(VariableCode.D, this));
			varTokenDic.Add("E", new Int1DVariableToken(VariableCode.E, this));
			varTokenDic.Add("F", new Int1DVariableToken(VariableCode.F, this));
			varTokenDic.Add("G", new Int1DVariableToken(VariableCode.G, this));
			varTokenDic.Add("H", new Int1DVariableToken(VariableCode.H, this));
			varTokenDic.Add("I", new Int1DVariableToken(VariableCode.I, this));
			varTokenDic.Add("J", new Int1DVariableToken(VariableCode.J, this));
			varTokenDic.Add("K", new Int1DVariableToken(VariableCode.K, this));
			varTokenDic.Add("L", new Int1DVariableToken(VariableCode.L, this));
			varTokenDic.Add("M", new Int1DVariableToken(VariableCode.M, this));
			varTokenDic.Add("N", new Int1DVariableToken(VariableCode.N, this));
			varTokenDic.Add("O", new Int1DVariableToken(VariableCode.O, this));
			varTokenDic.Add("P", new Int1DVariableToken(VariableCode.P, this));
			varTokenDic.Add("Q", new Int1DVariableToken(VariableCode.Q, this));
			varTokenDic.Add("R", new Int1DVariableToken(VariableCode.R, this));
			varTokenDic.Add("S", new Int1DVariableToken(VariableCode.S, this));
			varTokenDic.Add("T", new Int1DVariableToken(VariableCode.T, this));
			varTokenDic.Add("U", new Int1DVariableToken(VariableCode.U, this));
			varTokenDic.Add("V", new Int1DVariableToken(VariableCode.V, this));
			varTokenDic.Add("W", new Int1DVariableToken(VariableCode.W, this));
			varTokenDic.Add("X", new Int1DVariableToken(VariableCode.X, this));
			varTokenDic.Add("Y", new Int1DVariableToken(VariableCode.Y, this));
			varTokenDic.Add("Z", new Int1DVariableToken(VariableCode.Z, this));

			varTokenDic.Add("GLOBAL", new Int1DVariableToken(VariableCode.GLOBAL, this));
			varTokenDic.Add("RANDDATA", new Int1DVariableToken(VariableCode.RANDDATA, this));

			varTokenDic.Add("SAVESTR", new Str1DVariableToken(VariableCode.SAVESTR, this));
			varTokenDic.Add("TSTR", new Str1DVariableToken(VariableCode.TSTR, this));
			varTokenDic.Add("STR", new Str1DVariableToken(VariableCode.STR, this));
			varTokenDic.Add("RESULTS", new Str1DVariableToken(VariableCode.RESULTS, this));
			varTokenDic.Add("GLOBALS", new Str1DVariableToken(VariableCode.GLOBALS, this));

			varTokenDic.Add("SAVEDATA_TEXT", new StrVariableToken(VariableCode.SAVEDATA_TEXT, this));

			varTokenDic.Add("ISASSI", new CharaIntVariableToken(VariableCode.ISASSI, this));
			varTokenDic.Add("NO", new CharaIntVariableToken(VariableCode.NO, this));

			varTokenDic.Add("BASE", new CharaInt1DVariableToken(VariableCode.BASE, this));
			varTokenDic.Add("MAXBASE", new CharaInt1DVariableToken(VariableCode.MAXBASE, this));
			varTokenDic.Add("ABL", new CharaInt1DVariableToken(VariableCode.ABL, this));
			varTokenDic.Add("TALENT", new CharaInt1DVariableToken(VariableCode.TALENT, this));
			varTokenDic.Add("EXP", new CharaInt1DVariableToken(VariableCode.EXP, this));
			varTokenDic.Add("MARK", new CharaInt1DVariableToken(VariableCode.MARK, this));
			varTokenDic.Add("PALAM", new CharaInt1DVariableToken(VariableCode.PALAM, this));
			varTokenDic.Add("SOURCE", new CharaInt1DVariableToken(VariableCode.SOURCE, this));
			varTokenDic.Add("EX", new CharaInt1DVariableToken(VariableCode.EX, this));
			varTokenDic.Add("CFLAG", new CharaInt1DVariableToken(VariableCode.CFLAG, this));
			varTokenDic.Add("JUEL", new CharaInt1DVariableToken(VariableCode.JUEL, this));
			varTokenDic.Add("RELATION", new CharaInt1DVariableToken(VariableCode.RELATION, this));
			varTokenDic.Add("EQUIP", new CharaInt1DVariableToken(VariableCode.EQUIP, this));
			varTokenDic.Add("TEQUIP", new CharaInt1DVariableToken(VariableCode.TEQUIP, this));
			varTokenDic.Add("STAIN", new CharaInt1DVariableToken(VariableCode.STAIN, this));
			varTokenDic.Add("GOTJUEL", new CharaInt1DVariableToken(VariableCode.GOTJUEL, this));
			varTokenDic.Add("NOWEX", new CharaInt1DVariableToken(VariableCode.NOWEX, this));
			varTokenDic.Add("DOWNBASE", new CharaInt1DVariableToken(VariableCode.DOWNBASE, this));
			varTokenDic.Add("CUP", new CharaInt1DVariableToken(VariableCode.CUP, this));
			varTokenDic.Add("CDOWN", new CharaInt1DVariableToken(VariableCode.CDOWN, this));
			varTokenDic.Add("TCVAR", new CharaInt1DVariableToken(VariableCode.TCVAR, this));

			varTokenDic.Add("NAME", new CharaStrVariableToken(VariableCode.NAME, this));
			varTokenDic.Add("CALLNAME", new CharaStrVariableToken(VariableCode.CALLNAME, this));
			varTokenDic.Add("NICKNAME", new CharaStrVariableToken(VariableCode.NICKNAME, this));
			varTokenDic.Add("MASTERNAME", new CharaStrVariableToken(VariableCode.MASTERNAME, this));

			varTokenDic.Add("CSTR", new CharaStr1DVariableToken(VariableCode.CSTR, this));

			varTokenDic.Add("CDFLAG", new CharaInt2DVariableToken(VariableCode.CDFLAG, this));

			varTokenDic.Add("DITEMTYPE", new Int2DVariableToken(VariableCode.DITEMTYPE, this));
			varTokenDic.Add("DA", new Int2DVariableToken(VariableCode.DA, this));
			varTokenDic.Add("DB", new Int2DVariableToken(VariableCode.DB, this));
			varTokenDic.Add("DC", new Int2DVariableToken(VariableCode.DC, this));
			varTokenDic.Add("DD", new Int2DVariableToken(VariableCode.DD, this));
			varTokenDic.Add("DE", new Int2DVariableToken(VariableCode.DE, this));

			varTokenDic.Add("TA", new Int3DVariableToken(VariableCode.TA, this));
			varTokenDic.Add("TB", new Int3DVariableToken(VariableCode.TB, this));


			varTokenDic.Add("ITEMPRICE", new Int1DConstantToken(VariableCode.ITEMPRICE, this, constant.ItemPrice));
			varTokenDic.Add("ABLNAME", new Str1DConstantToken(VariableCode.ABLNAME, this));
			varTokenDic.Add("TALENTNAME", new Str1DConstantToken(VariableCode.TALENTNAME, this));
			varTokenDic.Add("EXPNAME", new Str1DConstantToken(VariableCode.EXPNAME, this));
			varTokenDic.Add("MARKNAME", new Str1DConstantToken(VariableCode.MARKNAME, this));
			varTokenDic.Add("PALAMNAME", new Str1DConstantToken(VariableCode.PALAMNAME, this));
			varTokenDic.Add("ITEMNAME", new Str1DConstantToken(VariableCode.ITEMNAME, this));
			varTokenDic.Add("TRAINNAME", new Str1DConstantToken(VariableCode.TRAINNAME, this));
			varTokenDic.Add("BASENAME", new Str1DConstantToken(VariableCode.BASENAME, this));
			varTokenDic.Add("SOURCENAME", new Str1DConstantToken(VariableCode.SOURCENAME, this));
			varTokenDic.Add("EXNAME", new Str1DConstantToken(VariableCode.EXNAME, this));
			varTokenDic.Add("EQUIPNAME", new Str1DConstantToken(VariableCode.EQUIPNAME, this));
			varTokenDic.Add("TEQUIPNAME", new Str1DConstantToken(VariableCode.TEQUIPNAME, this));
			varTokenDic.Add("FLAGNAME", new Str1DConstantToken(VariableCode.FLAGNAME, this));
			varTokenDic.Add("TFLAGNAME", new Str1DConstantToken(VariableCode.TFLAGNAME, this));
			varTokenDic.Add("CFLAGNAME", new Str1DConstantToken(VariableCode.CFLAGNAME, this));
			varTokenDic.Add("TCVARNAME", new Str1DConstantToken(VariableCode.TCVARNAME, this));
			varTokenDic.Add("CSTRNAME", new Str1DConstantToken(VariableCode.CSTRNAME, this));
			varTokenDic.Add("STAINNAME", new Str1DConstantToken(VariableCode.STAINNAME, this));

			varTokenDic.Add("CDFLAGNAME1", new Str1DConstantToken(VariableCode.CDFLAGNAME1, this));
			varTokenDic.Add("CDFLAGNAME2", new Str1DConstantToken(VariableCode.CDFLAGNAME2, this));
			varTokenDic.Add("STRNAME", new Str1DConstantToken(VariableCode.STRNAME, this));
			varTokenDic.Add("TSTRNAME", new Str1DConstantToken(VariableCode.TSTRNAME, this));
			varTokenDic.Add("SAVESTRNAME", new Str1DConstantToken(VariableCode.SAVESTRNAME, this));
			varTokenDic.Add("GLOBALNAME", new Str1DConstantToken(VariableCode.GLOBALNAME, this));
			varTokenDic.Add("GLOBALSNAME", new Str1DConstantToken(VariableCode.GLOBALSNAME, this));

			StrConstantToken token = new StrConstantToken(VariableCode.GAMEBASE_AUTHOR, this, gamebase.ScriptAutherName);
			varTokenDic.Add("GAMEBASE_AUTHER", token);
			varTokenDic.Add("GAMEBASE_AUTHOR", token);
			varTokenDic.Add("GAMEBASE_INFO", new StrConstantToken(VariableCode.GAMEBASE_INFO, this, gamebase.ScriptDetail));
			varTokenDic.Add("GAMEBASE_YEAR", new StrConstantToken(VariableCode.GAMEBASE_YEAR, this, gamebase.ScriptYear));
			varTokenDic.Add("GAMEBASE_TITLE", new StrConstantToken(VariableCode.GAMEBASE_TITLE, this, gamebase.ScriptTitle));


			varTokenDic.Add("GAMEBASE_GAMECODE", new IntConstantToken(VariableCode.GAMEBASE_GAMECODE, this, gamebase.ScriptUniqueCode));
			varTokenDic.Add("GAMEBASE_VERSION", new IntConstantToken(VariableCode.GAMEBASE_VERSION, this, gamebase.ScriptVersion));
			varTokenDic.Add("GAMEBASE_ALLOWVERSION", new IntConstantToken(VariableCode.GAMEBASE_ALLOWVERSION, this, gamebase.ScriptCompatibleMinVersion));
			varTokenDic.Add("GAMEBASE_DEFAULTCHARA", new IntConstantToken(VariableCode.GAMEBASE_DEFAULTCHARA, this, gamebase.DefaultCharacter));
			varTokenDic.Add("GAMEBASE_NOITEM", new IntConstantToken(VariableCode.GAMEBASE_NOITEM, this, gamebase.DefaultNoItem));

			VariableToken rand = null;
			if (Config.CompatiRAND)
				rand = new CompatiRandToken(VariableCode.RAND, this);
			else
				rand = new RandToken(VariableCode.RAND, this);
			varTokenDic.Add("RAND", rand);
			varTokenDic.Add("CHARANUM", new CHARANUM_Token(VariableCode.CHARANUM, this));


			varTokenDic.Add("LASTLOAD_TEXT", new LASTLOAD_TEXT_Token(VariableCode.LASTLOAD_TEXT, this));
			varTokenDic.Add("LASTLOAD_VERSION", new LASTLOAD_VERSION_Token(VariableCode.LASTLOAD_VERSION, this));
			varTokenDic.Add("LASTLOAD_NO", new LASTLOAD_NO_Token(VariableCode.LASTLOAD_NO, this));
			varTokenDic.Add("LINECOUNT", new LINECOUNT_Token(VariableCode.LINECOUNT, this));
            varTokenDic.Add("ISTIMEOUT", new ISTIMEOUTToken(VariableCode.ISTIMEOUT, this));
			varTokenDic.Add("__INT_MAX__", new __INT_MAX__Token(VariableCode.__INT_MAX__, this));
			varTokenDic.Add("__INT_MIN__", new __INT_MIN__Token(VariableCode.__INT_MIN__, this));
            varTokenDic.Add("EMUERA_VERSION", new EMUERA_VERSIONToken(VariableCode.EMUERA_VERSION, this));

			varTokenDic.Add("WINDOW_TITLE", new WINDOW_TITLE_Token(VariableCode.WINDOW_TITLE, this));
			varTokenDic.Add("MONEYLABEL", new MONEYLABEL_Token(VariableCode.MONEYLABEL, this));
			varTokenDic.Add("DRAWLINESTR", new DRAWLINESTR_Token(VariableCode.DRAWLINESTR, this));
			if (!Program.DebugMode)
			{
				varTokenDic.Add("__FILE__", new EmptyStrToken(VariableCode.__FILE__, this));
				varTokenDic.Add("__FUNCTION__", new EmptyStrToken(VariableCode.__FUNCTION__, this));
				varTokenDic.Add("__LINE__", new EmptyIntToken(VariableCode.__LINE__, this));
			}
			else
			{
				varTokenDic.Add("__FILE__", new Debug__FILE__Token(VariableCode.__FILE__, this));
				varTokenDic.Add("__FUNCTION__", new Debug__FUNCTION__Token(VariableCode.__FUNCTION__, this));
				varTokenDic.Add("__LINE__", new Debug__LINE__Token(VariableCode.__LINE__, this));
			}

			int size = constant.VariableIntArrayLength[(int)(VariableCode.__LOWERCASE__ & VariableCode.LOCAL)];
			localvarTokenDic.Add("LOCAL", new VariableLocal(VariableCode.LOCAL, size, CreateLocalInt));
			size = constant.VariableIntArrayLength[(int)(VariableCode.__LOWERCASE__ & VariableCode.ARG)];
			localvarTokenDic.Add("ARG", new VariableLocal(VariableCode.ARG, size, CreateLocalInt));
			size = constant.VariableStrArrayLength[(int)(VariableCode.__LOWERCASE__ & VariableCode.LOCALS)];
			localvarTokenDic.Add("LOCALS", new VariableLocal(VariableCode.LOCALS, size, CreateLocalStr));
			size = constant.VariableStrArrayLength[(int)(VariableCode.__LOWERCASE__ & VariableCode.ARGS)];
			localvarTokenDic.Add("ARGS", new VariableLocal(VariableCode.ARGS, size, CreateLocalStr));

		}

		private LocalVariableToken CreateLocalInt(VariableCode varCode, string subKey, int size)
		{
			return new LocalInt1DVariableToken(varCode, this, subKey, size);
		}
		private LocalVariableToken CreateLocalStr(VariableCode varCode, string subKey, int size)
		{
			return new LocalStr1DVariableToken(varCode, this, subKey, size);
		}
		public Dictionary<string, VariableToken> GetVarTokenDicClone()
		{
			Dictionary<string, VariableToken> clone = new Dictionary<string, VariableToken>();
			foreach (KeyValuePair<string, VariableToken> pair in varTokenDic)
				clone.Add(pair.Key, pair.Value);
			return clone;
		}
		public Dictionary<string, VariableToken> GetVarTokenDic()
		{
			return varTokenDic;
		}
		public Dictionary<string, VariableLocal> GetLocalvarTokenDic() { return localvarTokenDic; }
		public VariableToken GetSystemVariableToken(string str)
		{
			return varTokenDic[str];
		}


		public UserDefinedCharaVariableToken CreateUserDefCharaVariable(UserDefinedVariableData data)
		{
			UserDefinedCharaVariableToken ret = null;
			if (data.CharaData)
			{
				int index = UserDefinedCharaVarList.Count;
				if (data.TypeIsStr)
					switch (data.Dimension)
					{
						case 1: ret = new UserDefinedCharaStr1DVariableToken(data, this, index); break;
						case 2: ret = new UserDefinedCharaStr2DVariableToken(data, this, index); break;
						default: throw new ExeEE("異常な変数宣言");
					}
				else
					switch (data.Dimension)
					{
						case 1: ret = new UserDefinedCharaInt1DVariableToken(data, this, index); break;
						case 2: ret = new UserDefinedCharaInt2DVariableToken(data, this, index); break;
						default: throw new ExeEE("異常な変数宣言");
					}
			}
			UserDefinedCharaVarList.Add(ret);
			return ret;
		}
		public UserDefinedVariableToken CreateUserDefVariable(UserDefinedVariableData data)
		{
			UserDefinedVariableToken ret = null;
			if (data.TypeIsStr)
				switch (data.Dimension)
				{
					case 1: ret = new StaticStr1DVariableToken(data); break;
					case 2: ret = new StaticStr2DVariableToken(data); break;
					case 3: ret = new StaticStr3DVariableToken(data); break;
					default: throw new ExeEE("異常な変数宣言");
				}
			else
				switch (data.Dimension)
				{
					case 1: ret = new StaticInt1DVariableToken(data); break;
					case 2: ret = new StaticInt2DVariableToken(data); break;
					case 3: ret = new StaticInt3DVariableToken(data); break;
					default: throw new ExeEE("異常な変数宣言");
				}
			if (ret.IsGlobal)
				userDefinedGlobalVarList.Add(ret);
			else
				userDefinedStaticVarList.Add(ret);
			if (ret.IsSavedata)
			{
				int type = ret.Dimension * 2 - 2;
				if (!ret.IsString)
					type++;
				if (ret.IsGlobal)
					userDefinedGlobalSaveVarList[type].Add(ret);
				else
					userDefinedSaveVarList[type].Add(ret);
			}
			return ret;
		}

		public UserDefinedVariableToken CreatePrivateVariable(UserDefinedVariableData data)
		{
			UserDefinedVariableToken ret = null;
			if (data.Reference)//参照型
			{//すべて非Staticなはず
				if (data.TypeIsStr)
				{
					switch (data.Dimension)
					{
						case 1: ret = new ReferenceStr1DToken(data); break;
						case 2: ret = new ReferenceStr2DToken(data); break;
						case 3: ret = new ReferenceStr3DToken(data); break;
						default: throw new ExeEE("異常な変数宣言");
					}
				}
				else
				{
					switch (data.Dimension)
					{
						case 1: ret = new ReferenceInt1DToken(data); break;
						case 2: ret = new ReferenceInt2DToken(data); break;
						case 3: ret = new ReferenceInt3DToken(data); break;
						default: throw new ExeEE("異常な変数宣言");
					}
				}
			}
			else if (data.Static)
			{
				if (data.TypeIsStr)
				{
					switch (data.Dimension)
					{
						case 1: ret = new StaticStr1DVariableToken(data); break;
						case 2: ret = new StaticStr2DVariableToken(data); break;
						case 3: ret = new StaticStr3DVariableToken(data); break;
						default: throw new ExeEE("異常な変数宣言");
					}
				}
				else
				{
					switch (data.Dimension)
					{
						case 1: ret = new StaticInt1DVariableToken(data); break;
						case 2: ret = new StaticInt2DVariableToken(data); break;
						case 3: ret = new StaticInt3DVariableToken(data); break;
						default: throw new ExeEE("異常な変数宣言");
					}
				}
				userDefinedStaticVarList.Add(ret);
			}
			else
			{
				if (data.TypeIsStr)
				{
					switch (data.Dimension)
					{
						case 1: ret = new PrivateStr1DVariableToken(data); break;
						case 2: ret = new PrivateStr2DVariableToken(data); break;
						case 3: ret = new PrivateStr3DVariableToken(data); break;
						default: throw new ExeEE("異常な変数宣言");
					}
				}
				else
				{
					switch (data.Dimension)
					{
						case 1: ret = new PrivateInt1DVariableToken(data); break;
						case 2: ret = new PrivateInt2DVariableToken(data); break;
						case 3: ret = new PrivateInt3DVariableToken(data); break;
						default: throw new ExeEE("異常な変数宣言");
					}
				}
			}
			return ret;
		}

		public void SetDefaultGlobalValue()
		{

			Int64[] globalInt = dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.GLOBAL];
			string[] globalStr = dataStringArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.GLOBALS];
			for (int i = 0; i < globalInt.Length; i++)
				globalInt[i] = 0;
			for (int i = 0; i < globalStr.Length; i++)
				globalStr[i] = null;
			foreach (UserDefinedVariableToken var in userDefinedGlobalVarList)
				var.SetDefault();
		}

		public void SetDefaultLocalValue()
		{
			foreach (VariableLocal local in localvarTokenDic.Values)
				local.SetDefault();
			foreach (UserDefinedVariableToken var in userDefinedStaticVarList)
				var.SetDefault();
		}

		public void ClearLocalValue()
		{
			foreach (VariableLocal local in localvarTokenDic.Values)
				local.Clear();
		}


		/// <summary>
		/// ローカルとグローバル以外初期化
		/// </summary>
		public void SetDefaultValue(ConstantData constant)
		{

			for (int i = 0; i < dataInteger.Length; i++)
				dataInteger[i] = 0;

			for (int i = 0; i < dataIntegerArray.Length; i++)
			{
				switch (i)
				{
					case (int)(VariableCode.__LOWERCASE__ & VariableCode.GLOBAL):
						break;
					case (int)(VariableCode.__LOWERCASE__ & VariableCode.ITEMPRICE):
						//constant.ItemPrice.CopyTo(dataIntegerArray[i], 0);
						Buffer.BlockCopy(constant.ItemPrice, 0, dataIntegerArray[i], 0, 8 * dataIntegerArray[i].Length);
						break;
					default:
						for (int j = 0; j < dataIntegerArray[i].Length; j++)
							dataIntegerArray[i][j] = 0;
						break;
				}
			}

			for (int i = 0; i < dataString.Length; i++)
				dataString[i] = null;

			for (int i = 0; i < dataStringArray.Length; i++)
			{
				switch (i)
				{
					case (int)(VariableCode.__LOWERCASE__ & VariableCode.GLOBALS):
						break;
					case (int)(VariableCode.__LOWERCASE__ & VariableCode.STR):
						{
							string[] csvStrData = constant.GetCsvNameList(VariableCode.__DUMMY_STR__);
							csvStrData.CopyTo(dataStringArray[i], 0);
							break;
						}
					default:
						for (int j = 0; j < dataStringArray[i].Length; j++)
							dataStringArray[i][j] = null;
						break;
				}
			}
			for (int i = 0; i < dataIntegerArray2D.Length; i++)
			{
				Int64[,] array2D = dataIntegerArray2D[i];
				int length0 = array2D.GetLength(0);
				int length1 = array2D.GetLength(1);
				for (int x = 0; x < length0; x++)
					for (int y = 0; y < length1; y++)
						array2D[x, y] = 0;
			}
			for (int i = 0; i < dataStringArray2D.Length; i++)
			{
				string[,] array2D = dataStringArray2D[i];
				int length0 = array2D.GetLength(0);
				int length1 = array2D.GetLength(1);
				for (int x = 0; x < length0; x++)
					for (int y = 0; y < length1; y++)
						array2D[x, y] = null;
			}
			for (int i = 0; i < dataIntegerArray3D.Length; i++)
			{
				Int64[, ,] array3D = dataIntegerArray3D[i];
				int length0 = array3D.GetLength(0);
				int length1 = array3D.GetLength(1);
				int length2 = array3D.GetLength(2);
				for (int x = 0; x < length0; x++)
					for (int y = 0; y < length1; y++)
						for (int z = 0; z < length2; z++)
							array3D[x, y, z] = 0;
			}
			for (int i = 0; i < dataStringArray3D.Length; i++)
			{
				string[, ,] array3D = dataStringArray3D[i];
				int length0 = array3D.GetLength(0);
				int length1 = array3D.GetLength(1);
				int length2 = array3D.GetLength(2);
				for (int x = 0; x < length0; x++)
					for (int y = 0; y < length1; y++)
						for (int z = 0; z < length2; z++)
							array3D[x, y, z] = null;
			}

			Int64[] palamlv = dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.PALAMLV];
			List<Int64> defPalam = Config.PalamLvDef;
			defPalam.CopyTo(0, palamlv, 0, Math.Min(palamlv.Length, defPalam.Count));
			//palamlv[0] = 0;
			//palamlv[1] = 100;
			//palamlv[2] = 500;
			//palamlv[3] = 3000;
			//palamlv[4] = 10000;
			//palamlv[5] = 30000;
			//palamlv[6] = 60000;
			//palamlv[7] = 100000;
			//palamlv[8] = 150000;
			//palamlv[9] = 250000;

			Int64[] explv = dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.EXPLV];
			List<Int64> defExpLv = Config.ExpLvDef;
			defExpLv.CopyTo(0, explv, 0, Math.Min(explv.Length, defExpLv.Count));
			//explv[0] = 0;
			//explv[1] = 1;
			//explv[2] = 4;
			//explv[3] = 20;
			//explv[4] = 50;
			//explv[5] = 200;

			//dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.ASSIPLAY][0] = 0;
			//dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.MASTER][0] = 0;
			long[] array;
			array = dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.ASSI];
			if (array.Length > 0) array[0] = -1;
			array = dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.TARGET];
			if (array.Length > 0) array[0] = 1;
			array = dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.PBAND];
			if (array.Length > 0) array[0] = Config.PbandDef;
			array = dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.EJAC];
			if (array.Length > 0) array[0] = 10000;

			LastLoadVersion = -1;
			LastLoadNo = -1;
			LastLoadText = "";
		}


		const int strCount = (int)VariableCode.__COUNT_SAVE_STRING__;
		const int intCount = (int)VariableCode.__COUNT_SAVE_INTEGER__;
		const int intArrayCount = (int)VariableCode.__COUNT_SAVE_INTEGER_ARRAY__;
		const int strArrayCount = (int)VariableCode.__COUNT_SAVE_STRING_ARRAY__;
		public void SaveToStream(EraDataWriter writer)
		{

			for (int i = 0; i < strCount; i++)
				writer.Write(dataString[i]);
			for (int i = 0; i < intCount; i++)
				writer.Write(dataInteger[i]);
			for (int i = 0; i < intArrayCount; i++)
				writer.Write(dataIntegerArray[i]);
			for (int i = 0; i < strArrayCount; i++)
				writer.Write(dataStringArray[i]);
		}

		public void LoadFromStream(EraDataReader reader)
		{

			for (int i = 0; i < strCount; i++)
				dataString[i] = reader.ReadString();
			for (int i = 0; i < intCount; i++)
				dataInteger[i] = reader.ReadInt64();
			for (int i = 0; i < intArrayCount; i++)
				reader.ReadInt64Array(dataIntegerArray[i]);
			for (int i = 0; i < strArrayCount; i++)
				reader.ReadStringArray(dataStringArray[i]);
		}

		public void SaveToStreamExtended(EraDataWriter writer)
		{
			List<VariableCode> codeList = null;

			//dataString
			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__STRING__);
			foreach (VariableCode code in codeList)
				writer.WriteExtended(code.ToString(), dataString[(int)VariableCode.__LOWERCASE__ & (int)code]);
			writer.EmuSeparete();

			//datainteger
			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__INTEGER__);
			foreach (VariableCode code in codeList)
				writer.WriteExtended(code.ToString(), dataInteger[(int)VariableCode.__LOWERCASE__ & (int)code]);
			writer.EmuSeparete();

			//dataStringArray
			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__ARRAY_1D__ | VariableCode.__STRING__);
			foreach (VariableCode code in codeList)
				writer.WriteExtended(code.ToString(), dataStringArray[(int)VariableCode.__LOWERCASE__ & (int)code]);
			writer.EmuSeparete();

			//dataIntegerArray
			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__ARRAY_1D__ | VariableCode.__INTEGER__);
			foreach (VariableCode code in codeList)
				writer.WriteExtended(code.ToString(), dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)code]);
			writer.EmuSeparete();

			//dataStringArray2D
			//StringArray2Dの保存は未実装
			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__ARRAY_2D__ | VariableCode.__STRING__);
			foreach (VariableCode code in codeList)
				writer.WriteExtended(code.ToString(), dataStringArray2D[(int)VariableCode.__LOWERCASE__ & (int)code]);
			writer.EmuSeparete();

			//dataIntegerArray2D
			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__ARRAY_2D__ | VariableCode.__INTEGER__);
			foreach (VariableCode code in codeList)
				writer.WriteExtended(code.ToString(), dataIntegerArray2D[(int)VariableCode.__LOWERCASE__ & (int)code]);
			writer.EmuSeparete();

			//dataStringArray3D
			//StringArray3Dの保存は未実装
			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__ARRAY_3D__ | VariableCode.__STRING__);
			foreach (VariableCode code in codeList)
				writer.WriteExtended(code.ToString(), dataStringArray3D[(int)VariableCode.__LOWERCASE__ & (int)code]);
			writer.EmuSeparete();

			//dataIntegerArray3D
			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__ARRAY_3D__ | VariableCode.__INTEGER__);
			foreach (VariableCode code in codeList)
				writer.WriteExtended(code.ToString(), dataIntegerArray3D[(int)VariableCode.__LOWERCASE__ & (int)code]);
			writer.EmuSeparete();

			for (int i = 0; i < 6; i++)
			{
				foreach (UserDefinedVariableToken var in userDefinedSaveVarList[i])
				{
					//if (!var.IsSavedata) continue;
					switch (i)
					{
						case 0: writer.WriteExtended(var.Name, (string[])var.GetArray()); break;
						case 1: writer.WriteExtended(var.Name, (Int64[])var.GetArray()); break;
						case 2: writer.WriteExtended(var.Name, (string[,])var.GetArray()); break;
						case 3: writer.WriteExtended(var.Name, (Int64[,])var.GetArray()); break;
						case 4: writer.WriteExtended(var.Name, (string[, ,])var.GetArray()); break;
						case 5: writer.WriteExtended(var.Name, (Int64[, ,])var.GetArray()); break;
					}
				}
				writer.EmuSeparete();
			}
		}


		public void LoadFromStreamExtended(EraDataReader reader, int version)
		{
			Dictionary<string, string> strDic = reader.ReadStringExtended();
			Dictionary<string, Int64> intDic = reader.ReadInt64Extended();
			Dictionary<string, List<string>> strListDic = reader.ReadStringArrayExtended();
			Dictionary<string, List<Int64>> intListDic = reader.ReadInt64ArrayExtended();
			Dictionary<string, List<string[]>> str2DListDic = reader.ReadStringArray2DExtended();
			Dictionary<string, List<Int64[]>> int2DListDic = reader.ReadInt64Array2DExtended();
			Dictionary<string, List<List<string[]>>> str3DListDic = reader.ReadStringArray3DExtended();
			Dictionary<string, List<List<Int64[]>>> int3DListDic = reader.ReadInt64Array3DExtended();
			List<VariableCode> codeList = null;

            codeList = VariableIdentifier.GetExtSaveList(VariableCode.__STRING__);
			foreach (VariableCode code in codeList)
				if (strDic.TryGetValue(code.ToString(), out var sfound))
					dataString[(int)VariableCode.__LOWERCASE__ & (int)code] = sfound;

			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__INTEGER__);
			foreach (VariableCode code in codeList)
				if (intDic.TryGetValue(code.ToString(), out var lfound))
					dataInteger[(int)VariableCode.__LOWERCASE__ & (int)code] = lfound;


			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__ARRAY_1D__ | VariableCode.__STRING__);
			foreach (VariableCode code in codeList)
				if (strListDic.TryGetValue(code.ToString(), out var listfound))
					copyListToArray(listfound, dataStringArray[(int)VariableCode.__LOWERCASE__ & (int)code]);

			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__ARRAY_1D__ | VariableCode.__INTEGER__);
			foreach (VariableCode code in codeList)
				if (intListDic.TryGetValue(code.ToString(), out var listlongfound))
					copyListToArray(listlongfound, dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)code]);

			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__ARRAY_2D__ | VariableCode.__STRING__);
			foreach (VariableCode code in codeList)
				if (str2DListDic.TryGetValue(code.ToString(), out var liststrarrfound))
					copyListToArray2D(liststrarrfound, dataStringArray2D[(int)VariableCode.__LOWERCASE__ & (int)code]);

			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__ARRAY_2D__ | VariableCode.__INTEGER__);
			foreach (VariableCode code in codeList)
				if (int2DListDic.TryGetValue(code.ToString(), out var listlongarrfound))
					copyListToArray2D(listlongarrfound, dataIntegerArray2D[(int)VariableCode.__LOWERCASE__ & (int)code]);

			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__ARRAY_3D__ | VariableCode.__STRING__);
			foreach (VariableCode code in codeList)
				if (str3DListDic.TryGetValue(code.ToString(), out var listliststrarrfound))
					copyListToArray3D(listliststrarrfound, dataStringArray3D[(int)VariableCode.__LOWERCASE__ & (int)code]);

			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__ARRAY_3D__ | VariableCode.__INTEGER__);
			foreach (VariableCode code in codeList)
				if (int3DListDic.TryGetValue(code.ToString(), out var listlistlongarrfound))
					copyListToArray3D(listlistlongarrfound, dataIntegerArray3D[(int)VariableCode.__LOWERCASE__ & (int)code]);

			if (version < 1808)//ユーザー定義変数の保存の実装前
				return;

			strListDic = reader.ReadStringArrayExtended();
			intListDic = reader.ReadInt64ArrayExtended();
			str2DListDic = reader.ReadStringArray2DExtended();
			int2DListDic = reader.ReadInt64Array2DExtended();
			str3DListDic = reader.ReadStringArray3DExtended();
			int3DListDic = reader.ReadInt64Array3DExtended();
			List<UserDefinedVariableToken> varList = null;

			int i = 0;
			varList = userDefinedSaveVarList[i]; i++;
			foreach (UserDefinedVariableToken var in varList)
				if (strListDic.TryGetValue(var.Name, out var listfound))
					copyListToArray(listfound, (string[])var.GetArray());

			varList = userDefinedSaveVarList[i]; i++;
			foreach (UserDefinedVariableToken var in varList)
				if (intListDic.TryGetValue(var.Name, out var listlongfound))
					copyListToArray(listlongfound, (Int64[])var.GetArray());

			varList = userDefinedSaveVarList[i]; i++;
			foreach (UserDefinedVariableToken var in varList)
				if (str2DListDic.TryGetValue(var.Name, out var liststrarrfound))
					copyListToArray2D(liststrarrfound, (string[,])var.GetArray());

			varList = userDefinedSaveVarList[i]; i++;
			foreach (UserDefinedVariableToken var in varList)
				if (int2DListDic.TryGetValue(var.Name, out var listlongarrfound))
					copyListToArray2D(listlongarrfound, (Int64[,])var.GetArray());

			varList = userDefinedSaveVarList[i]; i++;
			foreach (UserDefinedVariableToken var in varList)
				if (str3DListDic.TryGetValue(var.Name, out var listliststrarrfound))
					copyListToArray3D(listliststrarrfound, (string[, ,])var.GetArray());

			varList = userDefinedSaveVarList[i]; i++;
			foreach (UserDefinedVariableToken var in varList)
				if (int3DListDic.TryGetValue(var.Name, out var listlistlongarrfound))
					copyListToArray3D(listlistlongarrfound, (Int64[, ,])var.GetArray());
		}

		private void copyListToArray<T>(List<T> srcList, T[] destArray)
		{
			int count = Math.Min(srcList.Count, destArray.Length);
			for (int i = 0; i < count; i++)
			{
				destArray[i] = srcList[i];
			}
		}

		private void copyListToArray2D<T>(List<T[]> srcList, T[,] destArray)
		{
			int countX = Math.Min(srcList.Count, destArray.GetLength(0));
			int dLength = destArray.GetLength(1);
			for (int x = 0; x < countX; x++)
			{
				T[] srcArray = srcList[x];
				int countY = Math.Min(srcArray.Length, dLength);
				for (int y = 0; y < countY; y++)
				{
					destArray[x, y] = srcArray[y];
				}
			}
		}
		private void copyListToArray3D<T>(List<List<T[]>> srcList, T[, ,] destArray)
		{
			int countX = Math.Min(srcList.Count, destArray.GetLength(0));
			int dLength1 = destArray.GetLength(1);
			int dLength2 = destArray.GetLength(2);
			for (int x = 0; x < countX; x++)
			{
				List<T[]> srcArray = srcList[x];
				int countY = Math.Min(srcArray.Count, dLength1);
				for (int y = 0; y < countY; y++)
				{
					T[] baseArray = srcArray[y];
					int countZ = Math.Min(baseArray.Length, dLength2);
					for (int z = 0; z < countZ; z++)
					{
						destArray[x, y, z] = baseArray[z];
					}
				}
			}
		}


		public void SaveGlobalToStream(EraDataWriter writer)
		{
			writer.Write(dataIntegerArray[(int)(VariableCode.__LOWERCASE__ & VariableCode.GLOBAL)]);
			writer.Write(dataStringArray[(int)(VariableCode.__LOWERCASE__ & VariableCode.GLOBALS)]);
		}

		public void LoadGlobalFromStream(EraDataReader reader)
		{
			reader.ReadInt64Array(dataIntegerArray[(int)(VariableCode.__LOWERCASE__ & VariableCode.GLOBAL)]);
			reader.ReadStringArray(dataStringArray[(int)(VariableCode.__LOWERCASE__ & VariableCode.GLOBALS)]);
		}

		public void SaveGlobalToStream1808(EraDataWriter writer)
		{
			for (int i = 0; i < 6; i++)
			{
				foreach (UserDefinedVariableToken var in userDefinedGlobalSaveVarList[i])
				{
					//if (!var.IsSavedata) continue;
					switch (i)
					{
						case 0: writer.WriteExtended(var.Name, (string[])var.GetArray()); break;
						case 1: writer.WriteExtended(var.Name, (Int64[])var.GetArray()); break;
						case 2: writer.WriteExtended(var.Name, (string[,])var.GetArray()); break;
						case 3: writer.WriteExtended(var.Name, (Int64[,])var.GetArray()); break;
						case 4: writer.WriteExtended(var.Name, (string[, ,])var.GetArray()); break;
						case 5: writer.WriteExtended(var.Name, (Int64[, ,])var.GetArray()); break;
					}
				}
				writer.EmuSeparete();
			}
		}

		public void LoadGlobalFromStream1808(EraDataReader reader)
		{
			Dictionary<string, List<string>> strListDic = reader.ReadStringArrayExtended();
			Dictionary<string, List<Int64>> intListDic = reader.ReadInt64ArrayExtended();
			Dictionary<string, List<string[]>> str2DListDic = reader.ReadStringArray2DExtended();
			Dictionary<string, List<Int64[]>> int2DListDic = reader.ReadInt64Array2DExtended();
			Dictionary<string, List<List<string[]>>> str3DListDic = reader.ReadStringArray3DExtended();
			Dictionary<string, List<List<Int64[]>>> int3DListDic = reader.ReadInt64Array3DExtended();

			List<UserDefinedVariableToken> varList = null;
			
            int i = 0;
			varList = userDefinedGlobalSaveVarList[i]; i++;
			foreach (UserDefinedVariableToken var in varList)
				if (strListDic.TryGetValue(var.Name, out var listfound))
					copyListToArray(listfound, (string[])var.GetArray());

			varList = userDefinedGlobalSaveVarList[i]; i++;
			foreach (UserDefinedVariableToken var in varList)
				if (intListDic.TryGetValue(var.Name, out var listlongfound))
					copyListToArray(listlongfound, (Int64[])var.GetArray());

			varList = userDefinedGlobalSaveVarList[i]; i++;
			foreach (UserDefinedVariableToken var in varList)
				if (str2DListDic.TryGetValue(var.Name, out var liststrarrfound))
					copyListToArray2D(liststrarrfound, (string[,])var.GetArray());

			varList = userDefinedGlobalSaveVarList[i]; i++;
			foreach (UserDefinedVariableToken var in varList)
				if (int2DListDic.TryGetValue(var.Name, out var listlongarrfound))
					copyListToArray2D(listlongarrfound, (Int64[,])var.GetArray());

			varList = userDefinedGlobalSaveVarList[i]; i++;
			foreach (UserDefinedVariableToken var in varList)
				if (str3DListDic.TryGetValue(var.Name, out var listliststrarrfound))
					copyListToArray3D(listliststrarrfound, (string[, ,])var.GetArray());

			varList = userDefinedGlobalSaveVarList[i]; i++;
			foreach (UserDefinedVariableToken var in varList)
				if (int3DListDic.TryGetValue(var.Name, out var listlistlongarrfound))
					copyListToArray3D(listlistlongarrfound, (Int64[, ,])var.GetArray());
		}

		public void SaveGlobalToStreamBinary(EraBinaryDataWriter writer)
		{
			foreach (KeyValuePair<string, VariableToken> pair in varTokenDic)
			{
				VariableToken var = pair.Value;
				if (var.IsSavedata && !var.IsCharacterData && var.IsGlobal)
				{
					writer.WriteWithKey(pair.Key, var.GetArray());
				}
			}
			foreach (UserDefinedVariableToken var in userDefinedGlobalVarList)
			{
				if (var.IsSavedata)
					writer.WriteWithKey(var.Name, var.GetArray());
			}
		}

		public void SaveToStreamBinary(EraBinaryDataWriter writer)
		{
			foreach (KeyValuePair<string, VariableToken> pair in varTokenDic)
			{
				VariableToken var = pair.Value;
				if (var.IsSavedata && !var.IsCharacterData && !var.IsGlobal)
				{
					writer.WriteWithKey(pair.Key, var.GetArray());
				}
			}
			foreach (UserDefinedVariableToken var in userDefinedStaticVarList)
			{
				if (var.IsSavedata)
					writer.WriteWithKey(var.Name, var.GetArray());
			}
		}

		public void LoadFromStreamBinary(EraBinaryDataReader bReader)
		{
			while (LoadVariableBinary(bReader)) { }
		}

		/// <summary>
		/// 1808 キャラクタ型でない変数を一つ読む
		/// ファイル終端の場合はfalseを返す
		/// </summary>
		/// <param name="reader"></param>
		public bool LoadVariableBinary(EraBinaryDataReader reader)
		{
			KeyValuePair<string, EraSaveDataType> nameAndType = reader.ReadVariableCode();
			VariableToken vToken = null;
			if (nameAndType.Key != null && !GlobalStatic.IdentifierDictionary.getVarTokenIsForbid(nameAndType.Key))
				vToken = GlobalStatic.IdentifierDictionary.GetVariableToken(nameAndType.Key, null, false);
			if (vToken != null && (vToken.IsCharacterData || vToken.IsConst || vToken.IsPrivate || vToken.IsLocal || vToken.IsCalc))
				vToken = null;
			switch (nameAndType.Value)
			{
				case EraSaveDataType.EOF:
					return false;
				case EraSaveDataType.Int:
					if (vToken == null || !vToken.IsInteger || vToken.Dimension != 0)
						reader.ReadInt();//該当変数なし、or型不一致なら読み捨てる
					else
						vToken.SetValue(reader.ReadInt(), null);
					break;
				case EraSaveDataType.Str:
					if (vToken == null || !vToken.IsString || vToken.Dimension != 0)
						reader.ReadString();
					else
						vToken.SetValue(reader.ReadString(), null);
					break;
				case EraSaveDataType.IntArray:
					if (vToken == null || !vToken.IsInteger || vToken.Dimension != 1)
						reader.ReadIntArray(null, true);
					else
						reader.ReadIntArray((long[])vToken.GetArray(), true);
					break;
				case EraSaveDataType.IntArray2D:
					if (vToken == null || !vToken.IsInteger || vToken.Dimension != 2)
						reader.ReadIntArray2D(null, true);
					else
						reader.ReadIntArray2D((long[,])vToken.GetArray(), true);
					break;
				case EraSaveDataType.IntArray3D:
					if (vToken == null || !vToken.IsInteger || vToken.Dimension != 3)
						reader.ReadIntArray3D(null, true);
					else
						reader.ReadIntArray3D((long[, ,])vToken.GetArray(), true);
					break;
				case EraSaveDataType.StrArray:
					if (vToken == null || !vToken.IsString || vToken.Dimension != 1)
						reader.ReadStrArray(null, true);
					else
						reader.ReadStrArray((string[])vToken.GetArray(), true);
					break;
				case EraSaveDataType.StrArray2D:
					if (vToken == null || !vToken.IsString || vToken.Dimension != 2)
						reader.ReadStrArray2D(null, true);
					else
						reader.ReadStrArray2D((string[,])vToken.GetArray(), true);
					break;
				case EraSaveDataType.StrArray3D:
					if (vToken == null || !vToken.IsString || vToken.Dimension != 3)
						reader.ReadStrArray3D(null, true);
					else
						reader.ReadStrArray3D((string[, ,])vToken.GetArray(), true);
					break;
				default:
					throw new FileEE("データ異常");
			}
			return true;
		}
		#region IDisposable メンバ

		public void Dispose()
		{
			ClearLocalValue();
			for (int i = 0; i < dataIntegerArray.Length; i++)
				dataIntegerArray[i] = null;
			for (int i = 0; i < dataStringArray.Length; i++)
				dataStringArray[i] = null;
			for (int i = 0; i < characterList.Count; i++)
				characterList[i].Dispose();
			characterList.Clear();
		}

		#endregion

	}
}