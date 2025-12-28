using UnityEngine;
using Bozo.ModularCharacters;
using System.Threading.Tasks;

namespace Game.Shop.Visuals
{
    /// <summary>
    /// Character viewer implementation using BoZo Stylized Modular Characters system
    /// </summary>
    public class CharacterViewer : MonoBehaviour, I3DProductViewer
    {
        [Header("References")]
        [SerializeField] private GameObject _viewerRoot;
        [SerializeField] private Transform _characterSpawnPoint;
        [SerializeField] private OutfitSystem _outfitSystemPrefab;

        [Header("Supported Categories")]
        [SerializeField] private string[] _supportedCategories = new string[]
        {
            "CHARCTERS", "CIVILIANS", "GANG-MEMBERS", "POLICE-LAW",
            "SPECIAL-OPS", "VIP-BUSINESS", "CUSTOM-AVATARS"
        };

        private OutfitSystem _currentCharacter;
        private ApiClient.ShopProduct _currentProduct;
        private CharacterData _currentCustomization;

        public void Initialize(ApiClient.ShopProduct productData)
        {
            _currentProduct = productData;
            _viewerRoot.SetActive(true);
        }

        public void LoadModel()
        {
            // Clear existing character
            if (_currentCharacter != null)
            {
                Destroy(_currentCharacter.gameObject);
            }

            // Instantiate character
            _currentCharacter = Instantiate(_outfitSystemPrefab, _characterSpawnPoint);
            _currentCharacter.transform.localPosition = Vector3.zero;
            _currentCharacter.transform.localRotation = Quaternion.identity;

            // Load default or saved customization
            if (_currentCustomization != null)
            {
                LoadCustomizationData(_currentCustomization);
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
            data.characterName = _currentProduct.name;

            // Convert to JSON
            string json = JsonUtility.ToJson(data);
            Debug.Log($"Character customization saved: {json}");

            return json;
        }

        public void LoadCustomization(string customizationJson)
        {
            if (string.IsNullOrEmpty(customizationJson))
            {
                Debug.LogWarning("Empty customization data!");
                return;
            }

            try
            {
                CharacterData data = JsonUtility.FromJson<CharacterData>(customizationJson);
                _currentCustomization = data;

                if (_currentCharacter != null)
                {
                    LoadCustomizationData(data);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load customization: {e.Message}");
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
            foreach (var category in _supportedCategories)
            {
                if (category.Equals(categorySlug, System.StringComparison.OrdinalIgnoreCase))
                {
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
