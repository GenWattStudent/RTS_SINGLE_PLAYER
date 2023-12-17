using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "ScriptableObjects/Unit", order = 1)]
public class UnitSo : DamagableSo
{
    public enum UnitType
    {
        Worker,
        Attacker,
        Healer,
        Commander
    }
    public UnitType type;
    public string unitName;
    public float speed;
    public float buildingDistance;
    public float attackDamage;
    public float attackRange;
    public float attackSpeed;
    public float attackCooldown;
    public float acceleration;
    public bool canAttack;
    public GameObject bulletPrefab;
    public int ammo;
    public int attackAngle;
    public int cost;
    public bool hasTurret;
    public float rotateSpeed;
    public float turretRotateSpeed;
    public float spawnTime;
    public Sprite sprite;
    public GameObject prefab;
    public BulletSo bulletSo;
}
