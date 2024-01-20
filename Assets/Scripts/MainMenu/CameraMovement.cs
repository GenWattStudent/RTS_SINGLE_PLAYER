using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float maxDistance = 10f;
    private int direction = 1;

    private void MoveCameraAnimation() {
        var newPosition = new Vector3(transform.position.x + maxDistance * direction, transform.position.y, transform.position.z);
        Debug.Log(Vector3.Lerp(transform.position, newPosition, Time.deltaTime * speed));
        Camera.main.transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * speed);
    }

    void Update()
    {
        if (transform.position.x >= maxDistance) direction = -1;
        if (transform.position.x <= -maxDistance) direction = 1;

        MoveCameraAnimation();
    }
}
