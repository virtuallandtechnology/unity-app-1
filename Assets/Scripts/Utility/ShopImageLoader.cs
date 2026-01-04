using UnityEngine;
using UnityEngine.UI;
using Best.HTTP;
using System;
using System.IO;
using System.Collections.Generic;

namespace VirtualLand.Utility
{
    /// <summary>
    /// Centralized utility for loading shop product images with disk and in-memory caching.
    /// Based on logic found in ShopItemUI and CartItemUI.
    /// </summary>
    public class ShopImageLoader : MonoBehaviour
    {
        private static ShopImageLoader _instance;
        public static ShopImageLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ShopImageLoader>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ShopImageLoader");
                        _instance = go.AddComponent<ShopImageLoader>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private Dictionary<string, Sprite> _memoryCache = new Dictionary<string, Sprite>();
        private string CustomCacheFolder => Path.Combine(Application.persistentDataPath, "ShopImages");

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (!Directory.Exists(CustomCacheFolder))
            {
                Directory.CreateDirectory(CustomCacheFolder);
            }
        }

        /// <summary>
        /// Loads an image from URL, checks local cache first.
        /// </summary>
        public void LoadImage(string url, Image targetImage, Action<bool> onComplete = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                onComplete?.Invoke(false);
                return;
            }

            // 1. Check Memory Cache
            if (_memoryCache.ContainsKey(url))
            {
                if (targetImage != null)
                {
                    targetImage.sprite = _memoryCache[url];
                    targetImage.color = Color.white;
                    targetImage.preserveAspect = true;
                }
                onComplete?.Invoke(true);
                return;
            }

            // 2. Check Disk Cache
            string fileName = url.GetHashCode().ToString("X") + ".png";
            string filePath = Path.Combine(CustomCacheFolder, fileName);

            if (File.Exists(filePath))
            {
                LoadFromDisk(url, filePath, targetImage, onComplete);
            }
            else
            {
                DownloadAndSave(url, filePath, targetImage, onComplete);
            }
        }

        private void LoadFromDisk(string url, string path, Image targetImage, Action<bool> onComplete)
        {
            try
            {
                byte[] fileData = File.ReadAllBytes(path);
                Texture2D texture = new Texture2D(2, 2);

                if (texture.LoadImage(fileData))
                {
                    Sprite sprite = CreateSprite(texture);
                    _memoryCache[url] = sprite;

                    if (targetImage != null)
                    {
                        targetImage.sprite = sprite;
                        targetImage.color = Color.white;
                        targetImage.preserveAspect = true;
                    }
                    onComplete?.Invoke(true);
                }
                else
                {
                    // If file is corrupted, delete and try downloading
                    File.Delete(path);
                    DownloadAndSave(url, path, targetImage, onComplete);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ShopImageLoader] Error loading from disk: {ex.Message}");
                onComplete?.Invoke(false);
            }
        }

        private void DownloadAndSave(string url, string savePath, Image targetImage, Action<bool> onComplete)
        {
            var request = new HTTPRequest(new Uri(url), HTTPMethods.Get, (req, res) =>
            {
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
                        Sprite sprite = CreateSprite(texture);
                        _memoryCache[url] = sprite;

                        if (targetImage != null)
                        {
                            targetImage.sprite = sprite;
                            targetImage.color = Color.white;
                            targetImage.preserveAspect = true;
                        }

                        try
                        {
                            File.WriteAllBytes(savePath, res.Data);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[ShopImageLoader] Error saving to disk: {ex.Message}");
                        }

                        onComplete?.Invoke(true);
                    }
                    else
                    {
                        onComplete?.Invoke(false);
                    }
                }
                else
                {
                    onComplete?.Invoke(false);
                }
            });

            request.Send();
        }

        private Sprite CreateSprite(Texture2D texture)
        {
            return Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
        }

        public void ClearCache()
        {
            _memoryCache.Clear();
        }
    }
}
