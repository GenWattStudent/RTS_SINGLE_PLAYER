using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class Stats : NetworkBehaviour
{
    private Unit unit;
    private Building building;
    private NetworkList<Stat> stats;

    private void Awake()
    {
        stats = new NetworkList<Stat>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        unit = GetComponent<Unit>();
        building = GetComponent<Building>();

        if (!IsServer) return;

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

        for (int i = 0; i < stats.Count; i++)
        {
            if (stats[i].Type == type)
            {
                return stats[i].Value;
            }
        }
        return -1;
    }

    public void SetStat(StatType type, float value)
    {
        if (!IsServer) return;
        for (int i = 0; i < stats.Count; i++)
        {
            if (stats[i].Type == type)
            {
                var stat = stats[i];
                stat.Value = value;
                stats[i] = stat;
                return;
            }
        }
    }

    public float AddToStat(StatType type, float value)
    {
        if (!IsServer) return -1;
        for (int i = 0; i < stats.Count; i++)
        {
            if (stats[i].Type == type)
            {
                var stat = stats[i];
                stat.Value += value;
                stats[i] = stat;
                return stat.Value;
            }
        }

        return -1;
    }

    public float SubstractFromStat(StatType type, float value)
    {
        if (!IsServer) return -1;

        for (int i = 0; i < stats.Count; i++)
        {
            if (stats[i].Type == type)
            {
                var stat = stats[i];
                stat.Value -= value;
                stats[i] = stat;
                return stat.Value;
            }
        }
        return -1;
    }

    public void AddStat(StatType type, float value)
    {
        if (!IsServer) return;

        for (int i = 0; i < stats.Count; i++)
        {
            if (stats[i].Type == type)
            {
                return;
            }
        }

        stats.Add(new Stat { Type = type, Value = value });
    }

    public override void OnDestroy()
    {
        stats.Dispose();
    }
}
