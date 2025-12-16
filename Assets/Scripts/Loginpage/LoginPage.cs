using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ApiClient;

public class LoginPage : BootStrapBasePanel
{
    [SerializeField] private TMP_InputField _loginusername;
    [SerializeField] private TMP_InputField _loginpassword;
   
    [SerializeField] private GameObject LoginePopup;
    public Action LoginAction;

    public override void Show()
    {
        base.Show();
        _loginusername.text = PlayerPrefs.HasKey("username") ? PlayerPrefs.GetString("username") : "";
       
    }
    
    public void Login()
    {
        if (!GetVisible())
        {
            Show();
            return;
        }

        if (string.IsNullOrEmpty(_loginusername.text) ||
            string.IsNullOrEmpty(_loginpassword.text))
            return;

        //_log.text = "";
        LoadingHandler.Get().SetVisible(true);
        Get().Login(_loginusername.text, _loginpassword.text, OnSuccess, OnFail);
    }
    private void OnFail(string obj)
    {
        // _log.text = obj;
        print(obj);
       // NotificationController.Get().Show(obj);
        //LoadingHandler.Get().SetVisible(false);
    }

    private void OnSuccess(ApiResponse<UserData> response)
    {
        print(response + "RRRRRRRRR");
        LoadingHandler.Get().SetVisible(false);
        Debug.Log(response.result);
        PlayerPrefs.SetString("username", response.result.user.profile.nickname);
        string token = response.result.token;
        PlayerPrefs.SetString("token", token);
        LoginAction.Invoke();
        LoginePopup.SetActive(false);
       // SceneManager.LoadScene(1);

    }




    //private TouchScreenKeyboard keyboard;



    //public void OnInputFieldSelected(string _)
    //{
    //    // Close TMP’s internally opened keyboard (optional)
    //    if (keyboard != null && keyboard.active)
    //        keyboard.active = false;

    //    // Reopen with your custom settings
    //    keyboard = TouchScreenKeyboard.Open(
    //        _loginusername.text,
    //        TouchScreenKeyboardType.Default,
    //        autocorrection: true,
    //        multiline: true,
    //        secure: false,
    //        alert: true
    //    );
    //    TouchScreenKeyboard.hideInput=false;
    //    Debug.Log("Custom keyboard opened!");
    //}  
    
    //public void OnInputFieldSelected2(string _)
    //{

    //    // Reopen with your custom settings
    //    keyboard = TouchScreenKeyboard.Open(
    //        _loginusername.text,
    //        TouchScreenKeyboardType.Default,
    //        autocorrection: true,
    //        multiline: true,
    //        secure: false,
    //        alert: true
    //    );
    //    TouchScreenKeyboard.hideInput=true;
    //    Debug.Log("Custom keyboard opened!");
    //}
}

