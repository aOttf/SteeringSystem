﻿using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace SteeringSystem
{
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

        public static SteeringOutput operator +(SteeringOutput a, SteeringOutput b)
            => new SteeringOutput(a.Linear + b.Linear, a.Angular + b.Angular);

        public static SteeringOutput operator -(SteeringOutput a, SteeringOutput b)
            => new SteeringOutput(a.Linear - b.Linear, a.Angular - b.Angular);

        public static bool IsZero(SteeringOutput a) => (a.Linear == Vector3.zero) && (a.Angular == 0);

        public static SteeringOutput ZeroSteering => new SteeringOutput(default, default);

        public static SteeringOutput LinearSteering(Vector3 pLinear) => new SteeringOutput(pLinear, default);

        public static SteeringOutput AngularSteering(float pAngular) => new SteeringOutput(default, pAngular);
    }

    /// <summary>
    /// The method blendedSteering used to combine steering behaviour outputs
    /// </summary>
    public enum CombineMethod
    { WeightedTruncated, PriorityWeightedTruncated, PriorityDithering };

    [RequireComponent(typeof(SteeringAgent))]
    public abstract class SteeringBehaviour : MonoBehaviour
    {
        [Tooltip("This is the name of the steering behaviour set manually used to store and access through a dictionary. By default it is \"\"." +
            "NOTE it is different from the name attribute in Object Class, " +
            "which is the name of the gameobject it is attached to")]
        public string SteeringName = "";

        protected SteeringAgent m_agent;

        protected float m_maxLinearAcceleration;
        protected float m_maxLinearSpeed;
        protected float m_maxAngularAcceleration;
        protected float m_maxAngularSpeed;

        //Cache the output
        protected SteeringOutput m_result;

        #region Weight Variables

        [SerializeField] protected float m_linearResultWeight = 1f;     //Multiplier of the result of the linear acceleration
        [SerializeField] protected float m_angularResultWeight = 1f;    //Multiplier of the result of the angular acceleration

        public float LinearResultWeight => m_linearResultWeight;
        public float AngularResultWeight => m_angularResultWeight;

        #endregion Weight Variables

        //#region Priority Variables

        //[SerializeField] protected int m_priority = 1;  //The Steering Behaviour's priority value is used in blended steering

        //public int Priority => m_priority;

        //#endregion Priority Variables

        #region Probability Variables

        [SerializeField] protected float m_probability = 1f;  //The Steering Behaviour's probability value is used in blended steering
        public float Probability => m_probability;

        #endregion Probability Variables

        //Whether the steering behavior is being performed
        public bool isActive = true;

        protected virtual void Awake()
        {
            //Init Components
            m_agent = GetComponent<SteeringAgent>();
        }

        protected virtual void Start()
        {
            //Init velocity variables
            m_maxLinearAcceleration = m_agent.maxLinearAcceleration;
            m_maxAngularAcceleration = m_agent.maxAngularAcceleration;
            m_maxLinearSpeed = m_agent.maxLinearSpeed;
            m_maxAngularSpeed = m_agent.maxAngularSpeed;
        }

        public SteeringOutput Steering => (m_result = (isActive) ? (Multiply(GetSteering())) : SteeringOutput.ZeroSteering);

        protected abstract SteeringOutput GetSteering();

        public virtual SteeringOutput MatchVariable(Vector3 tgt)
        { return SteeringOutput.ZeroSteering; }

        protected SteeringOutput Multiply(SteeringOutput pOutput) =>
            new SteeringOutput(pOutput.Linear * m_linearResultWeight, pOutput.Angular * m_angularResultWeight);

        //protected SteeringOutput Clip(SteeringOutput pOutput) =>
        //    new SteeringOutput(Vector3.ClampMagnitude(pOutput.Linear, m_maxLinearAcceleration * m_linearLimitWeight), Mathf.Max(pOutput.Angular, m_maxAngularSpeed * m_angularLimitWeight));

        protected virtual void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                if (!SteeringOutput.IsZero(m_result))
                    Handles.Label(transform.position, SteeringName);

                Gizmos.DrawLine(transform.position, transform.position + m_result.Linear);
            }
        }

        //public override string ToString() => "SteeringBehaviour: ";
    }

    public abstract class GroupSteering : SteeringBehaviour
    {
        public static string tagName;

        //Cache
        public static List<Transform> groupMembers;

        protected override void Awake()
        {
            //Find all group members
            groupMembers.AddRange(GameObject.FindGameObjectsWithTag(tagName).ToList().ConvertAll(member => member.transform));
            base.Awake();
        }
    }
}