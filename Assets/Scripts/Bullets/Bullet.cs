using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Motion))]
public class Bullet : NetworkBehaviour
{
    public float damage;
    public BulletSo bulletSo;
    public Damagable unitsBullet;
    public ObjectPool<Bullet> pool;
    private float lifeTimeTimer = 0f;
    private TrailRenderer trailRenderer;
    private PlayerController playerController;
    private NetworkObject networkObject;
    public Motion motion;

    private void Awake()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }
        trailRenderer = GetComponentInChildren<TrailRenderer>();
        motion = GetComponent<Motion>();
        networkObject = GetComponent<NetworkObject>();
        playerController = NetworkManager.Singleton.ConnectedClients[networkObject.OwnerClientId].PlayerObject.GetComponent<PlayerController>();
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

        if (damageableScript != null && damageableScript.OwnerClientId != OwnerClientId)
        {
            if (damageableScript.TakeDamage(damage))
            {
                unitsBullet.AddExpiernce(damageableScript.damagableSo.deathExpirence);
                if (unitsBullet.OwnerClientId == playerController.OwnerClientId)
                {
                    playerController.AddExpiernce(damageableScript.damagableSo.deathExpirence);
                }
            }
        }
    }

    private bool IsOwnUnit(RaycastHit hit)
    {
        var damagable = hit.collider.gameObject.GetComponent<Damagable>();
        return damagable != null && !damagable.isDead && damagable.OwnerClientId == OwnerClientId;
    }

    private bool IsOwnUnit(Collider collider)
    {
        var damagable = collider.gameObject.GetComponent<Damagable>();
        return damagable != null && !damagable.isDead && damagable.OwnerClientId == OwnerClientId;
    }

    private void CheckHit()
    {
        RaycastHit hit;

        var direction = transform.position - motion.previousPosition;
        // Debug.DrawRay(motion.previousPosition, direction.normalized * direction.magnitude, Color.red, 1f);
        if (Physics.Raycast(motion.previousPosition, direction.normalized, out hit, direction.magnitude))
        {
            // Draw long ray from this postion to forward of the bullet

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

            HideBullet();
        }
    }

    private void HideBullet()
    {
        pool.Release(this);
        lifeTimeTimer = 0f;
        motion.Hide();
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }

    public void Setup()
    {
        damage = unitsBullet.stats.GetStat(StatType.Damage);
        motion.speed = bulletSo.GetStat(StatType.Speed);
    }

    public void Reset()
    {
        motion.previousPosition = transform.position;
    }

    void Update()
    {
        if (!IsServer) return;

        lifeTimeTimer += Time.deltaTime;
        CheckHit();
        motion.Move();

        if (lifeTimeTimer > bulletSo.lifeTime)
        {
            Explode();
            HideBullet();
        }
    }
}
