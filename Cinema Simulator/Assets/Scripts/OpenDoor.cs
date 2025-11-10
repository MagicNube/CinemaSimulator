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
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("NPC"))
            anim.SetBool("Open", true);
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("NPC"))
            anim.SetBool("Open", false);
    }
}
