using UnityEngine;

public class ResourceUsage : MonoBehaviour
{
    private BuildingSo buildingSo;
    private UnitSo unitSo;
    private float usageTimer = 0;
    private float usageInterval = 0;
    public bool isInDebt = false;

    private void Awake()
    {
        var selectable = GetComponent<Selectable>();

        if (selectable is null)
        {
            return;
        }

        if (selectable.selectableType == Selectable.SelectableType.Building)
        {
            var building = GetComponent<Building>();
            buildingSo = building.buildingSo;
            usageInterval = buildingSo.usageInterval;
        }
        else if (selectable.selectableType == Selectable.SelectableType.Unit)
        {
            var unit = GetComponent<Unit>();
            unitSo = unit.unitSo;
            usageInterval = unitSo.usageInterval;
        }
    }

    public void UseResources()
    {
        if (buildingSo is not null)
        {
            if (UIStorage.Instance.HasEnoughResource(buildingSo.resourceUsage, buildingSo.usage))
            {
                isInDebt = false;
                UIStorage.Instance.DecreaseResource(buildingSo.resourceUsage, buildingSo.usage);
            }
            else
            {
                isInDebt = true;
            }
        }
        else if (unitSo is not null)
        {
            if (UIStorage.Instance.HasEnoughResource(unitSo.resourceUsage, unitSo.usage))
            {
                isInDebt = false;
                UIStorage.Instance.DecreaseResource(unitSo.resourceUsage, unitSo.usage);
            }
            else
            {
                isInDebt = true;
            }
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
