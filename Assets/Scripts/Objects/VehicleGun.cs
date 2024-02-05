using Unity.Netcode;
using UnityEngine;

public class VehicleGun : NetworkBehaviour
{
    private Attack attack;
    public float rotationSpeed = 8f;
    public float rotationAngle = -20f;
    public float threshold = 0.4f;

    void Start()
    {
        attack = GetComponentInParent<Attack>();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SlerpRotationServerRpc(float angle)
    {
        var targetRotation = Quaternion.Euler(angle, transform.eulerAngles.y, transform.eulerAngles.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public bool IsFinisehdRotation()
    {
        return Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.x, rotationAngle)) < threshold;
    }

    void Update()
    {
        if (attack.targetPosition == Vector3.zero)
        {
            SlerpRotationServerRpc(0);
            return;
        };

        SlerpRotationServerRpc(rotationAngle);
    }
}
