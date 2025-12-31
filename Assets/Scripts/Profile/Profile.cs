using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ApiClient;
using Game.Shop.Visuals;

namespace VirtualLand
{
    public class Profile : MonoBehaviour
    {
        [Header("UI References")]
        public TMP_InputField _username;
        public TMP_Text _email;

        [Header("Avatar Images")]
        public Image Avatar;
        public Image AvatarinSelectAvatar;
        public Image avatarImageProfile;
        public Image avatarImageHome;

        [Header("Purchased Characters")]
        [SerializeField] private Button _viewPurchasedCharactersButton;
        [SerializeField] private PurchasedCharactersViewer _purchasedCharactersViewer;

        [Header("Main Character Selection")]
        [SerializeField] private Button _selectMainCharacterButton;
        [SerializeField] private MainCharacterSelector _mainCharacterSelector;

        [Header("Character Editor")]
        [SerializeField] private Button _editMainCharacterButton;
        [SerializeField] private MainCharacterEditor _mainCharacterEditor;

        [Header("Main Character Display")]
        [SerializeField] private TextMeshProUGUI _mainCharacterText;

        private int _currentProfileID;

        private void OnEnable()
        {
            if (LoadingHandler.Get() != null) LoadingHandler.Get().SetVisible(true);

            ApiClient.Get().GetProfileInfo(OnGetInfoSuccess, OnFail);

            // Setup purchased characters button
            if (_viewPurchasedCharactersButton != null)
            {
                _viewPurchasedCharactersButton.onClick.RemoveAllListeners();
                _viewPurchasedCharactersButton.onClick.AddListener(OnViewPurchasedCharactersClicked);
            }

            // Setup main character selection button
            if (_selectMainCharacterButton != null)
            {
                _selectMainCharacterButton.onClick.RemoveAllListeners();
                _selectMainCharacterButton.onClick.AddListener(OnSelectMainCharacterClicked);
            }

            // Setup edit main character button
            if (_editMainCharacterButton != null)
            {
                _editMainCharacterButton.onClick.RemoveAllListeners();
                _editMainCharacterButton.onClick.AddListener(OnEditMainCharacterClicked);
            }

            // Subscribe to main character changes
            var mainCharacterManager = MainCharacterManager.Instance;
            if (mainCharacterManager != null)
            {
                mainCharacterManager.OnMainCharacterChanged += OnMainCharacterChanged;
            }

            // Update main character display
            UpdateMainCharacterDisplay();
        }

        private void OnViewPurchasedCharactersClicked()
        {
            if (_purchasedCharactersViewer != null)
            {
                _purchasedCharactersViewer.OpenViewer();
            }
            else
            {
                Debug.LogWarning("[Profile] PurchasedCharactersViewer is not assigned!");
            }
        }

        private void OnSelectMainCharacterClicked()
        {
            if (_mainCharacterSelector != null)
            {
                _mainCharacterSelector.OpenSelector();
            }
            else
            {
                Debug.LogWarning("[Profile] MainCharacterSelector is not assigned!");
            }
        }

        private void OnEditMainCharacterClicked()
        {
            // Close purchased characters viewer if open
            if (_purchasedCharactersViewer != null)
            {
                _purchasedCharactersViewer.CloseViewer(cleanup: false);
            }

            var mainCharacterManager = MainCharacterManager.Instance;
            if (mainCharacterManager == null || mainCharacterManager.MainCharacterId <= 0)
            {
                if (NotificationController.Get() != null)
                {
                    NotificationController.Get().Show("Please select a main character first");
                }
                return;
            }

            // Load main character and show editor
            mainCharacterManager.LoadMainCharacterProduct(
                (product) =>
                {
                    if (_mainCharacterEditor != null)
                    {
                        _mainCharacterEditor.OpenEditor(product, mainCharacterManager.MainCharacterStyle);
                    }
                    else
                    {
                        Debug.LogWarning("[Profile] MainCharacterEditor is not assigned!");
                    }
                },
                (error) =>
                {
                    Debug.LogError($"[Profile] Failed to load main character: {error}");
                    if (NotificationController.Get() != null)
                    {
                        NotificationController.Get().Show($"Error loading character: {error}");
                    }
                });
        }

        private void UpdateMainCharacterDisplay()
        {
            if (_mainCharacterText == null) return;

            var mainCharacterManager = MainCharacterManager.Instance;
            if (mainCharacterManager == null) return;

            if (mainCharacterManager.MainCharacterId > 0)
            {
                // Load main character name
                mainCharacterManager.LoadMainCharacterProduct(
                    (product) =>
                    {
                        _mainCharacterText.text = $"Main Character: {product.title}";
                    },
                    (error) =>
                    {
                        _mainCharacterText.text = $"Main Character: ID {mainCharacterManager.MainCharacterId}";
                    });
            }
            else
            {
                _mainCharacterText.text = "No main character selected";
            }
        }

        private void OnMainCharacterChanged(int characterId)
        {
            UpdateMainCharacterDisplay();
        }

        private void OnDisable()
        {
            // Unsubscribe from main character changes
            var mainCharacterManager = MainCharacterManager.Instance;
            if (mainCharacterManager != null)
            {
                mainCharacterManager.OnMainCharacterChanged -= OnMainCharacterChanged;
            }
        }

        private void OnGetInfoSuccess(ApiResponse<User> response)
        {
            if (LoadingHandler.Get() != null) LoadingHandler.Get().SetVisible(false);

            if (response.result != null)
            {
                var user = response.result;

                if (_email != null) _email.text = user.email;

                if (_username != null) _username.text = user.GetUsername();

                if (user.profile != null)
                {
                    _currentProfileID = user.profile.avatar_id;
                    
                    // Sync main character selection
                    if (MainCharacterManager.Instance != null && _currentProfileID > 0)
                    {
                        MainCharacterManager.Instance.SyncMainCharacterId(_currentProfileID);
                    }

                    UpdateAllImages(_currentProfileID);
                }
            }
        }

        public void SetProfile(int id)
        {
            _currentProfileID = id;

            UpdateAllImages(_currentProfileID);

            UpdateProfile();
        }

        private void UpdateAllImages(int id)
        {
            if (AvatarsConfig.Instance == null || AvatarsConfig.Instance.Avatars == null) return;

            if (id >= 0 && id < AvatarsConfig.Instance.Avatars.Count)
            {
                Sprite selectedSprite = AvatarsConfig.Instance.Avatars[id].sprite;

                if (Avatar != null) Avatar.sprite = selectedSprite;
                if (AvatarinSelectAvatar != null) AvatarinSelectAvatar.sprite = selectedSprite;
                if (avatarImageProfile != null) avatarImageProfile.sprite = selectedSprite;
                if (avatarImageHome != null) avatarImageHome.sprite = selectedSprite;
            }
        }

        public void UpdateProfile()
        {
            var currentprofile = new ApiClient.Profile();

            currentprofile.avatar_id = _currentProfileID;

            ApiClient.Get().UpdateProfile(currentprofile, onUpdateSuccess, OnFail);
        }

        private void OnFail(string errorMsg)
        {
            Debug.LogError("Error: " + errorMsg);

            if (LoadingHandler.Get() != null) LoadingHandler.Get().SetVisible(false);
            if (NotificationController.Get() != null) NotificationController.Get().Show(errorMsg);
        }

        private void onUpdateSuccess(ApiResponse<User> response)
        {
            Debug.Log("Update success");

            if (LoadingHandler.Get() != null) LoadingHandler.Get().SetVisible(false);
            if (NotificationController.Get() != null) NotificationController.Get().Show("Profile Updated Successfully");
        }

        public void LogOut()
        {
            PlayerPrefs.DeleteKey("token");
            SceneManager.LoadScene(0);
        }
    }
}