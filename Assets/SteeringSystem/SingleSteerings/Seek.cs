using UnityEngine;

using static SteeringSystem.SteeringUtilities;

namespace SteeringSystem
{
    public class Seek : SteeringBehaviour
    {
        [Space(50)]
        [Tooltip("The Target transform seeking to; set this will hide target position")]
        public Transform target;

        protected override Vector3 GetSteering() => m_entity.maxLinearSpeed * (target.position - m_entity.position).normalized;
    }
}