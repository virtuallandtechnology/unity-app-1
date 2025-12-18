using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ApiClient;

namespace VirtualLand
{
    public class Profile : MonoBehaviour
    {
        [Header("UI References")]
        public TMP_InputField _username;
        public TMP_Text _email;

        [Header("Avatar Images")]
        public Image Avatar;
        public Image AvatarinSelectAvatar;
        public Image avatarImageProfile;
        public Image avatarImageHome;

        private int _currentProfileID;

        private void OnEnable()
        {
            if (LoadingHandler.Get() != null) LoadingHandler.Get().SetVisible(true);

            ApiClient.Get().GetProfileInfo(OnGetInfoSuccess, OnFail);
        }

        private void OnGetInfoSuccess(ApiResponse<User> response)
        {
            if (LoadingHandler.Get() != null) LoadingHandler.Get().SetVisible(false);

            if (response.result != null)
            {
                var user = response.result;

                if (_email != null) _email.text = user.email;

                if (_username != null) _username.text = user.GetUsername();

                if (user.profile != null)
                {
                    _currentProfileID = user.profile.avatar_id;
                    UpdateAllImages(_currentProfileID);
                }
            }
        }

        public void SetProfile(int id)
        {
            _currentProfileID = id;

            UpdateAllImages(_currentProfileID);

            UpdateProfile();
        }

        private void UpdateAllImages(int id)
        {
            if (AvatarsConfig.Instance == null || AvatarsConfig.Instance.Avatars == null) return;

            if (id >= 0 && id < AvatarsConfig.Instance.Avatars.Count)
            {
                Sprite selectedSprite = AvatarsConfig.Instance.Avatars[id].sprite;

                if (Avatar != null) Avatar.sprite = selectedSprite;
                if (AvatarinSelectAvatar != null) AvatarinSelectAvatar.sprite = selectedSprite;
                if (avatarImageProfile != null) avatarImageProfile.sprite = selectedSprite;
                if (avatarImageHome != null) avatarImageHome.sprite = selectedSprite;
            }
        }

        public void UpdateProfile()
        {
            var currentprofile = new ApiClient.Profile();

            currentprofile.avatar_id = _currentProfileID;

            ApiClient.Get().UpdateProfile(currentprofile, onUpdateSuccess, OnFail);
        }

        private void OnFail(string errorMsg)
        {
            Debug.LogError("Error: " + errorMsg);

            if (LoadingHandler.Get() != null) LoadingHandler.Get().SetVisible(false);
            if (NotificationController.Get() != null) NotificationController.Get().Show(errorMsg);
        }

        private void onUpdateSuccess(ApiResponse<User> response)
        {
            Debug.Log("Update success");

            if (LoadingHandler.Get() != null) LoadingHandler.Get().SetVisible(false);
            if (NotificationController.Get() != null) NotificationController.Get().Show("Profile Updated Successfully");
        }

        public void LogOut()
        {
            PlayerPrefs.DeleteKey("token");
            SceneManager.LoadScene(0);
        }
    }
}