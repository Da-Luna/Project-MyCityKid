using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    public float timeToWakeUp = 1f;
    float m_TimeToWakeUp;

    PlayerCharacter m_PlayerCharacter;
    CharacterController3D m_CharacterController3D;
    Animator m_Animator;

    void OnEnable()
    {
        if(m_PlayerCharacter == null)
        {
            m_PlayerCharacter = GetComponentInParent<PlayerCharacter>();
            m_CharacterController3D = GetComponentInParent<CharacterController3D>();
            m_Animator = GetComponentInParent<Animator>();
        }

        m_TimeToWakeUp = timeToWakeUp;
    }
    private void Update()
    {
        m_CharacterController3D.UpdateGroundState();

        if (m_CharacterController3D.IsGrounded)
        {
            m_TimeToWakeUp -= Time.deltaTime;

            if (m_TimeToWakeUp < 0)
            {
                m_Animator.enabled = true;
                m_Animator.SetTrigger("GetUp");

                
                //m_CharacterController3D.IsRagDoll = false;
                //m_CharacterController3D.CharacterControllerActiveState(true);

                enabled = false;
            }
        }
    }
}
