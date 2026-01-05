using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ApiClient;

public class RegisterPage : BootStrapBasePanel
{
    [SerializeField] private TMP_InputField _name;
    [SerializeField] private TMP_InputField _email;
    [SerializeField] private TMP_InputField _password1;
    [SerializeField] private TMP_InputField _password2;
    [SerializeField] private GameObject Home;
    public Action<ApiClient.User> RegisterAction;


    public void Register()
    {
        if (!GetVisible())
        {
            Show();
            return;
        }
        //if (_password1.text != _password2.text)
        //{
        //    //_log.text = "Password1 is not like Password2 ";
        //    NotificationController.Get().Show("Password1 is not like Password2 ");
        //    return;
        //}
        //_log.text = "";
        //SetFormEnable(false);
        LoadingHandler.Get().SetVisible(true);
        ApiClient.Get().Register(_name.text, _password2.text, _email.text,
            OnSuccess, OnFail);

        //ApiClient.Get().Register("AAA","12345678", "aa@bb.cc",
        //    OnSuccess, OnFail);
    }
    private void OnFail(string obj)
    {
        LoadingHandler.Get().SetVisible(false);
       // _log.text = obj;
        NotificationController.Get().Show(obj);
    }

    private void OnSuccess(ApiResponse<UserData> response)
    {
        LoadingHandler.Get().SetVisible(false);
        Debug.Log(response.result);
        string token = response.result.token;
       
        PlayerPrefs.SetString("token", token);
        //Debug.Log(response.result.user.profile.nickname + " FFFFFFFFFFFFFFFFFFFFFFFFFFF");
        PlayerPrefs.SetString("username", _name.text);
        _root.SetActive(false);
        // Home activation is now handled by BootStrapController via RegisterAction
        // Home.SetActive(true);
        
        // Construct a basic user object since Register response might be limited or we want to pass specific data
        // Check if response.result.user is null, if so create one
        var user = response.result.user;
        if (user == null)
        {
            user = new User();
            user.username = _name.text;
            user.email = _email.text;
            // Avatar might define a default
        }
        
        RegisterAction?.Invoke(user);
    }
}
