using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SteeringSystem
{
    public class Seperation : GroupSteeringBehaviour
    {
        protected override SteeringOutput GetSteering()
        {
            return m_entity[GroupBehaviour.Seperation];
        }
    }
}