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

    // Variables internas
    private Outline outlineScriptMirado;
    private Transform objetoMirado;

    // --- [SISTEMA DE REPARACIÓN] ---
    [Header("Sistema de Reparación")]
    public Slider barraProgresoReparacion;
    public float tiempoParaReparar = 3.0f;
    private float _temporizadorReparacion = 0f;
    private bool _estaReparando = false;

    [Header("Audio Reparación")]
    public AudioSource sourceReparacion;
    public AudioClip clipReparandoLoop;
    public AudioClip clipReparacionCompletada;

    // --- [VARIABLES DEL FANTASMA Y SNAP] ---
    [Header("Feedback Visual Fantasma")]
    [Tooltip("Asigna aquí el Prefab de la Caja Fantasma (transparente)")]
    public GameObject ghostPrefab;
    private MeshRenderer currentGhostRenderer = null;

    [Header("Interfaz UI")]
    public Image imagenAyudaSoltar;

    [Header("Control de Movimiento")]
    public MonoBehaviour scriptMovimiento;

    void Start()
    {
        if (imagenAyudaSoltar != null) imagenAyudaSoltar.enabled = false;
        if (barraProgresoReparacion != null)
        {
            barraProgresoReparacion.gameObject.SetActive(false);
            barraProgresoReparacion.value = 0;
        }
    }

    // --- HELPER MÁGICO PARA BUSCAR EN PADRES ---
    // Esto soluciona el problema de que el raycast choque con un hijo sin script
    T ObtenerComponente<T>(Transform t)
    {
        T comp = t.GetComponent<T>();

        // Comprobación de nulidad segura para Unity
        if (comp != null && !comp.Equals(null)) return comp;

        return t.GetComponentInParent<T>();
    }
    // -------------------------------------------

    void Update()
    {
        Ray ray = new Ray(camaraJugador.transform.position, camaraJugador.transform.forward);
        RaycastHit hit;

        Transform seleccionActual = null;
        Outline outlineActual = null;
        MeshRenderer nextGhostRenderer = null;
        bool mirandoObjetoReparable = false;
        string mensajeInteraccion = "";

        // IMPORTANTE: Usamos una máscara para ignorar la capa "Ignore Raycast" o Triggers si molestan
        // (Por defecto Raycast choca con todo, asegúrate de que tus Triggers de zona no bloqueen el rayo)
        if (Physics.Raycast(ray, out hit, distanciaInteraccion))
        {
            seleccionActual = hit.transform;

            // 1. Detección del Fantasma (GHOST_BOX)
            if (hit.collider.CompareTag("GHOST_BOX"))
            {
                nextGhostRenderer = hit.collider.GetComponent<MeshRenderer>();
            }

            // 2. Lógica de Outline (Usamos el Helper)
            if (PuedeInteractuar(hit.transform))
            {
                outlineActual = ObtenerComponente<Outline>(hit.transform);
                mensajeInteraccion = ObtenerMensajeInteraccion(hit.transform);
            }

            // 3. Lógica de Reparación
            if (TieneElMartillo())
            {
                IMaquinaReparable maquina = ObtenerComponente<IMaquinaReparable>(hit.transform);
                if (maquina != null && maquina.EstaRota)
                {
                    outlineActual = ObtenerComponente<Outline>(hit.transform);
                    mirandoObjetoReparable = true;
                    mensajeInteraccion = "Mantener click izquierdo - Reparar";
                    ProcesarReparacion(maquina);
                }
            }
        }

        if (!mirandoObjetoReparable) ResetearReparacion();

        // Actualizar UI de interacción
        if (!string.IsNullOrEmpty(mensajeInteraccion) && UIInteractionManager.Instance != null)
        {
            UIInteractionManager.Instance.MostrarInteraccion(mensajeInteraccion);
        }
        else if (UIInteractionManager.Instance != null)
        {
            UIInteractionManager.Instance.OcultarInteraccion();
        }

        // Control de Visibilidad del Fantasma
        if (currentGhostRenderer != nextGhostRenderer)
        {
            if (currentGhostRenderer != null) currentGhostRenderer.enabled = false;
            if (nextGhostRenderer != null) nextGhostRenderer.enabled = true;
            currentGhostRenderer = nextGhostRenderer;
        }

        // Gestión del Outline
        if (outlineScriptMirado != outlineActual)
        {
            if (outlineScriptMirado != null) outlineScriptMirado.enabled = false;
            if (outlineActual != null)
            {
                outlineActual.enabled = true;
                outlineActual.OutlineColor = Color.white; // Color blanco para interacción
                outlineActual.OutlineWidth = 3f; // Ancho del outline
            }
            outlineScriptMirado = outlineActual;
        }
        objetoMirado = seleccionActual;

        // --- DETECCIÓN DE CLICK ---
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && !_estaReparando)
        {
            // Bloqueo de UI (Tu código anterior)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            if (objetoMirado != null)
            {
                // Usamos el Helper para buscar los scripts, estén donde estén
                IMaquinaReparable maquinaRota = ObtenerComponente<IMaquinaReparable>(objetoMirado);
                if (maquinaRota != null && maquinaRota.EstaRota)
                {
                    if (!TieneElMartillo()) Debug.Log("¡Está rota! Necesitas un martillo.");
                    return;
                }

                // --- INTERACCIONES ---

                GestorPedidos cliente = ObtenerComponente<GestorPedidos>(objetoMirado);
                if (cliente != null && Input.GetMouseButtonDown(0))
                {
                    ItemData itemDataEnMano = (itemActual != null) ? itemActual.GetComponent<ItemData>() : null;
                    if (cliente.RecibirItem(itemDataEnMano)) DestruirItem();
                    return;
                }

                TabletManager tablet = ObtenerComponente<TabletManager>(objetoMirado);
                if (tablet != null) { tablet.AbrirTablet(this); return; }

                CambiadorFase cambiador = ObtenerComponente<CambiadorFase>(objetoMirado);
                if (cambiador != null)
                {
                    if (TransitionManager.Instance != null && !TransitionManager.Instance.transicionando)
                    {
                        cambiador.Interactuar();
                    }
                    return;
                }

                // Máquinas (Usando Helper)
                MaquinaDePalomitas mPalomitas = ObtenerComponente<MaquinaDePalomitas>(objetoMirado);
                if (mPalomitas != null) { mPalomitas.Interactuar(this); return; }

                MaquinaDeBebidas mBebidas = ObtenerComponente<MaquinaDeBebidas>(objetoMirado);
                if (mBebidas != null) { mBebidas.Interactuar(this); return; }

                MaquinaDePerritos mPerritos = ObtenerComponente<MaquinaDePerritos>(objetoMirado);
                if (mPerritos != null) { mPerritos.Interactuar(this); return; }

                MaquinaDeItems mItems = ObtenerComponente<MaquinaDeItems>(objetoMirado);
                if (mItems != null) { mItems.Interactuar(this); return; }

                // Ghost Box Snap
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

                LightSwitch lightSwitch = ObtenerComponente<LightSwitch>(objetoMirado);
                if (lightSwitch != null && Input.GetMouseButtonDown(0)) { lightSwitch.Interact(); return; }

                // Objetos Simples
                if (Input.GetMouseButtonDown(0))
                {
                    Papelera papelera = ObtenerComponente<Papelera>(objetoMirado);
                    if (papelera != null) { DestruirItem(); return; }

                    CampanaInteractiva campana = ObtenerComponente<CampanaInteractiva>(objetoMirado);
                    if (campana != null) { campana.Interactuar(); return; }

                    ItemData itemData = ObtenerComponente<ItemData>(objetoMirado);
                    // Ojo: Para coger del suelo, necesitamos el objeto físico exacto, no el padre
                    // pero ObtenerComponente nos devuelve el script. Usamos su gameObject.
                    if (itemData != null) { CogerItemDelSuelo(itemData.gameObject); return; }
                }
            }
        }
        if (Input.GetKeyDown(teclaSoltar)) { SoltarItemAlSuelo(); }
    }

    // --- VERIFICACIONES "INTELIGENTES" ---
    string ObtenerMensajeInteraccion(Transform objeto)
    {
        // 1. Reparación
        if (TieneElMartillo())
        {
            IMaquinaReparable rep = ObtenerComponente<IMaquinaReparable>(objeto);
            if (rep != null && rep.EstaRota) return "Mantener click izquierdo - Reparar";
        }

        // 2. Ghost Box (Colocar caja)
        if (objeto.CompareTag("GHOST_BOX") && itemActual != null)
        {
            ItemData data = itemActual.GetComponent<ItemData>();
            if (data != null)
            {
                ItemData.TipoDeItem itemType = data.tipoDeItem;
                if (itemType == ItemData.TipoDeItem.CajaPalomitas || itemType == ItemData.TipoDeItem.CajaBebidas ||
                    itemType == ItemData.TipoDeItem.CajaEnvasesPalomitas || itemType == ItemData.TipoDeItem.CajaEnvasesBebidas ||
                    itemType == ItemData.TipoDeItem.CajaPerritos)
                {
                    return "Click izquierdo - Colocar caja";
                }
            }
        }

        // 3. Máquina de Palomitas
        MaquinaDePalomitas mPalomitas = ObtenerComponente<MaquinaDePalomitas>(objeto);
        if (mPalomitas != null)
        {
            if (itemActual == null) return "";
            ItemData data = itemActual.GetComponent<ItemData>();
            if (data != null)
            {
                if (data.tipoDeItem == ItemData.TipoDeItem.CuboVacio)
                    return "Click izquierdo - Llenar cubo";
                if (data.tipoDeItem == mPalomitas.tipoDeCajaRequerida)
                    return "Click izquierdo - Reponer 1 | Click derecho - Reponer todo";
            }
        }

        // 4. Máquina de Bebidas
        MaquinaDeBebidas mBebidas = ObtenerComponente<MaquinaDeBebidas>(objeto);
        if (mBebidas != null)
        {
            if (itemActual == null) return "";
            ItemData data = itemActual.GetComponent<ItemData>();
            if (data != null)
            {
                if (data.tipoDeItem == ItemData.TipoDeItem.VasoVacio)
                    return "Click izquierdo - Llenar vaso";
                if (data.tipoDeItem == mBebidas.tipoDeCajaRequerida)
                    return "Click izquierdo - Reponer 1 | Click derecho - Reponer todo";
            }
        }

        // 5. Máquina de Perritos
        MaquinaDePerritos mPerritos = ObtenerComponente<MaquinaDePerritos>(objeto);
        if (mPerritos != null)
        {
            if (itemActual == null) return "Click izquierdo - Coger perrito";
            ItemData data = itemActual.GetComponent<ItemData>();
            if (data != null && data.tipoDeItem == mPerritos.tipoDeCajaRequerida)
                return "Click izquierdo - Reponer 1 | Click derecho - Reponer todo";
        }

        // 6. Máquina de Items (Envases)
        MaquinaDeItems mItems = ObtenerComponente<MaquinaDeItems>(objeto);
        if (mItems != null)
        {
            if (itemActual == null) return "Click izquierdo - Coger envase";
            ItemData data = itemActual.GetComponent<ItemData>();
            if (data != null && data.tipoDeItem == mItems.tipoDeCajaRequerida)
                return "Click izquierdo - Reponer 1 | Click derecho - Reponer todo";
        }

        // 7. Papelera
        if (ObtenerComponente<Papelera>(objeto) != null)
        {
            if (itemActual != null) return "Click izquierdo - Tirar objeto";
        }

        // 8. Campana
        if (ObtenerComponente<CampanaInteractiva>(objeto) != null)
        {
            return "Click izquierdo - Pedir comanda";
        }

        // 9. Cliente
        if (ObtenerComponente<GestorPedidos>(objeto) != null)
        {
            return itemActual != null ? "Click izquierdo - Entregar pedido" : "Cliente esperando";
        }

        // 10. Item en el suelo
        if (ObtenerComponente<ItemData>(objeto) != null && itemActual == null)
        {
            return "Click izquierdo - Coger objeto";
        }

        // 11. Tablet
        if (ObtenerComponente<TabletManager>(objeto) != null)
        {
            return "Click izquierdo - Abrir tablet";
        }

        return "";
    }

    // --- VERIFICACIONES "INTELIGENTES" ---
    bool PuedeInteractuar(Transform objeto)
    {
        // 1. Reparación
        if (TieneElMartillo())
        {
            IMaquinaReparable rep = ObtenerComponente<IMaquinaReparable>(objeto);
            if (rep != null && rep.EstaRota) return true;
        }

        // 2. Ghost Box
        if (objeto.CompareTag("GHOST_BOX")) return (itemActual != null && itemActual.GetComponent<ItemData>() != null);

        // 3. Máquinas (Comprobación Robusta con Helper)
        MaquinaDePalomitas mPalomitas = ObtenerComponente<MaquinaDePalomitas>(objeto);
        if (mPalomitas != null)
        {
            if (itemActual == null) return true;
            ItemData data = itemActual.GetComponent<ItemData>();
            if (data == null) return false;
            return (data.tipoDeItem == ItemData.TipoDeItem.CuboVacio || data.tipoDeItem == mPalomitas.tipoDeCajaRequerida);
        }

        MaquinaDeBebidas mBebidas = ObtenerComponente<MaquinaDeBebidas>(objeto);
        if (mBebidas != null)
        {
            if (itemActual == null) return true;
            ItemData data = itemActual.GetComponent<ItemData>();
            if (data == null) return false;
            return (data.tipoDeItem == ItemData.TipoDeItem.VasoVacio || data.tipoDeItem == mBebidas.tipoDeCajaRequerida);
        }

        // MÁQUINA DE PERRITOS (AÑADIDA QUE FALTABA EN TU CÓDIGO ORIGINAL EN ESTE CHECK)
        MaquinaDePerritos mPerritos = ObtenerComponente<MaquinaDePerritos>(objeto);
        if (mPerritos != null)
        {
            if (itemActual == null) return true;
            ItemData data = itemActual.GetComponent<ItemData>();
            if (data == null) return false;
            // Perritos no usa "Envase vacío", coge directo, o rellena con caja
            return (data.tipoDeItem == mPerritos.tipoDeCajaRequerida);
        }

        MaquinaDeItems mItems = ObtenerComponente<MaquinaDeItems>(objeto);
        if (mItems != null)
        {
            if (itemActual == null) return true;
            ItemData data = itemActual.GetComponent<ItemData>();
            if (data != null && data.tipoDeItem == mItems.tipoDeCajaRequerida) return true;
            return false;
        }

        // 4. Otros
        if (ObtenerComponente<TabletManager>(objeto) != null) return true;
        if (ObtenerComponente<CambiadorFase>(objeto) != null) return true;
        if (ObtenerComponente<Papelera>(objeto) != null) return (itemActual != null);
        if (ObtenerComponente<CampanaInteractiva>(objeto) != null) return true;
        if (ObtenerComponente<ItemData>(objeto) != null) return (itemActual == null); // Coger del suelo
        if (ObtenerComponente<GestorPedidos>(objeto) != null) return true;

        return false;
    }

    // ... [RESTO DE TU CÓDIGO SIN CAMBIOS: ProcesarReparacion, CogerItemDelSuelo, etc.] ...
    // Solo copia las funciones de abajo de tu script antiguo (SnapItemToGhost, AsignarItem, etc.)
    // porque esas no necesitan cambios de lógica.

    // --- PEGA AQUÍ EL RESTO DE TUS MÉTODOS (SnapItemToGhost, CogerItemDelSuelo, AsignarItem...) ---
    // (Te los incluyo aquí resumidos para que el script esté completo al copiar y pegar)

    private bool TieneElMartillo()
    {
        if (itemActual == null) return false;
        ItemData data = itemActual.GetComponent<ItemData>();
        return (data != null && data.tipoDeItem == ItemData.TipoDeItem.Martillo);
    }

    private void ProcesarReparacion(IMaquinaReparable maquina)
    {
        if (Input.GetMouseButton(0))
        {
            if (!_estaReparando)
            {
                _estaReparando = true;
                if (sourceReparacion != null && clipReparandoLoop != null)
                {
                    sourceReparacion.clip = clipReparandoLoop;
                    sourceReparacion.loop = true;
                    sourceReparacion.Play();
                }
            }
            _temporizadorReparacion += Time.deltaTime;
            if (barraProgresoReparacion != null)
            {
                barraProgresoReparacion.gameObject.SetActive(true);
                barraProgresoReparacion.value = _temporizadorReparacion / tiempoParaReparar;
            }
            if (_temporizadorReparacion >= tiempoParaReparar)
            {
                if (sourceReparacion != null) { sourceReparacion.Stop(); if (clipReparacionCompletada != null) sourceReparacion.PlayOneShot(clipReparacionCompletada); }
                maquina.Reparar();
                _estaReparando = false;
                _temporizadorReparacion = 0f;
                if (barraProgresoReparacion != null) barraProgresoReparacion.gameObject.SetActive(false);
            }
        }
        else { ResetearReparacion(); }
    }
    private void ResetearReparacion()
    {
        if (_estaReparando && sourceReparacion != null && sourceReparacion.isPlaying) sourceReparacion.Stop();
        _estaReparando = false;
        _temporizadorReparacion = 0f;
        if (barraProgresoReparacion != null) { barraProgresoReparacion.value = 0; barraProgresoReparacion.gameObject.SetActive(false); }
    }
    public void SnapItemToGhost(GameObject carriedItem, GameObject ghostBox)
    {
        Transform anchor = ghostBox.transform.parent;
        Vector3 finalWorldPosition = ghostBox.transform.position;
        Quaternion finalWorldRotation = ghostBox.transform.rotation;
        Destroy(ghostBox);

        // --- CORRECCIÓN: RESTAURAR LA CAPA ORIGINAL ---
        SetLayerRecursively(carriedItem, 0); // Capa 0 = Default
        // ----------------------------------------------

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

        // --- CORRECCIÓN: HACER EL ITEM INVISIBLE AL RAYCAST ---
        SetLayerRecursively(itemObject, 2); // Capa 2 = Ignore Raycast
        // -----------------------------------------------------

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
    public void AsignarItem(GameObject nuevoItemPrefab)
    {
        if (itemActual != null) { Destroy(itemActual); itemActual = null; }
        if (nuevoItemPrefab == null) return;

        itemActual = Instantiate(nuevoItemPrefab);

        // --- CORRECCIÓN: HACER EL ITEM INVISIBLE AL RAYCAST ---
        SetLayerRecursively(itemActual, 2); // Capa 2 = Ignore Raycast
        // -----------------------------------------------------

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

        // --- CORRECCIÓN: RESTAURAR LA CAPA ORIGINAL ---
        SetLayerRecursively(itemActual, 0); // Capa 0 = Default
        // ----------------------------------------------

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
        if (imagenAyudaSoltar != null) imagenAyudaSoltar.enabled = false;
    }
    public void AlternarControlJugador(bool activo)
    {
        if (scriptMovimiento != null) scriptMovimiento.enabled = activo;
    }

    // --- HELPER PARA CAMBIAR CAPAS (EVITAR QUE LA CAJA BLOQUEE LA VISTA) ---
    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child != null) SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}