using System.Collections.Generic;
using P09.Modular.Humanoid;
using UnityEngine;
using UnityEngine.UI;

//*****************************************
//功能说明：创建角色 UI 面板
//
// 整体流程（和魔兽世界 CharacterCreatePanel 类似）：
//   1. 改 UISystem.ChoiePlayerData（全局唯一一份 PlayerData）
//   2. 调 dress.Apply(PD) 刷新预览模型
//   3. 刷新界面文字 / 色块
//   4. 点创建 → 写入角色名 → 打开 LoadPanel
//*****************************************
public class CharacterCreatePanel : BasePanel
{
    // PD 是 uiSystem.ChoiePlayerData 的简写，方便下面少写几个字
    // => 表示：访问 PD 就等于访问 UISystem 里那份全局 PlayerData
    private PlayerData PD => uiSystem.ChoiePlayerData;

    // 挂在 P09_Human 上的换装脚本，真正改模型靠它
    private P09CharacterDress dress;
    private CharacterPreviewRotator previewRotator;

    // —— 底部按钮 / 输入框 ——
    private Button exitBtn;       // 返回选角界面
    private Button agreeBtn;      // 确认创建
    private InputField nameIF;    // 角色名输入框

    // —— 左侧职业文案 ——
    private Text roleTitleText;   // 职业名（战士、法师…）
    private Text roleDescText;      // 职业简介
    private Text sexValueText;      // 性别行中间 Male / Female

    // —— 右侧形象区 UI 控件 ——
    private Text faceTypeText;      // 脸型数字 01~03
    private Text hairStyleText;     // 发型数字 01~14
    private Text hairColorText;     // 发色数字（ColorImage 下的 Text）
    private Text skinColorText;     // 肤色数字
    private Text eyeColorText;      // 眼色数字
    private Image hairColorImage;   // 发色预览色块
    private Image skinColorImage;   // 肤色预览色块
    private Image eyeColorImage;    // 眼色预览色块

    // 8 个职业按钮，用来高亮当前选中的职业
    private readonly Dictionary<ROLE, Button> roleButtons = new Dictionary<ROLE, Button>();

    // 职业按钮颜色：未选中 / 选中
    private static readonly Color RoleNormal = new Color(1f, 1f, 1f, 0.85f);
    private static readonly Color RoleSelected = new Color(1f, 0.85f, 0.35f, 1f);

    /// <summary>
    /// 面板第一次加载时调用：找 UI 控件、绑按钮点击事件
    /// </summary>
    public override void OnInit()
    {
        exitBtn = DeepFindTransform.DeepFindChild(transform, "Btn_Exit").GetComponent<Button>();
        exitBtn.onClick.AddListener(OnReturnToChoicePanelClick);

        agreeBtn = DeepFindTransform.DeepFindChild(transform, "Btn_CreateCharacter").GetComponent<Button>();
        agreeBtn.onClick.AddListener(OnAgreeCreateClick);

        nameIF = DeepFindTransform.DeepFindChild(transform, "IF_UserID").GetComponent<InputField>();

        BindRoleInfoTexts();    // 左侧职业标题和描述
        BindRoleButtons();      // 8 个职业图标
        BindSexControls();      // 性别左右箭头
        BindFaceTypeRow();      // 脸型
        BindHairStyleRow();     // 发型
        BindHairColorRow();     // 发色
        BindSkinColorRow();     // 肤色
        BindEyeColorRow();      // 眼色
        BindFaceEmotionRow();   // 表情

        previewRotator = CharacterPreviewRotator.GetOrCreate(transform);

        base.OnInit();
        CharacterPreviewModel.WarmUp();
    }

    /// <summary>
    /// 每次打开创建面板时调用
    /// </summary>
    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);

        // 重置为新建角色默认值（不 new 新对象，只清字段）
        uiSystem.ResetCreatePlayerData();

        if (previewRotator == null)
        {
            previewRotator = CharacterPreviewRotator.GetOrCreate(transform);
        }

        CharacterPreviewModel.ResetRotation();

        if (nameIF != null)
        {
            nameIF.text = "";
        }

        RefreshPreview();
    }

    private void RefreshPreview()
    {
        EnsureDress();
        ApplyDress();
        RefreshAllUi();
        BindPreviewRotator();
    }

    private void BindPreviewRotator()
    {
        if (previewRotator == null || dress == null) return;

        previewRotator.Bind(dress.transform);
    }

    public override void OnClose()
    {
        this.UnRegistEvent<CreateCharacterResultEvent>(OnCreateCharacterResult);
        if (agreeBtn != null)
        {
            agreeBtn.interactable = true;
        }

        previewRotator?.Unbind();
        HidePreviewModel();
        base.OnClose();
    }

    private void HidePreviewModel()
    {
        CharacterPreviewModel.Hide();
    }

    // ========== 找模型 + 应用外观 ==========

    /// <summary>
    /// 确保场景里有 P09_Human 和 P09CharacterDress
    /// 优先找场景里已有的，没有就从 Resources 实例化
    /// </summary>
    private void EnsureDress()
    {
        dress = CharacterPreviewModel.GetOrCreateDress();
        if (dress != null)
        {
            dress.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 把当前 PD 交给换装脚本，刷新预览模型
    /// </summary>
    private void ApplyDress()
    {
        EnsureDress();
        if (dress != null)
        {
            dress.Apply(PD);
        }
    }

    // ========== 职业 ==========

    /// <summary>
    /// 绑定 8 个职业按钮（按钮名来自 RoleVisualCatalog，如 Warrior、Mage）
    /// </summary>
    private void BindRoleButtons()
    {
        foreach (RoleVisualCatalog.RoleVisualInfo info in RoleVisualCatalog.All())
        {
            Transform btnTrans = DeepFindTransform.DeepFindChild(transform, info.ButtonName);
            if (btnTrans == null)
            {
                continue;
            }

            Button btn = btnTrans.GetComponent<Button>();
            if (btn == null)
            {
                continue;
            }

            // 先存到局部变量，避免 lambda 闭包抓错职业
            ROLE role = info.Role;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnRoleClicked(role));
            roleButtons[role] = btn; // 记下来，后面做高亮用
        }
    }

    /// <summary>点职业图标：改 PD.role → 刷模型 → 刷左侧文字和高亮</summary>
    private void OnRoleClicked(ROLE role)
    {
        PD.role = role;
        ApplyDress();
        RefreshRoleUi(role);
    }

    /// <summary>更新左上角职业名、简介，并把当前职业按钮变黄</summary>
    private void RefreshRoleUi(ROLE role)
    {
        RoleVisualCatalog.RoleVisualInfo info = RoleVisualCatalog.Get(role);
        if (roleTitleText != null)
        {
            roleTitleText.text = info.DisplayName;
        }
        if (roleDescText != null)
        {
            roleDescText.text = info.Description;
        }

        foreach (KeyValuePair<ROLE, Button> kv in roleButtons)
        {
            Graphic g = kv.Value.targetGraphic;
            if (g != null)
            {
                g.color = kv.Key == role ? RoleSelected : RoleNormal;
            }
        }
    }

    // ========== 性别 ==========

    /// <summary>绑定 Sex 行的左右箭头</summary>
    private void BindSexControls()
    {
        Transform sexRow = DeepFindTransform.DeepFindChild(transform, "Sex");
        if (sexRow == null)
        {
            return;
        }

        Transform searchRoot = DeepFindTransform.DeepFindChild(sexRow, "Select_Field");
        if (searchRoot == null)
        {
            searchRoot = sexRow;
        }

        BindArrow(searchRoot, "Left_Arrow", () => OnSexChanged());
        BindArrow(searchRoot, "Right_Arrow", () => OnSexChanged());

        // 找显示 Male / Female 的 Text
        Text[] texts = searchRoot.GetComponentsInChildren<Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            string v = texts[i].text != null ? texts[i].text.Trim() : "";
            if (v == "Male" || v == "Female" || v == "男" || v == "女")
            {
                sexValueText = texts[i];
                break;
            }
        }
        if (sexValueText == null && texts.Length > 0)
        {
            sexValueText = texts[0];
        }
    }

    /// <summary>男女切换</summary>
    private void OnSexChanged()
    {
        if (PD.gender == GENDER.MALE)
        {
            PD.gender = GENDER.FEMALE;
        }
        else
        {
            PD.gender = GENDER.MALE;
        }

        ApplyDress();
        RefreshSexUi();
        RefreshAppearanceTexts(); // 肤色和脸型跟性别有关，一起刷
    }

    private void RefreshSexUi()
    {
        if (sexValueText == null)
        {
            return;
        }

        sexValueText.text = PD.gender == GENDER.MALE ? "Male" : "Female";
    }

    // ========== 脸型 / 发型 / 颜色 / 表情（绑定左右箭头） ==========

    private void BindFaceTypeRow()
    {
        Transform row = DeepFindTransform.DeepFindChild(transform, "Face_Type");
        if (row == null) return;

        Transform searchRoot = GetSelectField(row);
        faceTypeText = FindNumberText(searchRoot);
        BindArrow(searchRoot, "Left_Arrow", () => StepFaceType(-1));
        BindArrow(searchRoot, "Right_Arrow", () => StepFaceType(1));
    }

    private void BindHairStyleRow()
    {
        Transform row = DeepFindTransform.DeepFindChild(transform, "Hair_Style");
        if (row == null) return;

        Transform searchRoot = GetSelectField(row);
        hairStyleText = FindNumberText(searchRoot);
        BindArrow(searchRoot, "Left_Arrow", () => StepHairStyle(-1));
        BindArrow(searchRoot, "Right_Arrow", () => StepHairStyle(1));
    }

    private void BindHairColorRow()
    {
        BindColorRow("Hair_Color", out hairColorImage, out hairColorText, StepHairColor);
    }

    private void BindSkinColorRow()
    {
        BindColorRow("Skin_Color", out skinColorImage, out skinColorText, StepSkinColor);
    }

    private void BindEyeColorRow()
    {
        BindColorRow("Eye_Color", out eyeColorImage, out eyeColorText, StepEyeColor);
    }

    /// <summary>
    /// 绑定发色/肤色/眼色行：找 ColorImage 里的数字 Text + 左右箭头
    /// 预制体上自带 P09 HorizontalSwitcher，关掉以免和咱们逻辑冲突
    /// </summary>
    private void BindColorRow(string rowName, out Image colorImage, out Text numberText, System.Action<int> onStep)
    {
        colorImage = null;
        numberText = null;

        Transform row = DeepFindTransform.DeepFindChild(transform, rowName);
        if (row == null) return;

        HorizontalSwitcher switcher = row.GetComponent<HorizontalSwitcher>();
        if (switcher != null)
        {
            switcher.enabled = false;
        }

        Transform searchRoot = GetSelectField(row);
        // Hair_Color 用 ColorImage；Skin_Color / Eye_Color 用 Color（预制体命名不一致）
        Transform colorTrans = DeepFindTransform.DeepFindChild(searchRoot, "ColorImage");
        if (colorTrans == null)
        {
            colorTrans = DeepFindTransform.DeepFindChild(searchRoot, "Color");
        }
        if (colorTrans != null)
        {
            colorImage = colorTrans.GetComponent<Image>();
            Transform textTrans = DeepFindTransform.DeepFindChild(colorTrans, "Text");
            if (textTrans != null)
            {
                numberText = textTrans.GetComponent<Text>();
            }
            if (numberText == null)
            {
                numberText = colorTrans.GetComponentInChildren<Text>(true);
            }
        }

        BindArrow(searchRoot, "Left_Arrow", () => onStep(-1));
        BindArrow(searchRoot, "Right_Arrow", () => onStep(1));
    }

    private void BindFaceEmotionRow()
    {
        Transform row = DeepFindTransform.DeepFindChild(transform, "FacialEmotion");
        if (row == null) return;

        Transform searchRoot = GetSelectField(row);
        BindArrow(searchRoot, "Left_Arrow", () => StepFaceEmotion(-1));
        BindArrow(searchRoot, "Right_Arrow", () => StepFaceEmotion(1));
    }

    // —— 左右箭头：改 PD 对应字段 → 刷模型 → 刷 UI ——

    /// <summary>脸型 ±1，范围 1~3</summary>
    private void StepFaceType(int delta)
    {
        PD.faceTypeId = WrapNumber(PD.faceTypeId + delta, P09CharacterDress.FaceMin, P09CharacterDress.FaceMax);
        ApplyDress();
        RefreshAppearanceTexts();
    }

    /// <summary>发型 ±1，范围 1~14（14=光头）</summary>
    private void StepHairStyle(int delta)
    {
        PD.hairStyleId = WrapNumber(PD.hairStyleId + delta, P09CharacterDress.HairMin, P09CharacterDress.HairMax);
        ApplyDress();
        RefreshAppearanceTexts();
    }

    /// <summary>发色 ±1，上限从 P09AppearanceDataProvider 读</summary>
    private void StepHairColor(int delta)
    {
        P09AppearanceDataProvider.EnsureLoaded();
        int maxId = P09AppearanceDataProvider.GetHairColorMaxId();
        PD.hairColorId = WrapNumber(PD.hairColorId + delta, 1, maxId);
        ApplyDress();
        RefreshAppearanceTexts();
    }

    /// <summary>肤色 ±1，男女上限可能不同</summary>
    private void StepSkinColor(int delta)
    {
        P09AppearanceDataProvider.EnsureLoaded();
        int maxId = P09AppearanceDataProvider.GetSkinMaxId(PD.gender);
        PD.skinId = WrapNumber(PD.skinId + delta, 1, maxId);
        ApplyDress();
        RefreshAppearanceTexts();
    }

    /// <summary>眼色 ±1</summary>
    private void StepEyeColor(int delta)
    {
        P09AppearanceDataProvider.EnsureLoaded();
        int maxId = P09AppearanceDataProvider.GetEyeColorMaxId();
        PD.eyeColorId = WrapNumber(PD.eyeColorId + delta, 1, maxId);
        ApplyDress();
        RefreshAppearanceTexts();
    }

    /// <summary>表情 ±1（只刷模型，表情行没有数字显示）</summary>
    private void StepFaceEmotion(int delta)
    {
        P09AppearanceDataProvider.EnsureLoaded();
        int maxId = P09AppearanceDataProvider.GetFaceEmotionMaxId();
        PD.faceEmotionId = WrapNumber(PD.faceEmotionId + delta, 1, maxId);
        ApplyDress();
    }

    // ========== UI 刷新 ==========

    /// <summary>打开面板时，把所有 UI 同步成当前 PD 的状态</summary>
    private void RefreshAllUi()
    {
        RefreshRoleUi(PD.role);
        RefreshSexUi();
        RefreshAppearanceTexts();
    }

    /// <summary>刷新脸型/发型数字，以及发色/肤色/眼色色块</summary>
    private void RefreshAppearanceTexts()
    {
        if (faceTypeText != null)
        {
            faceTypeText.text = PD.faceTypeId.ToString("00");
        }
        if (hairStyleText != null)
        {
            hairStyleText.text = PD.hairStyleId.ToString("00");
        }
        if (hairColorText != null)
        {
            hairColorText.text = PD.hairColorId.ToString("00");
        }
        if (skinColorText != null)
        {
            skinColorText.text = PD.skinId.ToString("00");
        }
        if (eyeColorText != null)
        {
            eyeColorText.text = PD.eyeColorId.ToString("00");
        }

        P09AppearanceDataProvider.EnsureLoaded();

        if (hairColorImage != null)
        {
            var hairData = P09AppearanceDataProvider.GetHairColor(PD.hairColorId);
            Material hairMat = hairData != null ? hairData.GetMaterial(PD.hairStyleId) : null;
            hairColorImage.color = P09AppearanceDataProvider.GetPreviewColor(hairMat);
        }
        if (skinColorImage != null)
        {
            var skinData = P09AppearanceDataProvider.GetSkinColor(PD.gender, PD.skinId);
            skinColorImage.color = P09AppearanceDataProvider.GetPreviewColor(skinData != null ? skinData.Material : null);
        }
        if (eyeColorImage != null)
        {
            var eyeData = P09AppearanceDataProvider.GetEyeColor(PD.eyeColorId);
            eyeColorImage.color = P09AppearanceDataProvider.GetPreviewColor(eyeData != null ? eyeData.Material : null);
        }
    }

    /// <summary>在 TextBox 里找职业标题（大字）和描述（长文）</summary>
    private void BindRoleInfoTexts()
    {
        Transform textBox = DeepFindTransform.DeepFindChild(transform, "TextBox");
        if (textBox == null) return;

        Text[] texts = textBox.GetComponentsInChildren<Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].fontSize >= 36 && texts[i].alignment == TextAnchor.MiddleCenter)
            {
                roleTitleText = texts[i];
            }
            else if (texts[i].text != null && texts[i].text.Length > 20)
            {
                roleDescText = texts[i];
            }
        }

        if (roleTitleText == null && texts.Length > 0) roleTitleText = texts[0];
        if (roleDescText == null && texts.Length > 1) roleDescText = texts[texts.Length - 1];
    }

    // ========== 按钮回调 / 工具方法 ==========

    private void OnReturnToChoicePanelClick()
    {
        OnClose();
        uiSystem.OpenPanel<ChoicePanel>();
    }

    /// <summary>确认创建：提交到服务器，写入 MySQL characters 表</summary>
    private void OnAgreeCreateClick()
    {
        string playerName = nameIF != null ? nameIF.text.Trim() : "";
        if (string.IsNullOrEmpty(playerName))
        {
            uiSystem.OpenPanel<TipPanel>("请输入角色名");
            return;
        }

        PD.id = playerName;

        INetSystem net = this.GetSystem<INetSystem>();
        if (net == null || !net.IsConnected)
        {
            uiSystem.OpenPanel<TipPanel>("未连接服务器，无法创建角色");
            return;
        }

        agreeBtn.interactable = false;
        this.RegistEvent<CreateCharacterResultEvent>(OnCreateCharacterResult);
        net.SendCreateCharacter(PD);
    }

    private void OnCreateCharacterResult(object data)
    {
        this.UnRegistEvent<CreateCharacterResultEvent>(OnCreateCharacterResult);
        if (agreeBtn != null)
        {
            agreeBtn.interactable = true;
        }

        CreateCharacterResMsg res = data as CreateCharacterResMsg;
        if (res == null)
        {
            uiSystem.OpenPanel<TipPanel>("创建角色回包异常");
            return;
        }

        if (!res.ok)
        {
            uiSystem.OpenPanel<TipPanel>(res.message);
            return;
        }

        PlayerData created = CharacterDataConverter.ToPlayerData(res.character);
        if (created != null)
        {
            uiSystem.AddPlayerData(created);
            uiSystem.SetChoiePlayerData(created);
        }

        uiSystem.OpenPanel<TipPanel>("创建成功");
        OnClose();
        uiSystem.OpenPanel<ChoicePanel>();
    }

    /// <summary>形象区每行一般在 Select_Field 子物体下找箭头</summary>
    private Transform GetSelectField(Transform row)
    {
        Transform selectField = DeepFindTransform.DeepFindChild(row, "Select_Field");
        return selectField != null ? selectField : row;
    }

    /// <summary>找行中间显示 01、02 那种数字的 Text</summary>
    private Text FindNumberText(Transform root)
    {
        Text[] texts = root.GetComponentsInChildren<Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            string v = texts[i].text != null ? texts[i].text.Trim() : "";
            // 跳过标题类文字（Face Type、Hair Style 等）
            if (v.Contains("Color") || v.Contains("Type") || v.Contains("Style") || v.Contains("Sex"))
            {
                continue;
            }
            if (v.Length <= 4)
            {
                return texts[i];
            }
        }
        return texts.Length > 0 ? texts[texts.Length - 1] : null;
    }

    /// <summary>给左右箭头 Button 绑点击事件</summary>
    private void BindArrow(Transform root, string arrowName, UnityEngine.Events.UnityAction onClick)
    {
        Transform arrow = DeepFindTransform.DeepFindChild(root, arrowName);
        if (arrow == null) return;

        Button btn = arrow.GetComponent<Button>();
        if (btn == null) return;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(onClick);
    }

    /// <summary>
    /// 数字在 min~max 之间循环
    /// 例：max=3 时，3+1→1，1-1→3
    /// </summary>
    private int WrapNumber(int value, int min, int max)
    {
        int count = max - min + 1;
        int offset = (value - min) % count;
        if (offset < 0) offset += count;
        return min + offset;
    }
}
