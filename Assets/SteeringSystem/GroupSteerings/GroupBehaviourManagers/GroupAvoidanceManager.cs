using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public class GroupAvoidanceManager : GroupBehaviourManager
    {
        public float time2Predict;

        /// <summary>
        /// <para>Derived from the Equation
        /// <code>dot(deltaV, deltaV) * t^2 + 2 * dot(deltaX, deltaV) * t + dot(deltaX, deltaX) - sumRadius^2 = 0</code>
        ///<code>t = (-b +- sqrt(b^2 - 4*a*c)) / (2*a) </code>
        /// After Simplification,<code> t = (-b +- sqrt(b^2 - a*c))/a</code>,
        /// where <code>b = dot(deltaX, deltaV), a = dot(deltaV, deltaV), c = (dektaX, deltaX) - sumRadius^2; </code></para>
        /// </summary>
        /// <param name="A">Collision Entity</param>
        /// <param name="B">Another Collision Entity</param>
        /// <returns></returns>
        protected float Time2Collision(SteerAgent A, SteerAgent B)
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

        protected override void GroupSteering(SteerAgent a, SteerAgent b)
        {
            float colTime = Time2Collision(a, b);
            if (colTime < time2Predict)
            {
                //Calculate acceleration direction
                Vector3 dir = (a.position - b.position + (a.linearVelocity - b.linearVelocity) * colTime).normalized;

                ////If the direction of avoidance acceleration is parallel w/ the velocity, apply a deviation angle
                //if (180f - Vector3.Angle(dir, A.linearVelocity) < float.Epsilon)
                //    dir = Quaternion.AngleAxis(deviationAngle, transform.up) * dir;

                Vector3 res = dir * (time2Predict - colTime) / (colTime + .1f);
                a[GroupBehaviour.CollisionAvoidance] += res;
                b[GroupBehaviour.CollisionAvoidance] -= res;
            }
        }
    }
}