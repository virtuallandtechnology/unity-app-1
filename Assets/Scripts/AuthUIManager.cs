// File: Assets/Scripts/VL_NewSystem/UI/AuthUIManager.cs
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace VirtualLand.NewSystem.UI
{
    public class AuthUIManager : MonoBehaviour
    {
        public static AuthUIManager Instance;

        [Header("Panels")]
        public GameObject loadingPanel;
        public GameObject connectionErrorPanel; 
        public GameObject forceUpdatePanel;
        public GameObject signUpPanel;
        public GameObject loginPanel;
        public GameObject errorPopup; 

        [Header("UI Elements")]
        public Image wifiIcon; 
        public TextMeshProUGUI errorPopupText;
        public Image userAvatarImage; 

        private void Awake()
        {
            Instance = this;
            CloseAllPanels();
        }

        public void CloseAllPanels()
        {
            loadingPanel.SetActive(false);
            connectionErrorPanel.SetActive(false);
            forceUpdatePanel.SetActive(false);
            signUpPanel.SetActive(false);
            loginPanel.SetActive(false);
            errorPopup.SetActive(false);
        }

        // --- Animations & Panels ---

        public void ShowLoading()
        {
            CloseAllPanels();
            loadingPanel.SetActive(true);
        }

        public void ShowConnectionError()
        {
            CloseAllPanels();
            connectionErrorPanel.SetActive(true);

            wifiIcon.color = Color.white;
            wifiIcon.DOFade(0.2f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetId("WifiBlink");
        }

        public void StopWifiAnimation()
        {
            DOTween.Kill("WifiBlink");
        }

        public void ShowForceUpdate()
        {
            CloseAllPanels();
            forceUpdatePanel.SetActive(true);
            AnimatePanelEntry(forceUpdatePanel);
        }

        public void ShowSignUp()
        {
            CloseAllPanels();
            signUpPanel.SetActive(true);
            AnimatePanelEntry(signUpPanel);
        }

        public void ShowLogin()
        {
            CloseAllPanels();
            loginPanel.SetActive(true);
            AnimatePanelEntry(loginPanel);
        }

        // نمایش ارور ۳ ثانیه‌ای با انیمیشن
        public void ShowErrorNotification(string message)
        {
            errorPopupText.text = message;
            errorPopup.SetActive(true);
            errorPopup.transform.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();
            seq.Append(errorPopup.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack)); // باز شدن
            seq.AppendInterval(3f); // صبر ۳ ثانیه
            seq.Append(errorPopup.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack)); // بسته شدن
            seq.OnComplete(() => errorPopup.SetActive(false));
        }

        // انیمیشن عمومی باز شدن پنل‌ها
        private void AnimatePanelEntry(GameObject panel)
        {
            panel.transform.localScale = Vector3.one * 0.8f;
            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();
            cg.alpha = 0;

            panel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
            cg.DOFade(1f, 0.4f);
        }

        // --- Avatar Logic ---
        public void UpdateAvatarDisplay(string avatarName)
        {
            // لود کردن از Resources
            Sprite sprite = Resources.Load<Sprite>(avatarName);
            if (sprite != null)
            {
                userAvatarImage.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"Avatar '{avatarName}' not found in Resources folder.");
            }
        }
    }
}