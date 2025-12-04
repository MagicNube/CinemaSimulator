using UnityEditorInternal;
using UnityEngine;

public class CambiadorFase : MonoBehaviour
{
    // Esta función la llamará el jugador al hacer clic
    [System.Obsolete]
    public void Interactuar()
    {

        if (GameManager.Instance != null)
        Debug.Log(GameManager.Instance.ToString());
        {
            if (!TransitionManager.Instance.transicionando)
            {
                GameManager.Instance.AvanzarSiguienteFase();
            }
        }
    }
}
