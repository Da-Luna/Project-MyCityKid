using UnityEngine;

public class PlayerBaseMovementSMB : SceneLinkedSMB<PlayerCharacter>
{
    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_MonoBehaviour.GroundStatsCheck();
    }

    public override void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_MonoBehaviour.GroundStatsCheck();
    }

    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_MonoBehaviour.GroundStatsCheck();
        m_MonoBehaviour.GroundedVerticalMovement();

        if (!m_MonoBehaviour.InputCheckForJump())
                m_MonoBehaviour.GravityApply();

        m_MonoBehaviour.Jump();

        m_MonoBehaviour.AimingCamera();
    }
}
