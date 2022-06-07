using UnityEngine;

namespace SteeringSystem
{
    public class CharacterAvoidance : GroupSteeringBehaviour
    {
        #region Debug Options

        [Header("CollisionAvoidance Debuggings")]
        public bool showAvoidance;
        public Color neighbourAvoidanceColor;

        #endregion Debug Options

        public static int count = 0;

        [Space(50)]
        public float time2Predict;  //Only Consider Future Collisions less than a given time
        public float deviationAngle;

        //Caches
        protected Vector3 m_acceleration;

        protected override void Awake()
        {
            //Init
            base.Awake();
        }

        protected override Vector3 GetSteering()
        {
            /** LEGACY
               * ////Get Neighbours
               * //FindNeighbours();
               *
               * //// Sum Avoidance Accelerations
               * //float colTime;
               * //Vector3 dir;
               * //m_acceleration = Vector3.zero;
               * //foreach (var neighbour in m_neighbours)
               * //{
               * //    colTime = Time2Collision(m_entity, neighbour);
               * //    if (colTime < time2Predict)
               * //    {
               * //        //Calculate acceleration direction
               * //        dir = (m_entity.position - neighbour.position + (m_entity.linearVelocity - neighbour.linearVelocity) * colTime).normalized;
               *
               * //        //If the direction of avoidance acceleration is parallel w/ the velocity, apply a deviation angle
               * //        if (180f - Vector3.Angle(dir, m_entity.linearVelocity) < float.Epsilon)
               * //            dir = Quaternion.AngleAxis(deviationAngle, transform.up) * dir;
               *
               * //        m_acceleration += dir * m_maxLinearAcceleration * (time2Predict - colTime) / (colTime + .1f);
               * //    }
               * //}
            */

            return m_entity.maxLinearSpeed * m_entity[GroupBehaviour.CollisionAvoidance].normalized;
        }
    }
}