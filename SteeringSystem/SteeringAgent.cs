using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

namespace SteeringSystem
{
    [RequireComponent(typeof(CharacterController))]
    public class SteeringAgent : MonoBehaviour, IMoveable
    {
        public float steeringRoutineTimeStep = .02f;
        public SteeringBehaviour currentSteering;   //The current steering behaviour chosen to be executed; Agent executes only one steeering behaviour in a period

        //Caches
        private CharacterController m_controller;

        //private List<SteeringBehaviour> m_steerings;    //Caches all the steering behaviors attached to this
        private Dictionary<string, SteeringBehaviour> m_steerings;
        //private Wander m_wander;
        //private Avoidance m_avoidance;

        #region Agent Type Settings

        public bool lookWhere2Go;
        public bool onGround;

        #endregion Agent Type Settings

        #region Agent Movement Attributes

        public float maxLinearAcceleration;
        public float maxLinearSpeed;
        public float maxAngularAcceleration;
        public float maxAngularSpeed;

        #endregion Agent Movement Attributes

        #region Agent Current Velocity

        [SerializeField] private Vector3 m_linearVelocity;
        [SerializeField] private Vector3 m_linearAcceleration;
        [SerializeField] private float m_angularVelocity;
        [SerializeField] private float m_angularAcceleration;

        #endregion Agent Current Velocity

        #region IMoveable Interface

        public Vector3 position => transform.position;
        public Vector3 linearVelocity => m_linearVelocity;
        public Vector3 linearAcceleration => m_linearAcceleration;
        public float angularVelocity => m_angularVelocity;
        public float angularAcceleration => m_angularAcceleration;

        #endregion IMoveable Interface

        public bool ChangeCurrentSteering(string pSteeringName) => m_steerings.TryGetValue(pSteeringName, out currentSteering);

        // Start is called before the first frame update
        private void Start()
        {
            //Get Components
            m_controller = GetComponent<CharacterController>();

            //Get all steering behaviours attached and cache into the dictionary
            m_steerings = new Dictionary<string, SteeringBehaviour>();
            foreach (var steer in GetComponents<SteeringBehaviour>())
                m_steerings.Add(steer.SteeringName, steer);

            //Routine Steering Calculation
            StartCoroutine(nameof(Steering));
        }

        // Update is called once per frame
        private void Update()
        {
            //Ground
            if (Mathf.Abs(m_linearVelocity.y) > float.Epsilon)
                m_linearVelocity.y = 0f;

            //Apply Acceleration
            m_linearVelocity = Vector3.ClampMagnitude(m_linearVelocity + m_linearAcceleration * Time.deltaTime, maxLinearSpeed);

            m_angularVelocity = Mathf.Clamp(m_angularVelocity + m_angularAcceleration * Time.deltaTime, -maxAngularSpeed, maxAngularSpeed);

            //Face
            //If not look to where to go, angular velocity doesn't affect linear velocity direction

            if (!lookWhere2Go)
            {
                //the angular velocity applies to face
                transform.forward = Quaternion.AngleAxis(m_angularVelocity * Time.deltaTime, transform.up) * transform.forward;
            }
            else
            {
                //Only if we have a linear velocity
                if (m_linearVelocity.sqrMagnitude > float.Epsilon)
                {
                    //the angular velocity applies to linear velocity
                    //m_linearVelocity = Quaternion.AngleAxis(Vector3.SignedAngle(m_linearVelocity, m_linearAcceleration, transform.up), transform.up) * m_linearVelocity;

                    //Sync Face with velocity direction
                    transform.forward = m_linearVelocity;
                }
            }

            //Move
            if (m_linearVelocity.sqrMagnitude > float.Epsilon)
                m_controller.Move(m_linearVelocity * Time.deltaTime);
        }

        private IEnumerator Steering()
        {
            SteeringOutput res;
            while (true)
            {
                res = currentSteering.Steering;
                m_linearAcceleration = res.Linear;
                m_angularAcceleration = res.Angular;
                //m_linearVelocity = (Vector3.Slerp(m_linearVelocity, m_linearAcceleration, .2f).normalized * m_linearVelocity.magnitude);
                //yield return new WaitForSeconds(steeringRoutineTimeStep);
                yield return new WaitForEndOfFrame();
            }
        }

        private void OnDrawGizmos()
        {
        }
    }
}