using UnityEngine;
using System;

namespace Game.Shop.Visuals
{
    /// <summary>
    /// Car viewer implementation - Example for other product types
    /// </summary>
    public class CarViewer : MonoBehaviour, I3DProductViewer
    {
        [Header("References")]
        [SerializeField] private GameObject _viewerRoot;
        [SerializeField] private Transform _carSpawnPoint;
        [SerializeField] private GameObject _carPrefab; // Default car prefab

        [Header("Supported Categories")]
        [SerializeField] private string[] _supportedCategories = new string[]
        {
            "CARS", "SPORTS-CARS", "SUPERCARS", "MUSCLE-CARS",
            "SUVS", "MOTORCYCLES", "TRUCKS-VANS", "OFF-ROAD"
        };

        private GameObject _currentCar;
        private ApiClient.ShopProduct _currentProduct;
        private CarCustomizationData _currentCustomization;

        public void Initialize(ApiClient.ShopProduct productData)
        {
            _currentProduct = productData;
            _viewerRoot.SetActive(true);
        }

        public void LoadModel()
        {
            // Clear existing car
            if (_currentCar != null)
            {
                Destroy(_currentCar);
            }

            // TODO: Load car prefab based on product data
            // For now, use default prefab
            _currentCar = Instantiate(_carPrefab, _carSpawnPoint);
            _currentCar.transform.localPosition = Vector3.zero;
            _currentCar.transform.localRotation = Quaternion.identity;

            // Apply saved customization if exists
            if (_currentCustomization != null)
            {
                ApplyCustomization(_currentCustomization);
            }
        }

        public string SaveCustomization()
        {
            if (_currentCar == null)
            {
                Debug.LogWarning("No car loaded to save!");
                return null;
            }

            // Create customization data
            CarCustomizationData data = new CarCustomizationData
            {
                productId = _currentProduct.id,
                paintColor = GetCarPaintColor(),
                rimType = GetRimType(),
                // Add more customization fields as needed
            };

            string json = JsonUtility.ToJson(data);
            Debug.Log($"Car customization saved: {json}");
            return json;
        }

        public void LoadCustomization(string customizationJson)
        {
            if (string.IsNullOrEmpty(customizationJson))
            {
                Debug.LogWarning("Empty customization data!");
                return;
            }

            try
            {
                CarCustomizationData data = JsonUtility.FromJson<CarCustomizationData>(customizationJson);
                _currentCustomization = data;

                if (_currentCar != null)
                {
                    ApplyCustomization(data);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load car customization: {e.Message}");
            }
        }

        private void ApplyCustomization(CarCustomizationData data)
        {
            if (_currentCar == null) return;

            // Apply paint color
            SetCarPaintColor(data.paintColor);

            // Apply rim type
            SetRimType(data.rimType);

            // Apply other customizations...
        }

        public void Cleanup()
        {
            if (_currentCar != null)
            {
                Destroy(_currentCar);
                _currentCar = null;
            }

            _currentCustomization = null;
            _viewerRoot.SetActive(false);
        }

        public GameObject GetViewerRoot()
        {
            return _viewerRoot;
        }

        public bool CanHandle(string categorySlug)
        {
            foreach (var category in _supportedCategories)
            {
                if (category.Equals(categorySlug, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        // Car-specific customization methods
        public void SetCarPaintColor(Color color)
        {
            if (_currentCar == null) return;

            // Find car body renderer and apply color
            var renderers = _currentCar.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer.CompareTag("CarBody")) // Use tags to identify car parts
                {
                    renderer.material.color = color;
                }
            }
        }

        public Color GetCarPaintColor()
        {
            if (_currentCar == null) return Color.white;

            var renderers = _currentCar.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer.CompareTag("CarBody"))
                {
                    return renderer.material.color;
                }
            }
            return Color.white;
        }

        public void SetRimType(int rimType)
        {
            // TODO: Implement rim changing logic
            Debug.Log($"Setting rim type to: {rimType}");
        }

        public int GetRimType()
        {
            // TODO: Get current rim type
            return 0;
        }
    }

    [Serializable]
    public class CarCustomizationData
    {
        public int productId;
        public Color paintColor = Color.white;
        public int rimType = 0;
        public int engineType = 0;
        public int spoilerType = 0;
        // Add more fields as needed
    }
}
