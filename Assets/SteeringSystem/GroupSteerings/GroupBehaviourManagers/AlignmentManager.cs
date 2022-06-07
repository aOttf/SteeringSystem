using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public class AlignmentManager : GroupBehaviourManager
    {
        protected override void Awake()
        {
            base.Awake();
            m_groupBehaviourIndex = GroupBehaviour.Alignment;
        }

        protected override void GroupSteering(SteerAgent a, SteerAgent b)
        {
            a[m_groupBehaviourIndex] += b.forward;
            b[m_groupBehaviourIndex] += a.forward;
        }
    }
}