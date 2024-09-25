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
    public NetworkObject networkObject;
    public Motion motion;
    public TeamType teamType;

    private float lifeTimeTimer = 0f;
    private TrailRenderer trailRenderer;
    private PlayerController playerController;

    private void Awake()
    {
        if (!IsServer) return;
        trailRenderer = GetComponentInChildren<TrailRenderer>();
        motion = GetComponent<Motion>();
    }

    private void Start()
    {
        if (!IsServer) return;
        networkObject = GetComponent<NetworkObject>();
        playerController = NetworkManager.Singleton.ConnectedClients[networkObject.OwnerClientId].PlayerObject.GetComponent<PlayerController>();
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(motion.previousPosition, bulletSo.radius);
        foreach (var collider in colliders)
        {
            if (IsOwnUnit(collider)) continue;
            DealDamage(collider);
        }

        ExplodeClientRpc(motion.previousPosition);
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

        if (damageableScript != null && damageableScript.OwnerClientId != OwnerClientId)
        {
            if (damageableScript.TakeDamage(damage))
            {
                unitsBullet.AddExpiernce(damageableScript.damagableSo.deathExpirence);
                if (unitsBullet.OwnerClientId == OwnerClientId)
                {
                    playerController.AddExpiernceServerRpc(damageableScript.damagableSo.deathExpirence);
                }
            }
        }
    }

    private bool IsOwnUnit(RaycastHit hit)
    {
        var damagable = hit.collider.gameObject.GetComponent<Damagable>();
        return damagable != null && !damagable.isDead && damagable.teamType.Value == teamType;
    }

    private bool IsOwnUnit(Collider collider)
    {
        var damagable = collider.gameObject.GetComponent<Damagable>();
        return damagable != null && !damagable.isDead && damagable.teamType.Value == teamType;
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
        lifeTimeTimer = 0f;
        motion.Hide();
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }

        networkObject.Despawn(true);
    }

    public void Setup()
    {
        damage = unitsBullet.stats.GetStat(StatType.Damage);
        motion.speed = bulletSo.GetStat(StatType.Speed);
    }

    public void Reset()
    {
        // motion.previousPosition = transform.position;
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
