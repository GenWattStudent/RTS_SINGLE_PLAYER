using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class Attack : MonoBehaviour
{
    public Damagable target;
    [SerializeField] private float checkTargetTimer;
    private Unit currentUnit;
    private Animator animator;
    [SerializeField] private GameObject bulletSpawnPoint;
    private float attackSpeedTimer;
    private float attackCooldownTimer;
    private int currentAmmo;
    private Turret turret;
    private UnitMovement unitMovement;
    [SerializeField] private bool autoAttack = true;
    private Coroutine checkForTargetsCoroutine;

    void Start()
    {
        currentUnit = GetComponent<Unit>();
        currentAmmo = currentUnit.attackableSo.ammo;
        unitMovement = GetComponent<UnitMovement>();
        animator = GetComponent<Animator>();

        if (currentUnit.attackableSo.hasTurret) {
            turret = GetComponentInChildren<Turret>();
        }
    }

    private void CheckForTargets() {
        var colliders = Physics.OverlapSphere(transform.position, currentUnit.attackableSo.attackRange);

        foreach (var collider in colliders) {
            var damagableScript = collider.gameObject.GetComponent<Damagable>();

            if (damagableScript != null && damagableScript.playerId != currentUnit.playerId) {
                SetTarget(damagableScript);
                break;
            }
        }
    }

    public void SetTarget(Damagable target) {
        this.target = target;
        target.OnDead += OnTargetDead;
    }

    private void OnTargetDead() {
        target = null;
    }

    private bool IsInRange() {
        return Vector3.Distance(transform.position, target.transform.position) <= currentUnit.attackableSo.attackRange;
    }

    private void ShootBullet() {
        GameObject bullet = Instantiate(currentUnit.attackableSo.bulletPrefab, bulletSpawnPoint.transform.position, Quaternion.identity);
        var bulletScript = bullet.GetComponent<Bullet>();
        var targetPosition = target.targetPoint != null ? target.targetPoint.transform.position : target.transform.position;

        bulletScript.damage = currentUnit.attackableSo.attackDamage;
        bulletScript.direction = (targetPosition- bulletSpawnPoint.transform.position).normalized;
        bulletScript.playerId = currentUnit.playerId;
        bulletScript.unitsBullet = GetComponent<Damagable>();

        attackSpeedTimer = currentUnit.attackableSo.attackSpeed;
        currentAmmo--;
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

    private IEnumerator CheckForTargetsCoroutine() {
        while (true) {
            CheckForTargets();
            yield return new WaitForSeconds(checkTargetTimer);
        }
    }

    private void PerformAttack() {
        if (attackSpeedTimer <= 0 && attackCooldownTimer <= 0 && IsInAngle()) {
            ShootBullet();
            
            if (currentAmmo <= 0) {
                attackCooldownTimer = currentUnit.attackableSo.attackCooldown;
                currentAmmo = currentUnit.attackableSo.ammo;
            }
        }
    }

    void Update()
    {
        attackSpeedTimer -= Time.deltaTime;
        attackCooldownTimer -= Time.deltaTime;

        if (target == null && autoAttack && currentUnit.attackableSo.canAttack && checkForTargetsCoroutine == null) {
           checkForTargetsCoroutine =  StartCoroutine(CheckForTargetsCoroutine());
        }

        if (target != null) {
            if (checkForTargetsCoroutine != null) StopCoroutine(checkForTargetsCoroutine);
            if (IsInRange() && currentUnit.attackableSo.canAttack) {
              PerformAttack();             
            } else {
                target = null;
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
