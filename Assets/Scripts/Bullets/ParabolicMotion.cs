using UnityEngine;

public class ParabolicMotion : Motion
{
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float totalDistance;
    private float distanceTravelled = 0f;
    private float arcFactor = .1f;
    private float initialHeight; // Initial Y position
    private float targetHeight;  // Target's Y position
    private float prevHeight;    // Previous height

    public override void Setup()
    {
        base.Setup();
        startPosition = transform.position;
        previousPosition = startPosition;
        targetPosition = target;

        initialHeight = startPosition.y;
        targetHeight = targetPosition.y;

        direction = (targetPosition - startPosition).normalized;

        totalDistance = Vector3.Distance(new Vector3(startPosition.x, 0, startPosition.z), new Vector3(targetPosition.x, 0, targetPosition.z));
        distanceTravelled = 0f;

        arcFactor = 1f / totalDistance;
    }

    public override void Move()
    {
        if (totalDistance <= 0) return;
        previousPosition = transform.position;

        distanceTravelled += speed * Time.deltaTime;
        float normalizedDistance = distanceTravelled / totalDistance;

        // Horizontal movement
        Vector3 flatPosition = Vector3.Lerp(startPosition, targetPosition, normalizedDistance);

        // Sinusoidal height calculation for a natural arc
        float heightOffset = Mathf.Sin(Mathf.PI * normalizedDistance);
        float height = Mathf.Lerp(initialHeight, targetHeight, normalizedDistance) + arcFactor * totalDistance * heightOffset;

        // Update projectile position
        Vector3 parabolicPosition = new Vector3(flatPosition.x, height, flatPosition.z);

        // Extend movement beyond target position
        if (normalizedDistance >= 1f)
        {
            Vector3 continuation = direction * speed * Time.deltaTime;
            transform.position += continuation;

            // Continue the height trajectory in the same parabolic fashion
            float continuationHeight = targetHeight + arcFactor * totalDistance * Mathf.Sin(Mathf.PI * normalizedDistance);

            if (continuationHeight < prevHeight)
            {
                continuationHeight -= arcFactor * totalDistance;
            }

            transform.position = new Vector3(transform.position.x, continuationHeight, transform.position.z);
            prevHeight = continuationHeight;
        }
        else
        {
            transform.position = parabolicPosition;
        }

        // Rotate bullet to face the direction of movement
        Vector3 moveDirection = (transform.position - previousPosition).normalized;
        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }
}
