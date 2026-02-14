using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerAimMovement : MonoBehaviour
{
    [Header("Refs")]
    public Transform cameraTransform;
    private Animator animator;

    private CharacterController controller;

    [Header("Rotation (aim)")]
    public float aimingRotationSmoothTime = 0.05f;
    private float aimTurnSmoothVelocity;

    [Header("Root Motion Direction")]
    [Tooltip("Si està activat: magnitud del root motion + direcció per input (recomanat per strafe net).")]
    public bool useRootMotionMagnitudeWithInputDirection = true;

    [Header("Gravity")]
    public float gravity = -9.81f;
    public float groundedStick = -2f;
    private float verticalVelocity;

    [Header("Animator Params (opcional)")]
    public bool setAimParams = true;
    public string aimXParam = "Aim_Move_X";
    public string aimYParam = "Aim_Move_Y";
    public float aimInputDamp = 0.08f;

    [Header("Aim Layer Weight (Avatar Mask en cames) (opcional)")]
    [Tooltip("Nom exacte de la capa d'aim (cames). Deixa buit si no vols tocar cap capa.")]
    public string aimLayerName = "Aim Locomotion";
    public float aimLayerBlendSpeed = 14f;
    private int aimLayerIndex = -1;
    private float aimLayerWeight = 0f;

    // Input
    private float horizontal;
    private float vertical;
    private bool hasInput;
    private Vector3 moveDirWorld;

    void Reset()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Awake()
    {
        animator = GetComponent<Animator>();

        controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (animator != null && !string.IsNullOrEmpty(aimLayerName))
            aimLayerIndex = animator.GetLayerIndex(aimLayerName);
    }


    void OnDisable()
    {
        // Neteja en eixir del mode (evita que es quede enganxat)
        if (animator == null) return;

        if (setAimParams)
        {
            if (!string.IsNullOrEmpty(aimXParam)) animator.SetFloat(aimXParam, 0f);
            if (!string.IsNullOrEmpty(aimYParam)) animator.SetFloat(aimYParam, 0f);
        }

        if (!string.IsNullOrEmpty(aimLayerName))
        {
            if (aimLayerIndex < 0) aimLayerIndex = animator.GetLayerIndex(aimLayerName);
            if (aimLayerIndex >= 0) animator.SetLayerWeight(aimLayerIndex, 0f);
            aimLayerWeight = 0f;
        }
    }

    void Update()
    {
        ReadInput();
        UpdateAimLayerWeight();
        RotateToCameraYaw();
        UpdateAimAnimatorParams();

        // gravetat
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = groundedStick;

        verticalVelocity += gravity * Time.deltaTime;
    }

    void ReadInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(horizontal, 0f, vertical);
        hasInput = input.sqrMagnitude > 0.001f;

        // direcció món relativa a càmera
        Vector3 cf = (cameraTransform != null) ? cameraTransform.forward : transform.forward;
        Vector3 cr = (cameraTransform != null) ? cameraTransform.right : transform.right;

        cf.y = 0f; cr.y = 0f;
        cf.Normalize(); cr.Normalize();

        Vector3 dir = (cf * vertical) + (cr * horizontal);
        dir.y = 0f;

        moveDirWorld = (dir.sqrMagnitude > 0.0001f) ? dir.normalized : Vector3.zero;
    }

    void RotateToCameraYaw()
    {
        if (cameraTransform == null) return;

        // yaw de càmera
        float targetYaw = cameraTransform.eulerAngles.y;

        float yaw = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetYaw,
            ref aimTurnSmoothVelocity,
            aimingRotationSmoothTime
        );

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    void UpdateAimAnimatorParams()
    {
        if (!setAimParams || animator == null) return;

        if (!string.IsNullOrEmpty(aimXParam))
            animator.SetFloat(aimXParam, horizontal, aimInputDamp, Time.deltaTime);

        if (!string.IsNullOrEmpty(aimYParam))
            animator.SetFloat(aimYParam, vertical, aimInputDamp, Time.deltaTime);
    }

    void UpdateAimLayerWeight()
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(aimLayerName)) return;

        if (aimLayerIndex < 0)
            aimLayerIndex = animator.GetLayerIndex(aimLayerName);

        if (aimLayerIndex < 0) return;

        // Si este script està actiu, volem weight 1
        float target = 1f;
        aimLayerWeight = Mathf.MoveTowards(aimLayerWeight, target, aimLayerBlendSpeed * Time.deltaTime);
        animator.SetLayerWeight(aimLayerIndex, aimLayerWeight);
    }

    void OnAnimatorMove()
    {
        if (animator == null || !animator.applyRootMotion) return;

        Vector3 delta = animator.deltaPosition;

        // planar
        float planarMag = new Vector3(delta.x, 0f, delta.z).magnitude;

        Vector3 final = Vector3.zero;

        if (hasInput && moveDirWorld.sqrMagnitude > 0.0001f)
        {
            if (useRootMotionMagnitudeWithInputDirection)
            {
                // La fórmula “Mafia”: magnitud del root motion però direcció per input
                final = moveDirWorld * planarMag;
            }
            else
            {
                // Alternativa: usar delta tal qual (no tan recomanat en strafe)
                final = new Vector3(delta.x, 0f, delta.z);
            }
        }

        final.y = verticalVelocity * Time.deltaTime;

        controller.Move(final);

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = groundedStick;
    }
}
