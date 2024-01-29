using System;

public enum StatType
{
    Health,
    MaxHealth,
    Damage,
    AttackSpeed,
    AttackRange,
    Speed,
    BuildingDistance,
    SpawnTime,
    Cost,
    Usage,
    UsageInterval,
    Income,
    IncomeInterval,
    Accuracy,
    Radius,
    AttackCooldown,
    Acceleration,
    DeathExperience,
}

[Serializable]
public class Stat
{
    public StatType Type;
    public float Value;
}
