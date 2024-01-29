using UnityEngine;

public class BulletMotion : Motion
{
    Vector3 startPosition;
    Rigidbody rb;

    public override void Setup()
    {
        base.Setup();

        startPosition = transform.position;

        rb = gameObject.GetComponent<Rigidbody>();
        rb.useGravity = false; // Disable gravity initially
        rb.velocity = direction.normalized * speed;

        // Rotate the rigidbody to match the launch angle
        rb.rotation = Quaternion.Euler(launchAngle, 0f, 0f);
    }

    private void RotateInDirection(Vector3 direction)
    {
        transform.rotation = Quaternion.LookRotation(direction);
    }

    public override void Move()
    {
        // Check if the bullet has reached the target
        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            // If it's close enough, stop the bullet and enable gravity
            rb.velocity = Vector3.zero;
            rb.useGravity = true;
        }
        else
        {
            // Otherwise, continue with the parabolic motion
            float t = Vector3.Distance(startPosition, transform.position) / Vector3.Distance(startPosition, target);
            float parabola = 1.0f - 4.0f * (t - 0.5f) * (t - 0.5f);

            Vector3 nextPos = Vector3.Lerp(startPosition, target, t);
            nextPos.y += parabola * arcHeight;

            RotateInDirection(nextPos - transform.position);
            transform.position = nextPos;
        }
    }
}