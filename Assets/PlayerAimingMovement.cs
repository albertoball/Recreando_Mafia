using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerAimingMovement : MonoBehaviour
{
    public Animator animator;

    [Header("Animator Params")]
    public string aimXParam = "Aim_Move_X";
    public string aimYParam = "Aim_Move_Y";
    public float damp = 0.08f;

    [Header("Gravity")]
    public float gravity = -9.81f;
    public float groundedStick = -2f;

    private CharacterController controller;
    private float verticalVel;

    private float horizontal;
    private float vertical;
    private bool hasInput;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (animator == null) return;

        // Input (pots canviar a GetAxisRaw si vols més sec)
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        hasInput = (horizontal * horizontal + vertical * vertical) > 0.001f;

        // Params del blend tree strafe
        animator.SetFloat(aimXParam, horizontal, damp, Time.deltaTime);
        animator.SetFloat(aimYParam, vertical, damp, Time.deltaTime);

        // Gravity
        if (controller.isGrounded && verticalVel < 0f)
            verticalVel = groundedStick;

        verticalVel += gravity * Time.deltaTime;
    }

    void OnAnimatorMove()
    {
        if (animator == null || !animator.applyRootMotion) return;

        Vector3 delta = animator.deltaPosition;

        float planarMag = new Vector3(delta.x, 0f, delta.z).magnitude;

        Vector3 dir = Vector3.zero;

        if (hasInput)
        {
            // Important: com el cos ja gira amb la càmera, forward/right del player serveix perfecte
            dir = (transform.forward * vertical + transform.right * horizontal);
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.0001f)
                dir.Normalize();
            else
                dir = transform.forward;
        }

        Vector3 finalMove = dir * planarMag;
        finalMove.y = verticalVel * Time.deltaTime;

        controller.Move(finalMove);
    }
}
