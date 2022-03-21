using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class IcePool : MonoBehaviour
{
    public PlayerController m_PlayerCon;

    public float m_SlowAmount = 0.0f;
    public float m_SlowTime = 1.0f;
    public float m_RecoverTime = 1.0f;

    void Start()
    {
        if (!m_PlayerCon)
        {
            m_PlayerCon = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        /* Apply slow to player if they have entered */
        if (other.tag == "Player")
            m_PlayerCon.Slow(m_SlowAmount, m_SlowTime * m_SlowAmount);
    }
    private void OnTriggerExit(Collider other)
    {
        /* Remove slow from player if they have exited */
        if (other.tag == "Player")
            m_PlayerCon.Slow(-m_SlowAmount, m_RecoverTime);
    }
}
