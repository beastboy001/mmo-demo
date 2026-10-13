using UnityEngine;
using UnityEngine.UI;

//*****************************************
//功能说明：好友列表一行
//*****************************************
public class FriendInformationView : MonoBehaviour
{
    private Text nameText;
    private Text levelText;
    private Text onlineText;
    private Button chatBtn;
    private FriendEntryMsg data;
    private System.Action<FriendEntryMsg> onChat;

    public void Bind()
    {
        Transform n = DeepFindTransform.DeepFindChild(transform, "FriendNameText");
        if (n != null)
        {
            nameText = n.GetComponent<Text>();
        }

        Transform l = DeepFindTransform.DeepFindChild(transform, "PlayerLevelText");
        if (l != null)
        {
            levelText = l.GetComponent<Text>();
        }

        Transform o = DeepFindTransform.DeepFindChild(transform, "OnlineStateText");
        if (o != null)
        {
            onlineText = o.GetComponent<Text>();
        }

        Transform b = DeepFindTransform.DeepFindChild(transform, "Btn_StartChatting");
        if (b != null)
        {
            chatBtn = b.GetComponent<Button>();
        }
    }

    public void SetData(FriendEntryMsg entry, System.Action<FriendEntryMsg> chatCallback)
    {
        data = entry;
        onChat = chatCallback;

        if (nameText != null)
        {
            nameText.text = entry != null ? entry.name : "";
        }

        if (levelText != null)
        {
            levelText.text = entry != null ? ("Lv." + entry.level) : "";
        }

        if (onlineText != null)
        {
            onlineText.text = entry != null && entry.online ? "在线" : "离线";
        }

        if (chatBtn != null)
        {
            chatBtn.onClick.RemoveAllListeners();
            chatBtn.onClick.AddListener(() => onChat?.Invoke(data));
        }
    }
}
