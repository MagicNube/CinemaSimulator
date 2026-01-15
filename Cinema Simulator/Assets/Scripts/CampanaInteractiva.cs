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
        queueManager = FindObjectOfType<QueueManager>();
    }

    public void Interactuar()
    {
        if (!puedeSonar || queueManager == null) return;

        // 1. Obtener cliente en posición
        GameObject clienteEnFrente = queueManager.GetCustomerAtOrderPoint();

        if (clienteEnFrente == null)
        {
            // Sonido opcional si no hay nadie
            SonarCampana(false);
            return;
        }

        PedidoCliente estado = clienteEnFrente.GetComponent<PedidoCliente>();
        GestorPedidos gestor = clienteEnFrente.GetComponent<GestorPedidos>();

        // --- VALIDACIONES SIMPLES ---

        // 1. Si no ha llegado físicamente al mostrador -> Salimos
        if (estado != null && !estado.EstaEnMostrador)
        {
            SonarCampana(false); // Suena pero no hace nada
            return;
        }

        // 2. Si ya tiene pedido -> Salimos
        if (estado != null && estado.TienePedidoActivo)
        {
            SonarCampana(false);
            return;
        }
        // -----------------------------

        // SI PASA LAS VALIDACIONES:
        if (gestor != null)
        {
            SonarCampana(true);

            gestor.GenerarNuevoPedido();

            // Bloquear para que no pueda pedir otra vez
            if (estado != null) estado.TienePedidoActivo = true;

            // Pequeño cooldown
            puedeSonar = false;
            Invoke("ResetearCooldown", 1.0f);
        }
    }

    void SonarCampana(bool exito)
    {
        if (audioSource) audioSource.Play();
        if (animator) animator.SetTrigger("Sonar");
        if (exito) Debug.Log("¡DING! Nuevo pedido generado.");
    }

    void ResetearCooldown()
    {
        puedeSonar = true;
    }
}
