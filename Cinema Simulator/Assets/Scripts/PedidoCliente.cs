using UnityEngine;
using TMPro; // ¡Importante para el texto!

public class PedidoCliente : MonoBehaviour
{
    // Arrastra tu TextMeshPro (TextoPedido) aquí en el Inspector
    public TextMeshPro textoDelPedido;

    // Lista de cosas que el cliente puede pedir
    private string[] posiblesPedidos = {
        "¡Unas palomitas!",
        "¡Un refresco, por favor!",
        "¡Quiero un perrito!",
        "¡Palomitas y bebida!",
        "¡Todo lo que tengas!"
    };

    void Start()
    {
        // El cliente empieza pidiendo algo
        //GenerarNuevoPedido();
    }

    // Esta función es PÚBLICA, para que la campana pueda llamarla
    public void GenerarNuevoPedido()
    {
        if (textoDelPedido == null || posiblesPedidos.Length == 0)
        {
            Debug.LogError("¡Falta el TextoMeshPro o la lista de pedidos!");
            return;
        }

        // 1. Elige un pedido al azar de la lista
        int indiceAleatorio = Random.Range(0, posiblesPedidos.Length);
        string nuevoPedido = posiblesPedidos[indiceAleatorio];

        // 2. Muestra el pedido en el bocadillo
        textoDelPedido.text = nuevoPedido;

        Debug.Log("¡NUEVO PEDIDO!: " + nuevoPedido);
    }
}