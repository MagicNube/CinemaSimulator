using UnityEngine;
using UnityEngine.UI; // Necesario para la barra

public class MaquinaDeItems : MonoBehaviour
{
    public GameObject itemPrefab; // El objeto que el jugador recoge (ej. Vaso Vacio)

    // Qué tipo de caja se necesita para rellenar esta máquina específica
    public ItemData.TipoDeItem tipoDeCajaRequerida;

    [SerializeField] private int capacidadMaxima = 10;
    [SerializeField] private int _capacidadActual = 0;

    [SerializeField] private Image barraRellenoImage;

    public int CapacidadActual
    {
        get { return _capacidadActual; }
        private set
        {
            _capacidadActual = Mathf.Clamp(value, 0, capacidadMaxima);
            ActualizarBarraVisual();
        }
    }

    private void Start()
    {
        ActualizarBarraVisual();
    }

    public void Interactuar(ControladorInteraccion jugador)
    {
        GameObject itemSujetado = jugador.itemActual;

        // --- CASO 1: MANO VACÍA (El jugador quiere coger un item) ---
        if (itemSujetado == null)
        {
            if (CapacidadActual > 0)
            {
                jugador.AsignarItem(itemPrefab);
                CapacidadActual--;
                Debug.Log("Has cogido un item.");
            }
            else
            {
                Debug.Log("¡La máquina está vacía! Necesitas rellenarla.");
            }
            return;
        }

        // --- CASO 2: EL JUGADOR TIENE ALGO EN LA MANO (¿Es una caja?) ---
        ItemData data = itemSujetado.GetComponent<ItemData>();
        if (data == null) return;

        // Comprobamos si el item es la caja CORRECTA para esta máquina
        if (data.tipoDeItem == tipoDeCajaRequerida)
        {
            // Detectar Click Derecho (Rellenar todo)
            // Nota: El click derecho se detecta gracias al cambio en ControladorInteraccion
            if (Input.GetMouseButton(1))
            {
                CapacidadActual = capacidadMaxima;
                Debug.Log("¡Máquina rellenada al máximo! Caja consumida.");
                jugador.AsignarItem(null); // Destruir la caja
            }
            // Click Izquierdo (Rellenar 1 unidad)
            else
            {
                if (CapacidadActual < capacidadMaxima)
                {
                    CapacidadActual++;
                    Debug.Log($"Has rellenado 1 unidad. ({CapacidadActual}/{capacidadMaxima})");
                }
                else
                {
                    Debug.Log("La máquina ya está llena.");
                }
            }
        }
        else
        {
            Debug.Log("Ese objeto no sirve para esta máquina.");
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