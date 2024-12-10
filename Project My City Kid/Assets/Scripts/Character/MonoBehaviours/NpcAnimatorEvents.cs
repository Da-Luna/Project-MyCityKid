using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcAnimatorEvents : MonoBehaviour
{
    [Header("REFERENCES")]

    [SerializeField, Tooltip("AAA")]
    AudioSource m_AudioSource;

    [Header("AUDIO CLIPS")]

    [SerializeField, Tooltip("AAA")]
    AudioClip landAudioClip;

    Animator m_Animator;

    void OnEnable()
    {
        if (m_Animator == null)
            m_Animator = GetComponent<Animator>();
    }

    public void LandAudio()
    {
        SetAudioClip(landAudioClip);

        if (!m_AudioSource.isPlaying)
            m_AudioSource.Play();
    }

    void SetAudioClip(AudioClip audioClip)
    {
        m_AudioSource.clip = audioClip;
    }
}
