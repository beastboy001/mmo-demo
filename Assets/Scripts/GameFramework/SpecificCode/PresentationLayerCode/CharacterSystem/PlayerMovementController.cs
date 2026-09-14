using UnityEngine;
//*****************************************
//功能说明：玩家移动
// WASD 走，Shift 跑，Space 跳
// 松键减速停步；跳跃落地防顿挫
//*****************************************
[DefaultExecutionOrder(100)]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour, IController
{
    public const string LocomotionControllerPath = "Animation/PlayerLocomotion";

    [Header("移动")]
    public float walkSpeed = 2.2f;
    public float runSpeed = 5.5f;
    public float rotateSmoothSpeed = 12f;
    public float accelerationTime = 0.12f;
    public float decelerationTime = 0.22f;
    public float stopSpeedThreshold = 0.15f;

    [Header("跳跃 / 重力")]
    public float gravity = 20f;
    public float maxJumpHeight = 1.2f;
    public float checkSphereRadius = 0.18f;
    public LayerMask groundLayer = ~0;
    public float jumpMinAirTime = 0.12f;
    public float landLockTime = 0.08f;
    public float groundedStickForce = 2.5f;

    [Header("动画")]
    public float animSpeedDampTime = 0.15f;

    [Header("引用（可空）")]
    public Transform groundCheckPointTrans;
    public Transform cameraTransform;
    public Animator animator;
    public RuntimeAnimatorController locomotionController;

    private CharacterController characterController;
    private InputController ic;
    private CharacterFSM characterFSM;
    private bool fsmReady;

    private Vector3 horizontalVelocity;
    private Vector3 horizontalAccel;
    private float verticalVelocity;
    private bool isJumping;
    private float jumpStartTime = -999f;
    private float landLockUntil;
    private bool wasGrounded = true;

    public bool IsMovingVisually => horizontalVelocity.sqrMagnitude > stopSpeedThreshold * stopSpeedThreshold;
    public float CurrentHorizontalSpeed => new Vector2(horizontalVelocity.x, horizontalVelocity.z).magnitude;
    public float AnimSpeed01 => Mathf.Clamp01(CurrentHorizontalSpeed / Mathf.Max(0.01f, runSpeed));

    /// <summary>对话等 UI 打开时设为 true，禁止移动</summary>
    public bool controlLocked;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        SetupCharacterController();
        EnsureGroundCheckPoint();
        DetachCinemachineFromPlayer();
        ResolveCamera();
        ResolveAnimator();

        ic = GetComponent<InputController>();
        if (ic == null)
        {
            ic = gameObject.AddComponent<InputController>();
        }

        characterFSM = GetComponent<CharacterFSM>();
        if (characterFSM == null)
        {
            characterFSM = gameObject.AddComponent<CharacterFSM>();
        }
    }

    private void Start()
    {
        ResolveAnimator();
        ResolveLocomotionController();

        if (animator != null)
        {
            animator.applyRootMotion = false;
            if (locomotionController != null)
            {
                animator.runtimeAnimatorController = locomotionController;
            }
            characterFSM.InitFSM(animator, ic);
            fsmReady = true;
        }
        else
        {
            Debug.LogError("[PlayerMovementController] 找不到 Animator。");
        }

        // 进入场景玩法时隐藏光标（架构未启动则跳过）
        try
        {
            this.GetSystem<ICursorSystem>()?.EnterGameplay();
        }
        catch
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Update()
    {
        if (characterController == null || !characterController.enabled)
        {
            return;
        }

        float h, v;
        bool wantRun, wantJump;
        if (controlLocked)
        {
            h = 0f;
            v = 0f;
            wantRun = false;
            wantJump = false;
        }
        else if (ic != null)
        {
            h = ic.GetFloatInputValue(InputCode.HorizontalMoveValue);
            v = ic.GetFloatInputValue(InputCode.VerticalMoveValue);
            wantRun = ic.GetBoolInputValue(InputCode.RunState);
            wantJump = ic.GetBoolInputValue(InputCode.JumpState);
        }
        else
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
            wantRun = Input.GetKey(KeyCode.LeftShift);
            wantJump = Input.GetButtonDown("Jump");
        }

        bool hasMoveInput = (h * h + v * v) > 0.0001f;
        Vector3 inputDir = GetCameraRelativeMoveDirection(h, v);

        // 水平：加速/减速，松键不瞬间归零
        Vector3 targetVel = Vector3.zero;
        if (hasMoveInput)
        {
            targetVel = inputDir * (wantRun ? runSpeed : walkSpeed);
            Quaternion targetRot = Quaternion.LookRotation(
                inputDir.sqrMagnitude > 0.001f ? inputDir : transform.forward, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRot,
                1f - Mathf.Exp(-rotateSmoothSpeed * Time.deltaTime));
        }

        float smoothTime = hasMoveInput ? accelerationTime : decelerationTime;
        horizontalVelocity = Vector3.SmoothDamp(
            horizontalVelocity, targetVel, ref horizontalAccel, Mathf.Max(0.01f, smoothTime));

        if (!hasMoveInput && horizontalVelocity.sqrMagnitude < stopSpeedThreshold * stopSpeedThreshold)
        {
            horizontalVelocity = Vector3.zero;
        }

        // 起跳
        bool canJump = Time.time >= landLockUntil;
        if (wantJump && wasGrounded && !isJumping && canJump)
        {
            isJumping = true;
            jumpStartTime = Time.time;
            verticalVelocity = Mathf.Sqrt(2f * gravity * maxJumpHeight);
            wasGrounded = false;
            if (fsmReady)
            {
                characterFSM.ChangeState(CHARACTERSTATE.JUMP);
            }
        }

        // 竖直
        if (isJumping || !wasGrounded)
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        else
        {
            verticalVelocity = -groundedStickForce;
        }

        Vector3 motion = horizontalVelocity * Time.deltaTime;
        motion.y = verticalVelocity * Time.deltaTime;
        CollisionFlags flags = characterController.Move(motion);

        // Move 后再判地
        bool groundedNow = (flags & CollisionFlags.Below) != 0
                           || characterController.isGrounded
                           || ProbeGround();

        bool pastMinAir = (Time.time - jumpStartTime) >= jumpMinAirTime;
        if (isJumping && !pastMinAir)
        {
            groundedNow = false;
        }

        if (isJumping && groundedNow && verticalVelocity <= 0f && pastMinAir)
        {
            isJumping = false;
            landLockUntil = Time.time + landLockTime;
            verticalVelocity = -groundedStickForce;
            if (fsmReady)
            {
                JudgeAndChangeStateIdleOrMove(IsMovingVisually || hasMoveInput);
            }
        }

        wasGrounded = groundedNow && !isJumping;

        // 减速滑步期间仍算 MOVE，避免动作硬切
        if (fsmReady && characterFSM.GetCurrentState() != CHARACTERSTATE.JUMP)
        {
            JudgeAndChangeStateIdleOrMove(IsMovingVisually || hasMoveInput);
        }

        if (animator != null)
        {
            animator.SetBool(PlayerAnimParams.IsGround, wasGrounded);
            animator.SetFloat(PlayerAnimParams.Speed, AnimSpeed01, animSpeedDampTime, Time.deltaTime);
            animator.SetFloat(PlayerAnimParams.MoveValue, AnimSpeed01, animSpeedDampTime, Time.deltaTime);
        }
    }

    private Vector3 GetCameraRelativeMoveDirection(float h, float v)
    {
        if (cameraTransform == null)
        {
            ResolveCamera();
        }

        Vector3 forward = cameraTransform != null ? cameraTransform.forward : transform.forward;
        Vector3 right = cameraTransform != null ? cameraTransform.right : transform.right;
        forward.y = 0f;
        right.y = 0f;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        if (right.sqrMagnitude < 0.001f) right = Vector3.right;
        forward.Normalize();
        right.Normalize();

        Vector3 dir = forward * v + right * h;
        if (dir.sqrMagnitude > 1f) dir.Normalize();
        return dir;
    }

    private void JudgeAndChangeStateIdleOrMove(bool moving)
    {
        if (!fsmReady) return;
        characterFSM.ChangeState(moving ? CHARACTERSTATE.MOVE : CHARACTERSTATE.IDLE);
    }

    private bool ProbeGround()
    {
        Vector3 origin = groundCheckPointTrans != null
            ? groundCheckPointTrans.position
            : transform.position + Vector3.up * 0.08f;

        return Physics.SphereCast(
            origin + Vector3.up * 0.2f,
            checkSphereRadius * 0.9f,
            Vector3.down,
            out _,
            checkSphereRadius + 0.12f,
            groundLayer,
            QueryTriggerInteraction.Ignore);
    }

    private void SetupCharacterController()
    {
        characterController.height = Mathf.Max(characterController.height, 1.6f);
        characterController.radius = Mathf.Max(characterController.radius, 0.25f);
        characterController.center = new Vector3(0f, characterController.height * 0.5f, 0f);
        characterController.slopeLimit = 45f;
        characterController.stepOffset = Mathf.Min(0.35f, characterController.height * 0.25f);
        characterController.minMoveDistance = 0f;
        characterController.skinWidth = 0.08f;
    }

    private void EnsureGroundCheckPoint()
    {
        if (groundCheckPointTrans != null) return;
        Transform found = transform.Find("GroundCheckPoint");
        if (found != null)
        {
            groundCheckPointTrans = found;
            return;
        }
        GameObject go = new GameObject("GroundCheckPoint");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        groundCheckPointTrans = go.transform;
    }

    private void DetachCinemachineFromPlayer()
    {
        // 远程玩家不应拆镜头到场景（避免抢走本地相机）
        if (GetComponent<RemotePlayerView>() != null)
        {
            return;
        }

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            string n = child.name;
            if (n.Contains("FreeLook") || n == "Camera" || n.Contains("CM "))
            {
                child.SetParent(null, true);
            }
        }
    }

    private void ResolveCamera()
    {
        if (cameraTransform != null) return;
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            return;
        }
        Camera cam = Object.FindObjectOfType<Camera>();
        if (cam != null) cameraTransform = cam.transform;
    }

    private void ResolveAnimator()
    {
        if (animator != null) return;
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void ResolveLocomotionController()
    {
        if (locomotionController != null) return;

        locomotionController = GameResSystem.GetRes<RuntimeAnimatorController>(LocomotionControllerPath);
        if (locomotionController == null)
        {
            Debug.LogError("[PlayerMovementController] 找不到动画控制器: " + LocomotionControllerPath);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = groundCheckPointTrans != null
            ? groundCheckPointTrans.position
            : transform.position + Vector3.up * 0.08f;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(origin, checkSphereRadius);
    }
#endif
}
