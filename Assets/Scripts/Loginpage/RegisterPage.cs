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

        // Validation
        if (string.IsNullOrEmpty(_name.text) || string.IsNullOrEmpty(_email.text) || 
            string.IsNullOrEmpty(_password2.text))
        {
            NotificationController.Get().Show("Validation Error", "Please fill in all fields.", null, null);
            return;
        }

        if (_password2.text.Length < 8)
        {
            NotificationController.Get().Show("Validation Error", "Password must be at least 8 characters.", null, null);
            return;
        }

        // Assuming _password1 is the confirm password field or vice versa. 
        // User code had _password1 != _password2 check commented out. I'm enabling it.
        // It seems _password1 might be hidden or unused in some prefabs, but logic implies it exists.
        // If _password1 is active, check match.
        if (_password1 != null && _password1.gameObject.activeSelf && _password1.text != _password2.text)
        {
           NotificationController.Get().Show("Validation Error", "Passwords do not match.", null, null);
           return;
        }

        LoadingHandler.Get().SetVisible(true);
        ApiClient.Get().Register(_name.text, _password1.text, _email.text,
            OnSuccess, OnFail);
    }

    private void OnFail(string obj)
    {
        LoadingHandler.Get().SetVisible(false);
        
        string message = obj;
        if (obj.Contains("401"))
        {
             // 401 on register is unusual but implies unauthorized, maybe token issue or similar.
             // User requested specific message for "wrong password" context which applies to Login, 
             // but applied 401 check here for consistency.
            message = "Registration failed. Please try again.";
        }

        // Use popup with OK button instead of auto-hide
        NotificationController.Get().Show("Registration Error", message, null, null);
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
