using System.Linq;
using UnityEngine;
using static SteeringSystem.SteeringUtilities;

namespace SteeringSystem
{
    public class Alignment : GroupSteeringBehaviour
    {
        protected override SteeringOutput GetSteering()
        {
            return m_entity[GroupBehaviour.Alignment];
        }
    }
}