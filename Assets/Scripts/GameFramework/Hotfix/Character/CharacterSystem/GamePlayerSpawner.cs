using System.Collections;
using UnityEngine;

//*****************************************
//功能说明：进入游戏场景时，在 Birthplace 下生成本地玩家
// 生成后向服务器进图，并启动位置上报 / 远程玩家管理
//*****************************************
public class GamePlayerSpawner : MonoBehaviour, IController
{
    public const string PlayerPrefabPath = "Prefabs/Character/P09_Human_Player";
    public const string BirthplaceName = "Birthplace";
    public const int DefaultMapId = 1;

    [Header("可不填，默认挂在本物体(Birthplace)下")]
    public Transform spawnPoint;

    private void Start()
    {
        StartCoroutine(SpawnWhenSceneReady());
    }

    private IEnumerator SpawnWhenSceneReady()
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        SpawnLocalPlayer();
    }

    public void SpawnLocalPlayer()
    {
        IUISystem uiSystem = this.GetSystem<IUISystem>();
        if (uiSystem == null)
        {
            Debug.LogError("[GamePlayerSpawner] 找不到 UISystem，请确认 LoginScene 里有 GameStarter");
            return;
        }

        PlayerData pd = uiSystem.ChoiePlayerData;
        if (pd == null || string.IsNullOrEmpty(pd.id))
        {
            Debug.LogError("[GamePlayerSpawner] 没有选中的角色数据，无法生成玩家");
            return;
        }

        CharacterPreviewModel.Hide();

        Transform parent = spawnPoint != null ? spawnPoint : transform;
        if (parent == null)
        {
            GameObject birthplace = GameObject.Find(BirthplaceName);
            parent = birthplace != null ? birthplace.transform : null;
        }

        if (parent == null)
        {
            Debug.LogError("[GamePlayerSpawner] 找不到 Birthplace 出生点");
            return;
        }

        GameObject prefab = GameResSystem.GetRes<GameObject>(PlayerPrefabPath);
        if (prefab == null)
        {
            Debug.LogError("[GamePlayerSpawner] 找不到预制体: " + PlayerPrefabPath);
            return;
        }

        GameObject player = Object.Instantiate(prefab, parent);
        player.name = "P09_Human_Player_Local";
        player.transform.localPosition = Vector3.zero;
        player.transform.localRotation = Quaternion.identity;
        player.transform.localScale = Vector3.one;

        P09CharacterDress dress = player.GetComponent<P09CharacterDress>();
        if (dress == null)
        {
            dress = player.AddComponent<P09CharacterDress>();
        }

        dress.Apply(pd);

        // ----- 联网同步 -----
        RemotePlayerManager.Ensure();

        if (player.GetComponent<LocalPlayerStateSender>() == null)
        {
            player.AddComponent<LocalPlayerStateSender>();
        }

        INetSystem net = this.GetSystem<INetSystem>();
        if (net != null && net.IsConnected && net.UserId > 0)
        {
            // 世界坐标发给服务器（parent 下 localZero → 转世界）
            Vector3 worldPos = player.transform.position;
            float yaw = player.transform.eulerAngles.y;
            net.SendEnterMap(pd, DefaultMapId, worldPos, yaw);
            Debug.Log("[GamePlayerSpawner] 已请求进图 mapId=" + DefaultMapId);
        }
        else
        {
            Debug.LogWarning("[GamePlayerSpawner] 未连接服务器，仅单机模式（别人看不见你）");
        }
    }
}
