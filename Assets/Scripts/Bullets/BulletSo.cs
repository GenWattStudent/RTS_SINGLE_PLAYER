using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Bullet", menuName = "ScriptableObjects/Bullet")]
public class BulletSo : ScriptableObject
{
    public string bulletName;
    public float arcHeight;
    public GameObject prefab;
    public float lifeTime;

    [Header("Explosion")]
    public GameObject explosionPrefab;
    public GameObject initialExplosionPrefab;
    public MusicManager.MusicType explosionSound;

    public float radius;
    public float damage;
    public float speed;

    public List<Stat> stats = new();

    public float GetStat(StatType type)
    {
        foreach (var stat in stats)
        {
            if (stat.Type == type)
            {
                return stat.Value;
            }
        }

        return -1;
    }
}
