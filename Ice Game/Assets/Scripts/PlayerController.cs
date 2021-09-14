using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float m_WalkSpeed = 6.5f;
    private float m_MoveSpeed;
    private Vector3 m_MoveDir;

    private float m_Gravity = 9.81f;
    private float m_Drag = 1f;
    private Vector3 m_Velocity;

    private bool m_IsGrounded;

    public float m_LookSensitivity = 4f;
    [Range(0f, 90f)]
    public float m_PitchClamp = 60f;
    private float m_Pitch = 0f;
    private float m_Yaw = 0f;

    public Transform m_Head;

    private CharacterController m_CharController;

    void Start()
    {
        m_CharController = GetComponent<CharacterController>();

        m_MoveSpeed = m_WalkSpeed;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        m_MoveDir = (transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical")).normalized;

        m_IsGrounded = IsGrounded();

        ApplyPhysics();

        Walk();
    }

    private void FixedUpdate()
    {
        Look();
    }

    private bool IsGrounded()
    {
        RaycastHit hit;
        return Physics.SphereCast(transform.position + m_CharController.center, m_CharController.radius + m_CharController.skinWidth, Vector3.down, out hit, m_CharController.height * 0.5f, ~gameObject.layer);
    }

    private void ApplyPhysics()
    {
        if (m_IsGrounded)
            m_Velocity.y = 0f;
        else if (m_Velocity.y > (m_Gravity * m_Gravity))
            m_Velocity.y -= m_Gravity * Time.deltaTime;

        m_Velocity.x = Mathf.MoveTowards(m_Velocity.x, 0f, m_Drag);
        m_Velocity.z = Mathf.MoveTowards(m_Velocity.z, 0f, m_Drag);
    }

    private void Look()
    {
        m_Pitch -= Input.GetAxis("Mouse Y") * m_LookSensitivity;
        m_Yaw += Input.GetAxis("Mouse X") * m_LookSensitivity;

        m_Pitch = Mathf.Clamp(m_Pitch, -m_PitchClamp, m_PitchClamp);

        m_Head.localRotation = Quaternion.Euler(m_Pitch, 0f, 0f);
        transform.localRotation = Quaternion.Euler(0f, m_Yaw, 0f);
    }

    private void Walk()
    {
        m_CharController.Move(((m_MoveDir * m_MoveSpeed) + m_Velocity) * Time.deltaTime);
    }
}
