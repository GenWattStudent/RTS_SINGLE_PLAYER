using UnityEngine;

[CreateAssetMenu(fileName = "Bullet", menuName = "ScriptableObjects/Bullet")]
public class BulletSo : ScriptableObject
{
    public string bulletName;
    public float damage;
    public float speed;
    public float arcHeight;
    public GameObject prefab;
    public float lifeTime;
    public float radius;
    public GameObject explosionPrefab;
    public GameObject initialExplosionPrefab;
    public MusicManager.MusicType explosionSound;
}
