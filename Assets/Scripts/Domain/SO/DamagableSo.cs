using UnityEngine;

public class DamagableSo : AttackableSo
{
    [Header("Health options")]
    public float health = 100f;
    public int deathExpirence = 10;

    [Header("Death Effect options")]
    public GameObject deathEffect;
    public float deathEffectTime = 2f;
    public GameObject explosionPrefab;
}
