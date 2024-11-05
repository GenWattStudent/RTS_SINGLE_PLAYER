using Unity.Netcode;
using UnityEngine;

public class TankShotController : NetworkBehaviour
{
    private VehicleGun vehicleGun;
    private Animator animator;
    private Attack attack;
    private float lastAttackTime;

    private void Start()
    {
        if (!IsServer) return;

        vehicleGun = GetComponentInChildren<VehicleGun>();
        if (vehicleGun == null) return;

        animator = vehicleGun.GetComponent<Animator>();
    }

    private void HandleAttack()
    {
        lastAttackTime = Time.time;
        animator.SetBool("isShot", true);
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
        if (attack == null) return;
        attack.OnAttack -= HandleAttack;
    }

    // Update is called once per frame
    private void Update()
    {
        if (vehicleGun == null) return;

        if (Time.time - lastAttackTime > 0.25f)
        {
            animator.SetBool("isShot", false);
        }
    }
}
