using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Game.Shop.Preview
{
    /// <summary>
    /// Manages widget instantiation and caching
    /// Handles lazy loading and lifecycle of widget instances
    /// </summary>
    public class ProductPreviewRegistry
    {
        private PreviewSystemConfig _config;
        private Transform _container;
        private Dictionary<string, IProductPreviewWidget> _instantiatedWidgets;

        public ProductPreviewRegistry(PreviewSystemConfig config, Transform container)
        {
            _config = config;
            _container = container;
            _instantiatedWidgets = new Dictionary<string, IProductPreviewWidget>();
        }

        /// <summary>
        /// Get or create widget for the given product
        /// Uses lazy loading - widgets are only instantiated when first needed
        /// </summary>
        public IProductPreviewWidget GetWidgetForProduct(ApiClient.ShopProduct product, string categorySlug)
        {
            // Find best widget config
            var widgetConfig = _config.FindBestWidgetConfig(product, categorySlug);
            if (widgetConfig == null)
            {
                Debug.LogError($"[ProductPreviewRegistry] No widget config found for category: {categorySlug}");
                return null;
            }

            // For BMAC character widgets, always find the CharacterViewerAdapter in scene
            bool isBMACWidget = widgetConfig.characterObject != null && widgetConfig.baseCharacterPrefab != null;
            
            if (isBMACWidget)
            {
                // Find CharacterViewerAdapter directly (don't use widgetId matching)
                // Include inactive objects because the adapter might be hidden from previous preview
                var adapter = Object.FindObjectOfType<Game.Shop.Preview.Adapters.CharacterViewerAdapter>(includeInactive: true);
                if (adapter != null)
                {
                    // Update config for this product
                    adapter.SetCurrentConfig(widgetConfig);
                    
                    if (_config.enableDebugLogs)
                        Debug.Log($"[ProductPreviewRegistry] Using CharacterViewerAdapter for BMAC widget: {widgetConfig.widgetId}");
                    
                    return adapter;
                }
                else
                {
                    Debug.LogError($"[ProductPreviewRegistry] CharacterViewerAdapter not found in scene!");
                    return null;
                }
            }

            // For non-BMAC widgets, use normal caching logic
            // Check if already instantiated/cached
            if (_instantiatedWidgets.TryGetValue(widgetConfig.widgetId, out var existingWidget))
            {
                if (_config.enableDebugLogs)
                    Debug.Log($"[ProductPreviewRegistry] Reusing existing widget: {widgetConfig.widgetId}");
                return existingWidget;
            }

            // Try to find widget in scene first (don't instantiate)
            var sceneWidget = FindWidgetInScene(widgetConfig.widgetId);
            if (sceneWidget != null)
            {
                _instantiatedWidgets[widgetConfig.widgetId] = sceneWidget;
                if (_config.enableDebugLogs)
                    Debug.Log($"[ProductPreviewRegistry] Found widget in scene: {widgetConfig.widgetId}");
                return sceneWidget;
            }

            // If not found in scene, instantiate from prefab as fallback
            var newWidget = InstantiateWidget(widgetConfig);
            if (newWidget != null)
            {
                _instantiatedWidgets[widgetConfig.widgetId] = newWidget;
                if (_config.enableDebugLogs)
                    Debug.Log($"[ProductPreviewRegistry] Instantiated new widget: {widgetConfig.widgetId}");
            }

            return newWidget;
        }

        private IProductPreviewWidget FindWidgetInScene(string widgetId)
        {
            // Find all widgets in scene
            var allWidgets = Object.FindObjectsOfType<MonoBehaviour>()
                .OfType<IProductPreviewWidget>()
                .ToArray();

            foreach (var widget in allWidgets)
            {
                if (widget.WidgetId == widgetId)
                {
                    return widget;
                }
            }

            return null;
        }

        private IProductPreviewWidget InstantiateWidget(PreviewWidgetConfig config)
        {
            // This method is now only for non-BMAC widgets
            // BMAC widgets are handled directly in GetWidgetForProduct
            
            Debug.LogError($"[ProductPreviewRegistry] Cannot instantiate widget: {config.widgetId}. BMAC widgets should be in scene, other widgets not yet supported.");
            return null;
        }

        /// <summary>
        /// Clear all instantiated widgets
        /// </summary>
        public void ClearCache()
        {
            foreach (var widget in _instantiatedWidgets.Values)
            {
                widget.Cleanup();
                if (widget.GetWidgetRoot() != null)
                {
                    Object.Destroy(widget.GetWidgetRoot());
                }
            }
            _instantiatedWidgets.Clear();
        }

        /// <summary>
        /// Get widget by ID (if already instantiated)
        /// </summary>
        public IProductPreviewWidget GetWidgetById(string widgetId)
        {
            _instantiatedWidgets.TryGetValue(widgetId, out var widget);
            return widget;
        }
    }
}
