using UnityEngine;

//*****************************************
//功能说明：本地玩家播放招手（Waving）动画
// 优先 SetTrigger("Wave")；没有参数则 CrossFade("Waving")
//*****************************************
public static class PlayerWavePlayer
{
    public static void PlayLocalWave()
    {
        PlayerMovementController move = Object.FindObjectOfType<PlayerMovementController>();
        if (move == null)
        {
            Debug.LogWarning("[PlayerWavePlayer] 找不到本地玩家");
            return;
        }

        Animator animator = move.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("[PlayerWavePlayer] 本地玩家没有 Animator");
            return;
        }

        Play(animator);
    }

    public static void Play(Animator animator)
    {
        if (animator == null)
        {
            return;
        }

        if (HasTrigger(animator, PlayerAnimParams.Wave))
        {
            animator.ResetTrigger(PlayerAnimParams.Wave);
            animator.SetTrigger(PlayerAnimParams.Wave);
            return;
        }

        int hash = Animator.StringToHash(PlayerAnimParams.WavingStateName);
        if (animator.HasState(0, hash))
        {
            animator.CrossFade(PlayerAnimParams.WavingStateName, 0.1f, 0);
            return;
        }

        Debug.LogWarning(
            "[PlayerWavePlayer] Animator 里没有 Wave 触发器 / Waving 状态。" +
            "请在菜单执行 Tools/MMO-demo/Add Waving To Player Locomotion");
    }

    private static bool HasTrigger(Animator animator, string paramName)
    {
        AnimatorControllerParameter[] ps = animator.parameters;
        for (int i = 0; i < ps.Length; i++)
        {
            if (ps[i].type == AnimatorControllerParameterType.Trigger && ps[i].name == paramName)
            {
                return true;
            }
        }

        return false;
    }
}
