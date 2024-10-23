using System.Collections.Generic;
using UnityEngine;

namespace RTS.Domain.SO
{

    [CreateAssetMenu(fileName = "Upgrade", menuName = "RTS/Upgrade")]
    public class UpgradeSO : ScriptableObject, IConstruction
    {
        public string Name;
        public string Description;
        public int Cost;
        public ResourceSO costResource;
        public int UnlockLevel;

        public Sprite Icon;
        public List<Stat> Stats = new();
        public List<UnitSo> ForUnits = new();

        [Header("Building Prefabs")]
        [SerializeField] private GameObject _prefab;
        [SerializeField] private GameObject _previewPrefab;
        [SerializeField] private GameObject _constructionManagerPrefab;

        public GameObject prefab
        {
            get => _prefab;
            set => _prefab = value;
        }

        public GameObject previewPrefab
        {
            get => _previewPrefab;
            set => _previewPrefab = value;
        }

        public GameObject constructionManagerPrefab
        {
            get => _constructionManagerPrefab;
            set => _constructionManagerPrefab = value;
        }
    }
}
