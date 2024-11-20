using Unity.Netcode;
using UnityEngine;

public class PowerUpItem : NetworkBehaviour
{
    [SerializeField] private PowerUpSo powerUpSo;
    private NetworkObject networkObject;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            // var go = Instantiate(powerUpSo.Prefab, transform.position, Quaternion.identity);
            // networkObject = go.GetComponent<NetworkObject>();

            // if (networkObject != null)
            // {
            //     networkObject.Spawn();
            // }
            networkObject = GetComponent<NetworkObject>();
        }
    }

    private void Update()
    {
        Rotate();
    }

    private void Rotate()
    {
        // Rotate to be curved pluse rotate up and down
        transform.Rotate(Vector3.up * Time.deltaTime * 50);
        transform.position = new Vector3(transform.position.x, Mathf.PingPong(Time.time, 0.2f) + 0.5f, transform.position.z);
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

        networkObject.Despawn(true);
    }
}
