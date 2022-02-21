using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SteeringSystem
{
    public class Seperation : GroupSteeringBehaviour
    {
        protected List<Vector3> m_neighbourStrengths;

        protected Vector3 m_centerOfStrength;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override SteeringOutput GetSteering()
        {
            FindNeighbours();

            if (m_neighbours.Count != 0)
            {
                //Inverse Squart Root
                m_neighbourStrengths = m_neighbours.
                    ConvertAll
                    (neighbour => (m_entity.position - neighbour.position) / (m_entity.position - neighbour.position).sqrMagnitude);

                m_centerOfStrength = new Vector3(m_neighbourStrengths.Average(strength => strength.x), m_neighbourStrengths.Average(strength => strength.y), m_neighbourStrengths.Average(strength => strength.z));

                return SteeringOutput.LinearSteering(m_centerOfStrength.normalized * m_maxLinearAcceleration);
            }
            else
                return SteeringOutput.ZeroSteering;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
        }
    }
}