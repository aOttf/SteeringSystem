using System;
using System.Collections;
using System.Linq;

using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    /// <summary>
    /// The Update timestep
    /// </summary>
    public enum UpdateType
    {
        Update, FixedUpdate, TimeStep
    }

    [RequireComponent(typeof(CharacterController))]
    public class SteerAgent : MonoBehaviour
    {
        #region Caches

        #region Components

        protected CharacterController m_cc;

        #endregion Components

        #region Steering

        protected Vector3 m_desiredVelocity = default; //Caches the target velocity per  frame
        protected Vector3 m_velocity = default;   //Current linear velocity of the agent

        protected List<SteeringBehaviour> m_steers;
        protected Vector3[] m_groupBehaviourOutputCaches = new Vector3[Enum.GetValues(typeof(GroupBehaviour)).Cast<int>().Last<int>() + 1];

        #endregion Steering

        #endregion Caches

        #region Inspector Serializations

        [Header("Agent Size")]
        public float height;
        public float radius;

        [Header("Steering")]
        // [Tooltip("The facing of the agent is always synced with the direction of velocity")]
        // public bool lookWhereToGo = true;

        [Header("Turn Around")]
        [Tooltip("Apply turn around speed restriction to linear velocity")]
        public bool turnSpeedRestrict = true;

        [Tooltip("Max speed of direction of linear velocity in deg/s")]
        public float maxTurnAroundSpeed = 1f;

        [Tooltip("Stop steering and start looking at the target. By default the target is the agent's current forward direction. Stop the agent by setting it to true.")]
        public bool lookAt = false;
        [SerializeField] protected Transform m_lookAtTarget = null;

        public void SetLookAt(bool islookAt, Transform target = null)
        {
            lookAt = islookAt;
            m_lookAtTarget = target;
        }

        [Space(10)]
        [Tooltip("Max Linear Acceleration of the agent")]
        public float maxLinearAcceleration;

        [Tooltip("Max Linear Speed of the agent")]
        public float maxLinearSpeed;

        [Tooltip("The Gravity applied to the agent")]
        [SerializeField] protected float m_gravity = -19.6f;

        [Tooltip("Is the agent lock on ground")]
        [SerializeField] protected bool m_lockOnGround = default;
        [SerializeField] protected float m_yLockOffset = 0f;

        [Tooltip("Current Steering Behaviour")]
        [SerializeField] protected SteeringBehaviour m_currentSteer;

        [Tooltip("Update Method")]
        public UpdateType type = UpdateType.Update;

        [Tooltip("How often does steering behaviour be executed if the Update Method is TimeStep")]
        public float steeringRoutineTimeStep = .02f;

        [SerializeField] protected bool m_syncSlope;

        [SerializeField] protected Vector3 m_planeNormal;

        [Header("Steer Agent Gizmos")]
        public bool showLinearVelocity;
        public bool showDirection;

        #endregion Inspector Serializations

        #region Transform

        public Vector3 linearVelocity
        { get => m_velocity; set => m_velocity = value; }

        public Vector3 desiredVelocity
        {
            get => m_desiredVelocity; set => m_desiredVelocity = value;
        }

        public Vector3 position => transform.position;

        public Vector3 forward => transform.forward;

        public Vector3 up => transform.up;

        #endregion Transform

        #region Steering

        public SteeringBehaviour GetSteeringBehaviour(string pSteerTag) => m_steers.FirstOrDefault<SteeringBehaviour>(st => string.Equals(st.steerTag, pSteerTag));

        public SteeringBehaviour CurrentSteer { get => m_currentSteer; set => m_currentSteer = value; }
        public Vector3 this[GroupBehaviour behaviour] { get => m_groupBehaviourOutputCaches[(int)behaviour]; set => m_groupBehaviourOutputCaches[(int)behaviour] = value; }

        #endregion Steering

        protected virtual void Awake()
        {
            //Components
            m_cc = GetComponent<CharacterController>();
            m_cc.radius = radius;
            m_cc.height = height;
            m_cc.minMoveDistance = float.Epsilon;
            m_cc.detectCollisions = false;
            Physics.autoSyncTransforms = true;

            //Steers
            m_steers = new List<SteeringBehaviour>();
            m_steers.AddRange(GetComponents<SteeringBehaviour>());
        }

        // Start is called before the first frame update
        protected void Start()
        {
            //StartSteering();
        }

        /// <summary>
        /// Update the Agent's Current Linear Velocity
        /// </summary>
        protected void UpdateLinearVelocity()
        {
            //Calculate target velocity
            float acceDelta = maxLinearAcceleration * Time.deltaTime;
            if (turnSpeedRestrict)
            {
                float cosine = Vector3.Dot(m_velocity.normalized, m_desiredVelocity.normalized);
                cosine += 1;
                m_velocity = Vector3.RotateTowards(m_velocity + forward * .0001f, m_desiredVelocity * ((cosine + .0001f) / 2), Time.deltaTime * Mathf.PI * maxTurnAroundSpeed / 180f, maxLinearAcceleration * Time.deltaTime);
            }
            else
                m_velocity = m_desiredVelocity;
        }

        /// <summary>
        ///
        /// </summary>
        protected void UpdatePosition()
        {
            //Move
            if (m_lockOnGround)
            {
                //Lock the yOffset
                transform.position = new Vector3(transform.position.x, m_yLockOffset, transform.position.z);
                Physics.SyncTransforms();

                m_cc.SimpleMove(m_velocity);
            }
            else
            {
                //Not Locked on ground, apply gravity to the move
                m_cc.Move((m_velocity + up * m_gravity) * Time.deltaTime);
            }
        }

        /// <summary>
        /// Update the facing direction of the agent
        /// </summary>
        protected void UpdateFace()
        {
            if (m_velocity != Vector3.zero)
                transform.forward = m_velocity;
        }

        // Update is called once per frame
        protected void Update()
        {
            if (!lookAt)
            {
                //If not manually controlled by others
                //Update Linear Velocity
                UpdateLinearVelocity();

                //If LookWhere2Go
                //Update angular velocity
                // if (!lookWhereToGo)
                //     UpdateAngularVelocity();

                UpdatePosition(); UpdateFace();
                //If Where2Go
                //Sync forward with linear vel

                //If !lookWheretogo, Rotate

                //Grounded Detection
                //m_isGrounded = m_cc.isGrounded;
            }
        }

        protected void OnDrawGizmos()
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
            ClearSteeringData();
            StartCoroutine(nameof(SteeringCoroutine));
        }

        /// <summary>
        /// Clean the old steering output data and stop steering routine
        /// </summary>
        public void StopSteering(bool clearSteeringData)
        {
            if (clearSteeringData)
                ClearSteeringData();

            StopCoroutine(nameof(SteeringCoroutine));
        }

        /// <summary>
        /// cleans the result from the steering behaivour executed in the last time
        /// </summary>
        protected void ClearSteeringData()
        {
            m_desiredVelocity = Vector3.zero;
            for (int i = 0; i < m_groupBehaviourOutputCaches.Length; i++)
                m_groupBehaviourOutputCaches[i] = Vector3.zero;
        }

        protected IEnumerator SteeringCoroutine()
        {
            while (true)
            {
                m_desiredVelocity = Vector3.Lerp(m_desiredVelocity, Vector3.ClampMagnitude(m_currentSteer.Steering, maxLinearSpeed), .4f);
                m_desiredVelocity.y = 0;
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

        public virtual void OnControlEnter(bool clearSteeringData)
        {
            StopSteering(clearSteeringData);
        }

        public virtual void OnControlExit()
        {
            StartSteering();
        }

        /// <summary>
        /// Stop current steering and Start Looking at the target
        /// </summary>
        /// <param name="pos"></param>
        public void SetLookAt(Vector3 target)
        {
            Vector3 dir = target - position;
            float sangle = Vector3.SignedAngle(forward, dir, up);
            float turnAroundDelta = maxTurnAroundSpeed * Time.deltaTime;

            if (sangle > turnAroundDelta)
            {
                transform.forward = Quaternion.AngleAxis(turnAroundDelta, up) * forward;
            }
            else
            {
                transform.forward = dir;
            }
        }
    }
}