using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

//*****************************************
//功能说明：游戏入口。对照魔兽课 GameStarter。
// 挂到场景空物体上即可启动架构。
//*****************************************
public class GameStarter : MonoBehaviour, IController
{
    //创建一个架构启动器的变量
    private StartArchitecture startArchitecture;

    private void Awake()
    {
        //获取唯一的架构启动器
        startArchitecture = StartArchitecture.Instance;
        startArchitecture.SetGameArchitecture(new GameArchitercture());//这里new出来时自动调用了构造函数，进行了初始化
        startArchitecture.InitAllModulesInArchitecture();//这里是通过架构启动器内存入的架构来把注册到架构内的系统全部初始化
        DontDestroyOnLoad(gameObject);
        Debug.Log("[GameStarter] 架构已启动");
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
