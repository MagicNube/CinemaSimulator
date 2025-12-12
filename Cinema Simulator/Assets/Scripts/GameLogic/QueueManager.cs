using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class QueueManager : MonoBehaviour
{
    [Header("Configuración de la Cola")]
    public Transform[] queuePositions;

    [Header("Configuración de Generación")]
    public GameObject[] customerPrefabs;   // <-- AHORA ES UN ARRAY
    public Transform spawnPoint;

    [Header("Ritmo y Cantidad")]
    public float spawnInterval = 10f;
    public int baseCustomers = 10;

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

    private Dictionary<GameObject, Coroutine> walkingCoroutines = new Dictionary<GameObject, Coroutine>();

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AlCambiarFase += HandleCambioFase;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AlCambiarFase -= HandleCambioFase;
    }

    public void HandleCambioFase(FaseJuego nuevaFase)
    {
        if (nuevaFase == FaseJuego.Fase2_Servicio)
            StartSpawningDay();
        else if (nuevaFase == FaseJuego.Fase3_Cierre)
            StopSpawning();
    }

    public void StartSpawningDay()
    {
        customersSpawnedCount = 0;
        isSpawningActive = true;
        totalCustomersForToday = baseCustomers;

        if (customerPrefabs.Length > 0 && spawnPoint != null && queuePositions.Length > 0)
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

            if (customerQueue.Count < queuePositions.Length && customerPrefabs.Length > 0)
            {
                int randomIndex = Random.Range(0, customerPrefabs.Length);
                GameObject prefabElegido = customerPrefabs[randomIndex];

                GameObject newCustomer = Instantiate(prefabElegido, spawnPoint.position, Quaternion.identity);
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

            Coroutine rutine = StartCoroutine(RutinaCaminarPorCamino(newCustomer, targetPosIndex));

            if (walkingCoroutines.ContainsKey(newCustomer))
                walkingCoroutines[newCustomer] = rutine;
            else
                walkingCoroutines.Add(newCustomer, rutine);

            if (customerQueue.Count == 1)
            {
                PedidoCliente clienteScript = newCustomer.GetComponent<PedidoCliente>();
                if (clienteScript != null)
                    clienteScript.StartWaitingProcess();
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
            int startIndex = queuePositions.Length - 1;

            for (int i = startIndex; i >= targetIndex; i--)
            {
                if (customer == null) yield break;

                agent.isStopped = false;
                agent.SetDestination(queuePositions[i].position);

                while (customer != null && agent.pathPending)
                    yield return null;

                while (customer != null && agent.remainingDistance > agent.stoppingDistance + 0.2f)
                {
                    if (!agent.hasPath || agent.isStopped)
                        break;

                    yield return null;
                }
            }
        }

        if (customer != null && walkingCoroutines.ContainsKey(customer))
        {
            walkingCoroutines.Remove(customer);
        }
    }

    public void ClientLeavesQueue(GameObject leavingCustomer, bool success)
    {
        if (walkingCoroutines.ContainsKey(leavingCustomer))
        {
            StopCoroutine(walkingCoroutines[leavingCustomer]);
            walkingCoroutines.Remove(leavingCustomer);
        }

        if (customerQueue.Count > 0)
            customerQueue.Dequeue();

        Transform targetExit = success ? exitPointSuccess : exitPointFail;
        StartCoroutine(CustomerExitRoutine(leavingCustomer, targetExit.position));

        MoveQueueForward();

        if (customerQueue.Count > 0)
        {
            GameObject newFirstCustomer = customerQueue.Peek();
            PedidoCliente clienteScript = newFirstCustomer.GetComponent<PedidoCliente>();

            if (clienteScript != null)
                clienteScript.StartWaitingProcess();
        }

        ComprobarFinDelDia();
    }

    private IEnumerator CustomerExitRoutine(GameObject customer, Vector3 exitPosition)
    {
        NavMeshAgent agent = customer.GetComponent<NavMeshAgent>();

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.ResetPath();
            agent.SetDestination(exitPosition);
        }

        float t = 0f;
        float tiempoMax = 6f;

        while (customer != null && Vector3.Distance(customer.transform.position, exitPosition) > 1.5f)
        {
            t += Time.deltaTime;
            if (t > tiempoMax) break;
            yield return null;
        }

        if (customer != null)
            Destroy(customer);
    }

    void ComprobarFinDelDia()
    {
        bool yaNoVienenMas = (customersSpawnedCount >= totalCustomersForToday);
        bool colaVacia = (customerQueue.Count == 0);

        if (yaNoVienenMas && colaVacia)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.FinalizarServicioPorFaltaDeClientes();
        }
    }

    private void MoveQueueForward()
    {
        GameObject[] currentCustomers = customerQueue.ToArray();

        for (int i = 0; i < currentCustomers.Length; i++)
        {
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
            agent.isStopped = false;
            agent.SetDestination(targetPosition);
        }
    }

    public GameObject GetCustomerAtOrderPoint()
    {
        if (customerQueue.Count > 0)
            return customerQueue.Peek();

        return null;
    }

    public void StopSpawning()
    {
        isSpawningActive = false;
    }

    public void DestroyClientsEnCola()
    {
        while (customerQueue.Count > 0)
        {
            GameObject cliente = customerQueue.Dequeue();
            if (cliente != null)
                Destroy(cliente);
        }
    }
}
