// CampanaInteractiva.cs
using UnityEngine;

// Asegúrate de que el objeto tiene estos componentes
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Outline))] // O como se llame tu script de outline
[RequireComponent(typeof(Collider))]

public class CampanaInteractiva : MonoBehaviour
{
    private AudioSource audioSource;
    private Animator animator;

    // Un "cooldown" para que no se pueda spamear
    private bool puedeSonar = true;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Esta función será llamada por el jugador
    public void Interactuar()
    {
        if (puedeSonar)
        {
            // 1. Reproducir sonido
            audioSource.Play();

            // 3. Iniciar cooldown
            puedeSonar = false;
            // Permitir que suene de nuevo después de 2 segundos
            Invoke("ResetearCooldown", 2.0f);

            Debug.Log("¡DING!");
        }
    }

    void ResetearCooldown()
    {
        puedeSonar = true;
    }
}
