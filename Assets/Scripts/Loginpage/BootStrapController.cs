using System;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BootStrapController : MonoBehaviour
{
    public LoginPage _loginpage;
    public RegisterPage _registerpage;
    public UpdatePage _updatepage;
    public NoInternetPanel _noInternetpage;

    [SerializeField] private Button _LoginButton;
    [SerializeField] private Button _RegisterButton;
    [SerializeField] private GameObject _buttonRoot;
    public bool AutoLogin = false;
    public ForceUpdateData ForceUpdateData;
    private void Start()
    {
        //_LoginButton.onClick.RemoveAllListeners();
        //_LoginButton.onClick.AddListener(_loginpage.Login);
        //_RegisterButton.onClick.RemoveAllListeners();
        //_RegisterButton.onClick.AddListener(_registerpage.Register);
        //LoadingHandler.Get().SetVisible(true);
        //Getverion();
    }

    public void Getverion()
    {
        LoadingHandler.Get().SetVisible(true);
        ApiClient.Get().GetServerConfig(onGetVersionSuccess, onfail);
    }

    private void onfail(string obj)
    {
        NotificationController.Get().Show(obj);
        LoadingHandler.Get().SetVisible(false);
        _buttonRoot.SetActive(false);
        _noInternetpage.Show();
    }

    private void onGetVersionSuccess(ApiClient.ApiResponse<ApiClient.Result> response)
    {
        LoadingHandler.Get().SetVisible(false);
        if (response != null)
        {
            ForceUpdateData = new ForceUpdateData(response.result.VERSION_INFO, Application.version);

#if (UNITY_EDITOR)

            //int st = 2;
            //ForceUpdateData.ForceUpdate = st == 1;
            //ForceUpdateData.CanUpdate = st == 2;
            //ForceUpdateData.NoUpdate = st == 0;
#endif

            if (ForceUpdateData.ForceUpdate)
            {
                _buttonRoot.SetActive(false);
                _updatepage.Show();
                _updatepage.ShowForceUpdate(ForceUpdateData.GetCurrentVersion,
                    ForceUpdateData.GetServerVersion(), response.result.VERSION_INFO.latest.description,
                    //update
                    () => { Application.OpenURL(ForceUpdateData.UpdateUrl); },
                    //skip
                    () => { });

            }
            else
            if (ForceUpdateData.CanUpdate)
            {
                _buttonRoot.SetActive(false);
                _updatepage.Show();
                _updatepage.ShowUpdate(ForceUpdateData.GetCurrentVersion,
                    ForceUpdateData.GetServerVersion(),
                    response.result.VERSION_INFO.latest.description,
                    //update
                    () => { Application.OpenURL(ForceUpdateData.UpdateUrl); },
                    //skip
                    () =>
                    {
                        //_loginpage.Show();
                        //_buttonRoot.SetActive(true);
                        GotoNextScene();

                    });
            }
            else
            if (ForceUpdateData.NoUpdate)
            {
                GotoNextScene();
            }
        }


    }

    private void GotoNextScene()
    {
        if (AutoLogin && (PlayerPrefs.HasKey("token")))
        {
            SceneManager.LoadScene(1);
        }
        else
        {
            _loginpage.Show();
            _buttonRoot.SetActive(true);
        }
    }
}
