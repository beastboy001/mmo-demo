using UnityEngine;

//*****************************************
//功能说明：对话时锁玩家操作（静态工具类）
//
// 锁两样东西：
//   1. PlayerMovementController.controlLocked → 不能走
//   2. CinemachineFreeLook.enabled = false  → 鼠标移了镜头也不转
//
// 由 GamePanel 进入/退出对话时调用 SetLocked(true/false)
//*****************************************
public static class GameplayControlLock
{
    private static bool locked;

    // 缓存找到的 FreeLook，避免每次开关都全场景搜
    private static Behaviour freeLook;

    public static bool IsLocked => locked;

    /// <summary>true = 锁住移动和镜头；false = 恢复</summary>
    public static void SetLocked(bool value)
    {
        locked = value;

        // 锁 / 解锁移动
        PlayerMovementController move = Object.FindObjectOfType<PlayerMovementController>();
        if (move != null)
        {
            move.controlLocked = value;
        }

        // 锁 / 解锁镜头
        if (value)
        {
            CacheAndDisableFreeLook();
        }
        else
        {
            RestoreFreeLook();
        }
    }

    /// <summary>用类型名找 FreeLook（避免强依赖 Cinemachine 程序集引用）</summary>
    private static void CacheAndDisableFreeLook()
    {
        if (freeLook == null)
        {
            Behaviour[] behaviours = Object.FindObjectsOfType<Behaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                Behaviour b = behaviours[i];
                if (b != null && b.GetType().Name == "CinemachineFreeLook")
                {
                    freeLook = b;
                    break;
                }
            }
        }

        if (freeLook != null)
        {
            freeLook.enabled = false;
        }
    }

    private static void RestoreFreeLook()
    {
        if (freeLook != null)
        {
            freeLook.enabled = true;
        }
    }
}
