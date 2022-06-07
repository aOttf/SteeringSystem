using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SteeringSystem
{
    public class Seperation : GroupSteeringBehaviour
    {
        protected override Vector3 GetSteering()
        {
            return m_entity.maxLinearSpeed * m_entity[GroupBehaviour.Seperation].normalized;
        }
    }
}