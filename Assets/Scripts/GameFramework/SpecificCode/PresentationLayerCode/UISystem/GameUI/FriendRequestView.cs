using UnityEngine;
using UnityEngine.UI;

//*****************************************
//功能说明：好友申请一行
//*****************************************
public class FriendRequestView : MonoBehaviour
{
    private Text nameText;
    private Text levelText;
    private Text onlineText;
    private Button agreeBtn;
    private Button refuseBtn;
    private FriendRequestEntryMsg data;
    private System.Action<FriendRequestEntryMsg, bool> onRespond;

    public void Bind()
    {
        Transform n = DeepFindTransform.DeepFindChild(transform, "FriendRequestNameText");
        if (n != null)
        {
            nameText = n.GetComponent<Text>();
        }

        Transform l = DeepFindTransform.DeepFindChild(transform, "FriendRequestLevel");
        if (l != null)
        {
            levelText = l.GetComponent<Text>();
        }

        Transform o = DeepFindTransform.DeepFindChild(transform, "OnlineState");
        if (o != null)
        {
            onlineText = o.GetComponent<Text>();
            if (onlineText == null)
            {
                onlineText = o.GetComponentInChildren<Text>();
            }
        }

        Transform a = DeepFindTransform.DeepFindChild(transform, "Btn_agrree")
                      ?? DeepFindTransform.DeepFindChild(transform, "Btn_agree");
        if (a != null)
        {
            agreeBtn = a.GetComponent<Button>();
        }

        Transform r = DeepFindTransform.DeepFindChild(transform, "Btn_refuse");
        if (r != null)
        {
            refuseBtn = r.GetComponent<Button>();
        }
    }

    public void SetData(FriendRequestEntryMsg entry, System.Action<FriendRequestEntryMsg, bool> respond)
    {
        data = entry;
        onRespond = respond;

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

        if (agreeBtn != null)
        {
            agreeBtn.onClick.RemoveAllListeners();
            agreeBtn.onClick.AddListener(() => onRespond?.Invoke(data, true));
        }

        if (refuseBtn != null)
        {
            refuseBtn.onClick.RemoveAllListeners();
            refuseBtn.onClick.AddListener(() => onRespond?.Invoke(data, false));
        }
    }
}
