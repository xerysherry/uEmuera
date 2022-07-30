using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MinorShift.Emuera.GameView;
using MinorShift.Emuera.Content;

public class EmueraImage : EmueraBehaviour
{
    class ImageInfo : MonoBehaviour
    {
        static void OnLoadImageCallback(object obj, SpriteManager.SpriteInfo spriteinfo)
        {
            if(obj == null)
            {
                SpriteManager.GivebackSpriteInfo(spriteinfo);
                return;
            }
            var c = obj as ImageInfo;
            if(!c.gameObject.activeSelf)
            {
                SpriteManager.GivebackSpriteInfo(spriteinfo);
                return;
            }
            c.SetSprite(spriteinfo);
        }
        public void Load(ASprite src)
        {
            SpriteManager.GetSprite(src, this, ImageInfo.OnLoadImageCallback);
        }
        void SetSprite(SpriteManager.SpriteInfo spriteinfo)
        {
            if(spriteinfo == null)
            {
                image.sprite = null;
                image.color = kTransparent;
            }
            else
            {
                image.sprite = spriteinfo.sprite;
                image.color = Color.white;
            }
            this.spriteinfo = spriteinfo;
        }
        public void Clear()
        {
            SpriteManager.GivebackSpriteInfo(spriteinfo);
            EmueraContent.instance.PushImage(image);
        }
        
        public SpriteManager.SpriteInfo spriteinfo = null;
        UnityEngine.UI.Image image
        {
            get
            {
                if(image_ == null)
                    image_ = GetComponent<UnityEngine.UI.Image>();
                return image_;
            }
        }
        UnityEngine.UI.Image image_ = null;
    }

    static readonly Color kTransparent = new Color(0, 0, 0, 0);
    GenericUtils.PointerClickListener click_handler_ = null;

    void Awake()
    {
        GenericUtils.SetListenerOnClick(gameObject, OnClick);
        click_handler_ = GetComponent<GenericUtils.PointerClickListener>();
    }

    public override void UpdateContent()
    {
        var ud = unit_desc;
        var image_indices = ud.image_indices;
        var ld = line_desc;
        var consoleline = ld.console_line as ConsoleDisplayLine;
        var cb = consoleline.Buttons[UnitIdx];
        
        if(ud.isbutton && ud.generation >= EmueraContent.instance.button_generation)
        {
            image.enabled = true;
            click_handler_.enabled = true;
#if UNITY_EDITOR
            code = ud.code;
            generation = ud.generation;
#endif
        }
        else
        {
            image.enabled = false;
            click_handler_.enabled = false;
        }

        int miny = int.MaxValue;
        for(int i = 0; i < image_indices.Count; ++i)
        {
            var str_index = image_indices[i];
            var image_part = cb.StrArray[str_index] as ConsoleImagePart;
            miny = System.Math.Min(miny, image_part.Top);
        }
        logic_y = line_desc.position_y + miny;
        logic_height = 0;

        var prt = rect_transform;
        int width = 0;
        for(int i = 0; i < image_indices.Count; ++i)
        {
            var image = EmueraContent.instance.PullImage();
            var imageinfo = image.gameObject.GetComponent<ImageInfo>();
            if(imageinfo == null)
                imageinfo = image.gameObject.AddComponent<ImageInfo>();
            image.raycastTarget = false;

            var rt = image.gameObject.transform as RectTransform;
            rt.SetParent(prt);

            var str_index = image_indices[i];
            var image_part = cb.StrArray[str_index] as ConsoleImagePart;
#if UNITY_EDITOR
            image.name = image_part.Image.Name;
#endif
            imageinfo.Load(image_part.Image);
            image_infos_.Add(imageinfo);

            var image_rect = image_part.dest_rect;
            rt.anchoredPosition = new Vector2(image_part.PointX - ud.posx, miny - image_rect.Top);
            rt.sizeDelta = new Vector2(image_rect.Width, image_rect.Height);
            rt.localScale = Vector3.one;
            logic_height = Mathf.Max(logic_height, image_rect.Height);

            width = Mathf.Max(image_part.PointX - ud.posx + image_rect.Width, width);
        }

        prt.sizeDelta = new Vector2(width, logic_height);
#if UNITY_EDITOR
        name = string.Format("image:{0}:{1}", LineNo, UnitIdx);
#endif
    }
    public void Clear()
    {
        var count = image_infos_.Count;
        for(var i=0; i<count; ++i)
        {
            var image = image_infos_[i];
            image.Clear();
        }
        image_infos_.Clear();
    }

    UnityEngine.UI.Image image
    {
        get
        {
            if(image_ == null)
                image_ = GetComponent<UnityEngine.UI.Image>();
            return image_;
        }
    }
    UnityEngine.UI.Image image_ = null;
    List<ImageInfo> image_infos_ = new List<ImageInfo>();
}
