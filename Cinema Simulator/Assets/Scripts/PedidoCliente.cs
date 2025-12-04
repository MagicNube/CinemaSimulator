using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class PedidoCliente : MonoBehaviour
{
    [Tooltip("Tiempo límite (paciencia) que tiene el cliente.")]
    public float maxWaitTime = 45f;

    private GestorPedidos gestorPedidos;
    private QueueManager queueManager;
    private bool isWaitingForOrder = false;

    void Awake()
    {
        gestorPedidos = GetComponent<GestorPedidos>();
        queueManager = FindObjectOfType<QueueManager>();
    }

    // Llamado por el QueueManager cuando este cliente llega a la posición 0
    public void StartWaitingProcess()
    {
        if (isWaitingForOrder) return;
        StartCoroutine(WaitForOrderCoroutine());
    }

    // Coroutine que gestiona la PACIENCIA del cliente
    private IEnumerator WaitForOrderCoroutine()
    {
        isWaitingForOrder = true;

        // --- CORRECCIÓN AQUÍ ---
        // Borramos o comentamos esta línea.
        // Ya no generamos el pedido automáticamente al llegar.
        // gestorPedidos.GenerarNuevoPedido();
        // -----------------------

        Debug.Log($"Cliente {gameObject.name}: He llegado al mostrador. Espero a que me toquen la campana...");

        // El temporizador sigue corriendo (es su paciencia antes de irse enfadado)
        yield return new WaitForSeconds(maxWaitTime);

        // SI EL TIEMPO SE ACABÓ Y SIGUE ESPERANDO (Nadie le atendió o tardaron mucho)
        if (isWaitingForOrder)
        {
            Debug.Log($"El cliente {gameObject.name} se cansó de esperar (tiempo agotado).");

            gestorPedidos.ForzarCierrePedido();
            OrderFinished(false);
        }
    }

    // Llamado por el GestorPedidos cuando el pedido termina (correcto o incorrecto)
    public void OrderFinished(bool success)
    {
        if (!isWaitingForOrder) return; // Ya se había ido

        isWaitingForOrder = false;
        StopAllCoroutines(); // Detiene el temporizador de paciencia

        if (queueManager != null)
        {
            queueManager.ClientLeavesQueue(gameObject, success);
        }
    }
}
