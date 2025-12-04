using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(Collider))]
public class CampanaInteractiva : MonoBehaviour
{
    private AudioSource audioSource;
    private Animator animator;
    private bool puedeSonar = true;
    private QueueManager queueManager;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        // Obtener el Manager de Colas una vez
        queueManager = FindObjectOfType<QueueManager>();
    }

    // Esta función será llamada por el jugador
    public void Interactuar()
    {
        if (!puedeSonar || queueManager == null) return;

        // 1. BUSCAR AL CLIENTE ACTIVO EN LA POSICIÓN DE PEDIDO
        GameObject clienteEnFrente = queueManager.GetCustomerAtOrderPoint();

        if (clienteEnFrente == null)
        {
            Debug.Log("¡DING! No hay cliente en el punto de pedido para llamar.");
            return;
        }

        // 2. Obtener el GestorPedidos del cliente que está al frente
        GestorPedidos gestorDelCliente = clienteEnFrente.GetComponent<GestorPedidos>();

        if (gestorDelCliente != null)
        {
            // 3. Sonar y llamar al cliente
            audioSource.Play();
            if (animator != null) animator.SetTrigger("Sonar");

            // 4. INICIAMOS EL PEDIDO DEL CLIENTE QUE ESTÁ AL FRENTE
            gestorDelCliente.GenerarNuevoPedido();

            // 5. Iniciar cooldown
            puedeSonar = false;
            Invoke("ResetearCooldown", 2.0f);

            Debug.Log("¡DING! Llamando al cliente en posición de pedido.");
        }
        else
        {
            Debug.LogError("El cliente en el punto de pedido no tiene el script GestorPedidos.");
        }
    }

    void ResetearCooldown()
    {
        puedeSonar = true;
    }
}
