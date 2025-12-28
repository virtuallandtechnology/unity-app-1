using UnityEngine;

namespace Game.Shop.Visuals
{
    /// <summary>
    /// Base interface for all 3D product viewers
    /// Implement this for different product types (Character, Car, Weapon, etc.)
    /// </summary>
    public interface I3DProductViewer
    {
        /// <summary>
        /// Initialize the viewer with product data
        /// </summary>
        void Initialize(ApiClient.ShopProduct productData);

        /// <summary>
        /// Load and display the 3D model
        /// </summary>
        void LoadModel();

        /// <summary>
        /// Save current customization state
        /// Returns JSON string of customization data
        /// </summary>
        string SaveCustomization();

        /// <summary>
        /// Load customization from saved data
        /// </summary>
        void LoadCustomization(string customizationJson);

        /// <summary>
        /// Clean up resources when viewer is closed
        /// </summary>
        void Cleanup();

        /// <summary>
        /// Get the root GameObject of the viewer
        /// </summary>
        GameObject GetViewerRoot();

        /// <summary>
        /// Check if this viewer can handle the given product type
        /// </summary>
        bool CanHandle(string categorySlug);
    }
}
