using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Unit), typeof(Damagable))]
public class Attack : NetworkBehaviour
{
    [SerializeField] private bool autoAttack = true;
    [SerializeField] private float checkTargetTimer = .2f;
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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        currentUnit = GetComponent<Unit>();
        currentDamagable = GetComponent<Damagable>();
        currentAmmo = currentUnit.attackableSo.ammo;
        unitMovement = GetComponent<UnitMovement>();
        vehicleGun = GetComponentInChildren<VehicleGun>();
    }

    private void Start()
    {
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

    private bool IsTargetHide(Damagable target)
    {
        var direction = target.TargetPoint.position - currentDamagable.TargetPoint.position;
        var hits = Physics.RaycastAll(currentDamagable.TargetPoint.position, direction, currentUnit.attackableSo.attackRange);

        foreach (var hit in hits)
        {
            if (hit.collider.gameObject.GetComponent<Damagable>() == target)
            {
                return false; // Target is not hidden
            }
        }

        return true;
    }

    private void CheckForTargets()
    {
        var unit = RTSObjectsManager.quadtree.FindClosestUnitInRange(transform.position, currentUnit.attackableSo.attackRange, currentDamagable.teamType.Value);
        if (unit == null) return;
        var damagableScript = unit.GetComponent<Damagable>();

        if (currentDamagable.CanAttack(damagableScript) && !IsTargetHide(damagableScript))
        {
            SetTarget(damagableScript);
            return;
        }

        //var colliders = Physics.OverlapSphere(transform.position, currentUnit.attackableSo.attackRange);

        //foreach (var collider in colliders)
        //{
        //    var damagableScript = collider.gameObject.GetComponent<Damagable>();

        //    if (currentDamagable.CanAttack(damagableScript))
        //    {
        //        SetTarget(damagableScript);
        //        return;
        //    }
        //}
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

    private void OnTargetDead(Damagable target)
    {
        SetTarget(null);
    }

    private bool IsInRange()
    {
        Vector3 closestPointOnTargetUnit = GetClosestPointOnCollider(target, transform.position);
        float distance = Vector3.Distance(transform.position, closestPointOnTargetUnit);

        return distance <= currentUnit.attackableSo.attackRange;
    }

    private bool IsInRange(Vector3 targetPosition)
    {
        return Vector3.Distance(transform.position, targetPosition) <= currentUnit.attackableSo.attackRange;
    }

    private Vector3 GetClosestPointOnCollider(Damagable damagable, Vector3 position)
    {
        Collider collider = damagable.GetComponent<Collider>();
        if (collider != null)
        {
            return collider.ClosestPoint(position);
        }
        return damagable.transform.position;
    }

    [ClientRpc]
    private void ShootBulletClientRpc(int ammo, int salveIndex, Vector3 targetPosition)
    {
        var spawnPoint = currentUnit.attackableSo.CanSalve ? salvePoints[salveIndex].transform : bulletSpawnPoint.transform;
        var bullet = BulletManager.Instance.Spawn(currentUnit, spawnPoint, targetPosition, vehicleGun, currentDamagable.teamType.Value);

        currentAmmo = ammo;
        OnAmmoChange?.Invoke(currentAmmo);
        if (currentUnit.attackableSo.bulletSo.initialExplosionPrefab != null)
        {
            var direction = targetPosition - spawnPoint.position;
            var rotation = Quaternion.LookRotation(direction);
            var salvePoint = currentUnit.attackableSo.CanSalve ? salvePoints[salveIndex] : bulletSpawnPoint;

            rotation *= Quaternion.Euler(0, -90, 0);
            Instantiate(currentUnit.attackableSo.bulletSo.initialExplosionPrefab, salvePoint.transform.position, rotation);
            MusicManager.Instance.PlayMusic(currentUnit.attackableSo.attackSound, salvePoint.transform.position);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShootBulletServerRpc()
    {
        if (currentAmmo <= 0) return;

        var targetPos = target != null ? target.TargetPoint.position : targetPosition;

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

        ShootBulletClientRpc(currentAmmo, salveIndex, targetPos);
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

    private void FixedUpdate()
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
            targetPosition = target.transform.position;
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
