using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    [RequireComponent(typeof(SteeringBehaviour), typeof(ISphereMoveable))]
    public class SteeringController : MonoBehaviour
    {
        #region Update Method

        /// <summary>
        /// The Update timestep
        /// </summary>
        public enum UpdateType
        {
            Update, FixedUpdate, TimeStep
        }

        [Tooltip("Update Method")]
        public UpdateType type = UpdateType.Update;

        [Tooltip("How often does steering behaviour be executed if the Update Method is TimeStep")]
        public float steeringRoutineTimeStep = .02f;

        #endregion Update Method

        [SerializeField] private SteeringBehaviour m_currentSteering;   //The current steering behaviour chosen to be executed; Agent executes only one steeering behaviour in a period
        public SteeringBehaviour CurrentSteering { get => m_currentSteering; set => m_currentSteering = value; }

        [SerializeField] private bool m_syncSlope;

        [SerializeField] private Vector3 m_planeNormal;

        /// <summary>
        /// Is the steering running?
        /// </summary>
        public bool IsRunning => m_isRunning;

        /// <summary>
        /// Is the Steering Controller sync with a sloped plane?
        /// </summary>
        public bool SyncSlope { get => m_syncSlope; set => m_syncSlope = value; }

        /// <summary>
        /// The Sloped Plane Normal
        /// </summary>
        public Vector3 PlaneNormal { get => m_planeNormal; set => m_planeNormal = value; }

        #region Caches

        private ISphereMoveable m_entity;
        private List<SteeringBehaviour> m_steerings;  //Caches all the steering behaviors attached to this

        #endregion Caches

        #region Result

        private SteeringOutput m_acce;   //Internal store of the result of steering behaviour

        public SteeringOutput result => m_acce;
        public Vector3 linearAcceleration => m_acce.Linear;
        public float angularAcceleration => m_acce.Angular;

        #endregion Result

        #region Internal State Data

        private bool m_isRunning = false;

        #endregion Internal State Data

        /// <summary>
        /// Clean the old steering output data and start steering routine
        /// </summary>
        public void StartSteering()
        {
            if (!m_isRunning)
            {
                ClearSteeringData();
                StartCoroutine(nameof(SteeringCoroutine));
                m_isRunning = true;
            }
        }

        /// <summary>
        /// Clean the old steering output data and stop steering routine
        /// </summary>
        public void StopSteering()
        {
            ClearSteeringData();
            PauseSteering();
        }

        /// <summary>
        /// pause steering routine but does not clean the old data
        /// </summary>
        public void PauseSteering()
        {
            if (m_isRunning)
            {
                StopCoroutine(nameof(SteeringCoroutine));
                m_isRunning = false;
            }
        }

        /// <summary>
        /// cleans the result from the steering behaivour executed in the last time
        /// </summary>
        public void ClearSteeringData() => m_acce = SteeringOutput.ZeroSteering;

        public SteeringBehaviour GetSteeringBehaviour<T>() where T : SteeringBehaviour
        {
            return GetComponent<SteeringBehaviour>();
        }

        public SteeringBehaviour[] GetSteeringBehaviours<T>() where T : SteeringBehaviour
        {
            return GetComponents<SteeringBehaviour>();
        }

        private void Awake()
        {
            m_entity = GetComponent<ISphereMoveable>();
            m_steerings = new List<SteeringBehaviour>();
            m_steerings.AddRange(GetSteeringBehaviours<SteeringBehaviour>());
        }

        private IEnumerator SteeringCoroutine()
        {
            while (true)
            {
                m_acce = m_currentSteering.Steering;
                if (m_syncSlope)
                    m_acce.Linear = Vector3.ProjectOnPlane(m_acce.Linear, m_planeNormal);

                //Yield time gap
                switch (type)
                {
                    case UpdateType.Update:
                        yield return null;
                        break;

                    case UpdateType.FixedUpdate:
                        yield return new WaitForFixedUpdate();
                        break;

                    case UpdateType.TimeStep:
                        yield return new WaitForSeconds(steeringRoutineTimeStep);
                        break;
                }
            }
        }
    }
}