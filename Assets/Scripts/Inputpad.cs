using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inputpad : MonoBehaviour
{
	void Start ()
    {
        GenericUtils.SetListenerOnClick(confirm_btn, OnConfirm);
        GenericUtils.SetListenerOnClick(confirm_btn, OnRepeat);
    }

    void OnConfirm()
    {
        lastinput = inputfield.text;
        EmueraThread.instance.Input(lastinput, true);
        inputfield.text = "";
    }
    void OnRepeat()
    {
        if(lastinput != null)
            EmueraThread.instance.Input(lastinput, true);
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
    public GameObject repeat_btn;
    public InputField inputfield;

    string lastinput = null;
}
