using UnityEngine;

public class Turret : MonoBehaviour
{
    public void RotateToTarget(Vector3 targetPosition, float rotateSpeed) {
        Vector3 direction = targetPosition - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Vector3 rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed).eulerAngles;
        transform.rotation = Quaternion.Euler(rotation);
    }

    public bool IsInFieldOfView(Vector3 targetPosition, float fieldOfViewAngle) {
        Vector3 direction = targetPosition - transform.position;
        float angle = Vector3.Angle(direction, transform.forward);
        return angle < fieldOfViewAngle * 0.5f;
    }
}