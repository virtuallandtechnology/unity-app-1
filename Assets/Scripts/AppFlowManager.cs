// File: Assets/Scripts/VL_NewSystem/Managers/AppFlowManager.cs
using UnityEngine;
using VirtualLand.NewSystem.Network;
using VirtualLand.NewSystem.UI;
using VirtualLand.NewSystem.Models;
using TMPro;
using System.Collections;

namespace VirtualLand.NewSystem.Managers
{
    public class AppFlowManager : MonoBehaviour
    {
        [Header("Inputs - Sign Up")]
        public TMP_InputField regUsername;
        public TMP_InputField regEmail;
        public TMP_InputField regPassword;

        [Header("Inputs - Login")]
        public TMP_InputField loginEmail;
        public TMP_InputField loginPassword;
        public GameObject MainMnue;

        private void Start()
        {
            StartCoroutine(InitFlow());
        }

        IEnumerator InitFlow()
        {
            AuthUIManager.Instance.ShowLoading();

            yield return new WaitForSeconds(2f);

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                AuthUIManager.Instance.ShowConnectionError();
            }
            else
            {
                CheckForUpdates();
            }
        }

        public void OnRetryConnection()
        {
            AuthUIManager.Instance.StopWifiAnimation();
            StartCoroutine(InitFlow());
        }

        void CheckForUpdates()
        {
            APIManager.Instance.CheckVersion(
                onForceUpdate: (isForce) =>
                {
                    AuthUIManager.Instance.ShowForceUpdate();
                },
                onLatest: () =>
                {
                    CheckAutoLogin();
                }
            );
        }

        void CheckAutoLogin()
        {
            string savedToken = PlayerPrefs.GetString("AUTH_TOKEN", "");

            if (!string.IsNullOrEmpty(savedToken))
            {
                string savedAvatar = PlayerPrefs.GetString("USER_AVATAR", "DefaultAvatar");
                AuthUIManager.Instance.UpdateAvatarDisplay(savedAvatar);

                EnterMainMenu();
            }
            else
            {
                AuthUIManager.Instance.ShowSignUp();
            }
        }


        public void Click_SignUp()
        {
            RegisterBody body = new RegisterBody
            {
                username = regUsername.text,
                email = regEmail.text,
                password = regPassword.text,
                profile = new ProfileBody
                {
                    name = "regUsername.text",
                    avatar = "" 
                }
            };

            APIManager.Instance.Register(body,
                (result) => {
                    SaveUserAndProceed(result);
                    AuthUIManager.Instance.ShowErrorNotification("Registration Successful!");
                },
                (error) => {
                    Debug.LogError("Registration Error: " + error); 
                    AuthUIManager.Instance.ShowErrorNotification(error);
                }
            );
        }

        public void Click_Login()
        {
            LoginBody body = new LoginBody
            {
                email = loginEmail.text,
                password = loginPassword.text
            };

            APIManager.Instance.Login(body,
                (result) => {
                    SaveUserAndProceed(result);
                    AuthUIManager.Instance.ShowErrorNotification("Welcome back!");
                },
                (error) => {
                    AuthUIManager.Instance.ShowErrorNotification(error);
                }
            );
        }

        public void Click_GoogleLogin()
        {
            AuthUIManager.Instance.ShowErrorNotification("Google Login not implemented yet.");
        }

        public void GoToLoginPage()
        {
            AuthUIManager.Instance.ShowLogin();
        }

        public void GoToSignUpPage()
        {
            AuthUIManager.Instance.ShowSignUp();
        }



        private void SaveUserAndProceed(AuthResult data)
        {
            PlayerPrefs.SetString("AUTH_TOKEN", data.token);

            string avatarToSave = "DefaultAvatar"; // یک اسم پیش‌فرض

            if (data.user != null && data.user.profile != null && !string.IsNullOrEmpty(data.user.profile.avatar))
            {
                avatarToSave = data.user.profile.avatar;
            }
            else
            {
                Debug.LogWarning("User profile or avatar is null. Using default.");
            }

            PlayerPrefs.SetString("USER_AVATAR", avatarToSave);
            AuthUIManager.Instance.UpdateAvatarDisplay(avatarToSave);

            PlayerPrefs.Save();
            EnterMainMenu();
        }

        private void EnterMainMenu()
        {
            AuthUIManager.Instance.CloseAllPanels();
            MainMnue.SetActive(true);
            // یا SceneManager.LoadScene("MainMenu");
            Debug.Log("User Entered Main Menu");
        }

        public void ChangeUserAvatar(string newAvatarResourceName)
        {
            string token = PlayerPrefs.GetString("AUTH_TOKEN");
            ProfileBody body = new ProfileBody
            {
                name = "MyName", 
                avatar = newAvatarResourceName
            };

            APIManager.Instance.UpdateProfile(body, token,
                (success) => {
                    PlayerPrefs.SetString("USER_AVATAR", newAvatarResourceName);
                    AuthUIManager.Instance.UpdateAvatarDisplay(newAvatarResourceName);
                    AuthUIManager.Instance.ShowErrorNotification("Avatar Updated!");
                },
                (error) => {
                    AuthUIManager.Instance.ShowErrorNotification("Failed to update avatar: " + error);
                }
            );
        }
    }
}