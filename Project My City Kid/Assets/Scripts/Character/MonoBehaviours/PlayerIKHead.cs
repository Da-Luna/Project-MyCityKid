using UnityEngine;

/// <summary>
/// Manages the player's head IK (Inverse Kinematics) to adjust head orientation
/// based on the direction of the camera and other settings.
/// </summary>
public class PlayerIKHead : MonoBehaviour
{
    [Header("REFERENCES")]

    [SerializeField, Tooltip("Animator component used to control the player's head using Inverse Kinematics (IK). If not manually assigned, the script will automatically search for an Animator component on the same GameObject.")]
    Animator m_Animator;

    [Header("IK Settings")]

    [SerializeField, Tooltip("The speed at which the player's head will rotate towards the target position (determined by the camera's forward direction). A higher value means quicker head movement.")]
    float ikRotationSpeed = 1.25f;

    [SerializeField, Tooltip("The speed at which the player's head position moves towards the focus point (usually set by the camera direction). A higher value means the head will track the focus point more responsively.")]
    float fokusMoveSpeed = 20.0f;

    [SerializeField, Tooltip("The maximum horizontal angle (in degrees) that the player's head can rotate to track the camera direction. Limits how far left or right the head can turn.")]
    float maxHzTurnAnlge = 85.0f;

    [SerializeField, Tooltip("The maximum vertical angle (in degrees) that the player's head can rotate to follow the camera. Limits how far up or down the head can move to maintain natural motion.")]
    float maxVtTurnAnlge = 135.0f;

    [SerializeField, Tooltip("A slight offset added to the vertical head position to adjust where the player character appears to be looking, ensuring the head tracking feels natural.")]
    float neckOffsetAnlge = 1.25f;

    private Vector3 m_CurrentPOIPosition;  // Position of Interest (focus point)
    private Vector3 m_TargetPOIPosition;  // Target Position of Interest

    private float m_CurrentLookWeight;  // Current IK weight
    private float m_TargetLookWeight;  // Desired IK weight

    void Start()
    {
        if (m_Animator == null)
        {
            m_Animator = GetComponent<Animator>();
        }

        Vector3 startPos = transform.position + Camera.main.transform.forward;
        startPos.y += neckOffsetAnlge; 

        m_CurrentPOIPosition = startPos;
        m_TargetPOIPosition = startPos;
    }

    /// <summary>
    /// Updates the player's head IK based on the camera's current position and orientation.
    /// Adjusts the head's target position and IK look weight (how strongly the IK influences the head).
    /// </summary>
    public void UpdateIKHead()
    {
        if (m_Animator != null)
        {
            // Set the target position where the player's head should look, based on the camera's forward direction
            m_TargetPOIPosition = transform.position + Camera.main.transform.forward;
            m_TargetPOIPosition.y += neckOffsetAnlge; 

            // Calculate the angles between the camera's forward direction and the player's current facing direction
            float hzAngle = Vector3.Angle(Camera.main.transform.forward, transform.forward);
            float vtAngle = Vector3.Angle(transform.up, Camera.main.transform.forward);

            if (hzAngle < maxHzTurnAnlge && vtAngle < maxVtTurnAnlge)
            {
                m_TargetLookWeight = 1.0f;
            }
            else
            {
                m_TargetLookWeight = 0.0f;
            }

            m_CurrentLookWeight = Mathf.MoveTowards(m_CurrentLookWeight, m_TargetLookWeight, ikRotationSpeed * Time.deltaTime);
            m_CurrentPOIPosition = Vector3.MoveTowards(m_CurrentPOIPosition, m_TargetPOIPosition, fokusMoveSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Uses the Animator's IK system to apply the calculated head rotation and position.
    /// </summary>
    void OnAnimatorIK()
    {
        if (m_Animator != null)
        {
            m_Animator.SetLookAtWeight(m_CurrentLookWeight);
            m_Animator.SetLookAtPosition(m_CurrentPOIPosition);
        }
    }

#if UNITY_EDITOR
    #region TLS CONTROLS
    [Header("TLS Controls - ONLY IN UNITY_EDITOR")]

    [SerializeField, Tooltip("Whether to display debug spheres in the Scene view to visualize the player's current head IK focus point. Useful for debugging and adjusting IK behavior.")]
    bool showCurrentLookPosition = false;

    [SerializeField, Tooltip("The size of the debug spheres drawn in the Scene view when visualizing the player's head IK focus points.")]
    float positionMarkSize = 0.05f;

    void OnDrawGizmos()
    {
        if (showCurrentLookPosition)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(m_CurrentPOIPosition, positionMarkSize);
        }
    }
    #endregion // TLS CONTROLS
#endif
}
