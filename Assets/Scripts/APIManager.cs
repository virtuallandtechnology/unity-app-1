using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Collections;
using VirtualLand.NewSystem.Models;

namespace VirtualLand.NewSystem.Network
{
    public class APIManager : MonoBehaviour
    {
        public static APIManager Instance;
        private const string BASE_URL = "https://game.virtualland.technology/api";

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        // --- Public Methods ---

        public void Register(RegisterBody body, Action<AuthResult> onSuccess, Action<string> onError)
        {
            string json = JsonUtility.ToJson(body);
            StartCoroutine(SendRequestCoroutine("/auth/register", "POST", json, null, onSuccess, onError));
        }

        public void Login(LoginBody body, Action<AuthResult> onSuccess, Action<string> onError)
        {
            string json = JsonUtility.ToJson(body);
            StartCoroutine(SendRequestCoroutine("/auth/login", "POST", json, null, onSuccess, onError));
        }

        public void UpdateProfile(ProfileBody body, string token, Action<bool> onSuccess, Action<string> onError)
        {
            string json = JsonUtility.ToJson(body);
            // برای متدهایی که خروجی خاصی ندارند و فقط موفقیت مهم است، یک Action خالی پاس میدهیم
            Action<AuthResult> internalSuccess = (res) => onSuccess?.Invoke(true);
            StartCoroutine(SendRequestCoroutine("/auth/update-profile", "POST", json, token, internalSuccess, onError));
        }

        public void CheckVersion(Action<bool> onForceUpdate, Action onLatest)
        {
            // شبیه‌سازی چک کردن ورژن
            bool needsUpdate = false;
            if (needsUpdate) onForceUpdate?.Invoke(true);
            else onLatest?.Invoke();
        }

        // --- Core Logic (Coroutine) ---

        private IEnumerator SendRequestCoroutine<T>(string endpoint, string method, string jsonBody, string token, Action<T> onSuccess, Action<string> onError)
        {
            string url = BASE_URL + endpoint;

            // ساخت ریکوئست
            using (UnityWebRequest request = new UnityWebRequest(url, method))
            {
                // تنظیم بادی (JSON)
                if (!string.IsNullOrEmpty(jsonBody))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.SetRequestHeader("Content-Type", "application/json");
                }

                // دریافت پاسخ
                request.downloadHandler = new DownloadHandlerBuffer();

                // تنظیم هدر توکن
                if (!string.IsNullOrEmpty(token))
                {
                    request.SetRequestHeader("Authorization", "Bearer " + token);
                }

                // ارسال و انتظار
                yield return request.SendWebRequest();

                // بررسی نتیجه
                ProcessResponse(request, onSuccess, onError);
            }
        }

        private void ProcessResponse<T>(UnityWebRequest req, Action<T> onSuccess, Action<string> onError)
        {
            // اگر ارور شبکه (قطعی اینترنت) باشد
            if (req.result == UnityWebRequest.Result.ConnectionError)
            {
                onError?.Invoke("Network Connection Error");
                return;
            }

            // تلاش برای خواندن متن پاسخ (چه موفق، چه ارور 422 و غیره)
            string responseText = req.downloadHandler.text;
            ApiResponse<T> wrapper = null;

            try
            {
                if (!string.IsNullOrEmpty(responseText))
                {
                    wrapper = JsonUtility.FromJson<ApiResponse<T>>(responseText);
                }
            }
            catch (Exception)
            {
                wrapper = null;
            }

            // بررسی کد وضعیت HTTP
            if (req.responseCode >= 200 && req.responseCode < 300)
            {
                // موفقیت آمیز (HTTP 200 OK)
                if (wrapper != null && wrapper.isSuccess)
                {
                    onSuccess?.Invoke(wrapper.result);
                }
                else
                {
                    string msg = wrapper != null ? wrapper.message : "Operation failed on server.";
                    onError?.Invoke(msg);
                }
            }
            else
            {
                // ارورهای سمت سرور (400, 401, 422, 500)
                // اینجا دقیقاً جایی است که پیام "Username taken" را می‌گیریم
                if (wrapper != null && !string.IsNullOrEmpty(wrapper.message))
                {
                    onError?.Invoke(wrapper.message);
                }
                else
                {
                    // اگر پیامی نبود، کد ارور را بده
                    onError?.Invoke($"Error {req.responseCode}: {req.error}");
                }
            }
        }
    }
}