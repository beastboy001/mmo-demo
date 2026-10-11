using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//*****************************************
//功能说明：加载面板
// Additive 加载游戏场景，等真正激活后再卸 Login，避免切场景空白
//***************************************** 
public class LoadPanel : BasePanel
{
    private const string LoginSceneName = "LoginScene";
    private const string GameSceneName = "Test";

    private Slider processViewSli;
    private Text progressText;

    public override void OnInit()
    {
        Transform slider = transform.Find("Slider");
        processViewSli = slider.GetComponent<Slider>();

        Transform textTrans = slider.Find("Text (Legacy)");
        if (textTrans != null)
        {
            progressText = textTrans.GetComponent<Text>();
        }

        base.OnInit();
    }

    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);
        SetLoadingPercentValue(0);
        StartCoroutine(StartLoading(GameSceneName));
    }

    private IEnumerator StartLoading(string sceneName)
    {
        int displayProgress = 0;
        SetLoadingPercentValue(0);

        // 先显示加载 UI，再开始读大场景
        yield return null;

        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        ao.allowSceneActivation = false;

        // 0~0.9 映射到 UI 0~90%
        while (ao.progress < 0.9f)
        {
            int toProgress = Mathf.Clamp(Mathf.RoundToInt(ao.progress / 0.9f * 90f), 0, 90);
            while (displayProgress < toProgress)
            {
                displayProgress++;
                SetLoadingPercentValue(displayProgress);
                yield return null;
            }
        }

        while (displayProgress < 90)
        {
            displayProgress++;
            SetLoadingPercentValue(displayProgress);
            yield return null;
        }

        CharacterPreviewModel.Hide();
        ao.allowSceneActivation = true;

        // 登录 UI 仍可见，等待场景真正激活（最耗时的 10%）
        while (!ao.isDone)
        {
            int toProgress = Mathf.Clamp(90 + Mathf.RoundToInt((ao.progress - 0.9f) / 0.1f * 8f), 90, 98);
            while (displayProgress < toProgress)
            {
                displayProgress++;
                SetLoadingPercentValue(displayProgress);
                yield return null;
            }
            yield return null;
        }

        Scene gameScene = SceneManager.GetSceneByName(sceneName);
        if (gameScene.IsValid())
        {
            SceneManager.SetActiveScene(gameScene);
        }

        while (displayProgress < 100)
        {
            displayProgress++;
            SetLoadingPercentValue(displayProgress);
            yield return null;
        }

        yield return null;
        yield return new WaitForEndOfFrame();

        // 先隐藏面板；卸场景交给 DontDestroyOnLoad 的 GameStarter，避免协程随 Login 场景销毁
        OnClose();

        GameStarter starter = Object.FindObjectOfType<GameStarter>();
        if (starter != null)
        {
            starter.FinishEnterGame(LoginSceneName);
        }
        else
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                canvas.SetActive(false);
            }

            Scene loginScene = SceneManager.GetSceneByName(LoginSceneName);
            if (!loginScene.IsValid())
            {
                loginScene = SceneManager.GetSceneByBuildIndex(0);
            }

            if (loginScene.IsValid())
            {
                yield return SceneManager.UnloadSceneAsync(loginScene);
            }

            this.GetSystem<ICursorSystem>()?.EnterGameplay();
        }
    }

    private void SetLoadingPercentValue(int value)
    {
        if (processViewSli != null)
        {
            processViewSli.value = value / 100f;
        }

        if (progressText != null)
        {
            progressText.text = value + "%";
        }
    }
}
