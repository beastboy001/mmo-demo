using UnityEngine;
using UnityEngine.UI;

//*****************************************
//功能说明：左侧一条 InteractionOption 的显示脚本
// 挂在（或运行时 Add 到）InteractionOption 预制体根节点
//
// 预制体结构约定：
//   InteractionOption (Image = 行背景)
//     ├─ InteractionContent (Text = 动作名)
//     └─ Image_KeyTip / TextKeyTip (Text = 按键提示)
//*****************************************
public class InteractionOptionView : MonoBehaviour
{
    private Image bgImage;      // 行背景
    private Text contentText;  // 「对话」等
    private Text keyTipText;   // 「F」等

    // 选中：白底黑字；未选中：灰底白字
    private static readonly Color SelectedBg = Color.white;
    private static readonly Color UnselectedBg = new Color(0.26f, 0.26f, 0.26f, 1f);
    private static readonly Color SelectedText = Color.black;
    private static readonly Color UnselectedText = Color.white;

    /// <summary>找齐子节点引用（Instantiate 之后调一次）</summary>
    public void Bind()
    {
        bgImage = GetComponent<Image>();
        Transform content = transform.Find("InteractionContent");
        if (content != null)
        {
            contentText = content.GetComponent<Text>();
        }

        Transform keyTip = transform.Find("Image_KeyTip/TextKeyTip");
        if (keyTip != null)
        {
            keyTipText = keyTip.GetComponent<Text>();
        }
    }

    /// <summary>用数据填文字</summary>
    public void SetData(InteractionOptionData data)
    {
        if (contentText != null)
        {
            contentText.text = data != null ? data.displayName : "";
        }

        if (keyTipText != null)
        {
            keyTipText.text = data != null ? data.keyTip : "";
        }
    }

    /// <summary>切换选中/未选中外观</summary>
    public void SetSelected(bool selected)
    {
        if (bgImage != null)
        {
            bgImage.color = selected ? SelectedBg : UnselectedBg;
        }

        if (contentText != null)
        {
            contentText.color = selected ? SelectedText : UnselectedText;
        }
    }
}
