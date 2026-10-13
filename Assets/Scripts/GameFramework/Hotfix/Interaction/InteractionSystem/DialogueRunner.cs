using System;
using UnityEngine;

//*****************************************
//功能说明：对话逻辑（纯 C#，不继承 MonoBehaviour）
//
// 只管：当前说到哪一句、怎么下一句、怎么选分支、怎么结束
// 不管：UI 怎么画 —— 通过 OnNodeChanged / OnEnded 通知 GamePanel
//
// 调用关系：
//   GamePanel.StartDialogue → Start
//   点 ClickArea           → TryAdvance
//   点 DialogOption        → Choose
//   结束时                  → End → OnEnded → GamePanel.CloseDialogue
//*****************************************
public class DialogueRunner
{
    private DialogueAsset asset;   // 当前这段对话配置
    private DialogueNode current;  // 当前正在显示的节点

    public bool IsRunning { get; private set; }
    public DialogueNode Current => current;

    /// <summary>当前句有没有分支选项（有的话 ClickArea 点了也不往下走）</summary>
    public bool HasChoices =>
        current != null && current.choices != null && current.choices.Count > 0;

    public string NpcName { get; private set; }
    public string NpcProfession { get; private set; }

    /// <summary>换了一句台词 → GamePanel 刷新 DialogBox 内容</summary>
    public event Action OnNodeChanged;

    /// <summary>对话彻底结束 → GamePanel 关面板、解锁操作</summary>
    public event Action OnEnded;

    /// <summary>开始一段对话，从 asset.startNodeId 那句开场</summary>
    public void Start(DialogueAsset dialogue, string npcName, string npcProfession)
    {
        if (dialogue == null)
        {
            Debug.LogWarning("[DialogueRunner] DialogueAsset 为空");
            return;
        }

        asset = dialogue;
        NpcName = npcName;
        NpcProfession = npcProfession;
        IsRunning = true;
        ShowNode(asset.GetNode(asset.startNodeId));
    }

    /// <summary>
    /// 点 ClickArea 推进：
    ///   · 有分支选项 → 忽略（必须点选项）
    ///   · 有 nextNodeId → 跳下一句
    ///   · 都没有 → 结束对话
    /// </summary>
    public void TryAdvance()
    {
        if (!IsRunning || current == null)
        {
            return;
        }

        if (HasChoices)
        {
            return;
        }

        if (!string.IsNullOrEmpty(current.nextNodeId))
        {
            ShowNode(asset.GetNode(current.nextNodeId));
            return;
        }

        End();
    }

    /// <summary>点了第 index 个分支（从 0 开始）</summary>
    public void Choose(int index)
    {
        if (!IsRunning || current == null || current.choices == null)
        {
            return;
        }

        if (index < 0 || index >= current.choices.Count)
        {
            return;
        }

        DialogueChoice choice = current.choices[index];
        if (choice == null)
        {
            End();
            return;
        }

        // 有跳转目标就去那句，否则直接结束
        if (!string.IsNullOrEmpty(choice.nextNodeId))
        {
            ShowNode(asset.GetNode(choice.nextNodeId));
            return;
        }

        End();
    }

    /// <summary>切换到某一句；节点无效则当作结束</summary>
    private void ShowNode(DialogueNode node)
    {
        if (node == null)
        {
            End();
            return;
        }

        current = node;
        OnNodeChanged?.Invoke();
    }

    /// <summary>结束对话并清空状态</summary>
    public void End()
    {
        if (!IsRunning)
        {
            return;
        }

        IsRunning = false;
        current = null;
        asset = null;
        OnEnded?.Invoke();
    }
}
