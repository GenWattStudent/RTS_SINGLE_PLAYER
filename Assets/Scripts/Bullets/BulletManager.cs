using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    private List<Bullet> bullets = new List<Bullet>();

    public static BulletManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public Bullet Spawn(Unit unit, Transform bulletSpawnPoint, Vector3 target, VehicleGun vehicleGun, TeamType teamType)
    {
        var bullet = BulletPool.Instance.GetPool(unit.unitSo.bulletSo.bulletName).Get();
        var motionScript = bullet.GetComponent<Motion>();

        bullet.teamType = teamType;
        bullet.motion = motionScript;

        bullet.transform.position = bulletSpawnPoint.position;
        bullet.transform.rotation = Quaternion.identity;

        // Take into account unit accuracy to target position
        var accuracy = unit.attackableSo.accuracy;
        var randomX = Random.Range(-accuracy, accuracy);
        var randomZ = Random.Range(-accuracy, accuracy);
        target += new Vector3(randomX, 0, randomZ);

        bullet.bulletSo = unit.attackableSo.bulletSo;
        bullet.motion.target = target;
        bullet.motion.launchAngle = vehicleGun != null ? vehicleGun.transform.eulerAngles.x : 0;
        bullet.unitsBullet = unit.GetComponent<Damagable>();
        bullet.motion.Setup();
        bullet.Setup();

        bullets.Add(bullet);
        return bullet;
    }

    private void Update()
    {
        for (int i = 0; i < bullets.Count; i++)
        {
            bullets[i].Update();
        }
    }
}
