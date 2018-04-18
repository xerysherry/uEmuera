using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

public class ToolKit : EditorWindow 
{
    [MenuItem("Tools/ToolKit", false, 100)]
    public static void Show()
    {
        EditorWindow.GetWindow(typeof(ToolKit));
    }

    void OnGUI()
    {
        some_utils_tool_.OnGUI();
        show_system_info_.OnGUI();
    }

    class SomeUtilsTool
    {
        public void OnGUI()
        {
            fold_out_ = EditorGUILayout.Foldout(fold_out_, "SomeUtilsTool");
            if(!fold_out_)
                return;

            if(GUILayout.Button("Open persistentDataPath"))
            {
                System.Diagnostics.Process.Start(Application.persistentDataPath);
            }
            if(GUILayout.Button("Open dataPath"))
            {
                System.Diagnostics.Process.Start(Application.dataPath);
            }
            var newkey = EditorGUILayout.TextField("Player Prefs Key:", key);
            if(newkey != key)
            {
                key = newkey;
                text = PlayerPrefs.GetString(newkey, "");
            }
            var ptext = EditorGUILayout.TextField(text);
            if(ptext != text)
            {
                PlayerPrefs.SetString(key, ptext);
                text = ptext;
            }
            if(GUILayout.Button("Delete"))
            {
                PlayerPrefs.DeleteKey(key);
            }
        }
        bool fold_out_ = true;

        string key = "";
        string text = "";
    }
    SomeUtilsTool some_utils_tool_ = new SomeUtilsTool();

    class ShowSystemInfo
    {
        public void OnGUI()
        {
            fold_out_ = EditorGUILayout.Foldout(fold_out_, "ShowSystemInfo");
            if(!fold_out_)
                return;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("operatingSystem:");
            EditorGUILayout.LabelField(SystemInfo.operatingSystem);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("deviceName:");
            EditorGUILayout.LabelField(SystemInfo.deviceName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("deviceModel:");
            EditorGUILayout.LabelField(SystemInfo.deviceModel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("systemMemorySize:");
            EditorGUILayout.LabelField(SystemInfo.systemMemorySize.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("deviceType:");
            EditorGUILayout.LabelField(SystemInfo.deviceType.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("graphicsDeviceName:");
            EditorGUILayout.LabelField(SystemInfo.graphicsDeviceName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("graphicsMemorySize:");
            EditorGUILayout.LabelField(SystemInfo.graphicsMemorySize.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("maxTextureSize:");
            EditorGUILayout.LabelField(SystemInfo.maxTextureSize.ToString());
            EditorGUILayout.EndHorizontal();
        }

        bool fold_out_ = true;
    }
    ShowSystemInfo show_system_info_ = new ShowSystemInfo();
}
