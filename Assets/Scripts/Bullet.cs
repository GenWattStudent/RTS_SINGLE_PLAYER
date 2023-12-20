using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public Vector3 direction;
    public float speed = 2f;
    public Guid playerId;
    public BulletSo bulletSo;
    public Damagable unitsBullet;

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
        return damagable != null && damagable.playerId == playerId;
    }

    private bool IsOwnUnit(Collider collider) {
        var damagable = collider.gameObject.GetComponent<Damagable>();
        return damagable != null && damagable.playerId == playerId;
    }

    private void CheckHit() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, speed * Time.deltaTime)) {
            Debug.Log(hit.collider.gameObject.name);
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
