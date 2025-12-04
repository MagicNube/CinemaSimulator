using UnityEngine;
using UnityEngine.AI;

public class ClienteAnimator : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (agent != null && animator != null)
        {
            float velocidadActual = agent.velocity.magnitude;
            animator.SetFloat("speed", velocidadActual);
        }
    }
}
