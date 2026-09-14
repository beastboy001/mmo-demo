using System;
using System.Collections.Generic;
using UnityEngine;

//*****************************************
//功能说明：附近可交互选项的「管家」（单例，DontDestroyOnLoad）
//
// 职责：
//   1. 维护当前附近有哪些 InteractionOptionData
//   2. 滚轮切换选中项
//   3. 检测确认键，执行选中项的 onExecute
//   4. 对话进行中（dialogueBusy）不再处理列表输入
//
// 不负责画 UI —— UI 由 GamePanel 订阅 OnListChanged / OnSelectionChanged 来刷新
//*****************************************
public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    // 当前附近所有可交互选项
    private readonly List<InteractionOptionData> options = new List<InteractionOptionData>();

    // 当前高亮的那一条（滚轮改这个）
    private int selectedIndex;

    // true = 正在对话，列表输入全部忽略
    private bool dialogueBusy;

    // true = 社交面板打开时，同样屏蔽交互列表输入
    private bool socialBusy;

    public IReadOnlyList<InteractionOptionData> Options => options;
    public int SelectedIndex => selectedIndex;
    public bool IsDialogueBusy => dialogueBusy;
    public bool IsSocialBusy => socialBusy;
    public bool IsInteractionBlocked => dialogueBusy || socialBusy;

    /// <summary>列表增删时触发（GamePanel 会整表重建 UI）</summary>
    public event Action OnListChanged;

    /// <summary>只改选中下标时触发（GamePanel 只改选中样式）</summary>
    public event Action OnSelectionChanged;

    /// <summary>没有实例就创建一个，保证随时能用</summary>
    public static InteractionManager EnsureExists()
    {
        if (Instance != null)
        {
            return Instance;
        }

        GameObject go = new GameObject("InteractionManager");
        DontDestroyOnLoad(go);
        return go.AddComponent<InteractionManager>();
    }

    private void Awake()
    {
        // 保证全局只有一个
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>进入/退出对话时由 GamePanel 调用</summary>
    public void SetDialogueBusy(bool busy)
    {
        dialogueBusy = busy;
        OnListChanged?.Invoke();
    }

    /// <summary>打开/关闭玩家互动、功能面板等时由 SocialUiController 调用</summary>
    public void SetSocialBusy(bool busy)
    {
        socialBusy = busy;
        OnListChanged?.Invoke();
    }

    /// <summary>NPC 走进范围时登记一条选项</summary>
    public void Register(InteractionOptionData data)
    {
        if (data == null)
        {
            return;
        }

        options.Add(data);
        if (selectedIndex < 0 || selectedIndex >= options.Count)
        {
            selectedIndex = 0;
        }

        OnListChanged?.Invoke();
    }

    /// <summary>按 owner 移除（NPC 离开范围时，把属于它的选项全删掉）</summary>
    public void UnregisterOwner(object owner)
    {
        if (owner == null)
        {
            return;
        }

        bool removed = false;
        for (int i = options.Count - 1; i >= 0; i--)
        {
            if (options[i] != null && ReferenceEquals(options[i].owner, owner))
            {
                options.RemoveAt(i);
                removed = true;
            }
        }

        if (!removed)
        {
            return;
        }

        // 删完后把选中下标夹回合法范围
        if (options.Count == 0)
        {
            selectedIndex = 0;
        }
        else
        {
            selectedIndex = Mathf.Clamp(selectedIndex, 0, options.Count - 1);
        }

        OnListChanged?.Invoke();
    }

    private void Update()
    {
        // 对话 / 社交面板中，或附近没人可交互 → 什么都不做
        if (IsInteractionBlocked || options.Count == 0)
        {
            return;
        }

        // ----- 滚轮切换选中 -----
        // 上滚 = 上一项，下滚 = 下一项，到头循环
        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0.01f)
        {
            selectedIndex--;
            if (selectedIndex < 0)
            {
                selectedIndex = options.Count - 1;
            }

            OnSelectionChanged?.Invoke();
        }
        else if (scroll < -0.01f)
        {
            selectedIndex++;
            if (selectedIndex >= options.Count)
            {
                selectedIndex = 0;
            }

            OnSelectionChanged?.Invoke();
        }

        // ----- 确认键：执行当前选中项 -----
        InteractionOptionData current = GetSelected();
        if (current != null && Input.GetKeyDown(current.key))
        {
            current.onExecute?.Invoke();
        }
    }

    /// <summary>取当前选中的那条数据，没有则 null</summary>
    public InteractionOptionData GetSelected()
    {
        if (selectedIndex < 0 || selectedIndex >= options.Count)
        {
            return null;
        }

        return options[selectedIndex];
    }
}
