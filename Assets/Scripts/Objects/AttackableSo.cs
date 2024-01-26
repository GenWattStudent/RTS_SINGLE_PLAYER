using UnityEngine;

public class AttackableSo : ScriptableObject
{
    [Header("Attack options")]
    public float attackDamage;
    public float attackRange;
    public float attackSpeed;
    public float attackCooldown;
    public float acceleration;
    public bool canAttack;
    public GameObject bulletPrefab;
    public int ammo;
    public int attackAngle;
    [Header("Turret options")]
    public bool hasTurret;
    public float turretRotateSpeed;
    public float rotateSpeed;
    public BulletSo bulletSo;
    public bool CanSalve;
    public float accuracy = 0.5f;
    public MusicManager.MusicType attackSound;
}
