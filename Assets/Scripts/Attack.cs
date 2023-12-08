using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class Attack : MonoBehaviour
{
    public Damagable target;
    [SerializeField] private float checkTargetTimer;
    private Unit currentUnit;
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
        currentAmmo = currentUnit.unitSo.ammo;
        unitMovement = GetComponent<UnitMovement>();

        if (currentUnit.unitSo.hasTurret) {
            turret = GetComponentInChildren<Turret>();
        }
    }

    private void CheckForTargets() {
        var colliders = Physics.OverlapSphere(transform.position, currentUnit.unitSo.attackRange);

        foreach (var collider in colliders) {
            var damagableScript = collider.gameObject.GetComponent<Damagable>();

            if (damagableScript != null && damagableScript.playerId != currentUnit.playerId) {
                target = damagableScript;
                break;
            }
        }
    }

    private bool IsInRange() {
        return Vector3.Distance(transform.position, target.transform.position) <= currentUnit.unitSo.attackRange;
    }

    private void ShootBullet() {
        GameObject bullet = Instantiate(currentUnit.unitSo.bulletPrefab, bulletSpawnPoint.transform.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().damage = currentUnit.unitSo.attackDamage;
        bullet.GetComponent<Bullet>().direction = (target.transform.position - transform.position).normalized;
        bullet.GetComponent<Bullet>().playerId = currentUnit.playerId;

        attackSpeedTimer = currentUnit.unitSo.attackSpeed;
        currentAmmo--;
    }

    private bool IsInAngle() {
        if (currentUnit.unitSo.hasTurret) {
            return turret.IsInFieldOfView(target.transform.position, currentUnit.unitSo.attackAngle);
        } else {
            // calculate angle between unit and target
            Vector3 targetDir = target.transform.position - transform.position;
            float angle = Vector3.Angle(targetDir, transform.forward);

            // if angle is less than attack angle, return true
            if (angle < currentUnit.unitSo.attackAngle) {
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
                attackCooldownTimer = currentUnit.unitSo.attackCooldown;
                currentAmmo = currentUnit.unitSo.ammo;
            }
        }
    }

    void Update()
    {
        attackSpeedTimer -= Time.deltaTime;
        attackCooldownTimer -= Time.deltaTime;

        if (target == null && autoAttack) {
           checkForTargetsCoroutine =  StartCoroutine(CheckForTargetsCoroutine());
        }

        if (target != null) {
            if (checkForTargetsCoroutine != null) StopCoroutine(checkForTargetsCoroutine);
            if (IsInRange() && currentUnit.unitSo.canAttack) {
              PerformAttack();             
            } else {
                target = null;
            }

            if (currentUnit.unitSo.hasTurret) {
                turret.RotateToTarget(target.transform.position, currentUnit.unitSo.turretRotateSpeed);
            } else {
                unitMovement.RotateToTarget(target.transform.position);
            }
        }
    }
}
