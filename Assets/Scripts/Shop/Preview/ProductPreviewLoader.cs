using UnityEngine;
using System;

namespace Game.Shop.Preview
{
    /// <summary>
    /// Main controller for the product preview system
    /// Singleton that manages loading and displaying product previews
    /// </summary>
    public class ProductPreviewLoader : MonoBehaviour
    {
        public static ProductPreviewLoader Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private PreviewSystemConfig _config;

        [Header("Container")]
        [SerializeField] private Transform _widgetContainer;

        // Events
        public event Action<ApiClient.ShopProduct> OnPreviewLoaded;
        public event Action OnPreviewClosed;
        public event Action<string> OnPreviewError;

        private ProductPreviewRegistry _registry;
        private IProductPreviewWidget _currentWidget;
        private ApiClient.ShopProduct _currentProduct;
        private string _currentCategorySlug;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            if (_config == null)
            {
                Debug.LogError("[ProductPreviewLoader] PreviewSystemConfig is not assigned!");
                return;
            }

            if (_widgetContainer == null)
            {
                _widgetContainer = transform;
            }

            _registry = new ProductPreviewRegistry(_config, _widgetContainer);
        }

        /// <summary>
        /// Load and show preview by Product ID
        /// Fetches product from API and displays appropriate widget
        /// </summary>
        public void LoadPreviewByProductId(int productId, string categorySlug, Action<bool> onComplete = null)
        {
            if (_config.enableDebugLogs)
                Debug.Log($"[ProductPreviewLoader] Loading preview for product ID: {productId}, category: {categorySlug}");

            ApiClient.Get().GetProductById(productId,
                (response) =>
                {
                    if (response.isSuccess && response.result != null)
                    {
                        LoadPreviewByProduct(response.result, categorySlug, onComplete);
                    }
                    else
                    {
                        string error = $"Failed to load product {productId}: {response.message}";
                        Debug.LogError($"[ProductPreviewLoader] {error}");
                        OnPreviewError?.Invoke(error);
                        onComplete?.Invoke(false);
                    }
                },
                (error) =>
                {
                    Debug.LogError($"[ProductPreviewLoader] API Error: {error}");
                    OnPreviewError?.Invoke(error);
                    onComplete?.Invoke(false);
                });
        }

        /// <summary>
        /// Load and show preview with product object
        /// </summary>
        public void LoadPreviewByProduct(ApiClient.ShopProduct product, string categorySlug, Action<bool> onComplete = null)
        {
            if (product == null)
            {
                Debug.LogError("[ProductPreviewLoader] Product is null!");
                OnPreviewError?.Invoke("Product is null");
                onComplete?.Invoke(false);
                return;
            }

            // Hide current preview
            if (_currentWidget != null)
            {
                _currentWidget.HidePreview();
            }

            // Get appropriate widget
            var widget = _registry.GetWidgetForProduct(product, categorySlug);
            if (widget == null)
            {
                string error = $"No compatible widget found for category: {categorySlug}";
                Debug.LogError($"[ProductPreviewLoader] {error}");
                OnPreviewError?.Invoke(error);
                onComplete?.Invoke(false);
                return;
            }

            // Show preview
            try
            {
                _currentWidget = widget;
                _currentProduct = product;
                _currentCategorySlug = categorySlug;

                widget.ShowPreview(product, categorySlug);

                if (_config.enableDebugLogs)
                    Debug.Log($"[ProductPreviewLoader] Preview loaded successfully for: {product.title}");

                OnPreviewLoaded?.Invoke(product);
                onComplete?.Invoke(true);
            }
            catch (Exception e)
            {
                string error = $"Error showing preview: {e.Message}";
                Debug.LogError($"[ProductPreviewLoader] {error}");
                OnPreviewError?.Invoke(error);
                onComplete?.Invoke(false);
            }
        }

        /// <summary>
        /// Close current preview
        /// </summary>
        public void ClosePreview()
        {
            if (_currentWidget != null)
            {
                _currentWidget.HidePreview();
                _currentWidget = null;
            }

            _currentProduct = null;
            _currentCategorySlug = null;

            OnPreviewClosed?.Invoke();
        }

        /// <summary>
        /// Get currently displayed widget
        /// </summary>
        public IProductPreviewWidget GetCurrentWidget()
        {
            return _currentWidget;
        }

        /// <summary>
        /// Get currently displayed product
        /// </summary>
        public ApiClient.ShopProduct GetCurrentProduct()
        {
            return _currentProduct;
        }

        private void OnDestroy()
        {
            if (_registry != null)
            {
                _registry.ClearCache();
            }
        }
    }
}
