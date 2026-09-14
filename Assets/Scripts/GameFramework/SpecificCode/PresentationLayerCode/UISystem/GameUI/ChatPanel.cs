using UnityEngine;
using UnityEngine.UI;

//*****************************************
//功能说明：私聊面板（第一版只做开关）
//*****************************************
public class ChatPanel : BasePanel
{
    protected override bool NeedCursor => true;

    private Text titleText;
    private Button exitBtn;
    private FriendEntryMsg currentFriend;

    public override void OnInit()
    {
        Transform title = DeepFindTransform.DeepFindChild(transform, "FriendNameText")
                          ?? DeepFindTransform.DeepFindChild(transform, "Title");
        if (title != null)
        {
            titleText = title.GetComponent<Text>();
        }

        Transform exitT = DeepFindTransform.DeepFindChild(transform, "Btn_Exit")
                          ?? DeepFindTransform.DeepFindChild(transform, "Btn_exit")
                          ?? DeepFindTransform.DeepFindChild(transform, "Exit");
        if (exitT != null)
        {
            exitBtn = exitT.GetComponent<Button>();
        }

        if (exitBtn == null)
        {
            // 预制体若还没命名，尝试找任意带 Exit 的按钮
            Button[] buttons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null && buttons[i].name.ToLower().Contains("exit"))
                {
                    exitBtn = buttons[i];
                    break;
                }
            }
        }

        if (exitBtn != null)
        {
            exitBtn.onClick.RemoveAllListeners();
            exitBtn.onClick.AddListener(() => uiSystem.ClosePanel(nameof(ChatPanel)));
        }
        else
        {
            Debug.LogWarning("[ChatPanel] 找不到 Btn_Exit，请在预制体上加关闭按钮并命名为 Btn_Exit");
        }

        base.OnInit();
    }

    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);
        currentFriend = null;
        if (objs != null && objs.Length > 0)
        {
            currentFriend = objs[0] as FriendEntryMsg;
        }

        if (titleText != null && currentFriend != null)
        {
            titleText.text = currentFriend.name;
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        SocialUiController social = Object.FindObjectOfType<SocialUiController>(true);
        if (social != null)
        {
            social.NotifyOverlayClosed();
        }
    }
}
