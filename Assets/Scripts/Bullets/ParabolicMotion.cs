using UnityEngine;

public class ParabolicMotion : Motion
{
    private Vector3 startPosition;
    private float time = 0f;
    private Vector3 ControlPointInBetweenStartAndTarget;
    private float speedDependingOnDistance = 1f;

    public override void Setup()
    {
        base.Setup();
        startPosition = transform.position;
        previousPosition = startPosition;
        target = new Vector3(target.x, 0, target.z);
        direction = (target - startPosition).normalized;
        time = 0f;

        // calulate ControlPointInBetweenStartAndTarget based on angle in between start and target
        var distance = Vector3.Distance(startPosition, target);
        var height = distance / 2f;
        ControlPointInBetweenStartAndTarget = startPosition + (direction * distance / 2f) + (Vector3.up * height);
        speedDependingOnDistance = 1f / distance;
    }

    private Vector3 EvaluateCurve(float t)
    {
        var startToControl = Vector3.Lerp(startPosition, ControlPointInBetweenStartAndTarget, t);
        var controlToEnd = Vector3.Lerp(ControlPointInBetweenStartAndTarget, target, t);

        return Vector3.Lerp(startToControl, controlToEnd, t);
    }

    public override void Move()
    {
        time += Time.deltaTime * speedDependingOnDistance * speed;
        var position = EvaluateCurve(time);
        var nextPos = EvaluateCurve(time + 0.01f) - position;

        previousPosition = transform.position;
        transform.rotation = Quaternion.LookRotation(nextPos);
        transform.position = position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        for (float i = 0; i < 20; i++)
        {
            Gizmos.DrawSphere(EvaluateCurve(i / 20), 0.1f);
        }

        // draw previous position
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(previousPosition, 1f);
    }
}