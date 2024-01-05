using UnityEngine;
using UnityEngine.Pool;

public class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int poolSize = 100;
    public ObjectPool<Bullet> bulletPool;
    public static BulletPool Instance;

    private Bullet CreateBullet() {
        var bullet = Instantiate(bulletPrefab);
        var bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.pool = bulletPool;

        return bulletScript;
    }

    private void OnGet(Bullet bullet) {
        bullet.Reset();
        bullet.gameObject.SetActive(true);
    }

    private void OnDesteoy(Bullet bullet) {
        bullet.gameObject.SetActive(false);
    }

    private void OnRelease(Bullet bullet) {
        bullet.gameObject.SetActive(false);
    }
    
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        bulletPool = new ObjectPool<Bullet>(
            CreateBullet,
            OnGet,
            OnRelease,
            OnDesteoy, 
            false,
            poolSize
        );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
