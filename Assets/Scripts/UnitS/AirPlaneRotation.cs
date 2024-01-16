using UnityEngine;
using UnityEngine.AI;

public class AirPlaneRotation : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("Rotation Settings")]
    public float rotationSpeed = 2f; // Adjust the speed as needed
    public float maxRotationAngle = 45f; // Maximum rotation angle in degrees

    private void Start()
    {
        agent = GetComponentInParent<NavMeshAgent>();
    }

    private void Update()
    {
        // Get the current z-axis rotation
        float currentZRotation = transform.eulerAngles.z;

        // Calculate the target z-axis rotation
        float targetZRotation;
        if (agent.hasPath)
        {
            Vector3 moveDirection = agent.velocity.normalized;
            targetZRotation = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

            // Apply the rotation around the Z-axis within the defined limits
            targetZRotation = Mathf.Clamp(-targetZRotation, -maxRotationAngle, maxRotationAngle);
        }
        else
        {
            targetZRotation = 0f;
        }

        // Interpolate between the current z-axis rotation and the target z-axis rotation
        float finalZRotation = Mathf.LerpAngle(currentZRotation, targetZRotation, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, finalZRotation);
    }
}
