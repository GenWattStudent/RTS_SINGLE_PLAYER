using System;
using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(2)]
public class Damagable : NetworkBehaviour
{
    [HideInInspector] public NetworkVariable<TeamType> teamType = new(TeamType.None);
    public DamagableSo damagableSo;
    public Levelable levelable;
    public Transform TargetPoint;
    public bool IsBot = false;
    public Stats stats;
    [HideInInspector] public NetworkVariable<bool> isDead = new(false);
    public Unit unitScript;

    private ProgresBar progressBarScript;

    public event Action<Damagable> OnDead;
    public event Action OnTakeDamage;

    public bool IsTeamMate(Damagable damagable) => teamType.Value != TeamType.None && teamType.Value == damagable.teamType.Value;
    public bool CanAttack(Damagable damagable) => damagable != null && !IsTeamMate(damagable) &&
        !damagable.isDead.Value && damagable.unitScript.isVisibile.Value;

    private void Awake()
    {
        stats = GetComponent<Stats>();
        progressBarScript = GetComponentInChildren<ProgresBar>();
        levelable = GetComponent<Levelable>();
        unitScript = GetComponent<Unit>();
        var pos = FindChildByName(transform, "TargetPoint");

        if (pos != null) TargetPoint = pos;
        else TargetPoint = transform;
    }

    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }
            Transform result = FindChildByName(child, name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    private void Start()
    {
        if (IsServer)
        {
            if (damagableSo.bulletSo != null) stats.AddStat(StatType.Damage, damagableSo.bulletSo.GetStat(StatType.Damage));
            var powerUp = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponentInChildren<PowerUp>();

            // add damage boost and helth boost
            if (powerUp != null)
            {
                ISkillApplicable skillApplicable = unitScript.unitSo ? unitScript : unitScript.GetComponent<Building>();
                powerUp.ApplySkills(skillApplicable);
            }

            // TakeDamage(100);
        }

        isDead.OnValueChanged += HandleDeadState;
        SetHealth(stats.GetStat(StatType.Health), stats.GetStat(StatType.MaxHealth));
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        stats.BaseStats.OnListChanged += HandleBaseStatsChange;
    }

    private void HandleBaseStatsChange(NetworkListEvent<Stat> changeEvent)
    {
        SetHealth(stats.GetStat(StatType.Health), stats.GetStat(StatType.MaxHealth));
    }

    private void HandleDeadState(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            DeathClientRpc();
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
        isDead.Value = true;
        no.Despawn(true);
    }

    [ClientRpc]
    private void DeathClientRpc()
    {
        OnDead?.Invoke(this);
        InstantiateExplosion();
        InstantiateDestroyedObject();
    }

    public void SetHealth(float health, float maxHealth)
    {
        progressBarScript.UpdateProgresBar(health, maxHealth);
    }

    public bool TakeDamage(float damage)
    {
        if (!IsServer) return false;

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

        return false;
    }
}
