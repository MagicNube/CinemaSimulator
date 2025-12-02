using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(Collider))]
public class MaquinaDeBebidas : MonoBehaviour, IMaquinaReparable
{
    public ItemData.TipoDeItem tipoDeCajaRequerida;

    [Header("Configuración de Máquina")]
    [SerializeField] private int capacidadMaxima = 10;
    [SerializeField] private int _capacidadActual = 0;

    [Header("Configuración de Rotura")]
    [Range(0, 100)] public float probabilidadDeRotura = 10f;
    public bool estaRota = false;
    [SerializeField] private Color colorBarraNormal = Color.white;
    [SerializeField] private Color colorBarraRota = Color.red;

    // 2. AÑADE ESTA PROPIEDAD para cumplir con la interfaz
    public bool EstaRota => estaRota;

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
        // 1. Chequeo de rotura
        if (estaRota)
        {
            Debug.Log("Grifo roto. Llama a mantenimiento.");
            return;
        }

        GameObject itemSujetado = jugador.itemActual;

        if (itemSujetado == null)
        {
            Debug.Log($"Estado: {CapacidadActual}/{capacidadMaxima}");
            return;
        }

        ItemData data = itemSujetado.GetComponent<ItemData>();
        if (data == null) return;

        // --- RELLENAR ---
        if (data.tipoDeItem == tipoDeCajaRequerida)
        {
            CajaDeSuministros cajaScript = itemSujetado.GetComponent<CajaDeSuministros>();
            if (cajaScript == null)
            {
                Debug.LogWarning("Caja sin script. Se consume entera.");
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
            }
            else
            {
                int cantidadRecibida = cajaScript.SacarSuministros(1);
                if (cantidadRecibida > 0) CapacidadActual++;
            }
        }

        // --- SERVIR BEBIDA ---
        if (data.tipoDeItem == ItemData.TipoDeItem.VasoVacio)
        {
            if (CapacidadActual > 0)
            {
                if (data.prefabItemLleno != null)
                {
                    jugador.AsignarItem(data.prefabItemLleno);
                    CapacidadActual--;

                    // Comprobamos si se rompe al servir
                    VerificarRotura();
                }
            }
            else
            {
                Debug.Log("La máquina de bebidas está vacía.");
            }
            return;
        }
    }

    private void VerificarRotura()
    {
        if (Random.Range(0f, 100f) < probabilidadDeRotura)
        {
            estaRota = true;
            Debug.LogWarning("¡El dispensador de bebidas ha fallado!");
            ActualizarBarraVisual();
        }
    }

    public void Reparar()
    {
        estaRota = false;
        ActualizarBarraVisual();
        Debug.Log("Máquina reparada.");
    }

    private void ActualizarBarraVisual()
    {
        if (barraRellenoImage != null)
        {
            float porcentaje = (float)CapacidadActual / (float)capacidadMaxima;
            barraRellenoImage.fillAmount = porcentaje;

            // Lógica de color
            barraRellenoImage.color = estaRota ? colorBarraRota : colorBarraNormal;
        }
    }
}