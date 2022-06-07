using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public class FlockManager : GroupBehaviourManager
    {
        [Header("Flock Behaviour Configs")]
        [SerializeField] protected float m_seperationWeight;
        [SerializeField] protected float m_cohesionWeight;
        [SerializeField] protected float m_alignmentWeight;

        protected override void GroupSteering(SteerAgent a, SteerAgent b)
        {
            Vector3 ab = b.position - a.position;

            //Seperation and Cohesion
            Vector3 acce = (m_cohesionWeight - m_seperationWeight / (ab.sqrMagnitude + .001f)) * ab;

            //Alignment
            Vector3 alignA = m_alignmentWeight * b.forward;
            Vector3 alignB = m_alignmentWeight * a.forward;

            a[GroupBehaviour.Flock] += acce + alignA;
            b[GroupBehaviour.Flock] += -acce + alignB;
        }
    }
}