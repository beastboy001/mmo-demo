using UnityEngine;
//*****************************************
//功能说明：移动状态。Speed 由 PlayerMovementController 驱动
//*****************************************
public class MoveState : BaseState
{
    public MoveState(CharacterFSM characterFSM, Animator animator, CHARACTERSTATE cs)
        : base(characterFSM, animator, cs)
    {
    }

    public override void InitState() { }

    public override void EnterState()
    {
        animator.SetBool(PlayerAnimParams.MoveState, true);
    }

    public override void ExitState()
    {
        animator.SetBool(PlayerAnimParams.MoveState, false);
    }

    public override void UpdateState() { }
}
