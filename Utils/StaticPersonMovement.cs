using System.Collections;
using UnityEngine;

namespace Assets.Scripts.CookBook
{
    [RequireComponent(typeof(CharacterController))]
    public class StaticPersonMovement : MonoBehaviour
    {
        [SerializeField] private float m_maxMoveSpeed;

        //  [SerializeField] private float m_maxTurnAroundSpeed;
        [SerializeField] private float m_maxJumpHeight;
        [SerializeField] private float m_gravity;
        private bool m_isGrounded;

        private CharacterController m_controller;

        private Vector3 m_velocity; //Current Velocity

        // Use this for initialization
        private void Start()
        {
            m_controller = GetComponent<CharacterController>();
        }

        // Update is called once per frame
        private void Update()
        {
            //Get input
            float hInput = Input.GetAxis("Horizontal");
            float vInput = Input.GetAxis("Vertical");

            //Derive velocity
            float y = m_velocity.y;
            m_velocity = transform.forward * Mathf.Max(Mathf.Abs(hInput), Mathf.Abs(vInput)) * m_maxMoveSpeed;
            m_velocity.y = y;

            //Derive direction
            if (m_velocity != Vector3.zero)
                transform.forward = Vector3.Slerp(transform.forward, new Vector3(hInput, 0, vInput), .2f);

            //Jump
            if (Input.GetKeyDown(KeyCode.Space) && m_isGrounded)
            {
                m_velocity.y += Mathf.Sqrt(m_maxJumpHeight * -2.0f * m_gravity);
            }

            //Move
            m_controller.Move((m_velocity + m_gravity * Vector3.up) * Time.deltaTime);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody rb = hit.rigidbody;
            if (rb)
                rb.AddForce(m_velocity, ForceMode.Impulse);
        }
    }
}