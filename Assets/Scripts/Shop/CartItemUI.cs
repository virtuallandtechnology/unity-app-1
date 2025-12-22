using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CartItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private Image _productImage; 
    [SerializeField] private Button _removeButton;

    private ApiClient.ShopProduct _product;
    private CartController _controller;

    public void Setup(ApiClient.ShopProduct product, CartController controller)
    {
        _product = product;
        _controller = controller;

        _titleText.text = product.title;

        if (product.price != null && product.price.Count > 0)
        {
            var p = product.price[0];
            _priceText.text = $"{p.price:N0} {p.payable}";
        }

        if (_productImage != null && !string.IsNullOrEmpty(product.image))
        {
            if (ImageLoader.Instance != null)
            {
                ImageLoader.Instance.LoadImage(product.image, _productImage);
            }
        }

        _removeButton.onClick.RemoveAllListeners();
        _removeButton.onClick.AddListener(() => {
            _controller.RemoveFromCart(_product);
        });
    }
}