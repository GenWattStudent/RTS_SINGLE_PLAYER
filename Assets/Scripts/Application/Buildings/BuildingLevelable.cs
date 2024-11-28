using Unity.Netcode;
using UnityEngine;

public class BuildingLevelable : NetworkBehaviour
{
    public BuildingLevelableSo buildingLevelableSo;
    public NetworkVariable<int> level = new(1);
    public NetworkVariable<int> reduceSpawnTime = new(0);
    public int maxLevel => buildingLevelableSo == null ? 1 : buildingLevelableSo.levels.Count;
    public Building building;
    public Damagable damagable;

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
        if (level.Value >= maxLevel) return null;
        return buildingLevelableSo.levels[level.Value];
    }

    [ClientRpc]
    private void InstantiateLevelUpEffectsClientRpc()
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
        if (spawner != null) reduceSpawnTime.Value += buildingLevel.reduceSpawnTime;
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

    [ServerRpc(RequireOwnership = false)]
    public void LevelUpServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var uIStorage = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject.GetComponent<PlayerController>().GetComponentInChildren<UIStorage>();
        if (level.Value >= maxLevel || !uIStorage.HasEnoughResource(building.buildingSo.costResource, buildingLevelableSo.levels[level.Value].cost)) return;

        level.Value++;
        var levelData = buildingLevelableSo.levels[level.Value - 1];
        uIStorage.DecreaseResource(building.buildingSo.costResource, levelData.cost);

        UpdateHealth(levelData);
        UpdateAttack(levelData);
        UpdateMiner(levelData);
        UpdateSpawner(levelData);

        UpdateScreen();
        InstantiateLevelUpEffectsClientRpc();
    }
}
