using System;
using UnityEngine;

public class Damagable : MonoBehaviour
{
    public DamagableSo damagableSo;
    public float health;
    public float maxHealth;
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject destroyedObjectPrefab;
    private ProgresBar progressBarScript;
    private Animator animator;
    public Guid playerId;
    public Levelable levelable;

    // On dead event handler
    public event Action OnDead;
    public GameObject targetPoint;

    void Awake()
    {
        health = damagableSo.health;
        maxHealth = damagableSo.health;
        progressBarScript = healthBar.GetComponent<ProgresBar>();
        animator = GetComponent<Animator>();
        levelable = GetComponent<Levelable>();
    }

    private void InstantiateExplosion() {
        if (explosionPrefab != null) {
            var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 2.4f);
        }
    }

    private void InstantiateDestroyedObject() {
        if (destroyedObjectPrefab != null) {
            var destroyedObject = Instantiate(destroyedObjectPrefab, transform.position, transform.rotation);
            Destroy(destroyedObject, 30f);
        }
    }

    public void AddExpiernce(int exp) {
        if (levelable == null) return;
        levelable.AddExpirence(exp);
    }

    public bool TakeDamage(float damage) {
        health -= damage;
        progressBarScript.UpdateProgresBar(health, damagableSo.health);

        if (health <= 0f) {
            Destroy(gameObject);
            InstantiateExplosion(); 
            InstantiateDestroyedObject();
            OnDead?.Invoke();
            if (animator != null) {
                animator.SetBool("isDead", true);
            }

            return true;
        }

        return false;
    }
}
