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

        inputfield.contentType = InputField.ContentType.Standard;
        inputfield.onValueChanged.AddListener(InputFieldValueChanged);
    }

	void InputFieldValueChanged(string str)
	{
        if (str == "")
            return;

        RefreshInputpad();

        switch (console.InputType)
        {
            case MinorShift.Emuera.GameProc.InputType.IntValue:
            case MinorShift.Emuera.GameProc.InputType.StrValue:
                if (console.IsWaintingOnePhrase)
                    OnConfirm();
                break;
            default:
                break;
		}
    }

    void RefreshInputpad()
    {
        console = MinorShift.Emuera.GlobalStatic.Console;
        if (background != console.bgColor)
        {
            background = console.bgColor;
            SetTextColor(GenericUtils.ToUnityColor(background));
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
        RefreshInputpad();
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

    MinorShift.Emuera.GameView.EmueraConsole console;

    string lastinput = null;
}
