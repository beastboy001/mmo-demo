#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

//*****************************************
//功能说明：给现有 PlayerLocomotion 加上 Waving（招手）状态
// 菜单：Tools/MMO-demo/Add Waving To Player Locomotion
//*****************************************
public static class PlayerWavingAnimatorPatcher
{
    private const string ControllerPath = "Assets/Resources/Animation/PlayerLocomotion.controller";
    private const string WavingGuid = "ab729bcef97d545458f93b430f84b478";

    [MenuItem("Tools/MMO-demo/Add Waving To Player Locomotion")]
    public static void Patch()
    {
        AnimatorController controller =
            AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            Debug.LogError("[Waving] 找不到 Controller: " + ControllerPath);
            return;
        }

        bool hasWave = false;
        for (int i = 0; i < controller.parameters.Length; i++)
        {
            if (controller.parameters[i].name == PlayerAnimParams.Wave)
            {
                hasWave = true;
                break;
            }
        }

        if (!hasWave)
        {
            controller.AddParameter(PlayerAnimParams.Wave, AnimatorControllerParameterType.Trigger);
        }

        AnimatorStateMachine root = controller.layers[0].stateMachine;
        AnimatorState wavingState = FindState(root, PlayerAnimParams.WavingStateName);
        AnimatorState locoState = FindState(root, "Locomotion");

        AnimationClip clip = LoadClip(WavingGuid);
        if (wavingState == null)
        {
            wavingState = root.AddState(PlayerAnimParams.WavingStateName, new Vector3(360, 220, 0));
        }

        if (clip != null)
        {
            wavingState.motion = clip;
        }
        else
        {
            Debug.LogWarning("[Waving] 未加载到 Waving 动画剪辑，请手动拖到 Waving 状态");
        }

        // Any State → Waving（Wave Trigger）
        if (!HasAnyTransitionTo(root, wavingState, PlayerAnimParams.Wave))
        {
            AnimatorStateTransition toWave = root.AddAnyStateTransition(wavingState);
            toWave.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.Wave);
            toWave.hasExitTime = false;
            toWave.duration = 0.08f;
            toWave.canTransitionToSelf = false;
        }

        // Waving → Locomotion（播完）
        if (locoState != null && wavingState.transitions.Length == 0)
        {
            AnimatorStateTransition back = wavingState.AddTransition(locoState);
            back.hasExitTime = true;
            back.exitTime = 0.9f;
            back.duration = 0.15f;
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("[Waving] 已写入 Wave 触发器与 Waving 状态到 " + ControllerPath);
    }

    private static AnimatorState FindState(AnimatorStateMachine root, string stateName)
    {
        ChildAnimatorState[] children = root.states;
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].state != null && children[i].state.name == stateName)
            {
                return children[i].state;
            }
        }

        return null;
    }

    private static bool HasAnyTransitionTo(AnimatorStateMachine root, AnimatorState dst, string trigger)
    {
        AnimatorStateTransition[] any = root.anyStateTransitions;
        for (int i = 0; i < any.Length; i++)
        {
            if (any[i].destinationState != dst)
            {
                continue;
            }

            AnimatorCondition[] conds = any[i].conditions;
            for (int c = 0; c < conds.Length; c++)
            {
                if (conds[c].parameter == trigger)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static AnimationClip LoadClip(string guid)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        for (int i = 0; i < assets.Length; i++)
        {
            AnimationClip clip = assets[i] as AnimationClip;
            if (clip != null && !clip.name.Contains("__preview__"))
            {
                return clip;
            }
        }

        return null;
    }
}
#endif
