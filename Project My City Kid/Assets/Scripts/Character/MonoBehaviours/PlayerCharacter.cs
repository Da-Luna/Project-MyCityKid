using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    // Singleton pattern to ensure only one PlayerCharacter instance exists
    static protected PlayerCharacter s_PlayerInstance;
    static public PlayerCharacter PlayerInstance { get { return s_PlayerInstance; } }

    [Header("REFERENCES")]

    [SerializeField, Tooltip("Reference to the 3D character controller for movement and gravity handling.")]
    CharacterController3D m_CharacterController3D;

    [SerializeField, Tooltip("Reference to the player's camera controller.")]
    PlayerCamera m_PlayerCamera;

    [SerializeField, Tooltip("Reference to the animator component for managing character animations.")]
    Animator m_Animator;

    [SerializeField, Tooltip("Reference to the IK controller for the player's head.")]
    PlayerIKHead m_PlayerIKHead;
    
    [Header("MOVEMENT SETTINGS")]

    [SerializeField, Tooltip("The maximum speed for jogging (without sprinting).")]
    float movementWalkSpeed = 50.0f;

    [SerializeField, Tooltip("The speed for running/sprinting.")]
    float movementRunSpeed = 260.0f;

    [SerializeField, Tooltip("Multiplier for the smoothing of the speed transitions.")]
    float speedChangeMultipier = 5.0f;

    [Tooltip("Specifies the value for the strength of the jump power.")]
    public float jumpHeight;

    [Tooltip("Specifies the value for the strength of the double jump power.")]
    public float doubleJumpHeight;

    [Tooltip("Jump boost applied when sprinting at medium speed.")]
    public float jumpBoost = 2.25f;

    [Tooltip("Time buffer after leaving a platform where a jump is still allowed.")]
    public float jumpCoyoteTime = 0.16f;

    // Flags to manage jump requests and the ability to jump
    protected bool m_JumpRequested;
    protected bool m_DoubleJumpRequested;
    protected bool m_CanJump;

    [Tooltip("The speed with which the body rotates in the direction of movement while jogging.")]
    public float bodyRotStartSpeed = 750.0f;

    [Tooltip("The speed with which the body rotates in the direction of movement while sprinting.")]
    public float bodyRotRunSpeed = 250.0f;

    // Variables for movement, speed, and gravity management
    protected Vector3 m_MoveDirection;
    protected float m_CurrentSpeed;
    protected float m_AnimatorMoveVelocity;
    protected Vector3 m_GravityVelocity;

    const float m_AnimMoveValue = 0.6f;

    const string m_StringMoveVelocity = "MoveVelocity";
    const string m_StringGravityVelocity = "GravityVelocity";
    const string m_StringGrounded = "Grounded";
    const string m_StringRun = "Run";

    void Awake()
    {
        // Ensure there's only one instance of the PlayerCharacter
        if (s_PlayerInstance == null)
        {
            s_PlayerInstance = this;
        }

        // Initialize the state machine with the linked animator
        SceneLinkedSMB<PlayerCharacter>.Initialise(m_Animator, this);
    }

    #region MOVEMENT

    /// <summary>
    /// Handles the grounded movement of the player, including input reading, body rotation, and movement speed updates.
    /// </summary>
    public void GroundedVerticalMovement()
    {
        Vector2 moveInput = PlayerInputManager.Instance.OnMovementInput();
        m_MoveDirection = new Vector3(moveInput.x, 0.0f, moveInput.y);

        // Update body direction and movement speed
        UpdateBodyDirection();
        UpdateMovementSpeed();
        UpdateAnimationMoveVelocity();
    }

    /// <summary>
    /// Rotates the player's body to face the direction of movement.
    /// </summary>
    void UpdateBodyDirection()
    {
        if (m_MoveDirection != Vector3.zero)
        {
            // Rotate body in the direction of movement based on camera's orientation
            m_MoveDirection = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up) * m_MoveDirection;
            Quaternion toRotation = Quaternion.LookRotation(m_MoveDirection, Vector3.up);

            // Smoothly rotate towards the desired direction
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, bodyRotStartSpeed * Time.deltaTime);

            // Normalize movement direction
            m_MoveDirection = Vector3.ClampMagnitude(m_MoveDirection, 1.0f);
        }
    }

    /// <summary>
    /// Updates the player's speed based on input (sprinting or normal movement).
    /// </summary>
    void UpdateMovementSpeed()
    {
        float sprintInput = PlayerInputManager.Instance.OnSprintInput();

        if (sprintInput > 0f)
        {
            // Sprinting
            Lerper(ref m_CurrentSpeed, movementRunSpeed, speedChangeMultipier);
        }
        else
        {
            // Normal movement
            Lerper(ref m_CurrentSpeed, movementWalkSpeed, speedChangeMultipier);
        }
    }

    /// <summary>
    /// Updates the animation parameters based on the player's movement velocity.
    /// </summary>
    void UpdateAnimationMoveVelocity()
    {
        float moveMagnitude;
        bool sprintInput = PlayerInputManager.Instance.OnSprintInput() > 0;

        if (sprintInput)
        {
            // Calculate sprint movement magnitude
            moveMagnitude = Mathf.Clamp01(new Vector3(m_MoveDirection.x, 0, m_MoveDirection.z).magnitude);
        }
        else
        {
            // Calculate normal movement magnitude
            moveMagnitude = Mathf.Clamp(new Vector3(m_MoveDirection.x, 0, m_MoveDirection.z).magnitude, 0f, m_AnimMoveValue);
        }

        // Smooth transition of movement velocity in animations
        Lerper(ref m_AnimatorMoveVelocity, moveMagnitude, speedChangeMultipier);
        m_Animator.SetFloat(m_StringMoveVelocity, m_AnimatorMoveVelocity);

        // Set the running animation state
        bool isRun = m_AnimatorMoveVelocity > 0.5f;
        m_Animator.SetBool(m_StringRun, isRun);
    }

    #endregion // MOVEMENT

    #region GROUND AND GRAVITY

    /// <summary>
    /// Updates the ground type based on the character's state.
    /// </summary>
    /// <returns>Returns true if grounded; otherwise, false.</returns>
    public bool UpdateSurfaceType()
    {
        return m_CharacterController3D.IsGrounded;
    }

    /// <summary>
    /// Gets the current ground type beneath the player.
    /// </summary>
    /// <returns>Returns the ground type as an enum.</returns>
    public CharacterController3D.SurfaceTypes GetCurrentsurfaceType()
    {
        return m_CharacterController3D.CurrentSurfaceType;
    }

    /// <summary>
    /// Gets the ground type based on an index.
    /// </summary>
    /// <param name="i">The index of the ground type.</param>
    /// <returns>Returns the corresponding ground type enum.</returns>
    public CharacterController3D.SurfaceTypes GetGroundTypeList(int i)
    {
        CharacterController3D.SurfaceTypes[] groundTypes = (CharacterController3D.SurfaceTypes[])System.Enum.GetValues(typeof(CharacterController3D.SurfaceTypes));
        return groundTypes[i];
    }

    /// <summary>
    /// Updates the ground and slope state of the character.
    /// </summary>
    public void GroundStatsCheck()
    {
        m_CharacterController3D.UpdateGroundState();
        bool grounded = m_CharacterController3D.IsGrounded;
        m_Animator.SetBool(m_StringGrounded, grounded);

        m_CharacterController3D.UpdateSlopeState();
    }

    /// <summary>
    /// Checks if the player is currently grounded.
    /// </summary>
    /// <returns>Returns true if the player is on the ground; otherwise, false.</returns>
    public bool GroundedCheck()
    {
        return m_CharacterController3D.IsGrounded;
    }

    /// <summary>
    /// Applies gravity to the player, managed by the CharacterController3D.
    /// </summary>
    public void GravityApply()
    {
        m_CharacterController3D.UpdateGravity();
        m_Animator.SetFloat(m_StringGravityVelocity, m_CharacterController3D.GravityVelocity.y);
    }



    #endregion // GROUND AND GRAVITY

    #region JUMPING

    /// <summary>
    /// Checks if the jump input has been triggered.
    /// </summary>
    /// <returns>Returns true if a jump is requested; otherwise, false.</returns>
    public bool InputCheckForJump()
    {
        bool _isJumping = PlayerInputManager.Instance.OnJumpInput();

        if (_isJumping)
        {
            if (!m_JumpRequested)
            {
                m_JumpRequested = true;
                return true;
            }
            else if (!m_DoubleJumpRequested)
            {
                m_DoubleJumpRequested = true;
                return true;
            }
        }
        else if (GroundedCheck() && !m_CanJump)
        {
            m_CanJump = true;
            m_JumpRequested = false;
            m_DoubleJumpRequested = false;
        }

        return false;
    }

    /// <summary>
    /// Performs the jump action if requested and allowed.
    /// </summary>
    public void Jump()
    {
        if (m_JumpRequested && m_CanJump)
        {
            // Boost the jump height slightly when sprinting
            if (m_CurrentSpeed > 200f && m_CurrentSpeed < 280f)
                m_CurrentSpeed += jumpBoost / 2f;

            // Trigger the jump in the character controller
            m_CharacterController3D.Jump(jumpHeight);
            m_Animator.SetTrigger("Jump");

            m_CanJump = false;
        }
    }

    public void DoubleJump()
    {
        if (!GroundedCheck())
        {
            if (m_DoubleJumpRequested && !m_CanJump)
            {
                m_CurrentSpeed += jumpBoost;

                // Trigger the jump in the character controller
                m_CharacterController3D.Jump(jumpHeight + doubleJumpHeight);
                m_Animator.SetTrigger("DoubleJump");
            }
            else if (GroundedCheck() && !m_CanJump)
            {
                m_CanJump = true;
            }
        }
    }

    #endregion // JUMPING

    #region AIMING

    /// <summary>
    /// Checks if the aim input has been triggered.
    /// </summary>
    /// <returns>Returns true if aiming is triggered; otherwise, false.</returns>
    bool InputCheckForAiming()
    {
        return PlayerInputManager.Instance.OnAimInput() > 0f;
    }

    /// <summary>
    /// Updates the camera's aiming mode based on player input.
    /// </summary>
    public void AimingCamera()
    {
        m_PlayerCamera.UpdateAimingCamera(InputCheckForAiming());
    }

    #endregion // AIMING

    void FixedUpdate()
    {
        m_CharacterController3D.Move(m_CurrentSpeed * Time.deltaTime * m_MoveDirection);
        m_PlayerIKHead.UpdateIKHead();
    }

    /// <summary>
    /// Smoothly transitions a value towards a target using linear interpolation (Lerp).
    /// </summary>
    /// <param name="a">Reference to the current value.</param>
    /// <param name="b">The target value.</param>
    /// <param name="tSpeed">Speed at which to transition.</param>
    void Lerper(ref float a, float b, float tSpeed)
    {
        a = Mathf.Lerp(a, b, tSpeed * Time.deltaTime);
    }
}
