using System;
using System.Collections.Generic;
using System.Text;
using MinorShift.Emuera.Sub;
using System.Text.RegularExpressions;
using MinorShift.Emuera.GameData.Variable;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.GameView;
using MinorShift.Emuera.GameData;
using MinorShift.Emuera.GameData.Function;
using MinorShift.Emuera.GameProc.Function;

namespace MinorShift.Emuera.GameProc
{

	internal sealed class UserDefinedVariableData
	{
		public string Name = null;
		public bool TypeIsStr = false;
		public bool Reference = false;
		public int Dimension = 1;
		public int[] Lengths = null;
		public Int64[] DefaultInt = null;
		public string[] DefaultStr = null;
		public bool Global = false;
		public bool Save = false;
		public bool Static = true;
		public bool Private = false;
		public bool CharaData = false;
		public bool Const = false;
		
		//1822 Privateの方もDIMだけ遅延させようとしたけどちょっと課題がおおいのでやめとく
		public static UserDefinedVariableData Create(DimLineWC dimline)
		{
			return Create(dimline.WC, dimline.Dims, dimline.IsPrivate, dimline.SC);
		}

		public static UserDefinedVariableData Create(WordCollection wc, bool dims, bool isPrivate, ScriptPosition sc)
		{
			string dimtype = dims ? "#DIM" : "#DIMS";
			UserDefinedVariableData ret = new UserDefinedVariableData();
			ret.TypeIsStr = dims;

			IdentifierWord idw;
			bool staticDefined = false;
			ret.Const = false;
			string keyword = dimtype;
			//List<string> keywords;
			while (!wc.EOL && (idw = wc.Current as IdentifierWord) != null)
			{
				wc.ShiftNext();
				keyword = idw.Code;
				if (Config.ICVariable)
					keyword = keyword.ToUpper();
				//TODO ifの数があたまわるい なんとかしたい
				switch (keyword)
				{
					case "CONST":
						if (ret.CharaData)
							throw new CodeEE(keyword + "とCHARADATAキーワードは同時に指定できません", sc);
						if (ret.Global)
							throw new CodeEE(keyword + "とGLOBALキーワードは同時に指定できません", sc);
						if (ret.Save)
							throw new CodeEE(keyword + "とSAVEDATAキーワードは同時に指定できません", sc);
						if (ret.Reference)
							throw new CodeEE(keyword + "とREFキーワードは同時に指定できません", sc);
						if (!ret.Static)
							throw new CodeEE(keyword + "とDYNAMICキーワードは同時に指定できません", sc);
						if (ret.Const)
							throw new CodeEE(keyword + "キーワードが二重に指定されています", sc);
						ret.Const = true;
						break;
					case "REF":
						//throw new CodeEE("未実装の機能です", sc);
						//if (!isPrivate)
						//	throw new CodeEE("広域変数の宣言に" + keyword + "キーワードは指定できません", sc);
						if (staticDefined && ret.Static)
							throw new CodeEE(keyword + "とSTATICキーワードは同時に指定できません", sc);
						if (ret.CharaData)
							throw new CodeEE(keyword + "とCHARADATAキーワードは同時に指定できません", sc);
						if (ret.Global)
							throw new CodeEE(keyword + "とGLOBALキーワードは同時に指定できません", sc);
						if (ret.Save)
							throw new CodeEE(keyword + "とSAVEDATAキーワードは同時に指定できません", sc);
						if (ret.Const)
							throw new CodeEE(keyword + "とCONSTキーワードは同時に指定できません", sc);
						if (ret.Reference)
							throw new CodeEE(keyword + "キーワードが二重に指定されています", sc);
						ret.Reference = true;
						ret.Static = false;
						break;
					case "DYNAMIC":
						if (!isPrivate)
							throw new CodeEE("広域変数の宣言に" + keyword + "キーワードは指定できません", sc);
						if (ret.CharaData)
							throw new CodeEE(keyword + "とCHARADATAキーワードは同時に指定できません", sc);
						if (ret.Const)
							throw new CodeEE(keyword + "とCONSTキーワードは同時に指定できません", sc);
						if (staticDefined)
							if (ret.Static)
								throw new CodeEE("STATICとDYNAMICキーワードは同時に指定できません", sc);
							else
								throw new CodeEE(keyword + "キーワードが二重に指定されています", sc);
						staticDefined = true;
						ret.Static = false;
						break;
					case "STATIC":
						if (!isPrivate)
							throw new CodeEE("広域変数の宣言に" + keyword + "キーワードは指定できません", sc);
						if (ret.CharaData)
							throw new CodeEE(keyword + "とCHARADATAキーワードは同時に指定できません", sc);
						if (staticDefined)
							if (!ret.Static)
								throw new CodeEE("STATICとDYNAMICキーワードは同時に指定できません", sc);
							else
								throw new CodeEE(keyword + "キーワードが二重に指定されています", sc);
						if (ret.Reference)
							throw new CodeEE(keyword + "とREFキーワードは同時に指定できません", sc);
						staticDefined = true;
						ret.Static = true;
						break;
					case "GLOBAL":
						if (isPrivate)
							throw new CodeEE("ローカル変数の宣言に" + keyword + "キーワードは指定できません", sc);
						if (ret.CharaData)
							throw new CodeEE(keyword + "とCHARADATAキーワードは同時に指定できません", sc);
						if (ret.Reference)
							throw new CodeEE(keyword + "とREFキーワードは同時に指定できません", sc);
						if (ret.Const)
							throw new CodeEE(keyword + "とCONSTキーワードは同時に指定できません", sc);
						if (staticDefined)
							if (ret.Static)
								throw new CodeEE("STATICとGLOBALキーワードは同時に指定できません", sc);
							else
								throw new CodeEE("DYNAMICとGLOBALキーワードは同時に指定できません", sc);
						ret.Global = true;
						break;
					case "SAVEDATA":
						if (isPrivate)
							throw new CodeEE("ローカル変数の宣言に" + keyword + "キーワードは指定できません", sc);
						if (staticDefined)
							if (ret.Static)
								throw new CodeEE("STATICとSAVEDATAキーワードは同時に指定できません", sc);
							else
								throw new CodeEE("DYNAMICとSAVEDATAキーワードは同時に指定できません", sc);
						if (ret.Reference)
							throw new CodeEE(keyword + "とREFキーワードは同時に指定できません", sc);
						if (ret.Const)
							throw new CodeEE(keyword + "とCONSTキーワードは同時に指定できません", sc);
						if (ret.Save)
							throw new CodeEE(keyword + "キーワードが二重に指定されています", sc);
						ret.Save = true;
						break;
					case "CHARADATA":
						if (isPrivate)
							throw new CodeEE("ローカル変数の宣言に" + keyword + "キーワードは指定できません", sc);
						if (ret.Reference)
							throw new CodeEE(keyword + "とREFキーワードは同時に指定できません", sc);
						if (ret.Const)
							throw new CodeEE(keyword + "とCONSTキーワードは同時に指定できません", sc);
						if (staticDefined)
							if (ret.Static)
                                throw new CodeEE(keyword + "とSTATICキーワードは同時に指定できません", sc);
							else
                                throw new CodeEE(keyword + "とDYNAMICキーワードは同時に指定できません", sc);
						if (ret.Global)
                            throw new CodeEE(keyword + "とGLOBALキーワードは同時に指定できません", sc);
						if (ret.CharaData)
							throw new CodeEE(keyword + "キーワードが二重に指定されています", sc);
						ret.CharaData = true;
						break;
					default:
						ret.Name = keyword;
						goto whilebreak;
				}
			}
		whilebreak:
			if (ret.Name == null)
				throw new CodeEE(keyword + "の後に有効な変数名が指定されていません", sc);
			string errMes = "";
			int errLevel = -1;
			if (isPrivate)
				GlobalStatic.IdentifierDictionary.CheckUserPrivateVarName(ref errMes, ref errLevel, ret.Name);
			else
				GlobalStatic.IdentifierDictionary.CheckUserVarName(ref errMes, ref errLevel, ret.Name);
			if (errLevel >= 0)
			{
				if (errLevel >= 2)
					throw new CodeEE(errMes, sc);
				ParserMediator.Warn(errMes, sc, errLevel);
			}


			List<int> sizeNum = new List<int>();
			if (wc.EOL)//サイズ省略
			{
				if (ret.Const)
					throw new CodeEE("CONSTキーワードが指定されていますが初期値が設定されていません");
				sizeNum.Add(1);
			}
			else if (wc.Current.Type == ',')//サイズ指定
			{
				while (!wc.EOL)
				{
					if (wc.Current.Type == '=')//サイズ指定解読完了＆初期値指定
						break;
					if (wc.Current.Type != ',')
						throw new CodeEE("書式が間違っています", sc);
					wc.ShiftNext();
					if (ret.Reference)//参照型の場合は要素数不要
					{
						sizeNum.Add(0);
						if (wc.EOL)
							break;
						if (wc.Current.Type == ',')
							continue;
					}
					if (wc.EOL)
						throw new CodeEE("カンマの後に有効な定数式が指定されていません", sc);
					IOperandTerm arg = ExpressionParser.ReduceIntegerTerm(wc, TermEndWith.Comma_Assignment);
					SingleTerm sizeTerm = arg.Restructure(null) as SingleTerm;
					if ((sizeTerm == null) || (sizeTerm.GetOperandType() != typeof(Int64)))
						throw new CodeEE("カンマの後に有効な定数式が指定されていません", sc);
					if (ret.Reference)//参照型には要素数指定不可(0にするか書かないかどっちか
					{
						if (sizeTerm.Int != 0)
							throw new CodeEE("参照型変数にはサイズを指定できません(サイズを省略するか0を指定してください)", sc);

						continue;
					}
					else if ((sizeTerm.Int <= 0) || (sizeTerm.Int > 1000000))
						throw new CodeEE("ユーザー定義変数のサイズは1以上1000000以下でなければなりません", sc);
					sizeNum.Add((int)sizeTerm.Int);
				}
			}


			if (wc.Current.Type != '=')//初期値指定なし
			{
				if (ret.Const)
					throw new CodeEE("CONSTキーワードが指定されていますが初期値が設定されていません");
			}
			else//初期値指定あり
			{
				if (((OperatorWord)wc.Current).Code != OperatorCode.Assignment)
					throw new CodeEE("予期しない演算子を発見しました");
				if (ret.Reference)
					throw new CodeEE("参照型変数には初期値を設定できません");
				if (sizeNum.Count >= 2)
					throw new CodeEE("多次元変数には初期値を設定できません");
				if (ret.CharaData)
					throw new CodeEE("キャラ型変数には初期値を設定できません");
				int size = 0;
				if (sizeNum.Count == 1)
					size = sizeNum[0];
				wc.ShiftNext();
				IOperandTerm[] terms = ExpressionParser.ReduceArguments(wc, ArgsEndWith.EoL, false);
				if (terms.Length == 0)
					throw new CodeEE("配列の初期値は省略できません");
				if (size > 0)
				{
					if (terms.Length > size)
						throw new CodeEE("初期値の数が配列のサイズを超えています");
					if (ret.Const && terms.Length != size)
						throw new CodeEE("定数の初期値の数が配列のサイズと一致しません");
				}
				if (dims)
					ret.DefaultStr = new string[terms.Length];
				else
					ret.DefaultInt = new Int64[terms.Length];

				for (int i = 0; i < terms.Length; i++)
				{
					if (terms[i] == null)
						throw new CodeEE("配列の初期値は省略できません");
					terms[i] = terms[i].Restructure(GlobalStatic.EMediator);
					SingleTerm sTerm = terms[i] as SingleTerm;
					if (sTerm == null)
						throw new CodeEE("配列の初期値には定数のみ指定できます");
					if (dims != sTerm.IsString)
						throw new CodeEE("変数の型と初期値の型が一致していません");
					if (dims)
						ret.DefaultStr[i] = sTerm.Str;
					else
						ret.DefaultInt[i] = sTerm.Int;
				}
				if (sizeNum.Count == 0)
					sizeNum.Add(terms.Length);
			}
			if (!wc.EOL)
				throw new CodeEE("書式が間違っています", sc);

			if (sizeNum.Count == 0)
				sizeNum.Add(1);

			ret.Private = isPrivate;
			ret.Dimension = sizeNum.Count;
			if (ret.Const && ret.Dimension > 1)
				throw new CodeEE("CONSTキーワードが指定された変数を多次元配列にはできません");
			if (ret.CharaData && ret.Dimension > 2)
				throw new CodeEE("3次元以上のキャラ型変数を宣言することはできません", sc);
			if (ret.Dimension > 3)
				throw new CodeEE("4次元以上の配列変数を宣言することはできません", sc);
			ret.Lengths = new int[sizeNum.Count];
			if (ret.Reference)
				return ret;
			Int64 totalBytes = 1;
			for (int i = 0; i < sizeNum.Count; i++)
			{
				ret.Lengths[i] = sizeNum[i];
				totalBytes *= ret.Lengths[i];
			}
			if ((totalBytes <= 0) || (totalBytes > 1000000))
				throw new CodeEE("ユーザー定義変数のサイズは1以上1000000以下でなければなりません", sc);
			if (!isPrivate && ret.Save && !Config.SystemSaveInBinary)
			{
				if (dims && ret.Dimension > 1)
					throw new CodeEE("文字列型の多次元配列変数にSAVEDATAフラグを付ける場合には「バイナリ型セーブ」オプションが必須です", sc);
				else if (ret.CharaData)
					throw new CodeEE("キャラ型変数にSAVEDATAフラグを付ける場合には「バイナリ型セーブ」オプションが必須です", sc);
			}
			return ret;
		}
	}
	internal sealed class DimLineWC
	{
		public WordCollection WC;
		public bool Dims;
		public bool IsPrivate;
		public ScriptPosition SC;
		public DimLineWC(WordCollection wc, bool isString, bool isPrivate, ScriptPosition position)
		{
			WC = wc;
			Dims = isString;
			IsPrivate = isPrivate;
			SC = position;
		}
	}

}