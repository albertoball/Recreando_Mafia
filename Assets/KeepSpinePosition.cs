using UnityEngine;

public class KeepSpinePosition : MonoBehaviour
{
    [Header("Bone a seguir")]
    public Transform targetBone;

    [Header("Offset local respecte al os")]
    public Vector3 positionOffset;

    [Header("Seguir també la rotació del os?")]
    public bool followRotation = false;

    void LateUpdate()
    {
        if (targetBone == null) return;

        // Posició amb offset en espai del os
        Vector3 desiredPosition = targetBone.position
                                + targetBone.TransformDirection(positionOffset);

        transform.position = desiredPosition;

        if (followRotation)
        {
            transform.rotation = targetBone.rotation;
        }
    }
}

