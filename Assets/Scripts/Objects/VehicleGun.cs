using UnityEngine;

public class VehicleGun : MonoBehaviour
{
    private Attack attack;
    public float rotationSpeed = 5f;

    void Start()
    {
        attack = GetComponentInParent<Attack>();
    }


    private void SlerpRotation(float angle) {
        var targetRotation = Quaternion.Euler(angle, transform.eulerAngles.y, transform.eulerAngles.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (attack.targetPosition == Vector3.zero) {
            SlerpRotation(0);
            return;
        };

        SlerpRotation(20);
    }
}
