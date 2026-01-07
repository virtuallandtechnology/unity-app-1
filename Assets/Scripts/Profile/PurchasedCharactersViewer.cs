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
        private Dictionary<int, GameObject> _characterItems = new Dictionary<int, GameObject>(); // Maps product ID to UI item
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
            Debug.Log($"[PurchasedCharactersViewer] Loaded total of {_purchasedCharacters.Count} purchased characters across categories.");

            // Auto-select last selected character if available
            int lastSelectedId = PlayerPrefs.GetInt("LastSelectedCharacterId", -1);
            if (lastSelectedId > 0)
            {
                var lastCharacter = _purchasedCharacters.Find(c => c.id == lastSelectedId);
                if (lastCharacter != null)
                {
                    Debug.Log($"[PurchasedCharactersViewer] Auto-selecting last character: {lastCharacter.title} (ID: {lastSelectedId})");
                    OnCharacterSelected(lastCharacter);
                }
                else
                {
                    Debug.Log($"[PurchasedCharactersViewer] Last selected character (ID: {lastSelectedId}) not found in purchased list. Total count: {_purchasedCharacters.Count}");
                }
            }
            else
            {
                Debug.Log("[PurchasedCharactersViewer] No character previously selected (LastSelectedCharacterId not found).");
            }
        }

        private void ClearCharacterList()
        {
            if (_characterListContainer == null) return;

            foreach (Transform child in _characterListContainer)
            {
                Destroy(child.gameObject);
            }
            _characterItems.Clear();
        }

        private void CreateCharacterListItem(ShopProduct product)
        {
            if (_characterItemPrefab == null || _characterListContainer == null) return;

            var item = Instantiate(_characterItemPrefab, _characterListContainer);
            _characterItems[product.id] = item;
            
            var button = item.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnCharacterSelected(product));
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
            if (product == null) return;
            
            _currentCharacterProduct = product;

            // Update UI selection highlights
            UpdateSelectionUI(product.id);

            // Save selected character ID for persistence
            PlayerPrefs.SetInt("LastSelectedCharacterId", product.id);
            PlayerPrefs.Save();
            Debug.Log($"[PurchasedCharactersViewer] Character Selected: {product.title} (ID: {product.id}). Saved to PlayerPrefs.");

            if (_characterNameText != null)
                _characterNameText.text = product.title;

            // Show character in 3D viewer
            if (_characterViewer != null)
            {
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
                        Debug.Log($"[PurchasedCharactersViewer] Loading Character: {product.title} via CharacterObject: {widgetConfig.characterObject.name}");

                        // Load base character first in CustomizedData mode
                        _characterViewer.LoadFromCharacterObject(
                            widgetConfig.characterObject,
                            widgetConfig.baseCharacterPrefab,
                            CharacterLoadMode.CustomizedData
                        );

                        // Fetch latest customization from API for this specific character
                        LoadCharacterCustomization(product.id);
                    }
                    else
                    {
                        Debug.LogWarning($"[PurchasedCharactersViewer] No widget config for {product.title}. Falling back to default LoadModel.");
                        _characterViewer.LoadModel();
                    }
                }
                else
                {
                    Debug.LogError("[PurchasedCharactersViewer] PreviewSystemConfig not found!");
                    _characterViewer.LoadModel();
                }
            }

            // Show editor panel
            if (_editorPanel != null) _editorPanel.SetActive(true);
            _currentCategoryIndex = 0;
            UpdateEditorUI();
        }

        private void UpdateSelectionUI(int selectedId)
        {
            foreach (var kvp in _characterItems)
            {
                int productId = kvp.Key;
                GameObject item = kvp.Value;
                bool isSelected = productId == selectedId;

                // Update background color or show checkmark
                var image = item.GetComponent<Image>();
                if (image != null)
                {
                    image.color = isSelected ? new Color(0.2f, 0.6f, 1.0f, 0.5f) : new Color(1, 1, 1, 0.1f);
                }

                // Highlighting text
                var text = item.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.color = isSelected ? Color.white : new Color(1, 1, 1, 0.7f);
                    text.fontStyle = isSelected ? FontStyles.Bold : FontStyles.Normal;
                }
            }
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

            // Capture customization JSON from viewer
            string json = _characterViewer.SaveCustomization();
            if (string.IsNullOrEmpty(json))
            {
                ShowFeedback("Failed to capture customization.");
                return;
            }

            // Save locally for immediate feedback
            var currentCharacter = _characterViewer.GetCurrentCharacter();
            if (currentCharacter != null)
            {
                BMAC_SaveSystem.SaveCharacter(currentCharacter, _currentCharacterProduct.title);
            }

            // Save to profile via API
            if (_saveButton != null) _saveButton.interactable = false;
            ShowFeedback("Saving to server...");

            // Create payload: Avatar ID + Style (as object)
            // Note: ApiClient should handle URL encoding for the key
            var styleObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            var payload = new { avatar_id = _currentCharacterProduct.id.ToString(), style = styleObj };
            string payloadJson = Newtonsoft.Json.JsonConvert.SerializeObject(payload);

            ApiClient.Get().UpdateProfileData(_currentCharacterProduct.title, payloadJson,
                (response) =>
                {
                    ShowFeedback("Saved successfully!");
                    if (_saveButton != null) _saveButton.interactable = true;
                    Debug.Log($"[PurchasedCharactersViewer] Successfully saved {_currentCharacterProduct.title} to API.");

                    // Sync to MainCharacterManager if this is the active character
                    if (VirtualLand.MainCharacterManager.Instance != null && 
                        VirtualLand.MainCharacterManager.Instance.IsMainCharacter(_currentCharacterProduct.id))
                    {
                        VirtualLand.MainCharacterManager.Instance.SyncFromProfile(_currentCharacterProduct.id, json);
                        Debug.Log("[PurchasedCharactersViewer] Synced active character style to MainCharacterManager");
                    }
                },
                (error) =>
                {
                    ShowFeedback($"Save failed: {error}");
                    if (_saveButton != null) _saveButton.interactable = true;
                    Debug.LogError($"[PurchasedCharactersViewer] API Save error: {error}");
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
            if (_currentCharacterProduct == null) return;

            string profileKey = _currentCharacterProduct.title;
            Debug.Log($"[PurchasedCharactersViewer] Fetching customization for '{profileKey}' from API...");
            
            ApiClient.Get().GetProfileData(profileKey,
                (response) =>
                {
                    if (response.isSuccess && response.result != null)
                    {
                        try
                        {
                            var profileData = response.result;
                            if (profileData.ContainsKey("style"))
                            {
                                var styleData = profileData["style"];
                                string styleJson = "";

                                if (styleData is string s)
                                {
                                    // If it's a string, it might be double-encoded JSON OR direct JSON
                                    styleJson = s;
                                }
                                else
                                {
                                    // If it's an object (JObject), serialize it to get JSON string
                                    styleJson = Newtonsoft.Json.JsonConvert.SerializeObject(styleData);
                                }

                                if (!string.IsNullOrEmpty(styleJson) && styleJson != "null" && styleJson != "{}")
                                {
                                    Debug.Log($"[PurchasedCharactersViewer] Applying style data from server for {profileKey}");
                                    if (_characterViewer != null)
                                    {
                                        _characterViewer.LoadCustomization(styleJson);
                                    }
                                }
                                else
                                {
                                    Debug.Log($"[PurchasedCharactersViewer] Server returned empty/null style for {profileKey}.");
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError($"[PurchasedCharactersViewer] Parse error in LoadCharacterCustomization: {ex.Message}");
                        }
                    }
                },
                (error) => Debug.LogWarning($"[PurchasedCharactersViewer] Failed to load customization for {profileKey}: {error}"));
        }
    }
}
