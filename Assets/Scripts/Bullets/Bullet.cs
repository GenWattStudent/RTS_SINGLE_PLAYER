using System;
using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    public float damage;
    public Vector3 direction;
    public float speed = 2f;
    public Guid playerId;
    public BulletSo bulletSo;
    public Damagable unitsBullet;
    public ObjectPool<Bullet> pool;
    private Vector3 previousPosition;
    private float lifeTimeTimer = 0f;

    private void Awake() {
        damage = bulletSo.damage;
        speed = bulletSo.speed;
    }

    private void Move() {
        previousPosition = transform.position;
        transform.rotation = Quaternion.LookRotation(direction);
        transform.position += direction * speed * Time.deltaTime;
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
        var direction = transform.position - previousPosition;
        
        if (Physics.Raycast(previousPosition, direction.normalized, out hit, direction.magnitude)) {
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
        previousPosition = transform.position;
    }

    void Update()
    {
        lifeTimeTimer += Time.deltaTime;
        Move();
        CheckHit();

        if (lifeTimeTimer > bulletSo.lifeTime) {
            pool.Release(this);
        }
    }
}
