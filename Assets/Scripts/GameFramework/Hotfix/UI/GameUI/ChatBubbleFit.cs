using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 微信式气泡：字少随内容变宽，超过 maxWidth 后换行变高。
/// 挂在 Bubble 上。
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class ChatBubbleFit : MonoBehaviour, ILayoutElement
{
    [SerializeField] private float maxWidth = 280f;
    [SerializeField] private float paddingX = 32f;
    [SerializeField] private float paddingY = 20f;
    [SerializeField] private bool resizeRow = true;

    private Text messageText;
    private RectTransform bubbleRect;
    private RectTransform textRect;
    private float cachedPreferredWidth;
    private float cachedPreferredHeight;

    public float minWidth => cachedPreferredWidth;
    public float preferredWidth => cachedPreferredWidth;
    public float flexibleWidth => -1f;
    public float minHeight => cachedPreferredHeight;
    public float preferredHeight => cachedPreferredHeight;
    public float flexibleHeight => -1f;
    public int layoutPriority => 1;

    public void CalculateLayoutInputHorizontal() => Recalculate();
    public void CalculateLayoutInputVertical() => Recalculate();

    private void OnEnable()
    {
        Refresh();
    }

    private void OnValidate()
    {
        Refresh();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
        {
            Refresh();
        }
    }
#endif

    /// <summary>改文字后调用；运行时发消息请走这个。</summary>
    public void SetText(string content)
    {
        EnsureRefs();
        if (messageText != null)
        {
            messageText.text = content ?? string.Empty;
        }

        Refresh();
    }

    public void Refresh()
    {
        Recalculate();
        ApplySizes();
        if (resizeRow)
        {
            ResizeParentRow();
        }

        if (bubbleRect != null)
        {
            LayoutRebuilder.MarkLayoutForRebuild(bubbleRect);
        }
    }

    private void Recalculate()
    {
        EnsureRefs();
        if (messageText == null || textRect == null)
        {
            cachedPreferredWidth = paddingX;
            cachedPreferredHeight = paddingY;
            return;
        }

        float maxTextWidth = Mathf.Max(1f, maxWidth - paddingX);
        float textWidth = Mathf.Clamp(messageText.preferredWidth, 1f, maxTextWidth);
        textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
        float textHeight = Mathf.Max(messageText.preferredHeight, messageText.fontSize);

        cachedPreferredWidth = textWidth + paddingX;
        cachedPreferredHeight = textHeight + paddingY;
    }

    private void ApplySizes()
    {
        EnsureRefs();
        if (bubbleRect == null || textRect == null || messageText == null)
        {
            return;
        }

        float textWidth = Mathf.Max(1f, cachedPreferredWidth - paddingX);
        float textHeight = Mathf.Max(1f, cachedPreferredHeight - paddingY);
        textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
        textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);
        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cachedPreferredWidth);
        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cachedPreferredHeight);
    }

    private void ResizeParentRow()
    {
        RectTransform row = transform.parent as RectTransform;
        if (row == null)
        {
            return;
        }

        float avatarHeight = 0f;
        for (int i = 0; i < row.childCount; i++)
        {
            RectTransform child = row.GetChild(i) as RectTransform;
            if (child == null || child == bubbleRect)
            {
                continue;
            }

            if (child.name.IndexOf("Avatar", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                avatarHeight = child.rect.height;
            }
        }

        float rowHeight = Mathf.Max(cachedPreferredHeight, avatarHeight, 40f);
        row.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowHeight);
    }

    private void EnsureRefs()
    {
        if (bubbleRect == null)
        {
            bubbleRect = transform as RectTransform;
        }

        if (messageText == null)
        {
            messageText = GetComponentInChildren<Text>(true);
        }

        if (messageText != null && textRect == null)
        {
            textRect = messageText.rectTransform;
        }
    }
}
