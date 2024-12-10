using UnityEngine;
using Cinemachine;

/// <summary>
/// Manages the player's camera system, switching between normal and aiming modes
/// by adjusting camera priorities and sensitivity settings accordingly.
/// </summary>
public class PlayerCamera : MonoBehaviour
{
    [Header("REFERENCES")]

    [SerializeField, Tooltip("The Cinemachine virtual camera used for normal gameplay.")]
    CinemachineVirtualCamera normCamera;

    [SerializeField, Tooltip("The Cinemachine virtual camera used when the player is aiming.")]
    CinemachineVirtualCamera aimCamera;

    [Header("MAIN CAMERA SETTINGS")]

    [SerializeField, Tooltip("Determines how the Cinemachine brain updates the camera (e.g., Fixed Update, Late Update).")]
    CinemachineBrain.UpdateMethod cMBrainUpdateMethod;

    [SerializeField, Tooltip("Duration of the camera blend transition when switching between cameras.")]
    float cameraBlandDuration = 0.25f;

    [Header("NORMAL VIRTUAL CAMERA SETTINGS")]

    [SerializeField, Tooltip("The base vertical sensitivity for normal camera control.")]
    float baseVerticalSpeed = 0.5f;

    [SerializeField, Tooltip("The base horizontal sensitivity for normal camera control.")]
    float baseHorizontalSpeed = 0.25f;

    [Header("AIMING CAMERA SETTINGS")]

    [SerializeField, Tooltip("The vertical sensitivity for camera control when aiming.")]
    float aimVerticalSpeed = 0.5f;

    [SerializeField, Tooltip("The horizontal sensitivity for camera control when aiming.")]
    float aimHorizontalSpeed = 0.25f;

    [Header("GENERAL VIRTUAL CAMERA SETTINGS")]

    [Tooltip("The amount to boost the camera's priority when aiming, allowing the aim camera to take precedence.")]
    public int priorityBoostAimCamera = 10;

    [SerializeField, Tooltip("Multiplier for adjusting the camera's sensitivity when using a gamepad.")]
    float gamepadSensitivityMultiplier = 10f;

    // Tracks whether the camera's priority has already been boosted.
    private bool priorityBoost = false;

    void Start()
    {
#if UNITY_EDITOR
        if (normCamera == null || aimCamera == null)
        {
            Debug.LogError("PlayerCamera - NormCamera or AimCamera is not assigned in the inspector.");
            return;
        }
#endif

        // Check if the main camera has a CinemachineBrain component
        CinemachineBrain cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();

        // If the component doesn't exist, add it
        if (cinemachineBrain == null)
        {
            cinemachineBrain = Camera.main.gameObject.AddComponent<CinemachineBrain>();
#if UNITY_EDITOR
            Debug.LogWarning("CinemachineBrain component was added to the main camera.");
#endif
        }

        // Set the update method and blend duration, whether the component was added or already existed
        cinemachineBrain.m_UpdateMethod = cMBrainUpdateMethod;
        cinemachineBrain.m_DefaultBlend.m_Time = cameraBlandDuration;
    }

    /// <summary>
    /// Updates the priority of the aiming camera based on whether the player is aiming.
    /// Increases priority when aiming and resets it when not aiming.
    /// </summary>
    /// <param name="isAiming">True if the player is currently aiming, false otherwise.</param>
    public void UpdateAimingCamera(bool isAiming)
    {
        if (isAiming)
        {
            // Boost the camera priority if it's not already boosted.
            if (!priorityBoost)
            {
                aimCamera.Priority += priorityBoostAimCamera;
                priorityBoost = true;
            }
        }
        else if (priorityBoost)
        {
            // Reset the camera priority when no longer aiming.
            aimCamera.Priority -= priorityBoostAimCamera;
            priorityBoost = false;
        }

        LookSensitivityAdjust(isAiming);
    }

    /// <summary>
    /// Adjusts the camera's look sensitivity based on whether the player is aiming
    /// and whether a gamepad or other input device is being used.
    /// </summary>
    /// <param name="isAiming">True if the player is currently aiming, false otherwise.</param>
    void LookSensitivityAdjust(bool isAiming)
    {
        var povNorm = normCamera.GetCinemachineComponent<CinemachinePOV>();
        var povAim = aimCamera.GetCinemachineComponent<CinemachinePOV>();

        if (!isAiming)
        {
            if (povNorm != null)
            {
                if (PlayerInputManager.Instance.UpdateControlScheme())
                {
                    // Increase sensitivity by multiplying the original values
                    povNorm.m_VerticalAxis.m_MaxSpeed = baseVerticalSpeed * gamepadSensitivityMultiplier;
                    povNorm.m_HorizontalAxis.m_MaxSpeed = baseHorizontalSpeed * gamepadSensitivityMultiplier;
                }
                else
                {
                    // Reset to the original sensitivity
                    povNorm.m_VerticalAxis.m_MaxSpeed = baseVerticalSpeed;
                    povNorm.m_HorizontalAxis.m_MaxSpeed = baseHorizontalSpeed;
                }
            }
        }
        else
        {
            if (povAim != null)
            {
                if (PlayerInputManager.Instance.UpdateControlScheme())
                {
                    // Increase sensitivity by multiplying the original values
                    povAim.m_VerticalAxis.m_MaxSpeed = aimVerticalSpeed * gamepadSensitivityMultiplier;
                    povAim.m_HorizontalAxis.m_MaxSpeed = aimHorizontalSpeed * gamepadSensitivityMultiplier;
                }
                else
                {
                    // Reset to the original sensitivity
                    povAim.m_VerticalAxis.m_MaxSpeed = aimVerticalSpeed;
                    povAim.m_HorizontalAxis.m_MaxSpeed = aimHorizontalSpeed;
                }
            }
        }
    }
}
