using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    // Distancia máxima para interactuar
    public float interactionDistance = 10f; 

    void Update()
    {
        // Si pulsamos el clic izquierdo
        if (Input.GetButtonDown("Fire1"))
        {
            // Lanza un rayo desde el centro de la cámara
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit; 

            // Si el rayo golpea algo...
            if (Physics.Raycast(ray, out hit, interactionDistance))
            {
                // ¿Y ese algo tiene la etiqueta "Bell"?
                if (hit.collider.CompareTag("Bell"))
                {
                    // Busca el AudioSource en el objeto golpeado
                    AudioSource audio = hit.collider.GetComponent<AudioSource>();
                    
                    if (audio != null)
                    {
                        // ¡Tócalo!
                        audio.Play();
                    }
                }
            }
        }
    }
}