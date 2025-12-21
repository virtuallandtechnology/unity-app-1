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

    [System.Serializable]
    public class ApiResponse<T>
    {
        public bool isSuccess;
        public string message;
        public T result;
        public int code;
        public object errors;
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
        public double balance;
        public WalletType wallet_type;
    }
    public event Action<List<Wallet>> OnWalletsUpdated;

    private List<Wallet> _cachedWallets;
    public List<Wallet> CachedWallets => _cachedWallets;
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
            (req, resp) => HandleResponse<List<Wallet>>(req, resp, onSuccess, onFail));

        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddHeader("Accept", "application/json");
        request.Send();
    }
    [Serializable]
    public class PaymentStartRequest
    {
        public string amount;
        public string base_token;      // مثلا "USDT"
        public string available_tokens; // مثلا "BTC,USDT"
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


    public void StartEasyBitPayment(string amount, Action<ApiResponse<PaymentStartResult>> onSuccess, Action<string> onFail)
    {
        string token = PlayerPrefs.GetString("token");

        string url = GameConfig.Instance.BaseURL + "/user/payments/easybitpay/start";

        var data = new PaymentStartRequest
        {
            amount = amount,
            base_token = "USDT",
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
        Action<ApiResponse<T>> onSuccess, Action<string> onFail) where T : class
    {
        switch (request.State)
        {
            case HTTPRequestStates.Finished:
                Debug.Log(response.DataAsText);
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(response.DataAsText);

                    if (apiResponse == null)
                    {
                        Debug.LogError("❌ Could not parse server response.");
                        onFail?.Invoke("Invalid server response.");
                        return;
                    }

                    if (apiResponse.isSuccess)
                    {
                        Debug.Log("✅ Success: " + request.Uri + "  " + apiResponse.message);
                        onSuccess?.Invoke(apiResponse);
                    }
                    else
                    {
                        string detailedError = apiResponse.message;
                        Debug.LogWarning("⚠️ Server returned failure: " + detailedError);
                        onFail?.Invoke(detailedError);
                    }
                }
                break;

            case HTTPRequestStates.Error:
                Debug.LogError($"❌ Network error: {request.Exception?.Message}");
                onFail?.Invoke($"Network error: {request.Exception?.Message}");
                break;

            case HTTPRequestStates.Aborted:
                Debug.LogError($"❌ Request was aborted");
                onFail?.Invoke("Request was aborted");
                break;

            case HTTPRequestStates.ConnectionTimedOut:
            case HTTPRequestStates.TimedOut:
                Debug.LogError("❌ Request timed out");
                onFail?.Invoke("Request timed out");
                break;
        }
    }

    [Serializable]
    public class Passwordclass
    {
        public string email;
        public string name;
        public string password;
    }

    public void Register(string username, string password, string email,
        Action<ApiResponse<UserData>> onSuccess, Action<string> onFail)
    {
        string url = GameConfig.Instance.BaseURL + "/auth/register";
        Debug.Log("url=" + url);
        var t = new Passwordclass() { name = username, password = password, email = email };
        Debug.Log("data=" + JsonUtility.ToJson(t));

        var request = new HTTPRequest(new System.Uri(url), HTTPMethods.Post,
            (req, resp) => HandleResponse<UserData>(req, resp, onSuccess, onFail));
        request.AddHeader("Content-Type", "application/json");

        byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(t));
        request.UploadSettings.UploadStream = new System.IO.MemoryStream(body);

        request.Send();
    }

    public void ProfileInfo(string token,
      Action<ApiResponse<User>> onSuccess, Action<string> onFail)
    {
        string url = GameConfig.Instance.BaseURL + "/user/profile/info";
        Debug.Log("url=" + url);

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

    public void GetServerConfig(Action<ApiResponse<Result>> onSuccess, Action<string> onFail)
    {
        string url = "https://soccer.ecogamecenter.net/api/configs/indexed";
        Debug.Log("url=" + url);

        var request = new HTTPRequest(new System.Uri(url), HTTPMethods.Get,
            (req, resp) => HandleResponse<Result>(req, resp, onSuccess, onFail));
        request.AddHeader("Accept", "application/json");

        request.Send();
    }

    internal void UpdateProfile(Profile profile, Action<ApiResponse<User>> onsuccess, Action<string> onFail)
    {
        string token = PlayerPrefs.GetString("token");

        string url = GameConfig.Instance.BaseURL + "/user/profile/update";
        Debug.Log("url=" + url);

        string prof = JsonConvert.SerializeObject(profile);
        Debug.Log("data=" + prof);

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

        string url = GameConfig.Instance.BaseURL + "/user/profile/info";
        Debug.Log("[API] GetProfileInfo URL: " + url);

        var request = new HTTPRequest(new Uri(url), HTTPMethods.Get,
            (req, resp) => HandleResponse<User>(req, resp, (ApiResponse<User> response) =>
            {
                if (response.result != null)
                {
                    Setplayer(response.result);
                }

                onSuccess?.Invoke(response);
            }, onFail));

        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddHeader("Accept", "application/json");

        request.Send();
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
                    PlayerPrefs.Save();
                }

                onSuccess.Invoke(response);
            }, onFail));

        request.AddHeader("Content-Type", "application/json");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(t));
        request.UploadSettings.UploadStream = new System.IO.MemoryStream(body);
        request.Send();
    }

    [Serializable]
    public class loginclass
    {
        public string email;
        public string password;
    }

    [System.Serializable]
    public class UserData
    {
        public string token { get; set; }
        public string token_type { get; set; }
        public User user { get; set; }
    }

    public static int playerUserId = -1;
    private static User _player;
    public static User GetPlayer() => _player;
    private void Setplayer(User userdata)
    {
        Debug.Log("--player data Updated");
        _player = userdata;
    }

    [System.Serializable]
    public class User
    {
        public int id { get; set; }
        public int role_id { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public bool email_verified { get; set; }
        public object email_verified_at { get; set; }
        public Profile profile { get; set; }
        public string created_at { get; set; }

        public string GetUsername()
        {
            if (profile != null && !string.IsNullOrEmpty(profile.nickname))
            {
                return profile.nickname;
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
    }

    [System.Serializable]
    public class Profile
    {
        public int avatar_id { get; set; }
        public string nickname { get; set; }
    }
}