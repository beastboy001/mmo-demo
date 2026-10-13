using UnityEngine;
using UnityEngine.UI;

//*****************************************
//功能说明：社交 UI 协调器（挂在 GamePanel 上）
//   · InteractingWithPlayers / PlayerFunctionPanel 互斥
//   · ESC 开关 PlayerFunctionPanel
//   · 打开时隐藏交互列表，关闭后若仍在范围则恢复
//*****************************************
public class SocialUiController : MonoBehaviour, IController
{
    private IUISystem uiSystem;
    private ICursorSystem cursorSystem;
    private INetSystem netSystem;

    private GameObject interactingRoot;
    private Text interactingNameText;
    private Text interactingLevelText;
    private Button interactingExitBtn;
    private Button addFriendBtn;

    private GameObject functionRoot;
    private Text localNameText;
    private Text localLevelText;
    private Button functionExitBtn;
    private Button functionFriendBtn;
    private Button exitGameBtn;

    private int targetCharacterId;
    private string targetName;
    private int targetLevel;
    private bool exitGameArmed;
    private bool cursorHeld;
    private int suppressOverlayNotify;

    private void Awake()
    {
        BindInteractingPanel();
        BindFunctionPanel();
        HideAllNested();
    }

    private void Start()
    {
        uiSystem = this.GetSystem<IUISystem>();
        cursorSystem = this.GetSystem<ICursorSystem>();
        netSystem = this.GetSystem<INetSystem>();
        this.RegistEvent<AddFriendResultEvent>(OnAddFriendResult);
    }

    private void OnDestroy()
    {
        this.UnRegistEvent<AddFriendResultEvent>(OnAddFriendResult);
        ReleaseCursor();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEsc();
        }
    }

    /// <summary>靠近玩家按 F 后调用</summary>
    public void OpenInteractingWithPlayer(int characterId, string playerName, int level)
    {
        CloseFriendsAndChat();
        CloseFunctionPanel(false);
        exitGameArmed = false;

        targetCharacterId = characterId;
        targetName = playerName;
        targetLevel = level;

        if (interactingNameText != null)
        {
            interactingNameText.text = string.IsNullOrEmpty(playerName) ? "未知玩家" : playerName;
        }

        if (interactingLevelText != null)
        {
            interactingLevelText.text = "Lv." + level;
        }

        if (interactingRoot != null)
        {
            interactingRoot.SetActive(true);
        }

        SetSocialBusy(true);
        RequestCursor();
        GameplayControlLock.SetLocked(true);
    }

    public void CloseInteractingPanel(bool refreshList = true)
    {
        if (interactingRoot != null)
        {
            interactingRoot.SetActive(false);
        }

        targetCharacterId = 0;
        if (!IsAnySocialOpen())
        {
            SetSocialBusy(false);
            ReleaseCursor();
            GameplayControlLock.SetLocked(false);
        }

        if (refreshList)
        {
            // InteractionManager.SetSocialBusy(false) 已触发列表刷新
        }
    }

    public void OpenFunctionPanel()
    {
        CloseFriendsAndChat();
        CloseInteractingPanel(false);
        exitGameArmed = false;
        RefreshLocalPlayerInfo();

        if (functionRoot != null)
        {
            functionRoot.SetActive(true);
        }

        SetSocialBusy(true);
        RequestCursor();
        GameplayControlLock.SetLocked(true);
    }

    public void CloseFunctionPanel(bool unlock = true)
    {
        if (functionRoot != null)
        {
            functionRoot.SetActive(false);
        }

        exitGameArmed = false;
        if (!IsAnySocialOpen())
        {
            SetSocialBusy(false);
            ReleaseCursor();
            if (unlock)
            {
                GameplayControlLock.SetLocked(false);
            }
        }
    }

    public void OpenFriendsList()
    {
        CloseInteractingPanel(false);
        CloseFunctionPanel(false);
        SetSocialBusy(true);
        RequestCursor();
        GameplayControlLock.SetLocked(true);

        if (uiSystem != null)
        {
            uiSystem.OpenPanel<FriendsListPanel>();
        }
    }

    public void OpenChatPanel(FriendEntryMsg friend)
    {
        if (uiSystem != null)
        {
            uiSystem.OpenPanel<ChatPanel>(friend);
        }
    }

    public bool IsInteractingOpen => interactingRoot != null && interactingRoot.activeSelf;
    public bool IsFunctionOpen => functionRoot != null && functionRoot.activeSelf;

    private void HandleEsc()
    {
        // 先关上层面板
        if (uiSystem != null)
        {
            // Chat / Friends 用 BasePanel：优先关它们
            ChatPanel chat = Object.FindObjectOfType<ChatPanel>(true);
            if (chat != null && chat.gameObject.activeSelf)
            {
                uiSystem.ClosePanel(nameof(ChatPanel));
                return;
            }

            FriendsListPanel friends = Object.FindObjectOfType<FriendsListPanel>(true);
            if (friends != null && friends.gameObject.activeSelf)
            {
                uiSystem.ClosePanel(nameof(FriendsListPanel));
                if (!IsInteractingOpen && !IsFunctionOpen)
                {
                    SetSocialBusy(false);
                    ReleaseCursor();
                    GameplayControlLock.SetLocked(false);
                }

                return;
            }
        }

        if (IsInteractingOpen)
        {
            CloseInteractingPanel();
            return;
        }

        if (IsFunctionOpen)
        {
            CloseFunctionPanel();
            return;
        }

        OpenFunctionPanel();
    }

    private void BindInteractingPanel()
    {
        Transform root = DeepFindTransform.DeepFindChild(transform, "InteractingWithPlayers");
        if (root == null)
        {
            return;
        }

        interactingRoot = root.gameObject;

        // 对方玩家信息：Image (1)/PlayerName/PlayerNameText（不是本地玩家）
        Transform nameT = DeepFindTransform.DeepFindChild(root, "PlayerNameText");
        if (nameT == null)
        {
            Transform nameParent = DeepFindTransform.DeepFindChild(root, "PlayerName");
            if (nameParent != null)
            {
                nameT = nameParent;
            }
        }

        if (nameT != null)
        {
            interactingNameText = nameT.GetComponent<Text>();
            if (interactingNameText == null)
            {
                interactingNameText = nameT.GetComponentInChildren<Text>(true);
            }
        }

        // 对方等级：Playerlevel/PlayerlevelText
        Transform levelT = DeepFindTransform.DeepFindChild(root, "PlayerlevelText");
        if (levelT == null)
        {
            levelT = DeepFindTransform.DeepFindChild(root, "Playerlevel");
        }

        if (levelT != null)
        {
            interactingLevelText = levelT.GetComponent<Text>();
            if (interactingLevelText == null)
            {
                interactingLevelText = levelT.GetComponentInChildren<Text>(true);
            }
        }

        Transform exitT = DeepFindTransform.DeepFindChild(root, "Btn_Exit");
        if (exitT != null)
        {
            interactingExitBtn = exitT.GetComponent<Button>();
            if (interactingExitBtn != null)
            {
                interactingExitBtn.onClick.RemoveAllListeners();
                interactingExitBtn.onClick.AddListener(() => CloseInteractingPanel());
            }
        }

        Transform addT = DeepFindTransform.DeepFindChild(root, "Btn_AddFriend");
        if (addT != null)
        {
            addFriendBtn = addT.GetComponent<Button>();
            if (addFriendBtn != null)
            {
                addFriendBtn.onClick.RemoveAllListeners();
                addFriendBtn.onClick.AddListener(OnClickAddFriend);
            }
        }
    }

    private void BindFunctionPanel()
    {
        Transform root = DeepFindTransform.DeepFindChild(transform, "PlayerFunctionPanel");
        if (root == null)
        {
            return;
        }

        functionRoot = root.gameObject;

        Transform nameT = DeepFindTransform.DeepFindChild(root, "LocalPlayerNameText")
                          ?? DeepFindTransform.DeepFindChild(root, "LocalPlayerName");
        if (nameT != null)
        {
            localNameText = nameT.GetComponent<Text>();
            if (localNameText == null)
            {
                localNameText = nameT.GetComponentInChildren<Text>();
            }
        }

        Transform levelT = DeepFindTransform.DeepFindChild(root, "LocalPlayerLevelText")
                           ?? DeepFindTransform.DeepFindChild(root, "LocalPlayerLevel");
        if (levelT != null)
        {
            localLevelText = levelT.GetComponent<Text>();
            if (localLevelText == null)
            {
                localLevelText = levelT.GetComponentInChildren<Text>();
            }
        }

        Transform exitT = FindDirectOrDeep(root, "Btn_Exit");
        if (exitT != null)
        {
            functionExitBtn = exitT.GetComponent<Button>();
            if (functionExitBtn != null)
            {
                functionExitBtn.onClick.RemoveAllListeners();
                functionExitBtn.onClick.AddListener(() => CloseFunctionPanel());
            }
        }

        Transform friendT = DeepFindTransform.DeepFindChild(root, "Btn_Friend");
        if (friendT != null)
        {
            functionFriendBtn = friendT.GetComponent<Button>();
            if (functionFriendBtn != null)
            {
                functionFriendBtn.onClick.RemoveAllListeners();
                functionFriendBtn.onClick.AddListener(OnClickOpenFriends);
            }
        }

        Transform quitT = DeepFindTransform.DeepFindChild(root, "Btn_ExitGame");
        if (quitT != null)
        {
            exitGameBtn = quitT.GetComponent<Button>();
            if (exitGameBtn != null)
            {
                exitGameBtn.onClick.RemoveAllListeners();
                exitGameBtn.onClick.AddListener(OnClickExitGame);
            }
        }
    }

    private static Transform FindDirectOrDeep(Transform root, string name)
    {
        Transform direct = root.Find(name);
        if (direct != null)
        {
            return direct;
        }

        return DeepFindTransform.DeepFindChild(root, name);
    }

    private void RefreshLocalPlayerInfo()
    {
        if (uiSystem == null)
        {
            uiSystem = this.GetSystem<IUISystem>();
        }

        PlayerData pd = uiSystem != null ? uiSystem.ChoiePlayerData : null;
        string n = pd != null && !string.IsNullOrEmpty(pd.id) ? pd.id : "玩家";
        int lv = pd != null ? pd.level : 1;

        if (localNameText != null)
        {
            localNameText.text = n;
        }

        if (localLevelText != null)
        {
            localLevelText.text = "Lv." + lv;
        }
    }

    private void OnClickAddFriend()
    {
        if (targetCharacterId <= 0)
        {
            ShowTip("无法添加：目标角色无效");
            return;
        }

        if (netSystem == null)
        {
            netSystem = this.GetSystem<INetSystem>();
        }

        if (netSystem == null || !netSystem.IsConnected)
        {
            ShowTip("未连接服务器");
            return;
        }

        PlayerWavePlayer.PlayLocalWave();
        netSystem.SendAddFriend(targetCharacterId);
    }

    private void OnAddFriendResult(object data)
    {
        AddFriendResMsg res = data as AddFriendResMsg;
        if (res == null)
        {
            return;
        }

        ShowTip(string.IsNullOrEmpty(res.message) ? (res.ok ? "已发送申请" : "失败") : res.message);
    }

    private void OnClickOpenFriends()
    {
        OpenFriendsList();
    }

    private void OnClickExitGame()
    {
        if (!exitGameArmed)
        {
            exitGameArmed = true;
            ShowTip("再按一次「退出游戏」确认退出");
            return;
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowTip(string msg)
    {
        if (uiSystem == null)
        {
            uiSystem = this.GetSystem<IUISystem>();
        }

        if (uiSystem != null)
        {
            uiSystem.OpenPanel<TipPanel>(msg);
        }
        else
        {
            Debug.Log("[Social] " + msg);
        }
    }

    private void CloseFriendsAndChat()
    {
        if (uiSystem == null)
        {
            return;
        }

        suppressOverlayNotify++;
        try
        {
            uiSystem.ClosePanel(nameof(ChatPanel));
            uiSystem.ClosePanel(nameof(FriendsListPanel));
        }
        finally
        {
            suppressOverlayNotify--;
        }
    }

    private bool IsAnySocialOpen()
    {
        if (IsInteractingOpen || IsFunctionOpen)
        {
            return true;
        }

        FriendsListPanel friends = Object.FindObjectOfType<FriendsListPanel>(true);
        if (friends != null && friends.gameObject.activeSelf)
        {
            return true;
        }

        ChatPanel chat = Object.FindObjectOfType<ChatPanel>(true);
        return chat != null && chat.gameObject.activeSelf;
    }

    private void HideAllNested()
    {
        if (interactingRoot != null)
        {
            interactingRoot.SetActive(false);
        }

        if (functionRoot != null)
        {
            functionRoot.SetActive(false);
        }
    }

    private void SetSocialBusy(bool busy)
    {
        InteractionManager mgr = InteractionManager.EnsureExists();
        mgr.SetSocialBusy(busy);
    }

    private void RequestCursor()
    {
        if (cursorHeld)
        {
            return;
        }

        if (cursorSystem == null)
        {
            cursorSystem = this.GetSystem<ICursorSystem>();
        }

        if (cursorSystem != null)
        {
            cursorSystem.ShowForUI();
            cursorHeld = true;
        }
    }

    private void ReleaseCursor()
    {
        if (!cursorHeld)
        {
            return;
        }

        if (cursorSystem != null)
        {
            cursorSystem.HideForUI();
        }

        cursorHeld = false;
    }

    /// <summary>FriendsList / Chat 关闭时回调，恢复操作</summary>
    public void NotifyOverlayClosed()
    {
        if (suppressOverlayNotify > 0)
        {
            return;
        }

        if (!IsAnySocialOpen())
        {
            SetSocialBusy(false);
            ReleaseCursor();
            GameplayControlLock.SetLocked(false);
        }
    }
}
