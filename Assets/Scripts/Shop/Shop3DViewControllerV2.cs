using UnityEngine;
using System.Collections.Generic;

namespace Game.Shop.Visuals
{
    /// <summary>
    /// Updated Shop3DViewController with support for multiple viewer types
    /// Manages different 3D product viewers (Character, Car, Weapon, etc.)
    /// </summary>
    public class Shop3DViewControllerV2 : MonoBehaviour
    {
        public static Shop3DViewControllerV2 Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Canvas _mainCanvas; // Added Canvas reference
        [SerializeField] private GameObject _shopRoot;
        [SerializeField] private GameObject _viewerContainer;
        [SerializeField] private UnityEngine.UI.Button _backButton;
        [SerializeField] private UnityEngine.UI.Button _saveButton;

        [Header("Viewers")]
        [SerializeField] private List<MonoBehaviour> _viewers = new List<MonoBehaviour>();

        private I3DProductViewer _currentViewer;
        private ApiClient.ShopProduct _currentProduct;
        private string _currentCategorySlug;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            
            // Setup buttons
            if (_backButton) _backButton.onClick.AddListener(CloseViewer);
            if (_saveButton) _saveButton.onClick.AddListener(SaveCustomization);

            // Hide viewer container initially
            if (_viewerContainer) _viewerContainer.SetActive(false);
        }

        /// <summary>
        /// Show 3D preview for a product
        /// </summary>
        public void ShowPreview(ApiClient.ShopProduct product, string categorySlug)
        {
            Debug.Log($"[Shop3DViewControllerV2] ShowPreview called for Category: {categorySlug}");
            _currentProduct = product;
            _currentCategorySlug = categorySlug;

            // Find appropriate viewer
            I3DProductViewer viewer = FindViewerForCategory(categorySlug);
            if (viewer == null)
            {
                Debug.LogError($"[Shop3DViewControllerV2] No viewer found for category: {categorySlug}. Registered viewers count: {_viewers.Count}");
                foreach(var v in _viewers) {
                   if(v != null) Debug.Log($"Registered Viewer Type: {v.GetType().Name}"); 
                }
                return;
            }

            // Ensure we have the main canvas
            if (_mainCanvas == null)
            {
                // 1. First priority: The Canvas that ShopManager belongs to
                var shopManager = FindObjectOfType<ShopManager>();
                if (shopManager != null)
                {
                    // Find the root canvas of the ShopManager
                    var rootCanvas = shopManager.GetComponentInParent<Canvas>();
                    if (rootCanvas != null && rootCanvas.isRootCanvas) 
                    {
                        _mainCanvas = rootCanvas;
                    }
                }

                // 2. Second priority: Explicit "MainCanvas" tag
                if (_mainCanvas == null)
                {
                     var canvasObj = GameObject.FindWithTag("MainCanvas"); 
                     if (canvasObj != null) _mainCanvas = canvasObj.GetComponent<Canvas>();
                }
            }

           

            if (_mainCanvas != null)
            {
                Debug.Log($"[Shop3DViewControllerV2] Disabling Main Canvas Component: {_mainCanvas.name}");
                _mainCanvas.enabled = false; 
                
                // Also disable GraphicRaycaster to stop clicks
                var raycaster = _mainCanvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                if (raycaster) raycaster.enabled = false;
            }
            else
            {
                Debug.LogError("[Shop3DViewControllerV2] Correct Main Canvas not found! UI might overlap.");
            }

            // Hide shop UI (Legacy support)
            if (_shopRoot) _shopRoot.SetActive(false);
            
            if (_viewerContainer) _viewerContainer.SetActive(true);

            // Cleanup previous viewer
            if (_currentViewer != null && _currentViewer != viewer)
            {
                _currentViewer.Cleanup();
            }

            // Initialize and load new viewer
            _currentViewer = viewer;
            _currentViewer.Initialize(product);
            _currentViewer.LoadModel();

            // Try to load saved customization if exists
            LoadSavedCustomization(product.id);
        }

        /// <summary>
        /// Close the 3D viewer and return to shop
        /// </summary>
        public void CloseViewer()
        {
            if (_currentViewer != null)
            {
                _currentViewer.Cleanup();
                _currentViewer = null;
            }

            if (_viewerContainer) _viewerContainer.SetActive(false);
            
            // Re-enable Main Canvas
            if (_mainCanvas) 
            {
                _mainCanvas.enabled = true;
                var raycaster = _mainCanvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                if (raycaster) raycaster.enabled = true;
            }

            // Show shop UI (Legacy support)
            if (_shopRoot) _shopRoot.SetActive(true);

            _currentProduct = null;
            _currentCategorySlug = null;
        }

        /// <summary>
        /// Save current customization
        /// </summary>
        public void SaveCustomization()
        {
            if (_currentViewer == null || _currentProduct == null)
            {
                Debug.LogWarning("No active viewer or product to save!");
                return;
            }

            string customizationJson = _currentViewer.SaveCustomization();
            if (!string.IsNullOrEmpty(customizationJson))
            {
                // Save to PlayerPrefs with product ID as key
                string saveKey = GetSaveKey(_currentProduct.id);
                PlayerPrefs.SetString(saveKey, customizationJson);
                PlayerPrefs.Save();

                Debug.Log($"Customization saved for product {_currentProduct.id}");
                
                // TODO: Optionally send to server
                // ApiClient.Get().SaveProductCustomization(_currentProduct.id, customizationJson, OnSaveSuccess, OnSaveError);
            }
        }

        /// <summary>
        /// Load saved customization for a product
        /// </summary>
        private void LoadSavedCustomization(int productId)
        {
            string saveKey = GetSaveKey(productId);
            if (PlayerPrefs.HasKey(saveKey))
            {
                string customizationJson = PlayerPrefs.GetString(saveKey);
                if (_currentViewer != null)
                {
                    _currentViewer.LoadCustomization(customizationJson);
                    Debug.Log($"Loaded saved customization for product {productId}");
                }
            }
        }

        /// <summary>
        /// Find the appropriate viewer for a category
        /// </summary>
        private I3DProductViewer FindViewerForCategory(string categorySlug)
        {
            foreach (var viewerMono in _viewers)
            {
                if (viewerMono == null) continue;

                if (viewerMono is I3DProductViewer viewer && viewer.CanHandle(categorySlug))
                {
                    return viewer;
                }
            }
            return null;
        }

        /// <summary>
        /// Get save key for product customization
        /// </summary>
        private string GetSaveKey(int productId)
        {
            return $"ProductCustomization_{productId}";
        }

        /// <summary>
        /// Get current active viewer (for external access)
        /// </summary>
        public I3DProductViewer GetCurrentViewer()
        {
            return _currentViewer;
        }

        /// <summary>
        /// Get current active viewer as specific type
        /// </summary>
        public T GetCurrentViewerAs<T>() where T : class, I3DProductViewer
        {
            return _currentViewer as T;
        }

        /// <summary>
        /// Register a new viewer at runtime
        /// </summary>
        public void RegisterViewer(MonoBehaviour viewer)
        {
            if (viewer is I3DProductViewer && !_viewers.Contains(viewer))
            {
                _viewers.Add(viewer);
            }
        }

        /// <summary>
        /// Clear all saved customizations (for testing)
        /// </summary>
        public void ClearAllCustomizations()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("All customizations cleared");
        }
    }
}
