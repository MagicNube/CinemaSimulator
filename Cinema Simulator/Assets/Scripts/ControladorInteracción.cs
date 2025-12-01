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

    // --- [VARIABLES DEL FANTASMA Y SNAP] ---
    [Header("Feedback Visual Fantasma")]
    [Tooltip("Asigna aquí el Prefab de la Caja Fantasma (transparente)")]
    public GameObject ghostPrefab;

    private MeshRenderer currentGhostRenderer = null; // El MeshRenderer del fantasma visible
    // ------------------------------------------

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

        // --- INICIO DE LA LÓGICA DE Detección (Raycast) ---
        MeshRenderer nextGhostRenderer = null;

        if (Physics.Raycast(ray, out hit, distanciaInteraccion))
        {
            seleccionActual = hit.transform;

            // 1. Detección del Fantasma (GHOST_BOX)
            if (hit.collider.CompareTag("GHOST_BOX"))
            {
                // Si golpeamos el Collider de un fantasma, obtenemos su MeshRenderer para controlarlo.
                nextGhostRenderer = hit.collider.GetComponent<MeshRenderer>();
            }

            // Lógica original de Outline
            if (PuedeInteractuar(hit.transform))
            {
                outlineActual = hit.collider.GetComponent<Outline>();
            }
        }

        // 2. Control de Visibilidad del Fantasma
        if (currentGhostRenderer != nextGhostRenderer)
        {
            if (currentGhostRenderer != null) currentGhostRenderer.enabled = false;
            if (nextGhostRenderer != null) nextGhostRenderer.enabled = true;
            currentGhostRenderer = nextGhostRenderer;
        }

        // Bloque original del Outline
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

                // --- NUEVA PRIORIDAD: COLOCAR CAJA EN FANTASMA/ANCLAJE (SNAP) ---
                // Solo si llevamos un item y apuntamos a un fantasma
                if (itemActual != null && objetoMirado.CompareTag("GHOST_BOX") && Input.GetMouseButtonDown(0))
                {
                    SnapItemToGhost(itemActual, objetoMirado.gameObject);
                    return;
                }
                // --------------------------------------------------------

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

    // -----------------------------------------------------------------------------------------------------
    // --- NUEVOS MÉTODOS AUXILIARES Y MODIFICACIONES ---
    // -----------------------------------------------------------------------------------------------------

    public void SnapItemToGhost(GameObject carriedItem, GameObject ghostBox)
    {
        // El anclaje es el padre del fantasma, que es donde queremos que quede la caja real.
        // Asumimos que el objeto padre tiene la rotación y posición final deseada (ANCHOR_POINT)
        Transform anchor = ghostBox.transform.parent;

        // 1. Desconectar el ítem del jugador
        carriedItem.transform.parent = null;

        // 2. Mover, Rotar y Anclar el ítem al punto exacto
        carriedItem.transform.SetParent(anchor);
        carriedItem.transform.localPosition = Vector3.zero; // Coincide con la posición del anchor
        carriedItem.transform.localRotation = Quaternion.identity; // Coincide con la rotación del anchor

        // 3. Deshabilitar la física
        Rigidbody rb = carriedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // 4. Destruir el objeto fantasma (para liberar la referencia y marcar el espacio como ocupado)
        Destroy(ghostBox);

        // 5. Limpiar el estado del jugador
        currentGhostRenderer = null;
        itemActual = null;
        if (animadorDelPersonaje != null) { animadorDelPersonaje.SetBool("estaSujetando", false); }
        if (imagenAyudaSoltar != null) { imagenAyudaSoltar.enabled = false; }

        Debug.Log($"Caja colocada y anclada en {anchor.name}.");
    }

    void CogerItemDelSuelo(GameObject itemObject)
    {
        if (itemActual != null) return;

        // 1. GUARDAMOS EL PADRE (ANCLAJE) antes de que el objeto se mueva a la mano.
        Transform parentAnchor = itemObject.transform.parent;

        Rigidbody rb = itemObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // 2. CONECTAR EL OBJETO A LA MANO DEL JUGADOR
        itemObject.transform.parent = puntoDeAgarre;
        itemObject.transform.localPosition = Vector3.zero;
        itemObject.transform.localRotation = Quaternion.identity;

        ItemData data = itemObject.GetComponent<ItemData>();
        if (data != null) { itemObject.transform.localScale = data.escalaOriginal; }

        itemActual = itemObject;

        if (animadorDelPersonaje != null) { animadorDelPersonaje.SetBool("estaSujetando", true); }

        // --- LÓGICA DE REGENERACIÓN DEL FANTASMA ---

        // 3. Verificamos si la caja venía de un ANCLAJE válido.
        if (parentAnchor != null && parentAnchor.CompareTag("ANCHOR_POINT"))
        {
            // 4. Instanciamos el fantasma de nuevo en la posición del anclaje.
            // Es crucial hacerlo hijo del Anchor para que ocupe ese Transform.
            Instantiate(ghostPrefab, parentAnchor.position, parentAnchor.rotation, parentAnchor);

            Debug.Log($"Espacio liberado. Fantasma recreado en {parentAnchor.name}.");
        }
        // ------------------------------------------

        if (imagenAyudaSoltar != null)
        {
            if (data == null || data.tipoDeItem != ItemData.TipoDeItem.Ticket)
            {
                imagenAyudaSoltar.enabled = true;
            }
        }
    }

    // ... (El resto de métodos sin cambios: Puedes ignorar la lógica de abajo si ya la tienes en tu archivo) ...

    bool PuedeInteractuar(Transform objeto)
    {
        // Esta lógica define si el Outline (borde brillante) debe aparecer.
        // Si quieres que el Outline aparezca en el fantasma cuando llevas una caja,
        // añade: if (objeto.CompareTag("GHOST_BOX")) return (itemActual != null);

        if (objeto.GetComponent<MaquinaDePalomitas>() != null)
        {
            if (itemActual == null) return true;
            ItemData data = itemActual.GetComponent<ItemData>();
            if (data == null) return false;

            MaquinaDePalomitas maquina = objeto.GetComponent<MaquinaDePalomitas>();
            return (data.tipoDeItem == ItemData.TipoDeItem.CuboVacio || data.tipoDeItem == maquina.tipoDeCajaRequerida);
        }
        if (objeto.GetComponent<MaquinaDeBebidas>() != null)
        {
            if (itemActual == null) return true;
            ItemData data = itemActual.GetComponent<ItemData>();
            if (data == null) return false;

            MaquinaDeBebidas maquina = objeto.GetComponent<MaquinaDeBebidas>();
            return (data.tipoDeItem == ItemData.TipoDeItem.VasoVacio || data.tipoDeItem == maquina.tipoDeCajaRequerida);
        }
        if (objeto.GetComponent<MaquinaDeItems>() != null)
        {
            if (itemActual == null) return true;
            ItemData data = itemActual.GetComponent<ItemData>();
            MaquinaDeItems maquina = objeto.GetComponent<MaquinaDeItems>();
            if (data != null && data.tipoDeItem == maquina.tipoDeCajaRequerida) return true;
            return false;
        }
        if (objeto.GetComponent<TabletManager>() != null) return true;

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
