using UnityEngine;
//*****************************************
//功能说明：闲置状态
// Speed 由 PlayerMovementController 平滑驱动，这里不再硬切 0
//*****************************************
public class IdleState : BaseState
{
    public IdleState(CharacterFSM characterFSM, Animator animator, CHARACTERSTATE cs)
        : base(characterFSM, animator, cs)
    {
    }

    public override void InitState() { }

    public override void EnterState()
    {
        animator.SetBool(PlayerAnimParams.MoveState, false);
    }

    public override void ExitState() { }

    public override void UpdateState() { }
}
