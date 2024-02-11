using UnityEngine;

public class Motion : MonoBehaviour
{
    public float speed;
    public Vector3 target;
    public Vector3 direction;
    public Vector3 previousPosition;
    public float arcHeight;
    public float launchAngle;

    virtual public void Setup()
    {
        direction = (target - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    virtual public void Move()
    {
        Debug.Log("Move " + transform.position + " " + target + " " + direction + " " + speed + " " + Time.deltaTime);
        previousPosition = transform.position;
        transform.rotation = Quaternion.LookRotation(direction);
        transform.position += direction * speed * Time.deltaTime;
    }

    virtual public void Hide() { }
}
