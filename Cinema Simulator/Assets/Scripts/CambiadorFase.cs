using UnityEngine;

public class CambiadorFase : MonoBehaviour
{
    // Esta función la llamará el jugador al hacer clic
    public void Interactuar()
    {
        Debug.Log("🛑 Has tocado la cápsula de tiempo.");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AvanzarSiguienteFase();
        }
    }
}