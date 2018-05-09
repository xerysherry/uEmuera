using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionWindow : MonoBehaviour
{
	// Use this for initialization
	void Start ()
    {
        GenericUtils.SetListenerOnClick(quick_button.gameObject, OnQuickButtonClick);
        GenericUtils.SetListenerOnClick(input_button.gameObject, OnInputPadButtonClick);
        GenericUtils.SetListenerOnClick(magnifier_button.gameObject, OnScalePadButtonClick);

        orientation_lock_image = orientation_lock_button.GetComponent<Image>();
        GenericUtils.SetListenerOnClick(orientation_lock_button.gameObject, OnLockOrientationClick);
    }
	
	void Update () {
		
	}

    void OnQuickButtonClick()
    {
        if(quick_buttons.IsShow)
        {
            quick_buttons.Hide();
            SwitchButton(-1);
        }
        else
        {
            input_pad.Hide();
            scale_pad.Hide();
            quick_buttons.Show();
            EmueraContent.instance.SetLastButtonGeneration(
                EmueraContent.instance.button_generation);

            SwitchButton(0);
        }
    }
    void OnInputPadButtonClick()
    {
        if(input_pad.IsShow)
        {
            input_pad.Hide();
            SwitchButton(-1);
        }
        else
        {
            quick_buttons.Hide();
            scale_pad.Hide();
            input_pad.Show();
            SwitchButton(1);
        }
    }
    void OnScalePadButtonClick()
    {
        if(scale_pad.IsShow)
        {
            scale_pad.Hide();
            SwitchButton(-1);
        }
        else
        {
            quick_buttons.Hide();
            input_pad.Hide();
            scale_pad.Show();
            SwitchButton(2);
        }
    }
    void OnLockOrientationClick()
    {
        if(Screen.autorotateToLandscapeLeft &&
            Screen.autorotateToLandscapeRight &&
            Screen.autorotateToPortrait &&
            Screen.autorotateToPortraitUpsideDown)
        {
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            switch(Input.deviceOrientation)
            {
            case DeviceOrientation.LandscapeLeft:
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                break;
            case DeviceOrientation.LandscapeRight:
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
                Screen.orientation = ScreenOrientation.LandscapeRight;
                break;
            case DeviceOrientation.PortraitUpsideDown:
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = true;
                Screen.orientation = ScreenOrientation.PortraitUpsideDown;
                break;
            case DeviceOrientation.Portrait:
            default:
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = true;
                Screen.orientation = ScreenOrientation.Portrait;
                break;
            }
            orientation_lock_image.sprite = lock_sprite;
        }
        else
        {
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.orientation = ScreenOrientation.AutoRotation;
            orientation_lock_image.sprite = unlock_sprite;
        }
    }

    public void Ready()
    {
        var texts = inprogress.GetComponentsInChildren<Text>();
        foreach(var text in texts)
            text.color = EmueraBehaviour.FontColor;

        var buttoncolor = EmueraBehaviour.FontColor;
        buttoncolor.a = 0.6f;
        quick_button.GetComponent<Image>().color = buttoncolor;
        input_button.GetComponent<Image>().color = buttoncolor;
        magnifier_button.GetComponent<Image>().color = buttoncolor;
        option_button.GetComponent<Image>().color = buttoncolor;
        orientation_lock_image.color = buttoncolor;
        scale_pad.SetColor(buttoncolor);
        input_pad.SetColor(buttoncolor, 
            GenericUtils.ToUnityColor(MinorShift.Emuera.Config.BackColor));

        buttoncolor.a = 1.0f;
        button_shadows = new List<Shadow>();
        var shadow = quick_button.GetComponent<Shadow>();
        shadow.effectColor = buttoncolor;
        button_shadows.Add(shadow);
        shadow = input_button.GetComponent<Shadow>();
        shadow.effectColor = buttoncolor;
        button_shadows.Add(shadow);
        shadow = magnifier_button.GetComponent<Shadow>();
        shadow.effectColor = buttoncolor;
        button_shadows.Add(shadow);
        shadow = option_button.GetComponent<Shadow>();
        shadow.effectColor = buttoncolor;
        button_shadows.Add(shadow);
    }

    public void ShowGameButton(bool value)
    {
        game_button.SetActive(value);
        if( Screen.autorotateToLandscapeLeft &&
            Screen.autorotateToLandscapeRight &&
            Screen.autorotateToPortrait &&
            Screen.autorotateToPortraitUpsideDown)
            orientation_lock_image.sprite = unlock_sprite;
        else
            orientation_lock_image.sprite = lock_sprite;
    }

    public void ShowInProgress(bool value)
    {
        inprogress.SetActive(value);
    }

    void SwitchButton(int index)
    {
        for(int i=0; i < button_shadows.Count; ++i)
        {
            var shadow = button_shadows[i];
            shadow.enabled = (i == index);
        }
    }

    public GameObject game_button;
    public Button quick_button;
    public Button input_button;
    public Button magnifier_button;
    public Button option_button;

    public Button orientation_lock_button;
    Image orientation_lock_image;
    public Sprite lock_sprite;
    public Sprite unlock_sprite;

    public GameObject inprogress;
    List<Shadow> button_shadows;

    public QuickButtons quick_buttons;
    public Inputpad input_pad;
    public Scalepad scale_pad;
}
