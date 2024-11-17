using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Motion))]
public class Bullet : MonoBehaviour
{
    public float damage;
    public BulletSo bulletSo;
    public Damagable unitsBullet;
    public ObjectPool<Bullet> pool;
    public Motion motion;
    public TeamType teamType;

    private float lifeTimeTimer = 0f;
    private TrailRenderer trailRenderer;

    private void Awake()
    {
        trailRenderer = GetComponentInChildren<TrailRenderer>();
        motion = GetComponent<Motion>();
    }

    private void Explode(Vector3 postion)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Collider[] colliders = Physics.OverlapSphere(postion, bulletSo.radius);
            foreach (var collider in colliders)
            {
                if (IsOwnUnit(collider)) continue;
                DealDamage(collider);
            }
        }

        ExplodeClientRpc(postion);
    }

    [ClientRpc]
    private void ExplodeClientRpc(Vector3 postion)
    {
        if (bulletSo.explosionPrefab != null)
        {
            var explosion = Instantiate(bulletSo.explosionPrefab, postion, Quaternion.identity);
            MusicManager.Instance.PlayMusic(bulletSo.explosionSound, postion);
            Destroy(explosion, 2f);
        }
    }

    private void DealDamage(Collider collider)
    {
        var damageableScript = collider.gameObject.GetComponent<Damagable>();

        if (damageableScript != null && !damageableScript.IsTeamMate(unitsBullet))
        {
            if (damageableScript.TakeDamage(damage))
            {
                unitsBullet.AddExpiernce(damageableScript.damagableSo.deathExpirence);

                var playerController = NetworkManager.Singleton.ConnectedClients[unitsBullet.OwnerClientId].PlayerObject.GetComponent<PlayerController>();
                playerController.AddExpiernceServerRpc(damageableScript.damagableSo.deathExpirence);
            }
        }
    }

    private bool IsOwnUnit(Collider collider)
    {
        var damagable = collider.gameObject.GetComponent<Damagable>();
        return damagable != null && !damagable.isDead.Value && damagable.teamType.Value == teamType;
    }

    private void CheckHit()
    {
        var direction = transform.position - motion.previousPosition;
        // Debug.DrawRay(motion.previousPosition, direction.normalized * direction.magnitude, Color.red, 1f);
        if (Physics.Raycast(motion.previousPosition, direction.normalized, out var hit, direction.magnitude))
        {
            // Draw long ray from this postion to forward of the bullet
            if (LayerMask.LayerToName(hit.collider.gameObject.layer) == "Bush" || LayerMask.LayerToName(hit.collider.gameObject.layer) == "Ghost")
            {
                return;
            }

            if (IsOwnUnit(hit.collider)) return;

            if (bulletSo.radius > 0)
            {
                Explode(hit.point);
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
        lifeTimeTimer = 0f;
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }

        BulletPool.Instance.GetPool(bulletSo.bulletName).Release(this);
        BulletManager.Instance.Remove(this);
    }

    public void Setup()
    {
        damage = unitsBullet.stats.GetStat(StatType.Damage);
        motion.speed = bulletSo.GetStat(StatType.Speed);
    }

    public void UpdateBullet()
    {
        CheckHit();
        motion.Move();

        if (lifeTimeTimer > bulletSo.lifeTime)
        {
            Explode(transform.position);
            HideBullet();
        }
    }
}
