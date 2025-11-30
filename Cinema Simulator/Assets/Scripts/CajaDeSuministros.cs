using UnityEngine;
using TMPro;

public class CajaDeSuministros : MonoBehaviour
{
    public int cantidadActual = 50;
    public TextMeshProUGUI textoCantidad;

    void Start()
    {
        ActualizarTexto();
    }

    public int SacarSuministros(int cantidadSolicitada)
    {
        if (cantidadActual <= 0) return 0; // Si ya está vacía, no damos nada

        int cantidadADar = 0;

        if (cantidadActual >= cantidadSolicitada)
        {
            cantidadADar = cantidadSolicitada;
            cantidadActual -= cantidadSolicitada;
        }
        else
        {
            cantidadADar = cantidadActual;
            cantidadActual = 0;
        }

        ActualizarTexto();
        return cantidadADar;
    }

    public bool EstaVacia()
    {
        return cantidadActual <= 0;
    }

    private void ActualizarTexto()
    {
        if (textoCantidad != null)
        {
            textoCantidad.text = cantidadActual.ToString();

            // --- CAMBIO VISUAL ---
            if (cantidadActual <= 0)
            {
                textoCantidad.color = Color.red; // Texto rojo si está vacía
                textoCantidad.text = "VACÍA";    // Opcional: cambiar el número por texto
            }
            else
            {
                textoCantidad.color = Color.white; // O el color original que tuvieras
            }
        }
    }
}