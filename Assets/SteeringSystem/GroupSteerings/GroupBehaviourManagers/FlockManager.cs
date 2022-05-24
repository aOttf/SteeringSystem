using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public class FlockManager : GroupBehaviourManager
    {
        [Header("Flock Behaviour Configs")]
        protected float m_seperationWeight;
        protected float m_cohesionWeight;
        protected float m_alignmentWeight;

        protected override void GroupSteering(ISphereMoveable a, ISphereMoveable b)
        {
            Vector3 ab = b.position - a.position;
            Vector3 adir = a.forward;
            Vector3 bdir = b.forward;

            //Seperation and Cohesion
            Vector3 acce = (m_cohesionWeight - m_seperationWeight / (ab.sqrMagnitude + .001f)) * ab;

            //Alignment
            Vector3 alignA = m_alignmentWeight * a.MaxLinearAcceleration * (bdir * a.linearVelocity.magnitude - a.linearVelocity);
            Vector3 alignB = m_alignmentWeight * b.MaxLinearAcceleration * (adir * b.linearVelocity.magnitude - b.linearVelocity);
            a[GroupBehaviour.Flock] += acce + alignA;
            b[GroupBehaviour.Flock] += -acce + alignB;
        }
    }
}