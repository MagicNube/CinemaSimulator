using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    [Header("Interfaz UI")]
    public Image imagenAyudaSoltar;

    [Header("Control de Movimiento")]
    public MonoBehaviour scriptMovimiento;

    void Start()
    {
        if (imagenAyudaSoltar != null)
        {
            imagenAyudaSoltar.enabled = false;
        }
    }

    void Update()
    {
        Ray ray = new Ray(camaraJugador.transform.position, camaraJugador.transform.forward);
        RaycastHit hit;
        Transform seleccionActual = null;
        Outline outlineActual = null;

        // ESTE BLOQUE CONTROLA SOLO EL "OUTLINE" (EL BORDE BRILLANTE)
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

        // --- DETECCIÓN DE CLICK ---
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            // Bloqueo de UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (objetoMirado != null)
            {
                // Prioridad: Pedidos
                GestorPedidos cliente = objetoMirado.GetComponent<GestorPedidos>();
                if (cliente != null && Input.GetMouseButtonDown(0))
                {
                    ItemData itemDataEnMano = (itemActual != null) ? itemActual.GetComponent<ItemData>() : null;
                    if (cliente.RecibirItem(itemDataEnMano)) DestruirItem();
                    return;
                }

                // Tablet
                if (objetoMirado.GetComponent<TabletManager>() != null)
                {
                    objetoMirado.GetComponent<TabletManager>().AbrirTablet(this);
                    return;
                }

                // Maquinas complejas
                if (objetoMirado.GetComponent<MaquinaDePalomitas>() != null) { objetoMirado.GetComponent<MaquinaDePalomitas>().Interactuar(this); return; }
                if (objetoMirado.GetComponent<MaquinaDeBebidas>() != null) { objetoMirado.GetComponent<MaquinaDeBebidas>().Interactuar(this); return; }

                // Maquina De Items Genérica
                if (objetoMirado.GetComponent<MaquinaDeItems>() != null) { objetoMirado.GetComponent<MaquinaDeItems>().Interactuar(this); return; }

                // Interacciones simples
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
            MaquinaDePalomitas maquina = objeto.GetComponent<MaquinaDePalomitas>();
            return (data.tipoDeItem == ItemData.TipoDeItem.CuboVacio ||
                    data.tipoDeItem == maquina.tipoDeCajaRequerida);
        }
        // --- Bebidas ---
        if (objeto.GetComponent<MaquinaDeBebidas>() != null)
        {
            if (itemActual == null) return true;
            ItemData data = itemActual.GetComponent<ItemData>();
            if (data == null) return false;
            MaquinaDeBebidas maquina = objeto.GetComponent<MaquinaDeBebidas>();
            return (data.tipoDeItem == ItemData.TipoDeItem.VasoVacio ||
                    data.tipoDeItem == maquina.tipoDeCajaRequerida);
        }
        // --- Maquina De Items Genérica ---
        if (objeto.GetComponent<MaquinaDeItems>() != null)
        {
            if (itemActual == null) return true;
            ItemData data = itemActual.GetComponent<ItemData>();
            MaquinaDeItems maquina = objeto.GetComponent<MaquinaDeItems>();
            if (data != null && data.tipoDeItem == maquina.tipoDeCajaRequerida) return true;
            return false;
        }
        //Tablet
        if (objeto.GetComponent<TabletManager>() != null) return true;

        // --- Resto de interacciones ---
        if (objeto.GetComponent<Papelera>() != null) { return (itemActual != null); }
        if (objeto.GetComponent<CampanaInteractiva>() != null) { return true; }
        if (objeto.GetComponent<ItemData>() != null) { return (itemActual == null); }
        if (objeto.GetComponent<PedidoCliente>() != null) { return true; }

        return false;
    }

    public void AsignarItem(GameObject nuevoItemPrefab)
    {
        if (itemActual != null) { Destroy(itemActual); itemActual = null; }
        if (nuevoItemPrefab == null) return;

        itemActual = Instantiate(nuevoItemPrefab);
        ItemData data = itemActual.GetComponent<ItemData>();
        itemActual.transform.parent = puntoDeAgarre;
        itemActual.transform.localPosition = Vector3.zero;
        itemActual.transform.localRotation = Quaternion.identity;
        if (data != null) { itemActual.transform.localScale = data.escalaOriginal; }
        if (animadorDelPersonaje != null) { animadorDelPersonaje.SetBool("estaSujetando", true); }

        if (imagenAyudaSoltar != null)
        {
            if (data == null || data.tipoDeItem != ItemData.TipoDeItem.Ticket)
                imagenAyudaSoltar.enabled = true;
        }
    }

    void CogerItem(GameObject prefabDelItem)
    {
        if (itemActual != null) { Debug.Log("Ya tienes un item. Tíralo primero."); return; }
        AsignarItem(prefabDelItem);
    }

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
        if (imagenAyudaSoltar != null)
        {
            if (data == null || data.tipoDeItem != ItemData.TipoDeItem.Ticket)
            {
                imagenAyudaSoltar.enabled = true;
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

        if (data != null)
        {
            itemActual.transform.localScale = data.escalaOriginal;
        }

        itemActual = null;
        if (imagenAyudaSoltar != null) { imagenAyudaSoltar.enabled = false; }
    }

    // --- AQUÍ ESTABA LA CLAVE ---
    public void DestruirItem()
    {
        if (itemActual == null) return;

        // 1. ANTES de destruir el objeto, avisamos al Manager del Minijuego
        // El Manager comprobará si este objeto era basura que había que limpiar
        if (MinijuegoLimpiezaManager.Instance != null)
        {
            MinijuegoLimpiezaManager.Instance.ObjetoRecogido(itemActual);
        }

        // 2. Ahora sí, lo destruimos
        Destroy(itemActual);
        itemActual = null;

        if (animadorDelPersonaje != null) { animadorDelPersonaje.SetBool("estaSujetando", false); }
        Debug.Log("Has tirado el item.");
        if (imagenAyudaSoltar != null) { imagenAyudaSoltar.enabled = false; }
    }

    public void AlternarControlJugador(bool activo)
    {
        if (scriptMovimiento != null) scriptMovimiento.enabled = activo;
    }
}