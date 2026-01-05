using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Configuration for filtering which categories are displayed in shop and profile
/// </summary>
[CreateAssetMenu(fileName = "CategoryFilterConfig", menuName = "VirtualLand/Category Filter Config")]
public class CategoryFilterConfig : ScriptableObject
{
    [Header("Filter Settings")]
    [Tooltip("Enable to filter categories. Disable to show all categories.")]
    public bool enableFilter = true;

    [Header("Allowed Categories")]
    [Tooltip("List of category slugs to show when filter is enabled (e.g., 'characters01')")]
    public List<string> allowedCategorySlugs = new List<string> { "characters01" };

    /// <summary>
    /// Check if a category should be displayed based on current filter settings
    /// </summary>
    public bool ShouldShowCategory(string categorySlug)
    {
        // If filter is disabled, show all categories
        if (!enableFilter)
            return true;

        // If filter is enabled, only show allowed categories
        return allowedCategorySlugs.Contains(categorySlug);
    }

    private static CategoryFilterConfig _instance;

    /// <summary>
    /// Get singleton instance. Loads from Resources if not already loaded.
    /// </summary>
    public static CategoryFilterConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<CategoryFilterConfig>("CategoryFilterConfig");
                
                if (_instance == null)
                {
                    Debug.LogWarning("[CategoryFilterConfig] No config found in Resources. Creating default instance.");
                    _instance = CreateInstance<CategoryFilterConfig>();
                }
            }
            return _instance;
        }
    }
}
