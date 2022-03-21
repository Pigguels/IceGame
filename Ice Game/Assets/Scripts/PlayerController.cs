using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Movement variables

    [Header("Movement:")]
    public float m_WalkSpeed = 6.5f;
    private float m_CurrentMoveSpeed = 0.0f;
    private float m_TargetMoveSpeed = 0.0f;
    private float m_MoveSpeedAdjustSpeed = 1.0f;
    
    public float m_AirControl = 0.5f;
    [Space]
    public float m_MaxSlopeAngle = 60f;
    public float m_SlopeAdustmentDistance = 0.2f;
    private Vector3 m_MoveDir;
    private Vector3 m_PreviousMoveDir;

    private bool m_IsGrounded;
    
    #endregion

    [Space]
    public float m_Gravity = 9.81f;
    public float m_Drag = 1f;
    private Vector3 m_Velocity;

    #region Camera variables

    [Header("Camera:")]
    public float m_LookSensitivity = 4f;
    [Range(0f, 90f)]
    public float m_UpwardPitchClamp = 60f;
    public float m_DownwardPitchClamp = 60f;
    private float m_Pitch = 0f;
    private float m_Yaw = 0f;
    public Transform m_Head;

    #endregion

    private CharacterController m_CharController;

    void Start()
    {
        m_CharController = GetComponent<CharacterController>();

        m_CurrentMoveSpeed = m_WalkSpeed;
        m_TargetMoveSpeed = m_WalkSpeed;
        m_MoveSpeedAdjustSpeed = 0.0f;
        m_MoveDir = Vector3.zero;
        m_PreviousMoveDir = Vector3.zero;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        m_PreviousMoveDir = m_MoveDir;
        m_MoveDir = (transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical")).normalized;

        m_IsGrounded = IsGrounded();

        ApplyPhysics();

        Walk();

        SlopeAdjustment();
        MoveSpeedAdjustment();
    }

    private void LateUpdate()
    {
        Look();
    }

    /// <summary>
    /// Returns true if the player is on ground
    /// </summary>
    private bool IsGrounded()
    {
        RaycastHit sphereHit;
        return Physics.SphereCast(transform.position + m_CharController.center, m_CharController.radius, Vector3.down, out sphereHit, (m_CharController.height * 0.5f) - m_CharController.radius + m_CharController.skinWidth + 0.01f, ~gameObject.layer);
    }

    /// <summary>
    /// Applies gravity and drag to the velocity
    /// </summary>
    private void ApplyPhysics()
    {
        /* Apply gravity to the velocity */
        if (m_IsGrounded)
            m_Velocity.y = 0f;
        else if (m_Velocity.y > -(m_Gravity * m_Gravity))
            m_Velocity.y -= m_Gravity * Time.deltaTime;

        /* Apply drag to the velocity */
        m_Velocity.x = Mathf.MoveTowards(m_Velocity.x, 0f, m_Drag);
        m_Velocity.z = Mathf.MoveTowards(m_Velocity.z, 0f, m_Drag);
    }

    /// <summary>
    /// The players look rotation and control
    /// </summary>
    private void Look()
    {
        m_Pitch -= Input.GetAxis("Mouse Y") * m_LookSensitivity * Time.deltaTime;
        m_Yaw += Input.GetAxis("Mouse X") * m_LookSensitivity * Time.deltaTime;

        m_Pitch = Mathf.Clamp(m_Pitch, -m_DownwardPitchClamp, m_UpwardPitchClamp);

        m_Head.localRotation = Quaternion.Euler(m_Pitch, 0f, 0f);
        transform.localRotation = Quaternion.Euler(0f, m_Yaw, 0f);
    }

    /// <summary>
    /// The players direction movement and slope handling
    /// </summary>
    private void Walk()
    {
        if (m_IsGrounded)
            m_CharController.Move(((m_MoveDir * m_CurrentMoveSpeed) + m_Velocity) * Time.deltaTime);
        else
            m_CharController.Move(((m_PreviousMoveDir * m_TargetMoveSpeed * m_AirControl) + m_Velocity) * Time.deltaTime);
    }

    /// <summary>
    /// Slope adjustment to keep the player grounded when they walk
    /// </summary>
    private void SlopeAdjustment()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, m_CharController.height * 0.5f + m_SlopeAdustmentDistance, ~gameObject.layer))
        {
            if (Mathf.Acos(Vector3.Dot(hit.normal, Vector3.up)) < m_MaxSlopeAngle * Mathf.Deg2Rad)
            {
                m_Velocity.y = -(m_Gravity * m_Gravity);
            }
        }
    }
    
    /// <summary>
    /// Adjust the players current move speed to the target move speed
    /// </summary>
    private void MoveSpeedAdjustment()
    {
        m_CurrentMoveSpeed = Mathf.MoveTowards(m_CurrentMoveSpeed, m_TargetMoveSpeed, m_MoveSpeedAdjustSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Slows the players movement speed by the given amount over the given time
    /// </summary>
    public void Slow(float amount, float time)
    {
        m_TargetMoveSpeed -= amount;
        m_MoveSpeedAdjustSpeed = time;
    }
}
