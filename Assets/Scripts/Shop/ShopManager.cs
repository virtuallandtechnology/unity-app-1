using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public enum ShopMode { Store, Inventory }
    public enum ViewLevel { MainCategories, SubCategories, Products }

    [Header("Configuration")]
    [SerializeField] private ShopMode _currentMode = ShopMode.Store;

    [Header("UI References")]
    [SerializeField] private Transform _itemsContainer;
    [SerializeField] private ShopItemUI _itemPrefab;
    [SerializeField] private GameObject _loadingSpinner;

    [Header("Category UI")]
    [SerializeField] private Transform _categoryButtonsContainer;
    [SerializeField] private CategoryButton _categoryButtonPrefab;
    [SerializeField] private Button _backButton;

    [Header("Mode Switching")]
    [SerializeField] private Button _storeTabButton;
    [SerializeField] private Button _inventoryTabButton;
    [SerializeField] private GameObject _cartPanel;

    private ViewLevel _currentViewLevel = ViewLevel.MainCategories;
    private List<ApiClient.CategoryItem> _allCategories;
    private ApiClient.CategoryItem _currentMainCategory;
    [Header("3D View")]
    [SerializeField] private Game.Shop.Visuals.Shop3DViewControllerV2 _shop3DViewControllerPrefab;
    private Game.Shop.Visuals.Shop3DViewControllerV2 _shop3DViewControllerInstance;

    private string _currentSubCategorySlug;
    private List<CategoryButton> _spawnedCategoryButtons = new List<CategoryButton>();

    public Game.Shop.Visuals.Shop3DViewControllerV2 Get3DViewController()
    {
        if (_shop3DViewControllerInstance == null)
        {
            // Try to find in scene first (even if inactive)
            _shop3DViewControllerInstance = FindObjectOfType<Game.Shop.Visuals.Shop3DViewControllerV2>(true);

            // If still null and we have a prefab, instantiate it
            if (_shop3DViewControllerInstance == null && _shop3DViewControllerPrefab != null)
            {
                _shop3DViewControllerInstance = Instantiate(_shop3DViewControllerPrefab);
            }
        }
        return _shop3DViewControllerInstance;
    }

    private void Start()
    {
        _storeTabButton.onClick.AddListener(() => SwitchMode(ShopMode.Store));
        _inventoryTabButton.onClick.AddListener(() => SwitchMode(ShopMode.Inventory));
        
        if (_backButton != null)
        {
            _backButton.onClick.AddListener(OnBackButtonClicked);
            _backButton.gameObject.SetActive(false);
        }

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
            _allCategories = response.result.data;
            ShowMainCategories();
            SwitchMode(ShopMode.Store);
        }
        else
        {
            OnError("Failed to load categories");
        }
        SetLoading(false);
    }

    private void ShowMainCategories()
    {
        _currentViewLevel = ViewLevel.MainCategories;
        ClearCategoryButtons();
        ClearGrid();
        
        if (_backButton != null)
            _backButton.gameObject.SetActive(false);

        if (_allCategories == null) return;

        // Show all top-level categories except ROOT
        // These are the main categories like Cars, Weapons, Skins, etc.
        foreach (var category in _allCategories)
        {
            if (category.slug != "ROOT")
            {
                CreateMainCategoryButton(category);
            }
        }
    }

    private void CreateMainCategoryButton(ApiClient.CategoryItem category)
    {
        var btn = Instantiate(_categoryButtonPrefab, _categoryButtonsContainer);
        btn.Initialize(category.name, category.slug, this);
        btn.SetClickAction(() => OnMainCategoryClicked(category));
        _spawnedCategoryButtons.Add(btn);
    }

    private void OnMainCategoryClicked(ApiClient.CategoryItem mainCategory)
    {
        _currentMainCategory = mainCategory;

        if (mainCategory.categories != null && mainCategory.categories.Count > 0)
        {
            ShowSubCategories(mainCategory);
        }
        else
        {
            ShowProducts(mainCategory.slug);
        }
    }

    private void ShowSubCategories(ApiClient.CategoryItem mainCategory)
    {
        _currentViewLevel = ViewLevel.SubCategories;
        ClearCategoryButtons();
        ClearGrid();
        
        if (_backButton != null)
            _backButton.gameObject.SetActive(true);

        foreach (var subCat in mainCategory.categories)
        {
            CreateSubCategoryButton(subCat);
        }
    }

    private void CreateSubCategoryButton(ApiClient.CategoryItem subCategory)
    {
        var btn = Instantiate(_categoryButtonPrefab, _categoryButtonsContainer);
        btn.Initialize(subCategory.name, subCategory.slug, this);
        btn.SetClickAction(() => OnSubCategoryClicked(subCategory.slug));
        _spawnedCategoryButtons.Add(btn);
    }

    private void OnSubCategoryClicked(string subCategorySlug)
    {
        _currentSubCategorySlug = subCategorySlug;
        ShowProducts(subCategorySlug);
    }

    private void ShowProducts(string categorySlug)
    {
        _currentViewLevel = ViewLevel.Products;
        _currentSubCategorySlug = categorySlug;
        
        if (_backButton != null)
            _backButton.gameObject.SetActive(true);

        FetchData(categorySlug);
    }

    private void OnBackButtonClicked()
    {
        switch (_currentViewLevel)
        {
            case ViewLevel.Products:
                if (_currentMainCategory != null)
                {
                    ShowSubCategories(_currentMainCategory);
                }
                else
                {
                    ShowMainCategories();
                }
                break;

            case ViewLevel.SubCategories:
                ShowMainCategories();
                break;

            case ViewLevel.MainCategories:
                break;
        }
    }

    public void SwitchMode(ShopMode mode)
    {
        _currentMode = mode;

        _storeTabButton.interactable = (mode != ShopMode.Store);
        _inventoryTabButton.interactable = (mode != ShopMode.Inventory);

        if (_cartPanel) _cartPanel.SetActive(mode == ShopMode.Store);

        if (_currentViewLevel == ViewLevel.Products && !string.IsNullOrEmpty(_currentSubCategorySlug))
        {
            FetchData(_currentSubCategorySlug);
        }
    }

    private void FetchData(string categorySlug)
    {
        SetLoading(true);
        ClearGrid();

        if (string.IsNullOrEmpty(categorySlug)) return;

        Debug.Log($"Fetching data for Mode: {_currentMode}, Category: {categorySlug}");

        if (_currentMode == ShopMode.Store)
        {
            ApiClient.Get().GetProductsByCategory(categorySlug,
                OnStoreDataReceived,
                OnError);
        }
        else
        {
            ApiClient.Get().GetPurchasedProducts(categorySlug,
                OnInventoryDataReceived,
                OnError);
        }
    }

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
            item.Setup(product, _currentMode == ShopMode.Inventory, _currentSubCategorySlug);
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

    private void ClearCategoryButtons()
    {
        foreach (Transform child in _categoryButtonsContainer) Destroy(child.gameObject);
        _spawnedCategoryButtons.Clear();
    }

    private void SetLoading(bool show)
    {
        if (_loadingSpinner) _loadingSpinner.SetActive(show);
    }

    public void SelectCategory(string slug)
    {
        foreach (var btn in _spawnedCategoryButtons)
        {
            btn.SetState(btn.Slug == slug);
        }
    }
}