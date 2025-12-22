using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public enum ShopMode { Store, Inventory }

    [Header("Configuration")]
    [SerializeField] private ShopMode _currentMode = ShopMode.Store;
    [SerializeField] private string _defaultCategory = "Cars";

    [Header("UI References")]
    [SerializeField] private Transform _itemsContainer;
    [SerializeField] private ShopItemUI _itemPrefab;
    [SerializeField] private GameObject _loadingSpinner;
    [SerializeField] private List<CategoryButton> _categoryButtons;

    [Header("Mode Switching")]
    [SerializeField] private Button _storeTabButton;
    [SerializeField] private Button _inventoryTabButton;
    [SerializeField] private GameObject _cartPanel;

    private string _currentCategorySlug;

    private void Start()
    {
        foreach (var btn in _categoryButtons)
        {
            btn.Setup(this);
        }

        _storeTabButton.onClick.AddListener(() => SwitchMode(ShopMode.Store));
        _inventoryTabButton.onClick.AddListener(() => SwitchMode(ShopMode.Inventory));

        _currentCategorySlug = _defaultCategory;
        SwitchMode(ShopMode.Store);
    }

    public void SelectCategory(string slug)
    {
        _currentCategorySlug = slug;

        foreach (var btn in _categoryButtons)
            btn.SetState(btn.Slug == slug);

        FetchData();
    }

    public void SwitchMode(ShopMode mode)
    {
        _currentMode = mode;

        _storeTabButton.interactable = (mode != ShopMode.Store);
        _inventoryTabButton.interactable = (mode != ShopMode.Inventory);

        if (_cartPanel) _cartPanel.SetActive(mode == ShopMode.Store);

        SelectCategory(_currentCategorySlug);
    }

    private void FetchData()
    {
        SetLoading(true);
        ClearGrid();

        if (_currentMode == ShopMode.Store)
        {
            // حالت فروشگاه: خروجی ShopResult است
            ApiClient.Get().GetProductsByCategory(_currentCategorySlug,
                OnStoreDataReceived,
                OnError);
        }
        else
        {
            // حالت اینونتوری: خروجی List<ShopProduct> است
            ApiClient.Get().GetPurchasedProducts(_currentCategorySlug,
                OnInventoryDataReceived,
                OnError);
        }
    }

    // کال‌بک مخصوص فروشگاه
    private void OnStoreDataReceived(ApiClient.ApiResponse<ApiClient.ShopResult> response)
    {
        SetLoading(false);
        if (response.isSuccess && response.result != null && response.result.data != null)
        {
            CreateProductItems(response.result.data);
        }
        else
        {
            Debug.Log("No items found in Store.");
        }
    }

    // کال‌بک مخصوص اینونتوری
    private void OnInventoryDataReceived(ApiClient.ApiResponse<List<ApiClient.ShopProduct>> response)
    {
        SetLoading(false);
        if (response.isSuccess && response.result != null)
        {
            CreateProductItems(response.result);
        }
        else
        {
            Debug.Log("No items found in Inventory.");
        }
    }

    // متد مشترک برای ساخت آیتم‌ها
    private void CreateProductItems(List<ApiClient.ShopProduct> products)
    {
        foreach (var product in products)
        {
            var item = Instantiate(_itemPrefab, _itemsContainer);
            item.Setup(product, _currentMode == ShopMode.Inventory);
        }
    }

    private void OnError(string error)
    {
        SetLoading(false);
        Debug.LogError($"Error fetching {_currentMode} data: {error}");
        NotificationController.Get().Show("خطا در دریافت اطلاعات");
    }

    private void ClearGrid()
    {
        foreach (Transform child in _itemsContainer) Destroy(child.gameObject);
    }

    private void SetLoading(bool show)
    {
        if (_loadingSpinner) _loadingSpinner.SetActive(show);
    }
}