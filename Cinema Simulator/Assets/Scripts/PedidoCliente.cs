using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class PedidoCliente : MonoBehaviour
{
    [Tooltip("Tiempo límite (paciencia) que tiene el cliente.")]
    public float maxWaitTime = 45f;

    // --- VARIABLES NUEVAS ---
    [HideInInspector] public bool EstaEnMostrador = false; // True solo cuando llega al punto 0
    [HideInInspector] public bool TienePedidoActivo = false; // True si ya le diste a la campana
    // ------------------------

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

    private IEnumerator WaitForOrderCoroutine()
    {
        isWaitingForOrder = true;

        // NOTA: Quitamos la autogeneración. Esperamos a la campana.
        Debug.Log($"Cliente {gameObject.name}: Esperando campana...");

        yield return new WaitForSeconds(maxWaitTime);

        if (isWaitingForOrder)
        {
            Debug.Log($"Cliente {gameObject.name} se cansó.");
            if (gestorPedidos != null) gestorPedidos.ForzarCierrePedido();
            OrderFinished(false);
        }
    }

    public void OrderFinished(bool success)
    {
        if (!isWaitingForOrder) return;

        isWaitingForOrder = false;
        StopAllCoroutines();

        if (queueManager != null)
        {
            queueManager.ClientLeavesQueue(gameObject, success);
        }
    }
}
