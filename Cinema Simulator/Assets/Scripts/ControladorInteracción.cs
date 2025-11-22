using UnityEngine;
using TMPro;

public class ControladorInteraccion : MonoBehaviour
{
    public float distanciaInteraccion = 3f;
    public Camera camaraJugador;
    public Transform puntoDeAgarre;
    public Animator animadorDelPersonaje;
    public GameObject itemActual;
    public KeyCode teclaSoltar = KeyCode.G;
    private Outline outlineScriptMirado;
    private Transform objetoMirado;
    public TextMeshProUGUI textoAyudaSoltar;

    void Start()
    {
        if (textoAyudaSoltar != null) textoAyudaSoltar.enabled = false;
    }

    void Update()
    {
        Ray ray = new Ray(camaraJugador.transform.position, camaraJugador.transform.forward);
        RaycastHit hit;
        Transform seleccionActual = null;
        Outline outlineActual = null;

        if (Physics.Raycast(ray, out hit, distanciaInteraccion))
        {
            seleccionActual = hit.transform;
            if (PuedeInteractuar(hit.transform))
            {
                outlineActual = hit.collider.GetComponent<Outline>();
            }
        }
        if (outlineScriptMirado != outlineActual)
        {
            if (outlineScriptMirado != null) outlineScriptMirado.enabled = false;
            if (outlineActual != null) outlineActual.enabled = true;
            outlineScriptMirado = outlineActual;
        }
        objetoMirado = seleccionActual;

        // --- CAMBIO 1: Detectamos Click Izquierdo (0) O Derecho (1) ---
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (objetoMirado != null)
            {
                // Prioridad: Pedidos (solo click izquierdo normalmente, pero lo dejamos pasar)
                GestorPedidos cliente = objetoMirado.GetComponent<GestorPedidos>();
                if (cliente != null && Input.GetMouseButtonDown(0))
                {
                    ItemData itemDataEnMano = (itemActual != null) ? itemActual.GetComponent<ItemData>() : null;
                    if (cliente.RecibirItem(itemDataEnMano)) DestruirItem();
                    return;
                }

                // Maquinas complejas
                if (objetoMirado.GetComponent<MaquinaDePalomitas>() != null) { objetoMirado.GetComponent<MaquinaDePalomitas>().Interactuar(this); return; }
                if (objetoMirado.GetComponent<MaquinaDeBebidas>() != null) { objetoMirado.GetComponent<MaquinaDeBebidas>().Interactuar(this); return; }

                // --- CAMBIO 2: Usamos Interactuar en lugar de CogerItem ---
                if (objetoMirado.GetComponent<MaquinaDeItems>() != null) { objetoMirado.GetComponent<MaquinaDeItems>().Interactuar(this); return; }

                // Interacciones simples (Solo click izquierdo)
                if (Input.GetMouseButtonDown(0))
                {
                    if (objetoMirado.GetComponent<Papelera>() != null) { DestruirItem(); return; }
                    if (objetoMirado.GetComponent<CampanaInteractiva>() != null) { objetoMirado.GetComponent<CampanaInteractiva>().Interactuar(); return; }
                    if (objetoMirado.GetComponent<ItemData>() != null) { CogerItemDelSuelo(objetoMirado.gameObject); return; }
                }
            }
        }
        if (Input.GetKeyDown(teclaSoltar)) { SoltarItemAlSuelo(); }
    }

    bool PuedeInteractuar(Transform objeto)
    {
        // --- Palomitas ---
        if (objeto.GetComponent<MaquinaDePalomitas>() != null)
        {
            if (itemActual == null) return true; // Ver estado
            ItemData data = itemActual.GetComponent<ItemData>();
            if (data == null) return false;

            // Obtenemos la referencia a la máquina para saber qué caja pide
            MaquinaDePalomitas maquina = objeto.GetComponent<MaquinaDePalomitas>();

            // 1. Si es un Cubo Vacío -> OK
            // 2. O SI es la caja que pide la máquina (leyendo la variable) -> OK
            return (data.tipoDeItem == ItemData.TipoDeItem.CuboVacio ||
                    data.tipoDeItem == maquina.tipoDeCajaRequerida);
        }
        // --- Bebidas ---
        if (objeto.GetComponent<MaquinaDeBebidas>() != null)
        {
            if (itemActual == null) return true;
            ItemData data = itemActual.GetComponent<ItemData>();
            if (data == null) return false;
            return (data.tipoDeItem == ItemData.TipoDeItem.VasoVacio);
            // NOTA: Aquí deberías añadir "|| data.tipoDeItem == ItemData.TipoDeItem.CajaBebidas" si quieres recargarla igual
        }
        // --- CAMBIO 3: Maquina De Items Genérica ---
        if (objeto.GetComponent<MaquinaDeItems>() != null)
        {
            // Permitimos interactuar si no tenemos nada (para coger)
            if (itemActual == null) return true;

            // O si tenemos la caja correcta (para rellenar)
            ItemData data = itemActual.GetComponent<ItemData>();
            MaquinaDeItems maquina = objeto.GetComponent<MaquinaDeItems>();
            if (data != null && data.tipoDeItem == maquina.tipoDeCajaRequerida) return true;

            return false;
        }

        // --- Resto de interacciones ---
        if (objeto.GetComponent<Papelera>() != null) { return (itemActual != null); }
        if (objeto.GetComponent<CampanaInteractiva>() != null) { return true; }
        if (objeto.GetComponent<ItemData>() != null) { return (itemActual == null); }
        if (objeto.GetComponent<PedidoCliente>() != null) { return true; }

        return false;
    }

    // ... (El resto de métodos AsignarItem, CogerItemDelSuelo, DestruirItem siguen igual) ...
    public void AsignarItem(GameObject nuevoItemPrefab)
    {
        if (itemActual != null) { Destroy(itemActual); itemActual = null; }
        if (nuevoItemPrefab == null) return; // Manejo de null para destruir items (cajas)

        itemActual = Instantiate(nuevoItemPrefab);
        ItemData data = itemActual.GetComponent<ItemData>();
        itemActual.transform.parent = puntoDeAgarre;
        itemActual.transform.localPosition = Vector3.zero;
        itemActual.transform.localRotation = Quaternion.identity;
        if (data != null) { itemActual.transform.localScale = data.escalaOriginal; }
        if (animadorDelPersonaje != null) { animadorDelPersonaje.SetBool("estaSujetando", true); }

        // Lógica del texto de ayuda
        if (textoAyudaSoltar != null)
        {
            if (data == null || data.tipoDeItem != ItemData.TipoDeItem.Ticket)
                textoAyudaSoltar.enabled = true;
        }
    }

    // He añadido este método auxiliar que usabas antes por si acaso, pero ya no se llama directamente desde MaquinaDeItems
    void CogerItem(GameObject prefabDelItem)
    {
        if (itemActual != null) { Debug.Log("Ya tienes un item. Tíralo primero."); return; }
        AsignarItem(prefabDelItem);
    }

    // ... resto de métodos (CogerItemDelSuelo, SoltarItemAlSuelo, DestruirItem) mantenlos igual ...
    void CogerItemDelSuelo(GameObject itemObject)
    {
        if (itemActual != null) return;
        Rigidbody rb = itemObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        itemObject.transform.parent = puntoDeAgarre;
        itemObject.transform.localPosition = Vector3.zero;
        itemObject.transform.localRotation = Quaternion.identity;
        ItemData data = itemObject.GetComponent<ItemData>();
        if (data != null) { itemObject.transform.localScale = data.escalaOriginal; }
        itemActual = itemObject;
        if (animadorDelPersonaje != null) { animadorDelPersonaje.SetBool("estaSujetando", true); }
        if (textoAyudaSoltar != null)
        {
            if (data == null || data.tipoDeItem != ItemData.TipoDeItem.Ticket)
            {
                textoAyudaSoltar.enabled = true;
            }
        }
    }

    void SoltarItemAlSuelo()
    {
        if (itemActual == null) return;
        ItemData data = itemActual.GetComponent<ItemData>();
        if (data != null && data.tipoDeItem == ItemData.TipoDeItem.Ticket)
        {
            Debug.Log("No puedes soltar este item.");
            return;
        }

        if (animadorDelPersonaje != null) { animadorDelPersonaje.SetBool("estaSujetando", false); }
        Rigidbody rb = itemActual.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
        itemActual.transform.parent = null;
        itemActual = null;
        if (textoAyudaSoltar != null) { textoAyudaSoltar.enabled = false; }
    }

    public void DestruirItem()
    {
        if (itemActual == null) return;

        Destroy(itemActual);
        itemActual = null;
        if (animadorDelPersonaje != null) { animadorDelPersonaje.SetBool("estaSujetando", false); }
        Debug.Log("Has tirado el item.");
        if (textoAyudaSoltar != null) { textoAyudaSoltar.enabled = false; }
    }
}