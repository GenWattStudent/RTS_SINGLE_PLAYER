using System;
using UnityEngine;

public class Damagable : MonoBehaviour
{
    [SerializeField] private RectTransform healthBar;
    public DamagableSo damagableSo;
    private ProgresBar progressBarScript;
    public Guid playerId;
    public Levelable levelable;
    public GameObject targetPoint;
    public bool isDead = false;
    public float damageBoost = 0;
    public float damage = 0f;
    public Stats stats;

    public event Action OnDead;
    public event Action OnTakeDamage;

    public void AddDamageBoost(float boost)
    {
        damageBoost += boost;
        var damageToAdd = damagableSo.bulletSo.GetStat(StatType.Damage) * boost / 100;
        stats.AddToStat(StatType.Damage, damageToAdd);
    }

    void Start()
    {
        stats = GetComponent<Stats>();

        if (damagableSo.bulletSo != null) stats.AddStat(StatType.Damage, damagableSo.bulletSo.GetStat(StatType.Damage));
        stats.AddStat(StatType.Health, stats.GetStat(StatType.MaxHealth));

        progressBarScript = healthBar.GetComponent<ProgresBar>();
        levelable = GetComponent<Levelable>();

        // TakeDamage(40);
    }

    private void InstantiateExplosion()
    {
        if (damagableSo.explosionPrefab != null)
        {
            var explosion = Instantiate(damagableSo.explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 2.4f);
        }
    }

    private void InstantiateDestroyedObject()
    {
        if (damagableSo.deathEffect != null)
        {
            var destroyedObject = Instantiate(damagableSo.deathEffect, transform.position, transform.rotation);
            Destroy(destroyedObject, 2.4f);
        }
    }

    public void AddExpiernce(int exp)
    {
        if (levelable == null) return;
        levelable.AddExpirence(exp);
    }

    public bool TakeDamage(float damage)
    {
        var newHealth = stats.SubstractFromStat(StatType.Health, damage);
        Debug.Log($"New health: {newHealth}");
        var maxHealth = stats.GetStat(StatType.MaxHealth);

        if (newHealth > maxHealth)
        {
            newHealth = maxHealth;
        }

        progressBarScript.UpdateProgresBar(newHealth, maxHealth);
        OnTakeDamage?.Invoke();

        if (newHealth <= 0f)
        {
            isDead = true;
            InstantiateExplosion();
            InstantiateDestroyedObject();
            OnDead?.Invoke();
            Destroy(gameObject);
            return true;
        }

        return false;
    }
}
