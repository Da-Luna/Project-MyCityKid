using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlayerInputManager : MonoBehaviour
{
    static protected PlayerInputManager s_PlayerInputInstance;
    static public PlayerInputManager Instance { get { return s_PlayerInputInstance; } }

    public enum UsedDevise { PC, GAMEPAD }
    public UsedDevise CurrentUserDevice { get; protected set; }

    protected PlayerInput m_PlayerInput;

    public PlayerInput PlayerInput => m_PlayerInput;
    
    [Header("CURSOR SETTINGS")]
    public bool lockCursorByStart;

    protected InputAction m_LookAction;

    const string m_StringInputMovement = "Movement";
    protected InputAction m_MovementAction;

    const string m_StringJump = "Jump";
    protected InputAction m_JumpAction;
    public UnityEvent OnJumpEvent;

    const string m_StringSprintHold = "SprintHold";
    protected InputAction m_SprintHoldAction;
    public UnityEvent OnSprintHoldEvent;

    const string m_StringAim = "Aim";
    protected InputAction m_AimAction;
    public UnityEvent OnAimEvent;

    const string m_StringInteraction = "Interaction";
    public UnityEvent OnInteractionEvent;
    protected InputAction m_InteractionAction;
    public bool IsInteracting { get; protected set; }

    const string m_StringGamePause = "GamePause";
    public UnityEvent OnGamePauseEvent;
    protected InputAction m_GamePauseAction;

    const string m_StringResumeGame = "ResumeGame";
    public UnityEvent OnResumePauseEvent;
    protected InputAction m_ResumeGameAction;

    public bool GamePauseRequest { get; protected set; }
    public bool JumpRequest { get; protected set; }

    const string m_StringControlSchemePC = "KeyboardAndMouse";
    const string m_StringControlSchemeGP = "Gamepad";

    void Awake()
    {
        s_PlayerInputInstance = this;
        m_PlayerInput = GetComponent<PlayerInput>();

        m_MovementAction = m_PlayerInput.actions.FindAction(m_StringInputMovement);
        
        m_JumpAction = m_PlayerInput.actions.FindAction(m_StringJump);
        m_JumpAction.performed += OnJumpInput;
        
        m_SprintHoldAction = m_PlayerInput.actions.FindAction(m_StringSprintHold);
        
        m_AimAction = m_PlayerInput.actions.FindAction(m_StringAim);

        m_InteractionAction = m_PlayerInput.actions.FindAction(m_StringInteraction);
        m_InteractionAction.performed += OnInteractionInput;
        m_InteractionAction.canceled += OnInteractionInput;

        m_GamePauseAction = m_PlayerInput.actions.FindAction(m_StringGamePause);
        m_GamePauseAction.performed += OnGamePauseInput;

        m_ResumeGameAction = m_PlayerInput.actions.FindAction(m_StringResumeGame);
        m_ResumeGameAction.performed += OnResumeGameInput;
    }

    void OnDisable()
    {
        m_JumpAction.performed -= OnJumpInput;

        m_InteractionAction.performed -= OnInteractionInput;
        m_InteractionAction.canceled -= OnInteractionInput;

        m_GamePauseAction.performed -= OnGamePauseInput;
        m_ResumeGameAction.performed -= OnGamePauseInput;
    }

    void Start()
    {
        SetCursorState(lockCursorByStart);
    }

    #region GET INPUT_ACTIONS

    public InputAction GetMovementAction()
    {
        return m_MovementAction;
    }

    public InputAction GetJumpAction()
    {
        return m_JumpAction;
    }

    public InputAction GetSprintAction()
    {
        return m_SprintHoldAction;
    }

    public InputAction GetAimAction()
    {
        return m_AimAction;
    }

    public InputAction GetGamePauseAction()
    {
        return m_GamePauseAction;
    }

    public InputAction GetResumeGameAction()
    {
        return m_ResumeGameAction;
    }

    #endregion // GET METHODS

    #region RETURN VALUES

    public Vector2 OnMovementInput()
    {
        return m_MovementAction.ReadValue<Vector2>();
    }

    public bool OnJumpInput()
    {
        return m_JumpAction.triggered;
    }

    public float OnSprintInput()
    {
        return m_SprintHoldAction.ReadValue<float>();
    }

    public float OnAimInput()
    {
        return m_AimAction.ReadValue<float>();
    }

    #endregion // ON METHODS

    #region ON INPUT EVENT

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (JumpRequest)
            return;

        if (context.performed)
        {
            JumpRequest = true;
            OnJumpEvent?.Invoke();
        }
    }

    public void OnInteractionInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsInteracting = true;
            OnInteractionEvent?.Invoke();
        }
        if (context.canceled)
        {
            IsInteracting = false;
            OnInteractionEvent?.Invoke();
        }
    }

    public void OnGamePauseInput(InputAction.CallbackContext context)
    {
        if (GamePauseRequest)
            return;

        if (context.performed)
        {
            GamePauseRequest = true;
            OnGamePauseEvent?.Invoke();
        }
    }

    public void OnResumeGameInput(InputAction.CallbackContext context)
    {
        if (!GamePauseRequest)
            return;

        if (context.performed)
            OnResumePauseEvent?.Invoke();
    }

    #endregion // ON INPUT EVENT

    #region OTHER METHODS

    public void SwitchActionMap(string mapKey)
    {
        PlayerInput.SwitchCurrentActionMap(mapKey);
#if UNITY_EDITOR
        Debug.Log($"PlayerInputManager : SwitchActionMap{mapKey}");
#endif
    }

    public void SetPauseState(bool bin)
    {
        GamePauseRequest = bin;
    }

    public void SetCursorState(bool isLocked)
    {
        if (isLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public bool UpdateControlScheme()
    {
        // Check the current control scheme from the PlayerInput component
        string controlScheme = m_PlayerInput.currentControlScheme;

        // Set the user device based on the control scheme directly
        if (controlScheme == m_StringControlSchemeGP)
        {
            CurrentUserDevice = UsedDevise.GAMEPAD;
            return true; 
        }
        else if (controlScheme == m_StringControlSchemePC)
        {
            CurrentUserDevice = UsedDevise.PC;
            return false;
        }

        return false;
    }

    #endregion // OTHER METHODS
}
