using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class QueueManager : MonoBehaviour
{
    [Header("Configuración de la Cola")]
    public Transform[] queuePositions;

    [Header("Configuración de Generación")]
    public GameObject[] customerPrefabs;
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
        {
            StartSpawningDay();
        }
        else if (nuevaFase == FaseJuego.Fase3_Cierre)
        {
            StopSpawning();
            BorrarTodosLosClientes();
        }
    }

    public void BorrarTodosLosClientes()
    {
        StopAllCoroutines();
        isSpawningActive = false;
        customerQueue.Clear();
        walkingCoroutines.Clear();

        PedidoCliente[] clientesEnEscena = FindObjectsOfType<PedidoCliente>();
        foreach (PedidoCliente cliente in clientesEnEscena)
        {
            if (cliente != null) Destroy(cliente.gameObject);
        }
    }

    public void StartSpawningDay()
    {
        BorrarTodosLosClientes();
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

                // TRUE porque es nuevo y tiene que hacer el camino largo
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

            // TRUE: Es nuevo, recorre los puntos
            MoveCustomer(newCustomer, queuePositions[targetPosIndex].position, targetPosIndex, true);

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

    private void MoveQueueForward()
    {
        GameObject[] currentCustomers = customerQueue.ToArray();

        for (int i = 0; i < currentCustomers.Length; i++)
        {
            GameObject c = currentCustomers[i];

            // FALSE: No es nuevo, solo avanza un paso (camina directo)
            MoveCustomer(c, queuePositions[i].position, i, false);
        }
    }

    // --- FUNCIÓN UNIFICADA DE MOVIMIENTO ---
    private void MoveCustomer(GameObject customer, Vector3 targetPosition, int targetIndex, bool esNuevoCliente)
    {
        // Cancelar corrutina anterior si existía
        if (walkingCoroutines.ContainsKey(customer))
        {
            if (walkingCoroutines[customer] != null) StopCoroutine(walkingCoroutines[customer]);
            walkingCoroutines.Remove(customer);
        }

        Coroutine rutine = StartCoroutine(RutinaCaminarInteligente(customer, targetIndex, esNuevoCliente));
        walkingCoroutines.Add(customer, rutine);
    }

    private IEnumerator RutinaCaminarInteligente(GameObject customer, int targetIndex, bool esNuevoCliente)
    {
        if (customer == null) yield break;

        NavMeshAgent agent = customer.GetComponent<NavMeshAgent>();
        PedidoCliente pc = customer.GetComponent<PedidoCliente>();

        // Si empieza a andar, ya no está quieto en el mostrador
        if (pc != null) pc.EstaEnMostrador = false;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;

            // CASO 1: Cliente Nuevo (Camina por los puntos definidos desde el final)
            if (esNuevoCliente)
            {
                int startIndex = queuePositions.Length - 1;
                for (int i = startIndex; i >= targetIndex; i--)
                {
                    if (customer == null) yield break;
                    agent.SetDestination(queuePositions[i].position);

                    // Esperar a que llegue a este punto intermedio
                    while (customer != null && agent.pathPending) yield return null;
                    while (customer != null && agent.remainingDistance > agent.stoppingDistance + 0.1f) yield return null;
                }
            }
            // CASO 2: Cliente Avanzando (Camina DIRECTO a su hueco)
            else
            {
                agent.SetDestination(queuePositions[targetIndex].position);

                // Esperar a que llegue al destino final
                while (customer != null && agent.pathPending) yield return null;
                while (customer != null && agent.remainingDistance > agent.stoppingDistance + 0.1f) yield return null;
            }
        }

        // Si el destino era el 0 (Mostrador), activamos la bandera
        if (targetIndex == 0 && pc != null && customer != null)
        {
            pc.EstaEnMostrador = true;

            // Truco visual: Asegurar rotación mirando al jugador (opcional)
            // customer.transform.LookAt(spawnPoint.position);
        }

        if (customer != null && walkingCoroutines.ContainsKey(customer))
        {
            walkingCoroutines.Remove(customer);
        }
    }

    public void ClientLeavesQueue(GameObject leavingCustomer, bool success)
    {
        // 1. Sacar al que se va
        if (walkingCoroutines.ContainsKey(leavingCustomer))
        {
            StopCoroutine(walkingCoroutines[leavingCustomer]);
            walkingCoroutines.Remove(leavingCustomer);
        }

        if (customerQueue.Count > 0)
            customerQueue.Dequeue();

        Transform targetExit = success ? exitPointSuccess : exitPointFail;
        StartCoroutine(CustomerExitRoutine(leavingCustomer, targetExit.position));

        // 2. Mover al resto (esto llamará a MoveCustomer con esNuevo = false)
        MoveQueueForward();

        // 3. Activar lógica del siguiente (si hay)
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
        PedidoCliente pc = customer.GetComponent<PedidoCliente>();

        // Al irse, ya no está en el mostrador
        if (pc != null) pc.EstaEnMostrador = false;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.ResetPath();
            agent.SetDestination(exitPosition);
            agent.speed = 3.5f; // Un poco más rápido al irse
        }

        float t = 0f;
        while (customer != null && t < 6f)
        {
            if (Vector3.Distance(customer.transform.position, exitPosition) < 1.5f) break;
            t += Time.deltaTime;
            yield return null;
        }

        if (customer != null) Destroy(customer);
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

    public GameObject GetCustomerAtOrderPoint()
    {
        if (customerQueue.Count > 0) return customerQueue.Peek();
        return null;
    }

    public void StopSpawning()
    {
        isSpawningActive = false;
    }

    public void DestroyClientsEnCola()
    {
        BorrarTodosLosClientes();
    }
}
