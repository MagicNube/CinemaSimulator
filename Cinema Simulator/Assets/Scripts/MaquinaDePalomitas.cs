using UnityEngine;
// IMPORTANTE: Necesitamos esta l�nea para trabajar con elementos de UI
using UnityEngine.UI;

public class MaquinaDePalomitas : MonoBehaviour
{
    public ItemData.TipoDeItem tipoDeCajaRequerida;

    private int capacidadMaxima = 10;
    // Hacemos que se vea en el inspector para debug, pero no editable
    [SerializeField] private int _capacidadActual = 0;

    [SerializeField] private Image barraRellenoImage;

    // Propiedad para asegurar que la barra se actualiza siempre que cambie la variable
    public int CapacidadActual
    {
        get { return _capacidadActual; }
        private set
        {
            // Aseguramos que no baje de 0 ni suba del m�ximo
            _capacidadActual = Mathf.Clamp(value, 0, capacidadMaxima);
            ActualizarBarraVisual();
        }
    }

    private void Start()
    {
        // Inicializamos la barra visual al empezar
        ActualizarBarraVisual();
    }

    public void Interactuar(ControladorInteraccion jugador)
    {
        GameObject itemSujetado = jugador.itemActual;

        if (itemSujetado == null)
        {
            // Feedback opcional si clickas sin nada
            Debug.Log($"Estado: {CapacidadActual}/{capacidadMaxima}");
            return;
        }

        ItemData data = itemSujetado.GetComponent<ItemData>();
        if (data == null) return;

        if (data.tipoDeItem == tipoDeCajaRequerida)
        {
            // Obtenemos el script de la caja para gestionar cantidades
            CajaDeSuministros cajaScript = itemSujetado.GetComponent<CajaDeSuministros>();

            // Si la caja no tiene el script, usamos la lógica antigua (rellenar todo y destruir)
            if (cajaScript == null)
            {
                Debug.LogWarning("Esta caja no tiene script de suministros. Se consumirá entera.");
                CapacidadActual = capacidadMaxima;
                jugador.AsignarItem(null);
                return;
            }

            // Calculamos cuánto espacio libre tiene la máquina
            int espacioLibre = capacidadMaxima - CapacidadActual;

            if (espacioLibre <= 0)
            {
                Debug.Log("¡La máquina ya está llena!");
                return;
            }

            // Click Derecho: Intentar llenar al MÁXIMO
            if (Input.GetMouseButton(1))
            {
                int cantidadRecibida = cajaScript.SacarSuministros(espacioLibre);
                CapacidadActual += cantidadRecibida;

                if (cantidadRecibida > 0)
                    Debug.Log($"Máquina rellenada. ({CapacidadActual}/{capacidadMaxima})");
                else
                    Debug.Log("¡La caja está vacía! Tírala a la papelera.");
            }
            // Click Izquierdo: Rellenar solo 1 unidad
            else
            {
                int cantidadRecibida = cajaScript.SacarSuministros(1);

                if (cantidadRecibida > 0)
                {
                    CapacidadActual++;
                    Debug.Log("Has añadido 1 unidad.");
                }
                else
                {
                    Debug.Log("No queda nada en la caja.");
                }
            }
        }

        // --- L�GICA DE SERVIDO (Usando Cubo Vac�o) ---
        //TODO: Falta a�adir los tama�os de palomitas en la capacidad
        if (data.tipoDeItem == ItemData.TipoDeItem.CuboVacio)
        {
            if (CapacidadActual > 0)
            {
                if (data.prefabItemLleno != null)
                {
                    jugador.AsignarItem(data.prefabItemLleno);
                    CapacidadActual--; // Usamos la propiedad
                }
            }
        }
    }

    private void ActualizarBarraVisual()
    {
        if (barraRellenoImage != null)
        {
            float porcentaje = (float)CapacidadActual / (float)capacidadMaxima;
            barraRellenoImage.fillAmount = porcentaje;
        }
    }
}