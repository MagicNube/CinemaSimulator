using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(Collider))]
public class CampanaInteractiva : MonoBehaviour
{
    public PedidoCliente clienteAsociado;

    private AudioSource audioSource;
    private Animator animator;
    private bool puedeSonar = true;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    // Esta función será llamada por el jugador
    public void Interactuar()
    {
        if (puedeSonar && clienteAsociado != null)
        {
            // 1. Sonar
            audioSource.Play();
            if (animator != null) animator.SetTrigger("Sonar");

            // 2. ¡LLAMAR AL CLIENTE!
            clienteAsociado.GenerarNuevoPedido();

            // 3. Iniciar cooldown
            puedeSonar = false;
            Invoke("ResetearCooldown", 2.0f);

            Debug.Log("¡DING! Llamando al cliente.");
        }
        else if (clienteAsociado == null)
        {
            Debug.LogError("¡La campana no tiene un cliente asociado!");
        }
    }

    void ResetearCooldown()
    {
        puedeSonar = true;
    }
}