using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//*****************************************
//功能说明：好友列表面板
//   FriendList / AddFriendList 页签 + 高光
//*****************************************
public class FriendsListPanel : BasePanel
{
    private const string FriendInfoPath = "Prefabs/UI/GamePanel/FriendInformation";
    private const string FriendRequestPath = "Prefabs/UI/GamePanel/FriendRequest";

    protected override bool NeedCursor => true;

    private INetSystem netSystem;

    private GameObject friendListPage;
    private GameObject addFriendListPage;
    private Transform friendListContent;
    private Transform requestListContent;
    private Text requestNumText;
    private InputField searchInput;
    private Button searchBtn;
    private Button exitBtn;
    private Button tabFriendBtn;
    private Button tabAddBtn;
    private GameObject highlightFriend;
    private GameObject highlightAdd;

    private GameObject friendInfoPrefab;
    private GameObject friendRequestPrefab;

    private readonly List<FriendRequestEntryMsg> allRequests = new List<FriendRequestEntryMsg>();
    private string searchFilter = "";

    public override void OnInit()
    {
        netSystem = this.GetSystem<INetSystem>();
        BindUi();
        friendInfoPrefab = Resources.Load<GameObject>(FriendInfoPath);
        friendRequestPrefab = Resources.Load<GameObject>(FriendRequestPath);
        base.OnInit();
    }

    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);
        this.RegistEvent<FriendListResultEvent>(OnFriendList);
        this.RegistEvent<FriendRequestListResultEvent>(OnFriendRequests);
        this.RegistEvent<RespondFriendResultEvent>(OnRespondResult);
        ShowFriendListTab();
        RequestData();
    }

    public override void OnClose()
    {
        this.UnRegistEvent<FriendListResultEvent>(OnFriendList);
        this.UnRegistEvent<FriendRequestListResultEvent>(OnFriendRequests);
        this.UnRegistEvent<RespondFriendResultEvent>(OnRespondResult);
        base.OnClose();

        SocialUiController social = Object.FindObjectOfType<SocialUiController>(true);
        if (social != null)
        {
            social.NotifyOverlayClosed();
        }
    }

    private void BindUi()
    {
        friendListPage = FindGo("FriendList");
        addFriendListPage = FindGo("AddFriendList");

        Transform flContent = DeepFindTransform.DeepFindChild(transform, "FriendListContent");
        friendListContent = flContent;

        Transform reqContent = null;
        if (addFriendListPage != null)
        {
            reqContent = DeepFindTransform.DeepFindChild(addFriendListPage.transform, "Content");
        }

        if (reqContent == null)
        {
            reqContent = DeepFindTransform.DeepFindChild(transform, "Content");
        }

        requestListContent = reqContent;

        Transform numT = DeepFindTransform.DeepFindChild(transform, "FriendRequestNum");
        if (numT != null)
        {
            requestNumText = numT.GetComponent<Text>();
        }

        Transform inputT = DeepFindTransform.DeepFindChild(transform, "InputField");
        if (inputT != null)
        {
            searchInput = inputT.GetComponent<InputField>();
        }

        Transform searchT = DeepFindTransform.DeepFindChild(transform, "Btn_Search");
        if (searchT != null)
        {
            searchBtn = searchT.GetComponent<Button>();
            if (searchBtn != null)
            {
                searchBtn.onClick.AddListener(OnClickSearch);
            }
        }

        Transform exitT = DeepFindTransform.DeepFindChild(transform, "Btn_Exit");
        if (exitT != null)
        {
            exitBtn = exitT.GetComponent<Button>();
            if (exitBtn != null)
            {
                exitBtn.onClick.AddListener(() => uiSystem.ClosePanel(nameof(FriendsListPanel)));
            }
        }

        Transform tabF = DeepFindTransform.DeepFindChild(transform, "Btn_FriendList");
        if (tabF != null)
        {
            tabFriendBtn = tabF.GetComponent<Button>();
            highlightFriend = FindHighlight(tabF);
            if (tabFriendBtn != null)
            {
                tabFriendBtn.onClick.AddListener(ShowFriendListTab);
            }
        }

        Transform tabA = DeepFindTransform.DeepFindChild(transform, "Btn_AddFriendList");
        if (tabA != null)
        {
            tabAddBtn = tabA.GetComponent<Button>();
            highlightAdd = FindHighlight(tabA);
            if (tabAddBtn != null)
            {
                tabAddBtn.onClick.AddListener(ShowAddFriendTab);
            }
        }
    }

    private GameObject FindGo(string name)
    {
        Transform t = DeepFindTransform.DeepFindChild(transform, name);
        return t != null ? t.gameObject : null;
    }

    private static GameObject FindHighlight(Transform btn)
    {
        Transform h = btn.Find("highlightImage");
        if (h == null)
        {
            h = DeepFindTransform.DeepFindChild(btn, "highlightImage");
        }

        return h != null ? h.gameObject : null;
    }

    private void RequestData()
    {
        if (netSystem == null)
        {
            netSystem = this.GetSystem<INetSystem>();
        }

        if (netSystem == null)
        {
            return;
        }

        netSystem.SendFriendList();
        netSystem.SendFriendRequestList();
    }

    private void ShowFriendListTab()
    {
        if (friendListPage != null)
        {
            friendListPage.SetActive(true);
        }

        if (addFriendListPage != null)
        {
            addFriendListPage.SetActive(false);
        }

        SetHighlight(true);
        if (netSystem != null)
        {
            netSystem.SendFriendList();
        }
    }

    private void ShowAddFriendTab()
    {
        if (friendListPage != null)
        {
            friendListPage.SetActive(false);
        }

        if (addFriendListPage != null)
        {
            addFriendListPage.SetActive(true);
        }

        SetHighlight(false);
        searchFilter = "";
        if (searchInput != null)
        {
            searchInput.text = "";
        }

        if (netSystem != null)
        {
            netSystem.SendFriendRequestList();
        }

        RebuildRequestList();
    }

    private void SetHighlight(bool friendTab)
    {
        if (highlightFriend != null)
        {
            highlightFriend.SetActive(friendTab);
        }

        if (highlightAdd != null)
        {
            highlightAdd.SetActive(!friendTab);
        }
    }

    private void OnClickSearch()
    {
        searchFilter = searchInput != null ? searchInput.text.Trim() : "";
        RebuildRequestList();
    }

    private void OnFriendList(object data)
    {
        FriendListResMsg res = data as FriendListResMsg;
        if (res == null || !res.ok)
        {
            return;
        }

        ClearChildren(friendListContent);
        if (friendInfoPrefab == null || friendListContent == null || res.friends == null)
        {
            return;
        }

        for (int i = 0; i < res.friends.Length; i++)
        {
            FriendEntryMsg entry = res.friends[i];
            if (entry == null)
            {
                continue;
            }

            GameObject go = Object.Instantiate(friendInfoPrefab, friendListContent);
            FriendInformationView view = go.GetComponent<FriendInformationView>();
            if (view == null)
            {
                view = go.AddComponent<FriendInformationView>();
            }

            view.Bind();
            view.SetData(entry, OpenChat);
        }
    }

    private void OnFriendRequests(object data)
    {
        FriendRequestListResMsg res = data as FriendRequestListResMsg;
        if (res == null || !res.ok)
        {
            return;
        }

        allRequests.Clear();
        if (res.requests != null)
        {
            for (int i = 0; i < res.requests.Length; i++)
            {
                if (res.requests[i] != null)
                {
                    allRequests.Add(res.requests[i]);
                }
            }
        }

        if (requestNumText != null)
        {
            requestNumText.text = allRequests.Count.ToString();
        }

        RebuildRequestList();
    }

    private void RebuildRequestList()
    {
        ClearChildren(requestListContent);
        if (friendRequestPrefab == null || requestListContent == null)
        {
            return;
        }

        for (int i = 0; i < allRequests.Count; i++)
        {
            FriendRequestEntryMsg entry = allRequests[i];
            if (entry == null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(searchFilter) &&
                (entry.name == null || entry.name.IndexOf(searchFilter, System.StringComparison.OrdinalIgnoreCase) < 0))
            {
                continue;
            }

            GameObject go = Object.Instantiate(friendRequestPrefab, requestListContent);
            FriendRequestView view = go.GetComponent<FriendRequestView>();
            if (view == null)
            {
                view = go.AddComponent<FriendRequestView>();
            }

            view.Bind();
            view.SetData(entry, Respond);
        }
    }

    private void OpenChat(FriendEntryMsg friend)
    {
        SocialUiController social = Object.FindObjectOfType<SocialUiController>(true);
        if (social != null)
        {
            social.OpenChatPanel(friend);
        }
        else if (uiSystem != null)
        {
            uiSystem.OpenPanel<ChatPanel>(friend);
        }
    }

    private void Respond(FriendRequestEntryMsg entry, bool accept)
    {
        if (entry == null || netSystem == null)
        {
            return;
        }

        netSystem.SendRespondFriend(entry.requestId, accept);
    }

    private void OnRespondResult(object data)
    {
        RespondFriendResMsg res = data as RespondFriendResMsg;
        if (res == null)
        {
            return;
        }

        if (uiSystem != null)
        {
            uiSystem.OpenPanel<TipPanel>(res.message);
        }

        if (res.ok && netSystem != null)
        {
            netSystem.SendFriendRequestList();
            netSystem.SendFriendList();
        }
    }

    private static void ClearChildren(Transform parent)
    {
        if (parent == null)
        {
            return;
        }

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(parent.GetChild(i).gameObject);
        }
    }
}
