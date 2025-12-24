using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Best.HTTP;
using System;
using System.IO;

public class CartItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private Image _productImage;
    [SerializeField] private Button _removeButton;

    private ApiClient.ShopProduct _product;
    private CartController _controller;

    private string CustomCacheFolder => Path.Combine(Application.persistentDataPath, "ShopImages");

    private void Awake()
    {
        if (!Directory.Exists(CustomCacheFolder))
        {
            Directory.CreateDirectory(CustomCacheFolder);
        }
    }

    public void Setup(ApiClient.ShopProduct product, CartController controller)
    {
        _product = product;
        _controller = controller;

        if (_titleText) _titleText.text = product.title;

        if (product.price != null && product.price.Count > 0)
        {
            var p = product.price[0];
            if (_priceText) _priceText.text = $"{p.price:N0} {p.payable}";
        }

        if (_removeButton)
        {
            _removeButton.onClick.RemoveAllListeners();
            _removeButton.onClick.AddListener(() =>
            {
                _controller.RemoveFromCart(_product);
            });
        }

        if (!string.IsNullOrEmpty(product.image))
        {
            LoadOrDownloadImage(product.image);
        }
    }

    private void LoadOrDownloadImage(string url)
    {
        string fileName = url.GetHashCode().ToString("X") + ".png";
        string filePath = Path.Combine(CustomCacheFolder, fileName);

        if (File.Exists(filePath))
        {
            LoadImageFromDisk(filePath);
        }
        else
        {
            DownloadAndSaveImage(url, filePath);
        }
    }

    private void DownloadAndSaveImage(string url, string savePath)
    {
        var request = new HTTPRequest(new Uri(url), HTTPMethods.Get, (req, res) =>
        {
            if (this == null) return;

            if (res != null && res.IsSuccess)
            {
                Texture2D texture = res.DataAsTexture2D;

                if (texture == null)
                {
                    texture = new Texture2D(2, 2);
                    texture.LoadImage(res.Data);
                }

                if (texture != null)
                {
                    ApplyTexture(texture);

                    try
                    {
                        File.WriteAllBytes(savePath, res.Data);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.Message);
                    }
                }
            }
        });

       // request.DisableCache = true;
        request.Send();
    }

    private void LoadImageFromDisk(string path)
    {
        try
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);

            if (texture.LoadImage(fileData))
            {
                ApplyTexture(texture);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private void ApplyTexture(Texture2D texture)
    {
        if (_productImage == null || texture == null) return;

        Sprite sprite = Sprite.Create(texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f));

        _productImage.sprite = sprite;
        _productImage.color = Color.white;
        _productImage.preserveAspect = true;
    }
}