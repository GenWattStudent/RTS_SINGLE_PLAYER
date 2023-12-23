using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Levelable", menuName = "ScriptableObjects/Levelable")]
public class LevelableSo : ScriptableObject
{
    [System.Serializable]
    public class Level {
        public int level;
        public int expirence;
        public float health;
        public float attackDamage;
    }

    public List<Level> levels;
}
