using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class QueueManager : MonoBehaviour
{
    [Header("Configuración de la Cola")]
    public Transform[] queuePositions;

    [Header("Configuración de Generación")]
    public GameObject customerPrefab;
    public Transform spawnPoint;

    [Header("Ritmo y Cantidad")]
    public float spawnInterval = 10f;
    public int baseMaxCustomers = 30;

    [Header("Configuración de Salida")]
    public Transform exitPointSuccess;
    public Transform exitPointFail;

    [Header("Referencias UI")]
    public GameObject REF_PanelMonitor;
    public Transform REF_ContenedorItems;

    private Queue<GameObject> customerQueue = new Queue<GameObject>();
    private int customersSpawnedCount = 0;
    private int totalCustomersForToday = 0;
    private bool isSpawningActive = false;

    // NUEVO: Para saber qué corrutina pertenece a cada cliente y poder cancelarla si la cola avanza
    private Dictionary<GameObject, Coroutine> walkingCoroutines = new Dictionary<GameObject, Coroutine>();

    void Start()
    {
        if (GameManager.Instance != null) GameManager.Instance.AlCambiarFase += HandleCambioFase;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.AlCambiarFase -= HandleCambioFase;
    }

    public void HandleCambioFase(FaseJuego nuevaFase)
    {
        if (nuevaFase == FaseJuego.Fase2_Servicio) StartSpawningDay();
        else if (nuevaFase == FaseJuego.Fase3_Cierre) StopSpawning();
    }

    public void StartSpawningDay()
    {
        customersSpawnedCount = 0;
        isSpawningActive = true;
        totalCustomersForToday = baseMaxCustomers;

        if (customerPrefab != null && spawnPoint != null && queuePositions.Length > 0)
        {
            StartCoroutine(CustomerSpawner());
        }
    }

    private IEnumerator CustomerSpawner()
    {
        while (isSpawningActive)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (customersSpawnedCount >= totalCustomersForToday)
            {
                isSpawningActive = false;
                ComprobarFinDelDia();
                yield break;
            }

            if (customerQueue.Count < queuePositions.Length)
            {
                GameObject newCustomer = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
                AddCustomerToQueue(newCustomer);
                customersSpawnedCount++;
            }
        }
    }

    public void AddCustomerToQueue(GameObject newCustomer)
    {
        if (customerQueue.Count < queuePositions.Length)
        {
            customerQueue.Enqueue(newCustomer);
            int targetPosIndex = customerQueue.Count - 1;

            // Guardamos la referencia de la corrutina para poder pararla si la cola avanza
            Coroutine rutine = StartCoroutine(RutinaCaminarPorCamino(newCustomer, targetPosIndex));

            if (walkingCoroutines.ContainsKey(newCustomer)) walkingCoroutines[newCustomer] = rutine;
            else walkingCoroutines.Add(newCustomer, rutine);

            if (customerQueue.Count == 1)
            {
                PedidoCliente clienteScript = newCustomer.GetComponent<PedidoCliente>();
                if (clienteScript != null) clienteScript.StartWaitingProcess();
            }
        }
        else
        {
            Destroy(newCustomer);
        }
    }

    private IEnumerator RutinaCaminarPorCamino(GameObject customer, int targetIndex)
    {
        NavMeshAgent agent = customer.GetComponent<NavMeshAgent>();

        if (agent != null && agent.isOnNavMesh)
        {
            // Empezamos desde el último punto disponible de la fila física
            int startIndex = queuePositions.Length - 1;

            for (int i = startIndex; i >= targetIndex; i--)
            {
                if (customer == null) yield break;

                agent.SetDestination(queuePositions[i].position);
                agent.isStopped = false;

                // ESPERA MEJORADA: Usamos remainingDistance para ser más precisos con el NavMesh
                // Esperamos hasta que el agente esté cerca o haya llegado
                while (customer != null && agent.pathPending) { yield return null; } // Esperar a que calcule ruta

                while (customer != null && agent.remainingDistance > agent.stoppingDistance + 0.2f)
                {
                    // Si el agente se queda atascado o deja de tener ruta, salimos para evitar bucle infinito
                    if(!agent.hasPath || agent.isStopped) break;
                    yield return null;
                }
            }
        }

        // Al terminar el camino, borramos al cliente del diccionario de caminantes
        if (customer != null && walkingCoroutines.ContainsKey(customer))
        {
            walkingCoroutines.Remove(customer);
        }
    }

    public void ClientLeavesQueue(GameObject leavingCustomer, bool success)
    {
        // Limpieza del diccionario por si acaso
        if (walkingCoroutines.ContainsKey(leavingCustomer))
        {
            StopCoroutine(walkingCoroutines[leavingCustomer]);
            walkingCoroutines.Remove(leavingCustomer);
        }

        customerQueue.Dequeue();

        Transform targetExit = success ? exitPointSuccess : exitPointFail;
        StartCoroutine(CustomerExitRoutine(leavingCustomer, targetExit.position));

        MoveQueueForward();

        if (customerQueue.Count > 0)
        {
            GameObject newFirstCustomer = customerQueue.Peek();
            PedidoCliente clienteScript = newFirstCustomer.GetComponent<PedidoCliente>();
            if (clienteScript != null) clienteScript.StartWaitingProcess();
        }

        ComprobarFinDelDia();
    }

    private IEnumerator CustomerExitRoutine(GameObject customer, Vector3 exitPosition)
    {
        NavMeshAgent agent = customer.GetComponent<NavMeshAgent>();
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(exitPosition);
            agent.isStopped = false;
        }

        while (customer != null && Vector3.Distance(customer.transform.position, exitPosition) > 1.5f)
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (customer != null)
        {
            Destroy(customer);
        }
    }

    void ComprobarFinDelDia()
    {
        bool yaNoVienenMas = (customersSpawnedCount >= totalCustomersForToday);
        bool colaVacia = (customerQueue.Count == 0);

        if (yaNoVienenMas && colaVacia)
        {
            // if (GameManager.Instance != null) { GameManager.Instance.FinalizarServicioPorFaltaDeClientes(); }
        }
    }

    private void MoveQueueForward()
    {
        GameObject[] currentCustomers = customerQueue.ToArray();
        for (int i = 0; i < currentCustomers.Length; i++)
        {
            // CRUCIAL: Si movemos la cola, debemos cancelar el "paseo bonito" de los que estén entrando
            // y obligarles a ir a su nuevo puesto inmediatamente.
            GameObject c = currentCustomers[i];

            if (walkingCoroutines.ContainsKey(c))
            {
                StopCoroutine(walkingCoroutines[c]);
                walkingCoroutines.Remove(c);
            }

            MoveCustomer(c, queuePositions[i].position);
        }
    }

    private void MoveCustomer(GameObject customer, Vector3 targetPosition)
    {
        NavMeshAgent agent = customer.GetComponent<NavMeshAgent>();
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(targetPosition);
            agent.isStopped = false;
        }
    }

    public GameObject GetCustomerAtOrderPoint()
    {
        if (customerQueue.Count > 0)
        {
            return customerQueue.Peek();
        }
        return null;
    }

    public void StopSpawning()
    {
        isSpawningActive = false;
    }
}
