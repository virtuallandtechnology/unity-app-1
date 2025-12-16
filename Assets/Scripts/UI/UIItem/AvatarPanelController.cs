using UnityEngine;
using DG.Tweening; // کتابخانه DoTween
using System.Collections.Generic;
using UnityEngine.UI;

public class AvatarPanelController : MonoBehaviour
{
    [Header("References")]
    public Profile profileScript;
    public GameObject avatarPanelContainer;
    public Transform avatarContentGrid; 
    public AvatarItemUI avatarItemPrefab; 
    public Transform panelContent; 
   

    [Header("Animation Settings")]
    public float animDuration = 0.5f;
    public Ease openEase = Ease.OutBack;
    public Ease closeEase = Ease.InBack;

    private void Start()
    {
        GenerateAvatarList();

        avatarPanelContainer.SetActive(false);
       // panelContent.localScale = Vector3.zero;
    }

    private void GenerateAvatarList()
    {
        var avatars = AvatarsConfig.Instance.Avatars;

        //foreach (Transform child in avatarContentGrid)
        //{
        //    Destroy(child.gameObject);
        //}

        for (int i = 0; i < avatars.Count; i++)
        {
            AvatarItemUI newItem = Instantiate(avatarItemPrefab, avatarContentGrid);
            newItem.Setup(i, avatars[i].sprite, this);
        }
    }


    public void OpenPanel()
    {
        avatarPanelContainer.SetActive(true);

        panelContent.localScale = Vector3.zero;

        panelContent.DOScale(1f, animDuration).SetEase(openEase);
    }

    public void ClosePanel()
    {
        panelContent.DOScale(0f, animDuration)
            .SetEase(closeEase)
            .OnComplete(() =>
            {
                avatarPanelContainer.SetActive(false);
            });
    }


    public void SelectAvatar(int id)
    {
        profileScript.SetProfile(id);

        ClosePanel();
    }
}