using System;
using System.Collections.Generic;
using System.Text;
using MinorShift.Emuera.GameProc;
using MinorShift.Emuera.GameData;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.GameData.Variable;
using MinorShift.Emuera.GameView;
using uEmuera.Window;

namespace MinorShift.Emuera
{
	/* 1756 作成
	 * できるだけデータはprivateにして必要なものだけが参照するようにしようという設計だったのは今は昔。
	 * 改変のたびにProcess.Instance.XXXなんかがどんどん増えていく。
	 * まあ、増えるのは仕方ないと諦める事にして、行儀の悪い参照の仕方をするものたちをせめて一箇所に集めて管理しようという計画である。
	 * これからはInstanceを public static に解放することはやめ、ここから参照する。
	 * しかし、できるならここからの参照は減らしたい。
	 */
	internal static class GlobalStatic
	{
		//これは生成される順序で並んでいる。
		//下から上を参照した場合、nullを返されることがある。
		//Config Replace
		public static MainWindow MainWindow;
		public static EmueraConsole Console;
		public static Process Process;
		//Config.RenameDic
		public static GameBase GameBaseData;
		public static ConstantData ConstantData;
		public static VariableData VariableData;
		//StrForm
		public static VariableEvaluator VEvaluator;
		public static IdentifierDictionary IdentifierDictionary;
		public static ExpressionMediator EMediator;
		//
		public static LabelDictionary LabelDictionary;


		//ERBloaderに引数解析の結果を渡すための橋渡し変数
		//1756 Processから移動。Program.AnalysisMode用
		public static Dictionary<string, Int64> tempDic = new Dictionary<string, long>();
#if UEMUERA_DEBUG
		public static List<FunctionLabelLine> StackList = new List<FunctionLabelLine>();
#endif
		public static void Reset()
		{
			Process = null;
			ConstantData = null;
			GameBaseData = null;
			EMediator = null;
			VEvaluator = null;
			VariableData = null;
			Console = null;
			MainWindow = null;
			LabelDictionary = null;
			IdentifierDictionary = null;
			tempDic.Clear();
		}
	}
}
