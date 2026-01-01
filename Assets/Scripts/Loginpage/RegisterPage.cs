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
        SceneManager.LoadScene(1);
    }
}
