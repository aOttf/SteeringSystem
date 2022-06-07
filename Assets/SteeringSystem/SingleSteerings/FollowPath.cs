using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public enum FollowMode
    {
        Linear, InverseLinear, Circular
    }

    public class FollowPath : SteeringBehaviour
    {
        [Header("FollowPath Params")]
        [SerializeField] protected FollowMode mode;
        [SerializeField] protected List<Vector3> m_path;
        [SerializeField] protected float m_threshold;

        [Header("FollowPath Gizmos")]
        public bool drawPath;
        public Color drawPathColor;

        protected int m_idx;

        public Vector3 AdvanceTarget()
        {
            switch (mode)
            {
                case FollowMode.Linear:
                    if (m_idx == m_path.Count - 1)
                    {
                        mode = FollowMode.InverseLinear;
                        return m_path[--m_idx];
                    }
                    else
                        return m_path[++m_idx];

                case FollowMode.InverseLinear:
                    if (m_idx == 0)
                    {
                        mode = FollowMode.Linear;
                        return m_path[++m_idx];
                    }
                    else
                        return m_path[--m_idx];

                case FollowMode.Circular:
                    return m_path[++m_idx % m_path.Count];

                default:
                    throw new System.ArgumentException();
            }
        }

        public Vector3 CurrentTarget => m_path[m_idx];

        public Vector3 RemaingDistance => CurrentTarget - m_entity.position;

        public bool HasReach => RemaingDistance.sqrMagnitude < m_threshold * m_threshold;

        protected override Vector3 GetSteering()
        {
            //Check if has reached the current target
            if (HasReach)
                AdvanceTarget();

            //Seek to the target
            return m_entity.maxLinearSpeed * (CurrentTarget - m_entity.position).normalized;
        }

        protected override void OnDrawGizmosSelected()
        {
            void DrawPath()
            {
                for (int i = 0; i < m_path.Count - 1; i++)
                    Gizmos.DrawLine(m_path[i], m_path[i + 1]);

                //If Circular Path
                if (mode == FollowMode.Circular)
                    Gizmos.DrawLine(m_path[m_path.Count - 1], m_path[0]);
            }
            base.OnDrawGizmosSelected();
            if (drawPath)
            {
                Gizmos.color = drawPathColor;
                DrawPath();
            }
        }
    }
}