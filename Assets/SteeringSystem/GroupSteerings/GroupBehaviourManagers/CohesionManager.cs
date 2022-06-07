using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SteeringSystem.SteeringUtilities;

namespace SteeringSystem
{
    public class CohesionManager : GroupBehaviourManager
    {
        protected override void Awake()
        {
            base.Awake();
            m_groupBehaviourIndex = GroupBehaviour.Cohesion;
        }

        protected override void GroupSteering(SteerAgent a, SteerAgent b)
        {
            Vector3 ab = b.position - a.position;
            a[GroupBehaviour.Cohesion] += ab;
            b[GroupBehaviour.Cohesion] -= ab;
        }
    }
}