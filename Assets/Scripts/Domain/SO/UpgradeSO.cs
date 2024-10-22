using System.Collections.Generic;
using UnityEngine;

namespace RTS.Domain.SO
{

    [CreateAssetMenu(fileName = "Upgrade", menuName = "RTS/Upgrade")]
    public class UpgradeSO : ScriptableObject
    {
        public string Name;
        public string Description;
        public int Cost;
        public ResourceSO costResource;
        public int UnlockLevel;

        public Sprite Icon;
        public GameObject ConstructionPrefab;
        public List<Stat> Stats = new();
        public List<UnitSo> ForUnits = new();
    }
}
