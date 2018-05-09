using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MinorShift.Emuera;
using MinorShift._Library;

public class MainEntry : MonoBehaviour
{
    void Awake()
    {
        first_window_ = GameObject.Find("FirstWindow");
        scroll_rect_ = GenericUtils.FindChildByName<UnityEngine.UI.ScrollRect>(first_window_, "ScrollRect");
        item_ = GenericUtils.FindChildByName(first_window_, "Item", true);
        GenericUtils.FindChildByName<UnityEngine.UI.Text>(first_window_, "version")
            .text = Application.version + " ";
    }

    void Start()
    {
        Application.targetFrameRate = 24;
        LoadConfigMaps();

#if UNITY_EDITOR
        uEmuera.Logger.info = GenericUtils.Info;
        uEmuera.Logger.warn = GenericUtils.Warn;
        uEmuera.Logger.error = GenericUtils.Error;
#endif

        emuera = GameObject.FindObjectOfType<EmueraMain>();
        GetList(Application.persistentDataPath);
#if UNITY_EDITOR
        if(!string.IsNullOrEmpty(era_path))
            GetList(era_path);
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        GetList("storage/emulated/0/emuera");
        GetList("storage/emulated/1/emuera");
        GetList("storage/emulated/2/emuera");
#endif
    }

#if UNITY_EDITOR
    public string era_path;
#endif
    EmueraMain emuera;

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

    void GetList(string workspace)
    {
        workspace = uEmuera.Utils.NormalizePath(workspace);
        try
        {
            var paths = Directory.GetDirectories(workspace, "*", SearchOption.TopDirectoryOnly);
            foreach(var p in paths)
            {
                var path = uEmuera.Utils.NormalizePath(p);
                if(File.Exists(path + "/emuera.config") || Directory.Exists(path + "/ERB"))
                    AddItem(path.Substring(workspace.Length + 1), workspace);
            }
        }
        catch(DirectoryNotFoundException e)
        { }
    }

    void AddItem(string folder, string workspace)
    {
        var rrt = item_.transform as UnityEngine.RectTransform;
        var obj = GameObject.Instantiate(item_);
        var text = GenericUtils.FindChildByName<UnityEngine.UI.Text>(obj, "name");
        text.text = folder;
        text = GenericUtils.FindChildByName<UnityEngine.UI.Text>(obj, "path");
        text.text = workspace + "/" + folder;

        GenericUtils.SetListenerOnClick(obj, () => 
        {
            GameObject.Destroy(first_window_);
            scroll_rect_ = null;
            item_ = null;
            //Start Game
            GenericUtils.StartCoroutine(Run(workspace, folder));
        });

        var rt = obj.transform as UnityEngine.RectTransform;
        var content = scroll_rect_.content;
        rt.SetParent(content);
        rt.localScale = Vector3.one;
        rt.anchorMax = rrt.anchorMax;
        rt.anchorMin = rrt.anchorMin;
        rt.offsetMax = rrt.offsetMax;
        rt.offsetMin = rrt.offsetMin;
        rt.sizeDelta = rrt.sizeDelta;
        rt.localPosition = new Vector2(0, -rt.sizeDelta.y * itemcount_);
        itemcount_ += 1;

        var ih = rt.sizeDelta.y * itemcount_;
        if(ih > content.sizeDelta.y)
        {
            content.sizeDelta = new Vector2(content.sizeDelta.x, ih);
        }
        obj.SetActive(true);
    }

    System.Collections.IEnumerator Run(string workspace, string era)
    {
        var async = Resources.UnloadUnusedAssets();
        while(!async.isDone)
            yield return null;

        var ow = EmueraContent.instance.option_window;
        ow.gameObject.SetActive(true);
        ow.ShowGameButton(true);
        ow.ShowInProgress(true);
        yield return null;

        System.GC.Collect();
        SpriteManager.Init();

        Sys.SetWorkFolder(workspace);
        Sys.SetSourceFolder(era);
        uEmuera.Utils.ResourcePrepare();

        async = Resources.UnloadUnusedAssets();
        while(!async.isDone)
            yield return null;

        emuera.Run();
    }

    GameObject first_window_ = null;
    UnityEngine.UI.ScrollRect scroll_rect_ = null;
    GameObject item_ = null;
    int itemcount_ = 0;
}
