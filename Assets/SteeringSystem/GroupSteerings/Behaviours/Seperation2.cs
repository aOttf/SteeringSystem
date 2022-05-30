using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public class Seperation2 : GroupSteeringBehaviour
    {
        protected override SteeringOutput GetSteering()
        {
            return m_entity[GroupBehaviour.Seperation];
        }
    }
}