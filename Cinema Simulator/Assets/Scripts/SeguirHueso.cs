using UnityEngine;

public class SeguirHueso : MonoBehaviour
{
    public Transform huesoASeguir;

    void LateUpdate()
    {
        if (huesoASeguir != null)
        {
            transform.position = huesoASeguir.position;
            transform.rotation = huesoASeguir.rotation;
        }
    }
}
