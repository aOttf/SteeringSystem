using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public class SeperationManager : GroupBehaviourManager
    {
        protected override void Awake()
        {
            base.Awake();
            m_groupBehaviourIndex = GroupBehaviour.Seperation;
        }

        protected override void GroupSteering(SteerAgent a, SteerAgent b)
        {
            Vector3 ba = a.position - b.position;
            Vector3 acce = ba / (ba.sqrMagnitude + .001f);
            a[m_groupBehaviourIndex] += acce;
            b[m_groupBehaviourIndex] -= acce;
        }
    }
}