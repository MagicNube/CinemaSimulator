using UnityEngine;
using UnityEngine.UI;

public class MaquinaDeItems : MonoBehaviour
{
    public GameObject itemPrefab;
    public ItemData.TipoDeItem tipoDeCajaRequerida;

    [Header("Configuración de Máquina")]
    [SerializeField] private int capacidadMaxima = 10;
    [SerializeField] private int _capacidadActual = 0;

    [SerializeField] private Color colorBarraNormal = Color.yellow;
    [SerializeField] private Color colorBarraRota = Color.red;

    [Header("UI")]
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
                Debug.Log("La máquina está vacía. Necesitas rellenarla.");
            }
            return;
        }

        ItemData data = itemSujetado.GetComponent<ItemData>();
        if (data == null) return;

        if (data.tipoDeItem == tipoDeCajaRequerida)
        {
            CajaDeSuministros cajaScript = itemSujetado.GetComponent<CajaDeSuministros>();

            if (cajaScript == null)
            {
                Debug.LogWarning("Esta caja no tiene script de suministros. Se consumirá entera.");
                CapacidadActual = capacidadMaxima;
                jugador.AsignarItem(null);
                return;
            }

            int espacioLibre = capacidadMaxima - CapacidadActual;
            if (espacioLibre <= 0)
            {
                Debug.Log("¡La máquina ya está llena!");
                return;
            }

            if (Input.GetMouseButton(1))
            {
                int cantidadRecibida = cajaScript.SacarSuministros(espacioLibre);
                CapacidadActual += cantidadRecibida;
                if (cantidadRecibida > 0) Debug.Log($"Máquina rellenada. ({CapacidadActual}/{capacidadMaxima})");
                else Debug.Log("¡La caja está vacía!");
            }
            else
            {
                int cantidadRecibida = cajaScript.SacarSuministros(1);
                if (cantidadRecibida > 0)
                {
                    CapacidadActual++;
                    Debug.Log("Has añadido 1 unidad.");
                }
                else Debug.Log("No queda nada en la caja.");
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
            barraRellenoImage.color = colorBarraNormal;
        }
    }
}