using System.Linq;
using UnityEngine;
using static SteeringSystem.SteeringUtilities;

namespace SteeringSystem
{
    public class Cohesion : GroupSteeringBehaviour
    {
        protected override Vector3 GetSteering()
        {
            return m_entity.maxLinearSpeed * m_entity[GroupBehaviour.Cohesion].normalized;
        }
    }
}