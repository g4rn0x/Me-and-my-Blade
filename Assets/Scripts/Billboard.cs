using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform cam;

    private void Start()
    {
        if (Camera.main != null)
        {
            cam = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }

        if (cam != null)
        {
            transform.rotation = cam.rotation;
        }
    }
}