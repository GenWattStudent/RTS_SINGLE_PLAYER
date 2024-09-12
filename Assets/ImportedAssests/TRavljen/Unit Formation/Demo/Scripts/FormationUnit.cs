using UnityEngine;
using UnityEngine.AI;

namespace TRavljen.UnitFormation.Demo
{

    /// <summary>
    /// Simple unit for demonstration of movement to target position
    /// with built-in Unity AI system. It also faces the angle of
    /// the formation after it reaches its destination.
    /// </summary>
    public class FormationUnit : MonoBehaviour
    {

        private NavMeshAgent agent;

        private float facingAngle = 0f;

        private bool faceOnDestination = false;

        [SerializeField, Tooltip("Speed with which the unit will rotate towards the formation facing angle.")]
        private float rotationSpeed = 100;

        /// <summary>
        /// Specifies if rotating towards the facing angle is enabled.
        /// Set this to 'false' if you wish to manually handle synced rotation of
        /// units in rotation.
        /// </summary>
        [HideInInspector]
        public bool FacingRotationEnabled = true;

        public bool IsWithinStoppingDistance =>
            Vector3.Distance(transform.position, agent.destination) <= agent.stoppingDistance;

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            // If unit is within its stopping distance, start rotating towards the facing angle of the formation.
            if (Vector3.Distance(agent.destination, transform.position) < agent.stoppingDistance &&
                faceOnDestination &&
                FacingRotationEnabled)
            {
                float currentAngle = transform.rotation.eulerAngles.y;
                var newAngle = Mathf.MoveTowardsAngle(currentAngle, facingAngle, rotationSpeed * Time.deltaTime);

                if (Mathf.Approximately(facingAngle, newAngle))
                {
                    faceOnDestination = false;
                }

                transform.rotation = Quaternion.AngleAxis(newAngle, Vector3.up);
            }
        }

        public void SetTargetDestination(Vector3 newTargetDestination, float newFacingAngle)
        {
            // If start has not yet been called - this can happen if unit was instantiated in scene
            // and this method invoked immediately after.
            if (agent == null)
            {
                agent = GetComponent<NavMeshAgent>();
            }

            faceOnDestination = true;
            agent.destination = newTargetDestination;
            facingAngle = newFacingAngle;
        }

    }

}