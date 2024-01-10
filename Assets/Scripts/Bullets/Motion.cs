using UnityEngine;

public class Motion : MonoBehaviour
{
    public float speed;
    public Vector3 target;
    public Vector3 direction;
    public Vector3 previousPosition;
    public float arcHeight;

    virtual public void Setup() {
        direction = (target - transform.position).normalized;
    }

    virtual public void Move() {
        previousPosition = transform.position;
        transform.rotation = Quaternion.LookRotation(direction);
        transform.position += direction * speed * Time.deltaTime;
    }
}
