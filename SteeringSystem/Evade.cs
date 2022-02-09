using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public class Evade : SteeringBehaviour
    {
        [SerializeField] protected Vector3 m_targetPosition; //The final predicted target's position

        public float time2Predict = .1f; //Max Possible time to predict the target
        public float angle2Predict = 30f; //Max Unsigned Angle Degree to Ignore the Prediction

        protected SteeringAgent m_target;
        public SteeringAgent Target { get => m_target; set => m_target = value; }

        protected override SteeringOutput GetSteering()
        {
            //If the cosine falls in the threshould
            float angle = Vector3.Angle(m_target.linearVelocity, m_agent.linearVelocity);

            ////If the target is moving away from the agent, then return zero
            //if (angle > 95f)
            //    return SteeringOutput.ZeroSteering;

            //If the cosine falls in the threshold, same as flee
            if (angle < angle2Predict || angle > 180f - angle2Predict)
                m_targetPosition = Target.transform.position;
            //return SteeringOutput.LinearSteering((transform.position - Target.transform.position).normalized * m_maxLinearAcceleration);

            //else, predict the target's next position and flee from that
            //Predict the position of the target
            float agentSpd = m_agent.linearVelocity.magnitude;
            float dist = (Target.transform.position - transform.position).magnitude;
            float agentPredTime = dist / agentSpd;
            float predTime = (agentPredTime < time2Predict) ? Mathf.Sqrt(agentPredTime) : time2Predict;
            m_targetPosition = Target.transform.position + predTime * Target.linearVelocity;

            //Flee away from the final position
            return SteeringOutput.LinearSteering((transform.position - m_targetPosition).normalized * m_maxLinearAcceleration);
        }
    }
}