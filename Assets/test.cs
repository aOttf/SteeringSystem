using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    private CharacterController m_cc;
    [SerializeField] private Transform other;

    private void Awake()
    {
        m_cc = GetComponent<CharacterController>();
    }

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        m_cc.Move((other.position - transform.position - -Vector3.up).normalized * 10 * Time.deltaTime);
    }
}