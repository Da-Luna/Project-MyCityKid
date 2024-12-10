using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[RequireComponent(typeof(SettingsSystem))]
/// <summary>
/// Manages the main menu functionality, including navigation between menu pages, starting the game mode, and quitting the application.
/// Handles initialization of required components, such as the Input System UI Input Module, Audio System, Settings System, and Highscore System.
/// Provides methods for activating and deactivating menu canvases, handling input actions for menu navigation, and playing UI animations.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [System.Serializable]
    class MenuCanvases
    {
        [SerializeField]
        string canvasTitle;
        public GameObject canvasObject;
        public Animator animator;
        public Button button;
    }

    [Header("EVENT SYSTEM INPUT MODULE")]
    [SerializeField, Tooltip("The Input System UI Input Module asset responsible for handling input events.")]
    InputSystemUIInputModule inputModule;

    [Header("SETTINGS SYSTEM REFERENCE")]
    [SerializeField, Tooltip("Reference to the SettingsSystem script. If not set, it will be searched for on this GameObject.")]
    SettingsSystem settingsSystem;

    [Header("SETTINGS")]
    [SerializeField, Tooltip("The delay time before the player can interact with the interface after changing menu pages.")]
    float changingMenuPageDelay = 0.85f;
    WaitForSeconds menuChangeDelay;

    [SerializeField, Tooltip("The time before the scene changes after selecting an option.")]
    float sceneChangeDelayTime = 0.85f;
    WaitForSeconds sceneChangeDelay;

    [SerializeField, Tooltip("The Canvas Group responsible for controlling the alpha value of the background.")]
    UICanvasGroupeAlpha backgroundCanvasGroup;

    [Header("MAIN MENU PAGES AND COMPONENTS ")]
    [SerializeField, Tooltip("Array of MainMenuCanvases that representing the different menus objects and components. CanvasName variable will be use to search, only the Button must be set manually.")]
    MenuCanvases[] mainMenu;

    int activeCanvasID;
    
    bool hasBufferTime;
    bool navAudioWasPlayed = false;

    Button lastSelectedMainMenuButton;

    // All UI Animator Controller use the same animation names
    const string uIAnimationIn = "UIIn";
    const string uIAnimationOut = "UIOut";

    #region Initialization

    /// <summary>
    /// Initializes references to required components such as the Input System UI Input Module, Audio System, Settings System, and Highscore System.
    /// </summary>
    void InitReferences()
    {
        // Find and assign the Input System UI Input Module if not assigned
        if (inputModule == null)
        {
            inputModule = GameObject.Find("EventSystem").GetComponent<InputSystemUIInputModule>();
#if UNITY_EDITOR
            if (inputModule != null)
            {
                Debug.Log("MainMenuManager: inputModule is NULL, global search with name **EventSystem** function was used");
            }
            else if (inputModule == null)
            {
                Debug.LogError("MainMenuManager: inputModule still NULL after global search with name **EventSystem**");
                Debug.Break();
            }
#endif
        }

        // Get the SettingsSystem component if not assigned
        if (settingsSystem == null)
        {
            settingsSystem = GetComponent<SettingsSystem>();
#if UNITY_EDITOR
            if (settingsSystem != null)
            {
                Debug.Log("MainMenuManager: settingsSystem is NULL, get component from GameObject");
            }
            else if (settingsSystem == null)
            {
                Debug.LogError("MainMenuManager: settingsSystem still NULL after try to get component from GameObject");
                Debug.Break();
            }
#endif
        }

        // Get the CanvasBackground component if not assigned
        if (backgroundCanvasGroup == null)
        {
            backgroundCanvasGroup = GameObject.Find("CanvasBackground").GetComponent<UICanvasGroupeAlpha>();
#if UNITY_EDITOR
            if (backgroundCanvasGroup != null)
                Debug.Log("MainMenuManager: backgroundCanvasGroup is NULL, global search with name **CanvasBackground** function was used");
            else if (backgroundCanvasGroup == null)
            {
                Debug.LogError("MainMenuManager: backgroundCanvasGroup still NULL after try to local search with name **CanvasBackground**");
                Debug.Break();
            }
#endif
        }
    }

    /// <summary>
    /// Initializes the main menu canvases and sets up their initial state.
    /// </summary>
    void InitMainMenu()
    {
        foreach (MenuCanvases entry in mainMenu)
        {
            if (!entry.canvasObject.activeSelf)
                entry.canvasObject.SetActive(true);

            if (entry.animator == null)
                entry.animator = entry.canvasObject.GetComponent<Animator>();

            entry.canvasObject.SetActive(false);
        }

#if UNITY_EDITOR
        if (mainMenu != null || mainMenu.Length == 0)
        {
            for (int i = 0; i < mainMenu.Length; i++)
            {
                if (mainMenu[i].canvasObject == null)
                    Debug.LogError("Canvas in mainMenu[" + i + "].canvas is NULL.");

                if (mainMenu[i].button == null)
                    Debug.LogError("Button in mainMenu[" + i + "].button is NULL.");

                if (mainMenu[i].animator == null)
                    Debug.LogError("CanvasGroup in mainMenu[" + i + "].animator is NULL.");
            }
        }
        else 
        {
            Debug.LogError("MainMenuManager: mainMenu is NULL");
            Debug.Break();
        }
#endif
    }

    #endregion // Initialization

    void OnEnable()
    {
        InitReferences();

        settingsSystem.InitMixerSliderReferences();

        InitMainMenu();
        activeCanvasID = 0;

        EnableInputs();

        mainMenu[activeCanvasID].canvasObject.SetActive(true);
        mainMenu[activeCanvasID].button.Select();

        lastSelectedMainMenuButton = mainMenu[activeCanvasID].button;

        menuChangeDelay = new WaitForSeconds(changingMenuPageDelay);
        sceneChangeDelay = new WaitForSeconds(sceneChangeDelayTime);

        hasBufferTime = false;
    }

    void Start()
    {
        // NOTE - If settingsSystem.LoadMixerVolumeValues() will execute in OnEnable, settings will not applyed!
        settingsSystem.LoadMixerVolumeValues();
    }

    void OnDisable()
    {
        DisableInputs();
    }

    #region Button OnClick Methods

    /// <summary>
    /// Activates the canvas associated with the given element number.
    /// </summary>
    /// <param name="elementNo">Index of the canvas to activate.</param>
    public void ButtonActivateCanvas(int elementNo)
    {
        if (hasBufferTime) return;

        hasBufferTime = true;

        lastSelectedMainMenuButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();

        activeCanvasID = elementNo;
        mainMenu[activeCanvasID].canvasObject.SetActive(true);

        PlayUIAnimation(mainMenu[0].animator, uIAnimationOut);
        StartCoroutine(CanvasActivateBuffer());

        PlayUIAnimation(mainMenu[activeCanvasID].animator, uIAnimationIn);
    }
    
    /// <summary>
    /// Deactivates the currently active canvas.
    /// </summary>
    public void ButtonDeactivateCurrentCanvas()
    {
        if (activeCanvasID == 0) return;
        if (hasBufferTime) return;

        PlayUIAnimation(mainMenu[activeCanvasID].animator, uIAnimationOut);
        PlayUIAnimation(mainMenu[0].animator, uIAnimationIn);

        StartCoroutine(CanvasDeactivateBuffer());
    }

    /// <summary>
    /// Starts the game mode by transitioning to the specified scene.
    /// </summary>
    /// <param name="sceneBuildIndex">The build index of the scene to transition to.</param>
    public void ButtonStartGameMode(int sceneBuildIndex)
    {
        PlayUIAnimation(mainMenu[activeCanvasID].animator, uIAnimationOut);

        backgroundCanvasGroup.startValue = 1.0f;
        backgroundCanvasGroup.targetValue = 0.0f;
        backgroundCanvasGroup.changeMultipier = 1.2f;
        backgroundCanvasGroup.enabled = true;


        StartCoroutine(CanvasStartGameDelay(sceneBuildIndex));
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void ButtonQuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        Debug.Log("Quit Game Button was pressed");
#endif
    }

    #endregion // Button OnClick Methods

    #region Coroutines to Activate and Deactivate Canvas Elements

    /// <summary>
    /// Coroutine to add a buffer time before activating a canvas.
    /// </summary>
    IEnumerator CanvasActivateBuffer()
    {
        yield return menuChangeDelay;
        mainMenu[activeCanvasID].button.Select();

        hasBufferTime = false;
    }

    /// <summary>
    /// Coroutine to add a buffer time before deactivating a canvas.
    /// </summary>
    IEnumerator CanvasDeactivateBuffer()
    {
        hasBufferTime = true;

        yield return menuChangeDelay;
        mainMenu[activeCanvasID].canvasObject.SetActive(false);

        lastSelectedMainMenuButton.Select();

        activeCanvasID = 0;
        hasBufferTime = false;
    }

    /// <summary>
    /// Coroutine to add a delay before starting the game mode.
    /// </summary>
    IEnumerator CanvasStartGameDelay(int sceneBuildIndex)
    {
        yield return sceneChangeDelay;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneBuildIndex);
    }

    #endregion

    #region Menu Navigation from Input Module

    /// <summary>
    /// Enables the input actions for menu navigation.
    /// </summary>
    void EnableInputs()
    {
        inputModule.cancel.action.performed += ReadCancelInput;
        inputModule.move.action.performed += ReadMoveInput;
    }

    /// <summary>
    /// Disables the input actions for menu navigation.
    /// </summary>
    void DisableInputs()
    {
        inputModule.cancel.action.performed -= ReadCancelInput;
        inputModule.move.action.performed -= ReadMoveInput;
    }

    /// <summary>
    /// Reads the cancel input action and deactivates the current canvas.
    /// </summary>
    /// <param name="context">The context of the input action.</param>
    void ReadCancelInput(InputAction.CallbackContext context)
    {
        if (context.performed)
            ButtonDeactivateCurrentCanvas();
    }

    /// <summary>
    /// Reads the move input action and plays navigation audio feedback.
    /// </summary>
    /// <param name="context">The context of the input action.</param>
    void ReadMoveInput(InputAction.CallbackContext context)
    {
        if (hasBufferTime) return;

        if (inputModule.move.action.ReadValue<Vector2>() == Vector2.zero)
        {
            navAudioWasPlayed = false;
        }
        else if (navAudioWasPlayed)
        {
            return;
        }
        else if (inputModule.move.action.ReadValue<Vector2>() != Vector2.zero)
        {
            navAudioWasPlayed = true;
        }
    }

    #endregion

    /// <summary>
    /// Plays the specified UI animation on the given animator.
    /// </summary>
    /// <param name="animator">The animator component to play the animation on.</param>
    /// <param name="animationName">The name of the animation to play.</param>
    void PlayUIAnimation(Animator animator, string animationName)
    {
        if (animator == null) return;

        animator.Play(animationName);
    }
}
