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
        if (level >= maxLevel || UIStorage.Instance.HasEnoughEnergy(buildingLevelableSo.levels[level].cost)) return;
        Debug.Log($"Level up to {buildingLevelableSo.levels[level].cost} level");
        level++;
        var levelData = buildingLevelableSo.levels[level - 1];
        Debug.Log($"Level up to {levelData.cost} level");
        UIStorage.Instance.DecreaseEnergy(levelData.cost);

        damagable.health += levelData.health;
        damagable.maxHealth += levelData.health;

        if (building.attackableSo != null) building.attackableSo.attackDamage += levelData.attackDamage;
        building.buildingSo.income += levelData.income;
        reduceSpawnTime += levelData.reduceSpawnTime;
        UpdateScreen();
    }
}
