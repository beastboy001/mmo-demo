using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//*****************************************
//功能说明：人物选择面板
// 列表有角色 → 点 Toggle 显示对应模型
// 列表为空   → 不显示模型
//***************************************** 
public class ChoicePanel : BasePanel
{
    private const string TogglePrefabPath = "Prefabs/UI/ChoicePanel/Toggle_CharacterInfo";

    private Button enterGameBtn;
    private Button createCharacterBtn;
    private Button deleteCharacterBtn;
    private Button exitBtn;
    private ToggleGroup toggleGroup;
    private Transform characterListContent;

    private GameObject characterGo;
    private P09CharacterDress dress;
    private CharacterPreviewRotator previewRotator;
    private readonly List<GameObject> togglesGo = new List<GameObject>();

    private int selectedIndex = -1;
    private int pendingDeleteIndex = -1; // 第一次点删除只提示，同一角色再点一次才删
    private bool isRefreshingList;

    public override void OnInit()
    {
        enterGameBtn = DeepFindTransform.DeepFindChild(transform, "Btn_StartGame").GetComponent<Button>();
        createCharacterBtn = DeepFindTransform.DeepFindChild(transform, "Btn_CreateCharacter").GetComponent<Button>();
        deleteCharacterBtn = DeepFindTransform.DeepFindChild(transform, "Btn_DeleteCharacter").GetComponent<Button>();
        exitBtn = DeepFindTransform.DeepFindChild(transform, "Btn_Exit").GetComponent<Button>();
        toggleGroup = DeepFindTransform.DeepFindChild(transform, "CharacterInfoContent").GetComponent<ToggleGroup>();
        characterListContent = toggleGroup.transform;

        enterGameBtn.onClick.AddListener(OnEnterGameClick);
        createCharacterBtn.onClick.AddListener(OnOpenCreateCharacterPanelClick);
        deleteCharacterBtn.onClick.AddListener(OnDeleteCharacterClick);
        exitBtn.onClick.AddListener(OnReturnToLoginPanelClick);

        previewRotator = CharacterPreviewRotator.GetOrCreate(transform);

        // 提前加载外观资源，避免第一次点按钮时卡
        P09AppearanceDataProvider.EnsureLoaded();

        base.OnInit();
    }

    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);

        if (previewRotator == null)
        {
            previewRotator = CharacterPreviewRotator.GetOrCreate(transform);
        }

        CharacterPreviewModel.ResetRotation();
        CharacterPreviewModel.WarmUp();
        pendingDeleteIndex = -1;

        this.RegistEvent<CharacterListResultEvent>(OnCharacterListResult);
        this.RegistEvent<DeleteCharacterResultEvent>(OnDeleteCharacterResult);

        INetSystem net = this.GetSystem<INetSystem>();
        if (net != null && net.IsConnected)
        {
            net.SendCharacterList();
        }

        RefreshCharacterList();
    }

    public override void OnClose()
    {
        this.UnRegistEvent<CharacterListResultEvent>(OnCharacterListResult);
        this.UnRegistEvent<DeleteCharacterResultEvent>(OnDeleteCharacterResult);
        previewRotator?.Unbind();
        base.OnClose();
    }

    private void OnCharacterListResult(object data)
    {
        CharacterListResMsg res = data as CharacterListResMsg;
        if (res == null || !res.ok)
        {
            return;
        }

        uiSystem.ClearPlayerDataList();
        if (res.characters != null)
        {
            for (int i = 0; i < res.characters.Length; i++)
            {
                PlayerData pd = CharacterDataConverter.ToPlayerData(res.characters[i]);
                if (pd != null)
                {
                    uiSystem.AddPlayerData(pd);
                }
            }
        }

        RefreshCharacterList();
    }

    private void OnDeleteCharacterResult(object data)
    {
        DeleteCharacterResMsg res = data as DeleteCharacterResMsg;
        if (res == null)
        {
            uiSystem.OpenPanel<TipPanel>("删除回包异常");
            return;
        }

        if (!res.ok)
        {
            uiSystem.OpenPanel<TipPanel>(res.message);
            return;
        }

        uiSystem.RemovePlayerDataByCharacterId(res.characterId);
        pendingDeleteIndex = -1;
        if (selectedIndex >= uiSystem.PlayerDataCount)
        {
            selectedIndex = uiSystem.PlayerDataCount - 1;
        }

        RefreshCharacterList();
        uiSystem.OpenPanel<TipPanel>("角色已删除");
    }

    /// <summary>根据 UISystem 里的角色列表刷新 Toggle</summary>
    private void RefreshCharacterList()
    {
        isRefreshingList = true;
        ClearCharacterToggles();

        if (uiSystem.PlayerDataCount == 0)
        {
            selectedIndex = -1;
            HideCharacterModel();
            isRefreshingList = false;
            return;
        }

        GameObject togglePrefab = GameResSystem.GetRes<GameObject>(TogglePrefabPath);
        if (togglePrefab == null)
        {
            Debug.LogError("[ChoicePanel] 找不到 Toggle 预制体: " + TogglePrefabPath);
            isRefreshingList = false;
            return;
        }

        for (int i = 0; i < uiSystem.PlayerDataCount; i++)
        {
            int index = i;
            PlayerData pd = uiSystem.GetPlayerDataAt(index);
            if (pd == null) continue;

            GameObject toggleGo = Object.Instantiate(togglePrefab, characterListContent);
            togglesGo.Add(toggleGo);

            Toggle toggle = toggleGo.GetComponent<Toggle>();
            toggle.group = toggleGroup;

            Text idText = DeepFindTransform.DeepFindChild(toggleGo.transform, "Text_CharacterID")?.GetComponent<Text>();
            Text infoText = DeepFindTransform.DeepFindChild(toggleGo.transform, "Text_CharacterInfo")?.GetComponent<Text>();
            if (idText != null) idText.text = pd.id;
            if (infoText != null) infoText.text = BuildCharacterInfoText(pd);

            toggle.onValueChanged.AddListener(isOn =>
            {
                if (isRefreshingList || !isOn) return;
                OnCharacterToggleSelected(index);
            });
        }

        int selectIndex = selectedIndex >= 0 && selectedIndex < uiSystem.PlayerDataCount
            ? selectedIndex
            : 0;

        if (togglesGo.Count > 0)
        {
            togglesGo[selectIndex].GetComponent<Toggle>().isOn = true;
        }

        isRefreshingList = false;
        OnCharacterToggleSelected(selectIndex);
    }

    private void OnCharacterToggleSelected(int index)
    {
        PlayerData pd = uiSystem.GetPlayerDataAt(index);
        if (pd == null)
        {
            selectedIndex = -1;
            HideCharacterModel();
            return;
        }

        selectedIndex = index;
        pendingDeleteIndex = -1; // 换了角色，删除确认作废
        uiSystem.SetChoiePlayerData(pd);
        ShowCharacterModel();
    }

    private void ShowCharacterModel()
    {
        EnsureCharacterModel();
        if (characterGo == null || dress == null) return;

        dress.Apply(uiSystem.ChoiePlayerData);
        characterGo.SetActive(true);
        previewRotator?.Bind(characterGo.transform);
    }

    private void HideCharacterModel()
    {
        previewRotator?.Unbind();
        CharacterPreviewModel.Hide();
        characterGo = null;
        dress = null;
    }

    private void EnsureCharacterModel()
    {
        dress = CharacterPreviewModel.GetOrCreateDress();
        characterGo = dress != null ? dress.gameObject : null;
    }

    private void ClearCharacterToggles()
    {
        for (int i = togglesGo.Count - 1; i >= 0; i--)
        {
            if (togglesGo[i] != null)
            {
                Object.Destroy(togglesGo[i]);
            }
        }

        togglesGo.Clear();
    }

    private static string BuildCharacterInfoText(PlayerData pd)
    {
        string gender = pd.gender == GENDER.MALE ? "男" : "女";
        string roleName = RoleVisualCatalog.Get(pd.role).DisplayName;
        return "等级" + pd.level + " " + gender + " " + roleName;
    }

    /// <summary>加载或销毁预览角色（保留给动画事件 / 外部调用）</summary>
    public void LoadOrDestoryPlayerModelGameObjects(object obj)
    {
        bool ifLoad = (bool)obj;
        if (ifLoad)
        {
            if (selectedIndex >= 0)
            {
                ShowCharacterModel();
            }
        }
        else
        {
            HideCharacterModel();
        }
    }

    private void OnEnterGameClick()
    {
        if (selectedIndex < 0)
        {
            uiSystem.OpenPanel<TipPanel>("请先选择一个角色");
            return;
        }

        previewRotator?.Unbind();
        uiSystem.OpenPanel<LoadPanel>();
        OnClose();
    }

    private void OnOpenCreateCharacterPanelClick()
    {
        previewRotator?.Unbind();
        uiSystem.OpenPanel<CharacterCreatePanel>();
        OnClose();
    }

    private void OnDeleteCharacterClick()
    {
        if (selectedIndex < 0)
        {
            uiSystem.OpenPanel<TipPanel>("请先选择要删除的角色");
            return;
        }

        // 第一次：弹 Tip 确认；同一角色第二次：真正删除
        if (pendingDeleteIndex != selectedIndex)
        {
            pendingDeleteIndex = selectedIndex;
            uiSystem.OpenPanel<TipPanel>("你是否要删除该角色？");
            return;
        }

        PlayerData pd = uiSystem.GetPlayerDataAt(selectedIndex);
        if (pd == null)
        {
            return;
        }

        INetSystem net = this.GetSystem<INetSystem>();
        if (net == null || !net.IsConnected)
        {
            uiSystem.OpenPanel<TipPanel>("未连接服务器");
            return;
        }

        if (pd.characterId <= 0)
        {
            uiSystem.RemovePlayerDataAt(selectedIndex);
            pendingDeleteIndex = -1;
            RefreshCharacterList();
            return;
        }

        net.SendDeleteCharacter(pd.characterId);
    }

    private void OnReturnToLoginPanelClick()
    {
        OnClose();
        uiSystem.OpenPanel<LoginPanel>();
    }
}
