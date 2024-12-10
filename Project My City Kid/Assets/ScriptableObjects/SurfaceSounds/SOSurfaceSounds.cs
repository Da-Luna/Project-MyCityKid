using UnityEngine;

[CreateAssetMenu(fileName = "SurfaceSounds", menuName = "TLSDigital/Audio/SurfaceSounds", order = 1)]
/// <summary>
/// The SOSurfaceSounds class defines different sound effects for various types of ground surfaces. 
/// It holds arrays of AudioClips for different sound actions like footstep, jump, and land. 
/// This class is primarily used by the <see cref="PlayerSoundManager"/> class to play appropriate sounds 
/// based on the player's current ground type and action. It allows easy customization of sounds 
/// through the Unity Editor by defining sound sets for each surface type.
/// </summary>
public class SOSurfaceSounds : ScriptableObject
{
    public enum SurfaceType
    {
        GRASS,
        GRAVEL,
        LEAVES,
        METAL,
        ROCK,
        SAND,
        WATER,
        WOOD
    }

    public enum SurfaceSoundType
    {
        FOOTSTEP_SOFT,
        FOOTSTEP_HARD,
        JUMP,
        LAND
    }

    [Tooltip("The current type of ground the player is walking on. This is used to select the appropriate sound effects.")]
    public SurfaceType currentSurfaceType;

    [Tooltip("An array of footstep sound effects that correspond to this ground type.")]
    public AudioClip[] footstepSoftSounds;

    [Tooltip("An array of footstep sound effects that correspond to this ground type.")]
    public AudioClip[] footstepHardSounds;

    [Tooltip("An array of jump sound effects that correspond to this ground type.")]
    public AudioClip[] jumpSounds;

    [Tooltip("An array of landing sound effects that correspond to this ground type.")]
    public AudioClip[] landSounds;

    /// <summary>
    /// Plays the appropriate sound based on the given sound type (footstep, jump, or land).
    /// A random sound from the corresponding array is played.
    /// </summary>
    /// <param name="source">The AudioSource that plays the sound.</param>
    /// <param name="type">The type of sound to play (footstep, jump, land).</param>
    public void PlaySound(AudioSource source, SurfaceSoundType type)
    {
        AudioClip[] clips = type switch
        {
            SurfaceSoundType.FOOTSTEP_SOFT => footstepSoftSounds,
            SurfaceSoundType.FOOTSTEP_HARD => footstepHardSounds,
            SurfaceSoundType.JUMP => jumpSounds,
            SurfaceSoundType.LAND => landSounds,
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
        if (footstepSoftSounds == null || footstepSoftSounds.Length == 0)
            Debug.LogWarning("Warning: Footstep sounds are missing.");

        if (footstepHardSounds == null || footstepHardSounds.Length == 0)
            Debug.LogWarning("Warning: Footstep sounds are missing.");

        if (jumpSounds == null || jumpSounds.Length == 0)
            Debug.LogWarning("Warning: Jump sounds are missing.");

        if (landSounds == null || landSounds.Length == 0)
            Debug.LogWarning("Warning: Land sounds are missing.");
    }
#endif
}
