using UnityEngine;
using UnityEngine.UI;

namespace VirtualLand
{
    public class AvatarItemUI : MonoBehaviour
    {
        public Image avatarImage;
        public Button btnSelect;

        private int _avatarId;
        private AvatarPanelController _controller;

        public void Setup(int id, Sprite sprite, AvatarPanelController controller)
        {
            _avatarId = id;
            avatarImage.sprite = sprite;
            _controller = controller;

            btnSelect.onClick.RemoveAllListeners();
            btnSelect.onClick.AddListener(OnItemClicked);
        }

        private void OnItemClicked()
        {
            _controller.SelectAvatar(_avatarId);
        }
    }
}