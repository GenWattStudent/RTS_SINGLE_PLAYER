using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySo : ScriptableObject
{
    public virtual void Activate(Unit unit) { }
}

[CreateAssetMenu(fileName = "Skill", menuName = "RTS/Skill")]
public class SkillSo : AbilitySo
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
    public GameObject skillPrefab;

    public bool isAbility = false;

    public float duration;
    public bool isPermanent => duration <= 0;

    public float cooldown;
    public bool hasCooldown => cooldown > 0;
}