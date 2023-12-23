using UnityEngine;

public class BuildingLevelable : MonoBehaviour
{
    public BuildingLevelableSo buildingLevelableSo;
    public int level = 1;
    public int maxLevel => buildingLevelableSo.levels.Count;
    public Building building;
    public Damagable damagable;
    public int reduceSpawnTime = 0;

    private void Start() {
        building = GetComponent<Building>();
        damagable = GetComponent<Damagable>();
    }

    public void LevelUp() {
        if (level >= maxLevel && UIStorage.Instance.HasEnoughEnergy(buildingLevelableSo.levels[level + 1].cost)) return;

        level++;
        var levelData = buildingLevelableSo.levels[level];
        Debug.Log($"Level up to {levelData.cost} level");
        UIStorage.Instance.DecreaseEnergy(levelData.cost);

        damagable.health += levelData.health;
        if (building.attackableSo != null) building.attackableSo.attackDamage += levelData.attackDamage;
        building.buildingSo.income += levelData.income;
        reduceSpawnTime += levelData.reduceSpawnTime;
    }
}
