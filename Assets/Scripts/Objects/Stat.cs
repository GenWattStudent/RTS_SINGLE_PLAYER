using System;
using Unity.Netcode;

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
public struct Stat : INetworkSerializable, IEquatable<Stat>
{
    public StatType Type;
    public float Value;

    public bool Equals(Stat other)
    {
        return Type == other.Type;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Type);
        serializer.SerializeValue(ref Value);
    }
}


