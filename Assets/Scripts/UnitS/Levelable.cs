using Unity.Netcode;
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
        if (expirence.Value >= levelableSo.levels[level.Value].expirence)
        {
            LevelUp();
        }
    }

    public void LevelUp()
    {
        level.Value++;
        expirence.Value = 0;
        var levelData = levelableSo.levels[level.Value - 1];

        damagable.stats.AddToStat(StatType.MaxHealth, levelData.health);
        damagable.stats.AddToStat(StatType.Health, levelData.health);
        damagable.stats.AddToStat(StatType.Damage, levelData.attackDamage);

        expirenceToNextLevel.Value = levelData.expirence;
    }
}
