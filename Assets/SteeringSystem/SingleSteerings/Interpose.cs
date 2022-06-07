using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public class Interpose : SteeringBehaviour
    {
        [SerializeField] protected SteerAgent m_targetA;
        [SerializeField] protected SteerAgent m_targetB;

        protected override Vector3 GetSteering()
        {
            //Get the first midpoint of A and B
            Vector3 midPoint1 = (m_targetA.position + m_targetB.position) / 2;
            //Predict the Time to reach
            float timePred = Vector3.Distance(m_entity.position, midPoint1) / m_entity.maxLinearSpeed;
            //Predict the future position of A and B
            Vector3 posPredA = m_targetA.position + m_targetA.linearVelocity * timePred;
            Vector3 posPredB = m_targetB.position + m_targetB.linearVelocity * timePred;
            //Get the second midpoint of A and B
            Vector3 midPoint2 = (posPredA + posPredB) / 2;

            //Seek
            return m_entity.maxLinearSpeed * (midPoint2 - m_entity.position).normalized;
        }
    }
}