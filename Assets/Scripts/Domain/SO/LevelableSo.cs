using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Levelable", menuName = "ScriptableObjects/Levelable")]
public class LevelableSo : ScriptableObject
{
    [System.Serializable]
    public class Level
    {
        public int level;
        public int expirence;
        public List<Stat> stats;
    }

    public List<Level> levels;
}
