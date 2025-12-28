using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Shop.Visuals
{
    [Serializable]
    public struct Item3DMapping
    {
        public int ProductId;
        public GameObject Prefab;
    }

    [Serializable]
    public struct CategoryEnvironmentMapping
    {
        public string CategorySlug;
        public Shop3DEnvironment EnvironmentPrefab;
    }

    [CreateAssetMenu(fileName = "Shop3DConfig", menuName = "Shop/3D Config")]
    public class Shop3DConfig : ScriptableObject
    {
        [SerializeField] private List<Item3DMapping> _itemMappings;
        [SerializeField] private List<CategoryEnvironmentMapping> _categoryEnvironments;
        [SerializeField] private Shop3DEnvironment _fallbackEnvironment;

        public GameObject GetItemPrefab(int id)
        {
            foreach (var map in _itemMappings)
            {
                if (map.ProductId == id) return map.Prefab;
            }
            return null;
        }

        public Shop3DEnvironment GetEnvironment(string slug)
        {
            foreach (var map in _categoryEnvironments)
            {
                if (map.CategorySlug == slug) return map.EnvironmentPrefab;
            }
            return _fallbackEnvironment;
        }
    }
}
