using UnityEngine;

namespace Game.Shop.Preview
{
    /// <summary>
    /// ScriptableObject configuration for a single preview widget
    /// Create instances via: Create > Shop > Preview Widget Config
    /// </summary>
    [CreateAssetMenu(fileName = "WidgetConfig", menuName = "Shop/Preview Widget Config", order = 1)]
    public class PreviewWidgetConfig : ScriptableObject
    {
        [Header("Widget Identity")]
        [Tooltip("Unique identifier for this widget")]
        public string widgetId = "default_widget";

        [Tooltip("Priority for widget selection (higher = preferred)")]
        [Range(0, 100)]
        public int priority = 10;

        [Header("Category Support")]
        [Tooltip("List of category slugs this widget supports (case-insensitive)")]
        public string[] supportedCategories = new string[0];

        [Header("Widget Prefab")]
        [Tooltip("Prefab containing the widget MonoBehaviour with IProductPreviewWidget")]
        public GameObject widgetPrefab;

        [Header("Metadata (Optional)")]
        [Tooltip("Additional metadata for this widget")]
        public string description;

        /// <summary>
        /// Check if this widget supports a specific category
        /// </summary>
        public bool CanHandleCategory(string categorySlug)
        {
            if (supportedCategories == null || supportedCategories.Length == 0)
            {
                // Empty means it handles all categories (fallback widget)
                return true;
            }

            foreach (var category in supportedCategories)
            {
                if (!string.IsNullOrEmpty(category) && 
                    category.Trim().Equals(categorySlug.Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
