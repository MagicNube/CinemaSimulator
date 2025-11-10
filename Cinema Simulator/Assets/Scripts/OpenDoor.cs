using UnityEngine;
using System.Collections;
public class OpenDoor : MonoBehaviour
{
    private Animator anim;
    void Start()
    {
        anim = GetComponent<Animator>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            anim.SetBool("Open", true);
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            anim.SetBool("Open", false);
    }
}
