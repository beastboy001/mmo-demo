using UnityEngine;
//*****************************************
//功能说明：状态基类
//*****************************************
public abstract class BaseState
{
    protected CharacterFSM cfsm;
    protected Animator animator;
    public CHARACTERSTATE stateType;

    public BaseState(CharacterFSM characterFSM, Animator animator, CHARACTERSTATE cs)
    {
        cfsm = characterFSM;
        this.animator = animator;
        stateType = cs;
    }

    public abstract void InitState();
    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void UpdateState();
}

public enum CHARACTERSTATE
{
    IDLE,
    MOVE,
    JUMP,
    ATTACK,
    HIT,
    DEAD
}
