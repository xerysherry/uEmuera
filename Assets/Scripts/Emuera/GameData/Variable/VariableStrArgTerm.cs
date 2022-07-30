using System;
using System.Collections.Generic;
using System.Text;
using MinorShift.Emuera.Sub;
using MinorShift.Emuera.GameProc;
using MinorShift.Emuera.GameData.Expression;

namespace MinorShift.Emuera.GameData.Variable
{
	
	//変数の引数のうち文字列型のもの。
	internal sealed class VariableStrArgTerm : IOperandTerm
	{
		public VariableStrArgTerm(VariableCode code, IOperandTerm strTerm, int index)
			: base(typeof(Int64))
		{
			this.strTerm = strTerm;
			parentCode = code;
			this.index = index;
		}
		IOperandTerm strTerm;
		readonly VariableCode parentCode;
		readonly int index;
		Dictionary<string, int> dic = null;
		string errPos = null;
		
        public override Int64 GetIntValue(ExpressionMediator exm)
		{
			if (dic == null)
				dic = exm.VEvaluator.Constant.GetKeywordDictionary(out errPos, parentCode, index);
			string key = strTerm.GetStrValue(exm);
			if (key == "")
				throw new CodeEE("キーワードを空には出来ません");
            if (!dic.TryGetValue(key, out int i))
            {
                if (errPos == null)
                    throw new CodeEE("配列変数" + parentCode.ToString() + "の要素を文字列で指定することはできません");
                else
                    throw new CodeEE(errPos + "の中に\"" + key + "\"の定義がありません");
            }
            return i;
        }
		
        public override IOperandTerm Restructure(ExpressionMediator exm)
        {
			if (dic == null)
				dic = exm.VEvaluator.Constant.GetKeywordDictionary(out errPos, parentCode, index);
			strTerm = strTerm.Restructure(exm);
			if (!(strTerm is SingleTerm))
				return this;
			return new SingleTerm(this.GetIntValue(exm));
        }
	}

}