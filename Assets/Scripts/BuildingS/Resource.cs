using UnityEngine;

public class Resource : MonoBehaviour
{
    public BuildingSo buildingSo;
    public ResourceSO resourceSo;
    public float incomeInterval;
    public float incomeTimer;
    public float currentIncome = 0;
    public float totalIncome = 0;
    private Stats stats;

    private void Income()
    {
        var income = stats.GetStat(StatType.Income);

        currentIncome += income;
        incomeTimer = incomeInterval;
        totalIncome += income;
        UIStorage.Instance.IncreaseResource(resourceSo, income);
    }

    void Start()
    {
        stats = GetComponent<Stats>();
        incomeInterval = stats.GetStat(StatType.IncomeInterval);
    }

    void FixedUpdate()
    {
        incomeTimer -= Time.fixedDeltaTime;

        if (incomeTimer <= 0)
        {
            Income();
        }
    }
}
