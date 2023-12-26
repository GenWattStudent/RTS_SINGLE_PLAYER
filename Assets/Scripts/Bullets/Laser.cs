using UnityEngine;

public class Laser : MonoBehaviour
{
    // laser beam have light, paricle system and line renderer
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private GameObject goLaserBeam;
    private LineRenderer lineRenderer;
    private ParticleSystem laserHitEffect;
    private Light laserLight;
    public LaserSo laserSo;
    public float currentDamageInterval = 0;
    public bool isAttacking = false;
    private Damagable target;
    private bool areEffectsInstantiated = false;

    public void SetTarget(Damagable target) {
        this.target = target;
    }

    private void Awake() {
        currentDamageInterval = laserSo.damgeInterval;
    }

    private void Attack() {
        if (currentDamageInterval <= 0 && isAttacking) {
            currentDamageInterval = laserSo.damgeInterval;
            target.TakeDamage(laserSo.damage);
        }
    }

    private void DrawLineToTarget() {
        lineRenderer.SetPosition(0, spawnPoint.transform.position);
        lineRenderer.SetPosition(1, target.transform.position);
    }

    private void PlayLaserHitEffect() {
        laserHitEffect.transform.position = target.transform.position;
        laserHitEffect.Play();
    }

    private void PlayLaserLight() {
        var newPosition = new Vector3(target.transform.position.x, target.transform.position.y + .5f, target.transform.position.z);
        laserLight.transform.position = newPosition;
    }
    
    private void InstantiateAllEffects() {
        goLaserBeam.gameObject.SetActive(true);

        lineRenderer = goLaserBeam.GetComponentInChildren<LineRenderer>();
        laserHitEffect = goLaserBeam.GetComponentInChildren<ParticleSystem>();
        laserLight = goLaserBeam.GetComponentInChildren<Light>();

        areEffectsInstantiated = true;
    }

    private void DestroyAllEffects() {
        goLaserBeam.gameObject.SetActive(false);

        lineRenderer = null;
        laserHitEffect = null;
        laserLight = null;

        areEffectsInstantiated = false;
    }

    private void PlayAllEffects() {
        DrawLineToTarget();
        PlayLaserHitEffect();
        PlayLaserLight();
    }

    private void Update() {
        currentDamageInterval -= Time.deltaTime;

        if (target == null) {
            if (lineRenderer != null && laserHitEffect != null && laserLight != null) DestroyAllEffects();
            return;
        }

        if (!areEffectsInstantiated) InstantiateAllEffects();
        if (areEffectsInstantiated) { 
            PlayAllEffects();
        }

        if (isAttacking) {
            Attack();
        }
    }
}
