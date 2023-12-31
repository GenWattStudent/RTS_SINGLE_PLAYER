using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingSo buildingSo;
    public AttackableSo attackableSo;
    public BuildingLevelable buildingLevelable;

    private void Start() {
        buildingLevelable = GetComponent<BuildingLevelable>();
    }

    public void Sell() {
        UIStorage.Instance.IncreaseResource(buildingSo.costResource, buildingSo.cost / 2);
        var damagableScript = GetComponent<Damagable>();
        damagableScript.TakeDamage(damagableScript.health);
    }
}
