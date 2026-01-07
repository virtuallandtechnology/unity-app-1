using UnityEngine;
using Bozo.ModularCharacters;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Game.Shop.Visuals
{
    /// <summary>
    /// Character loading mode
    /// </summary>
    public enum CharacterLoadMode
    {
        OriginalData,    // Load from CharacterObject.data (Shop)
        CustomizedData   // Load from API user customization (Profile)
    }

    /// <summary>
    /// Character viewer implementation using BoZo Stylized Modular Characters system
    /// </summary>
    public class CharacterViewer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _viewerRoot;
        [SerializeField] private Transform _characterSpawnPoint;
        [SerializeField] private OutfitSystem _outfitSystemPrefab;
        [SerializeField] private CharacterCreator _characterCreator;

        [Header("Supported Categories")]
        [SerializeField] private string[] _supportedCategories = new string[]
        {
            "CHARCTERS", "CIVILIANS", "GANG-MEMBERS", "POLICE-LAW",
            "SPECIAL-OPS", "VIP-BUSINESS", "CUSTOM-AVATARS"
        };

        private OutfitSystem _currentCharacter;
        private ApiClient.ShopProduct _currentProduct;
        private CharacterData _currentCustomization;
        private CharacterLoadMode _loadMode = CharacterLoadMode.OriginalData;
        private Task _loadingTask = Task.CompletedTask;

        private void Awake()
        {
            // When instantiated from prefab, find CharacterCreator in scene
            if (_characterCreator == null)
            {
                _characterCreator = FindObjectOfType<CharacterCreator>();
                if (_characterCreator != null)
                {
                    Debug.Log("[CharacterViewer] Found CharacterCreator in scene");
                }
                else
                {
                    Debug.LogWarning("[CharacterViewer] CharacterCreator not found in scene!");
                }
            }
        }

        public CharacterCreator GetCharacterCreator()
        {
            return _characterCreator;
        }

        public void Initialize(ApiClient.ShopProduct productData, CharacterLoadMode loadMode = CharacterLoadMode.OriginalData)
        {
            _currentProduct = productData;
            _loadMode = loadMode;
            this.gameObject.SetActive(true); // Ensure the viewer itself is active
            _viewerRoot.SetActive(true);
        }
        
        /// <summary>
        /// Load character from BMAC CharacterObject
        /// </summary>
        public void LoadFromCharacterObject(CharacterObject characterObject, OutfitSystem basePrefab, CharacterLoadMode loadMode = CharacterLoadMode.OriginalData)
        {
            _loadingTask = LoadFromCharacterObjectAsync(characterObject, basePrefab, loadMode);
        }

        private async Task LoadFromCharacterObjectAsync(CharacterObject characterObject, OutfitSystem basePrefab, CharacterLoadMode loadMode = CharacterLoadMode.OriginalData)
        {
            if (characterObject == null || basePrefab == null)
            {
                Debug.LogError("[CharacterViewer] CharacterObject or basePrefab is null!");
                return;
            }

            _loadMode = loadMode;

            // Clear existing character
            if (_currentCharacter != null)
            {
                Destroy(_currentCharacter.gameObject);
            }

            // Instantiate base character
            _currentCharacter = Instantiate(basePrefab, _characterSpawnPoint);
            if (_characterCreator != null)
            {
                _characterCreator.ReplaceCharacter(_currentCharacter);
            }
            _currentCharacter.transform.localPosition = Vector3.zero;
            _currentCharacter.transform.localRotation = Quaternion.identity;

            // Load character data based on mode
            if (_loadMode == CharacterLoadMode.OriginalData)
            {
                // Shop mode: Load from CharacterObject.data
                Debug.Log($"[CharacterViewer] Loading character from CharacterObject (Original Data)");
                await BMAC_SaveSystem.LoadCharacter(_currentCharacter, characterObject.data, false, true);
            }
            else
            {
                // Profile mode: Load customized data from API (if available)
                Debug.Log($"[CharacterViewer] Loading character in Customized Data mode");
                if (_currentCustomization != null)
                {
                    await BMAC_SaveSystem.LoadCharacter(_currentCharacter, _currentCustomization, false, true);
                }
                else
                {
                    // Fallback to original data if no customization exists
                    Debug.Log($"[CharacterViewer] No customization found, using original data");
                    await BMAC_SaveSystem.LoadCharacter(_currentCharacter, characterObject.data, false, true);
                }
            }
        }

        public void LoadModel()
        {
            _loadingTask = LoadModelAsync();
        }

        private async Task LoadModelAsync()
        {
            // Clear existing character
            if (_currentCharacter != null)
            {
                Destroy(_currentCharacter.gameObject);
            }

            // Instantiate character
            _currentCharacter = Instantiate(_outfitSystemPrefab, _characterSpawnPoint);
            if (_characterCreator != null)
            {
                _characterCreator.ReplaceCharacter(_currentCharacter);
            }
            _currentCharacter.transform.localPosition = Vector3.zero;
            _currentCharacter.transform.localRotation = Quaternion.identity;
            
            // Load default or saved customization
            if (_currentCustomization != null)
            {
                await BMAC_SaveSystem.LoadCharacter(_currentCharacter, _currentCustomization, false, true);
            }
        }

        public string SaveCustomization()
        {
            if (_currentCharacter == null)
            {
                Debug.LogWarning("No character loaded to save!");
                return null;
            }

            // Get character data from the outfit system
            CharacterData data = BMAC_SaveSystem.GetCharacterData(_currentCharacter);
            data.characterName = _currentProduct.title;

            // Convert to JSON
            string json = JsonUtility.ToJson(data);
            Debug.Log($"Character customization saved: {json}");

            return json;
        }

        #region Editing Methods
        public void ChangePart(string partType, int direction)
        {
            if (_currentCharacter == null) return;

            // This logic depends on BoZo implementation. 
            // Assuming OutfitSystem has methods to cycle parts or we access components directly.
            // Since we don't have the exact API, we'll implement a wrapper that looks for common BoZo patterns
            // or uses the SaveSystem helper if available.
            
            // Example implementation logic suitable for most modular character systems:
            switch(partType.ToLower())
            {
                case "hair":
                    // _currentCharacter.NextHair(); // Hypothetical
                    Debug.Log($"[CharacterViewer] Changing Hair: {direction}");
                    CyclePart("Hair", direction);
                    break;
                case "head":
                    Debug.Log($"[CharacterViewer] Changing Head: {direction}");
                    CyclePart("Head", direction);
                    break;
                case "torso":
                    Debug.Log($"[CharacterViewer] Changing Torso: {direction}");
                    CyclePart("Torso", direction);
                    break;
                case "legs":
                    Debug.Log($"[CharacterViewer] Changing Legs: {direction}");
                    CyclePart("Legs", direction);
                    break;
            }
        }

        private void CyclePart(string partName, int direction)
        {
            if (_currentCharacter == null) return;

            // Load all available outfits from Resources
            var allOutfits = Resources.LoadAll<Outfit>("");
            
            // Filter outfits by type (Hair, Head, Torso, Legs)
            var matchingOutfits = new List<Outfit>();
            foreach (var outfit in allOutfits)
            {
                if (outfit != null && outfit.Type != null && outfit.Type.name == partName)
                {
                    matchingOutfits.Add(outfit);
                }
            }

            if (matchingOutfits.Count == 0)
            {
                Debug.LogWarning($"[CharacterViewer] No outfits found for type: {partName}");
                return;
            }

            // Get current outfit of this type
            var currentOutfit = _currentCharacter.GetOutfit(partName);
            int currentIndex = -1;

            if (currentOutfit != null)
            {
                // Find current outfit index
                for (int i = 0; i < matchingOutfits.Count; i++)
                {
                    if (matchingOutfits[i].name == currentOutfit.name || 
                        matchingOutfits[i].name.Replace("(Clone)", "") == currentOutfit.name.Replace("(Clone)", ""))
                    {
                        currentIndex = i;
                        break;
                    }
                }
            }

            // Calculate new index
            int newIndex;
            if (currentIndex == -1)
            {
                newIndex = direction > 0 ? 0 : matchingOutfits.Count - 1;
            }
            else
            {
                newIndex = currentIndex + direction;
                if (newIndex < 0) newIndex = matchingOutfits.Count - 1;
                if (newIndex >= matchingOutfits.Count) newIndex = 0;
            }

            // Remove current outfit and attach new one
            if (currentOutfit != null)
            {
                // Remove using OutfitType from the current outfit
                _currentCharacter.RemoveOutfit(currentOutfit.Type, true);
            }

            var newOutfit = matchingOutfits[newIndex];
            var inst = _currentCharacter.InstantiateOutfit(newOutfit);
            inst.Attach();
            
            Debug.Log($"[CharacterViewer] Changed {partName} from index {currentIndex} to {newIndex} ({newOutfit.name})");
        }
        #endregion

        public void LoadCustomization(string customizationJson)
        {
            if (string.IsNullOrEmpty(customizationJson) || customizationJson == "null" || customizationJson == "{}")
            {
                Debug.LogWarning("[CharacterViewer] Empty or null customization data received.");
                return;
            }

            try
            {
                CharacterData data = null;
                
                // Try to see if it's double-encoded JSON (a string within a string)
                // Newtonsoft is much better at handling this than JsonUtility
                if (customizationJson.StartsWith("\"") && customizationJson.EndsWith("\""))
                {
                    // It's a double-encoded string, unescape it
                    string unescaped = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(customizationJson);
                    data = Newtonsoft.Json.JsonConvert.DeserializeObject<CharacterData>(unescaped);
                }
                else
                {
                    // Regular JSON object string
                    data = Newtonsoft.Json.JsonConvert.DeserializeObject<CharacterData>(customizationJson);
                }

                if (data != null)
                {
                    _currentCustomization = data;
                    ApplyCustomization(data);
                }
                else
                {
                    Debug.LogError("[CharacterViewer] Deserialization returned null for customization data.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CharacterViewer] Failed to load customization: {e.Message}\nData: {customizationJson}");
            }
        }

        private async void ApplyCustomization(CharacterData data)
        {
            // Wait for loading to finish if in progress
            if (!_loadingTask.IsCompleted)
            {
                Debug.Log("[CharacterViewer] Waiting for character load before applying customization...");
                await _loadingTask;
            }

            if (_currentCharacter != null)
            {
                LoadCustomizationData(data);
            }
            else
            {
                Debug.LogError("[CharacterViewer] Cannot apply customization: _currentCharacter is null");
            }
        }

        private async void LoadCustomizationData(CharacterData data)
        {
            if (_currentCharacter == null) return;

            // Use the BMAC save system to load character data
            await BMAC_SaveSystem.LoadCharacter(_currentCharacter, data, false, true);
            Debug.Log("Character customization loaded successfully");
        }

        public void Cleanup()
        {
            if (_currentCharacter != null)
            {
                Destroy(_currentCharacter.gameObject);
                _currentCharacter = null;
            }

            _currentCustomization = null;
            _viewerRoot.SetActive(false);
        }

        public GameObject GetViewerRoot()
        {
            return _viewerRoot;
        }

        public bool CanHandle(string categorySlug)
        {
            // Detailed Debug
            // Debug.Log($"[CharacterViewer] Checking '{categorySlug}' against supported list...");
            
            foreach (var category in _supportedCategories)
            {
                // Debug.Log($"   - Comparing with '{category}'");
                if (category.Trim().Equals(categorySlug.Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    // Debug.Log($"   -> MATCH FOUND!");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the current character's OutfitSystem for external customization UI
        /// </summary>
        public OutfitSystem GetCurrentCharacter()
        {
            return _currentCharacter;
        }

        /// <summary>
        /// Apply a specific outfit to the character
        /// </summary>
        public void ApplyOutfit(Outfit outfit)
        {
            if (_currentCharacter == null || outfit == null) return;

            var inst = _currentCharacter.InstantiateOutfit(outfit);
            inst.Attach();
        }

        /// <summary>
        /// Set character body blend shape
        /// </summary>
        public void SetBodyShape(string shapeId, float value)
        {
            if (_currentCharacter == null) return;
            _currentCharacter.SetShape(shapeId, value);
        }

        /// <summary>
        /// Set character stance/pose
        /// </summary>
        public void SetStance(float stanceValue)
        {
            if (_currentCharacter == null) return;
            _currentCharacter.SetStance(stanceValue);
        }
    }
}
