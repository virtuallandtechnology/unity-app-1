using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

public class WalletManager : MonoBehaviour
{
    [SerializeField] private WalletItem _walletPrefab;
    [SerializeField] private Transform _walletContainer;
    [SerializeField] private TextMeshProUGUI _noWalletText;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Button _closeButton;

    private void Start()
    {
        if (_closeButton != null)
            _closeButton.onClick.AddListener(Hide);

        gameObject.SetActive(false);
        if (_canvasGroup != null) _canvasGroup.alpha = 0;
        transform.localScale = Vector3.zero;
    }

    public void Show()
    {
        gameObject.SetActive(true);

        transform.localScale = Vector3.zero;
        if (_canvasGroup != null) _canvasGroup.alpha = 0;

        transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        if (_canvasGroup != null) _canvasGroup.DOFade(1f, 0.4f);

        FetchWallets();
    }

    public void Hide()
    {
        transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        if (_canvasGroup != null) _canvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public void FetchWallets()
    {
        ApiClient.Get().GetWallets(OnSuccess, OnFail);
    }

    private void OnSuccess(ApiClient.ApiResponse<List<ApiClient.Wallet>> response)
    {
        ClearContainer();

        if (response.isSuccess && response.result != null && response.result.Count > 0)
        {
            _noWalletText.gameObject.SetActive(false);

            foreach (var wallet in response.result)
            {
                WalletItem item = Instantiate(_walletPrefab, _walletContainer);
                string wName = wallet.wallet_type != null ? wallet.wallet_type.name : "Unknown";
                item.Setup(wName, wallet.balance.ToString());
            }
        }
        else
        {
            ShowNoWalletMessage();
        }
    }

    private void OnFail(string error)
    {
        ClearContainer();
        ShowNoWalletMessage();
    }

    private void ClearContainer()
    {
        foreach (Transform child in _walletContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void ShowNoWalletMessage()
    {
        if (_noWalletText != null)
        {
            _noWalletText.text = "No wallets registered";
            _noWalletText.gameObject.SetActive(true);
        }
    }
}