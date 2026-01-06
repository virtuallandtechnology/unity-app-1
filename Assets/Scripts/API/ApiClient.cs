using Best.HTTP;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public partial class ApiClient
{
    private static ApiClient instance;
    private GameConfig _config;
    private UserData _userdata;

    public static ApiClient Get()
    {
        if (instance == null)
        {
            instance = new ApiClient();
        }
        return instance;
    }

    public UserData GetUserData()
    {
        return _userdata;
    }

    [Serializable]
    public class ApiResponse<T>
    {
        public bool isSuccess;
        public string message;
        public T result;
        public int code;
        public object errors;
        public double timestamp;
    }

    [Serializable]
    public class ShopResponse
    {
        public ShopResult result;
    }

    [Serializable]
    public class ShopResult
    {
        public int current_page;
        public int last_page;
        public List<ShopProduct> data;
    }

    [Serializable]
    public class ShopProduct
    {
        public int id;
        public string title;
        public string description;
        public string image;
        public List<ProductPrice> price;
        public int in_stock;
        public bool is_purchased;
    }

    [Serializable]
    public class ProductPrice
    {
        public double price;
        public string payable;
    }

    [Serializable]
    public class BuyProductRequest
    {
        public int product_id;
        public string payable_slug;
        public Dictionary<string, string> metadata;
    }

    [Serializable]
    public class UpdateProfileRequest
    {
        public string avatar_id;
        public string style;
    }

    [Serializable]
    public class InventoryItemWrapper
    {
        public ShopProduct product;
        public string created_at;
    }

    public void GetShopProducts(int page, Action<ApiResponse<ShopResult>> onSuccess, Action<string> onFail)
    {
        string token = PlayerPrefs.GetString("token");
        string url = $"{GameConfig.Instance.BaseURL}/user/products?page={page}";

        var request = new HTTPRequest(new Uri(url), HTTPMethods.Get,
            (req, resp) => HandleResponse<ShopResult>(req, resp, onSuccess, onFail));

        request.AddHeader("Authorization", $"Bearer {token}");
        request.Send();
    }

    public void GetProductsByCategory(string categorySlug, Action<ApiResponse<ShopResult>> onSuccess, Action<string> onFail)
    {
        string token = PlayerPrefs.GetString("token");
        string url = $"{GameConfig.Instance.BaseURL}/user/products/{categorySlug}";

        var request = new HTTPRequest(new Uri(url), HTTPMethods.Get,
            (req, resp) => HandleResponse<ShopResult>(req, resp, onSuccess, onFail));

        request.AddHeader("Authorization", $"Bearer {token}");
        request.Send();
    }

    public void GetPurchasedProducts(string categorySlug, Action<ApiResponse<List<InventoryItemWrapper>>> onSuccess, Action<string> onFail)
    {
        string token = PlayerPrefs.GetString("token");
        string url = $"{GameConfig.Instance.BaseURL}/user/profile/products/{categorySlug}";

        var request = new HTTPRequest(new Uri(url), HTTPMethods.Get,
            (req, resp) => HandleResponse<List<InventoryItemWrapper>>(req, resp, onSuccess, onFail));

        request.AddHeader("Authorization", $"Bearer {token}");
        request.Send();
    }

    public void GetProductById(int productId, Action<ApiResponse<ShopProduct>> onSuccess, Action<string> onFail)
    {
        if (productId <= 0)
        {
            // If product ID is invalid, return a default successful response
            var apiResponse = new ApiResponse<ShopProduct> { isSuccess = true, result = null };
            onSuccess?.Invoke(apiResponse);
            return;
        }

        string token = PlayerPrefs.GetString("token");
        string url = $"{GameConfig.Instance.BaseURL}/user/products/details/{productId}";

        var request = new HTTPRequest(new Uri(url), HTTPMethods.Get,
           (req, resp) => HandleResponse<ShopProduct>(req, resp, onSuccess, onFail));


        request.AddHeader("Authorization", $"Bearer {token}");
        request.Send();
    }

    public void GetProfileById(int id, Action<ApiResponse<ProfileDataResult>> onSuccess, Action<string> onFail)
    {
        string token = PlayerPrefs.GetString("token");
        string url = $"{GameConfig.Instance.BaseURL}/user/profile/get/{id}";

        var request = new HTTPRequest(new Uri(url), HTTPMethods.Get,
            (req, resp) => HandleResponse<ProfileDataResult>(req, resp, onSuccess, onFail));

        request.AddHeader("Authorization", $"Bearer {token}");
        request.Send();
    }

    public void BuyProduct(int productId, string payableSlug, Action<ApiResponse<object>> onSuccess, Action<string> onFail)
    {
        string token = PlayerPrefs.GetString("token");
        string url = $"{GameConfig.Instance.BaseURL}/user/products/buy";

        var data = new BuyProductRequest
        {
            product_id = productId,
            payable_slug = payableSlug
        };

        var request = new HTTPRequest(new Uri(url), HTTPMethods.Post,
            (req, resp) => HandleResponse<object>(req, resp, onSuccess, onFail));

        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddHeader("Content-Type", "application/json");

        byte[] body = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
        request.UploadSettings.UploadStream = new System.IO.MemoryStream(body);

        request.Send();
    }

    [Serializable]
    public class WalletType
    {
        public int id;
        public string name;
        public string slug;
        public string image;
        public string description;
    }

    [Serializable]
    public class Wallet
    {
        public int id;
        public string name;
        public string slug;
        public string image;
        public string description;
        public double balance;
    }

    public event Action<List<Wallet>> OnWalletsUpdated;

    private List<Wallet> _cachedWallets;
    public List<Wallet> CachedWallets => _cachedWallets;

    public class ProfileWalletDto
    {
        public double balance;
        public WalletTypeDto wallet_type;
    }

    public class WalletTypeDto
    {
        public int id;
        public string name;
        public string slug;
        public string image;
        public string description;
    }

    public void GetWallets(Action<ApiResponse<List<Wallet>>> onSuccess, Action<string> onFail)
    {
        string token = PlayerPrefs.GetString("token");
        if (string.IsNullOrEmpty(token))
        {
            onFail?.Invoke("Token is missing");
            return;
        }

        string url = GameConfig.Instance.BaseURL + "/user/profile/wallets";

        var request = new HTTPRequest(new Uri(url), HTTPMethods.Get,
            (req, resp) =>
            {
                // Handle nested response structure
                HandleResponse<List<ProfileWalletDto>>(req, resp,
                (dtoResponse) =>
                {
                    // Convert DTOs to flat Wallet objects
                    var wallets = new List<Wallet>();
                    if (dtoResponse.result != null)
                    {
                        foreach (var dto in dtoResponse.result)
                        {
                            if (dto.wallet_type != null)
                            {
                                wallets.Add(new Wallet
                                {
                                    id = dto.wallet_type.id,
                                    name = dto.wallet_type.name,
                                    slug = dto.wallet_type.slug,
                                    image = dto.wallet_type.image,
                                    description = dto.wallet_type.description,
                                    balance = dto.balance
                                });
                            }
                        }
                    }

                    // Create new response with flattened data
                    var finalResponse = new ApiResponse<List<Wallet>>
                    {
                        isSuccess = dtoResponse.isSuccess,
                        message = dtoResponse.message,
                        code = dtoResponse.code,
                        result = wallets,
                        timestamp = dtoResponse.timestamp
                    };

                    onSuccess?.Invoke(finalResponse);

                    // Update cache
                    _cachedWallets = wallets;
                    OnWalletsUpdated?.Invoke(_cachedWallets);

                }, onFail);
            });

        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddHeader("Accept", "application/json");
        request.Send();
    }

    [Serializable]
    public class PaymentStartRequest
    {
        public string amount;
        public string base_token;
        public string available_tokens;
    }

    [Serializable]
    public class PaymentStartResult
    {
        public long payment_id;
        public string order_id;
        public string code;
        public string redirect;
    }

    [Serializable]
    public class PaymentVerificationRequest
    {
        public int user_id;
        public string msisdn;
        public double amount;
        public string description;
        public string national_code;
    }

    public void StartEasyBitPayment(string amount, string baseToken, Action<ApiResponse<PaymentStartResult>> onSuccess, Action<string> onFail)
    {
        string token = PlayerPrefs.GetString("token");
        string url = GameConfig.Instance.BaseURL + "/user/payments/easybitpay/start";

        var data = new PaymentStartRequest
        {
            amount = amount,
            base_token = baseToken,
            available_tokens = "BTC,USDT"
        };

        var request = new HTTPRequest(new Uri(url), HTTPMethods.Post,
            (req, resp) => HandleResponse<PaymentStartResult>(req, resp, onSuccess, onFail));

        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddHeader("Content-Type", "application/json");

        byte[] body = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
        request.UploadSettings.UploadStream = new System.IO.MemoryStream(body);

        request.Send();
    }

    public void VerifyEasyBitPayment(PaymentVerificationRequest data, Action<ApiResponse<object>> onSuccess, Action<string> onFail)
    {
        string token = PlayerPrefs.GetString("token");
        string url = GameConfig.Instance.BaseURL + "/payments/easybitpay/callback";

        var request = new HTTPRequest(new Uri(url), HTTPMethods.Post,
            (req, resp) => HandleResponse<object>(req, resp, onSuccess, onFail));

        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddHeader("Content-Type", "application/json");

        byte[] body = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
        request.UploadSettings.UploadStream = new System.IO.MemoryStream(body);

        request.Send();
    }

    public void RequestWalletsUpdate()
    {
        GetWallets(
            (response) =>
            {
                if (response.isSuccess && response.result != null)
                {
                    _cachedWallets = response.result;
                    OnWalletsUpdated?.Invoke(response.result);
                }
            },
            (error) =>
            {
                Debug.LogError("Wallet Update Failed: " + error);
            }
        );
    }

    private void HandleResponse<T>(HTTPRequest request, HTTPResponse response,
        Action<ApiResponse<T>> onSuccess, Action<string> onFail)
    {
        if (request.State == HTTPRequestStates.Finished && response != null && response.IsSuccess)
        {
            try
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(response.DataAsText);
                if (apiResponse.isSuccess)
                    onSuccess?.Invoke(apiResponse);
                else
                    onFail?.Invoke(apiResponse.message);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ApiClient] Parse Error for {request.Uri}: {ex.Message}\nData: {response.DataAsText}");
                onFail?.Invoke("Parse Error: " + ex.Message);
            }
        }
        else
        {
            string error = "Request Failed";
            if (response != null)
            {
                error = $"Server Error: {response.StatusCode} {response.Message}";
                if (response.StatusCode == 401)
                {
                    Debug.LogWarning($"[ApiClient] Unauthorized (401) for {request.Uri}. Token might be expired.");
                    onFail?.Invoke(error);
                    return;
                }
                else if (response.StatusCode == 404)
                {
                    // 404 is often valid (e.g. profile not set yet). Log as warning.
                    Debug.LogWarning($"[ApiClient] Not Found (404) for {request.Uri}");
                    onFail?.Invoke(error);
                    return;
                }
            }
            else if (request.State != HTTPRequestStates.Finished)
                error = $"Network Error: {request.State}";

            Debug.LogError($"[ApiClient] {error} for {request.Uri}");
            onFail?.Invoke(error);
        }
    }

    [Serializable]
    public class RegisterRequest
    {
        public string username;
        public string email;
        public string password;
        public RegisterProfile profile;
    }

    [Serializable]
    public class RegisterProfile
    {
        public string name;
        public string avatar;
    }

    public void Register(string username, string password, string email,
        Action<ApiResponse<UserData>> onSuccess, Action<string> onFail)
    {
        string url = GameConfig.Instance.BaseURL + "/auth/register";

        var payload = new RegisterRequest
        {
            username = username,
            email = email,
            password = password,
            profile = new RegisterProfile
            {
                name = username, // Using username as name for now
                avatar = "" // Placeholder or default
            }
        };

        var request = new HTTPRequest(new System.Uri(url), HTTPMethods.Post,
            (req, resp) => HandleResponse<UserData>(req, resp, onSuccess, onFail));
        request.AddHeader("Content-Type", "application/json");

        byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload));
        request.UploadSettings.UploadStream = new System.IO.MemoryStream(body);

        request.Send();
    }

    public void ProfileInfo(string token,
      Action<ApiResponse<User>> onSuccess, Action<string> onFail)
    {
        string url = GameConfig.Instance.BaseURL.TrimEnd('/') + "/user/profile/get";

        var request = new HTTPRequest(new System.Uri(url), HTTPMethods.Get,
            (req, resp) => HandleResponse<User>(req, resp, (ApiResponse<User> t) =>
            {
                Setplayer(t.result);
                onSuccess.Invoke(t);
            }, onFail));
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddHeader("Accept", "application/json");

        request.Send();
    }

    public class Latest
    {
        public string version { get; set; }
        public string download { get; set; }
        public string redirect { get; set; }
        public string description { get; set; }
    }

    public class Result
    {
        public VERSIONINFO VERSION_INFO { get; set; }
    }

    public class VERSIONINFO
    {
        public Latest latest { get; set; }
        public string force_update { get; set; }
    }

    [Serializable]
    public class ConfigItem
    {
        public string key;
        public VERSIONINFO value;
        public string type;
    }

    public void GetServerConfig(Action<ApiResponse<Result>> onSuccess, Action<string> onFail)
    {
        string url = $"{GameConfig.Instance.BaseURL.TrimEnd('/')}/configs";

        var request = new HTTPRequest(new System.Uri(url), HTTPMethods.Get,
            (req, resp) =>
            {
                if (req.State != HTTPRequestStates.Finished || resp == null || !resp.IsSuccess)
                {
                    HandleResponse<Result>(req, resp, onSuccess, onFail); // Let the standard handler report network/server errors
                    return;
                }

                try
                {
                    // The new response has a 'result' that is an array.
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConfigItem>>>(resp.DataAsText);
                    if (apiResponse.isSuccess && apiResponse.result != null)
                    {
                        // Find the config item with the key "VERSION_INFO"
                        ConfigItem versionInfoItem = apiResponse.result.Find(item => item.key == "VERSION_INFO");

                        if (versionInfoItem != null && versionInfoItem.value != null)
                        {
                            // Construct the old `Result` object that the rest of the app expects.
                            var finalResult = new Result { VERSION_INFO = versionInfoItem.value };

                            // Create a new ApiResponse to pass to the original onSuccess callback.
                            var finalResponse = new ApiResponse<Result>
                            {
                                isSuccess = true,
                                message = apiResponse.message,
                                result = finalResult,
                                code = apiResponse.code,
                                timestamp = apiResponse.timestamp
                            };
                            onSuccess?.Invoke(finalResponse);
                        }
                        else
                        {
                            onFail?.Invoke("VERSION_INFO key not found in /configs response.");
                        }
                    }
                    else
                    {
                        onFail?.Invoke(apiResponse.message ?? "Failed to get server config from /configs.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ApiClient] Parse Error for {req.Uri}: {ex.Message}\nData: {resp.DataAsText}");
                    onFail?.Invoke("Parse Error: " + ex.Message);
                }
            });
        request.AddHeader("Accept", "application/json");

        request.Send();
    }

    internal void UpdateProfile(Profile profile, Action<ApiResponse<User>> onsuccess, Action<string> onFail)
    {
        string token = PlayerPrefs.GetString("token");
        string url = GameConfig.Instance.BaseURL + "/user/profile/update";
        string prof = JsonConvert.SerializeObject(profile);

        var request = new HTTPRequest(new System.Uri(url), HTTPMethods.Post,
            (req, resp) => HandleResponse<User>(req, resp, (ApiResponse<User> t) =>
            {
                Setplayer(t.result);
                onsuccess.Invoke(t);
            }, onFail));

        byte[] body = Encoding.UTF8.GetBytes(prof);
        request.UploadSettings.UploadStream = new System.IO.MemoryStream(body);

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddHeader("Accept", "application/json");

        request.Send();
    }

    public void GetProfileInfo(Action<ApiResponse<User>> onSuccess, Action<string> onFail)
    {
        string token = PlayerPrefs.GetString("token");
        if (string.IsNullOrEmpty(token))
        {
            onFail?.Invoke("Token not found. Please login.");
            return;
        }

        // Use the dynamic endpoint to get profile data
        string key = PlayerPrefs.GetString("CurrentProfileKey", "var1");
        GetProfileData(key,
            (response) =>
            {
                if (response.isSuccess && response.result != null)
                {
                    // Construct a User object with the profile data
                    // Construct a User object with the profile data
                    var user = new User();
                    var tempProfile = new Profile();

                    if (response.result.style != null)
                    {
                        if (response.result.style is string strStyle)
                            tempProfile.style = strStyle;
                        else
                            tempProfile.style = Newtonsoft.Json.JsonConvert.SerializeObject(response.result.style);
                    }

                    tempProfile.avatar_id = response.result.avatar_id;
                    user.profile = tempProfile;

                    // We don't have username/email from this endpoint, but BootStrap might need a non-null User
                    // Set basic info if stored
                    user.username = PlayerPrefs.GetString("username", "User");

                    var userResponse = new ApiResponse<User>();
                    userResponse.isSuccess = true;
                    userResponse.code = 200;
                    userResponse.message = "Profile loaded from var1";
                    userResponse.result = user;

                    Setplayer(user);
                    onSuccess?.Invoke(userResponse);
                }
                else
                {
                    onFail?.Invoke(response.message);
                }
            },
            onFail);
    }
}

public partial class ApiClient
{
    public void Login(string username, string password, Action<ApiResponse<UserData>> onSuccess, Action<string> onFail)
    {
        string url = GameConfig.Instance.BaseURL + "/auth/login";
        var t = new loginclass() { email = username, password = password };

        var request = new HTTPRequest(new Uri(url), HTTPMethods.Post,
            (req, resp) => HandleResponse(req, resp, (ApiResponse<UserData> response) =>
            {
                if (response.result != null && response.result.user != null)
                {
                    Setplayer(response.result.user);

                    PlayerPrefs.SetString("token", response.result.token);
                    PlayerPrefs.SetString("username", response.result.user.GetUsername());

                    // Sync character from profile if available
                    // Sync character from profile if available
                    // Handle dynamic profile object (might be JObject or empty JArray)
                    if (response.result.user.profile != null && !(response.result.user.profile is Array))
                    {
                        try
                        {
                            string json = Newtonsoft.Json.JsonConvert.SerializeObject(response.result.user.profile);
                            if (json != "[]")
                            {
                                var profileObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Profile>(json);
                                if (profileObj != null && !string.IsNullOrEmpty(profileObj.avatar_id))
                                {
                                    if (int.TryParse(profileObj.avatar_id, out int avatarId))
                                    {
                                        if (VirtualLand.MainCharacterManager.Instance != null)
                                        {
                                            VirtualLand.MainCharacterManager.Instance.SyncFromProfile(avatarId, profileObj.style);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[ApiClient] Failed to parse profile for sync: {ex.Message}");
                        }
                    }

                    // Cache wallets if present in login response
                    if (response.result.wallets != null)
                    {
                        _cachedWallets = response.result.wallets;
                        OnWalletsUpdated?.Invoke(_cachedWallets);
                    }

                    PlayerPrefs.Save();
                }

                onSuccess.Invoke(response);
            }, onFail));

        request.AddHeader("Content-Type", "application/json");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(t));
        request.UploadSettings.UploadStream = new System.IO.MemoryStream(body);
        request.Send();
    }

    public void RefreshToken(Action<ApiResponse<UserData>> onSuccess, Action<string> onFail)
    {
        string url = GameConfig.Instance.BaseURL + "/auth/refresh";
        string token = PlayerPrefs.GetString("token");

        if (string.IsNullOrEmpty(token))
        {
            onFail?.Invoke("No token found locally.");
            return;
        }

        var request = new HTTPRequest(new Uri(url), HTTPMethods.Post,
            (req, resp) => HandleResponse<UserData>(req, resp, (response) =>
            {
                if (response.isSuccess && response.result != null)
                {
                    PlayerPrefs.SetString("token", response.result.token);

                    if (response.result.user != null)
                    {
                        Setplayer(response.result.user);
                        PlayerPrefs.SetString("username", response.result.user.GetUsername());

                        // Sync character from profile if available
                        // Sync character from profile if available
                        var profile = response.result.user.GetProfile();
                        if (profile != null && !string.IsNullOrEmpty(profile.avatar_id))
                        {
                            if (int.TryParse(profile.avatar_id, out int avatarId))
                            {
                                // Use the style from the refresh response directly
                                if (VirtualLand.MainCharacterManager.Instance != null)
                                {
                                    VirtualLand.MainCharacterManager.Instance.SyncFromProfile(avatarId, profile.style);
                                }
                            }
                        }
                    }

                    PlayerPrefs.Save();
                }

                onSuccess?.Invoke(response);
            }, onFail));

        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddHeader("Accept", "application/json");

        request.Send();
    }
    [Serializable]
    public class CategoryResponse
    {
        public CategoryResult result;
    }

    [Serializable]
    public class CategoryResult
    {
        public int current_page;
        public int last_page;
        public List<CategoryItem> data;
    }

    [Serializable]
    public class CategoryItem
    {
        public int id;
        public int? parent_id;
        public string name;
        public string slug;
        public string image;
        public List<CategoryItem> categories; // Sub-categories
    }

    public void GetAllCategories(Action<ApiResponse<CategoryResult>> onSuccess, Action<string> onFail)
    {
        string token = PlayerPrefs.GetString("token");
       
        string url = $"{GameConfig.Instance.BaseURL}/user/categories?page=1";

        var request = new HTTPRequest(new Uri(url), HTTPMethods.Get,
            (req, resp) => HandleResponse<CategoryResult>(req, resp, onSuccess, onFail));

        request.AddHeader("Authorization", $"Bearer {token}");
        request.Send();
    }

    [Serializable]
    public class loginclass
    {
        public string email;
        public string password;
    }

    [Serializable]
    public class UserData
    {
        public string token { get; set; }
        public string token_type { get; set; }
        public User user { get; set; }
        public List<Wallet> wallets { get; set; }
    }

    public static int playerUserId = -1;
    private static User _player;
    public static User GetPlayer() => _player;
    private void Setplayer(User userdata)
    {
        _player = userdata;
    }

    [Serializable]
    public class User
    {
        public int id { get; set; }
        public int role_id { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public bool email_verified { get; set; }
        public object email_verified_at { get; set; }
        public object profile { get; set; }
        public string created_at { get; set; }

        public string GetUsername()
        {
            var p = GetProfile();
            if (p != null && !string.IsNullOrEmpty(p.nickname))
            {
                return p.nickname;
            }

            if (!string.IsNullOrEmpty(username))
            {
                return username;
            }

            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }

            if (!string.IsNullOrEmpty(email))
            {
                return email;
            }

            return "User_" + id;
        }

        public Profile GetProfile()
        {
            if (profile == null) return null;

            // Check if it's already the correct type (unlikely with Newtonsoft->object but possible if manually set)
            if (profile is Profile p) return p;

            try
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(profile);
                if (json != "[]" && json != "null")
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<Profile>(json);
                }
            }
            catch { }
            return null;
        }
    }

    [Serializable]
    public class ProfileDataResult
    {
        public object style;
        public string avatar_id;
        public double timestamp;
    }

    public void GetProfileVersion(string key, Action<ApiResponse<int>> onSuccess, Action<string> onFail)
    {
        string token = PlayerPrefs.GetString("token");
        string url = $"{GameConfig.Instance.BaseURL.TrimEnd('/')}/user/profile/version/{key}";

        var request = new HTTPRequest(new Uri(url), HTTPMethods.Get,
            (req, resp) => HandleResponse<int>(req, resp, onSuccess, onFail));

        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddHeader("Accept", "application/json");
        request.Send();
    }

    public void GetProfileData(string key, Action<ApiResponse<ProfileDataResult>> onSuccess, Action<string> onFail)
    {
        string token = PlayerPrefs.GetString("token").Trim();
        string url = $"{GameConfig.Instance.BaseURL.TrimEnd('/')}/user/profile/get/{key}";

        var request = new HTTPRequest(new Uri(url), HTTPMethods.Get,
            (req, resp) =>
            {
                // Fix: If data is not found (404), return empty result instead of error
                if (resp != null && resp.StatusCode == 404)
                {
                    var res = new ApiResponse<ProfileDataResult>();
                    res.isSuccess = true;
                    res.result = new ProfileDataResult();
                    onSuccess?.Invoke(res);
                    return;
                }
                HandleResponse<ProfileDataResult>(req, resp, onSuccess, onFail);
            });

        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddHeader("Accept", "application/json");
        request.Send();
    }

    public void UpdateProfileData(string key, string data, Action<ApiResponse<object>> onSuccess, Action<string> onFail)
    {
        string token = PlayerPrefs.GetString("token");
        string url = $"{GameConfig.Instance.BaseURL.TrimEnd('/')}/user/profile/update/{key}";

        var request = new HTTPRequest(new Uri(url), HTTPMethods.Post,
            (req, resp) => HandleResponse<object>(req, resp, onSuccess, onFail));

        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddHeader("Content-Type", "application/json");

        // Assuming the API expects the raw data in a specific format or as body
        // The user's update endpoint is user/profile/update/var1
        // Usually these generic endpoints just take the payload
        byte[] body = Encoding.UTF8.GetBytes(data);
        request.UploadSettings.UploadStream = new System.IO.MemoryStream(body);

        request.Send();
    }



    public void UpdateUserProfile(int avatarId, string styleJson, Action<ApiResponse<object>> onSuccess, Action<string> onFail)
    {
        string token = PlayerPrefs.GetString("token");
        // User requested to append product ID to the update URL: .../user/profile/update/41
        string url = $"{GameConfig.Instance.BaseURL}/user/profile/update/{avatarId}";

        var data = new UpdateProfileRequest
        {
            avatar_id = avatarId.ToString(),
            // style might be needed or not depending on API, keeping it safely as user only mentioned URL change
            style = styleJson 
        };

        var request = new HTTPRequest(new Uri(url), HTTPMethods.Post,
            (req, resp) => HandleResponse<object>(req, resp, onSuccess, onFail));

        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddHeader("Content-Type", "application/json");

        byte[] body = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
        request.UploadSettings.UploadStream = new System.IO.MemoryStream(body);

        request.Send();
    }

    [Serializable]
    public class Profile
    {
        public string avatar_id { get; set; }
        public string nickname { get; set; }
        public string style { get; set; }
    }
}