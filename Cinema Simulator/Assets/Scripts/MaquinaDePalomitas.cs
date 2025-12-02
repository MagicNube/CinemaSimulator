using UnityEngine;
using UnityEngine.UI;

public class MaquinaDePalomitas : MonoBehaviour
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

    [SerializeField] private Image barraRellenoImage;

    [Range(0, 100)] public float probabilidadDeRotura = 10f;
    public bool estaRota = false; // Tu variable existente

    // 2. AÑADE ESTA PROPIEDAD para cumplir con la interfaz
    public bool EstaRota => estaRota;


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
            Debug.Log("La máquina de palomitas echa humo... Está rota.");
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
                Debug.LogWarning("Esta caja no tiene script de suministros.");
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

        // --- SERVIR PALOMITAS ---
        if (data.tipoDeItem == ItemData.TipoDeItem.CuboVacio)
        {
            if (CapacidadActual > 0)
            {
                if (data.prefabItemLleno != null)
                {
                    jugador.AsignarItem(data.prefabItemLleno);
                    CapacidadActual--;

                    // Solo comprobamos rotura al servir, no al rellenar
                    VerificarRotura();
                }
            }
            else
            {
                Debug.Log("No quedan palomitas.");
            }
        }
    }

    private void VerificarRotura()
    {
        if (Random.Range(0f, 100f) < probabilidadDeRotura)
        {
            estaRota = true;
            Debug.LogWarning("¡La máquina de palomitas se ha atascado!");
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