using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public class Flock : GroupSteeringBehaviour
    {
        protected override void Awake()
        {
            //Init
            base.Awake();
        }

        protected override Vector3 GetSteering()
        {
            return m_entity.maxLinearSpeed * m_entity[GroupBehaviour.Flock].normalized;
        }
    }
}