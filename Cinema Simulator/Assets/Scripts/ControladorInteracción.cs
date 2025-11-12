// ControladorInteraccion.cs
using UnityEngine;
// using QuickOutline; // O el 'using' que sea de tu asset de Outline

public class ControladorInteraccion : MonoBehaviour
{
    public float distanciaInteraccion = 3f;
    public Camera camaraJugador;
    public Transform puntoDeAgarre;
    public Animator animadorDelPersonaje;
    public GameObject itemActual;

    private Outline outlineScriptMirado;
    private Transform objetoMirado; // ¡Este objetoMirado es solo para el clic!

    void Update()
    {
        Ray ray = new Ray(camaraJugador.transform.position, camaraJugador.transform.forward);
        RaycastHit hit;

        Transform seleccionActual = null; // Para el clic
        Outline outlineActual = null; // Para el brillo

        // 1. LÓGICA DE DETECCIÓN Y RESALTADO (¡MODIFICADA!)
        if (Physics.Raycast(ray, out hit, distanciaInteraccion))
        {
            // A. Guardamos lo que estamos mirando (para la lógica del clic)
            seleccionActual = hit.transform;

            // B. Comprobamos si podemos interactuar con ello
            // ¡Aquí está la nueva magia!
            if (PuedeInteractuar(hit.transform))
            {
                // C. Si SÍ podemos, guardamos su Outline para activarlo
                outlineActual = hit.collider.GetComponent<Outline>();
            }
            // Si no podemos interactuar, 'outlineActual' se queda 'null'
            // y el outline no se mostrará, que es lo que quieres.
        }

        // 2. GESTIÓN DEL RESALTADO (Esta parte no cambia)
        if (outlineScriptMirado != outlineActual)
        {
            if (outlineScriptMirado != null) outlineScriptMirado.enabled = false;
            if (outlineActual != null) outlineActual.enabled = true;
            outlineScriptMirado = outlineActual;
        }

        // 3. LÓGICA DE INTERACCIÓN (AL HACER CLIC)
        // Guardamos el objeto mirado para el clic
        objetoMirado = seleccionActual;

        if (Input.GetMouseButtonDown(0))
        {
            if (objetoMirado != null)
            {
                // CASO A: ¿Es una máquina de palomitas (inteligente)?
                MaquinaDePalomitas maquinaPalomitas = objetoMirado.GetComponent<MaquinaDePalomitas>();
                if (maquinaPalomitas != null)
                {
                    maquinaPalomitas.Interactuar(this);
                    return;
                }

                // CASO B: ¿Es una máquina de items (simple)? (Ej: Bebidas)
                MaquinaDeItems maquinaItems = objetoMirado.GetComponent<MaquinaDeItems>();
                if (maquinaItems != null)
                {
                    CogerItem(maquinaItems.itemPrefab);
                    return;
                }

                // CASO C: ¿Es una papelera?
                Papelera papelera = objetoMirado.GetComponent<Papelera>();
                if (papelera != null)
                {
                    SoltarItem();
                    return;
                }
            }
        }
    }

    // --- ¡NUEVA FUNCIÓN DE AYUDA! ---
    // Comprueba si la interacción es válida (para mostrar el outline)
    bool PuedeInteractuar(Transform objeto)
    {
        // Caso A: Máquina de palomitas
        if (objeto.GetComponent<MaquinaDePalomitas>() != null)
        {
            // Solo podemos interactuar si tenemos un CuboVacio
            if (itemActual == null) return false; // Mano vacía, no brilla

            ItemData data = itemActual.GetComponent<ItemData>();
            if (data == null) return false; // No es un item con DNI, no brilla

            // ¿Es un CuboVacio? ¡SÍ brilla!
            return (data.tipoDeItem == ItemData.TipoDeItem.CuboVacio);
        }

        // Caso B: Máquina de items simple (Bebidas)
        if (objeto.GetComponent<MaquinaDeItems>() != null)
        {
            // Solo podemos interactuar si tenemos la mano vacía
            // Si la mano está vacía, ¡SÍ brilla!
            return (itemActual == null);
        }

        // Caso C: Papelera
        if (objeto.GetComponent<Papelera>() != null)
        {
            // Solo podemos interactuar si tenemos algo en la mano
            // Si la mano tiene algo, ¡SÍ brilla!
            return (itemActual != null);
        }

        // Si no es ninguno de estos, no se puede interactuar
        return false;
    }

    // --- (El resto de tus funciones no cambian) ---

    public void AsignarItem(GameObject nuevoItemPrefab)
    {
        if (itemActual != null)
        {
            Destroy(itemActual);
            itemActual = null;
        }
        itemActual = Instantiate(nuevoItemPrefab, puntoDeAgarre.position, puntoDeAgarre.rotation);
        itemActual.transform.parent = puntoDeAgarre;
        if (animadorDelPersonaje != null)
        {
            animadorDelPersonaje.SetBool("estaSujetando", true);
        }
    }

    void CogerItem(GameObject prefabDelItem)
    {
        if (itemActual != null)
        {
            Debug.Log("Ya tienes un item. Tíralo primero.");
            return;
        }
        AsignarItem(prefabDelItem);
    }

    public void SoltarItem()
    {
        if (itemActual != null)
        {
            Destroy(itemActual);
            itemActual = null;
            if (animadorDelPersonaje != null)
            {
                animadorDelPersonaje.SetBool("estaSujetando", false);
            }
            Debug.Log("Has tirado el item.");
        }
    }
}