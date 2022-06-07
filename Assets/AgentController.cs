using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SteeringSystem;

[RequireComponent(typeof(SteerAgent))]
public class AgentController : MonoBehaviour
{
    private SteerAgent m_agent;

    private void Awake()
    {
        m_agent = GetComponent<SteerAgent>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        m_agent.StartSteering();
    }

    // Update is called once per frame
    private void Update()
    {
    }
}