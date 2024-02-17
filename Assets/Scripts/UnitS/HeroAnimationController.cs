using Unity.Netcode;
using UnityEngine;

public class HeroAnimationController : NetworkBehaviour
{
    private Animator animator;
    private UnitMovement unitMovement;
    private Attack attackScript;
    private Damagable damagableScript;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        unitMovement = GetComponent<UnitMovement>();
        attackScript = GetComponent<Attack>();
        damagableScript = GetComponent<Damagable>();
        damagableScript.OnDead += HandleOnDead;
    }

    private void HandleOnDead()
    {
        if (!IsServer) return;
        animator.SetBool("isShooting", false);
        animator.SetBool("isDead", damagableScript.isDead);
        animator.SetBool("isWalking", false);
    }

    private void Update()
    {
        if (!IsServer || animator == null) return;
        animator.SetBool("isWalking", unitMovement.isMoving);
        animator.SetBool("isShooting", attackScript.targetPosition != Vector3.zero);
    }
}
