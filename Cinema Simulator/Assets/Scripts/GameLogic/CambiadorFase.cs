using UnityEngine;

public class CambiadorFase : MonoBehaviour
{
    // Esta función la llamará el jugador al hacer clic
    [System.Obsolete]
    public void Interactuar()
    {

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AvanzarSiguienteFase();
        }
    }
}