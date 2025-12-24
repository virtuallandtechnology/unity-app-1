using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public enum ShopMode { Store, Inventory }

    [Header("Configuration")]
    [SerializeField] private ShopMode _currentMode = ShopMode.Store;
    [SerializeField] private string _defaultCategory = "CARS"; // Use slug

    [Header("UI References")]
    [SerializeField] private Transform _itemsContainer;
    [SerializeField] private ShopItemUI _itemPrefab;
    [SerializeField] private GameObject _loadingSpinner;

    [Header("Category UI")]
    [SerializeField] private Transform _categoryButtonsContainer; // Parent for buttons
    [SerializeField] private CategoryButton _categoryButtonPrefab;

    [Header("Mode Switching")]
    [SerializeField] private Button _storeTabButton;
    [SerializeField] private Button _inventoryTabButton;
    [SerializeField] private GameObject _cartPanel;

    private string _currentCategorySlug;
    private List<CategoryButton> _spawnedCategoryButtons = new List<CategoryButton>();

    private void Start()
    {
        _storeTabButton.onClick.AddListener(() => SwitchMode(ShopMode.Store));
        _inventoryTabButton.onClick.AddListener(() => SwitchMode(ShopMode.Inventory));

        // 1. Fetch Categories first
        FetchCategories();
    }

    private void FetchCategories()
    {
        SetLoading(true);
        ApiClient.Get().GetAllCategories(OnCategoriesReceived, OnError);
    }

    private void OnCategoriesReceived(ApiClient.ApiResponse<ApiClient.CategoryResult> response)
    {
        if (response.isSuccess && response.result != null)
        {
            SpawnCategoryButtons(response.result.data);

            // After spawning buttons, select the default or first one
            string initialCategory = !string.IsNullOrEmpty(_currentCategorySlug) ? _currentCategorySlug : _defaultCategory;
            SwitchMode(ShopMode.Store); // This will trigger FetchData
        }
        else
        {
            OnError("Failed to load categories");
        }
    }

    private void SpawnCategoryButtons(List<ApiClient.CategoryItem> rootCategories)
    {
        // Clear existing
        foreach (Transform child in _categoryButtonsContainer) Destroy(child.gameObject);
        _spawnedCategoryButtons.Clear();

        foreach (var rootCat in rootCategories)
        {
            // Option A: Create button for Root Category itself
            CreateButton(rootCat.name, rootCat.slug);

            // Option B: Create buttons for Sub-Categories (Flattened list)
            if (rootCat.categories != null)
            {
                foreach (var subCat in rootCat.categories)
                {
                    // You might want to indent sub-categories visually or just list them
                    CreateButton(subCat.name, subCat.slug);
                }
            }
        }
    }

    private void CreateButton(string name, string slug)
    {
        var btn = Instantiate(_categoryButtonPrefab, _categoryButtonsContainer);
        btn.Initialize(name, slug, this);
        _spawnedCategoryButtons.Add(btn);
    }

    public void SelectCategory(string slug)
    {
        _currentCategorySlug = slug;

        // Update visual state of buttons
        foreach (var btn in _spawnedCategoryButtons)
        {
            btn.SetState(btn.Slug == slug);
        }

        FetchData();
    }

    public void SwitchMode(ShopMode mode)
    {
        _currentMode = mode;

        _storeTabButton.interactable = (mode != ShopMode.Store);
        _inventoryTabButton.interactable = (mode != ShopMode.Inventory);

        if (_cartPanel) _cartPanel.SetActive(mode == ShopMode.Store);

        if (!string.IsNullOrEmpty(_currentCategorySlug))
        {
            SelectCategory(_currentCategorySlug);
        }
    }

    private void FetchData()
    {
        SetLoading(true);
        ClearGrid();

        if (string.IsNullOrEmpty(_currentCategorySlug)) return;

        Debug.Log($"Fetching data for Mode: {_currentMode}, Category: {_currentCategorySlug}");

        if (_currentMode == ShopMode.Store)
        {
            ApiClient.Get().GetProductsByCategory(_currentCategorySlug,
                OnStoreDataReceived,
                OnError);
        }
        else
        {
            ApiClient.Get().GetPurchasedProducts(_currentCategorySlug,
                OnInventoryDataReceived,
                OnError);
        }
    }

    // ... (Rest of your callbacks: OnStoreDataReceived, OnInventoryDataReceived, etc.) ...

    private void OnStoreDataReceived(ApiClient.ApiResponse<ApiClient.ShopResult> response)
    {
        SetLoading(false);
        if (response.isSuccess && response.result != null && response.result.data != null)
        {
            CreateProductItems(response.result.data);
        }
    }

    private void OnInventoryDataReceived(ApiClient.ApiResponse<List<ApiClient.InventoryItemWrapper>> response)
    {
        SetLoading(false);
        if (response.isSuccess && response.result != null)
        {
            List<ApiClient.ShopProduct> extractedProducts = new List<ApiClient.ShopProduct>();
            foreach (var item in response.result)
            {
                if (item.product != null) extractedProducts.Add(item.product);
            }
            CreateProductItems(extractedProducts);
        }
    }

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
        Debug.LogError($"API Error: {error}");
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