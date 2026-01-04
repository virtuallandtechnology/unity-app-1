using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static ApiClient;
using VirtualLand.Utility;

namespace VirtualLand
{
    /// <summary>
    /// UI for selecting the main character from purchased characters
    /// </summary>
    public class MainCharacterSelector : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _selectorPanel;
        [SerializeField] private Transform _characterListContainer;
        [SerializeField] private GameObject _characterItemPrefab;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _currentMainCharacterText;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI _feedbackText;

        private List<ShopProduct> _purchasedCharacters = new List<ShopProduct>();
        private MainCharacterManager _characterManager;

        private void Start()
        {
            _characterManager = MainCharacterManager.Instance;

            if (_selectorPanel != null) _selectorPanel.SetActive(false);

            if (_closeButton != null)
                _closeButton.onClick.AddListener(CloseSelector);

            UpdateCurrentMainCharacterDisplay();
        }

        /// <summary>
        /// Open the selector and load purchased characters
        /// </summary>
        public void OpenSelector()
        {
            if (_selectorPanel != null) _selectorPanel.SetActive(true);
            LoadPurchasedCharacters();
            UpdateCurrentMainCharacterDisplay();
        }

        /// <summary>
        /// Close the selector
        /// </summary>
        public void CloseSelector()
        {
            if (_selectorPanel != null) _selectorPanel.SetActive(false);
        }

        /// <summary>
        /// Load all purchased characters from API
        /// </summary>
        private void LoadPurchasedCharacters()
        {
            ClearCharacterList();

            string[] characterCategories = new string[] 
            { 
                "CHARCTERS", "CIVILIANS", "GANG-MEMBERS", 
                "POLICE-LAW", "SPECIAL-OPS", "VIP-BUSINESS", "CUSTOM-AVATARS" 
            };

            int categoriesToLoad = characterCategories.Length;
            _purchasedCharacters.Clear();

            foreach (var category in characterCategories)
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
            Debug.Log($"[MainCharacterSelector] Loaded {_purchasedCharacters.Count} purchased characters");
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
            
            // Check if this is the main character
            bool isMain = _characterManager != null && _characterManager.IsMainCharacter(product.id);
            
            if (button != null)
            {
                button.onClick.AddListener(() => OnCharacterSelected(product));
            }

            // Set product name
            var text = item.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                string displayText = product.title;
                if (isMain)
                {
                    displayText += " (Main Character)";
                }
                text.text = displayText;
            }

            // Set product image
            var characterImage = item.GetComponentInChildren<ShopItemUI>()._productImage;
            if (characterImage != null && !string.IsNullOrEmpty(product.image))
            {
                ShopImageLoader.Instance.LoadImage(product.image, characterImage);
            }

            // Highlight main character (you can add visual indication here)
            if (isMain)
            {
                var image = item.GetComponent<Image>();
                if (image != null)
                {
                    // You can change color or add border to indicate main character
                    // image.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
                }
            }
        }

        /// <summary>
        /// Called when a character is selected as main character
        /// </summary>
        private void OnCharacterSelected(ShopProduct product)
        {
            if (_characterManager == null)
            {
                ShowFeedback("Error: Character management system not found");
                return;
            }

            // Set as main character
            _characterManager.SetMainCharacter(product);

            ShowFeedback($"Character '{product.title}' has been selected as the main character");
            UpdateCurrentMainCharacterDisplay();

            // Refresh list to show updated main character indicator
            LoadPurchasedCharacters();
        }

        private void UpdateCurrentMainCharacterDisplay()
        {
            if (_currentMainCharacterText == null || _characterManager == null) return;

            if (_characterManager.MainCharacterId > 0)
            {
                // Load main character name
                _characterManager.LoadMainCharacterProduct(
                    (product) =>
                    {
                        _currentMainCharacterText.text = $"Main Character: {product.title}";
                    },
                    (error) =>
                    {
                        _currentMainCharacterText.text = $"Main Character: ID {_characterManager.MainCharacterId}";
                    });
            }
            else
            {
                _currentMainCharacterText.text = "No main character selected";
            }
        }

        private void ShowFeedback(string message)
        {
            if (_feedbackText != null)
            {
                _feedbackText.text = message;
                CancelInvoke(nameof(ClearFeedback));
                Invoke(nameof(ClearFeedback), 3f);
            }
            Debug.Log($"[MainCharacterSelector] {message}");
        }

        private void ClearFeedback()
        {
            if (_feedbackText != null) _feedbackText.text = "";
        }
    }
}

