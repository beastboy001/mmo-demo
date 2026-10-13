using System.Collections.Generic;

//*****************************************
//功能说明：职业「翻译表」
//
// 把 ROLE 枚举翻译成三样东西：
//   1. ArmorId     → 模型里开哪个盔甲（战士=3 → Armor_003）
//   2. ButtonName  → UI 按钮物体名（Warrior、Mage…）
//   3. DisplayName / Description → 左上角中文名和简介
//
// 对照策划表「职业.xlsx」
// 改某个职业穿哪套甲：只改对应 ArmorId，不用动 prefab
//*****************************************
public static class RoleVisualCatalog
{
    /// <summary>一个职业的全部外观和文案信息</summary>
    public struct RoleVisualInfo
    {
        public ROLE Role;           // 职业枚举（代码里用）
        public int ArmorId;         // P09 模型节点 Armor_00X 的数字部分
        public string DisplayName;  // UI 显示的中文名
        public string Description;  // UI 显示的职业简介
        public string ButtonName;   // 创建面板里按钮 GameObject 的名字
    }

    // 8 个职业的映射表（新增/修改职业主要改这里）
    private static readonly Dictionary<ROLE, RoleVisualInfo> Map = new Dictionary<ROLE, RoleVisualInfo>
    {
        {
            ROLE.WARRIOR, new RoleVisualInfo
            {
                Role = ROLE.WARRIOR,
                ArmorId = 3,              // → 开 Armor_003
                ButtonName = "Warrior",   // → 找名叫 Warrior 的按钮
                DisplayName = "战士",
                Description = "身披重甲、手持利刃，是战场上最坚实的壁垒。他们以钢铁意志冲锋陷阵，用身躯为同伴挡住伤害，在刀光剑影中诠释「不退半步」的誓言。"
            }
        },
        {
            ROLE.ALCHEMIST, new RoleVisualInfo
            {
                Role = ROLE.ALCHEMIST,
                ArmorId = 9,
                ButtonName = "Alchemist",
                DisplayName = "炼金师",
                Description = "精通药剂与奇异反应的智者。用瓶中之物扭转战局，既能治愈同伴，也能让敌人在毒雾与爆炸中溃散。"
            }
        },
        {
            ROLE.MARKSMAN, new RoleVisualInfo
            {
                Role = ROLE.MARKSMAN,
                ArmorId = 4,
                ButtonName = "Marksman",
                DisplayName = "射手",
                Description = "远距精准打击的猎手。弩机与火器交织，在敌人靠近之前就结束战斗。"
            }
        },
        {
            ROLE.MAGE, new RoleVisualInfo
            {
                Role = ROLE.MAGE,
                ArmorId = 8,
                ButtonName = "Mage",
                DisplayName = "法师",
                Description = "操控奥术与元素的施法者。烛火与书卷是他们的武器，一念之间便能改写战场。"
            }
        },
        {
            ROLE.ROGUE, new RoleVisualInfo
            {
                Role = ROLE.ROGUE,
                ArmorId = 12,
                ButtonName = "Rogue",
                DisplayName = "潜行者",
                Description = "隐于阴影的刺客型游侠。双匕与潜行姿态让他们总能出现在最意想不到的位置。"
            }
        },
        {
            ROLE.PALADIN, new RoleVisualInfo
            {
                Role = ROLE.PALADIN,
                ArmorId = 6,
                ButtonName = "Paladin",
                DisplayName = "圣骑士",
                Description = "信念与利剑并重的守护者。圣光加持下，他们既是盾墙，也是冲破黑暗的锋刃。"
            }
        },
        {
            ROLE.ASSASSIN, new RoleVisualInfo
            {
                Role = ROLE.ASSASSIN,
                ArmorId = 11,
                ButtonName = "Assassin",
                DisplayName = "刺客",
                Description = "戴着假面的致命一击者。匕首只为关键一击而存在，战场上的他们来去无声。"
            }
        },
        {
            ROLE.ENGINEER, new RoleVisualInfo
            {
                Role = ROLE.ENGINEER,
                ArmorId = 7,
                ButtonName = "Engineer",
                DisplayName = "工程师",
                Description = "齿轮与机关的大师。用机械造物改变地形与火力，把战场变成自己的工坊。"
            }
        },
    };

    // ========== 对外查询 ==========

    /// <summary>
    /// 按职业取配置
    /// 用法：RoleVisualCatalog.Get(ROLE.WARRIOR).ArmorId → 3
    /// 找不到时默认返回战士
    /// </summary>
    public static RoleVisualInfo Get(ROLE role)
    {
        if (Map.TryGetValue(role, out RoleVisualInfo info))
        {
            return info;
        }
        return Map[ROLE.WARRIOR];
    }

    /// <summary>
    /// 用 UI 按钮名字反查职业
    /// 例：TryGetByButtonName("Mage", out info) → ROLE.MAGE
    /// </summary>
    public static bool TryGetByButtonName(string buttonName, out RoleVisualInfo info)
    {
        foreach (var kv in Map)
        {
            if (kv.Value.ButtonName == buttonName)
            {
                info = kv.Value;
                return true;
            }
        }
        info = default;
        return false;
    }

    /// <summary>
    /// 遍历全部 8 个职业
    /// CharacterCreatePanel.BindRoleButtons 用来逐个绑按钮
    /// </summary>
    public static IEnumerable<RoleVisualInfo> All()
    {
        return Map.Values;
    }
}
