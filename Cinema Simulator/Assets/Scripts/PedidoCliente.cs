using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class PedidoCliente : MonoBehaviour
{
    [Tooltip("Tiempo límite para que el jugador complete el pedido.")]
    public float maxWaitTime = 45f;

    private GestorPedidos gestorPedidos;
    private QueueManager queueManager;
    private bool isWaitingForOrder = false;

    void Awake()
    {
        gestorPedidos = GetComponent<GestorPedidos>();
        // NOTA: Es más seguro usar FindObjectOfType en Awake para obtener el Manager
        queueManager = FindObjectOfType<QueueManager>();
    }

    // Llamado por el QueueManager cuando este cliente llega a la posición 0
    public void StartWaitingProcess()
    {
        if (isWaitingForOrder) return;

        StartCoroutine(WaitForOrderCoroutine());
    }

    // Coroutine que gestiona la espera y el tiempo límite
    private IEnumerator WaitForOrderCoroutine()
    {
        isWaitingForOrder = true;

        // 1. EL CLIENTE PIDE
        gestorPedidos.GenerarNuevoPedido();

        // 2. TEMPORIZADOR DE ESPERA
        yield return new WaitForSeconds(maxWaitTime);

        // 3. SI EL TIEMPO SE ACABÓ Y EL PEDIDO NO HA FINALIZADO
        if (isWaitingForOrder)
        {
            Debug.Log($"El cliente {gameObject.name} se cansó de esperar (tiempo agotado).");

            // Limpia el bocadillo/UI y avisa al QueueManager que este cliente se va (INCORRECTAMENTE: false)
            gestorPedidos.DetenerTodo();
            OrderFinished(false);
        }
    }

    // Llamado por el GestorPedidos cuando el pedido termina (correcto o incorrecto)
    public void OrderFinished(bool success)
    {
        if (!isWaitingForOrder) return;

        isWaitingForOrder = false;
        StopAllCoroutines(); // Detiene el temporizador de espera

        // Notifica al QueueManager para que gestione la salida y el avance de la cola
        if (queueManager != null)
        {
            queueManager.ClientLeavesQueue(gameObject, success);
        }
    }
}
