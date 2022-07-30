using System;
using System.Collections.Generic;
using MinorShift.Emuera.Sub;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.GameData.Function;
using MinorShift.Emuera.GameView;
using MinorShift.Emuera.GameData.Variable;

namespace MinorShift.Emuera.GameProc
{
	//1756 インナークラス解除して一般に開放


	//難読化用属性。enum.ToString()やenum.Parse()を行うなら(Exclude=true)にすること。
	[global::System.Reflection.Obfuscation(Exclude = false)]
	internal enum SystemStateCode
	{
		__CAN_SAVE__ = 0x10000,//セーブロード画面を呼び出し可能か？
		__CAN_BEGIN__ = 0x20000,//BEGIN命令を呼び出し可能か？
		Title_Begin = 0,//初期状態
		Openning = 1,//最初の入力待ち
		Train_Begin = 0x10,//BEGIN TRAINから。
		Train_CallEventTrain = 0x11,//@EVENTTRAINの呼び出し中。スキップ可能
		Train_CallShowStatus = 0x12,//@SHOW_STATUSの呼び出し中
		Train_CallComAbleXX = 0x13,//@COM_ABLExxの呼び出し中。スキップの場合、RETURN 1とする。
		Train_CallShowUserCom = 0x14,//@SHOW_USERCOMの呼び出し中
		Train_WaitInput = 0x15,//入力待ち状態。選択が実行可能ならEVENTCOMからCOMxx、そうでなければ@USERCOMにRESULTを渡す
		Train_CallEventCom = 0x16 | __CAN_BEGIN__,//@EVENTCOMの呼び出し中

		Train_CallComXX = 0x17 | __CAN_BEGIN__,//@COMxxの呼び出し中
		Train_CallSourceCheck = 0x18 | __CAN_BEGIN__,//@SOURCE_CHECKの呼び出し中
		Train_CallEventComEnd = 0x19 | __CAN_BEGIN__,//@EVENTCOMENDの呼び出し中。スキップ可能。Train_CallEventTrainへ帰る。@USERCOMの呼び出し中もここ

		Train_DoTrain = 0x1A,

		AfterTrain_Begin = 0x20 | __CAN_BEGIN__,//BEGIN AFTERTRAINから。@EVENTENDを呼び出してNormalへ。

		Ablup_Begin = 0x30,//BEGIN ABLUPから。
		Ablup_CallShowJuel = 0x31,//@SHOW_JUEL
		Ablup_CallShowAblupSelect = 0x32,//@SHOW_ABLUP_SELECT
		Ablup_WaitInput = 0x33,//
		Ablup_CallAblupXX = 0x34 | __CAN_BEGIN__,//@ABLUPxxがない場合は、@USERABLUPにRESULTを渡す。Ablup_CallShowJuelへ戻る。

		Turnend_Begin = 0x40 | __CAN_BEGIN__,//BEGIN TURNENDから。@EVENTTURNENDを呼び出してNormalへ。

		Shop_Begin = 0x50 | __CAN_SAVE__,//BEGIN SHOPから
		Shop_CallEventShop = 0x51 | __CAN_BEGIN__ | __CAN_SAVE__,//@EVENTSHOPの呼び出し中。スキップ可能
		Shop_CallShowShop = 0x52 | __CAN_SAVE__,//@SHOW_SHOPの呼び出し中
		Shop_WaitInput = 0x53 | __CAN_SAVE__,//入力待ち状態。アイテムが存在するならEVENTBUYにBOUGHT、そうでなければ@USERSHOPにRESULTを渡す
		Shop_CallEventBuy = 0x54 | __CAN_BEGIN__ | __CAN_SAVE__,//@USERSHOPまた@EVENTBUYはの呼び出し中

		SaveGame_Begin = 0x100,//SAVEGAMEから
		SaveGame_WaitInput = 0x101,//入力待ち
		SaveGame_WaitInputOverwrite = 0x102,//上書きの許可待ち
		SaveGame_CallSaveInfo = 0x103,//@SAVEINFO呼び出し中。20回。
		LoadGame_Begin = 0x110,//LOADGAMEから
		LoadGame_WaitInput = 0x111,//入力待ち
		LoadGameOpenning_Begin = 0x120,//最初に[1]を選択したとき。
		LoadGameOpenning_WaitInput = 0x121,//入力待ち


		//AutoSave_Begin = 0x200,
		AutoSave_CallSaveInfo = 0x201,
		AutoSave_CallUniqueAutosave = 0x202,
		AutoSave_Skipped = 0x203,

		LoadData_DataLoaded = 0x210,//データロード直後
		LoadData_CallSystemLoad = 0x211 | __CAN_BEGIN__,//データロード直後
		LoadData_CallEventLoad = 0x212 | __CAN_BEGIN__,//@EVENTLOADの呼び出し中。スキップ可能

		Openning_TitleLoadgame = 0x220,

		System_Reloaderb = 0x230,
		First_Begin = 0x240,

		Normal = 0xFFFF | __CAN_BEGIN__ | __CAN_SAVE__,//特に何でもないとき。ScriptEndに達したらエラー
	}

	//難読化用属性。enum.ToString()やenum.Parse()を行うなら(Exclude=true)にすること。
	[global::System.Reflection.Obfuscation(Exclude = false)]
	internal enum BeginType
	{
		NULL = 0,
		SHOP = 2,
		TRAIN = 3,
		AFTERTRAIN = 4,
		ABLUP = 5,
		TURNEND = 6,
		FIRST = 7,
		TITLE = 8,
	}

	internal sealed class ProcessState
	{
		public ProcessState(EmueraConsole console)
		{
			if (Program.DebugMode)//DebugModeでなければ知らなくて良い
				this.console = console;
		}
		readonly EmueraConsole console = null;
		readonly List<CalledFunction> functionList = new List<CalledFunction>();
		private LogicalLine currentLine;
		//private LogicalLine nextLine;
		public int lineCount = 0;
        public int currentMin = 0;
        //private bool sequential;

		public bool ScriptEnd
		{
			get
			{
                return functionList.Count == currentMin;
            }
		}

        public int functionCount
        {
            get
            {
                return functionList.Count;
            }
        }

		SystemStateCode sysStateCode = SystemStateCode.Title_Begin;
		BeginType begintype = BeginType.NULL;
		public bool isBegun { get { return (begintype != BeginType.NULL) ? true : false; } }

        public LogicalLine CurrentLine { get { return currentLine; } set { currentLine = value; } }
        public LogicalLine ErrorLine
		{
			get
			{
				//if (RunningLine != null)
				//	return RunningLine;
				return currentLine;
			}
		}

		//IF文中でELSEIF文の中身をチェックするなどCurrentLineと作業中のLineが違う時にセットする
		//public LogicalLine RunningLine { get; set; }
		//1755a 呼び出し元消滅
		//public bool Sequential { get { return sequential; } }
		public CalledFunction CurrentCalled
		{
			get
			{
				//実行関数なしの状態は一部のシステムINPUT以外では存在しないのでGOTO系の処理でしかここに来ない関係上、前提を満たしようがない
				//if (functionList.Count == 0)
				//    throw new ExeEE("実行中関数がない");
				return functionList[functionList.Count - 1];
			}
		}
		public SystemStateCode SystemState
		{
			get { return sysStateCode; }
			set { sysStateCode = value; }
		}

		public void ShiftNextLine()
		{
            currentLine = currentLine.NextLine;
            //nextLine = nextLine.NextLine;
            //RunningLine = null;
            //sequential = true;
			//GlobalStatic.Process.lineCount++;
			lineCount++;
		}

		/// <summary>
		/// 関数内の移動。JUMPではなくGOTOやIF文など
		/// </summary>
		/// <param name="line"></param>
		public void JumpTo(LogicalLine line)
		{
            currentLine = line;
            lineCount++;
            //sequential = false;
			//ShfitNextLine();
		}

		public void SetBegin(string keyword)
		{//TrimとToUpper済みのはず
			switch (keyword)
			{
				case "SHOP":
					SetBegin(BeginType.SHOP); return;
				case "TRAIN":
					SetBegin(BeginType.TRAIN); return;
				case "AFTERTRAIN":
					SetBegin(BeginType.AFTERTRAIN); return;
				case "ABLUP":
					SetBegin(BeginType.ABLUP); return;
				case "TURNEND":
					SetBegin(BeginType.TURNEND); return;
				case "FIRST":
					SetBegin(BeginType.FIRST); return;
				case "TITLE":
					SetBegin(BeginType.TITLE); return;
			}
			throw new CodeEE("BEGINのキーワード\"" + keyword + "\"は未定義です");
		}

		public void SetBegin(BeginType type)
		{
			string errmes;
			switch (type)
			{
				case BeginType.SHOP:
				case BeginType.TRAIN:
				case BeginType.AFTERTRAIN:
				case BeginType.ABLUP:
				case BeginType.TURNEND:
				case BeginType.FIRST:
					if ((sysStateCode & SystemStateCode.__CAN_BEGIN__) != SystemStateCode.__CAN_BEGIN__)
					{
						errmes = "BEGIN";
						goto err;
					}
					break;
				//1.729 BEGIN TITLEはどこでも使えるように
				case BeginType.TITLE:
					break;
				//BEGINの処理中でチェック済み
				//default:
				//    throw new ExeEE("不適当なBEGIN呼び出し");
			}
			begintype = type;
			return;
		err:
			CalledFunction func = functionList[0];
			string funcName = func.FunctionName;
			throw new CodeEE("@" + funcName + "中で" + errmes + "命令を実行することはできません");
		}

		public void SaveLoadData(bool saveData)
		{

			if (saveData)
				sysStateCode = SystemStateCode.SaveGame_Begin;
			else
				sysStateCode = SystemStateCode.LoadGame_Begin;
			//ClearFunctionList();
			return;
		}

		public void ClearFunctionList()
		{
			if (Program.DebugMode && !isClone && GlobalStatic.Process.MethodStack() == 0)
				console.DebugClearTraceLog();
			foreach (CalledFunction called in functionList)
                if (called.CurrentLabel.hasPrivDynamicVar)
                    called.CurrentLabel.Out();
			functionList.Clear();
			begintype = BeginType.NULL;
		}

		public bool calledWhenNormal = true;
		/// <summary>
		/// BEGIN命令によるプログラム状態の変化
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public void Begin()
		{
			//@EVENTSHOPからの呼び出しは一旦破棄
			if (sysStateCode == SystemStateCode.Shop_CallEventShop)
				return;

			switch (begintype)
			{
				case BeginType.SHOP:
					if (sysStateCode == SystemStateCode.Normal)
						calledWhenNormal = true;
					else
						calledWhenNormal = false;
					sysStateCode = SystemStateCode.Shop_Begin;
					break;
				case BeginType.TRAIN:
					sysStateCode = SystemStateCode.Train_Begin;
					break;
				case BeginType.AFTERTRAIN:
					sysStateCode = SystemStateCode.AfterTrain_Begin;
					break;
				case BeginType.ABLUP:
					sysStateCode = SystemStateCode.Ablup_Begin;
					break;
				case BeginType.TURNEND:
					sysStateCode = SystemStateCode.Turnend_Begin;
					break;
				case BeginType.FIRST:
					sysStateCode = SystemStateCode.First_Begin;
					break;
				case BeginType.TITLE:
					sysStateCode = SystemStateCode.Title_Begin;
					break;
				//セット時に判定してるので、ここには来ないはず
				//default:
				//    throw new ExeEE("不適当なBEGIN呼び出し");
			}
			if (Program.DebugMode)
			{
				console.DebugClearTraceLog();
				console.DebugAddTraceLog("BEGIN:" + begintype.ToString());
			}
			foreach (CalledFunction called in functionList)
                if (called.CurrentLabel.hasPrivDynamicVar)
                    called.CurrentLabel.Out();
			functionList.Clear();
			begintype = BeginType.NULL;
			return;
		}

		/// <summary>
		/// システムによる強制的なBEGIN
		/// </summary>
		/// <param name="type"></param>
		public void Begin(BeginType type)
		{
			begintype = type;
			sysStateCode = SystemStateCode.Title_Begin;
			Begin();
		}

		public LogicalLine GetCurrentReturnAddress
		{
			get
			{
                if (functionList.Count == currentMin)
                    return null;
				return functionList[functionList.Count - 1].ReturnAddress;
			}
		}

        public LogicalLine GetReturnAddressSequensial(int curerntDepth)
        {
            if (functionList.Count == currentMin)
                return null;
            return functionList[functionList.Count - curerntDepth - 1].ReturnAddress;
        }

		public string Scope
		{
			get
			{
				//スクリプトの実行中処理からしか呼び出されないので、ここはない…はず
				//if (functionList.Count == 0)
				//{
				//    throw new ExeEE("実行中の関数が存在しません");
				//}
				if (functionList.Count == 0)
					return null;//1756 デバッグコマンドから呼び出されるようになったので
				return functionList[functionList.Count - 1].FunctionName;
			}
		}

		public void Return(Int64 ret)
		{
			if (IsFunctionMethod)
			{
				ReturnF(null);
				return;
			}
			//sequential = false;//いずれにしろ順列ではない。
			//呼び出し元は全部スクリプト処理
			//if (functionList.Count == 0)
			//{
			//    throw new ExeEE("実行中の関数が存在しません");
			//}
			CalledFunction called = functionList[functionList.Count - 1];
			if (called.IsJump)
			{//JUMPした場合。即座にRETURN RESULTする。
                if (called.TopLabel.hasPrivDynamicVar)
                    called.TopLabel.Out();
				functionList.Remove(called);
				if (Program.DebugMode)
					console.DebugRemoveTraceLog();
				Return(ret);
				return;
			}
			if (!called.IsEvent)
			{
                if (called.TopLabel.hasPrivDynamicVar)
                    called.TopLabel.Out();
                currentLine = null;
            }
			else
			{
                if (called.CurrentLabel.hasPrivDynamicVar)
                    called.CurrentLabel.Out();
				//#Singleフラグ付き関数で1が返された。
				//1752 非0ではなく1と等価であることを見るように修正
				//1756 全てを終了ではなく#PRIや#LATERのグループごとに修正
                if (called.IsOnly)
                    called.FinishEvent();
				else if ((called.HasSingleFlag) && (ret == 1))
					called.ShiftNextGroup();
				else
                    called.ShiftNext();//次の同名関数に進む。
                currentLine = called.CurrentLabel;//関数の始点(@～～)へ移動。呼ぶべき関数が無ければnull
                if (called.CurrentLabel != null)
                {
                    lineCount++;
                    if (called.CurrentLabel.hasPrivDynamicVar)
                        called.CurrentLabel.In();
                }
            }
			if (Program.DebugMode)
				console.DebugRemoveTraceLog();
			//関数終了
            if (currentLine == null)
            {
                currentLine = called.ReturnAddress;
                functionList.RemoveAt(functionList.Count - 1);
				if (currentLine == null)
				{
					//この時点でfunctionListは空のはず
					//functionList.Clear();//全て終了。stateEndProcessに処理を返す
					if (begintype != BeginType.NULL)//BEGIN XXが行なわれていれば
					{
						Begin();
					}
					return;
				}
                lineCount++;
                //ShfitNextLine();
                return;
			}
			else if (Program.DebugMode)
			{
				FunctionLabelLine label = called.CurrentLabel;
				console.DebugAddTraceLog("CALL :@" + label.LabelName + ":" + label.Position.ToString() + "行目");
			}
            lineCount++;
            //ShfitNextLine();
            return;
		}

		public void IntoFunction(CalledFunction call, UserDefinedFunctionArgument srcArgs, ExpressionMediator exm)
		{

			if (call.IsEvent)
			{
				foreach (CalledFunction called in functionList)
				{
					if (called.IsEvent)
						throw new CodeEE("EVENT関数の解決前にCALLEVENT命令が行われました");
				}
			}
			if (Program.DebugMode)
			{
				FunctionLabelLine label = call.CurrentLabel;
				if (call.IsJump)
					console.DebugAddTraceLog("JUMP :@" + label.LabelName + ":" + label.Position.ToString() + "行目");
				else
					console.DebugAddTraceLog("CALL :@" + label.LabelName + ":" + label.Position.ToString() + "行目");
			}
            if (srcArgs != null)
            {
                //引数の値を確定させる
                srcArgs.SetTransporter(exm);
                //プライベート変数更新
                if (call.TopLabel.hasPrivDynamicVar)
                    call.TopLabel.In();
                //更新した変数へ引数を代入
                for (int i = 0; i < call.TopLabel.Arg.Length; i++)
                {
                    if (srcArgs.Arguments[i] != null)
                    {
						if (call.TopLabel.Arg[i].Identifier.IsReference)
							((ReferenceToken)(call.TopLabel.Arg[i].Identifier)).SetRef(srcArgs.TransporterRef[i]);
                        else if (srcArgs.Arguments[i].GetOperandType() == typeof(Int64))
                            call.TopLabel.Arg[i].SetValue(srcArgs.TransporterInt[i], exm);
                        else
                            call.TopLabel.Arg[i].SetValue(srcArgs.TransporterStr[i], exm);
                    }
                }
            }
            else//こっちに来るのはシステムからの呼び出し=引数は存在しない関数のみ ifネストの外に出していい気もしないでもないがはてさて
            {
                //プライベート変数更新
                if (call.TopLabel.hasPrivDynamicVar)
                    call.TopLabel.In();
            }
			functionList.Add(call);
			//sequential = false;
            currentLine = call.CurrentLabel;
            lineCount++;
            //ShfitNextLine();
        }

		#region userdifinedmethod
		public bool IsFunctionMethod
		{
			get
			{
                return functionList[currentMin].TopLabel.IsMethod;
            }
		}

		public SingleTerm MethodReturnValue = null;

		public void ReturnF(SingleTerm ret)
		{
			//読み込み時のチェック済みのはず
			//if (!IsFunctionMethod)
			//    throw new ExeEE("ReturnFと#FUNCTIONのチェックがおかしい");
			//sequential = false;//いずれにしろ順列ではない。
			//呼び出し元はRETURNFコマンドか関数終了時のみ
			//if (functionList.Count == 0)
			//    throw new ExeEE("実行中の関数が存在しません");
			//非イベント呼び出しなので、これは起こりえない
			//else if (functionList.Count != 1)
			//    throw new ExeEE("関数が複数ある");
			if (Program.DebugMode)
			{
				console.DebugRemoveTraceLog();
			}
			//OutはGetValue側で行う
			//functionList[0].TopLabel.Out();
            currentLine = functionList[functionList.Count - 1].ReturnAddress;
            functionList.RemoveAt(functionList.Count - 1);
            //nextLine = null;
            MethodReturnValue = ret;
            return;
		}

		#endregion

		bool isClone = false;
        public bool IsClone { get { return isClone; } set { isClone = value; } }

		// functionListのコピーを必要とする呼び出し元が無かったのでコピーしないことにする。
		public ProcessState Clone()
		{
			ProcessState ret = new ProcessState(console);
			ret.isClone = true;
			//どうせ消すからコピー不要
			//foreach (CalledFunction func in functionList)
			//	ret.functionList.Add(func.Clone());
			ret.currentLine = this.currentLine;
            //ret.nextLine = this.nextLine;
            //ret.sequential = this.sequential;
			ret.sysStateCode = this.sysStateCode;
			ret.begintype = this.begintype;
			//ret.MethodReturnValue = this.MethodReturnValue;
			return ret;

		}
		//public ProcessState CloneForFunctionMethod()
		//{
		//    ProcessState ret = new ProcessState(console);
		//    ret.isClone = true;

		//    //どうせ消すからコピー不要
		//    //foreach (CalledFunction func in functionList)
		//    //	ret.functionList.Add(func.Clone());
		//    ret.currentLine = this.currentLine;
		//    ret.nextLine = this.nextLine;
		//    //ret.sequential = this.sequential;
		//    ret.sysStateCode = this.sysStateCode;
		//    ret.begintype = this.begintype;
		//    //ret.MethodReturnValue = this.MethodReturnValue;
		//    return ret;
		//}
	}
}