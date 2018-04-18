using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using MinorShift.Emuera;
using MinorShift.Emuera.GameView;

public class EmueraContent : MonoBehaviour
{
    public static EmueraContent instance { get { return instance_; } }
    static EmueraContent instance_ = null;

    public static int FontSize { get; private set; }
    public static Color FontColor { get; private set; }

    public class TextObj
    {
        public LineDesc line;
        public int unit_index;
        public UnitDesc unit
        {
            get
            {
                if(line == null || unit_index >= line.units.Count)
                    return null;
                return line.units[unit_index];
            }
        }
    }
    public enum Align
    {
        LEFT = 0,
        CENTER = 1,
        RIGHT = 2,
    }
    public class UnitDesc
    {
        public string content;
        public string code;
        public int generation;
        public int posx;
        public int relative_posx;
        public int width;
        public Color color;
        public string fontname;

        public uint flags = 0;
        public bool isbutton
        {
            get { return (flags & 0x1) == 1; }
            set { flags = value ? (flags | 0x1) : (flags & (0xFFFFFFFF ^ 0x1U)); }
        }
        public bool underline
        {
            get { return ((flags >> 1) & 0x1) == 1; }
            set { flags = value ? (flags | (0x1U << 1)) : (flags & (0xFFFFFFFF ^ (0x1U << 1)));}
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
            foreach(var u in units)
                str.Append(u.content);
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

    public string default_fontname;
    public Text template_text;
    public Image template_block;
    public Button template_button;
    public RectTransform text_content;
    public RectTransform image_content_1;
    public RectTransform image_content_2;
    public RectTransform image_content_3;

    public Button quick_button;
    public Button input_button;

    public QuickButtons quick_buttons;
    public Inputpad input_pad;

    Image background;
    uEmuera.Drawing.Color background_color;

    public RectTransform rect_transform { get { return (RectTransform)transform; } }

    void Awake()
    {
        FontUtils.SetDefaultFont(default_fontname, true);
    }

    void Start()
    {
        instance_ = this;
        background = GetComponent<Image>();

        GenericUtils.SetListenerOnBeginDrag(gameObject, OnBeginDrag);
        GenericUtils.SetListenerOnDrag(gameObject, OnDrag);
        GenericUtils.SetListenerOnEndDrag(gameObject, OnEndDrag);
        GenericUtils.SetListenerOnClick(gameObject, OnClick);
        GenericUtils.SetListenerOnClick(quick_button.gameObject, OnQuickButtonClick);
        GenericUtils.SetListenerOnClick(input_button.gameObject, OnInputPadButtonClick);
    }

    int GetLineNoIndex(int lineno)
    {
        int high = end_index - 1;
        int low = begin_index;
        int mid = 0;
        int found = -1;

        while(low <= high)
        {
            mid = (low + high) / 2;
            int k = console_lines_[mid % max_log_count].LineNo;
            if(k > lineno)
                high = mid - 1;
            else if(k < lineno)
                low = mid + 1;
            else
            {
                found = mid;
                break; 
            }
        }

        if(found < 0)
            return -1;
        return found;
    }

    int GetLineNoIndexForPosY(float y)
    {
        int high = end_index - 1;
        int low = begin_index;
        int mid = 0;
        while(low <= high)
        {
            mid = (low + high) / 2;
            var l = console_lines_[mid % max_log_count];
            float top = l.position_y;
            float bottom = l.position_y + l.height;
            if(y < top)
                high = mid - 1;
            else if(y > bottom)
                low = mid + 1;
            else
                return mid;
        }
        if(high <= begin_index)
            return begin_index;
        else if(low >= end_index - 1)
            return end_index - 1;
        return -1;
    }

    int GetPrevLineNoIndex(int index)
    {
        if(index > max_index || index < 0)
            return -1;

        var lineno = 0;
        var cindex = index;
        var zero = begin_index;

        if(!console_lines_[index % max_log_count].IsLogicalLine)
        {
            lineno = console_lines_[index % max_log_count].LineNo;
            for(; cindex >= zero; --cindex)
            {
                if(console_lines_[cindex % max_log_count].LineNo != lineno)
                    break;
            }
            return cindex + 1;
        }
        else
            return cindex;
    }

    int GetNextLineNoIndex(int index)
    {
        if(index > max_index || index < 0)
            return -1;

        var lineno = console_lines_[index % max_log_count].LineNo;
        var cindex = index;
        for(; cindex < max_index; ++cindex)
        {
            if(console_lines_[cindex % max_log_count].LineNo != lineno)
                break;
        }
        return cindex - 1;
    }

    public void Update()
    {
        if(!dirty && drag_delta == Vector2.zero)
            return;
        dirty = false;

        float display_width = rect_transform.rect.width;
        float display_height = rect_transform.rect.height;

        if(drag_delta != Vector2.zero)
        {
            float t = drag_delta.magnitude;
            drag_delta *= (Mathf.Max(0, t - 300.0f * Time.deltaTime) / t);
            local_position = GetLimitPosition(local_position + drag_delta,
                                            display_width, display_height);
            if((local_position.x <= display_width - content_width && drag_delta.x < 0) ||
                (local_position.x >= 0 && drag_delta.x > 0))
                drag_delta.x = 0;
            if((local_position.y >= content_height - display_height && drag_delta.y > 0) || 
                (local_position.y <= offset_height && drag_delta.y < 0))
                drag_delta.y = 0;
        }

        var pos = local_position + (drag_curr_position - drag_begin_position);
        pos = GetLimitPosition(pos, display_width, display_height);

        int remove_count = 0;
        int count = display_lines_.Count;
        int max_line_no = -1;
        int min_line_no = int.MaxValue;
        for(int i = 0; i < count - remove_count; ++i)
        {
            var line = display_lines_[i];
            if(line.logic_y > pos.y + display_height ||
                line.logic_y + line.logic_height < pos.y)
            {
                display_lines_[i] = display_lines_[count - remove_count - 1];
                PushLineConfig(line);
                ++remove_count;
                --i;
            }
            else
            {
                line.SetPosition(pos.x + line.unit_desc.posx, pos.y - line.logic_y);
                max_line_no = System.Math.Max(line.LineNo, max_line_no);
                min_line_no = System.Math.Min(line.LineNo, min_line_no);
            }
        }
        if(remove_count > 0)
            display_lines_.RemoveRange(count - remove_count, remove_count);

        var index = GetLineNoIndex(min_line_no - 1);
        index = GetPrevLineNoIndex(index);
        if(index >= 0)
        {
            UpdateLine(pos, display_height, index, -1);
        }
        index = GetLineNoIndex(max_line_no + 1);
        index = GetNextLineNoIndex(index);
        if(index >= 0)
        {
            UpdateLine(pos, display_height, index, +1);
        }
        if(display_lines_.Count == 0 && console_lines_.Count > 0)
        {
            index = GetLineNoIndexForPosY(pos.y);
            UpdateLine(pos, display_height, index, -1);
            UpdateLine(pos, display_height, index+1, +1);
        }

    }
    void UpdateLine(Vector2 local, float display_height, int index, int delta)
    {
        var zero = begin_index;
        while(zero <= index && index < end_index)
        {
            var l = console_lines_[index % max_log_count];
            if(l.position_y > local.y + display_height ||
                l.position_y + l.height < local.y)
                break;

            if(l.units.Count > 1)
            {
                for(int li = 0; li < l.units.Count; ++li)
                {
                    if(l.units[li].empty)
                        continue;
                    var lc = PullLineConfig();
                    lc.line_desc = l;
                    lc.UnitIdx = li;
                    lc.SizeFitter = true;
                    lc.Width = l.units[li].width;
                    lc.UpdateContent();
                    lc.SetPosition(l.units[li].posx + local.x, local.y - lc.logic_y);
                    display_lines_.Add(lc);
                }
            }
            else if(l.units.Count == 1 && !l.units[0].empty)
            {
                var lc = PullLineConfig();
                lc.line_desc = l;
                lc.UnitIdx = 0;
                lc.SizeFitter = false;

                if(l.units[0].isbutton)
                    lc.Width = l.units[0].width;
                else
                    lc.Width = content_width;

                lc.UpdateContent();
                lc.SetPosition(l.units[0].posx + local.x, local.y - lc.logic_y);
                display_lines_.Add(lc);
            }
            index += delta;
        }
    }
    Vector2 GetLimitPosition(Vector2 local,
        float display_width, float display_height)
    {
        if(content_width > display_width)
        {
            //左右移动
            if(local.x > 0)
                local.x = 0;
            else if(local.x < display_width - content_width)
                local.x = display_width - content_width;
        }
        else
            local.x = 0;

        var valid_height = content_height - offset_height;
        if(offset_height > 0 && valid_height < display_height)
        {
            local.y = offset_height;
        }
        else
        {
            var display_delta = content_height - display_height;
            if(content_height <= display_height)
                local.y = display_delta;
            else if(local.y > display_delta)
                local.y = display_delta;
            else if(local.y < offset_height)
                local.y = offset_height;
        }

        return local;
    }

    bool dirty = false;
    void OnBeginDrag(UnityEngine.EventSystems.PointerEventData e)
    {
        drag_begin_position = e.position;
        drag_curr_position = e.position;
        drag_delta = Vector3.zero;
    }
    void OnDrag(UnityEngine.EventSystems.PointerEventData e)
    {
        dirty = true;
        drag_curr_position = e.position;
        drag_delta = Vector3.zero;
    }
    void OnEndDrag(UnityEngine.EventSystems.PointerEventData e)
    {
        dirty = true;

        float display_width = rect_transform.rect.width;
        float display_height = rect_transform.rect.height;
        local_position = GetLimitPosition(
            local_position + (e.position - drag_begin_position),
            display_width, display_height);

        drag_delta = e.position - drag_curr_position;
        drag_begin_position = Vector2.zero;
        drag_curr_position = Vector2.zero;
    }
    void OnClick()
    {
        EmueraThread.instance.Input("", false);
    }
    void OnQuickButtonClick()
    {
        if(quick_buttons.IsShow)
            quick_buttons.Hide();
        else
        {
            input_pad.Hide();
            quick_buttons.Show();
            SetLastButtonGeneration(last_button_generation);
        }
    }
    void OnInputPadButtonClick()
    {
        if(input_pad.IsShow)
            input_pad.Hide();
        else
        {
            quick_buttons.Hide();
            input_pad.Show();
        }
    }
    Vector2 drag_begin_position = Vector2.zero;
    Vector2 drag_curr_position = Vector2.zero;
    Vector2 drag_delta = Vector2.zero;

    public void SetBackgroundColor(uEmuera.Drawing.Color color)
    {
        if(background_color == color)
            return;
        background.color = GenericUtils.ToUnityColor(color);
        background_color = color;
    }
    public void Ready()
    {
        background.color = GenericUtils.ToUnityColor(Config.BackColor);
        background_color = Config.BackColor;
        content_width = Config.WindowX;

        FontUtils.SetDefaultFont(Config.FontName, true);

        template_text.color = GenericUtils.ToUnityColor(Config.ForeColor);
        template_text.font = FontUtils.default_font;
        if(template_text.font == null)
            template_text.font = FontUtils.default_font;
        template_text.fontSize = Config.FontSize;
        template_text.rectTransform.sizeDelta = 
            new Vector2(template_text.rectTransform.sizeDelta.x, 0);
        template_text.gameObject.SetActive(false);

        FontSize = template_text.fontSize;
        FontColor = template_text.color;

        console_lines_ = new List<LineDesc>(max_log_count);
        while(console_lines_.Count < max_log_count)
            console_lines_.Add(null);
        invalid_count = max_log_count;
    }
    bool ready_ = false;

    public void Clear()
    {
        invalid_count = 0;
        max_index = 0;
        console_lines_.Clear();

        for(int i = 0; i < display_lines_.Count; ++i)
        {
            PushLineConfig(display_lines_[i]);
        }

        display_lines_.Clear();
        content_height = 0;
        offset_height = 0;
        local_position = Vector2.zero;
        drag_delta = Vector2.zero;
        dirty = true;
    }
    public void AddLine(object line, bool roll_to_bottom = false)
    {
        if(line == null)
            return;
        if(!ready_)
        {
            Ready();
            ready_ = true;
        }
        LineDesc ld = new LineDesc(line, content_height, Config.LineHeight);

        console_lines_[max_index % max_log_count] = ld;
        if(invalid_count > 0)
            invalid_count -= 1;
        //添加偏移高
        if(valid_count >= max_log_count)
            offset_height += Config.LineHeight;
        max_index += 1;

        ld.Update();
        
        //添加容器高
        content_height += Config.LineHeight;
        if(roll_to_bottom)
        {
            local_position.y = content_height - rect_transform.rect.height;
            drag_delta.y = 0;
        }
        else
        {
            drag_delta.y += Config.LineHeight * 1.5f;
        }
        dirty = true;
    }
    public object GetLine(int index)
    {
        if(index < begin_index || index >= end_index)
            return null;
        return console_lines_[index % max_log_count].console_line;
    }
    public int GetLineCount()
    {
        return valid_count;
    }
    public int GetMinLineNo()
    {
        return begin_index;
    }
    public int GetMaxLineNo()
    {
        if(valid_count == 0)
            return -1;
        return max_index;
    }
    public void RemoveLine(int count)
    {
        if(!ready_)
        {
            Ready();
            ready_ = true;
        }
        if(count > valid_count)
            count = valid_count;
        if(count == 0)
            return;

        var lineno = console_lines_[(max_index - count) % max_log_count].LineNo;
        display_lines_.Sort((l, r) =>
        {
            return (l.LineNo * 10 + l.UnitIdx) -
                (r.LineNo * 10 + r.UnitIdx);
        });

        var i = 0;
        for(; i < display_lines_.Count; ++i)
        {
            if(display_lines_[i].LineNo >= lineno)
                break;
        }
        var remove = 0;
        for(; i < display_lines_.Count; ++i, ++remove)
        {
            PushLineConfig(display_lines_[i]);
        }
        if(remove > 0)
        {
            if(remove >= display_lines_.Count)
                display_lines_.Clear();
            else
                display_lines_.RemoveRange(display_lines_.Count - remove, remove);
        }

        var eidx = end_index - 1;
        var bidx = end_index - count ;
        for(i = eidx; i >= bidx; --i)
            console_lines_[i % max_log_count] = null;

        content_height -= Config.LineHeight * count;
        if(count >= valid_count)
        {
            offset_height = 0;
            content_height = 0;
        }
        else if(max_index > max_log_count)
        {
            if(max_index - count <= max_log_count)
            {
                //offset_height -= Config.LineHeight * (max_index - max_log_count);
                //offset_height = Config.LineHeight * (max_index - max_log_count);
                var l = console_lines_[max_index - count - 1];
                offset_height = l.position_y + l.height;
            }
        }

        invalid_count += count;
        max_index -= count;
        dirty = true;
    }
    public void ToBottom()
    {
        local_position.y = content_height - rect_transform.rect.height;
        drag_delta = Vector2.zero;
        dirty = true;
        Update();
    }
    public void SetLastButtonGeneration(int generation)
    {
        last_button_generation = generation;
        if(quick_buttons.IsShow)
        {
            quick_buttons.Clear();
            if(last_button_generation < 0)
                return;

            for(int i = end_index - 1; i >= begin_index; --i)
            {
                var cl = console_lines_[i%max_log_count];
                if(cl.units == null)
                    continue;
                var bl = 0;
                for(int j = 0; j < cl.units.Count; ++j)
                {
                    var cu = cl.units[j];
                    if(cu.isbutton)
                    {
                        if(cu.generation != last_button_generation)
                            return;
                        if(cu.empty)
                            continue;
                        quick_buttons.AddButton(cu.content, cu.color, cu.code);
                        bl += 1;
                    }
                }
                if(bl > 0)
                    quick_buttons.ShiftLine();    
            }
        }
    }

    public int button_generation { get { return last_button_generation; } }
#if UNITY_EDITOR
    public
#endif
    int last_button_generation = 0;
    int max_index = 0;
    int invalid_count = 0;
    int begin_index
    {
        get
        {
            return System.Math.Max(0, max_index - valid_count);
        }
    }
    int end_index
    {
        get
        {
            return max_index;
        }
    }
    int valid_count
    {
        get
        {
            return max_log_count - invalid_count;
        }
    }
    public int max_log_count { get { return MinorShift.Emuera.Config.MaxLog; } }
    List<LineDesc> console_lines_;

    /// <summary>
    /// 偏移高
    /// </summary>
    float offset_height = 0;
    /// <summary>
    /// 内容宽
    /// </summary>
    float content_width = 0;
    /// <summary>
    /// 内容高
    /// </summary>
    float content_height = 0;
    /// <summary>
    /// 当前移动点
    /// </summary>
    Vector2 local_position = Vector2.zero;
    
    EmueraLine PullLineConfig()
    {
        EmueraLine config = null;
        if(cache_lines_.Count > 0)
            config = cache_lines_.Dequeue();
        else
        {
            var obj = GameObject.Instantiate(template_text.gameObject);
            config = obj.GetComponent<EmueraLine>();
            config.transform.SetParent(text_content);
            config.transform.localScale = Vector3.one;
        }
        config.gameObject.SetActive(true);  
        return config;
    }
    void PushLineConfig(EmueraLine config)
    {
        config.Clear();
        config.gameObject.SetActive(false);
        config.gameObject.name = "unused";
        cache_lines_.Enqueue(config);
    }

    List<EmueraLine> display_lines_ = new List<EmueraLine>();
    Queue<EmueraLine> cache_lines_ = new Queue<EmueraLine>();
}
