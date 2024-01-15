using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class AirPlaneRotation : MonoBehaviour
{
    private NavMeshAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 desiredDirection = agent.desiredVelocity.normalized;
        desiredDirection.y = 0; // Ignore the y-axis difference in direction

        if (desiredDirection != Vector3.zero) // Avoid creating a Quaternion from a zero vector
        {
            Quaternion desiredRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
            float rotationSpeed = 5f; // Adjust this value to change the speed of rotation

            // Interpolate between the current rotation and the desired rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
