using UnityEngine;
using TMPro;
using System.Collections;

public class CambiadorFase : MonoBehaviour
{
    [Header("Aviso selección de película")]
    [SerializeField] private TextMeshProUGUI avisoSeleccionPelicula;
    [SerializeField] private string textoAviso = "Debes seleccionar una película en la tablet";
    [SerializeField] private float tiempoMostrarAviso = 3f;
    private Coroutine corrutinAvisoActual;

    void Start()
    {
        if (avisoSeleccionPelicula != null)
            avisoSeleccionPelicula.gameObject.SetActive(false);
    }

    public void Interactuar()
    {
        if (GameManager.Instance != null)
        {
            Debug.Log(GameManager.Instance.ToString());
            if (!TransitionManager.Instance.transicionando)
            {
                // Verificar si está en Fase1 sin película
                if (GameManager.Instance.faseActual == FaseJuego.Fase1_Preparacion)
                {
                    if (TaskManager.Instance != null && !TaskManager.Instance.haElegidoPelicula)
                    {
                        MostrarAvisoTemporalmente();
                        return;
                    }
                }

                GameManager.Instance.AvanzarSiguienteFase();
            }
        }
    }

    private void MostrarAvisoTemporalmente()
    {
        if (avisoSeleccionPelicula == null) return;

        // Cancelar corrutina anterior si existe
        if (corrutinAvisoActual != null)
            StopCoroutine(corrutinAvisoActual);

        // Mostrar aviso
        avisoSeleccionPelicula.text = textoAviso;
        avisoSeleccionPelicula.gameObject.SetActive(true);

        // Iniciar corrutina para ocultarlo
        corrutinAvisoActual = StartCoroutine(OcultarAvisoEnSegundos());
    }

    private IEnumerator OcultarAvisoEnSegundos()
    {
        yield return new WaitForSeconds(tiempoMostrarAviso);
        if (avisoSeleccionPelicula != null)
            avisoSeleccionPelicula.gameObject.SetActive(false);
    }
}
