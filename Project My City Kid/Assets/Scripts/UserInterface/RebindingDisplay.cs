using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class RebindingManager : MonoBehaviour
{
    [Serializable]
    protected class RebindButton
    {
        public string Name;
        public Transform rebindTransform;
        public Button rebindButtons;
        public TMP_Text rebindText;
    }

    [SerializeField]
    RebindButton []m_RebindButton;
    
    [SerializeField]
    Button m_SaveButton; // Der Save-Button zum Speichern der Änderungen

    [SerializeField]
    GameObject m_WaitingForInputObject; // Anzeige, wenn auf Eingabe gewartet wird

    InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    InputAction[] actionsToRebind; // Array mit allen Aktionen
    InputAction[] temporaryActionsToRebind; // Temporäre Liste für die Bindings

    const string c_RebindsKey = "rebinds"; // Schlüssel für PlayerPrefs
    const string c_MouseString = "Mouse"; // Schlüssel für PlayerPrefs

    void Start()
    {
        var PlayerInput = PlayerInputManager.Instance;

        actionsToRebind = new InputAction[]
        {
            PlayerInput.GetJumpAction(),
            PlayerInput.GetSprintAction(),
            PlayerInput.GetAimAction(),
            PlayerInput.GetGamePauseAction(),
            PlayerInput.GetResumeGameAction()
         };

        if (actionsToRebind.Length != m_RebindButton.Length)
        {
            Debug.LogError("The number of actions does not match the number of binding display texts.");
        }

        // Kopiere die Actions in die temporäre Liste
        CreateTemporaryBindings();

        // Lade die gespeicherten Bindings
        LoadRebinds();

        // Initial: verstecke Save-Button und Back-Button
        m_SaveButton.interactable = false;

        Debug.LogError("In Game Console einbauen für Debugs");
    }

    // Kopiert die aktuellen Bindings in die temporäre Liste
    void CreateTemporaryBindings()
    {
        temporaryActionsToRebind = new InputAction[actionsToRebind.Length];
        for (int i = 0; i < actionsToRebind.Length; i++)
        {
            temporaryActionsToRebind[i] = new InputAction(actionsToRebind[i].name);
            foreach (var binding in actionsToRebind[i].bindings)
            {
                temporaryActionsToRebind[i].AddBinding(binding);
            }
        }
    }

    // Lädt die gespeicherten Rebinds und aktualisiert die UI
    void LoadRebinds()
    {
        string rebinds = PlayerPrefs.GetString(c_RebindsKey, string.Empty);

        if (!string.IsNullOrEmpty(rebinds))
        {
            PlayerInputManager.Instance.PlayerInput.actions.LoadBindingOverridesFromJson(rebinds);
        }

        UpdateBindingDisplayTexts();
    }

    // Aktualisiert alle Textfelder mit den aktuellen Bindings (temporäre Liste)
    void UpdateBindingDisplayTexts()
    {
        for (int i = 0; i < temporaryActionsToRebind.Length; i++)
        {
            InputAction action = temporaryActionsToRebind[i];
            int bindingIndex = action.GetBindingIndexForControl(action.controls[0]);

            m_RebindButton[i].rebindText.text = InputControlPath.ToHumanReadableString(
                action.bindings[bindingIndex].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);
        }
    }

    // Startet den Rebinding-Prozess für eine bestimmte Aktion (auf der temporären Liste)
    public void StartRebinding(int actionIndex)
    {
        m_RebindButton[actionIndex].rebindButtons.interactable = false;
        m_RebindButton[actionIndex].rebindText.text = string.Empty;

        m_WaitingForInputObject.SetActive(true);
        m_WaitingForInputObject.transform.SetParent(m_RebindButton[actionIndex].rebindTransform);
        m_WaitingForInputObject.transform.localPosition = Vector3.zero;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Hole die zu bindende Aktion (aus der temporären Liste)
        InputAction actionToRebind = temporaryActionsToRebind[actionIndex];

        // Starte den Rebinding-Prozess (auf der temporären Liste)
        rebindingOperation = actionToRebind.PerformInteractiveRebinding()
            .WithControlsExcluding(c_MouseString) // Schließe Mausbewegungen aus
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => RebindComplete(actionIndex))
            .Start();

        Debug.Log($"Start Rebinding {m_RebindButton[actionIndex].rebindButtons.name}");
    }

    // Wenn der Rebinding-Prozess abgeschlossen ist
    void RebindComplete(int actionIndex)
    {
        InputAction action = temporaryActionsToRebind[actionIndex];
        int bindingIndex = action.GetBindingIndexForControl(action.controls[0]);

        // Zeige das neue Binding in der UI an
        m_RebindButton[actionIndex].rebindText.text = InputControlPath.ToHumanReadableString(
            action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);

        rebindingOperation.Dispose();

        m_RebindButton[actionIndex].rebindButtons.interactable = true;
        m_WaitingForInputObject.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Setze das Override für die temporäre Action
        m_SaveButton.interactable = true;

        Debug.Log($"Rebind Complete {m_RebindButton[actionIndex].rebindButtons} is in temporaryActionsToRebind-List. New Key is {m_RebindButton[actionIndex].rebindText.text}");

    }

    // Speichert die temporären Bindings und wendet sie auf die Original-Aktionen an
    public void SaveRebinds()
    {
        for (int i = 0; i < temporaryActionsToRebind.Length; i++)
        {
            InputAction originalAction = actionsToRebind[i];
            InputAction tempAction = temporaryActionsToRebind[i];

            // Entferne existierende Overrides im Original
            originalAction.RemoveAllBindingOverrides();

            // Anwenden der temporären Bindings als Overrides auf das Original
            for (int bindingIndex = 0; bindingIndex < tempAction.bindings.Count; bindingIndex++)
            {
                var bindingOverride = tempAction.bindings[bindingIndex];

                // Überprüfe, ob das overridePath nicht null oder leer ist
                if (!string.IsNullOrEmpty(bindingOverride.overridePath))
                {
                    originalAction.ApplyBindingOverride(bindingIndex, bindingOverride.overridePath);
                }
                else
                {
                    Debug.LogWarning($"Binding Override für {originalAction.name} ist leer oder ungültig am Index {bindingIndex}");
                }
            }
        }

        // Speichere die neuen Bindings in PlayerPrefs
        string allRebinds = PlayerInputManager.Instance.PlayerInput.actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(c_RebindsKey, allRebinds);
        PlayerPrefs.Save();

        Debug.Log("Rebinded key bindings saved.");

        // Setze Save-Button und Änderungen zurück
        m_SaveButton.interactable = false;
    }

    // Set the Default Key Assigment
    public void ResetAllBindings()
    {
        // Entferne alle benutzerdefinierten Bindings aus der temporären Liste
        for (int i = 0; i < temporaryActionsToRebind.Length; i++)
        {
            temporaryActionsToRebind[i].RemoveAllBindingOverrides();  // Setzt temporäre Bindings auf Default zurück
        }

        // Aktualisiere die UI mit den Standardwerten der temporären Bindings
        UpdateBindingDisplayTexts();

        // Zeige Save-Button an, um die Änderungen speichern zu können
        m_SaveButton.interactable = true;

        Debug.Log("All key bindings have been reset to defaults (temporarily). Press Save to confirm.");
    }

    // Verwirft die Änderungen und setzt die temporären Bindings auf die Originalwerte zurück
    public void DiscardChanges()
    {
        // Setze die temporären Bindings zurück auf die Originale
        CreateTemporaryBindings();

        // Verstecke Save- und Back-Buttons
        m_SaveButton.interactable = false;

        // Aktualisiere die UI mit den Original-Bindings
        UpdateBindingDisplayTexts();
    }
}
