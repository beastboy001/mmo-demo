using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

//*****************************************
//功能说明：游戏入口。对照魔兽课 GameStarter。
// 挂到场景空物体上即可启动架构。
//*****************************************
public class GameStarter : MonoBehaviour, IController
{
    private void Awake()
    {
        // 架构的创建现在交给热更层（Game.Hotfix 里的 GameBootstrap）。
        // 这里用反射调用，是为了让 AOT 程序集在编译期完全不依赖热更程序集。
        StartHotfix();

        DontDestroyOnLoad(gameObject);
        Debug.Log("[GameStarter] AOT 启动完成");
    }

    private static void StartHotfix()
    {
        Assembly hotfix = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "Game.Hotfix");

        if (hotfix == null)
        {
            Debug.LogError("[GameStarter] 找不到热更程序集 Game.Hotfix");
            return;
        }

        Type entry = hotfix.GetType("GameBootstrap");
        if (entry == null)
        {
            Debug.LogError("[GameStarter] 热更程序集里找不到入口类型 GameBootstrap");
            return;
        }

        MethodInfo start = entry.GetMethod("Start", BindingFlags.Public | BindingFlags.Static);
        if (start == null)
        {
            Debug.LogError("[GameStarter] GameBootstrap 里找不到静态方法 Start");
            return;
        }

        start.Invoke(null, null);
    }

    private void Update()
    {
        // 把网络收包派发到主线程（事件 / UI）
        this.GetSystem<INetSystem>()?.Update();
    }

    /// <summary>LoadPanel 切场景后由 DontDestroyOnLoad 对象完成卸 Login 场景</summary>
    public void FinishEnterGame(string loginSceneName)
    {
        StartCoroutine(FinishEnterGameCoroutine(loginSceneName));
    }

    private IEnumerator FinishEnterGameCoroutine(string loginSceneName)
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            canvas.SetActive(false);
        }

        Scene loginScene = SceneManager.GetSceneByName(loginSceneName);
        if (!loginScene.IsValid())
        {
            loginScene = SceneManager.GetSceneByBuildIndex(0);
        }

        if (loginScene.IsValid() && loginScene.isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync(loginScene);
        }

        this.GetSystem<IUISystem>()?.UnLoadLoginPanelsAndLoadGamePanels();
        this.GetSystem<ICursorSystem>()?.EnterGameplay();
    }
}
