using System.Collections;
using UnityEngine;

/// <summary>
/// The PlayerSoundManager class handles the audio playback for various player actions such as footstep, 
/// jump, and landing sounds. It interacts with <see cref="SOSurfaceSounds"/> and <see cref="SOGenderSounds"/>
/// to play the appropriate sound depending on the player's current surface and selected gender.
/// </summary>
public class PlayerSoundManager : MonoBehaviour
{
    public enum GenderType
    {
        MALE,
        FEMALE
    }

    [Header("REFERENCES")]

    [SerializeField, Tooltip("The AudioSource used for playing sounds related to the player's gender, such as jump and land sounds.")]
    AudioSource playerGenderAudioSource;

    [SerializeField, Tooltip("The AudioSource used for playing general sounds for the player, such as footsteps.")]
    AudioSource playerSoundAudioSource;

    [Header("GENDER SOUND ASSETS")]

    [SerializeField, Tooltip("ScriptableObject that contains the sounds for characters based on gender.")]
    SOGenderSounds genderSounds;

    [Header("SURFACE SOUND SETTINGS")]

    [SerializeField, Tooltip("The delay between each footstep sound to prevent them from playing too quickly in succession, especially during animation transitions.")]
    float footstepSoundDelay = 0.125f;

    [Header("SURFACE SOUND ASSETS")]

    [SerializeField, Tooltip("ScriptableObject that contains the sounds for walking on grass surfaces.")]
    SOSurfaceSounds grassSounds;

    [SerializeField, Tooltip("ScriptableObject that contains the sounds for walking on gravel surfaces.")]
    SOSurfaceSounds gravelSounds;

    [SerializeField, Tooltip("ScriptableObject that contains the sounds for walking on leaves surfaces.")]
    SOSurfaceSounds leavesSounds;

    [SerializeField, Tooltip("ScriptableObject that contains the sounds for walking on metal surfaces.")]
    SOSurfaceSounds metalSounds;

    [SerializeField, Tooltip("ScriptableObject that contains the sounds for walking on rock surfaces.")]
    SOSurfaceSounds rockSounds;

    [SerializeField, Tooltip("ScriptableObject that contains the sounds for walking on sand surfaces.")]
    SOSurfaceSounds sandSounds;

    [SerializeField, Tooltip("ScriptableObject that contains the sounds for walking on water surfaces.")]
    SOSurfaceSounds waterSounds;

    [SerializeField, Tooltip("ScriptableObject that contains the sounds for walking on wood surfaces.")]
    SOSurfaceSounds woodSounds;

    private bool m_DelaySound; // Indicates if footstep sound is delayed
    private WaitForSeconds m_FootstepSoundDelay; // Wait duration for footstep sound delay
    private SOSurfaceSounds.SurfaceType currentSurfaceType; // Tracks the current surface type


    void Start()
    {
#if UNITY_EDITOR
        // Check if any sound list is null or empty in the ScriptableObject instances
        SOSurfaceSounds[] surfaces =
        {
            grassSounds,
            gravelSounds,
            leavesSounds,
            metalSounds,
            rockSounds,
            sandSounds,
            waterSounds,
            woodSounds
        };

        foreach (var surface in surfaces)
        {
            if (surface != null)
            {
                surface.CheckAudioClips();
            }
        }
        Debug.LogError("PlayerSoundManager - Slope-Slide-Sound ist provisorisch (läuft aktuell im UPDATE und braucht eine Referenz zum Skript CharacterController3D.cs)");
#endif

        m_FootstepSoundDelay = new WaitForSeconds(footstepSoundDelay);
    }

    #region TEST FUNCTION FOR SLOPE-SLIDE SOUND
    [Space]
    [Space]
    [Header("TEST FUNCTION FOR SLOPE-SLIDE SOUND")]
    [SerializeField, Tooltip("Reference to the CharacterController3D component that handles player movement.")]
    CharacterController3D m_CharacterController3D;
    [SerializeField, Tooltip("Sound clip played when the player is sliding down a slope.")]
    AudioClip slopeSlideSound;
    bool isSlide;
    void Update()
    {
        if (m_CharacterController3D.IsSliding && !isSlide)
        {
            playerSoundAudioSource.clip = slopeSlideSound;

            playerSoundAudioSource.Play();

            isSlide = true;
        }
        else if (!m_CharacterController3D.IsSliding && isSlide)
        {
            playerSoundAudioSource.Stop();

            isSlide = false;
        }
    }
    #endregion // TEST FUNCTION FOR SLOPE-SLIDE SOUND

    #region CALL VIA ANIMATION EVENTS

    /// <summary>
    /// Plays a soft footstep sound based on the player's current ground type. This is called via animation events.
    /// </summary>
    public void PlayFootstepSoftAudio()
    {
        if (m_DelaySound)
        {
            return;
        }

        UpdateSurfaceType();
        PlaySoundBasedOnSurfaceType(SOSurfaceSounds.SurfaceSoundType.FOOTSTEP_SOFT);

        StartCoroutine(DelayFootstepSound());
    }

    /// <summary>
    /// Plays a hard footstep sound based on the player's current ground type. This is called via animation events.
    /// </summary>
    public void PlayFootstepHardAudio()
    {
        if (m_DelaySound)
        {
            return;
        }

        UpdateSurfaceType();
        PlaySoundBasedOnSurfaceType(SOSurfaceSounds.SurfaceSoundType.FOOTSTEP_HARD);

        StartCoroutine(DelayFootstepSound());
    }

    /// <summary>
    /// Plays a jump sound based on the player's current ground type. This is called via animation events.
    /// </summary>
    public void PlayJumpAudio()
    {
        UpdateSurfaceType();
        PlaySoundBasedOnSurfaceType(SOSurfaceSounds.SurfaceSoundType.JUMP);
        PlayGenderSound(SOGenderSounds.GenderSoundType.JUMP);
    }

    /// <summary>
    /// Plays a landing sound based on the player's current ground type. This is called via animation events.
    /// </summary>
    public void PlayLandAudio()
    {
        UpdateSurfaceType();
        PlaySoundBasedOnSurfaceType(SOSurfaceSounds.SurfaceSoundType.LAND);
        PlayGenderSound(SOGenderSounds.GenderSoundType.LAND);
    }

    /// <summary>
    /// Plays a jump voice sound based on the player's current 'genderSounds'. This method is called via animation events.
    /// </summary>
    public void PlayGenderJumpAudio()
    {
        PlayGenderSound(SOGenderSounds.GenderSoundType.JUMP);
    }

    #endregion // CALL VIA ANIMATION EVENTS

    /// <summary>
    /// Plays a sound based on the selected gender type.
    /// </summary>
    /// <param name="soundType">The type of sound to play (e.g., jump, land).</param>
    void PlayGenderSound(SOGenderSounds.GenderSoundType soundType)
    {
        if (genderSounds != null)
        {
            genderSounds.PlaySound(playerSoundAudioSource, soundType);
        }
    }

    void UpdateSurfaceType()
    {
        if (PlayerCharacter.PlayerInstance != null)
        {
            currentSurfaceType = (SOSurfaceSounds.SurfaceType)PlayerCharacter.PlayerInstance.GetCurrentsurfaceType();
        }
    }

    /// <summary>
    /// Plays the appropriate sound (footstep, jump, or land) based on the current ground type.
    /// </summary>
    /// <param name="soundType">The type of sound to play.</param>
    void PlaySoundBasedOnSurfaceType(SOSurfaceSounds.SurfaceSoundType soundType)
    {
        SOSurfaceSounds surfaceSounds = GetSurfaceSoundsForSurfaceType(currentSurfaceType);

        if (surfaceSounds != null)
        {
            surfaceSounds.PlaySound(playerSoundAudioSource, soundType);
        }
    }

    /// <summary>
    /// Returns the appropriate SOSurfaceSounds instance based on the current ground type.
    /// </summary>
    /// <param name="surfaceType">The current ground type.</param>
    /// <returns>Returns the SOSurfaceSounds for the given ground type.</returns>
    SOSurfaceSounds GetSurfaceSoundsForSurfaceType(SOSurfaceSounds.SurfaceType surfaceType)
    {
        return surfaceType switch
        {
            SOSurfaceSounds.SurfaceType.GRASS => grassSounds,
            SOSurfaceSounds.SurfaceType.GRAVEL => gravelSounds,
            SOSurfaceSounds.SurfaceType.LEAVES => leavesSounds,
            SOSurfaceSounds.SurfaceType.METAL => metalSounds,
            SOSurfaceSounds.SurfaceType.ROCK => rockSounds,
            SOSurfaceSounds.SurfaceType.SAND => sandSounds,
            SOSurfaceSounds.SurfaceType.WATER => waterSounds,
            SOSurfaceSounds.SurfaceType.WOOD => woodSounds,
            _ => null,
        };
    }

    IEnumerator DelayFootstepSound()
    {
        m_DelaySound = true;
        
        yield return m_FootstepSoundDelay;
        
        m_DelaySound = false;
    }
}
