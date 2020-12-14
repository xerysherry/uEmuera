using System;
using System.Collections.Generic;
using System.Text;

namespace MinorShift.Emuera.GameData.Variable
{
	//難読化用属性。enum.ToString()やenum.Parse()を行うなら(Exclude=true)にすること。
	[global::System.Reflection.Obfuscation(Exclude=true)]
	internal enum VariableCode
	{
		__NULL__ = 0x00000000,
        __CAN_FORBID__ = 0x00010000,
		__INTEGER__ = 0x00020000,
		__STRING__ = 0x00040000,
		__ARRAY_1D__ = 0x00080000,
		__CHARACTER_DATA__ = 0x00100000,//第一引数を省略可能。TARGETで補う
		__UNCHANGEABLE__ = 0x00400000,//変更不可属性
		__CALC__ = 0x00800000,//計算値
		__EXTENDED__ = 0x01000000,//Emueraで追加した変数
		__LOCAL__ = 0x02000000,//ローカル変数。
		__GLOBAL__ = 0x04000000,//グローバル変数。
		__ARRAY_2D__ = 0x08000000,//二次元配列。キャラクタ変数フラグと排他
		__SAVE_EXTENDED__ = 0x10000000,//拡張セーブ機能によってセーブするべき変数。
							//このフラグを立てておけば勝手にセーブされる(はず)。名前を変えると正常にロードできなくなるので注意。
        __ARRAY_3D__ = 0x20000000,//三次元配列
        __CONSTANT__ = 0x40000000,//完全定数CSVから読み込まれる～NAME系がこれに該当

		__UPPERCASE__ = 0x7FFF0000,
		__LOWERCASE__ = 0x0000FFFF,

		__COUNT_SAVE_INTEGER__ = 0x00,//実は全て配列
		__COUNT_INTEGER__ = 0x00,
		//PALAMLV, EXPLV, RESULT, COUNT, TARGET, SELECTCOMは禁止設定不可
		DAY = 0x00 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//経過日数。
		MONEY = 0x01 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//金
		ITEM = 0x02 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//所持数
		FLAG = 0x03 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//フラグ
		TFLAG = 0x04 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//一時フラグ
		UP = 0x05 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//調教中パラメータの上昇値。indexはPALAM.CSVのもの。
		PALAMLV = 0x06 | __INTEGER__ | __ARRAY_1D__,//調教中パラメータのレベルわけの境界値。境界値を越えると珠の数が多くなる。
		EXPLV = 0x07 | __INTEGER__ | __ARRAY_1D__,//経験のレベルわけの境界値。境界値を越えると調教の効果が上がる。
		EJAC = 0x08 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//射精チェックのための一時変数。
		DOWN = 0x09 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//調教中パラメータの減少値。indexはPALAM.CSVのもの
		RESULT = 0x0A | __INTEGER__ | __ARRAY_1D__,//戻り値(数値)
		COUNT = 0x0B | __INTEGER__ | __ARRAY_1D__,//繰り返しカウンター
		TARGET = 0x0C | __INTEGER__ | __ARRAY_1D__,//調教中のキャラの"登録番号"
		ASSI = 0x0D | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//助手のキャラの"登録番号"
		MASTER = 0x0E | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//主人公のキャラの"登録番号"。通常0
		NOITEM = 0x0F | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//アイテムが存在しないか？存在しない設定なら１．GAMEBASE.CSV
		LOSEBASE = 0x10 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//基礎パラメータの減少値。通常はLOSEBASE:0が体力の消耗、LOSEBASE:1が気力の消耗。
		SELECTCOM = 0x11 | __INTEGER__ | __ARRAY_1D__,//選択されたコマンド。TRAIN.CSVのものと同じ
		ASSIPLAY = 0x12 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//助手現在調教しているか？1 = true, 0 = false
		PREVCOM = 0x13 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//前回のコマンド。
		NOTUSE_14 = 0x14 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//eramakerではRANDが格納されている領域。
		NOTUSE_15 = 0x15 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//eramakerではCHARANUMが格納されている領域。
		TIME = 0x16 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//時刻
		ITEMSALES = 0x17 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//売っているか？
		PLAYER = 0x18 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//調教している人間のキャラの登録番号。通常はMASTERかASSI
		NEXTCOM = 0x19 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//調教している人間のキャラの登録番号。通常はMASTERかASSI
		PBAND = 0x1A | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//ペニスバンドのアイテム番号
		BOUGHT = 0x1B | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//直前に購入したアイテム番号
		NOTUSE_1C = 0x1C | __INTEGER__ | __ARRAY_1D__,//未使用領域
		NOTUSE_1D = 0x1D | __INTEGER__ | __ARRAY_1D__,//未使用領域
		A = 0x1E | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//汎用変数
        B = 0x1F | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        C = 0x20 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        D = 0x21 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        E = 0x22 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        F = 0x23 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        G = 0x24 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        H = 0x25 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        I = 0x26 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        J = 0x27 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        K = 0x28 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        L = 0x29 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        M = 0x2A | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        N = 0x2B | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        O = 0x2C | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        P = 0x2D | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        Q = 0x2E | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        R = 0x2F | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        S = 0x30 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        T = 0x31 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        U = 0x32 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        V = 0x33 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        W = 0x34 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        X = 0x35 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        Y = 0x36 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        Z = 0x37 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
		NOTUSE_38 = 0x38 | __INTEGER__ | __ARRAY_1D__,//未使用領域
		NOTUSE_39 = 0x39 | __INTEGER__ | __ARRAY_1D__,//未使用領域
		NOTUSE_3A = 0x3A | __INTEGER__ | __ARRAY_1D__,//未使用領域
		NOTUSE_3B = 0x3B | __INTEGER__ | __ARRAY_1D__,//未使用領域
		__COUNT_SAVE_INTEGER_ARRAY__ = 0x3C,

		ITEMPRICE = 0x3C | __INTEGER__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CAN_FORBID__,//アイテム価格
		LOCAL = 0x3D | __INTEGER__ | __ARRAY_1D__ | __LOCAL__ | __EXTENDED__ | __CAN_FORBID__,//ローカル変数
		ARG = 0x3E | __INTEGER__ | __ARRAY_1D__ | __LOCAL__ | __EXTENDED__ | __CAN_FORBID__,//関数の引数用
		GLOBAL = 0x3F | __INTEGER__ | __ARRAY_1D__ | __GLOBAL__ | __EXTENDED__ | __CAN_FORBID__,//グローバル数値型変数
		RANDDATA = 0x40 | __INTEGER__ | __ARRAY_1D__ | __SAVE_EXTENDED__ | __EXTENDED__,//グローバル数値型変数
		__COUNT_INTEGER_ARRAY__ = 0x41,


		SAVESTR = 0x00 | __STRING__ | __ARRAY_1D__ | __CAN_FORBID__,//文字列データ。保存される
		__COUNT_SAVE_STRING_ARRAY__ = 0x01,


		//RESULTSは禁止設定不可
		STR = 0x01 | __STRING__ | __ARRAY_1D__ | __CAN_FORBID__,//文字列データ。STR.CSV。書き換え可能。
		RESULTS = 0x02 | __STRING__ | __ARRAY_1D__,//実はこいつも配列
		LOCALS = 0x03 | __STRING__ | __ARRAY_1D__ | __LOCAL__ | __EXTENDED__ | __CAN_FORBID__, //ローカル文字列変数
		ARGS = 0x04 | __STRING__ | __ARRAY_1D__ | __LOCAL__ | __EXTENDED__ | __CAN_FORBID__,//関数の引数用
		GLOBALS = 0x05 | __STRING__ | __ARRAY_1D__ | __GLOBAL__ | __EXTENDED__ | __CAN_FORBID__, //グローバル文字列変数
		TSTR = 0x06 | __STRING__ | __ARRAY_1D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,

		__COUNT_STRING_ARRAY__ = 0x07,



		SAVEDATA_TEXT = 0x00 | __STRING__ | __EXTENDED__, //セーブ時につかう文字列。PUTFORMで追加できるやつ
		__COUNT_SAVE_STRING__ = 0x00,
		__COUNT_STRING__ = 0x01,






		ISASSI = 0x00 | __INTEGER__ | __CHARACTER_DATA__,//助手か？1 = ture, 0 = false
		NO = 0x01 | __INTEGER__ | __CHARACTER_DATA__,//キャラ番号

		__COUNT_SAVE_CHARACTER_INTEGER__ = 0x02,//こいつらは配列ではないらしい。
		__COUNT_CHARACTER_INTEGER__ = 0x02,

		BASE = 0x00 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//基礎パラメータ。
		MAXBASE = 0x01 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//基礎パラメータの最大値。
		ABL = 0x02 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//能力。ABL.CSV
		TALENT = 0x03 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//素質。TALENT.CSV
		EXP = 0x04 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//経験。EXP.CSV
		MARK = 0x05 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//刻印。MARK.CSV
		PALAM = 0x06 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//調教中パラメータ。PALAM.CSV
		SOURCE = 0x07 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//調教中パラメータ。直前のコマンドで発生した調教ソース。
		EX = 0x08 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//調教中パラメータ。この調教中、どこで何回絶頂したか。
		CFLAG = 0x09 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//フラグ。
		JUEL = 0x0A | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//珠。PALAM.CSV
		RELATION = 0x0B | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//関係。indexは登録番号ではなくキャラ番号
		EQUIP = 0x0C | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//未使用変数
		TEQUIP = 0x0D | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//調教中パラメータ。アイテムを使用中か。ITEM.CSV
		STAIN = 0x0E | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__,//調教中パラメータ。汚れ
		GOTJUEL = 0x0F | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//調教中パラメータ。今回獲得した珠。PALAM.CSV
		NOWEX = 0x10 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//調教中パラメータ。直前のコマンドでどこで何回絶頂したか。
        DOWNBASE = 0x11 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__, //調教中パラメータ。LOSEBASEのキャラクタ変数版
        CUP = 0x12 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,//調教中パラメータ。UPのキャラクタ変数版
        CDOWN = 0x13 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,//調教中パラメータ。DOWNのキャラクタ変数版
        TCVAR = 0x14 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,//キャラクタ変数での一時変数


		__COUNT_SAVE_CHARACTER_INTEGER_ARRAY__ = 0x11,
		__COUNT_CHARACTER_INTEGER_ARRAY__ = 0x54,

		NAME = 0x00 | __STRING__ | __CHARACTER_DATA__,//名前//登録番号で呼び出す
		CALLNAME = 0x01 | __STRING__ | __CHARACTER_DATA__,//呼び名
		NICKNAME = 0x02 | __STRING__ | __CHARACTER_DATA__ | __SAVE_EXTENDED__ | __EXTENDED__,//あだ名
		MASTERNAME = 0x03 | __STRING__ | __CHARACTER_DATA__ | __SAVE_EXTENDED__ | __EXTENDED__,//あだ名

		__COUNT_SAVE_CHARACTER_STRING__ = 0x02,
		__COUNT_CHARACTER_STRING__ = 0x04,

		CSTR = 0x00 | __STRING__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,//キャラクタ用文字列配列

		__COUNT_SAVE_CHARACTER_STRING_ARRAY__ = 0x00,
		__COUNT_CHARACTER_STRING_ARRAY__ = 0x01,

		CDFLAG = 0x00 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_2D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,

		__COUNT_CHARACTER_INTEGER_ARRAY_2D__ = 0x01,

		__COUNT_CHARACTER_STRING_ARRAY_2D__ = 0x00,


		DITEMTYPE = 0x00 | __INTEGER__ | __ARRAY_2D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,
		DA = 0x01 | __INTEGER__ | __ARRAY_2D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,
		DB = 0x02 | __INTEGER__ | __ARRAY_2D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,
		DC = 0x03 | __INTEGER__ | __ARRAY_2D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,
		DD = 0x04 | __INTEGER__ | __ARRAY_2D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,
		DE = 0x05 | __INTEGER__ | __ARRAY_2D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,
        __COUNT_INTEGER_ARRAY_2D__ = 0x06,

		__COUNT_STRING_ARRAY_2D__ = 0x00,

		TA = 0x00 | __INTEGER__ | __ARRAY_3D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,
        TB = 0x01 | __INTEGER__ | __ARRAY_3D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,
        __COUNT_INTEGER_ARRAY_3D__ = 0x02,

        __COUNT_STRING_ARRAY_3D__ = 0x00,

		//CALCな変数については番号順はどうでもいい。
		//1803beta004 ～～NAME系については番号順をConstantDataが使用するので重要
		
		RAND = 0x00 | __INTEGER__ | __ARRAY_1D__ | __CALC__ | __UNCHANGEABLE__,//乱数。０～引数-1までの値を返す。
		CHARANUM = 0x01 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__,//キャラクタ数。キャラクタ登録数を返す。

		ABLNAME = 0x00 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __CONSTANT__ | __CAN_FORBID__,//能力。ABL.CSV//csvから読まれるデータは保存されない。変更不可
		EXPNAME = 0x01 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __CONSTANT__ | __CAN_FORBID__,//経験。EXP.CSV
		TALENTNAME = 0x02 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __CONSTANT__ | __CAN_FORBID__,//素質。TALENT.CSV
		PALAMNAME = 0x03 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __CONSTANT__ | __CAN_FORBID__,//能力。PALAM.CSV
		TRAINNAME = 0x04 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,//調教名。TRAIN.CSV
		MARKNAME = 0x05 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __CONSTANT__ | __CAN_FORBID__,//刻印。MARK.CSV
		ITEMNAME = 0x06 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __CONSTANT__ | __CAN_FORBID__,//アイテム。ITEM.CSV
		BASENAME = 0x07 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,//基礎能力名。BASE.CSV
		SOURCENAME = 0x08 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,//調教ソース名。SOURCE.CSV
		EXNAME = 0x09 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,//絶頂名。EX.CSV
		__DUMMY_STR__ = 0x0A | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__,
		EQUIPNAME = 0x0B | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,//装着物名。EQUIP.CSV
		TEQUIPNAME = 0x0C | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,//調教時装着物名。TEQUIP.CSV
		FLAGNAME = 0x0D | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,//フラグ名。FLAG.CSV
		TFLAGNAME = 0x0E | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,//一時フラグ名。TFLAG.CSV
		CFLAGNAME = 0x0F | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,//キャラクタフラグ名。CFLAG.CSV
		TCVARNAME = 0x10 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,
		CSTRNAME = 0x11 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,
		STAINNAME = 0x12 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,

		CDFLAGNAME1 = 0x13 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,
		CDFLAGNAME2 = 0x14 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,
		STRNAME = 0x15 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,
		TSTRNAME = 0x16 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,
		SAVESTRNAME = 0x17 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,
		GLOBALNAME = 0x18 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,
		GLOBALSNAME = 0x19 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,

        __COUNT_CSV_STRING_ARRAY_1D__ = 0x1A,


		GAMEBASE_AUTHER = 0x04 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//文字列型。作者。綴りを間違えていたが互換性のため残す。
		GAMEBASE_AUTHOR = 0x00 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//文字列型。作者
		GAMEBASE_INFO = 0x01 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//文字列型。追加情報
		GAMEBASE_YEAR = 0x02 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//文字列型。製作年
		GAMEBASE_TITLE = 0x03 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//文字列型。タイトル
		WINDOW_TITLE = 0x05 | __STRING__ | __CALC__ | __EXTENDED__,//文字列型。ウインドウのタイトル。変更可能。
		//アンダースコア2つで囲まれた変数を追加したらVariableTokenに特別な処理が必要。
		__FILE__ = 0x06 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//現在実行中のファイル名
		__FUNCTION__ = 0x07 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//現在実行中の関数名
        MONEYLABEL = 0x08 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//お金のラベル
        DRAWLINESTR = 0x09 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//DRAWLINEの描画文字列
        EMUERA_VERSION = 0x0A | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__, //Emeuraのバージョン

		LASTLOAD_TEXT = 0x05 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//数値型。

		GAMEBASE_GAMECODE = 0x00 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//数値型。コード
		GAMEBASE_VERSION = 0x01 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//数値型。バージョン
		GAMEBASE_ALLOWVERSION = 0x02 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//数値型。バージョン違い認める
		GAMEBASE_DEFAULTCHARA = 0x03 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//数値型。最初からいるキャラ
		GAMEBASE_NOITEM = 0x04 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//数値型。アイテムなし

		LASTLOAD_VERSION = 0x05 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//数値型。
		LASTLOAD_NO = 0x06 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//数値型。
		__LINE__ = 0x07 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//現在実行中の行番号
		LINECOUNT = 0x08 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//描画した行の総数。CLEARで減少
        ISTIMEOUT = 0x0B | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//TINPUT系等がTIMEOUTしたか？

        __INT_MAX__ = 0x09 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//Int64最大値
        __INT_MIN__ = 0x0A | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//Int64最小値

		CVAR = 0xFC | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __EXTENDED__,//ユーザー定義変数
		CVARS = 0xFC | __STRING__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __EXTENDED__,//ユーザー定義変数
		CVAR2D = 0xFC | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_2D__ | __EXTENDED__,//ユーザー定義変数
		CVARS2D = 0xFC | __STRING__ | __CHARACTER_DATA__ | __ARRAY_2D__ | __EXTENDED__,//ユーザー定義変数
		//CVAR3D = 0xFC | __INTEGER__ | __ARRAY_3D__ | __EXTENDED__,//ユーザー定義変数
		//CVARS3D = 0xFC | __STRING__ | __ARRAY_3D__ | __EXTENDED__,//ユーザー定義変数
		REF = 0xFD | __INTEGER__ | __ARRAY_1D__ | __EXTENDED__,//参照型
		REFS = 0xFD | __STRING__ | __ARRAY_1D__ | __EXTENDED__,
		REF2D = 0xFD | __INTEGER__ | __ARRAY_2D__ | __EXTENDED__,
		REFS2D = 0xFD | __STRING__ | __ARRAY_2D__ | __EXTENDED__,
		REF3D = 0xFD | __INTEGER__ | __ARRAY_3D__ | __EXTENDED__,
		REFS3D = 0xFD | __STRING__ | __ARRAY_3D__ | __EXTENDED__,
		VAR = 0xFE | __INTEGER__ | __ARRAY_1D__ | __EXTENDED__,//ユーザー定義変数 1808 プライベート変数と広域変数を区別しない
		VARS = 0xFE | __STRING__ | __ARRAY_1D__ | __EXTENDED__,//ユーザー定義変数
		VAR2D = 0xFE | __INTEGER__ | __ARRAY_2D__ | __EXTENDED__,//ユーザー定義変数
		VARS2D = 0xFE | __STRING__ | __ARRAY_2D__ | __EXTENDED__,//ユーザー定義変数
		VAR3D = 0xFE | __INTEGER__ | __ARRAY_3D__ | __EXTENDED__,//ユーザー定義変数
		VARS3D = 0xFE | __STRING__ | __ARRAY_3D__ | __EXTENDED__,//ユーザー定義変数
		//PRIVATE = 0xFF | __INTEGER__ | __ARRAY_1D__ | __EXTENDED__,//プライベート変数
		//PRIVATES = 0xFF | __STRING__ | __ARRAY_1D__ | __EXTENDED__,//プライベート変数
		//PRIVATE2D = 0xFF | __INTEGER__ | __ARRAY_2D__ | __EXTENDED__,//プライベート変数
		//PRIVATES2D = 0xFF | __STRING__ | __ARRAY_2D__ | __EXTENDED__,//プライベート変数
		//PRIVATE3D = 0xFF | __INTEGER__ | __ARRAY_3D__ | __EXTENDED__,//プライベート変数
		//PRIVATES3D = 0xFF | __STRING__ | __ARRAY_3D__ | __EXTENDED__,//プライベート変数
	}
}

