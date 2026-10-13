using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//*****************************************
//功能说明：游戏内主面板
//   · HUD（血条、等级）
//   · 左侧交互列表（Interaction_images）
//   · 对话面板（DialogBox）
//
// 和逻辑层的关系：
//   InteractionManager  ←→  订阅事件刷交互列表
//   DialogueRunner      ←→  Start / TryAdvance / Choose / CloseDialogue
//   GameplayControlLock ←→  对话时锁移动镜头
//   ICursorSystem       ←→  对话时显示鼠标
//*****************************************
public class GamePanel : BasePanel
{
    private const int DefaultMaxHp = 100;

    // Resources 下的预制体路径（不要带 .prefab 后缀）
    private const string InteractionOptionPath = "Prefabs/UI/GamePanel/InteractionOption";
    private const string DialogOptionPath = "Prefabs/UI/GamePanel/DialogOption";

    // ----- HUD -----
    private Slider hpSlider;
    private Text hpText;
    private Text levelText;

    // ----- 交互列表 UI -----
    private GameObject interactionRoot;   // Interaction_images（整块显隐）
    private Transform interactionContent; // 动态生成 InteractionOption 的父节点

    // ----- 对话 UI -----
    private GameObject dialogBox;
    private Text npcNameText;
    private Text npcProfessionText;
    private Text dialogContentText;
    private Transform dialogOptionsRoot;  // 动态生成 DialogOption 的父节点
    private Button clickAreaButton;       // 点空白推进对话

    private GameObject interactionOptionPrefab;
    private GameObject dialogOptionPrefab;

    // 当前已经生成的左侧选项 View，方便只改选中样式
    private readonly List<InteractionOptionView> optionViews = new List<InteractionOptionView>();

    // 对话逻辑（纯 C#）
    private readonly DialogueRunner dialogueRunner = new DialogueRunner();

    // 是否已经为这次对话调用过 ShowForUI（退出时要对齐 HideForUI）
    private bool cursorHeldForDialog;

    public override void OnInit()
    {
        BindHud();
        BindInteractionUi();
        BindDialogUi();

        if (GetComponent<SocialUiController>() == null)
        {
            gameObject.AddComponent<SocialUiController>();
        }

        // 确保交互管家存在，并加载两条选项预制体
        InteractionManager.EnsureExists();
        interactionOptionPrefab = Resources.Load<GameObject>(InteractionOptionPath);
        dialogOptionPrefab = Resources.Load<GameObject>(DialogOptionPath);

        // 清掉预制体里可能残留的示例子物体，默认先藏起来
        ClearChildren(interactionContent);
        ClearChildren(dialogOptionsRoot);
        if (dialogBox != null)
        {
            dialogBox.SetActive(false);
        }

        if (interactionRoot != null)
        {
            interactionRoot.SetActive(false);
        }

        // 对话逻辑 → UI 刷新 / 关闭
        dialogueRunner.OnNodeChanged += RefreshDialogNode;
        dialogueRunner.OnEnded += CloseDialogue;

        base.OnInit();
    }

    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);
        RefreshHud();
        SubscribeInteraction();
        RefreshInteractionList();
    }

    public override void OnClose()
    {
        UnsubscribeInteraction();
        // 面板关掉时如果还在对话，强制结束，避免锁住操作
        if (dialogueRunner.IsRunning)
        {
            dialogueRunner.End();
        }

        base.OnClose();
    }

    private void OnDestroy()
    {
        UnsubscribeInteraction();
        dialogueRunner.OnNodeChanged -= RefreshDialogNode;
        dialogueRunner.OnEnded -= CloseDialogue;
    }

    /// <summary>
    /// 开始对话（由 NpcInteractable.StartTalk 调用）
    /// 顺序：标记忙碌 → 关列表/HUD → 显示鼠标 → 锁操作 → 开 DialogBox → Runner.Start
    /// </summary>
    public void StartDialogue(DialogueAsset asset, string npcName, string npcProfession)
    {
        if (dialogueRunner.IsRunning)
        {
            return;
        }

        // 对话期间不要再响应滚轮 / 确认键
        InteractionManager mgr = InteractionManager.Instance;
        if (mgr != null)
        {
            mgr.SetDialogueBusy(true);
        }

        if (interactionRoot != null)
        {
            interactionRoot.SetActive(false);
        }

        if (hpSlider != null)
        {
            hpSlider.gameObject.SetActive(false);
        }

        RequestDialogCursor();
        GameplayControlLock.SetLocked(true);

        if (dialogBox != null)
        {
            dialogBox.SetActive(true);
        }

        dialogueRunner.Start(asset, npcName, npcProfession);
    }

    /// <summary>根据当前角色数据刷新血条 / 等级</summary>
    public void RefreshHud()
    {
        if (uiSystem == null)
        {
            return;
        }

        PlayerData pd = uiSystem.ChoiePlayerData;
        if (pd == null)
        {
            return;
        }

        int maxHp = DefaultMaxHp;
        float hpPercent = maxHp > 0 ? Mathf.Clamp01((float)pd.hp / maxHp) : 0f;

        if (hpSlider != null)
        {
            hpSlider.value = hpPercent;
        }

        if (hpText != null)
        {
            hpText.text = pd.hp + "/" + maxHp;
        }

        if (levelText != null)
        {
            levelText.text = "Lv." + pd.level;
        }
    }

    // ==================== 绑定预制体节点 ====================

    private void BindHud()
    {
        Transform hpSliderTrans = transform.Find("HP_Slider");
        if (hpSliderTrans == null)
        {
            return;
        }

        hpSlider = hpSliderTrans.GetComponent<Slider>();
        Transform hpTextTrans = hpSliderTrans.Find("HP_Text");
        if (hpTextTrans != null)
        {
            hpText = hpTextTrans.GetComponent<Text>();
        }

        Transform levelTrans = hpSliderTrans.Find("Level");
        if (levelTrans != null)
        {
            levelText = levelTrans.GetComponent<Text>();
        }
    }

    private void BindInteractionUi()
    {
        Transform root = transform.Find("Interaction_images");
        if (root != null)
        {
            interactionRoot = root.gameObject;
            // 结构：Interaction_images / Viewport / PlayorNpcInteractionContent
            interactionContent = root.Find("Viewport/PlayorNpcInteractionContent");
            if (interactionContent == null)
            {
                interactionContent = DeepFindTransform.DeepFindChild(root, "PlayorNpcInteractionContent");
            }
        }
    }

    private void BindDialogUi()
    {
        Transform box = transform.Find("DialogBox");
        if (box == null)
        {
            return;
        }

        dialogBox = box.gameObject;
        Transform info = box.Find("DialogInfo");
        if (info != null)
        {
            Transform nameT = info.Find("NpcName");
            if (nameT != null)
            {
                npcNameText = nameT.GetComponent<Text>();
            }

            Transform jobT = info.Find("NpcProfession");
            if (jobT != null)
            {
                npcProfessionText = jobT.GetComponent<Text>();
            }
        }

        Transform contentT = box.Find("DialogContent");
        if (contentT != null)
        {
            dialogContentText = contentT.GetComponent<Text>();
        }

        dialogOptionsRoot = box.Find("DialogOptions");
        EnsureClickArea(box);
    }

    /// <summary>
    /// ClickArea 预制体里可能只有 RectTransform，这里补透明 Image + Button
    /// 点击 → DialogueRunner.TryAdvance
    /// </summary>
    private void EnsureClickArea(Transform dialogBoxTrans)
    {
        Transform click = dialogBoxTrans.Find("ClickArea");
        if (click == null)
        {
            return;
        }

        Image img = click.GetComponent<Image>();
        if (img == null)
        {
            img = click.gameObject.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0f); // 全透明，只用来接收点击
        }

        img.raycastTarget = true;

        clickAreaButton = click.GetComponent<Button>();
        if (clickAreaButton == null)
        {
            clickAreaButton = click.gameObject.AddComponent<Button>();
            clickAreaButton.transition = Selectable.Transition.None;
        }

        clickAreaButton.targetGraphic = img;
        clickAreaButton.onClick.RemoveAllListeners();
        clickAreaButton.onClick.AddListener(() => dialogueRunner.TryAdvance());
    }

    // ==================== 交互列表 ====================

    /// <summary>订阅管家事件（先减再加，避免重复订阅）</summary>
    private void SubscribeInteraction()
    {
        InteractionManager mgr = InteractionManager.EnsureExists();
        mgr.OnListChanged -= RefreshInteractionList;
        mgr.OnSelectionChanged -= RefreshInteractionSelection;
        mgr.OnListChanged += RefreshInteractionList;
        mgr.OnSelectionChanged += RefreshInteractionSelection;
    }

    private void UnsubscribeInteraction()
    {
        if (InteractionManager.Instance == null)
        {
            return;
        }

        InteractionManager.Instance.OnListChanged -= RefreshInteractionList;
        InteractionManager.Instance.OnSelectionChanged -= RefreshInteractionSelection;
    }

    /// <summary>列表有增删：清空后按 Options 重新生成 InteractionOption</summary>
    private void RefreshInteractionList()
    {
        if (interactionContent == null || interactionOptionPrefab == null)
        {
            return;
        }

        InteractionManager mgr = InteractionManager.Instance;
        if (mgr == null)
        {
            return;
        }

        ClearChildren(interactionContent);
        optionViews.Clear();

        IReadOnlyList<InteractionOptionData> list = mgr.Options;
        // 有选项且没被对话/社交占用 → 显示列表
        bool show = list.Count > 0 && !mgr.IsInteractionBlocked;
        if (interactionRoot != null)
        {
            interactionRoot.SetActive(show);
        }

        if (!show)
        {
            return;
        }

        for (int i = 0; i < list.Count; i++)
        {
            GameObject go = Object.Instantiate(interactionOptionPrefab, interactionContent);
            go.name = "InteractionOption_" + i;
            InteractionOptionView view = go.GetComponent<InteractionOptionView>();
            if (view == null)
            {
                view = go.AddComponent<InteractionOptionView>();
            }

            view.Bind();
            view.SetData(list[i]);
            view.SetSelected(i == mgr.SelectedIndex);
            optionViews.Add(view);
        }
    }

    /// <summary>只改选中样式（滚轮时走这里，比重建整表更轻）</summary>
    private void RefreshInteractionSelection()
    {
        InteractionManager mgr = InteractionManager.Instance;
        if (mgr == null)
        {
            return;
        }

        for (int i = 0; i < optionViews.Count; i++)
        {
            optionViews[i].SetSelected(i == mgr.SelectedIndex);
        }
    }

    // ==================== 对话 UI ====================

    /// <summary>Runner 换句时：刷新名字/职业/正文，并生成分支按钮</summary>
    private void RefreshDialogNode()
    {
        DialogueNode node = dialogueRunner.Current;
        if (node == null)
        {
            return;
        }

        if (npcNameText != null)
        {
            npcNameText.text = dialogueRunner.NpcName;
        }

        if (npcProfessionText != null)
        {
            npcProfessionText.text = dialogueRunner.NpcProfession;
        }

        if (dialogContentText != null)
        {
            dialogContentText.text = node.content;
        }

        ClearChildren(dialogOptionsRoot);
        if (!dialogueRunner.HasChoices || dialogOptionPrefab == null || dialogOptionsRoot == null)
        {
            return;
        }

        for (int i = 0; i < node.choices.Count; i++)
        {
            DialogueChoice choice = node.choices[i];
            if (choice == null)
            {
                continue;
            }

            // 闭包要用局部变量，否则循环结束 i 会变成最后一个
            int captured = i;
            GameObject go = Object.Instantiate(dialogOptionPrefab, dialogOptionsRoot);
            go.name = "DialogOption_" + (i + 1);
            DialogOptionView view = go.GetComponent<DialogOptionView>();
            if (view == null)
            {
                view = go.AddComponent<DialogOptionView>();
            }

            view.Bind();
            view.SetData(i + 1, choice.text, () => dialogueRunner.Choose(captured));
        }
    }

    /// <summary>
    /// 对话结束：关 DialogBox → 开 HUD → 解锁 → 藏鼠标 → 恢复列表
    /// </summary>
    private void CloseDialogue()
    {
        ClearChildren(dialogOptionsRoot);
        if (dialogBox != null)
        {
            dialogBox.SetActive(false);
        }

        if (hpSlider != null)
        {
            hpSlider.gameObject.SetActive(true);
        }

        GameplayControlLock.SetLocked(false);
        ReleaseDialogCursor();

        InteractionManager mgr = InteractionManager.Instance;
        if (mgr != null)
        {
            mgr.SetDialogueBusy(false);
        }

        // 若还在 NPC 范围内，列表会再显示出来
        RefreshInteractionList();
        RefreshHud();
    }

    /// <summary>对话开始：显示鼠标（引用计数 +1）</summary>
    private void RequestDialogCursor()
    {
        if (cursorHeldForDialog || cursorSystem == null)
        {
            return;
        }

        cursorSystem.ShowForUI();
        cursorHeldForDialog = true;
    }

    /// <summary>对话结束：隐藏鼠标（引用计数 -1），要和 Show 配对</summary>
    private void ReleaseDialogCursor()
    {
        if (!cursorHeldForDialog || cursorSystem == null)
        {
            return;
        }

        cursorSystem.HideForUI();
        cursorHeldForDialog = false;
    }

    /// <summary>删掉父节点下所有子物体（清动态生成的选项）</summary>
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
