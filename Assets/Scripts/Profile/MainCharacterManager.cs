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
        // The name of the object in the profile to update/get
        private const string PROFILE_OBJECT_NAME = "main_character";
        private const string PROFILE_VERSION_PREF = "main_character_version";

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
            if (!string.IsNullOrEmpty(styleJson))
            {
                PlayerPrefs.SetString(MAIN_CHARACTER_STYLE_KEY, styleJson);
            }
            else
            {
                PlayerPrefs.DeleteKey(MAIN_CHARACTER_STYLE_KEY);
            }
            
            // Cache the ShopProduct as JSON to avoid API call on next load
            if (characterProduct != null)
            {
                string productJson = JsonUtility.ToJson(characterProduct);
                PlayerPrefs.SetString("MainCharacterProduct", productJson);
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

            // Prepare payload with both avatar_id and style
            object styleObj = null;
            if (!string.IsNullOrEmpty(_mainCharacterStyle))
            {
                try
                {
                    styleObj = Newtonsoft.Json.JsonConvert.DeserializeObject(_mainCharacterStyle);
                }
                catch
                {
                    styleObj = _mainCharacterStyle;
                }
            }

            var payload = new
            {
                avatar_id = _mainCharacterId.ToString(),
                style = styleObj
            };

            string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);

            // Use the fixed object name for update
            string currentKey = PROFILE_OBJECT_NAME;

            ApiClient.Get().UpdateProfileData(currentKey, jsonPayload,
                (response) =>
                {
                    Debug.Log($"[MainCharacterManager] Profile saved successfully to {currentKey}");
                    
                    // Update local version timestamp if server provided one (for sync checks)
                    if (response.timestamp > 0)
                    {
                        PlayerPrefs.SetString(PROFILE_VERSION_PREF, response.timestamp.ToString());
                    }
                    PlayerPrefs.Save();
                },
                (error) =>
                {
                    Debug.LogError($"[MainCharacterManager] Failed to save profile to {currentKey}: {error}");
                });
        }


        /// <summary>
        /// Check if a character is the main character
        /// </summary>
        public bool IsMainCharacter(int characterId)
        {
            return _mainCharacterId == characterId;
        }

        /// <summary>
        /// Sync character data from profile (called during login or version mismatch)
        /// </summary>
        public void SyncFromProfile(int avatarId, string styleJson)
        {
            if (avatarId <= 0) return;

            _mainCharacterId = avatarId;
            _mainCharacterStyle = styleJson;

            // Save to PlayerPrefs
            PlayerPrefs.SetInt(MAIN_CHARACTER_ID_KEY, _mainCharacterId);
            if (!string.IsNullOrEmpty(styleJson))
            {
                PlayerPrefs.SetString(MAIN_CHARACTER_STYLE_KEY, styleJson);
                
                // Also save style as property of the cached ShopProduct if we have it
                // but usually style is separate.
            }
            else
            {
                PlayerPrefs.DeleteKey(MAIN_CHARACTER_STYLE_KEY);
            }
            
            PlayerPrefs.Save();
            
            OnMainCharacterChanged?.Invoke(_mainCharacterId);
            Debug.Log($"[MainCharacterManager] Synced from profile: Avatar ID {avatarId}, Style length: {styleJson?.Length ?? 0}");
        }

        /// <summary>
        /// Check if the server has a newer version of the profile data
        /// </summary>
        public void CheckProfileVersionAndSync()
        {
            string key = PROFILE_OBJECT_NAME;
            double localVersion = double.Parse(PlayerPrefs.GetString(PROFILE_VERSION_PREF, "0"));

            ApiClient.Get().GetProfileData(key,
                (response) =>
                {
                    if (response.isSuccess && response.result != null)
                    {
                        double serverVersion = response.result.timestamp; // Assuming result has timestamp as per ProfileDataResult
                        // Note: ProfileDataResult definition in ApiClient has timestamp.
                        // However, response.timestamp is usually typically on the root response for updates?
                        // Let's check ApiClient.ProfileDataResult definition.
                        // It has 'public double timestamp;'
                        
                        if (serverVersion > localVersion)
                        {
                            ApplySyncedProfileData(key, response.result, serverVersion);
                        }
                        else
                        {
                            Debug.Log($"[MainCharacterManager] Profile data {key} is up to date (Version {serverVersion})");
                        }
                    }
                },
                (error) =>
                {
                    Debug.LogError($"[MainCharacterManager] Failed to check profile version: {error}");
                });
        }

        private void ApplySyncedProfileData(string key, ApiClient.ProfileDataResult result, double newVersion)
        {
            // 1. Extract style JSON
            string styleJson = "";
            if (result.style != null)
            {
                if (result.style is string strStyle)
                    styleJson = strStyle;
                else
                    styleJson = Newtonsoft.Json.JsonConvert.SerializeObject(result.style);
            }

            // 2. Extract avatar ID
            int avatarId = -1;
            if (!string.IsNullOrEmpty(result.avatar_id))
            {
                int.TryParse(result.avatar_id, out avatarId);
            }

            // 3. Sync if we have valid data
            if (avatarId > 0)
            {
                SyncFromProfile(avatarId, styleJson);
            }
            
            PlayerPrefs.SetString(PROFILE_VERSION_PREF, newVersion.ToString());
            PlayerPrefs.Save();
            Debug.Log($"[MainCharacterManager] Profile data {key} synced (Version {newVersion})");
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
            
            // Try loading from cache first
            if (PlayerPrefs.HasKey("MainCharacterProduct"))
            {
                string json = PlayerPrefs.GetString("MainCharacterProduct");
                try
                {
                    var cachedProduct = JsonUtility.FromJson<ShopProduct>(json);
                    if (cachedProduct != null && cachedProduct.id == _mainCharacterId)
                    {
                        _mainCharacterProduct = cachedProduct;
                        Debug.Log("[MainCharacterManager] Loaded main character from cache");
                        onSuccess?.Invoke(_mainCharacterProduct);
                        return; // Return early if cache hit
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[MainCharacterManager] Failed to load cached product: {e.Message}");
                }
            }

            // Load from API via GetPurchasedProducts (since GetProductById is unreliable for user products)
            ApiClient.Get().GetPurchasedProducts("Characters",
                (response) =>
                {
                    if (response.isSuccess && response.result != null)
                    {
                        var product = response.result.Find(p => p.product.id == _mainCharacterId);
                        if (product != null && product.product != null)
                        {
                            _mainCharacterProduct = product.product;
                            
                            // Cache the new product
                            string newJson = JsonUtility.ToJson(_mainCharacterProduct);
                            PlayerPrefs.SetString("MainCharacterProduct", newJson);

                            onSuccess?.Invoke(_mainCharacterProduct);
                        }
                        else
                        {
                            onFail?.Invoke($"Main character (ID: {_mainCharacterId}) not found in purchased characters");
                        }
                    }
                    else
                    {
                        onFail?.Invoke("Failed to load purchased characters to find main character");
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

        /// <summary>
        /// Sync main character ID from API profile (does not trigger API save)
        /// </summary>
        public void SyncMainCharacterId(int characterId)
        {
            if (characterId <= 0) return;

            if (_mainCharacterId != characterId)
            {
                _mainCharacterId = characterId;
                // We don't have the style JSON from this call usually, so we might need to rely on API loading it later
                // or just keep it null until edited.
                
                PlayerPrefs.SetInt(MAIN_CHARACTER_ID_KEY, _mainCharacterId);
                PlayerPrefs.Save();
                
                OnMainCharacterChanged?.Invoke(_mainCharacterId);
                Debug.Log($"[MainCharacterManager] Synced main character ID from profile: {_mainCharacterId}");
            }
        }
    }
}
