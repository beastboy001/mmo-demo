using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//*****************************************
//功能说明：好友私聊面板（历史 + 发送 + 推送）
//*****************************************
public class ChatPanel : BasePanel
{
    private const string MinePrefabPath = "Prefabs/UI/GamePanel/ChatMessageMine";
    private const string OtherPrefabPath = "Prefabs/UI/GamePanel/ChatMessageOther";

    protected override bool NeedCursor => true;

    private INetSystem netSystem;
    private Text titleText;
    private Button exitBtn;
    private Button sendBtn;
    private InputField inputField;
    private Transform content;
    private ScrollRect scrollRect;

    private GameObject minePrefab;
    private GameObject otherPrefab;
    private FriendEntryMsg currentFriend;
    private readonly HashSet<int> shownMessageIds = new HashSet<int>();

    public override void OnInit()
    {
        netSystem = this.GetSystem<INetSystem>();
        BindUi();
        minePrefab = Resources.Load<GameObject>(MinePrefabPath);
        otherPrefab = Resources.Load<GameObject>(OtherPrefabPath);
        base.OnInit();
    }

    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);
        this.RegistEvent<ChatHistoryResultEvent>(OnChatHistory);
        this.RegistEvent<ChatSendResultEvent>(OnChatSendResult);
        this.RegistEvent<ChatPushEvent>(OnChatPush);

        currentFriend = null;
        if (objs != null && objs.Length > 0)
        {
            currentFriend = objs[0] as FriendEntryMsg;
        }

        if (titleText != null)
        {
            titleText.text = currentFriend != null ? currentFriend.name : "私聊";
        }

        ClearMessages();
        if (inputField != null)
        {
            inputField.text = string.Empty;
        }

        RequestHistory();
    }

    public override void OnClose()
    {
        this.UnRegistEvent<ChatHistoryResultEvent>(OnChatHistory);
        this.UnRegistEvent<ChatSendResultEvent>(OnChatSendResult);
        this.UnRegistEvent<ChatPushEvent>(OnChatPush);
        base.OnClose();

        SocialUiController social = Object.FindObjectOfType<SocialUiController>(true);
        if (social != null)
        {
            social.NotifyOverlayClosed();
        }
    }

    private void BindUi()
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
            Debug.LogWarning("[ChatPanel] 找不到 Btn_Exit");
        }

        Transform sendT = DeepFindTransform.DeepFindChild(transform, "Btn_Send");
        if (sendT != null)
        {
            sendBtn = sendT.GetComponent<Button>();
            if (sendBtn != null)
            {
                sendBtn.onClick.RemoveAllListeners();
                sendBtn.onClick.AddListener(OnClickSend);
            }
        }

        Transform inputT = DeepFindTransform.DeepFindChild(transform, "Input_Message")
                           ?? DeepFindTransform.DeepFindChild(transform, "Input Message")
                           ?? DeepFindTransform.DeepFindChild(transform, "InputField (Legacy)")
                           ?? DeepFindTransform.DeepFindChild(transform, "InputField");
        if (inputT != null)
        {
            inputField = inputT.GetComponent<InputField>();
            if (inputField != null)
            {
                inputField.onEndEdit.RemoveAllListeners();
                inputField.onEndEdit.AddListener(OnInputEndEdit);
            }
        }

        Transform contentT = DeepFindTransform.DeepFindChild(transform, "Content");
        if (contentT != null)
        {
            content = contentT;
            scrollRect = contentT.GetComponentInParent<ScrollRect>();
        }
    }

    private void RequestHistory()
    {
        if (netSystem == null)
        {
            netSystem = this.GetSystem<INetSystem>();
        }

        if (netSystem == null || currentFriend == null || currentFriend.characterId <= 0)
        {
            return;
        }

        netSystem.SendChatHistory(currentFriend.characterId, 50);
    }

    private void OnClickSend()
    {
        if (netSystem == null)
        {
            netSystem = this.GetSystem<INetSystem>();
        }

        if (netSystem == null || currentFriend == null || inputField == null)
        {
            return;
        }

        string text = inputField.text != null ? inputField.text.Trim() : string.Empty;
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        netSystem.SendChatMessage(currentFriend.characterId, text);
        inputField.text = string.Empty;
        inputField.ActivateInputField();
    }

    private void OnInputEndEdit(string _)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnClickSend();
        }
    }

    private void OnChatHistory(object data)
    {
        ChatHistoryResMsg res = data as ChatHistoryResMsg;
        if (res == null)
        {
            return;
        }

        if (currentFriend == null || res.peerCharacterId != currentFriend.characterId)
        {
            return;
        }

        if (!res.ok)
        {
            Debug.LogWarning("[ChatPanel] 拉历史失败: " + res.message);
            return;
        }

        ClearMessages();
        if (res.messages == null)
        {
            return;
        }

        for (int i = 0; i < res.messages.Length; i++)
        {
            AppendMessage(res.messages[i], false);
        }

        ScrollToBottom();
    }

    private void OnChatSendResult(object data)
    {
        ChatSendResMsg res = data as ChatSendResMsg;
        if (res == null)
        {
            return;
        }

        if (!res.ok)
        {
            Debug.LogWarning("[ChatPanel] 发送失败: " + res.message);
            if (uiSystem != null && !string.IsNullOrEmpty(res.message))
            {
                uiSystem.OpenPanel<TipPanel>(res.message);
            }

            return;
        }

        if (res.msg == null || currentFriend == null)
        {
            return;
        }

        if (res.msg.toCharacterId != currentFriend.characterId &&
            res.msg.fromCharacterId != currentFriend.characterId)
        {
            return;
        }

        AppendMessage(res.msg, true);
    }

    private void OnChatPush(object data)
    {
        ChatPushMsg push = data as ChatPushMsg;
        if (push == null || push.msg == null || currentFriend == null)
        {
            return;
        }

        ChatMessageItemMsg msg = push.msg;
        bool related = msg.fromCharacterId == currentFriend.characterId ||
                       msg.toCharacterId == currentFriend.characterId;
        if (!related)
        {
            return;
        }

        AppendMessage(msg, true);
    }

    private void ClearMessages()
    {
        shownMessageIds.Clear();
        if (content == null)
        {
            return;
        }

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(content.GetChild(i).gameObject);
        }
    }

    private void AppendMessage(ChatMessageItemMsg msg, bool scroll)
    {
        if (msg == null || content == null)
        {
            return;
        }

        if (msg.id > 0 && !shownMessageIds.Add(msg.id))
        {
            return;
        }

        int myId = netSystem != null ? netSystem.EntityId : 0;
        bool isMine = myId > 0 && msg.fromCharacterId == myId;
        GameObject prefab = isMine ? minePrefab : otherPrefab;
        if (prefab == null)
        {
            Debug.LogWarning("[ChatPanel] 消息预制体未加载: " + (isMine ? MinePrefabPath : OtherPrefabPath));
            return;
        }

        GameObject go = Object.Instantiate(prefab, content, false);
        go.name = isMine ? "ChatMessageMine" : "ChatMessageOther";
        ApplyBubbleText(go, msg.content ?? string.Empty);

        if (scroll)
        {
            ScrollToBottom();
        }
    }

    private static void ApplyBubbleText(GameObject row, string text)
    {
        ChatBubbleFit fit = row.GetComponentInChildren<ChatBubbleFit>(true);
        if (fit != null)
        {
            fit.SetText(text);
            return;
        }

        Transform textT = DeepFindTransform.DeepFindChild(row.transform, "MessageText");
        if (textT != null)
        {
            Text uiText = textT.GetComponent<Text>();
            if (uiText != null)
            {
                uiText.text = text;
            }
        }
    }

    private void ScrollToBottom()
    {
        if (scrollRect == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
