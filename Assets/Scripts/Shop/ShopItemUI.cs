using UnityEngine;
using UnityEngine.UI;
using Best.HTTP;
using System;
using TMPro;
using System.IO;
using Game.Shop.Visuals;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Image _productImage;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private Toggle _actionToggle;
    [SerializeField] private TextMeshProUGUI _toggleText;
    [SerializeField] private GameObject _ownedIndicator;
    [SerializeField] private Button _view3DButton;

    private ApiClient.ShopProduct _data;
    private bool _isInventoryMode;
    private string _categorySlug;

    private string CustomCacheFolder => Path.Combine(Application.persistentDataPath, "ShopImages");

    private void Awake()
    {
        if (!Directory.Exists(CustomCacheFolder))
        {
            Directory.CreateDirectory(CustomCacheFolder);
        }
    }

    public void Setup(ApiClient.ShopProduct product, bool isInventory, string categorySlug)
    {
        _data = product;
        _isInventoryMode = isInventory;
        _categorySlug = categorySlug;

        if (_titleText) _titleText.text = product.title;

        _actionToggle.onValueChanged.RemoveAllListeners();
        _actionToggle.interactable = true;

        if (_view3DButton)
        {
            _view3DButton.onClick.RemoveAllListeners();
            _view3DButton.onClick.AddListener(() =>
            {
                if (Shop3DViewController.Instance != null)
                {
                    Shop3DViewController.Instance.ShowPreview(product.id, _categorySlug);
                }
            });
        }

        if (_ownedIndicator) _ownedIndicator.SetActive(false);

        if (_isInventoryMode)
        {
            if (_priceText) _priceText.text = "Owned";
            if (_toggleText) _toggleText.text = "Select";

            _actionToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn) OnSelectClicked();
            });
        }
        else
        {
            if (product.is_purchased)
            {
                if (_ownedIndicator) _ownedIndicator.SetActive(true);
                if (_priceText) _priceText.text = "Owned";
                if (_toggleText) _toggleText.text = "Owned";
                _actionToggle.interactable = false;
            }
            else
            {
                if (product.price != null && product.price.Count > 0)
                {
                    if (_priceText) _priceText.text = $"{product.price[0].price:N0} USDT";
                }
                else
                {
                    if (_priceText) _priceText.text = "Free";
                }

                if (_toggleText) _toggleText.text = "Buy";

                _actionToggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn) OnBuyClicked();
                });
            }
        }

        if (!string.IsNullOrEmpty(product.image))
        {
            LoadOrDownloadImage(product.image);
        }
    }

    // ... (بقیه متدها مثل LoadOrDownloadImage بدون تغییر باقی می‌مانند) ...
    // برای رعایت اختصار متدهای قبلی تکرار نشدند اما باید در کلاس باشند

    private void LoadOrDownloadImage(string url)
    {
        string fileName = url.GetHashCode().ToString("X") + ".png";
        string filePath = Path.Combine(CustomCacheFolder, fileName);
        if (File.Exists(filePath)) LoadImageFromDisk(filePath);
        else DownloadAndSaveImage(url, filePath);
    }

    private void DownloadAndSaveImage(string url, string savePath)
    {
        var request = new HTTPRequest(new Uri(url), HTTPMethods.Get, (req, res) =>
        {
            if (res != null && res.IsSuccess)
            {
                Texture2D texture = res.DataAsTexture2D;
                if (texture != null)
                {
                    ApplyTexture(texture);
                    try { File.WriteAllBytes(savePath, res.Data); } catch { }
                }
            }
        });
        request.Send();
    }

    private void LoadImageFromDisk(string path)
    {
        try
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData)) ApplyTexture(texture);
        }
        catch { }
    }

    private void ApplyTexture(Texture2D texture)
    {
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        if (_productImage) { _productImage.sprite = sprite; _productImage.preserveAspect = true; }
    }

    private void OnBuyClicked()
    {
        if (CartController.Instance != null) CartController.Instance.AddToCart(_data);
    }

    private void OnSelectClicked()
    {
        Debug.Log($"Selected Product ID: {_data.id}");
        NotificationController.Get().Show("Item Selected");
    }
}
