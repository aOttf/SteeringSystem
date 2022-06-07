using System.Linq;
using UnityEngine;
using static SteeringSystem.SteeringUtilities;

namespace SteeringSystem
{
    public class Alignment : GroupSteeringBehaviour
    {
        protected override Vector3 GetSteering()
        {
            return m_entity.maxLinearSpeed * m_entity[GroupBehaviour.Alignment].normalized;
        }
    }
}