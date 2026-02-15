using UnityEngine;

public class SetWeaponPositionWhenAiming : MonoBehaviour
{
    

    void Update()
    {
        transform.rotation = Quaternion.Euler(0.91f, 96, transform.position.z);
    }

}
