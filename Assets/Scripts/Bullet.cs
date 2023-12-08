using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public Vector3 direction;
    public float speed = 2f;
    public Guid playerId;
    public BulletSo bulletSo;

    private void Awake() {
        damage = bulletSo.damage;
        speed = bulletSo.speed;
    }

    private void Move() {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void Explode() {
        if (bulletSo.explosionPrefab != null)  {
            var explosion = Instantiate(bulletSo.explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 1f);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, bulletSo.radius);
        foreach (var collider in colliders) {
            DealDamage(collider);
        }
    }

    private void DealDamage(Collider collider) {
        var damegableScript = collider.gameObject.GetComponent<Damagable>();

        if (damegableScript != null && damegableScript.playerId != playerId) {
            damegableScript.TakeDamage(damage);
        }
    }

    private bool IsOwnUnit(RaycastHit hit) {
        var damagable = hit.collider.gameObject.GetComponent<Damagable>();
        return damagable != null && damagable.playerId == playerId;

    }

    private void CheckHit() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, bulletSo.speed * Time.deltaTime)) {
            if (IsOwnUnit(hit)) return;

            if (bulletSo.radius > 0f) {
                Explode();
            }
            else {
                DealDamage(hit.collider);
            }

            Destroy(gameObject);
        }
    }

    void Start()
    {
        Destroy(gameObject, bulletSo.lifeTime);
    }

    void Update()
    {
        CheckHit();
        Move();
    }
}
