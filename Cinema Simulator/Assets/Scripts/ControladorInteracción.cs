using UnityEngine;

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
    public PedidoCliente pedidoCliente;

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

        if (Input.GetMouseButtonDown(0))
        {
            if (objetoMirado != null)
            {
                if (objetoMirado.GetComponent<MaquinaDePalomitas>() != null) { objetoMirado.GetComponent<MaquinaDePalomitas>().Interactuar(this); return; }
                if (objetoMirado.GetComponent<MaquinaDeBebidas>() != null) { objetoMirado.GetComponent<MaquinaDeBebidas>().Interactuar(this); return; }
                if (objetoMirado.GetComponent<MaquinaDeItems>() != null) { CogerItem(objetoMirado.GetComponent<MaquinaDeItems>().itemPrefab); return; }
                if (objetoMirado.GetComponent<Papelera>() != null) { DestruirItem(); return; }
                if (objetoMirado.GetComponent<CampanaInteractiva>() != null) { objetoMirado.GetComponent<CampanaInteractiva>().Interactuar(); return; }
                if (objetoMirado.GetComponent<ItemData>() != null) { CogerItemDelSuelo(objetoMirado.gameObject); return; }
                if (objetoMirado.CompareTag("Bell"))
                {
                    AudioSource bellSound = objetoMirado.GetComponent<AudioSource>();
                    if (bellSound != null)
                    {
                        bellSound.Play();
                        pedidoCliente.GenerarNuevoPedido();
                    }
                    return;
                }

                if (objetoMirado.CompareTag("NPC"))
                {
                    PedidoCliente cliente = objetoMirado.GetComponent<PedidoCliente>();
                    if (cliente != null)
                    {
                        ItemData itemEnMano = (itemActual != null) ? itemActual.GetComponent<ItemData>() : null;
                        bool exito = cliente.RecibirItem(itemEnMano);

                        if (exito)
                        {
                            DestruirItem();
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Objeto con tag 'NPC' no tiene script 'PedidoCliente.cs'");
                    }
                    return;
                }
            }

        }
        if (Input.GetKeyDown(teclaSoltar)) { SoltarItemAlSuelo(); }
    }

    bool PuedeInteractuar(Transform objeto)
    {
        if (objeto.GetComponent<MaquinaDePalomitas>() != null)
        {
            if (itemActual == null) return false;
            ItemData data = itemActual.GetComponent<ItemData>();
            if (data == null) return false;
            return (data.tipoDeItem == ItemData.TipoDeItem.CuboVacio);
        }
        if (objeto.GetComponent<MaquinaDeBebidas>() != null)
        {
            if (itemActual == null) return false;
            ItemData data = itemActual.GetComponent<ItemData>();
            if (data == null) return false;
            return (data.tipoDeItem == ItemData.TipoDeItem.VasoVacio);
        }
        if (objeto.GetComponent<MaquinaDeItems>() != null) { return (itemActual == null); }
        if (objeto.GetComponent<Papelera>() != null) { return (itemActual != null); }
        if (objeto.GetComponent<CampanaInteractiva>() != null) { return true; }
        if (objeto.GetComponent<ItemData>() != null) { return (itemActual == null); }
        if (objeto.CompareTag("Bell")) { return true; }
        if (objeto.CompareTag("NPC"))
        {
            return true;
        }
        return false;
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
        if (data != null)
        {
            itemObject.transform.localScale = data.escalaOriginal;
        }

        itemActual = itemObject;

        if (animadorDelPersonaje != null) { animadorDelPersonaje.SetBool("estaSujetando", true); }
    }

    void SoltarItemAlSuelo()
    {
        if (itemActual == null) return;
        ItemData data = itemActual.GetComponent<ItemData>();
        if(data.tipoDeItem == ItemData.TipoDeItem.Ticket) return;

        if (animadorDelPersonaje != null) { animadorDelPersonaje.SetBool("estaSujetando", false); }
        Rigidbody rb = itemActual.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        itemActual.transform.parent = null;
        itemActual = null;
    }

    public void AsignarItem(GameObject nuevoItemPrefab)
    {
        if (itemActual != null) { Destroy(itemActual); itemActual = null; }

        itemActual = Instantiate(nuevoItemPrefab, puntoDeAgarre.position, puntoDeAgarre.rotation);
        ItemData data = itemActual.GetComponent<ItemData>();

        itemActual.transform.parent = puntoDeAgarre;
        itemActual.transform.localPosition = Vector3.zero;
        itemActual.transform.localRotation = Quaternion.identity;

        if (data != null)
        {
            itemActual.transform.localScale = data.escalaOriginal;
        }

        if (animadorDelPersonaje != null) { animadorDelPersonaje.SetBool("estaSujetando", true); }
    }

    void CogerItem(GameObject prefabDelItem)
    {
        if (itemActual != null) { Debug.Log("Ya tienes un item. Tíralo primero."); return; }
        AsignarItem(prefabDelItem);
    }

    public void DestruirItem()
    {
        if (itemActual != null)
        {
            Destroy(itemActual);
            itemActual = null;
            if (animadorDelPersonaje != null) { animadorDelPersonaje.SetBool("estaSujetando", false); }
            Debug.Log("Has tirado el item.");
        }
    }
}
