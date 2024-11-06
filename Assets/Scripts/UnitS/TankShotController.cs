using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class TankShotController : NetworkBehaviour
{
    private VehicleGun vehicleGun;
    private NetworkAnimator animator;
    private Attack attack;
    private float lastAttackTime;

    private int isShotHash = Animator.StringToHash("isShot");

    private void Start()
    {
        if (!IsServer) return;

        vehicleGun = GetComponentInChildren<VehicleGun>();
        if (vehicleGun == null) return;

        animator = vehicleGun.GetComponent<NetworkAnimator>();
    }

    private void HandleAttack()
    {
        lastAttackTime = Time.time;
        animator.Animator.SetBool("isShot", true);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        enabled = IsServer;
        if (!IsServer) return;

        Debug.Log("Tank shoot");
        attack = GetComponent<Attack>();
        attack.OnAttack += HandleAttack;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (attack == null || !IsServer) return;
        attack.OnAttack -= HandleAttack;
    }

    // Update is called once per frame
    private void Update()
    {
        if (vehicleGun == null) return;

        if (Time.time - lastAttackTime > 0.25f)
        {
            animator.Animator.SetBool("isShot", false);
        }
    }
}
