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
    public bool IsBot = false;
    public float damage = 0f;
    public Stats stats;
    public NetworkVariable<TeamType> teamType = new(TeamType.None);
    public Unit unitScript;

    public event Action OnDead;
    public event Action OnTakeDamage;

    public bool IsTeamMate(Damagable damagable) => teamType.Value != TeamType.None && teamType.Value == damagable.teamType.Value;
    public bool CanAttack(Damagable damagable, Unit unit) => damagable != null && !IsTeamMate(damagable) &&
        !damagable.isDead && unit.isVisibile;

    public void AddDamageBoost(float boost)
    {
        if (damagableSo.bulletSo == null) return;

        damageBoost += boost;
        var damageToAdd = damagableSo.bulletSo.GetStat(StatType.Damage) * boost / 100;
        stats.AddToStat(StatType.Damage, damageToAdd);
    }

    public void AddHealthBoost(float boost)
    {
        var newHealth = stats.GetStat(StatType.MaxHealth) * boost / 100;
        stats.AddToStat(StatType.MaxHealth, newHealth);
        stats.AddToStat(StatType.Health, newHealth);
    }

    private void Awake()
    {
        stats = GetComponent<Stats>();
        progressBarScript = healthBar.GetComponent<ProgresBar>();
        levelable = GetComponent<Levelable>();
        unitScript = GetComponent<Unit>();
    }

    private void Start()
    {
        if (IsServer)
        {
            stats.AddStat(StatType.Health, stats.GetStat(StatType.MaxHealth));
            if (damagableSo.bulletSo != null) stats.AddStat(StatType.Damage, damagableSo.bulletSo.GetStat(StatType.Damage));
            var powerUp = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponentInChildren<PowerUp>();

            // add damage boost and helth boost
            if (powerUp != null)
            {
                var unit = GetComponent<Unit>();
                if (unit.GetComponent<Building>() != null) return;

                var damagePercent = powerUp.GetPercentAmountOfByUnitName(unit.unitSo.unitName, "damage");
                var healthPercent = powerUp.GetPercentAmountOfByUnitName(unit.unitSo.unitName, "health");

                AddDamageBoost(damagePercent);
                AddHealthBoost(healthPercent);
            }

            SetHealthClientRpc(stats.GetStat(StatType.Health), stats.GetStat(StatType.MaxHealth));

            // TakeDamage(2000);
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
        DeathClientRpc();
        no.Despawn(true);
    }

    public override void OnDestroy()
    {
        DeathClientRpc();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
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
