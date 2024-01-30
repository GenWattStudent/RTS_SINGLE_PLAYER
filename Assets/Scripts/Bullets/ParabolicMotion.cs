using System.Collections;
using UnityEngine;

public class ParabolicMotion : Motion
{
    Vector3 startPosition;
    private float gravity = 9.8f;

    public override void Setup()
    {
        base.Setup();
        startPosition = transform.position;
        StartCoroutine(ParabolicMotion2(target, launchAngle + 10));
    }

    private IEnumerator ParabolicMotion2(Vector3 target, float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        float heightDifference = startPosition.y - target.y;
        Vector3 direction = (target - startPosition).normalized;
        float targetRange = Vector3.Distance(startPosition, target);
        float targetDistance = Vector3.Distance(startPosition, target);
        // float targetRange = Mathf.Abs(startPosition.x - target.x) + Mathf.Abs(startPosition.z - target.z);

        float projectile_Velocity
            = (Mathf.Sqrt(2) * targetRange * Mathf.Sqrt(gravity) * Mathf.Sqrt(1 / (Mathf.Sin(2 * angleRad)))) /
            (Mathf.Sqrt((2 * targetRange) + (heightDifference * Mathf.Sin(2 * angleRad) * (1 / Mathf.Sin(angleRad)) * (1 / Mathf.Sin(angleRad)))));

        float Vx = projectile_Velocity * Mathf.Cos(angleRad);
        float Vy = projectile_Velocity * Mathf.Sin(angleRad);

        float flightDuration = targetRange / Vx;

        float elapse_time = 0;
        Debug.Log(flightDuration);
        while (transform.position.y > 0)
        {
            float x = startPosition.x + direction.x * Vx * elapse_time;
            float y = startPosition.y + Vy * elapse_time - 0.5f * gravity * elapse_time * elapse_time;
            float z = startPosition.z + direction.z * Vx * elapse_time;

            previousPosition = transform.position;
            Vector3 newPosition = new Vector3(x, y, z);
            transform.position = newPosition;

            elapse_time += Time.deltaTime;
            Debug.Log(elapse_time);
            yield return null;
        }
    }

    public override void Move()
    {
        // Implement this method if needed
    }
}