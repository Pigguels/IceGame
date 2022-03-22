using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Basic Movement Variables

    [Header("Basic Movement:")]
    public float m_WalkSpeed = 6.5f;
    public float m_MinMoveSpeed = 0.1f;
    public float m_AirControl = 0.5f;

    private float m_CurrentMoveSpeed = 0.0f;
    private float m_TargetMoveSpeed = 0.0f;
    private float m_MoveSpeedAdjustSpeed = 1.0f;
    
    [Space]
    public float m_MaxSlopeAngle = 60f;
    public float m_SlopeAdustmentDistance = 0.2f;
    private Vector3 m_MoveDir;
    private Vector3 m_PreviousMoveDir;

    [Space]
    public float m_Gravity = 9.81f;
    public float m_Drag = 1f;
    private Vector3 m_Velocity;

    private bool m_IsGrounded;

    public enum MovementStates { walk, crouch, climb }
    private MovementStates m_MoveState;

    #endregion

    #region Crouch Variables

    [Header("Crouching:")]
    public KeyCode m_CrouchKey = KeyCode.LeftControl;
    [Space]
    public float m_CrouchMoveSpeed = 2.5f;
    public float m_VerticalCrouchSpeed = 0.5f;
    [Space]
    public float m_StandingHeight = 2f;
    public float m_CrouchHeight = 1f;
    public float m_EyeLevel = 1.75f;

    private float m_TargetHeight = 2f;

    #endregion

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

    private LayerMask m_IgnoreMask;

    void Start()
    {
        m_CharController = GetComponent<CharacterController>();

        m_IgnoreMask = LayerMask.GetMask("PlayerCheckIgnore");

        m_TargetHeight = m_StandingHeight;

        m_CurrentMoveSpeed = m_WalkSpeed;
        m_TargetMoveSpeed = m_WalkSpeed;
        m_MoveSpeedAdjustSpeed = 0.0f;
        m_MoveDir = Vector3.zero;
        m_PreviousMoveDir = Vector3.zero;

        m_MoveState = MovementStates.walk;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        m_PreviousMoveDir = m_MoveDir;
        m_MoveDir = (transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical")).normalized;

        m_IsGrounded = IsGrounded();

        ApplyPhysics();
        MoveSpeedAdjustment();
        AdjustYScale(m_TargetHeight, m_VerticalCrouchSpeed);
        SlopeAdjustment();

        switch (m_MoveState)
        {
            case MovementStates.walk:
                Walk();
                break;

            case MovementStates.crouch:
                Crouch();
                break;

            case MovementStates.climb:
                break;
        }
    }

    private void LateUpdate()
    {
        Look();
    }

    #region Checks

    /// <summary>
    /// Returns true if the player is on ground
    /// </summary>
    private bool IsGrounded()
    {
        RaycastHit sphereHit;
        return Physics.SphereCast(transform.position + m_CharController.center, m_CharController.radius, Vector3.down, out sphereHit, (m_CharController.height * 0.5f) - m_CharController.radius + m_CharController.skinWidth + 0.01f, ~gameObject.layer & ~m_IgnoreMask);
    }

    /// <summary>
    /// Returns true if there is enough room above character controller to stand,calso returns true if already standing
    /// </summary>
    private bool CanStand()
    {
        if (m_TargetHeight == m_StandingHeight)
            return true;

        RaycastHit sphereHit;
        return !Physics.SphereCast(transform.position + m_CharController.center, m_CharController.radius, Vector3.up, out sphereHit, (m_CharController.height * 0.5f) + (m_StandingHeight - m_CharController.height) - m_CharController.radius + m_CharController.skinWidth, ~gameObject.layer & ~m_IgnoreMask);
    }

    #endregion

    #region Adjustments

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
    /// Lerp the character controllers y scale to the height and adjust the position to make it seamless
    /// </summary>
    private void AdjustYScale(float height, float speed)
    {
        m_CharController.enabled = false;

        float controllerHeightCache = m_CharController.height;

        /* Change the character controllers height */
        m_CharController.height = Mathf.Lerp(controllerHeightCache, height, speed);

        /* Offset position */
        float yOffset = (Mathf.Lerp(controllerHeightCache, height, speed) - controllerHeightCache) * 0.5f;
        m_CharController.center += new Vector3(0f, yOffset, 0f);

        /* Adjust eye level */
        m_Head.localPosition = new Vector3(m_Head.localPosition.x, m_EyeLevel * m_CharController.height * 0.5f, m_Head.localPosition.z);

        m_CharController.enabled = true;
    }

    #endregion

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

    #region Movestates

    /// <summary>
    /// The players direction movement and slope handling
    /// </summary>
    private void Walk()
    {
        /* Walk in the desired direction */
        if (m_IsGrounded)
            m_CharController.Move(((m_MoveDir * m_CurrentMoveSpeed) + m_Velocity) * Time.deltaTime);
        else
            m_CharController.Move(((m_PreviousMoveDir * m_TargetMoveSpeed * m_AirControl) + m_Velocity) * Time.deltaTime);

        /* Switch to crouch */
        if (m_IsGrounded && Input.GetKeyDown(m_CrouchKey))
        {
            m_TargetHeight = m_CrouchHeight;
            m_MoveState = MovementStates.crouch;
        }
    }

    /// <summary>
    /// The players crouch state
    /// </summary>
    private void Crouch()
    {
        /* Exit the crouch state */
        if (!Input.GetKey(m_CrouchKey) && CanStand())
        {
            m_TargetHeight = m_StandingHeight;
            m_MoveState = MovementStates.walk;
            return;
        }

        /* If the players grounded move them in their desired direction */
        if (m_IsGrounded)
            m_CharController.Move((m_MoveDir * m_CrouchMoveSpeed + m_Velocity) * Time.deltaTime);
        else
        {
            /* If player tries to move move them in tht direction, if not continue in there last move direction */
            if (m_MoveDir != Vector3.zero)
                m_CharController.Move((m_MoveDir * m_CrouchMoveSpeed * m_AirControl + m_Velocity) * Time.deltaTime);
            else
                m_CharController.Move((m_PreviousMoveDir * m_CrouchMoveSpeed * m_AirControl + m_Velocity) * Time.deltaTime);
        }
    }

    #endregion

    /// <summary>
    /// Slows the players movement speed by the given amount over the given time
    /// </summary>
    public void Slow(float amount, float time)
    {
        m_TargetMoveSpeed -= amount;
        m_MoveSpeedAdjustSpeed = time;

        if (m_TargetMoveSpeed < m_MinMoveSpeed)
        {
            m_TargetMoveSpeed = m_MinMoveSpeed;
        }
    }
}
