using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class CheckVersion : MonoBehaviour
{
    public ForceUpdateData ForceUpdateData;
    //public UpdatePresenter UpdatePresenter;
    public string CurrentPayableCoinInGame;
    public GameObject NoNetwork;
    public GameObject _loadingPanle;
    //public IntroVidoController videoController;

    private void Start()
    {
        GetVersionFromServer();
    }
    public void GetVersionFromServer()
    {
        ApiClient.Get().GetServerConfig((t) =>
        {
           
           // ForceUpdateData = new ForceUpdateData(t.result.result.VERSION_INFO, Application.version);
            //ForceUpdateData.ForceUpdate = false;
            //ForceUpdateData.CanUpdate = true;
            //ForceUpdateData.NoUpdate=false; 
            //UpdatePresenter.Setup(new UpdateData()
            //{
            //    CurrentVersion = ForceUpdateData.GetCurrentVersion,
            //    ServerVersion = ForceUpdateData.GetServerVersion(),
            //    forceupdate = ForceUpdateData.ForceUpdate,
            //    Description = ForceUpdateData.description
            //});

            //if (ForceUpdateData.ForceUpdate)
            //    ShowUpdatePanel();
            //else if (ForceUpdateData.CanUpdate)
            //    ShowUpdatePanel();
            //else if (ForceUpdateData.NoUpdate)
            //{
            //    _loadingPanle.SetActive(true);
            //    videoController.GoToNextScene();
            //}
            //else
            //    NoNetwork.SetActive(true);

            //Debug.Log("AI_DIFFICULTY_LEVEL=" + data.result.AI_DIFFICULTY_LEVEL);

            //PlayerPrefs.SetInt("ailevel", Mathf.Clamp(
            //    Mathf.RoundToInt(int.Parse(data.result.AI_DIFFICULTY_LEVEL)), 1, 10));
        }, 
        (t) =>
        {
        });
        //CardGameApiHandler.GetVersion(new HttpRequestAction<ServerConfig.ServerConfig>()
        //{
        //    OnFinishedSuccess = (request, data) =>
        //    {
        //        ServerConfig.LastConfig = data;
        //        CurrentPayableCoinInGame = GetPayableCoinInGame(data);
        //        ForceUpdateData = new ForceUpdateData(data.result.VERSION_INFO, Application.version);
        //        //ForceUpdateData.ForceUpdate = false;
        //        //ForceUpdateData.CanUpdate = true;
        //        //ForceUpdateData.NoUpdate=false; 
        //        UpdatePresenter.Setup(new UpdateData()
        //        {
        //            CurrentVersion = ForceUpdateData.GetCurrentVersion,
        //            ServerVersion = ForceUpdateData.GetServerVersion(),
        //            forceupdate =  ForceUpdateData.ForceUpdate,
        //            Description = ForceUpdateData.description
        //        }) ;

        //        if (ForceUpdateData.ForceUpdate)
        //            ShowUpdatePanel();
        //        else if (ForceUpdateData.CanUpdate)
        //            ShowUpdatePanel();
        //        else if (ForceUpdateData.NoUpdate)
        //        {
        //            _loadingPanle.SetActive(true);
        //          videoController.GoToNextScene();
        //        }
        //        else
        //            NoNetwork.SetActive(true);

        //        Debug.Log("AI_DIFFICULTY_LEVEL="+data.result.AI_DIFFICULTY_LEVEL);

        //        PlayerPrefs.SetInt("ailevel", Mathf.Clamp(
        //            Mathf.RoundToInt(int.Parse(data.result.AI_DIFFICULTY_LEVEL)), 1, 10));
        //    },
        //    OnFinishedFail = request => { },
        //    OnConnectionTimedOut = request => { },
        //    OnTimedOut = request => { },
        //    OnError = request => { }
        //});
    }


    public void OpenUrlForDownload()
    {
        Application.OpenURL(ForceUpdateData.UpdateUrl);
    }




}
