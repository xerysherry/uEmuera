using System.Collections.Generic;

namespace uEmuera.VisualBasic
{
    public enum VbStrConv
    {
        None = 0,
        Uppercase = 1,
        Lowercase = 2,
        ProperCase = 3,
        Wide = 4,
        Narrow = 8,
        Katakana = 16,
        Hiragana = 32,
        SimplifiedChinese = 256,
        TraditionalChinese = 512,
        LinguisticCasing = 1024
    }

    public static class Strings
    {
        public static readonly Dictionary<char, char> ToNarrow = 
            new Dictionary<char, char>
        {
                {'０', '0'},
                {'１', '1'},
                {'２', '2'},
                {'３', '3'},
                {'４', '4'},
                {'５', '5'},
                {'６', '6'},
                {'７', '7'},
                {'８', '8'},
                {'９', '9'},
                {'ａ', 'a'},
                {'ｂ', 'b'},
                {'ｃ', 'c'},
                {'ｄ', 'd'},
                {'ｅ', 'e'},
                {'ｆ', 'f'},
                {'ｇ', 'g'},
                {'ｈ', 'h'},
                {'ｉ', 'i'},
                {'ｊ', 'j'},
                {'ｋ', 'k'},
                {'ｌ', 'l'},
                {'ｍ', 'm'},
                {'ｎ', 'n'},
                {'ｏ', 'o'},
                {'ｐ', 'p'},
                {'ｑ', 'q'},
                {'ｒ', 'r'},
                {'ｓ', 's'},
                {'ｔ', 't'},
                {'ｕ', 'u'},
                {'ｖ', 'v'},
                {'ｗ', 'w'},
                {'ｘ', 'x'},
                {'ｙ', 'y'},
                {'ｚ', 'z'},
                {'Ａ', 'A'},
                {'Ｂ', 'B'},
                {'Ｃ', 'C'},
                {'Ｄ', 'D'},
                {'Ｅ', 'E'},
                {'Ｆ', 'F'},
                {'Ｇ', 'G'},
                {'Ｈ', 'H'},
                {'Ｉ', 'I'},
                {'Ｊ', 'J'},
                {'Ｋ', 'K'},
                {'Ｌ', 'L'},
                {'Ｍ', 'M'},
                {'Ｎ', 'N'},
                {'Ｏ', 'O'},
                {'Ｐ', 'P'},
                {'Ｑ', 'Q'},
                {'Ｒ', 'R'},
                {'Ｓ', 'S'},
                {'Ｔ', 'T'},
                {'Ｕ', 'U'},
                {'Ｖ', 'V'},
                {'Ｗ', 'W'},
                {'Ｘ', 'X'},
                {'Ｙ', 'Y'},
                {'Ｚ', 'Z'},
                {'　', ' '},
        };
        public static readonly Dictionary<char, char> ToWide =
            new Dictionary<char, char>
        {
                {'0', '０'},
                {'1', '１'},
                {'2', '２'},
                {'3', '３'},
                {'4', '４'},
                {'5', '５'},
                {'6', '６'},
                {'7', '７'},
                {'8', '８'},
                {'9', '９'},
                {'a', 'ａ'},
                {'b', 'ｂ'},
                {'c', 'ｃ'},
                {'d', 'ｄ'},
                {'e', 'ｅ'},
                {'f', 'ｆ'},
                {'g', 'ｇ'},
                {'h', 'ｈ'},
                {'i', 'ｉ'},
                {'j', 'ｊ'},
                {'k', 'ｋ'},
                {'l', 'ｌ'},
                {'m', 'ｍ'},
                {'n', 'ｎ'},
                {'o', 'ｏ'},
                {'p', 'ｐ'},
                {'q', 'ｑ'},
                {'r', 'ｒ'},
                {'s', 'ｓ'},
                {'t', 'ｔ'},
                {'u', 'ｕ'},
                {'v', 'ｖ'},
                {'w', 'ｗ'},
                {'x', 'ｘ'},
                {'y', 'ｙ'},
                {'z', 'ｚ'},
                {'A', 'Ａ'},
                {'B', 'Ｂ'},
                {'C', 'Ｃ'},
                {'D', 'Ｄ'},
                {'E', 'Ｅ'},
                {'F', 'Ｆ'},
                {'G', 'Ｇ'},
                {'H', 'Ｈ'},
                {'I', 'Ｉ'},
                {'J', 'Ｊ'},
                {'K', 'Ｋ'},
                {'L', 'Ｌ'},
                {'M', 'Ｍ'},
                {'N', 'Ｎ'},
                {'O', 'Ｏ'},
                {'P', 'Ｐ'},
                {'Q', 'Ｑ'},
                {'R', 'Ｒ'},
                {'S', 'Ｓ'},
                {'T', 'Ｔ'},
                {'U', 'Ｕ'},
                {'V', 'Ｖ'},
                {'W', 'Ｗ'},
                {'X', 'Ｘ'},
                {'Y', 'Ｙ'},
                {'Z', 'Ｚ'},
                {' ', '　'},
        };

        public static string StrConv(string str, VbStrConv Conversion, int LocaleID = 0)
        {
            switch(Conversion)
            {
            case VbStrConv.Wide:
                {
                    var result = "";
                    for(int i = 0; i < str.Length; ++i)
                    {
                        char c = str[i];
                        char found = '\x0';
                        if(ToWide.TryGetValue(c, out found))
                            result += found;
                        else
                        {
                            result += ' ';
                            result += c;
                        }
                    }
                    return result;
                }
            case VbStrConv.Narrow:
                {
                    var result = "";
                    for(int i = 0; i < str.Length; ++i)
                    {
                        char c = str[i];
                        char found = '\x0';
                        if(ToNarrow.TryGetValue(c, out found))
                            result += found;
                        else
                            result += c;
                    }
                    return result;
                }
            }
            return str;
        }
    }
}