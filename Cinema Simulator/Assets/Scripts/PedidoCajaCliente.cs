using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI; // Necesario si usas Toggles estándar de UI, aunque aquí usaremos TMP

public class PedidoCajaCliente : MonoBehaviour
{
    [Header("Referencias UI del Monitor")]
    public GameObject panelComanda;

    public TextMeshProUGUI textoComandaActual;

    public Transform contenedorItemsComanda;

    [Header("Configuración de Pedidos")]
    public GameObject prefabItemLista;
    public Pedido[] posiblesPedidos;

    private Pedido pedidoActual;
    private Coroutine typewriterCoroutine;

    void Start()
    {
        if (panelComanda != null)
        {
            panelComanda.SetActive(false);
        }
    }

    public void GenerarNuevoPedido()
    {
        if (textoComandaActual == null || posiblesPedidos.Length == 0 || panelComanda == null || contenedorItemsComanda == null)
        {
            Debug.LogError("¡Faltan referencias en el inspector! Revisa PanelComanda, Texto, Contenedor o el Prefab.");
            return;
        }

        panelComanda.SetActive(true);

        int indiceAleatorio = Random.Range(0, posiblesPedidos.Length);
        pedidoActual = posiblesPedidos[indiceAleatorio];


        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }
        typewriterCoroutine = StartCoroutine(EscribirFrase("Orden #00" + (indiceAleatorio + 1) + ":\n" + pedidoActual.textoDelPedido));

        ActualizarListaVisual();
    }

    private void ActualizarListaVisual()
    {
        foreach (Transform hijo in contenedorItemsComanda)
        {
            Destroy(hijo.gameObject);
        }

        if (prefabItemLista != null && pedidoActual != null)
        {
            GameObject nuevoItem = Instantiate(prefabItemLista, contenedorItemsComanda);

            TextMeshProUGUI textoDelPrefab = nuevoItem.GetComponentInChildren<TextMeshProUGUI>();

            if (textoDelPrefab != null)
            {
                textoDelPrefab.text = "- " + pedidoActual.itemRequerido.ToString();
            }

            Toggle checkbox = nuevoItem.GetComponentInChildren<Toggle>();
            if (checkbox != null) checkbox.isOn = false;
        }
    }

    public bool RecibirItem(ItemData itemDelJugador)
    {
        if (pedidoActual == null)
        {
            Debug.Log("No hay pedido activo.");
            return false;
        }

        if (itemDelJugador == null)
        {
            StartCoroutine(MostrarMensajeTemporal("¡Pedido vacío! Faltan items.", 1.5f));
            return false;
        }

        if (itemDelJugador.tipoDeItem == pedidoActual.itemRequerido)
        {
            MarcarCheckboxVisual(true);

            StartCoroutine(MostrarMensajeTemporal("¡Pedido Completado! Enviando...", 2f, true));
            return true;
        }
        else
        {
            string mensajeError = "Error: El cliente no pidió " + itemDelJugador.tipoDeItem;
            StartCoroutine(MostrarMensajeTemporal(mensajeError, 2.5f));
            return false;
        }
    }

    private void MarcarCheckboxVisual(bool estado)
    {
        if (contenedorItemsComanda.childCount > 0)
        {
            Toggle checkbox = contenedorItemsComanda.GetChild(0).GetComponentInChildren<Toggle>();
            if (checkbox != null) checkbox.isOn = estado;
        }
    }

    IEnumerator EscribirFrase(string frase)
    {
        textoComandaActual.text = "";

        foreach (char letra in frase.ToCharArray())
        {
            textoComandaActual.text += letra;
            yield return new WaitForSeconds(0.015f);
        }
    }

    IEnumerator MostrarMensajeTemporal(string mensaje, float duracion, bool pedidoCompletado = false)
    {
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);

        textoComandaActual.text = mensaje;

        yield return new WaitForSeconds(duracion);

        if (pedidoCompletado)
        {
            panelComanda.SetActive(false);
            pedidoActual = null;
            foreach (Transform hijo in contenedorItemsComanda) Destroy(hijo.gameObject);
        }
        else
        {
            typewriterCoroutine = StartCoroutine(EscribirFrase("Reintento:\n" + pedidoActual.textoDelPedido));
        }
    }
}