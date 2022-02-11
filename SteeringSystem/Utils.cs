using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringSystem
{
    public static class SteeringUtilities
    {
        /// <summary>
        /// Fgoal = k * (Vgoal - V)
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static SteeringOutput MatchVelocity(Vector3 goalVelocity, Vector3 currentVelocity, float k = 1f) => SteeringOutput.LinearSteering(k * (goalVelocity - currentVelocity));
    }
}