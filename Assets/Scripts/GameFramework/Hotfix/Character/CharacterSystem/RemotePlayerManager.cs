using System.Collections.Generic;
using UnityEngine;

//*****************************************
//功能说明：管理场景里「别人」的角色
// 订阅 EntityEnter / Leave / State，生成或更新远程玩家
//*****************************************
public class RemotePlayerManager : MonoBehaviour, IController
{
    public const string PlayerPrefabPath = "Prefabs/Character/P09_Human_Player";

    private static RemotePlayerManager instance;

    private readonly Dictionary<int, RemotePlayerView> remoteDict = new Dictionary<int, RemotePlayerView>();
    private Transform remoteRoot;
    private INetSystem netSystem;

    public static RemotePlayerManager Ensure()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject go = new GameObject("RemotePlayerManager");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<RemotePlayerManager>();
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        remoteRoot = new GameObject("RemotePlayers").transform;
        remoteRoot.SetParent(transform, false);
    }

    private void OnEnable()
    {
        this.RegistEvent<EntityEnterEvent>(OnEntityEnter);
        this.RegistEvent<EntityLeaveEvent>(OnEntityLeave);
        this.RegistEvent<EntityStateEvent>(OnEntityState);
    }

    private void OnDisable()
    {
        this.UnRegistEvent<EntityEnterEvent>(OnEntityEnter);
        this.UnRegistEvent<EntityLeaveEvent>(OnEntityLeave);
        this.UnRegistEvent<EntityStateEvent>(OnEntityState);
    }

    private void Start()
    {
        netSystem = this.GetSystem<INetSystem>();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void OnEntityEnter(object data)
    {
        EntityEnterMsg msg = data as EntityEnterMsg;
        if (msg == null)
        {
            return;
        }

        if (netSystem == null)
        {
            netSystem = this.GetSystem<INetSystem>();
        }

        // 不生成自己
        if (netSystem != null && netSystem.EntityId > 0 && msg.entityId == netSystem.EntityId)
        {
            return;
        }

        if (remoteDict.ContainsKey(msg.entityId))
        {
            remoteDict[msg.entityId].ApplyEnter(msg);
            return;
        }

        GameObject prefab = GameResSystem.GetRes<GameObject>(PlayerPrefabPath);
        if (prefab == null)
        {
            Debug.LogError("[RemotePlayerManager] 找不到玩家预制体");
            return;
        }

        // 先以未激活方式生成，避免 Awake 里把 FreeLook 拆到场景根节点抢走镜头
        bool prefabWasActive = prefab.activeSelf;
        prefab.SetActive(false);
        GameObject go = Instantiate(prefab, remoteRoot);
        prefab.SetActive(prefabWasActive);

        go.name = "Remote_" + msg.entityId + "_" + msg.name;
        PrepareAsRemote(go);
        go.SetActive(true);

        RemotePlayerView view = go.GetComponent<RemotePlayerView>();
        if (view == null)
        {
            view = go.AddComponent<RemotePlayerView>();
        }

        view.Setup(msg.entityId);
        view.ApplyEnter(msg);
        remoteDict[msg.entityId] = view;

        // 再保险：清掉场景里误拆出来的、跟在这个角色上的镜头
        CleanupOrphanCamerasFollowing(go.transform);

        Debug.Log("[RemotePlayerManager] 生成远程玩家 " + msg.name + " id=" + msg.entityId);
    }

    private void OnEntityLeave(object data)
    {
        EntityLeaveMsg msg = data as EntityLeaveMsg;
        if (msg == null)
        {
            return;
        }

        if (!remoteDict.TryGetValue(msg.entityId, out RemotePlayerView view))
        {
            return;
        }

        remoteDict.Remove(msg.entityId);
        if (view != null)
        {
            CleanupOrphanCamerasFollowing(view.transform);
            Destroy(view.gameObject);
        }
    }

    private void OnEntityState(object data)
    {
        EntityStateMsg msg = data as EntityStateMsg;
        if (msg == null)
        {
            return;
        }

        if (netSystem != null && msg.entityId == netSystem.EntityId)
        {
            return;
        }

        if (remoteDict.TryGetValue(msg.entityId, out RemotePlayerView view))
        {
            view.ApplyState(msg);
        }
    }

    /// <summary>远程角色：删镜头、关移动，只保留外观</summary>
    private static void PrepareAsRemote(GameObject go)
    {
        // 先删相机相关（此时物体仍是 inactive，Awake 还没跑）
        DestroyCameraObjects(go.transform);

        InputController ic = go.GetComponent<InputController>();
        if (ic == null)
        {
            ic = go.AddComponent<InputController>();
        }

        ic.isSyncCharacter = true;

        PlayerMovementController move = go.GetComponent<PlayerMovementController>();
        if (move != null)
        {
            // 避免激活时再 Detach 镜头；远程不需要这套逻辑
            Object.Destroy(move);
        }

        CharacterController cc = go.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
        }

        CharacterFSM fsm = go.GetComponent<CharacterFSM>();
        if (fsm != null)
        {
            Object.Destroy(fsm);
        }

        AudioListener listener = go.GetComponentInChildren<AudioListener>(true);
        if (listener != null)
        {
            Object.Destroy(listener);
        }
    }

    private static void DestroyCameraObjects(Transform root)
    {
        // 收集后再 Destroy，避免遍历时改层级
        List<GameObject> toDestroy = new List<GameObject>();
        Transform[] all = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null || all[i] == root)
            {
                continue;
            }

            string n = all[i].name;
            if (n.Contains("FreeLook") || n.Contains("CM ") || n == "Camera" || n.Contains("Cinemachine"))
            {
                toDestroy.Add(all[i].gameObject);
            }
        }

        for (int i = 0; i < toDestroy.Count; i++)
        {
            if (toDestroy[i] != null)
            {
                Object.DestroyImmediate(toDestroy[i]);
            }
        }
    }

    /// <summary>清掉已拆到场景根、但仍 Follow 该角色的 FreeLook</summary>
    private static void CleanupOrphanCamerasFollowing(Transform targetRoot)
    {
        if (targetRoot == null)
        {
            return;
        }

        Behaviour[] behaviours = Object.FindObjectsOfType<Behaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            Behaviour b = behaviours[i];
            if (b == null)
            {
                continue;
            }

            string typeName = b.GetType().Name;
            if (typeName != "CinemachineFreeLook" && typeName != "CinemachineVirtualCamera")
            {
                continue;
            }

            // 用反射读 Follow / LookAt，避免强依赖 Cinemachine 程序集写法
            Transform follow = GetTransformProperty(b, "Follow");
            Transform lookAt = GetTransformProperty(b, "LookAt");
            if (IsUnder(follow, targetRoot) || IsUnder(lookAt, targetRoot))
            {
                Object.Destroy(b.gameObject);
            }
        }
    }

    private static Transform GetTransformProperty(Behaviour b, string propName)
    {
        var prop = b.GetType().GetProperty(propName);
        if (prop == null)
        {
            return null;
        }

        return prop.GetValue(b, null) as Transform;
    }

    private static bool IsUnder(Transform t, Transform root)
    {
        if (t == null || root == null)
        {
            return false;
        }

        return t == root || t.IsChildOf(root);
    }

    public void ClearAll()
    {
        foreach (var kv in remoteDict)
        {
            if (kv.Value != null)
            {
                CleanupOrphanCamerasFollowing(kv.Value.transform);
                Destroy(kv.Value.gameObject);
            }
        }

        remoteDict.Clear();
    }
}
