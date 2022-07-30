using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
//using System.Windows.Forms;
using MinorShift.Emuera.GameData;
using MinorShift.Emuera.Sub;
using MinorShift.Emuera.GameView;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.GameData.Variable;
using MinorShift.Emuera.GameProc.Function;
using MinorShift.Emuera.GameData.Function;
using System.Linq;
using uEmuera.Forms;

namespace MinorShift.Emuera.GameProc
{

	internal sealed partial class Process
	{
		public Process(EmueraConsole view)
		{
			console = view;
		}

        public LogicalLine getCurrentLine { get { return state.CurrentLine; } }

		/// <summary>
		/// @~~と$~~を集めたもの。CALL命令などで使う
		/// 実行順序はLogicalLine自身が保持する。
		/// </summary>
		LabelDictionary labelDic;
		public LabelDictionary LabelDictionary { get { return labelDic; } }

		/// <summary>
		/// 変数全部。スクリプト中で必要になる変数は（ユーザーが直接触れないものも含め）この中にいれる
		/// </summary>
		private VariableEvaluator vEvaluator;
		public VariableEvaluator VEvaluator { get { return vEvaluator; } }
		private ExpressionMediator exm;
		private GameBase gamebase;
		readonly EmueraConsole console;
		private IdentifierDictionary idDic;
		ProcessState state;
		ProcessState originalState;//リセットする時のために
        bool noError = false;
        //色々あって復活させてみる
        bool initialiing;
        public bool inInitializeing { get { return initialiing;  } }

        public bool Initialize()
		{
			LexicalAnalyzer.UseMacro = false;
            state = new ProcessState(console);
            originalState = state;
            initialiing = true;
            try
            {
				ParserMediator.Initialize(console);
				//コンフィグファイルに関するエラーの処理（コンフィグファイルはこの関数に入る前に読込済み）
				if (ParserMediator.HasWarning)
				{
					ParserMediator.FlushWarningList();
					if(MessageBox.Show("コンフィグファイルに異常があります\nEmueraを終了しますか","コンフィグエラー", MessageBoxButtons.YesNo)
						== DialogResult.Yes)
					{
						console.PrintSystemLine("コンフィグファイルに異常があり、終了が選択されたため処理を終了しました");
						return false;
					}
				}
				//リソースフォルダ読み込み
				if (!Content.AppContents.LoadContents())
				{
					ParserMediator.FlushWarningList();
					console.PrintSystemLine("リソースフォルダ読み込み中に異常が発見されたため処理を終了します");
					return false;
				}
				ParserMediator.FlushWarningList();
				//キーマクロ読み込み
                if (Config.UseKeyMacro && !Program.AnalysisMode)
                {
                    if (File.Exists(Program.ExeDir + "macro.txt"))
                    {
                        if (Config.DisplayReport)
							console.PrintSystemLine("macro.txt読み込み中・・・");
                        KeyMacro.LoadMacroFile(Program.ExeDir + "macro.txt");
                    }
				}
				//_replace.csv読み込み
                if (Config.UseReplaceFile && !Program.AnalysisMode)
                {
					if (File.Exists(Program.CsvDir + "_Replace.csv"))
					{
						if (Config.DisplayReport)
							console.PrintSystemLine("_Replace.csv読み込み中・・・");
						ConfigData.Instance.LoadReplaceFile(Program.CsvDir + "_Replace.csv");
						if (ParserMediator.HasWarning)
						{
							ParserMediator.FlushWarningList();
							if (MessageBox.Show("_Replace.csvに異常があります\nEmueraを終了しますか", "_Replace.csvエラー", MessageBoxButtons.YesNo)
								== DialogResult.Yes)
							{
								console.PrintSystemLine("_Replace.csvに異常があり、終了が選択されたため処理を終了しました");
								return false;
							}
						}
					}
                }
                Config.SetReplace(ConfigData.Instance);
                //ここでBARを設定すれば、いいことに気づいた予感
                console.setStBar(Config.DrawLineString);

				//_rename.csv読み込み
				if (Config.UseRenameFile)
                {
					if (File.Exists(Program.CsvDir + "_Rename.csv"))
                    {
                        if (Config.DisplayReport || Program.AnalysisMode)
							console.PrintSystemLine("_Rename.csv読み込み中・・・");
						ParserMediator.LoadEraExRenameFile(Program.CsvDir + "_Rename.csv");
                    }
                    else
                        console.PrintError("csv\\_Rename.csvが見つかりません");
                }
                if (!Config.DisplayReport)
                {
                    console.PrintSingleLine(Config.LoadLabel);
                    console.RefreshStrings(true);
				}
				//gamebase.csv読み込み
				gamebase = new GameBase();
                if (!gamebase.LoadGameBaseCsv(Program.CsvDir + "GAMEBASE.CSV"))
                {
					ParserMediator.FlushWarningList();
                    console.PrintSystemLine("GAMEBASE.CSVの読み込み中に問題が発生したため処理を終了しました");
                    return false;
                }
				console.SetWindowTitle(gamebase.ScriptWindowTitle);
				GlobalStatic.GameBaseData = gamebase;

				//前記以外のcsvを全て読み込み
				ConstantData constant = new ConstantData();
				constant.LoadData(Program.CsvDir, console, Config.DisplayReport);
				GlobalStatic.ConstantData = constant;
				TrainName = constant.GetCsvNameList(VariableCode.TRAINNAME);

                vEvaluator = new VariableEvaluator(gamebase, constant);
				GlobalStatic.VEvaluator = vEvaluator;

				idDic = new IdentifierDictionary(vEvaluator.VariableData);
				GlobalStatic.IdentifierDictionary = idDic;

				StrForm.Initialize();
				VariableParser.Initialize();

				exm = new ExpressionMediator(this, vEvaluator, console);
				GlobalStatic.EMediator = exm;

				labelDic = new LabelDictionary();
				GlobalStatic.LabelDictionary = labelDic;
				HeaderFileLoader hLoader = new HeaderFileLoader(console, idDic, this);

				LexicalAnalyzer.UseMacro = false;

				//ERH読込
				if (!hLoader.LoadHeaderFiles(Program.ErbDir, Config.DisplayReport))
				{
					ParserMediator.FlushWarningList();
					console.PrintSystemLine("ERHの読み込み中にエラーが発生したため処理を終了しました");
					return false;
				}
				LexicalAnalyzer.UseMacro = idDic.UseMacro();

				//TODO:ユーザー定義変数用のcsvの適用

				//ERB読込
				ErbLoader loader = new ErbLoader(console, exm, this);
                if (Program.AnalysisMode)
                    noError = loader.loadErbs(Program.AnalysisFiles, labelDic);
                else
                    noError = loader.LoadErbFiles(Program.ErbDir, Config.DisplayReport, labelDic);
                initSystemProcess();
                initialiing = false;
            }
			catch (Exception e)
			{
                handleException(e, null, true);
				console.PrintSystemLine("初期化中に致命的なエラーが発生したため処理を終了しました");
				return false;
			}
			if (labelDic == null)
			{
				return false;
			}
			state.Begin(BeginType.TITLE);
			GC.Collect();
            return true;
		}

		public void ReloadErb()
		{
			saveCurrentState(false);
			state.SystemState = SystemStateCode.System_Reloaderb;
			ErbLoader loader = new ErbLoader(console, exm, this);
            loader.LoadErbFiles(Program.ErbDir, false, labelDic);
			console.ReadAnyKey();
		}

		public void ReloadPartialErb(List<string> path)
		{
			saveCurrentState(false);
			state.SystemState = SystemStateCode.System_Reloaderb;
			ErbLoader loader = new ErbLoader(console, exm, this);
			loader.loadErbs(path, labelDic);
			console.ReadAnyKey();
		}

		public void SetCommnds(Int64 count)
		{
			coms = new List<long>((int)count);
			isCTrain = true;
			Int64[] selectcom = vEvaluator.SELECTCOM_ARRAY;
			if (count >= selectcom.Length)
			{
				throw new CodeEE("CALLTRAIN命令の引数の値がSELECTCOMの要素数を超えています");
			}
			for (int i = 0; i < (int)count; i++)
			{
				coms.Add(selectcom[i + 1]);
			}
		}

        public bool ClearCommands()
        {
            coms.Clear();
            count = 0;
            isCTrain = false;
            skipPrint = true;
            return (callFunction("CALLTRAINEND", false, false));
        }

		public void InputResult5(int r0, int r1, int r2, int r3, int r4)
		{
			long[] result = vEvaluator.RESULT_ARRAY;
			result[0] = r0;
			result[1] = r1;
			result[2] = r2;
			result[3] = r3;
			result[4] = r4;
		}
		public void InputInteger(Int64 i)
		{
			vEvaluator.RESULT = i;
		}
		public void InputSystemInteger(Int64 i)
		{
			systemResult = i;
		}
		public void InputString(string s)
		{
			vEvaluator.RESULTS = s;
		}

		private uint startTime = 0;
		
		public void DoScript()
		{
			startTime = _Library.WinmmTimer.TickCount;
			state.lineCount = 0;
			bool systemProcRunning = true;
			try
			{
				while (true)
				{
					methodStack = 0;
					systemProcRunning = true;
					while (state.ScriptEnd && console.IsRunning)
						runSystemProc();
					if (!console.IsRunning)
						break;
					systemProcRunning = false;
					runScriptProc();
				}
			}
			catch (Exception ec)
			{
				LogicalLine currentLine = state.ErrorLine;
				if (currentLine != null && currentLine is NullLine)
					currentLine = null;
				if (systemProcRunning)
					handleExceptionInSystemProc(ec, currentLine, true);
				else
					handleException(ec, currentLine, true);
			}
		}
		
		public void BeginTitle()
		{
			vEvaluator.ResetData();
			state = originalState;
			state.Begin(BeginType.TITLE);
		}

		public void UpdateCheckInfiniteLoopState()
		{
			startTime = _Library.WinmmTimer.TickCount;
			state.lineCount = 0;
		}

		private void checkInfiniteLoop()
		{
			//うまく動かない。BEEP音が鳴るのを止められないのでこの処理なかったことに（1.51）
			////フリーズ防止。処理中でも履歴を見たりできる
			//System.Windows.Forms.Application.DoEvents();
			////System.Threading.Thread.Sleep(0);

			//if (!console.Enabled)
			//{
			//    //DoEvents()の間にウインドウが閉じられたらおしまい。
			//    console.ReadAnyKey();
			//    return;
			//}
			uint time = _Library.WinmmTimer.TickCount - startTime;
			if (time < Config.InfiniteLoopAlertTime)
				return;
			LogicalLine currentLine = state.CurrentLine;
			if ((currentLine == null) || (currentLine is NullLine))
				return;//現在の行が特殊な状態ならスルー
			if (!console.Enabled)
				return;//クローズしてるとMessageBox.Showができないので。
			string caption = string.Format("無限ループの可能性があります");
			string text = string.Format(
				"現在、{0}の{1}行目を実行中です。\n最後の入力から{3}ミリ秒経過し{2}行が実行されました。\n処理を中断し強制終了しますか？",
				currentLine.Position.Filename, currentLine.Position.LineNo, state.lineCount, time);
			DialogResult result = MessageBox.Show(text, caption, MessageBoxButtons.YesNo);
			if (result == DialogResult.Yes)
			{
				throw new CodeEE("無限ループの疑いにより強制終了が選択されました");
			}
			else
			{
				state.lineCount = 0;
				startTime = _Library.WinmmTimer.TickCount;
			}
		}

		int methodStack = 0;
		public SingleTerm GetValue(SuperUserDefinedMethodTerm udmt)
		{
			methodStack++;
            if (methodStack > 100)
            {
                //StackOverflowExceptionはcatchできない上に再現性がないので発生前に一定数で打ち切る。
                //環境によっては100以前にStackOverflowExceptionがでるかも？
                throw new CodeEE("関数の呼び出しスタックが溢れました(無限に再帰呼び出しされていませんか？)");
            }
            SingleTerm ret = null;
            int temp_current = state.currentMin;
            state.currentMin = state.functionCount;
            udmt.Call.updateRetAddress(state.CurrentLine);
            try
            {
				state.IntoFunction(udmt.Call, udmt.Argument, exm);
                //do whileの中でthrow されたエラーはここではキャッチされない。
				//#functionを全て抜けてDoScriptでキャッチされる。
    			runScriptProc();
                ret = state.MethodReturnValue;
			}
			finally
			{
				if (udmt.Call.TopLabel.hasPrivDynamicVar)
					udmt.Call.TopLabel.Out();
                //1756beta2+v3:こいつらはここにないとデバッグコンソールで式中関数が事故った時に大事故になる
                state.currentMin = temp_current;
                methodStack--;
            }
			return ret;
		}

        public void clearMethodStack()
        {
            methodStack = 0;
        }

        public int MethodStack()
        {
            return methodStack;
        }

		public ScriptPosition GetRunningPosition()
		{
			LogicalLine line = state.ErrorLine;
			if (line == null)
				return null;
			return line.Position;
		}
/*
		private readonly string scaningScope = null;
		private string GetScaningScope()
		{
			if (scaningScope != null)
				return scaningScope;
			return state.Scope;
		}
*/
		public LogicalLine scaningLine = null;
		internal LogicalLine GetScaningLine()
		{
			if (scaningLine != null)
				return scaningLine;
			LogicalLine line = state.ErrorLine;
			if (line == null)
				return null;
			return line;
		}
		
		
		private void handleExceptionInSystemProc(Exception exc, LogicalLine current, bool playSound)
		{
			console.ThrowError(playSound);
			if (exc is CodeEE)
			{
				console.PrintError("関数の終端でエラーが発生しました:" + Program.ExeName);
				console.PrintError(exc.Message);
			}
			else if (exc is ExeEE)
			{
				console.PrintError("関数の終端でEmueraのエラーが発生しました:" + Program.ExeName);
				console.PrintError(exc.Message);
			}
			else
			{
				console.PrintError("関数の終端で予期しないエラーが発生しました:" + Program.ExeName);
				console.PrintError(exc.GetType().ToString() + ":" + exc.Message);
				string[] stack = exc.StackTrace.Split('\n');
				for (int i = 0; i < stack.Length; i++)
				{
					console.PrintError(stack[i]);
				}
			}
		}
		
		private void handleException(Exception exc, LogicalLine current, bool playSound)
		{
            UnityEngine.Debug.Log(exc);

			console.ThrowError(playSound);
			ScriptPosition position = null;
            if ((exc is EmueraException ee) && (ee.Position != null))
                position = ee.Position;
            else if ((current != null) && (current.Position != null))
				position = current.Position;
			string posString = "";
			if (position != null)
			{
				if (position.LineNo >= 0)
					posString = position.Filename + "の" + position.LineNo.ToString() + "行目で";
				else
					posString = position.Filename + "で";
					
			}
			if (exc is CodeEE)
			{
                if (position != null)
				{
                    if (current is InstructionLine procline && procline.FunctionCode == FunctionCode.THROW)
                    {
                        console.PrintErrorButton(posString + "THROWが発生しました", position);
                        printRawLine(position);
                        console.PrintError("THROW内容：" + exc.Message);
                    }
                    else
                    {
                        console.PrintErrorButton(posString + "エラーが発生しました:" + Program.ExeName, position);
						printRawLine(position);
						console.PrintError("エラー内容：" + exc.Message);
                    }
                    console.PrintError("現在の関数：@" + current.ParentLabelLine.LabelName + "（" + current.ParentLabelLine.Position.Filename + "の" + current.ParentLabelLine.Position.LineNo.ToString() + "行目）");
                    console.PrintError("関数呼び出しスタック：");
                    LogicalLine parent;
                    int depth = 0;
                    while ((parent = state.GetReturnAddressSequensial(depth++)) != null)
                    {
                        if (parent.Position != null)
                        {
                            console.PrintErrorButton("↑" + parent.Position.Filename + "の" + parent.Position.LineNo.ToString() + "行目（関数@" + parent.ParentLabelLine.LabelName + "内）", parent.Position);
                        }
                    } 
				}
				else
				{
					console.PrintError(posString + "エラーが発生しました:" + Program.ExeName);
					console.PrintError(exc.Message);
				}
			}
			else if (exc is ExeEE)
			{
				console.PrintError(posString + "Emueraのエラーが発生しました:" + Program.ExeName);
				console.PrintError(exc.Message);
			}
			else
            {
				console.PrintError(posString + "予期しないエラーが発生しました:" + Program.ExeName);
				console.PrintError(exc.GetType().ToString() + ":" + exc.Message);
				string[] stack = exc.StackTrace.Split('\n');
				for (int i = 0; i < stack.Length; i++)
				{
					console.PrintError(stack[i]);
				}
			}
		}

		public void printRawLine(ScriptPosition position)
		{
			string str = getRawTextFormFilewithLine(position);
			if (str != "")
				console.PrintError(str);
		}

		public string getRawTextFormFilewithLine(ScriptPosition position)
        {
			string extents = position.Filename.Substring(position.Filename.Length - 4).ToLower();
			if (extents == ".erb")
			{
				return File.Exists(Program.ErbDir + position.Filename)
					? position.LineNo > 0 ? File.ReadLines(Program.ErbDir + position.Filename, Config.Encode).Skip(position.LineNo - 1).First() : ""
					: "";
			}
			else if (extents == ".csv")
			{
				return File.Exists(Program.CsvDir + position.Filename)
					? position.LineNo > 0 ? File.ReadLines(Program.CsvDir + position.Filename, Config.Encode).Skip(position.LineNo - 1).First() : ""
					: "";
			}
			else
				return "";
		}

	}
}
