using UnityEngine;

//*****************************************
//功能说明：远程玩家可交互（走近登记「互动」选项，按 F 打开 InteractingWithPlayers）
//*****************************************
[RequireComponent(typeof(SphereCollider))]
public class PlayerInteractable : MonoBehaviour
{
    [Header("左侧交互条")]
    public string actionDisplayName = "互动";
    public string keyTip = "F";
    public KeyCode key = KeyCode.F;
    public float triggerRadius = 2.5f;

    private RemotePlayerView remoteView;
    private bool playerInside;
    private InteractionOptionData registeredOption;

    private void Reset()
    {
        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = triggerRadius;
        col.center = new Vector3(0f, 1f, 0f);
    }

    private void Awake()
    {
        remoteView = GetComponent<RemotePlayerView>();
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
            if (col.radius < 0.5f)
            {
                col.radius = triggerRadius;
            }

            if (col.center == Vector3.zero)
            {
                col.center = new Vector3(0f, 1f, 0f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsLocalPlayer(other) || playerInside)
        {
            return;
        }

        playerInside = true;
        RegisterOption();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsLocalPlayer(other) || !playerInside)
        {
            return;
        }

        playerInside = false;
        UnregisterOption();
    }

    private void OnDisable()
    {
        playerInside = false;
        UnregisterOption();
    }

    private static bool IsLocalPlayer(Collider other)
    {
        return other != null && other.GetComponentInParent<PlayerMovementController>() != null;
    }

    private void RegisterOption()
    {
        InteractionManager mgr = InteractionManager.EnsureExists();
        UnregisterOption();

        registeredOption = new InteractionOptionData
        {
            displayName = actionDisplayName,
            keyTip = keyTip,
            key = key,
            owner = this,
            onExecute = OpenInteractPanel
        };
        mgr.Register(registeredOption);
    }

    private void UnregisterOption()
    {
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.UnregisterOwner(this);
        }

        registeredOption = null;
    }

    private void OpenInteractPanel()
    {
        SocialUiController social = Object.FindObjectOfType<SocialUiController>(true);
        if (social == null)
        {
            Debug.LogWarning("[PlayerInteractable] 找不到 SocialUiController（应挂在 GamePanel 上）");
            return;
        }

        int characterId = remoteView != null ? remoteView.CharacterId : 0;
        string playerName = remoteView != null ? remoteView.DisplayName : name;
        int level = remoteView != null ? remoteView.Level : 1;
        social.OpenInteractingWithPlayer(characterId, playerName, level);
    }
}
