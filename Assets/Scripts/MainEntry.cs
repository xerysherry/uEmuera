using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MinorShift.Emuera;
using MinorShift._Library;

public class MainEntry : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 24;
        LoadConfigMaps();

#if UNITY_EDITOR
        uEmuera.Logger.info = GenericUtils.Info;
        uEmuera.Logger.warn = GenericUtils.Warn;
        uEmuera.Logger.error = GenericUtils.Error;
#endif
    }

#if UNITY_EDITOR
    public string era_path;
#endif

    void LoadConfigMaps()
    {
        char[] split = new char[] { '\x0d', '\x0a' };
        var shiftjis = Resources.Load<TextAsset>("Text/emuera_config_shiftjis");
        if(shiftjis == null)
            return;
        var utf8 = Resources.Load<TextAsset>("Text/emuera_config_utf8");
        if(utf8 == null)
            return;

        var jis_text = System.Text.Encoding.UTF8.GetString(shiftjis.bytes);
        var jis_strs = jis_text.Split(split);
        var utf8_strs = utf8.text.Split(split);
        if(jis_strs.Length != utf8_strs.Length)
            return;

        Dictionary<string, string> jis_map = new Dictionary<string, string>();
        for(int i = 0; i < jis_strs.Length; ++i)
        {
            jis_map[jis_strs[i]] = utf8_strs[i];
        }
        uEmuera.Utils.SetSHIFTJIS_to_UTF8Dict(jis_map);
    }
}
