using Unity.Netcode;
using UnityEngine;

public class Turret : NetworkBehaviour
{
    public void RotateToTarget(Vector3 targetPosition, float rotateSpeed)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0; // This will ignore the y-axis difference in direction
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Vector3 rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed).eulerAngles;
        transform.rotation = Quaternion.Euler(0, rotation.y, 0); // This will only rotate around the y-axis
    }

    public bool IsInFieldOfView(Vector3 targetPosition, float fieldOfViewAngle)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0; // Ignore the y-axis difference in direction
        float angle = Vector3.Angle(direction, transform.forward);
        return angle < fieldOfViewAngle * 0.5f;
    }
}