using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class InteractOnTrigger : MonoBehaviour
{
    [Header("INTERACTION SETTINGS")]

    [Tooltip("Layers that the trigger can interact with.")]
    public LayerMask triggerLayerMask;

    [Header("EVENTS")]

    public UnityEvent OnEnter;
    public UnityEvent OnExit;

    protected WaitForSeconds m_DisableTime;
    protected Collider2D m_Collider;

    private void OnEnable()
    {
        m_Collider = GetComponent<Collider2D>();
    }

    void OnTriggerEnter(Collider target)
    {
        if (!enabled)
            return;

        // Check the LayerMask of GameObject thats hit the collider
        if (triggerLayerMask.Contains(target.gameObject))
        {
            ExecuteOnEnter(target);
        }
    }

    void OnTriggerExit(Collider target)
    {
        if (!enabled)
            return;

        // Check the LayerMask of GameObject thats hit the collider
        if (triggerLayerMask.Contains(target.gameObject))
        {
            ExecuteOnExit(target);
        }
    }

    /// <summary>
    /// Executes actions when the trigger collider is entered.
    /// </summary>
    /// <param name="target">The collider that entered the trigger.</param>
    protected virtual void ExecuteOnEnter(Collider target)
    {
        OnEnter.Invoke();
    }

    /// <summary>
    /// Executes actions when the trigger collider is exited.
    /// </summary>
    /// <param name="target">The collider that exited the trigger.</param>
    protected virtual void ExecuteOnExit(Collider target)
    {
        OnExit.Invoke();
    }
}
