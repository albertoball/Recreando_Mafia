using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerAimingMovement : MonoBehaviour
{
    public Transform cameraTransform;
    public Animator animator;

    public float rotationSpeed = 15f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private float verticalVelocity;

    float horizontal;
    float vertical;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        // ROTACIÓ cap a la càmera
        if (cameraTransform != null)
        {
            float targetYaw = cameraTransform.eulerAngles.y;
            Quaternion targetRot = Quaternion.Euler(0f, targetYaw, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Anim strafe
        animator.SetFloat("Aim_Move_X", horizontal, 0.05f, Time.deltaTime);
        animator.SetFloat("Aim_Move_Y", vertical, 0.05f, Time.deltaTime);

        // gravetat
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
    }

    void OnAnimatorMove()
    {
        if (!animator.applyRootMotion) return;

        Vector3 delta = animator.deltaPosition;

        float mag = new Vector3(delta.x, 0f, delta.z).magnitude;

        Vector3 cf = cameraTransform.forward;
        Vector3 cr = cameraTransform.right;
        cf.y = 0f;
        cr.y = 0f;
        cf.Normalize();
        cr.Normalize();

        Vector3 dir = (cf * vertical + cr * horizontal);
        dir = dir.sqrMagnitude > 0.001f ? dir.normalized : transform.forward;

        Vector3 final = dir * mag;
        final.y = verticalVelocity * Time.deltaTime;

        controller.Move(final);
    }
}
