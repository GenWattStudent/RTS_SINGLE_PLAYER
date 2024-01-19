using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "ScriptableObjects/Skill")]
public class SkillSo : ScriptableObject
{
    public string skillName;
    public string description;
    public Sprite icon;
    public List<SkillSo> requiredSkills = new ();
    public int requiredSkillPoints;
    public event Action<SkillSo> OnSkillUnlocked;
    public string skillTag;
    public string valueName;
    public int value;
    public string unitName;

    private void AddDamageToUnits(string unitName, int value)
    {
        foreach (var unit in PlayerController.units)
        {
            var damagable = unit.GetComponent<Damagable>();
            var unitScript = unit.GetComponent<Unit>();

            if (damagable != null && unitScript != null && unitScript.unitSo.unitName == unitName)
            {
                damagable.AddDamageBoost(value);
            }
        }
    }

    private void AddHealthToUnits(string unitName, int value)
    {
        foreach (var unit in PlayerController.units)
        {
            var damagable = unit.GetComponent<Damagable>();
            var unitScript = unit.GetComponent<Unit>();

            if (damagable != null && unitScript != null && unitScript.unitSo.unitName == unitName)
            {
                var newHealth = damagable.maxHealth * value / 100;

                damagable.maxHealth += newHealth;
                damagable.TakeDamage(-newHealth);
            }
        }
    }

    public bool Unlock(List<SkillSo> unlockedSkills, int skillPoints)
    {
        if (CanBePurchased(unlockedSkills, skillPoints))
        {
            OnSkillUnlocked?.Invoke(this);
            if (valueName == "damage") AddDamageToUnits(unitName, value);
            else if (valueName == "health") AddHealthToUnits(unitName, value);
            return true;
        }

        return false;
    }

    public bool IsUnlocked(List<SkillSo> unlockedSkills)
    {
        foreach (var skill in unlockedSkills)
        {
            if (skill == this)
            {
                return true;
            }
        }

        return false;
    }

    public bool CanBePurchased(List<SkillSo> unlockedSkills, int skillPoints)
    {
        if (skillPoints >= requiredSkillPoints)
        {
            foreach (var skill in requiredSkills)
            {
                if (!skill.IsUnlocked(unlockedSkills))
                {
                    return false;
                }
            }

            if (unlockedSkills.Contains(this))
            {
                return false;
            }

            return true;
        }

        return false;
    }
}
