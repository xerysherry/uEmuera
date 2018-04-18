using System;
using System.Collections.Generic;
using System.Text;
using MinorShift.Emuera.GameProc;
using MinorShift.Emuera.GameData.Variable;

namespace MinorShift.Emuera.GameData.Function
{
	internal sealed class UserDefinedRefMethod
	{
		public CalledFunction CalledFunction { get; private set; }
		public string Name { get; private set; }
		public Type RetType { get; private set; }
		public UserDifinedFunctionDataArgType[] ArgTypeList { get; private set; }

		internal static UserDefinedRefMethod Create(UserDefinedFunctionData funcData)
		{
			UserDefinedRefMethod ret = new UserDefinedRefMethod();
			ret.Name = funcData.Name;
			if (funcData.TypeIsStr)
				ret.RetType = typeof(string);
			else
				ret.RetType = typeof(Int64);
			ret.ArgTypeList = funcData.ArgList;
			return ret;
		}

		/// <summary>
		/// 戻り値と引数の数・型の完全一致が必要
		/// </summary>
		/// <param name="call"></param>
		/// <returns>一致ならtrue</returns>
		internal bool MatchType(CalledFunction call)
		{
			FunctionLabelLine label = call.TopLabel;
			if (label.IsError)
				return false;
			if (RetType != label.MethodType)
				return false;
			if (ArgTypeList.Length != label.Arg.Length)
				return false;
			for (int i = 0; i < ArgTypeList.Length; i++)
			{
				VariableToken vToken = label.Arg[i].Identifier;
				if (vToken.IsReference)
				{
					UserDifinedFunctionDataArgType type = UserDifinedFunctionDataArgType.__Ref;
					type += vToken.Dimension;
					if (vToken.IsInteger)
						type |= UserDifinedFunctionDataArgType.Int;
					else
						type |= UserDifinedFunctionDataArgType.Str;
					if (ArgTypeList[i] != type)
						return false;
				}
				else
				{
					if (vToken.IsInteger && ArgTypeList[i] !=  UserDifinedFunctionDataArgType.Int)
						return false;
					if (vToken.IsString && ArgTypeList[i] != UserDifinedFunctionDataArgType.Str)
						return false;
				}
			}
			return true;
		}

		/// <summary>
		/// 戻り値と引数の数・型の完全一致が必要
		/// </summary>
		/// <param name="rother"></param>
		/// <returns>一致ならtrue</returns>
		internal bool MatchType(UserDefinedRefMethod rother)
		{
			if (RetType != rother.RetType)
				return false;
			if (ArgTypeList.Length != rother.ArgTypeList.Length)
				return false;
			for (int i = 0; i < ArgTypeList.Length; i++)
			{
				if (ArgTypeList[i] != rother.ArgTypeList[i])
					return false;
			}
			return true;
		}

		internal void SetReference(CalledFunction call)
		{
			CalledFunction = call;
		}
	}
}
