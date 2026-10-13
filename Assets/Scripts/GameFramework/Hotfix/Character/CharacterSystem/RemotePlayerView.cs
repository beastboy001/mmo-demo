using UnityEngine;

//*****************************************
//功能说明：远程玩家表现：插值跟位置 + 外观
//*****************************************
public class RemotePlayerView : MonoBehaviour
{
    public int EntityId { get; private set; }
    public int CharacterId { get; private set; }
    public string DisplayName { get; private set; }
    public int Level { get; private set; }

    private Vector3 targetPos;
    private Quaternion targetRot;
    private float targetAnimSpeed;
    private Animator animator;

    public void Setup(int entityId)
    {
        EntityId = entityId;
        animator = GetComponentInChildren<Animator>();
        targetPos = transform.position;
        targetRot = transform.rotation;

        // 交互触发器（走近出「互动」）
        PlayerInteractable interact = GetComponent<PlayerInteractable>();
        if (interact == null)
        {
            interact = gameObject.AddComponent<PlayerInteractable>();
        }
    }

    public void ApplyEnter(EntityEnterMsg msg)
    {
        CharacterId = msg.characterId > 0 ? msg.characterId : msg.entityId;
        DisplayName = msg.name;
        Level = msg.level;

        targetPos = new Vector3(msg.x, msg.y, msg.z);
        targetRot = Quaternion.Euler(0f, msg.yaw, 0f);
        transform.position = targetPos;
        transform.rotation = targetRot;

        PlayerData pd = new PlayerData();
        pd.id = msg.name;
        pd.level = msg.level;
        pd.role = (ROLE)msg.role;
        pd.gender = (GENDER)msg.gender;
        pd.faceTypeId = msg.faceTypeId;
        pd.hairStyleId = msg.hairStyleId;
        pd.hairColorId = msg.hairColorId;
        pd.skinId = msg.skinId;
        pd.eyeColorId = msg.eyeColorId;
        pd.faceEmotionId = msg.faceEmotionId;

        P09CharacterDress dress = GetComponent<P09CharacterDress>();
        if (dress == null)
        {
            dress = gameObject.AddComponent<P09CharacterDress>();
        }

        dress.Apply(pd);
    }

    public void ApplyState(EntityStateMsg msg)
    {
        targetPos = new Vector3(msg.x, msg.y, msg.z);
        targetRot = Quaternion.Euler(0f, msg.yaw, 0f);
        targetAnimSpeed = msg.animSpeed;
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, 1f - Mathf.Exp(-12f * Time.deltaTime));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1f - Mathf.Exp(-12f * Time.deltaTime));

        if (animator != null)
        {
            float cur = animator.GetFloat(PlayerAnimParams.Speed);
            float next = Mathf.Lerp(cur, targetAnimSpeed, 10f * Time.deltaTime);
            animator.SetFloat(PlayerAnimParams.Speed, next);
            animator.SetFloat(PlayerAnimParams.MoveValue, next);
            animator.SetBool(PlayerAnimParams.MoveState, next > 0.05f);
            animator.SetBool(PlayerAnimParams.IsGround, true);
        }
    }
}
