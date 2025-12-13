using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ApiClient;

public class Profile : MonoBehaviour
{
    public TMP_InputField _username;
    public TMP_Text _email;
    //public string token;
    public Image Avatar;
    public Image AvatarinSelectAvatar;
    private int _currentProfileID;

    private void OnEnable()
    {

        _email.text = ApiClient.GetPlayer().email;
        if ((ApiClient.GetPlayer().profile != null))
        {
            Avatar.sprite = AvatarsConfig.Instance.Avatars[ApiClient.GetPlayer().profile.avatar_id].sprite;
            AvatarinSelectAvatar.sprite = AvatarsConfig.Instance.Avatars[ApiClient.GetPlayer().profile.avatar_id].sprite;
        }

        _username.text = ApiClient.GetPlayer().GetUsername();
        //if ((UserData.player.user.profile != null) && !string.IsNullOrEmpty(UserData.player.user.profile.nickname))
        //    _username.text = UserData.player.user.profile.nickname;
        //else
        //{
        //    if (!string.IsNullOrEmpty((string)UserData.player.user.username))
        //        _username.text = (string)UserData.player.user.username;
        //    else
        //        if (!string.IsNullOrEmpty(UserData.player.user.name))
        //        _username.text = UserData.player.user.name;
        //    else
        //        _username.text = UserData.player.user.email;
        //}

    }

    public void SetProfile(int id)
    {
        _currentProfileID = id;
        Avatar.sprite = AvatarsConfig.Instance.Avatars[_currentProfileID].sprite;
    }

    public void UpdateProfile()
    {
        var currentprofile = new ApiClient.Profile();
        if (!string.IsNullOrEmpty(_username.text))
            currentprofile.nickname = _username.text;
        currentprofile.avatar_id = _currentProfileID;


        ApiClient.Get().UpdateProfile(currentprofile, onsuccess, onFail);
        LoadingHandler.Get().SetVisible(true);
    }

    private void onFail(string obj)
    {
        Debug.Log("fail");
        LoadingHandler.Get().SetVisible(true);
        NotificationController.Get().Show(obj);
    }

    private void onsuccess(ApiResponse<User> response)
    {
        Debug.Log("update success");
        LoadingHandler.Get().SetVisible(false);
        NotificationController.Get().Show("Profile Updated Successfully");
    }


    public void LogOut()
    {
        PlayerPrefs.DeleteKey("token");
        SceneManager.LoadScene(0);
    }


}
