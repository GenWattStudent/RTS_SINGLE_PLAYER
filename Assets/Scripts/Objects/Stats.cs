using System;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class Stats : NetworkBehaviour
{
    public NetworkList<Stat> BaseStats;

    private Unit unit;
    private Building building;

    private void Awake()
    {
        BaseStats = new NetworkList<Stat>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        unit = GetComponent<Unit>();
        building = GetComponent<Building>();

        if (!IsServer) return;

        if (building != null)
        {
            AddStatsFromProperties(building.buildingSo);
        }
        else
        {
            AddStatsFromProperties(unit.unitSo);
        }
    }

    private void AddStatsFromProperties(object source)
    {
        FieldInfo[] fields = source.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            if (field.FieldType == typeof(int) || field.FieldType == typeof(float) || field.FieldType == typeof(double))
            {
                if (Enum.TryParse(field.Name, true, out StatType statType))
                {
                    var value = Convert.ToSingle(field.GetValue(source));
                    if (statType == StatType.Health)
                    {
                        Debug.Log("Adding health");
                        AddStat(StatType.MaxHealth, value);
                    }
                    Debug.Log("Adding " + statType + " " + value);
                    AddStat(statType, value);
                }
            }
        }
    }

    public void AddStat(StatType type, float value)
    {
        if (!IsServer) return;
        Debug.Log("Adding stat " + type + " " + value);
        for (int i = 0; i < BaseStats.Count; i++)
        {
            if (BaseStats[i].Type == type)
            {
                return;
            }
        }

        BaseStats.Add(new Stat { Type = type, CurrentValue = value, BaseValue = value });
    }

    public float GetStat(StatType type)
    {
        for (int i = 0; i < BaseStats.Count; i++)
        {
            if (BaseStats[i].Type == type)
            {
                return BaseStats[i].CurrentValue;
            }
        }

        return -1;
    }

    public float GetBaseStat(StatType type)
    {
        for (int i = 0; i < BaseStats.Count; i++)
        {
            if (BaseStats[i].Type == type)
            {
                return BaseStats[i].BaseValue;
            }
        }

        return -1;
    }

    public void SetStat(StatType type, float value)
    {
        if (!IsServer) return;
        for (int i = 0; i < BaseStats.Count; i++)
        {
            if (BaseStats[i].Type == type)
            {
                var stat = BaseStats[i];
                stat.CurrentValue = value;
                BaseStats[i] = stat;
                return;
            }
        }
    }

    public float AddToStat(StatType type, float value)
    {
        if (!IsServer) return -1;
        for (int i = 0; i < BaseStats.Count; i++)
        {
            if (BaseStats[i].Type == type)
            {
                var stat = BaseStats[i];
                stat.CurrentValue += value;
                BaseStats[i] = stat;
                return stat.CurrentValue;
            }
        }

        return -1;
    }

    public float SubstractFromStat(StatType type, float value)
    {
        if (!IsServer) return -1;

        for (int i = 0; i < BaseStats.Count; i++)
        {
            if (BaseStats[i].Type == type)
            {
                var stat = BaseStats[i];
                stat.CurrentValue -= value;
                BaseStats[i] = stat;
                return stat.CurrentValue;
            }
        }
        return -1;
    }
}
