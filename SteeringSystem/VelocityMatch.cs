using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public class VelocityMatch : SteeringBehaviour
    {
        public float time2Match = .5f;

        [SerializeField] protected Vector3 m_targetVelocity;
        [SerializeField] protected SteeringAgent m_target;

        public Vector3 TargetVelocity
        {
            get => (m_target == null) ? m_targetVelocity : m_target.linearVelocity;
            set
            {
                m_targetVelocity = value; m_target = null;
            }
        }

        public SteeringAgent Target { get => m_target; set => m_target = value; }

        protected override SteeringOutput GetSteering()
         => SteeringOutput.LinearSteering(Vector3.ClampMagnitude((TargetVelocity - m_agent.linearVelocity) / time2Match, m_maxLinearAcceleration));

        public override string ToString() => base.ToString() + "VelocityMatch";
    }
}