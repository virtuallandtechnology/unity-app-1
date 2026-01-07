using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Shop.Visuals;
using Bozo.ModularCharacters;
using static ApiClient;

namespace VirtualLand
{
    /// <summary>
    /// Manages viewing and editing purchased characters in 3D
    /// Similar to Fortnite's character locker/editor
    /// </summary>
    public class PurchasedCharactersViewer : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _viewerPanel;
        [SerializeField] private Transform _characterListContainer;
        [SerializeField] private GameObject _characterItemPrefab;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _backButton;

        [Header("Character Viewer")]
        [SerializeField] private CharacterViewer _characterViewer;
        [SerializeField] private GameObject _viewerRoot;

        [Header("Editor UI")]
        [SerializeField] private GameObject _editorPanel;
        [SerializeField] private TextMeshProUGUI _categoryLabel;
        [SerializeField] private Button _nextCategoryButton;
        [SerializeField] private Button _prevCategoryButton;
        [SerializeField] private Button _nextPartButton;
        [SerializeField] private Button _prevPartButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private TextMeshProUGUI _feedbackText;

        [Header("Character Display")]
        [SerializeField] private TextMeshProUGUI _characterNameText;

        private string[] _editableCategories = new string[] { "Hair", "Head", "Torso", "Legs" };
        private int _currentCategoryIndex = 0;
        private List<ShopProduct> _purchasedCharacters = new List<ShopProduct>();
        private Dictionary<int, string> _productCategoryMap = new Dictionary<int, string>(); // Maps product ID to category slug
        private ShopProduct _currentCharacterProduct;

        private void Start()
        {
            // Initial state
            if (_viewerPanel != null) _viewerPanel.SetActive(false);
            if (_editorPanel != null) _editorPanel.SetActive(false);

            // // Button listeners
            // if (_closeButton != null)
            //     _closeButton.onClick.AddListener(CloseViewer);

            // if (_backButton != null)
            //     _backButton.onClick.AddListener(CloseViewer);

            if (_nextCategoryButton != null)
                _nextCategoryButton.onClick.AddListener(NextCategory);

            if (_prevCategoryButton != null)
                _prevCategoryButton.onClick.AddListener(PrevCategory);

            if (_nextPartButton != null)
                _nextPartButton.onClick.AddListener(() => ChangePart(1));

            if (_prevPartButton != null)
                _prevPartButton.onClick.AddListener(() => ChangePart(-1));

            if (_saveButton != null)
                _saveButton.onClick.AddListener(OnSaveClicked);
        }

        /// <summary>
        /// Open the viewer and load purchased characters
        /// </summary>
        public void OpenViewer()
        {
            if (_viewerPanel != null) _viewerPanel.SetActive(true);
            LoadPurchasedCharacters();
        }

        /// <summary>
        /// Close the viewer
        /// </summary>
        public void CloseViewer(bool cleanup = true)
        {
            if (_viewerPanel != null) _viewerPanel.SetActive(false);
            if (_editorPanel != null) _editorPanel.SetActive(false);
            if (cleanup && _characterViewer != null) _characterViewer.Cleanup();
            _currentCharacterProduct = null;
        }

        /// <summary>
        /// Load all purchased characters from API
        /// </summary>
        private void LoadPurchasedCharacters()
        {
            ClearCharacterList();

            // Load purchased characters from all character categories
            string[] characterCategories = new string[] 
            { 
                "CHARCTERS", "CIVILIANS", "GANG-MEMBERS", 
                "POLICE-LAW", "SPECIAL-OPS", "VIP-BUSINESS", "CUSTOM-AVATARS" 
            };

            // Get filter config to check which categories should be loaded
            var filterConfig = CategoryFilterConfig.Instance;
            
            // Filter categories based on CategoryFilterConfig
            List<string> categoriesToFetch = new List<string>();
            foreach (var category in characterCategories)
            {
                if (filterConfig.ShouldShowCategory(category))
                {
                    categoriesToFetch.Add(category);
                }
            }

            // If no categories pass the filter, show all (fallback behavior)
            if (categoriesToFetch.Count == 0)
            {
                Debug.LogWarning("[PurchasedCharactersViewer] No categories passed filter. Loading all categories.");
                categoriesToFetch.AddRange(characterCategories);
            }

            int categoriesToLoad = categoriesToFetch.Count;
            _purchasedCharacters.Clear();

            foreach (var category in categoriesToFetch)
            {
                ApiClient.Get().GetPurchasedProducts(category,
                    (response) =>
                    {
                        if (response.isSuccess && response.result != null)
                        {
                            foreach (var item in response.result)
                            {
                                if (item.product != null)
                                {
                                    _purchasedCharacters.Add(item.product);
                                    _productCategoryMap[item.product.id] = category; // Track category
                                    CreateCharacterListItem(item.product);
                                }
                            }
                        }

                        categoriesToLoad--;
                        if (categoriesToLoad <= 0)
                        {
                            OnCharactersLoaded();
                        }
                    },
                    (error) =>
                    {
                        Debug.LogError($"Error loading purchased characters from {category}: {error}");
                        categoriesToLoad--;
                        if (categoriesToLoad <= 0)
                        {
                            OnCharactersLoaded();
                        }
                    });
            }
        }

        private void OnCharactersLoaded()
        {
            Debug.Log($"[PurchasedCharactersViewer] Loaded {_purchasedCharacters.Count} purchased characters");
        }

        private void ClearCharacterList()
        {
            if (_characterListContainer == null) return;

            foreach (Transform child in _characterListContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private void CreateCharacterListItem(ShopProduct product)
        {
            if (_characterItemPrefab == null || _characterListContainer == null) return;

            var item = Instantiate(_characterItemPrefab, _characterListContainer);
            var button = item.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnCharacterSelected(product));
            }

            // Set product image if available
            var image = item.GetComponentInChildren<Image>();
            if (image != null && !string.IsNullOrEmpty(product.image))
            {
                // Load image from URL (you may need to implement image loading)
                // For now, we'll just set the product name
            }

            // Set product name
            var text = item.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = product.title;
            }
        }

        /// <summary>
        /// Called when a character is selected from the list
        /// </summary>
        private void OnCharacterSelected(ShopProduct product)
        {
            _currentCharacterProduct = product;
            
            if (_characterNameText != null)
                _characterNameText.text = product.title;

            // Show character in 3D viewer
            if (_characterViewer != null)
            {
                // Use CustomizedData mode for profile (loads edited data from API)
                _characterViewer.Initialize(product, CharacterLoadMode.CustomizedData);
                
                // Find the appropriate CharacterObject from PreviewSystemConfig
                var previewConfig = Resources.Load<Game.Shop.Preview.PreviewSystemConfig>("Shop/Configs/PreviewSystemConfig");
                if (previewConfig != null)
                {
                    // Get category for this product
                    string categorySlug = "CIVILIANS"; // Default fallback
                    if (_productCategoryMap.ContainsKey(product.id))
                    {
                        categorySlug = _productCategoryMap[product.id];
                    }
                    
                    var widgetConfig = previewConfig.FindBestWidgetConfig(product, categorySlug);
                    if (widgetConfig != null && widgetConfig.characterObject != null && widgetConfig.baseCharacterPrefab != null)
                    {
                        // Load from CharacterObject with CustomizedData mode
                        _characterViewer.LoadFromCharacterObject(
                            widgetConfig.characterObject, 
                            widgetConfig.baseCharacterPrefab, 
                            CharacterLoadMode.CustomizedData
                        );
                    }
                    else
                    {
                        Debug.LogWarning($"[PurchasedCharactersViewer] No CharacterObject config found for product {product.id}, falling back to LoadModel");
                        _characterViewer.LoadModel();
                    }
                }
                else
                {
                    Debug.LogWarning("[PurchasedCharactersViewer] PreviewSystemConfig not found, falling back to LoadModel");
                    _characterViewer.LoadModel();
                }

                // Try to load saved customization from user profile
                // Note: Style is stored in user profile, not in product
                // We'll load it when the character is displayed
                LoadCharacterCustomization(product.id);
            }

            // Show editor panel
            if (_editorPanel != null) _editorPanel.SetActive(true);
            _currentCategoryIndex = 0;
            UpdateEditorUI();
        }

        private void NextCategory()
        {
            _currentCategoryIndex = (_currentCategoryIndex + 1) % _editableCategories.Length;
            UpdateEditorUI();
        }

        private void PrevCategory()
        {
            _currentCategoryIndex--;
            if (_currentCategoryIndex < 0) _currentCategoryIndex = _editableCategories.Length - 1;
            UpdateEditorUI();
        }

        private void UpdateEditorUI()
        {
            if (_categoryLabel != null)
            {
                _categoryLabel.text = _editableCategories[_currentCategoryIndex];
            }
        }

        private void ChangePart(int direction)
        {
            if (_characterViewer == null) return;

            string category = _editableCategories[_currentCategoryIndex];
            _characterViewer.ChangePart(category, direction);
        }

        private void OnSaveClicked()
        {
            if (_characterViewer == null || _currentCharacterProduct == null)
            {
                ShowFeedback("No character selected to save.");
                return;
            }

            string json = _characterViewer.SaveCustomization();
            if (string.IsNullOrEmpty(json))
            {
                ShowFeedback("Failed to save customization.");
                return;
            }

            // Save to profile via API
            if (_saveButton != null) _saveButton.interactable = false;
            ShowFeedback("Saving...");

            // Construct style object/json
            // We want to wrap it in avatar_id and style if that's what the endpoint expects
            // Based on user screenshot, the response has "result": { "style": {...}, "avatar_id": "3" }
            // So we should probably send the same structure
            var payload = new
            {
                avatar_id = _currentCharacterProduct.id.ToString(),
                style = Newtonsoft.Json.JsonConvert.DeserializeObject(json) // Deserialize to object so it serializes as object, not string
            };
            
            string payloadJson = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
            string key = _currentCharacterProduct.title;

            ApiClient.Get().UpdateProfileData(key, payloadJson,
                (response) =>
                {
                    ShowFeedback("Character saved successfully!");
                    if (_saveButton != null) _saveButton.interactable = true;
                },
                (error) =>
                {
                    ShowFeedback($"Error: {error}");
                    if (_saveButton != null) _saveButton.interactable = true;
                });
        }

        private void ShowFeedback(string message)
        {
            if (_feedbackText != null)
            {
                _feedbackText.text = message;
                CancelInvoke(nameof(ClearFeedback));
                Invoke(nameof(ClearFeedback), 3f);
            }
            Debug.Log($"[PurchasedCharactersViewer] {message}");
        }

        private void ClearFeedback()
        {
            if (_feedbackText != null) _feedbackText.text = "";
        }

        /// <summary>
        /// Load character customization from user profile
        /// </summary>
        private void LoadCharacterCustomization(int productId)
        {
            // Get user profile to find style for this product
            ApiClient.Get().GetProfileInfo(
                (response) =>
                {
                    if (response.isSuccess && response.result != null)
                    {
                        var user = response.result;
                        // Check if user has style saved for this product
                        // This depends on your API structure - you may need to adjust
                        // For now, we'll try to load from product metadata if available
                        // In a real implementation, you'd query the profile for the specific product's style
                    }
                },
                (error) =>
                {
                    Debug.LogWarning($"[PurchasedCharactersViewer] Could not load customization: {error}");
                });
        }
    }
}
