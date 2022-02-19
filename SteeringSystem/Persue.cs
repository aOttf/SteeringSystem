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
        [SerializeField] protected IMoveable m_target;
        public IMoveable Target { get => m_target; set => m_target = value; }

        protected override SteeringOutput GetSteering()
        {
            //angle btw agent and target's velocity
            float angle = Vector3.Angle(m_target.linearVelocity, m_entity.linearVelocity);

            //If the angle falls in the threshold, same as seek
            if (angle < angle2Predict || angle > 180f - angle2Predict)
                m_targetPosition = Target.position;

            //else, predict the target's next position and seek to that
            //Predict the position of the target
            float agentSpd = m_entity.linearVelocity.magnitude;
            float dist = (Target.position - m_entity.position).magnitude;
            float agentPredTime = dist / agentSpd;
            float predTime = (agentPredTime < time2Predict) ? Mathf.Sqrt(agentPredTime) : time2Predict;
            m_targetPosition = Target.position + predTime * Target.linearVelocity;

            //Seek to the final position
            return PersueTo(m_targetPosition);
        }

        protected SteeringOutput PersueTo(Vector3 pTarget)
        {
            switch (mode)
            {
                case MatchMode.MatchPosition:
                    return MatchPosition(pTarget, m_entity.position, m_maxLinearAcceleration);

                case MatchMode.MatchVelocity:
                    return MatchVelocity((pTarget - m_entity.position) * m_maxLinearAcceleration, m_entity.linearVelocity, m_maxLinearAcceleration);

                default:
                    throw new System.NotImplementedException();
            }
        }
    }
}