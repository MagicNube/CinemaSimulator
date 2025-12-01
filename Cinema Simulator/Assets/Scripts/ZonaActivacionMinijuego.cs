using UnityEngine;

public class ZonaActivacionMinijuego : MonoBehaviour
{
    private bool jugadorDentro = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !jugadorDentro)
        {
            jugadorDentro = true; 

            if (MinijuegoLimpiezaManager.Instance != null)
            {
                MinijuegoLimpiezaManager.Instance.MostrarPregunta();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = false;
        }
    }
}