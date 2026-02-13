using UnityEngine;
using UnityEngine.Animations.Rigging;

public class GunWeightManagerLocomotion : MonoBehaviour
{
    private Animator animator;
    public Rig rig;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void GetHoldWeaponPose(int layerIndex, int layerWeight, float rigWeight) 
    { 
        animator.SetLayerWeight(layerIndex, layerWeight); 
        rig.weight = rigWeight; 
    }

}
