using UnityEngine;
using UnityEditor;

namespace SteeringSystem
{
    [RequireComponent(typeof(CharacterController))]
    public class Avoidance : SteeringBehaviour
    {
        public LayerMask collisionLayer;

        public float collisionRayStep = 2f;
        public float collisionRayRate = 30f;
        public float collisionRayLength = 10f;

        protected Ray m_forwardRay;
        protected Ray m_collisionRay;

        //Cache
        protected float m_radius;
        protected float m_halfCollisionRayRate;
        protected Vector3 m_targetPosition;

        protected bool m_isAvoiding = false;

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            m_radius = GetComponent<CharacterController>().radius;
            m_halfCollisionRayRate = collisionRayRate / 2;
        }

        protected override SteeringOutput GetSteering()
        {
            m_forwardRay = new Ray(transform.position, transform.forward);

            //Detect Collision
            if (Physics.SphereCast(m_forwardRay, m_radius, collisionRayLength, collisionLayer))
            {
                float angle = collisionRayStep;
                int i = 0;

                //looping Until we find a ray that doesn't hit colliders AND passes the sweep test
                bool hit;
                Color rayColor;
                do
                {
                    m_collisionRay = new Ray(transform.position, Quaternion.AngleAxis(angle, Vector3.up) * transform.forward);

                    hit =
                        Physics.Raycast(m_collisionRay, collisionRayLength, collisionLayer, QueryTriggerInteraction.UseGlobal) ||
                        Physics.SphereCast(m_collisionRay, m_radius, collisionRayLength, collisionLayer);

                    #region Runtime Debug

                    rayColor = hit ? Color.red : Color.green;

                    Debug.DrawLine(m_collisionRay.origin, m_collisionRay.origin + m_collisionRay.direction * collisionRayLength, rayColor, .5f);

                    #endregion Runtime Debug

                    angle = -angle + i++ % 2 * collisionRayStep;
                }
                while (hit && Mathf.Abs(angle) < m_halfCollisionRayRate);

                //Seek to the new position
                m_targetPosition = m_collisionRay.direction * m_maxLinearSpeed;
                return SteeringOutput.LinearSteering((m_targetPosition - m_agent.linearVelocity) * m_maxLinearAcceleration);
            }

            return SteeringOutput.ZeroSteering;
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            //Draw Collision Arc
            Vector3 from;
            from = Quaternion.AngleAxis(-collisionRayRate / 2, Vector3.up) * transform.forward;
            Handles.color = new Color(Color.blue.r, Color.blue.g, Color.blue.b, .1f);
            Handles.DrawSolidArc(transform.position, Vector3.up, from, collisionRayRate, collisionRayLength);

            //Draw forward Ray
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * collisionRayLength);
        }

        public override string ToString() => base.ToString() + "Avoidance";
    }
}