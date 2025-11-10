using UnityEngine;

public class BellRingSound : MonoBehaviour
{

    private AudioSource bellRingSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bellRingSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void onMouseDown()
    {
        Debug.Log("¡CLIC DETECTADO en el ring!");
        bellRingSound.Play();
    }
}
