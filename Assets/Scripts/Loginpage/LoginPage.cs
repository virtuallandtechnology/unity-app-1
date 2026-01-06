using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ApiClient;

namespace VirtualLand
{
    public class LoginPage : BootStrapBasePanel
    {
        [SerializeField] private TMP_InputField _loginusername;
        [SerializeField] private TMP_InputField _loginpassword;
        [SerializeField] private GameObject LoginePopup;
        public Action<ApiClient.User> LoginAction;

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
            {
                NotificationController.Get().Show("Validation Error", "Please enter username and password.", null, null);
                return;
            }

            if (_loginpassword.text.Length < 5)
            {
                NotificationController.Get().Show("Validation Error", "Password must be at least 8 characters.", null, null);
                return;
            }

            LoadingHandler.Get().SetVisible(true);
            Get().Login(_loginusername.text, _loginpassword.text, OnSuccess, OnFail);
        }

        private void OnFail(string obj)
        {
            print(obj);
            LoadingHandler.Get().SetVisible(false);
            
            string message = obj;
            if (obj.Contains("401"))
            {
                message = "Incorrect email or password.";
            }

            // Use popup with OK button instead of auto-hide
            NotificationController.Get().Show("Login Error", message, null, null);
        }

        private void OnSuccess(ApiResponse<UserData> response)
        {
            print(response + "RRRRRRRRR");

            LoadingHandler.Get().SetVisible(false);
            Debug.Log(response.result);
            PlayerPrefs.SetString("username", response.result.user.GetUsername());
            string token = response.result.token;
            PlayerPrefs.SetString("token", token);
            
            // Pass the user object to the bootstrap controller
            LoginAction?.Invoke(response.result.user);
            
            LoginePopup.SetActive(false);
            // RequestWalletsUpdate is now handled in ProceedToHome
            // ApiClient.Get().RequestWalletsUpdate();
        }
    }
}