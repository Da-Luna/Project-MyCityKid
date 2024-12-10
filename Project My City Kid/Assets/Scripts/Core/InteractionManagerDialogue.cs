using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

/// <summary>
/// Represents a character in the dialogue system, including their name and associated icon.
/// </summary>
[System.Serializable]
public class DialogueCharacter
{
    [Tooltip("The name of the character.")]
    public string name; // The name of the character.

    [Tooltip("The icon representing the character.")]
    public Sprite icon; // The icon representing the character.
}

/// <summary>
/// Represents a single line of dialogue spoken by a character, including the character and the dialogue text.
/// </summary>
[System.Serializable]
public class DialogueLine
{
    [Tooltip("The character speaking the line.")]
    public DialogueCharacter character; // The character speaking the line.

    [TextArea(3, 10), Tooltip("The actual line of dialogue spoken by the character.")]
    public string line; // The actual line of dialogue.
}

/// <summary>
/// Represents a collection of dialogue lines that can be displayed in sequence during interactions.
/// </summary>
[System.Serializable]
public class Dialogue
{
    [Tooltip("A list of dialogue lines for the conversation.")]
    public List<DialogueLine> dialogueLines = new(); // A list of dialogue lines for the conversation.
}

/// <summary>
/// Manages the dialogue system within the game, handling the display of dialogue lines and the interactions 
/// between the player and dialogue-triggering objects, specifically those of type <see cref="InteractionTriggerDialogue"/>. 
/// This class provides functionality to start, display, and end dialogue sequences, utilizing a queue to manage 
/// multiple dialogue lines from a <see cref="Dialogue"/> object. It also controls the UI elements associated with the dialogue, 
/// such as the character's icon and name, and provides feedback on dialogue progress through typing animations. 
/// Additionally, it manages input states, ensuring the player can only interact with the UI during dialogue.
/// </summary>
public class InteractionManagerDialogue : MonoBehaviour
{
    static protected InteractionManagerDialogue s_DialogueManagerInstance;
    static public InteractionManagerDialogue Instance { get { return s_DialogueManagerInstance; } }

    [Header("REFERENCES")]

    [SerializeField, Tooltip("The image component that displays the character's icon during dialogue.")]
    Image characterIcon;

    [SerializeField, Tooltip("The text component that displays the character's name during dialogue.")]
    TextMeshProUGUI titleText;

    [SerializeField, Tooltip("The text component that displays the dialogue line.")]
    TextMeshProUGUI dialogueText;

    [SerializeField, Tooltip("The button that allows the player to progress through the dialogue.")]
    Button dialogeButton;


    [SerializeField, Tooltip("The time interval between each character being typed in normal typing speed.")]
    float dialogueNormalTypingInterval = 0.2f;

    [SerializeField, Tooltip("The time interval between each character being typed in fast typing speed.")]
    float dialogueFastTypingInterval = 0.05f;

    private Queue<DialogueLine> lines;
    protected WaitForSeconds m_TypingInterval;

    public bool IsDialogueActive { get; protected set; } // Indicates whether a dialogue is currently active.

    private Animator m_Animator; // Reference to the Animator component for dialogue animations.
    private bool m_FinishTyping; // Flag to indicate if typing is finished for the current dialogue line.

    private const string m_StringUIActionMap = "UI"; // Action map name for UI interactions.
    private const string m_StringPlayerActionMap = "Player"; // Action map name for player interactions.

    private readonly int m_HashFadeInPara = Animator.StringToHash("FadeIn"); // Hash for fade-in animation.
    private readonly int m_HashFadeOutPara = Animator.StringToHash("FadeOut"); // Hash for fade-out animation.

    private InteractionTriggerDialogue currentTrigger; // Reference to the dialogue trigger that started the current dialogue.

    void Awake()
    {
        s_DialogueManagerInstance = this;
        lines = new Queue<DialogueLine>();
    }

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_FinishTyping = true;
    }

    /// <summary>
    /// Starts the dialogue sequence with the provided <see cref="Dialogue"/> and associates it with the given <see cref="InteractionTriggerDialogue"/>.
    /// </summary>
    /// <param name="dialogue">The dialogue object containing the lines to be displayed.</param>
    /// <param name="trigger">The trigger that initiated the dialogue.</param>
    public void StartDialogue(Dialogue dialogue, InteractionTriggerDialogue trigger)
    {
        IsDialogueActive = true;

        currentTrigger = trigger;

        PlayerInputManager.Instance.SwitchActionMap(m_StringUIActionMap);
        PlayerInputManager.Instance.SetCursorState(false);

        m_Animator.ResetTrigger(m_HashFadeOutPara);
        m_Animator.SetTrigger(m_HashFadeInPara);

        lines.Clear();

        foreach (DialogueLine dialogueLine in dialogue.dialogueLines)
        {
            lines.Enqueue(dialogueLine);
        }

        StartCoroutine(StartFirstDialogueLine());
    }

    IEnumerator StartFirstDialogueLine()
    {
        yield return new WaitForEndOfFrame();
        DisplayNextDialogueLine();
    }

    /// <summary>
    /// Displays the next line of dialogue. If there are no more lines, it ends the dialogue.
    /// </summary>
    public void DisplayNextDialogueLine()
    {
        if (!m_FinishTyping)
        {
            m_TypingInterval = new WaitForSeconds(dialogueFastTypingInterval);
            dialogeButton.interactable = false;
            return;
        }
        else if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        m_FinishTyping = false;
        m_TypingInterval = new WaitForSeconds(dialogueNormalTypingInterval);

        dialogeButton.interactable = true;
        dialogeButton.Select();

        DialogueLine currentLine = lines.Dequeue();
        characterIcon.sprite = currentLine.character.icon;
        titleText.text = currentLine.character.name;

        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentLine));
    }

    /// <summary>
    /// Types out the given dialogue line character by character.
    /// </summary>
    /// <param name="dialogueLine">The line of dialogue to be typed out.</param>
    /// <returns>An enumerator for the coroutine.</returns>
    IEnumerator TypeSentence(DialogueLine dialogueLine)
    {
        dialogueText.text = string.Empty;

        foreach (char letter in dialogueLine.line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return m_TypingInterval;
        }

        m_FinishTyping = true;
        dialogeButton.interactable = true;
        dialogeButton.Select();
    }

    /// <summary>
    /// Ends the dialogue sequence, hides the dialogue UI, and resets input controls.
    /// </summary>
    void EndDialogue()
    {
        m_Animator.SetTrigger(m_HashFadeOutPara);

        PlayerInputManager.Instance.SwitchActionMap(m_StringPlayerActionMap);
        PlayerInputManager.Instance.SetCursorState(true);

        IsDialogueActive = false;

        if (currentTrigger != null)
        {
            currentTrigger.OnDialogueEnd();
            currentTrigger = null;
        }
    }
}
