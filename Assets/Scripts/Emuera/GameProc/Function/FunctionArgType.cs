
namespace MinorShift.Emuera.GameProc.Function
{
	/// <summary>
	/// 命令の引数タイプ
	/// </summary>
	//難読化用属性。enum.ToString()やenum.Parse()を行うなら(Exclude=true)にすること。
	[global::System.Reflection.Obfuscation(Exclude=false)]
	enum FunctionArgType
	{//数値不要
		__NULL__ = 0x0000,//未設定。エラー。引数がないならばVOIDを指定すること。
		METHOD,//式中関数。

		VOID,//引数なし
		INT_EXPRESSION,//数式型。省略可能
		INT_EXPRESSION_NULLABLE,//数式型
		STR_EXPRESSION,//文字列式型
        STR_EXPRESSION_NULLABLE,
		STR,//単純文字列型
		KEYWORD,//単純文字列型。特定の文字のみ有効
		STR_NULLABLE,//単純文字列型、省略可能
		FORM_STR,//書式付文字列型。
		FORM_STR_NULLABLE,//書式付文字列型。省略可能
		SP_PRINTV,//複数数式型。'～～,文字列可
		SP_TIMES,//<数値型変数>,<実数定数>
		SP_BAR,//<数値>,<数値>,<数値>
		SP_SET,//可変数値変数・数式型。
		SP_SETS,//可変文字列変数・単純又は複合文字列型。
		SP_SWAP,//<数値>,<数値>
		SP_VAR,//<変数>
		SP_SAVEDATA,//<数値>,<文字列式>
        SP_INPUT,//(<数値>) //引数はオプションでないのがデフォ、INT_EXPRESSION_NULLABLEとは処理が違う
        SP_INPUTS,//(<FORM文字列>) //引数はオプションでないのがデフォ、STR_EXPRESSION_NULLABLEとは処理が違う
        SP_ONEINPUT,//(<数値>, <数値>) //引数はオプションでないのがデフォ、第2引数はマウス入力時の2桁以上の値時の処理指定
        SP_ONEINPUTS,//(<FORM文字列>, <数値>) //引数はオプションでないのがデフォ、第2引数はマウス入力時の2文字以上の文字列時の処理指定
        SP_TINPUT,//<数値>,<数値>(,<数値>,<文字列>)
        SP_TINPUTS,//<数値>,<文字列式>(,<数値>,<文字列>)
		SP_SORTCHARA,//<キャラクタ変数>,<ソート順序>(両方省略可能)
		SP_CALL,//<文字列>,<引数>,... //引数は省略可能
		SP_CALLF,
		SP_CALLFORM,//<書式付文字列>,<引数>,... //引数は省略可能
		SP_CALLFORMF,//<書式付文字列>,<引数>,... //引数は省略可能
		SP_FOR_NEXT,//<可変数値変数>,<数値>,<数値>,<数値> //引数は省略可能
		SP_POWER,//<可変数値変数>,<数値>,<数値>
		SP_SWAPVAR,//<可変変数>,<可変変数>(同型のみ)
		EXPRESSION,//<式>、変数の型は不問
		EXPRESSION_NULLABLE,//<式>、変数の型は不問
		CASE,//<CASE条件式>(, <CASE条件式>...)
		

        //TODO　省略時の処理に違いがあるが統合可能なはず
		VAR_INT,//<可変数値変数> //引数は省略可
        SP_GETINT,//<可変数値変数>(今までこれがないことに驚いた)

		VAR_STR,//<可変数値変数> //引数は省略可
		BIT_ARG,//<可変数値変数>,<数値>*n (SP_SETが使えないため新設)
		SP_VAR_SET,//<可変変数>,<数式 or 文字列式 or null>(,<範囲初値>, <範囲終値>)
		SP_BUTTON,//<文字列式>,<数式>
		SP_SET_ARRAY,//可変数値変数・<数式配列型>。未使用
		SP_SETS_ARRAY,//可変文字列変数・<文字列配列型>。未使用
		SP_COLOR,
		SP_SPLIT,//<文字列式>, <文字列式>, <可変文字変数>
		SP_CVAR_SET,//<可変変数>,<式>,<数式 or 文字列式 or null>(,<範囲初値>, <範囲終値>)
		SP_CONTROL_ARRAY,//<可変変数>,<数値>,<数値>
		SP_SHIFT_ARRAY,//<可変変数>,<数値>,<数値or文字列>(,<数値>,<数値>)
        SP_SORTARRAY,//<対象変数>, (<ソート順序>, <範囲初値>, <範囲終値>)
        INT_ANY,//1つ以上の数値を任意数
		FORM_STR_ANY,//1つ以上のFORM文字列を任意数  
		SP_COPYCHARA,//<数値>(, <数値)第二引数省略可
		SP_COPY_ARRAY,//<文字列式>,<文字列式>
		SP_SAVEVAR,//<数値>,<文字列式>, <変数>（, <変数>...）
		SP_SAVECHARA,//<数値>, <文字列式>, <数値>（, <数値>...）第二引数省略可
		SP_REF,
		SP_REFBYNAME,
		SP_HTMLSPLIT,
	}
}
