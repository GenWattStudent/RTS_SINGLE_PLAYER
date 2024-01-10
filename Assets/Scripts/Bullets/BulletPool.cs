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
    [SerializeField] private int poolSize = 100;
    public ObjectPool<Bullet> bulletPool;
    public ObjectPool<Bullet> rocketPool;
    public List<PoolData> pools = new ();
    public static BulletPool Instance;

    private Bullet CreateBullet() {
        var bullet = Instantiate(bulletPrefab);
        bullet.transform.SetParent(transform);
        var bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.pool = bulletPool;

        return bulletScript;
    }

    private Bullet CreateRocket() {
        var rocket = Instantiate(rocketPrefab);
        rocket.transform.SetParent(transform);
        var rocketScript = rocket.GetComponent<Bullet>();
        rocketScript.pool = rocketPool;

        return rocketScript;
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

        pools.Add(new PoolData {
            name = "Rocket",
            bulletPool = rocketPool
        });
    }
}
