using Unity.Netcode;
using UnityEngine;

public class Motion : NetworkBehaviour
{
    public float speed;
    public Vector3 target;
    public Vector3 direction;
    public Vector3 previousPosition;
    public float arcHeight;
    public float launchAngle;

    virtual public void Setup()
    {
        previousPosition = transform.position;
        direction = (target - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    virtual public void Move()
    {
        previousPosition = transform.position;
        transform.rotation = Quaternion.LookRotation(direction);
        transform.position += direction * speed * Time.deltaTime;
    }

    virtual public void Hide() { }
}
