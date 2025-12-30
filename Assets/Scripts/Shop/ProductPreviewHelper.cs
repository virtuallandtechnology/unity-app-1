using UnityEngine;
using System;

namespace Game.Shop
{
    /// <summary>
    /// Helper class to load product previews by Product ID
    /// Works with existing Shop3DViewControllerV2 system
    /// NO CHANGES TO EXISTING CODE REQUIRED!
    /// </summary>
    public class ProductPreviewHelper : MonoBehaviour
    {
        public static ProductPreviewHelper Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private ShopManager _shopManager;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Load and show preview by Product ID
        /// Fetches product from API and displays it using existing 3D viewer
        /// </summary>
        public void ShowPreviewByProductId(int productId, string categorySlug, Action<bool> onComplete = null)
        {
            ApiClient.Get().GetProductById(productId, 
                (response) =>
                {
                    if (response.isSuccess && response.result != null)
                    {
                        ShowPreview(response.result, categorySlug);
                        onComplete?.Invoke(true);
                    }
                    else
                    {
                        Debug.LogError($"[ProductPreviewHelper] Failed to load product {productId}: {response.message}");
                        onComplete?.Invoke(false);
                    }
                },
                (error) =>
                {
                    Debug.LogError($"[ProductPreviewHelper] Error loading product {productId}: {error}");
                    onComplete?.Invoke(false);
                });
        }

        /// <summary>
        /// Show preview using existing Shop3DViewControllerV2
        /// </summary>
        public void ShowPreview(ApiClient.ShopProduct product, string categorySlug)
        {
            if (_shopManager == null)
            {
                _shopManager = FindObjectOfType<ShopManager>();
            }

            if (_shopManager != null)
            {
                var viewer = _shopManager.Get3DViewController();
                if (viewer != null)
                {
                    viewer.ShowPreview(product, categorySlug);
                }
                else
                {
                    Debug.LogError("[ProductPreviewHelper] Shop3DViewControllerV2 not found!");
                }
            }
            else
            {
                Debug.LogError("[ProductPreviewHelper] ShopManager not found in scene!");
            }
        }
    }
}
