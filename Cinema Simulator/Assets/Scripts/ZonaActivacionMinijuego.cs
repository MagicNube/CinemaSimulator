using UnityEngine;

public class ZonaActivacionMinijuego : MonoBehaviour
{
    private bool jugadorDentro = false;
    // Nueva variable para controlar si ya se usó esta zona
    private bool yaActivado = false;

    void OnTriggerEnter(Collider other)
    {
        // Añadimos !yaActivado a la condición
        if (other.CompareTag("Player") && !jugadorDentro && !yaActivado && GameManager.Instance.faseActual == FaseJuego.Fase4_Limpieza)
        {
            jugadorDentro = true;

            // Marcamos como activado para que no vuelva a entrar en este if
            yaActivado = true;

            if (MinijuegoLimpiezaManager.Instance != null)
            {
                MinijuegoLimpiezaManager.Instance.MostrarPregunta();

                // OPCIONAL: Si quieres que el objeto desaparezca o deje de detectar físicas inmediatamente:
                // GetComponent<Collider>().enabled = false; 
            }
            else
            {
                Debug.LogError("¡ERROR! No se encuentra la Instancia de MinijuegoLimpiezaManager.");
                // Si hubo error, quizás quieras permitir que se reactive:
                yaActivado = false;
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