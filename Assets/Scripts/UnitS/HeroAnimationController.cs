using UnityEngine;

public class HeroAnimationController : MonoBehaviour
{
    private Animator animator;
    private UnitMovement unitMovement;
    private Attack attackScript;
    private Damagable damagableScript;

    private void Awake() {
        animator = GetComponent<Animator>();
        unitMovement = GetComponent<UnitMovement>();
        attackScript = GetComponent<Attack>();
        damagableScript = GetComponent<Damagable>();
        damagableScript.OnDead += HandleOnDead;
    }

    private void HandleOnDead() {
        animator.SetBool("isShooting", false);
        animator.SetBool("isDead", damagableScript.isDead);
        animator.SetBool("isWalking", false);
    }

    private void Update() {
        if (animator == null) return;
        
        animator.SetBool("isShooting", attackScript.target != null);
        animator.SetBool("isWalking", unitMovement.isMoving);
    }
}
