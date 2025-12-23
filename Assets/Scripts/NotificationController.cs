using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationController : MonoBehaviour
{
    private static NotificationController instance;
    [SerializeField] private GameObject NotiicationRoot;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private Button _okButton;
    [SerializeField] private Button _cancelButton;

    [SerializeField] private GameObject timerPanel;
    [SerializeField] private TMP_Text _TimerPaneldescriptionText;
    [SerializeField] private float timerDelay = 4;

    [SerializeField] private GameObject yesNoPanel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static NotificationController Get()
    {
        return instance;
    }

    public void Show(string title, string description, Action onOk, Action onCancel)
    {
        if (NotiicationRoot != null) NotiicationRoot.SetActive(true);
        if (timerPanel != null) timerPanel.SetActive(false);
        if (yesNoPanel != null)
        {
            yesNoPanel.SetActive(true);
            yesNoPanel.transform.localScale = Vector3.zero;
            yesNoPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }

        if (_titleText != null) _titleText.text = title;
        if (_descriptionText != null) _descriptionText.text = description;

        if (_okButton != null)
        {
            _okButton.onClick.RemoveAllListeners();
            _okButton.onClick.AddListener(() =>
            {
                onOk?.Invoke();
                ClosePanel(yesNoPanel);
            });
        }

        if (_cancelButton != null)
        {
            _cancelButton.onClick.RemoveAllListeners();
            _cancelButton.onClick.AddListener(() =>
            {
                onCancel?.Invoke();
                ClosePanel(yesNoPanel);
            });
        }
    }

    public void Show(string description)
    {
        if (NotiicationRoot != null) NotiicationRoot.SetActive(true);
        if (yesNoPanel != null) yesNoPanel.SetActive(false);

        if (_TimerPaneldescriptionText != null)
            _TimerPaneldescriptionText.text = description;

        if (timerPanel != null)
        {
            timerPanel.SetActive(true);
            timerPanel.transform.localScale = Vector3.zero;

            timerPanel.transform.DOScale(Vector3.one, 0.5f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    DOVirtual.DelayedCall(timerDelay, () =>
                    {
                        if (timerPanel != null)
                        {
                            timerPanel.transform.DOScale(Vector3.zero, 0.3f)
                                .SetEase(Ease.InBack)
                                .OnComplete(() =>
                                {
                                    timerPanel.SetActive(false);
                                    if (NotiicationRoot != null) NotiicationRoot.SetActive(false);
                                });
                        }
                    });
                });
        }
    }

    private void ClosePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    panel.SetActive(false);
                    if (NotiicationRoot != null) NotiicationRoot.SetActive(false);
                });
        }
    }
}

public static class ApiErrorHandler
{
    public static void HandleError(int statusCode, string message)
    {
        if (NotificationController.Get() == null) return;

        switch (statusCode)
        {
            case 401:
                NotificationController.Get().Show(
                    "Unauthorized",
                    "Session expired. Please login again.",
                    () =>
                    {
                        Debug.Log("Redirect to Login or Refresh Token");
                    },
                    null
                );
                break;

            case 402:
                NotificationController.Get().Show(
                    "Payment Required",
                    "Insufficient funds. Please charge your wallet.",
                    () =>
                    {
                        Debug.Log("Open Wallet Page");
                    },
                    null
                );
                break;

            case 403:
                NotificationController.Get().Show("Access Denied: You do not have permission.");
                break;

            case 404:
                NotificationController.Get().Show("Not Found: The requested resource was not found.");
                break;

            case 420:
                NotificationController.Get().Show(!string.IsNullOrEmpty(message) ? message : "Invalid Parameters (420)");
                break;

            case 422:
                NotificationController.Get().Show(!string.IsNullOrEmpty(message) ? message : "Validation Error (422)");
                break;

            case 500:
                NotificationController.Get().Show("Server Error: Please try again later.");
                break;

            default:
                NotificationController.Get().Show($"Error {statusCode}: {message}");
                break;
        }
    }
}