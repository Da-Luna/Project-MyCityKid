using System.Collections;
using UnityEngine;

/// <summary>
/// Abstract base class for all interactable objects in the game. 
/// Defines core functionality such as interaction types (CLICK, HOLD), highlighting, and UI management.
/// Derived classes must implement the interaction logic by overriding the <see cref="Interact"/> method.
/// </summary>
/// <remarks>
/// This base class serves as the foundation for the following derived scripts:
/// <list type="bullet">
/// <item>
/// <description>InteractionTriggerDialogue</description>
/// </item>
/// <item>
/// <description>InteractionTriggerEvent</description>
/// </item>
/// </list>
/// </remarks>
public abstract class InteractableBase : MonoBehaviour
{
    public enum InteractionType { CLICK, HOLD }

    [Header("INTERACTION SETTINGS")]

    [Tooltip("The text that will be displayed on the UI when this object can be interacted with.")]
    public string interactText;

    [Tooltip("The type of interaction for this object. 'CLICK' for immediate interaction and 'HOLD' for interactions that require holding the input.")]
    public InteractionType interactionType;

    [Tooltip("The amount of time (in seconds) the player must hold the interaction button to complete the interaction. Only relevant for HOLD-type interactions.")]
    [Range(0.5f, 60.0f)]
    public float interactionHoldTime;

    protected float holdTime;

    protected WaitForSeconds m_DisableTime;
    protected Collider m_Collider;

    [Header("USER INTERFACE SETTINGS")]

    [Tooltip("The transform that will act as the parent for the interaction UI elements (like the interaction prompt or progress bar).")]
    public Transform parentForCanvas;

    [Tooltip("Determines if the object should highlight when the player is in range. If enabled, the object will be visually emphasized to signal interactivity.")]
    public bool useHighlight;

    [Tooltip("The MeshRenderer component of the object that will receive the highlight effect. This is only required if 'useHighlight' is enabled and don't work with a SkinnedMeshRenderer component")]
    public MeshRenderer objectToHighlight;

    [Tooltip("The material that will be applied to the object when it is highlighted. This should be set if 'useHighlight' is enabled.")]
    public Material highlightMaterial;

    public abstract void Interact();

    public void IncreaseHoldTime() => holdTime += Time.deltaTime;

    public void ResetHoldTime() => holdTime = 0f;

    public float GetHoldTime() => holdTime;

    public void SetInteractionHoldTime(float interactTime)
    {
        interactionHoldTime = interactTime;
    }

    /// <summary>
    /// Disables the collider of the interactable object. 
    /// This method is intended to disable the object's ability to be interacted with. 
    /// Can be used in Unity's Inspector to trigger after an event.
    /// </summary>
    /// <param name="timeBeforeDisable">The time in seconds to wait before disabling the object's collider.</param>
    public void StartDisabling(float timeBeforeDisable)
    {
        m_Collider = GetComponent<Collider>();
        m_DisableTime = new WaitForSeconds(timeBeforeDisable);

        m_Collider.enabled = false;
        StartCoroutine(DisableCoroutine(m_DisableTime));
    }

    /// <summary>
    /// Coroutine that waits for the specified time and then disables the object.
    /// Useful for delaying the deactivation of the object after an interaction is completed.
    /// </summary>
    /// <param name="m_DisableTime">The delay duration before the object is disabled.</param>
    IEnumerator DisableCoroutine(WaitForSeconds m_DisableTime)
    {
        yield return m_DisableTime;
        enabled = false;
    }
}
