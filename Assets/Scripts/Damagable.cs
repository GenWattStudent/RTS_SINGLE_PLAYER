using System;
using UnityEngine;

public class Damagable : MonoBehaviour
{
    public DamagableSo damagableSo;
    public float health;
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject destroyedObjectPrefab;
    private ProgresBar progressBarScript;
    public Guid playerId;

    void Awake()
    {
        health = damagableSo.health;
        progressBarScript = healthBar.GetComponent<ProgresBar>();
    }

    private void InstantiateExplosion() {
        if (explosionPrefab != null) {
            var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            explosion.transform.localScale = Vector3.one * 2f;
            Destroy(explosion, 2.4f);
        }
    }

    private void InstantiateDestroyedObject() {
        if (destroyedObjectPrefab != null) {
            var destroyedObject = Instantiate(destroyedObjectPrefab, transform.position, transform.rotation);
            Destroy(destroyedObject, 30f);
        }
    }

    public void TakeDamage(float damage) {
        health -= damage;
        progressBarScript.UpdateProgresBar(health, damagableSo.health);

        if (health <= 0f) {
            Destroy(gameObject);
            InstantiateExplosion(); 
            InstantiateDestroyedObject();
        }
    }
}
