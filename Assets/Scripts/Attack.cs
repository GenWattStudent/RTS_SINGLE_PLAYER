using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class Attack : MonoBehaviour
{
    public Damagable target;
    [SerializeField] private float checkTargetTimer = 0.2f;
    public Unit currentUnit;
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

        if (autoAttack) checkForTargetsCoroutine = StartCoroutine(CheckForTargetsCoroutine());
    }

    private void CheckForTargets() {
        var colliders = Physics.OverlapSphere(transform.position, currentUnit.attackableSo.attackRange);
        
        foreach (var collider in colliders) {
            var damagableScript = collider.gameObject.GetComponent<Damagable>();

            if (damagableScript != null && damagableScript.playerId != currentUnit.playerId) {
                Debug.Log("Found target");
                SetTarget(damagableScript);
                break;
            }
        }
    }

    public void SetTarget(Damagable target) {
        this.target = target;

        if (this.target != null) {
            if (checkForTargetsCoroutine != null) StopCoroutine(checkForTargetsCoroutine);
            target.OnDead += OnTargetDead;
            return;
        } 

        if (autoAttack) checkForTargetsCoroutine = StartCoroutine(CheckForTargetsCoroutine());
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
        GameObject bullet = Instantiate(currentUnit.attackableSo.bulletPrefab, bulletSpawnPoint.transform.position, Quaternion.identity);
        var bulletScript = bullet.GetComponent<Bullet>();
        var targetPosition = target.targetPoint != null ? target.targetPoint.transform.position : target.transform.position;

        bulletScript.bulletSo = currentUnit.attackableSo.bulletSo;
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

        if (target != null) {
            if (IsInRange() && currentUnit.attackableSo.canAttack) {
                if (PlayerController.Instance.playerId == currentUnit.playerId && gameObject.name == "HPCharacter(Clone)") {
                    Debug.Log("Attack");
                }
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
