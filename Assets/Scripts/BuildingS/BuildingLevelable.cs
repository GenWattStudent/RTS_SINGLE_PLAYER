using UnityEngine;

public class BuildingLevelable : MonoBehaviour
{
    public BuildingLevelableSo buildingLevelableSo;
    public int level = 1;
    public int maxLevel => buildingLevelableSo.levels.Count;
    public Building building;
    public Damagable damagable;
    public int reduceSpawnTime = 0;
    private ScreenController screenController;

    private void Start() {
        building = GetComponent<Building>();
        damagable = GetComponent<Damagable>();
        screenController = GetComponentInChildren<ScreenController>();
    }

    public void UpdateScreen() {
        if (screenController == null) return;

        screenController.SetText($"{level} lvl");
    }

    public void LevelUp() {
        Debug.Log($"Level up to {level} level {building.buildingSo.costResource}  {buildingLevelableSo.levels[level].cost}");
        if (level >= maxLevel || !UIStorage.Instance.HasEnoughResource(building.buildingSo.costResource, buildingLevelableSo.levels[level].cost)) return;
        Debug.Log($"Level up to {buildingLevelableSo.levels[level].cost} level");
        level++;
        var levelData = buildingLevelableSo.levels[level - 1];
        Debug.Log($"Level up to {levelData.cost} level");
        UIStorage.Instance.DecreaseResource(building.buildingSo.costResource, levelData.cost);

        damagable.health += levelData.health;
        damagable.maxHealth += levelData.health;

        if (building.attackableSo != null) building.attackableSo.attackDamage += levelData.attackDamage;
        building.buildingSo.income += levelData.income;
        reduceSpawnTime += levelData.reduceSpawnTime;
        UpdateScreen();
    }
}
