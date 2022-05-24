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

        protected override void GroupSteering(ISphereMoveable a, ISphereMoveable b)
        {
            a[m_groupBehaviourIndex] += a.MaxLinearAcceleration * (b.forward * a.linearVelocity.magnitude - a.linearVelocity);
            b[m_groupBehaviourIndex] += b.MaxLinearAcceleration * (a.forward * b.linearVelocity.magnitude - b.linearVelocity);
        }
    }
}