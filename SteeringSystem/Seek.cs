using UnityEngine;

namespace SteeringSystem
{
    public class Seek : SteeringBehaviour
    {
        [SerializeField] protected Vector3 m_targetPosition;
        [SerializeField] protected Transform m_target;

        [Header("Gizmos")]
        public bool showTarget;

        public Vector3 TargetPosition
        {
            get => (m_target == null) ? m_targetPosition : m_target.position;
            set { m_targetPosition = value; m_target = null; }
        }

        public Transform Target { get => m_target; set => m_target = value; }

        //      protected override SteeringOutput GetSteering() => SteeringOutput.LinearSteering(((TargetPosition - transform.position).normalized * m_maxLinearSpeed - m_agent.linearVelocity) * m_maxLinearAcceleration);
        protected override SteeringOutput GetSteering() => SteeringOutput.LinearSteering((TargetPosition - transform.position).normalized * m_maxLinearAcceleration);

        public override string ToString() => base.ToString() + "Seek";

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if (showTarget)
            {
                //Draw Target Position
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, TargetPosition);
            }
        }
    }
}