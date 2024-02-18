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
        uIStorage = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<PlayerController>().GetComponentInChildren<UIStorage>();

        Debug.Log("Income: " + uIStorage.OwnerClientId);

        uIStorage.IncreaseResource(resourceSo, income);
    }

    void Start()
    {
        stats = GetComponent<Stats>();
        incomeInterval = stats.GetStat(StatType.IncomeInterval);
    }

    void FixedUpdate()
    {
        if (!IsServer) return;
        incomeTimer -= Time.fixedDeltaTime;

        if (incomeTimer <= 0)
        {
            Income();
        }
    }
}
