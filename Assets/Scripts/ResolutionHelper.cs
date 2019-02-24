using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResolutionHelper
{
    public static readonly string kResolutionIndex = "ResolutionIndex";
    /// <summary>
    /// 分辨率列表
    /// </summary>
    public static List<int> resolutions = new List<int>
    {
        2160,
        1080,
        900,
        720,
        540,
    };

    public static void Apply()
    {
        if(resolution_index < 0)
        {
            var current_height = Screen.height;
            if(current_height >= 1000)
                resolution_index = 1;
            else if(current_height >= 900)
                resolution_index = 2;
            else
                resolution_index = 3;
        }
        ApplyResolution();
    }

    static void ApplyResolution()
    {
        var height = resolutions[resolution_index];
        var width = (int)Mathf.Ceil(height * aspect);
        if(Screen.width > Screen.height)
            Screen.SetResolution(width, height, true, 24);
        else
            Screen.SetResolution(height, width, true, 24);
    }

    static float aspect {
        get
        {
            if(_aspect < 0.0f)
            {
                _aspect = Screen.width * 1.0f / Screen.height;
                if(_aspect < 1)
                    _aspect = 1 / _aspect;
            }
            return _aspect;
        }
    }
    static float _aspect = -1.0f;

    public static int resolution_index
    {
        get
        {
            if(_resolution_index < 0)
                _resolution_index = PlayerPrefs.GetInt(kResolutionIndex, -1);
            return _resolution_index;
        }
        set
        {
            _resolution_index = value;
            if(_resolution_index < 0)
                return;
            PlayerPrefs.SetInt(kResolutionIndex, _resolution_index);
        }
    }
    static int _resolution_index = -1;
}
