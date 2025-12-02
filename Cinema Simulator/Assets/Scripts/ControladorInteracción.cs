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

    // --- [SISTEMA DE REPARACIÓN] ---
    [Header("Sistema de Reparación")]
    public Slider barraProgresoReparacion; // ASIGNA ESTO EN EL INSPECTOR
    public float tiempoParaReparar = 3.0f; // Tiempo en segundos que define el desarrollador
    private float _temporizadorReparacion = 0f;
    private bool _estaReparando = false;
    // -------------------------------

    // --- [VARIABLES DEL FANTASMA Y SNAP] ---
    [Header("Feedback Visual Fantasma")]
    [Tooltip("Asigna aquí el Prefab de la Caja Fantasma (transparente)")]
    public GameObject ghostPrefab;

    private MeshRenderer currentGhostRenderer = null;
    // ------------------------------------------

    [Header("Interfaz UI")]
    public Image imagenAyudaSoltar;

    [Header("Control de Movimiento")]
    public MonoBehaviour scriptMovimiento;

    void Start()
    {
        if (imagenAyudaSoltar != null) imagenAyudaSoltar.enabled = false;

        // Inicializar barra de reparación oculta
        if (barraProgresoReparacion != null)
        {
            barraProgresoReparacion.gameObject.SetActive(false);
            barraProgresoReparacion.value = 0;
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

        // Variable para controlar si estamos mirando algo reparable este frame
        bool mirandoObjetoReparable = false;

        if (Physics.Raycast(ray, out hit, distanciaInteraccion))
        {
            seleccionActual = hit.transform;

            // 1. Detección del Fantasma (GHOST_BOX)
            if (hit.collider.CompareTag("GHOST_BOX"))
            {
                nextGhostRenderer = hit.collider.GetComponent<MeshRenderer>();
            }

            // Lógica de Outline estándar
            if (PuedeInteractuar(hit.transform))
            {
                outlineActual = hit.collider.GetComponent<Outline>();
            }

            // --- LÓGICA DE REPARACIÓN (Outline específico o reutilizado) ---
            // Si tenemos el martillo y miramos algo reparable que esté roto
            if (TieneElMartillo() && hit.transform.GetComponent<IMaquinaReparable>() != null)
            {
                IMaquinaReparable maquina = hit.transform.GetComponent<IMaquinaReparable>();
                if (maquina.EstaRota)
                {
                    outlineActual = hit.collider.GetComponent<Outline>(); // Reutilizamos el outline
                    mirandoObjetoReparable = true;
                    ProcesarReparacion(maquina);
                }
            }
        }

        // Si no estamos mirando nada reparable o dejamos de mirar, reseteamos la reparación
        if (!mirandoObjetoReparable)
        {
            ResetearReparacion();
        }

        // 2. Control de Visibilidad del Fantasma
        if (currentGhostRenderer != nextGhostRenderer)
        {
            if (currentGhostRenderer != null) currentGhostRenderer.enabled = false;
            if (nextGhostRenderer != null) nextGhostRenderer.enabled = true;
            currentGhostRenderer = nextGhostRenderer;
        }

        // Gestión del Outline visual
        if (outlineScriptMirado != outlineActual)
        {
            if (outlineScriptMirado != null) outlineScriptMirado.enabled = false;
            if (outlineActual != null) outlineActual.enabled = true;
            outlineScriptMirado = outlineActual;
        }
        objetoMirado = seleccionActual;

        // --- DETECCIÓN DE CLICK (Interacciones normales) ---
        // Solo permitimos interacciones normales si NO estamos reparando activamente
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && !_estaReparando)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            if (objetoMirado != null)
            {
                // Si la máquina está rota, impedimos interacción normal (Excepto repararla que ya se gestiona arriba)
                IMaquinaReparable maquinaRota = objetoMirado.GetComponent<IMaquinaReparable>();
                if (maquinaRota != null && maquinaRota.EstaRota)
                {
                    // Si no tengo martillo, aviso. Si tengo martillo, la lógica de reparación va por otro lado (mantener click)
                    if (!TieneElMartillo()) Debug.Log("¡Está rota! Necesitas un martillo.");
                    return;
                }

                // ... [RESTO DE TU CÓDIGO DE INTERACCIONES ORIGINAL] ...
                // (Lo he comprimido para ahorrar espacio, es idéntico al tuyo)
                GestorPedidos cliente = objetoMirado.GetComponent<GestorPedidos>();
                if (cliente != null && Input.GetMouseButtonDown(0))
                {
                    ItemData itemDataEnMano = (itemActual != null) ? itemActual.GetComponent<ItemData>() : null;
                    if (cliente.RecibirItem(itemDataEnMano)) DestruirItem();
                    return;
                }
                if (objetoMirado.GetComponent<TabletManager>() != null) { objetoMirado.GetComponent<TabletManager>().AbrirTablet(this); return; }
                if (objetoMirado.GetComponent<CambiadorFase>() != null) { objetoMirado.GetComponent<CambiadorFase>().Interactuar(); return; }

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
                if (objetoMirado.GetComponent<MaquinaDeItems>() != null) { objetoMirado.GetComponent<MaquinaDeItems>().Interactuar(this); return; }

                if (itemActual != null && objetoMirado.CompareTag("GHOST_BOX") && Input.GetMouseButtonDown(0))
                {
                    ItemData carriedData = itemActual.GetComponent<ItemData>();
                    if (carriedData != null)
                    {
                        ItemData.TipoDeItem itemType = carriedData.tipoDeItem;
                        if (itemType == ItemData.TipoDeItem.CajaPalomitas || itemType == ItemData.TipoDeItem.CajaBebidas ||
                            itemType == ItemData.TipoDeItem.CajaEnvasesPalomitas || itemType == ItemData.TipoDeItem.CajaEnvasesBebidas ||
                            itemType == ItemData.TipoDeItem.CajaPerritos)
                        {
                            SnapItemToGhost(itemActual, objetoMirado.gameObject);
                            return;
                        }
                    }
                }

                LightSwitch lightSwitch = objetoMirado.GetComponent<LightSwitch>();
                if (lightSwitch != null && Input.GetMouseButtonDown(0)) { lightSwitch.Interact(); return; }

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
    // --- LÓGICA DE REPARACIÓN ---
    // -----------------------------------------------------------------------------------------------------

    private bool TieneElMartillo()
    {
        if (itemActual == null) return false;
        ItemData data = itemActual.GetComponent<ItemData>();
        return (data != null && data.tipoDeItem == ItemData.TipoDeItem.Martillo);
    }

    private void ProcesarReparacion(IMaquinaReparable maquina)
    {
        // El usuario debe mantener pulsado Click Izquierdo (0)
        if (Input.GetMouseButton(0))
        {
            _estaReparando = true;
            _temporizadorReparacion += Time.deltaTime;

            // Actualizar UI
            if (barraProgresoReparacion != null)
            {
                barraProgresoReparacion.gameObject.SetActive(true);
                barraProgresoReparacion.value = _temporizadorReparacion / tiempoParaReparar;
            }

            // Chequeo de finalización
            if (_temporizadorReparacion >= tiempoParaReparar)
            {
                maquina.Reparar();
                ResetearReparacion();
                Debug.Log("¡Reparación completada!");
            }
        }
        else
        {
            // Si suelta el click, se resetea el progreso
            ResetearReparacion();
        }
    }

    private void ResetearReparacion()
    {
        _estaReparando = false;
        _temporizadorReparacion = 0f;
        if (barraProgresoReparacion != null)
        {
            barraProgresoReparacion.value = 0;
            barraProgresoReparacion.gameObject.SetActive(false);
        }
    }

    // -----------------------------------------------------------------------------------------------------
    // --- MÉTODOS AUXILIARES ORIGINALES ---
    // -----------------------------------------------------------------------------------------------------

    public void SnapItemToGhost(GameObject carriedItem, GameObject ghostBox)
    {
        Transform anchor = ghostBox.transform.parent;
        Vector3 finalWorldPosition = ghostBox.transform.position;
        Quaternion finalWorldRotation = ghostBox.transform.rotation;
        Destroy(ghostBox);
        carriedItem.transform.parent = null;
        carriedItem.transform.position = finalWorldPosition;
        carriedItem.transform.rotation = finalWorldRotation;
        carriedItem.transform.SetParent(anchor);
        Rigidbody rb = carriedItem.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        currentGhostRenderer = null;
        itemActual = null;
        if (animadorDelPersonaje != null) { animadorDelPersonaje.SetBool("estaSujetando", false); }
        if (imagenAyudaSoltar != null) { imagenAyudaSoltar.enabled = false; }
    }

    void CogerItemDelSuelo(GameObject itemObject)
    {
        if (itemActual != null) return;
        Transform parentAnchor = itemObject.transform.parent;
        Rigidbody rb = itemObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        itemObject.transform.parent = puntoDeAgarre;
        itemObject.transform.localPosition = Vector3.zero;
        itemObject.transform.localRotation = Quaternion.identity;
        ItemData data = itemObject.GetComponent<ItemData>();
        if (data != null) { itemObject.transform.localScale = data.escalaOriginal; }
        itemActual = itemObject;
        if (animadorDelPersonaje != null) { animadorDelPersonaje.SetBool("estaSujetando", true); }

        if (parentAnchor != null && parentAnchor.CompareTag("ANCHOR_POINT"))
        {
            Instantiate(ghostPrefab, parentAnchor.position, parentAnchor.rotation, parentAnchor);
        }

        if (imagenAyudaSoltar != null)
        {
            if (data == null || data.tipoDeItem != ItemData.TipoDeItem.Ticket) imagenAyudaSoltar.enabled = true;
        }
    }

    bool PuedeInteractuar(Transform objeto)
    {
        // Prioridad: Si tengo martillo y es máquina rota
        if (TieneElMartillo())
        {
            IMaquinaReparable rep = objeto.GetComponent<IMaquinaReparable>();
            if (rep != null && rep.EstaRota) return true;
        }

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
        if (objeto.GetComponent<CambiadorFase>() != null) return true;
        if (objeto.GetComponent<Papelera>() != null) return (itemActual != null);
        if (objeto.GetComponent<CampanaInteractiva>() != null) return true;
        if (objeto.GetComponent<ItemData>() != null) return (itemActual == null);
        if (objeto.GetComponent<GestorPedidos>() != null) return true;

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
            if (data == null || data.tipoDeItem != ItemData.TipoDeItem.Ticket) imagenAyudaSoltar.enabled = true;
        }
    }

    void SoltarItemAlSuelo()
    {
        if (itemActual == null) return;
        ItemData data = itemActual.GetComponent<ItemData>();
        if (data != null && data.tipoDeItem == ItemData.TipoDeItem.Ticket) { Debug.Log("No puedes soltar este item."); return; }
        if (animadorDelPersonaje != null) { animadorDelPersonaje.SetBool("estaSujetando", false); }
        Rigidbody rb = itemActual.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
        itemActual.transform.parent = null;
        if (data != null) itemActual.transform.localScale = data.escalaOriginal;
        itemActual = null;
        if (imagenAyudaSoltar != null) imagenAyudaSoltar.enabled = false;
    }

    public void DestruirItem()
    {
        if (itemActual == null) return;
        if (MinijuegoLimpiezaManager.Instance != null) MinijuegoLimpiezaManager.Instance.ObjetoRecogido(itemActual);
        Destroy(itemActual);
        itemActual = null;
        if (animadorDelPersonaje != null) animadorDelPersonaje.SetBool("estaSujetando", false);
        Debug.Log("Has tirado el item.");
        if (imagenAyudaSoltar != null) imagenAyudaSoltar.enabled = false;
    }

    public void AlternarControlJugador(bool activo)
    {
        if (scriptMovimiento != null) scriptMovimiento.enabled = activo;
    }
}