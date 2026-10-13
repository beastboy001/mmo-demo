using System.Collections;
using UnityEngine;
using UnityEngine.UI;
//*****************************************
//功能说明：提示面板
//***************************************** 
public class TipPanel : BasePanel
{
    private Text tipText;
    private RectTransform contentRect;
    private Vector2 restAnchoredPos;
    private Coroutine showCoroutine;

    private const float SlideDistance = 120f;
    private const float SlideInDuration = 0.35f;
    private const float DisplayDuration = 2.5f;
    private const float SlideOutDuration = 0.35f;

    public override void OnInit()
    {
        tipText = DeepFindTransform.DeepFindChild(transform, "TipText").GetComponent<Text>();
        contentRect = transform as RectTransform;
        restAnchoredPos = contentRect.anchoredPosition;
        base.OnInit();
    }

    /// <summary>
    /// 打开提示面板
    /// </summary>
    /// <param name="objs">提示内容(string)</param>
    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);
        if (objs != null && objs.Length > 0)
        {
            tipText.text = (string)objs[0];
        }

        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }
        showCoroutine = StartCoroutine(ShowTipRoutine());
    }

    public override void OnClose()
    {
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }
        contentRect.anchoredPosition = restAnchoredPos;
        base.OnClose();
    }

    private IEnumerator ShowTipRoutine()
    {
        Vector2 fromAbove = restAnchoredPos + new Vector2(0f, SlideDistance);

        contentRect.anchoredPosition = fromAbove;

        float elapsed = 0f;
        while (elapsed < SlideInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / SlideInDuration);
            contentRect.anchoredPosition = Vector2.Lerp(fromAbove, restAnchoredPos, t);
            yield return null;
        }
        contentRect.anchoredPosition = restAnchoredPos;

        yield return new WaitForSeconds(DisplayDuration);

        elapsed = 0f;
        while (elapsed < SlideOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / SlideOutDuration);
            contentRect.anchoredPosition = Vector2.Lerp(restAnchoredPos, fromAbove, t);
            yield return null;
        }

        showCoroutine = null;
        OnClose();
    }
}
