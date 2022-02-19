using UnityEngine;
using UnityEditor;

namespace SteeringSystem
{
    /// <summary>
    /// Wandering Behaviour in 2.5D environment
    /// </summary>
    public class Wander : SteeringBehaviour
    {
        protected float m_wanderOrientation;
        protected Vector3 m_targetDirection;

        public float wanderOffset;
        public float wanderRadius;
        public float wanderRate;

        #region Debug Options

        [Header("Gizmos")]
        public bool showWanderOffset;
        public bool showWanderRadius;

        #endregion Debug Options

        protected override void Start()
        {
            base.Start();
            m_wanderOrientation = 0f;
        }

        protected override SteeringOutput GetSteering()
        {
            //Wander
            //Get target orientation
            m_wanderOrientation += UnityEngine.Random.Range(-wanderRate, wanderRate);
            m_targetDirection = Quaternion.AngleAxis(m_wanderOrientation, m_entity.up) * m_entity.forward;
            return SteeringOutput.LinearSteering(m_maxLinearAcceleration * ((m_entity.forward * wanderOffset + m_targetDirection * wanderRadius).normalized));
        }

        public override string ToString() => base.ToString() + "Wander";

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            //Draw Wander Params
            Gizmos.color = Color.yellow;
            Vector3 to = transform.position + transform.forward * wanderOffset;
            if (showWanderOffset)
            {
                //wander sphere with offset
                Gizmos.DrawLine(transform.position, to);
            }
            if (showWanderRadius)
                Gizmos.DrawSphere(to, wanderRadius);
        }
    }
}