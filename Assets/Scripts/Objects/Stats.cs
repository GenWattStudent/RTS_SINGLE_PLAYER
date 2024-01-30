using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    private Unit unit;
    private Building building;
    private List<Stat> stats = new();

    private void Awake()
    {
        unit = GetComponent<Unit>();
        building = GetComponent<Building>();

        if (building != null)
        {
            foreach (var stat in building.buildingSo.stats)
            {
                stats.Add(new Stat { Type = stat.Type, Value = stat.Value });
            }
        }
        else
        {
            foreach (var stat in unit.unitSo.stats)
            {
                stats.Add(new Stat { Type = stat.Type, Value = stat.Value });
            }
        }
    }

    public float GetStat(StatType type)
    {
        foreach (var stat in stats)
        {
            if (stat.Type == type)
            {
                return stat.Value;
            }
        }

        return -1;
    }

    public void SetStat(StatType type, float value)
    {
        foreach (var stat in stats)
        {
            if (stat.Type == type)
            {
                stat.Value = value;
            }
        }
    }

    public float AddToStat(StatType type, float value)
    {
        foreach (var stat in stats)
        {
            if (stat.Type == type)
            {
                stat.Value += value;
                return stat.Value;
            }
        }

        return -1;
    }

    public float SubstractFromStat(StatType type, float value)
    {
        foreach (var stat in stats)
        {
            if (stat.Type == type)
            {
                stat.Value -= value;
                return stat.Value;
            }
        }

        return -1;
    }

    public void AddStat(StatType type, float value)
    {
        stats.Add(new Stat { Type = type, Value = value });
    }
}
