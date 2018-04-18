using System;
using System.Collections.Generic;
using System.Text;

namespace MinorShift.Emuera.GameData.Expression
{
	internal enum OperatorCode
	{
		
		//単項 > "*/%" > "+-" > 比較                > ビット演算  > 論理演算 > 代入
		//xx   > 90    > 80   > 65,60(不等価優先)   > 50          > 40       > xx
		//優先順は本家に準拠
		NULL = 0,
		__PRIORITY_MASK__ = 0xFF,
		__UNARY__ = 0x10000,//単項
		__BINARY__ =  0x20000,//2項
        __TERNARY__ = 0x40000,//3項
		__UNARY_AFTER__ = 0x80000,//後置単項
		Plus = 0x0100 + 0x80 | __UNARY__ | __BINARY__,//"+"単項可
		Minus = 0x0200 + 0x80 | __UNARY__ | __BINARY__,//"-"
		Mult = 0x0300 + 0x90 | __BINARY__,//"*"
		Div = 0x0400 + 0x90 | __BINARY__,//"/"
		Mod = 0x0500 + 0x90 | __BINARY__,//"%"
		Equal = 0x0600 + 0x60 | __BINARY__,//"=="
		Greater = 0x0700 + 0x65 | __BINARY__,//">"
		Less = 0x0800 + 0x65 | __BINARY__,//"<"
		GreaterEqual = 0x0900 + 0x65 | __BINARY__,//">="
		LessEqual = 0x0A00 + 0x65 | __BINARY__,//"<="
		NotEqual = 0x0B00 + 0x60 | __BINARY__,//"!="
        Increment = 0x2000 | __UNARY__ | __UNARY_AFTER__,//"++"
        Decrement = 0x2100 | __UNARY__ | __UNARY_AFTER__,//"--"

		And = 0x0C00 + 0x40 | __BINARY__,//"&&"
		Or = 0x0D00 + 0x40 | __BINARY__,//"||"
        Xor = 0x1500 + 0x40 | __BINARY__,//"^^"
        Nand = 0x1600 + 0x40 | __BINARY__,//"!&"
        Nor = 0x1700 + 0x40 | __BINARY__,//"!^"
		BitAnd = 0x0E00 + 0x50 | __BINARY__,//"&"
		BitOr = 0x0F00 + 0x50 | __BINARY__,//"|"
		BitXor = 0x1000 + 0x50 | __BINARY__,//"^"、優先順位は&と|の中間。
		Not = 0x1100 | __UNARY__,//"!"単項
		BitNot = 0x1200 | __UNARY__,//"~"単項
		RightShift = 0x1300 + 0x70 | __BINARY__,//">>"
		LeftShift = 0x1400 + 0x70 | __BINARY__,//"<<"

        Ternary_a = 0x1800 + 0x05 | __TERNARY__,//"?"、三項演算子
        Ternary_b = 0x1900 + 0x10 | __TERNARY__,//"#"、三項演算子区切り":"が使えないのでかわり


		Assignment = 0x0100 + 0xFE,//"="
		AssignmentStr = 0x0200 + 0xFE,//"'="
	}
	internal static class OperatorManager
	{

		readonly static Dictionary<string, OperatorCode> opDictionary;
		static OperatorManager()
		{
		
			opDictionary = new Dictionary<string, OperatorCode>();
			opDictionary.Add("+", OperatorCode.Plus);
			opDictionary.Add("-", OperatorCode.Minus);
			opDictionary.Add("*", OperatorCode.Mult);
			opDictionary.Add("/", OperatorCode.Div);
			opDictionary.Add("%", OperatorCode.Mod);
			opDictionary.Add("==", OperatorCode.Equal);
			opDictionary.Add(">", OperatorCode.Greater);
			opDictionary.Add("<", OperatorCode.Less);
			opDictionary.Add(">=", OperatorCode.GreaterEqual);
			opDictionary.Add("<=", OperatorCode.LessEqual);
			opDictionary.Add("!=", OperatorCode.NotEqual);
			opDictionary.Add("&&", OperatorCode.And);
			opDictionary.Add("||", OperatorCode.Or);
            opDictionary.Add("^^", OperatorCode.Xor);
            opDictionary.Add("!&", OperatorCode.Nand);
            opDictionary.Add("!|", OperatorCode.Nor);
            opDictionary.Add("&", OperatorCode.BitAnd);
			opDictionary.Add("|", OperatorCode.BitOr);
			opDictionary.Add("!", OperatorCode.Not);
			opDictionary.Add("^", OperatorCode.BitXor);
			opDictionary.Add("~", OperatorCode.BitNot);
            opDictionary.Add("?", OperatorCode.Ternary_a);
            opDictionary.Add("#", OperatorCode.Ternary_b);
			opDictionary.Add(">>", OperatorCode.RightShift);
			opDictionary.Add("<<", OperatorCode.LeftShift);
            opDictionary.Add("++", OperatorCode.Increment);
            opDictionary.Add("--", OperatorCode.Decrement);

			opDictionary.Add("=", OperatorCode.Assignment);
			opDictionary.Add("'=", OperatorCode.AssignmentStr);
		}

		public static string ToOperatorString(OperatorCode op)
		{
			if (op == OperatorCode.NULL)
				return "";
			foreach(KeyValuePair<string, OperatorCode> pair in opDictionary)
			{
				if(op == pair.Value)
					return pair.Key;
			}
			return "";
		}

		public static bool IsUnary(OperatorCode type)
		{
			return ((type & OperatorCode.__UNARY__) == OperatorCode.__UNARY__);
		}
        public static bool IsUnaryAfter(OperatorCode type)
        {
            return ((type & OperatorCode.__UNARY_AFTER__) == OperatorCode.__UNARY_AFTER__);
        }
        public static bool IsBinary(OperatorCode type)
		{
			return ((type & OperatorCode.__BINARY__) == OperatorCode.__BINARY__);
		}
        public static bool IsTernary(OperatorCode type)
        {
            return ((type & OperatorCode.__TERNARY__) == OperatorCode.__TERNARY__);
        }

		/// <summary>
		/// 大きい方が優先度が高い。 '&' < '+' < '*'等
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static int GetPriority(OperatorCode type)
		{
			return (int)(type & OperatorCode.__PRIORITY_MASK__);
		}
	}
}
