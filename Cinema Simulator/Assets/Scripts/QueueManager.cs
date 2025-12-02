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

    [Header("Configuración de Generación de Clientes")]
    [Tooltip("El Prefab del cliente que será instanciado.")]
    public GameObject customerPrefab;

    [Tooltip("Punto donde se instanciarán los nuevos clientes.")]
    public Transform spawnPoint;

    [Tooltip("Tiempo en segundos entre cada intento de generar un cliente.")]
    public float spawnInterval = 10f; // Generar cada 10 segundos

    [Tooltip("Máximo de clientes que se generarán en total durante el tiempo límite.")]
    public int maxCustomersAllowed = 50;

    [Tooltip("Tiempo total (en segundos) que el generador estará activo.")]
    public float timeLimitForSpawning = 180f; // Por ejemplo, 3 minutos (180s)

    // --- VARIABLES DE ESTADO Y ESTRUCTURA ---

    private Queue<GameObject> customerQueue = new Queue<GameObject>();
    private int customersSpawnedCount = 0;
    private float spawningTimer = 0f;
    private bool isSpawningActive = true;

    // --- MÉTODOS DE INICIO ---

    void Start()
    {
        // Iniciamos la Coroutine al inicio del juego
        if (customerPrefab != null && spawnPoint != null)
        {
            StartCoroutine(CustomerSpawner());
        }
        else
        {
            Debug.LogError("Asigna el Prefab del Cliente y el Spawn Point en el Inspector.");
            isSpawningActive = false;
        }
    }

    // --- COROUTINE DE GENERACIÓN (SPAWNING) ---

    private IEnumerator CustomerSpawner()
    {
        while (isSpawningActive)
        {
            // 1. Esperamos el intervalo de tiempo antes de intentar generar
            yield return new WaitForSeconds(spawnInterval);

            // 2. Aumentamos el contador de tiempo total
            spawningTimer += spawnInterval;

            // --- Lógica de Fin de Generación ---

            // 3. Condición: Se acabó el tiempo
            if (spawningTimer >= timeLimitForSpawning)
            {
                Debug.Log($"¡Tiempo límite de generación ({timeLimitForSpawning}s) alcanzado! Deteniendo el generador.");
                isSpawningActive = false;
                yield break; // Termina la Coroutine
            }

            // 4. Condición: Se alcanzó el límite total de clientes generados
            if (customersSpawnedCount >= maxCustomersAllowed)
            {
                Debug.Log($"¡Límite total de clientes generados ({maxCustomersAllowed}) alcanzado! Deteniendo el generador.");
                isSpawningActive = false;
                yield break; // Termina la Coroutine
            }

            // --- Lógica de Generación y Capacidad ---

            // 5. Condición: Hay espacio en la cola?
            if (customerQueue.Count < queuePositions.Length)
            {
                // A. Instancia el cliente en el punto de spawn
                GameObject newCustomer = Instantiate(
                    customerPrefab,
                    spawnPoint.position,
                    Quaternion.identity
                );

                // B. Añade el nuevo cliente a la cola
                AddCustomerToQueue(newCustomer);

                customersSpawnedCount++;
                Debug.Log($"Cliente generado y añadido. Clientes totales generados: {customersSpawnedCount}");
            }
            else
            {
                Debug.Log("No hay espacio en la cola. Saltando generación y esperando el próximo intervalo.");
                // Si no hay espacio, el bucle simplemente espera otro 'spawnInterval'
            }
        }
    }

    // --- GESTIÓN DE LA COLA ---

    public void AddCustomerToQueue(GameObject newCustomer)
    {
        // Esta comprobación es redundante con el Spawner, pero previene errores si llamas a AddCustomerToQueue desde otro script
        if (customerQueue.Count < queuePositions.Length)
        {
            customerQueue.Enqueue(newCustomer);

            // Mueve al nuevo cliente a la última posición disponible de la cola
            int targetPosIndex = customerQueue.Count - 1;
            MoveCustomer(newCustomer, queuePositions[targetPosIndex].position);
        }
        else
        {
            Debug.Log($"Cliente {newCustomer.name} intentó entrar pero la cola estaba llena.");
            // Opcional: Destruir el cliente si no cabe.
            Destroy(newCustomer);
        }
    }

    public void CompleteOrder()
    {
        if (customerQueue.Count > 0)
        {
            // Saca al cliente que acaba de pedir (el primero)
            GameObject completedCustomer = customerQueue.Dequeue();

            // Lógica de salida del cliente (destruir o mover a una posición de "salida")
            // Asumiendo que el cliente es destruido después de salir de la zona de servicio.
            Destroy(completedCustomer, 0.5f);

            // Mueve el resto de los clientes una posición hacia adelante
            MoveQueueForward();
        }
    }

    private void MoveQueueForward()
    {
        GameObject[] currentCustomers = customerQueue.ToArray();

        for (int i = 0; i < currentCustomers.Length; i++)
        {
            GameObject customer = currentCustomers[i];
            // La posición 'i' es la nueva posición
            Transform targetPosition = queuePositions[i];

            MoveCustomer(customer, targetPosition.position);
        }
    }

    private void MoveCustomer(GameObject customer, Vector3 targetPosition)
    {
        NavMeshAgent agent = customer.GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.SetDestination(targetPosition);
        }
        else
        {
            Debug.LogError($"¡El cliente {customer.name} no tiene NavMeshAgent! Asegúrate de que el Prefab lo tiene.");
        }
    }

    // Método público para detener la generación desde otro script (ej. Game Over)
    public void StopSpawning()
    {
        isSpawningActive = false;
        Debug.Log("Generación de clientes detenida por script externo.");
    }
}
