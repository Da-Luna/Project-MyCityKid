using UnityEngine;

[CreateAssetMenu(fileName = "GenderSounds", menuName = "TLSDigital/Audio/GenderSounds", order = 0)]
/// <summary>
/// The SOSurfaceSounds class defines different sound effects for various types of ground surfaces. 
/// It holds arrays of AudioClips for different sound actions like footstep, jump, and land. 
/// This class is primarily used by the <see cref="PlayerSoundManager"/> class to play appropriate sounds 
/// based on the player's current ground type and action. It allows easy customization of sounds 
/// through the Unity Editor by defining sound sets for each surface type.
/// </summary>
public class SOGenderSounds : ScriptableObject
{
    public enum GenderType
    {
        MALE,
        FEMALE
    }

    public enum GenderSoundType
    {
        JUMP,
        LAND
    }

    [Tooltip("The current type of ground the player is walking on. This is used to select the appropriate sound effects.")]
    public GenderType selectedGenderType;

    [Tooltip("An array of footstep sound effects that correspond to this ground type.")]
    public AudioClip[] genderJumpSounds;

    [Tooltip("An array of footstep sound effects that correspond to this ground type.")]
    public AudioClip[] genderLandSounds;

    /// <summary>
    /// Plays the appropriate sound based on the given sound type (footstep, jump, or land).
    /// A random sound from the corresponding array is played.
    /// </summary>
    /// <param name="source">The AudioSource that plays the sound.</param>
    /// <param name="type">The type of sound to play (footstep, jump, land).</param>
    public void PlaySound(AudioSource source, GenderSoundType type)
    {
        AudioClip[] clips = type switch
        {
            GenderSoundType.JUMP => genderJumpSounds,
            GenderSoundType.LAND => genderLandSounds,
            _ => null
        };

        if (clips != null && clips.Length > 0)
        {
            var clip = clips[Random.Range(0, clips.Length)];
            source.PlayOneShot(clip);
        }
    }

#if UNITY_EDITOR
    public void CheckAudioClips()
    {
        if (genderJumpSounds == null || genderJumpSounds.Length == 0)
            Debug.LogWarning("Warning: Footstep sounds are missing.");

        if (genderLandSounds == null || genderLandSounds.Length == 0)
            Debug.LogWarning("Warning: Footstep sounds are missing.");
    }
#endif
}
