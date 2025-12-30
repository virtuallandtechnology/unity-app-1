using UnityEngine;
using Game.Shop.Visuals;

namespace Game.Shop.Preview.Adapters
{
    /// <summary>
    /// Adapter to make existing CarViewer compatible with IProductPreviewWidget
    /// This allows using CarViewer with the new ScriptableObject preview system
    /// WITHOUT modifying the existing CarViewer code
    /// </summary>
    public class CarViewerAdapter : MonoBehaviour, IProductPreviewWidget
    {
        [Header("Configuration")]
        [SerializeField] private PreviewWidgetConfig _config;

        [Header("Existing Viewer")]
        [SerializeField] private CarViewer _carViewer;

        [Header("Widget Root")]
        [SerializeField] private GameObject _widgetRoot;

        public string WidgetId => _config != null ? _config.widgetId : "car_adapter";
        public int Priority => _config != null ? _config.priority : 20;

        private void Awake()
        {
            // Find CarViewer if not assigned
            if (_carViewer == null)
            {
                _carViewer = GetComponentInChildren<CarViewer>(true);
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

            if (_carViewer != null)
            {
                // Use existing CarViewer interface
                _carViewer.Initialize(product);
                _carViewer.LoadModel();
            }
            else
            {
                Debug.LogWarning("[CarViewerAdapter] CarViewer not assigned!");
            }
        }

        public void HidePreview()
        {
            if (_widgetRoot != null)
            {
                _widgetRoot.SetActive(false);
            }

            if (_carViewer != null)
            {
                _carViewer.Cleanup();
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
        /// Get the underlying CarViewer for direct access (e.g., for customization UI)
        /// </summary>
        public CarViewer GetCarViewer()
        {
            return _carViewer;
        }
    }
}
