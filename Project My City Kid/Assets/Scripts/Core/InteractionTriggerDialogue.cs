using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This class handles dialogue interactions when the player interacts with an object. 
/// It inherits from <see cref="InteractableBase"/> and requires the <see cref="InteractionManagerDialogue"/> 
/// to manage dialogue events. The class accesses <see cref="InteractionManagerDialogue.Instance.IsDialogueActive"/> 
/// to determine if a dialogue is currently active and uses <see cref="InteractionManagerDialogue.Instance.StartDialogue"/> 
/// to initiate the dialogue sequence. It triggers events for starting and ending dialogues, 
/// and it can also rotate to face the player during interactions.
/// </summary>
public class InteractionTriggerDialogue : InteractableBase
{
    [Header("ROTATING SETTING")]

    [SerializeField, Tooltip("Set this to true if the object should rotate to face the player when interacting.")]
    bool rotateToPlayer;

    [SerializeField, Tooltip("The speed at which the object rotates towards the player (in degrees per second).")]
    float rotationSpeed = 4f;

    [Header("DIALOGUE")]

    [SerializeField, Tooltip("The dialogue asset that will be triggered during the interaction.")]
    Dialogue dialogue;

    [Header("EVENTS ON INTERACT")]

    [Tooltip("Event that gets invoked when dialogue starts.")]
    public UnityEvent OnDialogueStartEvent;

    [Tooltip("Event that gets invoked when dialogue ends.")]
    public UnityEvent OnDialogueEndEvent;

    public override void Interact()
    {
        var dialogueManager = InteractionManagerDialogue.Instance;
        if (dialogueManager == null)
        {
#if UNITY_EDITOR
            StartDisabling(0f);
            enabled = false;
            Debug.LogError($"InteractionManagerDialogue.Instance is NULL. Ensure that the Interaction Manager is initialized before using this script. Script will be disabled.");
#endif
            return;
        }

        if (!dialogueManager.IsDialogueActive)
        {
            OnDialogueStartEvent?.Invoke();
            SetInteractionHoldTime(interactionHoldTime);

            dialogueManager.StartDialogue(dialogue, this);

            if (rotateToPlayer)
            {
                StartCoroutine(RotateToPlayer());
            }
        }
    }

    /// <summary>
    /// Invoked to signal the end of a dialogue interaction. 
    /// This method triggers the <see cref="OnDialogueEndEvent"/> event, allowing any subscribed listeners to respond to the end of the dialogue. 
    /// It also disables this component to prevent further interactions until re-enabled, ensuring that the dialogue state is managed properly.
    /// </summary>
    public void OnDialogueEnd()
    {
        OnDialogueEndEvent?.Invoke();
        enabled = false;
    }

    /// <summary>
    /// Coroutine that smoothly rotates the object to face the player.
    /// </summary>
    /// <returns>An enumerator to control the coroutine.</returns>
    IEnumerator RotateToPlayer()
    {
        var player = PlayerCharacter.PlayerInstance.transform;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.05f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            yield return null;
        }

        transform.rotation = targetRotation;
    }
}
