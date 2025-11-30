using UnityEngine;
using UnityEngine.UI; // Necesario para la barra

public class MaquinaDeItems : MonoBehaviour
{
    public GameObject itemPrefab; // El objeto que el jugador recoge (ej. Vaso Vacio)

    // Qu� tipo de caja se necesita para rellenar esta m�quina espec�fica
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

        // --- CASO 1: MANO VAC�A (El jugador quiere coger un item) ---
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
                Debug.Log("�La m�quina est� vac�a! Necesitas rellenarla.");
            }
            return;
        }

        // --- CASO 2: EL JUGADOR TIENE ALGO EN LA MANO (�Es una caja?) ---
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
        else
        {
            Debug.Log("Ese objeto no sirve para esta m�quina.");
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