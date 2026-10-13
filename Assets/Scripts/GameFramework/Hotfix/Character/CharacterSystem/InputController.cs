using System.Collections.Generic;
using UnityEngine;
//*****************************************
//功能说明：输入控制。本地读键盘；远程玩家可由同步层写入字典。
//*****************************************
public class InputController : MonoBehaviour
{
    private Dictionary<string, bool> inputBoolValueDict;
    private Dictionary<string, float> inputFloatValueDict;
    public bool isSyncCharacter;

    void Awake()
    {
        inputBoolValueDict = new Dictionary<string, bool>()
        {
            { InputCode.JumpState, false },
            { InputCode.RunState, false },
        };
        inputFloatValueDict = new Dictionary<string, float>()
        {
            { InputCode.HorizontalMoveValue, 0 },
            { InputCode.VerticalMoveValue, 0 },
        };
    }

    void Update()
    {
        if (isSyncCharacter)
        {
            return;
        }

        SetInputValue(InputCode.HorizontalMoveValue, Input.GetAxisRaw("Horizontal"));
        SetInputValue(InputCode.VerticalMoveValue, Input.GetAxisRaw("Vertical"));
        SetInputValue(InputCode.JumpState, Input.GetButtonDown("Jump"));
        // Unity 默认 Fire3 = Left Shift
        SetInputValue(InputCode.RunState, Input.GetButton("Fire3") || Input.GetKey(KeyCode.LeftShift));
    }

    public void SetInputValue(string inputCode, bool inputValue)
    {
        if (inputBoolValueDict.ContainsKey(inputCode))
        {
            inputBoolValueDict[inputCode] = inputValue;
        }
        else
        {
            Debug.Log("设置输入码错误，错误码为：" + inputCode);
        }
    }

    public bool GetBoolInputValue(string inputCode)
    {
        if (inputBoolValueDict.ContainsKey(inputCode))
        {
            return inputBoolValueDict[inputCode];
        }

        Debug.Log("获取输入码错误，错误码为：" + inputCode);
        return false;
    }

    public void SetInputValue(string inputCode, float inputValue)
    {
        if (inputFloatValueDict.ContainsKey(inputCode))
        {
            inputFloatValueDict[inputCode] = inputValue;
        }
        else
        {
            Debug.Log("设置输入码错误，错误码为：" + inputCode);
        }
    }

    public float GetFloatInputValue(string inputCode)
    {
        if (inputFloatValueDict.ContainsKey(inputCode))
        {
            return inputFloatValueDict[inputCode];
        }

        Debug.Log("获取输入码错误，错误码为：" + inputCode);
        return 0;
    }
}

public static class InputCode
{
    public const string HorizontalMoveValue = "HorizontalMoveValue";
    public const string VerticalMoveValue = "VerticalMoveValue";
    public const string JumpState = "JumpState";
    public const string RunState = "RunState";
}
