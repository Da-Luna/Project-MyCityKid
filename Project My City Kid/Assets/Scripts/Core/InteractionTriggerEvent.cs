using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This class triggers specific events based on player interaction and inherits from <see cref="InteractableBase"/>. 
/// It supports both single interactions and toggle switches. 
/// This class will be utilized and triggered by the <see cref="InteractionManager"/> to manage interaction events.
/// </summary>
public class InteractionTriggerEvent : InteractableBase
{
    /// <summary>
    /// Defines the method of interaction. 
    /// 'SINGLE' triggers an event once, while 'SWITCH' toggles between two events.
    /// </summary>
    [SerializeField]
    enum InteractionMethod { SINGLE, SWITCH }

    [Header("EVENT TYPE SETTING")]

    [SerializeField, Tooltip("Specifies the interaction method for this event. Use 'SINGLE' for one-time interactions or 'SWITCH' for toggling events.")]
    InteractionMethod interactionMethod;

    [Header("EVENTS ON INTERACT")]

    [Tooltip("The event that will be triggered on the first interaction. Can be assigned in the Inspector.")]
    public UnityEvent OnFirstInteractionEvent;

    [Tooltip("The event that will be triggered on the second interaction. This is only relevant for 'SWITCH' interaction method.")]
    public UnityEvent OnSecondInteractionEvent;

    protected bool isOn; // used only for the InteractionMethod 'SWITCH'

    public override void Interact()
    {
        SetInteractionHoldTime(interactionHoldTime);

        switch (interactionMethod)
        {
            case InteractionMethod.SINGLE:
                SingleEvent();
                break;
            case InteractionMethod.SWITCH:
                SwitchEvent();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Triggers the first interaction event when called.
    /// </summary>
    void SingleEvent()
    {
        OnFirstInteractionEvent?.Invoke();
    }

    /// <summary>
    /// Toggles between the first and second interaction events based on the current state.
    /// </summary>
    void SwitchEvent()
    {
        if (!isOn)
            OnFirstInteractionEvent?.Invoke();
        else
            OnSecondInteractionEvent?.Invoke();

        isOn = !isOn;
    }
}
