using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class IcePool : MonoBehaviour
{
    public PlayerController m_PlayerCon;

    public float m_SlowAmount = 0.0f;
    public float m_SlowTimePerSecond = 1.0f;
    public float m_RecoverTimePerSecond = 1.0f;

    private bool m_PlayerInside = false;

    private BoxCollider m_Box;

    void Start()
    {
        m_Box = gameObject.GetComponent<BoxCollider>();

        if (!m_PlayerCon)
        {
            m_PlayerCon = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }
    }

    private void FixedUpdate()
    {
        bool playerHit = IsPlayerInside();

        if (playerHit && !m_PlayerInside)
        {
            /* Player entered */
            m_PlayerCon.Slow(m_SlowAmount, m_SlowTimePerSecond);
        }
        else if (!playerHit && m_PlayerInside)
        {
            /* Player exited */
            m_PlayerCon.Slow(-m_SlowAmount, m_RecoverTimePerSecond);
        }

        m_PlayerInside = playerHit;
    }

    /// <summary>
    /// Returns true if the players controller collider is within this objects box collider
    /// </summary>
    private bool IsPlayerInside()
    {
        Collider[] hits = Physics.OverlapBox(transform.position + m_Box.center, new Vector3(transform.localScale.x * m_Box.size.x, transform.localScale.y * m_Box.size.y, transform.localScale.z * m_Box.size.z) * 0.5f);
        foreach (Collider col in hits)
            if (col.CompareTag("Player"))
                return true;
        return false;
    }
}
