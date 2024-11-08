using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "ScriptableObjects/Skill")]
public class SkillSo : ScriptableObject
{
    public string skillName;
    public string description;
    public Sprite icon;
    public List<SkillSo> requiredSkills = new();
    public int requiredSkillPoints;
    public string skillTag;
    public StatType[] statTypes;
    public int value;
    public string unitName;
    public bool isPercentage = true;

    public float duration;
    public bool isPermanent => duration <= 0;

    public float cooldown;
    public bool hasCooldown => cooldown > 0;
}
