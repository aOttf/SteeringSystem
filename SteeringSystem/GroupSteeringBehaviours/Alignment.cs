using System.Linq;
using UnityEngine;
using static SteeringSystem.SteeringUtilities;

namespace SteeringSystem
{
    public class Alignment : GroupSteeringBehaviour
    {
        #region Debug Options

        [Header("Alignment Debuggings")]
        public bool showDirection;
        public Color directionColor;

        #endregion Debug Options

        protected Vector3 m_centerOfHead;

        protected override void Awake() => base.Awake();

        protected override SteeringOutput GetSteering()
        {
            FindNeighbours();

            if (m_neighbours.Count != 0)
            {
                m_centerOfHead = new Vector3
                    (m_neighbours.Average(neighbour => neighbour.forward.x),
                    m_neighbours.Average(neighbour => neighbour.forward.y),
                    m_neighbours.Average(neighbour => neighbour.forward.z)
                    ).normalized;

                return MatchVelocity
                    (
                    m_centerOfHead.normalized * m_entity.linearVelocity.magnitude,
                    m_entity.linearVelocity,
                    m_maxLinearAcceleration
                    );
            }
            else
                return SteeringOutput.ZeroSteering;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (Application.isPlaying)
            {
                if (showDirection && m_neighbours != null && m_neighbours.Count != 0)
                {
                    Gizmos.color = directionColor;
                    Gizmos.DrawLine(m_entity.position, m_entity.position + m_centerOfHead);
                }
            }
        }
    }
}