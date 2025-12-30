using UnityEngine;

namespace Game.Shop.Preview
{
    /// <summary>
    /// Interface for all product preview widgets
    /// Implement this interface for different product types (Character, Car, Weapon, etc.)
    /// </summary>
    public interface IProductPreviewWidget
    {
        /// <summary>
        /// Unique identifier for this widget type
        /// </summary>
        string WidgetId { get; }

        /// <summary>
        /// Priority for widget selection (higher = preferred)
        /// Used when multiple widgets can handle the same category
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Check if this widget can handle a specific product/category
        /// </summary>
        bool CanHandleProduct(ApiClient.ShopProduct product, string categorySlug);

        /// <summary>
        /// Show preview for the given product
        /// </summary>
        void ShowPreview(ApiClient.ShopProduct product, string categorySlug);

        /// <summary>
        /// Hide the preview (but keep state)
        /// </summary>
        void HidePreview();

        /// <summary>
        /// Clean up and destroy resources
        /// </summary>
        void Cleanup();

        /// <summary>
        /// Get the root GameObject of this widget
        /// </summary>
        GameObject GetWidgetRoot();
    }
}
