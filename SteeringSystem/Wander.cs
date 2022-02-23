using UnityEngine;
using UnityEditor;

namespace SteeringSystem
{
    /// <summary>
    /// Wandering Behaviour in 2.5D environment
    /// </summary>
    public class Wander : SteeringBehaviour
    {
        #region Debug Options

        public bool showWanderSphere;
        public Color wanderSphereColor;

        #endregion Debug Options

        protected float m_wanderOrientation;
        protected Vector3 m_targetDirection;

        [Space(50)]
        public float wanderOffset;
        public float wanderRadius;
        public float wanderRate;

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

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            //Draw Wander Params
            Vector3 to = transform.position + transform.forward * wanderOffset;
            if (showWanderSphere)
            {
                Gizmos.color = wanderSphereColor;
                //wander sphere with offset
                Gizmos.DrawSphere(to, wanderRadius);
            }
        }
    }
}