using UnityEngine;

public class VehicleGun : MonoBehaviour
{
    private Attack attack;
    public float rotationSpeed = 8f;
    public float rotationAngle = -20f;

    void Start()
    {
        attack = GetComponentInParent<Attack>();
    }

    private void SlerpRotation(float angle) {
        var targetRotation = Quaternion.Euler(angle, transform.eulerAngles.y, transform.eulerAngles.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public bool IsFinisehdRotation() {
        return transform.rotation == Quaternion.Euler(rotationAngle, transform.eulerAngles.y, transform.eulerAngles.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (attack.targetPosition == Vector3.zero) {
            SlerpRotation(0);
            return;
        };

        SlerpRotation(rotationAngle);
    }
}
