using UnityEngine;

namespace SteeringSystem
{
    /// <summary>
    /// <para>Use Time to Collision Method to perform agents avoidance</para>
    /// References : <see href="http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter19_Guide_to_Anticipatory_Collision_Avoidance.pdf">Anticipitory Collision Avoidance </see>
    /// </summary>
    ///
    public class CollisionAvoidance : GroupSteeringBehaviour
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

        protected override SteeringOutput GetSteering()
        {
            //Get Neighbours
            FindNeighbours();

            // Sum Avoidance Accelerations
            float colTime;
            Vector3 dir;
            m_acceleration = Vector3.zero;
            foreach (var neighbour in m_neighbours)
            {
                colTime = Time2Collision(m_entity, neighbour);
                if (colTime < time2Predict)
                {
                    //Calculate acceleration direction
                    dir = (m_entity.position - neighbour.position + (m_entity.linearVelocity - neighbour.linearVelocity) * colTime).normalized;

                    //If the direction of avoidance acceleration is parallel w/ the velocity, apply a deviation angle
                    if (180f - Vector3.Angle(dir, m_entity.linearVelocity) < float.Epsilon)
                        dir = Quaternion.AngleAxis(deviationAngle, transform.up) * dir;

                    m_acceleration += dir * m_maxLinearAcceleration * (time2Predict - colTime) / (colTime + .1f);
                }
            }
            return SteeringOutput.LinearSteering(Vector3.ClampMagnitude(m_acceleration, m_maxLinearAcceleration));
        }

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
        protected static float Time2Collision(ISphereMoveable A, ISphereMoveable B)
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

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (Application.isPlaying)
            {
                if (m_neighbours != null && m_neighbours.Count != 0)
                {
                    //Draw Lines to Neighbours
                    foreach (var nei in m_neighbours)
                    {
                        Gizmos.color = (Time2Collision(m_entity, nei) < time2Predict) ? neighbourAvoidanceColor : neighbourSphereColor;
                        Gizmos.DrawLine(m_entity.position, nei.position);
                    }
                }
            }
        }

        //private void OnControllerColliderHit(ControllerColliderHit hit)
        //{
        //    if (hit.gameObject.tag == this.tag)
        //        count++;
        //    print(count);
        //}
    }
}