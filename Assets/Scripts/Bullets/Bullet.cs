using System;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Motion))]
public class Bullet : MonoBehaviour
{
    public float damage;
    public Guid playerId;
    public BulletSo bulletSo;
    public Damagable unitsBullet;
    public ObjectPool<Bullet> pool;
    private float lifeTimeTimer = 0f;
    private TrailRenderer trailRenderer;
    public Motion motion;

    private void Awake()
    {
        trailRenderer = GetComponentInChildren<TrailRenderer>();
        motion = GetComponent<Motion>();
    }

    private void Explode()
    {
        if (bulletSo.explosionPrefab != null)
        {
            var explosion = Instantiate(bulletSo.explosionPrefab, motion.previousPosition, Quaternion.identity);
            MusicManager.Instance.PlayMusic(bulletSo.explosionSound, motion.previousPosition);
            Destroy(explosion, 2f);
        }

        Collider[] colliders = Physics.OverlapSphere(motion.previousPosition, bulletSo.radius);
        foreach (var collider in colliders)
        {
            if (IsOwnUnit(collider)) continue;
            DealDamage(collider);
        }
    }

    private void DealDamage(Collider collider)
    {
        var damageableScript = collider.gameObject.GetComponent<Damagable>();

        if (damageableScript != null && damageableScript.playerId != playerId)
        {
            if (damageableScript.TakeDamage(damage))
            {
                unitsBullet.AddExpiernce(damageableScript.damagableSo.deathExpirence);
                if (unitsBullet.playerId == PlayerController.playerId)
                {
                    PlayerController.Instance.AddExpiernce(damageableScript.damagableSo.deathExpirence);
                }
            }
        }
    }

    private bool IsOwnUnit(RaycastHit hit)
    {
        var damagable = hit.collider.gameObject.GetComponent<Damagable>();
        return damagable != null && !damagable.isDead && damagable.playerId == playerId;
    }

    private bool IsOwnUnit(Collider collider)
    {
        var damagable = collider.gameObject.GetComponent<Damagable>();
        return damagable != null && !damagable.isDead && damagable.playerId == playerId;
    }

    private void CheckHit()
    {
        RaycastHit hit;
        var direction = transform.position - motion.previousPosition;

        if (Physics.Raycast(motion.previousPosition, direction.normalized, out hit, direction.magnitude))
        {

            if (LayerMask.LayerToName(hit.collider.gameObject.layer) == "Bush" || LayerMask.LayerToName(hit.collider.gameObject.layer) == "Ghost")
            {
                return;
            }

            if (IsOwnUnit(hit)) return;

            if (bulletSo.radius > 0)
            {
                Explode();
            }
            else
            {
                DealDamage(hit.collider);
            }

            pool.Release(this);
        }
    }

    public void Setup()
    {
        damage = unitsBullet.stats.GetStat(StatType.Damage);
        motion.speed = bulletSo.GetStat(StatType.Speed);
    }

    public void Reset()
    {
        lifeTimeTimer = 0f;
        motion.previousPosition = transform.position;
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }

    void Update()
    {
        lifeTimeTimer += Time.deltaTime;
        motion.Move();
        CheckHit();

        if (lifeTimeTimer > bulletSo.lifeTime)
        {
            Explode();
            pool.Release(this);
        }
    }
}
