using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public class Arrive : SteeringBehaviour
    {
        [Header("Arrive Params")]
        public Transform target;
        public float slowRadius = 5f;

        [Tooltip("This is used if you want to follow a target by an offset. By Default it is zero.")]
        [SerializeField] protected float m_offset;

        protected override Vector3 GetSteering()
        {
            //Get Distance and Direction
            Vector3 toTarget = target.position - m_entity.position;
            float dist = toTarget.magnitude + m_offset;
            Vector3 dir = toTarget.normalized;

            float tgtSpd = m_entity.maxLinearSpeed * ((dist > slowRadius) ? 1 : dist / slowRadius);

            return dir * tgtSpd;
        }
    }
}