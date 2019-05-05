using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MultiLanguage
{
    public static readonly char[] splits = new char[] { '\xa', '\xd' };
    static List<KeyValuePair<string, string>> Load(string lang)
    {
        var text = Resources.Load<TextAsset>(string.Concat("Lang/", lang));
        if(text == null)
            return null;

        var lines = text.text.Split(splits);
        List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

        for(int i = 0; i < lines.Length; ++i)
        {
            var l = lines[i].Trim();
            if(string.IsNullOrEmpty(l) || 
                string.CompareOrdinal(l, 0, ";", 0, 1) == 0)
                continue;
            var s = l.IndexOf('=');
            var left = l.Substring(0, s).Trim();
            var right = l.Substring(s + 1).Trim();
            if(string.IsNullOrEmpty(right))
                continue;

            list.Add(new KeyValuePair<string, string>(left, right));
        }
        return list;
    }
    public static bool SetLanguage()
    {
        var lang = PlayerPrefs.GetString("language", "");
        if(string.IsNullOrEmpty(lang))
            return false;
        SetLanguage(lang);
        return true;
    }
    public static void SetLanguage(string lang)
    {
        var list = Load(lang);
        if(list == null)
            return;

        float menu1 = 0;
        float menu2 = 0;
        for(int i = 0; i < list.Count; ++i)
        {
            var kv = list[i];
            var key = kv.Key;
            var value = kv.Value;
            if(key[0] == '<')
            {
                try
                {
                    var mv = key.Substring(1, key.Length - 2);
                    var fv = float.Parse(value);
                    SetMenuWidth(mv, fv);

                    if(mv == "Menu1")
                        menu1 = fv;
                    else if(mv == "Menu2")
                        menu2 = fv;
                }
                catch(System.Exception e)
                {
                }
            }
        }

        language_map = new Dictionary<string, string>();
        for(int i = 0; i < list.Count; ++i)
        {
            var kv = list[i];
            var key = kv.Key;
            var value = kv.Value;
            if(key[0] == '<')
                continue;
            else if(key[0] == '[')
            {
                language_map[key] = value;
            }
            var obj = GenericUtils.Get(key);
            if(obj == null)
                continue;
            var text = obj.GetComponent<UnityEngine.UI.Text>();
            text.text = value;

            if(string.CompareOrdinal(key, 0, "FirstWindow", 0, 11) == 0)
                FirstWindowTitlebar = value;

            float fv = 0;
            if(key.IndexOf("Menu1") >= 0)
                fv = menu1;
            else if(key.IndexOf("Menu2") >= 0)
                fv = menu2;
            if(fv <= 0.001f)
                continue;

            var rt = text.transform as RectTransform;
            rt.sizeDelta = new Vector2(fv - 52, rt.sizeDelta.y);
        }

        PlayerPrefs.SetString("language", lang);
    }
    public static string GetText(string key)
    {
        string v = null;
        language_map.TryGetValue(key, out v);
        if(string.IsNullOrEmpty(v))
            return key;
        else
            return v;
    }
    static void SetMenuWidth(string menu, float v)
    {
        var o = GenericUtils.Get("Options.MenuPad."+menu);
        if(o == null)
            return;
        var rt = o.transform as RectTransform;
        rt.sizeDelta = new Vector2(v, rt.sizeDelta.y);
    }
    static Dictionary<string, string> language_map = new Dictionary<string, string>();
    public static string FirstWindowTitlebar = null;
}
