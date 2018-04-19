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
    static readonly Dictionary<string, bool> name_monospaced_map = new Dictionary<string, bool>
    {
        {"MS Gothic", true},
        {"MS PGothic", false},
    };

    public static void SetDefaultFont(string fontname, bool monospaced)
    {
        default_font = GetFont(fontname);
        if(default_font == null)
        {
            default_fontname = "ＭＳ ゴシック";
            default_font = GetFont(default_fontname);
            default_monospaced = GetMonospaced(default_fontname);
        }
        else
        {
            default_fontname = fontname;
            default_monospaced = monospaced;
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

        LoadMonospaced(path);
        return LoadFont(path);
    }

    public static bool GetMonospaced(string name)
    {
        if(string.IsNullOrEmpty(name))
            return default_monospaced;

        if(name == last_name)
            return last_monospaced;
        last_name = name;

        string path = null;
        name_path_map.TryGetValue(name, out path);

        LoadFont(path);
        return LoadMonospaced(path);
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
    static bool LoadMonospaced(string path)
    {
        last_monospaced = default_monospaced;
        if(string.IsNullOrEmpty(path))
            return last_monospaced;
        else if(!name_monospaced_map.TryGetValue(path, out last_monospaced))
            last_monospaced = default_monospaced;
        return last_monospaced;
    }

    public static string last_name = null;
    public static Font last_font = null;
    public static bool last_monospaced = true;

    public static string default_fontname { get; private set; }
    public static Font default_font { get; private set; }
    public static bool default_monospaced { get; private set; }

    static Dictionary<string, Font> font_map = new Dictionary<string, Font>();
}
