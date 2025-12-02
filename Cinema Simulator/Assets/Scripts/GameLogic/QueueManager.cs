using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class QueueManager : MonoBehaviour
{
    // --- VARIABLES DE CONFIGURACIÓN DE LA COLA ---

    [Header("Configuración de la Cola")]
    [Tooltip("Asigna aquí los GameObjects vacíos que marcan las posiciones. ¡El índice 0 es el punto de pedido!")]
    public Transform[] queuePositions;

    // --- VARIABLES DE GENERACIÓN (SPAWN) ---

    [Header("Configuración de Generación")]
    public GameObject customerPrefab;
    public Transform spawnPoint;

    [Header("Ritmo y Cantidad")]
    [Tooltip("Cada cuántos segundos llega un cliente nuevo.")]
    public float spawnInterval = 10f;

    [Tooltip("Clientes base en un día normal.")]
    public int baseMaxCustomers = 30;

    [Header("Configuración de Salida")]
    [Tooltip("Punto donde van los clientes que se van por mal servicio.")]
    public Transform despawnPoint;

    // --- VARIABLES INTERNAS Y ESTRUCTURA ---

    private Queue<GameObject> customerQueue = new Queue<GameObject>();
    private int customersSpawnedCount = 0;
    private int totalCustomersForToday = 0;
    private bool isSpawningActive = false;

    // --- INICIO Y EVENTOS ---

    void Start()
    {
        if (GameManager.Instance != null) GameManager.Instance.AlCambiarFase += HandleCambioFase;
    }

    void OnDestroy()
    {
         if (GameManager.Instance != null) GameManager.Instance.AlCambiarFase -= HandleCambioFase;
    }

    // Método ejemplo para iniciar la generación (debería llamarse desde tu GameManager)
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

    // --- GENERADOR ---
    private IEnumerator CustomerSpawner()
    {
        while (isSpawningActive)
        {
            yield return new WaitForSeconds(spawnInterval);

            // 1. ¿Ya hemos generado todos los de hoy?
            if (customersSpawnedCount >= totalCustomersForToday)
            {
                isSpawningActive = false;
                ComprobarFinDelDia();
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

    // --- GESTIÓN DE LA COLA Y SALIDA ---

    public void AddCustomerToQueue(GameObject newCustomer)
    {
        if (customerQueue.Count < queuePositions.Length)
        {
            customerQueue.Enqueue(newCustomer);
            int targetPosIndex = customerQueue.Count - 1;
            MoveCustomer(newCustomer, queuePositions[targetPosIndex].position);

            // Si el cliente es el primero, debe hacer su pedido (solo si el puesto está vacío)
            if (customerQueue.Count == 1)
            {
                PedidoCliente clienteScript = newCustomer.GetComponent<PedidoCliente>();
                if (clienteScript != null) clienteScript.StartWaitingProcess();
            }
        }
        else { Destroy(newCustomer); }
    }

    // Método llamado por el PedidoCliente cuando el pedido termina (correcto/incorrecto/tiempo)
    public void ClientLeavesQueue(GameObject leavingCustomer, bool success)
    {
        // A. Quitar al cliente de la cola (Asumimos que es el primero)
        customerQueue.Dequeue();

        // B. Mover el cliente a su destino final (NavMeshAgent)
        if (success)
        {
            // Éxito: Va a la posición final (última posición de la cola, e.g., Posición 4)
            Transform finalPos = queuePositions[queuePositions.Length - 1];
            MoveCustomer(leavingCustomer, finalPos.position);
        }
        else
        {
            // Fracaso/Tiempo Agotado: Va al despawn point
            MoveCustomer(leavingCustomer, despawnPoint.position);
        }

        Destroy(leavingCustomer, 3f); // Destruye tras el movimiento

        // C. Mover el resto de la cola hacia adelante
        MoveQueueForward();

        // D. El nuevo cliente de pedido (si existe) debe pedir
        if (customerQueue.Count > 0)
        {
            GameObject newFirstCustomer = customerQueue.Peek();
            PedidoCliente clienteScript = newFirstCustomer.GetComponent<PedidoCliente>();
            if (clienteScript != null) clienteScript.StartWaitingProcess();
        }

        ComprobarFinDelDia();
    }


    // --- LÓGICA AUXILIAR ---

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
        if (agent != null) agent.SetDestination(targetPosition);
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
