using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using VirtualLand;

public class ChargePanelController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField _amountInput;
    [SerializeField] private TMP_InputField _phoneNumberInput; 
    [SerializeField] private Button _payButton;
    [SerializeField] private Button _checkStatusButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private GameObject _loadingObj; 
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Settings")]
    public float animDuration = 0.4f;

    private int _currentUserId;
    private string _lastOrderId; 

    private void Start()
    {
        _payButton.onClick.AddListener(OnPayClicked);
        _checkStatusButton.onClick.AddListener(OnVerifyClicked);
        _closeButton.onClick.AddListener(Hide);

        _checkStatusButton.interactable = false;

        if (ApiClient.GetPlayer() != null)
        {
            _currentUserId = ApiClient.GetPlayer().id;
        }

       // gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero;
        if (_canvasGroup != null) _canvasGroup.alpha = 0;

        transform.DOScale(Vector3.one, animDuration).SetEase(Ease.OutBack);
        if (_canvasGroup != null) _canvasGroup.DOFade(1f, animDuration);
    }

    public void Hide()
    {
        transform.DOScale(Vector3.zero, animDuration * 0.8f).SetEase(Ease.InBack)
            .OnComplete(() => gameObject.SetActive(false));
    }

    private void OnPayClicked()
    {
        string amount = _amountInput.text;
        if (string.IsNullOrEmpty(amount))
        {
            NotificationController.Get().Show("لطفا مبلغ را وارد کنید");
            return;
        }

        SetLoading(true);

        // 1. فراخوانی API اول برای دریافت لینک
        ApiClient.Get().StartEasyBitPayment(amount,
            (response) => {
                SetLoading(false);
                if (response.isSuccess && response.result != null)
                {
                    _lastOrderId = response.result.order_id;

                    Application.OpenURL(response.result.redirect);

                    _checkStatusButton.interactable = true;
                  //  NotificationController.Get().Show("مرورگر باز شد. پس از پرداخت دکمه بررسی را بزنید.");
                }
            },
            (error) => {
                SetLoading(false);
               // NotificationController.Get().Show("خطا در ایجاد درگاه: " + error);
            }
        );
    }

    private void OnVerifyClicked()
    {
        string phone = _phoneNumberInput.text;
        string amountStr = _amountInput.text;

        if (string.IsNullOrEmpty(phone))
        {
            NotificationController.Get().Show("لطفا شماره موبایل را وارد کنید");
            return;
        }

        SetLoading(true);

        var verifyData = new ApiClient.PaymentVerificationRequest
        {
            user_id = _currentUserId, 
            msisdn = phone,
            amount = double.Parse(amountStr),
            description = "خرید اشتراک (از اپلیکیشن)",
            national_code = null
        };

        ApiClient.Get().VerifyEasyBitPayment(verifyData,
            (response) => {
                SetLoading(false);
                if (response.isSuccess)
                {
                  //  NotificationController.Get().Show("پرداخت با موفقیت انجام شد!");
                    Hide();
                    var walletManager = FindFirstObjectByType<WalletManager>();
                    if (walletManager != null)
                    {
                        ApiClient.Get().RequestWalletsUpdate();
                    }
                }
                else
                {
                   // NotificationController.Get().Show("پرداخت تایید نشد یا انجام نشده است.");
                }
            },
            (error) => {
                SetLoading(false);
              //  NotificationController.Get().Show("خطا در بررسی وضعیت: " + error);
            }
        );
    }

    private void SetLoading(bool show)
    {
        if (_loadingObj != null) _loadingObj.SetActive(show);
      //  else LoadingHandler.Get().SetVisible(show);
    }
}