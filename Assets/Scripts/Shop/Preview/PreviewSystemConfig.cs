using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Game.Shop.Preview
{
    /// <summary>
    /// Main ScriptableObject configuration for the preview system
    /// Create via: Create > Shop > Preview System Config
    /// </summary>
    [CreateAssetMenu(fileName = "PreviewSystemConfig", menuName = "Shop/Preview System Config", order = 0)]
    public class PreviewSystemConfig : ScriptableObject
    {
        [Header("Widget Registry")]
        [Tooltip("List of all available widget configurations")]
        public List<PreviewWidgetConfig> widgets = new List<PreviewWidgetConfig>();

        [Header("Cache Settings")]
        [Tooltip("Maximum number of products to cache")]
        public int maxCacheSize = 20;

        [Tooltip("Cache expiration time in seconds")]
        public float cacheExpirationTime = 300f; // 5 minutes

        [Header("UI Settings")]
        [Tooltip("Fade in/out duration for transitions")]
        public float transitionDuration = 0.3f;

        [Header("Debug")]
        [Tooltip("Enable debug logging")]
        public bool enableDebugLogs = false;

        /// <summary>
        /// Find the best widget config for a given product and category
        /// Returns the widget with highest priority that can handle the product
        /// </summary>
        public PreviewWidgetConfig FindBestWidgetConfig(ApiClient.ShopProduct product, string categorySlug)
        {
            if (widgets == null || widgets.Count == 0)
            {
                if (enableDebugLogs)
                    Debug.LogWarning("[PreviewSystemConfig] No widgets configured!");
                return null;
            }

            // Filter widgets that can handle this category and have valid config
            var compatibleWidgets = widgets.Where(w => 
                w != null && 
                (w.characterObject != null || w.baseCharacterPrefab != null) && 
                w.CanHandleProduct(product.id, categorySlug)
            ).ToList();

            if (compatibleWidgets.Count == 0)
            {
                if (enableDebugLogs)
                    Debug.LogWarning($"[PreviewSystemConfig] No compatible widget found for product: {product.id}, category: {categorySlug}");
                return null;
            }

            // Sort by priority (descending) and return the highest
            var bestWidget = compatibleWidgets.OrderByDescending(w => w.priority).First();

            if (enableDebugLogs)
                Debug.Log($"[PreviewSystemConfig] Selected widget '{bestWidget.widgetId}' (priority: {bestWidget.priority}) for product: {product.id}, category: {categorySlug}");

            return bestWidget;
        }
    }
}
