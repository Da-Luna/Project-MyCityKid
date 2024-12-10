using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Manages the settings menu functionality, including volume sliders for different audio channels.
/// Handles initialization of slider references, loading saved volume values, and setting volume levels.
/// </summary>
public class SettingsSystem : MonoBehaviour
{
    [SerializeField, Tooltip("Slider for the main audio output.")]
    Slider sliderAudioMain;
    [SerializeField, Tooltip("Value of the main audio slider.")]
    float sliderMainValue;

    [SerializeField, Tooltip("Slider for the background music audio output.")]
    Slider sliderAudioBGM;
    [SerializeField, Tooltip("Value of the background music audio slider.")]
    float sliderBGMValue;

    [SerializeField, Tooltip("Slider for the UI audio output.")]
    Slider sliderAudioUI;
    [SerializeField, Tooltip("Value of the UI audio slider.")]
    float sliderUIValue;

    [SerializeField, Tooltip("Reference to the AudioMixer used to control audio levels.")]
    AudioMixer audioMixer;

    /// <summary>
    /// Initializes references to slider components and assigns them to their respective variables.
    /// </summary>
    public void InitMixerSliderReferences()
    {
#if UNITY_EDITOR
        if (audioMixer == null)
        {
            Debug.LogError("SettingsSaveSystem: audioMixer still NULL and can not be searched");
            Debug.Break();
        }
#endif
        GameObject settingsPanel = GameObject.Find("SettingsPanel");

        // Find and assign the Main Slider References if not assigned
        if (sliderAudioMain == null)
        {
            sliderAudioMain = settingsPanel.transform.Find("SliderAudioMain").GetComponent<Slider>();
#if UNITY_EDITOR
            if (sliderAudioMain != null)
                Debug.Log("SettingsSaveSystem: sliderAudioMain is NULL, local search with name **SliderAudioMain** function was used");
            else if (sliderAudioMain == null)
            {
                Debug.LogError("SettingsSaveSystem: sliderAudioMain still NULL after try to local search with name **SliderAudioMain**");
                Debug.Break();
            }
#endif
        }

        // Find and assign the BGM Slider References if not assigned
        if (sliderAudioBGM == null)
        {
            sliderAudioBGM = settingsPanel.transform.Find("SliderAudioBGM").GetComponent<Slider>();
#if UNITY_EDITOR
            if (sliderAudioBGM != null)
                Debug.Log("SettingsSaveSystem: sliderAudioBGM is NULL, local search with name **SliderAudioBGM** function was used");
            else if (sliderAudioBGM == null)
            {
                Debug.LogError("SettingsSaveSystem: sliderAudioBGM still NULL after try to local search with name **SliderAudioBGM**");
                Debug.Break();
            }
#endif
        }

        // Find and assign the UI Slider References if not assigned
        if (sliderAudioUI == null)
        {
            sliderAudioUI = settingsPanel.transform.Find("SliderAudioUI").GetComponent<Slider>();
#if UNITY_EDITOR
            if (sliderAudioUI != null)
                Debug.Log("SettingsSaveSystem: sliderAudioUI is NULL, local search with name **SliderAudioUI** function was used");
            else if (sliderAudioUI == null)
            {
                Debug.LogError("SettingsSaveSystem: sliderAudioUI still NULL after try to local search with name **SliderAudioUI**");
                Debug.Break();
            }
#endif
        }
    }

    /// <summary>
    /// Loads the saved mixer volume values from player preferences and sets the sliders accordingly.
    /// </summary>
    public void LoadMixerVolumeValues()
    {
        bool hasMainKey = PlayerPrefs.HasKey("sliderMainValueKey");
        if (hasMainKey)
        {
            sliderMainValue = PlayerPrefs.GetFloat("sliderMainValueKey");
            sliderAudioMain.value = sliderMainValue;
            SliderSetMainVolume(sliderAudioMain.value);
        }
        else if(!hasMainKey)
        {
            sliderMainValue = 1.0f;
            sliderAudioMain.value = sliderMainValue;
            SliderSetMainVolume(sliderAudioMain.value);
        }

        bool hasBGMKey = PlayerPrefs.HasKey("sliderBGMValueKey");
        if (hasBGMKey)
        {
            sliderBGMValue = PlayerPrefs.GetFloat("sliderBGMValueKey");
            sliderAudioBGM.value = sliderBGMValue;
            SliderSetBGMVolume(sliderAudioBGM.value);
        }
        else if(!hasBGMKey)
        {
            sliderBGMValue = 1.0f;
            sliderAudioBGM.value = sliderBGMValue;
            SliderSetBGMVolume(sliderAudioBGM.value);
        }

        bool hasUIKey = PlayerPrefs.HasKey("sliderUIValueKey");
        if (hasUIKey)
        {
            sliderUIValue = PlayerPrefs.GetFloat("sliderUIValueKey");
            sliderAudioUI.value = sliderUIValue;
            SliderSetUIVolume(sliderAudioUI.value);
        }
        else if(!hasUIKey)
        {
            sliderUIValue = 1.0f;
            sliderAudioUI.value = sliderUIValue;
            SliderSetUIVolume(sliderAudioUI.value);
        }
    }

    /// <summary>
    /// Sets the main volume level and saves it to player preferences.
    /// </summary>
    /// <param name="mainVolume">The value of the main volume slider.</param>
    public void SliderSetMainVolume(float mainVolume)
    {
        audioMixer.SetFloat("MainVolume", Mathf.Log10(mainVolume) * 20f);
        sliderMainValue = mainVolume;
        PlayerPrefs.SetFloat("sliderMainValueKey", sliderMainValue);
    }

    /// <summary>
    /// Sets the background music volume level and saves it to player preferences.
    /// </summary>
    /// <param name="bGMVolume">The value of the background music volume slider.</param>
    public void SliderSetBGMVolume(float bGMVolume)
    {
        audioMixer.SetFloat("BGMVolume", Mathf.Log10(bGMVolume) * 20f);
        sliderBGMValue = bGMVolume;
        PlayerPrefs.SetFloat("sliderBGMValueKey", sliderBGMValue);
    }

    /// <summary>
    /// Sets the UI volume level and saves it to player preferences.
    /// </summary>
    /// <param name="uIVolume">The value of the UI volume slider.</param>
    public void SliderSetUIVolume(float uIVolume)
    {
        audioMixer.SetFloat("UIVolume", Mathf.Log10(uIVolume) * 20f);
        sliderUIValue = uIVolume;
        PlayerPrefs.SetFloat("sliderUIValueKey", sliderUIValue);
    }
}
