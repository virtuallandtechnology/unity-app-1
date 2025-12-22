using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _priceText; // برای اینونتوری مخفی می‌شود
    [SerializeField] private Image _productImage;
    [SerializeField] private Toggle _selectToggle;
    [SerializeField] private GameObject _ownedBadge; // نشانگری که بگوید "خریداری شده"

    private ApiClient.ShopProduct _data;
    private bool _isInventoryMode;

    public void Setup(ApiClient.ShopProduct data, bool isInventoryMode)
    {
        _data = data;
        _isInventoryMode = isInventoryMode;

        _titleText.text = data.title;

        // لود عکس با استفاده از ImageLoader که قبلا ساختیم
        if (ImageLoader.Instance != null && !string.IsNullOrEmpty(data.image))
            ImageLoader.Instance.LoadImage(data.image, _productImage);

        // تنظیمات بر اساس مود (فروشگاه یا اینونتوری)
        if (_isInventoryMode)
        {
            _priceText.gameObject.SetActive(false); // در اینونتوری قیمت مهم نیست
            if (_ownedBadge) _ownedBadge.SetActive(true);

            // در حالت اینونتوری شاید تاگل معنی "تجهیز کردن" بدهد یا اصلا نباشد
            _selectToggle.gameObject.SetActive(false);
        }
        else
        {
            // --- حالت فروشگاه ---
            _priceText.gameObject.SetActive(true);
            if (_ownedBadge) _ownedBadge.SetActive(false);
            _selectToggle.gameObject.SetActive(true);

            if (data.price != null && data.price.Count > 0)
                _priceText.text = $"{data.price[0].price:N0} {data.price[0].payable}";

            // تنظیم تاگل
            _selectToggle.onValueChanged.RemoveAllListeners();
            _selectToggle.isOn = false; // پیش‌فرض خاموش

            _selectToggle.onValueChanged.AddListener((isOn) => {
                if (isOn)
                    CartController.Instance.AddToCart(_data);
                else
                    CartController.Instance.RemoveFromCart(_data);
            });
        }
    }
}