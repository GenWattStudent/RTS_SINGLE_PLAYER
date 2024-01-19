using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerLevel", menuName = "ScriptableObjects/PlayerLevel")]
public class PlayerLevelSo : ScriptableObject
{
    public List<PlayerLevelData> levelsData = new ();
}


[Serializable]
public class PlayerLevelData {
    public int Level;
    public int expToNextLevel;
}
