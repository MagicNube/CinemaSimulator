using UnityEngine;

public class UIAnchorFollower : MonoBehaviour
{
    public float distance = 1f;

    void LateUpdate()
    {
        Transform cam = Camera.main.transform;
        transform.position = cam.position + cam.forward * distance;
        transform.rotation = cam.rotation;
    }
}
