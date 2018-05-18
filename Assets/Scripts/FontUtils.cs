using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FontUtils
{
    static readonly Dictionary<string, string> name_path_map = new Dictionary<string, string>
    {
        {"ＭＳ ゴシック", "MS Gothic"},
        {"MS Gothic", "MS Gothic"},
        {"ＭＳ Ｐゴシック", "MS PGothic"},
        {"MS PGothic", "MS PGothic"},
    };

    public static void SetDefaultFont(string fontname)
    {
        default_font = GetFont(fontname);
        if(default_font == null)
        {
            default_fontname = "ＭＳ ゴシック";
            default_font = GetFont(default_fontname);
        }
        else
        {
            default_fontname = fontname;
        }
    }

    public static Font GetFont(string name)
    {
        if(string.IsNullOrEmpty(name))
            return default_font;

        if(name == last_name)
            return last_font;
        last_name = name;

        string path = null;
        name_path_map.TryGetValue(name, out path);

        return LoadFont(path);
    }
    static Font LoadFont(string path)
    {
        if(string.IsNullOrEmpty(path))
            last_font = default_font;
        else if(!font_map.TryGetValue(path, out last_font))
        {
            last_font = Resources.Load<Font>("Fonts/" + path);
            if(last_font == null)
                last_font = default_font;
            font_map[path] = last_font;
        }
        return last_font;
    }

    public static string last_name = null;
    public static Font last_font = null;

    public static string default_fontname { get; private set; }
    public static Font default_font { get; private set; }

    static Dictionary<string, Font> font_map = new Dictionary<string, Font>();
}
