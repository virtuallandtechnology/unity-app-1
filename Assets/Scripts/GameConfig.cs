using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Game Config", order = 0)]

public class GameConfig : ScriptableObject
{
    [Header("Server Settings")]
    public string BaseURL;
    private static GameConfig _instance;
    public static GameConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameConfig>("GameConfig");

                if (_instance == null)
                {
                    Debug.LogError("GameConfig not found in Resources folder! " +
                                 "Create a GameConfig asset and place it in a Resources folder.");
                }
            }
            return _instance;
        }
    }
}

