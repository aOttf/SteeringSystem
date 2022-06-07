using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public class MatchDirection : SteeringBehaviour
    {
        public Vector3 desiredDirection;

        protected override Vector3 GetSteering()
        {
            return desiredDirection.normalized * m_entity.maxLinearSpeed;
        }
    }
}