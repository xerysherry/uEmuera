using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MinorShift.Emuera;
using MinorShift.Emuera.GameView;

public abstract class EmueraBehaviour : MonoBehaviour
{
    public static void Ready()
    {
        FontSize = Config.FontSize;
        FontColor = GenericUtils.ToUnityColor(Config.ForeColor);
    }
    public static int FontSize { get; private set; }
    public static Color FontColor { get; private set; }

    public enum Align
    {
        LEFT = 0,
        CENTER = 1,
        RIGHT = 2,
    }
    public class UnitDesc
    {
        //Text
        public string content
        {
            get { return resource_content; }
            set { resource_content = value; }
        }
        public string fontname
        {
            get { return resource_name; }
            set { resource_name = value; }
        }
        //internal
        string resource_content;
        string resource_name;

        public string code;
        public int generation;
        public int posx;
        public int relative_posx;
        public int width;
        public int height;
        public Color color;
        public List<int> image_indices;

        public uint flags = 0;
        public bool isbutton
        {
            get { return (flags & 0x1) == 1; }
            set { flags = value ? (flags | 0x1) : (flags & (0xFFFFFFFF ^ 0x1U)); }
        }
        public bool underline
        {
            get { return ((flags >> 1) & 0x1) == 1; }
            set { flags = value ? (flags | (0x1U << 1)) : (flags & (0xFFFFFFFF ^ (0x1U << 1))); }
        }
        public bool strickout
        {
            get { return ((flags >> 2) & 0x1) == 1; }
            set { flags = value ? (flags | (0x1U << 2)) : (flags & (0xFFFFFFFF ^ (0x1U << 2))); }
        }
        public bool empty
        {
            get { return ((flags >> 3) & 0x1) == 1; }
            set { flags = value ? (flags | (0x1U << 3)) : (flags & (0xFFFFFFFF ^ (0x1U << 3))); }
        }
        public bool richedit
        {
            get { return ((flags >> 4) & 0x1) == 1; }
            set { flags = value ? (flags | (0x1U << 4)) : (flags & (0xFFFFFFFF ^ (0x1U << 4))); }
        }
        public bool monospaced
        {
            get { return ((flags >> 5) & 0x1) == 1; }
            set { flags = value ? (flags | (0x1U << 5)) : (flags & (0xFFFFFFFF ^ (0x1U << 5))); }
        }
    }
    public class LineDesc
    {
        public LineDesc(object display, float posy, float h)
        {
            display_line = (ConsoleDisplayLine)display;
            position_y = posy;
            height = h;
        }
        public object console_line { get { return display_line; } }
        ConsoleDisplayLine display_line = null;

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            var count = units.Count;
            for(var i=0; i<count; ++i)
                str.Append(units[i].content);
            return str.ToString();
        }
        public void Update()
        {
            units = new List<UnitDesc>();

            var Buttons = display_line.Buttons;
            for(int i = 0; i < Buttons.Length; ++i)
            {
                var btn = Buttons[i];
                var ud = new UnitDesc();
                var fontname = FontUtils.default_fontname;
                var btnlength = btn.StrArray.Length;
                var validstr = 0;
                var validlength = 0;
                var richedit = false;
                ud.width = 0;
                ud.color = FontColor;
                ud.monospaced = true;
                StringBuilder content = new StringBuilder();
                for(int si = 0; si < btnlength; ++si)
                {
                    var s = btn.StrArray[si];
                    if(string.IsNullOrEmpty(s.Str.Trim()))
                        continue;
                    if(validstr == 0)
                        validstr = 1;
                    else
                    {
                        validstr += 1;
                        break;
                    }
                }
                for(int si = 0; si < btnlength; ++si)
                {
                    var s = btn.StrArray[si];
                    if(s is MinorShift.Emuera.GameView.ConsoleImagePart)
                    {
                        var cp = s as MinorShift.Emuera.GameView.ConsoleImagePart;
                        if(cp.Image != null)
                        {
                            if(ud.image_indices == null)
                                ud.image_indices = new List<int>();
                            ud.image_indices.Add(si);
                            continue;
                        }
                    }

                    var str = s.Str;
                    validlength += str.Trim().Length;
                    if(string.IsNullOrEmpty(str))
                        continue;
                    var fontsize = (float)FontSize;
                    var fontstyle = uEmuera.Drawing.FontStyle.Regular;
                    var fontcolor = FontColor;

                    if(s is MinorShift.Emuera.GameView.ConsoleStyledString)
                    {
                        var u = (MinorShift.Emuera.GameView.ConsoleStyledString)s;
                        fontsize = u.Font.Size;
                        fontstyle = u.Font.Style;
                        fontname = u.Font.FontFamily.Name;
                        ud.monospaced = u.Font.Monospaced;
                    }
                    if(s is MinorShift.Emuera.GameView.AConsoleColoredPart)
                    {
                        var u = (MinorShift.Emuera.GameView.AConsoleColoredPart)s;
                        fontcolor = GenericUtils.ToUnityColor(u.pColor);
                    }
                    if(fontstyle != uEmuera.Drawing.FontStyle.Regular)
                    {
                        if((fontstyle & uEmuera.Drawing.FontStyle.Bold) > 0)
                        {
                            str = string.Format("<b>{0}</b>", str);
                            richedit = true;
                        }
                        if((fontstyle & uEmuera.Drawing.FontStyle.Italic) > 0)
                        {
                            str = string.Format("<i>{0}</i>", str);
                            richedit = true;
                        }
                        if((fontstyle & uEmuera.Drawing.FontStyle.Underline) > 0)
                            ud.underline = true;
                        if((fontstyle & uEmuera.Drawing.FontStyle.Strikeout) > 0)
                            ud.strickout = true;
                    }
                    if(fontsize != FontSize)
                    {
                        str = string.Format("<size={0}>{1}</size>", (int)fontsize, str);
                        richedit = true;
                    }
                    if(fontcolor != FontColor)
                    {
                        if(validstr != 1)
                        {
                            str = string.Format("<color=#{0}>{1}</color>", GenericUtils.GetColorCode(fontcolor), str);
                            richedit = true;
                        }
                        else
                            ud.color = fontcolor;
                    }
                    content.Append(str);
                    ud.width += uEmuera.Utils.GetDisplayLength(s.Str, fontsize);
                }
                ud.empty = (validlength == 0);
                if(ud.empty)
                    ud.content = null;
                else
                    ud.content = content.ToString();
                ud.isbutton = btn.IsButton;
                ud.generation = (int)btn.Generation;
                ud.code = btn.Inputs;
                ud.posx = btn.PointX;
                ud.relative_posx = btn.RelativePointX;
                if(fontname != FontUtils.default_fontname)
                    ud.fontname = fontname;
                else
                    ud.fontname = null;
                ud.richedit = richedit;

                units.Add(ud);
            }
        }
        /// <summary>
        /// 对其方式
        /// </summary>
        public Align align { get { return (Align)display_line.Align; } }
        /// <summary>
        /// 行号
        /// </summary>
        public int LineNo { get { return display_line.LineNo; } }
        /// <summary>
        /// 是否为逻辑行
        /// </summary>
        public bool IsLogicalLine { get { return display_line.IsLogicalLine; } }
        /// <summary>
        /// 坐标Y
        /// </summary>
        public float position_y = 0.0f;
        /// <summary>
        /// 高度
        /// </summary>
        public float height = 0.0f;
        /// <summary>
        /// 子对象
        /// </summary>
        public List<UnitDesc> units = null;
    }

    public static void OnClick(UnityEngine.EventSystems.PointerEventData e)
    {
        var obj = e.rawPointerPress;
        if(obj == null)
            return;
        var behaviour = obj.GetComponent<EmueraBehaviour>();
        if(behaviour == null)
        {
            EmueraThread.instance.Input("", false);
            return;
        }
        var unit_desc = behaviour.unit_desc;
        if(!unit_desc.isbutton)
            return;
        if(unit_desc.generation < EmueraContent.instance.button_generation)
            EmueraThread.instance.Input("", false);
        else
            EmueraThread.instance.Input(unit_desc.code, true);
    }

    public abstract void UpdateContent();

    public void SetPosition(float x, float y)
    {
        var rt = (RectTransform)transform;
        rt.anchoredPosition = new Vector2(x, y);
    }
    public RectTransform rect_transform { get { return transform as RectTransform; } }

    public LineDesc line_desc = null;
    public int LineNo { get { return line_desc.LineNo; } }
    [HideInInspector]
    public int UnitIdx = -1;
    public UnitDesc unit_desc
    {
        get
        {
            if(line_desc == null || UnitIdx >= line_desc.units.Count)
                return null;
            return line_desc.units[UnitIdx];
        }
    }
    [HideInInspector]
    public float Width = 0;
    [HideInInspector]
    public float Height = 0;
    [HideInInspector]
    public float logic_y = 0.0f;
    [HideInInspector]
    public float logic_height = 0.0f;
#if UNITY_EDITOR
    public string code;
    public int generation;
#endif
}
