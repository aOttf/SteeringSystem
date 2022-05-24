using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public class Arrive : SteeringBehaviour
    {
        [SerializeField] protected Vector3 m_targetPosition;
        [SerializeField] protected Transform m_target;

        [Tooltip("This is used if you want to follow a target by an offset. By Default it is zero.")]
        [SerializeField] protected Vector3 m_offset = Vector3.zero;
        public float slowRadius = 5f;

        public Vector3 TargetPosition
        {
            get => ((m_target == null) ? m_targetPosition : m_target.position) - m_offset;
            set
            {
                m_targetPosition = value; m_target = null;
            }
        }

        public Transform Target { get => m_target; set => m_target = value; }

        protected override SteeringOutput GetSteering()
        {
            //Get Distance and Direction
            Vector3 toTarget = TargetPosition - transform.position;
            float dist = toTarget.magnitude;
            Vector3 dir = toTarget.normalized;

            float tgtSpd = m_maxLinearSpeed * ((dist > slowRadius) ? 1 : dist / slowRadius);

            return SteeringOutput.LinearSteering(dir * tgtSpd);
        }

        public override string ToString() => base.ToString() + "Arrive";
    }
}