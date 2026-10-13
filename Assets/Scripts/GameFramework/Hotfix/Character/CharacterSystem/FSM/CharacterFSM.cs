using System.Collections.Generic;
using UnityEngine;
//*****************************************
//功能说明：人物有限状态机，驱动动画状态
//*****************************************
public class CharacterFSM : MonoBehaviour
{
    private Dictionary<CHARACTERSTATE, BaseState> statesDict;
    private BaseState currentState;
    private BaseState lastState;
    private InputController ic;
    public CHARACTERSTATE debugState;

    public void InitFSM(Animator currentAnimator, InputController inputController)
    {
        statesDict = new Dictionary<CHARACTERSTATE, BaseState>()
        {
            { CHARACTERSTATE.IDLE, new IdleState(this, currentAnimator, CHARACTERSTATE.IDLE) },
            { CHARACTERSTATE.MOVE, new MoveState(this, currentAnimator, CHARACTERSTATE.MOVE) },
            { CHARACTERSTATE.JUMP, new JumpState(this, currentAnimator, CHARACTERSTATE.JUMP) },
        };
        ic = inputController;
        SetDefaultState();
    }

    private void SetDefaultState()
    {
        foreach (var item in statesDict)
        {
            item.Value.InitState();
        }

        currentState = statesDict[CHARACTERSTATE.IDLE];
        currentState.EnterState();
    }

    public void ChangeState(CHARACTERSTATE newStateType)
    {
        if (statesDict == null || currentState == null)
        {
            return;
        }

        if (!statesDict.ContainsKey(newStateType))
        {
            return;
        }

        BaseState changeState = statesDict[newStateType];
        if (changeState == currentState)
        {
            return;
        }

        currentState.ExitState();
        lastState = currentState;
        currentState = changeState;
        currentState.EnterState();
    }

    public CHARACTERSTATE GetCurrentState()
    {
        return currentState != null ? currentState.stateType : CHARACTERSTATE.IDLE;
    }

    public CHARACTERSTATE GetLastState()
    {
        return lastState != null ? lastState.stateType : CHARACTERSTATE.IDLE;
    }

    void Update()
    {
        if (currentState == null)
        {
            return;
        }

        debugState = currentState.stateType;
        currentState.UpdateState();
    }

    public void SetInputValue(string inputCode, bool inputValue)
    {
        ic.SetInputValue(inputCode, inputValue);
    }

    public bool GetBoolInputValue(string inputCode)
    {
        return ic.GetBoolInputValue(inputCode);
    }

    public void SetInputValue(string inputCode, float inputValue)
    {
        ic.SetInputValue(inputCode, inputValue);
    }

    public float GetFloatInputValue(string inputCode)
    {
        return ic.GetFloatInputValue(inputCode);
    }
}

/// <summary>
/// Animator 参数名，和 PlayerLocomotion.controller 保持一致
/// </summary>
public static class PlayerAnimParams
{
    public const string MoveState = "MoveState";
    public const string MoveValue = "MoveValue";
    public const string Speed = "Speed";
    public const string IsGround = "IsGround";
    public const string Jump = "Jump";
    public const string JumpStateName = "Jump";
    public const string Wave = "Wave";
    public const string WavingStateName = "Waving";
}
