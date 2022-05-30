using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public class Cohesion2 : GroupSteeringBehaviour
    {
        protected override SteeringOutput GetSteering()
        {
            return m_entity[GroupBehaviour.Cohesion];
        }
    }
}