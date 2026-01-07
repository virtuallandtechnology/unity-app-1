using UnityEngine;
using Game.Shop.Preview;

namespace Game.Shop.Testing
{
    public class PreviewSystemTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private int testProductId = 123;
        [SerializeField] private string testCategorySlug = "CHARCTERS";

        [ContextMenu("Test Load by Product ID")]
        private void TestLoadByProductId()
        {
            var loader = ProductPreviewLoader.Instance;
            if (loader == null)
            {
                Debug.LogError("[Test] ProductPreviewLoader not found in scene!");
                return;
            }

            Debug.Log($"[Test] Loading preview for Product ID: {testProductId}");
            
            loader.LoadPreviewByProductId(testProductId, testCategorySlug, false, (success) =>
            {
                if (success)
                {
                    Debug.Log("[Test] ✅ Preview loaded successfully!");
                }
                else
                {
                    Debug.LogError("[Test] ❌ Failed to load preview");
                }
            });
        }

        [ContextMenu("Test Close Preview")]
        private void TestClosePreview()
        {
            var loader = ProductPreviewLoader.Instance;
            if (loader != null)
            {
                loader.ClosePreview();
                Debug.Log("[Test] Preview closed");
            }
        }

        private void OnEnable()
        {
            var loader = ProductPreviewLoader.Instance;
            if (loader != null)
            {
                loader.OnPreviewLoaded += OnPreviewLoaded;
                loader.OnPreviewClosed += OnPreviewClosed;
                loader.OnPreviewError += OnPreviewError;
            }
        }

        private void OnDisable()
        {
            var loader = ProductPreviewLoader.Instance;
            if (loader != null)
            {
                loader.OnPreviewLoaded -= OnPreviewLoaded;
                loader.OnPreviewClosed -= OnPreviewClosed;
                loader.OnPreviewError -= OnPreviewError;
            }
        }

        private void OnPreviewLoaded(ApiClient.ShopProduct product)
        {
            Debug.Log($"[Test Event] Preview loaded: {product.title}");
        }

        private void OnPreviewClosed()
        {
            Debug.Log("[Test Event] Preview closed");
        }

        private void OnPreviewError(string error)
        {
            Debug.LogError($"[Test Event] Preview error: {error}");
        }
    }
}
