using UnityEngine;

public class ZonaActivacionMinijuego : MonoBehaviour
{
    private bool jugadorDentro = false;

    void OnTriggerEnter(Collider other)
    {
        // 1. Verificar si Unity detecta la colisión física
        Debug.Log("Alguien ha tocado el timbre (Trigger): " + other.name + " | Tag: " + other.tag);

        if (other.CompareTag("Player") && !jugadorDentro)
        {
            Debug.Log("¡Es el jugador! Intentando llamar al Manager...");
            jugadorDentro = true;

            if (MinijuegoLimpiezaManager.Instance != null)
            {
                // Vamos a ver si el manager responde
                MinijuegoLimpiezaManager.Instance.MostrarPregunta();
            }
            else
            {
                Debug.LogError("¡ERROR! No se encuentra la Instancia de MinijuegoLimpiezaManager.");
            }
        }
        else if (!other.CompareTag("Player"))
        {
            Debug.LogWarning("El objeto que entró NO tiene el tag 'Player'.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = false;
            Debug.Log("El jugador salió del trigger.");
        }
    }
}