using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SteeringSystem.SteeringUtilities;

namespace SteeringSystem
{
    public class CohesionManager : GroupBehaviourManager
    {
        #region Caches

        private Vector3 m_acce;

        #endregion Caches

        protected override void Awake()
        {
            base.Awake();
            m_groupBehaviourIndex = GroupBehaviour.Cohesion;
        }

        protected override void GroupSteering(ISphereMoveable a, ISphereMoveable b)
        {
            m_acce = b.position - a.position;
            a[GroupBehaviour.Cohesion] += m_acce;
            b[GroupBehaviour.Cohesion] -= m_acce;
        }
    }
}