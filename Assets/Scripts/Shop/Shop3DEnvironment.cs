using UnityEngine;

namespace Game.Shop.Visuals
{
    public class Shop3DEnvironment : MonoBehaviour
    {
        [SerializeField] private Transform _spawnPoint;

        public Transform SpawnPoint => _spawnPoint ? _spawnPoint : transform;
    }
}
