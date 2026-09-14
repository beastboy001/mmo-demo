using UnityEngine;

//*****************************************
//功能说明：本地玩家定时向服务器上报位置
// 挂在自己控制的角色上
//*****************************************
public class LocalPlayerStateSender : MonoBehaviour, IController
{
    [Tooltip("上报间隔（秒）")]
    public float sendInterval = 0.1f;

    private INetSystem netSystem;
    private PlayerMovementController move;
    private float timer;
    private Vector3 lastPos;
    private float lastYaw;

    private void Start()
    {
        netSystem = this.GetSystem<INetSystem>();
        move = GetComponent<PlayerMovementController>();
        lastPos = transform.position;
        lastYaw = transform.eulerAngles.y;
    }

    private void Update()
    {
        if (netSystem == null || !netSystem.IsConnected || netSystem.EntityId <= 0)
        {
            return;
        }

        timer += Time.deltaTime;
        if (timer < sendInterval)
        {
            return;
        }

        timer = 0f;

        Vector3 pos = transform.position;
        float yaw = transform.eulerAngles.y;
        float anim = move != null ? move.AnimSpeed01 : 0f;

        // 几乎没动也可以低频发，这里简单每次到点都发，保证别人能跟上
        bool moved = (pos - lastPos).sqrMagnitude > 0.0001f || Mathf.Abs(Mathf.DeltaAngle(lastYaw, yaw)) > 0.5f;
        if (!moved && anim < 0.01f)
        {
            // 静止时偶尔仍发一次，避免别人永远卡在半路；用较长间隔
            // 这里仍发送，逻辑简单
        }

        netSystem.SendEntityState(pos, yaw, anim);
        lastPos = pos;
        lastYaw = yaw;
    }
}
