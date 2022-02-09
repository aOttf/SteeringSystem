using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public class Persue : SteeringBehaviour
    {
        [SerializeField] protected Vector3 m_targetPosition; //The final predicted target's position

        public float time2Predict = .1f;  //Max Possible time to predict the target
        public float angle2Predict = 30f; //Max Signed Angle Degree to Ignore the Prediction

        protected SteeringAgent m_target;
        public SteeringAgent Target { get => m_target; set => m_target = value; }

        protected override SteeringOutput GetSteering()
        {
            //angle btw agent and target's velocity
            float angle = Vector3.Angle(m_target.linearVelocity, m_agent.linearVelocity);

            //If the angle falls in the threshold, same as seek
            if (angle < angle2Predict || angle > 180f - angle2Predict)
                m_targetPosition = Target.transform.position;

            //else, predict the target's next position and seek to that
            //Predict the position of the target
            float agentSpd = m_agent.linearVelocity.magnitude;
            float dist = (Target.transform.position - transform.position).magnitude;
            float agentPredTime = dist / agentSpd;
            float predTime = (agentPredTime < time2Predict) ? Mathf.Sqrt(agentPredTime) : time2Predict;
            m_targetPosition = Target.transform.position + predTime * Target.linearVelocity;

            //Seek to the final position
            return SteeringOutput.LinearSteering((m_targetPosition - transform.position).normalized * m_maxLinearAcceleration);
        }
    }
}