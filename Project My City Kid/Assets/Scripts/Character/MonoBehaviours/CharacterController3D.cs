using UnityEngine;

public class CharacterController3D : MonoBehaviour
{
    [Header("GRAVITY SETTINGS")]
    [SerializeField, Tooltip("Base gravity that affects the player. Typically set to -9.81 for realism.")]
    float gravity = -9.81f;

    [SerializeField, Tooltip("Multiplier applied to the base gravity for stronger or weaker gravity effects.")]
    float gravityMultiplier = 2.0f;

    protected bool m_GravityReachZero;
    const float m_GroundedGravity = -12.0f;

    [Header("GROUNDED SETTINGS")]
    [SerializeField, Tooltip("Offset position for the ground check sphere. Can be visualized in the scene view with TLS Scene Control.")]
    Vector3 groundCheckOffset = new(0.0f, 0.28f, 0.0f);
    [SerializeField, Tooltip("Radius of the ground check sphere used to determine if the player is grounded.")]
    float groundSphereRadius = 0.32f;
    [SerializeField, Tooltip("Layer mask for identifying the ground surfaces the player interacts with.")]
    LayerMask groundMask = 1 << 3; // Use bit shifting to select multiple layers: 1 << 3 | 1 << 4

    public enum SurfaceTypes { GRASS, GRAVEL, LEAVES, METAL, ROCK, SAND, WATER, WOOD }
    public SurfaceTypes CurrentSurfaceType { get; protected set; }

    [Header("SLOPE SETTINGS")]
    [SerializeField, Tooltip("Offset position for the slope check. Can be visualized as a wire cube in the scene view.")]
    Vector3 slopeCheckOffset = new(0.0f, 0.275f, 0.0f);
    [SerializeField, Tooltip("Length of the raycast used to detect slopes in front of the character.")]
    float raycastLength = 0.16f;
    [SerializeField, Tooltip("Maximum slope angle the player can walk on before sliding begins.")]
    float slopeLimit = 40.0f;
    [SerializeField, Range(0.1f, 10.0f), Tooltip("Initial force applied when sliding begins down a slope.")]
    float slopeStartForce = 3f;
    [SerializeField, Range(0.1f, 10.0f), Tooltip("Multiplier for acceleration while sliding down slopes.")]
    float onSlopeAcceleration = 3f;
    [SerializeField, Tooltip("Maximum downward speed the character can reach due to gravity.")]
    float limitGravityVelocity = 40.0f;

    protected CharacterController m_CharacterController;
    protected Collider m_Collider;
    protected Animator m_Animator;
    protected Vector3 m_GroundNormal;
    protected RaycastHit m_GrdHit;

    public bool IsGrounded { get; protected set; }
    public bool IsSliding { get; protected set; }
    public float GroundAngle { get; protected set; }
    public bool IsOnStairs { get; protected set; }
    public Vector3 MoveVector { get; protected set; }
    public Vector3 ExternalMoveVector { get; protected set; }
    public Vector3 GravityVelocity { get; protected set; }

    // Ground Type Tags
    const string m_TagGrdGrass = "GrdGrass";
    const string m_TagGrdGravel = "GrdGravel";
    const string m_TagGrdLeaves = "GrdLeaves";
    const string m_TagGrdMetal = "GrdMetal";
    const string m_TagGrdRock = "GrdRock";
    const string m_TagGrdSand = "GrdSand";
    const string m_TagGrdWater = "GrdWater";
    const string m_TagGrdWood = "GrdWood";
    const string m_TagStairs = "Stairs";

    void Awake()
    {
        if (m_CharacterController == null)
        {
            m_CharacterController = GetComponent<CharacterController>();
        }
        if (m_Collider == null)
        {
            m_Collider = GetComponent<Collider>();
        }
        if (m_Animator == null)
        {
            m_Animator = GetComponent<Animator>();
        }
    }

    void Start()
    {
        // Set default ground type to ROCK at the beginning.
        CurrentSurfaceType = SurfaceTypes.ROCK;
    }

    public void UpdateGravity()
    {
        Vector3 tempVelocity = GravityVelocity; // Store current gravity velocity temporarily.

        if (IsOnStairs && IsGrounded)
        {
            tempVelocity.y = m_GroundedGravity; // Apply a specific gravity when on stairs.
        }
        else if (IsSliding)
        {
            // Adjust velocity based on slope and sliding.
            tempVelocity += Vector3.ProjectOnPlane(m_GroundNormal.normalized + (Vector3.down * (GroundAngle / 30)).normalized * Mathf.Pow(slopeStartForce, onSlopeAcceleration), m_GroundNormal) * Time.deltaTime;
            tempVelocity.y += gravity * Time.deltaTime; // Apply gravity over time.

        }
        else if (!IsGrounded)
        {
            if (IsSliding)
            {
                IsSliding = false; // Player is grounded if not sliding.
            }

            if (IsOnStairs)
            {
                IsOnStairs = false; // If no stairs detected, set IsOnStairs to false.
            }


            // Apply gravity and reduce velocity when in the air.
            tempVelocity.x = Mathf.Lerp(tempVelocity.x, 0.0f, 0.2f * Time.deltaTime); // Gradually reduce X-axis velocity.
            tempVelocity.y += gravity * gravityMultiplier * Time.deltaTime; // Apply scaled gravity.
            tempVelocity.z = Mathf.Lerp(tempVelocity.z, 0.0f, 0.2f * Time.deltaTime); // Gradually reduce Z-axis velocity.

            m_GravityReachZero = false; // Reset when in the air.
        }
        else if (IsGrounded)
        {
            // Smoothly reduce velocity when grounded and not moving.
            Vector3 vZero = Vector3.zero;
            float resistanceAngle = Vector3.Angle(Vector3.ProjectOnPlane(tempVelocity, m_GroundNormal),
                Vector3.ProjectOnPlane(MoveVector, m_GroundNormal));

            resistanceAngle = resistanceAngle == 0 ? 90 : resistanceAngle; // Avoid angle division by zero.
            tempVelocity = (tempVelocity + MoveVector).magnitude <= 0.1f
                ? Vector3.zero
                : Vector3.SmoothDamp(tempVelocity, Vector3.zero, ref vZero, onSlopeAcceleration / 50.0f / (180.0f / resistanceAngle));

            // Ensure velocity is zero if it's very small.
            if (tempVelocity.magnitude <= 0.01f)
            {
                tempVelocity = Vector3.zero; // Ensure exact zero velocity.
            }

            if (tempVelocity == Vector3.zero)
            {
                m_GravityReachZero = true; // Mark gravity as settled.
            }
            else
            {
                m_GravityReachZero = false; // Reset if character is still moving.
            }
        }

        // Clamp velocity to avoid exceeding the gravity limit.
        tempVelocity.x = Mathf.Clamp(tempVelocity.x, -limitGravityVelocity, limitGravityVelocity);
        tempVelocity.y = Mathf.Clamp(tempVelocity.y, -limitGravityVelocity, limitGravityVelocity);
        tempVelocity.z = Mathf.Clamp(tempVelocity.z, -limitGravityVelocity, limitGravityVelocity);

        GravityVelocity = tempVelocity; // Update the gravity velocity.
    }

    public void UpdateGroundState()
    {
        // Check if the player is grounded by casting a sphere at the groundCheckOffset position.
        if (Physics.CheckSphere(transform.position + groundCheckOffset, groundSphereRadius, groundMask))
        {
            if (!IsSliding)
                IsGrounded = true; // Player is grounded if not sliding.

            Vector3 center = transform.position + slopeCheckOffset;
            Vector3 halfExtens = new(groundSphereRadius / 2.0f, groundSphereRadius / 2.0f, groundSphereRadius / 2.0f);

            // Cast a box to check for slope and ground properties.
            if (Physics.BoxCast(center, halfExtens, Vector3.down, out m_GrdHit, transform.rotation, raycastLength, groundMask))
            {
                SurfaceTypeCheck(); // Determine the type of ground.

                if (!m_GrdHit.collider.CompareTag(m_TagStairs))
                {
                    if (IsOnStairs)
                    {
                        IsOnStairs = false; // If no stairs detected, set IsOnStairs to false.
                    }
                }
                else if (!IsOnStairs || !IsGrounded)
                {
                    IsOnStairs = true; // If stairs are detected, set IsOnStairs to true.
                }
            }
        }
        else
        {
            // If not grounded or sliding, reset both states.
            IsSliding = false;
            IsGrounded = false;
        }
    }

    void SurfaceTypeCheck()
    {
        // Determine the type of ground based on the collider's tag.
        switch (m_GrdHit.collider.tag)
        {
            case m_TagGrdGrass:
                CurrentSurfaceType = SurfaceTypes.GRASS;
                break;
            case m_TagGrdGravel:
                CurrentSurfaceType = SurfaceTypes.GRAVEL;
                break;
            case m_TagGrdLeaves:
                CurrentSurfaceType = SurfaceTypes.LEAVES;
                break;
            case m_TagGrdMetal:
                CurrentSurfaceType = SurfaceTypes.METAL;
                break;
            case m_TagGrdRock:
                CurrentSurfaceType = SurfaceTypes.ROCK;
                break;
            case m_TagGrdSand:
                CurrentSurfaceType = SurfaceTypes.SAND;
                break;
            case m_TagGrdWater:
                CurrentSurfaceType = SurfaceTypes.WATER;
                break;
            case m_TagGrdWood:
                CurrentSurfaceType = SurfaceTypes.WOOD;
                break;
            default:
                break; // Do nothing if the tag doesn't match.
        }
    }

    public void UpdateSlopeState()
    {
        // Ursprung des Raycasts ist der Mittelpunkt des Charakters
        Vector3 rayOrigin = transform.position + slopeCheckOffset;

        // Raycasts in different directions to detect slopes.
        bool hasHitDown = Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hitDown, raycastLength, groundMask);
        bool hasHitForward = Physics.Raycast(rayOrigin, transform.forward, out RaycastHit hitForward, raycastLength, groundMask);
        bool hasHitBackward = Physics.Raycast(rayOrigin, -transform.forward, out RaycastHit hitBackward, raycastLength, groundMask);
        bool hasHitLeft = Physics.Raycast(rayOrigin, -transform.right, out RaycastHit hitLeft, raycastLength, groundMask);
        bool hasHitRight = Physics.Raycast(rayOrigin, transform.right, out RaycastHit hitRight, raycastLength, groundMask);

        // Calculate surface normals for each direction.
        Vector3 normalDown = hasHitDown ? hitDown.normal : Vector3.up;
        Vector3 normalForward = hasHitForward ? hitForward.normal : Vector3.up;
        Vector3 normalBackward = hasHitBackward ? hitBackward.normal : Vector3.up;
        Vector3 normalLeft = hasHitLeft ? hitLeft.normal : Vector3.up;
        Vector3 normalRight = hasHitRight ? hitRight.normal : Vector3.up;

        // Calculate the maximum slope angle from detected surfaces.
        float angleDown = Vector3.Angle(Vector3.up, normalDown);
        float angleForward = Vector3.Angle(Vector3.up, normalForward);
        float angleBackward = Vector3.Angle(Vector3.up, normalBackward);
        float angleLeft = Vector3.Angle(Vector3.up, normalLeft);
        float angleRight = Vector3.Angle(Vector3.up, normalRight);

        // Determine if the character should slide based on the slope angle.
        float maxSlopeAngle = Mathf.Max(angleDown, angleForward, angleBackward, angleLeft, angleRight);

        GroundAngle = maxSlopeAngle;

        // Determine if the character should slide based on the slope angle.
        IsSliding = GroundAngle > slopeLimit;

        // Average normal vector for movement calculations.
        m_GroundNormal = (normalDown + normalForward + normalBackward + normalLeft + normalRight).normalized;
    }

    Vector3 AdjustMoveVectorToSlope(Vector3 moveVector)
    {
        // Check if the ground is normal by making a raycast from below onto the terrain
        if (Physics.Raycast(transform.position + slopeCheckOffset, Vector3.down, out RaycastHit hitInfo, raycastLength, groundMask))
        {
            // Calculate the rotation to the inclination
            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            // Apply this rotation to the MoveVector
            Vector3 adjustedMoveVector = slopeRotation * moveVector;

            // Check whether the fitted vector points downwards (i.e. has a negative y-component)
            if (adjustedMoveVector.y < 0)
            {
                return adjustedMoveVector;
            }
        }

        return moveVector;
    }

    void FixedUpdate()
    {
        MoveVector = AdjustMoveVectorToSlope(MoveVector);

        // Combine movement and gravity for character movement.
        Vector3 CurrentSpeed = MoveVector + ExternalMoveVector;
        CurrentSpeed += GravityVelocity;

        // Move the character controller based on the combined speed.
        m_CharacterController.Move(CurrentSpeed * Time.deltaTime);
    }

    /// <summary>
    /// This moves a CharacterController and so should only be called from FixedUpdate or other Physics messages.
    /// </summary>
    /// <param name="movement">Applied movement to the Vector3 m_Movement.</param>
    public void Move(Vector3 currentSpeed)
    {
        MoveVector = currentSpeed; // Store the movement input for later use in FixedUpdate.
    }

    /// <summary>
    /// Applies an upward force to simulate a jump based on the specified jump height.
    /// This method calculates the necessary vertical velocity to achieve the desired jump height
    /// using the formula for projectile motion and sets the GravityVelocity accordingly.
    /// </summary>
    /// <param name="jumpHeight">The desired height of the jump in units.</param>
    public void Jump(float jumpHeight)
    {
        // Calculate jump force based on desired jump height and gravity.
        Vector3 _jumpForce = Vector3.zero;
        _jumpForce.y = Mathf.Sqrt(jumpHeight * -2.0f * gravity);

        GravityVelocity = _jumpForce; // Apply the calculated jump force to the gravity velocity.
    }

    public void ExternalMove(Vector3 externalMove)
    {
        ExternalMoveVector = externalMove; // Store the externalMove for later use in FixedUpdate.
    }

#if UNITY_EDITOR
    #region TLS CONTROLS
    [Header("TLS Controls - ONLY IN UNITY_EDITOR")]
    [SerializeField, Tooltip("Displays the GroundCheck sphere as a WireSphere in the scene.")]
    bool showGroundCheckShpere = false;
    [SerializeField, Tooltip("Displays the current surface type in the console.")]
    bool showGroundTypeDebug = false;
    [SerializeField, Tooltip("Displays the SlopeCheck box as a WireCube in the scene.")]
    bool showAngleCheckBox = false;
    [SerializeField, Tooltip("Displays the SlopeCheck box as a WireCube in the scene.")]
    bool showAngleForwardCheckBox = false;

    void OnDrawGizmosSelected()
    {
        if (showGroundCheckShpere)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position + groundCheckOffset, groundSphereRadius); // Visualize the ground check sphere.
        }

        if (showGroundTypeDebug && Application.isPlaying)
        {
            Debug.Log($"Underground type is: {CurrentSurfaceType}"); // Print the current ground type to the console.
        }

        if (showAngleCheckBox)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + slopeCheckOffset, new Vector3(groundSphereRadius, groundSphereRadius, groundSphereRadius)); // Visualize the slope check box.
        }

        if (showAngleForwardCheckBox)
        {
            Vector3 rayOrigin = transform.position + slopeCheckOffset;

            // Ray nach unten (weiß)
            Vector3 downDirection = Vector3.down * raycastLength;
            Gizmos.color = Color.white;
            Gizmos.DrawLine(rayOrigin, rayOrigin + downDirection);
            Gizmos.DrawWireSphere(rayOrigin + downDirection, 0.02f);

            // Forward Raycast (Rot)
            Vector3 forwardDirection = transform.forward * raycastLength;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(rayOrigin, rayOrigin + forwardDirection);
            Gizmos.DrawWireSphere(rayOrigin + forwardDirection, 0.02f);

            // Backward Raycast (Blau)
            Vector3 backwardDirection = -transform.forward * raycastLength;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(rayOrigin, rayOrigin + backwardDirection);
            Gizmos.DrawWireSphere(rayOrigin + backwardDirection, 0.02f);

            // Left Raycast (Grün)
            Vector3 leftDirection = -transform.right * raycastLength;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(rayOrigin, rayOrigin + leftDirection);
            Gizmos.DrawWireSphere(rayOrigin + leftDirection, 0.02f);

            // Right Raycast (Gelb)
            Vector3 rightDirection = transform.right * raycastLength;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(rayOrigin, rayOrigin + rightDirection);
            Gizmos.DrawWireSphere(rayOrigin + rightDirection, 0.02f);
        }
    }
    #endregion
#endif
}
