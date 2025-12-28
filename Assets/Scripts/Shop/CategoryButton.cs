using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CategoryButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Button _button;
    [SerializeField] private Image _selectionHighlight;

    private string _categorySlug;
    private ShopManager _manager;
    private Action _customClickAction;

    public void Initialize(string name, string slug, ShopManager manager)
    {
        _categorySlug = slug;
        _manager = manager;

        if (_nameText) _nameText.text = name;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnClick);
    }

    public void SetClickAction(Action customAction)
    {
        _customClickAction = customAction;
    }

    private void OnClick()
    {
        if (_customClickAction != null)
        {
            _customClickAction.Invoke();
        }
        else if (_manager != null)
        {
            _manager.SelectCategory(_categorySlug);
        }
    }

    public void SetState(bool isSelected)
    {
        if (_selectionHighlight != null)
            _selectionHighlight.gameObject.SetActive(isSelected);

        _button.interactable = !isSelected;
    }

    public string Slug => _categorySlug;
}