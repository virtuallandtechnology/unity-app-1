using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ApiClient;

public class LoginPage : BootStrapBasePanel
{
    [SerializeField] private TMP_InputField _loginusername;
    [SerializeField] private TMP_InputField _loginpassword;

    public override void Show()
    {
        //base.Show();
        //_loginusername.text = PlayerPrefs.HasKey("username") ? PlayerPrefs.GetString("username") : "";
        //_loginpassword.text = "";
    }
    public void Login()
    {
        Debug.Log("**************");
        Get().Login("Masood1@gmail.com", "12345678", OnSuccess, OnFail);
    }

    public void Register()
    {
        ApiClient.Get().Register("Masood1@gmail.com", "12345678", "Masood1@gmail.com",
          OnSuccess, OnFail);
    }
    private void OnFail(string obj)
    {
        //NotificationController.Get().Show(obj);
        //LoadingHandler.Get().SetVisible(false);
    }

    private void OnSuccess(ApiResponse<UserData> response)
    {
        //LoadingHandler.Get().SetVisible(false);
        //Debug.Log(response.result);
        //PlayerPrefs.SetString("username", _loginusername.text);
        //string token = response.result.token;
        //PlayerPrefs.SetString("token", token);

        //SceneManager.LoadScene(1);

    }
}

