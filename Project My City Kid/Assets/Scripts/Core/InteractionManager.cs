using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages player interaction with objects in the game world.
/// This script performs raycasts to detect interactive objects, handles UI updates for interaction feedback, 
/// and triggers interactions based on player input. It supports both click and hold interactions.
/// </summary>
/// <remarks>
/// To work correctly, this script requires other scripts, such as:
/// <list type="bullet">
/// <item>
/// <description>InteractionTriggerDialogue</description>
/// </item>
/// <item>
/// <description>InteractionTriggerEvent</description>
/// </item>
/// </list>
/// These scripts should handle specific types of interactions and implement logic for triggering interaction events.
/// </remarks>
public class InteractionManager : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField, Tooltip("Image reference for interaction hold feedback.")]
    private Image interactHoldImage;

    [SerializeField, Tooltip("Text component to display the interaction title.")]
    private TMP_Text interactText;

    [Header("INTERACTION DETECTION")]
    [SerializeField, Tooltip("Layer mask of the objects with which the player can interact.")]
    private LayerMask interactLayerMask;

    [SerializeField, Tooltip("Maximum distance of the raycast querying for interaction elements.")]
    private float interactDistance = 4.25f;

    [Header("INTERACTION USER INTERFACE")]
    [SerializeField, Tooltip("Oversize of the interaction hold background. This is a value that will be added to the m_InteractRectTransform")]
    private float interactFillOversize = 0.025f;

    [SerializeField, Tooltip("The speed at which the canvas is faded in or out."),
        Range(0.001f, 1f)]
    private float alphaMaxValue = 6.0f;

    [SerializeField, Tooltip("The speed at which the canvas is faded in or out.")]
    private float alphaChangeSpeed = 6.0f;

    private Transform m_InteractionTransform;
    private GameObject m_InteractHoldGO;
    private RectTransform m_InteractRectTransform;

    private GameObject m_CanvasObject;
    private CanvasGroup m_CanvasGroup;

    private Camera m_MainCamera;

    private RaycastHit hitInfo; // Information about the raycast hit
    private InteractableBase currentInteractable; // Current interactable object
    private Coroutine holdCoroutine; // Coroutine for handling hold interactions
    private Material[] originalMaterials; // Store original materials for highlighting
    private int highlightMaterialIndex = -1; // Last Added Highlight-Materials Index-No

    private bool isHighlighted = false; // Flag to check if the object is highlighted
    private bool m_IsInteracting = false; // Flag to check if currently interacting
    private bool m_DoubleCheck = false; // Flag for double-checking interaction

    #region START AND DISABLE

    void Start()
    {
        InitializeReferences();
        SetupInitialUIState();

        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.OnInteractionEvent.AddListener(HandleInteractionInput);
        }
    }

    void OnDisable()
    {
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.OnInteractionEvent.RemoveListener(HandleInteractionInput);
        }
    }

    /// <summary>
    /// Sets up references to core components such as the main camera, the interaction hold UI image, 
    /// the text display for interaction feedback, and the canvas group for UI fading.
    /// These references are essential for performing raycasts and updating UI elements during interaction.
    /// </summary>
    void InitializeReferences()
    {
        m_MainCamera = Camera.main;

        m_InteractHoldGO = interactHoldImage.gameObject;
        m_InteractRectTransform = interactText.GetComponentInParent<RectTransform>();
        m_InteractionTransform = GetComponent<Transform>();

        m_CanvasObject = m_InteractionTransform.GetComponentInChildren<Canvas>(true).gameObject;
        m_CanvasGroup = m_CanvasObject.GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Configures the initial state of the interaction UI.
    /// Sets the interaction hold image fill to zero, hides the canvas and its elements, 
    /// ensuring no interaction prompts are visible when the game starts.
    /// </summary>
    void SetupInitialUIState()
    {
        interactHoldImage.fillAmount = 0f;
        m_CanvasGroup.alpha = 0f;

        m_InteractHoldGO.SetActive(false);
        m_CanvasObject.SetActive(false);
    }

    #endregion // START AND DISABLE

    #region INTERACTION

    /// <summary>
    /// Performs a raycast from the center of the screen to detect interactive objects within a certain range.
    /// If an interactable object is detected, the method updates UI elements to display relevant interaction information.
    /// It also handles highlighting of objects when necessary and controls UI fade-in and fade-out effects.
    /// </summary>
    void PerformRaycast()
    {
        Ray ray = m_MainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        if (Physics.Raycast(ray, out hitInfo, interactDistance, interactLayerMask))
        {
            var interactableComponent = hitInfo.collider.GetComponent<InteractableBase>();

            if (interactableComponent == null || m_DoubleCheck)
            {
                return;
            }

            if (currentInteractable == interactableComponent)
            {
                UpdateUIElements();

                m_DoubleCheck = true;
                return;
            }

            currentInteractable = interactableComponent;
            UpdateUIElements();

            StopAllCoroutines();
            StartCoroutine(UIFade(alphaMaxValue));

            if (currentInteractable.useHighlight && !isHighlighted)
            {
                AddHighlightMaterial();
                isHighlighted = true;
            }
        }
        else if (currentInteractable != null)
        {
            StopAllCoroutines();
            StartCoroutine(UIFade(0.001f));

            StopHoldingInteraction();

            if (currentInteractable.useHighlight)
            {
                RemoveHighlightMaterial();
                isHighlighted = false;
            }

            m_IsInteracting = false;
            m_DoubleCheck = false;

            currentInteractable = null;
        }
    }

    /// <summary>
    /// Handles player interaction input. 
    /// If the player is interacting with an object, it starts the interaction process. 
    /// If the player stops interacting, it stops any ongoing hold interaction.
    /// </summary>
    void HandleInteractionInput()
    {
        if (currentInteractable == null)
        {
            return;
        }

        if (PlayerInputManager.Instance.IsInteracting)
        {
            if (!m_IsInteracting)
            {
                StartInteraction();
                m_IsInteracting = true;
            }
        }
        else if (m_IsInteracting)
        {
            StopHoldingInteraction();
            m_IsInteracting = false;
        }
    }

    /// <summary>
    /// Begins the interaction process based on the type of interaction (Click or Hold).
    /// - Click interactions immediately trigger the interaction method.
    /// - Hold interactions start a coroutine to handle the progress of the hold.
    /// </summary>
    void StartInteraction()
    {
        switch (currentInteractable.interactionType)
        {
            case InteractableBase.InteractionType.CLICK:
                currentInteractable.Interact();
                break;
            case InteractableBase.InteractionType.HOLD:
                holdCoroutine = StartCoroutine(HoldInteractionRoutine(currentInteractable));
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Stops an ongoing hold interaction. 
    /// This cancels any active coroutine that tracks the hold progress and resets the UI.
    /// </summary>
    void StopHoldingInteraction()
    {
        if (currentInteractable.interactionType == InteractableBase.InteractionType.HOLD)
        {
            if (holdCoroutine != null)
            {
                StopCoroutine(holdCoroutine);
                holdCoroutine = null;
            }

            currentInteractable.ResetHoldTime();
            m_InteractHoldGO.SetActive(false);
            interactHoldImage.fillAmount = 0f;
        }
    }

    /// <summary>
    /// Coroutine that handles the hold interaction logic.
    /// It fills the UI image to reflect the hold progress, and once the hold time is completed, 
    /// it triggers the interaction.
    /// </summary>
    /// <param name="interactable">The interactable object being held.</param>
    /// <returns>IEnumerator to allow frame-by-frame updates for the hold interaction.</returns>
    IEnumerator HoldInteractionRoutine(InteractableBase interactable)
    {
        m_InteractHoldGO.SetActive(true);
        interactable.ResetHoldTime();

        while (PlayerInputManager.Instance.IsInteracting)
        {
            interactable.IncreaseHoldTime();

            interactHoldImage.fillAmount = interactable.GetHoldTime() / interactable.interactionHoldTime;

            if (interactable.GetHoldTime() >= interactable.interactionHoldTime)
            {
                interactable.Interact();
                interactable.ResetHoldTime();
                break;
            }

            yield return null;
        }

        m_InteractHoldGO.SetActive(false);
    }

    #endregion // INTERACTION

    #region UI ELEMENTS AND EFFECTS

    /// <summary>
    /// Updates the UI elements when the player is near an interactable object.
    /// This method activates the canvas, sets the interaction text to display the 
    /// name of the interactable, and adjusts the size of the interaction hold 
    /// background based on the text width.
    /// </summary>
    void UpdateUIElements()
    {
        Canvas canvas = m_CanvasObject.GetComponent<Canvas>();
        canvas.enabled = false;

        m_CanvasObject.SetActive(true);

        m_InteractionTransform.SetParent(currentInteractable.parentForCanvas, false);
        interactText.text = currentInteractable.interactText.ToString();

        canvas.enabled = true;

        Vector2 textWidth = new(m_InteractRectTransform.sizeDelta.x, m_InteractRectTransform.sizeDelta.y);
        RectTransform fillBGRect = interactHoldImage.GetComponent<RectTransform>();
        fillBGRect.sizeDelta = new Vector2(textWidth.x + interactFillOversize, textWidth.y + interactFillOversize);
    }

    /// <summary>
    /// Adds a highlight material to the interactable object's renderer.
    /// This method stores the original materials, adds the highlight material,
    /// and updates the object's materials to include the highlight. It also tracks
    /// the index of the highlight material for future removal.
    /// </summary>
    void AddHighlightMaterial()
    {
        // Speichere das Originalmaterial
        originalMaterials = currentInteractable.objectToHighlight.materials;

        // Konvertiere das Materialarray in eine Liste
        List<Material> materials = new (originalMaterials);

        // Füge das Highlight-Material hinzu und speichere seinen Index
        materials.Add(currentInteractable.highlightMaterial);
        highlightMaterialIndex = materials.Count - 1;

        // Setze die neuen Materialien
        currentInteractable.objectToHighlight.materials = materials.ToArray();
    }

    /// <summary>
    /// Removes the highlight material from the interactable object's renderer.
    /// This method checks if a highlight material was added, removes it from the 
    /// material list, and resets the highlight index to avoid errors in future operations.
    /// </summary>
    void RemoveHighlightMaterial()
    {
        // Prüfe, ob ein Highlight-Material vorhanden ist
        if (isHighlighted && highlightMaterialIndex != -1)
        {
            // Konvertiere das Materialarray in eine Liste
            List<Material> materials = new (currentInteractable.objectToHighlight.materials);

            // Entferne das Highlight-Material an der gespeicherten Position
            materials.RemoveAt(highlightMaterialIndex);

            // Setze die neuen Materialien
            currentInteractable.objectToHighlight.materials = materials.ToArray();

            // Reset the highlight index
            highlightMaterialIndex = -1;
        }
    }

    /// <summary>
    /// Fades the UI canvas in or out based on the target alpha value.
    /// This coroutine smoothly transitions the alpha value of the canvas group
    /// to create a fade effect, allowing for a more visually appealing UI experience.
    /// </summary>
    /// <param name="targetAlphaValue">The target alpha value for the fade effect.</param>
    /// <returns>IEnumerator to allow for frame-by-frame updates while fading.</returns>
    IEnumerator UIFade(float targetAlphaValue)
    {
        while (!Mathf.Approximately(m_CanvasGroup.alpha, targetAlphaValue))
        {
            m_CanvasGroup.alpha = Mathf.Lerp(m_CanvasGroup.alpha, targetAlphaValue, alphaChangeSpeed* Time.deltaTime);
            yield return null;
        }

        m_CanvasGroup.alpha = targetAlphaValue;
        m_CanvasObject.SetActive(targetAlphaValue > 0.001f);
    }

    #endregion // UI ELEMENTS AND EFFECTS

    void Update()
    {
        PerformRaycast();
    }

#if UNITY_EDITOR
    #region TLS CONTROLS
    [Header("TLS Controls - ONLY IN UNITY_EDITOR")]

    [SerializeField, Tooltip("Toggle to visualize the raycast in the scene view. When enabled, it will draw the raycast line and hit indicators.")]
    bool showInteractionRaycast = false;

    [SerializeField, Tooltip("Radius of the sphere that indicates where the raycast hits an object in the scene view.")]
    float raycastHitMarkRadius = 0.05f;

    void OnDrawGizmos()
    {
        if (showInteractionRaycast)
        {
            if (m_MainCamera == null)
            {
                return;
            }

            Ray ray = new(m_MainCamera.transform.position, m_MainCamera.transform.forward);

            Gizmos.color = hitInfo.collider != null ? Color.green : Color.red;
            Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * interactDistance);

            if (hitInfo.collider != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(hitInfo.point, raycastHitMarkRadius);
            }
        }
    }
    #endregion
#endif
}
