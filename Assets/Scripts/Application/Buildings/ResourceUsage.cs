using System;
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

    public event Action<bool> OnDebtChanged;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            uIStorage = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<PlayerController>().GetComponentInChildren<UIStorage>();
            uIStorage.OnStoragesChanged += HandleStoragesChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            uIStorage.OnStoragesChanged -= HandleStoragesChanged;
        }
    }

    private void Start()
    {
        building = GetComponent<Building>();
        unit = GetComponent<Unit>();
        stats = GetComponent<Stats>();
        infoBox = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponentInChildren<InfoBox>();
        usageInterval = stats.GetStat(StatType.UsageInterval);
        ResourceSO = building != null ? building.buildingSo.resourceUsage : unit.unitSo.resourceUsage;
    }

    private void HandleStoragesChanged()
    {
        if (isInDebt && uIStorage.HasEnoughResource(ResourceSO, stats.GetStat(StatType.Usage)))
        {
            UseResources();
        }
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

        var clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { OwnerClientId }
            }
        };

        if (uIStorage.HasEnoughResource(usageData.resourceSO, usageData.usage))
        {
            HandleDebtEnd();
            isInDebt = false;
            uIStorage.DecreaseResource(usageData.resourceSO, usageData.usage);
            OnDebtChanged?.Invoke(false);
            UserDebtEndClientRpc(clientRpcParams);
        }
        else
        {
            HandleDebt();
            isInDebt = true;
            OnDebtChanged?.Invoke(true);
            UserDebtClientRpc(clientRpcParams);
        }
    }

    private void HandleDebtEnd()
    {
        if (building != null || !isInDebt)
        {
            return;
        }

        if (unit.unitSo.debuff != null)
        {
            stats.RemovePowerUp(unit.unitSo.debuff);
        }
    }

    private void HandleDebt()
    {
        if (building != null || isInDebt)
        {
            return;
        }

        if (unit.unitSo.debuff != null)
        {
            stats.AddPowerUp(unit.unitSo.debuff);
        }

        if (unit.unitSo.debuffEffect != null)
        {
            DebuffEffectClientRpc();
        }
    }

    [ClientRpc]
    private void DebuffEffectClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (unit.unitSo.debuffEffect != null)
        {
            var debuffEffect = Instantiate(unit.unitSo.debuffEffect, transform.position, transform.rotation);
            debuffEffect.transform.SetParent(transform);
            Destroy(debuffEffect, 2.4f);
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
