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
        if (stats != null && objectName == skillSo.unitName)
        {
            foreach (var statType in skillSo.statTypes)
            {
                float newValue = skillSo.isPercentage ? skillSo.value.GetValueFromPercent(stats.GetStat(statType)) : skillSo.value;
                stats.AddToStat(statType, newValue);
            }
        }
    }

    public SkillTreeSo GetBoughtSkillByUnitName(string unitName)
    {
        foreach (var skill in skillTreeManager.skills)
        {
            if (skill.unitName == unitName && IsUnlocked(skill))
            {
                return skill;
            }
        }

        return null;
    }

    public List<SkillTreeSo> GetUnlockedAbilities()
    {
        return skillTreeManager.skills.Where((skill) => skill.isAbility && IsUnlocked(skill)).ToList();
    }

    public float GetPercentAmountOfByUnitName(string unitName, StatType valueName)
    {
        var value = 0f;
        foreach (var skill in skillTreeManager.skills)
        {
            if (skill.unitName == unitName && IsUnlocked(skill) && skill.statTypes.Contains(valueName))
            {
                value += skill.value;
            }
        }
        Debug.Log($"GetAmountOfByUnitName {unitName} {valueName} {value}");
        return value;
    }

    public void ApplySkillForUnits(RTSObjectsManager player, SkillTreeSo skill)
    {
        foreach (var unit in RTSObjectsManager.Units[player.OwnerClientId])
        {
            if (unit.unitSo.unitName == skill.unitName)
            {
                ApplySkill(unit.GetComponent<Stats>(), unit.unitSo.unitName, skill);
            }
        }
    }

    public void ApplySkills(ISkillApplicable applicable)
    {
        foreach (var skill in skillTreeManager.skills)
        {
            if (skill.unitName == applicable.Name && IsUnlocked(skill))
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
            Debug.Log($"Unlocking skill {skill.skillName} for player {rtsObjectManager.OwnerClientId}");
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
        if (skillPoints >= skillSo.requiredSkillPoints)
        {
            foreach (var skill in skillSo.requiredSkills)
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
