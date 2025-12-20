using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VirtualLand;
using static ApiClient;

public class BootStrapController : MonoBehaviour
{
    public LoginPage _loginpage;
    public RegisterPage _registerpage;
    public UpdatePage _updatepage;
    public NoInternetPanel _noInternetpage;
    public GameObject Home;

    [SerializeField] private Button _LoginButton;
    [SerializeField] private Button _RegisterButton;
    [SerializeField] private GameObject _buttonRoot;
    [SerializeField] private TextMeshProUGUI _userNameHome;
    [SerializeField] private Image _avatarHome;

    [Header("Wallet Section")]
    [SerializeField] private WalletManager _walletManager;
    [SerializeField] private Button _openWalletButton;

    public bool AutoLogin = false;
    public ForceUpdateData ForceUpdateData;

    private void Start()
    {
        _registerpage.RegisterAction += Register;
        _loginpage.LoginAction += Login;

        _LoginButton.onClick.RemoveAllListeners();
        _LoginButton.onClick.AddListener(_loginpage.Login);

        _RegisterButton.onClick.RemoveAllListeners();
        _RegisterButton.onClick.AddListener(_registerpage.Register);

        if (_openWalletButton != null)
        {
            _openWalletButton.onClick.RemoveAllListeners();
            _openWalletButton.onClick.AddListener(OpenWalletPage);
        }

        LoadingHandler.Get().SetVisible(true);
        Getverion();
    }

    public void OpenWalletPage()
    {
        if (_walletManager != null)
        {
            _walletManager.Show();
        }
    }

    private void Login()
    {
        _userNameHome.text = PlayerPrefs.GetString("username");
      
    }

    private void Register(string obj)
    {
        _userNameHome.text = PlayerPrefs.GetString("username");
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

            if (ForceUpdateData.ForceUpdate)
            {
                _buttonRoot.SetActive(false);
                _updatepage.Show();
                _updatepage.ShowForceUpdate(ForceUpdateData.GetCurrentVersion,
                    ForceUpdateData.GetServerVersion(), response.result.VERSION_INFO.latest.description,
                    () => { Application.OpenURL(ForceUpdateData.UpdateUrl); },
                    () => { });
            }
            else if (ForceUpdateData.CanUpdate)
            {
                _buttonRoot.SetActive(false);
                _updatepage.Show();
                _updatepage.ShowUpdate(ForceUpdateData.GetCurrentVersion,
                    ForceUpdateData.GetServerVersion(),
                    response.result.VERSION_INFO.latest.description,
                    () => { print("gtutyu"); Application.OpenURL(ForceUpdateData.UpdateUrl); },
                    () => { GotoNextScene(); });
            }
            else if (ForceUpdateData.NoUpdate)
            {
                GotoNextScene();
            }
        }
    }

    private void GotoNextScene()
    {
        if (AutoLogin && PlayerPrefs.HasKey("token"))
        {
            Home.gameObject.SetActive(true);
            _userNameHome.text = PlayerPrefs.GetString("username");

            ApiClient.Get().GetProfileInfo((response) =>
            {
                if (response.result != null)
                {
                    if (response.result.profile != null)
                    {
                        int id = response.result.profile.avatar_id;
                        if (AvatarsConfig.Instance != null && id >= 0 && id < AvatarsConfig.Instance.Avatars.Count)
                        {
                            _avatarHome.sprite = AvatarsConfig.Instance.Avatars[id].sprite;
                        }
                    }
                    _userNameHome.text = response.result.GetUsername();
                }
            }, (fail) => { });

         
            if (_walletManager != null) _walletManager.FetchWallets();
        }
        else
        {
            _loginpage.Show();
            _buttonRoot.SetActive(true);
        }
    }
}