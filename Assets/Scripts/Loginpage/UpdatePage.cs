using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdatePage : BootStrapBasePanel
{
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _currentVersion;
    [SerializeField] private TMP_Text _NewVersion;
    [SerializeField] private TMP_Text _Description;
    [SerializeField] private Button _update;
    [SerializeField] private Button _skip;


    public void ShowUpdate(string cuversion, string newversion, string description, Action update, Action skip)
    {
        _title.text = "UPDATE";
        _currentVersion.text = "CurrentVersion: " + cuversion;
        _NewVersion.text = "New Version: " + newversion;
        _Description.text = description;
        _skip.interactable = true;
        _skip.onClick.RemoveAllListeners();
        _skip.onClick.AddListener(() => skip());

        _update.onClick.RemoveAllListeners();
        _update.onClick.AddListener(() => update());
    }

    public void ShowForceUpdate(string cuversion, string newversion, string description, Action update, Action skip)
    {
        _title.text = "FORCE UPDATE";
        _currentVersion.text = "CurrentVersion: " + cuversion;
        _NewVersion.text = "New Version: " + newversion;
        _Description.text = description;
        _skip.interactable = false;
        _skip.onClick.RemoveAllListeners();
        //_skip.onClick.AddListener(() => skip());

        _update.onClick.RemoveAllListeners();
        _update.onClick.AddListener(() => update());
    }


}
