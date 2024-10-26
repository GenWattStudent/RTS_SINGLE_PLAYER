using Unity.Netcode;
using UnityEngine;

public class Resource : NetworkBehaviour
{
    public BuildingSo buildingSo;
    public ResourceSO resourceSo;
    public float incomeTimer;
    public float currentIncome = 0;
    public float totalIncome = 0;

    private Stats stats;
    private UIStorage uIStorage;
    private Building building;

    private void Income()
    {
        var income = stats.GetStat(StatType.Income);

        currentIncome += income;
        incomeTimer = stats.GetStat(StatType.IncomeInterval);
        totalIncome += income;
        uIStorage = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<PlayerController>().GetComponentInChildren<UIStorage>();

        uIStorage.IncreaseResource(resourceSo, income);
    }

    private void Start()
    {
        stats = GetComponent<Stats>();
        building = GetComponent<Building>();
        incomeTimer = stats.GetStat(StatType.IncomeInterval);
        resourceSo = building.buildingSo.incomeResource;
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;
        incomeTimer -= Time.fixedDeltaTime;

        if (incomeTimer <= 0)
        {
            Income();
        }
    }
}
