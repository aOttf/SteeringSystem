using UnityEngine;
using UnityEditor;

namespace SteeringSystem
{
    [RequireComponent(typeof(CharacterController))]
    public class ObstacleAvoidance : SteeringBehaviour
    {
        #region Gizmos

        [Header("ObstacleAvoidance Gizmos")]
        public bool showCollisionRay;
        public Color rayColor;
        public Color hitColor;
        public bool showCollisionArc;
        public Color arcColor;

        #endregion Gizmos

        [Header("ObstacleAvoidance Params")]
        public LayerMask collisionLayer;

        public float collisionRayStep = 2f;
        public float collisionRayRate = 30f;
        public float collisionRayLength = 10f;

        protected Ray m_forwardRay;
        protected Ray m_collisionRay;

        #region Caches

        //Cache
        protected float m_radius;
        protected float m_halfCollisionRayRate;
        //protected Vector3 m_targetVelocity;

        protected bool m_isAvoiding = false;
        protected bool m_isHit;

        #endregion Caches

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            m_radius = m_entity.radius;
            m_halfCollisionRayRate = collisionRayRate / 2;
        }

        protected override Vector3 GetSteering()
        {
            m_forwardRay = new Ray(transform.position, transform.forward);

            //Detect Collision
            if (Physics.SphereCast(m_forwardRay, m_radius, collisionRayLength, collisionLayer))
            {
                float angle = collisionRayStep;
                int i = 0;

                //looping Until we find a ray that doesn't hit colliders AND passes the sweep test
                do
                {
                    m_collisionRay = new Ray(transform.position, Quaternion.AngleAxis(angle, Vector3.up) * transform.forward);

                    m_isHit =
                        Physics.Raycast(m_collisionRay, collisionRayLength, collisionLayer, QueryTriggerInteraction.UseGlobal) ||
                        Physics.SphereCast(m_collisionRay, m_radius, collisionRayLength, collisionLayer);

                    angle = -angle + i++ % 2 * collisionRayStep;
                }
                while (m_isHit && Mathf.Abs(angle) < m_halfCollisionRayRate);

                //Match the goal velocity
                return m_collisionRay.direction * m_entity.maxLinearSpeed;
            }

            return Vector3.zero;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            //Draw Collision Arc
            Vector3 from;
            from = Quaternion.AngleAxis(-collisionRayRate / 2, Vector3.up) * transform.forward;
            Handles.color = arcColor;
            Handles.DrawSolidArc(transform.position, Vector3.up, from, collisionRayRate, collisionRayLength);

            //Draw Collision Ray
            if (showCollisionRay && Application.isPlaying)
            {
                Color rayColor = m_isHit ? this.rayColor : hitColor;
                Debug.DrawLine(m_collisionRay.origin, m_collisionRay.origin + m_collisionRay.direction * collisionRayLength, rayColor, .2f);
            }
        }
    }
}