using System;
using Unity.Netcode;
using UnityEngine;

public class PowerUp : NetworkBehaviour
{
    public NetworkList<int> unlockedSkillsIndex;

    private SkillTreeManager skillTreeManager;
    public event Action<SkillSo> OnSkillUnlocked;

    private void Awake()
    {
        unlockedSkillsIndex = new NetworkList<int>();
        skillTreeManager = GetComponent<SkillTreeManager>();
    }

    private void AddDamageToUnits(RTSObjectsManager player, string unitName, int value)
    {
        foreach (var unit in RTSObjectsManager.Units[player.OwnerClientId])
        {
            var damagable = unit.GetComponent<Damagable>();
            var unitScript = unit.GetComponent<Unit>();

            if (damagable != null && unitScript != null && unitScript.unitSo.unitName == unitName)
            {
                damagable.AddDamageBoost(value);
            }
        }
    }

    private void AddHealthToUnits(RTSObjectsManager player, string unitName, int value)
    {
        foreach (var unit in RTSObjectsManager.Units[player.OwnerClientId])
        {
            var damagable = unit.GetComponent<Damagable>();
            var unitScript = unit.GetComponent<Unit>();

            if (damagable != null && unitScript != null && unitScript.unitSo.unitName == unitName)
            {
                var newHealth = damagable.stats.GetStat(StatType.MaxHealth) * value / 100;

                damagable.stats.AddToStat(StatType.MaxHealth, newHealth);
                damagable.TakeDamage(-newHealth);
            }
        }
    }

    public SkillSo GetBoughtSkillByUnitName(string unitName)
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

    public float GetPercentAmountOfByUnitName(string unitName, string valueName)
    {
        var value = 0f;
        foreach (var skill in skillTreeManager.skills)
        {
            if (skill.unitName == unitName && IsUnlocked(skill) && skill.valueName == valueName)
            {
                value += skill.value;
            }
        }
        Debug.Log($"GetAmountOfByUnitName {unitName} {valueName} {value}");
        return value;
    }

    public bool Unlock(SkillSo skill, int skillIndex, int skillPoints, ServerRpcParams serverRpcParams = default)
    {
        if (CanBePurchased(skill, skillPoints))
        {
            var rtsObjectManager = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject.GetComponent<RTSObjectsManager>();
            Debug.Log($"Unlocking skill {skill.skillName} for player {rtsObjectManager.OwnerClientId}");
            if (skill.valueName == "damage") AddDamageToUnits(rtsObjectManager, skill.unitName, skill.value);
            else if (skill.valueName == "health") AddHealthToUnits(rtsObjectManager, skill.unitName, skill.value);
            unlockedSkillsIndex.Add(skillIndex);
            OnSkillUnlocked?.Invoke(skill);
            return true;
        }

        return false;
    }

    public bool IsUnlocked(SkillSo skillSo)
    {
        var skillIndex = skillTreeManager.skills.IndexOf(skillSo);
        return unlockedSkillsIndex.Contains(skillIndex);
    }

    public bool CanBePurchased(SkillSo skillSo, int skillPoints)
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
