using Unity.Netcode;
using UnityEngine;

public class Laser : NetworkBehaviour
{
    // laser beam have light, paricle system and line renderer
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private GameObject goLaserBeam;
    public LaserSo laserSo;
    public float currentDamageInterval = 0;
    public bool isAttacking = false;

    private Damagable target;
    private bool areEffectsInstantiated = false;
    private LineRenderer lineRenderer;
    private ParticleSystem laserHitEffect;
    private Light laserLight;
    private Stats stats;

    public void SetTarget(Damagable target)
    {
        this.target = target;
    }

    private void Awake()
    {
        stats = GetComponentInParent<Stats>();
        currentDamageInterval = stats.GetStat(StatType.AttackSpeed);
    }

    private void Attack()
    {
        if (currentDamageInterval <= 0 && isAttacking)
        {
            currentDamageInterval = stats.GetStat(StatType.AttackSpeed);
            target.TakeDamage(stats.GetStat(StatType.Damage));
        }
    }

    private void DrawLineToTarget()
    {
        lineRenderer.SetPosition(0, spawnPoint.transform.position);
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

    [ServerRpc(RequireOwnership = false)]
    private void InstantiateAllEffectsServerRpc()
    {
        InstantiateAllEffectsClientRpc();
    }

    [ClientRpc]
    private void InstantiateAllEffectsClientRpc()
    {
        goLaserBeam.gameObject.SetActive(true);

        lineRenderer = goLaserBeam.GetComponentInChildren<LineRenderer>();
        laserHitEffect = goLaserBeam.GetComponentInChildren<ParticleSystem>();
        laserLight = goLaserBeam.GetComponentInChildren<Light>();

        areEffectsInstantiated = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyAllEffectsServerRpc()
    {
        DestroyAllEffectsClientRpc();
    }

    [ClientRpc]
    private void DestroyAllEffectsClientRpc()
    {
        goLaserBeam.gameObject.SetActive(false);

        lineRenderer = null;
        laserHitEffect = null;
        laserLight = null;

        areEffectsInstantiated = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayAllEffectsServerRpc()
    {
        PlayAllEffectsClientRpc();
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

        currentDamageInterval -= Time.deltaTime;

        if (target == null)
        {
            if (lineRenderer != null && laserHitEffect != null && laserLight != null) DestroyAllEffectsServerRpc();
            return;
        }

        if (!areEffectsInstantiated) InstantiateAllEffectsServerRpc();
        if (areEffectsInstantiated)
        {
            PlayAllEffectsServerRpc();
        }

        if (isAttacking)
        {
            Attack();
        }
    }
}
