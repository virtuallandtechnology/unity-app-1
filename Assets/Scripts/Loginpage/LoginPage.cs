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

            LoadingHandler.Get().SetVisible(true);
            Get().Login(_loginusername.text, _loginpassword.text, OnSuccess, OnFail);
        }

        private void OnFail(string obj)
        {
            print(obj);
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
            ApiClient.Get().RequestWalletsUpdate();
        }
    }
}