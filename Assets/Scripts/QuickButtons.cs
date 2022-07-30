using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickButtons : MonoBehaviour
{
    class QuickButton : MonoBehaviour
    {
        void Awake()
        {
            background = gameObject.GetComponent<Image>();
            content = gameObject.GetComponentInChildren<Text>();
            GenericUtils.SetListenerOnClick(gameObject, OnClick);
        }
        void OnClick()
        {
            EmueraThread.instance.Input(code, true);
        }

        public float x;
        public int line;
        public Image background;
        public Text content;
        public string code;
    }

    static readonly int kMaxContentButtonCountShow = 4;

    public Button template_button;

    RectTransform parent
    {
        get
        {
            if(parent_ == null)
            {
                parent_ = transform.parent as RectTransform;
                while(parent_.parent != null)
                {
                    parent_ = parent_.parent as RectTransform; ;
                } 
            }
            return parent_;
        }
    }
    RectTransform parent_;
    //float WIDTH {
    //    get { return parent.sizeDelta.x; }
    //}
    //float HEIGHT {
    //    get { return parent.sizeDelta.y; }
    //}

	void Start ()
    {
        GenericUtils.SetListenerOnBeginDrag(gameObject, OnBeginDrag);
        GenericUtils.SetListenerOnDrag(gameObject, OnDrag);
        GenericUtils.SetListenerOnEndDrag(gameObject, OnEndDrag);
        GenericUtils.SetListenerOnClick(gameObject, OnClick);
    }

    void Update()
    {
        if(!dirty && drag_delta == Vector2.zero && last_wh_flag == Screen.width > Screen.height)
            return;
        dirty = false;
        last_wh_flag = Screen.width > Screen.height;

        float content_width = max_line_count * (width + interval);
        float content_height = lineidx * (height + interval);
        float max_width = (width + interval) * kMaxContentButtonCountShow;
        float display_width = System.Math.Min(content_width, max_width);

        var ec_rt = parent.rect;
        if(Screen.width < Screen.height)
            display_width = System.Math.Min(display_width, ec_rt.width);

        float display_height = System.Math.Min(ec_rt.height - 70, content_height);
        rect_transform.sizeDelta = new Vector2(display_width, display_height);
        
        if(drag_delta != Vector2.zero)
        {
            float t = drag_delta.magnitude;
            drag_delta *= (Mathf.Max(0, t - 300.0f * Time.deltaTime) / t);
            local_position = GetLimitPosition(local_position + drag_delta,
                content_width, content_height, display_width, display_height);
            if((local_position.x >= content_width - display_width && drag_delta.x > 0) ||
                (local_position.x <= 0 && drag_delta.x < 0))
                drag_delta.x = 0;
            if((local_position.y <= display_height - content_height  && drag_delta.y < 0) ||
                (local_position.y >= 0 && drag_delta.y > 0))
                drag_delta.y = 0;
        }

        var pos = local_position + (drag_curr_position - drag_begin_position);
        pos = GetLimitPosition(pos,
            content_width, content_height,
            display_width, rect_transform.rect.height);

        float total_width = content_width - (width + interval);
        for(int i = 0; i < display_buttons_.Count; ++i)
        {
            var b = display_buttons_[i];
            var rt = b.transform as RectTransform;
            
            rt.anchoredPosition = 
                new Vector2(- total_width + b.x, (b.line + 1) * (height + interval)) + pos;
        }
	}

    Vector2 GetLimitPosition(Vector2 local, 
        float content_width, float content_height,
        float display_width, float display_height)
    {
        if(content_width > display_width)
        {
            //左右移动
            if(local.x < 0)
                local.x = 0;
            else if(local.x > content_width - display_width)
                local.x = content_width - display_width;
        }
        else
            local.x = 0;

        var valid_height = content_height;
        if(valid_height < display_height)
        {
            local.y = 0;
        }
        else
        {
            var display_delta = content_height - display_height;
            if(content_height <= display_height)
                local.y = 0;
            else if(local.y < -display_delta)
                local.y = -display_delta;
            else if(local.y > 0)
                local.y = 0;
        }

        return local;
    }

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

        float content_width = max_line_count * (width + interval);
        float content_height = lineidx * (height + interval);
        float display_width = rect_transform.rect.width;
        float display_height = rect_transform.rect.height;

        local_position = GetLimitPosition(
            local_position + (e.position - drag_begin_position),
            content_width, content_height,
            display_width, display_height);

        drag_delta = e.position - drag_curr_position;
        drag_begin_position = Vector2.zero;
        drag_curr_position = Vector2.zero;
    }
    void OnClick()
    {
        EmueraThread.instance.Input("", false);
    }

    RectTransform rect_transform { get { return (RectTransform)transform; } }
    Vector2 drag_begin_position = Vector2.zero;
    Vector2 drag_curr_position = Vector2.zero;
    Vector2 drag_delta = Vector2.zero;
    bool dirty = false;
    Vector2 local_position = Vector2.zero;

    public void Show()
    {
        gameObject.SetActive(true);
        Clear();
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public bool IsShow { get { return gameObject.activeSelf; } }
    public void Clear()
    {
        for(int i = display_buttons_.Count - 1; i >= 0; --i)
        {
            var b = display_buttons_[i];
            PushButton(b);
        }
        display_buttons_.Clear();
        add_x = 0;
        lineidx = 0;
        curr_line_count = 0;
        max_line_count = 0;
        local_position.x = 0;
        dirty = true;
    }
    public void AddButton(string text, Color color, string code)
    {
        var button = PullButton();
        button.gameObject.SetActive(true);
        display_buttons_.Add(button);

#if UNITY_EDITOR
        button.name = "Code:" + code;
#endif
        button.content.text = text.Trim();
        button.content.color = color;
        button.code = code;
        button.line = lineidx;
        button.x = add_x - interval;
        //var bc = button.background.color;
        var bc = color;
        button.background.color = new Color(
            Mathf.Clamp(1 - bc.r + 0.10f, 0, 1),
            Mathf.Clamp(1 - bc.g + 0.10f, 0, 1),
            Mathf.Clamp(1 - bc.b + 0.10f, 0, 1), 0.6f);
        add_x += (width + interval);
        curr_line_count += 1;
        max_line_count = System.Math.Max(max_line_count, curr_line_count);

        local_position.x = max_line_count * (width + interval);
        dirty = true;
    }
    public void ShiftLine()
    {
        if(curr_line_count == 0)
            return;
        lineidx += 1;
        add_x = 0;
        curr_line_count = 0;
    }
    QuickButton PullButton()
    {
        QuickButton button = null;
        if(cache_buttons_.Count > 0)
            button = cache_buttons_.Pop();
        else
        {
            var obj = GameObject.Instantiate(template_button.gameObject);
            button = obj.AddComponent<QuickButton>();
            button.transform.SetParent(transform);
            var rt = button.transform as RectTransform;
            if(width == 0 || height == 0)
            {
                width = rt.sizeDelta.x;
                height = rt.sizeDelta.y;
            }

            rt.localScale = Vector3.one;
            rt.anchorMax = new Vector2(1, 0);
            rt.anchorMin = rt.anchorMax;
            rt.pivot = Vector2.one;
        }
        return button;
    }
    void PushButton(QuickButton button)
    {
#if UNITY_EDITOR
        button.name = "unused";
#endif
        button.gameObject.SetActive(false);
        var rt = button.transform as RectTransform;
        rt.anchoredPosition = new Vector2(0, 0);
        cache_buttons_.Push(button);
    }

    bool last_wh_flag = false;
    float width = 0;
    float height = 0;
    float interval = 5;

    float add_x = 0;
    int lineidx = 0;
    int curr_line_count = 0;
    int max_line_count = 0;
    List<QuickButton> display_buttons_ = new List<QuickButton>();
    Stack<QuickButton> cache_buttons_ = new Stack<QuickButton>();
}
