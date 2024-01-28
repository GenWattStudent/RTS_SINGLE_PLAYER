using System;
using UnityEngine;

public class Damagable : MonoBehaviour
{
    public DamagableSo damagableSo;
    public float health;
    public float maxHealth;
    [SerializeField] private RectTransform healthBar;
    private ProgresBar progressBarScript;
    public Guid playerId;
    public Levelable levelable;

    // On dead event handler
    public event Action OnDead;
    public event Action OnTakeDamage;
    public GameObject targetPoint;
    public bool isDead = false;
    public float damageBoost = 0;
    public float damage = 0f;

    public void AddDamageBoost(float boost) {
        damageBoost += boost;
        var damageToAdd = damagableSo.bulletSo.damage * boost / 100;
        damage += damageToAdd;
    }

    void Awake()
    {
        health = damagableSo.health;
        maxHealth = damagableSo.health;
        if (damagableSo.bulletSo) {
            damage = damagableSo.bulletSo.damage;
        }

        progressBarScript = healthBar.GetComponent<ProgresBar>();
        levelable = GetComponent<Levelable>();

        TakeDamage(40);
    }

    private void InstantiateExplosion() {
        if (damagableSo.explosionPrefab != null) {
            var explosion = Instantiate(damagableSo.explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 2.4f);
        }
    }

    private void InstantiateDestroyedObject() {
        if (damagableSo.deathEffect != null) {
            var destroyedObject = Instantiate(damagableSo.deathEffect, transform.position, transform.rotation);
            Destroy(destroyedObject, 2.4f);
        }
    }

    public void AddExpiernce(int exp) {
        if (levelable == null) return;
        levelable.AddExpirence(exp);
    }

    public bool TakeDamage(float damage) {
        health -= damage;
        progressBarScript.UpdateProgresBar(health, maxHealth);
        OnTakeDamage?.Invoke();
        
        if (health <= 0f) {
            isDead = true;
            InstantiateExplosion(); 
            InstantiateDestroyedObject();
            OnDead?.Invoke();
            Destroy(gameObject);
            return true;
        }

        return false;
    }

    public void Heal(float amount) {
        health += amount;
        if (health > maxHealth) {
            health = maxHealth;
        }
        progressBarScript.UpdateProgresBar(health, maxHealth);
    }
}
