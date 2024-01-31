using System.Collections;
using UnityEngine;

public class ParabolicMotion : Motion
{
    private Vector3 startPosition;
    private float time = 0f;
    private Vector3 targetPosition;
    private Vector3 ControlPointInBetweenStartAndTarget;

    public override void Setup()
    {
        base.Setup();
        startPosition = transform.position;
        targetPosition = target;
        direction = (targetPosition - startPosition).normalized;
        time = 0f;

        // calulate ControlPointInBetweenStartAndTarget based on angle in between start and target
        var distance = Vector3.Distance(startPosition, targetPosition);
        var height = distance / 2f;
        ControlPointInBetweenStartAndTarget = startPosition + (direction * distance / 2f) + (Vector3.up * height);
    }

    private Vector3 EvaluateCurve(float t)
    {
        var startToControl = Vector3.Lerp(startPosition, ControlPointInBetweenStartAndTarget, t);
        var controlToEnd = Vector3.Lerp(ControlPointInBetweenStartAndTarget, targetPosition, t);
        return Vector3.Lerp(startToControl, controlToEnd, t);
    }

    public override void Move()
    {
        time += Time.deltaTime * speed;
        var position = EvaluateCurve(time);
        var nextPos = EvaluateCurve(time + 0.01f) - position;

        transform.rotation = Quaternion.LookRotation(nextPos);
        transform.position = position;
        previousPosition = transform.position;
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

    public override void Hide()
    {
        // this method runs when bu
    }
}