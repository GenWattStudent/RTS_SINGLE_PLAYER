using Unity.Netcode;
using UnityEngine;

public class Building : NetworkBehaviour
{
    public BuildingSo buildingSo;
    public AttackableSo attackableSo;
    public BuildingLevelable buildingLevelable;

    private void Start()
    {
        buildingLevelable = GetComponent<BuildingLevelable>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SellServerRpc()
    {
        var uIStorage = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<PlayerController>().GetComponentInChildren<UIStorage>();
        Debug.Log("SellServerRpc " + uIStorage.OwnerClientId);
        uIStorage.IncreaseResource(buildingSo.costResource, buildingSo.cost / 2);
        var damagableScript = GetComponent<Damagable>();
        damagableScript.TakeDamage(damagableScript.stats.GetStat(StatType.Health));
    }
}
