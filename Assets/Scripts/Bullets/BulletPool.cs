using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolData {
    public string name;
    public ObjectPool<Bullet> bulletPool;
}

public class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private GameObject heroBulletPrefab;
    [SerializeField] private int poolSize = 100;
    public ObjectPool<Bullet> bulletPool;
    public ObjectPool<Bullet> rocketPool;
    public ObjectPool<Bullet> heroBulletPool;
    private GameObject bulletParent;
    private GameObject rocketParent;
    private GameObject heroBulletParent;
    public List<PoolData> pools = new ();
    public static BulletPool Instance;

    private Bullet CreateBullet() {
        var bullet = Instantiate(bulletPrefab);
        bullet.transform.SetParent(bulletParent.transform);
        var bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.pool = bulletPool;

        return bulletScript;
    }

    private Bullet CreateRocket() {
        var rocket = Instantiate(rocketPrefab);
        rocket.transform.SetParent(rocketParent.transform);
        var rocketScript = rocket.GetComponent<Bullet>();
        rocketScript.pool = rocketPool;

        return rocketScript;
    }

    private Bullet CreateHeroBullet() {
        var bullet = Instantiate(heroBulletPrefab);
        bullet.transform.SetParent(heroBulletParent.transform);
        var bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.pool = heroBulletPool;

        return bulletScript;
    }

    private void OnGet(Bullet bullet) {
        bullet.Reset();
        bullet.gameObject.SetActive(true);
    }

    private void OnDestroyed(Bullet bullet) {
        Destroy(bullet.gameObject);
    }

    private void OnRelease(Bullet bullet) {
        bullet.gameObject.SetActive(false);
    }

    public ObjectPool<Bullet> GetPool(string name) {
        foreach (var pool in pools) {
            if (pool.name == name) {
                return pool.bulletPool;
            }
        }

        return null;
    }
    
    void Awake()
    {
        Instance = this;
        bulletPool = new ObjectPool<Bullet>(
            CreateBullet,
            OnGet,
            OnRelease,
            OnDestroyed, 
            false,
            poolSize
        );

        pools.Add(new PoolData {
            name = "Bullet",
            bulletPool = bulletPool
        });

        rocketPool = new ObjectPool<Bullet>(
            CreateRocket,
            OnGet,
            OnRelease,
            OnDestroyed, 
            false,
            poolSize
        );

        heroBulletPool = new ObjectPool<Bullet>(
            CreateHeroBullet,
            OnGet,
            OnRelease,
            OnDestroyed, 
            false,
            poolSize
        );

        bulletParent = new GameObject("Bullet Parent");
        bulletParent.transform.SetParent(transform);
        rocketParent = new GameObject("Rocket Parent");
        rocketParent.transform.SetParent(transform);
        heroBulletParent = new GameObject("Hero Bullet Parent");
        heroBulletParent.transform.SetParent(transform);

        pools.Add(new PoolData {
            name = "Rocket",
            bulletPool = rocketPool
        });

        pools.Add(new PoolData {
            name = "Hero Bullet",
            bulletPool = heroBulletPool
        });
    }
}
