using UnityEngine;

public class ParabolicMotion : Motion
{
    // private Vector3 startPosition;
    // private float time = 0f;
    // private Vector3 ControlPointInBetweenStartAndTarget;
    // private float speedDependingOnDistance = 1f;

    // public override void Setup()
    // {
    //     base.Setup();
    //     startPosition = transform.position;
    //     previousPosition = startPosition;
    //     target = new Vector3(target.x, 0, target.z);
    //     direction = (target - startPosition).normalized;
    //     time = 0f;

    //     // calulate ControlPointInBetweenStartAndTarget based on angle in between start and target
    //     var distance = Vector3.Distance(startPosition, target);
    //     var height = distance / 2f;
    //     ControlPointInBetweenStartAndTarget = startPosition + (direction * distance / 2f) + (Vector3.up * height);
    //     speedDependingOnDistance = 1f / distance;
    // }

    // private Vector3 EvaluateCurve(float t)
    // {
    //     var startToControl = Vector3.Lerp(startPosition, ControlPointInBetweenStartAndTarget, t);
    //     var controlToEnd = Vector3.Lerp(ControlPointInBetweenStartAndTarget, target, t);

    //     return Vector3.Lerp(startToControl, controlToEnd, t);
    // }

    // public override void Move()
    // {
    //     time += Time.deltaTime * speedDependingOnDistance * speed;
    //     var position = EvaluateCurve(time);
    //     var nextPos = EvaluateCurve(time + 0.01f) - position;

    //     previousPosition = transform.position;
    //     transform.rotation = Quaternion.LookRotation(nextPos);
    //     transform.position = position;
    // }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;

    //     for (float i = 0; i < 20; i++)
    //     {
    //         Gizmos.DrawSphere(EvaluateCurve(i / 20), 0.1f);
    //     }

    //     // draw previous position
    //     Gizmos.color = Color.blue;
    //     Gizmos.DrawSphere(previousPosition, 1f);
    // }
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float totalDistance;
    private float distanceTravelled = 0f;
    private float arcFactor = .1f;
    private float initialHeight; // Initial Y position
    private float targetHeight;  // Target's Y position

    public override void Setup()
    {
        base.Setup();
        startPosition = transform.position;
        previousPosition = startPosition;
        targetPosition = target; // Now using the full target including height (Y axis)

        // Capture initial and target heights
        initialHeight = startPosition.y;
        targetHeight = targetPosition.y;

        direction = (targetPosition - startPosition).normalized;

        // Calculate the total distance to the target (horizontal distance)
        totalDistance = Vector3.Distance(new Vector3(startPosition.x, 0, startPosition.z), new Vector3(targetPosition.x, 0, targetPosition.z));
        distanceTravelled = 0f; // Reset the distance travelled

        arcFactor = 1f / totalDistance; // Adjust the arc factor based on the total distance
    }

    public override void Move()
    {
        if (totalDistance <= 0) return; // Prevent division by zero
        previousPosition = transform.position;

        // Move towards the target in a straight line (ignoring Y-axis for now)
        distanceTravelled += speed * Time.deltaTime;
        float normalizedDistance = distanceTravelled / totalDistance; // 0 -> 1

        // Interpolate between start and target position (horizontal movement)
        Vector3 flatPosition = Vector3.Lerp(startPosition, targetPosition, normalizedDistance);

        // Calculate the height at the current point based on a parabolic trajectory
        float heightOffset = Mathf.Sin(Mathf.PI * normalizedDistance); // Sinusoidal path for arc

        // Combine height difference between start and target heights
        float height = Mathf.Lerp(initialHeight, targetHeight, normalizedDistance) + arcFactor * totalDistance * heightOffset;

        // Update the projectile's position with the new height
        Vector3 parabolicPosition = new Vector3(flatPosition.x, height, flatPosition.z);
        transform.position = parabolicPosition;
    }
}