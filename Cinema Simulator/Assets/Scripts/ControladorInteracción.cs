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

    [System.Obsolete]
    void Update()
    {
        Ray ray = new Ray(camaraJugador.transform.position, camaraJugador.transform.forward);
        RaycastHit hit;
        Transform seleccionActual = null;
        Outline outlineActual = null;

        MeshRenderer nextGhostRenderer = null;

        if (Physics.Raycast(ray, out hit, distanciaInteraccion))
        {
            seleccionActual = hit.transform;

            // 1. Detección del Fantasma (GHOST_BOX)
            if (hit.collider.CompareTag("GHOST_BOX"))
            {
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

                //Boton cambiador de fase
                if (objetoMirado.GetComponent<CambiadorFase>() != null)
                {
                    if (!TransitionManager.Instance.transicionando)
                    {
                        objetoMirado.GetComponent<CambiadorFase>().Interactuar();
                    }
                    return;
                }

                // Maquinas complejas
                if (objetoMirado.GetComponent<MaquinaDePalomitas>() != null) { objetoMirado.GetComponent<MaquinaDePalomitas>().Interactuar(this); return; }
                if (objetoMirado.GetComponent<MaquinaDeBebidas>() != null) { objetoMirado.GetComponent<MaquinaDeBebidas>().Interactuar(this); return; }

                // Maquina De Items Genérica
                if (objetoMirado.GetComponent<MaquinaDeItems>() != null) { objetoMirado.GetComponent<MaquinaDeItems>().Interactuar(this); return; }

                // --- PRIORIDAD: COLOCAR CAJA EN FANTASMA/ANCLAJE (SNAP) ---
                if (itemActual != null && objetoMirado.CompareTag("GHOST_BOX") && Input.GetMouseButtonDown(0))
                {
                    ItemData carriedData = itemActual.GetComponent<ItemData>();

                    // Verificación de Tipo: SÓLO si es uno de los tipos de caja de recarga
                    if (carriedData != null)
                    {
                        ItemData.TipoDeItem itemType = carriedData.tipoDeItem;

                        if (itemType == ItemData.TipoDeItem.CajaPalomitas ||
                            itemType == ItemData.TipoDeItem.CajaBebidas ||
                            itemType == ItemData.TipoDeItem.CajaEnvasesPalomitas ||
                            itemType == ItemData.TipoDeItem.CajaEnvasesBebidas ||
                            itemType == ItemData.TipoDeItem.CajaPerritos)
                        {
                            SnapItemToGhost(itemActual, objetoMirado.gameObject);
                            return; // Consumir el click
                        }
                    }
                }
                // --------------------------------------------------------

                LightSwitch lightSwitch = objetoMirado.GetComponent<LightSwitch>();

                if (lightSwitch != null && Input.GetMouseButtonDown(0))
                {
                    lightSwitch.Interact(); // Llama al método Interact del script LightSwitch
                    return; // Consume el click
                }

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
    // --- MÉTODOS AUXILIARES Y MODIFICACIONES ---
    // -----------------------------------------------------------------------------------------------------

    public void SnapItemToGhost(GameObject carriedItem, GameObject ghostBox)
    {
        // El anclaje es el padre del fantasma, que es donde queremos que quede la caja real.
        Transform anchor = ghostBox.transform.parent;

        // 1. OBTENER POSICIÓN Y ROTACIÓN ABSOLUTA DEL FANTASMA
        // Esto captura la colocación exacta que tú definiste visualmente en el editor.
        Vector3 finalWorldPosition = ghostBox.transform.position;
        Quaternion finalWorldRotation = ghostBox.transform.rotation;

        // 2. Destruir el objeto fantasma
        Destroy(ghostBox);

        // 3. Desconectar el ítem del jugador
        carriedItem.transform.parent = null;

        // 4. Mover la caja real a la posición/rotación absoluta del fantasma
        carriedItem.transform.position = finalWorldPosition;
        carriedItem.transform.rotation = finalWorldRotation;

        // 5. Vincular al Anchor para la lógica de recolección (SIN cambiar posición/rotación)
        carriedItem.transform.SetParent(anchor);

        // 6. Deshabilitar la física
        Rigidbody rb = carriedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // 7. Limpiar el estado del jugador
        currentGhostRenderer = null;
        itemActual = null;
        if (animadorDelPersonaje != null) { animadorDelPersonaje.SetBool("estaSujetando", false); }
        if (imagenAyudaSoltar != null) { imagenAyudaSoltar.enabled = false; }

        Debug.Log($"Caja colocada y anclada en {anchor.name}.");
    }

    void CogerItemDelSuelo(GameObject itemObject)
    {
        if (itemActual != null) return;

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

        if (parentAnchor != null && parentAnchor.CompareTag("ANCHOR_POINT"))
        {
            // Instanciamos el fantasma de nuevo en el anclaje.
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

    // El resto de los métodos se mantienen igual

    bool PuedeInteractuar(Transform objeto)
    {
        // El Outline aparece en el fantasma si llevamos una caja
        if (objeto.CompareTag("GHOST_BOX")) return (itemActual != null && itemActual.GetComponent<ItemData>() != null);

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


        //Boton cambiador de fase
        if (objeto.GetComponent<CambiadorFase>() != null) return true;

        // --- Resto de interacciones ---
        if (objeto.GetComponent<Papelera>() != null) { return (itemActual != null); }
        if (objeto.GetComponent<CampanaInteractiva>() != null) { return true; }
        if (objeto.GetComponent<ItemData>() != null) { return (itemActual == null); }
        if (objeto.GetComponent<GestorPedidos>() != null) { return true; }

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

    public void DestruirItem()
    {
        if (itemActual == null) return;

        // Limpieza de objetos de minijuego (si aplica)
        if (MinijuegoLimpiezaManager.Instance != null)
        {
            MinijuegoLimpiezaManager.Instance.ObjetoRecogido(itemActual);
        }

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
