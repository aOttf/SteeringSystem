using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public class Interpose : SteeringBehaviour
    {
        [SerializeField] protected IMoveable m_targetA;
        [SerializeField] protected IMoveable m_targetB;

        protected override SteeringOutput GetSteering()
        {
            //Get the first midpoint of A and B
            Vector3 midPoint1 = (m_targetA.position + m_targetB.position) / 2;
            //Predict the Time to reach
            float timePred = Vector3.Distance(transform.position, midPoint1) / m_maxLinearSpeed;
            //Predict the future position of A and B
            Vector3 posPredA = m_targetA.position + m_targetA.linearVelocity * timePred;
            Vector3 posPredB = m_targetB.position + m_targetB.linearVelocity * timePred;
            //Get the second midpoint of A and B
            Vector3 midPoint2 = (posPredA + posPredB) / 2;

            //Seek
            return SteeringOutput.LinearSteering((midPoint2 - transform.position).normalized * m_maxLinearAcceleration);
        }
    }
}