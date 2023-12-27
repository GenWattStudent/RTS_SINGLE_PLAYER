using System;
using UnityEngine;
using UnityEngine.AI;

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
    private Attack attackScript;
    private Collider collider;
    private UnitMovement unitMovement;
    private NavMeshAgent agent;
    private Selectable selectable;

    void Awake()
    {
        health = damagableSo.health;
        maxHealth = damagableSo.health;
        progressBarScript = healthBar.GetComponent<ProgresBar>();
        animator = GetComponent<Animator>();
        levelable = GetComponent<Levelable>();
        attackScript = GetComponent<Attack>();
        collider = GetComponent<Collider>();
        unitMovement = GetComponent<UnitMovement>();
        agent = GetComponent<NavMeshAgent>();
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

    private void PlayDisapperShader() {
        var unitScript = GetComponent<Unit>();

        if (unitScript != null) {
            unitScript.ChangeMaterial(damagableSo.deathMaterial);
            unitScript.StartShaderValues("_DissolveOffest", 1f);
        }
    }

    private void DisableAfterDeath() {
        Destroy(collider);
        if (unitMovement != null) {
            Destroy(unitMovement);
        }

        if (attackScript != null) {
            Destroy(attackScript);
        }

        if (targetPoint != null) {
            Destroy(targetPoint);
        }

        if (agent != null) {
            Destroy(agent);
        }

        if (selectable != null) {
            Destroy(selectable);
        }
    }

    public bool TakeDamage(float damage) {
        health -= damage;
        progressBarScript.UpdateProgresBar(health, damagableSo.health);

        if (health <= 0f) {
            PlayDisapperShader();
            InstantiateExplosion(); 
            InstantiateDestroyedObject();
            OnDead?.Invoke();
            if (animator != null) {
                animator.SetBool("isDead", true);
            }
            Destroy(gameObject, 20f);
            DisableAfterDeath();
            return true;
        }

        return false;
    }
}
