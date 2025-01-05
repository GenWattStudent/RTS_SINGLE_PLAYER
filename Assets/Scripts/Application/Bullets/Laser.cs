using Unity.Netcode;
using UnityEngine;

public class Laser : NetworkBehaviour
{
    // laser beam have light, paricle system and line renderer
    [SerializeField] private GameObject SpawnPoint;
    [SerializeField] private GameObject GoLaserBeam;
    public LaserSo LaserSo;
    public float CurrentDamageInterval = 0;
    public bool IsAttacking = false;
    public bool IsLaserOn = false;
    public NetworkVariable<bool> IsTarget = new(false);

    private Transform target;
    private LineRenderer lineRenderer;
    private ParticleSystem laserHitEffect;
    private Light laserLight;
    private Stats stats;

    public void SetTarget(Transform target)
    {
        this.target = target;
        HandleTarget(false, this.target != null);
    }

    private void Awake()
    {
        stats = GetComponentInParent<Stats>();
        CurrentDamageInterval = stats.GetStat(StatType.AttackSpeed);
    }

    [ClientRpc]
    public void SetTargetClientRpc(NetworkObjectReference nor)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            SetTarget(no.transform);
        }
        else
        {
            SetTarget(null);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            var targetOrNull = target == null ? null : target.GetComponent<NetworkObject>();
            SetTargetClientRpc(targetOrNull);
        }
    }

    private void HandleTarget(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            InstantiateAllEffectsClientRpc();
            PlayAllEffectsClientRpc();
        }
        else
        {
            DestroyAllEffectsClientRpc();
        }
    }

    private void Attack()
    {
        if (CurrentDamageInterval <= 0 && IsAttacking)
        {
            CurrentDamageInterval = stats.GetStat(StatType.AttackSpeed);
            var damagable = target.GetComponent<Damagable>();

            if (damagable != null) damagable.TakeDamage(stats.GetStat(StatType.Damage));
        }
    }

    private void DrawLineToTarget()
    {
        lineRenderer.SetPosition(0, SpawnPoint.transform.position);
        lineRenderer.SetPosition(1, target.transform.position);
    }

    private void PlayLaserHitEffect()
    {
        laserHitEffect.transform.position = target.transform.position;
        laserHitEffect.Play();
    }

    private void PlayLaserLight()
    {
        var newPosition = new Vector3(target.transform.position.x, target.transform.position.y + .5f, target.transform.position.z);
        laserLight.transform.position = newPosition;
    }

    [ClientRpc]
    private void InstantiateAllEffectsClientRpc()
    {
        GoLaserBeam.gameObject.SetActive(true);

        lineRenderer = GoLaserBeam.GetComponentInChildren<LineRenderer>();
        laserHitEffect = GoLaserBeam.GetComponentInChildren<ParticleSystem>();
        laserLight = GoLaserBeam.GetComponentInChildren<Light>();
    }

    [ClientRpc]
    private void DestroyAllEffectsClientRpc()
    {
        GoLaserBeam.gameObject.SetActive(false);

        lineRenderer = null;
        laserHitEffect = null;
        laserLight = null;
    }

    [ClientRpc]
    private void PlayAllEffectsClientRpc()
    {
        DrawLineToTarget();
        PlayLaserHitEffect();
        PlayLaserLight();
    }

    private void Update()
    {
        if (!IsServer) return;

        CurrentDamageInterval -= Time.deltaTime;

        if (IsAttacking)
        {
            Attack();
        }
    }
}
