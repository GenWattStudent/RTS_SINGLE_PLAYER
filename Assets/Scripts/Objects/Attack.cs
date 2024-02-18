using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class Attack : NetworkBehaviour
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
    private VehicleGun vehicleGun;
    private UnitMovement unitMovement;
    [SerializeField] private bool autoAttack = true;
    public bool isRealoading = false;

    public event Action OnAttack;
    public event Action<Damagable, Unit> OnTarget;
    public Vector3 targetPosition;
    private List<GameObject> salvePoints = new();
    private int salveIndex = 0;
    public float lastAttackTime;
    public float rot = 0;

    void Start()
    {
        currentUnit = GetComponent<Unit>();
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

            if (damagableScript != null && damagableScript.OwnerClientId != currentUnit.OwnerClientId && !damagableScript.isDead && unitScript.isVisibile)
            {
                if (IsTargetHideInTerrain(damagableScript)) continue;
                SetTarget(damagableScript);
                break;
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
    private void ShootBulletClientRpc(Vector3 direction)
    {
        if (currentUnit.attackableSo.bulletSo.initialExplosionPrefab != null)
        {
            var rotation = Quaternion.LookRotation(direction);
            rotation *= Quaternion.Euler(0, -90, 0);
            Instantiate(currentUnit.attackableSo.bulletSo.initialExplosionPrefab, bulletSpawnPoint.transform.position, rotation);
            MusicManager.Instance.PlayMusic(currentUnit.attackableSo.attackSound, bulletSpawnPoint.transform.position);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShootBulletServerRpc()
    {
        var bulletObjcet = Instantiate(currentUnit.attackableSo.bulletSo.prefab);
        var bullet = bulletObjcet.GetComponent<Bullet>();
        var motionScript = bullet.GetComponent<Motion>();
        var no = bulletObjcet.GetComponent<NetworkObject>();

        bullet.motion = motionScript;
        bullet.networkObject = no;

        var bulletSpawnPoint = this.bulletSpawnPoint;

        if (currentUnit.attackableSo.CanSalve)
        {
            bulletSpawnPoint = salvePoints[salveIndex];
            salveIndex++;

            if (salveIndex >= salvePoints.Count)
            {
                salveIndex = 0;
            }
        }

        bullet.transform.position = bulletSpawnPoint.transform.position;
        bullet.transform.rotation = Quaternion.identity;
        bullet.Reset();

        var targetPosition = this.targetPosition;

        if (target != null)
            targetPosition = target.targetPoint != null ? target.targetPoint.transform.position : target.transform.position;

        // take in account unit acccuracy to target position
        var accuracy = currentUnit.attackableSo.accuracy;
        var randomX = UnityEngine.Random.Range(-accuracy, accuracy);
        var randomZ = UnityEngine.Random.Range(-accuracy, accuracy);
        targetPosition += new Vector3(randomX, 0, randomZ);

        bullet.bulletSo = currentUnit.attackableSo.bulletSo;
        Debug.Log("bullet.bulletSo: " + OwnerClientId);
        bullet.motion.target = targetPosition;

        bullet.motion.launchAngle = vehicleGun != null ? vehicleGun.transform.eulerAngles.x : 0;
        bullet.unitsBullet = GetComponent<Damagable>();
        bullet.motion.Setup();
        bullet.Setup();

        attackSpeedTimer = currentUnit.attackableSo.attackSpeed;
        currentAmmo--;

        no.SpawnWithOwnership(OwnerClientId);
        ShootBulletClientRpc(bullet.motion.direction);
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

    private void Realod()
    {
        if (!isRealoading) attackCooldownTimer = currentUnit.attackableSo.attackCooldown;
        isRealoading = true;

        if (isRealoading && attackCooldownTimer <= 0)
        {
            currentAmmo = currentUnit.attackableSo.ammo;
            isRealoading = false;
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
        Debug.Log(IsInGunAngle());
        if (attackSpeedTimer <= 0 && attackCooldownTimer <= 0 && IsInAngle() && IsInGunAngle())
        {
            Debug.Log("PerformAttackServerRpc");
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

    void Update()
    {
        if (!IsServer) return;
        attackSpeedTimer -= Time.deltaTime;
        attackCooldownTimer -= Time.deltaTime;
        checkTargetTimerTimer -= Time.deltaTime;

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
