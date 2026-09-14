using UnityEngine;

//*****************************************
//功能说明：可对话 NPC（挂到场景里的 NPC 物体上）
//
// 使用步骤：
//   1. NPC 加 Collider，勾 Is Trigger（半径调大一点方便走近）
//   2. 挂本脚本
//   3. 填名字 / 职业，拖 DialogueAsset
//
// 流程：
//   玩家走进 Trigger → 向 InteractionManager 登记「对话」选项
//   玩家按确认键     → StartTalk → GamePanel.StartDialogue
//   玩家走出 Trigger → 从列表里删掉本 NPC 的选项
//*****************************************
[RequireComponent(typeof(Collider))]
public class NpcInteractable : MonoBehaviour
{
    [Header("显示信息（会显示在 DialogBox 上）")]
    public string npcName = "村民";
    public string npcProfession = "普通居民";

    [Header("对话配置（拖 Dialogue Asset）")]
    public DialogueAsset dialogue;

    [Header("左侧交互条显示")]
    [Tooltip("InteractionContent 上的文字，例如「对话」")]
    public string actionDisplayName = "对话";

    [Tooltip("按键提示显示文字，例如 F / E")]
    public string keyTip = "F";

    [Tooltip("真正要按的键，要和 keyTip 对应")]
    public KeyCode key = KeyCode.F;

    // 当前登记出去的那条选项（方便理解；注销时其实按 owner=this 删）
    private InteractionOptionData registeredOption;

    // 防止 Trigger 抖动导致重复登记
    private bool playerInside;

    /// <summary>在 Inspector 里 Add Component 时自动把 Collider 设成 Trigger</summary>
    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 只认带 PlayerMovementController 的物体（本地玩家），不依赖 Tag
        if (!IsPlayer(other) || playerInside)
        {
            return;
        }

        playerInside = true;
        RegisterTalkOption();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other) || !playerInside)
        {
            return;
        }

        playerInside = false;
        UnregisterTalkOption();
    }

    private void OnDisable()
    {
        // NPC 被禁用 / 销毁时也要清掉列表，避免残留幽灵选项
        playerInside = false;
        UnregisterTalkOption();
    }

    /// <summary>是否为本地玩家（CharacterController 可能在子物体上，所以用 GetComponentInParent）</summary>
    private static bool IsPlayer(Collider other)
    {
        return other != null && other.GetComponentInParent<PlayerMovementController>() != null;
    }

    /// <summary>往 InteractionManager 登记一条「对话」</summary>
    private void RegisterTalkOption()
    {
        InteractionManager mgr = InteractionManager.EnsureExists();
        UnregisterTalkOption(); // 先清一次，避免重复

        registeredOption = new InteractionOptionData
        {
            displayName = actionDisplayName,
            keyTip = keyTip,
            key = key,
            owner = this,
            onExecute = StartTalk // 按键确认后走这里
        };
        mgr.Register(registeredOption);
    }

    /// <summary>从列表移除本 NPC 相关选项</summary>
    private void UnregisterTalkOption()
    {
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.UnregisterOwner(this);
        }

        registeredOption = null;
    }

    /// <summary>确认交互：打开 GamePanel 对话</summary>
    private void StartTalk()
    {
        if (dialogue == null)
        {
            Debug.LogWarning("[NpcInteractable] 还没拖 DialogueAsset：" + name);
            return;
        }

        // true = 包含未激活的面板（GamePanel 可能刚创建）
        GamePanel panel = Object.FindObjectOfType<GamePanel>(true);
        if (panel == null)
        {
            Debug.LogWarning("[NpcInteractable] 找不到 GamePanel");
            return;
        }

        panel.StartDialogue(dialogue, npcName, npcProfession);
    }
}
