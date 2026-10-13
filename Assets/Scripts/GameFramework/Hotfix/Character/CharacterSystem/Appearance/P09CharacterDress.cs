using System.Text.RegularExpressions;
using P09.Modular.Humanoid.Data;
using UnityEngine;

//*****************************************
//功能说明：P09 角色换装（挂在 P09_Human 预制体上）
//
// 对外只有一个方法：Apply(PlayerData pd)
// 对标魔兽世界项目的 ChoiceCharacterDress.SetMaterial(pd)
//
// 改法分三种：
//   SetActive  → 性别、职业盔甲、脸型、发型
//   换材质     → 发色、肤色、眼色
//   播动画     → 表情
//*****************************************
public class P09CharacterDress : MonoBehaviour
{
    // 合法范围（和 PlayerData 里的 Id 对应）
    public const int FaceMin = 1;
    public const int FaceMax = 3;   // 脸型 01~03
    public const int HairMin = 1;
    public const int HairMax = 14;  // 发型 01~13；14 = 光头 SkinHead

    [Header("可不填，启动时会自动在 Mesh 下查找")]
    public Transform maleRoot;    // Mesh/Male
    public Transform femaleRoot;  // Mesh/Female
    public Transform nakedBody;   // Mesh/Nakid_Body（穿甲时关掉）

    private Animator animator;    // 播表情用

    // ========== 对外唯一入口 ==========

    /// <summary>
    /// 根据 PlayerData 刷新整套外观
    /// 创建界面、进游戏生成角色，都调这一个方法
    /// </summary>
    public void Apply(PlayerData pd)
    {
        if (pd == null) return;

        P09AppearanceDataProvider.EnsureLoaded();
        FindMeshRoots();

        // 防止 Id 越界
        pd.faceTypeId = Mathf.Clamp(pd.faceTypeId, FaceMin, FaceMax);
        pd.hairStyleId = Mathf.Clamp(pd.hairStyleId, HairMin, HairMax);

        SetGender(pd);      // 1. 开男或女
        SetArmor(pd);       // 2. 开职业盔甲 Armor_00X
        SetFace(pd);        // 3. 开脸型
        SetHair(pd);        // 4. 开发型 / 光头
        SetHairColor(pd);   // 5. 换发色材质
        SetSkinColor(pd);   // 6. 换肤色材质
        SetEyeColor(pd);    // 7. 换眼色材质
        SetFaceEmotion(pd); // 8. 播表情动画
    }

    // ========== 1. 性别：开关 Male / Female ==========

    private void SetGender(PlayerData pd)
    {
        if (maleRoot != null)
        {
            maleRoot.gameObject.SetActive(pd.gender == GENDER.MALE);
        }
        if (femaleRoot != null)
        {
            femaleRoot.gameObject.SetActive(pd.gender == GENDER.FEMALE);
        }
        if (nakedBody != null)
        {
            nakedBody.gameObject.SetActive(false); // 穿甲状态不显示裸体层
        }
    }

    // ========== 2. 职业盔甲：只开一个 Armor_00X ==========

    private void SetArmor(PlayerData pd)
    {
        Transform genderRoot = pd.gender == GENDER.MALE ? maleRoot : femaleRoot;
        if (genderRoot == null) return;

        // 重要：Body 必须保持开启，头在 Body 子节点下，关了会没头
        Transform body = genderRoot.Find("Body");
        if (body != null)
        {
            body.gameObject.SetActive(true);
        }

        // 查 RoleVisualCatalog：战士 → ArmorId=3 → 拼成 "Armor_003"
        int armorId = RoleVisualCatalog.Get(pd.role).ArmorId;
        string openName = "Armor_" + armorId.ToString("000");

        // 遍历该性别下所有 Armor_*，只开目标那一套
        for (int i = 0; i < genderRoot.childCount; i++)
        {
            Transform child = genderRoot.GetChild(i);
            if (!child.name.StartsWith("Armor_")) continue;
            child.gameObject.SetActive(child.name == openName);
        }
    }

    // ========== 3. 脸型：开 Male_Face_01 或 Female_Face_02 等 ==========

    private void SetFace(PlayerData pd)
    {
        string prefix = pd.gender == GENDER.MALE ? "Male" : "Female";
        string openName = prefix + "_Face_" + pd.faceTypeId.ToString("00");

        string[] allFaces =
        {
            "Male_Face_01", "Male_Face_02", "Male_Face_03",
            "Female_Face_01", "Female_Face_02", "Female_Face_03"
        };

        SetOnlyOneActive(allFaces, openName);
    }

    // ========== 4. 发型：开 Hair_0X；14 时开 SkinHead ==========

    private void SetHair(PlayerData pd)
    {
        bool bald = pd.hairStyleId >= HairMax; // 14 = 光头
        string openHairName = bald ? "" : "Hair_" + pd.hairStyleId.ToString("00");

        // 关/开 Hair_01 ~ Hair_13
        for (int i = HairMin; i <= 13; i++)
        {
            string hairName = "Hair_" + i.ToString("00");
            Transform hair = DeepFindTransform.DeepFindChild(transform, hairName);
            if (hair != null)
            {
                hair.gameObject.SetActive(!bald && hairName == openHairName);
            }
        }

        // 光头：开 SkinHead_Male 或 SkinHead_Female
        Transform maleBald = DeepFindTransform.DeepFindChild(transform, "SkinHead_Male");
        Transform femaleBald = DeepFindTransform.DeepFindChild(transform, "SkinHead_Female");
        if (maleBald != null)
        {
            maleBald.gameObject.SetActive(bald && pd.gender == GENDER.MALE);
        }
        if (femaleBald != null)
        {
            femaleBald.gameObject.SetActive(bald && pd.gender == GENDER.FEMALE);
        }
    }

    // ========== 5. 发色：给当前发型换材质（光头跳过） ==========

    private void SetHairColor(PlayerData pd)
    {
        if (pd.hairStyleId >= HairMax) return;

        HairColorEditPartData colorData = P09AppearanceDataProvider.GetHairColor(pd.hairColorId);
        if (colorData == null) return;

        // 有的发型有单独材质，按 hairStyleId 取
        Material mat = colorData.GetMaterial(pd.hairStyleId);
        if (mat == null) return;

        string hairName = "Hair_" + pd.hairStyleId.ToString("00");
        Transform hairRoot = DeepFindTransform.DeepFindChild(transform, hairName);
        if (hairRoot == null) return;

        Renderer[] renderers = hairRoot.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material = mat;
        }
    }

    // ========== 6. 肤色：替换名字含 Skin 的材质 ==========

    private void SetSkinColor(PlayerData pd)
    {
        ColorEditPartData colorData = P09AppearanceDataProvider.GetSkinColor(pd.gender, pd.skinId);
        if (colorData == null || colorData.Material == null) return;

        ReplaceMaterialByName(colorData.Material, @"^P09_.*_Skin.*$");
    }

    // ========== 7. 眼色：替换名字含 Eye 的材质 ==========

    private void SetEyeColor(PlayerData pd)
    {
        ColorEditPartData colorData = P09AppearanceDataProvider.GetEyeColor(pd.eyeColorId);
        if (colorData == null || colorData.Material == null) return;

        ReplaceMaterialByName(colorData.Material, @"^P09_Eye.*$");
    }

    // ========== 8. 表情：挂 ModelPlayer，在 Face Layer 播表情 ==========

    private void SetFaceEmotion(PlayerData pd)
    {
        // 游戏玩家走 PlayerLocomotion，预览才用 ModelPlayer 播表情
        if (GetComponent<PlayerMovementController>() != null)
        {
            return;
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }
        if (animator == null) return;

        // 打包后常见问题：预览模型不在主相机视野里，CullUpdateTransforms 会停动画
        PreparePreviewAnimator(animator);

        // 先挂 ModelPlayer（含 Idle），再播表情；避免表情数据缺失时整段 return 导致站桩
        RuntimeAnimatorController modelPlayer = P09AppearanceDataProvider.GetModelPlayerController();
        if (modelPlayer != null)
        {
            animator.applyRootMotion = false;
            animator.runtimeAnimatorController = modelPlayer;
        }

        int faceLayer = animator.GetLayerIndex("Face Layer");
        if (faceLayer < 0)
        {
            faceLayer = 0;
        }

        animator.SetLayerWeight(faceLayer, 1f);

        FaceEmotionData emotionData = P09AppearanceDataProvider.GetFaceEmotion(pd.faceEmotionId);
        if (emotionData != null && !string.IsNullOrEmpty(emotionData.AnimationName))
        {
            animator.Play(emotionData.AnimationName, faceLayer, 0f);
        }

        animator.Update(0f);
    }

    /// <summary>选角/创角预览：强制播动画（不受相机裁剪影响）</summary>
    public static void PreparePreviewAnimator(Animator anim)
    {
        if (anim == null)
        {
            return;
        }

        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        anim.updateMode = AnimatorUpdateMode.Normal;

        SkinnedMeshRenderer[] skins = anim.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < skins.Length; i++)
        {
            if (skins[i] != null)
            {
                skins[i].updateWhenOffscreen = true;
            }
        }
    }

    // ========== 工具方法 ==========

    /// <summary>自动查找 Mesh/Male、Mesh/Female、Nakid_Body</summary>
    private void FindMeshRoots()
    {
        if (maleRoot != null && femaleRoot != null) return;

        Transform mesh = DeepFindTransform.DeepFindChild(transform, "Mesh");
        if (mesh == null) mesh = transform;

        if (maleRoot == null) maleRoot = DeepFindTransform.DeepFindChild(mesh, "Male");
        if (femaleRoot == null) femaleRoot = DeepFindTransform.DeepFindChild(mesh, "Female");
        if (nakedBody == null) nakedBody = DeepFindTransform.DeepFindChild(mesh, "Nakid_Body");
    }

    /// <summary>一组名字里只激活 openName，其余关掉</summary>
    private void SetOnlyOneActive(string[] names, string openName)
    {
        for (int i = 0; i < names.Length; i++)
        {
            Transform t = DeepFindTransform.DeepFindChild(transform, names[i]);
            if (t != null)
            {
                t.gameObject.SetActive(t.name == openName);
            }
        }
    }

    /// <summary>
    /// 遍历全身 Renderer，把材质名匹配 namePattern 的 slot 换成 newMat
    /// 用于肤色、眼色批量替换
    /// </summary>
    private void ReplaceMaterialByName(Material newMat, string namePattern)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int r = 0; r < renderers.Length; r++)
        {
            Material[] mats = renderers[r].materials;
            bool changed = false;

            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null && Regex.IsMatch(mats[i].name, namePattern))
                {
                    mats[i] = newMat;
                    changed = true;
                }
            }

            if (changed)
            {
                renderers[r].materials = mats;
            }
        }
    }
}
