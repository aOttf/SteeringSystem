using UnityEngine;

namespace SteeringSystem
{
    [RequireComponent(typeof(CharacterController), typeof(SteeringController))]
    public class SteeringAgent : MonoBehaviour, ISphereMoveable
    {
        #region Caches

        private CharacterController m_cc;
        private SteeringController m_sc;

        private Vector3 m_linearVelocity;
        private Vector3 m_linearAcceleration;
        private float m_angularVelocity;
        private float m_angularAcceleration;

        #endregion Caches

        #region Agent Movement

        [Tooltip("Sync Agent's orientation with its velocity direction. If enabled, the angular velocity is omitted.")]
        public bool lookWhere2Go;

        public float maxLinearAcceleration;
        public float maxLinearSpeed;
        public float maxAngularAcceleration;
        public float maxAngularSpeed;

        #endregion Agent Movement

        #region Ground Detection Settings

        private bool m_isGrounded;

        [Tooltip("Value of the gravity")]
        [SerializeField] private float m_gravity = -19.6f;

        [Tooltip("Lock the agent's y offset to yLockOffset. If enabled, the gravity settings are ignored.")]
        [SerializeField] private bool m_lockOnGround = false;
        [SerializeField] private float m_yLockOffset = 0f;

        #endregion Ground Detection Settings

        #region IMoveable Interface

        public Vector3 position => transform.position;
        public Vector3 forward => transform.forward;
        public Vector3 up => transform.up;
        public Vector3 linearVelocity => m_linearVelocity;
        public Vector3 linearAcceleration => m_linearAcceleration;
        public float angularVelocity => m_angularVelocity;
        public float angularAcceleration => m_angularAcceleration;

        public float radius => m_cc.radius;

        public float MaxLinearSpeed => maxLinearSpeed;

        public float MaxAngularSpeed => maxAngularSpeed;

        public float MaxLinearAcceleration => maxLinearAcceleration;

        public float MaxAngularAcceleration => maxAngularAcceleration;

        #endregion IMoveable Interface

        #region Debug Options

        [Header("Gizmos")]
        public bool showLinearVelocity;
        public bool showDirection;

        #endregion Debug Options

        //----------Debug
        private void Awake()
        {
            //Get Components
            m_cc = GetComponent<CharacterController>();
            m_cc.minMoveDistance = float.Epsilon;
            m_sc = GetComponent<SteeringController>();
        }

        // Start is called before the first frame update
        private void Start()
        {
            //Start Steering
            m_sc.StartSteering();
        }

        // Update is called once per frame
        private void Update()
        {
            //Grounded Detection
            m_isGrounded = m_cc.isGrounded;
            //if (m_isGrounded && m_linearVelocity.y < 0)
            //{
            //    m_linearVelocity.y = 0;
            //}
            //else if (!m_isGrounded && m_applyGravity)
            //{
            //    //Apply Gravity
            //    m_linearVelocity.y += m_gravity * Time.deltaTime;
            //}

            ////----------- Debug
            //m_linearVelocity.y = 0;

            //Apply Acceleration
            m_linearVelocity = Vector3.ClampMagnitude(m_linearVelocity + (m_linearAcceleration = m_sc.linearAcceleration) * Time.deltaTime, maxLinearSpeed);
            m_angularVelocity = Mathf.Clamp(m_angularVelocity + (m_angularAcceleration = m_sc.angularAcceleration) * Time.deltaTime, -maxAngularSpeed, maxAngularSpeed);

            //Apply Angular Velocity
            if (!lookWhere2Go)
            {
                transform.forward = Quaternion.AngleAxis(m_angularVelocity * Time.deltaTime, transform.up) * transform.forward;
            }
            else if (m_linearVelocity != Vector3.zero)
            {
                //Sync Face with velocity direction
                transform.forward = m_linearVelocity;
            }

            //Apply Linear Velocity
            if (m_lockOnGround)
            {
                m_linearVelocity.y = 0;
                transform.position = new Vector3(transform.position.x, m_yLockOffset, transform.position.z);
            }
            m_cc.Move((m_linearVelocity + Vector3.up * m_gravity) * Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            if (showDirection)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2);
            }
            if (Application.isPlaying)
            {
                if (showLinearVelocity)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawLine(transform.position, transform.position + linearVelocity);
                }
            }
        }
    }
}