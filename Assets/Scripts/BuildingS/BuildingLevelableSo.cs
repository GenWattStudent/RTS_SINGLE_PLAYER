using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingLevelable", menuName = "ScriptableObjects/BuildingLevelable")]
public class BuildingLevelableSo : ScriptableObject
{
    [System.Serializable]
    public class BuildingLevel
    {
        public int level;
        public int health;
        public int attackDamage;
        public int cost;
        public int income;
        public int reduceSpawnTime;
        public ResourceSO resourceSO;
    }

    public List<BuildingLevel> levels;
}
