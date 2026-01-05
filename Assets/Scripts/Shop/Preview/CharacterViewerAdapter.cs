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

        [Header("Shop Canvas")]
        [Tooltip("Assign the Canvas component of the Shop Canvas GameObject")]
        public Canvas _canvasShop;

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

            // Try to find canvasShop if not assigned
            if (_canvasShop == null)
            {
                var shopCanvasGO = GameObject.Find("Canvas Shop");
                if (shopCanvasGO != null)
                {
                    _canvasShop = shopCanvasGO.GetComponent<Canvas>();
                    Debug.Log($"[CharacterViewerAdapter] Found Canvas Shop: {_canvasShop != null}");
                }
                else
                {
                    Debug.LogWarning("[CharacterViewerAdapter] Could not find 'Canvas Shop' GameObject. Please assign _canvasShop in Inspector.");
                }
            }
            else
            {
                Debug.Log("[CharacterViewerAdapter] Canvas Shop already assigned in Inspector");
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

            // Disable shop canvas when preview opens
            if (_canvasShop != null)
            {
                _canvasShop.enabled = false;
                Debug.Log("[CharacterViewerAdapter] Canvas Shop disabled");
            }
            else
            {
                Debug.LogWarning("[CharacterViewerAdapter] Canvas Shop reference is null!");
            }

            if (_characterViewer != null)
            {
                Debug.Log("[CharacterViewerAdapter] Initializing CharacterViewer");
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

            // Re-enable shop canvas when preview closes
            if (_canvasShop != null)
            {
                _canvasShop.enabled = true;
                Debug.Log("[CharacterViewerAdapter] Canvas Shop re-enabled");
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
