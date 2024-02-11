using Unity.Netcode;
using UnityEngine;

public class Building : NetworkBehaviour
{
    public BuildingSo buildingSo;
    public AttackableSo attackableSo;
    public BuildingLevelable buildingLevelable;
    private UIStorage uIStorage;

    private void Start()
    {
        buildingLevelable = GetComponent<BuildingLevelable>();
        uIStorage = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>().GetComponentInChildren<UIStorage>();
    }

    public void Sell()
    {
        uIStorage.IncreaseResource(buildingSo.costResource, buildingSo.cost / 2);
        var damagableScript = GetComponent<Damagable>();
        damagableScript.TakeDamage(damagableScript.stats.GetStat(StatType.Health));
    }
}
