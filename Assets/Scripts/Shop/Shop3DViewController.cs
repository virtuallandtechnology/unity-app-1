using UnityEngine;
using UnityEngine.UI;

namespace Game.Shop.Visuals
{
    public class Shop3DViewController : MonoBehaviour
    {
        public static Shop3DViewController Instance { get; private set; }

        [SerializeField] private Shop3DConfig _config;
        [SerializeField] private GameObject _shopRoot;
        [SerializeField] private Transform _previewRootContainer;
        [SerializeField] private Button _backButton;

        private Shop3DEnvironment _currentEnvironment;
        private GameObject _currentItemModel;

        private void Awake()
        {
            Instance = this;
            if (_backButton) _backButton.onClick.AddListener(ClosePreview);
            if (_previewRootContainer) _previewRootContainer.gameObject.SetActive(false);
        }

        public void ShowPreview(int productId, string categorySlug)
        {
            var itemPrefab = _config.GetItemPrefab(productId);
            var envPrefab = _config.GetEnvironment(categorySlug);

            if (itemPrefab == null || envPrefab == null) return;

            if (_shopRoot) _shopRoot.SetActive(false);
            if (_previewRootContainer) _previewRootContainer.gameObject.SetActive(true);

            ClearScene();

            _currentEnvironment = Instantiate(envPrefab, _previewRootContainer);

            _currentItemModel = Instantiate(itemPrefab, _currentEnvironment.SpawnPoint);
            _currentItemModel.transform.localPosition = Vector3.zero;
            _currentItemModel.transform.localRotation = Quaternion.identity;
        }

        public void ClosePreview()
        {
            if (_previewRootContainer) _previewRootContainer.gameObject.SetActive(false);
            if (_shopRoot) _shopRoot.SetActive(true);

            ClearScene();
        }

        private void ClearScene()
        {
            if (_currentItemModel) Destroy(_currentItemModel);
            if (_currentEnvironment) Destroy(_currentEnvironment.gameObject);
        }
    }
}
