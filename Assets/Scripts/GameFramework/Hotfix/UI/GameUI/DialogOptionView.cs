using System;
using UnityEngine;
using UnityEngine.UI;

//*****************************************
//功能说明：对话分支一条 DialogOption 的显示脚本
//
// 预制体结构约定：
//   DialogOption (Button)
//     ├─ OptionContent (Text = 选项文字)
//     └─ Index_background / TextIndex (Text = 1、2…)
//*****************************************
public class DialogOptionView : MonoBehaviour
{
    private Text indexText;    // TextIndex
    private Text contentText;  // OptionContent
    private Button button;
    private Action onClick;     // 点下去时回调（一般是 DialogueRunner.Choose）

    /// <summary>找齐引用，并绑好 Button 点击</summary>
    public void Bind()
    {
        Transform indexT = transform.Find("Index_background/TextIndex");
        if (indexT == null)
        {
            indexT = DeepFindTransform.DeepFindChild(transform, "TextIndex");
        }

        if (indexT != null)
        {
            indexText = indexT.GetComponent<Text>();
        }

        Transform content = transform.Find("OptionContent");
        if (content == null)
        {
            content = DeepFindTransform.DeepFindChild(transform, "OptionContent");
        }

        if (content != null)
        {
            contentText = content.GetComponent<Text>();
        }

        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke());
        }
    }

    /// <summary>
    /// 填数据和点击回调
    /// index1Based：显示用的序号，从 1 开始（对应 TextIndex）
    /// </summary>
    public void SetData(int index1Based, string text, Action click)
    {
        onClick = click;
        if (indexText != null)
        {
            indexText.text = index1Based.ToString();
        }

        if (contentText != null)
        {
            contentText.text = text ?? "";
        }
    }
}
