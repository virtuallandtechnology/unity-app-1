using UnityEngine;
using Game.Shop.Visuals;

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

        [Header("Existing Viewer")]
        [SerializeField] private CharacterViewer _characterViewer;

        [Header("Widget Root")]
        [SerializeField] private GameObject _widgetRoot;

        public string WidgetId => _config != null ? _config.widgetId : "character_adapter";
        public int Priority => _config != null ? _config.priority : 20;

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
            if (_widgetRoot != null)
            {
                _widgetRoot.SetActive(true);
            }

            if (_characterViewer != null)
            {
                // Use existing CharacterViewer interface
                _characterViewer.Initialize(product);
                _characterViewer.LoadModel();
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
