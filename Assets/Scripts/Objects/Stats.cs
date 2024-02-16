using Unity.Netcode;
using UnityEngine;

public class Stats : NetworkBehaviour
{
    private Unit unit;
    private Building building;
    private NetworkList<Stat> stats = new NetworkList<Stat>();

    private void Start()
    {
        unit = GetComponent<Unit>();
        building = GetComponent<Building>();

        Debug.Log("Stats Start");
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
                Debug.Log("Stats Start " + stat.Type + " " + stat.Value);
                stats.Add(new Stat { Type = stat.Type, Value = stat.Value });
            }
        }

        Debug.Log("Stats Start " + stats.Count);
    }

    public float GetStat(StatType type)
    {

        Debug.Log("GetStat " + type + " " + stats.Count);
        for (int i = 0; i < stats.Count; i++)
        {
            Debug.Log("GetStat " + stats[i].Type + " " + stats[i].Value);
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
        Debug.Log("AddStat " + type + " " + value);
        stats.Add(new Stat { Type = type, Value = value });
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }
}
