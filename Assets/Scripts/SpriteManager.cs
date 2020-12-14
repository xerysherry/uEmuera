using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MinorShift.Emuera.Content;
using uEmuera.Drawing;
using WebP;

internal static class SpriteManager
{
    static float kPastTime = 300.0f;

    internal class SpriteInfo : IDisposable
    {
        internal SpriteInfo(TextureInfo p, Sprite s)
        {
            parent = p;
            sprite = s;
        }
        public void Dispose()
        {
            UnityEngine.Object.Destroy(sprite);
            sprite = null;
        }
        internal Sprite sprite;
        internal TextureInfo parent;
    }
    internal class TextureInfo : IDisposable
    {
        internal TextureInfo(string b, Texture2D tex)
        {
            imagename = b;
            texture = tex;
            pasttime = Time.unscaledTime + kPastTime;
        }
        internal SpriteInfo GetSprite(ASprite src)
        {
            SpriteInfo sprite = null;
            if(!sprites.TryGetValue(src.Name, out sprite))
            {
                sprite = new SpriteInfo(this, 
                    Sprite.Create(texture,
                        GenericUtils.ToUnityRect(src.Rectangle, texture.width, texture.height),
                        Vector2.zero)
                    );
                sprites[src.Name] = sprite;
            }
            if(sprite != null)
                refcount += 1;
            return sprite;
        }
        internal void Release()
        {
            refcount -= 1;
            pasttime = Time.unscaledTime + kPastTime;
        }
        public void Dispose()
        {
            var iter = sprites.Values.GetEnumerator();
            while(iter.MoveNext())
            {
                iter.Current.Dispose();
            }
            sprites.Clear();
            sprites = null;

            UnityEngine.Object.Destroy(texture);
            texture = null;
        }
        internal string imagename = null;
        internal int refcount = 0;
        internal float pasttime = 0;
        internal float width { get { return texture.width; } }
        internal float height { get { return texture.height; } }
        internal Texture2D texture = null;
        Dictionary<string, SpriteInfo> sprites = new Dictionary<string, SpriteInfo>();
    }
    class CallbackInfo
    {
        public CallbackInfo(ASprite src, object obj, 
                            Action<object, SpriteInfo> callback)
        {
            this.src = src;
            this.obj = obj;
            this.callback = callback;
        }
        public void DoCallback(SpriteInfo info)
        {
            callback(obj, info);
        }
        public ASprite src;
        object obj;
        Action<object, SpriteInfo> callback;
    }

    public static void Init()
    {
#if UNITY_EDITOR
        kPastTime = 300.0f;
        GenericUtils.StartCoroutine(Update());
        GenericUtils.StartCoroutine(UpdateRenderOP());
#else
        var memorysize = SystemInfo.systemMemorySize;
        if(memorysize <= 4096)
        {
            kPastTime = 300.0f;
            GenericUtils.StartCoroutine(Update());
            GenericUtils.StartCoroutine(UpdateRenderOP());
        }
        else if(memorysize <= 8192)
        {
            kPastTime = 600.0f;
            GenericUtils.StartCoroutine(Update());
            GenericUtils.StartCoroutine(UpdateRenderOP());
        }
        //else
        //{
            //
        //}
#endif
    }
    public static void GetSprite(ASprite src, 
                                object obj, Action<object, SpriteInfo> callback)
    {
        if(src == null || src.Bitmap == null)
        {
            if(callback != null)
                callback(null, null);
            return;
        }

        var basename = src.Bitmap.filename;
        TextureInfo ti = null;
        texture_dict.TryGetValue(basename, out ti);
        if(ti == null)
        {
            var item = new CallbackInfo(src, obj, callback);
            List<CallbackInfo> list = null;
            if(loading_set.TryGetValue(basename, out list))
                list.Add(item);
            else
            {
                list = new List<CallbackInfo> { item };
                loading_set.Add(basename, list);
                GenericUtils.StartCoroutine(Loading(src.Bitmap));
            }
        }
        else
            callback(obj, GetSpriteInfo(ti, src));
    }

    public static TextureInfo GetTextureInfo(string name, string filename)
    {
        TextureInfo ti = null;
        if(texture_dict.TryGetValue(name, out ti))
            return ti;
        if(string.IsNullOrEmpty(filename))
            return null;

        FileInfo fi = new FileInfo(filename);
        if(!fi.Exists)
            return null;

        FileStream fs = fi.OpenRead();
        var filesize = fs.Length;
        byte[] content = new byte[filesize];
        fs.Read(content, 0, (int)filesize);

        TextureFormat format = TextureFormat.DXT1;

        var extname = uEmuera.Utils.GetSuffix(filename).ToLower();
        if (extname == "png")
            format = TextureFormat.DXT5;

        if (extname == "webp")
        {
            var tex = Texture2DExt.CreateTexture2DFromWebP(content, false, false,
                out Error err);
            if (err != Error.Success)
            {
                Debug.LogWarning($"{filename} {err.ToString()}");
                return null;
            }
            ti = new TextureInfo(name, tex);
            texture_dict.Add(name, ti);
        }
        else
        {
            var tex = new Texture2D(4, 4, format, false);
            if (tex.LoadImage(content))
            {
                ti = new TextureInfo(name, tex);
                texture_dict.Add(name, ti);
            }
        }
        return ti;
    }

    public static TextureInfoOtherThread GetTextureInfoOtherThread(
        string name, string path, Action<TextureInfo> callback)
    {
        var ti = new TextureInfoOtherThread
        {
            name = name,
            path = path,
            callback = callback,
            mutex = null,
        };
        texture_other_threads.Add(ti);
        return ti;
    }
    public class TextureInfoOtherThread
    {
        public string name;
        public string path;
        public Action<TextureInfo> callback;
        public System.Threading.Mutex mutex;
    }
    static List<TextureInfoOtherThread> texture_other_threads = new List<TextureInfoOtherThread>();

    public static RenderTextureOtherThread GetRenderTextureOtherThread(int x, int y, Action<RenderTexture> callback)
    {
        var ti = new RenderTextureOtherThread
        {
            x = x,
            y = y,
            callback = callback,
            mutex = null,
        };
        render_texture_other_threads.Add(ti);
        return ti;
    }
    public class RenderTextureOtherThread
    {
        public int x;
        public int y;
        public Action<RenderTexture> callback;
        public System.Threading.Mutex mutex;
    }
    static List<RenderTextureOtherThread> render_texture_other_threads = new List<RenderTextureOtherThread>();

    ///public static RenderTextureDoSomething RenderTexture
    ///

    public class RenderTextureDoSomething
    {
        public enum Code
        {
            kClear,
            kDrawRectangle,
            kFillRectangle,
            kDrawCImg,
            kDrawG,
            kDrawGWithMask,
            kSetColor,
            kGetColor,
        }
        //Todo: 实现对于方法
    }

    static IEnumerator Loading(Bitmap baseimage)
    {
        TextureInfo ti = null;
        FileInfo fi = new FileInfo(baseimage.path);
        if(fi.Exists)
        {
            FileStream fs = fi.OpenRead();
            var filesize = fs.Length;
            byte[] content = new byte[filesize];

            var async = fs.BeginRead(content, 0, (int)filesize, null, null);
            while(!async.IsCompleted)
                yield return null;

            TextureFormat format = TextureFormat.DXT1;

            var extname = uEmuera.Utils.GetSuffix(baseimage.path).ToLower();
            if (extname == "png")
                format = TextureFormat.DXT5;

            if (extname == "webp")
            {
                var tex = Texture2DExt.CreateTexture2DFromWebP(content, false, false,
                out Error err);
                if (err != Error.Success)
                {
                    Debug.LogWarning($"{baseimage.path} {err.ToString()}");
                    yield break;
                }
                ti = new TextureInfo(baseimage.filename, tex);
                texture_dict.Add(baseimage.filename, ti);

                baseimage.size.Width = tex.width;
                baseimage.size.Height = tex.height;
            }
            else
            {
                var tex = new Texture2D(4, 4, format, false);
                if (tex.LoadImage(content))
                {
                    ti = new TextureInfo(baseimage.filename, tex);
                    texture_dict.Add(baseimage.filename, ti);

                    baseimage.size.Width = tex.width;
                    baseimage.size.Height = tex.height;
                }
            }
        }
        List<CallbackInfo> list = null;
        if(loading_set.TryGetValue(baseimage.filename, out list))
        {
            var count = list.Count;
            CallbackInfo item = null;
            for(int i=0; i<count; ++i)
            {
                item = list[i];
                item.DoCallback(GetSpriteInfo(ti, item.src));
            }
            list.Clear();
            loading_set.Remove(baseimage.filename);
        }
    }
    static SpriteInfo GetSpriteInfo(TextureInfo textinfo, ASprite src)
    {
        return textinfo.GetSprite(src);
    }
    internal static void GivebackSpriteInfo(SpriteInfo info)
    {
        if(info == null)
            return;
        info.parent.Release();
    }
    static IEnumerator Update()
    {
        while(true)
        {
            do
            {
                yield return new WaitForSeconds(15.0f);
            } while(texture_dict.Count == 0);

            var now = Time.unscaledTime;
            TextureInfo tinfo = null;
            TextureInfo ti = null;
            var iter = texture_dict.Values.GetEnumerator();
            while(iter.MoveNext())
            {
                ti = iter.Current;
                if(ti.refcount == 0 && now > ti.pasttime)
                {
                    tinfo = ti;
                    break;
                }
            }
            if(tinfo != null)
            {
                Debug.Log("Unload Texture " + tinfo.imagename);

                tinfo.Dispose();
                texture_dict.Remove(tinfo.imagename);
                tinfo = null;

                GC.Collect();
            }
        }
    }
    static IEnumerator UpdateRenderOP()
    {
        while(true)
        {
            do
            {
                yield return new WaitForSeconds(15);
            } while(texture_other_threads.Count == 0
                && render_texture_other_threads.Count == 0);

            TextureInfo ti = null;
            if(texture_other_threads.Count > 0)
            {
                TextureInfoOtherThread tiot = null;
                var tiotiter = texture_other_threads.GetEnumerator();
                while(tiotiter.MoveNext())
                {
                    tiot = tiotiter.Current;
                    tiot.mutex = new System.Threading.Mutex(true);
                    //tiot.mutex.WaitOne();
                    ti = GetTextureInfo(tiot.name, tiot.path);
                    tiot.callback(ti);
                    tiot.mutex.ReleaseMutex();
                }
                texture_other_threads.Clear();
            }
            if(render_texture_other_threads.Count > 0)
            {
                RenderTextureOtherThread rtot = null;
                var rtotiter = render_texture_other_threads.GetEnumerator();
                while(rtotiter.MoveNext())
                {
                    rtot = rtotiter.Current;
                    rtot.mutex = new System.Threading.Mutex(true);
                    //tiot.mutex.WaitOne();
                    var rt = new RenderTexture(rtot.x, rtot.y, 24, RenderTextureFormat.ARGB32);
                    rtot.callback(rt);
                    rtot.mutex.ReleaseMutex();
                }
                render_texture_other_threads.Clear();
            }
        }
    }
    internal static void ForceClear()
    {
        var iter = texture_dict.Values.GetEnumerator();
        while(iter.MoveNext())
        {
            iter.Current.Dispose();
        }
        texture_dict.Clear();
        GC.Collect();
    }
    internal static void SetResourceCSVLine(string filename, string[] lines)
    {
        var cache = string.Join("\n", lines);
        UnityEngine.PlayerPrefs.SetInt(filename + "_fixed", 1);
        UnityEngine.PlayerPrefs.SetString(filename + "_time",
                        File.GetLastWriteTime(filename).ToString());
        UnityEngine.PlayerPrefs.SetString(filename, cache);
    }
    internal static string[] GetResourceCSVLines(string filename)
    {
        if(PlayerPrefs.GetInt(filename + "_fixed", 0) == 0)
            return null;
        var oldwritetime = PlayerPrefs.GetString(filename + "_time", null);
        if(string.IsNullOrEmpty(oldwritetime))
            return null;
        var writetime = File.GetLastWriteTime(filename).ToString();
        if(oldwritetime != writetime)
            return null;
        var cache = UnityEngine.PlayerPrefs.GetString(filename, null);
        if(string.IsNullOrEmpty(cache))
            return null;
        return cache.Split('\n');
    }
    internal static void ClearResourceCSVLines(string filename)
    {
        UnityEngine.PlayerPrefs.SetInt(filename + "_fixed", 0);
        UnityEngine.PlayerPrefs.SetString(filename + "_time", null);
        UnityEngine.PlayerPrefs.SetString(filename, null);
    }
    static Dictionary<string, List<CallbackInfo>> loading_set =
        new Dictionary<string, List<CallbackInfo>>();
    static Dictionary<string, TextureInfo> texture_dict =
        new Dictionary<string, TextureInfo>();
}
