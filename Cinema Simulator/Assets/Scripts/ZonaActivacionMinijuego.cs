using UnityEngine;

public class ZonaActivacionMinijuego : MonoBehaviour
{
    private bool jugadorDentro = false;

    void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player") && !jugadorDentro && GameManager.Instance.faseActual == FaseJuego.Fase4_Limpieza)
        {
            jugadorDentro = true;

            if (MinijuegoLimpiezaManager.Instance != null)
            {
                MinijuegoLimpiezaManager.Instance.MostrarPregunta();
            }
            else
            {
                Debug.LogError("�ERROR! No se encuentra la Instancia de MinijuegoLimpiezaManager.");
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