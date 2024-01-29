using UnityEngine;

public class TankShotController : MonoBehaviour
{
    private VehicleGun vehicleGun;
    private Animator animator;
    private Attack attack;
    private float lastAttackTime;

    // Start is called before the first frame update
    void Start()
    {
        vehicleGun = GetComponentInChildren<VehicleGun>();
        if (vehicleGun == null) return;

        animator = vehicleGun.GetComponent<Animator>();
        attack = GetComponent<Attack>();

        attack.OnAttack += HandleAttack;
    }

    private void HandleAttack()
    {
        lastAttackTime = Time.time;
        animator.SetBool("isShot", true);
    }

    private void OnDestroy()
    {
        attack.OnAttack -= HandleAttack;
    }

    // Update is called once per frame
    void Update()
    {
        if (vehicleGun == null) return;

        if (Time.time - lastAttackTime > 0.25f)
        {
            animator.SetBool("isShot", false);
        }
    }
}
