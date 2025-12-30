using UnityEngine;
using UnityEngine.UI;
using Game.Shop.Preview;
using Bozo.ModularCharacters;

namespace Game.Shop.UI
{
    /// <summary>
    /// Manages the UI state when previewing a product (Shop Canvas vs Preview Canvas)
    /// Handles Back and Save actions
    /// </summary>
    public class ShopPreviewUI : MonoBehaviour
    {
        [Header("Canvas References")]
        [Tooltip("The main Shop UI Canvas/Panel to hide when preview is active")]
        [SerializeField] private GameObject _shopCanvas;
        
        [Tooltip("The Preview UI Canvas/Panel to show when preview is active")]
        [SerializeField] private GameObject _previewCanvas;

        [Header("Buttons")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _saveButton;

        [Header("Feedback")]
        [SerializeField] private Text _feedbackText;

        [Header("Editor UI")]
        [SerializeField] private GameObject _editorPanel;
        [SerializeField] private Text _categoryLabel;
        [SerializeField] private Button _nextCategoryButton;
        [SerializeField] private Button _prevCategoryButton;
        [SerializeField] private Button _nextPartButton;
        [SerializeField] private Button _prevPartButton;

        private string[] _editableCategories = new string[] { "Hair", "Head", "Torso", "Legs" };
        private int _currentCategoryIndex = 0;

        private void Start()
        {
            // Initial State
            if (_previewCanvas != null) _previewCanvas.SetActive(false);
            if (_shopCanvas != null) _shopCanvas.SetActive(true);

            // Subscribe to Loader Events
            var loader = ProductPreviewLoader.Instance;
            if (loader != null)
            {
                loader.OnPreviewLoaded += OnPreviewLoaded;
                loader.OnPreviewClosed += OnPreviewClosed;
            }

            // Button Listeners
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }

            if (_saveButton != null)
            {
                _saveButton.onClick.AddListener(OnSaveClicked);
            }

            // Editor Listeners
            if (_nextCategoryButton) _nextCategoryButton.onClick.AddListener(NextCategory);
            if (_prevCategoryButton) _prevCategoryButton.onClick.AddListener(PrevCategory);
            if (_nextPartButton) _nextPartButton.onClick.AddListener(() => ChangePart(1));
            if (_prevPartButton) _prevPartButton.onClick.AddListener(() => ChangePart(-1));

            UpdateEditorUI();
        }

        private void OnDestroy()
        {
            var loader = ProductPreviewLoader.Instance;
            if (loader != null)
            {
                loader.OnPreviewLoaded -= OnPreviewLoaded;
                loader.OnPreviewClosed -= OnPreviewClosed;
            }
        }

        private void OnPreviewClosed()
        {
            // Show Shop, Hide Preview UI
            if (_shopCanvas != null) _shopCanvas.SetActive(true);
            if (_previewCanvas != null) _previewCanvas.SetActive(false);
            if (_editorPanel != null) _editorPanel.SetActive(false);
        }

        private void OnPreviewLoaded(ApiClient.ShopProduct product)
        {
            // Hide Shop, Show Preview UI
            if (_shopCanvas != null) _shopCanvas.SetActive(false);
            if (_previewCanvas != null) _previewCanvas.SetActive(true);
            
            // Check if editable
            bool isEditable = IsEditable(product);
            if (_editorPanel != null) _editorPanel.SetActive(isEditable);
            if (_saveButton != null) _saveButton.gameObject.SetActive(isEditable);

            if (isEditable)
            {
                _currentCategoryIndex = 0;
                UpdateEditorUI();
            }
        }

        private bool IsEditable(ApiClient.ShopProduct product)
        {
            // Use category slug to determine if editable (e.g. only characters)
            // This is a simple check; expand as needed
            // Currently assuming "CHARCTERS" is the slug, verifying via Configs would be better but this suffices.
            var widget = ProductPreviewLoader.Instance.GetCurrentWidget();
            return widget is Game.Shop.Preview.Adapters.CharacterViewerAdapter;
        }

        private void NextCategory()
        {
            _currentCategoryIndex = (_currentCategoryIndex + 1) % _editableCategories.Length;
            UpdateEditorUI();
        }

        private void PrevCategory()
        {
            _currentCategoryIndex--;
            if (_currentCategoryIndex < 0) _currentCategoryIndex = _editableCategories.Length - 1;
            UpdateEditorUI();
        }

        private void UpdateEditorUI()
        {
            if (_categoryLabel != null)
            {
                _categoryLabel.text = _editableCategories[_currentCategoryIndex];
            }
        }

        private void ChangePart(int direction)
        {
            var widget = ProductPreviewLoader.Instance.GetCurrentWidget();
            var charAdapter = widget as Game.Shop.Preview.Adapters.CharacterViewerAdapter;
            
            if (charAdapter != null)
            {
                var viewer = charAdapter.GetCharacterViewer();
                if (viewer != null)
                {
                    string category = _editableCategories[_currentCategoryIndex];
                    viewer.ChangePart(category, direction);
                }
            }
        }

        private void OnBackClicked()
        {
            ProductPreviewLoader.Instance.ClosePreview();
        }

        private void OnSaveClicked()
        {
            var widget = ProductPreviewLoader.Instance.GetCurrentWidget();
            
            // Try to find CharacterViewerAdapter to get customization
            var characterAdapter = widget as Game.Shop.Preview.Adapters.CharacterViewerAdapter;
            if (characterAdapter != null)
            {
                var viewer = characterAdapter.GetCharacterViewer();
                if (viewer != null)
                {
                    string json = viewer.SaveCustomization();
                    if (!string.IsNullOrEmpty(json))
                    {
                        SendUpdateProfile(json);
                    }
                }
            }
            else
            {
                ShowFeedback("Nothing to save for this item.");
            }
        }

        private void SendUpdateProfile(string styleJson)
        {
            var product = ProductPreviewLoader.Instance.GetCurrentProduct();
            if (product == null) return;

            if (_saveButton) _saveButton.interactable = false;
            ShowFeedback("Saving...");

            ApiClient.Get().UpdateUserProfile(product.id, styleJson,
                (response) =>
                {
                    ShowFeedback("Profile Updated Successfully!");
                    if (_saveButton) _saveButton.interactable = true;
                },
                (error) =>
                {
                    ShowFeedback($"Error: {error}");
                    if (_saveButton) _saveButton.interactable = true;
                });
        }

        private void ShowFeedback(string message)
        {
            if (_feedbackText != null)
            {
                _feedbackText.text = message;
                CancelInvoke(nameof(ClearFeedback));
                Invoke(nameof(ClearFeedback), 3f);
            }
            Debug.Log($"[ShopPreviewUI] {message}");
        }

        private void ClearFeedback()
        {
            if (_feedbackText != null) _feedbackText.text = "";
        }
    }
}
