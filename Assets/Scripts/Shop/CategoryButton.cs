using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CategoryButton : MonoBehaviour
{
    [SerializeField] private string _categorySlug; 
    [SerializeField] private Button _button;
    [SerializeField] private Image _selectionHighlight; 

    private ShopManager _manager;

    public void Setup(ShopManager manager)
    {
        _manager = manager;
        _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        _manager.SelectCategory(_categorySlug);
    }

    public void SetState(bool isSelected)
    {
        if (_selectionHighlight != null)
            _selectionHighlight.gameObject.SetActive(isSelected);

        _button.interactable = !isSelected;
    }

    public string Slug => _categorySlug;
}