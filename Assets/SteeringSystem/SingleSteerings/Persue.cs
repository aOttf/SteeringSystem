using UnityEngine;
using static SteeringSystem.SteeringUtilities;

namespace SteeringSystem
{
    public class Persue : SteeringBehaviour
    {
        protected Vector3 m_targetPosition; //The final predicted target's position

        public float time2Predict = .1f;  //Max Possible time to predict the target
        public float angle2Predict = 30f; //Max Signed Angle Degree to Ignore the Prediction

        public MatchMode mode = MatchMode.MatchPosition;
        [SerializeField] public SteerAgent target;

        protected override Vector3 GetSteering()
        {
            //angle btw agent and target's velocity
            float angle = Vector3.Angle(target.linearVelocity, m_entity.linearVelocity);

            //If the angle falls in the threshold, same as seek
            if (angle < angle2Predict || angle > 180f - angle2Predict)
                m_targetPosition = target.position;

            //else, predict the target's next position and seek to that
            //Predict the position of the target
            else
            {
                float agentSpd = m_entity.linearVelocity.magnitude;
                float dist = (target.position - m_entity.position).magnitude;
                float agentPredTime = dist / agentSpd;
                float predTime = (agentPredTime < time2Predict) ? Mathf.Sqrt(agentPredTime) : time2Predict;
                m_targetPosition = target.position + predTime * target.linearVelocity;
            }
            //Seek to the final position
            return m_entity.maxLinearSpeed * (m_targetPosition - m_entity.position).normalized;
        }
    }
}