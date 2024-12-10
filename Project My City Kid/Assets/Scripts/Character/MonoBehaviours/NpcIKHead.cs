using System.Collections;
using UnityEngine;

/// <summary>
/// The NpcIKHead class manages the Inverse Kinematics (IK) behavior of an NPC's head. It allows the NPC
/// to track the player's position and gradually disable this behavior when the player leaves the interaction range. 
/// The class works in conjunction with the <see cref="NpcIKHeadTrigger"/> class to enable and disable head IK when the 
/// player enters or exits the NPC's trigger collider.
/// </summary>
public class NpcIKHead : MonoBehaviour
{
    [Header("REFERENCES")]

    [SerializeField, Tooltip("Animator component used to control the NPC's head using Inverse Kinematics (IK). If not assigned, the script will attempt to find an Animator component attached to the same GameObject.")]
    Animator m_Animator;

    [Header("IK SETTINGS")]

    [SerializeField, Tooltip("The speed at which the NPC's head will rotate to face the target position (usually the player). A higher value means faster rotation towards the target.")]
    float ikLerpSpeed = 1.0f;

    [SerializeField, Tooltip("The speed at which the NPC's head position moves towards the calculated focus point. This affects how quickly the head follows the player's position.")]
    float fokusMoveSpeed = 3.5f;

    [SerializeField, Tooltip("The maximum angle, in degrees, that the NPC's head can turn horizontally to face the player. Prevents unnatural head movements beyond a certain limit.")]
    float maxHzTurnAnlge = 85.0f;

    [SerializeField, Tooltip("The maximum angle, in degrees, that the NPC's head can turn vertically to face the player. Limits how much the NPC can look up or down.")]
    float maxVtTurnAnlge = 135.0f;

    [SerializeField, Tooltip("A small angle adjustment applied to the NPC's neck to ensure that the head appears to be tracking the player's position naturally, rather than staring unnaturally straight ahead.")]
    float neckOffsetAnlge = 1.4f;

    [Header("IK POSITION SETTINGS")]

    [SerializeField, Tooltip("The initial starting position for the NPC's head, used for IK calculations. This defines where the NPC's head will reset to when not tracking a target.")]
    Vector3 startPosition;

    private Vector3 m_CurrentPOIPosition;  // Position of Interest (focus point)
    private Vector3 m_TargetPOIPosition;  // Target Position of Interest

    private float m_CurrentLookWeight;  // Current IK weight
    private float m_TargetLookWeight;  // Desired IK weight

    private Coroutine disableCoroutine;

    void OnEnable()
    {
        if (m_Animator == null)
        {
            m_Animator = GetComponent<Animator>();
        }

        m_CurrentPOIPosition = startPosition + transform.position;
        m_TargetPOIPosition = startPosition + transform.position;

        m_CurrentLookWeight = 0f;
        m_TargetLookWeight = 0f;
    }

    /// <summary>
    /// Updates the NPC's head IK to track the player's position and direction.
    /// This method is called every frame, adjusting both the look weight (how strongly the NPC focuses on the player)
    /// and the target position (where the NPC should look).
    /// </summary>
    public void UpdateIKHead()
    {
        Transform playerTransform = PlayerCharacter.PlayerInstance.transform;

        if (m_Animator != null && playerTransform != null)
        {
            // Set the target position where the NPC should look, based on the playerTransform.position
            m_TargetPOIPosition = playerTransform.position;
            m_TargetPOIPosition.y += neckOffsetAnlge;

            // Calculate angles between player and NPC.
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            float hzAngle = Vector3.Angle(transform.forward, directionToPlayer);
            float vtAngle = Vector3.Angle(transform.up, directionToPlayer);

            if (hzAngle < maxHzTurnAnlge && vtAngle < maxVtTurnAnlge)
            {
                m_TargetLookWeight = 1.0f;
            }
            else
            {
                m_TargetLookWeight = 0.0f;
            }

            m_CurrentLookWeight = Mathf.MoveTowards(m_CurrentLookWeight, m_TargetLookWeight, ikLerpSpeed * Time.deltaTime);
            m_CurrentPOIPosition = Vector3.MoveTowards(m_CurrentPOIPosition, m_TargetPOIPosition, fokusMoveSpeed * Time.deltaTime);
        }
    }

    void Update()
    {
        if (disableCoroutine != null)
        {
            // As long as the coroutine (StartDisable) is running, m_TargetPOIPosition is updated
            m_TargetPOIPosition = startPosition + transform.position;
            return;
        }

        UpdateIKHead();
    }

    void OnAnimatorIK()
    {
        if (m_Animator != null)
        {
            m_Animator.SetLookAtWeight(m_CurrentLookWeight);
            m_Animator.SetLookAtPosition(m_CurrentPOIPosition);
        }
    }

    /// <summary>
    /// Starts the coroutine to gradually reduce the IK weight, effectively making the NPC stop looking at the player.
    /// This method is triggered when the player leaves the trigger collider, as called by <see cref="NpcIKHeadTrigger.OnTriggerExit"/>.
    /// </summary>
    public void DisableNpcIKHead()
    {
        if (disableCoroutine == null)
        {
            disableCoroutine = StartCoroutine(StartDisable());
        }
    }

    /// <summary>
    /// If a coroutine that disables the NPC's head IK is running, this method stops it,
    /// allowing the NPC to immediately resume looking at the player. This is triggered by <see cref="NpcIKHeadTrigger.OnTriggerEnter"/>.
    /// </summary>
    public void StopDisableCoroutine()
    {
        if (disableCoroutine != null)
        {
            StopCoroutine(disableCoroutine);
            disableCoroutine = null;
        }
    }

    /// <summary>
    /// Coroutine that gradually reduces the IK look weight and moves the NPC's head back to its original position.
    /// This process continues until the IK is fully disabled.
    /// </summary>
    IEnumerator StartDisable()
    {
        while (m_CurrentLookWeight > 0.05f || Vector3.Distance(m_CurrentPOIPosition, m_TargetPOIPosition) > 0.05f)
        {
            m_CurrentLookWeight = Mathf.MoveTowards(m_CurrentLookWeight, 0f, ikLerpSpeed * Time.deltaTime);
            m_CurrentPOIPosition = Vector3.MoveTowards(m_CurrentPOIPosition, m_TargetPOIPosition, fokusMoveSpeed * Time.deltaTime);

            yield return null;
        }

        m_CurrentLookWeight = 0f;
        m_CurrentPOIPosition = startPosition + transform.position;

        enabled = false;

        disableCoroutine = null;
    }

#if UNITY_EDITOR
    #region TLS CONTROLS
    [Header("TLS Controls - ONLY IN UNITY_EDITOR")]

    [SerializeField, Tooltip("Whether to display debug spheres in the Scene view representing the current focus points. Useful for visualizing the NPC's IK focus.")]
    bool showCurrentLookPosition = false;
    
    [SerializeField, Tooltip("The size of the debug spheres drawn in the Scene view when visualizing the current and initial focus positions.")]
    float positionMarkSize = 0.05f;

    void OnDrawGizmos()
    {
        if (showCurrentLookPosition)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPosition + transform.position, positionMarkSize);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(m_CurrentPOIPosition, positionMarkSize);
        }
    }
    #endregion // TLS CONTROLS
#endif
}
