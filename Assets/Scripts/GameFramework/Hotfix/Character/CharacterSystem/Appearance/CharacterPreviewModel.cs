using UnityEngine;

//*****************************************
//功能说明：选角 / 创角共用的预览模型（全局只保留一个 P09_Human）
//
// GameObject.Find 找不到隐藏物体，所以必须用 FindObjectsOfType(true)
// 缓存实例，避免每次 Hide / Reset 都全场景搜索
//*****************************************
public static class CharacterPreviewModel
{
    public const string ModelName = "P09_Human";
    private const string PrefabPath = "Prefabs/Character/P09_Human";
    private static readonly Vector3 DefaultEuler = new Vector3(0f, 180f, 0f);

    private static GameObject cachedModel;

    public static GameObject FindExisting()
    {
        if (cachedModel != null)
        {
            return cachedModel;
        }

        GameObject active = GameObject.Find(ModelName);
        if (active != null)
        {
            DestroyDuplicates(active);
            cachedModel = active;
            return cachedModel;
        }

        P09CharacterDress[] dresses = Object.FindObjectsOfType<P09CharacterDress>(true);
        GameObject first = null;
        for (int i = 0; i < dresses.Length; i++)
        {
            GameObject go = dresses[i].gameObject;
            if (go.name != ModelName) continue;

            if (first == null)
            {
                first = go;
            }
            else
            {
                Object.Destroy(go);
            }
        }

        cachedModel = first;
        return cachedModel;
    }

    public static GameObject GetOrCreate()
    {
        GameObject model = FindExisting();
        if (model != null)
        {
            return model;
        }

        GameObject prefab = GameResSystem.GetRes<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError("[CharacterPreviewModel] 找不到预制体: " + PrefabPath);
            return null;
        }

        model = Object.Instantiate(prefab);
        model.name = ModelName;
        model.SetActive(false);
        cachedModel = model;
        return model;
    }

    /// <summary>预加载外观资源 + 预览预制体（登录/选角阶段调用）</summary>
    public static void WarmUp()
    {
        P09AppearanceDataProvider.EnsureLoaded();
        GetOrCreate();
    }

    public static P09CharacterDress GetOrCreateDress()
    {
        GameObject model = GetOrCreate();
        if (model == null) return null;

        P09CharacterDress dress = model.GetComponent<P09CharacterDress>();
        if (dress == null)
        {
            dress = model.AddComponent<P09CharacterDress>();
        }

        // 一创建就配好预览 Animator，避免打包后 Idle/表情不播
        Animator anim = model.GetComponent<Animator>();
        if (anim == null)
        {
            anim = model.GetComponentInChildren<Animator>();
        }

        P09CharacterDress.PreparePreviewAnimator(anim);

        P09AppearanceDataProvider.EnsureLoaded();
        RuntimeAnimatorController modelPlayer = P09AppearanceDataProvider.GetModelPlayerController();
        if (anim != null && modelPlayer != null)
        {
            anim.applyRootMotion = false;
            if (anim.runtimeAnimatorController != modelPlayer)
            {
                anim.runtimeAnimatorController = modelPlayer;
            }

            int faceLayer = anim.GetLayerIndex("Face Layer");
            if (faceLayer >= 0)
            {
                anim.SetLayerWeight(faceLayer, 1f);
            }
        }

        return dress;
    }

    public static void Hide()
    {
        if (cachedModel != null)
        {
            cachedModel.SetActive(false);
            return;
        }

        GameObject model = FindExisting();
        if (model != null)
        {
            model.SetActive(false);
        }
    }

    /// <summary>恢复预制体默认朝向（创角面板每次打开时调用）</summary>
    public static void ResetRotation()
    {
        GameObject model = FindExisting();
        if (model != null)
        {
            model.transform.rotation = Quaternion.Euler(DefaultEuler);
        }
    }

    private static void DestroyDuplicates(GameObject keep)
    {
        P09CharacterDress[] dresses = Object.FindObjectsOfType<P09CharacterDress>(true);
        for (int i = 0; i < dresses.Length; i++)
        {
            GameObject go = dresses[i].gameObject;
            if (go.name != ModelName || go == keep) continue;

            if (cachedModel == go)
            {
                cachedModel = null;
            }

            Object.Destroy(go);
        }
    }
}
