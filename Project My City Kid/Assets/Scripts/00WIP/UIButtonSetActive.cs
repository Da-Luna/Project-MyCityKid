
using UnityEngine;

public class UIButtonSetActive : MonoBehaviour
{
    [SerializeField, Tooltip("The reference object that is to be activated or deactivated")]
    GameObject targetGameObject;

    [SerializeField, Tooltip("If this checkbox is aktive, the GameObject will be enabled. Otherwise it will be disabled.")]
    bool setActiveTrue;

    public void ButtonSetActiveState()
    {
        if (setActiveTrue) targetGameObject.SetActive(true);
        else targetGameObject.SetActive(false);
    }
}
