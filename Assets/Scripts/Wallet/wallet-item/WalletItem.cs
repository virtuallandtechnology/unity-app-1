using TMPro;
using UnityEngine;

public class WalletItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _balanceText;

    public void Setup(string name, string balance)
    {
        _nameText.text = name;
        _balanceText.text = balance;
    }
}
