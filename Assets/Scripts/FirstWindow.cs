﻿using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MinorShift._Library;

public class FirstWindow : MonoBehaviour
{
    public static void Show()
    {
        var obj = Resources.Load<GameObject>("Prefab/FirstWindow");
        GameObject.Instantiate(obj);
    }
    static System.Collections.IEnumerator Run(string workspace, string era)
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

        EmueraContent.instance.SetNoReady();
        var emuera = GameObject.FindObjectOfType<EmueraMain>();
        emuera.Run();
    }

    void Start()
    {
        scroll_rect_ = GenericUtils.FindChildByName<ScrollRect>(gameObject, "ScrollRect");
        item_ = GenericUtils.FindChildByName(gameObject, "Item", true);
        setting_ = GenericUtils.FindChildByName(gameObject, "optionbtn");
        GenericUtils.SetListenerOnClick(setting_, OnOptionClick);

        GenericUtils.FindChildByName<Text>(gameObject, "version")
            .text = Application.version + " ";

        GetList(Application.persistentDataPath);
        if(Application.platform == RuntimePlatform.Android) {
            GetList("storage/emulated/0/emuera");
            GetList("storage/emulated/1/emuera");
            GetList("storage/emulated/2/emuera");
            GetList("storage/sdcard0/emuera");
            GetList("storage/sdcard1/emuera");
            GetList("storage/sdcard2/emuera");
        }
        else {
            GetList("game");
        }
    }

    void OnOptionClick()
    {
        var ow = EmueraContent.instance.option_window;
        ow.ShowMenu();
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
            scroll_rect_ = null;
            item_ = null;
            GameObject.Destroy(gameObject);
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

    void GetList(string workspace)
    {
        workspace = uEmuera.Utils.NormalizePath(workspace);
        if(!Directory.Exists(workspace))
            return;
        try
        {
            var paths = Directory.GetDirectories(workspace, "*", SearchOption.TopDirectoryOnly);
            print(paths);
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

    ScrollRect scroll_rect_ = null;
    GameObject item_ = null;
    GameObject setting_ = null;
    int itemcount_ = 0;
}
