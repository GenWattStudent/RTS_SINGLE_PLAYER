using System;
using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(2)]
public class Damagable : NetworkBehaviour
{
    [SerializeField] private RectTransform healthBar;
    public DamagableSo damagableSo;
    private ProgresBar progressBarScript;
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

    private void Awake()
    {
        stats = GetComponent<Stats>();
        progressBarScript = healthBar.GetComponent<ProgresBar>();
        levelable = GetComponent<Levelable>();
    }

    void Start()
    {
        if (IsServer)
        {
            stats.AddStat(StatType.Health, stats.GetStat(StatType.MaxHealth));
            if (damagableSo.bulletSo != null) stats.AddStat(StatType.Damage, damagableSo.bulletSo.GetStat(StatType.Damage));
            SetHealthClientRpc(stats.GetStat(StatType.Health), stats.GetStat(StatType.MaxHealth));
        }
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
        levelable.AddExpirenceServerRpc(exp);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeathServerRpc()
    {
        var no = GetComponent<NetworkObject>();
        no.Despawn(true);
        DeathClientRpc();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        Console.WriteLine("OnNetworkDespawn");
        OnDead?.Invoke();
    }

    [ClientRpc]
    private void DeathClientRpc()
    {
        isDead = true;
        InstantiateExplosion();
        InstantiateDestroyedObject();
    }

    [ClientRpc]
    public void SetHealthClientRpc(float health, float maxHealth)
    {
        progressBarScript.UpdateProgresBar(health, maxHealth);
    }

    public bool TakeDamage(float damage)
    {
        var newHealth = stats.SubstractFromStat(StatType.Health, damage);
        var maxHealth = stats.GetStat(StatType.MaxHealth);

        if (newHealth > maxHealth)
        {
            stats.SetStat(StatType.Health, maxHealth);
        }

        OnTakeDamage?.Invoke();

        if (newHealth <= 0f)
        {
            DeathServerRpc();
            return true;
        }

        SetHealthClientRpc(newHealth, maxHealth);

        return false;
    }
}
