using System;

namespace UnityEngine.UI
{
    public class Monospaced : BaseMeshEffect
    {
        static readonly char[] _index_any = new char[] { '<', '\n' };

        int GetNextValidIndex(string content, int i)
        {
            if (i >= content.Length)
                return i;

            var c = content[i];
            while (c == '<')
            {
                var t1 = content.IndexOf('>', i + 1);
                var t2 = content.IndexOfAny(_index_any, i + 1);
                if (t2 == -1)
                    t2 = int.MaxValue;
                if (t1 < t2 && t1 - i > 1)
                {
                    var sub = content.Substring(i + 1, t1 - i - 1).ToLower();
                    if (sub == "b" ||
                        sub == "i" ||
                        sub == "/b" ||
                        sub == "/i" ||
                        sub == "/size" ||
                        sub == "/color" ||
                        sub.IndexOf("size") == 0 ||
                        sub.IndexOf("color") == 0)
                    {
                        i = t1 + 1;
                        if (i >= content.Length)
                            return i;
                        c = content[i];
                    }
                    else
                        break;
                }
                else
                    break;
            }
            return i;
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!enabled || string.IsNullOrEmpty(text.text))
                return;

            var size = widthsize;
            if (size < text.fontSize)
                size = text.fontSize;
            var count = vh.currentVertCount / 4;
            var content = text.text;
            var length = content.Length;
            var richtext = text.supportRichText;
            if (string.IsNullOrWhiteSpace(content))
                return;

            int i = 0;
            if (richtext)
                i = GetNextValidIndex(content, i);
            float a = size / 2.0f * 1.30f;
            float b = size * 1.50f;
            float d = 0;
            float s = 0;
            float si = 0;

            UIVertex v1 = new UIVertex();
            UIVertex v2 = new UIVertex();
            float linestart = -rectTransform.sizeDelta.x * rectTransform.pivot.x;

            //顶点索引记录
            int vi = 0;
            char c = '\x0';
            for (; i < length && vi < count; ++i)
            {
                c = content[i];
                if (c == '\n')
                {
                    if (richtext)
                    {
                        var ni = i + 1;
                        i = GetNextValidIndex(content, ni);
                        if (i >= length)
                            break;
                        else if (i == ni)
                            //非特殊
                            i -= 1;
                    }
                    s = 0;
                    continue;
                }
                else if (c == ' ')
                {
                    s += size / 2.0f;
                    continue;
                } 
                else if (richtext && c == '<')
                {
                    var nexti = GetNextValidIndex(content, i);
                    if (nexti > i)
                    {
                        i = nexti;
                        if (i >= length)
                            break;
                        i -= 1;
                        continue;
                    }
                }

                vh.PopulateUIVertex(ref v1, vi * 4 + 0);
                vh.PopulateUIVertex(ref v2, vi * 4 + 2);

                d = v2.position.x - v1.position.x;
                if (d > b)
                    //字形大小超过文本尺寸时
                    //可能使用<size>富文本标记
                    si = d;
                else if (uEmuera.Utils.CheckFullSize(c))
                    si = size;
                else if (uEmuera.Utils.CheckHalfSize(c))
                    si = size / 2.0f;
                else if (c == ' ')
                    si = size / 2.0f;
                else if (c == 8203)
                    si = 0;
                //else if(c == '　')
                //    si = size;
                //else if(d < a)
                //    si = size / 2.0f;
                else
                    si = size;

                var o = s + (si - d) / 2.0f;
                v1.position.x = linestart + o;
                v2.position.x = v1.position.x + d;

                vh.SetUIVertex(v1, vi * 4 + 0);
                vh.SetUIVertex(v2, vi * 4 + 2);

                vh.PopulateUIVertex(ref v1, vi * 4 + 3);
                vh.PopulateUIVertex(ref v2, vi * 4 + 1);

                v1.position.x = linestart + o;
                v2.position.x = v1.position.x + d;

                vh.SetUIVertex(v1, vi * 4 + 3);
                vh.SetUIVertex(v2, vi * 4 + 1);
                vi += 1;

                s += si;
            }
        }

        public float widthsize = 0;

        RectTransform rectTransform => transform as RectTransform;
        Text text
        {
            get
            {
                if (text_ == null)
                    text_ = GetComponent<Text>();
                return text_;
            }
        }
        Text text_ = null;
    }
}