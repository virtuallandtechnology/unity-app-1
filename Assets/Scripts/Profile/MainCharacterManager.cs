using UnityEngine;
using System;
using static ApiClient;

namespace VirtualLand
{
    /// <summary>
    /// Manages the main/primary character selection and storage
    /// Stores the selected character ID and loads it when editing profile
    /// </summary>
    public class MainCharacterManager : MonoBehaviour
    {
        private static MainCharacterManager _instance;
        public static MainCharacterManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MainCharacterManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("MainCharacterManager");
                        _instance = go.AddComponent<MainCharacterManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private const string MAIN_CHARACTER_ID_KEY = "MainCharacterID";
        private const string MAIN_CHARACTER_STYLE_KEY = "MainCharacterStyle";

        private int _mainCharacterId = -1;
        private string _mainCharacterStyle = null;
        private ShopProduct _mainCharacterProduct = null;

        public int MainCharacterId => _mainCharacterId;
        public string MainCharacterStyle => _mainCharacterStyle;
        public ShopProduct MainCharacterProduct => _mainCharacterProduct;

        public event Action<int> OnMainCharacterChanged;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadMainCharacterFromPrefs();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Set the main character and save it
        /// </summary>
        public void SetMainCharacter(ShopProduct characterProduct, string styleJson = null)
        {
            if (characterProduct == null)
            {
                Debug.LogWarning("[MainCharacterManager] Cannot set null character as main character");
                return;
            }

            _mainCharacterId = characterProduct.id;
            _mainCharacterProduct = characterProduct;
            _mainCharacterStyle = styleJson;

            // Save to PlayerPrefs
            PlayerPrefs.SetInt(MAIN_CHARACTER_ID_KEY, _mainCharacterId);
            if (!string.IsNullOrEmpty(_mainCharacterStyle))
            {
                PlayerPrefs.SetString(MAIN_CHARACTER_STYLE_KEY, _mainCharacterStyle);
            }
            PlayerPrefs.Save();

            // Also save to API profile
            SaveMainCharacterToProfile();

            OnMainCharacterChanged?.Invoke(_mainCharacterId);
            Debug.Log($"[MainCharacterManager] Main character set to: {characterProduct.title} (ID: {_mainCharacterId})");
        }

        /// <summary>
        /// Load main character from PlayerPrefs
        /// </summary>
        private void LoadMainCharacterFromPrefs()
        {
            if (PlayerPrefs.HasKey(MAIN_CHARACTER_ID_KEY))
            {
                _mainCharacterId = PlayerPrefs.GetInt(MAIN_CHARACTER_ID_KEY, -1);
                if (PlayerPrefs.HasKey(MAIN_CHARACTER_STYLE_KEY))
                {
                    _mainCharacterStyle = PlayerPrefs.GetString(MAIN_CHARACTER_STYLE_KEY);
                }
                Debug.Log($"[MainCharacterManager] Loaded main character ID from prefs: {_mainCharacterId}");
            }
        }

        /// <summary>
        /// Save main character to API profile
        /// </summary>
        private void SaveMainCharacterToProfile()
        {
            if (_mainCharacterId <= 0) return;

            // Update profile with main character ID and style
            if (!string.IsNullOrEmpty(_mainCharacterStyle))
            {
                ApiClient.Get().UpdateUserProfile(_mainCharacterId, _mainCharacterStyle,
                    (response) =>
                    {
                        Debug.Log("[MainCharacterManager] Main character saved to profile successfully");
                    },
                    (error) =>
                    {
                        Debug.LogError($"[MainCharacterManager] Failed to save main character to profile: {error}");
                    });
            }
        }

        /// <summary>
        /// Check if a character is the main character
        /// </summary>
        public bool IsMainCharacter(int characterId)
        {
            return _mainCharacterId == characterId;
        }

        /// <summary>
        /// Get main character product from API if not loaded
        /// </summary>
        public void LoadMainCharacterProduct(Action<ShopProduct> onSuccess, Action<string> onFail)
        {
            if (_mainCharacterId <= 0)
            {
                onFail?.Invoke("No main character selected");
                return;
            }

            // If we already have the product, return it
            if (_mainCharacterProduct != null && _mainCharacterProduct.id == _mainCharacterId)
            {
                onSuccess?.Invoke(_mainCharacterProduct);
                return;
            }

            // Load from API
            ApiClient.Get().GetProductById(_mainCharacterId,
                (response) =>
                {
                    if (response.isSuccess && response.result != null)
                    {
                        _mainCharacterProduct = response.result;
                        onSuccess?.Invoke(_mainCharacterProduct);
                    }
                    else
                    {
                        onFail?.Invoke("Failed to load main character product");
                    }
                },
                (error) =>
                {
                    onFail?.Invoke(error);
                });
        }

        /// <summary>
        /// Clear main character
        /// </summary>
        public void ClearMainCharacter()
        {
            _mainCharacterId = -1;
            _mainCharacterProduct = null;
            _mainCharacterStyle = null;
            PlayerPrefs.DeleteKey(MAIN_CHARACTER_ID_KEY);
            PlayerPrefs.DeleteKey(MAIN_CHARACTER_STYLE_KEY);
            PlayerPrefs.Save();
            OnMainCharacterChanged?.Invoke(-1);
        }
    }
}

