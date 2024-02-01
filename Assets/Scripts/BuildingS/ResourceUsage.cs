using UnityEngine;

public class UsageData
{
    public ResourceSO resourceSO;
    public float usage;
}

public class ResourceUsage : MonoBehaviour
{
    private float usageTimer = 0;
    private float usageInterval = 0;
    public bool isInDebt = false;
    private Stats stats;
    private Building building;
    private Unit unit;

    private void Start()
    {
        building = GetComponent<Building>();
        unit = GetComponent<Unit>();
        stats = GetComponent<Stats>();
        usageInterval = stats.GetStat(StatType.UsageInterval);
    }

    private UsageData GetUsageDataFromStats()
    {
        var resourceSo = building != null ? building.buildingSo.resourceUsage : unit.unitSo.resourceUsage;

        var usageData = new UsageData
        {
            resourceSO = resourceSo,
            usage = stats.GetStat(StatType.Usage),
        };
        return usageData;
    }

    public void UseResources()
    {
        var usageData = GetUsageDataFromStats();

        if (UIStorage.Instance.HasEnoughResource(usageData.resourceSO, usageData.usage))
        {
            isInDebt = false;
            UIStorage.Instance.DecreaseResource(usageData.resourceSO, usageData.usage);
        }
        else
        {
            isInDebt = true;
        }
    }

    private void Update()
    {
        if (usageInterval == 0)
        {
            return;
        }

        if (usageTimer >= usageInterval)
        {
            UseResources();
            usageTimer = 0;
        }

        usageTimer += Time.deltaTime;
    }
}
