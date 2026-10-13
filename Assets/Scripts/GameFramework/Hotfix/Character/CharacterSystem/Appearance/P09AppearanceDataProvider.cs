using System.Collections.Generic;
using System.Linq;
using P09.Modular.Humanoid.Data;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

//*****************************************
//功能说明：P09 发色 / 肤色 / 眼色 / 表情的「资源字典」
//
// Editor：可从 Demo 原路径或 Resources 加载
// 打包：必须从 Resources/P09Appearance 加载（AssetDatabase 在 Player 不可用）
//*****************************************
public static class P09AppearanceDataProvider
{
    private const string DemoSoRoot =
        "Assets/P09_Modular_Humanoid/Scenes/DemoScene_Data/ScriptableObject/";

    private const string ResourcesSoRoot = "P09Appearance/";

    /// <summary>男女共用的角色 Animator Controller（Resources/Animation/ModelPlayer）</summary>
    public const string ModelPlayerResPath = "Animation/ModelPlayer";

    private static bool initialized;

    private static List<HairColorEditPartData> hairColors;
    private static List<ColorEditPartData> eyeColors;
    private static List<ColorEditPartData> skinMale;
    private static List<ColorEditPartData> skinFemale;
    private static List<FaceEmotionData> faceEmotions;

    private static RuntimeAnimatorController modelPlayerController;

    public static IReadOnlyList<HairColorEditPartData> HairColors => hairColors;
    public static IReadOnlyList<ColorEditPartData> EyeColors => eyeColors;
    public static IReadOnlyList<FaceEmotionData> FaceEmotions => faceEmotions;

    public static void EnsureLoaded()
    {
        if (initialized)
        {
            return;
        }

        hairColors = LoadHairColors();
        eyeColors = LoadColorParts("EditPartDataContainer_EyeColor");
        skinMale = LoadColorParts("EditPartDataContainer_SkinColor_Male");
        skinFemale = LoadColorParts("EditPartDataContainer_SkinColor_Famale");
        faceEmotions = LoadFaceEmotions();

        modelPlayerController = GameResSystem.GetRes<RuntimeAnimatorController>(ModelPlayerResPath);
        if (modelPlayerController == null)
        {
            Debug.LogError("[P09AppearanceDataProvider] 找不到 ModelPlayer 控制器: " + ModelPlayerResPath);
        }

        if (hairColors.Count == 0 || faceEmotions.Count == 0)
        {
            Debug.LogWarning(
                "[P09AppearanceDataProvider] 外观配置为空。请确认 Resources/P09Appearance 下有对应 .asset");
        }

        initialized = true;
    }

    public static int GetHairColorMaxId()
    {
        EnsureLoaded();
        return hairColors.Count > 0 ? hairColors.Max(d => d.ContentId) : 1;
    }

    public static int GetSkinMaxId(GENDER gender)
    {
        EnsureLoaded();
        var list = GetSkinColors(gender);
        return list.Count > 0 ? list.Max(d => d.ContentId) : 1;
    }

    public static int GetEyeColorMaxId()
    {
        EnsureLoaded();
        return eyeColors.Count > 0 ? eyeColors.Max(d => d.ContentId) : 1;
    }

    public static int GetFaceEmotionMaxId()
    {
        EnsureLoaded();
        return faceEmotions.Count > 0 ? faceEmotions.Max(d => d.ContentId) : 1;
    }

    public static IReadOnlyList<ColorEditPartData> GetSkinColors(GENDER gender)
    {
        EnsureLoaded();
        return gender == GENDER.MALE ? skinMale : skinFemale;
    }

    public static HairColorEditPartData GetHairColor(int id)
    {
        EnsureLoaded();
        return hairColors.FirstOrDefault(d => d.ContentId == id);
    }

    public static ColorEditPartData GetSkinColor(GENDER gender, int id)
    {
        EnsureLoaded();
        return GetSkinColors(gender).FirstOrDefault(d => d.ContentId == id);
    }

    public static ColorEditPartData GetEyeColor(int id)
    {
        EnsureLoaded();
        return eyeColors.FirstOrDefault(d => d.ContentId == id);
    }

    public static FaceEmotionData GetFaceEmotion(int id)
    {
        EnsureLoaded();
        return faceEmotions.FirstOrDefault(d => d.ContentId == id);
    }

    public static RuntimeAnimatorController GetFacialController(GENDER gender)
    {
        EnsureLoaded();
        return modelPlayerController;
    }

    public static RuntimeAnimatorController GetModelPlayerController()
    {
        EnsureLoaded();
        return modelPlayerController;
    }

    public static Color GetPreviewColor(Material mat)
    {
        if (mat == null) return Color.white;
        if (mat.HasProperty("_Color")) return mat.color;
        if (mat.HasProperty("_BaseColor")) return mat.GetColor("_BaseColor");
        return Color.white;
    }

    private static List<HairColorEditPartData> LoadHairColors()
    {
        EditPartDataContainer c = LoadContainer("EditPartDataContainer_HairColor");
        return c?.PartDataList.OfType<HairColorEditPartData>().OrderBy(d => d.ContentId).ToList()
               ?? new List<HairColorEditPartData>();
    }

    private static List<ColorEditPartData> LoadColorParts(string assetName)
    {
        EditPartDataContainer c = LoadContainer(assetName);
        return c?.PartDataList.OfType<ColorEditPartData>().OrderBy(d => d.ContentId).ToList()
               ?? new List<ColorEditPartData>();
    }

    private static List<FaceEmotionData> LoadFaceEmotions()
    {
        EditPartDataContainer c = LoadContainer("EditPartDataContainer_FaceEmotion");
        return c?.PartDataList.OfType<FaceEmotionData>().OrderBy(d => d.ContentId).ToList()
               ?? new List<FaceEmotionData>();
    }

    /// <summary>
    /// 优先 Resources（打包可用）；Editor 下若 Resources 没有再回退 Demo 原路径
    /// </summary>
    private static EditPartDataContainer LoadContainer(string assetNameNoExt)
    {
        EditPartDataContainer fromRes =
            Resources.Load<EditPartDataContainer>(ResourcesSoRoot + assetNameNoExt);
        if (fromRes != null)
        {
            return fromRes;
        }

#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<EditPartDataContainer>(
            DemoSoRoot + assetNameNoExt + ".asset");
#else
        Debug.LogWarning("[P09AppearanceDataProvider] Resources 缺少: " + ResourcesSoRoot + assetNameNoExt);
        return null;
#endif
    }
}
