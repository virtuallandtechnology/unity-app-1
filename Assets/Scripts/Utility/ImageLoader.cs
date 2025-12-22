using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class ImageLoader : MonoBehaviour
{
    public static ImageLoader Instance;

    private Dictionary<string, Sprite> _imageCache = new Dictionary<string, Sprite>();

    [SerializeField] private Sprite _loadingPlaceholder; 
    [SerializeField] private Sprite _errorPlaceholder;   

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void LoadImage(string url, Image targetImage)
    {
        if (targetImage == null) return;
        if (string.IsNullOrEmpty(url))
        {
            if (_errorPlaceholder != null) targetImage.sprite = _errorPlaceholder;
            return;
        }

        if (_imageCache.ContainsKey(url))
        {
            if (_imageCache[url] != null)
                targetImage.sprite = _imageCache[url];
            return;
        }

        if (_loadingPlaceholder != null) targetImage.sprite = _loadingPlaceholder;

        StartCoroutine(DownloadRoutine(url, targetImage));
    }

    private IEnumerator DownloadRoutine(string url, Image targetImage)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Download Error: " + uwr.error);
                if (_errorPlaceholder != null && targetImage != null)
                    targetImage.sprite = _errorPlaceholder;
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

                Sprite sprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));

                if (!_imageCache.ContainsKey(url))
                    _imageCache.Add(url, sprite);

                if (targetImage != null)
                {
                    targetImage.sprite = sprite;
                    targetImage.preserveAspect = true; 
                }
            }
        }
    }
}