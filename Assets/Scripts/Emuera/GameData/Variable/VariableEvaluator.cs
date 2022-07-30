using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using MinorShift.Emuera.Sub;
using MinorShift.Emuera.GameView;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.GameProc;
using MinorShift._Library;
using MinorShift.Emuera.GameProc.Function;
//using System.Windows.Forms;
using uEmuera.Forms;

namespace MinorShift.Emuera.GameData.Variable
{
	internal sealed class VariableEvaluator : IDisposable
	{
		readonly GameBase gamebase;
		readonly ConstantData constant;
		readonly VariableData varData;
		MTRandom rand = new MTRandom();

		public VariableData VariableData { get { return varData; } }
		internal ConstantData Constant { get { return constant; } }

		public VariableEvaluator(GameBase gamebase, ConstantData constant)
		{
			this.gamebase = gamebase;
			this.constant = constant;
			varData = new VariableData(gamebase, constant);
			GlobalStatic.VariableData = varData;
		}
		#region set/get

		public void Randomize(Int64 seed)
		{
			rand = new MTRandom(seed);
		}

		public void InitRanddata()
		{
			rand.SetRand(this.RANDDATA);
		}

		public void DumpRanddata()
		{
			rand.GetRand(this.RANDDATA);
		}
		public Int64 GetNextRand(Int64 max)
		{
			return rand.NextInt64(max);
		}

		public Int64 getPalamLv(Int64 pl, Int64 maxlv)
		{
			for (int i = 0; i < (int)maxlv; i++)
			{
				if (pl < varData.DataIntegerArray[(int)(VariableCode.PALAMLV & VariableCode.__LOWERCASE__)][i + 1])
					return i;
			}
			return maxlv;
		}

		public Int64 getExpLv(Int64 pl, Int64 maxlv)
		{
			for (int i = 0; i < (int)maxlv; i++)
			{
				if (pl < varData.DataIntegerArray[(int)(VariableCode.EXPLV & VariableCode.__LOWERCASE__)][i + 1])
					return i;
			}
			return maxlv;
		}

		public void SetValueAll(FixedVariableTerm p, Int64 srcValue, int start, int end)
		{
            var identifier = p.Identifier;
            //呼び出し元で判定済み
            //if (!p.Identifier.IsInteger)
            //    throw new CodeEE("整数型でない変数" + p.Identifier.Name + "に整数値を代入しようとしました");
            //if (p.Identifier.Readonly)
            //    throw new CodeEE("読み取り専用の変数" + p.Identifier.Name + "に代入しようとしました");
            if (identifier.IsCalc)
				return;
			//一応チェック済み
			//throw new ExeEE("READONLYでないCALC変数の代入処理が設定されていない");

			else
			{
				if (identifier.IsArray1D)
				{
					if (start != 0 || end != identifier.GetLength())
						p.IsArrayRangeValid((Int64)start, (Int64)end, "VARSET", 3L, 4L);
					else if (identifier.IsCharacterData)
						identifier.CheckElement(new Int64[] { p.Index1, p.Index2 });
				}
				else if (identifier.IsCharacterData)
				{
                    identifier.CheckElement(new Int64[] { p.Index1, p.Index2, p.Index3 });
				}
                identifier.SetValueAll(srcValue, start, end, (int)p.Index1);
				return;
			}
		}

		public void SetValueAll(FixedVariableTerm p, string srcValue, int start, int end)
		{
            var identifier = p.Identifier;
            //呼び出し元で判定済み
            //if (!identifier.IsString)
            //    throw new CodeEE("文字列型でない変数" + identifier.Name + "に文字列型を代入しようとしました");
            //if (identifier.Readonly)
            //    throw new CodeEE("読み取り専用の変数" + identifier.Name + "に代入しようとしました");
            if (identifier.IsCalc)
			{
				if (identifier.Code == VariableCode.WINDOW_TITLE)
				{
					GlobalStatic.Console.SetWindowTitle(srcValue);
					return;
				}
				return;
				//一応チェック済み
				//throw new ExeEE("READONLYでないCALC変数の代入処理が設定されていない");
			}
			else
			{
				if (identifier.IsArray1D)
				{
					if (start != 0 || end != identifier.GetLength())
						p.IsArrayRangeValid((Int64)start, (Int64)end, "VARSET", 3L, 4L);
					else if (identifier.IsCharacterData)
						identifier.CheckElement(new Int64[] { p.Index1, p.Index2 });
				}
				else if (identifier.IsCharacterData)
				{
                    identifier.CheckElement(new Int64[] { p.Index1, p.Index2, p.Index3 });
				}
				identifier.SetValueAll(srcValue, start, end, (int)p.Index1);
				return;
			}
		}

		public void SetValueAllEachChara(FixedVariableTerm p, SingleTerm index, Int64 srcValue, int start, int end)
		{
            var identifier = p.Identifier;
            if (!identifier.IsInteger)
				throw new CodeEE("整数型でない変数" + identifier.Name + "に整数値を代入しようとしました");
			if (identifier.IsConst)
				throw new CodeEE("読み取り専用の変数" + identifier.Name + "に代入しようとしました");
			if (identifier.IsCalc)
				return;
			//一応チェック済み
			//throw new ExeEE("READONLYでないCALC変数の代入処理が設定されていない");
			if (varData.CharacterList.Count == 0)
				return;

			CharacterData chara = varData.CharacterList[0];
			Int64 indexNum = -1;

			if (identifier.IsArray1D)
			{
				if (index.GetOperandType() == typeof(Int64))
					indexNum = index.Int;
				else
					indexNum = constant.KeywordToInteger(identifier.Code, index.Str, 1);
                if (indexNum < 0 || indexNum >= ((long[])(identifier.GetArrayChara(0))).Length)
					throw new CodeEE("キャラクタ配列変数" + identifier.Name + "の第２引数(" + indexNum.ToString() + ")は配列の範囲外です");
			}

            long[] arguments = new long[] { -1, indexNum };
            for (int i = start; i < end; i++)
            {
                arguments[0] = i;
                identifier.SetValue(srcValue, arguments);
            }
		}

		public void SetValueAllEachChara(FixedVariableTerm p, SingleTerm index, string srcValue, int start, int end)
		{
            var identifier = p.Identifier;
            if (!identifier.IsString)
				throw new CodeEE("文字列型でない変数" + identifier.Name + "に文字列型を代入しようとしました");
			if (identifier.IsConst)
				throw new CodeEE("読み取り専用の変数" + identifier.Name + "に代入しようとしました");
			if (identifier.IsCalc)
			{
				if (identifier.Code == VariableCode.WINDOW_TITLE)
				{
					GlobalStatic.Console.SetWindowTitle(srcValue);
					return;
				}
				//一応チェック済み
				//throw new ExeEE("READONLYでないCALC変数の代入処理が設定されていない");
				return;
			}
			if (varData.CharacterList.Count == 0)
				return;

			Int64 indexNum = -1;

			if (identifier.IsArray1D)
			{
				if (index.GetOperandType() == typeof(Int64))
					indexNum = index.Int;
				else
					indexNum = constant.KeywordToInteger(identifier.Code, index.Str, 1);
                if (indexNum < 0 || indexNum >= ((string[])(identifier.GetArrayChara(0))).Length)
					throw new CodeEE("キャラクタ配列変数" + identifier.Name + "の第２引数(" + indexNum.ToString() + ")は配列の範囲外です");
			}

            long[] arguments = new long[] { -1, indexNum };
            for (int i = start; i < end; ++i)
			{
                arguments[0] = i;
                identifier.SetValue(srcValue, arguments);
            }
		}

		public Int64 GetArraySum(FixedVariableTerm p, Int64 index1, Int64 index2)
		{
			Int64 sum = 0;
            var identifier = p.Identifier;

            if (identifier.IsCharacterData)
			{
                if (identifier.IsArray1D)
                {
                    long[] arguments = new long[] { p.Index1, -1 };
                    for(int i = (int)index1; i < (int)index2; ++i)
                    {
                        arguments[1] = i;
                        sum += identifier.GetIntValue(GlobalStatic.EMediator, arguments);
                    }
                }
                else
                {
                    long[] arguments = new long[] { p.Index1, p.Index2, -1 };
                    for(int i = (int)index1; i < (int)index2; ++i)
                    {
                        arguments[2] = i;
                        sum += identifier.GetIntValue(GlobalStatic.EMediator, arguments);
                    }
                }
			}
			else
			{
				if (identifier.IsArray1D)
				{
                    long[] arguments = new long[] { -1 };
                    for(int i = (int)index1; i < (int)index2; ++i)
                    {
                        arguments[0] = i;
                        sum += identifier.GetIntValue(GlobalStatic.EMediator, arguments);
                    }
				}
				else if (identifier.IsArray2D)
				{
                    long[] arguments = new long[] { p.Index1, -1 };
                    for(int i = (int)index1; i < (int)index2; ++i)
                    {
                        arguments[1] = i;
                        sum += identifier.GetIntValue(GlobalStatic.EMediator, arguments);
                    }
				}
				else
				{
                    long[] arguments = new long[] { p.Index1, p.Index2, -1 };
                    for(int i = (int)index1; i < (int)index2; ++i)
                    {
                        arguments[2] = i;
                        sum += identifier.GetIntValue(GlobalStatic.EMediator, arguments);
                    }
				}
			}

			return sum;
		}

		public Int64 GetArraySumChara(FixedVariableTerm p, Int64 index1, Int64 index2)
		{
			Int64 sum = 0;
            var identifier = p.Identifier;
            long[] arguments = new long[2] { -1, p.Index2 }; 

            for (int i = (int)index1; i < (int)index2; ++i)
            {
                arguments[0] = i;
                sum += identifier.GetIntValue(GlobalStatic.EMediator, arguments);
            }
            return sum;
		}

        public string GetJoinedStr(FixedVariableTerm p, string delimiter, Int64 index1, Int64 length)
        {
            string sum = "";
            var pIdentifier = p.Identifier;

            if (p.IsString)
            {
                if (pIdentifier.IsArray1D)
                {
                    return string.Join(delimiter, (string[])pIdentifier.GetArray(), (int)index1, (int)length);
                }
                else if (pIdentifier.IsArray2D)
                {
                    var arguments = new long[] { p.Index1, 0 };
                    for(int i = 0; i < (int)length; i++)
                    {
                        arguments[1] = index1 + i;
                        sum += pIdentifier.GetStrValue(GlobalStatic.EMediator, arguments) + ((i < ((int)length - 1)) ? delimiter : "");
                    }
                }
                else
                {
                    var arguments = new long[] { p.Index1, p.Index2, 0 };
                    for(int i = 0; i < (int)length; i++)
                    {
                        arguments[2] = index1 + i;
                        sum += pIdentifier.GetStrValue(GlobalStatic.EMediator, arguments) + ((i < ((int)length - 1)) ? delimiter : "");
                    }
                }
            }
            else
            {
                if (pIdentifier.IsArray1D)
                {
                    var arguments = new long[] { 0 };
                    for(int i = 0; i < (int)length; i++)
                    {
                        arguments[0] = index1 + i;
                        sum += (pIdentifier.GetIntValue(GlobalStatic.EMediator, arguments)).ToString() + ((i < ((int)length - 1)) ? delimiter : "");
                    }
                }
                else if (pIdentifier.IsArray2D)
                {
                    var arguments = new long[] { p.Index1, 0 };
                    for(int i = 0; i < (int)length; i++)
                    {
                        arguments[1] = index1 + i;
                        sum += (pIdentifier.GetIntValue(GlobalStatic.EMediator, arguments)).ToString() + ((i < ((int)length - 1)) ? delimiter : "");
                    }
                }
                else
                {
                    var arguments = new long[] { p.Index1, p.Index2, 0 };
                    for(int i = 0; i < (int)length; i++)
                    {
                        arguments[2] = index1 + i;
                        sum += (pIdentifier.GetIntValue(GlobalStatic.EMediator, arguments)).ToString() + ((i < ((int)length - 1)) ? delimiter : "");
                    }
                }
            }
            return sum;
        }

        public Int64 GetMatch(FixedVariableTerm p, Int64 target, Int64 start, Int64 end)
        {
            Int64 ret = 0;
            var identifier = p.Identifier;
            long[] arguments = null;
            int idx = 0;
            if(identifier.IsCharacterData)
            {
                arguments = new long[] { p.Index1, -1 };
                idx = 1;
            }
            else
                arguments = new long[] { -1 };

            for(int i = (int)start; i < (int)end; ++i)
            {
                arguments[idx] = i;
                if(identifier.GetIntValue(GlobalStatic.EMediator, arguments) == target)
                    ++ret;
            }
			return ret;
		}

		public Int64 GetMatch(FixedVariableTerm p, string target, Int64 start, Int64 end)
		{
            Int64 ret = 0;
            var identifier = p.Identifier;
            bool targetIsNullOrEmpty = string.IsNullOrEmpty(target);
            long[] arguments = null;
            int idx = 0;
            if(identifier.IsCharacterData)
            {
                arguments = new long[] { p.Index1, -1 };
                idx = 1;
            }
            else
                arguments = new long[] { -1 };

            for(int i = (int)start; i < (int)end; ++i)
            {
                arguments[idx] = i;
                if((identifier.GetStrValue(GlobalStatic.EMediator, arguments) == target) || 
                    (targetIsNullOrEmpty && string.IsNullOrEmpty(identifier.GetStrValue(GlobalStatic.EMediator, arguments))))
                    ++ret;
            }

			return ret;
		}

        public Int64 GetMatchChara(FixedVariableTerm p, Int64 target, Int64 start, Int64 end)
        {
            Int64 ret = 0;
            var identifier = p.Identifier;
            long[] arguments = new long[3] { -1, p.Index2, p.Index3 };

            for (int i = (int)start; i < (int)end; ++i)
            {
                arguments[0] = i;
                if (identifier.GetIntValue(GlobalStatic.EMediator, arguments) == target)
                    ret++;
            }

            return ret;
        }

		public Int64 GetMatchChara(FixedVariableTerm p, string target, Int64 start, Int64 end)
		{
			Int64 ret = 0;
            var identifier = p.Identifier;
            bool targetIsNullOrEmpty = string.IsNullOrEmpty(target);
            long[] arguments = new long[3] { -1, p.Index2, p.Index3 };

            for (int i = (int)start; i < (int)end; ++i)
            {
                arguments[0] = i;
                if ((identifier.GetStrValue(GlobalStatic.EMediator, arguments) == target) || 
                    (targetIsNullOrEmpty && string.IsNullOrEmpty(identifier.GetStrValue(GlobalStatic.EMediator, arguments))))
                    ret++;
            }

			return ret;
		}

		public Int64 FindElement(FixedVariableTerm p, Int64 target, Int64 start, Int64 end, bool isExact, bool isLast)
		{
			Int64[] array;
            var identifier = p.Identifier;

            //指定値の配列要素の範囲外かのチェックは済んでるので、これだけでよい
            if (start >= end)
				return -1;

			if (identifier.IsCharacterData)
                array = (long[])identifier.GetArrayChara((int)p.Index1);
			else
				array = (Int64[])identifier.GetArray();

			if (isLast)
			{
				for (int i = (int)end - 1; i >= (int)start; --i)
				{
					if (target == array[i])
						return (Int64)i;
				}
			}
			else
			{
				for (int i = (int)start; i < (int)end; ++i)
				{
					if (target == array[i])
						return (Int64)i;
				}
			}
			return -1;
		}

		public Int64 FindElement(FixedVariableTerm p, Regex target, Int64 start, Int64 end, bool isExact, bool isLast)
		{
			string[] array;

			//指定値の配列要素の範囲外かのチェックは済んでるので、これだけでよい
			if (start >= end)
				return -1;
            var identifier = p.Identifier;
            if (identifier.IsCharacterData)
                array = (string[])identifier.GetArrayChara((int)p.Index1);
			else
				array = (string[])identifier.GetArray();

			if (isLast)
			{
				for (int i = (int)end - 1; i >= (int)start; --i)
				{
					//1823 Nullなら空文字列として扱う
					string str = array[i] ?? "";
					if (isExact)
					{
						Match match = target.Match(str);
						//正規表現に引っかかった文字列の長さ＝元の文字列の長さなら完全一致
						if (match.Success && str.Length == match.Length)
							return (Int64)i;
					}
					else
					{
						//部分一致なのでひっかかればOK
						if (target.IsMatch(str))
							return (Int64)i;
					}
				}
			}
			else
			{
				for (int i = (int)start; i < (int)end; ++i)
				{
					//1823 Nullなら空文字列として扱う
					string str = array[i] ?? "";
					if (isExact)
					{
						//正規表現に引っかかった文字列の長さ＝元の文字列の長さなら完全一致
						Match match = target.Match(str);
						if (match.Success && str.Length == match.Length)
							return (Int64)i;
					}
					else
					{
						//部分一致なのでひっかかればOK
						if (target.IsMatch(str))
							return (Int64)i;
					}
				}
			}
			return -1;
		}

		public Int64 GetMaxArray(FixedVariableTerm p, Int64 start, Int64 end, bool isMax)
		{
            Int64 value;
            var identifier = p.Identifier;
            int idx = 0;
            long[] arguments = new long[2] { -1, -1 };
            if(identifier.IsCharacterData)
            {
                arguments = new long[] { p.Index1, start };
                idx = 1;
            }
            else
                arguments = new long[] { start };

            Int64 ret = p.Identifier.GetIntValue(GlobalStatic.EMediator, arguments);
            if(isMax)
            {
                for(int i = (int)start + 1; i < (int)end; ++i)
                {
                    arguments[idx] = i;
                    value = identifier.GetIntValue(GlobalStatic.EMediator, arguments);
                    if(value > ret)
                        ret = value;
                }
            }
            else
            { 
                for(int i = (int)start + 1; i < (int)end; ++i)
                {
                    arguments[idx] = i;
                    value = identifier.GetIntValue(GlobalStatic.EMediator, arguments);
                    if(value < ret)
                        ret = value;
                }
            }
			return ret;
		}

        public Int64 GetMaxArrayChara(FixedVariableTerm p, Int64 start, Int64 end, bool isMax)
        {
            Int64 value;
            var identifier = p.Identifier;
            long[] arguments = new long[3] { start, p.Index2, p.Index3 };

            Int64 ret = identifier.GetIntValue(GlobalStatic.EMediator, arguments);
            if(isMax)
            {
                for(int i = (int)start + 1; i < (int)end; ++i)
                {
                    arguments[0] = i;
                    value = identifier.GetIntValue(GlobalStatic.EMediator, arguments);
                    if(value > ret)
                        ret = value;
                }
            }
            else
            {
                for(int i = (int)start + 1; i < (int)end; ++i)
                {
                    arguments[0] = i;
                    value = identifier.GetIntValue(GlobalStatic.EMediator, arguments);
                    if(value < ret)
                        ret = value;
                }
            }

            return ret;
        }

		public Int64 GetInRangeArray(FixedVariableTerm p, Int64 min, Int64 max, Int64 start, Int64 end)
		{
            Int64 value;
			Int64 ret = 0;
            var identifier = p.Identifier;
            long[] arguments = null;
            int idx = 0;
            if(identifier.IsCharacterData)
            {
                arguments = new long[] { p.Index1, -1 };
                idx = 1;
            }
            else
                arguments = new long[] { -1 };

            for (int i = (int)start; i < (int)end; ++i)
            {
                arguments[idx] = i;
                value = identifier.GetIntValue(GlobalStatic.EMediator, arguments);
                if (value >= min && value < max)
                    ret++;
            }
			return ret;
		}

		public Int64 GetInRangeArrayChara(FixedVariableTerm p, Int64 min, Int64 max, Int64 start, Int64 end)
		{
			Int64 ret = 0;
            Int64 value;
            var identifier = p.Identifier;
            long[] arguments = new long[3] { -1, p.Index2, p.Index3 };
            for (int i = (int)start; i < (int)end; i++)
            {
                arguments[0] = i;
                value = identifier.GetIntValue(GlobalStatic.EMediator, arguments);
                if (value >= min && value < max)
                    ret++;
            }
            
			return ret;
		}

		public void ShiftArray(FixedVariableTerm p, int shift, Int64 def, int start, int num)
		{
			Int64[] array;
            var identifier = p.Identifier;
            if (identifier.IsCharacterData)
                array = (long[])identifier.GetArrayChara((int)p.Index1);
			else
				array = (Int64[])identifier.GetArray();

			if (start >= array.Length)
				throw new CodeEE("命令ARRAYSHIFTの第４引数(" + start.ToString() + ")が配列" + p.Identifier.Name + "の範囲を超えています");

			if (num == -1)
				num = array.Length - start;
			if ((start + num) > array.Length)
				num = array.Length - start;

			if (Math.Abs(shift) >= array.Length && start == 0 && num >= array.Length)
			{
				for (int i = 0; i < array.Length; i++)
					array[i] = def;
				return;
			}

			int sourceStart = 0;
			int destStart = start + shift;
			int length = num - Math.Abs(shift);
			if (shift < 0)
			{
				sourceStart = -shift;
				destStart = start;
			}
			Int64[] temp = new Int64[num];
			Buffer.BlockCopy(array, start * 8, temp, 0, 8 * num);

			//これを満たすのはshift > 0であることは自明
			if (sourceStart == 0)
			{
				if (length > 0)
					for (int i = start; i < (start + shift); i++)
						array[i] = def;
				else
				{
					for (int i = start; i < (start + num); i++)
						array[i] = def;
					return;
				}
			}
			else
			{
				if (length > 0)
					for (int i = (start + length); i < (start + num); i++)
						array[i] = def;
				else
				{
					for (int i = start; i < (start + num); i++)
						array[i] = def;
					return;
				}
			}

			//if (start > 0)
			//    //Array.Copy(temp, 0, array, 0, start);
			//    Buffer.BlockCopy(temp, 0, array, 0, 8 * start);

			if (length > 0)
				//Array.Copy(temp, sourceStart, array, destStart, length);
				Buffer.BlockCopy(temp, sourceStart * 8, array, destStart * 8, length * 8);

			//if ((start + num) < array.Length)
			//    //Array.Copy(temp, (start + num), array, (start + num), array.Length - (start + num));
			//    Buffer.BlockCopy(temp, (start + num) * 8, array, (start + num) * 8, (array.Length - (start + num)) * 8);
		}

		public void ShiftArray(FixedVariableTerm p, int shift, string def, int start, int num)
		{
			string[] arrays;
            var identifier = p.Identifier;
            if (identifier.IsCharacterData)
                arrays = (string[])identifier.GetArrayChara((int)p.Index1);
			else
				arrays = (string[])identifier.GetArray();

			if (start >= arrays.Length)
				throw new CodeEE("命令ARRAYSHIFTの第４引数(" + start.ToString() + ")が配列" + p.Identifier.Name + "の範囲を超えています");

			//for (int i = 0; i < arrays.Length; i++)
			//    arrays[i] = "";
			//Array.Clear(arrays, 0, arrays.Length);

			if (num == -1)
				num = arrays.Length - start;
			if ((start + num) > arrays.Length)
				num = arrays.Length - start;

			if (Math.Abs(shift) >= arrays.Length && start == 0 && num >= arrays.Length)
			{
				for (int i = 0; i < arrays.Length; i++)
					arrays[i] = def;
				return;
			}

			//if (start > 0)
			//    Array.Copy(temps, 0, arrays, 0, start);

			int sourceStart = 0;
			int destStart = start + shift;
			int length = num - Math.Abs(shift);
			if (shift < 0)
			{
				sourceStart = -shift;
				destStart = start;
			}
			string[] temps = new string[num];
			Array.Copy(arrays, start, temps, 0, num);

			if (destStart > start)
			{
				if (length > 0)
					for (int i = start; i < (start + shift); i++)
						arrays[i] = def;
				else
				{
					for (int i = start; i < (start + num); i++)
						arrays[i] = def;
					return;
				}
			}
			else
			{
				if (length > 0)
					for (int i = (start + length); i < (start + num); i++)
						arrays[i] = def;
				else
				{
					for (int i = start; i < (start + num); i++)
						arrays[i] = def;
					return;
				}
			}

			if (length > 0)
				Array.Copy(temps, sourceStart, arrays, destStart, length);
			//if ((start + num) < arrays.Length)
			//    Array.Copy(temps, (start + num), arrays, (start + num), arrays.Length - (start + num));
		}

		public void RemoveArray(FixedVariableTerm p, int start, int num)
		{
            var identifier = p.Identifier;
            if (identifier.IsInteger)
			{
				Int64[] array;
				if (identifier.IsCharacterData)
                    array = (long[])identifier.GetArrayChara((int)p.Index1);
				else
					array = (Int64[])identifier.GetArray();

                if (start >= array.Length)
					throw new CodeEE("命令ARRAYREMOVEの第２引数(" + start.ToString() + ")が配列" + p.Identifier.Name + "の範囲を超えています");
				if (num <= 0)
					num = array.Length;
				Int64[] temp = new Int64[array.Length];
				//array.CopyTo(temp, 0);
				//for (int i = 0; i < array.Length; i++)
				//    array[i] = 0;
				//Array.Clear(array, 0, array.Length);
				if (start > 0)
					//Array.Copy(array, 0, temp, 0, start);
					Buffer.BlockCopy(array, 0, temp, 0, start * 8);
				if ((start + num) < array.Length)
					//Array.Copy(array, (start + num), temp, start, (array.Length - (start + num)));
					Buffer.BlockCopy(array, (start + num) * 8, temp, start * 8, (array.Length - (start + num)) * 8);
				//temp.CopyTo(array, 0);
				Buffer.BlockCopy(temp, 0, array, 0, temp.Length * 8);
			}
			else
			{
				string[] arrays;
				if (identifier.IsCharacterData)
                    arrays = (string[])identifier.GetArrayChara((int)p.Index1);
				else
					arrays = (string[])identifier.GetArray();

                if (num <= 0)
					num = arrays.Length;
				string[] temps = new string[arrays.Length];
				//arrays.CopyTo(temps, 0);
				//for (int i = 0; i < arrays.Length; i++)
				//    arrays[i] = "";
				if (start > 0)
					Array.Copy(arrays, 0, temps, 0, start);
				if ((start + num) < arrays.Length)
					Array.Copy(arrays, (start + num), temps, start, (arrays.Length - (start + num)));
				temps.CopyTo(arrays, 0);
			}
		}

		public void SortArray(FixedVariableTerm p, SortOrder order, int start, int num)
		{
			if (order == SortOrder.UNDEF)
				order = SortOrder.ASCENDING;
            var identifier = p.Identifier;
            if (identifier.IsInteger)
			{
				Int64[] array;
				if (identifier.IsCharacterData)
                    array = (long[])identifier.GetArrayChara((int)p.Index1);
				else
					array = (Int64[])identifier.GetArray();

                if (start >= array.Length)
					throw new CodeEE("命令ARRAYSORTの第３引数(" + start.ToString() + ")が配列" + identifier.Name + "の範囲を超えています");
				if (num <= 0)
					num = array.Length - start;
				Int64[] temp = new Int64[num];
				Array.Copy(array, start, temp, 0, num);

				if (order == SortOrder.ASCENDING)
					Array.Sort(temp);
				else if (order == SortOrder.DESENDING)
					Array.Sort(temp, delegate(Int64 a, Int64 b) { return b.CompareTo(a); });
				Array.Copy(temp, 0, array, start, num);
			}
			else
			{
				string[] array;
				if (identifier.IsCharacterData)
                    array = (string[])identifier.GetArrayChara((int)p.Index1);
                else
					array = (string[])identifier.GetArray();

                if (start >= array.Length)
					throw new CodeEE("命令ARRAYSORTの第３引数(" + start.ToString() + ")が配列" + identifier.Name + "の範囲を超えています");
				if (num <= 0)
					num = array.Length - start;
				string[] temp = new string[num];
				Array.Copy(array, start, temp, 0, num);

				if (order == SortOrder.ASCENDING)
					Array.Sort(temp);
				else if (order == SortOrder.DESENDING)
					Array.Sort(temp, delegate(string a, string b) { return b.CompareTo(a); });
				Array.Copy(temp, 0, array, start, num);
			}
		}



		public void CopyArray(VariableToken var1, VariableToken var2)
		{
			if (var1.IsInteger)
			{
				if (var1.IsArray1D)
				{
					Int64[] array1 = (Int64[])var1.GetArray();
					Int64[] array2 = (Int64[])var2.GetArray();
					int length = (array1.Length >= array2.Length) ? array2.Length : array1.Length;
					for (int i = 0; i < length; i++)
						array2[i] = array1[i];
				}
				else if (var1.IsArray2D)
				{
					Int64[,] array1 = (Int64[,])var1.GetArray();
					Int64[,] array2 = (Int64[,])var2.GetArray();
					int length1 = (array1.GetLength(0) >= array2.GetLength(0)) ? array2.GetLength(0) : array1.GetLength(0);
					int length2 = (array1.GetLength(1) >= array2.GetLength(1)) ? array2.GetLength(1) : array1.GetLength(1);
					for (int i = 0; i < length1; i++)
					{
						for (int j = 0; j < length2; j++)
							array2[i, j] = array1[i, j];
					}
				}
				else
				{
					Int64[, ,] array1 = (Int64[, ,])var1.GetArray();
					Int64[, ,] array2 = (Int64[, ,])var2.GetArray();
					int length1 = (array1.GetLength(0) >= array2.GetLength(0)) ? array2.GetLength(0) : array1.GetLength(0);
					int length2 = (array1.GetLength(1) >= array2.GetLength(1)) ? array2.GetLength(1) : array1.GetLength(1);
					int length3 = (array1.GetLength(2) >= array2.GetLength(2)) ? array2.GetLength(2) : array1.GetLength(2);
					for (int i = 0; i < length1; i++)
					{
						for (int j = 0; j < length2; j++)
						{
							for (int k = 0; k < length3; k++)
								array2[i, j, k] = array1[i, j, k];
						}
					}
				}
			}
			else
			{
				if (var1.IsArray1D)
				{
					string[] array1 = (string[])var1.GetArray();
					string[] array2 = (string[])var2.GetArray();
					int length = (array1.Length >= array2.Length) ? array2.Length : array1.Length;
					for (int i = 0; i < length; i++)
						array2[i] = array1[i];
				}
				else if (var1.IsArray2D)
				{
					string[,] array1 = (string[,])var1.GetArray();
					string[,] array2 = (string[,])var2.GetArray();
					int length1 = (array1.GetLength(0) >= array2.GetLength(0)) ? array2.GetLength(0) : array1.GetLength(0);
					int length2 = (array1.GetLength(1) >= array2.GetLength(1)) ? array2.GetLength(1) : array1.GetLength(1);
					for (int i = 0; i < length1; i++)
					{
						for (int j = 0; j < length2; j++)
							array2[i, j] = array1[i, j];
					}
				}
				else
				{
					string[, ,] array1 = (string[, ,])var1.GetArray();
					string[, ,] array2 = (string[, ,])var2.GetArray();
					int length1 = (array1.GetLength(0) >= array2.GetLength(0)) ? array2.GetLength(0) : array1.GetLength(0);
					int length2 = (array1.GetLength(1) >= array2.GetLength(1)) ? array2.GetLength(1) : array1.GetLength(1);
					int length3 = (array1.GetLength(2) >= array2.GetLength(2)) ? array2.GetLength(2) : array1.GetLength(2);
					for (int i = 0; i < length1; i++)
					{
						for (int j = 0; j < length2; j++)
						{
							for (int k = 0; k < length3; k++)
								array2[i, j, k] = array1[i, j, k];
						}
					}
				}
			}
		}


		public string GetHavingItemsString()
		{
			Int64[] array = this.ITEM;
			string[] itemnames = this.ITEMNAME;
			int length = Math.Min(array.Length, itemnames.Length);
			int count = 0;
			StringBuilder builder = new StringBuilder(100);
			builder.Append("所持アイテム：");
			for (int i = 0; i < length; i++)
			{
				if (array[i] == 0)
					continue;
				count++;
				if (itemnames[i] != null)
					builder.Append(itemnames[i]);
				builder.Append("(");
				builder.Append(array[i].ToString());
				builder.Append(") ");
			}
			if (count == 0)
				builder.Append("なし");
			return builder.ToString();
		}

		//public string GetItemSalesString()
		//{
		//	Int64[] itemsales = varData.DataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.ITEMSALES];
		//	string[] itemname = constant.GetCsvNameList(VariableCode.ITEMNAME);
		//	StringBuilder builder = new StringBuilder(100);
		//	for (int i = 0; i < itemsales.Length; i++)
		//	{
		//		if (itemsales[i] != 0)
		//			continue;
		//		builder.Append(itemname[i]);
		//		builder.Append("(");
		//		builder.Append(itemsales[i].ToString());
		//		builder.Append(")");
		//	}
		//	return builder.ToString();
		//}

		public string GetCharacterDataString(Int64 target, FunctionCode func)
		{
			StringBuilder builder = new StringBuilder(100);
			if ((target < 0) || (target >= varData.CharacterList.Count))
				throw new CodeEE("存在しない登録キャラクタを参照しようとしました");
			CharacterData chara = varData.CharacterList[(int)target];
			Int64[] array = null;
			string[] arrayName = null;
			int i = 0;
			switch (func)
			{
				case FunctionCode.PRINT_ABL:
					array = chara.DataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.ABL];
					arrayName = constant.GetCsvNameList(VariableCode.ABLNAME);
					for (i = 0; i < array.Length; i++)
					{
						if (i >= arrayName.Length)
							break;
						if (array[i] == 0)
							continue;
						if (string.IsNullOrEmpty(arrayName[i]))
							continue;
						builder.Append(arrayName[i]);
						builder.Append("LV");
						builder.Append(array[i].ToString());
						builder.Append(" ");

					}
					break;
				case FunctionCode.PRINT_TALENT:
					array = chara.DataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.TALENT];
					arrayName = constant.GetCsvNameList(VariableCode.TALENTNAME);
					for (i = 0; i < array.Length; i++)
					{
						if (i >= arrayName.Length)
							break;
						if (array[i] == 0)
							continue;
						if (string.IsNullOrEmpty(arrayName[i]))
							continue;
						builder.Append("[");
						builder.Append(arrayName[i]);
						builder.Append("]");
					}
					break;
				case FunctionCode.PRINT_MARK:
					array = chara.DataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.MARK];
					arrayName = constant.GetCsvNameList(VariableCode.MARKNAME);
					for (i = 0; i < array.Length; i++)
					{
						if (i >= arrayName.Length)
							break;
						if (array[i] == 0)
							continue;
						if (string.IsNullOrEmpty(arrayName[i]))
							continue;
						builder.Append(arrayName[i]);
						builder.Append("LV");
						builder.Append(array[i].ToString());
						builder.Append(" ");
					}
					break;
				case FunctionCode.PRINT_EXP:
					array = chara.DataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.EXP];
					arrayName = constant.GetCsvNameList(VariableCode.EXPNAME);
					for (i = 0; i < array.Length; i++)
					{
						if (i >= arrayName.Length)
							break;
						if (array[i] == 0)
							continue;
						if (string.IsNullOrEmpty(arrayName[i]))
							continue;
						builder.Append(arrayName[i]);
						builder.Append(array[i].ToString());
						builder.Append(" ");
					}
					break;
				//現状ここに来ることはないはず
				//default:
				//    throw new ExeEE("未定義の関数");
			}
			return builder.ToString();
		}

		public string GetCharacterParamString(Int64 target, int paramCode)
		{
			if ((target < 0) || (target >= varData.CharacterList.Count))
				throw new CodeEE("存在しない登録キャラクタを参照しようとしました");
			//そもそも呼び出し元がint i = 0; i < 100; i++)でこの条件が満たされる可能性0
			//if ((paramCode < 0) || (paramCode >= constant.ParamName.Length))
			//    throw new ExeEE("存在しない名称を取得しようとした");
			CharacterData chara = varData.CharacterList[(int)target];
			Int64 param = chara.DataIntegerArray[(int)(VariableCode.PALAM & VariableCode.__LOWERCASE__)][paramCode];
			Int64[] paramlv = varData.DataIntegerArray[(int)(VariableCode.PALAMLV & VariableCode.__LOWERCASE__)];
			string paramName = constant.GetCsvNameList(VariableCode.PALAMNAME)[paramCode];
			if ((param == 0) && (string.IsNullOrEmpty(paramName)))
				return null;
			if (paramName == null)
				paramName = "";
			char c = '-';
			Int64 border = paramlv[1];
			if (param >= border)
			{
				c = '=';
				border = paramlv[2];
			}
			if (param >= border)
			{
				c = '>';
				border = paramlv[3];
			}
			if (param >= border)
			{
				c = '*';
				border = paramlv[4];
			}
			StringBuilder bar = new StringBuilder(100);

			bar.Append('[');
			if ((border <= 0) || (border <= param))
				bar.Append(c, 10);
			else if (param <= 0)
				bar.Append('.', 10);
			else
			{
				unchecked
				{
					int count = (int)(param * 10 / border);
					bar.Append(c, count);
					bar.Append('.', 10 - count);
				}
			}
			bar.Append(']');
			return string.Format("{0}{1}{2,6}", paramName, bar.ToString(), param);

		}

		public void AddCharacter(Int64 charaTmplNo)
		{
			CharacterTemplate tmpl = constant.GetCharacterTemplate(charaTmplNo);
			if (tmpl == null)
				throw new CodeEE("定義していないキャラクタを作成しようとしました");
			CharacterData chara = new CharacterData(constant, tmpl, varData);
			varData.CharacterList.Add(chara);
		}

		public void AddCharacter_UseSp(Int64 charaTmplNo, bool isSp)
		{
			CharacterTemplate tmpl = constant.GetCharacterTemplate_UseSp(charaTmplNo, isSp);
			if (tmpl == null)
				throw new CodeEE("定義していないキャラクタを作成しようとしました");
			CharacterData chara = new CharacterData(constant, tmpl, varData);
			varData.CharacterList.Add(chara);
		}

		public void AddCharacterFromCsvNo(Int64 CsvNo)
		{
			CharacterTemplate tmpl = constant.GetCharacterTemplateFromCsvNo(CsvNo);
			if (tmpl == null)
				//throw new CodeEE("定義していないキャラクタを作成しようとしました");
				tmpl = constant.GetPseudoChara();
			CharacterData chara = new CharacterData(constant, tmpl, varData);
			varData.CharacterList.Add(chara);
		}

		public void AddPseudoCharacter()
		{
			CharacterTemplate tmpl = constant.GetPseudoChara();
			CharacterData chara = new CharacterData(constant, tmpl, varData);
			varData.CharacterList.Add(chara);
		}

		public void DelCharacter(Int64 charaNo)
		{
			if ((charaNo < 0) || (charaNo >= varData.CharacterList.Count))
				throw new CodeEE("存在しない登録キャラクタ(" + charaNo.ToString() + ")を削除しようとしました");
			varData.CharacterList[(int)charaNo].Dispose();
			varData.CharacterList.RemoveAt((int)charaNo);
		}

		public void DelCharacter(Int64[] charaNoList)
		{
			List<CharacterData> DelList = new List<CharacterData>();
			foreach(Int64 charaNo in charaNoList)
			{
				if ((charaNo < 0) || (charaNo >= varData.CharacterList.Count))
					throw new CodeEE("存在しない登録キャラクタ(" + charaNoList.ToString() + ")を削除しようとしました");
				CharacterData chara = varData.CharacterList[(int)charaNo];
				if (DelList.Contains(chara))
					throw new CodeEE("同一の登録キャラクタ番号(" + charaNo.ToString() + ")が複数回指定されました");
				DelList.Add(chara);
				chara.Dispose();
			}
			foreach (CharacterData chara in DelList)
				varData.CharacterList.Remove(chara);
		}

		public void DelAllCharacter()
		{
			if (varData.CharacterList.Count == 0)
				return;
			foreach (CharacterData chara in varData.CharacterList)
				chara.Dispose();
			varData.CharacterList.Clear();
		}

		public void PickUpChara(Int64[] NoList)
		{
			List<Int64> pickList = new List<long>();
			Int64 oldTarget = this.TARGET;
			Int64 oldAssi = this.ASSI;
			Int64 oldMaster = this.MASTER;
			this.TARGET = -1;
			this.ASSI = -1;
			this.MASTER = -1;
			//同じキャラが複数出てこないようにリストを整理
			for (int i = 0; i < NoList.Length; i++)
			{
				if (!pickList.Contains(NoList[i]) && NoList[i] >= 0)
					pickList.Add(NoList[i]);
			}
			for (int i = 0; i < pickList.Count; i++)
			{
				if (i != pickList[i])
				{
					SwapChara(pickList[i], (Int64)i);
					if (pickList.IndexOf((Int64)i) > i)
						pickList[pickList.IndexOf((Int64)i)] = pickList[i];
				}
				if (this.TARGET < 0 && pickList[i] == oldTarget)
					this.TARGET = i;
				if (this.ASSI < 0 && pickList[i] == oldAssi)
					this.ASSI = i;
				if (this.MASTER < 0 && pickList[i] == oldMaster)
					this.MASTER = i;
			}
			if (pickList.Count < varData.CharacterList.Count)
			{
				for (int i = (varData.CharacterList.Count - 1); i >= pickList.Count; i--)
					DelCharacter((Int64)i);
			}
		}

		public void ResetData()
		{
			//グローバルは初期化しない方が都合がよい。
			//varData.SetDefaultGlobalValue();
			varData.SetDefaultLocalValue();
			varData.SetDefaultValue(constant);
			foreach (CharacterData chara in varData.CharacterList)
				chara.Dispose();
			varData.CharacterList.Clear();
		}

		public void ResetGlobalData()
		{
			varData.SetDefaultGlobalValue();
		}

		public void CopyChara(Int64 x, Int64 y)
		{
			if ((x < 0) || (x >= varData.CharacterList.Count))
				throw new CodeEE("コピー元のキャラクタが存在しません");
			if ((y < 0) || (y >= varData.CharacterList.Count))
				throw new CodeEE("コピー先のキャラクタが存在しません");
			varData.CharacterList[(int)x].CopyTo(varData.CharacterList[(int)y], varData);
		}

		public void AddCopyChara(Int64 x)
		{
			if ((x < 0) || (x >= varData.CharacterList.Count))
				throw new CodeEE("コピー元のキャラクタが存在しません");
			AddPseudoCharacter();
			varData.CharacterList[(int)x].CopyTo(varData.CharacterList[varData.CharacterList.Count - 1], varData);
		}

		public void SwapChara(Int64 x, Int64 y)
		{
			if (((x < 0) || (x >= varData.CharacterList.Count)) || ((y < 0) || (y >= varData.CharacterList.Count)))
				throw new CodeEE("存在しない登録キャラクタを入れ替えようとしました");
			if (x == y)
				return;
			CharacterData data = varData.CharacterList[(int)y];
			varData.CharacterList[(int)y] = varData.CharacterList[(int)x];
			varData.CharacterList[(int)x] = data;
		}

		public void SortChara(VariableToken sortkey, Int64 elem, SortOrder sortorder, bool fixMaster)
		{
			if (varData.CharacterList.Count <= 1)
				return;
			if (sortorder == SortOrder.UNDEF)
				sortorder = SortOrder.ASCENDING;
			if (sortkey == null)
				sortkey = GlobalStatic.VariableData.GetSystemVariableToken("NO");
			CharacterData masterChara = null;
			CharacterData targetChara = null;
			CharacterData assiChara = null;
			if (this.MASTER >= 0 && this.MASTER < varData.CharacterList.Count)
				masterChara = varData.CharacterList[(int)this.MASTER];
			if (this.TARGET >= 0 && this.TARGET < varData.CharacterList.Count)
				targetChara = varData.CharacterList[(int)this.TARGET];
			if (this.ASSI >= 0 && this.ASSI < varData.CharacterList.Count)
				assiChara = varData.CharacterList[(int)this.ASSI];

			for (int i = 0; i < varData.CharacterList.Count; i++)
			{
				varData.CharacterList[i].temp_CurrentOrder = i;
				varData.CharacterList[i].SetSortKey(sortkey, elem);
			}
			if ((fixMaster) && (masterChara != null))
			{
				if (varData.CharacterList.Count <= 2)
					return;
				varData.CharacterList.Remove(masterChara);
			}
			if (sortorder == SortOrder.ASCENDING)
				varData.CharacterList.Sort(CharacterData.AscCharacterComparison);
			else// if (sortorder == SortOrder.DESENDING)
				varData.CharacterList.Sort(CharacterData.DescCharacterComparison);
			//引数解析でチェック済み
			//else
			//    throw new ExeEE("ソート順序不明");

			if ((fixMaster) && (masterChara != null))
			{
				varData.CharacterList.Insert((int)this.MASTER, masterChara);
			}
			for (int i = 0; i < varData.CharacterList.Count; i++)
				varData.CharacterList[i].temp_CurrentOrder = i;
			if ((masterChara != null) && (!fixMaster))
				this.MASTER = masterChara.temp_CurrentOrder;
			if (targetChara != null)
				this.TARGET = targetChara.temp_CurrentOrder;
			if (assiChara != null)
				this.ASSI = assiChara.temp_CurrentOrder;
		}


		internal Int64 FindChara(VariableToken varID, Int64 elem64, string word, Int64 startIndex, Int64 lastIndex, bool isLast)
		{
			if (startIndex >= lastIndex)
				return -1;
			FixedVariableTerm fvp = new FixedVariableTerm(varID);
			if (varID.IsArray1D)
				fvp.Index2 = elem64;
			else if (varID.IsArray2D)
			{
				fvp.Index2 = elem64 >> 32;
				fvp.Index3 = elem64 & 0x7FFFFFFF;
			}
			//int count = varData.CharacterList.Count;
			if (isLast)
			{
				for (Int64 i = lastIndex - 1; i >= startIndex; i--)
				{
					fvp.Index1 = i;
					if (word == fvp.GetStrValue(null))
						return i;
				}
			}
			else
			{
				for (Int64 i = startIndex; i < lastIndex; i++)
				{
					fvp.Index1 = i;
					if (word == fvp.GetStrValue(null))
						return i;
				}
			}
			return -1;
		}

		internal Int64 FindChara(VariableToken varID, Int64 elem64, Int64 word, Int64 startIndex, Int64 lastIndex, bool isLast)
		{
			if (startIndex >= lastIndex)
				return -1;
			FixedVariableTerm fvp = new FixedVariableTerm(varID);
			if (varID.IsArray1D)
				fvp.Index2 = elem64;
			else if (varID.IsArray2D)
			{
				fvp.Index2 = elem64 >> 32;
				fvp.Index3 = elem64 & 0x7FFFFFFF;
			}
			//int count = varData.CharacterList.Count;
			if (isLast)
			{
				for (Int64 i = lastIndex - 1; i >= startIndex; i--)
				{
					fvp.Index1 = i;
					if (word == fvp.GetIntValue(null))
						return i;
				}
			}
			else
			{
				for (Int64 i = startIndex; i < lastIndex; i++)
				{
					fvp.Index1 = i;
					if (word == fvp.GetIntValue(null))
						return i;
				}
			}
			return -1;
		}

		public Int64 GetChara(Int64 charaNo)
		{
			int i;
			for (i = 0; i < varData.CharacterList.Count; i++)
			{
				if (varData.CharacterList[i].NO == charaNo)
					return (Int64)i;
			}
			return -1;
		}
		
		public Int64 GetChara_UseSp(Int64 charaNo, bool getSp)
		{
			//後天的にNOを変更する場合も考慮し、chara*.csvで定義されているかどうかは調べない。
			//CharacterTemplate tmpl = constant.GetCharacterTemplate(charaNo, false);
			//if (tmpl == null)
			//    return -1;
			int i;
			for (i = 0; i < varData.CharacterList.Count; i++)
			{
				if (varData.CharacterList[i].NO == charaNo)
				{
					bool isSp = varData.CharacterList[i].CFlag[0] != 0;
					if (isSp == getSp)
						return (Int64)i;
				}
			}
			return -1;
		}

		public Int64 ExistCsv(Int64 charaNo, bool getSp)
		{
			//SPキャラ廃止に伴う問題は呼び出し元で処理
			CharacterTemplate tmpl = constant.GetCharacterTemplate_UseSp(charaNo, getSp);
			if (tmpl == null)
				return 0;
			else
				return 1;
		}

		public string GetCharacterStrfromCSVData(Int64 charaTmplNo, CharacterStrData type, bool isSp, Int64 arg2Long)
		{
			//SPキャラ廃止に伴う問題は呼び出し元で処理
			CharacterTemplate tmpl = constant.GetCharacterTemplate_UseSp(charaTmplNo, isSp);
			if (tmpl == null)
				throw new CodeEE("定義していないキャラクタを参照しようとしました");
			int arg2 = (int)arg2Long;
			switch (type)
			{
				case CharacterStrData.CALLNAME:
					if (tmpl.Callname != null)
						return tmpl.Callname;
					else
						return "";
				case CharacterStrData.NAME:
					if (tmpl.Name != null)
						return tmpl.Name;
					else
						return "";
				case CharacterStrData.NICKNAME:
					if (tmpl.Nickname != null)
						return tmpl.Nickname;
					else
						return "";
				case CharacterStrData.MASTERNAME:
					if (tmpl.Mastername != null)
						return tmpl.Mastername;
					else
						return "";
				case CharacterStrData.CSTR:
					if (tmpl.CStr != null)
					{
						string ret = null;
						if (arg2 >= tmpl.ArrayStrLength(CharacterStrData.CSTR) || arg2 < 0)
							throw new CodeEE("CSTRの参照可能範囲外を参照しました");
						if (tmpl.CStr.TryGetValue(arg2, out ret))
							return ret;
						else
							return "";
					}
					else
						return "";
				default:
					throw new CodeEE("存在しないデータを参照しようとしました");
			}
		}

		public Int64 GetCharacterIntfromCSVData(Int64 charaTmplNo, CharacterIntData type, bool isSp, Int64 arg2Long)
		{
			//SPキャラ廃止に伴う問題は呼び出し元で処理
			CharacterTemplate tmpl = constant.GetCharacterTemplate_UseSp(charaTmplNo, isSp);
			if (tmpl == null)
				throw new CodeEE("定義していないキャラクタを参照しようとしました");
			if (arg2Long >= tmpl.ArrayLength(type) || arg2Long < 0)
				throw new CodeEE("参照可能範囲外を参照しました");
			int arg2 = (int)arg2Long;
			Dictionary<int, Int64> intDic = null;
			switch (type)
			{
				case CharacterIntData.BASE:
					intDic = tmpl.Maxbase; break;
				case CharacterIntData.MARK:
					intDic = tmpl.Mark; break;
				case CharacterIntData.ABL:
					intDic = tmpl.Abl; break;
				case CharacterIntData.EXP:
					intDic = tmpl.Exp; break;
				case CharacterIntData.RELATION:
					intDic = tmpl.Relation; break;
				case CharacterIntData.TALENT:
					intDic = tmpl.Talent; break;
				case CharacterIntData.CFLAG:
					intDic = tmpl.CFlag; break;
				case CharacterIntData.EQUIP:
					intDic = tmpl.Equip; break;
				case CharacterIntData.JUEL:
					intDic = tmpl.Juel; break;
				default:
					throw new CodeEE("存在しないデータを参照しようとしました");
			}
			Int64 ret;
			if (intDic.TryGetValue(arg2, out ret))
				return ret;
			return 0L;
		}

		public void UpdateInBeginTrain()
		{
			ASSIPLAY = 0;
			PREVCOM = -1;
			NEXTCOM = -1;
			Int64[] array;
			string[] sarray;
			array = varData.DataIntegerArray[(int)(VariableCode.TFLAG & VariableCode.__LOWERCASE__)];
			for (int i = 0; i < array.Length; i++)
				array[i] = 0;
			sarray = varData.DataStringArray[(int)(VariableCode.TSTR & VariableCode.__LOWERCASE__)];
			for (int i = 0; i < sarray.Length; i++)
				sarray[i] = "";
			//本家の仕様にあわせ、選択中以外のキャラクタも全部リセット。
			foreach (CharacterData chara in varData.CharacterList)
			{
				array = chara.DataIntegerArray[(int)(VariableCode.GOTJUEL & VariableCode.__LOWERCASE__)];
				for (int i = 0; i < array.Length; i++)
					array[i] = 0;
				array = chara.DataIntegerArray[(int)(VariableCode.TEQUIP & VariableCode.__LOWERCASE__)];
				for (int i = 0; i < array.Length; i++)
					array[i] = 0;
				array = chara.DataIntegerArray[(int)(VariableCode.EX & VariableCode.__LOWERCASE__)];
				for (int i = 0; i < array.Length; i++)
					array[i] = 0;
				//STAINは関数に切り出す（RESET_STAIN対応のため）
				setDefaultStain(chara);
				array = chara.DataIntegerArray[(int)(VariableCode.PALAM & VariableCode.__LOWERCASE__)];
				for (int i = 0; i < array.Length; i++)
					array[i] = 0;
				//1.728 このタイミングでSOURCEも更新されていた
				array = chara.DataIntegerArray[(int)(VariableCode.SOURCE & VariableCode.__LOWERCASE__)];
				for (int i = 0; i < array.Length; i++)
					array[i] = 0;
				//1.728 NOWEXはここでは更新されていない
				//1736f CTFLAGはTFLAGと同じ仕様で
				array = chara.DataIntegerArray[(int)(VariableCode.TCVAR & VariableCode.__LOWERCASE__)];
				for (int i = 0; i < array.Length; i++)
					array[i] = 0;
			}
		}

		public void UpdateAfterShowUsercom()
		{
			//UP = 0,DOWN = 0,LOSEBASE = 0
			Int64[] array;
			array = varData.DataIntegerArray[(int)(VariableCode.UP & VariableCode.__LOWERCASE__)];
			for (int i = 0; i < array.Length; i++)
				array[i] = 0;
			array = varData.DataIntegerArray[(int)(VariableCode.DOWN & VariableCode.__LOWERCASE__)];
			for (int i = 0; i < array.Length; i++)
				array[i] = 0;
			array = varData.DataIntegerArray[(int)(VariableCode.LOSEBASE & VariableCode.__LOWERCASE__)];
			for (int i = 0; i < array.Length; i++)
				array[i] = 0;
			foreach (CharacterData chara in varData.CharacterList)
			{
				array = chara.DataIntegerArray[(int)(VariableCode.DOWNBASE & VariableCode.__LOWERCASE__)];
				for (int i = 0; i < array.Length; i++)
					array[i] = 0;
				array = chara.DataIntegerArray[(int)(VariableCode.CUP & VariableCode.__LOWERCASE__)];
				for (int i = 0; i < array.Length; i++)
					array[i] = 0;
				array = chara.DataIntegerArray[(int)(VariableCode.CDOWN & VariableCode.__LOWERCASE__)];
				for (int i = 0; i < array.Length; i++)
					array[i] = 0;
			}

			//SOURCEはリセットタイミングが違うので消し
			//1.728 NOWEXも微妙に違うので移動
		}

		//1.728 NOWEXもリセットタイミングが違うので移動
		//UP,DOWN,LOSEBASEはUSERCOMに移動する場合にもリセットされるがNOWEXはCOMが実行される場合のみ更新される
		//なのでEVENTCOM直前に呼ぶ
		public void UpdateAfterInputCom()
		{
			//本家の仕様にあわせ、選択中以外のキャラクタも全部リセット。
			Int64[] array;
			foreach (CharacterData chara in varData.CharacterList)
			{
				array = chara.DataIntegerArray[(int)(VariableCode.NOWEX & VariableCode.__LOWERCASE__)];
				for (int i = 0; i < array.Length; i++)
					array[i] = 0;
			}
		}

		//SOURCEのリセットタイミングはUP、DOWN、LOSEBASE、NOWEXと違いSOURCECHECK終了後なので切り分け
		public void UpdateAfterSourceCheck()
		{
			//本家の仕様にあわせ、選択中以外のキャラクタも全部リセット。
			Int64[] array;
			foreach (CharacterData chara in varData.CharacterList)
			{
				array = chara.DataIntegerArray[(int)(VariableCode.SOURCE & VariableCode.__LOWERCASE__)];
				for (int i = 0; i < array.Length; i++)
					array[i] = 0;
			}
		}

		//PREVCOMは更新されない。スクリプトの方で更新する必要がある。
		//Data側からEmueraConsoleを操作するのはここだけ。
		//1756 ↑だったのは今は昔の話である
		public void UpdateInUpcheck(EmueraConsole window, bool skipPrint)
		{
			Int64[] up, down, param;
			string[] paramname = constant.GetCsvNameList(VariableCode.PALAMNAME);
			up = varData.DataIntegerArray[(int)(VariableCode.UP & VariableCode.__LOWERCASE__)];
			down = varData.DataIntegerArray[(int)(VariableCode.DOWN & VariableCode.__LOWERCASE__)];
			Int64 target = TARGET;
			if ((target < 0) || (target >= varData.CharacterList.Count))
				goto end;
			CharacterData chara = varData.CharacterList[(int)target];
			param = chara.DataIntegerArray[(int)(VariableCode.PALAM & VariableCode.__LOWERCASE__)];
			int length = param.Length;
			if (param.Length > up.Length)
				length = up.Length;
			if (param.Length > down.Length)
				length = down.Length;

			for (int i = 0; i < length; i++)
			{
				//本家の仕様では負の値は無効。
				if ((up[i] <= 0) && (down[i] <= 0))
					continue;
				StringBuilder builder = new StringBuilder();
				if (!skipPrint)
				{
					builder.Append(paramname[i]);
					builder.Append(' ');
					builder.Append(param[i].ToString());
					if (up[i] > 0)
					{
						builder.Append('+');
						builder.Append(up[i].ToString());
					}
					if (down[i] > 0)
					{
						builder.Append('-');
						builder.Append(down[i].ToString());
					}
				}
				unchecked { param[i] += up[i] - down[i]; }
				if (!skipPrint)
				{
					builder.Append('=');
					builder.Append(param[i].ToString());
					window.Print(builder.ToString());
					window.NewLine();
				}

			}
		end:
			for (int i = 0; i < up.Length; i++)
				up[i] = 0;
			for (int i = 0; i < down.Length; i++)
				down[i] = 0;
		}

		public void CUpdateInUpcheck(EmueraConsole window, Int64 target, bool skipPrint)
		{
			Int64[] up, down, param;
			string[] paramname = constant.GetCsvNameList(VariableCode.PALAMNAME);
			if ((target < 0) || (target >= varData.CharacterList.Count))
				return;
			CharacterData chara = varData.CharacterList[(int)target];
			up = chara.DataIntegerArray[(int)(VariableCode.CUP & VariableCode.__LOWERCASE__)];
			down = chara.DataIntegerArray[(int)(VariableCode.CDOWN & VariableCode.__LOWERCASE__)];
			param = chara.DataIntegerArray[(int)(VariableCode.PALAM & VariableCode.__LOWERCASE__)];
			int length = param.Length;
			if (param.Length > up.Length)
				length = up.Length;
			if (param.Length > down.Length)
				length = down.Length;

			for (int i = 0; i < length; i++)
			{
				//本家の仕様では負の値は無効。
				if ((up[i] <= 0) && (down[i] <= 0))
					continue;
				StringBuilder builder = new StringBuilder();
				if (!skipPrint)
				{
					builder.Append(paramname[i]);
					builder.Append(' ');
					builder.Append(param[i].ToString());
					if (up[i] > 0)
					{
						builder.Append('+');
						builder.Append(up[i].ToString());
					}
					if (down[i] > 0)
					{
						builder.Append('-');
						builder.Append(down[i].ToString());
					}
				}
				unchecked { param[i] += up[i] - down[i]; }
				if (!skipPrint)
				{
					builder.Append('=');
					builder.Append(param[i].ToString());
					window.Print(builder.ToString());
					window.NewLine();
				}
			}
			for (int i = 0; i < up.Length; i++)
				up[i] = 0;
			for (int i = 0; i < down.Length; i++)
				down[i] = 0;
		}

		private void setDefaultStain(CharacterData chara)
		{
			long[] array = chara.DataIntegerArray[(int)(VariableCode.STAIN & VariableCode.__LOWERCASE__)];
			//STAINの配列要素数 < _REPLACE.CSVのSTAIN初期値の指定数の時エラーになるのを対処
			if (array.Length >= Config.StainDefault.Count)
			{
				Config.StainDefault.CopyTo(array);
				for (int i = Config.StainDefault.Count; i < array.Length; i++)
					array[i] = 0;
			}
			else
			{
				for (int i = 0; i < array.Length; i++)
					array[i] = Config.StainDefault[i];
			}
		}

		public void SetDefaultStain(Int64 no)
		{
			if (no < 0 || no >= varData.CharacterList.Count)
				throw new CodeEE("存在しないキャラクターを参照しようとしました");
			CharacterData chara = varData.CharacterList[(int)no];
			setDefaultStain(chara);
		}

		/// <summary>
		/// RESULTに配列のサイズを代入。二次元配列ならRESULT:1に二番目のサイズを代入。三次元配列ならRESULT:1に二番目、RESULT:2に三番目のサイズを代入
		/// </summary>
		/// <param name="varID"></param>
		/// <returns></returns>
		public void VarSize(VariableToken varID)
		{
			Int64[] resultArray = RESULT_ARRAY;
			if (varID.IsArray2D)
			{
				resultArray[0] = varID.GetLength(0);
				resultArray[1] = varID.GetLength(1);
			}
			else if (varID.IsArray3D)
			{
				resultArray[0] = varID.GetLength(0);
				resultArray[1] = varID.GetLength(1);
				resultArray[2] = varID.GetLength(2);
			}
			else
			{
				resultArray[0] = varID.GetLength();
			}
		}

		public bool ItemSales(Int64 itemNo)
		{
			Int64[] itemSales = ITEMSALES;
			string[] itemNames = constant.GetCsvNameList(VariableCode.ITEMNAME);
			if ((itemNo < 0) || (itemNo >= itemSales.Length) || (itemNo >= itemNames.Length))
				return false;
			int index = (int)itemNo;
			return ((itemSales[index] != 0) && (itemNames[index] != null));
		}

		public bool BuyItem(Int64 itemNo)
		{
			if (!ItemSales(itemNo))
				return false;
			Int64[] itemPrice = constant.ItemPrice;
			if (itemNo >= itemPrice.Length)
				return false;
			int index = (int)itemNo;
			if (MONEY < itemPrice[index])
				return false;
			MONEY -= itemPrice[index];
			ITEM[index]++;
			BOUGHT = itemNo;
			return true;
		}


		public void SetEncodingResult(int[] ary)
		{
			long[] resary = varData.DataIntegerArray[(int)(VariableCode.RESULT & VariableCode.__LOWERCASE__)];
			resary[0] = ary.Length;
			for (int i = 0; i < ary.Length; i++)
				resary[i + 1] = ary[i];
		}

		#endregion

		//ちーと
		public void IamaMunchkin()
		{
			if ((MASTER < 0) || (MASTER >= varData.CharacterList.Count))
				return;
			varData.CharacterList[(int)MASTER].DataString[(int)(VariableCode.NAME & VariableCode.__LOWERCASE__)] = "イカサマ";
			varData.CharacterList[(int)MASTER].DataString[(int)(VariableCode.CALLNAME & VariableCode.__LOWERCASE__)] = "イカサマ";
			varData.CharacterList[(int)MASTER].DataString[(int)(VariableCode.NICKNAME & VariableCode.__LOWERCASE__)] = "イカサマ";

		}

		public void SetResultX(List<long> values)
		{
			for (int i = 0; i < values.Count; i++)
			{
				if (i >= varData.DataIntegerArray[(int)(VariableCode.RESULT & VariableCode.__LOWERCASE__)].Length)
					return;
				varData.DataIntegerArray[(int)(VariableCode.RESULT & VariableCode.__LOWERCASE__)][i] = values[i];
			}
		}

		#region File操作


		private string getSaveDataPathG() { return Config.SavDir + "global.sav"; }
		private string getSaveDataPath(int index) { return string.Format("{0}save{1:00}.sav", Config.SavDir, index); }
		private string getSaveDataPath(string s) { return string.Format("{0}save{1:00}.sav", Config.SavDir, s); }

		private string getSaveDataPathV(int index) { return Program.DatDir + string.Format("var_{0:00}.dat", index); }
		private string getSaveDataPathC(int index) { return Program.DatDir + string.Format("chara_{0:00}.dat", index); }
		private string getSaveDataPathV(string s) { return Program.DatDir + "var_" + s + ".dat"; }
		private string getSaveDataPathC(string s) { return Program.DatDir + "chara_" + s + ".dat"; }

		/// <summary>
		/// DatFolderが存在せず、かつ作成に失敗したらエラーを投げる
		/// </summary>
		/// <returns></returns>
		public void CreateDatFolder()
		{
			if (Directory.Exists(Program.DatDir))
				return;
			try
			{
				Directory.CreateDirectory(Program.DatDir);
			}
			catch
			{
				MessageBox.Show("datフォルダーの作成に失敗しました");
				throw new CodeEE("datフォルダーの作成に失敗しました");
			}
		}

		public List<string> GetDatFiles(bool charadat, string pattern)
		{
			List<string> files = new List<string>();
			if (!Directory.Exists(Program.DatDir))
				return files;
			string searchPattern = "var_" + pattern + ".dat";
			if (charadat)
				searchPattern = "chara_" + pattern + ".dat";
			string[] pathes = Directory.GetFiles(Program.DatDir, searchPattern, SearchOption.TopDirectoryOnly);
			foreach (string path in pathes)
			{
				if (!Path.GetExtension(path).Equals(".dat", StringComparison.OrdinalIgnoreCase))
					continue;
				string filename = Path.GetFileNameWithoutExtension(path);
				if (charadat)
					filename = filename.Substring(6);
				else
					filename = filename.Substring(4);
				if (string.IsNullOrEmpty(filename))
					continue;
				files.Add(filename);
			}
			return files;
		}

		/// <summary>
		/// 文字列がファイル名の一部として適切かどうか調べる
		/// </summary>
		/// <param name="datfilename"></param>
		/// <returns>適切ならnull、不適切ならエラーメッセージ</returns>
		public string CheckDatFilename(string datfilename)
		{
			if (string.IsNullOrEmpty(datfilename))
				return "ファイル名が指定されていません";
			if (datfilename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
				return "ファイル名に不正な文字が含まれています";
			return null;
		}

		public EraDataResult CheckData(string savename, EraSaveFileType type)
		{
			string filename = null;
			switch (type)
			{
				case EraSaveFileType.Normal:
					filename = getSaveDataPath(savename); break;
				case EraSaveFileType.Global:
					filename = getSaveDataPathG(); break;
				case EraSaveFileType.Var:
					filename = getSaveDataPathV(savename); break;
				case EraSaveFileType.CharVar:
					filename = getSaveDataPathC(savename); break;
			}
			return CheckDataByFilename(filename, type);
		}

		public EraDataResult CheckData(int saveIndex, EraSaveFileType type)
		{
			string filename = null;
			switch (type)
			{
				case EraSaveFileType.Normal:
					filename = getSaveDataPath(saveIndex); break;
				case EraSaveFileType.Global:
					filename = getSaveDataPathG(); break;
				case EraSaveFileType.Var:
					filename = getSaveDataPathV(saveIndex); break;
				case EraSaveFileType.CharVar:
					filename = getSaveDataPathC(saveIndex); break;
			}
			return CheckDataByFilename(filename, type);
		}

		public EraDataResult CheckDataByFilename(string filename, EraSaveFileType type)
		{
			EraDataResult result = new EraDataResult();
			if (!File.Exists(filename))
			{
				result.State = EraDataState.FILENOTFOUND;
				result.DataMes = "----";
				return result;
			}
			FileStream fs = null;
			EraBinaryDataReader bReader = null;
			EraDataReader reader = null;
			Int64 version = 0;
			try
			{
				fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
				bReader = EraBinaryDataReader.CreateReader(fs);
				if (bReader == null)//eramaker形式
				{
					reader = new EraDataReader(fs);
					if (!gamebase.UniqueCodeEqualTo(reader.ReadInt64()))
					{
						result.State = EraDataState.GAME_ERROR;
						result.DataMes = "異なるゲームのセーブデータです";
						return result;
					}
					version = reader.ReadInt64();
					if (!gamebase.CheckVersion(version))
					{
						result.State = EraDataState.VIRSION_ERROR;
						result.DataMes = "セーブデータのバーションが異なります";
						return result;
					}
					result.State = EraDataState.OK;
					result.DataMes = reader.ReadString();
					return result;
					//result.State = EraDataState.ETC_ERROR;
					//result.DataMes = "セーブデータが壊れています";
					//return result;
				}
				EraSaveFileType fileType = bReader.ReadFileType();
				if (type != fileType)
				{
					result.State = EraDataState.ETC_ERROR;
					result.DataMes = "セーブデータが壊れています";
					return result;
				}
				if (!gamebase.UniqueCodeEqualTo(bReader.ReadInt64()))
				{
					result.State = EraDataState.GAME_ERROR;
					result.DataMes = "異なるゲームのセーブデータです";
					return result;
				}
				version = bReader.ReadInt64();
				if (!gamebase.CheckVersion(version))
				{
					result.State = EraDataState.VIRSION_ERROR;
					result.DataMes = "セーブデータのバーションが異なります";
					return result;
				}
				result.State = EraDataState.OK;
				result.DataMes = bReader.ReadString();
				return result;
			}
			catch (FileEE fee)
			{
				result.State = EraDataState.ETC_ERROR;
				result.DataMes = fee.Message;
			}
			catch (Exception)
			{
				result.State = EraDataState.ETC_ERROR;
				result.DataMes = "読み込み中にエラーが発生しました";
			}
			finally
			{
				if (reader != null)
					reader.Close();
				else if (bReader != null)
					bReader.Close();
				else if (fs != null)
					fs.Close();
			}
			return result;
		}

		////これは理屈上VariableEvaluator上で動くはず
		//public EraDataResult checkData(int saveIndex)
		//{
		//    string filename = getSaveDataPath(saveIndex);
		//    EraDataResult result = new EraDataResult();
		//    EraDataReader reader = null;
		//    try
		//    {
		//        if (!File.Exists(filename))
		//        {
		//            result.State = EraDataState.FILENOTFOUND;
		//            result.DataMes = "----";
		//            return result;
		//        }
		//        reader = new EraDataReader(filename);
		//        if (!gamebase.UniqueCodeEqualTo(reader.ReadInt64()))
		//        {
		//            result.State = EraDataState.GAME_ERROR;
		//            result.DataMes = "異なるゲームのセーブデータです";
		//            return result;
		//        }
		//        Int64 version = reader.ReadInt64();
		//        if (!gamebase.CheckVersion(version))
		//        {
		//            result.State = EraDataState.VIRSION_ERROR;
		//            result.DataMes = "セーブデータのバーションが異なります";
		//            return result;
		//        }
		//        result.State = EraDataState.OK;
		//        result.DataMes = reader.ReadString();
		//        return result;
		//    }
		//    catch (FileEE fee)
		//    {
		//        result.State = EraDataState.ETC_ERROR;
		//        result.DataMes = fee.Message;
		//    }
		//    catch (Exception)
		//    {
		//        result.State = EraDataState.ETC_ERROR;
		//        result.DataMes = "読み込み中にエラーが発生しました";
		//    }
		//    finally
		//    {
		//        if (reader != null)
		//            reader.Close();
		//    }
		//    return result;
		//}

		public void SaveChara(string savename, string savMes, int[] charas)
		{
			CreateDatFolder();
			CheckDatFilename(savename);
			string filepath = getSaveDataPathC(savename);
			EraBinaryDataWriter bWriter = null;
			FileStream fs = null;
			try
			{
				Config.CreateSavDir();
				fs = new FileStream(filepath, FileMode.Create, FileAccess.Write);
				bWriter = new EraBinaryDataWriter(fs);
				bWriter.WriteHeader();
				bWriter.WriteFileType(EraSaveFileType.CharVar);
				bWriter.WriteInt64(gamebase.ScriptUniqueCode);
				bWriter.WriteInt64(gamebase.ScriptVersion);
				bWriter.WriteString(savMes);
				bWriter.WriteInt64(charas.Length);//保存するキャラ数
				for (int i = 0; i < charas.Length; i++)
				{
					varData.CharacterList[charas[i]].SaveToStreamBinary(bWriter, varData);
				}
				bWriter.WriteEOF();
				//RESULT = 1;
				return;
			}
			//catch (Exception)
			//{
			//	throw new CodeEE("セーブ中にエラーが発生しました");
			//}
			finally
			{
				if (bWriter != null)
					bWriter.Close();
				else if (fs != null)
					fs.Close();
			}
		}

		public void LoadChara(string savename)
		{
			string filepath = getSaveDataPathC(savename);
			RESULT = 0;
			if (!File.Exists(filepath))
				return;
			EraBinaryDataReader bReader = null;
			FileStream fs = null;
			try
			{
				List<CharacterData> addCharaList = new List<CharacterData>();
				fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
				bReader = EraBinaryDataReader.CreateReader(fs);
				if (bReader == null)
					return;
				if (bReader.ReadFileType() != EraSaveFileType.CharVar)
					return;

				if (!gamebase.UniqueCodeEqualTo(bReader.ReadInt64()))
					return;
				Int64 version = bReader.ReadInt64();
				if (!gamebase.CheckVersion(version))
					return;
				bReader.ReadString();//saveMes
				Int64 loadnum = bReader.ReadInt64();
				for (int i = 0; i < loadnum; i++)
				{
					CharacterData chara = new CharacterData(constant, varData);
					chara.LoadFromStreamBinary(bReader);
					addCharaList.Add(chara);
				}
				varData.CharacterList.AddRange(addCharaList);
				RESULT = 1;
				return;
			}
			//catch (Exception)
			//{
			//	return;
			//}
			finally
			{
				if (bReader != null)
					bReader.Close();
				else if (fs != null)
					fs.Close();
			}
		}

		public void SaveVariable(string savename, string savMes, VariableToken[] vars)
		{
			CreateDatFolder();
			CheckDatFilename(savename);
			string filepath = getSaveDataPathV(savename);
			EraBinaryDataWriter bWriter = null;
			FileStream fs = null;
			try
			{
				Config.CreateSavDir();
				fs = new FileStream(filepath, FileMode.Create, FileAccess.Write);
				bWriter = new EraBinaryDataWriter(fs);
				bWriter.WriteHeader();
				bWriter.WriteFileType(EraSaveFileType.Var);
				bWriter.WriteInt64(gamebase.ScriptUniqueCode);
				bWriter.WriteInt64(gamebase.ScriptVersion);
				bWriter.WriteString(savMes);

				for (int i = 0; i < vars.Length; i++)
					bWriter.WriteWithKey(vars[i].Name, vars[i].GetArray());
				bWriter.WriteEOF();
				//RESULT = 1;
				return;
			}
			//catch (Exception)
			//{
			//	throw new CodeEE("セーブ中にエラーが発生しました");
			//}
			finally
			{
				if (bWriter != null)
					bWriter.Close();
				else if (fs != null)
					fs.Close();
			}
		}

		public void LoadVariable(string savename)
		{
			string filepath = getSaveDataPathV(savename);
			RESULT = 0;
			if (!File.Exists(filepath))
				return;
			EraBinaryDataReader bReader = null;
			FileStream fs = null;
			try
			{
				fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
				bReader = EraBinaryDataReader.CreateReader(fs);
				if (bReader == null)
					return;
				if (bReader.ReadFileType() != EraSaveFileType.Var)
					return;

				if (!gamebase.UniqueCodeEqualTo(bReader.ReadInt64()))
					return;
				Int64 version = bReader.ReadInt64();
				if (!gamebase.CheckVersion(version))
					return;
				bReader.ReadString();//saveMes
				while (varData.LoadVariableBinary(bReader)) { }
				RESULT = 1;
				return;
			}
			//catch (Exception)
			//{
			//	return;
			//}
			finally
			{
				if (bReader != null)
					bReader.Close();
				else if (fs != null)
					fs.Close();
			}
		}

		public void SaveToStream(EraDataWriter writer, string saveDataText)
		{
			writer.Write(gamebase.ScriptUniqueCode);
			writer.Write(gamebase.ScriptVersion);
			writer.Write(saveDataText);
			writer.Write(varData.CharacterList.Count);
			for (int i = 0; i < varData.CharacterList.Count; i++)
			{
				varData.CharacterList[i].SaveToStream(writer);
			}
			varData.SaveToStream(writer);
			writer.EmuStart();
			for (int i = 0; i < varData.CharacterList.Count; i++)
			{
				varData.CharacterList[i].SaveToStreamExtended(writer);
			}
			varData.SaveToStreamExtended(writer);
		}

		public void LoadFromStream(EraDataReader reader)
		{
			if (!gamebase.UniqueCodeEqualTo(reader.ReadInt64()))
				throw new FileEE("異なるゲームのセーブデータです");
			Int64 version = reader.ReadInt64();
			if (!gamebase.CheckVersion(version))
				throw new FileEE("セーブデータのバーションが異なります");
			string text = reader.ReadString();//PUTFORM
			varData.SetDefaultValue(constant);
			varData.SetDefaultLocalValue();
			varData.LastLoadVersion = version;
			//varData.LastLoadNo = dataIndex;
			varData.LastLoadText = text;

			int charaCount = (int)reader.ReadInt64();
			varData.CharacterList.Clear();
			for (int i = 0; i < charaCount; i++)
			{
				CharacterData chara = new CharacterData(constant, varData);
				varData.CharacterList.Add(chara);
				chara.LoadFromStream(reader);
			}
			varData.LoadFromStream(reader);
			if (reader.SeekEmuStart())
			{
				if (reader.DataVersion < 1803)//キャラ2次元配列追加以前
					for (int i = 0; i < charaCount; i++)
						varData.CharacterList[i].LoadFromStreamExtended_Old1802(reader);
				else
					for (int i = 0; i < charaCount; i++)
						varData.CharacterList[i].LoadFromStreamExtended(reader);
				varData.LoadFromStreamExtended(reader, reader.DataVersion);
			}
		}

		public bool SaveGlobal()
		{
			string filepath = getSaveDataPathG();
			try
			{
				Config.CreateSavDir();
				using (FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write))
				{
					if (Config.SystemSaveInBinary)
					{

						using (EraBinaryDataWriter bWriter = new EraBinaryDataWriter(fs))
						{
							bWriter.WriteHeader();
							bWriter.WriteFileType(EraSaveFileType.Global);
							bWriter.WriteInt64(gamebase.ScriptUniqueCode);
							bWriter.WriteInt64(gamebase.ScriptVersion);
							bWriter.WriteString("");//saveMes
							varData.SaveGlobalToStreamBinary(bWriter);
							bWriter.WriteEOF();
							bWriter.Close();
						}
					}
					else
					{
						using (EraDataWriter writer = new EraDataWriter(fs))
						{
							writer.Write(gamebase.ScriptUniqueCode);
							writer.Write(gamebase.ScriptVersion);
							varData.SaveGlobalToStream(writer);
							writer.EmuStart();
							varData.SaveGlobalToStream1808(writer);
							writer.Close();
						}
					}
				}
			}
			catch (SystemException)
			{
				throw new CodeEE("グローバルデータの保存中にエラーが発生しました");
				//console.PrintError(
				//console.NewLine();
				//return false;
			}
			//finally
			//{
			//	if (writer != null)
			//		writer.Close();
			//	else if (bWriter != null)
			//		bWriter.Close();
			//	else if (fs != null)
			//		fs.Close();
			//}
			return true;
		}

		public bool LoadGlobal()
		{
			string filepath = getSaveDataPathG();
			if (!File.Exists(filepath))
				return false;
			EraDataReader reader = null;
			EraBinaryDataReader bReader = null;
			FileStream fs = null;
			try
			{
				fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
				bReader = EraBinaryDataReader.CreateReader(fs);
				if (bReader != null)
				{
					EraSaveFileType fileType = bReader.ReadFileType();
					if (fileType != EraSaveFileType.Global)
						return false;
					if (!gamebase.UniqueCodeEqualTo(bReader.ReadInt64()))
						return false;
					Int64 version = bReader.ReadInt64();
					if (!gamebase.CheckVersion(version))
						return false;
					bReader.ReadString();//saveMes
					varData.LoadFromStreamBinary(bReader);
				}
				else
				{
					reader = new EraDataReader(fs);
					if (!gamebase.UniqueCodeEqualTo(reader.ReadInt64()))
						return false;
					Int64 version = reader.ReadInt64();
					if (!gamebase.CheckVersion(version))
						return false;
					varData.LoadGlobalFromStream(reader);
					if (reader.SeekEmuStart())
					{
						varData.LoadGlobalFromStream1808(reader);
					}
				}
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				if (reader != null)
					reader.Close();
				else if (bReader != null)
					bReader.Close();
				else if (fs != null)
					fs.Close();
			}
		}

		public void SaveToStreamBinary(EraBinaryDataWriter bWriter, string saveDataText)
		{
			bWriter.WriteHeader();
			bWriter.WriteFileType(EraSaveFileType.Normal);
			bWriter.WriteInt64(gamebase.ScriptUniqueCode);
			bWriter.WriteInt64(gamebase.ScriptVersion);
			bWriter.WriteString(saveDataText);
			bWriter.WriteInt64(varData.CharacterList.Count);
			for (int i = 0; i < varData.CharacterList.Count; i++)
			{
				varData.CharacterList[i].SaveToStreamBinary(bWriter, varData);
			}
			varData.SaveToStreamBinary(bWriter);
			bWriter.WriteEOF();
		}

		public void LoadFromStreamBinary(EraBinaryDataReader bReader)
		{
			EraSaveFileType fileType = bReader.ReadFileType();
			if (fileType != EraSaveFileType.Normal)
				throw new FileEE("セーブデータが壊れています");
			if (!gamebase.UniqueCodeEqualTo(bReader.ReadInt64()))
				throw new FileEE("異なるゲームのセーブデータです");
			Int64 version = bReader.ReadInt64();
			if (!gamebase.CheckVersion(version))
				throw new FileEE("セーブデータのバーションが異なります");
			string text = bReader.ReadString();//PUTFORM
			varData.SetDefaultValue(constant);
			varData.SetDefaultLocalValue();
			varData.LastLoadVersion = version;
			//varData.LastLoadNo = dataIndex;
			varData.LastLoadText = text;

			int charaCount = (int)bReader.ReadInt64();
			varData.CharacterList.Clear();
			for (int i = 0; i < charaCount; i++)
			{
				CharacterData chara = new CharacterData(constant, varData);
				varData.CharacterList.Add(chara);
				chara.LoadFromStreamBinary(bReader);
			}
			varData.LoadFromStreamBinary(bReader);
		}

		public bool SaveTo(int saveIndex, string saveText)
		{
			string filepath = getSaveDataPath(saveIndex);
			FileStream fs = null;
			EraDataWriter writer = null;
			EraBinaryDataWriter bWriter = null;
			try
			{
				Config.CreateSavDir();
				fs = new FileStream(filepath, FileMode.Create, FileAccess.Write);
				if (Config.SystemSaveInBinary)
				{
					bWriter = new EraBinaryDataWriter(fs);
					SaveToStreamBinary(bWriter, saveText);
				}
				else
				{
					writer = new EraDataWriter(fs);
					SaveToStream(writer, saveText);
				}
				return true;
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				if (writer != null)
					writer.Close();
				else if (bWriter != null)
					bWriter.Close();
				else if (fs != null)
					fs.Close();
			}
		}

		public bool LoadFrom(int dataIndex)
		{
			string filepath = getSaveDataPath(dataIndex);
			if (!File.Exists(filepath))
				throw new ExeEE("存在しないパスを呼び出した");
			EraDataReader reader = null;
			EraBinaryDataReader bReader = null;
			FileStream fs = null;
			try
			{
				fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
				bReader = EraBinaryDataReader.CreateReader(fs);
				if (bReader != null)
				{
					LoadFromStreamBinary(bReader);
				}
				else
				{
					reader = new EraDataReader(fs);
					LoadFromStream(reader);
				}
				varData.LastLoadNo = dataIndex;
			}
			finally
			{
				if (reader != null)
					reader.Close();
				else if (bReader != null)
					bReader.Close();
				else if (fs != null)
					fs.Close();
			}
			return true;
		}

		public void DelData(int dataIndex)
		{
			string filepath = getSaveDataPath(dataIndex);
			if (!File.Exists(filepath))
				return;
			FileAttributes att = File.GetAttributes(filepath);
			if ((att & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				throw new CodeEE("指定されたファイル\"" + filepath + "\"は読み込み専用のため削除できません");
			//{

			//    console.PrintError("指定されたファイル\"" + filepath + "\"は読み込み専用のため削除できません");
			//    return;
			//}
			File.Delete(filepath);
			return;
		}



		#endregion
		#region IDisposable メンバ

		public void Dispose()
		{
			varData.Dispose();
		}

		#endregion
		#region Property
		public Int64[] RESULT_ARRAY
		{
			get { return varData.DataIntegerArray[(int)(VariableCode.RESULT & VariableCode.__LOWERCASE__)]; }
		}
		public Int64 RESULT
		{
			get { return varData.DataIntegerArray[(int)(VariableCode.RESULT & VariableCode.__LOWERCASE__)][0]; }
			set { varData.DataIntegerArray[(int)(VariableCode.RESULT & VariableCode.__LOWERCASE__)][0] = value; }
		}
		public Int64 COUNT
		{
			get { return varData.DataIntegerArray[(int)(VariableCode.COUNT & VariableCode.__LOWERCASE__)][0]; }
			set { varData.DataIntegerArray[(int)(VariableCode.COUNT & VariableCode.__LOWERCASE__)][0] = value; }
		}
		public string RESULTS
		{
			get
			{
				string ret = varData.DataStringArray[(int)(VariableCode.RESULTS & VariableCode.__LOWERCASE__)][0];
				if (ret == null)
					return "";
				return ret;
			}
			set { varData.DataStringArray[(int)(VariableCode.RESULTS & VariableCode.__LOWERCASE__)][0] = value; }
		}
		public string[] RESULTS_ARRAY
		{
			get { return varData.DataStringArray[(int)(VariableCode.RESULTS & VariableCode.__LOWERCASE__)]; }
		}

		public Int64 TARGET
		{
			get { return varData.DataIntegerArray[(int)(VariableCode.TARGET & VariableCode.__LOWERCASE__)][0]; }
			set { varData.DataIntegerArray[(int)(VariableCode.TARGET & VariableCode.__LOWERCASE__)][0] = value; }
		}
		public Int64[] SELECTCOM_ARRAY
		{
			get { return varData.DataIntegerArray[(int)(VariableCode.SELECTCOM & VariableCode.__LOWERCASE__)]; }
		}
		public Int64 SELECTCOM
		{
			get { return varData.DataIntegerArray[(int)(VariableCode.SELECTCOM & VariableCode.__LOWERCASE__)][0]; }
			set { varData.DataIntegerArray[(int)(VariableCode.SELECTCOM & VariableCode.__LOWERCASE__)][0] = value; }
		}
		public string[] ITEMNAME
		{
			get { return constant.GetCsvNameList(VariableCode.ITEMNAME); }
		}

		public Int64[] ITEMSALES
		{
			get { return varData.DataIntegerArray[(int)(VariableCode.ITEMSALES & VariableCode.__LOWERCASE__)]; }
		}

		public Int64[] ITEMPRICE
		{
			get { return constant.ItemPrice; }
		}

		private Int64[] ITEM
		{
			get { return varData.DataIntegerArray[(int)(VariableCode.ITEM & VariableCode.__LOWERCASE__)]; }
		}

		public Int64[] RANDDATA
		{
			get { return varData.DataIntegerArray[(int)(VariableCode.RANDDATA & VariableCode.__LOWERCASE__)]; }
		}

		public string SAVEDATA_TEXT
		{
			get { return varData.DataString[(int)(VariableCode.SAVEDATA_TEXT & VariableCode.__LOWERCASE__)]; }
			set { varData.DataString[(int)(VariableCode.SAVEDATA_TEXT & VariableCode.__LOWERCASE__)] = value; }
		}
		public Int64 CHARANUM
		{
			get { return varData.CharacterList.Count; }
		}




		private Int64 get_Variable_canforbid(VariableCode code)
		{
			long[] array = varData.DataIntegerArray[(int)(code & VariableCode.__LOWERCASE__)];
			if (array.Length == 0)
				return -1;
			return array[0];
		}
		private void set_Variable_canforbid(VariableCode code, Int64 value)
		{
			long[] array = varData.DataIntegerArray[(int)(code & VariableCode.__LOWERCASE__)];
			if (array.Length == 0)
				return;
			array[0] = value;
		}

		public Int64 MASTER
		{
			get { return get_Variable_canforbid(VariableCode.MASTER); }
			set { set_Variable_canforbid(VariableCode.MASTER, value); }
		}
		public Int64 ASSI
		{
			get { return get_Variable_canforbid(VariableCode.ASSI); }
			set { set_Variable_canforbid(VariableCode.ASSI, value); }
		}
		public Int64 ASSIPLAY
		{
			set { set_Variable_canforbid(VariableCode.ASSIPLAY, value); }
		}
		public Int64 PREVCOM
		{
			get { return get_Variable_canforbid(VariableCode.PREVCOM); }
			set { set_Variable_canforbid(VariableCode.PREVCOM, value); }
		}
		public Int64 NEXTCOM
		{
			get { return get_Variable_canforbid(VariableCode.NEXTCOM); }
			set { set_Variable_canforbid(VariableCode.NEXTCOM, value); }
		}
		private Int64 MONEY
		{
			get { return get_Variable_canforbid(VariableCode.MONEY); }
			set { set_Variable_canforbid(VariableCode.MONEY, value); }
		}

		private Int64 BOUGHT
		{
			set { set_Variable_canforbid(VariableCode.BOUGHT, value); }
		}

		//public Int64 MASTER
		//{
		//	get { return varData.DataIntegerArray[(int)(VariableCode.MASTER & VariableCode.__LOWERCASE__)][0]; }
		//	set { varData.DataIntegerArray[(int)(VariableCode.MASTER & VariableCode.__LOWERCASE__)][0] = value; }
		//}
		//public Int64 ASSI
		//{
		//	get { return varData.DataIntegerArray[(int)(VariableCode.ASSI & VariableCode.__LOWERCASE__)][0]; }
		//	set { varData.DataIntegerArray[(int)(VariableCode.ASSI & VariableCode.__LOWERCASE__)][0] = value; }
		//}
		//public Int64 ASSIPLAY
		//{
		//	//get { return varData.DataIntegerArray[(int)(VariableCode.ASSIPLAY & VariableCode.__LOWERCASE__)][0]; }
		//	set { varData.DataIntegerArray[(int)(VariableCode.ASSIPLAY & VariableCode.__LOWERCASE__)][0] = value; }
		//}
		//public Int64 PREVCOM
		//{
		//	//get { return varData.DataIntegerArray[(int)(VariableCode.PREVCOM & VariableCode.__LOWERCASE__)][0]; }
		//	set { varData.DataIntegerArray[(int)(VariableCode.PREVCOM & VariableCode.__LOWERCASE__)][0] = value; }
		//}
		//public Int64 NEXTCOM
		//{
		//	get { return varData.DataIntegerArray[(int)(VariableCode.NEXTCOM & VariableCode.__LOWERCASE__)][0]; }
		//	set { varData.DataIntegerArray[(int)(VariableCode.NEXTCOM & VariableCode.__LOWERCASE__)][0] = value; }
		//}
		//private Int64 MONEY
		//{
		//	get { return varData.DataIntegerArray[(int)(VariableCode.MONEY & VariableCode.__LOWERCASE__)][0]; }
		//	set { varData.DataIntegerArray[(int)(VariableCode.MONEY & VariableCode.__LOWERCASE__)][0] = value; }
		//}

		//private Int64 BOUGHT
		//{
		//	//get { return varData.DataIntegerArray[(int)(VariableCode.BOUGHT & VariableCode.__LOWERCASE__)][0]; }
		//	set { varData.DataIntegerArray[(int)(VariableCode.BOUGHT & VariableCode.__LOWERCASE__)][0] = value; }
		//}
		#endregion

	}
}