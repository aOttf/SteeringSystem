using System;
using System.Collections;
using System.Linq;

using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    [RequireComponent(typeof(CharacterController))]
    public class SteerAgent : MonoBehaviour
    {
        #region Caches

        #region Components

        private CharacterController m_cc;

        #endregion Components

        #region Steering

        private float m_turnAroundSpeed;    //Caches the max turn around speed in rad/s
        private SteeringOutput m_acce;  //Caches the last steering output received
        private Vector3 m_targetLinearVelocity = default; //Caches the target velocity per  frame
        private Vector3 m_linearVelocity = default;   //Current linear velocity of the agent
        private float m_angularVelocity = default;  //Current angular velocity of the agent
        private bool m_isControlled = default;  // Is the agent is manually controlled by player
        private bool m_isGrounded = default;

        private List<SteeringBehaviour> m_steers;
        private SteeringOutput[] m_groupBehaviourOutputCaches = new SteeringOutput[Enum.GetValues(typeof(GroupBehaviour)).Cast<int>().Last<int>()];

        #endregion Steering

        #endregion Caches

        #region Inspector Serializations

        [Header("Agent Size")]
        public float height;
        public float radius;

        [Header("Steering")]
        [Tooltip("The facing of the agent is always synced with the direction of velocity")]
        public bool lookWhereToGo = true;

        [Header("Turn Around")]
        [Tooltip("Apply turn around speed restriction to linear velocity")]
        public bool turnSpeedRestrict = false;

        [Tooltip("Max speed of direction of linear velocity in deg/s")]
        public float maxTurnAroundSpeed = 1f;

        [Space(10)]
        [Tooltip("Max Linear Acceleration of the agent")]
        public float maxLinearAcceleration;

        [Tooltip("Max Linear Speed of the agent")]
        public float maxLinearSpeed;

        [Header("Angular")]
        [Tooltip("Max Angular Acceleration of the agent")]
        public float maxAngularAcceleration;

        [Tooltip("Max Angular Speed of the agent")]
        public float maxAngularSpeed;

        [Tooltip("The Gravity applied to the agent")]
        [SerializeField] private float m_gravity = -19.6f;

        [Tooltip("Is the agent lock on ground")]
        [SerializeField] private bool m_lockOnGround = default;
        [SerializeField] private float m_yLockOffset = 0f;

        [Tooltip("Current Steering Behaviour")]
        [SerializeField] private SteeringBehaviour m_currentSteer;

        [Tooltip("Update Method")]
        public UpdateType type = UpdateType.Update;

        [Tooltip("How often does steering behaviour be executed if the Update Method is TimeStep")]
        public float steeringRoutineTimeStep = .02f;

        [SerializeField] private bool m_syncSlope;

        [SerializeField] private Vector3 m_planeNormal;

        [Header("Gizmos")]
        public bool showLinearVelocity;
        public bool showDirection;

        #endregion Inspector Serializations

        #region Transform

        public Vector3 linearVelocity
        { get => m_linearVelocity; set { m_linearVelocity = value; m_isControlled = true; } }

        public float angularVelocity
        { get => m_angularVelocity; set { m_angularVelocity = value; m_isControlled = true; } }

        public Vector3 position => transform.position;

        public Vector3 forward => transform.forward;

        public Vector3 up => transform.up;

        #endregion Transform

        #region Steering

        public SteeringBehaviour GetSteeringBehaviour(string pSteerTag) => m_steers.FirstOrDefault<SteeringBehaviour>(st => string.Equals(st.steerTag, pSteerTag));

        public SteeringBehaviour CurrentSteer { get => m_currentSteer; set => m_currentSteer = value; }
        public SteeringOutput this[GroupBehaviour behaviour] { get => m_groupBehaviourOutputCaches[(int)behaviour]; set => m_groupBehaviourOutputCaches[(int)behaviour] = value; }

        #endregion Steering

        private void Awake()
        {
            //Components
            m_cc = GetComponent<CharacterController>();
            m_cc.radius = radius;
            m_cc.height = height;
            m_cc.minMoveDistance = float.Epsilon;
            Physics.autoSyncTransforms = true;

            //Steers
            m_steers = new List<SteeringBehaviour>();
            m_steers.AddRange(GetComponents<SteeringBehaviour>());
        }

        // Start is called before the first frame update
        private void Start()
        {
            StartSteering();
        }

        /// <summary>
        /// Update the Agent's Current Linear Velocity
        /// </summary>
        private void UpdateLinearVelocity()
        {
            //Calculate target velocity
            m_targetLinearVelocity = Vector3.ClampMagnitude(m_linearVelocity + m_acce.Linear * Time.deltaTime, maxLinearSpeed);

            ////Implementation1 : If turnAround restricting, Clamp rotating angle
            //if (turnSpeedRestrict)
            //    m_linearVelocity = Vector3.RotateTowards(m_linearVelocity, m_targetLinearVelocity, Time.deltaTime * Mathf.PI * maxTurnAroundSpeed / 180f, maxLinearAcceleration * Time.deltaTime);
            //else
            //    m_linearVelocity = Vector3.ClampMagnitude(m_linearVelocity + m_acce.Linear * Time.deltaTime, maxLinearSpeed);

            //Implementation2 : Target Velocity
            if (turnSpeedRestrict)
                m_linearVelocity = Vector3.RotateTowards(m_linearVelocity, m_acce.Linear, Time.deltaTime * Mathf.PI * maxTurnAroundSpeed / 180f, maxLinearAcceleration * Time.deltaTime);
            else
                m_linearVelocity = m_acce.Linear;
        }

        /// <summary>
        /// Update the Agent's Current Angular Velocity
        /// </summary>
        private void UpdateAngularVelocity()
        {
            //Calculate target velocity
            m_angularVelocity = Mathf.Min(m_angularVelocity + m_acce.Angular * Time.deltaTime, maxAngularSpeed);
        }

        /// <summary>
        ///
        /// </summary>
        private void UpdatePosition()
        {
            //Move
            if (m_lockOnGround)
            {
                //Lock the yOffset
                transform.position = new Vector3(transform.position.x, m_yLockOffset, transform.position.z);
                Physics.SyncTransforms();

                m_cc.SimpleMove(m_linearVelocity);
            }
            else
            {
                //Not Locked on ground, apply gravity to the move
                m_cc.Move((m_linearVelocity + up * m_gravity) * Time.deltaTime);
            }
        }

        /// <summary>
        /// Update the facing direction of the agent
        /// </summary>
        private void UpdateFace()
        {
            if (lookWhereToGo && m_linearVelocity != Vector3.zero)
                transform.forward = m_linearVelocity;
            else if (!lookWhereToGo)
                transform.forward = Quaternion.AngleAxis(m_angularVelocity * Time.deltaTime, up) * transform.forward;
        }

        // Update is called once per frame
        private void Update()
        {
            //If not manually controlled by others
            if (!m_isControlled)
            {
                //Update Linear Velocity
                UpdateLinearVelocity();

                //If LookWhere2Go
                //Update angular velocity
                if (!lookWhereToGo)
                    UpdateAngularVelocity();
            }

            UpdatePosition(); UpdateFace();
            //If Where2Go
            //Sync forward with linear vel

            //If !lookWheretogo, Rotate

            //Grounded Detection
            m_isGrounded = m_cc.isGrounded;
        }

        private void OnDrawGizmos()
        {
            //Direction of the Agent
            if (showDirection)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(position, position + forward * 2);
            }

            //Linear Velocity of the Agent
            if (Application.isPlaying)
            {
                if (showLinearVelocity)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawLine(position, position + linearVelocity);
                }
            }
        }

        /// <summary>
        /// Clean the old steering output data and start steering routine
        /// </summary>
        public void StartSteering()
        {
            if (m_isControlled)
            {
                ClearSteeringData();
                StartCoroutine(nameof(SteeringCoroutine));
                m_isControlled = false;
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
            if (!m_isControlled)
            {
                StopCoroutine(nameof(SteeringCoroutine));
                m_isControlled = true;
            }
        }

        /// <summary>
        /// cleans the result from the steering behaivour executed in the last time
        /// </summary>
        private void ClearSteeringData()
        {
            m_acce = SteeringOutput.ZeroSteering;
            for (int i = 0; i < m_groupBehaviourOutputCaches.Length; i++)
                m_groupBehaviourOutputCaches[i] = default;
        }

        private IEnumerator SteeringCoroutine()
        {
            while (true)
            {
                m_acce = SteeringOutput.Clip(m_currentSteer.Steering, maxLinearAcceleration, maxAngularAcceleration);
                //if (m_syncSlope)
                //    m_acce.Linear = Vector3.ProjectOnPlane(m_acce.Linear, m_planeNormal);

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