using System;
using System.Collections.Generic;
using System.Text;
using MinorShift.Emuera.GameProc;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.Sub;

namespace MinorShift.Emuera.GameData.Function
{
	internal abstract class SuperUserDefinedMethodTerm : IOperandTerm
	{
		protected SuperUserDefinedMethodTerm(Type returnType)
			: base(returnType)
		{
		}
		public abstract UserDefinedFunctionArgument Argument { get;}
		public abstract CalledFunction Call { get; }
		public override long GetIntValue(ExpressionMediator exm)
		{
			SingleTerm term = exm.Process.GetValue(this);
			if (term == null)
				return 0;
			return term.Int;
		}
		public override string GetStrValue(ExpressionMediator exm)
		{
			SingleTerm term = exm.Process.GetValue(this);
			if (term == null)
				return "";
			return term.Str;
		}
		public override SingleTerm GetValue(ExpressionMediator exm)
		{
			SingleTerm term = exm.Process.GetValue(this);
			if (term == null)
			{
				if (GetOperandType() == typeof(Int64))
					return new SingleTerm(0);
				else
					return new SingleTerm("");
			}
			return term;
		}
	}

	internal sealed class UserDefinedMethodTerm : SuperUserDefinedMethodTerm
	{
		
		/// <summary>
		/// エラーならnullを返す。
		/// </summary>
		public static UserDefinedMethodTerm Create(FunctionLabelLine targetLabel, IOperandTerm[] srcArgs, out string errMes)
		{
			CalledFunction call = CalledFunction.CreateCalledFunctionMethod(targetLabel, targetLabel.LabelName);
			UserDefinedFunctionArgument arg = call.ConvertArg(srcArgs, out errMes);
			if (arg == null)
				return null;
			return new UserDefinedMethodTerm(arg, call.TopLabel.MethodType, call);
		}

		private UserDefinedMethodTerm(UserDefinedFunctionArgument arg, Type returnType, CalledFunction call)
			: base(returnType)
		{
			argment = arg;
			called = call;
		}
		public override UserDefinedFunctionArgument Argument { get { return argment; } }
		public override CalledFunction Call { get { return called; } }
		private readonly UserDefinedFunctionArgument argment;
		private readonly CalledFunction called;

		public override IOperandTerm Restructure(ExpressionMediator exm)
		{
			Argument.Restructure(exm);
			return this;
		}


		
	}
	internal sealed class UserDefinedRefMethodTerm : SuperUserDefinedMethodTerm
	{
		public UserDefinedRefMethodTerm(UserDefinedRefMethod reffunc, IOperandTerm[] srcArgs)
			: base(reffunc.RetType)
		{
			this.srcArgs = srcArgs;
			this.reffunc = reffunc;
		}
		IOperandTerm[] srcArgs = null;
		readonly UserDefinedRefMethod reffunc = null;
		public override UserDefinedFunctionArgument Argument
		{
			get
			{
				if (reffunc.CalledFunction == null)
					throw new CodeEE("何も参照していない関数参照" + reffunc.Name + "を呼び出しました");
				string errMes;
				UserDefinedFunctionArgument arg = reffunc.CalledFunction.ConvertArg(srcArgs, out errMes);
				if (arg == null)
					throw new CodeEE(errMes);
				return arg;
			}
		}
		public override CalledFunction Call
		{
			get
			{
				if (reffunc.CalledFunction == null)
					throw new CodeEE("何も参照していない関数参照" + reffunc .Name+ "を呼び出しました");
				return reffunc.CalledFunction;
			}
		}

		public override IOperandTerm Restructure(ExpressionMediator exm)
		{
			for (int i = 0; i < srcArgs.Length; i++)
			{
				if ((reffunc.ArgTypeList[i] & UserDifinedFunctionDataArgType.__Ref) == UserDifinedFunctionDataArgType.__Ref)
					srcArgs[i].Restructure(exm);
				else
					srcArgs[i] = srcArgs[i].Restructure(exm);
			}
			return this;
		}


	}

	internal sealed class UserDefinedRefMethodNoArgTerm : SuperUserDefinedMethodTerm
	{
		public UserDefinedRefMethodNoArgTerm(UserDefinedRefMethod reffunc)
			: base(reffunc.RetType)
		{
			this.reffunc = reffunc;
		}
		readonly UserDefinedRefMethod reffunc = null;
		public override UserDefinedFunctionArgument Argument
		{ get { throw new CodeEE("引数のない関数参照" + reffunc.Name + "を呼び出しました"); } }
		public override CalledFunction Call
		{ get { throw new CodeEE("引数のない関数参照" + reffunc.Name + "を呼び出しました"); } }
		public string GetRefName()
		{
			if (reffunc.CalledFunction == null)
				return "";
			return reffunc.CalledFunction.TopLabel.LabelName;
		}
		public override long GetIntValue(ExpressionMediator exm)
		{ throw new CodeEE("引数のない関数参照" + reffunc.Name + "を呼び出しました"); }
		public override string GetStrValue(ExpressionMediator exm)
		{ throw new CodeEE("引数のない関数参照" + reffunc.Name + "を呼び出しました"); }
		public override SingleTerm GetValue(ExpressionMediator exm)
		{ throw new CodeEE("引数のない関数参照" + reffunc.Name + "を呼び出しました"); }
		public override IOperandTerm Restructure(ExpressionMediator exm)
		{
			return this;
		}
	}
}
