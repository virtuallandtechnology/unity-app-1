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
                _characterViewer.Initialize(characterProduct, CharacterLoadMode.CustomizedData);

                // Strategy: Load from API/savedStyle if available, otherwise check fallbacks.
                // We use LoadFromCharacterObject to set up the base, then Apply customizations.

                // Find config for base model
                var previewConfig = Resources.Load<Game.Shop.Preview.PreviewSystemConfig>("Shop/Configs/PreviewSystemConfig");
                Game.Shop.Preview.PreviewWidgetConfig foundConfig = null;
                
                if (previewConfig != null)
                {
                    string[] characterCategories = new string[]
                    {
                        "CIVILIANS", "GANG-MEMBERS", "POLICE-LAW",
                        "SPECIAL-OPS", "VIP-BUSINESS", "CUSTOM-AVATARS"
                    };

                    foreach (var category in characterCategories)
                    {
                        var config = previewConfig.FindBestWidgetConfig(characterProduct, category);
                        if (config != null && config.characterObject != null && config.baseCharacterPrefab != null)
                        {
                            foundConfig = config;
                            break;
                        }
                    }
                }

                if (foundConfig != null)
                {
                    // Load the base character
                    _characterViewer.LoadFromCharacterObject(
                        foundConfig.characterObject,
                        foundConfig.baseCharacterPrefab,
                        CharacterLoadMode.CustomizedData
                    );
                }
                else
                {
                    // Fallback to model if no config
                    _characterViewer.LoadModel();
                }

                // NOW apply customization (API parameter has highest priority)
                if (!string.IsNullOrEmpty(savedStyle))
                {
                    Debug.Log($"[MainCharacterEditor] Applying API customization for {characterProduct.title}");
                    _characterViewer.LoadCustomization(savedStyle);
                }
                else
                {
                    // Try local fallback if API is empty
                    var localSaveData = BMAC_SaveSystem.GetDataFromID(characterProduct.title);
                    if (localSaveData != null)
                    {
                        Debug.Log($"[MainCharacterEditor] Applying local fallback customization for {characterProduct.title}");
                        string json = JsonUtility.ToJson(localSaveData);
                        _characterViewer.LoadCustomization(json);
                    }
                }
            }

            _currentCategoryIndex = 0;
            canvasShop.enabled = false;
            UpdateEditorUI();
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

            // Capture current style
            string currentStyle = _characterViewer.SaveCustomization();

            // Use CharacterCreator's save logic which handles local save and API call
            var creator = _characterViewer.GetCharacterCreator();
            if (creator != null)
            {
                creator.SaveCharacter(_currentCharacter.id, _currentCharacter.title, () =>
                {
                    ShowFeedback("Changes saved successfully!");
                    if (_saveButton != null) _saveButton.interactable = true;

                    // Update manager with the new style so it persists in the session
                    if (_characterManager != null && _characterManager.IsMainCharacter(_currentCharacter.id))
                    {
                        _characterManager.SyncFromProfile(_currentCharacter.id, currentStyle);
                    }
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
