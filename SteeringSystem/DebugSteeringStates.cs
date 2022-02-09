using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SteeringSystem;

public class DebugSteeringStates : MonoBehaviour
{
    private Wander m_wander;
    private Avoidance m_avoidance;

    // Start is called before the first frame update
    private void Start()
    {
        m_wander = GetComponent<Wander>();
        m_avoidance = GetComponent<Avoidance>();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            string text = (m_avoidance.isActive) ? m_avoidance.ToString() : default;
            if (m_wander.isActive)
                text += "+ " + m_wander.ToString();

            Handles.Label(transform.position, text);
        }
    }
}