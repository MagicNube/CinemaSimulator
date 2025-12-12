using UnityEngine;
using UnityEngine.UI;

public class MaquinaDePerritos : MonoBehaviour, IMaquinaReparable
{
    public GameObject itemPrefab;
    public ItemData.TipoDeItem tipoDeCajaRequerida;

    [Header("Configuraci�n de M�quina")]
    [SerializeField] private int capacidadMaxima = 10;
    [SerializeField] private int _capacidadActual = 0;

    [Header("Configuraci�n de Rotura")]
    [Range(0, 100)] public float probabilidadDeRotura = 10f; // 10% de probabilidad
    public bool estaRota = false;
    [SerializeField] private Color colorBarraNormal = Color.yellow;
    [SerializeField] private Color colorBarraRota = Color.red;

    [Header("UI")]
    [SerializeField] private Image barraRellenoImage;

    // 2. A�ADE ESTA PROPIEDAD para cumplir con la interfaz
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
        // 1. Si est� rota, no hacemos nada
        if (estaRota)
        {
            Debug.Log("�La m�quina est� rota! Necesitas repararla.");
            return;
        }

        GameObject itemSujetado = jugador.itemActual;

        // --- CASO 1: MANO VAC�A (Coger Item) ---
        if (itemSujetado == null)
        {
            if (CapacidadActual > 0)
            {
                jugador.AsignarItem(itemPrefab);
                CapacidadActual--;
                Debug.Log("Has cogido un item.");

                // Intentamos romper la m�quina despu�s de usarla
                VerificarRotura();
            }
            else
            {
                Debug.Log("La m�quina est� vac�a. Necesitas rellenarla.");
            }
            return;
        }

        // --- CASO 2: RELLENAR M�QUINA ---
        ItemData data = itemSujetado.GetComponent<ItemData>();
        if (data == null) return;

        if (data.tipoDeItem == tipoDeCajaRequerida)
        {
            CajaDeSuministros cajaScript = itemSujetado.GetComponent<CajaDeSuministros>();

            if (cajaScript == null)
            {
                Debug.LogWarning("Esta caja no tiene script de suministros. Se consumir� entera.");
                CapacidadActual = capacidadMaxima;
                jugador.AsignarItem(null);
                return;
            }

            int espacioLibre = capacidadMaxima - CapacidadActual;
            if (espacioLibre <= 0)
            {
                Debug.Log("�La m�quina ya est� llena!");
                return;
            }

            if (Input.GetMouseButton(1)) // Click Derecho (Llenar todo)
            {
                int cantidadRecibida = cajaScript.SacarSuministros(espacioLibre);
                CapacidadActual += cantidadRecibida;
                if (cantidadRecibida > 0) Debug.Log($"M�quina rellenada. ({CapacidadActual}/{capacidadMaxima})");
                else Debug.Log("�La caja est� vac�a!");
            }
            else // Click Izquierdo (Llenar 1)
            {
                int cantidadRecibida = cajaScript.SacarSuministros(1);
                if (cantidadRecibida > 0)
                {
                    CapacidadActual++;
                    Debug.Log("Has a�adido 1 unidad.");
                }
                else Debug.Log("No queda nada en la caja.");
            }
        }
        else
        {
            Debug.Log("Ese objeto no sirve para esta m�quina.");
        }
    }

    // L�gica para calcular si se rompe
    private void VerificarRotura()
    {
        float randomVal = Random.Range(0f, 100f);
        if (randomVal < probabilidadDeRotura)
        {
            estaRota = true;
            Debug.LogWarning("�CRACK! La m�quina se ha roto.");
            ActualizarBarraVisual(); // Para que se ponga roja inmediatamente
        }
    }

    // M�todo p�blico para llamar desde una herramienta (ej. Llave Inglesa)
    public void Reparar()
    {
        estaRota = false;
        ActualizarBarraVisual();
        Debug.Log("M�quina reparada.");
    }

    private void ActualizarBarraVisual()
    {
        if (barraRellenoImage != null)
        {
            float porcentaje = (float)CapacidadActual / (float)capacidadMaxima;
            barraRellenoImage.fillAmount = porcentaje;

            // Cambiar color seg�n estado
            barraRellenoImage.color = estaRota ? colorBarraRota : colorBarraNormal;
        }
    }
}
