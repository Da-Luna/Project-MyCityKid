using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class InteractOnCollision: MonoBehaviour
{
    [Header("INTERACTION SETTINGS")]

    [Tooltip("Layers that the trigger can interact with.")]
    public LayerMask triggerLayerMask;

    [Header("EVENTS")]

    public UnityEvent OnEnter;

    protected WaitForSeconds m_DisableTime;
    protected Collider2D m_Collider;

    private void OnEnable()
    {
        if (m_Collider == null)
            m_Collider = GetComponent<Collider2D>();
    }

    void OnCollisionEnter(Collision target)
    {
        if (!enabled)
            return;

        if (triggerLayerMask.Contains(target.gameObject))
        {
            ExecuteOnEnter(target);
            Debug.Log($"void OnCollisionEnter - ExecuteOnEnter");
        }

        Debug.Log($"void OnCollisionEnter");
    }

    /// <summary>
    /// Executes actions when the trigger collider is entered.
    /// </summary>
    /// <param name="target">The collider that entered the trigger.</param>
    protected virtual void ExecuteOnEnter(Collision target)
    {
        OnEnter.Invoke();
        Debug.Log($"virtual void ExecuteOnEnter - OnEnter.Invoke();");
    }
}
