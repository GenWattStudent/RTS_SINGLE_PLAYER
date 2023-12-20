using UnityEngine;

public class AttackableSo : ScriptableObject
{
    public float attackDamage;
    public float attackRange;
    public float attackSpeed;
    public float attackCooldown;
    public float acceleration;
    public bool canAttack;
    public GameObject bulletPrefab;
    public int ammo;
    public int attackAngle;
    public bool hasTurret;
    public float rotateSpeed;
    public float turretRotateSpeed;
}
