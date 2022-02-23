using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace SteeringSystem
{
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

    /// <summary>
    /// The method blendedSteering used to combine steering behaviour outputs
    /// </summary>
    public enum CombineMethod
    { WeightedTruncated, PriorityWeightedTruncated, PriorityDithering };

    /// <summary>
    /// SteeringBehaviour is the base class from which every concrete Steering Behaviour Script derives
    /// </summary>
    [RequireComponent(typeof(SteeringController))]
    public abstract class SteeringBehaviour : MonoBehaviour
    {
        //[Tooltip("This is the name of the steering behaviour set manually used to store and access through a dictionary.\n If it hasn't been set, it will be initialized as the class name.\n" +
        //    "NOTE it is different from the name attribute in Object Class, " +
        //    "which is the name of the gameobject it is attached to")]
        //public string SteeringName = "";

        #region Caches

        protected ISphereMoveable m_entity;

        protected float m_maxLinearAcceleration;
        protected float m_maxLinearSpeed;
        protected float m_maxAngularAcceleration;
        protected float m_maxAngularSpeed;

        protected SteeringOutput m_result;

        #endregion Caches

        #region Weight Variables

        [Tooltip("Multiplier of the result of the linear acceleration")]
        [SerializeField] protected float m_linearResultWeight = 1f;

        [Tooltip("Multiplier of the result of the angular acceleration")]
        [SerializeField] protected float m_angularResultWeight = 1f;

        public float LinearResultWeight => m_linearResultWeight;
        public float AngularResultWeight => m_angularResultWeight;

        #endregion Weight Variables

        #region Probability Variables

        [Tooltip("The Steering Behaviour's probability value; used in blended steering")]
        [SerializeField] protected float m_probability = 1f;
        public float Probability => m_probability;

        #endregion Probability Variables

        [Tooltip("Is the steering behaviour currently active")]
        public bool isActive = true;

        #region Debug Options

        [Header("Gizmos")]
        public bool showAcceleration;
        public Color accelerationColor;
        public bool showState;

        [Tooltip("Additional Information shown for Debugging")]
        public string additionalInfo = "";

        #endregion Debug Options

        protected virtual void Awake()
        {
            //Init Components
            m_entity = GetComponent<ISphereMoveable>();
        }

        protected virtual void Start()
        {
            //Init velocity variables
            m_maxLinearAcceleration = m_entity.MaxLinearAcceleration;
            m_maxAngularAcceleration = m_entity.MaxAngularAcceleration;
            m_maxLinearSpeed = m_entity.MaxLinearSpeed;
            m_maxAngularSpeed = m_entity.MaxAngularSpeed;
        }

        /// <summary>
        /// Steering Output
        /// </summary>
        public SteeringOutput Steering => m_result = isActive ? Multiply(GetSteering()) : SteeringOutput.ZeroSteering;

        public SteeringBehaviour GetSteeringBehaviour<T>() where T : SteeringBehaviour
        {
            return GetComponent<SteeringBehaviour>();
        }

        public SteeringBehaviour[] GetSteeringBehaviours<T>() where T : SteeringBehaviour
        {
            return GetComponents<SteeringBehaviour>();
        }

        protected abstract SteeringOutput GetSteering();

        protected SteeringOutput Multiply(SteeringOutput pOutput) =>
            new SteeringOutput(pOutput.Linear * m_linearResultWeight, pOutput.Angular * m_angularResultWeight);

        protected virtual void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                if (showState)
                {
                    if (!SteeringOutput.IsZero(m_result))
                        Handles.Label(m_entity.position, GetType().ToString() + additionalInfo);
                }

                if (showAcceleration)
                {
                    Gizmos.color = accelerationColor;
                    print("Arrived");
                    Gizmos.DrawLine(m_entity.position, m_entity.position + m_result.Linear);
                }
            }
        }
    }
}