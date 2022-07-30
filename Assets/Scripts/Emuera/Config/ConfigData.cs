using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
//using System.Windows.Forms;
//using System.Drawing;
using MinorShift.Emuera.Sub;
using System.Text.RegularExpressions;
using MinorShift.Emuera.GameData.Expression;
using uEmuera.Drawing;

namespace MinorShift.Emuera
{
	/// <summary>
	/// プログラム全体で使用される値でWindow作成前に設定して以後変更されないもの
	/// (という予定だったが今は違う)
	/// 1756 Config → ConfigDataへ改名
	/// </summary>
	internal sealed class ConfigData
	{
		static string configPath
        { get { return Program.ExeDir + "emuera.config"; } }
		static string configdebugPath
        { get { return Program.DebugDir + "debug.config"; } }

static ConfigData() { }
		private static ConfigData instance = new ConfigData();
		public static ConfigData Instance { get { return instance; } }

		private ConfigData() { setDefault(); }

		//適当に大き目の配列を作っておく。
		private AConfigItem[] configArray = new AConfigItem[70];
		private AConfigItem[] replaceArray = new AConfigItem[50];
		private AConfigItem[] debugArray = new AConfigItem[20];

		private void setDefault()
		{
			int i = 0;
			configArray[i++] = new ConfigItem<bool>(ConfigCode.IgnoreCase, "大文字小文字の違いを無視する", true);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.UseRenameFile, "_Rename.csvを利用する", false);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.UseReplaceFile, "_Replace.csvを利用する", true);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.UseMouse, "マウスを使用する", true);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.UseMenu, "メニューを使用する", true);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.UseDebugCommand, "デバッグコマンドを使用する", false);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.AllowMultipleInstances, "多重起動を許可する", true);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.AutoSave, "オートセーブを行なう", true);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.UseKeyMacro, "キーボードマクロを使用する", true);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.SizableWindow, "ウィンドウの高さを可変にする", true);
			configArray[i++] = new ConfigItem<TextDrawingMode>(ConfigCode.TextDrawingMode, "描画インターフェース", TextDrawingMode.TEXTRENDERER);
			//configArray[i++] = new ConfigItem<bool>(ConfigCode.UseImageBuffer, "イメージバッファを使用する", true);
			configArray[i++] = new ConfigItem<int>(ConfigCode.WindowX, "ウィンドウ幅", 760);
			configArray[i++] = new ConfigItem<int>(ConfigCode.WindowY, "ウィンドウ高さ", 480);
			configArray[i++] = new ConfigItem<int>(ConfigCode.WindowPosX, "ウィンドウ位置X", 0);
			configArray[i++] = new ConfigItem<int>(ConfigCode.WindowPosY, "ウィンドウ位置Y", 0);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.SetWindowPos, "起動時のウィンドウ位置を指定する", false);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.WindowMaximixed, "起動時にウィンドウを最大化する", false);
			configArray[i++] = new ConfigItem<int>(ConfigCode.MaxLog, "履歴ログの行数", 5000);
			configArray[i++] = new ConfigItem<int>(ConfigCode.PrintCPerLine, "PRINTCを並べる数", 3);
			configArray[i++] = new ConfigItem<int>(ConfigCode.PrintCLength, "PRINTCの文字数", 25);
			configArray[i++] = new ConfigItem<string>(ConfigCode.FontName, "フォント名", "ＭＳ ゴシック");
			configArray[i++] = new ConfigItem<int>(ConfigCode.FontSize, "フォントサイズ", 18);
			configArray[i++] = new ConfigItem<int>(ConfigCode.LineHeight, "一行の高さ", 19);
			configArray[i++] = new ConfigItem<Color>(ConfigCode.ForeColor, "文字色", Color.FromArgb(192, 192, 192));//LIGHTGRAY
			configArray[i++] = new ConfigItem<Color>(ConfigCode.BackColor, "背景色", Color.FromArgb(0, 0, 0));//BLACK
			configArray[i++] = new ConfigItem<Color>(ConfigCode.FocusColor, "選択中文字色", Color.FromArgb(255, 255, 0));//YELLOW
			configArray[i++] = new ConfigItem<Color>(ConfigCode.LogColor, "履歴文字色", Color.FromArgb(192, 192, 192));//LIGHTGRAY//Color.FromArgb(128, 128, 128);//GRAY
			configArray[i++] = new ConfigItem<int>(ConfigCode.FPS, "フレーム毎秒", 5);
			configArray[i++] = new ConfigItem<int>(ConfigCode.SkipFrame, "最大スキップフレーム数", 3);
			configArray[i++] = new ConfigItem<int>(ConfigCode.ScrollHeight, "スクロール行数", 1);
			configArray[i++] = new ConfigItem<int>(ConfigCode.InfiniteLoopAlertTime, "無限ループ警告までのミリ秒数", 5000);
			configArray[i++] = new ConfigItem<int>(ConfigCode.DisplayWarningLevel, "表示する最低警告レベル", 1);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.DisplayReport, "ロード時にレポートを表示する", false);
			configArray[i++] = new ConfigItem<ReduceArgumentOnLoadFlag>(ConfigCode.ReduceArgumentOnLoad, "ロード時に引数を解析する", ReduceArgumentOnLoadFlag.NO);
			//configArray[i++] = new ConfigItem<bool>(ConfigCode.ReduceFormattedStringOnLoad, "ロード時にFORM文字列を解析する", true);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.IgnoreUncalledFunction, "呼び出されなかった関数を無視する", true);
			configArray[i++] = new ConfigItem<DisplayWarningFlag>(ConfigCode.FunctionNotFoundWarning, "関数が見つからない警告の扱い", DisplayWarningFlag.IGNORE);
			configArray[i++] = new ConfigItem<DisplayWarningFlag>(ConfigCode.FunctionNotCalledWarning, "関数が呼び出されなかった警告の扱い", DisplayWarningFlag.IGNORE);
			//configArray[i++] = new ConfigItem<List<string>>(ConfigCode.IgnoreWarningFiles, "指定したファイル中の警告を無視する", new List<string>());
			configArray[i++] = new ConfigItem<bool>(ConfigCode.ChangeMasterNameIfDebug, "デバッグコマンドを使用した時にMASTERの名前を変更する", true);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.ButtonWrap, "ボタンの途中で行を折りかえさない", false);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.SearchSubdirectory, "サブディレクトリを検索する", false);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.SortWithFilename, "読み込み順をファイル名順にソートする", false);
			configArray[i++] = new ConfigItem<long>(ConfigCode.LastKey, "最終更新コード", 0);
			configArray[i++] = new ConfigItem<int>(ConfigCode.SaveDataNos, "表示するセーブデータ数", 20);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.WarnBackCompatibility, "eramaker互換性に関する警告を表示する", true);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.AllowFunctionOverloading, "システム関数の上書きを許可する", true);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.WarnFunctionOverloading, "システム関数が上書きされたとき警告を表示する", true);
			configArray[i++] = new ConfigItem<string>(ConfigCode.TextEditor, "関連づけるテキストエディタ", "notepad");
            configArray[i++] = new ConfigItem<TextEditorType>(ConfigCode.EditorType, "テキストエディタコマンドライン指定", TextEditorType.USER_SETTING);
			configArray[i++] = new ConfigItem<string>(ConfigCode.EditorArgument, "エディタに渡す行指定引数", "");
			configArray[i++] = new ConfigItem<bool>(ConfigCode.WarnNormalFunctionOverloading, "同名の非イベント関数が複数定義されたとき警告する", false);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.CompatiErrorLine, "解釈不可能な行があっても実行する", false);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.CompatiCALLNAME, "CALLNAMEが空文字列の時にNAMEを代入する", false);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.UseSaveFolder, "セーブデータをsavフォルダ内に作成する", false);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.CompatiRAND, "擬似変数RANDの仕様をeramakerに合わせる", false);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.CompatiDRAWLINE, "DRAWLINEを常に新しい行で行う", false);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.CompatiFunctionNoignoreCase, "関数・属性については大文字小文字を無視しない", false); ;
			configArray[i++] = new ConfigItem<bool>(ConfigCode.SystemAllowFullSpace, "全角スペースをホワイトスペースに含める", true);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.SystemSaveInUTF8, "セーブデータをUTF-8で保存する", false);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.CompatiLinefeedAs1739, "ver1739以前の非ボタン折り返しを再現する", false);
            configArray[i++] = new ConfigItem<UseLanguage>(ConfigCode.useLanguage, "内部で使用する東アジア言語", UseLanguage.JAPANESE);
            configArray[i++] = new ConfigItem<bool>(ConfigCode.AllowLongInputByMouse, "ONEINPUT系命令でマウスによる2文字以上の入力を許可する", false);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.CompatiCallEvent, "イベント関数のCALLを許可する", false);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.CompatiSPChara, "SPキャラを使用する", false);
			
			configArray[i++] = new ConfigItem<bool>(ConfigCode.SystemSaveInBinary, "セーブデータをバイナリ形式で保存する", false);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.CompatiFuncArgOptional, "ユーザー関数の全ての引数の省略を許可する", false);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.CompatiFuncArgAutoConvert, "ユーザー関数の引数に自動的にTOSTRを補完する", false);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.SystemIgnoreTripleSymbol, "FORM中の三連記号を展開しない", false);
            configArray[i++] = new ConfigItem<bool>(ConfigCode.TimesNotRigorousCalculation, "TIMESの計算をeramakerにあわせる", false);
            //一文字変数の禁止オプションを考えた名残
			//configArray[i++] = new ConfigItem<bool>(ConfigCode.ForbidOneCodeVariable, "一文字変数の使用を禁止する", false);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.SystemNoTarget, "キャラクタ変数の引数を補完しない", false);
			configArray[i++] = new ConfigItem<bool>(ConfigCode.SystemIgnoreStringSet, "文字列変数の代入に文字列式を強制する", false);

			i = 0;
			debugArray[i++] = new ConfigItem<bool>(ConfigCode.DebugShowWindow, "起動時にデバッグウインドウを表示する", true);
			debugArray[i++] = new ConfigItem<bool>(ConfigCode.DebugWindowTopMost, "デバッグウインドウを最前面に表示する", true);
			debugArray[i++] = new ConfigItem<int>(ConfigCode.DebugWindowWidth, "デバッグウィンドウ幅", 400);
			debugArray[i++] = new ConfigItem<int>(ConfigCode.DebugWindowHeight, "デバッグウィンドウ高さ", 300);
			debugArray[i++] = new ConfigItem<bool>(ConfigCode.DebugSetWindowPos, "デバッグウィンドウ位置を指定する", false);
			debugArray[i++] = new ConfigItem<int>(ConfigCode.DebugWindowPosX, "デバッグウィンドウ位置X", 0);
			debugArray[i++] = new ConfigItem<int>(ConfigCode.DebugWindowPosY, "デバッグウィンドウ位置Y", 0);

			i = 0;
			replaceArray[i++] = new ConfigItem<string>(ConfigCode.MoneyLabel, "お金の単位", "$");
			replaceArray[i++] = new ConfigItem<bool>(ConfigCode.MoneyFirst, "単位の位置", true);
			replaceArray[i++] = new ConfigItem<string>(ConfigCode.LoadLabel, "起動時簡略表示", "Now Loading...");
			replaceArray[i++] = new ConfigItem<int>(ConfigCode.MaxShopItem, "販売アイテム数", 100);
			replaceArray[i++] = new ConfigItem<string>(ConfigCode.DrawLineString, "DRAWLINE文字", "-");
			replaceArray[i++] = new ConfigItem<char>(ConfigCode.BarChar1, "BAR文字1", '*');
			replaceArray[i++] = new ConfigItem<char>(ConfigCode.BarChar2, "BAR文字2", '.');
			replaceArray[i++] = new ConfigItem<string>(ConfigCode.TitleMenuString0, "システムメニュー0", "最初からはじめる");
			replaceArray[i++] = new ConfigItem<string>(ConfigCode.TitleMenuString1, "システムメニュー1", "ロードしてはじめる");
			replaceArray[i++] = new ConfigItem<int>(ConfigCode.ComAbleDefault, "COM_ABLE初期値", 1);
			replaceArray[i++] = new ConfigItem<List<Int64>>(ConfigCode.StainDefault, "汚れの初期値", new List<Int64>(new Int64[] { 0, 0, 2, 1, 8 }));
			replaceArray[i++] = new ConfigItem<string>(ConfigCode.TimeupLabel, "時間切れ表示", "時間切れ");
			replaceArray[i++] = new ConfigItem<List<Int64>>(ConfigCode.ExpLvDef, "EXPLVの初期値", new List<long>(new Int64[] { 0, 1, 4, 20, 50, 200 }));
			replaceArray[i++] = new ConfigItem<List<Int64>>(ConfigCode.PalamLvDef, "PALAMLVの初期値", new List<long>(new Int64[] { 0, 100, 500, 3000, 10000, 30000, 60000, 100000, 150000, 250000 }));
			replaceArray[i++] = new ConfigItem<Int64>(ConfigCode.pbandDef, "PBANDの初期値", 4);
            replaceArray[i++] = new ConfigItem<Int64>(ConfigCode.RelationDef, "RELATIONの初期値", 0);
		}

        public void Clear()
        {
            configArray = new AConfigItem[70];
            replaceArray = new AConfigItem[50];
            debugArray = new AConfigItem[20];
            setDefault();
        }

		public ConfigData Copy()
		{
			ConfigData config = new ConfigData();
			for (int i = 0; i < configArray.Length; i++)
				if ((this.configArray[i] != null) && (config.configArray[i] != null))
					this.configArray[i].CopyTo(config.configArray[i]);
			for (int i = 0; i < configArray.Length; i++)
				if ((this.configArray[i] != null) && (config.configArray[i] != null))
					this.configArray[i].CopyTo(config.configArray[i]);
			for (int i = 0; i < replaceArray.Length; i++)
				if ((this.replaceArray[i] != null) && (config.replaceArray[i] != null))
					this.replaceArray[i].CopyTo(config.replaceArray[i]);
			return config;
		}

		public Dictionary<ConfigCode,string> GetConfigNameDic()
		{
			Dictionary<ConfigCode, string> ret = new Dictionary<ConfigCode, string>();
			foreach (AConfigItem item in configArray)
			{
				if (item != null)
					ret.Add(item.Code, item.Text);
			}
			return ret;
		}

		public T GetConfigValue<T>(ConfigCode code)
		{
			AConfigItem item = GetItem(code);
            //if ((item != null) && (item is ConfigItem<T>))
				return ((ConfigItem<T>)item).Value;
            //throw new ExeEE("GetConfigValueのCodeまたは型が不適切");
		}

#region getitem
		public AConfigItem GetItem(ConfigCode code)
		{
			AConfigItem item = GetConfigItem(code);
            if (item == null)
            {
                item = GetReplaceItem(code);
	            if (item == null)
	            {
	                item = GetDebugItem(code);
	            }
            }
			return item;
		}
		public AConfigItem GetItem(string key)
		{
			AConfigItem item = GetConfigItem(key);
			if (item == null)
			{
				item = GetReplaceItem(key);
	            if (item == null)
	            {
					item = GetDebugItem(key);
	            }
	        }
			return item;
		}

		public AConfigItem GetConfigItem(ConfigCode code)
		{
			foreach (AConfigItem item in configArray)
			{
				if (item == null)
					continue;
				if (item.Code == code)
					return item;
			}
			return null;
		}
		public AConfigItem GetConfigItem(string key)
		{
			foreach (AConfigItem item in configArray)
			{
				if (item == null)
					continue;
				if (item.Name == key)
					return item;
				if (item.Text == key)
					return item;
			}
			return null;
		}

		public AConfigItem GetReplaceItem(ConfigCode code)
		{
			foreach (AConfigItem item in replaceArray)
			{
				if (item == null)
					continue;
				if (item.Code == code)
					return item;
			}
			return null;
		}
		public AConfigItem GetReplaceItem(string key)
		{
			foreach (AConfigItem item in replaceArray)
			{
				if (item == null)
					continue;
				if (item.Name == key)
					return item;
				if (item.Text == key)
					return item;
			}
			return null;
		}
		
		public AConfigItem GetDebugItem(ConfigCode code)
		{
			foreach (AConfigItem item in debugArray)
			{
				if (item == null)
					continue;
				if (item.Code == code)
					return item;
			}
			return null;
		}
		public AConfigItem GetDebugItem(string key)
		{
			foreach (AConfigItem item in debugArray)
			{
				if (item == null)
					continue;
				if (item.Name == key)
					return item;
				if (item.Text == key)
					return item;
			}
			return null;
		}
		
		public SingleTerm GetConfigValueInERB(string text, ref string errMes)
		{
			AConfigItem item = ConfigData.Instance.GetItem(text);
			if(item == null)
			{
				errMes = "文字列\"" + text + "\"は適切なコンフィグ名ではありません";
				return null;
			}
			SingleTerm term;
			switch(item.Code)
			{
				//<bool>
				case ConfigCode.AutoSave://"オートセーブを行なう"
				case ConfigCode.MoneyFirst://"単位の位置"
					if(item.GetValue<bool>())
						term = new SingleTerm(1);
					else
						term = new SingleTerm(0);
					break;
				//<int>
				case ConfigCode.WindowX:// "ウィンドウ幅"
				case ConfigCode.PrintCPerLine:// "PRINTCを並べる数"
				case ConfigCode.PrintCLength:// "PRINTCの文字数"
				case ConfigCode.FontSize:// "フォントサイズ"
				case ConfigCode.LineHeight:// "一行の高さ"
				case ConfigCode.SaveDataNos:// "表示するセーブデータ数"
				case ConfigCode.MaxShopItem:// "販売アイテム数"
				case ConfigCode.ComAbleDefault:// "COM_ABLE初期値"
					term = new SingleTerm(item.GetValue<int>());
					break;
				//<Color>
				case ConfigCode.ForeColor://"文字色"
				case ConfigCode.BackColor://"背景色"
				case ConfigCode.FocusColor://"選択中文字色"
				case ConfigCode.LogColor://"履歴文字色"
					{
						Color color = item.GetValue<Color>();
						term = new SingleTerm( ((color.R * 256) + color.G) * 256 + color.B);
					}
					break;

				//<Int64>
				case ConfigCode.pbandDef:// "PBANDの初期値"
				case ConfigCode.RelationDef:// "RELATIONの初期値"
					term = new SingleTerm(item.GetValue<Int64>());
					break;

				//<string>
				case ConfigCode.FontName:// "フォント名"
				case ConfigCode.MoneyLabel:// "お金の単位"
				case ConfigCode.LoadLabel:// "起動時簡略表示"
				case ConfigCode.DrawLineString:// "DRAWLINE文字"
				case ConfigCode.TitleMenuString0:// "システムメニュー0"
				case ConfigCode.TitleMenuString1:// "システムメニュー1"
				case ConfigCode.TimeupLabel:// "時間切れ表示"
					term = new SingleTerm(item.GetValue<string>());
					break;
				
				//<char>
				case ConfigCode.BarChar1:// "BAR文字1"
				case ConfigCode.BarChar2:// "BAR文字2"
					term = new SingleTerm(item.GetValue<char>().ToString());
					break;
				//<TextDrawingMode>
				case ConfigCode.TextDrawingMode:// "描画インターフェース"
					term = new SingleTerm(item.GetValue<TextDrawingMode>().ToString());
					break;
				default:
				{
					errMes = "コンフィグ文字列\"" + text + "\"の値の取得は許可されていません";
					return null;
				}
			}
			return term;
		}
#endregion


		public bool SaveConfig()
		{
			StreamWriter writer = null;

			try
			{
				writer = new StreamWriter(configPath, false, Config.Encode);
				for (int i = 0; i < configArray.Length; i++)
				{
					AConfigItem item = configArray[i];
					if (item == null)
						continue;
					
					//1806beta001 CompatiDRAWLINEの廃止、CompatiLinefeedAs1739へ移行
					if (item.Code == ConfigCode.CompatiDRAWLINE)
						continue;
					if ((item.Code == ConfigCode.ChangeMasterNameIfDebug) && (item.GetValue<bool>()))
						continue;
					if ((item.Code == ConfigCode.LastKey) && (item.GetValue<long>() == 0))
						continue;
					//if (item.Code == ConfigCode.IgnoreWarningFiles)
					//{
					//    List<string> files = item.GetValue<List<string>>();
					//    foreach (string filename in files)
					//        writer.WriteLine(item.Text + ":" + filename.ToString());
					//    continue;
					//}
					writer.WriteLine(item.ToString());
				}
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				if (writer != null)
					writer.Close();
			}
			return true;
		}

        public bool ReLoadConfig()
        {
            //_fixed.configの中身が変わった場合、非固定になったものが保持されてしまうので、ここで一旦すべて解除
            foreach (AConfigItem item in configArray)
            {
                if (item == null)
                    continue;
                if (item.Fixed)
                    item.Fixed = false;
            }
            LoadConfig();
            return true;
        }

		public bool LoadConfig()
		{
			Config.ClearFont();
			string defaultConfigPath = Program.CsvDir + "_default.config";
			string fixedConfigPath = Program.CsvDir + "_fixed.config";
			if(!File.Exists(defaultConfigPath))
				defaultConfigPath = Program.CsvDir + "default.config";
			if (!File.Exists(fixedConfigPath))
				fixedConfigPath = Program.CsvDir + "fixed.config";

			loadConfig(defaultConfigPath, false);
			loadConfig(configPath, false);
			loadConfig(fixedConfigPath, true);
			
			Config.SetConfig(this);
			bool needSave = false;
			if (!File.Exists(configPath))
				needSave = true;
			if (Config.CheckUpdate())
			{
				GetItem(ConfigCode.LastKey).SetValue(Config.LastKey);
				needSave = true;
			}
			if (needSave)
				SaveConfig();
            return true;
		}

		private bool loadConfig(string confPath, bool fix)
		{
			if (!File.Exists(confPath))
				return false;
			EraStreamReader eReader = new EraStreamReader(false);
			if (!eReader.Open(confPath))
				return false;

			//加载二进制数据
			var bytes = File.ReadAllBytes(confPath);
			var md5s = GenericUtils.CalcMd5ListForConfig(bytes);

			ScriptPosition pos = null;
			int md5i = 0;
			try
			{
				string line = null;
				//bool defineIgnoreWarningFiles = false;
				while ((line = eReader.ReadLine()) != null)
				{
					var md5 = md5s[md5i++];
					if ((line.Length == 0) || (line[0] == ';'))
						continue;
					pos = new ScriptPosition(eReader.Filename, eReader.LineNo);
					string[] tokens = line.Split(new char[] { ':' });
					if (tokens.Length < 2)
						continue;
                    var token_0 = tokens[0].Trim();
                    AConfigItem item = GetConfigItem(token_0);
                    if(item == null)
                    {
                        token_0 = uEmuera.Utils.SHIFTJIS_to_UTF8(token_0, md5);
                        if(!string.IsNullOrEmpty(token_0))
                            item = GetConfigItem(token_0);
                    }
					if (item != null)
					{
						//1806beta001 CompatiDRAWLINEの廃止、CompatiLinefeedAs1739へ移行
						if(item.Code == ConfigCode.CompatiDRAWLINE)
						{
							item = GetConfigItem(ConfigCode.CompatiLinefeedAs1739);
						}
						//if ((item.Code == ConfigCode.IgnoreWarningFiles))
						//{ 
						//    if (!defineIgnoreWarningFiles)
						//        (item.GetValue<List<string>>()).Clear();
						//    defineIgnoreWarningFiles = true;
						//    if ((item.Fixed) && (fix))
						//        item.Fixed = false;
						//}
						
						if (item.Code == ConfigCode.TextEditor)
						{
							//パスの関係上tokens[2]は使わないといけない
							if (tokens.Length > 2)
							{
								if (tokens[2].StartsWith("\\"))
									tokens[1] += ":" + tokens[2];
								if (tokens.Length > 3)
								{
									for (int i = 3; i < tokens.Length; i++)
									{
										tokens[1] += ":" + tokens[i];
									}
								}
							}
						}
						if (item.Code == ConfigCode.EditorArgument)
						{
							//半角スペースを要求する引数が必要なエディタがあるので別処理で
							((ConfigItem<string>)item).Value = tokens[1];
							continue;
						}
                        if (item.Code == ConfigCode.MaxLog && Program.AnalysisMode)
                        {
                            //解析モード時はここを上書きして十分な長さを確保する
                            tokens[1] = "10000";
                        }
						if ((item.TryParse(tokens[1])) && (fix))
							item.Fixed = true;
					}
#if UEMUERA_DEBUG
					//else
					//	throw new Exception("コンフィグファイルが変");
#endif
				}
			}
			catch (EmueraException ee)
			{
				ParserMediator.ConfigWarn(ee.Message, pos, 1, null);
			}
			catch (Exception exc)
			{
				ParserMediator.ConfigWarn(exc.GetType().ToString() + ":" + exc.Message, pos, 1, exc.StackTrace);
			}
			finally { eReader.Dispose(); }
			return true;
		}

#region replace
		// 1.52a改変部分　（単位の差し替えおよび前置、後置のためのコンフィグ処理）
		public void LoadReplaceFile(string filename)
		{
			EraStreamReader eReader = new EraStreamReader(false);
			if (!eReader.Open(filename))
				return;
			ScriptPosition pos = null;
			try
			{
				string line = null;
				while ((line = eReader.ReadLine()) != null)
				{
					if ((line.Length == 0) || (line[0] == ';'))
						continue;
					pos = new ScriptPosition(eReader.Filename, eReader.LineNo);
                    string[] tokens = line.Split(new char[] { ',', ':' });
					if (tokens.Length < 2)
						continue;
                    string itemName = tokens[0].Trim();
                    tokens[1] = line.Substring(tokens[0].Length + 1);
                    if (string.IsNullOrEmpty(tokens[1].Trim()))
                        continue;
                    AConfigItem item = GetReplaceItem(itemName);
                    if (item != null)
                        item.TryParse(tokens[1]);
				}
			}
			catch (EmueraException ee)
			{
				ParserMediator.Warn(ee.Message, pos, 1);
			}
			catch (Exception exc)
			{
				ParserMediator.Warn(exc.GetType().ToString() + ":" + exc.Message, pos, 1, exc.StackTrace);
			}
			finally { eReader.Dispose(); }
		}

#endregion 

#region debug


		public bool SaveDebugConfig()
		{
			StreamWriter writer = null;
			try
			{
				writer = new StreamWriter(configdebugPath, false, Config.Encode);
				for (int i = 0; i < debugArray.Length; i++)
				{
					AConfigItem item = debugArray[i];
					if (item == null)
						continue;
					writer.WriteLine(item.ToString());
				}
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				if (writer != null)
					writer.Close();
			}
			return true;
		}
		
		public bool LoadDebugConfig()
		{
			if (!File.Exists(configdebugPath))
				goto err;
			EraStreamReader eReader = new EraStreamReader(false);
			if (!eReader.Open(configdebugPath))
				goto err;
			ScriptPosition pos = null;
			try
			{
				string line = null;
				while ((line = eReader.ReadLine()) != null)
				{
					if ((line.Length == 0) || (line[0] == ';'))
						continue;
					pos = new ScriptPosition(eReader.Filename, eReader.LineNo);
					string[] tokens = line.Split(new char[] { ':' });
					if (tokens.Length < 2)
						continue;
					AConfigItem item = GetDebugItem(tokens[0].Trim());
					if (item != null)
					{
						item.TryParse(tokens[1]);
					}
#if UEMUERA_DEBUG
					//else
					//	throw new Exception("コンフィグファイルが変");
#endif
				}
			}
			catch (EmueraException ee)
			{
				ParserMediator.ConfigWarn(ee.Message, pos, 1, null);
				goto err;
			}
			catch (Exception exc)
			{
				ParserMediator.ConfigWarn(exc.GetType().ToString() + ":" + exc.Message, pos, 1, exc.StackTrace);
				goto err;
			}
			finally { eReader.Dispose(); }
			Config.SetDebugConfig(this);
            return true;
		err:
			Config.SetDebugConfig(this);
			return false;
		}

#endregion
	}
}