using System;
using System.Collections.Generic;
using UnityEngine;




[CreateAssetMenu(fileName = "Avatars", menuName = "Game/Avatars", order = 0)]

public class AvatarsConfig : ScriptableObject
{
    [Serializable]
    public class AvatarData
    {
        public int id;
        public Sprite sprite;
    }

    public List<AvatarData> Avatars;
    
    private static AvatarsConfig _instance;
    public static AvatarsConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<AvatarsConfig>("Avatars");

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

