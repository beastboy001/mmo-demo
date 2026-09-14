using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//*****************************************
//功能说明：UI系统
// 当前角色数据 ChoiePlayerData 只在这里有一份，其它地方通过 GetSystem 访问
//***************************************** 
public class UISystem : IUISystem
{
    private static readonly string[] LoginPanelTypes =
    {
        "LoginPanel",
        "RegisterPanel",
        "MaskPanel",
        "ChoicePanel",
        "TipPanel",
        "LoadPanel",
        "CharacterCreatePanel",
    };

    private readonly Dictionary<string, BasePanel> panelDict = new Dictionary<string, BasePanel>();
    private Transform canvasTrans;
    private Transform gameCanvasTrans;
    private bool gameUiLoaded;

    // 全局唯一一份当前角色数据（整个游戏只 new 这一次）
    private readonly PlayerData choiePlayerData = new PlayerData();
    private readonly List<PlayerData> playerDataList = new List<PlayerData>();

    public PlayerData ChoiePlayerData => choiePlayerData;
    public IReadOnlyList<PlayerData> PlayerDataList => playerDataList;
    public int PlayerDataCount => playerDataList.Count;

    public void Init()
    {
        choiePlayerData.ResetToCreateDefaults();

        canvasTrans = GameObject.Find("Canvas").transform;
        AddPanelToDict<LoginPanel>();
        AddPanelToDict<RegisterPanel>();
        AddPanelToDict<MaskPanel>();
        AddPanelToDict<ChoicePanel>();
        AddPanelToDict<TipPanel>();
        AddPanelToDict<LoadPanel>();
        AddPanelToDict<CharacterCreatePanel>();
        OpenPanel<LoginPanel>();
    }

    public void ResetCreatePlayerData()
    {
        choiePlayerData.ResetToCreateDefaults();
    }

    public void SetChoiePlayerData(PlayerData data)
    {
        choiePlayerData.CopyFrom(data);
    }

    public PlayerData GetPlayerDataAt(int index)
    {
        if (index < 0 || index >= playerDataList.Count)
        {
            return null;
        }

        return playerDataList[index];
    }

    public void AddPlayerData(PlayerData data)
    {
        if (data == null || string.IsNullOrEmpty(data.id))
        {
            return;
        }

        PlayerData copy = new PlayerData();
        copy.CopyFrom(data);
        playerDataList.Add(copy);
    }

    public void RemovePlayerDataAt(int index)
    {
        if (index < 0 || index >= playerDataList.Count)
        {
            return;
        }

        playerDataList.RemoveAt(index);
    }

    public void ClearPlayerDataList()
    {
        playerDataList.Clear();
    }

    public void RemovePlayerDataByCharacterId(int characterId)
    {
        if (characterId <= 0)
        {
            return;
        }

        for (int i = playerDataList.Count - 1; i >= 0; i--)
        {
            if (playerDataList[i] != null && playerDataList[i].characterId == characterId)
            {
                playerDataList.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 添加面板到UI系统
    /// </summary>
    private void AddPanelToDict<T>(Transform parent = null) where T : BasePanel
    {
        Transform root = parent != null ? parent : canvasTrans;
        if (root == null)
        {
            Debug.LogError("[UISystem] Canvas 为空，无法创建面板: " + typeof(T));
            return;
        }

        string panelType = typeof(T).ToString();
        if (panelDict.ContainsKey(panelType))
        {
            Debug.Log("当前字典已有：" + panelType + "类型的面板");
            return;
        }

        GameObject panelGo = GameObject.Instantiate(
            GameResSystem.GetRes<GameObject>("Prefabs/UI/" + panelType),
            root);
        panelDict.Add(panelType, panelGo.AddComponent<T>());
        panelDict[panelType].OnInit();
    }

    /// <summary>
    /// 从UI系统移除面板
    /// </summary>
    private void RemovePanelInDict(string panelType)
    {
        if (!panelDict.ContainsKey(panelType))
        {
            return;
        }

        GameObject panelGo = panelDict[panelType].gameObject;
        panelDict.Remove(panelType);
        GameObject.Destroy(panelGo);
    }

    public void OpenPanel<T>(params object[] objs) where T : BasePanel
    {
        string panelType = typeof(T).ToString();
        if (!panelDict.ContainsKey(panelType))
        {
            Debug.Log("当前字典不包含：" + panelType + "类型的面板");
            return;
        }

        panelDict[panelType].OnShow(objs);
    }

    public void ClosePanel(string panelType)
    {
        if (!panelDict.ContainsKey(panelType))
        {
            Debug.Log("当前字典不包含：" + panelType + "类型的面板");
            return;
        }

        panelDict[panelType].OnClose();
    }

    /// <summary>
    /// 进入游戏场景：卸登录 UI，创建 DontDestroyOnLoad 游戏 Canvas 并打开 GamePanel
    /// </summary>
    public void UnLoadLoginPanelsAndLoadGamePanels()
    {
        if (gameUiLoaded)
        {
            OpenPanel<GamePanel>();
            return;
        }

        for (int i = 0; i < LoginPanelTypes.Length; i++)
        {
            RemovePanelInDict(LoginPanelTypes[i]);
        }

        canvasTrans = null;
        EnsureGameCanvas();
        EnsureEventSystem();
        AddPanelToDict<GamePanel>(gameCanvasTrans);
        AddPanelToDict<FriendsListPanel>(gameCanvasTrans);
        AddPanelToDict<ChatPanel>(gameCanvasTrans);
        AddPanelToDict<TipPanel>(gameCanvasTrans);

        gameUiLoaded = true;
        OpenPanel<GamePanel>();
    }

    private void EnsureGameCanvas()
    {
        if (gameCanvasTrans != null)
        {
            return;
        }

        GameObject canvasGo = new GameObject("GameCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0f;

        canvasGo.AddComponent<GraphicRaycaster>();
        Object.DontDestroyOnLoad(canvasGo);
        gameCanvasTrans = canvasGo.transform;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<StandaloneInputModule>();
        Object.DontDestroyOnLoad(eventSystemGo);
    }
}
