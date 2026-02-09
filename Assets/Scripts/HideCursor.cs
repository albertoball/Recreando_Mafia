using UnityEngine;

public class HideCursor : MonoBehaviour
{
    [SerializeField] bool lockCursor = true;   
    [SerializeField] KeyCode toggleKey = KeyCode.Escape;

    void Start()
    {
        ApplyState(true);
    }

    void Update()
    {
        
        if (Input.GetKeyDown(toggleKey))
        {
            bool shouldShow = Cursor.visible == false; 
            ApplyState(!shouldShow);
        }
    }

    void ApplyState(bool hide)
    {
        Cursor.visible = !hide;
        Cursor.lockState = hide && lockCursor
            ? CursorLockMode.Locked
            : CursorLockMode.None;
    }
}
