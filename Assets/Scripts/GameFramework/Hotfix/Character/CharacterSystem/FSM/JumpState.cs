using UnityEngine;
//*****************************************
//功能说明：跳跃状态
//*****************************************
public class JumpState : BaseState
{
    public JumpState(CharacterFSM characterFSM, Animator animator, CHARACTERSTATE cs)
        : base(characterFSM, animator, cs)
    {
    }

    public override void InitState() { }

    public override void EnterState()
    {
        animator.SetBool(PlayerAnimParams.IsGround, false);
        animator.ResetTrigger(PlayerAnimParams.Jump);
        animator.SetTrigger(PlayerAnimParams.Jump);
        animator.CrossFade(PlayerAnimParams.JumpStateName, 0.08f);
    }

    public override void ExitState()
    {
        animator.SetBool(PlayerAnimParams.IsGround, true);
        animator.ResetTrigger(PlayerAnimParams.Jump);
    }

    public override void UpdateState() { }
}
