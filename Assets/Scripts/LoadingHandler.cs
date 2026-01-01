using Unity.VisualScripting;
using UnityEngine;

public class LoadingHandler : MonoBehaviour
{
    [SerializeField] private GameObject _loading;
    private static LoadingHandler instance;

    private void Awake()
    {
        instance = this;
    }

    public static LoadingHandler Get()
    {
        return instance;
    }

    public void SetVisible(bool visible)
    {
        _loading.SetActive(visible);
    }
}
