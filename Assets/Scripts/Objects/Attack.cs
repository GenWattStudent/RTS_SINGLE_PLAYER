using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class Attack : NetworkBehaviour
{
    [SerializeField] private bool autoAttack = true;
    [SerializeField] private float checkTargetTimer = 0.2f;
    [SerializeField] private GameObject bulletSpawnPoint;
    public bool isRealoading = false;
    public Vector3 targetPosition;
    public Damagable target;
    public Unit currentUnit;
    public Damagable currentDamagable;
    public int currentAmmo;
    public float attackCooldownTimer;
    public float lastAttackTime;

    private float checkTargetTimerTimer = 0f;
    private float attackSpeedTimer;
    private Turret turret;
    private VehicleGun vehicleGun;
    private UnitMovement unitMovement;
    private List<GameObject> salvePoints = new();
    private int salveIndex = 0;

    public event Action OnAttack;
    public event Action<Damagable, Unit> OnTarget;
    public event Action<int> OnAmmoChange;

    private void Start()
    {
        currentUnit = GetComponent<Unit>();
        currentDamagable = GetComponent<Damagable>();
        currentAmmo = currentUnit.attackableSo.ammo;
        unitMovement = GetComponent<UnitMovement>();
        vehicleGun = GetComponentInChildren<VehicleGun>();
        lastAttackTime = Time.time;

        if (currentUnit.attackableSo.hasTurret)
        {
            turret = GetComponentInChildren<Turret>();
        }

        if (autoAttack) checkTargetTimerTimer = checkTargetTimer;

        if (currentUnit.attackableSo.CanSalve)
        {
            // all children of bulletSpawnPoint are salve points
            foreach (Transform child in bulletSpawnPoint.transform)
            {
                salvePoints.Add(child.gameObject);
            }
        }
    }

    private bool IsTargetHideInTerrain(Damagable target)
    {
        if (target == null) return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position + new Vector3(0, 1, 0), target.transform.position - transform.position, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Terrain"))
            {
                return true;
            }
        }

        return false;
    }


    private void CheckForTargets()
    {
        var colliders = Physics.OverlapSphere(transform.position, currentUnit.attackableSo.attackRange);

        foreach (var collider in colliders)
        {
            var damagableScript = collider.gameObject.GetComponent<Damagable>();
            var unitScript = collider.gameObject.GetComponent<Unit>();

            if (currentDamagable.CanAttack(damagableScript, unitScript) && !IsTargetHideInTerrain(damagableScript))
            {
                SetTarget(damagableScript);
                return;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTargetServerRpc(NetworkObjectReference nor)
    {
        if (nor.TryGet(out NetworkObject networkObject))
        {
            var damagable = networkObject.GetComponent<Damagable>();
            SetTarget(damagable);
        }
    }

    private void SetTarget(Damagable target)
    {
        this.target = target;

        if (this.target != null)
        {
            targetPosition = target.transform.position;
            target.OnDead += OnTargetDead;
            OnTarget?.Invoke(target, currentUnit);
            return;
        }

        targetPosition = Vector3.zero;
        OnTarget?.Invoke(target, currentUnit);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTargetPositionServerRpc(Vector3 position)
    {
        targetPosition = position;
        target = null;
    }

    private void OnTargetDead()
    {
        SetTarget(null);
    }

    private bool IsInRange()
    {
        // we need to use target collider
        Collider[] colliders = Physics.OverlapSphere(transform.position, currentUnit.attackableSo.attackRange);

        foreach (var collider in colliders)
        {
            if (collider.gameObject == target.gameObject)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsInRange(Vector3 targetPosition)
    {
        return Vector3.Distance(transform.position, targetPosition) <= currentUnit.attackableSo.attackRange;
    }

    [ClientRpc]
    private void ShootBulletClientRpc(Vector3 direction, int ammo, int salveIndex)
    {
        currentAmmo = ammo;
        OnAmmoChange?.Invoke(currentAmmo);
        if (currentUnit.attackableSo.bulletSo.initialExplosionPrefab != null)
        {
            var rotation = Quaternion.LookRotation(direction);
            var salvePoint = currentUnit.attackableSo.CanSalve ? salvePoints[salveIndex] : bulletSpawnPoint;
            rotation *= Quaternion.Euler(0, -90, 0);
            Instantiate(currentUnit.attackableSo.bulletSo.initialExplosionPrefab, salvePoint.transform.position, rotation);
            // MusicManager.Instance.PlayMusic(currentUnit.attackableSo.attackSound, salvePoint.transform.position);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShootBulletServerRpc()
    {
        if (currentAmmo <= 0) return;

        var targetPos = target != null ? (target.targetPoint != null ? target.targetPoint.transform.position : target.transform.position) : targetPosition;
        var bullet = BulletFactory.CreateBullet(currentUnit, bulletSpawnPoint.transform, targetPos, salveIndex, salvePoints, vehicleGun, currentDamagable.teamType);

        if (currentUnit.attackableSo.CanSalve)
        {
            salveIndex++;
            if (salveIndex >= salvePoints.Count)
            {
                salveIndex = 0;
            }
        }

        attackSpeedTimer = currentUnit.attackableSo.attackSpeed;
        currentAmmo--;

        bullet.networkObject.SpawnWithOwnership(OwnerClientId);
        ShootBulletClientRpc(bullet.motion.direction, currentAmmo, salveIndex);
    }

    private bool IsInAngle()
    {
        if (currentUnit.attackableSo.hasTurret)
        {
            return turret.IsInFieldOfView(targetPosition, currentUnit.attackableSo.attackAngle);
        }
        else
        {
            // calculate angle between unit and target
            Vector3 targetDir = targetPosition - transform.position;
            float angle = Vector3.Angle(targetDir, transform.forward);

            // if angle is less than attack angle, return true
            if (angle < currentUnit.attackableSo.attackAngle)
            {
                return true;
            }

            return false;
        }
    }

    [ClientRpc]
    private void ReloadClientRpc(int ammo)
    {
        OnAmmoChange?.Invoke(ammo);
    }

    private void Realod()
    {
        if (!isRealoading) attackCooldownTimer = currentUnit.attackableSo.attackCooldown;
        isRealoading = true;

        if (isRealoading && attackCooldownTimer <= 0)
        {
            currentAmmo = currentUnit.attackableSo.ammo;
            isRealoading = false;
            ReloadClientRpc(currentAmmo);
        }
    }

    private bool IsInGunAngle()
    {
        if (vehicleGun == null) return true;

        return vehicleGun.IsFinisehdRotation();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PerformAttackServerRpc()
    {
        if (attackSpeedTimer <= 0 && attackCooldownTimer <= 0 && IsInAngle() && IsInGunAngle())
        {
            OnAttack?.Invoke();
            ShootBulletServerRpc();
            lastAttackTime = Time.time;
            if (currentAmmo <= 0)
            {
                Realod();
            }
        }
    }

    private void PerformTargetAiming()
    {
        if (IsInRange() && currentUnit.attackableSo.canAttack)
        {
            PerformAttackServerRpc();
        }
        else
        {
            SetTarget(null);
            return;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RotateToTargetServerRpc()
    {
        if (currentUnit.attackableSo.hasTurret)
        {
            turret.RotateToTarget(targetPosition, currentUnit.attackableSo.turretRotateSpeed);
        }
        else
        {
            unitMovement.RotateToTarget(targetPosition);
        }
    }

    void FixedUpdate()
    {
        if (!IsServer) return;
        attackSpeedTimer -= Time.fixedDeltaTime;
        attackCooldownTimer -= Time.fixedDeltaTime;
        checkTargetTimerTimer -= Time.fixedDeltaTime;

        if (checkTargetTimerTimer <= 0 && autoAttack && target == null && targetPosition == Vector3.zero)
        {
            CheckForTargets();
            checkTargetTimerTimer = checkTargetTimer;
        }

        if (target != null)
        {
            PerformTargetAiming();
            RotateToTargetServerRpc();
            return;
        }

        if (targetPosition != Vector3.zero)
        {
            if (IsInRange(targetPosition) && currentUnit.attackableSo.canAttack)
            {
                PerformAttackServerRpc();
                RotateToTargetServerRpc();
            }
            else
            {
                SetTarget(null);
                return;
            }
        }
    }
}
