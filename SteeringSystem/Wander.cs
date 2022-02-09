using UnityEngine;
using UnityEditor;

namespace SteeringSystem
{
    public class Wander : SteeringBehaviour
    {
        protected Vector3 m_targetDirection;

        public float wanderOffset;
        public float wanderRadius;
        public float wanderRate;

        protected override SteeringOutput GetSteering()
        {
            //Wander
            //Get target orientation
            m_targetDirection = Quaternion.AngleAxis(Random.Range(-wanderRate, wanderRate), Vector3.up) * transform.forward;
            return SteeringOutput.LinearSteering(m_maxLinearAcceleration * ((transform.forward * wanderOffset + m_targetDirection * wanderRadius).normalized));
        }

        public override string ToString() => base.ToString() + "Wander";

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            //Draw Wander Params
            Gizmos.color = Color.yellow;
            Vector3 to = transform.position + transform.forward * wanderOffset;
            //wander sphere with offset
            Gizmos.DrawLine(transform.position, to);
            Gizmos.DrawSphere(to, wanderRadius);
            //linear acceleration
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + m_targetDirection);
        }
    }
}