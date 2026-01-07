using UnityEngine;
using Bozo.ModularCharacters;

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

        [Header("BMAC Character Configuration")]
        [Tooltip("BMAC Character Object containing character data")]
        public CharacterObject characterObject;
        
        [Tooltip("Base BMAC character prefab (OutfitSystem) to instantiate")]
        public OutfitSystem baseCharacterPrefab;
        
        [Header("Product Mapping")]
        [Tooltip("Specific product IDs this widget handles. If empty, handles all products in supportedCategories")]
        public int[] productIds = new int[0];

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

        /// <summary>
        /// Check if this widget can handle a specific product
        /// </summary>
        public bool CanHandleProduct(int productId, string categorySlug)
        {
            // First check if category is supported
            if (!CanHandleCategory(categorySlug))
                return false;

            // If no specific product IDs are defined, handle all products in this category
            if (productIds == null || productIds.Length == 0)
                return true;

            // Check if this specific product ID is in the list
            foreach (var id in productIds)
            {
                if (id == productId)
                    return true;
            }

            return false;
        }
    }
}
