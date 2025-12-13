using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ApiClient;

public class TopBarProfile : MonoBehaviour
{
    public TMP_Text Username;
    public Image Avatar;
    public GameObject MenuRoot;

    private void OnEnable()
    {
        //if (!ApiClient.GetPlayer().IsReady())
        //{
        //    Debug.LogError("UserData.player.IsReady = false");
        //    return;
        //}
        Username.text = GetPlayer().GetUsername();
        if ((ApiClient.GetPlayer().profile != null))
            Avatar.sprite = AvatarsConfig.Instance.Avatars[ApiClient.GetPlayer().profile.avatar_id].sprite;
        _ = showDelay();

    }

    public async Task showDelay()
    {
        await Task.Delay(500);
        MenuRoot.SetActive(true);
    }
}
