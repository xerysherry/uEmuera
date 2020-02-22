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
        GenericUtils.SetListenerOnClick(option_button.gameObject, OnShowMenu2);

        orientation_lock_image = orientation_lock_button.GetComponent<Image>();
        GenericUtils.SetListenerOnClick(orientation_lock_button.gameObject, OnLockOrientationClick);

        GenericUtils.SetListenerOnClick(msg_confirm, OnMsgConfirm);
        GenericUtils.SetListenerOnClick(msg_cancel, OnMsgCancel);

        GenericUtils.SetListenerOnClick(menu_pad, OnMenuPad);
        GenericUtils.SetListenerOnClick(menu_1_resolution, OnMenuResolution);
        GenericUtils.SetListenerOnClick(menu_1_language, ShowLanguageBox);
        GenericUtils.SetListenerOnClick(menu_1_github, OnGithub);
        GenericUtils.SetListenerOnClick(menu_1_exit, OnMenuExit);

        GenericUtils.SetListenerOnClick(menu_2_back, OnMenu2Back);
        GenericUtils.SetListenerOnClick(menu_2_restart, OnMenu2Restart);
        GenericUtils.SetListenerOnClick(menu_2_gototitle, OnMenuGotoTitle);
        GenericUtils.SetListenerOnClick(menu_2_savelog, OnMenuSaveLog);
        GenericUtils.SetListenerOnClick(menu_2_intent, OnIntentBoxShow);
        GenericUtils.SetListenerOnClick(menu_2_exit, OnMenuExit);

        GenericUtils.SetListenerOnClick(resolution_pad, OnResolutionOut);
        GenericUtils.SetListenerOnClick(resolution_1080p, OnResolution1080p);
        GenericUtils.SetListenerOnClick(resolution_900p, OnResolution900p);
        GenericUtils.SetListenerOnClick(resolution_720p, OnResolution720p);
        GenericUtils.SetListenerOnClick(resolution_540p, OnResolution540p);

        GenericUtils.SetListenerOnClick(language_zhcn, OnSelectLanguage);
        GenericUtils.SetListenerOnClick(language_jp, OnSelectLanguage);
        GenericUtils.SetListenerOnClick(language_enus, OnSelectLanguage);

        GenericUtils.SetListenerOnClick(intentbox_L_left, OnIntentLLeft);
        GenericUtils.SetListenerOnClick(intentbox_L_right, OnIntentLRight);
        GenericUtils.SetListenerOnClick(intentbox_R_left, OnIntentRLeft);
        GenericUtils.SetListenerOnClick(intentbox_R_right, OnIntentRRight);
        GenericUtils.SetListenerOnClick(intentbox_close, OnIntentClose);
        GenericUtils.SetListenerOnClick(intentbox_reset, OnIntentReset);

        HideResolutionIcon();
        switch(ResolutionHelper.resolution_index)
        {
        case 2:
            resolution_900p_icon.SetActive(true);
            break;
        case 3:
            resolution_720p_icon.SetActive(true);
            break;
        case 4:
            resolution_540p_icon.SetActive(true);
            break;
        case 1:
        default:
            resolution_1080p_icon.SetActive(true);
            break;
        }
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
        if(auto_rotation)
        {
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            orientation_lock_image.sprite = lock_sprite;
        }
        else
        {
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            orientation_lock_image.sprite = unlock_sprite;
        }
    }

    public void ShowMenu()
    {
        menu_pad.SetActive(true);
        menu_1.SetActive(true);
    }

    void OnShowMenu2()
    {
        menu_pad.SetActive(true);
        menu_2.SetActive(true);
    }
    void OnMenu2Back()
    {
        if(EmueraThread.instance.Running())
        {
            ShowMessageBox(
                MultiLanguage.GetText("[Wait]"), 
                MultiLanguage.GetText("[WaitContent]"));
        }
        else
        {
            ShowMessageBox(
                MultiLanguage.GetText("[BackMenu]"),
                MultiLanguage.GetText("[BackMenuContent]"),
                () =>
                {
                    var emuera = GameObject.FindObjectOfType<EmueraMain>();
                    emuera.Clear();
                }, () => { });
        }
        HideMenu();
    }
    void OnMenu2Restart()
    {
        if(EmueraThread.instance.Running())
        {
            ShowMessageBox(
                MultiLanguage.GetText("[Wait]"),
                MultiLanguage.GetText("[WaitContent]"));
        }
        else
        {
            ShowMessageBox(
                MultiLanguage.GetText("[ReloadGame]"),
                MultiLanguage.GetText("[ReloadGameContent]"),
            () =>
            {
                var emuera = GameObject.FindObjectOfType<EmueraMain>();
                emuera.Restart();
            }, () => { });
        }
        HideMenu();
    }
    void OnMenuGotoTitle()
    {
        if(EmueraThread.instance.Running())
        {
            ShowMessageBox(
                MultiLanguage.GetText("[Wait]"),
                MultiLanguage.GetText("[WaitContent]"));
        }
        else
        {
            ShowMessageBox(
                MultiLanguage.GetText("[BackTitle]"),
                MultiLanguage.GetText("[BackTitleContent]"),
            () =>
            {
                MinorShift.Emuera.GlobalStatic.Console.GotoTitle();
            }, () => { });
        }
        HideMenu();
    }
    void OnMenuSaveLog()
    {
        var path = MinorShift.Emuera.Program.ExeDir;
        System.DateTime time = System.DateTime.Now;
        string fname = time.ToString("yyyyMMdd-HHmmss");
        path = path + fname + ".log";
        bool result = MinorShift.Emuera.GlobalStatic.Console.OutputLog(path);

        ShowMessageBox(MultiLanguage.GetText("[SaveLog]"), 
            result ? string.Format("{1}：\n{0}", path, MultiLanguage.GetText("[SavePath]")) : MultiLanguage.GetText("[Failure]"));
        HideMenu();
    }
    void OnMenuResolution()
    {
        resolution_pad.SetActive(true);
        HideMenu();
    }
    void OnMenuExit()
    {
        ShowMessageBox(
            MultiLanguage.GetText("[Exit]"),
            MultiLanguage.GetText("[ExitContent]"), 
            ()=> {
                Application.Quit();
            }, ()=> { });
        HideMenu();
    }

    void OnMenuPad()
    {
        HideMenu();
    }

    void OnResolutionOut()
    {
        resolution_pad.SetActive(false);
    }

    void OnResolution1080p()
    {
        ResolutionHelper.resolution_index = 1;
        ResolutionHelper.Apply();
        HideResolutionIcon();
        resolution_1080p_icon.SetActive(true);
    }

    void OnResolution900p()
    {
        ResolutionHelper.resolution_index = 2;
        ResolutionHelper.Apply();
        HideResolutionIcon();
        resolution_900p_icon.SetActive(true);
    }

    void OnResolution720p()
    {
        ResolutionHelper.resolution_index = 3;
        ResolutionHelper.Apply();
        HideResolutionIcon();
        resolution_720p_icon.SetActive(true);
    }

    void OnResolution540p()
    {
        ResolutionHelper.resolution_index = 4;
        ResolutionHelper.Apply();
        HideResolutionIcon();
        resolution_540p_icon.SetActive(true);
    }

    void HideResolutionIcon()
    {
        resolution_1080p_icon.SetActive(false);
        resolution_900p_icon.SetActive(false);
        resolution_720p_icon.SetActive(false);
        resolution_540p_icon.SetActive(false);
    }

    public void Ready()
    {
        var texts = inprogress.GetComponentsInChildren<Text>();
        var length = texts.Length;
        for(int i=0; i<length; ++i)
        {
            var text = texts[i];
            text.color = EmueraBehaviour.FontColor;
        }

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
        if(auto_rotation)
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

    void HideMenu()
    {
        menu_1.SetActive(false);
        menu_2.SetActive(false);
        menu_pad.SetActive(false);
    }

    void ShowMessageBox(string title, string content,
        System.Action confirm_callback = null,
        System.Action cancel_callback = null)
    {
        msg_confirm_callback = confirm_callback;
        msg_cancel_callback = cancel_callback;
        msg_cancel.SetActive(msg_cancel_callback != null);
        msg_title.text = title;
        msg_content.text = content;
        msg_box.SetActive(true);
    }
    void HideMessageBox()
    {
        msg_title.text = "";
        msg_content.text = "";
        msg_confirm_callback = null;
        msg_cancel_callback = null;
        msg_box.SetActive(false);
    }
    void OnMsgConfirm()
    {
        if(msg_confirm_callback != null)
            msg_confirm_callback();
        HideMessageBox();
    }
    void OnMsgCancel()
    {
        if(msg_cancel_callback != null)
            msg_cancel_callback();
        HideMessageBox();
    }

    public void ShowLanguageBox()
    {
        HideMenu();
        language_box.SetActive(true);
    }
    void OnSelectLanguage(UnityEngine.EventSystems.PointerEventData e)
    {
        MultiLanguage.SetLanguage(e.pointerPress.name);
        language_box.SetActive(false);
    }

    void OnGithub()
    {
        Application.OpenURL("https://github.com/xerysherry/uEmuera/releases");
    }

    void OnIntentBoxShow()
    {
        intentbox.SetActive(true);
        HideMenu();
    }
    void OnIntentLLeft()
    {
        int value = PlayerPrefs.GetInt("IntentBox_L", 0);
        value -= 1;
        if(value < 0)
            value = 0;
        PlayerPrefs.SetInt("IntentBox_L", value);
        intentbox_L_text.text = value.ToString();

        EmueraContent.instance.SetIntentBox(PlayerPrefs.GetInt("IntentBox_L", 0),
                                            PlayerPrefs.GetInt("IntentBox_R", 0));
    }
    void OnIntentLRight()
    {
        int value = PlayerPrefs.GetInt("IntentBox_L", 0);
        value += 1;
        if(value > 99)
            value = 99;
        PlayerPrefs.SetInt("IntentBox_L", value);
        intentbox_L_text.text = value.ToString();

        EmueraContent.instance.SetIntentBox(PlayerPrefs.GetInt("IntentBox_L", 0),
                                            PlayerPrefs.GetInt("IntentBox_R", 0));
    }
    void OnIntentRLeft()
    {
        int value = PlayerPrefs.GetInt("IntentBox_R", 0);
        value += 1;
        if(value > 99)
            value = 99;
        PlayerPrefs.SetInt("IntentBox_R", value);
        intentbox_R_text.text = value.ToString();

        EmueraContent.instance.SetIntentBox(PlayerPrefs.GetInt("IntentBox_L", 0),
                                            PlayerPrefs.GetInt("IntentBox_R", 0));
    }
    void OnIntentRRight()
    {
        int value = PlayerPrefs.GetInt("IntentBox_R", 0);
        value -= 1;
        if(value < 0)
            value = 0;
        PlayerPrefs.SetInt("IntentBox_R", value);
        intentbox_R_text.text = value.ToString();

        EmueraContent.instance.SetIntentBox(PlayerPrefs.GetInt("IntentBox_L", 0),
                                            PlayerPrefs.GetInt("IntentBox_R", 0));
    }
    void OnIntentClose()
    {
        intentbox.SetActive(false);
    }
    void OnIntentReset()
    {
        PlayerPrefs.SetInt("IntentBox_L", 0);
        PlayerPrefs.SetInt("IntentBox_R", 0);
        intentbox_L_text.text = "0";
        intentbox_R_text.text = "0";
        EmueraContent.instance.SetIntentBox(0, 0);
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

    public GameObject msg_box;
    public Text msg_title;
    public Text msg_content;
    public GameObject msg_confirm;
    public GameObject msg_cancel;
    System.Action msg_confirm_callback;
    System.Action msg_cancel_callback;

    public GameObject menu_pad;
    public GameObject menu_1;
    public GameObject menu_1_resolution;
    public GameObject menu_1_language;
    public GameObject menu_1_github;
    public GameObject menu_1_exit;

    public GameObject menu_2;
    public GameObject menu_2_back;
    public GameObject menu_2_restart;
    public GameObject menu_2_gototitle;
    public GameObject menu_2_savelog;
    public GameObject menu_2_intent;
    public GameObject menu_2_exit;

    public GameObject resolution_pad;
    public GameObject resolution_1080p;
    public GameObject resolution_1080p_icon;
    public GameObject resolution_900p;
    public GameObject resolution_900p_icon;
    public GameObject resolution_720p;
    public GameObject resolution_720p_icon;
    public GameObject resolution_540p;
    public GameObject resolution_540p_icon;

    public GameObject language_box;
    public GameObject language_zhcn;
    public GameObject language_jp;
    public GameObject language_enus;

    public GameObject intentbox;
    public GameObject intentbox_L_left;
    public GameObject intentbox_L_right;
    public GameObject intentbox_R_left;
    public GameObject intentbox_R_right;
    public GameObject intentbox_close;
    public GameObject intentbox_reset;
    public Text intentbox_L_text;
    public Text intentbox_R_text;

    bool auto_rotation
    {
        get
        {
            return Screen.autorotateToLandscapeLeft &&
                    Screen.autorotateToLandscapeRight &&
                    Screen.autorotateToPortrait &&
                    Screen.autorotateToPortraitUpsideDown;
        }
    }
}
