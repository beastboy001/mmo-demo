using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//*****************************************
//功能说明：创建/选角界面 DragArea 拖动旋转预览角色
// 挂在 UI 的 DragArea 上，Panel 打开时 Bind(角色 Transform)
//*****************************************
public class CharacterPreviewRotator : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public const string DragAreaName = "DragArea";
    private const string DragAreaAltName = "Drag area";

    [SerializeField] private float rotateSpeed = 0.4f;

    private Transform target;

    private void Awake()
    {
        EnsureRaycastGraphic();
    }

    public void Bind(Transform characterRoot)
    {
        target = characterRoot;
    }

    public void Unbind()
    {
        target = null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
        RotateByDelta(eventData.delta.x);
    }

    private void RotateByDelta(float deltaX)
    {
        if (target == null || Mathf.Approximately(deltaX, 0f)) return;

        target.Rotate(0f, -deltaX * rotateSpeed, 0f, Space.World);
    }

    /// <summary>DragArea 需要 Graphic 才能接收射线与拖动</summary>
    private void EnsureRaycastGraphic()
    {
        Graphic graphic = GetComponent<Graphic>();
        if (graphic == null)
        {
            var image = gameObject.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0f);
            graphic = image;
        }

        graphic.raycastTarget = true;
    }

    /// <summary>在面板根节点下找 DragArea，没有 Rotator 就自动挂上</summary>
    public static CharacterPreviewRotator GetOrCreate(Transform panelRoot)
    {
        if (panelRoot == null) return null;

        Transform dragArea = FindDragArea(panelRoot);
        if (dragArea == null)
        {
            Debug.LogWarning("[CharacterPreviewRotator] 找不到 DragArea / Drag area");
            return null;
        }

        // 确保 DragArea 在最上层，避免被 RawImage 挡住
        dragArea.SetAsLastSibling();

        CharacterPreviewRotator rotator = dragArea.GetComponent<CharacterPreviewRotator>();
        if (rotator == null)
        {
            rotator = dragArea.gameObject.AddComponent<CharacterPreviewRotator>();
        }

        rotator.EnsureRaycastGraphic();
        DisableBlockingRaycasts(panelRoot);

        return rotator;
    }

    private static Transform FindDragArea(Transform panelRoot)
    {
        Transform dragArea = DeepFindTransform.DeepFindChild(panelRoot, DragAreaName);
        if (dragArea == null)
        {
            dragArea = DeepFindTransform.DeepFindChild(panelRoot, DragAreaAltName);
        }

        return dragArea;
    }

    /// <summary>中间 RawImage 只负责显示 RenderTexture，不需要挡射线</summary>
    private static void DisableBlockingRaycasts(Transform panelRoot)
    {
        Transform rawImageTrans = DeepFindTransform.DeepFindChild(panelRoot, "RawImage");
        if (rawImageTrans == null) return;

        RawImage rawImage = rawImageTrans.GetComponent<RawImage>();
        if (rawImage != null)
        {
            rawImage.raycastTarget = false;
        }
    }
}
