using System;
using System.Collections.Generic;
using System.Text;
//using System.Windows.Forms;
using MinorShift.Emuera.Sub;
using MinorShift.Emuera.GameData.Function;
using MinorShift.Emuera.GameData.Variable;

namespace MinorShift.Emuera.GameData.Expression
{
	/// <summary>
	/// 引数のチェック、戻り値の型チェック等は全て呼び出し元が責任を負うこと。
	/// </summary>
	internal abstract class OperatorMethod : FunctionMethod
	{
		public OperatorMethod()
		{
			argumentTypeArray = null;
		}
		public override string CheckArgumentType(string name, IOperandTerm[] arguments) { throw new ExeEE("型チェックは呼び出し元が行うこと"); }
	}

	internal static class OperatorMethodManager
	{
		readonly static Dictionary<OperatorCode, OperatorMethod> unaryDic = new Dictionary<OperatorCode, OperatorMethod>();
		readonly static Dictionary<OperatorCode, OperatorMethod> unaryAfterDic = new Dictionary<OperatorCode, OperatorMethod>();
		readonly static Dictionary<OperatorCode, OperatorMethod> binaryIntIntDic = new Dictionary<OperatorCode, OperatorMethod>();
		readonly static Dictionary<OperatorCode, OperatorMethod> binaryStrStrDic = new Dictionary<OperatorCode, OperatorMethod>();
		readonly static OperatorMethod binaryMultIntStr = null;
		readonly static OperatorMethod ternaryIntIntInt = null;
		readonly static OperatorMethod ternaryIntStrStr = null;

		static OperatorMethodManager()
		{
			unaryDic[OperatorCode.Plus] = new PlusInt();
			unaryDic[OperatorCode.Minus] = new MinusInt();
			unaryDic[OperatorCode.Not] = new NotInt();
			unaryDic[OperatorCode.BitNot] = new BitNotInt();
			unaryDic[OperatorCode.Increment] = new IncrementInt();
			unaryDic[OperatorCode.Decrement] = new DecrementInt();

			unaryAfterDic[OperatorCode.Increment] = new IncrementAfterInt();
			unaryAfterDic[OperatorCode.Decrement] = new DecrementAfterInt();

			binaryIntIntDic[OperatorCode.Plus] = new PlusIntInt();
			binaryIntIntDic[OperatorCode.Minus] = new MinusIntInt();
			binaryIntIntDic[OperatorCode.Mult] = new MultIntInt();
			binaryIntIntDic[OperatorCode.Div] = new DivIntInt();
			binaryIntIntDic[OperatorCode.Mod] = new ModIntInt();
			binaryIntIntDic[OperatorCode.Equal] = new EqualIntInt();
			binaryIntIntDic[OperatorCode.Greater] = new GreaterIntInt();
			binaryIntIntDic[OperatorCode.Less] = new LessIntInt();
			binaryIntIntDic[OperatorCode.GreaterEqual] = new GreaterEqualIntInt();
			binaryIntIntDic[OperatorCode.LessEqual] = new LessEqualIntInt();
			binaryIntIntDic[OperatorCode.NotEqual] = new NotEqualIntInt();
			binaryIntIntDic[OperatorCode.And] = new AndIntInt();
			binaryIntIntDic[OperatorCode.Or] = new OrIntInt();
			binaryIntIntDic[OperatorCode.Xor] = new XorIntInt();
			binaryIntIntDic[OperatorCode.Nand] = new NandIntInt();
			binaryIntIntDic[OperatorCode.Nor] = new NorIntInt();
			binaryIntIntDic[OperatorCode.BitAnd] = new BitAndIntInt();
			binaryIntIntDic[OperatorCode.BitOr] = new BitOrIntInt();
			binaryIntIntDic[OperatorCode.BitXor] = new BitXorIntInt();
			binaryIntIntDic[OperatorCode.RightShift] = new RightShiftIntInt();
			binaryIntIntDic[OperatorCode.LeftShift] = new LeftShiftIntInt();

			binaryStrStrDic[OperatorCode.Plus] = new PlusStrStr();
			binaryStrStrDic[OperatorCode.Equal] = new EqualStrStr();
			binaryStrStrDic[OperatorCode.Greater] = new GreaterStrStr();
			binaryStrStrDic[OperatorCode.Less] = new LessStrStr();
			binaryStrStrDic[OperatorCode.GreaterEqual] = new GreaterEqualStrStr();
			binaryStrStrDic[OperatorCode.LessEqual] = new LessEqualStrStr();
			binaryStrStrDic[OperatorCode.NotEqual] = new NotEqualStrStr();

			binaryMultIntStr = new MultStrInt();
			ternaryIntIntInt = new TernaryIntIntInt();
			ternaryIntStrStr = new TernaryIntStrStr();
		}
		
		
		
		public static IOperandTerm ReduceUnaryTerm(OperatorCode op, IOperandTerm o1)
		{
            OperatorMethod method = null;
			if (op == OperatorCode.Increment || op == OperatorCode.Decrement)
			{
				VariableTerm var = o1 as VariableTerm;
				if (var == null)
					throw new CodeEE("変数以外をインクリメントすることはできません");
				if (var.Identifier.IsConst)
					throw new CodeEE("変更できない変数をインクリメントすることはできません");
			}
			if (o1.GetOperandType() == typeof(Int64))
			{
				if (op == OperatorCode.Plus)
					return o1;
                OperatorMethod operator_method = null;
				if (unaryDic.TryGetValue(op, out operator_method))
					method = operator_method;
			}
			if(method != null)
				return new FunctionMethodTerm(method, new IOperandTerm[] { o1 });
            string errMes = "";
            if (o1.GetOperandType() == typeof(Int64))
                errMes += "数値型";
            else if (o1.GetOperandType() == typeof(string))
                errMes += "文字列型";
            else
                errMes += "不定型";
            errMes += "に単項演算子\'" + OperatorManager.ToOperatorString(op) + "\'は適用できません";
            throw new CodeEE(errMes);
		}
		
		public static IOperandTerm ReduceUnaryAfterTerm(OperatorCode op, IOperandTerm o1)
		{
            OperatorMethod method = null;
			if (op == OperatorCode.Increment || op == OperatorCode.Decrement)
			{
				VariableTerm var = o1 as VariableTerm;
				if (var == null)
					throw new CodeEE("変数以外をインクリメントすることはできません");
				if (var.Identifier.IsConst)
					throw new CodeEE("変更できない変数をインクリメントすることはできません");
			}
			if (o1.GetOperandType() == typeof(Int64))
			{
                OperatorMethod operator_method = null;
                if (unaryAfterDic.TryGetValue(op, out operator_method))
					method = operator_method;
			}
			if (method != null)
				return new FunctionMethodTerm(method, new IOperandTerm[] { o1 });
            string errMes = "";
            if (o1.GetOperandType() == typeof(Int64))
                errMes += "数値型";
            else if (o1.GetOperandType() == typeof(string))
                errMes += "文字列型";
            else
                errMes += "不定型";
            errMes += "に後置単項演算子\'" + OperatorManager.ToOperatorString(op) + "\'は適用できません";
            throw new CodeEE(errMes);
		}
		
		public static IOperandTerm ReduceBinaryTerm(OperatorCode op, IOperandTerm left, IOperandTerm right)
		{
            OperatorMethod method = null;
			if ((left.GetOperandType() == typeof(Int64)) && (right.GetOperandType() == typeof(Int64)))
			{
                OperatorMethod operator_method = null;
                if (binaryIntIntDic.TryGetValue(op, out operator_method))
					method = operator_method;
			}
			else if ((left.GetOperandType() == typeof(string)) && (right.GetOperandType() == typeof(string)))
			{
                OperatorMethod operator_method = null;
                if (binaryStrStrDic.TryGetValue(op, out operator_method))
					method = operator_method;
			}
			else if (((left.GetOperandType() == typeof(Int64)) && (right.GetOperandType() == typeof(string)))
				 || ((left.GetOperandType() == typeof(string)) && (right.GetOperandType() == typeof(Int64))))
			{
				if (op == OperatorCode.Mult)
					method = binaryMultIntStr;
			}
			if (method != null)
				return new FunctionMethodTerm(method, new IOperandTerm[] { left, right });
			string errMes = "";
                if (left.GetOperandType() == typeof(Int64))
                    errMes += "数値型と";
                else if (left.GetOperandType() == typeof(string))
                    errMes += "文字列型と";
                else
                    errMes += "不定型と";
                if (right.GetOperandType() == typeof(Int64))
                    errMes += "数値型の";
                else if (right.GetOperandType() == typeof(string))
                    errMes += "文字列型の";
                else
                    errMes += "不定型の";
                errMes += "演算に二項演算子\'" + OperatorManager.ToOperatorString(op) + "\'は適用できません";
                throw new CodeEE(errMes);
		}
		
		public static IOperandTerm ReduceTernaryTerm(IOperandTerm o1, IOperandTerm o2, IOperandTerm o3)
		{
            OperatorMethod method = null;
			if ((o1.GetOperandType() == typeof(Int64)) && (o2.GetOperandType() == typeof(Int64)) && (o3.GetOperandType() == typeof(Int64)))
				method = ternaryIntIntInt;
			else if ((o1.GetOperandType() == typeof(Int64)) && (o2.GetOperandType() == typeof(string)) && (o3.GetOperandType() == typeof(string)))
				method = ternaryIntStrStr;
			if (method != null)
				return new FunctionMethodTerm(method, new IOperandTerm[] { o1, o2, o3 });
			throw new CodeEE("三項演算子の使用法が不正です");
			
		}










		#region OperatorMethod SubClasses

		private sealed class PlusIntInt : OperatorMethod
		{
			public PlusIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				return arguments[0].GetIntValue(exm) + arguments[1].GetIntValue(exm);
			}
		}

		private sealed class PlusStrStr : OperatorMethod
		{
			public PlusStrStr()
			{
				CanRestructure = true;
				ReturnType = typeof(string);
				argumentTypeArray = new Type[] { typeof(string), typeof(string) };
			}

			public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				return arguments[0].GetStrValue(exm) + arguments[1].GetStrValue(exm);
			}
		}

		private sealed class MinusIntInt : OperatorMethod
		{
			public MinusIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				return arguments[0].GetIntValue(exm) - arguments[1].GetIntValue(exm);
			}
		}

		private sealed class MultIntInt : OperatorMethod
		{
			public MultIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				return arguments[0].GetIntValue(exm) * arguments[1].GetIntValue(exm);
			}
		}

		private sealed class MultStrInt : OperatorMethod
		{
			public MultStrInt()
			{
				CanRestructure = true;
				ReturnType = typeof(string);
			}
			public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				Int64 value = 0;
				string str = null;
				if (arguments[0].GetOperandType() == typeof(Int64))
				{
					value = arguments[0].GetIntValue(exm);
					str = arguments[1].GetStrValue(exm);
				}
				else
				{
					str = arguments[0].GetStrValue(exm);
					value = arguments[1].GetIntValue(exm);
				}
				if (value < 0)
					throw new CodeEE("文字列に負の値(" + value.ToString() + ")を乗算しようとしました");
				if (value >= 10000)
					throw new CodeEE("文字列に10000以上の値(" + value.ToString() + ")を乗算しようとしました");
				if ((str == "") || (value == 0))
					return "";
                StringBuilder builder = new StringBuilder
                {
                    Capacity = str.Length * (int)value
                };
                for (int i = 0; i < value; i++)
				{
					builder.Append(str);
				}
				return builder.ToString();
			}
		}

		private sealed class DivIntInt : OperatorMethod
		{
			public DivIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
	        {
				Int64 right = arguments[1].GetIntValue(exm);
				if (right == 0)
					throw new CodeEE("0による除算が行なわれました");
				return arguments[0].GetIntValue(exm) / right;
			}
		}

		private sealed class ModIntInt : OperatorMethod
		{
			public ModIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
	        {
				Int64 right = arguments[1].GetIntValue(exm);
				if (right == 0)
					throw new CodeEE("0による除算が行なわれました");
				return arguments[0].GetIntValue(exm) % right;
			}
		}


		private sealed class EqualIntInt : OperatorMethod
		{
			public EqualIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				if (arguments[0].GetIntValue(exm) == arguments[1].GetIntValue(exm))
					return 1L;
				return 0L;
			}

		}

		private sealed class EqualStrStr : OperatorMethod
		{
			public EqualStrStr()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				if (arguments[0].GetStrValue(exm) == arguments[1].GetStrValue(exm))
					return 1L;
				return 0L;
			}
		}

		private sealed class NotEqualIntInt : OperatorMethod
		{
			public NotEqualIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				if (arguments[0].GetIntValue(exm) != arguments[1].GetIntValue(exm))
					return 1L;
				return 0L;
			}
		}

		private sealed class NotEqualStrStr : OperatorMethod
		{
			public NotEqualStrStr()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}
			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				if (arguments[0].GetStrValue(exm) != arguments[1].GetStrValue(exm))
					return 1L;
				return 0L;
			}

		}

		private sealed class GreaterIntInt : OperatorMethod
		{
			public GreaterIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				if (arguments[0].GetIntValue(exm) > arguments[1].GetIntValue(exm))
					return 1L;
				return 0L;
			}
		}

		private sealed class GreaterStrStr : OperatorMethod
		{
			public GreaterStrStr()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}
			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				int c = string.Compare(arguments[0].GetStrValue(exm), arguments[1].GetStrValue(exm), Config.SCExpression);
				if (c > 0)
					return 1L;
				return 0L;
			}
		}
		private sealed class LessIntInt : OperatorMethod
		{
			public LessIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				if (arguments[0].GetIntValue(exm) < arguments[1].GetIntValue(exm))
					return 1L;
				return 0L;
			}
		}
		private sealed class LessStrStr : OperatorMethod
		{
			public LessStrStr()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}
			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				int c = string.Compare(arguments[0].GetStrValue(exm), arguments[1].GetStrValue(exm), Config.SCExpression);
				if (c < 0)
					return 1L;
				return 0L;
			}

		}

		private sealed class GreaterEqualIntInt : OperatorMethod
		{
			public GreaterEqualIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				if (arguments[0].GetIntValue(exm) >= arguments[1].GetIntValue(exm))
					return 1L;
				return 0L;
			}
		}

		private sealed class GreaterEqualStrStr : OperatorMethod
		{
			public GreaterEqualStrStr()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}
			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				int c = string.Compare(arguments[0].GetStrValue(exm), arguments[1].GetStrValue(exm), Config.SCExpression);
				if (c < 0)
					return 1L;
				return 0L;
			}
		}
		private sealed class LessEqualIntInt : OperatorMethod
		{
			public LessEqualIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				if (arguments[0].GetIntValue(exm) <= arguments[1].GetIntValue(exm))
					return 1L;
				return 0L;
			}

		}
		private sealed class LessEqualStrStr : OperatorMethod
		{
			public LessEqualStrStr()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}
			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				int c = string.Compare(arguments[0].GetStrValue(exm), arguments[1].GetStrValue(exm), Config.SCExpression);
				if (c < 0)
					return 1L;
				return 0L;
			}
		}

		private sealed class AndIntInt : OperatorMethod
		{
			public AndIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				if ((arguments[0].GetIntValue(exm) != 0) && (arguments[1].GetIntValue(exm) != 0))
					return 1L;
				return 0L;
			}

		}

		private sealed class OrIntInt : OperatorMethod
		{
			public OrIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				if ((arguments[0].GetIntValue(exm) != 0) || (arguments[1].GetIntValue(exm) != 0))
					return 1L;
				return 0L;
			}
		}

		private sealed class XorIntInt : OperatorMethod
		{
			public XorIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				Int64 i1 = arguments[0].GetIntValue(exm);
				Int64 i2 = arguments[1].GetIntValue(exm);
				if (((i1 == 0) && (i2 != 0)) || ((i1 != 0) && (i2 == 0)))
					return 1L;
				return 0L;
			}

		}

		private sealed class NandIntInt : OperatorMethod
		{
			public NandIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				if ((arguments[0].GetIntValue(exm) == 0) || (arguments[1].GetIntValue(exm) == 0))
					return 1L;
				return 0L;
			}

		}

		private sealed class NorIntInt : OperatorMethod
		{
			public NorIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				if ((arguments[0].GetIntValue(exm) == 0) && (arguments[1].GetIntValue(exm) == 0))
					return 1L;
				return 0L;
			}
		}

		private sealed class BitAndIntInt : OperatorMethod
		{
			public BitAndIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				return arguments[0].GetIntValue(exm) & arguments[1].GetIntValue(exm);
			}
		}

		private sealed class BitOrIntInt : OperatorMethod
		{
			public BitOrIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				return arguments[0].GetIntValue(exm) | arguments[1].GetIntValue(exm);
			}
		}

		private sealed class BitXorIntInt : OperatorMethod
		{
			public BitXorIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				return arguments[0].GetIntValue(exm) ^ arguments[1].GetIntValue(exm);
			}
		}

		private sealed class RightShiftIntInt : OperatorMethod
		{
			public RightShiftIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				return arguments[0].GetIntValue(exm) >> (Int32)(arguments[1].GetIntValue(exm));
			}
		}

		private sealed class LeftShiftIntInt : OperatorMethod
		{
			public LeftShiftIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				return arguments[0].GetIntValue(exm) << (Int32)(arguments[1].GetIntValue(exm));
			}
		}

		private sealed class PlusInt : OperatorMethod
		{
			public PlusInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				return arguments[0].GetIntValue(exm);
			}
		}

		private sealed class MinusInt : OperatorMethod
		{
			public MinusInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				return -arguments[0].GetIntValue(exm);
			}
		}

		private sealed class NotInt : OperatorMethod
		{
			public NotInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				if (arguments[0].GetIntValue(exm) == 0)
					return 1L;
				return 0L;
			}
		}
		private sealed class BitNotInt : OperatorMethod
		{
			public BitNotInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				return ~arguments[0].GetIntValue(exm);
			}
		}

		private sealed class IncrementInt : OperatorMethod
		{
			public IncrementInt()
			{
				CanRestructure = false;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				VariableTerm var = (VariableTerm)arguments[0];
				return var.PlusValue(1L, exm);
			}
		}
		private sealed class DecrementInt : OperatorMethod
		{
			public DecrementInt()
			{
				CanRestructure = false;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				VariableTerm var = (VariableTerm)arguments[0];
				return var.PlusValue(-1L, exm);
			}
		}
		private sealed class IncrementAfterInt : OperatorMethod
		{
			public IncrementAfterInt()
			{
				CanRestructure = false;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				VariableTerm var = (VariableTerm)arguments[0];
				return var.PlusValue(1L, exm) - 1;
			}
		}

		private sealed class DecrementAfterInt : OperatorMethod
		{
			public DecrementAfterInt()
			{
				CanRestructure = false;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				VariableTerm var = (VariableTerm)arguments[0];
				return var.PlusValue(-1L, exm) + 1;
			}
		}


		private sealed class TernaryIntIntInt : OperatorMethod
		{
			public TernaryIntIntInt()
			{
				CanRestructure = true;
				ReturnType = typeof(Int64);
			}

			public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				return (arguments[0].GetIntValue(exm) != 0) ? arguments[1].GetIntValue(exm) : arguments[2].GetIntValue(exm);
			}
		}

		private sealed class TernaryIntStrStr : OperatorMethod
		{
			public TernaryIntStrStr()
			{
				CanRestructure = true;
				ReturnType = typeof(string);
			}

			public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments)
			{
				return (arguments[0].GetIntValue(exm) != 0) ? arguments[1].GetStrValue(exm) : arguments[2].GetStrValue(exm);
			}
		}

		#endregion
	}
}
