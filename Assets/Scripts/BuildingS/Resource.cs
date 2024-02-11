using Unity.Netcode;
using UnityEngine;

public class Resource : NetworkBehaviour
{
    public BuildingSo buildingSo;
    public ResourceSO resourceSo;
    public float incomeInterval;
    public float incomeTimer;
    public float currentIncome = 0;
    public float totalIncome = 0;
    private Stats stats;
    private UIStorage uIStorage;

    private void Income()
    {
        var income = stats.GetStat(StatType.Income);

        currentIncome += income;
        incomeTimer = incomeInterval;
        totalIncome += income;
        uIStorage = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>().GetComponentInChildren<UIStorage>();
        uIStorage.IncreaseResource(resourceSo, income);
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
