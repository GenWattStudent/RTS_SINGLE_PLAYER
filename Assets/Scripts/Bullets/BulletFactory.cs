using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class BulletFactory
{
    public static Bullet CreateBullet(Unit unit, Transform bulletSpawnPoint, Vector3 targetPosition, int salveIndex, List<GameObject> salvePoints, VehicleGun vehicleGun, TeamType teamType)
    {
        var bulletObject = Object.Instantiate(unit.attackableSo.bulletSo.prefab);
        var bullet = bulletObject.GetComponent<Bullet>();
        var motionScript = bullet.GetComponent<Motion>();
        var networkObject = bulletObject.GetComponent<NetworkObject>();

        bullet.teamType = teamType;
        bullet.motion = motionScript;
        bullet.networkObject = networkObject;

        if (unit.attackableSo.CanSalve)
        {
            bulletSpawnPoint = salvePoints[salveIndex].transform;
        }

        bullet.transform.position = bulletSpawnPoint.position;
        bullet.transform.rotation = Quaternion.identity;
        bullet.Reset();

        // Take into account unit accuracy to target position
        var accuracy = unit.attackableSo.accuracy;
        var randomX = Random.Range(-accuracy, accuracy);
        var randomZ = Random.Range(-accuracy, accuracy);
        targetPosition += new Vector3(randomX, 0, randomZ);

        bullet.bulletSo = unit.attackableSo.bulletSo;
        bullet.motion.target = targetPosition;
        bullet.motion.launchAngle = vehicleGun != null ? vehicleGun.transform.eulerAngles.x : 0;
        bullet.unitsBullet = unit.GetComponent<Damagable>();
        bullet.motion.Setup();
        bullet.Setup();

        return bullet;
    }
}