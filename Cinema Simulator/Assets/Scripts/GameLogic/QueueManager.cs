using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class QueueManager : MonoBehaviour
{
    [Header("Configuración de la Cola")]
    [Tooltip("Asigna aquí los GameObjects vacíos que marcan las posiciones. ¡El índice 0 es el punto de pedido!")]
    public Transform[] queuePositions;

    [Header("Configuración de Generación")]
    public GameObject customerPrefab;
    public Transform spawnPoint;

    [Header("Ritmo y Cantidad")]
    [Tooltip("Cada cuántos segundos llega un cliente nuevo.")]
    public float spawnInterval = 10f;

    [Tooltip("Clientes base en un día normal.")]
    public int baseMaxCustomers = 30;

    [Header("Configuración de Salida")]
    [Tooltip("Punto de salida para pedidos CORRECTOS.")]
    public Transform exitPointSuccess;
    [Tooltip("Punto de salida para pedidos INCORRECTOS o TIEMPO AGOTADO.")]
    public Transform exitPointFail;

    [Header("Referencias UI (Arrastra aquí los objetos)")]
    public GameObject REF_PanelMonitor;
    public Transform REF_ContenedorItems;

    private Queue<GameObject> customerQueue = new Queue<GameObject>();
    private int customersSpawnedCount = 0;
    private int totalCustomersForToday = 0;
    private bool isSpawningActive = false;

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
            MoveCustomer(newCustomer, queuePositions[targetPosIndex].position);

            if (customerQueue.Count == 1)
            {
                PedidoCliente clienteScript = newCustomer.GetComponent<PedidoCliente>();
                if (clienteScript != null) clienteScript.StartWaitingProcess();
            }
        }
        else { Destroy(newCustomer); }
    }

    public void ClientLeavesQueue(GameObject leavingCustomer, bool success)
    {
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
            Debug.Log("¡Todo vendido! Avisando al GameManager para cerrar.");
            // if (GameManager.Instance != null) { GameManager.Instance.FinalizarServicioPorFaltaDeClientes(); }
        }
    }

    private void MoveQueueForward()
    {
        GameObject[] currentCustomers = customerQueue.ToArray();
        for (int i = 0; i < currentCustomers.Length; i++)
        {
            MoveCustomer(currentCustomers[i], queuePositions[i].position);
        }
    }

    private void MoveCustomer(GameObject customer, Vector3 targetPosition)
    {
        NavMeshAgent agent = customer.GetComponent<NavMeshAgent>();
        if (agent != null && agent.isOnNavMesh) agent.SetDestination(targetPosition);
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
