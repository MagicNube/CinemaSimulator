using UnityEngine;
using TMPro;
using System.Collections;

public class PedidoCliente : MonoBehaviour
{
    public GameObject contenedorBocadillo;
    public TextMeshProUGUI textoDelPedido;
    public Pedido[] posiblesPedidos;

    private Pedido pedidoActual;
    private Coroutine typewriterCoroutine;

    void Start()
    {
        if (contenedorBocadillo != null)
        {
            contenedorBocadillo.SetActive(false);
        }
    }

    public void GenerarNuevoPedido()
    {
        if (textoDelPedido == null || posiblesPedidos.Length == 0 || contenedorBocadillo == null)
        {
            Debug.LogError("¡Falta el BocadilloCanvas, el TextoMeshPro o la lista de pedidos!");
            return;
        }

        contenedorBocadillo.SetActive(true);
        int indiceAleatorio = Random.Range(0, posiblesPedidos.Length);
        pedidoActual = posiblesPedidos[indiceAleatorio];

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }
        typewriterCoroutine = StartCoroutine(EscribirFrase("Buenos días.\n" + pedidoActual.textoDelPedido));
    }

    public bool RecibirItem(ItemData itemDelJugador)
    {
        if (pedidoActual == null)
        {
            Debug.Log("No hay pedido activo. Toca la campana.");
            return false;
        }

        if (itemDelJugador == null)
        {
            StartCoroutine(MostrarMensajeTemporal("¡Manos vacías! " + pedidoActual.textoDelPedido, 1.5f));
            return false;
        }

        if (itemDelJugador.tipoDeItem == pedidoActual.itemRequerido)
        {
            StartCoroutine(MostrarMensajeTemporal("¡Gracias!", 2f, true));
            return true;
        }
        else
        {
            string mensajeError = "No quiero eso... " + pedidoActual.textoDelPedido;
            StartCoroutine(MostrarMensajeTemporal(mensajeError, 2.5f));
            return false;
        }
    }

    IEnumerator EscribirFrase(string frase)
    {
        textoDelPedido.text = "";

        foreach (char letra in frase.ToCharArray())
        {
            textoDelPedido.text += letra;
            yield return new WaitForSeconds(0.02f);
        }
    }

    IEnumerator MostrarMensajeTemporal(string mensaje, float duracion, bool pedidoCompletado = false)
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }
        textoDelPedido.text = mensaje;

        yield return new WaitForSeconds(duracion);

        if (pedidoCompletado)
        {
            contenedorBocadillo.SetActive(false);
            pedidoActual = null;
        }
        else
        {
            typewriterCoroutine = StartCoroutine(EscribirFrase(pedidoActual.textoDelPedido));
        }
    }
}