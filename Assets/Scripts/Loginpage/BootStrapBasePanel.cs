using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class BootStrapBasePanel:MonoBehaviour
{
    [SerializeField] protected GameObject _root;
    //[SerializeField] protected TMP_Text _log;

    private static List<BootStrapBasePanel> _allPanels = new List<BootStrapBasePanel>();

    private void Awake()
    {
        _allPanels.Add(this);
    }
    private void OnDestroy()
    {
        _allPanels.Remove(this);
    }

    public virtual void Show()
    {
        _root.SetActive(true);
        foreach (var panel in _allPanels)
        {
            if (panel._root!=_root)
                panel.Hide();
        }
    }

    public virtual void Hide()
    {
        _root.SetActive(false);
    }

 

    public bool GetVisible()
    {
        return _root.activeInHierarchy;
    }


}
