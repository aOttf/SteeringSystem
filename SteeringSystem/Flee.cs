using UnityEngine;

namespace SteeringSystem
{
    public class Flee : SteeringBehaviour
    {
        public Vector3 targetPosition;
        public Transform target;

        public Vector3 TargetPosition => (target == null) ? targetPosition : target.position;

        public void SetTarget(Transform pTarget) => target = pTarget;

        public void SetTargetPosition(Vector3 pTargetPosition)
        {
            targetPosition = pTargetPosition;
            target = null;
        }

        protected override SteeringOutput GetSteering() => SteeringOutput.LinearSteering((transform.position - TargetPosition).normalized * m_maxLinearAcceleration);

        public override string ToString() => base.ToString() + "Flee";
    }
}