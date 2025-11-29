using UnityEngine;
// IMPORTANTE: Necesitamos esta línea para trabajar con elementos de UI
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
            // Aseguramos que no baje de 0 ni suba del máximo
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

        // --- LÓGICA DE RELLENADO (Usando una Caja) ---
        if (data.tipoDeItem == tipoDeCajaRequerida)
        {
            if (Input.GetMouseButton(1)) // Click Derecho
            {
                CapacidadActual = capacidadMaxima;
                jugador.AsignarItem(null);
            }
            else // Click Izquierdo
            {
                if (CapacidadActual < capacidadMaxima) CapacidadActual++; 
            }
            return;
        }

        // --- LÓGICA DE SERVIDO (Usando Cubo Vacío) ---
        //TODO: Falta añadir los tamaños de palomitas en la capacidad
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