using System;
using System.Collections.Generic;
using System.Text;

namespace MinorShift.Emuera.GameData.Expression
{
	//難読化用属性。enum.ToString()やenum.Parse()を行うなら(Exclude=true)にすること。
	[global::System.Reflection.Obfuscation(Exclude=false)]
	internal enum CaseExpressionType
	{
		Normal = 1,
		To = 2,
		Is = 3,
	}
	internal sealed class CaseExpression
	{
		public CaseExpressionType CaseType = CaseExpressionType.Normal;
		public IOperandTerm LeftTerm;
		public IOperandTerm RightTerm;

		public OperatorCode Operator;
		public Type GetOperandType()
		{
			if(LeftTerm != null)
				return LeftTerm.GetOperandType();
			return typeof(void);
		}
		
		public void Reduce(ExpressionMediator exm)
		{
			LeftTerm = LeftTerm.Restructure(exm);
			if (CaseType == CaseExpressionType.To)
				RightTerm = RightTerm.Restructure(exm);
		}
		
		public override string ToString()
		{
			switch (CaseType)
			{
				case CaseExpressionType.Normal:
					return LeftTerm.ToString();
				case CaseExpressionType.Is:
					return "Is " + Operator.ToString() + " " + LeftTerm.ToString();
				case CaseExpressionType.To:
					return LeftTerm.ToString() + " To " + RightTerm.ToString();
			}

			return base.ToString();
		}

		public bool GetBool(Int64 Is, ExpressionMediator exm)
		{
			if (CaseType == CaseExpressionType.To)
				return LeftTerm.GetIntValue(exm) <= Is && Is <= RightTerm.GetIntValue(exm);
			if (CaseType == CaseExpressionType.Is)
			{
				IOperandTerm term = OperatorMethodManager.ReduceBinaryTerm(Operator, new SingleTerm(Is), LeftTerm);
				return term.GetIntValue(exm) != 0;
			}
			return LeftTerm.GetIntValue(exm) == Is;
		}
		
		public bool GetBool(string Is, ExpressionMediator exm)
		{
			if (CaseType == CaseExpressionType.To)
			{
				return string.Compare(LeftTerm.GetStrValue(exm), Is, Config.SCExpression) <= 0
					&& string.Compare(Is, RightTerm.GetStrValue(exm), Config.SCExpression) <= 0;
			}
			if (CaseType == CaseExpressionType.Is)
			{
				IOperandTerm term = OperatorMethodManager.ReduceBinaryTerm(Operator, new SingleTerm(Is), LeftTerm);
				return term.GetIntValue(exm) != 0;
			}
			return LeftTerm.GetStrValue(exm) == Is;
		}
	}
}

