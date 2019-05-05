using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

public static class MultiLanguageExport
{
    static string GetPath(Transform t)
    {
        var p = t.name;
        while(t.parent != null)
        {
            t = t.parent;
            p = string.Concat(t.name, ".", p);
        }
        return p;
    }

    [MenuItem("Tools/MultiLanguageExport", false, 100)]
    public static void Show()
    {
        var dir = Application.dataPath + "/Resources/Lang/default.txt";
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

        var objs = scene.GetRootGameObjects();
        var texts = new List<UnityEngine.UI.Text>();
        for(int i = 0; i < objs.Length; ++i)
        {
            Debug.Log(objs[i].name);
            texts.AddRange(objs[i].GetComponentsInChildren<UnityEngine.UI.Text>(true));
        }

        var sb = new StringBuilder();
        sb.AppendLine(";Default Language");
        for(int i = 0; i < texts.Count; ++i)
        {
            var t = texts[i];
            sb.AppendLine(string.Format("{0}={1}", GetPath(t.transform), t.text));
        }
        File.WriteAllText(dir, sb.ToString());
    }
}
