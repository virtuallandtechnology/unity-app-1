//using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationController : MonoBehaviour
{
    private static NotificationController instance;
    [SerializeField] private GameObject NotiicationRoot;
    [SerializeField] private Text _titleText;
    [SerializeField] private Text _descriptionText;
    [SerializeField] private Button _okButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private TMP_Text _TimerPaneldescriptionText;
    [SerializeField] private GameObject yesNoPanel;
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private float timerDelay = 4;
    private void Start()
    {
        instance = this;
    }

    public static NotificationController Get()
    {
        return instance;
    }

    public void Show(string title, string description, Action onOk, Action onCancel)
    {

    }

    public void Show(string description)
    {
        Debug.Log(description);
        NotiicationRoot.SetActive(true);
        _TimerPaneldescriptionText.SetText(description);
        // Reset scale and make visible
        timerPanel.transform.localScale = Vector3.zero;
        //backgroundImage.gameObject.SetActive(true);
        timerPanel.SetActive(true);
        // Scale animation
        //timerPanel.transform.DOScale(Vector3.one, 0.5f)
        //               .SetEase(Ease.OutBack)
        //               .OnComplete(() =>
        //               {
        //                   // Auto hide after delay
        //                   DOVirtual.DelayedCall(timerDelay, () =>
        //                   {
        //                       timerPanel.transform.DOScale(Vector3.zero, 0.3f)
        //                                      .SetEase(Ease.InBack)
        //                                      .OnComplete(() => {
        //                                          //backgroundImage.gameObject.SetActive(false); 
        //                                          timerPanel.SetActive(false);
        //                                      }); 
        //                   });
        //               });
    }
}
