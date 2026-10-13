#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

//*****************************************
//功能说明：Tools 菜单一键生成 PlayerLocomotion.controller
// Idle / Walk / Run（BlendTree by Speed）+ Jump
//*****************************************
public static class PlayerLocomotionControllerCreator
{
    private const string OutputFolder =
        "Assets/Scripts/GameFramework/SpecificCode/PresentationLayerCode/CharacterSystem/Animation";
    private const string OutputPath = OutputFolder + "/PlayerLocomotion.controller";

    // Kevin Iglesias Female clips（工程内 .meta 真实 guid）
    private const string IdleGuid = "0f69fd4f4ad106447896fb775199ed37";
    private const string WalkGuid = "2d87094962c8b48478651fa8fe1f7a5a";
    private const string RunGuid = "40786e67f3ceb094b9c00543f295cb5f";
    private const string JumpGuid = "d0d93c131bda12e4bb9798ea4aefb6cb";
    private const string FallGuid = "73f1e892de373654ea6e3fff3ca15f76";

    [MenuItem("Tools/MMO-demo/Create Player Locomotion Controller")]
    public static void Create()
    {
        if (!Directory.Exists(OutputFolder))
        {
            Directory.CreateDirectory(OutputFolder);
            AssetDatabase.Refresh();
        }

        if (File.Exists(OutputPath))
        {
            AssetDatabase.DeleteAsset(OutputPath);
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(OutputPath);

        controller.AddParameter(PlayerAnimParams.Speed, AnimatorControllerParameterType.Float);
        controller.AddParameter(PlayerAnimParams.MoveValue, AnimatorControllerParameterType.Float);
        controller.AddParameter(PlayerAnimParams.MoveState, AnimatorControllerParameterType.Bool);
        controller.AddParameter(PlayerAnimParams.IsGround, AnimatorControllerParameterType.Bool);
        controller.AddParameter(PlayerAnimParams.Jump, AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine root = controller.layers[0].stateMachine;

        AnimationClip idleClip = LoadClip(IdleGuid);
        AnimationClip walkClip = LoadClip(WalkGuid);
        AnimationClip runClip = LoadClip(RunGuid);
        AnimationClip jumpClip = LoadClip(JumpGuid);
        AnimationClip fallClip = LoadClip(FallGuid);

        AnimatorState locoState = root.AddState("Locomotion", new Vector3(300, 0, 0));
        BlendTree tree = new BlendTree
        {
            name = "LocomotionTree",
            blendType = BlendTreeType.Simple1D,
            blendParameter = PlayerAnimParams.Speed,
            useAutomaticThresholds = false
        };
        AssetDatabase.AddObjectToAsset(tree, controller);

        if (idleClip != null) tree.AddChild(idleClip, 0f);
        if (walkClip != null) tree.AddChild(walkClip, 0.5f);
        if (runClip != null) tree.AddChild(runClip, 1f);
        locoState.motion = tree;
        root.defaultState = locoState;

        AnimatorState jumpState = root.AddState(PlayerAnimParams.JumpStateName, new Vector3(300, 120, 0));
        jumpState.motion = jumpClip != null ? jumpClip : fallClip;

        AnimatorStateTransition toJump = root.AddAnyStateTransition(jumpState);
        toJump.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.Jump);
        toJump.hasExitTime = false;
        toJump.duration = 0.05f;
        toJump.canTransitionToSelf = false;

        AnimatorStateTransition jumpToLoco = jumpState.AddTransition(locoState);
        jumpToLoco.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.IsGround);
        jumpToLoco.hasExitTime = false;
        jumpToLoco.duration = 0.1f;

        AnimatorStateTransition jumpToLocoExit = jumpState.AddTransition(locoState);
        jumpToLocoExit.hasExitTime = true;
        jumpToLocoExit.exitTime = 0.85f;
        jumpToLocoExit.duration = 0.1f;
        jumpToLocoExit.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.IsGround);

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = controller;
        Debug.Log("[PlayerLocomotion] 已生成: " + OutputPath +
                  "\n请把该 Controller 拖到 P09 的 Animator，并给角色挂 PlayerBootstrap。");
    }

    private static AnimationClip LoadClip(string guid)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("找不到动画 guid: " + guid);
            return null;
        }

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (Object asset in assets)
        {
            if (asset is AnimationClip clip && !clip.name.Contains("__preview__"))
            {
                return clip;
            }
        }

        Debug.LogWarning("在 " + path + " 未找到 AnimationClip，请在 Animator 里手动拖动画。");
        return null;
    }
}
#endif
