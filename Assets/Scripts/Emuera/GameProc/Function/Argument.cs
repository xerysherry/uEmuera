using System;
using System.Collections.Generic;
using MinorShift.Emuera.GameData;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.GameData.Variable;
using MinorShift.Emuera.GameData.Function;

namespace MinorShift.Emuera.GameProc.Function
{
	internal abstract class Argument
	{
		public bool IsConst;
		public string ConstStr;
		public Int64 ConstInt;
	}

	/// <summary>
	/// 一般的な引数。複数の文字列式及び数式
	/// </summary>
	internal sealed class ExpressionsArgument : Argument
	{
		public ExpressionsArgument(Type[] types, IOperandTerm[] terms)
		{
			ArgumentTypeArray = types;
			ArgumentArray = terms;
		}
		/// <summary>
		/// 引数の型(ArgumentArrayよりもLengthが大きい可能性があるので見るのはArgumentArrayにすること)
		/// </summary>
		readonly public Type[] ArgumentTypeArray;
		readonly public IOperandTerm[] ArgumentArray;
	}

	internal sealed class VoidArgument : Argument
	{
		public VoidArgument() { }
	}

	internal sealed class ErrorArgument : Argument
	{
		public ErrorArgument(string errorMes)
		{
			this.errorMes = errorMes;
		}
		readonly string errorMes;
	}

	internal sealed class ExpressionArgument : Argument
	{
		public ExpressionArgument(IOperandTerm termSrc)
		{
			Term = termSrc;
		}
		readonly public IOperandTerm Term;
	}

	internal sealed class ExpressionArrayArgument : Argument
	{
		public ExpressionArrayArgument(List<IOperandTerm> termList)
		{
			TermList = new IOperandTerm[termList.Count];
			termList.CopyTo(TermList);
		}
		readonly public IOperandTerm[] TermList;
	}

	internal sealed class SpPrintVArgument : Argument
	{
		public SpPrintVArgument(IOperandTerm[] list)
		{
			Terms = list;
		}
		readonly public IOperandTerm[] Terms;
	}

	internal sealed class SpTimesArgument : Argument
	{
		public SpTimesArgument(VariableTerm var, double d)
		{
			VariableDest = var;
			DoubleValue = d;
		}
		readonly public VariableTerm VariableDest;
		readonly public double DoubleValue;
	}

	internal sealed class SpBarArgument : Argument
	{
		public SpBarArgument(IOperandTerm value, IOperandTerm max, IOperandTerm length)
		{
			Terms[0] = value;
			Terms[1] = max;
			Terms[2] = length;
		}
		readonly public IOperandTerm[] Terms = new IOperandTerm[3];
	}


	internal sealed class SpSwapCharaArgument : Argument
	{
		public SpSwapCharaArgument(IOperandTerm x, IOperandTerm y)
		{
			X = x;
			Y = y;
		}
		readonly public IOperandTerm X;
		readonly public IOperandTerm Y;
	}

	internal sealed class SpSwapVarArgument : Argument
	{
		public SpSwapVarArgument(VariableTerm v1, VariableTerm v2)
		{
			var1 = v1;
			var2 = v2;
		}
		readonly public VariableTerm var1;
		readonly public VariableTerm var2;
	}

	internal sealed class SpVarsizeArgument : Argument
	{
		public SpVarsizeArgument(VariableToken var)
		{
			VariableID = var;
		}
		readonly public VariableToken VariableID;
	}

	internal sealed class SpSaveDataArgument : Argument
	{
		public SpSaveDataArgument(IOperandTerm target, IOperandTerm var)
		{
			Target = target;
			StrExpression = var;
		}
		readonly public IOperandTerm Target;
		readonly public IOperandTerm StrExpression;
	}

	internal sealed class SpTInputsArgument : Argument
	{
		public SpTInputsArgument(IOperandTerm time, IOperandTerm def, IOperandTerm disp, IOperandTerm timeout)
		{
			Time = time;
			Def = def;
			Disp = disp;
            Timeout = timeout;
		}
		readonly public IOperandTerm Time;
		readonly public IOperandTerm Def;
		readonly public IOperandTerm Disp;
        readonly public IOperandTerm Timeout;
	}

	//難読化用属性。enum.ToString()やenum.Parse()を行うなら(Exclude=true)にすること。
	[global::System.Reflection.Obfuscation(Exclude = false)]
	internal enum SortOrder
	{
		UNDEF = 0,
		ASCENDING = 1,
		DESENDING = 2,
	}

	internal sealed class SpSortcharaArgument : Argument
	{
		public SpSortcharaArgument(VariableTerm var, SortOrder order)
		{
			SortKey = var;
			SortOrder = order;
		}
		readonly public VariableTerm SortKey;
		readonly public SortOrder SortOrder;
	}

	internal sealed class SpCallFArgment : Argument
	{
		public SpCallFArgment(IOperandTerm funcname, IOperandTerm[] subNames, IOperandTerm[] args)
		{
			FuncnameTerm = funcname;
			SubNames = subNames;
			RowArgs = args;
		}
		readonly public IOperandTerm FuncnameTerm;
		readonly public IOperandTerm[] SubNames;
		readonly public IOperandTerm[] RowArgs;
		public IOperandTerm FuncTerm;
	}

	internal sealed class SpCallArgment : Argument
	{
		public SpCallArgment(IOperandTerm funcname, IOperandTerm[] subNames, IOperandTerm[] args)
		{
			FuncnameTerm = funcname;
			SubNames = subNames;
			RowArgs = args;
		}
		readonly public IOperandTerm FuncnameTerm;
		readonly public IOperandTerm[] SubNames;
		readonly public IOperandTerm[] RowArgs;
		public UserDefinedFunctionArgument UDFArgument;
		public CalledFunction CallFunc;
	}

	internal sealed class SpForNextArgment : Argument
	{
		public SpForNextArgment(VariableTerm var, IOperandTerm start, IOperandTerm end, IOperandTerm step)
		{
			this.Cnt = var;
			this.Start = start;
			this.End = end;
			this.Step = step;
		}
		readonly public VariableTerm Cnt;
		readonly public IOperandTerm Start;
		readonly public IOperandTerm End;
		readonly public IOperandTerm Step;
	}

	internal sealed class SpPowerArgument : Argument
	{
		public SpPowerArgument(VariableTerm var, IOperandTerm x, IOperandTerm y)
		{
			VariableDest = var;
			X = x;
			Y = y;
		}
		readonly public VariableTerm VariableDest;
		readonly public IOperandTerm X;
		readonly public IOperandTerm Y;
	}

	internal sealed class CaseArgument : Argument
	{
		public CaseArgument(CaseExpression[] args)
		{
			CaseExps = args;
		}
		readonly public CaseExpression[] CaseExps;
	}

	internal sealed class PrintDataArgument : Argument
	{
		public PrintDataArgument(VariableTerm var)
		{
			Var = var;
		}
		readonly public VariableTerm Var;
	}

    internal sealed class StrDataArgument : Argument
    {
        public StrDataArgument(VariableTerm var)
        {
            Var = var;
        }
        readonly public VariableTerm Var;
    }

	internal sealed class MethodArgument : Argument
	{
		public MethodArgument(IOperandTerm method)
		{
			MethodTerm = method;
		}
		readonly public IOperandTerm MethodTerm;
	}

	internal sealed class BitArgument : Argument
	{
		public BitArgument(VariableTerm var, IOperandTerm[] termSrc)
		{
			VariableDest = var;
			Term = termSrc;
		}
		readonly public VariableTerm VariableDest;
		readonly public IOperandTerm[] Term;
	}

	internal sealed class SpVarSetArgument : Argument
	{
		public SpVarSetArgument(VariableTerm var, IOperandTerm termSrc, IOperandTerm start, IOperandTerm end)
		{
			VariableDest = var;
			Term = termSrc;
			Start = start;
			End = end;
		}
		readonly public VariableTerm VariableDest;
		readonly public IOperandTerm Term;
		readonly public IOperandTerm Start;
		readonly public IOperandTerm End;
	}

	internal sealed class SpCVarSetArgument : Argument
	{
		public SpCVarSetArgument(VariableTerm var, IOperandTerm indexTerm, IOperandTerm termSrc, IOperandTerm start, IOperandTerm end)
		{
			VariableDest = var;
			Index = indexTerm;
			Term = termSrc;
			Start = start;
			End = end;
		}
		readonly public VariableTerm VariableDest;
		readonly public IOperandTerm Index;
		readonly public IOperandTerm Term;
		readonly public IOperandTerm Start;
		readonly public IOperandTerm End;
	}

	internal sealed class SpButtonArgument : Argument
	{
		public SpButtonArgument(IOperandTerm p1, IOperandTerm p2)
		{
			PrintStrTerm = p1;
			ButtonWord = p2;
		}
		readonly public IOperandTerm PrintStrTerm;
		readonly public IOperandTerm ButtonWord;
	}


	internal sealed class SpColorArgument : Argument
	{
		public SpColorArgument(IOperandTerm r, IOperandTerm g, IOperandTerm b)
		{
			R = r;
			G = g;
			B = b;
		}
		public SpColorArgument(IOperandTerm rgb)
		{
			RGB = rgb;
		}
		readonly public IOperandTerm R;
		readonly public IOperandTerm G;
		readonly public IOperandTerm B;
		readonly public IOperandTerm RGB;
	}

	internal sealed class SpSplitArgument : Argument
	{
		public SpSplitArgument(IOperandTerm s1, IOperandTerm s2, VariableToken varId, VariableTerm num)
		{
			TargetStr = s1;
			Split = s2;
			Var = varId;
            Num = num;
		}
		readonly public IOperandTerm TargetStr;
		readonly public IOperandTerm Split;
		readonly public VariableToken Var;
        readonly public VariableTerm Num;
	}
	
	internal sealed class SpHtmlSplitArgument : Argument
	{
		public SpHtmlSplitArgument(IOperandTerm s1,VariableToken varId, VariableTerm num)
		{
			TargetStr = s1;
			Var = varId;
            Num = num;
		}
		readonly public IOperandTerm TargetStr;
		readonly public VariableToken Var;
        readonly public VariableTerm Num;
	}

	internal sealed class SpGetIntArgument : Argument
	{
		public SpGetIntArgument(VariableTerm var)
		{
			VarToken = var;
		}
		readonly public VariableTerm VarToken;
	}

	internal sealed class SpArrayControlArgument : Argument
	{
		public SpArrayControlArgument(VariableTerm var, IOperandTerm num1, IOperandTerm num2)
		{
			VarToken = var;
			Num1 = num1;
			Num2 = num2;
		}
		readonly public VariableTerm VarToken;
		readonly public IOperandTerm Num1;
		readonly public IOperandTerm Num2;
	}

	internal sealed class SpArrayShiftArgument : Argument
	{
		public SpArrayShiftArgument(VariableTerm var, IOperandTerm num1, IOperandTerm num2, IOperandTerm num3, IOperandTerm num4)
		{
			VarToken = var;
			Num1 = num1;
			Num2 = num2;
			Num3 = num3;
			Num4 = num4;
		}
		readonly public VariableTerm VarToken;
		readonly public IOperandTerm Num1;
		readonly public IOperandTerm Num2;
		readonly public IOperandTerm Num3;
		readonly public IOperandTerm Num4;
	}

    internal sealed class SpArraySortArgument : Argument
    {
        public SpArraySortArgument(VariableTerm var, SortOrder order, IOperandTerm num1, IOperandTerm num2)
        {
            VarToken = var;
            Order = order;
            Num1 = num1;
            Num2 = num2;
        }
        readonly public VariableTerm VarToken;
        readonly public SortOrder Order;
        readonly public IOperandTerm Num1;
        readonly public IOperandTerm Num2;
    }

    internal sealed class SpCopyArrayArgument : Argument
    {
        public SpCopyArrayArgument(IOperandTerm str1, IOperandTerm str2)
        {
            VarName1 = str1;
            VarName2 = str2;
        }
        readonly public IOperandTerm VarName1;
        readonly public IOperandTerm VarName2;
    }

	internal sealed class SpSaveVarArgument : Argument
	{
		public SpSaveVarArgument(IOperandTerm term, IOperandTerm mes, VariableToken[] varTokens)
		{
			Term = term;
			SavMes = mes;
			VarTokens = varTokens;
		}
		readonly public IOperandTerm Term;
		readonly public IOperandTerm SavMes;
		readonly public VariableToken[] VarTokens;
	}

	internal sealed class RefArgument : Argument
	{
		public RefArgument(UserDefinedRefMethod udrm, UserDefinedRefMethod src)
		{
			RefMethodToken = udrm;
			SrcRefMethodToken = src;
		}
		public RefArgument(UserDefinedRefMethod udrm, CalledFunction src)
		{
			RefMethodToken = udrm;
			SrcCalledFunction = src;
		}
		public RefArgument(UserDefinedRefMethod udrm, IOperandTerm src)
		{
			RefMethodToken = udrm;
			SrcTerm = src;
		}
		
		public RefArgument(ReferenceToken vt, VariableToken src)
		{
			RefVarToken = vt;
			SrcVarToken = src;
		}
		public RefArgument(ReferenceToken vt, IOperandTerm src)
		{
			RefVarToken = vt;
			SrcTerm = src;
		}
		readonly public UserDefinedRefMethod RefMethodToken = null;
		readonly public UserDefinedRefMethod SrcRefMethodToken = null;
		readonly public CalledFunction SrcCalledFunction = null;

		readonly public ReferenceToken RefVarToken = null;
		readonly public VariableToken SrcVarToken = null;
		readonly public IOperandTerm SrcTerm = null;
	}

    internal sealed class OneInputArgument : Argument
    {
        public OneInputArgument(IOperandTerm term, IOperandTerm flag)
        {
            Term = term;
            Flag = flag;
        }
        readonly public IOperandTerm Term;
        readonly public IOperandTerm Flag;
    }

    internal sealed class OneInputsArgument : Argument
    {
        public OneInputsArgument(IOperandTerm term, IOperandTerm flag)
        {
            Term = term;
            Flag = flag;
        }
        readonly public IOperandTerm Term;
        readonly public IOperandTerm Flag;
    }
    
	#region set系
	internal sealed class SpSetArgument : Argument
	{
		public SpSetArgument(VariableTerm var, IOperandTerm termSrc)
		{
			VariableDest = var;
			Term = termSrc;
		}
		readonly public VariableTerm VariableDest;
		readonly public IOperandTerm Term;
		public bool AddConst = false;
	}

	internal sealed class SpSetArrayArgument : Argument
	{
		public SpSetArrayArgument(VariableTerm var, IOperandTerm[] termList, Int64[] constList)
		{
			VariableDest = var;
			TermList = termList;
			ConstIntList = constList;
		}
		public SpSetArrayArgument(VariableTerm var, IOperandTerm[] termList, string[] constList)
		{
			VariableDest = var;
			TermList = termList;
			ConstStrList = constList;
		}
		readonly public VariableTerm VariableDest;
		readonly public IOperandTerm[] TermList;
		readonly public Int64[] ConstIntList;
		readonly public string[] ConstStrList;
	}
	#endregion




}
