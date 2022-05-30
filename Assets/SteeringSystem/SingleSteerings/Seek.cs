using UnityEngine;

using static SteeringSystem.SteeringUtilities;

namespace SteeringSystem
{
    public class Seek : SteeringBehaviour
    {
        #region Debug Options

        public bool showTarget;
        public Color targetColor;

        #endregion Debug Options

        [Space(50)]
        [Tooltip("The Target Position seeking to")]
        public Vector3 targetPosition;

        [Tooltip("The Target transform seeking to; set this will hide target position")]
        public Transform target;

        [Tooltip("Match by Velocity or Match by Position")]
        public MatchMode mode = MatchMode.MatchPosition;

        public Vector3 TargetPosition
        {
            get => (target == null) ? targetPosition : target.position;
            set { targetPosition = value; target = null; }
        }

        protected override SteeringOutput GetSteering() => SeekTo(TargetPosition);

        protected SteeringOutput SeekTo(Vector3 pTarget)
        {
            switch (mode)
            {
                case MatchMode.MatchPosition:
                    return MatchPosition(pTarget, m_entity.position, m_maxLinearAcceleration);

                case MatchMode.MatchVelocity:
                    return MatchVelocity((pTarget - m_entity.position) * m_maxLinearAcceleration, m_entity.linearVelocity, m_maxLinearAcceleration);

                default:
                    throw new System.NotImplementedException();
            }
        }

        public override string ToString() => base.ToString() + "Seek";

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            if (showTarget)
            {
                //Draw Target Position
                Gizmos.color = targetColor;
                Gizmos.DrawLine(m_entity.position, TargetPosition);
            }
        }
    }
}