using UnityEngine;
using static LevelableSo;

public class Levelable : MonoBehaviour
{
    public LevelableSo levelableSo;
    public int level = 1;
    public int expirence = 0;
    public int maxLevel => levelableSo.levels.Count;
    public int expirenceToNextLevel;
    public Level curentLevel => levelableSo.levels[level];
    private Damagable damagable;

    private void Start()
    {
        damagable = GetComponent<Damagable>();

        if (levelableSo.levels.Count > 0)
        {
            expirenceToNextLevel = levelableSo.levels[level].expirence;
        }
    }

    public void AddExpirence(int amount)
    {
        expirence += amount;
        if (level >= maxLevel) return;

        if (expirence >= levelableSo.levels[level].expirence)
        {
            LevelUp();
        }
    }

    public void LevelUp()
    {
        level++;
        expirence = 0;
        var levelData = levelableSo.levels[level - 1];

        damagable.stats.AddToStat(StatType.MaxHealth, levelData.health);
        damagable.stats.AddToStat(StatType.Health, levelData.health);
        damagable.stats.AddToStat(StatType.Damage, levelData.attackDamage);

        expirenceToNextLevel = levelData.expirence;
    }
}
