using System.Linq;
using UnityEngine;
using static SteeringSystem.SteeringUtilities;

namespace SteeringSystem
{
    public class Cohesion : GroupSteeringBehaviour
    {
        #region Debug Options

        [Header("Cohesion Debuggings")]
        public bool showCenter;
        public Color centerColor;

        #endregion Debug Options

        protected Vector3 m_centerOfMass;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override SteeringOutput GetSteering()
        {
            FindNeighbours();
            if (m_neighbours.Count != 0)
            {
                m_centerOfMass =
                    new Vector3(m_neighbours.
                    Average(neighbour => neighbour.position.x),
                    m_neighbours.Average(neighbour => neighbour.position.y),
                    m_neighbours.Average(neighbour => neighbour.position.z));

                return MatchPosition(m_centerOfMass, m_entity.position, m_maxLinearAcceleration);
            }
            else
                return SteeringOutput.ZeroSteering;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (Application.isPlaying)
            {
                if (showCenter && m_neighbours != null && m_neighbours.Count != 0)
                {
                    Gizmos.color = centerColor;
                    Gizmos.DrawLine(m_entity.position, m_centerOfMass);
                }
            }
        }
    }
}