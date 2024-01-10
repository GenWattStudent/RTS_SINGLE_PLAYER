using UnityEngine;

public class BulletMotion : Motion
{
    Vector3 _startPosition;
    float _progress;
    float _stepSize = 0.01f;
    Vector3 targetPosition;

    public override void Setup() {
        _progress = 0.0f;
        _startPosition = transform.position;
        targetPosition = new Vector3(target.x, -0.3f, target.z);
        float distance = Vector3.Distance(_startPosition, targetPosition);
        // This is one divided by the total flight duration, to help convert it to 0-1 progress.
        _stepSize = speed / distance;
    }

    private void RotateInDirection(Vector3 direction) {
        transform.rotation = Quaternion.LookRotation(direction);
    }

    public override void Move() {
        // Increment our progress from 0 at the start, to 1 when we arrive.
        _progress = Mathf.Min(_progress + Time.deltaTime * _stepSize, 1.0f);

        // Turn this 0-1 value into a parabola that goes from 0 to 1, then back to 0.
        float parabola = 1.0f - 4.0f * (_progress - 0.5f) * (_progress - 0.5f);

        // Travel in a straight line from our start position to the target.        
        Vector3 nextPos = Vector3.Lerp(_startPosition, targetPosition, _progress);

        // Then add a vertical arc in excess of this.
        nextPos.y += parabola * arcHeight;

        // Continue as before.
        RotateInDirection(nextPos - transform.position);
        transform.position = nextPos;
    }
}