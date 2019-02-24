
namespace MinorShift.Emuera
{
	//難読化用属性。enum.ToString()やenum.Parse()を行うなら(Exclude=true)にすること。
	[global::System.Reflection.Obfuscation(Exclude=true)]
	internal enum DisplayWarningFlag
	{
		IGNORE = 0,
		LATER = 1,
		ONCE = 2,
		DISPLAY = 3,
	}

	[global::System.Reflection.Obfuscation(Exclude=true)]
	internal enum ReduceArgumentOnLoadFlag
	{
		YES = 0,
		ONCE = 1,
		NO = 2,
	}

	[global::System.Reflection.Obfuscation(Exclude=true)]
	internal enum TextDrawingMode
	{
		GRAPHICS = 0,
		TEXTRENDERER = 1,
		WINAPI = 2,
	}

    [global::System.Reflection.Obfuscation(Exclude = true)]
    internal enum UseLanguage
    {
        JAPANESE = 0,
        KOREAN = 1,
        CHINESE_HANS = 2,
        CHINESE_HANT = 3,        
    }

    [global::System.Reflection.Obfuscation(Exclude = true)]
    internal enum TextEditorType
    {
        SAKURA = 0,
        TERAPAD = 1,
        EMEDITOR = 2,
        USER_SETTING = 3,
    }

	//数字に意味は無い。
	[global::System.Reflection.Obfuscation(Exclude = true)]
	internal enum ConfigCode
	{
		IgnoreCase = 0,
		UseRenameFile = 1,
		UseReplaceFile = 2,
		UseMouse = 3,
		UseMenu = 4,
		UseDebugCommand = 5,
		AllowMultipleInstances = 6,
		AutoSave = 7,
		SizableWindow = 8,
		TextDrawingMode = 9,
		UseImageBuffer = 10,
		WindowX = 11,
		WindowY = 12,
		MaxLog = 13,
		PrintCPerLine = 14,
		PrintCLength = 15,
		FontName = 16,
		FontSize = 17,
		LineHeight = 18,
		ForeColor = 19,
		BackColor = 20,
		FocusColor = 21,
		LogColor = 22,
		FPS = 23,
		SkipFrame = 24,
		InfiniteLoopAlertTime = 25,
		DisplayWarningLevel = 26,
		DisplayReport = 27,
		ReduceArgumentOnLoad = 28,
		//ReduceFormattedStringOnLoad = 29,
		IgnoreUncalledFunction = 30,
		FunctionNotFoundWarning = 31,
		FunctionNotCalledWarning = 32,
		//IgnoreWarningFiles = 33,
		ChangeMasterNameIfDebug = 34,
		LastKey = 35,
		ButtonWrap = 36,
		SearchSubdirectory = 37,
		SortWithFilename = 38,
		SetWindowPos = 39,
		WindowPosX = 40,
		WindowPosY = 41,
		ScrollHeight = 42,
		SaveDataNos = 43,
		WarnBackCompatibility = 44,
		AllowFunctionOverloading = 45,
		WarnFunctionOverloading = 46,
		WindowMaximixed = 47,
		TextEditor = 48,
        EditorType = 99,
		EditorArgument = 49,
		WarnNormalFunctionOverloading = 50,
		CompatiErrorLine = 51,
		CompatiCALLNAME = 52,
		DebugShowWindow = 53,
		DebugWindowTopMost = 54,
		DebugWindowWidth = 55,
		DebugWindowHeight = 56,
		DebugSetWindowPos = 57,
		DebugWindowPosX = 58,
		DebugWindowPosY = 59,
		UseSaveFolder = 60,
		CompatiRAND = 61,
		CompatiDRAWLINE = 62,
		CompatiFunctionNoignoreCase,
		SystemAllowFullSpace,
		SystemSaveInUTF8,
		CompatiLinefeedAs1739,
        useLanguage,
		SystemSaveInBinary,
		CompatiFuncArgAutoConvert,
		CompatiFuncArgOptional,
		AllowLongInputByMouse,
		CompatiCallEvent,
		SystemIgnoreTripleSymbol,
		CompatiSPChara,
        TimesNotRigorousCalculation,
        //一文字変数の禁止オプションを考えた名残
        //ForbidOneCodeVariable,
		SystemNoTarget,
		SystemIgnoreStringSet,

		MoneyLabel = 100,
		MoneyFirst = 101,
		LoadLabel = 102,
		MaxShopItem = 103,
		DrawLineString = 104,
		BarChar1 = 105,
		BarChar2 = 106,
		TitleMenuString0 = 107,
		TitleMenuString1 = 108,
		ComAbleDefault = 109,
		StainDefault = 110,
		TimeupLabel = 111,
		ExpLvDef = 112,
		PalamLvDef = 113,
		pbandDef = 114,
        RelationDef = 115,

		UseKeyMacro = 162,
	}
}