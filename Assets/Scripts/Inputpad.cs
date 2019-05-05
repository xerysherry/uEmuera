using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inputpad : MonoBehaviour
{
	void Start ()
    {
        GenericUtils.SetListenerOnClick(confirm_btn, OnConfirm);
        GenericUtils.SetListenerOnClick(repeat_btn, OnRepeat);
    }
    void Update()
    {
        var console = MinorShift.Emuera.GlobalStatic.Console;
        if(background != console.bgColor)
        {
            background = console.bgColor;
            SetTextColor(GenericUtils.ToUnityColor(background));
        }

        var now_type = console.InputType;
        if(now_type == lastinputtype)
            return;

        lastinputtype = now_type;
        switch(lastinputtype)
        {
        case MinorShift.Emuera.GameProc.InputType.IntValue:
            inputfield.contentType = InputField.ContentType.IntegerNumber;
            inputfield.gameObject.SetActive(true);
            break;
        case MinorShift.Emuera.GameProc.InputType.StrValue:
            inputfield.contentType = InputField.ContentType.Standard;
            inputfield.gameObject.SetActive(true);
            break;
        case MinorShift.Emuera.GameProc.InputType.Void:
        case MinorShift.Emuera.GameProc.InputType.EnterKey:
        case MinorShift.Emuera.GameProc.InputType.AnyKey:
            inputfield.gameObject.SetActive(false);
            break;
        }
    }
    void OnConfirm()
    {
        if(inputfield.gameObject.activeSelf)
        {
            lastinput = inputfield.text;
            EmueraThread.instance.Input(lastinput, true);
        }
        else
            EmueraThread.instance.Input("", true);
        inputfield.text = "";
    }
    void OnRepeat()
    {
        if(inputfield.gameObject.activeSelf)
            inputfield.text = lastinput;
        else
            EmueraThread.instance.Input(lastinput, true);
    }
    void OnClear()
    {
        inputfield.text = "";
    }
    public void SetColor(Color sprite_color, Color text_color)
    {
        var images = GenericUtils.FindChildren<Image>(gameObject, true);
        var length = images.Count;
        for(int i=0; i<length; ++i)
        {
            var image = images[i];
            if(image.sprite == null)
                continue;
            image.color = sprite_color;
        }
    }
    void SetTextColor(Color text_color)
    {
        inputfield.textComponent.color = text_color;
    }
    public void Show()
    {
        gameObject.SetActive(true);
        inputfield.text = "";
        lastinput = null;
    }
    public void Hide()
    {
        gameObject.SetActive(false);
        inputfield.text = "";
        lastinput = null;
    }
    public bool IsShow { get { return gameObject.activeSelf; } }

    public GameObject confirm_btn;
    public InputField inputfield;
    public GameObject repeat_btn;

    uEmuera.Drawing.Color background;

    MinorShift.Emuera.GameProc.InputType lastinputtype 
        = MinorShift.Emuera.GameProc.InputType.Void;
    string lastinput = null;
}
