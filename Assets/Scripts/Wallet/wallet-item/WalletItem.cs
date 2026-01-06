using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class WalletItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _balanceText;
    [SerializeField] private Toggle _selectionToggle;
    [SerializeField] private Image _iconImage;

    private string _walletSlug;
    private Action<string> _onWalletSelected;

    public void Setup(ApiClient.Wallet wallet, ToggleGroup group, Action<string> onSelected)
    {
        if (wallet == null) return;

        _nameText.text = wallet.name;
        _balanceText.text = wallet.balance.ToString("N0");
        _walletSlug = wallet.slug;
        _onWalletSelected = onSelected;

        _selectionToggle.group = group;
        _selectionToggle.isOn = false;

        _selectionToggle.onValueChanged.RemoveAllListeners();
        _selectionToggle.onValueChanged.AddListener(OnToggleChanged);
    }
    public void Setup(string name, string balance)
    {
        if (_nameText != null) _nameText.text = name;
        if (_balanceText != null) _balanceText.text = balance;

        //if (_selectionToggle != null)
        //{
        //    _selectionToggle.gameObject.SetActive(false);
        //}
    }

    private void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            _onWalletSelected?.Invoke(_walletSlug);

        }
    }
}