using UnityEngine;

/// <summary>
/// The NpcIKHeadTrigger class is responsible for detecting when the player enters or exits the NPC's 
/// trigger collider. It activates and deactivates the NPC's head IK behavior by interacting with the 
/// <see cref="NpcIKHead"/> class. It handles enabling and disabling the IK tracking when the player is 
/// within the interaction range.
/// </summary>
public class NpcIKHeadTrigger : MonoBehaviour
{
    [Header("REFERENCES")]

    [SerializeField, Tooltip("Reference to the NpcIKHead script that controls head IK behavior. If not manually assigned, the script will try to automatically find the NpcIKHead component from the parent object at runtime.")]
    NpcIKHead m_NpcIKHead;

    private const string m_StringPlayerTag = "Player";

    void Start()
    {
        // If m_NpcIKHead is not set, attempt to get it from the parent object
        if (m_NpcIKHead == null)
        {
            m_NpcIKHead = GetComponentInParent<NpcIKHead>();

            if (m_NpcIKHead == null)
            {
#if UNITY_EDITOR
                Debug.LogError("NpcIKHeadTrigger - 'NpcIKHead' is NULL and fetching the component from the parent failed. NpcIKHeadTrigger and GameObject will be disabled!");
#endif
                enabled = false;
                gameObject.SetActive(false);

                return;
            }
        }

        if (m_NpcIKHead.enabled)
        {
            m_NpcIKHead.enabled = false;
        }
    }

    /// <summary>
    /// When the player enters the trigger collider, this method activates the NPC's head IK behavior
    /// by calling <see cref="NpcIKHead.StopDisableCoroutine"/> to ensure the NPC immediately starts
    /// tracking the player's position. It also enables the NpcIKHead script.
    /// </summary>
    /// <param name="targetTag">The collider object that entered the trigger.</param>
    void OnTriggerEnter(Collider targetTag)
    {
        if (targetTag.CompareTag(m_StringPlayerTag))
        {
            // Stop the disable coroutine if it's running
            m_NpcIKHead.StopDisableCoroutine();

            // Enable the NPC IK head behavior
            m_NpcIKHead.enabled = true;
        }
    }

    /// <summary>
    /// When the player exits the trigger collider, this method initiates the coroutine to gradually 
    /// disable the NPC's head IK behavior by calling <see cref="NpcIKHead.DisableNpcIKHead"/>.
    /// </summary>
    /// <param name="targetTag">The collider object that exited the trigger.</param>
    void OnTriggerExit(Collider targetTag)
    {
        if (targetTag.CompareTag(m_StringPlayerTag))
        {
            // Start the process to disable the NPC's IK head
            m_NpcIKHead.DisableNpcIKHead();
        }
    }
}
