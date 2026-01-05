using Best.HTTP;
using Best.HTTP.Shared;
using System.Collections.Generic;
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
    //[SerializeField] private GameObject _buttonRoot;
    [SerializeField] private TextMeshProUGUI _userNameHome;
    [SerializeField] private Image _avatarHome;

    [Header("Wallet Section")]
    [SerializeField] private WalletManager _walletManager;
    [SerializeField] private Button _openWalletButton;
    public Transform WalletParent;

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

        LoadingHandler.Get().SetVisible(true);
        Getverion();
        ApiClient.Get().OnWalletsUpdated += OnWalletsReceivedForHome;
        HTTPManager.RootSaveFolderProvider = () =>
        System.IO.Path.Combine(Application.persistentDataPath, "BestHTTP_Cache");
    }

    public void OpenWalletPage()
    {

    }

    private void Login(ApiClient.User user)
    {
        ProceedToHome(user);
    }

    private void Register(ApiClient.User user)
    {
        ProceedToHome(user);
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
       // _buttonRoot.SetActive(false);
        _noInternetpage.Show();
    }

    private void OnWalletsReceivedForHome(List<ApiClient.Wallet> wallets)
    {
        if (wallets == null) return;

        List<string> activeSlugs = new List<string>();

        foreach (var wallet in wallets)
        {
            string slug = wallet.wallet_type.slug;
            string balance = wallet.balance.ToString("N0");
            activeSlugs.Add(slug);

            Transform existingChild = WalletParent.Find(slug);

            if (existingChild != null)
            {
                var itemScript = existingChild.GetComponent<WalletItem>();
                if (itemScript != null) itemScript.Setup(slug, balance);
            }
            else
            {
                var newItem = Instantiate(_openWalletButton, WalletParent);

                newItem.name = slug;
                newItem.gameObject.SetActive(true);

                var itemScript = newItem.GetComponent<WalletItem>();
                if (itemScript != null) itemScript.Setup(slug, balance);

                var btn = newItem.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        if (_walletManager != null) _walletManager.gameObject.SetActive(true); _walletManager.Show();
                    });
                }
            }
        }

        for (int i = WalletParent.childCount - 1; i >= 0; i--)
        {
            Transform child = WalletParent.GetChild(i);

            if (!activeSlugs.Contains(child.name) && child.gameObject != _openWalletButton.gameObject)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void onGetVersionSuccess(ApiClient.ApiResponse<ApiClient.Result> response)
    {
        LoadingHandler.Get().SetVisible(false);
        if (response != null)
        {
            ForceUpdateData = new ForceUpdateData(response.result.VERSION_INFO, Application.version);

            if (ForceUpdateData.ForceUpdate)
            {
               // _buttonRoot.SetActive(false);
                _updatepage.Show();
                _updatepage.ShowForceUpdate(ForceUpdateData.GetCurrentVersion,
                    ForceUpdateData.GetServerVersion(), response.result.VERSION_INFO.latest.description,
                    () => { Application.OpenURL(ForceUpdateData.UpdateUrl); },
                    () => { });
            }
            else if (ForceUpdateData.CanUpdate)
            {
              //  _buttonRoot.SetActive(false);
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
            AttemptRefreshToken();
        }
        else
        {
            ShowLoginScreen();
        }
    }

    private void AttemptRefreshToken()
    {
        ApiClient.Get().RefreshToken(
            (response) =>
            {
                if (response.isSuccess && response.result != null)
                {
                    ProceedToHome(response.result.user);
                }
                else
                {
                    HandleLoginFailure();
                }
            },
            (error) =>
            {
                HandleLoginFailure();
            }
        );
    }

    private void HandleLoginFailure()
    {
        PlayerPrefs.DeleteKey("token");
        PlayerPrefs.Save();
        ShowLoginScreen();
    }

    private void ShowLoginScreen()
    {
        Home.gameObject.SetActive(false);
        _loginpage.Show();
        //_buttonRoot.SetActive(true);
    }

    private void ProceedToHome(ApiClient.User user)
    {
        _loginpage.Hide();
        //_buttonRoot.SetActive(false);
        Home.gameObject.SetActive(true);

        if (user != null)
        {
            _userNameHome.text = user.GetUsername();

            if (user.profile != null)
            {
                if (int.TryParse(user.profile.avatar_id, out int id))
                {
                    if (AvatarsConfig.Instance != null && id >= 0 && id < AvatarsConfig.Instance.Avatars.Count)
                    {
                        _avatarHome.sprite = AvatarsConfig.Instance.Avatars[id].sprite;
                    }
                }
            }
        }
        else
        {
            _userNameHome.text = PlayerPrefs.GetString("username");
        }

        ApiClient.Get().RequestWalletsUpdate();
        
        // No need to sync profile version here separately. 
        // Sync happened during Login or RefreshToken.
    }

    private void OnDestroy()
    {
        ApiClient.Get().OnWalletsUpdated -= OnWalletsReceivedForHome;
    }
}