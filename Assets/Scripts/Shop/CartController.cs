using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class CartController : MonoBehaviour
{
    public static CartController Instance;

    [Header("UI References")]
    [SerializeField] private Transform _cartItemsContainer;
    [SerializeField] private CartItemUI _cartItemPrefab;
    [SerializeField] private TextMeshProUGUI _totalPriceText;
    [SerializeField] private TextMeshProUGUI _warningText;
    [SerializeField] private Button _payButton;
    [SerializeField] private GameObject _cartPanel;

    private List<ApiClient.ShopProduct> _cartItems = new List<ApiClient.ShopProduct>();
    private Dictionary<string, double> _userBalances = new Dictionary<string, double>();

    private void Awake()
    {
        Instance = this;
        if (_payButton != null) _payButton.onClick.AddListener(ProcessCheckout);
    }

    private void Start()
    {
        // درخواست آپدیت ولت‌ها در شروع بازی
        ApiClient.Get().RequestWalletsUpdate();
    }

    private void OnEnable()
    {
        ApiClient.Get().OnWalletsUpdated += OnWalletsUpdatedFromServer;
    }

    private void OnDisable()
    {
        ApiClient.Get().OnWalletsUpdated -= OnWalletsUpdatedFromServer;
    }

    private void OnWalletsUpdatedFromServer(List<ApiClient.Wallet> wallets)
    {
        FetchUserBalances();
        if (_cartPanel.activeSelf)
        {
            UpdateCartUI();
        }
    }

    public void AddToCart(ApiClient.ShopProduct product)
    {
        _cartItems.Add(product);
        UpdateCartUI();

        if (!_cartPanel.activeSelf) ShowCart();
    }

    public void RemoveFromCart(ApiClient.ShopProduct product)
    {
        if (_cartItems.Contains(product))
        {
            _cartItems.Remove(product);
            UpdateCartUI();
        }
    }

    public void ShowCart()
    {
        _cartPanel.SetActive(true);
        FetchUserBalances();
        UpdateCartUI();
    }

    private void FetchUserBalances()
    {
        var wallets = ApiClient.Get().CachedWallets;
        _userBalances.Clear();

        if (wallets != null)
        {
            foreach (var w in wallets)
            {
                if (!string.IsNullOrEmpty(w.slug))
                {
                    // برای مقایسه راحت‌تر در UI همه را کوچک می‌کنیم
                    string key = w.slug.ToLower().Trim();

                    if (_userBalances.ContainsKey(key))
                        _userBalances[key] += w.balance;
                    else
                        _userBalances[key] = w.balance;
                }
            }
        }
        else
        {
            ApiClient.Get().RequestWalletsUpdate();
        }
    }

    private void UpdateCartUI()
    {
        foreach (Transform child in _cartItemsContainer) Destroy(child.gameObject);

        foreach (var item in _cartItems)
        {
            CartItemUI uiItem = Instantiate(_cartItemPrefab, _cartItemsContainer);
            uiItem.Setup(item, this);
        }

        ValidateAndCalculateTotal();
    }

    private void ValidateAndCalculateTotal()
    {
        Dictionary<string, double> tempBalances = new Dictionary<string, double>(_userBalances);
        Dictionary<string, double> finalBill = new Dictionary<string, double>();

        bool canAffordAll = true;
        string warningMsg = "";

        foreach (var item in _cartItems)
        {
            if (item.price == null || item.price.Count == 0) continue;

            double costUSDT = 0;
            double costGEM = 0;
            bool hasUSDTPrice = false;
            bool hasGEMPrice = false;

            foreach (var p in item.price)
            {
                string type = p.payable.ToLower().Trim();
                if (type == "usdt")
                {
                    costUSDT = p.price;
                    hasUSDTPrice = true;
                }
                else if (type == "gem")
                {
                    costGEM = p.price;
                    hasGEMPrice = true;
                }
            }

            double userUSDT = tempBalances.ContainsKey("usdt") ? tempBalances["usdt"] : 0;
            double userGEM = tempBalances.ContainsKey("gem") ? tempBalances["gem"] : 0;

            if (hasUSDTPrice && userUSDT >= costUSDT)
            {
                if (!finalBill.ContainsKey("usdt")) finalBill["usdt"] = 0;
                finalBill["usdt"] += costUSDT;
                tempBalances["usdt"] -= costUSDT;
            }
            else if (hasGEMPrice && userGEM >= costGEM)
            {
                if (!finalBill.ContainsKey("gem")) finalBill["gem"] = 0;
                finalBill["gem"] += costGEM;
                tempBalances["gem"] -= costGEM;
            }
            else
            {
                canAffordAll = false;

                if (hasUSDTPrice)
                {
                    if (!finalBill.ContainsKey("usdt")) finalBill["usdt"] = 0;
                    finalBill["usdt"] += costUSDT;
                    warningMsg = $"Low USDT";
                }
                else if (hasGEMPrice)
                {
                    if (!finalBill.ContainsKey("gem")) finalBill["gem"] = 0;
                    finalBill["gem"] += costGEM;
                    warningMsg = $"Low GEM";
                }
            }
        }

        string totalStr = "Total: ";
        if (finalBill.Count > 0)
        {
            foreach (var kvp in finalBill)
            {
                totalStr += $"{kvp.Value:N0} {kvp.Key.ToUpper()}  ";
            }
        }
        else
        {
            totalStr += "0";
        }

        if (_totalPriceText) _totalPriceText.text = totalStr;

        if (_cartItems.Count == 0)
        {
            if (_payButton) _payButton.interactable = false;
            if (_warningText)
            {
                _warningText.text = "Empty";
                _warningText.gameObject.SetActive(true);
            }
        }
        else if (canAffordAll)
        {
            if (_payButton) _payButton.interactable = true;
            if (_warningText) _warningText.gameObject.SetActive(false);
        }
        else
        {
            if (_payButton) _payButton.interactable = false;
            if (_warningText)
            {
                _warningText.text = warningMsg;
                _warningText.gameObject.SetActive(true);
            }
        }
    }

    private void ProcessCheckout()
    {
        if (_payButton) _payButton.interactable = false;
        StartCoroutine(CheckoutRoutine());
    }

    private IEnumerator CheckoutRoutine()
    {
        // کپی موجودی برای محاسبه دقیق
        Dictionary<string, double> tempBalances = new Dictionary<string, double>(_userBalances);
        // کپی لیست خرید برای جلوگیری از خطای تغییر لیست
        List<ApiClient.ShopProduct> itemsToBuy = new List<ApiClient.ShopProduct>(_cartItems);

        int successCount = 0;

        foreach (var item in itemsToBuy)
        {
            bool isDone = false;
            bool isSuccess = false;

            // 1. استخراج قیمت‌ها
            double costUSDT = 0;
            double costGEM = 0;
            bool hasUSDTPrice = false;
            bool hasGEMPrice = false;

            // گرفتن اسم دقیق Slug از جیسون برای اطمینان
            string rawUsdtSlug = "USDT";
            string rawGemSlug = "GEM";

            foreach (var p in item.price)
            {
                string type = p.payable.ToLower().Trim();
                if (type == "usdt")
                {
                    costUSDT = p.price;
                    hasUSDTPrice = true;
                    rawUsdtSlug = p.payable; // ذخیره اسم اصلی
                }
                else if (type == "gem")
                {
                    costGEM = p.price;
                    hasGEMPrice = true;
                    rawGemSlug = p.payable;
                }
            }

            // 2. انتخاب ارز برای پرداخت
            double userUSDT = tempBalances.ContainsKey("usdt") ? tempBalances["usdt"] : 0;
            double userGEM = tempBalances.ContainsKey("gem") ? tempBalances["gem"] : 0;

            string selectedSlug = "";

            if (hasUSDTPrice && userUSDT >= costUSDT)
            {
                selectedSlug = "USDT";
                tempBalances["usdt"] -= costUSDT;
            }
            else if (hasGEMPrice && userGEM >= costGEM)
            {
                selectedSlug = "GEM";
                tempBalances["gem"] -= costGEM;
            }
            else
            {
                
                isDone = true;
            }

            if (!string.IsNullOrEmpty(selectedSlug))
            {
                Debug.Log($"Buying {item.title} using {selectedSlug}");
                ApiClient.Get().BuyProduct(item.id, selectedSlug,
                    (res) => {
                        isSuccess = true;
                        isDone = true;
                        // Show Title + "Purchase Successful" for 2 seconds
                        NotificationController.Get().Show(item.title, "Purchase Successful", 2f);
                    },
                    (err) => {
                        isSuccess = false;
                        isDone = true;
                        Debug.LogError($"Buy Failed for {item.title}: " + err);
                    }
                );
            }

            yield return new WaitUntil(() => isDone);

            if (isSuccess)
            {
                successCount++;
                _cartItems.Remove(item);
            }
        }

        // آپدیت نهایی
        ApiClient.Get().RequestWalletsUpdate();

        if (successCount > 0)
            Debug.Log($"Success: {successCount}");
        else
            Debug.Log("Failed to buy items.");

        UpdateCartUI();
    }
}