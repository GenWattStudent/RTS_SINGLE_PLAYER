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
    private Stats stats;

    private void Start()
    {
        building = GetComponent<Building>();
        damagable = GetComponent<Damagable>();
        stats = GetComponent<Stats>();
        screenController = GetComponentInChildren<ScreenController>();
    }

    public void UpdateScreen()
    {
        if (screenController == null) return;

        screenController.SetText($"{level} lvl");
    }

    public BuildingLevelableSo.BuildingLevel GetNextBuildingLevel()
    {
        if (level >= maxLevel) return null;
        return buildingLevelableSo.levels[level];
    }

    private void InstantiateLevelUpEffects()
    {
        for (int i = 0; i < 5; i++)
        {
            var effect = Instantiate(building.buildingSo.levelUpEffect, transform.position, Quaternion.identity);
            effect.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            Destroy(effect, 1f);
        }
    }

    private void UpdateSpawner(BuildingLevelableSo.BuildingLevel buildingLevel)
    {
        var spawner = GetComponent<ISpawnerBuilding>();
        if (spawner != null) reduceSpawnTime += buildingLevel.reduceSpawnTime;
    }

    private void UpdateMiner(BuildingLevelableSo.BuildingLevel buildingLevel)
    {
        if (stats != null) stats.AddToStat(StatType.Income, buildingLevel.income);
    }

    private void UpdateHealth(BuildingLevelableSo.BuildingLevel buildingLevel)
    {
        if (damagable != null)
        {
            damagable.stats.AddToStat(StatType.MaxHealth, buildingLevel.health);
            damagable.TakeDamage(-buildingLevel.health);
        }
    }

    private void UpdateAttack(BuildingLevelableSo.BuildingLevel buildingLevel)
    {
        if (building.attackableSo != null) damagable.stats.AddToStat(StatType.Damage, buildingLevel.attackDamage);
    }

    public void LevelUp()
    {
        if (level >= maxLevel || !UIStorage.Instance.HasEnoughResource(building.buildingSo.costResource, buildingLevelableSo.levels[level].cost)) return;

        level++;
        var levelData = buildingLevelableSo.levels[level - 1];
        UIStorage.Instance.DecreaseResource(building.buildingSo.costResource, levelData.cost);

        UpdateHealth(levelData);
        UpdateAttack(levelData);
        UpdateMiner(levelData);
        UpdateSpawner(levelData);

        UpdateScreen();
        InstantiateLevelUpEffects();
    }
}
