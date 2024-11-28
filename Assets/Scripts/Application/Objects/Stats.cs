using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

public class PowerUpData
{
    public PowerUpSo PowerUpSo;
    public float CollectedTime;
    public List<Stat> StatsAdded;
}

[DefaultExecutionOrder(-1)]
public class Stats : NetworkBehaviour
{
    public NetworkList<Stat> BaseStats;
    public List<PowerUpData> PowerUps = new List<PowerUpData>();

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

    private void CalculatePowerUpData(PowerUpSo powerUpSo)
    {
        var powerUpData = new PowerUpData
        {
            PowerUpSo = powerUpSo,
            CollectedTime = Time.time,
            StatsAdded = new List<Stat>()
        };

        foreach (var stat in powerUpSo.Stats)
        {
            var statValue = stat.CurrentValue;
            var baseStat = GetBaseStat(stat.Type);

            if (baseStat == -1)
            {
                continue;
            }

            if (!powerUpSo.IsStackable)
            {
                for (int i = 0; i < PowerUps.Count; i++)
                {
                    if (PowerUps[i].PowerUpSo.Name == powerUpSo.Name)
                    {
                        PowerUps[i].CollectedTime = Time.time;
                        return;
                    }
                }
            }

            if (powerUpSo.IsPercentage)
            {
                statValue = baseStat * stat.CurrentValue / 100;
            }


            AddToStat(stat.Type, statValue);
            powerUpData.StatsAdded.Add(new Stat { Type = stat.Type, CurrentValue = statValue, BaseValue = statValue });
            PowerUps.Add(powerUpData);
        }
    }

    public void AddPowerUp(PowerUpSo powerUpSo)
    {
        if (!IsServer) return;

        CalculatePowerUpData(powerUpSo);
    }

    public void RemovePowerUp(PowerUpSo powerUpSo)
    {
        if (!IsServer) return;

        for (int i = 0; i < PowerUps.Count; i++)
        {
            if (PowerUps[i].PowerUpSo == powerUpSo)
            {
                foreach (var stat in PowerUps[i].StatsAdded)
                {
                    SubstractFromStat(stat.Type, stat.CurrentValue);
                }

                PowerUps.RemoveAt(i);
                return;
            }
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
                        AddStat(StatType.MaxHealth, value);
                    }

                    AddStat(statType, value);
                }
            }
        }
    }

    public void AddStat(StatType type, float value)
    {
        if (!IsServer) return;

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

        for (int i = BaseStats.Count - 1; i >= 0; i--)
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

        for (int i = BaseStats.Count - 1; i >= 0; i--)
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

        for (int i = BaseStats.Count - 1; i >= 0; i--)
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

    private void FixedUpdate()
    {
        if (!IsServer) return;

        for (int i = 0; i < PowerUps.Count; i++)
        {
            var powerUpData = PowerUps[i];

            if (!powerUpData.PowerUpSo.IsPermanent && Time.fixedTime - powerUpData.CollectedTime > powerUpData.PowerUpSo.Duration)
            {

                foreach (var stat in powerUpData.StatsAdded)
                {
                    SubstractFromStat(stat.Type, stat.CurrentValue);
                }

                PowerUps.RemoveAt(i);
                i--;
            }
        }
    }
}
