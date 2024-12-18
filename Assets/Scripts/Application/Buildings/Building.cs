using Unity.Netcode;
using UnityEngine;

public class Building : NetworkBehaviour, ISkillApplicable
{
    public BuildingSo buildingSo;
    public AttackableSo attackableSo;
    public BuildingLevelable buildingLevelable;

    public Stats Stats => GetComponent<Stats>();
    public string Name => buildingSo.buildingName;

    private void Start()
    {
        buildingLevelable = GetComponent<BuildingLevelable>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) return;

        var rtsObjectManager = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<RTSObjectsManager>();
        rtsObjectManager.AddLocalBuilding(this);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsOwner) return;

        var rtsObjectManager = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<RTSObjectsManager>();
        rtsObjectManager.RemoveLocalBuilding(this);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SellServerRpc()
    {
        var uIStorage = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponentInChildren<UIStorage>();
        var damagableScript = GetComponent<Damagable>();

        uIStorage.IncreaseResource(buildingSo.costResource, buildingSo.cost / 2);
        damagableScript.TakeDamage(damagableScript.stats.GetStat(StatType.MaxHealth));
    }
}
