using UnityEngine;
using TMPro; // ¡Importante para el texto!
using System.Collections; // ¡Importante para las esperas!

// --- ¡NUEVO! Definición de la clase "Pedido" ---
// [System.Serializable] permite que esto aparezca en el Inspector.
[System.Serializable]
public class Pedido
{
    public string textoDelPedido;           // "¡Quiero un perrito!"
    public ItemData.TipoDeItem itemRequerido; // ItemData.TipoDeItem.Perrito
}

public class PedidoCliente : MonoBehaviour
{
    // Arrastra tu TextMeshPro (TextoPedido) aquí en el Inspector
    // *** ¡OJO! Asegúrate de que sea TextMeshProUGUI si es un Canvas, o TextMeshPro si es 3D ***
    // Si usaste el Canvas (como te expliqué), cambia la línea de abajo por:
    // public TextMeshProUGUI textoDelPedido;
    public TextMeshPro textoDelPedido;


    // --- ¡MODIFICADO! ---
    // Ya no es un string[], ahora es un array de nuestra nueva clase "Pedido"
    public Pedido[] posiblesPedidos;

    // Guardamos el pedido actual para poder comprobarlo
    private Pedido pedidoActual;

    void Start()
    {
        // El cliente empieza pidiendo algo
        //GenerarNuevoPedido();
    }

    // Esta función sigue siendo pública, para que la campana la llame
    public void GenerarNuevoPedido()
    {
        if (textoDelPedido == null || posiblesPedidos.Length == 0)
        {
            Debug.LogError("¡Falta el TextoMeshPro o la lista de pedidos!");
            return;
        }

        // 1. Elige un pedido al azar de la lista
        int indiceAleatorio = Random.Range(0, posiblesPedidos.Length);
        pedidoActual = posiblesPedidos[indiceAleatorio]; // Guardamos el pedido COMPLETO

        // 2. Muestra el texto del pedido en el bocadillo
        textoDelPedido.text = pedidoActual.textoDelPedido;

        Debug.Log("¡NUEVO PEDIDO!: " + pedidoActual.textoDelPedido);
    }

    // --- ¡FUNCIÓN TOTALMENTE NUEVA! ---
    // Esta es la función que llamará el JUGADOR al hacer clic
    public bool RecibirItem(ItemData itemDelJugador)
    {
        // Caso 1: El jugador hace clic con las manos vacías
        if (itemDelJugador == null)
        {
            StartCoroutine(MostrarMensajeTemporal("¡Manos vacías!", 1.5f));
            return false; // No fue un éxito
        }

        // Caso 2: El jugador entrega un item. ¿Es el correcto?
        if (itemDelJugador.tipoDeItem == pedidoActual.itemRequerido)
        {
            // ¡ÉXITO!
            // --- ¡CAMBIO AQUÍ! ---
            StartCoroutine(MostrarMensajeTemporal("Gracias", 2f, true));
            return true; // ¡Sí fue un éxito!
        }
        else
        {
            // Caso 3: ¡Item incorrecto!
            // --- ¡CAMBIO AQUÍ! ---
            string mensajeError = "No quiero eso...  " + pedidoActual.textoDelPedido;
            StartCoroutine(MostrarMensajeTemporal(mensajeError, 2.5f)); // 2.5s para que dé tiempo a leerlo
            return false; // No fue un éxito
        }
    }

    // --- ¡NUEVA FUNCIÓN DE AYUDA (Corrutina)! ---
    // Muestra un mensaje ("OK", "No", etc.) y luego vuelve al pedido
    IEnumerator MostrarMensajeTemporal(string mensaje, float duracion, bool pedidoCompletado = false)
    {
        // Guarda el texto original para después
        string textoOriginal = pedidoActual.textoDelPedido;

        textoDelPedido.text = mensaje;
        yield return new WaitForSeconds(duracion);

        if (pedidoCompletado)
        {
            // Si el pedido fue "OK" (Gracias), genera uno nuevo
            textoDelPedido.text = "Dale a la campana para atenderme otra vez.";
        }
        else
        {
            // Si fue "No", vuelve a mostrar el pedido original
            textoDelPedido.text = textoOriginal;
        }
    }
}