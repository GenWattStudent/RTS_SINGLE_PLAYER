using System.Collections.Generic;
using UnityEngine;

public class AbilitySo : ScriptableObject
{
    public virtual void Activate(Unit unit) { }
}

[CreateAssetMenu(fileName = "Skill", menuName = "RTS/Skill")]
public class SkillTreeSo : AbilitySo
{
    public string Description;
    public List<SkillTreeSo> RequiredSkills = new();
    public int RequiredSkillPoints;
    public string SkillTag;
    public List<string> UnitNames = new();

    public PowerUpSo PowerUp;
}