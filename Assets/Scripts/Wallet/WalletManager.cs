using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

public class WalletManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private WalletItem _walletPrefab;
    [SerializeField] private Transform _walletContainer;
    [SerializeField] private TextMeshProUGUI _noWalletText;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Button _closeButton;
    [SerializeField] private GameObject _loadingIndicator;

    private void Start()
    {
        if (_closeButton != null) _closeButton.onClick.AddListener(Hide);

        //if (_canvasGroup != null)
        //{
        //    _canvasGroup.alpha = 0;
        //    _canvasGroup.blocksRaycasts = false; 
        //    _canvasGroup.interactable = false; 
        //}

        if (_loadingIndicator != null) _loadingIndicator.SetActive(false);

      //  gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        ApiClient.Get().OnWalletsUpdated += UpdateWalletUI;
    }

    private void OnDisable()
    {
        ApiClient.Get().OnWalletsUpdated -= UpdateWalletUI;
    }

    public void Show()
    {
        //transform.DOKill();
        //if (_canvasGroup != null) _canvasGroup.DOKill();



        //transform.localScale = Vector3.zero;
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
        }

        //transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        //if (_canvasGroup != null) _canvasGroup.DOFade(1f, 0.4f);

        //if (_loadingIndicator != null) _loadingIndicator.SetActive(true);

        ClearContainer();
        if (_noWalletText) _noWalletText.gameObject.SetActive(false);

        ApiClient.Get().RequestWalletsUpdate();
      //  gameObject.SetActive(true);
    }

    public void Hide()
    {
        transform.DOKill();
        if (_canvasGroup != null) _canvasGroup.DOKill();

        if (_canvasGroup != null)
        {
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

      //  transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        if (_canvasGroup != null)
        {
            _canvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }
        
    }

    private void UpdateWalletUI(List<ApiClient.Wallet> wallets)
    {
        if (_loadingIndicator != null) _loadingIndicator.SetActive(false);

        ClearContainer();

        if (wallets != null && wallets.Count > 0)
        {
            if (_noWalletText) _noWalletText.gameObject.SetActive(false);

            foreach (var wallet in wallets)
            {
                WalletItem item = Instantiate(_walletPrefab, _walletContainer);
                string wName = wallet.wallet_type != null ? wallet.wallet_type.name : "Unknown";
                string balanceStr = wallet.balance.ToString("N0");
                item.Setup(wName, balanceStr);
            }
        }
        else
        {
            if (_noWalletText)
            {
                _noWalletText.gameObject.SetActive(true);
                _noWalletText.text = "No wallets found";
            }
        }
    }

    private void ClearContainer()
    {
        foreach (Transform child in _walletContainer) Destroy(child.gameObject);
    }
}