
namespace MinorShift.Emuera.GameProc.Function
{
	/// <summary>
	/// 命令コード
	/// </summary>
	//難読化用属性。enum.ToString()やenum.Parse()を行うなら(Exclude=true)にすること。
	[global::System.Reflection.Obfuscation(Exclude = true)]
	enum FunctionCode
	{//数値不要
		//FunctionCodeを定義したらstatic FunctionIdentifier()内でaddFunctionすること。
		//その際に適切なFunctionArgとフラグを選ぶ。

		//FunctionMethod(式中関数)を定義した場合には自動で拾うので何もしなくてよい。
		//ただし、式中関数バージョンと命令バージョンで動作が違うなら追加する必要がある。

		__NULL__ = 0x0000,
		SET,//数値代入文 or 文字列代入文
		//SETS,//文字列代入文
		PRINT,//文字を表示する
		PRINTL,//改行
		PRINTW,//入力待ち(実質改行)

		PRINTV,//変数の内容
		PRINTVL,
		PRINTVW,

		PRINTS,//文字列変数の内容
		PRINTSL,
		PRINTSW,

		PRINTFORM,//{数式}、%文字列変数%などの書式が使える。
		PRINTFORML,
		PRINTFORMW,

		PRINTFORMS,//文字列変数の内容を変換して表示。
		PRINTFORMSL,
		PRINTFORMSW,

		PRINTC,//??

		CLEARLINE,
		REUSELASTLINE,

		WAIT,//改行待ち。
		INPUT,//整数入力。入力はRESULTへ。
		INPUTS,//文字列入力。入力はRESULTSへ。
		TINPUT,
		TINPUTS,
		TWAIT,
		WAITANYKEY,
		FORCEWAIT,//スキップで省略できないWAIT、強制TWAITと違い、スキップを打ち切る
		ONEINPUT,
		ONEINPUTS,
		TONEINPUT,
		TONEINPUTS,
		AWAIT,//入力不可 DoEvents

		DRAWLINE,//画面の左端から右端まで----と線を引く。
		BAR,//[*****....]のようなグラフを書く。BAR (変数) , (最大値), (長さ)
		BARL,//改行付き。
		TIMES,//小数計算。TIMES (変数) , (小数値)という形で使う。

		PRINT_ABL,//能力。引数は登録番号
		PRINT_TALENT,//素質
		PRINT_MARK,//刻印
		PRINT_EXP,//経験
		PRINT_PALAM,//パラメータ
		PRINT_ITEM,//所持アイテム
		PRINT_SHOPITEM,//ショップで売っているアイテム

		UPCHECK,//パラメータの変動
		CUPCHECK,
		ADDCHARA,//(キャラ番号)のキャラクタを追加
		ADDSPCHARA,//(キャラ番号)のSPキャラクタを追加（フラグ0を1にして作成）
		ADDDEFCHARA,
		ADDVOIDCHARA,//変数に何の設定のないキャラを作成
		DELCHARA,//(キャラ登録番号)のキャラクタを削除。

		PUTFORM,//@SAVEINFO関数でのみ使用可能。PRINTFORMと同様の書式でセーブデータに概要をつける。
		QUIT,//ゲームを終了
		OUTPUTLOG,

		BEGIN,//システム関数の実行。実行するとCALLの呼び出し元などを忘れてしまう。

		SAVEGAME,//セーブ画面を呼ぶ。ショップのみ。
		LOADGAME,//

		SIF,//一行のみIF
		IF,
		ELSE,
		ELSEIF,
		ENDIF,

		REPEAT,//RENDまで繰り返し。繰り返した回数がCOUNTへ。ネスト不可。
		REND,
		CONTINUE,//REPEATに戻る
		BREAK,//RENDの次の行まで

		GOTO,//$ラベルへジャンプ

		JUMP,//関数に移動
		CALL,//関数に移動。移動元を記憶し、RETURNで帰る。
		CALLEVENT,
		RETURN,//__INT_EXPRESSION__,//関数の終了。RESULTに整数を格納可能。省略した場合、０。(次の@～～がRETURNと見なされる。)  
		RETURNFORM,//__FORM_STR__,//関数の終了。RESULTに整数を格納可能。省略した場合、０。(次の@～～がRETURNと見なされる。)  
		RETURNF,
		RESTART,//関数の再開。関数の最初に戻る。


		STRLEN,
		//STRLENS,//
		STRLENFORM,
		STRLENU,
		//STRLENSU,
		STRLENFORMU,

		PRINTLC,
		PRINTFORMC,
		PRINTFORMLC,

		SWAPCHARA,
		COPYCHARA,
		ADDCOPYCHARA,
		VARSIZE,//動作が違うので__METHOD__化できない
		SPLIT,

		PRINTSINGLE,
		PRINTSINGLEV,
		PRINTSINGLES,
		PRINTSINGLEFORM,
		PRINTSINGLEFORMS,

		PRINTBUTTON,
		PRINTBUTTONC,
		PRINTBUTTONLC,

		PRINTPLAIN,
		PRINTPLAINFORM,

		SAVEDATA,
		LOADDATA,
		DELDATA,
		GETTIME,//2つに代入する必要があるので__METHOD__化できない

		TRYJUMP,
		TRYCALL,
		TRYGOTO,
		JUMPFORM,
		CALLFORM,
		GOTOFORM,
		TRYJUMPFORM,
		TRYCALLFORM,
		TRYGOTOFORM,
		CALLTRAIN,
		STOPCALLTRAIN,
		CATCH,
		ENDCATCH,
		TRYCJUMP,
		TRYCCALL,
		TRYCGOTO,
		TRYCJUMPFORM,
		TRYCCALLFORM,
		TRYCGOTOFORM,
		TRYCALLLIST,
		TRYJUMPLIST,
		TRYGOTOLIST,
		FUNC,
		ENDFUNC,
		CALLF,
		CALLFORMF,

		SETCOLOR,
		SETCOLORBYNAME,
		RESETCOLOR,
		SETBGCOLOR,
		SETBGCOLORBYNAME,
		RESETBGCOLOR,
		FONTBOLD,
		FONTITALIC,
		FONTREGULAR,
		SORTCHARA,
		FONTSTYLE,
		ALIGNMENT,
		CUSTOMDRAWLINE,
		DRAWLINEFORM,
		CLEARTEXTBOX,

		SETFONT,

		FOR,
		NEXT,
		WHILE,
		WEND,

		POWER,//引数が違うのでMETHOD化できない。
		SAVEGLOBAL,
		LOADGLOBAL,
		SWAP,

		RESETDATA,
		RESETGLOBAL,

		RANDOMIZE,
		DUMPRAND,
		INITRAND,

		REDRAW,
		DOTRAIN,

		SELECTCASE,
		CASE,
		CASEELSE,
		ENDSELECT,

		DO,
		LOOP,

		PRINTDATA,
		PRINTDATAL,
		PRINTDATAW,
		DATA,
		DATAFORM,
		ENDDATA,
		DATALIST,
		ENDLIST,
		STRDATA,

		PRINTCPERLINE,//よく考えたら引数の仕様違うや


		SETBIT,
		CLEARBIT,
		INVERTBIT,
		DELALLCHARA,
		PICKUPCHARA,

		VARSET,
		CVARSET,

		RESET_STAIN,

		SAVENOS,//引数の仕様が違うので(ry

		FORCEKANA,

		SKIPDISP,
		NOSKIP,
		ENDNOSKIP,

		ARRAYSHIFT,
		ARRAYREMOVE,
		ARRAYSORT,
		ARRAYCOPY,

		ENCODETOUNI,

		DEBUGPRINT,
		DEBUGPRINTL,
		DEBUGPRINTFORM,
		DEBUGPRINTFORML,
		DEBUGCLEAR,
		ASSERT,
		THROW,

		SAVEVAR,
		LOADVAR,
		//		CHKVARDATA,
		SAVECHARA,
		LOADCHARA,
		//		CHKCHARADATA,

		REF,
		REFBYNAME,

		PRINTK,
		PRINTKL,
		PRINTKW,

		PRINTVK,//変数の内容
		PRINTVKL,
		PRINTVKW,

		PRINTSK,//文字列変数の内容
		PRINTSKL,
		PRINTSKW,

		PRINTFORMK,//{数式}、%文字列変数%などの書式が使える。
		PRINTFORMKL,
		PRINTFORMKW,

		PRINTFORMSK,//文字列変数の内容を変換して表示。
		PRINTFORMSKL,
		PRINTFORMSKW,

		PRINTCK,//??
		PRINTLCK,
		PRINTFORMCK,
		PRINTFORMLCK,

		PRINTSINGLEK,
		PRINTSINGLEVK,
		PRINTSINGLESK,
		PRINTSINGLEFORMK,
		PRINTSINGLEFORMSK,

		PRINTDATAK,
		PRINTDATAKL,
		PRINTDATAKW,

		PRINTD,//文字を表示する
		PRINTDL,//改行
		PRINTDW,//入力待ち(実質改行)

		PRINTVD,//変数の内容
		PRINTVDL,
		PRINTVDW,

		PRINTSD,//文字列変数の内容
		PRINTSDL,
		PRINTSDW,

		PRINTFORMD,//{数式}、%文字列変数%などの書式が使える。
		PRINTFORMDL,
		PRINTFORMDW,

		PRINTFORMSD,//文字列変数の内容を変換して表示。
		PRINTFORMSDL,
		PRINTFORMSDW,

		PRINTCD,//??
		PRINTLCD,
		PRINTFORMCD,
		PRINTFORMLCD,

		PRINTSINGLED,
		PRINTSINGLEVD,
		PRINTSINGLESD,
		PRINTSINGLEFORMD,
		PRINTSINGLEFORMSD,

		PRINTDATAD,
		PRINTDATADL,
		PRINTDATADW,

		HTML_PRINT,
		HTML_TAGSPLIT,

		TOOLTIP_SETCOLOR,
		TOOLTIP_SETDELAY,
        TOOLTIP_SETDURATION,

		PRINT_IMG,
		PRINT_RECT,
		PRINT_SPACE,

		INPUTMOUSEKEY,
	}
}
