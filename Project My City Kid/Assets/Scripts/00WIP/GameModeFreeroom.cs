using UnityEngine;
using Cinemachine;

public class GameModeFreeroom : MonoBehaviour
{
    [SerializeField, Tooltip("")]
    GameObject pauseCanvasObject;

    [SerializeField, Tooltip("")]
    float pausePressDelay;

    [Header("VIRTUAL CAMARA SETTINGS")]

    [SerializeField, Tooltip("")]
    CinemachineVirtualCamera gUICamera;

    [Tooltip("The amount to boost the camera's priority when aiming.")]
    public int priorityBoostAmount = 10;

    protected bool priorityBoost = false;

    const string c_PlayerActionMapKey = "Player";
    const string c_UIActionMapKey = "UI";

    void Start()
    {
        pauseCanvasObject.SetActive(false);

        var PlayerInput = PlayerInputManager.Instance;
        PlayerInput.OnGamePauseEvent.AddListener(PauseGame);
        PlayerInput.OnResumePauseEvent.AddListener(ResumeGame);
    }

    void OnDisable()
    {
        var PlayerInput = PlayerInputManager.Instance;
        PlayerInput.OnGamePauseEvent.RemoveListener(PauseGame);
        PlayerInput.OnResumePauseEvent.RemoveListener(ResumeGame);
    }

    void PauseGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        pauseCanvasObject.SetActive(true);

        CameraLookReset();
        UpdatePauseCamera(true);

        PlayerInputManager.Instance.SwitchActionMap(c_UIActionMapKey);
        PlayerInputManager.Instance.SetPauseState(true);


        Debug.Log("GameManager : PauseGame()");
    }

    public void ResumeGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pauseCanvasObject.SetActive(false);

        UpdatePauseCamera(false);

        PlayerInputManager.Instance.SwitchActionMap(c_PlayerActionMapKey);
        PlayerInputManager.Instance.SetPauseState(false);

        Debug.Log("GameManager : ResumeGame()");
    }

    void UpdatePauseCamera(bool isPaused)
    {
        if (isPaused)
        {
            // Boost the camera priority if it's not already boosted.
            if (!priorityBoost)
            {
                gUICamera.Priority += priorityBoostAmount;
                priorityBoost = true;
            }
        }
        else if (priorityBoost)
        {
            // Reset the camera priority when no longer aiming.
            gUICamera.Priority -= priorityBoostAmount;
            priorityBoost = false;
        }
    }

    void CameraLookReset()
    {
        var pov = gUICamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        pov.m_XAxis.Value = 0f;
    }

    public void QuitGameButton()
    {
        Application.Quit();
    }
}
