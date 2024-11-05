using Unity.Netcode;
using UnityEngine;

public class HeroAnimationController : NetworkBehaviour
{
    private Animator animator;
    private UnitMovement unitMovement;
    private Attack attackScript;
    private Damagable damagableScript;
    private int isWalkingHash = Animator.StringToHash("isWalking");
    private int isShootingHash = Animator.StringToHash("isShooting");
    private int isDeadHash = Animator.StringToHash("isDead");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        unitMovement = GetComponent<UnitMovement>();
        attackScript = GetComponent<Attack>();
        damagableScript = GetComponent<Damagable>();
        damagableScript.OnDead += HandleOnDead;
    }

    private void HandleOnDead(Damagable damagable)
    {
        if (!IsServer) return;
        animator.SetBool(isShootingHash, false);
        animator.SetBool(isDeadHash, damagableScript.isDead.Value);
        animator.SetBool(isWalkingHash, false);
    }

    private void Update()
    {
        if (!IsServer || animator == null) return;
        animator.SetBool(isWalkingHash, unitMovement.isMoving);
        animator.SetBool(isShootingHash, attackScript.targetPosition != Vector3.zero);
    }
}
