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
    [Tooltip("Cada cuántos segundos llega un cliente nuevo (Fijo).")]
    public float spawnInterval = 5f; // <--- RITMO FIJO

    [Tooltip("Clientes base en un día normal.")]
    public int baseMaxCustomers = 30;

    // --- VARIABLES INTERNAS ---
    private Queue<GameObject> customerQueue = new Queue<GameObject>();
    private int customersSpawnedCount = 0;
    private int totalCustomersForToday = 0;
    private bool isSpawningActive = false;

    // --- INICIO ---
    void Start()
    {
        if (GameManager.Instance != null) GameManager.Instance.AlCambiarFase += HandleCambioFase;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.AlCambiarFase -= HandleCambioFase;
    }

    void HandleCambioFase(FaseJuego nuevaFase)
    {
        if (nuevaFase == FaseJuego.Fase2_Servicio) StartSpawningDay();
        else if (nuevaFase == FaseJuego.Fase3_Cierre) StopSpawning();
    }

    public void StartSpawningDay()
    {
        customersSpawnedCount = 0;
        isSpawningActive = true;

        // 1. Calculamos CANTIDAD TOTAL según la película
        float multiplicador = 1.0f;
        TabletManager tablet = FindObjectOfType<TabletManager>();
        if (tablet != null) multiplicador = tablet.multiplicadorClientes;

        totalCustomersForToday = Mathf.RoundToInt(baseMaxCustomers * multiplicador);

        Debug.Log($"Apertura: Vendrán {totalCustomersForToday} clientes. Intervalo: {spawnInterval}s");

        if (customerPrefab != null && spawnPoint != null)
        {
            StartCoroutine(CustomerSpawner());
        }
    }

    // --- GENERADOR ---
    private IEnumerator CustomerSpawner()
    {
        while (isSpawningActive)
        {
            // Esperamos el tiempo FIJO definido en el inspector
            yield return new WaitForSeconds(spawnInterval);

            // 1. ¿Ya hemos generado todos los de hoy?
            if (customersSpawnedCount >= totalCustomersForToday)
            {
                Debug.Log("Ya no vienen más clientes nuevos.");
                isSpawningActive = false;
                ComprobarFinDelDia(); // Chequeamos si también se ha vaciado la cola
                yield break;
            }

            // 2. ¿Hay sitio en la cola?
            if (customerQueue.Count < queuePositions.Length)
            {
                GameObject newCustomer = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
                AddCustomerToQueue(newCustomer);
                customersSpawnedCount++;
            }
        }
    }

    // --- GESTIÓN COLA ---
    public void AddCustomerToQueue(GameObject newCustomer)
    {
        if (customerQueue.Count < queuePositions.Length)
        {
            customerQueue.Enqueue(newCustomer);
            int targetPosIndex = customerQueue.Count - 1;
            MoveCustomer(newCustomer, queuePositions[targetPosIndex].position);
        }
        else { Destroy(newCustomer); }
    }

    public void CompleteOrder()
    {
        if (customerQueue.Count > 0)
        {
            GameObject completedCustomer = customerQueue.Dequeue();
            Destroy(completedCustomer, 0.5f);
            MoveQueueForward();

            // IMPORTANTE: Cada vez que atendemos a uno, miramos si era el último
            ComprobarFinDelDia();
        }
    }

    // --- COMPROBACIÓN DE FIN DE FASE AUTOMÁTICO ---
    void ComprobarFinDelDia()
    {
        // Si ya no vamos a generar más clientes (isSpawningActive == false o cupo lleno)
        // Y la cola está vacía (customerQueue.Count == 0)
        // SIGNIFICA QUE HEMOS TERMINADO POR HOY

        bool yaNoVienenMas = (customersSpawnedCount >= totalCustomersForToday);
        bool colaVacia = (customerQueue.Count == 0);

        if (yaNoVienenMas && colaVacia)
        {
            Debug.Log("¡Todo vendido! Avisando al GameManager para cerrar.");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.FinalizarServicioPorFaltaDeClientes();
            }
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
        if (agent != null) agent.SetDestination(targetPosition);
    }

    public void StopSpawning()
    {
        isSpawningActive = false;
    }
}