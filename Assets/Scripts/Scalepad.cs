using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MinorShift.Emuera;

public class Scalepad : MonoBehaviour
{
    void Start ()
    {
        GenericUtils.SetListenerOnClick(oneperone_btn, OnOnePerOneClick);
        GenericUtils.SetListenerOnClick(autofit_btn, OnAutoFit);
        slider.onValueChanged.AddListener(OnValueChanged);
        text.text = "1x";
    }
    public bool IsShow { get { return gameObject.activeSelf; } }

    void OnOnePerOneClick()
    {
        emuera_main.OnePerOne();
        UpdateSlider();
    }
    void OnAutoFit()
    {
        emuera_main.AutoFit();
        UpdateSlider();
    }
    void OnValueChanged(float value)
    {
        if(value >= 0)
            value = (1 + value);
        else
            value = (1 + value / 2);
        emuera_main.SetScaleValue(value);
        text.text = string.Format("{0:F1}x", value);
    }
    public void SetColor(Color sprite_color)
    {
        var images = GenericUtils.FindChildren<Image>(gameObject, true);
        var count = images.Count;
        Image image = null;
        for(int i=0; i<count; ++i)
        {
            image = images[i];
            if(image.sprite == null)
                continue;
            image.color = sprite_color;
        }
        var texts = GenericUtils.FindChildren<Text>(gameObject, true);
        count = texts.Count;
        Text text = null;
        for(int i=0; i<count; ++i)
        {
            text = texts[i];
            text.color = sprite_color;
        }
    }
    public void Show()
    {
        gameObject.SetActive(true);
        UpdateSlider();
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    void UpdateSlider()
    {
        var value = emuera_main.scale_value;
        text.text = string.Format("{0:F1}x", value);
        if(value > 1)
            value = value - 1;
        else
            value = (value - 1) * 2;
        slider.value = value;
    }

    public EmueraMain emuera_main;
    public GameObject oneperone_btn;
    public GameObject autofit_btn;
    public Slider slider;
    public Text text;
}
