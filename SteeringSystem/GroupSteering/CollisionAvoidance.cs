using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace SteeringSystem
{
    /// <summary>
    /// Use Time to Collision Method to perform agents avoidance
    /// References : <see href="http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter19_Guide_to_Anticipatory_Collision_Avoidance.pdf">Anticipitory Collision Avoidance </see>
    /// </summary>
    ///
    public class CollisionAvoidance : GroupSteering
    {
        //public int maxNeighboursCount;

        public float radius;    //Detecting neighbours
        public float time2Predict;  //Only Consider Future Collisions less than a given time
        public float deviationAngle;

        //Caches
        protected List<ICapsuleMoveable> m_neighbours;
        protected Vector3 m_acceleration;

        protected override void Awake()
        {
            //Init
            tagName = "SphereMoveable";
            base.Awake();
        }

        protected override SteeringOutput GetSteering()
        {
            //Get Neighbours
            m_neighbours
                 = groupMembers.
                 FindAll
                 (member => member != transform && Vector3.Distance(member.position, transform.position) < radius)
                 .ConvertAll
                 (member => member.GetComponent<ICapsuleMoveable>());
            //.Take
            //(maxNeighboursCount)
            //.ToList();

            //    m_neighbourCount = neighbours.Count;

            // Sum Avoidance Accelerations
            float colTime;
            Vector3 dir;
            m_acceleration = Vector3.zero;
            foreach (var neighbour in m_neighbours)
            {
                colTime = Time2Collision(m_agent, neighbour);
                if (colTime < time2Predict)
                {
                    //Calculate acceleration direction
                    dir = (m_agent.position - neighbour.position + (m_agent.linearVelocity - neighbour.linearVelocity) * colTime).normalized;

                    //If the direction of avoidance acceleration is parallel w/ the velocity, apply a deviation angle
                    if (180f - Vector3.Angle(dir, m_agent.linearVelocity) < float.Epsilon)
                        dir = Quaternion.AngleAxis(deviationAngle, transform.up) * dir;

                    m_acceleration += dir * m_maxLinearAcceleration * (time2Predict - colTime) / (colTime + .1f);
                }
            }
            print(m_acceleration);
            return SteeringOutput.LinearSteering(Vector3.ClampMagnitude(m_acceleration, m_maxLinearAcceleration));
        }

        /// <summary>
        /// <para>Derived from the Equation
        /// dot(deltaV, deltaV) * t^2 + 2*dot(deltaX, deltaV)*t + dot(deltaX, deltaX) - sumRadius^2 = 0
        /// </para>
        /// t = (-b +- sqrt(b^2 - 4*a*c)) / (2*a)
        /// After Simplification, t = (-b +- sqrt(b^2 - a*c))/a,
        /// where b = dot(deltaX, deltaV), a = dot(deltaV, deltaV), c = (dektaX, deltaX) - sumRadius^2;
        /// </summary>
        /// <param name="A">Collision Entity</param>
        /// <param name="B">Another Collision Entity</param>
        /// <returns></returns>
        protected static float Time2Collision(ICapsuleMoveable A, ICapsuleMoveable B)
        {
            //Variables
            float sumRadius = A.radius + B.radius;
            Vector3 deltaV = B.linearVelocity - A.linearVelocity;
            Vector3 deltaX = B.position - A.position;

            float b = Vector3.Dot(deltaX, deltaV);
            float a = Vector3.Dot(deltaV, deltaV);
            float c = Vector3.Dot(deltaX, deltaX) - sumRadius * sumRadius;
            float delta = b * b - a * c;

            //Already in Collision
            if (c < 0)
                return 0f;

            //Delta < 0 or t < 0, the collision will never happen in the future
            float time;
            if (delta < 0 || (time = (-b - Mathf.Sqrt(delta)) / a) < 0)
                return float.PositiveInfinity;

            return time;
        }

        protected void OnDrawGizmosSelected()
        {
            base.OnDrawGizmos();
            if (Application.isPlaying)
            {
                if (showNeighbours)
                {
                    //Draw Neighbour Radius
                    Gizmos.DrawWireSphere(transform.position, radius);

                    //Draw Lines to Neighbours
                    foreach (var nei in m_neighbours)
                    {
                        Gizmos.color = (Time2Collision(m_agent, nei) < time2Predict) ? Color.red : Color.white;
                        Gizmos.DrawLine(transform.position, nei.position);
                    }
                }
            }
        }
    }
}