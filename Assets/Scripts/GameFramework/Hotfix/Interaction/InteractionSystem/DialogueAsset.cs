using System;
using System.Collections.Generic;
using UnityEngine;

//*****************************************
//功能说明：一段对话的配置资源（ScriptableObject）
// 用法：Project 右键 → Create → MMORPG → Dialogue Asset
// 再用 NpcInteractable.dialogue 拖上去
//*****************************************
[CreateAssetMenu(fileName = "NewDialogue", menuName = "MMORPG/Dialogue Asset")]
public class DialogueAsset : ScriptableObject
{
    [Tooltip("开场节点的 Id，一般填 start，要和下面某个节点的 id 一致")]
    public string startNodeId = "start";

    [Tooltip("本段对话里的所有节点（句子）")]
    public List<DialogueNode> nodes = new List<DialogueNode>();

    /// <summary>按 id 找节点，找不到返回 null</summary>
    public DialogueNode GetNode(string id)
    {
        if (string.IsNullOrEmpty(id) || nodes == null)
        {
            return null;
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] != null && nodes[i].id == id)
            {
                return nodes[i];
            }
        }

        return null;
    }
}

/// <summary>对话里的一句话（一个节点）</summary>
[Serializable]
public class DialogueNode
{
    [Tooltip("节点唯一 Id，例如 start / ask / bye，给跳转用")]
    public string id = "start";

    [TextArea(2, 6)]
    [Tooltip("显示在 DialogContent 里的正文")]
    public string content;

    [Tooltip("无分支时：点 ClickArea 后跳到的下一句 Id。留空且没有选项 = 对话结束")]
    public string nextNodeId;

    [Tooltip("有分支时填这里（一般 1～2 个）。有选项时点 ClickArea 无效，必须点选项")]
    public List<DialogueChoice> choices = new List<DialogueChoice>();
}

/// <summary>对话分支选项（对应 UI 上的 DialogOption）</summary>
[Serializable]
public class DialogueChoice
{
    [Tooltip("选项按钮上显示的文字")]
    public string text = "选项";

    [Tooltip("选完跳到哪个节点 Id；留空 = 选完直接结束对话")]
    public string nextNodeId;
}
