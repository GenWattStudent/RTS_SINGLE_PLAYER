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

    private void Awake() {
        trailRenderer = GetComponentInChildren<TrailRenderer>();
        motion = GetComponent<Motion>();
        damage = bulletSo.damage;
        motion.speed = bulletSo.speed;
        motion.arcHeight = bulletSo.arcHeight;
    }

    private void Explode() {
        if (bulletSo.explosionPrefab != null)  {
            var explosion = Instantiate(bulletSo.explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 1f);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, bulletSo.radius);
        foreach (var collider in colliders) {
            if (IsOwnUnit(collider)) continue;
            DealDamage(collider);
        }
    }

    private void DealDamage(Collider collider) {
        var damegableScript = collider.gameObject.GetComponent<Damagable>();

        if (damegableScript != null && damegableScript.playerId != playerId) {
            if (damegableScript.TakeDamage(damage)) {
                unitsBullet.AddExpiernce(damegableScript.damagableSo.deathExpirence);
            }
        }
    }

    private bool IsOwnUnit(RaycastHit hit) {
        var damagable = hit.collider.gameObject.GetComponent<Damagable>();
        return damagable != null && !damagable.isDead && damagable.playerId == playerId;
    }

    private bool IsOwnUnit(Collider collider) {
        var damagable = collider.gameObject.GetComponent<Damagable>();
        return damagable != null && !damagable.isDead && damagable.playerId == playerId;
    }

    private void CheckHit() {
        RaycastHit hit;
        var direction = transform.position - motion.previousPosition;
        
        if (Physics.Raycast(motion.previousPosition, direction.normalized, out hit, direction.magnitude)) {
            Debug.DrawRay(motion.previousPosition, direction.normalized * direction.magnitude, Color.red, 1f);
            if (LayerMask.LayerToName(hit.collider.gameObject.layer) == "Bush" || LayerMask.LayerToName(hit.collider.gameObject.layer) == "Ghost") {
                return;
            }
            
            if (IsOwnUnit(hit)) return;

            if (bulletSo.radius > 0) {
                Explode();
            }
            else {
                DealDamage(hit.collider);
            }

            pool.Release(this);
        }
    }

    public void Reset() {
        lifeTimeTimer = 0f;
        motion.previousPosition = transform.position;
        if (trailRenderer != null) {
            trailRenderer.Clear();
        }
    }

    void Update()
    {
        lifeTimeTimer += Time.deltaTime;
        motion.Move();
        CheckHit();

        if (lifeTimeTimer > bulletSo.lifeTime) {
            pool.Release(this);
        }
    }
}
