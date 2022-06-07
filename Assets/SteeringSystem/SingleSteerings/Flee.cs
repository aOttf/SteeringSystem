using UnityEngine;

namespace SteeringSystem
{
    public class Flee : SteeringBehaviour
    {
        [Header("Flee Params")]
        public Transform target;

        protected override Vector3 GetSteering() => m_entity.maxLinearSpeed * (m_entity.position - target.position).normalized;
    }
}