using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // 1. NECESARIO para usar NavMeshAgent

public class QueueManager : MonoBehaviour
{
    // --- VARIABLES DE CONFIGURACIÓN ---

    [Header("Configuración de la Cola")]
    [Tooltip("Asigna aquí los GameObjects vacíos que marcan las posiciones. ¡El índice 0 es el punto de pedido!")]
    public Transform[] queuePositions; // Puntos de parada en la escena

    [Tooltip("El cliente que usarás para la prueba de movimiento (cápsula verde).")]
    public GameObject TestCustomer; // Cliente temporal para probar con la tecla 'T'

    // --- ESTRUCTURA DE LA COLA ---

    // Una Queue de C# mantiene el orden FIFO (First-In, First-Out).
    private Queue<GameObject> customerQueue = new Queue<GameObject>();

    // --- LÓGICA DE PRUEBA (Temporal) ---

    void Update()
    {
        // 2. PRUEBA: Presiona la tecla 'T' para añadir el cliente de prueba a la cola
        if (Input.GetKeyDown(KeyCode.T) && TestCustomer != null)
        {
            // Usamos 'customerQueue.Count == 0' para asegurar que el cliente de prueba solo se añade una vez
            // y que la cola esté vacía.
            if (customerQueue.Count == 0)
            {
                Debug.Log("Cliente de prueba entrando en la cola (presiona 'T').");
                AddCustomerToQueue(TestCustomer);
            }
            else
            {
                Debug.Log("La cola ya tiene clientes. Usa 'CompleteOrder' para moverlos.");
            }
        }

        // PRUEBA: Presiona la tecla 'C' para simular que el pedido ha terminado
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (customerQueue.Count > 0)
            {
                Debug.Log("Pedido completado. Cliente saliendo y cola avanzando.");
                CompleteOrder();
            }
            else
            {
                Debug.Log("La cola está vacía.");
            }
        }
    }

    // --- MÉTODOS PÚBLICOS ---

    public void AddCustomerToQueue(GameObject newCustomer)
    {
        // 3. Verifica si la cola no está llena
        if (customerQueue.Count < queuePositions.Length)
        {
            customerQueue.Enqueue(newCustomer);

            // Mueve al nuevo cliente a la última posición disponible de la cola
            int targetPosIndex = customerQueue.Count - 1;

            // Si el cliente es nuevo, se mueve a la posición final (e.g., Posición 4 si hay 5 puntos)
            MoveCustomer(newCustomer, queuePositions[targetPosIndex].position);
        }
        else
        {
            // Opcional: El cliente se va si la cola está llena.
            Debug.Log("Cola llena. El cliente se ha ido.");
            Destroy(newCustomer);
        }
    }

    public void CompleteOrder()
    {
        if (customerQueue.Count > 0)
        {
            // 4. Saca al cliente que acaba de pedir (el primero)
            GameObject completedCustomer = customerQueue.Dequeue();

            // Destruye o mueve al cliente a una posición de "salida"
            // Lo destruye después de un pequeño retraso
            Destroy(completedCustomer, 0.5f);

            // 5. Mueve el resto de los clientes una posición hacia adelante
            MoveQueueForward();
        }
    }

    // --- MÉTODOS PRIVADOS DE MOVIMIENTO ---

    private void MoveQueueForward()
    {
        // Obtiene la lista actual de clientes restantes
        GameObject[] currentCustomers = customerQueue.ToArray();

        for (int i = 0; i < currentCustomers.Length; i++)
        {
            GameObject customer = currentCustomers[i];
            // La posición 'i' es la nueva posición, ya que el cliente anterior se fue (índice 0)
            Transform targetPosition = queuePositions[i];

            // Usa la función MoveCustomer con la nueva posición
            MoveCustomer(customer, targetPosition.position);
        }
    }

    private void MoveCustomer(GameObject customer, Vector3 targetPosition)
    {
        NavMeshAgent agent = customer.GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            // 6. ¡El método clave! El NavMeshAgent calcula el camino inteligente.
            agent.SetDestination(targetPosition);
        }
        else
        {
            Debug.LogError($"¡El cliente {customer.name} no tiene NavMeshAgent! No puede moverse.");
        }
    }
}
