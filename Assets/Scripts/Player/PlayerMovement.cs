using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;

    [Header("Input")]
    public KeyCode walkToggleKey = KeyCode.CapsLock;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public bool walkToggled;

    [Header("References")]
    public Transform cameraTransform;
    private GunWeightManagerLocomotion gunWeightManagerLocomotion;

    [Header("Rotation")]
    public float rotationSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    [Header("Animation")]
    public string moveBlendParam = "Move";     // Float BlendTree: 0 idle, 0.5 walk, 1 jog
    public string runBoolParam = "Sprint";     // Bool: controls Sprint state outside BlendTree
    public float animLerpSpeed = 10f;

    [Header("Physics")]
    public float gravity = -9.81f;
    private float verticalVelocity = 0f;

    public float horizontal;
    public float vertical;

    // Internal state
    private Vector3 moveDirWorld = Vector3.zero;
    private bool hasInput = false;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        animator.applyRootMotion = true;

        gunWeightManagerLocomotion = GetComponent<GunWeightManagerLocomotion>();
    }

    private void Update()
    {
        // Read WASD input
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(horizontal, 0f, vertical);
        hasInput = input.sqrMagnitude > 0.001f;

        // Sprint: active while moving + Shift
        bool runningNow = hasInput && Input.GetKey(sprintKey);
        animator.SetBool(runBoolParam, runningNow);

        // Reset WalkToggle when Idle OR Sprint (Sprint behaves like a "hard reset" state)
        if (!hasInput || runningNow)
        {
            walkToggled = false;
        }
        else
        {
            // Toggle Walk only while moving and NOT sprinting
            if (Input.GetKeyDown(walkToggleKey))
            {
                walkToggled = !walkToggled; // Jog <-> Walk
            }
        }

        // Compute world movement direction relative to camera
        moveDirWorld = Vector3.zero;

        if (hasInput)
        {
            if (cameraTransform != null)
            {
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;

                camForward.y = 0f;
                camRight.y = 0f;

                camForward.Normalize();
                camRight.Normalize();

                moveDirWorld = (camForward * input.z + camRight * input.x).normalized;
            }
            else
            {
                moveDirWorld = (transform.forward * input.z + transform.right * input.x).normalized;
            }

            // Smoothly rotate towards movement direction
            float targetAngle = Mathf.Atan2(moveDirWorld.x, moveDirWorld.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        // BlendTree control:
        // - Idle: 0
        // - Walk: 0.5 (only if walkToggled and not sprinting)
        // - Jog: 1
        // While sprinting, keep Move at 1 so when you exit Sprint you return to Jog.
        float targetBlend = 0f;

        if (!hasInput)
        {
            targetBlend = 0f;
        }
        else
        {
            if (runningNow)
                targetBlend = 1f;
            else
                targetBlend = walkToggled ? 0.5f : 1f;
        }


        // Set the weight of the Rig and Layer Animation depending on what animation is reproducing
        if (targetBlend == 0f || targetBlend == 0.5f)
        {
            gunWeightManagerLocomotion.GetHoldWeaponPose(2, 1, 0.658f);
        }
        else
        {
            gunWeightManagerLocomotion.GetHoldWeaponPose(2, 0, 0f);
        }

        float currentBlend = animator.GetFloat(moveBlendParam);
        float newBlend = Mathf.Lerp(currentBlend, targetBlend, animLerpSpeed * Time.deltaTime);
        animator.SetFloat(moveBlendParam, newBlend);
    }

    private void OnAnimatorMove()
    {
        if (!animator || !animator.applyRootMotion || controller == null) return;

        // Root motion magnitude comes from the current animation (Idle/Walk/Jog/Sprint)
        Vector3 rootMotion = animator.deltaPosition;
        rootMotion.y = 0f;

        Vector3 finalMove = Vector3.zero;

        if (hasInput && moveDirWorld.sqrMagnitude > 0.0001f)
        {
            // Direction from input, speed from root motion
            finalMove = moveDirWorld * rootMotion.magnitude;
        }

        // Manual gravity for CharacterController
        verticalVelocity += gravity * Time.deltaTime;
        finalMove.y = verticalVelocity * Time.deltaTime;

        controller.Move(finalMove);

        // Keep grounded
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
    }
}
