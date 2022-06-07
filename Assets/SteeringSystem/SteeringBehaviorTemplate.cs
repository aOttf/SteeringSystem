using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace SteeringSystem
{
    #region LEGACY

    /// <summary>
    /// Linear Acceleration and Angular Acceleration
    /// </summary>
    public struct SteeringOutput
    {
        public Vector3 Linear { get; set; }
        public float Angular { get; set; }

        public SteeringOutput(Vector3 pLinear, float pAngular)
        {
            Linear = pLinear;
            Angular = pAngular;
        }

        public static SteeringOutput Clip(SteeringOutput pResult, float maxLinear, float maxAngualr)
            => new SteeringOutput(Vector3.ClampMagnitude(pResult.Linear, maxLinear), Mathf.Max(pResult.Angular, maxAngualr));

        #region Operator Overloadings

        public static SteeringOutput operator +(SteeringOutput a, SteeringOutput b)
            => new SteeringOutput(a.Linear + b.Linear, a.Angular + b.Angular);

        public static SteeringOutput operator +(SteeringOutput a, Vector3 linearResult)
        => new SteeringOutput(a.Linear + linearResult, a.Angular);

        public static SteeringOutput operator +(Vector3 linearResult, SteeringOutput a)
        => new SteeringOutput(a.Linear + linearResult, a.Angular);

        public static SteeringOutput operator -(Vector3 linearResult, SteeringOutput a)
        => new SteeringOutput(a.Linear - linearResult, a.Angular);

        public static SteeringOutput operator -(SteeringOutput a, Vector3 linearResult)
       => new SteeringOutput(a.Linear - linearResult, a.Angular);

        public static SteeringOutput operator -(SteeringOutput a, SteeringOutput b)
            => new SteeringOutput(a.Linear - b.Linear, a.Angular - b.Angular);

        public static SteeringOutput operator *(SteeringOutput a, float mult)
            => new SteeringOutput(a.Linear * mult, a.Angular * mult);

        public static SteeringOutput operator *(float mult, SteeringOutput a)
          => new SteeringOutput(a.Linear * mult, a.Angular * mult);

        public static SteeringOutput operator *(SteeringOutput a, int mult)
            => new SteeringOutput(a.Linear * mult, a.Angular * mult);

        public static SteeringOutput operator *(int mult, SteeringOutput a)
          => new SteeringOutput(a.Linear * mult, a.Angular * mult);

        public static SteeringOutput operator -(SteeringOutput a)
            => new SteeringOutput(-a.Linear, -a.Angular);

        #endregion Operator Overloadings

        /// <summary>
        /// Is the steering output Zero
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static bool IsZero(SteeringOutput a) => (a.Linear == Vector3.zero) && Mathf.Approximately(a.Angular, 0f);

        /// <summary>
        /// A Zero Steering Output
        /// </summary>
        public static SteeringOutput ZeroSteering => new SteeringOutput(default, default);

        /// <summary>
        /// A Linear Steering Output that contains <paramref name="pLinear"/> linear acceleration
        /// </summary>
        /// <param name="pLinear"></param>
        /// <returns></returns>
        public static SteeringOutput LinearSteering(Vector3 pLinear) => new SteeringOutput(pLinear, default);

        /// <summary>
        /// An Angular Steering Output that contains <paramref name="pAngular"/> angular acceleration
        /// </summary>
        /// <param name="pAngular"></param>
        /// <returns></returns>
        public static SteeringOutput AngularSteering(float pAngular) => new SteeringOutput(default, pAngular);
    }

    #endregion LEGACY

    /// <summary>
    /// The method blendedSteering used to combine steering behaviour outputs
    /// </summary>
    public enum CombineMethod
    { WeightedTruncated, PriorityWeightedTruncated, PriorityDithering };

    /// <summary>
    /// SteeringBehaviour is the base class from which every concrete Steering Behaviour Script derives
    /// </summary>
    public abstract class SteeringBehaviour : MonoBehaviour
    {
        //[Tooltip("This is the name of the steering behaviour set manually used to store and access through a dictionary.\n If it hasn't been set, it will be initialized as the class name.\n" +
        //    "NOTE it is different from the name attribute in Object Class, " +
        //    "which is the name of the gameobject it is attached to")]
        //public string SteeringName = "";

        #region Caches

        protected SteerAgent m_entity;

        //protected float m_maxLinearAcceleration;
        //protected float m_maxLinearSpeed;
        //protected float m_maxAngularAcceleration;
        //protected float m_maxAngularSpeed;

        protected SteeringOutput m_result;

        protected Vector3 m_targetVelocity;

        #endregion Caches

        #region Weight Variables

        [Tooltip("Multiplier of the result of the linear acceleration")]
        [SerializeField] protected float m_linearResultWeight = 1f;

        //  [Tooltip("Multiplier of the result of the angular acceleration")]
        //[SerializeField] protected float m_angularResultWeight = 1f;

        public float LinearResultWeight => m_linearResultWeight;
        //public float AngularResultWeight => m_angularResultWeight;

        #endregion Weight Variables

        #region Probability Variables

        [Tooltip("The Steering Behaviour's probability value; used in blended steering")]
        [SerializeField] protected float m_probability = 1f;
        public float Probability => m_probability;

        #endregion Probability Variables

        [Tooltip("Is the steering behaviour currently active")]
        public bool isActive = true;

        #region Gizmos

        [Header("Gizmos")]
        public bool showGizmos;

        [Tooltip("Shows the target velocity of the steering")]
        public bool showTargetVelocity;
        public Color targetVelocityColor;

        [Tooltip("Shows Tag of the Steering Behaviour.")]
        public bool showState;

        [Tooltip("Steer Tag of the behavior")]
        public string steerTag = "Steering";

        #endregion Gizmos

        protected virtual void Awake()
        {
            //Init Components
            m_entity = GetComponent<SteerAgent>();
        }

        protected virtual void Start()
        {
            //Init velocity variables
            //m_maxLinearAcceleration = m_entity.MaxLinearAcceleration;
            //m_maxAngularAcceleration = m_entity.MaxAngularAcceleration;
            //m_maxLinearSpeed = m_entity.maxLinearSpeed;
            //m_maxAngularSpeed = m_entity.MaxAngularSpeed;
        }

        /// <summary>
        /// Steering Output
        /// </summary>
       // public SteeringOutput Steering => m_result = isActive ? Multiply(GetSteering()) : SteeringOutput.ZeroSteering;

        public Vector3 Steering => m_targetVelocity = isActive ? GetSteering() * m_linearResultWeight : Vector3.zero;

        public SteeringBehaviour GetSteeringBehaviour<T>() where T : SteeringBehaviour
        {
            return GetComponent<SteeringBehaviour>();
        }

        public SteeringBehaviour[] GetSteeringBehaviours<T>() where T : SteeringBehaviour
        {
            return GetComponents<SteeringBehaviour>();
        }

        // protected abstract SteeringOutput GetSteering();

        protected abstract Vector3 GetSteering();

        //protected SteeringOutput Multiply(SteeringOutput pOutput) =>
        // new SteeringOutput(pOutput.Linear * m_linearResultWeight, pOutput.Angular * m_angularResultWeight);

        protected virtual void OnDrawGizmosSelected()
        {
            if (showGizmos && Application.isPlaying)
            {
                if (showState)
                {
                    Handles.Label(m_entity.position, steerTag);
                }

                if (showTargetVelocity)
                {
                    Gizmos.color = targetVelocityColor;
                    Gizmos.DrawRay(m_entity.position, m_targetVelocity);
                }
            }
        }
    }
}