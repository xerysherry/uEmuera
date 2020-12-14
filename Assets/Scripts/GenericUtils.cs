using UnityEngine;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class GenericUtils
{
    public static void Info(object content)
    {
        UnityEngine.Debug.Log(content);
    }
    public static void Warn(object content)
    {
        UnityEngine.Debug.LogWarning(content);
    }
    public static void Error(object content)
    {
        UnityEngine.Debug.LogError(content);
    }
    /// <summary>
    /// 获得子对象
    /// </summary>
    public static Component FindChildByName(System.Type type, GameObject obj,
                                            string childname, bool includeInactive = false)
    {
        if(!obj)
            return null;
        var list = obj.GetComponentsInChildren(type, includeInactive);
        var length = list.Length;
        for(int i=0; i<length; ++i)
        {
            var v = list[i];
            if(v.name.CompareTo(childname) == 0)
                return v;
        }
        return null;
    }
    /// <summary>
    /// 获取子对象列表
    /// </summary>
    public static List<T> FindChildren<T>(GameObject obj, bool includeInactive = false)
        where T : Component
    {
        var result_list = new List<T>();
        if(!obj)
            return result_list;

        var list = obj.GetComponentsInChildren<Transform>(includeInactive);
        var length = list.Length;
        for(int i = 0; i < length; ++i)
        {
            var v = list[i];
            var c = v.GetComponent<T>();
            if(c)
                result_list.Add(c);
        }

        return result_list;
    }
    /// <summary>
    /// 获得子对象
    /// </summary>
    public static T FindChildByName<T>(GameObject obj, string childname, bool includeInactive = false) where T : Component
    {
        if(!obj)
            return null;
        var list = obj.GetComponentsInChildren<T>(includeInactive);
        var length = list.Length;
        for(int i = 0; i < length; ++i)
        {
            var v = list[i];
            if(v.name.CompareTo(childname) == 0)
                return v;
        }
        return null;
    }
    /// <summary>
    /// 获得子对象
    /// </summary>
    public static GameObject FindChildByName(GameObject obj, string childname, bool includeInactive = false)
    {
        if(!obj)
            return null;
        var list = obj.GetComponentsInChildren<Transform>(includeInactive);
        var length = list.Length;
        for(int i = 0; i < length; ++i)
        {
            var v = list[i];
            if(v.name.CompareTo(childname) == 0)
                return v.gameObject;
        }
        return null;
    }
    public static Transform Get(string path)
    {
        var str = path.Split('.');
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var objs = scene.GetRootGameObjects();

        Transform t = null;
        for(int i = 0; i < objs.Length; ++i)
        {
            if(string.CompareOrdinal(objs[i].name, str[0]) == 0)
            {
                t = objs[i].transform;
                break;
            }
        }
        if(t == null)
            return null;

        Transform nt = null;
        int si = 1;
        while(si < str.Length)
        {
            var n = str[si];
            var c = t.childCount;
            for(int i = 0; i < c; ++i)
            {
                var o = t.GetChild(i);
                if(string.CompareOrdinal(o.name, n) == 0)
                {
                    nt = o;
                    break;
                }
            }
            if(nt == null)
                return null;
            t = nt;
            nt = null;
            si += 1;
        }
        return t;
    }
    /// <summary>
    /// 获得文件名
    /// ex. FolderA/FolderB/Filename -> Filename
    /// </summary>
    /// <param name="fullname"></param>
    /// <returns></returns>
    public static string GetFilename(string fullname)
    {
        int last_slash = fullname.LastIndexOf('/');
        if(last_slash != -1)
            return fullname.Substring(last_slash + 1);
        return fullname;
    }

    public static UnityEngine.Color ToUnityColor(uEmuera.Drawing.Color color)
    {
        return new UnityEngine.Color(color.r, color.g, color.b, color.a);
    }
    public static string GetColorCode(UnityEngine.Color color)
    {
        return GetColorCode(new uEmuera.Drawing.Color(color.r, color.g, color.b, color.a));
    }
    public static string GetColorCode(uEmuera.Drawing.Color color)
    {
        return string.Format("{0:x8}", color.ToRGBA());
    }
    public static Rect ToUnityRect(uEmuera.Drawing.Rectangle rect)
    {
        return new Rect(rect.X, rect.Y, rect.Width, rect.Height);
    }
    public static Rect ToUnityRect(uEmuera.Drawing.Rectangle rect, int width, int height)
    {
        var x = rect.X;
        var y = height - rect.Height - rect.Y;
        var w = rect.Width;
        if(x + w > width)
            w = width - x;
        var h = rect.Height;
        if(y + h > height)
            h = height - y;
        return new Rect(x, y, w, h);
    }

    public class PointerClickListener : MonoBehaviour, IPointerClickHandler
    {
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            var _callbacks1 = callbacks1;
            var iter = _callbacks1.GetEnumerator();
            while(iter.MoveNext())
                iter.Current();

            var _callbacks2 = callbacks2;
            var iter2 = _callbacks2.GetEnumerator();
            while(iter2.MoveNext())
                iter2.Current(eventData);
        }
        void OnDestroy()
        {
            callbacks1.Clear();
            callbacks2.Clear();
        }
        public HashSet<Action> callbacks1 = new HashSet<Action>();
        public HashSet<Action<PointerEventData>> callbacks2 = new HashSet<Action<PointerEventData>>();
    }
    /// <summary>
    /// 设置OnClick回调
    /// </summary>
    /// <param name="obj">设置回调的目标UI</param>
    /// <param name="callback">回调函数</param>
    public static void SetListenerOnClick(GameObject obj, Action callback)
    {
        if(!obj || callback == null)
            return;
        var l = obj.GetComponent<PointerClickListener>();
        if(!l)
            l = obj.AddComponent<PointerClickListener>();
        l.callbacks1.Add(callback);
    }
    public static void SetListenerOnClick(GameObject obj, Action<PointerEventData> callback)
    {
        if(!obj || callback == null)
            return;
        var l = obj.GetComponent<PointerClickListener>();
        if(!l)
            l = obj.AddComponent<PointerClickListener>();
        l.callbacks2.Add(callback);
    }
    public static void RemoveListenerOnClick(GameObject obj, Action callback)
    {
        if(!obj || callback == null)
            return;
        var l = obj.GetComponent<PointerClickListener>();
        if(!l)
            return;
        l.callbacks1.Remove(callback);
    }
    public static void RemoveListenerOnClick(GameObject obj, Action<PointerEventData> callback)
    {
        if(!obj || callback == null)
            return;
        var l = obj.GetComponent<PointerClickListener>();
        if(!l)
            return;
        l.callbacks2.Remove(callback);
    }
    public static void RemoveListenerOnClick(GameObject obj)
    {
        if(!obj)
            return;
        var l = obj.GetComponent<PointerClickListener>();
        if(!l)
            return;
        l.callbacks1 = new HashSet<Action>();
        l.callbacks2 = new HashSet<Action<PointerEventData>>();
    }

    class PointerDownListener : MonoBehaviour, IPointerDownHandler
    {
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            var iter = callbacks.GetEnumerator();
            while(iter.MoveNext())
                iter.Current(eventData);
        }
        void OnDestroy()
        {
            callbacks.Clear();
        }
        public HashSet<Action<PointerEventData>> callbacks = new HashSet<Action<PointerEventData>>();
    }
    public static void SetListenerOnPointerDown(GameObject obj, Action<PointerEventData> callback)
    {
        if(!obj || callback == null)
            return;
        var l = obj.GetComponent<PointerDownListener>();
        if(!l)
            l = obj.AddComponent<PointerDownListener>();
        l.callbacks.Add(callback);
    }
    public static void RemoveListenerOnPointerDown(GameObject obj, Action<PointerEventData> callback)
    {
        if(!obj || callback == null)
            return;
        var l = obj.GetComponent<PointerDownListener>();
        if(!l)
            return;
        l.callbacks.Remove(callback);
    }
    public static void RemoveListenerOnPointerDown(GameObject obj)
    {
        if(!obj)
            return;
        var l = obj.GetComponent<PointerDownListener>();
        if(!l)
            return;
        l.callbacks.Clear();
    }

    class PointerUpListener : MonoBehaviour, IPointerUpHandler
    {
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            var iter = callbacks.GetEnumerator();
            while(iter.MoveNext())
                iter.Current(eventData);
        }
        void OnDestroy()
        {
            callbacks.Clear();
        }
        public HashSet<Action<PointerEventData>> callbacks = new HashSet<Action<PointerEventData>>();
    }
    public static void SetListenerOnPointerUp(GameObject obj, Action<PointerEventData> callback)
    {
        if(!obj || callback == null)
            return;
        var l = obj.GetComponent<PointerUpListener>();
        if(!l)
            l = obj.AddComponent<PointerUpListener>();
        l.callbacks.Add(callback);
    }
    public static void RemoveListenerOnPointerUp(GameObject obj, Action<PointerEventData> callback)
    {
        if(!obj || callback == null)
            return;
        var l = obj.GetComponent<PointerUpListener>();
        if(!l)
            return;
        l.callbacks.Remove(callback);
    }
    public static void RemoveListenerOnPointerUp(GameObject obj)
    {
        if(!obj)
            return;
        var l = obj.GetComponent<PointerUpListener>();
        if(!l)
            return;
        l.callbacks.Clear();
    }

    //监听类
    class BeginDragListener : MonoBehaviour, IBeginDragHandler
    {
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            var iter = callbacks.GetEnumerator();
            while(iter.MoveNext())
                iter.Current(eventData);
        }
        void OnDestroy()
        {
            callbacks.Clear();
        }
        public HashSet<Action<PointerEventData>> callbacks = new HashSet<Action<PointerEventData>>();
    }
    class DragListener : MonoBehaviour, IDragHandler
    {
        public virtual void OnDrag(PointerEventData eventData)
        {
            var iter = callbacks.GetEnumerator();
            while(iter.MoveNext())
                iter.Current(eventData);
        }
        void OnDestroy()
        {
            callbacks.Clear();
        }
        public HashSet<Action<PointerEventData>> callbacks = new HashSet<Action<PointerEventData>>();
    }
    class EndDragListener : MonoBehaviour, IEndDragHandler
    {
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            var iter = callbacks.GetEnumerator();
            while(iter.MoveNext())
                iter.Current(eventData);
        }
        void OnDestroy()
        {
            callbacks.Clear();
        }
        public HashSet<Action<PointerEventData>> callbacks = new HashSet<Action<PointerEventData>>();
    }

    /// <summary>
    /// 设置OnDrag回调
    /// </summary>
    /// <param name="obj">设置回调的目标UI</param>
    /// <param name="callback">回调函数</param>
    public static void SetListenerOnDrag(GameObject obj, Action<PointerEventData> callback)
    {
        if(!obj || callback == null)
            return;
        var l = obj.GetComponent<DragListener>();
        if(!l)
            l = obj.AddComponent<DragListener>();
        l.callbacks.Add(callback);
    }
    public static void RemoveListenerOnDrag(GameObject obj, Action<PointerEventData> callback)
    {
        if(!obj || callback == null)
            return;
        var l = obj.GetComponent<DragListener>();
        if(!l)
            return;
        l.callbacks.Remove(callback);
    }
    public static void RemoveListenerOnDrag(GameObject obj)
    {
        if(!obj)
            return;
        var l = obj.GetComponent<DragListener>();
        if(!l)
            return;
        l.callbacks.Clear();
    }
    /// <summary>
    /// 设置OnBeginDrag回调
    /// </summary>
    /// <param name="obj">设置回调的目标UI</param>
    /// <param name="callback">回调函数</param>
    public static void SetListenerOnBeginDrag(GameObject obj, Action<PointerEventData> callback)
    {
        if(!obj || callback == null)
            return;
        var l = obj.GetComponent<BeginDragListener>();
        if(!l)
            l = obj.AddComponent<BeginDragListener>();
        l.callbacks.Add(callback);
    }
    public static void RemoveListenerOnBeginDrag(GameObject obj, Action<PointerEventData> callback)
    {
        if(!obj || callback == null)
            return;
        var l = obj.GetComponent<BeginDragListener>();
        if(!l)
            return;
        l.callbacks.Remove(callback);
    }
    public static void RemoveListenerOnBeginDrag(GameObject obj)
    {
        if(!obj)
            return;
        var l = obj.GetComponent<BeginDragListener>();
        if(!l)
            return;
        l.callbacks.Clear();
    }
    /// <summary>
    /// 设置OnEndDrag回调
    /// </summary>
    /// <param name="obj">设置回调的目标UI</param>
    /// <param name="callback">回调函数</param>
    public static void SetListenerOnEndDrag(GameObject obj, Action<PointerEventData> callback)
    {
        if(!obj || callback == null)
            return;
        var l = obj.GetComponent<EndDragListener>();
        if(!l)
            l = obj.AddComponent<EndDragListener>();
        l.callbacks.Add(callback);
    }
    public static void RemoveListenerOnEndDrag(GameObject obj, Action<PointerEventData> callback)
    {
        if(!obj || callback == null)
            return;
        var l = obj.GetComponent<EndDragListener>();
        if(!l)
            return;
        l.callbacks.Remove(callback);
    }
    public static void RemoveListenerOnEndDrag(GameObject obj)
    {
        if(!obj)
            return;
        var l = obj.GetComponent<EndDragListener>();
        if(!l)
            return;
        l.callbacks.Clear();
    }

    static string CalcMd5(byte[] data, int offset, int count)
    {
        using (var md5 = new MD5CryptoServiceProvider())
        {
            md5.Initialize();
            var md5data = md5.ComputeHash(data, offset, count);
            return ToMd5String(md5data);
        }
    }

    static string ToMd5String(byte[] data)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < data.Length; ++i)
        {
            sb.AppendFormat("{0:X2}", data[i]);
        }
        return sb.ToString();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static List<string> CalcMd5List(byte[] data)
    {
        var md5s = new List<string>();

        int start = 0;
        int count = 0;

        do
        {
            while (data[start + count] != '\xd' &&
                    data[start + count] != '\xa')
            {
                count += 1;
                if (start + count >= data.Length)
                    break;
            }
            md5s.Add(CalcMd5(data, start, count));

            if (start + count >= data.Length)
                break;

            start += count;
            count = 0;
            while (data[start] == '\xd' ||
                    data[start] == '\xa')
            {
                start += 1;
            }

        } while (data[start] != 0);

        return md5s;
    }
    /// <summary>
    /// 处理中间的‘：’
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static List<string> CalcMd5ListForConfig(byte[] data)
    {
        var md5s = new List<string>();

        int start = 0;
        int count = 0;

        do
        {
            while (data[start + count] != ':')
            {
                count += 1;
            }
            md5s.Add(CalcMd5(data, start, count));

            start += count;
            count = 0;

            while (data[start] != '\xd' &&
                    data[start] != '\xa')
            {
                start += 1;
                if (start >= data.Length)
                    break;
            }
            if (start >= data.Length)
                break;

            while (data[start] == '\xd' ||
                    data[start] == '\xa')
            {
                start += 1;

                if (start >= data.Length)
                    break;
            }

            if (start >= data.Length)
                break;

        } while (data[start] != 0);

        return md5s;
    }

    //==========================================================================
    class CoroutineHelper : MonoBehaviour
    {
        public static CoroutineHelper instance
        {
            get
            {
                if(!instance_)
                {
                    var obj = new GameObject();
                    obj.name = "CoroutineHelper";
                    instance_ = obj.AddComponent<CoroutineHelper>();
                }
                return instance_;
            }
        }
        static CoroutineHelper instance_ = null;

        public Coroutine DoCoroutine(System.Collections.IEnumerator e)
        {
            return StartCoroutine(e);
        }
    }
    /// <summary>
    /// 开启协程，方便在非MonoBehaviour对象中使用协程
    /// </summary>
    public static Coroutine StartCoroutine(System.Collections.IEnumerator e)
    {
        return CoroutineHelper.instance.DoCoroutine(e);
    }
    public static void StopAllCoroutines()
    {
        CoroutineHelper.instance.StopAllCoroutines();
    }
    public static void StopCoroutine(Coroutine co)
    {
        CoroutineHelper.instance.StopCoroutine(co);
    }

    public static void SetBackgroundColor(uEmuera.Drawing.Color color)
    {
        text_content.SetBackgroundColor(color);
    }
    public static void ClearText()
    {
        //text_content.Clear();
        text_content.RemoveLine(text_content.max_log_count);
    }
    public static void AddText(object console_line, bool roll_to_bottom)
    {
        text_content.AddLine(console_line, roll_to_bottom);
    }
    public static object GetText(int index)
    {
        return text_content.GetLine(index);
    }
    public static int GetTextCount()
    {
        return text_content.GetLineCount();
    }
    public static int GetTextMaxLineNo()
    {
        return text_content.GetMaxLineNo();
    }
    public static int GetTextMinLineNo()
    {
        return text_content.GetMinLineNo();
    }
    public static void RemoveTextCount(int count)
    {
        text_content.RemoveLine(count);
    }
    public static void ToBottom()
    {
        text_content.ToBottom();
    }
    public static void TextUpdate()
    {
        text_content.Update();
    }
    public static void SetLastButtonGeneration(int generation)
    {
        text_content.SetLastButtonGeneration(generation);
    }
    public static void ShowIsInProcess(bool value)
    {
        text_content.ShowIsInProcess(value);
    }
    static EmueraContent text_content
    {
        get
        {
            if(_text_content == null)
            {
                _text_content = EmueraContent.instance;
            }
            return _text_content;
        }
    }
    static EmueraContent _text_content = null;
}
