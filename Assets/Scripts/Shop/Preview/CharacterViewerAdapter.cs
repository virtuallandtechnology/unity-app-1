using UnityEngine;
using Game.Shop.Visuals;
using static Game.Shop.Visuals.CharacterLoadMode;

namespace Game.Shop.Preview.Adapters
{
    /// <summary>
    /// Adapter to make existing CharacterViewer compatible with IProductPreviewWidget
    /// This allows using CharacterViewer with the new ScriptableObject preview system
    /// WITHOUT modifying the existing CharacterViewer code
    /// </summary>
    public class CharacterViewerAdapter : MonoBehaviour, IProductPreviewWidget
    {
        [Header("Configuration")]
        [SerializeField] private PreviewWidgetConfig _config;
        
        // Current config for dynamic loading (can be different from _config)
        private PreviewWidgetConfig _currentConfig;

        [Header("Existing Viewer")]
        [SerializeField] private CharacterViewer _characterViewer;

        [Header("Widget Root")]
        [SerializeField] private GameObject _widgetRoot;

        public string WidgetId => _config != null ? _config.widgetId : "character_adapter";
        public int Priority => _config != null ? _config.priority : 20;

        /// <summary>
        /// Set the current config for this preview (allows dynamic CharacterObject loading)
        /// </summary>
        public void SetCurrentConfig(PreviewWidgetConfig config)
        {
            _currentConfig = config;
            Debug.Log($"[CharacterViewerAdapter] Config updated to: {config?.widgetId ?? "NULL"}");
        }

        private void Awake()
        {
            // Find CharacterViewer if not assigned
            if (_characterViewer == null)
            {
                _characterViewer = GetComponentInChildren<CharacterViewer>(true);
            }

            if (_widgetRoot == null)
            {
                _widgetRoot = gameObject;
            }
        }

        public bool CanHandleProduct(ApiClient.ShopProduct product, string categorySlug)
        {
            if (_config == null) return false;
            return _config.CanHandleCategory(categorySlug);
        }

        public void ShowPreview(ApiClient.ShopProduct product, string categorySlug)
        {
            Debug.Log($"[CharacterViewerAdapter] ShowPreview called for product: {product?.title ?? "NULL"}");

            if (_widgetRoot != null)
            {
                _widgetRoot.SetActive(true);
                Debug.Log("[CharacterViewerAdapter] Widget root activated");
            }

            if (_characterViewer != null)
            {
                // Use currentConfig if set, otherwise fall back to _config
                var configToUse = _currentConfig ?? _config;
                
                if (configToUse != null)
                {
                    Debug.Log($"[CharacterViewerAdapter] Initializing CharacterViewer with CharacterObject: {configToUse.characterObject?.name ?? "NULL"}");
                    
                    // Initialize with shop mode (original data)
                    _characterViewer.Initialize(product, CharacterLoadMode.OriginalData);
                    
                    // Load from CharacterObject if available
                    if (configToUse.characterObject != null && configToUse.baseCharacterPrefab != null)
                    {
                        _characterViewer.LoadFromCharacterObject(
                            configToUse.characterObject, 
                            configToUse.baseCharacterPrefab, 
                            CharacterLoadMode.OriginalData
                        );
                    }
                    else
                    {
                        Debug.LogWarning("[CharacterViewerAdapter] CharacterObject or baseCharacterPrefab not configured, falling back to LoadModel");
                        _characterViewer.LoadModel();
                    }
                }
                else
                {
                    Debug.LogWarning("[CharacterViewerAdapter] No config available!");
                }
            }
            else
            {
                Debug.LogWarning("[CharacterViewerAdapter] CharacterViewer not assigned!");
            }
        }

        public void HidePreview()
        {
            if (_widgetRoot != null)
            {
                _widgetRoot.SetActive(false);
            }

            if (_characterViewer != null)
            {
                _characterViewer.Cleanup();
            }
        }

        public void Cleanup()
        {
            HidePreview();
        }

        public GameObject GetWidgetRoot()
        {
            return _widgetRoot;
        }

        /// <summary>
        /// Get the underlying CharacterViewer for direct access (e.g., for customization UI)
        /// </summary>
        public CharacterViewer GetCharacterViewer()
        {
            return _characterViewer;
        }
    }
}
