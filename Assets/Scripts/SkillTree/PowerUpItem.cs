using Unity.Netcode;
using UnityEngine;

public class PowerUpItem : NetworkBehaviour
{
    [SerializeField] private PowerUpSo powerUpSo;

    private void Update()
    {
        Rotate();
    }

    private void Rotate()
    {
        transform.Rotate(Vector3.up, 1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent(out Stats stats))
        {
            HandlePickup(stats);
        }
    }

    private void HandlePickup(Stats stats)
    {
        stats.AddPowerUp(powerUpSo);

        Destroy(gameObject);
    }
}
