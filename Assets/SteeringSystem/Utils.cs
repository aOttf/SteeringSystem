using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public static class SteeringUtilities
    {
        public enum MatchMode
        {
            MatchVelocity, MatchPosition
        }

        /// <summary>
        /// Fgoal = k * (Vgoal - V)
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static SteeringOutput MatchVelocity(Vector3 goalVelocity, Vector3 currentVelocity, float k = 1f) => SteeringOutput.LinearSteering(k * (goalVelocity - currentVelocity));

        public static SteeringOutput MatchPosition(Vector3 tgtPos, Vector3 curPos, float k = 1f) => SteeringOutput.LinearSteering(k * (tgtPos - curPos).normalized);

        
    }
}