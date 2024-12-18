using Unity.Netcode;
using UnityEngine;
using static LevelableSo;

public class Levelable : NetworkBehaviour
{
    public LevelableSo levelableSo;
    public NetworkVariable<int> level = new(1);
    public NetworkVariable<int> expirence = new(0);
    public NetworkVariable<int> expirenceToNextLevel = new(0);

    public int maxLevel => levelableSo.levels.Count;
    public Level curentLevel => levelableSo.levels[level.Value];
    private Damagable damagable;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        level.OnValueChanged += HandleLevelUp;
    }

    private void HandleLevelUp(int prev, int current)
    {
        if (damagable.unitScript.unitSo.levelUpPrefab != null)
        {
            var levelUp = Instantiate(damagable.unitScript.unitSo.levelUpPrefab, transform.position, Quaternion.identity);
            levelUp.transform.SetParent(transform);

            Destroy(levelUp, 2f);
        }
    }

    private void Start()
    {
        damagable = GetComponent<Damagable>();

        if (levelableSo.levels.Count > 0 && IsServer)
        {
            expirenceToNextLevel.Value = levelableSo.levels[level.Value].expirence;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddExpirenceServerRpc(int amount)
    {
        if (level.Value >= maxLevel) return;

        expirence.Value += amount;

        while (level.Value < maxLevel && expirence.Value >= levelableSo.levels[level.Value].expirence)
        {
            expirence.Value -= levelableSo.levels[level.Value].expirence;
            LevelUp();
        }
    }

    public void LevelUp()
    {
        var levelData = levelableSo.levels[level.Value];
        level.Value++;

        foreach (var stat in levelData.stats)
        {
            damagable.stats.AddToStat(stat.Type, stat.BaseValue);
        }

        if (level.Value < maxLevel)
        {
            expirenceToNextLevel.Value = levelableSo.levels[level.Value].expirence;
        }
    }
}
