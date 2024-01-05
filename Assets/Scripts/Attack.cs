using System;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class Attack : MonoBehaviour
{
    public Damagable target;
    [SerializeField] private float checkTargetTimer = 0.2f;
    private float checkTargetTimerTimer = 0f;
    public Unit currentUnit;
    [SerializeField] private GameObject bulletSpawnPoint;
    private float attackSpeedTimer;
    public float attackCooldownTimer;
    public int currentAmmo;
    private Turret turret;
    private UnitMovement unitMovement;
    [SerializeField] private bool autoAttack = true;
    private bool isRealoading = false;

    public event Action OnAttack;
    public event Action<Damagable, Unit> OnTarget;

    void Start()
    {
        currentUnit = GetComponent<Unit>();
        currentAmmo = currentUnit.attackableSo.ammo;
        unitMovement = GetComponent<UnitMovement>();

        if (currentUnit.attackableSo.hasTurret) {
            turret = GetComponentInChildren<Turret>();
        }

        if (autoAttack) checkTargetTimerTimer = checkTargetTimer;
    }

    private bool IsTargetHideInTerrain(Damagable target) {
        if (target == null) return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, target.transform.position - transform.position, out hit, Mathf.Infinity)) {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Terrain")) {
                return true;
            }
        }

        return false;
    }

    private void CheckForTargets() {
        var colliders = Physics.OverlapSphere(transform.position, currentUnit.attackableSo.attackRange);
        
        foreach (var collider in colliders) {
            var damagableScript = collider.gameObject.GetComponent<Damagable>();

            if (damagableScript != null && damagableScript.playerId != currentUnit.playerId && !damagableScript.isDead) {
                if (IsTargetHideInTerrain(damagableScript)) return;
                SetTarget(damagableScript);
                break;
            }
        }
    }

    public void SetTarget(Damagable target) {
        this.target = target;

        if (this.target != null) {
            target.OnDead += OnTargetDead;
            OnTarget?.Invoke(target, currentUnit);
            return;
        } 

        OnTarget?.Invoke(target, currentUnit);
    }   

    private void OnTargetDead() {
        SetTarget(null);
    }

    private bool IsInRange() {
        // we need to use target collider
        Collider[] colliders = Physics.OverlapSphere(transform.position, currentUnit.attackableSo.attackRange);

        foreach (var collider in colliders) {
            if (collider.gameObject == target.gameObject) {
                return true;
            }
        }

        return false;
    }

    private void ShootBullet() {
        Bullet bullet = BulletPool.Instance.bulletPool.Get();
        bullet.transform.position = bulletSpawnPoint.transform.position;
        bullet.transform.rotation = Quaternion.identity;

        var targetPosition = target.targetPoint != null ? target.targetPoint.transform.position : target.transform.position;

        bullet.bulletSo = currentUnit.attackableSo.bulletSo;
        bullet.direction = (targetPosition- bulletSpawnPoint.transform.position).normalized;
        bullet.playerId = currentUnit.playerId;
        bullet.unitsBullet = GetComponent<Damagable>();

        attackSpeedTimer = currentUnit.attackableSo.attackSpeed;
        currentAmmo--;

        if (currentUnit.attackableSo.bulletSo.initialExplosionPrefab != null) {
            Instantiate(currentUnit.attackableSo.bulletSo.initialExplosionPrefab, bulletSpawnPoint.transform.position, Quaternion.identity);
        }
    }

    private bool IsInAngle() {
        if (currentUnit.attackableSo.hasTurret) {
            return turret.IsInFieldOfView(target.transform.position, currentUnit.attackableSo.attackAngle);
        } else {
            // calculate angle between unit and target
            Vector3 targetDir = target.transform.position - transform.position;
            float angle = Vector3.Angle(targetDir, transform.forward);

            // if angle is less than attack angle, return true
            if (angle < currentUnit.attackableSo.attackAngle) {
                return true;
            }

            return false;
        }
    }

    private void Realod() {
        if (!isRealoading) attackCooldownTimer = currentUnit.attackableSo.attackCooldown;
        isRealoading = true;
        
        if (isRealoading && attackCooldownTimer <= 0) {
            currentAmmo = currentUnit.attackableSo.ammo;
            isRealoading = false;
        }
    }

    private void PerformAttack() {
        if (attackSpeedTimer <= 0 && attackCooldownTimer <= 0 && IsInAngle()) {
            OnAttack?.Invoke();
            ShootBullet();
            
            if (currentAmmo <= 0) {
                Realod();
            }
        }
    }

    void Update()
    {
        attackSpeedTimer -= Time.deltaTime;
        attackCooldownTimer -= Time.deltaTime;
        checkTargetTimerTimer -= Time.deltaTime;

        if (checkTargetTimerTimer <= 0 && autoAttack && target == null) {
            CheckForTargets();
            checkTargetTimerTimer = checkTargetTimer;
        }

        if (target != null) {
            if (IsInRange() && currentUnit.attackableSo.canAttack) {
              PerformAttack();             
            } else {
                SetTarget(null);
                return;
            }

            if (currentUnit.attackableSo.hasTurret) {
                turret.RotateToTarget(target.transform.position, currentUnit.attackableSo.turretRotateSpeed);
            } else {
                unitMovement.RotateToTarget(target.transform.position);
            }
        }
    }
}
