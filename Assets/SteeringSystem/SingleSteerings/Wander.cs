using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

namespace SteeringSystem
{
    /// <summary>
    /// Wandering Behaviour in 2.5D environment
    /// </summary>
    public class Wander : SteeringBehaviour
    {
        #region Gizmos

        [Header("Wander Gizmos")]
        public bool showWanderSphere;
        public Color wanderSphereColor;

        #endregion Gizmos

        protected float m_wanderOrientation;
        protected Vector3 m_targetDirection;

        [Header("Wander Params")]
        public float minTimeStep;
        public float maxTimeStep;
        public float wanderOffset;
        public float wanderRadius;
        public float wanderRate;

        protected Vector3 m_wanderTarget;

        protected override void Awake()
        {
            base.Awake();

            minTimeStep = Mathf.Max(0f, minTimeStep);
            maxTimeStep = Mathf.Max(minTimeStep, maxTimeStep);
        }

        protected override void Start()
        {
            base.Start();
            //StartCoroutine(nameof(NextWander));
            m_wanderOrientation = 0f;
        }

        protected override Vector3 GetSteering()
        {
            //Wander
            //Get target orientation
            m_wanderOrientation += UnityEngine.Random.Range(-wanderRate, wanderRate);
            m_targetDirection = Quaternion.AngleAxis(m_wanderOrientation, m_entity.up) * m_entity.forward;
            return m_entity.maxLinearSpeed * (m_entity.forward * wanderOffset + m_targetDirection * wanderRadius).normalized;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            //Draw Wander Params
            Vector3 to = transform.position + transform.forward * wanderOffset;
            if (showWanderSphere)
            {
                Gizmos.color = wanderSphereColor;
                //wander sphere with offset
                Gizmos.DrawSphere(to, wanderRadius);
            }
        }

        protected IEnumerator NextWander()
        {
            while (true)
            {
                Vector3 spawnPoint = wanderRadius * UnityEngine.Random.onUnitSphere;
                m_wanderTarget = m_entity.maxLinearSpeed * (m_entity.forward * wanderOffset + spawnPoint);

                /*
                m_wanderOrientation += UnityEngine.Random.Range(-wanderRate, wanderRate);
                m_targetDirection = Quaternion.AngleAxis(m_wanderOrientation, m_entity.up) * m_entity.forward;
                m_wanderTarget = m_entity.maxLinearSpeed * (m_entity.forward * wanderOffset + m_targetDirection * wanderRadius).normalized;
                */
                yield return new WaitForSeconds(UnityEngine.Random.Range(minTimeStep, maxTimeStep));
            }
        }
    }
}