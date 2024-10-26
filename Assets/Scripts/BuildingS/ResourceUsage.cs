using Unity.Netcode;
using UnityEngine;

public class UsageData
{
    public ResourceSO resourceSO;
    public float usage;
}

[DefaultExecutionOrder(1)]
public class ResourceUsage : NetworkBehaviour
{
    public bool isInDebt = false;
    public ResourceSO ResourceSO;

    private float usageTimer = 0;
    private float usageInterval = 0;
    private Stats stats;
    private Building building;
    private Unit unit;
    private UIStorage uIStorage;
    private InfoBox infoBox;

    private void Start()
    {
        building = GetComponent<Building>();
        unit = GetComponent<Unit>();
        stats = GetComponent<Stats>();
        infoBox = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponentInChildren<InfoBox>();
        usageInterval = stats.GetStat(StatType.UsageInterval);
        ResourceSO = building != null ? building.buildingSo.resourceUsage : unit.unitSo.resourceUsage;
        Debug.Log("UsageInterval: " + usageInterval);
        if (IsServer) uIStorage = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<PlayerController>().GetComponentInChildren<UIStorage>();
    }

    private UsageData GetUsageDataFromStats()
    {
        var usageData = new UsageData
        {
            resourceSO = ResourceSO,
            usage = stats.GetStat(StatType.Usage),
        };

        return usageData;
    }

    public void UseResources()
    {
        var usageData = GetUsageDataFromStats();
        if (usageData.resourceSO == null) return;

        Debug.Log($"UseResources {usageData.resourceSO.resourceName} {usageData.usage} {OwnerClientId}");
        var clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { OwnerClientId }
            }
        };

        if (uIStorage.HasEnoughResource(usageData.resourceSO, usageData.usage))
        {
            isInDebt = false;
            uIStorage.DecreaseResource(usageData.resourceSO, usageData.usage);
            UserDebtEndClientRpc(clientRpcParams);
        }
        else
        {
            isInDebt = true;
            UserDebtClientRpc(clientRpcParams);
        }
    }

    [ClientRpc]
    public void UserDebtClientRpc(ClientRpcParams clientRpcParams = default)
    {
        isInDebt = true;
        infoBox.AddError($"Not enough {ResourceSO.resourceName}!");
    }

    [ClientRpc]
    public void UserDebtEndClientRpc(ClientRpcParams clientRpcParams = default)
    {
        isInDebt = false;
    }

    private void Update()
    {
        if (!IsServer) return;

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
