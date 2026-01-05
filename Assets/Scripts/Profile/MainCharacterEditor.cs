using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Shop.Visuals;
using static ApiClient;
using Bozo.ModularCharacters;

namespace VirtualLand
{
    /// <summary>
    /// Editor for the main character - similar to Fortnite's character editor
    /// </summary>
    public class MainCharacterEditor : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _editorPanel;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _backButton;

        [Header("Character Viewer")]
        [SerializeField] private CharacterViewer _characterViewer;

        [Header("Editor Controls")]
        [SerializeField] private TextMeshProUGUI _categoryLabel;
        [SerializeField] private Button _nextCategoryButton;
        [SerializeField] private Button _prevCategoryButton;
        [SerializeField] private Button _nextPartButton;
        [SerializeField] private Button _prevPartButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private TextMeshProUGUI _feedbackText;
        [SerializeField] private TextMeshProUGUI _characterNameText;
        [SerializeField] private Canvas canvasShop;

        private string[] _editableCategories = new string[] { "Hair", "Head", "Torso", "Legs" };
        private int _currentCategoryIndex = 0;
        private ShopProduct _currentCharacter;
        private MainCharacterManager _characterManager;

        private void Start()
        {
            _characterManager = MainCharacterManager.Instance;

            if (_editorPanel != null) _editorPanel.SetActive(false);

            // Button listeners
            if (_closeButton != null)
                _closeButton.onClick.AddListener(CloseEditor);

            if (_backButton != null)
                _backButton.onClick.AddListener(CloseEditor);

            if (_nextCategoryButton != null)
                _nextCategoryButton.onClick.AddListener(NextCategory);

            if (_prevCategoryButton != null)
                _prevCategoryButton.onClick.AddListener(PrevCategory);

            if (_nextPartButton != null)
                _nextPartButton.onClick.AddListener(() => ChangePart(1));

            if (_prevPartButton != null)
                _prevPartButton.onClick.AddListener(() => ChangePart(-1));

            if (_saveButton != null)
                _saveButton.onClick.AddListener(OnSaveClicked);

        }

        /// <summary>
        /// Open editor for main character
        /// </summary>
        public void OpenEditor(ShopProduct characterProduct, string savedStyle = null)
        {
            if (characterProduct == null)
            {
                Debug.LogWarning("[MainCharacterEditor] Cannot open editor for null character");
                return;
            }
            _characterViewer.gameObject.SetActive(true);
            _currentCharacter = characterProduct;

            if (_editorPanel != null) _editorPanel.SetActive(true);

            if (_characterNameText != null)
                _characterNameText.text = characterProduct.title;

            // Initialize character viewer
            if (_characterViewer != null)
            {
                _characterViewer.Initialize(characterProduct);

                _characterViewer.LoadModel();

                // Check for local save first
                var localSaveData = BMAC_SaveSystem.GetDataFromID(characterProduct.title);
                // var localSaveData = BMAC_SaveSystem.GetDataFromID(characterProduct.title); // Using title as ID for local save matching

                if (localSaveData != null)
                {
                    string json = JsonUtility.ToJson(localSaveData);
                    _characterViewer.LoadCustomization(json);
                    Debug.Log($"[MainCharacterEditor] Loaded local save for {characterProduct.title}");
                }
                // Load saved customization from API if available and no local save
                else if (!string.IsNullOrEmpty(savedStyle))
                {
                    _characterViewer.LoadCustomization(savedStyle);
                }
            }

            _currentCategoryIndex = 0;
            UpdateEditorUI();
            canvasShop.enabled = false;
        }

        /// <summary>
        /// Close the editor
        /// </summary>
        public void CloseEditor()
        {
            if (_editorPanel != null) _editorPanel.SetActive(false);
            if (_characterViewer != null) _characterViewer.Cleanup();
            _currentCharacter = null;
            canvasShop.enabled = true;
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
            if (_characterViewer == null) return;

            string category = _editableCategories[_currentCategoryIndex];
            _characterViewer.ChangePart(category, direction);
        }

        private void OnSaveClicked()
        {
            if (_characterViewer == null || _currentCharacter == null)
            {
                ShowFeedback("No character to save");
                return;
            }

            // Disable save button during save
            if (_saveButton != null) _saveButton.interactable = false;
            ShowFeedback("Saving...");

            // Use CharacterCreator's save logic which handles local save and API call
            var creator = _characterViewer.GetCharacterCreator();
            if (creator != null)
            {
                creator.SaveCharacter(_currentCharacter.id, _currentCharacter.title, () =>
                {
                    ShowFeedback("Changes saved successfully!");
                    if (_saveButton != null) _saveButton.interactable = true;

                    // Update manager with the new style (we might need to reload it or just notify)
                    // Since we saved to disk, next time we load, we should load from disk if possible
                    // or rely on the API update we just did.
                });
            }
            else
            {
                ShowFeedback("Error: Character Creator not found");
                if (_saveButton != null) _saveButton.interactable = true;
            }
        }


        private void ShowFeedback(string message)
        {
            if (_feedbackText != null)
            {
                _feedbackText.text = message;
                CancelInvoke(nameof(ClearFeedback));
                Invoke(nameof(ClearFeedback), 3f);
            }
            Debug.Log($"[MainCharacterEditor] {message}");
        }

        private void ClearFeedback()
        {
            if (_feedbackText != null) _feedbackText.text = "";
        }
    }
}

