using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Utils;

public class PowerUp : NetworkBehaviour
{
    public NetworkList<int> unlockedSkillsIndex;

    private SkillTreeManager skillTreeManager;
    public event Action<SkillTreeSo> OnSkillUnlocked;

    private void Awake()
    {
        unlockedSkillsIndex = new NetworkList<int>();
        skillTreeManager = GetComponent<SkillTreeManager>();
    }

    private void ApplySkill(Stats stats, string objectName, SkillTreeSo skillSo)
    {
        if (stats != null && skillSo.UnitNames.Contains(objectName))
        {
            foreach (var stat in skillSo.PowerUp.Stats)
            {
                float newValue = skillSo.PowerUp.IsPercentage ? stat.BaseValue.GetValueFromPercent(stats.GetStat(stat.Type)) : stat.BaseValue;
                stats.AddToStat(stat.Type, Mathf.Round(newValue));
            }
        }
    }

    public SkillTreeSo GetBoughtSkillByUnitName(string unitName)
    {
        foreach (var skill in skillTreeManager.skills)
        {
            if (skill.UnitNames.Contains(unitName) && IsUnlocked(skill))
            {
                return skill;
            }
        }

        return null;
    }

    public List<SkillTreeSo> GetUnlockedAbilities()
    {
        return skillTreeManager.skills.Where((skill) => skill.PowerUp.IsAbility && IsUnlocked(skill)).ToList();
    }

    public float GetPercentAmountOfByUnitName(string unitName, StatType valueName)
    {
        var value = 0f;
        foreach (var skill in skillTreeManager.skills)
        {
            foreach (var stat in skill.PowerUp.Stats)
            {
                if (skill.UnitNames.Contains(unitName) && IsUnlocked(skill) && stat.Type == valueName)
                {
                    value += stat.BaseValue;
                }
            }
        }
        Debug.Log($"GetAmountOfByUnitName {unitName} {valueName} {value}");
        return value;
    }

    public void ApplySkillForUnits(RTSObjectsManager player, SkillTreeSo skill)
    {
        foreach (var unit in RTSObjectsManager.Units[player.OwnerClientId])
        {
            if (skill.UnitNames.Contains(unit.unitSo.unitName))
            {
                ApplySkill(unit.GetComponent<Stats>(), unit.unitSo.unitName, skill);
            }
        }
    }

    public void ApplySkills(ISkillApplicable applicable)
    {
        foreach (var skill in skillTreeManager.skills)
        {
            if (skill.UnitNames.Contains(applicable.Name) && IsUnlocked(skill))
            {
                ApplySkill(applicable.Stats, applicable.Name, skill);
            }
        }
    }

    public bool Unlock(SkillTreeSo skill, int skillIndex, int skillPoints, ServerRpcParams serverRpcParams = default)
    {
        if (CanBePurchased(skill, skillPoints))
        {
            var rtsObjectManager = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject.GetComponent<RTSObjectsManager>();
            Debug.Log($"Unlocking skill {skill.PowerUp.Name} for player {rtsObjectManager.OwnerClientId}");
            ApplySkillForUnits(rtsObjectManager, skill);
            unlockedSkillsIndex.Add(skillIndex);
            OnSkillUnlocked?.Invoke(skill);
            return true;
        }

        return false;
    }

    public bool IsUnlocked(SkillTreeSo skillSo)
    {
        var skillIndex = skillTreeManager.skills.IndexOf(skillSo);
        return unlockedSkillsIndex.Contains(skillIndex);
    }

    public bool CanBePurchased(SkillTreeSo skillSo, int skillPoints)
    {
        if (skillPoints >= skillSo.RequiredSkillPoints)
        {
            foreach (var skill in skillSo.RequiredSkills)
            {
                if (!IsUnlocked(skill))
                {
                    return false;
                }
            }

            return !IsUnlocked(skillSo);
        }

        return false;
    }
}
